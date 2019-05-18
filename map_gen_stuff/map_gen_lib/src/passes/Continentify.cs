using System;
using System.Linq;
using System.Collections.Generic;

namespace MapGen
{
    public class Continentify : IMapGenPass
    {
        public IEnumerable<string> getMapDataRead()
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> getMapDataWritten()
        {
            return new string[] { CoreDataKeys.PARAM_MHEIGHT_KEY };
        }

        public string getPassDesc()
        {
            return "Continentifying";
        }

        public string getPassName()
        {
            return "Continentify";
        }

        public IEnumerable<string> getReqMapParams()
        {
            return new string[] { CoreDataKeys.PARAM_CONTINENT_RAD_KEY };
        }

        public void run(IMapInfo mapData)
        {
            float sea_start_rad_perc = (float)mapData.getParamData(CoreDataKeys.PARAM_CONTINENT_RAD_KEY);
            var hMap = (HeightMap)mapData.getMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY);

            var centPos = new Vector2(hMap.dim.x / 2, hMap.dim.y / 2);
            var mapEdgeRadSqrd = Math.Pow(centPos.x, 2) + Math.Pow(centPos.y, 2);
            var shoreRadSqrd = Math.Pow(centPos.x * sea_start_rad_perc, 2) + Math.Pow(centPos.y * sea_start_rad_perc, 2);
            var mapCornRadFromShoreSqrd = mapEdgeRadSqrd - shoreRadSqrd;

            // ... Slooooow ...
            for (int x = 0; x < hMap.dim.x; x++)
                for (int y = 0; y < hMap.dim.y; y++)
                {
                    var xDistFromCentSqrd = Math.Pow(Math.Abs(centPos.x - x), 2);
                    var yDistFromCentSqrd = Math.Pow(Math.Abs(centPos.y - y), 2);
                    var distSqrd = xDistFromCentSqrd + yDistFromCentSqrd;
                    
                    if (distSqrd < shoreRadSqrd)
                        continue;

                    // Sink this tile into the ocean!
                    var radFromShoreSqrd = distSqrd - shoreRadSqrd;
                    var origCellPercHeight = 1.0f - (float)Math.Sqrt(radFromShoreSqrd / mapCornRadFromShoreSqrd);
                    
                    var newCellHeight = ((float)hMap.cells[x, y] * origCellPercHeight);
                    hMap.cells[x, y] = (byte)newCellHeight;
                }

        }
    }
}