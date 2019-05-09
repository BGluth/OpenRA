using System.Collections.Generic;
using System.Linq;

namespace MapGen
{
    public class HeightMapGenPass : IMapGenPass
    {
        public IEnumerable<string> getMapDataWritten()
        {
            return new string[] {CoreDataKeys.MDATA_HEIGHT_MAP_KEY};
        }

        public IEnumerable<string> getMapDataRead()
        {
            return Enumerable.Empty<string>();
        }

        public string getPassDesc()
        {
            return "Generating heightmaps";
        }

        public string getPassName()
        {
            return "HeightMapGenPass";
        }

        public IEnumerable<string> getPrereqPasses()
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> getReqMapParams()
        {
            return new string[] { CoreDataKeys.PARAM_DIM_KEY, CoreDataKeys.PARAM_MHEIGHT_KEY };
        }

        public void run(IMapInfo mapData)
        {
            HeightMap hMap = (HeightMap)mapData.getMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY);

            
            
        }
    }
}
