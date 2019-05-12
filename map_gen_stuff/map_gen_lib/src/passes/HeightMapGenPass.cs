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
            return "Generating terrain heightmaps";
        }

        public string getPassName()
        {
            return "HeightMapGen";
        }

        public IEnumerable<string> getReqMapParams()
        {
            return new string[] { CoreDataKeys.PARAM_DIM_KEY, CoreDataKeys.PARAM_MHEIGHT_KEY, CoreDataKeys.PARAM_SEED_KEY };
        }

        public void run(IMapInfo mapData)
        {
            var map_dim = (Vector2)mapData.getParamData(CoreDataKeys.PARAM_DIM_KEY);
            var max_height = (int)mapData.getParamData(CoreDataKeys.PARAM_MHEIGHT_KEY);
            var seed = (int)mapData.getParamData(CoreDataKeys.PARAM_SEED_KEY);

            HeightMap hMap = new HeightMap(map_dim, max_height);
            var noiseGen = new FastNoise();

            noiseGen.SetNoiseType(FastNoise.NoiseType.CubicFractal);
            noiseGen.SetSeed(seed);
            noiseGen.SetFractalOctaves(5);
            noiseGen.SetFractalGain(0.5f);
            noiseGen.SetFrequency(2.0f);

            for (int x = 0; x < map_dim.x; x++)
                for (int y = 0; y < map_dim.y; y++)
                {
                    var noise_val = (byte)(noiseGen.GetCubicFractal(x, y) * max_height);
                    hMap.cells[x, y] = noise_val;
                }

            mapData.writeMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY, hMap);
        }
    }
}
