namespace MapGen
{
    public class HeightMap
    {
        public readonly Vector2 dim;
        public readonly int maxHeight;

        public byte[,] cells;

        public HeightMap(Vector2 dim, int maxHeight)
        {
            this.dim = dim;
            this.maxHeight = maxHeight;
            this.cells = new byte[dim.x, dim.y];
        }
    }
}
