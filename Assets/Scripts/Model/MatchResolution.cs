using System.Collections.Generic;

namespace Match3.Model
{
    public sealed class MatchResolution
    {
        private readonly List<PieceData> _clearedPieces = new List<PieceData>();
        private readonly List<SpecialCreation> _createdSpecials = new List<SpecialCreation>();
        private readonly List<SpecialActivation> _activatedSpecials = new List<SpecialActivation>();

        public IReadOnlyList<PieceData> ClearedPieces => _clearedPieces;
        public IReadOnlyList<SpecialCreation> CreatedSpecials => _createdSpecials;
        public IReadOnlyList<SpecialActivation> ActivatedSpecials => _activatedSpecials;

        public void AddClearedPiece(PieceData piece)
        {
            _clearedPieces.Add(piece);
        }

        public void AddCreatedSpecial(SpecialCreation creation)
        {
            _createdSpecials.Add(creation);
        }

        public void AddActivatedSpecial(SpecialActivation activation)
        {
            _activatedSpecials.Add(activation);
        }
    }
}
