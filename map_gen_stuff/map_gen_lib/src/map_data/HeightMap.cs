namespace MapGen
{
    public class HeightMap
    {
        public readonly Vector2 dim;
        public readonly int maxHeight;

        public byte[,] cells;

        public HeightMap(Vector2 dim, float maxHeightPerc)
        {
            this.dim = dim;
            this.maxHeight = (int)(byte.MaxValue * maxHeightPerc);
            this.cells = new byte[dim.x, dim.y];
        }
    }
}
