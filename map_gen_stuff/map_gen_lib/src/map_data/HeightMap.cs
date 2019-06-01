namespace MapGen
{
    public class HeightMap
    {
        public readonly Vector2 dim;
        public readonly float maxHeight;

        public float[,] cells;

        public HeightMap(Vector2 dim, float maxHeightPerc)
        {
            this.dim = dim;
            this.maxHeight = maxHeightPerc;
            this.cells = new float[dim.x, dim.y];
        }
    }
}
