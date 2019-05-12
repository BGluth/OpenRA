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

        public static void generateGreyscaleNoiseImage(CommonParams cParams, byte[,] noise, int maxVal)
        {
            Func<int, int, SKColor> heightValToGreyColFunc = delegate (int x, int y)
            {
                var col = (byte)(((float)noise[x, y] / (float)maxVal) * 256);
                return new SKColor(col, col, col, 0);
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

            for (int xNoise = 0; xNoise < mDim.x; xNoise++)
                for (int yNoise = 0; yNoise < mDim.y; yNoise++)
                {
                    var col = get_color_func(xNoise, yNoise);

                    var xImgStrt = xNoise * cParams.scalingFactor;
                    var xImgEnd = xImgStrt + cParams.scalingFactor;
                    var yImgStrt = yNoise * cParams.scalingFactor;
                    var yImgEnd = yImgStrt + cParams.scalingFactor;

                    for (int xImage = xImgStrt; xImage < xImgEnd; xImage++)
                        for (int yImage = yImgStrt; yImage < yImgEnd; yImage++)
                            bMap.SetPixel(xImage, yImage, col);
                }

            var imgNameWithExt = string.Format("{0}.png", cParams.imgName);
            var stream = new SKFileWStream(imgNameWithExt);
            SKPixmap.Encode(stream, bMap, SKEncodedImageFormat.Png, 100);
        }
    }
}