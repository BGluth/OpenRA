using System;
using System.Collections.Generic;

namespace MapGen
{
    public class MapGenInfo : IMapInfo
    {
        const string DEFAULT_MAP_NAME = "Unmamed_Map";

        Queue<IMapGenPass> generationPasses = new Queue<IMapGenPass>();
        Dictionary<string, object> mapParams; 
        Dictionary<string, object> mapData;
        HashSet<string> existingMapData;
        HashSet<string> finishedPasses;

        public MapGenInfo()
        {
            generationPasses = new Queue<IMapGenPass>();
            mapParams = new Dictionary<string, object>();
            mapData = new Dictionary<string, object>();
            existingMapData = new HashSet<string>();
            finishedPasses = new HashSet<string>();
        }

        public void addPass(IMapGenPass pass)
        {
            generationPasses.Enqueue(pass);
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

            int numItersSinceLastSuccPass = 0;
            while (generationPasses.Count != 0)
            {
                if (numItersSinceLastSuccPass == generationPasses.Count)
                {
                    Utils.writeError("Depencency cycle detectected with passes! Aborting!");
                    return;
                }

                var pass = generationPasses.Dequeue();

                if (!allPrereqPassesHaveRun(pass) || !prereqDataExistsForPass(pass))
                {
                    generationPasses.Enqueue(pass);
                    numItersSinceLastSuccPass++;
                    continue;
                }

                // Ready to run
                Utils.writeMessage(String.Format("{0}...", pass.getPassDesc()));
                pass.run(this);
                finishedPasses.Add(pass.getPassName());
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

            foreach (var pass in generationPasses)
            {
                foreach (var reqParam in pass.getReqMapParams())
                {
                    if (!existingMapData.Contains(reqParam))
                    {
                        ok = false;
                        var passName = pass.getPassName();
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

        bool allPrereqPassesHaveRun(IMapGenPass passToCheck)
        {
            foreach (var prereqPassName in passToCheck.getPrereqPasses())
            {
                if (!finishedPasses.Contains(prereqPassName))
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
