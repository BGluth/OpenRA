using System;
using System.Collections.Generic;
using System.Linq;

namespace MapGen
{
    public class SteepnessPass : IMapGenPass
    {
        public IEnumerable<string> getMapDataRead()
        {
            return new string[] { CoreDataKeys.MDATA_HEIGHT_MAP_KEY };
        }

        public IEnumerable<string> getMapDataWritten()
        {
            return new string[] { CoreDataKeys.MDATA_STEPNESS_MAP_KEY };
        }

        public string getPassDesc()
        {
            return "Analysing steepness";
        }

        public string getPassName()
        {
            return "Steepness";
        }

        public IEnumerable<string> getReqMapParams()
        {
            return Enumerable.Empty<string>();
        }

        public void run(IMapInfo mapData)
        {
            var hMap = (HeightMap)mapData.getMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY);

            // Since we look at the neighbors of each cell, we need to handle the ones at the ege of the map.
            // We're going to pad the heightmap in order to handle this.

            var pHMap = CreatePaddedHMap(hMap);
            var sMap = CreateSteepnessMap(hMap, pHMap);

            mapData.writeMapData(CoreDataKeys.MDATA_STEPNESS_MAP_KEY, sMap);
        }

        HeightMap CreatePaddedHMap(HeightMap origHMap)
        {
            var newDim = new Vector2(origHMap.dim.x + 2, origHMap.dim.y + 2);
            var paddedHMap = new HeightMap(newDim, 255);

            // Copy over origional into center
            for (int x = 0; x < origHMap.dim.x; x++)
                for (int y = 0; y < origHMap.dim.y; y++)
                    paddedHMap.cells[x + 1, y + 1] = origHMap.cells[x, y];

            // Pad. Copy cell to border on edges.
            for (int x = 0; x < paddedHMap.dim.x; x++)
            {
                paddedHMap.cells[x, 0] = origHMap.cells[x, 0];
                paddedHMap.cells[x, paddedHMap.dim.y - 1] = origHMap.cells[x, origHMap.dim.y - 1];
            }

            for (int y = 0; y < paddedHMap.dim.y; y++)
            {
                paddedHMap.cells[0, y] = origHMap.cells[y, 0];
                paddedHMap.cells[origHMap.dim.x - 1, y] = origHMap.cells[origHMap.dim.x - 1, y];
            }

            var endIdxs = new Vector2(paddedHMap.dim.x - 1, paddedHMap.dim.x - 1);

            // Corners
            paddedHMap.cells[0, 0] = (byte)((paddedHMap.cells[1, 0] + paddedHMap.cells[0, 1]) / 2); // TL
            paddedHMap.cells[endIdxs.x, 0] = (byte)((paddedHMap.cells[endIdxs.x - 1, 0] + paddedHMap.cells[endIdxs.x, 1]) / 2); // TR
            paddedHMap.cells[endIdxs.x, endIdxs.y] = (byte)((paddedHMap.cells[endIdxs.x - 1, endIdxs.y] + paddedHMap.cells[endIdxs.x, endIdxs.y - 1]) / 2); // BR
            paddedHMap.cells[0, endIdxs.y] = (byte)((paddedHMap.cells[0, endIdxs.y - 1] + paddedHMap.cells[1, endIdxs.y]) / 2); // BL

            return paddedHMap;
        }

        HeightMap CreateSteepnessMap(HeightMap hMap, HeightMap pHMap)
        {
            var sMap = new HeightMap(hMap.dim, 255);

            for (int x = 1; x < pHMap.dim.x - 1; x++)
                for (int y = 1; y < pHMap.dim.y - 1; y++)
                {
                    var cellHgt = hMap.cells[x, y];

                    var dx = (((float)hMap.cells[x - 1, y] + (float)hMap.cells[x + 1, y]) / 2.0f) - cellHgt;
                    var dy = (((float)hMap.cells[x, y - 1] + (float)hMap.cells[x, y + 1]) / 2.0f) - cellHgt;

                    sMap.cells[x - 1, y - 1] = (byte)Math.Sqrt(dx * dx + dy * dy);
                }
                
            return sMap;
        }
    }
}