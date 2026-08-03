namespace Match3.Model
{
    public sealed class SpecialActivation
    {
        public PieceData Piece { get; }
        public PieceType Type { get; }

        public SpecialActivation(PieceData piece, PieceType type)
        {
            Piece = piece;
            Type = type;
        }
    }
}
