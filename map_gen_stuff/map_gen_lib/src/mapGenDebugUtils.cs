using System;
using SkiaSharp;

namespace MapGen
{
    public static class MapGenDebugUtils
    {
        public struct CommonParams
        {
            public string imgName;
            public int scalingFactor;

            public CommonParams(string imgName, int scalingFactor)
            {
                this.imgName = imgName;
                this.scalingFactor = scalingFactor;
            }
        }

        public static void generateGreyscaleNoiseImage(CommonParams cParams, float[,] noise, float maxVal)
        {
            Func<int, int, SKColor> heightValToGreyColFunc = delegate (int x, int y)
            {
                var col = (byte)((noise[x, y] / maxVal) * 255);
                return new SKColor(col, col, col, 255);
            };

            Vector2 mDim = new Vector2(noise.GetLength(0), noise.GetLength(1));
            generateMap(cParams, mDim, heightValToGreyColFunc);
        }

        public static void generateBoolMap(CommonParams cParams, bool[,] noise, SKColor falseCol, SKColor trueCol)
        {
            Func<int, int, SKColor> waterLevelToColFunc = delegate (int x, int y)
            {
                return noise[x, y] ? trueCol: falseCol;
            };

            Vector2 mDim = new Vector2(noise.GetLength(0), noise.GetLength(1));
            generateMap(cParams, mDim, waterLevelToColFunc);
        }

        static void generateMap(CommonParams cParams, Vector2 mDim, Func<int, int, SKColor> get_color_func)
        {
            var bMap = new SKBitmap(mDim.x, mDim.y, false);

            for (int x = 0; x < mDim.x; x++)
                for (int y = 0; y < mDim.y; y++)
                {
                    var col = get_color_func(x, y);
                    bMap.SetPixel(x, y, col);
                }

            var bMapResized = new SKBitmap(mDim.x * cParams.scalingFactor, mDim.y * cParams.scalingFactor);
            bMap.ScalePixels(bMapResized, SKFilterQuality.None);

            var imgNameWithExt = string.Format("{0}.png", cParams.imgName);
            var stream = new SKFileWStream(imgNameWithExt);
            SKPixmap.Encode(stream, bMapResized, SKEncodedImageFormat.Png, 100);
        }
    }
}