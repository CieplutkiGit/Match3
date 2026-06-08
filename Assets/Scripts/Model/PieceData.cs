namespace Match3.Model
{
    public class PieceData
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public PieceData(int x, int y, PieceColor color = PieceColor.None, PieceType type = PieceType.Empty)
        {
            X = x;
            Y = y;
            Color = color;
            Type = type;
        }
    }
}
