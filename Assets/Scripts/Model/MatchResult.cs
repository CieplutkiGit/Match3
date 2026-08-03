using System.Collections.Generic;

namespace Match3.Model
{
    public class MatchResult
    {
        public List<PieceData> MatchedPieces { get; }
        public PieceType SpecialCreationType { get; set; }

        public MatchResult()
        {
            MatchedPieces = new List<PieceData>();
            SpecialCreationType = PieceType.Normal;
        }

        public void AddPiece(PieceData piece)
        {
            if (!MatchedPieces.Contains(piece))
            {
                MatchedPieces.Add(piece);
            }
        }
    }
}
