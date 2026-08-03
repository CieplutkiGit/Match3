namespace Match3.Model
{
    public class GridData : IGrid<PieceData>
    {
        private readonly PieceData[,] _grid;

        public int Width { get; }
        public int Height { get; }

        public GridData(int width, int height)
        {
            if (width <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _grid = new PieceData[width, height];
        }

        public PieceData Get(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return _grid[x, y];
            }
            return null;
        }

        public void Set(int x, int y, PieceData value)
        {
            if (!IsValidPosition(x, y))
                throw new System.ArgumentOutOfRangeException(nameof(x));

            _grid[x, y] = value;
            if (value == null)
                return;

            value.X = x;
            value.Y = y;
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
