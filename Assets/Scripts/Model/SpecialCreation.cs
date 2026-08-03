namespace Match3.Model
{
    public sealed class SpecialCreation
    {
        public PieceData Piece { get; }
        public PieceType Type { get; }

        public SpecialCreation(PieceData piece, PieceType type)
        {
            Piece = piece;
            Type = type;
        }
    }
}
