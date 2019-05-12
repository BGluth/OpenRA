using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MapGen
{
    class PassInfo
    {
        public List<string> prereqPasses;
        public IMapGenPass pass;

        public PassInfo(IMapGenPass pass)
        {
            this.prereqPasses = new List<string>();
            this.pass = pass;
        }
    }

    class MapDataUsage
    {
        int numReaders;
        bool passIsWriting;

        public bool canWrite()
        {
            return numReaders == 0 && passIsWriting == false;
        }

        public bool canRead()
        {
            return passIsWriting == false;
        }

        public void addReader()
        {
            numReaders++;
        }

        public void removeReader()
        {
            numReaders--;
        }

        public void setWriting(bool state)
        {
            passIsWriting = state;
        }
    }

    public class MapGenInfo : IMapInfo
    {
        const string DEFAULT_MAP_NAME = "Unmamed_Map";

        Dictionary<string, PassInfo> passes;
        Dictionary<string, object> mapParams; 
        Dictionary<string, object> mapData;
        Dictionary<string, MapDataUsage> mapDataUsageState;
        HashSet<string> finishedPasses;
        ConcurrentQueue<PassInfo> passesAwatingCompletionProcessing = new ConcurrentQueue<PassInfo>();

        int numThreadsFree;


        public MapGenInfo()
        {
            passes = new Dictionary<string, PassInfo>();
            mapParams = new Dictionary<string, object>();
            mapData = new Dictionary<string, object>();
            finishedPasses = new HashSet<string>();
        }

        public string addPass(IMapGenPass pass)
        {
            var passKey = pass.getPassName();
            var passInfo = new PassInfo(pass);
            passes.Add(passKey, passInfo);

            return passKey;
        }

        public string addPass(IMapGenPass pass, params string[] prereqPasses)
        {
            addPass(pass);

            string passKey = pass.getPassName();
            var passPrereqsList = passes[passKey].prereqPasses;
            foreach (var prereqPassKey in prereqPasses)
                passPrereqsList.Add(prereqPassKey);

            return passKey;
        }

        public object getParamData(string paramKey)
        {
            return mapParams[paramKey];
        }

        public void addParam(string name, object param)
        {
            mapParams.Add(name, param);
        }

        public void writeMapData(string dataKey, object data)
        {
            mapData[dataKey] = data;
        }

        public object getMapData(string dataKey)
        {
            return mapData[dataKey];
        }

        public void generateMap(int numThreads)
        {
            setupMapDataUsageStates();

            if (!allPassesHaveReqParams())
                return;

            var remainingPasses = new List<PassInfo>();
            var passesThatAreReadyToRun = new Queue<PassInfo>();

            foreach (var passInfo in passes.Values)
                remainingPasses.Add(passInfo);

            // -1 to stop a false positive at start
            int numTasksCompletedSinceLastIter = -1;
            numThreadsFree = numThreads;
            while (remainingPasses.Count != 0)
            {
                if (numTasksCompletedSinceLastIter == 0 && numThreadsFree == numThreads)
                {
                    Utils.writeError("Depencency cycle detectected with passes! Aborting!");
                    return;
                }

                numTasksCompletedSinceLastIter = processAnyPassCompletedMessages();
                upgradeAnyPassesThatCanRunToReady(remainingPasses, passesThatAreReadyToRun);

                while (numThreadsFree > 0 && passesThatAreReadyToRun.Count > 0)
                {
                    var passInfo = passesThatAreReadyToRun.Dequeue();

                    numThreadsFree--;
                    updateDataReadWriteStateForPassStart(passInfo.pass);

                    Utils.writeMessage(String.Format("Starting {0}...", passInfo.pass.getPassDesc()));
                    ThreadPool.QueueUserWorkItem(threadRunPass, passInfo.pass);
                }
            }

            var mapName = CoreDataKeys.getMapName(this);
            Utils.writeMessage(String.Format("Finished generating \"{0}\".", mapName));
        }

        void threadRunPass(object passObj)
        {
            PassInfo passInfo = (PassInfo)passObj;
            passInfo.pass.run(this);
            Utils.writeMessage(String.Format("Finished {}", passInfo.pass.getPassDesc()));
            passesAwatingCompletionProcessing.Enqueue(passInfo);
        }

        public float getCurrPassPercComplete()
        {
            return 0; // TODO
        }

        bool allPassesHaveReqParams()
        {
            bool ok = true;

            foreach (var passInfo in passes.Values)
            {
                foreach (var reqParam in passInfo.pass.getReqMapParams())
                {
                    if (!mapDataUsageState.ContainsKey(reqParam))
                    {
                        ok = false;
                        var passName = passInfo.pass.getPassName();
                        Utils.writeMessage(String.Format("Map pass \"{0}\" is missing required parameter \"{1}\".", passName, reqParam));
                    }
                }
            }

            return ok;
        }
        
        void upgradeAnyPassesThatCanRunToReady(IList<PassInfo> remainingPasses, Queue<PassInfo> readyToRunPasses)
        {
            for (int i = remainingPasses.Count - 1; i >= 0; i--)
            {
                var passInfo = remainingPasses[i];
                if (passIsAbleToRun(passInfo))
                {
                    readyToRunPasses.Enqueue(passInfo);
                    Utils.listSwapRemove(remainingPasses, i);
                    updateDataReadWriteStateForPassStart(passInfo.pass);
                }
            }
        }

        int processAnyPassCompletedMessages()
        {
            int numCompletedPasses = 0;
            foreach (var pInfo in passesAwatingCompletionProcessing)
            {
                finishedPasses.Add(pInfo.pass.getPassName());
                updateDataReadWriteStateForPassEnd(pInfo.pass);
                numThreadsFree++;
                numCompletedPasses++;
            }

            return numCompletedPasses;
        }

        bool passIsAbleToRun(PassInfo passInfo)
        {
            return
                allPrereqPassesHaveRun(passInfo.prereqPasses) &&
                prereqDataExistsForPass(passInfo.pass) &&
                passCanAccessAllNeededReadData(passInfo.pass.getMapDataRead()) &&
                passCanAccessAllNeededWriteData(passInfo.pass.getMapDataWritten());
        }

        void setupDefaultValuesForMissingKeyParams()
        {
            if (!mapParams.ContainsKey(CoreDataKeys.PARAM_MAP_NAME_KEY))
                mapParams[CoreDataKeys.PARAM_MAP_NAME_KEY] = DEFAULT_MAP_NAME;
        }

        void setupMapDataUsageStates()
        {
            foreach (var passInfo in passes.Values)
            {
                addUsageEntryForUnseenMapDataTypes(passInfo.pass.getMapDataRead());
                addUsageEntryForUnseenMapDataTypes(passInfo.pass.getMapDataWritten());
            }
        }

        bool allPrereqPassesHaveRun(IEnumerable<string> prereqPasses)
        {
            foreach (var prereqPassKey in prereqPasses)
            {
                if (!finishedPasses.Contains(prereqPassKey))
                    return false;
            }

            return true;
        }

        void updateDataReadWriteStateForPassStart(IMapGenPass pass)
        {
            foreach (var readKey in pass.getMapDataRead()) { mapDataUsageState[readKey].addReader(); }
            foreach (var writeKey in pass.getMapDataWritten()) { mapDataUsageState[writeKey].setWriting(true); }
        }

        void updateDataReadWriteStateForPassEnd(IMapGenPass pass)
        {
            foreach (var readKey in pass.getMapDataRead()) { mapDataUsageState[readKey].removeReader(); }
            foreach (var writeKey in pass.getMapDataWritten()) { mapDataUsageState[writeKey].setWriting(false); }
        }

        bool prereqDataExistsForPass(IMapGenPass pass)
        {
            foreach (var reqMapDataKey in pass.getMapDataRead())
            {
                if (!finishedPasses.Contains(reqMapDataKey))
                    return false;
            }

            return true;
        }

        void addUsageEntryForUnseenMapDataTypes(IEnumerable<string> mapDataTypes)
        {
            foreach (var readDataKey in mapDataTypes)
            {
                if (!mapDataUsageState.ContainsKey(readDataKey))
                    mapDataUsageState[readDataKey] = new MapDataUsage();
            }
        }

        // Could condense these two functions, but just thinking of readability at the call sites
        bool passCanAccessAllNeededReadData(IEnumerable<string> dataThatPassNeedsToRead)
        {
            return Enumerable.All(dataThatPassNeedsToRead, readKey => mapDataUsageState[readKey].canRead());
        }

        bool passCanAccessAllNeededWriteData(IEnumerable<string> dataThatPassNeedsToWrite)
        {
            return Enumerable.All(dataThatPassNeedsToWrite, writeKey => mapDataUsageState[writeKey].canWrite());
        }
    }
}
