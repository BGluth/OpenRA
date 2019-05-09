using System;
using System.Collections.Generic;

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

    public class MapGenInfo : IMapInfo
    {
        const string DEFAULT_MAP_NAME = "Unmamed_Map";

        Dictionary<string, PassInfo> passes;
        Dictionary<string, object> mapParams; 
        Dictionary<string, object> mapData;
        HashSet<string> existingMapData;
        HashSet<string> finishedPasses;

        public MapGenInfo()
        {
            passes = new Dictionary<string, PassInfo>();
            mapParams = new Dictionary<string, object>();
            mapData = new Dictionary<string, object>();
            existingMapData = new HashSet<string>();
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

                if (!allPrereqPassesHaveRun(passInfo.prereqPasses) || !prereqDataExistsForPass(passInfo.pass))
                {
                    generationPasses.Enqueue(passInfo);
                    numItersSinceLastSuccPass++;
                    continue;
                }

                // Ready to run
                Utils.writeMessage(String.Format("{0}...", passInfo.pass.getPassDesc()));
                passInfo.pass.run(this);
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
                    if (!existingMapData.Contains(reqParam))
                    {
                        ok = false;
                        var passName = passInfo.pass.getPassName();
                        Utils.writeMessage(String.Format("Map pass \"{0}\" is missing required parameter \"{1}\".", passName, reqParam));
                    }
                }
            }

            return ok;
        }
        
        void setupDefaultValuesForMissingKeyParams()
        {
            if (!mapParams.ContainsKey(CoreDataKeys.PARAM_MAP_NAME_KEY))
                mapParams[CoreDataKeys.PARAM_MAP_NAME_KEY] = DEFAULT_MAP_NAME;
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

        bool prereqDataExistsForPass(IMapGenPass pass)
        {
            foreach (var reqMapDataKey in pass.getMapDataRead())
            {
                if (!finishedPasses.Contains(reqMapDataKey))
                    return false;
            }

            return true;
        }
    }
}
