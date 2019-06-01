using System.Collections.Generic;
using System.Linq;

namespace MapGen
{
    public class HeightMapGenPass : IMapGenPass
    {
        int numOct;
        float gain;
        float freq;
        float lacunarity;

        public HeightMapGenPass(int numOct, float gain, float freq, float lac)
        {
            this.numOct = numOct;
            this.gain = gain;
            this.freq = freq;
            this.lacunarity = lac;
        }

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
            return new string[] { CoreDataKeys.PARAM_DIM_KEY, CoreDataKeys.PARAM_MAX_HEIGHT_KEY_PERC, CoreDataKeys.PARAM_SEED_KEY };
        }

        public void run(IMapInfo mapData)
        {
            var map_dim = (Vector2)mapData.getParamData(CoreDataKeys.PARAM_DIM_KEY);
            var max_height_perc = (float)mapData.getParamData(CoreDataKeys.PARAM_MAX_HEIGHT_KEY_PERC);
            var seed = (int)mapData.getParamData(CoreDataKeys.PARAM_SEED_KEY);

            HeightMap hMap = new HeightMap(map_dim, max_height_perc);
            var noiseGen = new FastNoise();

            // TODO: Later make this stuff parameters...
            noiseGen.SetNoiseType(FastNoise.NoiseType.CubicFractal);
            noiseGen.SetFractalType(FastNoise.FractalType.FBM);
            noiseGen.SetSeed(seed);
            noiseGen.SetFractalOctaves(numOct);
            noiseGen.SetFractalGain(gain);
            noiseGen.SetFrequency(freq);
            noiseGen.SetFractalLacunarity(lacunarity);

            for (int x = 0; x < map_dim.x; x++)
                for (int y = 0; y < map_dim.y; y++)
                {
                    var noise = ((noiseGen.GetCubicFractal(x, y) + 1.0f) / 2.0);
                    var noise_val = (float)(noise * hMap.maxHeight);
                    hMap.cells[x, y] = noise_val;
                }

            mapData.writeMapData(CoreDataKeys.MDATA_HEIGHT_MAP_KEY, hMap);
        }
    }
}
