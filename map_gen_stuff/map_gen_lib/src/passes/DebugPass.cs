using System.Collections.Generic;
using System.Linq;

using SkiaSharp;

namespace MapGen
{
    public class DebugPass : IMapGenPass
    {
        public IEnumerable<string> getMapDataRead()
        {
            return new string[] { CoreDataKeys.MDATA_HEIGHT_MAP_KEY, CoreDataKeys.MDATA_WATER_COVERED_CELLS };
        }

        public IEnumerable<string> getMapDataWritten()
        {
            return Enumerable.Empty<string>();
        }

        public string getPassDesc()
        {
            return "Generating debug images";
        }

        public string getPassName()
        {
            return "Debug";
        }

        public IEnumerable<string> getReqMapParams()
        {
            return Enumerable.Empty<string>();
        }

        public void run(IMapInfo mapData)
        {
            var hMap = (HeightMap)mapData.getMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY);
            var cParams = new MapGenDebugUtils.CommonParams("Height_Map", 2);
            MapGenDebugUtils.generateGreyscaleNoiseImage(cParams, hMap.cells, 256);

            var wMap = (bool[,])mapData.getMapData(CoreDataKeys.MDATA_WATER_COVERED_CELLS);
            cParams = new MapGenDebugUtils.CommonParams("Sea_Coverage_Map", 2);
            var waterCol = new SKColor(86, 190, 255, 0);
            var landCol = new SKColor(66, 242, 43, 0);
            MapGenDebugUtils.generateBoolMap(cParams, wMap, landCol, waterCol);
        }
    }
}