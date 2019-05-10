using System;
using System.Collections.Generic;
using System.Linq;

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


        public MapGenInfo()
        {
            passes = new Dictionary<string, PassInfo>();
            mapParams = new Dictionary<string, object>();
            mapData = new Dictionary<string, object>();
            finishedPasses = new HashSet<string>();
        }

        public void addPass(IMapGenPass pass)
        {
            var passKey = pass.getPassName();
            var passInfo = new PassInfo(pass);
            passes.Add(passKey, passInfo);
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

        public void generateMap()
        {
            setupMapDataUsageStates();

            if (!allPassesHaveReqParams())
                return;

            Queue<PassInfo> generationPasses = new Queue<PassInfo>();
            foreach (var passInfo in passes.Values)
                generationPasses.Enqueue(passInfo);

            int numItersSinceLastSuccPass = 0;
            while (generationPasses.Count != 0)
            {
                if (numItersSinceLastSuccPass == generationPasses.Count)
                {
                    Utils.writeError("Depencency cycle detectected with passes! Aborting!");
                    return;
                }

                var passInfo = generationPasses.Dequeue();

                if (passIsAbleToRun(passInfo))
                {
                    generationPasses.Enqueue(passInfo);
                    numItersSinceLastSuccPass++;
                    continue;
                }

                // Ready to run
                updateDataReadWriteStateForPassStart(passInfo.pass);

                Utils.writeMessage(String.Format("Starting {0}...", passInfo.pass.getPassDesc()));
                passInfo.pass.run(this);

                updateDataReadWriteStateForPassEnd(passInfo.pass);
                finishedPasses.Add(passInfo.pass.getPassName());
                numItersSinceLastSuccPass = 0;
            }

            var mapName = CoreDataKeys.getMapName(this);
            Utils.writeMessage(String.Format("Finished generating \"{0}\".", mapName));
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
