using System;
using System.Collections.Generic;
using Match3.Model;

namespace Match3.Core
{
    public sealed class BoardSystem
    {
        private readonly BoardConfiguration _configuration;
        private readonly IMatchDetector _matchDetector;
        private readonly Random _random;

        public GridData Grid { get; }

        public event Action<int, int, int, int> OnPiecesSwapped;
        public event Action OnBoardGenerated;

        public BoardSystem(BoardConfiguration configuration, IMatchDetector matchDetector)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _matchDetector = matchDetector ?? throw new ArgumentNullException(nameof(matchDetector));
            _random = new Random(_configuration.RandomSeed);
            Grid = new GridData(_configuration.Width, _configuration.Height);
        }

        public void FillBoard()
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    var validColors = GetValidInitialColors(x, y);
                    var color = validColors[_random.Next(validColors.Count)];
                    Grid.Set(x, y, new PieceData(x, y, color, PieceType.Normal));
                }
            }

            OnBoardGenerated?.Invoke();
        }

        public bool TrySwap(int x1, int y1, int x2, int y2, out List<MatchResult> matches)
        {
            matches = new List<MatchResult>();

            if (!Grid.IsValidPosition(x1, y1) || !Grid.IsValidPosition(x2, y2))
                return false;

            int horizontalDistance = Math.Abs(x1 - x2);
            int verticalDistance = Math.Abs(y1 - y2);
            bool adjacent = horizontalDistance + verticalDistance == 1;
            if (!adjacent)
                return false;

            SwapInGrid(x1, y1, x2, y2);
            matches = _matchDetector.FindMatches(Grid);
            if (matches.Count > 0)
            {
                OnPiecesSwapped?.Invoke(x1, y1, x2, y2);
                return true;
            }

            SwapInGrid(x1, y1, x2, y2);
            return false;
        }

        public MatchResolution ResolveMatches(IReadOnlyList<MatchResult> matches, GridPosition? focus = null)
        {
            if (matches == null)
                throw new ArgumentNullException(nameof(matches));

            var resolution = new MatchResolution();
            var creations = SelectSpecialCreations(matches, focus);
            var cleared = new HashSet<PieceData>();
            var queued = new HashSet<PieceData>();
            var activationQueue = new Queue<SpecialActivation>();

            foreach (var match in matches)
            {
                foreach (var piece in match.MatchedPieces)
                {
                    if (creations.TryGetValue(piece, out PieceType createdType))
                    {
                        piece.Type = createdType;
                        resolution.AddCreatedSpecial(new SpecialCreation(piece, createdType));
                        continue;
                    }

                    QueueSpecial(piece, activationQueue, queued);
                    ClearPiece(piece, resolution, cleared);
                }
            }

            while (activationQueue.Count > 0)
            {
                var activation = activationQueue.Dequeue();
                resolution.AddActivatedSpecial(activation);

                foreach (var piece in GetBlastArea(activation.Piece, activation.Type))
                {
                    if (creations.ContainsKey(piece))
                        continue;

                    QueueSpecial(piece, activationQueue, queued);
                    ClearPiece(piece, resolution, cleared);
                }
            }

            return resolution;
        }

        public List<PieceFallInfo> ApplyGravityAndRefill(out List<PieceData> spawnedPieces)
        {
            var fallInfos = new List<PieceFallInfo>();
            spawnedPieces = new List<PieceData>();

            for (int x = 0; x < Grid.Width; x++)
            {
                int emptySpaces = 0;
                for (int y = 0; y < Grid.Height; y++)
                {
                    var current = Grid.Get(x, y);
                    if (current == null)
                    {
                        emptySpaces++;
                        continue;
                    }

                    if (emptySpaces == 0)
                        continue;

                    int targetY = y - emptySpaces;
                    Grid.Set(x, targetY, current);
                    Grid.Set(x, y, null);
                    fallInfos.Add(new PieceFallInfo(x, y, x, targetY));
                }

                for (int index = 0; index < emptySpaces; index++)
                {
                    int targetY = Grid.Height - emptySpaces + index;
                    int spawnY = Grid.Height + index;
                    var color = NextColor();
                    var piece = new PieceData(x, targetY, color, PieceType.Normal);
                    Grid.Set(x, targetY, piece);
                    spawnedPieces.Add(piece);
                    fallInfos.Add(new PieceFallInfo(x, spawnY, x, targetY));
                }
            }

            return fallInfos;
        }

        private Dictionary<PieceData, PieceType> SelectSpecialCreations(
            IReadOnlyList<MatchResult> matches,
            GridPosition? focus)
        {
            var creations = new Dictionary<PieceData, PieceType>();

            foreach (var match in matches)
            {
                if (match.SpecialCreationType == PieceType.Normal)
                    continue;

                var piece = SelectSpecialPiece(match, focus);
                if (piece != null)
                    creations[piece] = match.SpecialCreationType;
            }

            return creations;
        }

        private static PieceData SelectSpecialPiece(MatchResult match, GridPosition? focus)
        {
            if (focus.HasValue)
            {
                foreach (var piece in match.MatchedPieces)
                {
                    if (piece.Type == PieceType.Normal &&
                        piece.X == focus.Value.X &&
                        piece.Y == focus.Value.Y)
                        return piece;
                }
            }

            PieceData selected = null;
            foreach (var piece in match.MatchedPieces)
            {
                if (piece.Type != PieceType.Normal)
                    continue;

                if (selected == null || piece.Y < selected.Y || piece.Y == selected.Y && piece.X < selected.X)
                    selected = piece;
            }

            return selected;
        }

        private static void QueueSpecial(
            PieceData piece,
            Queue<SpecialActivation> activationQueue,
            HashSet<PieceData> queued)
        {
            if (piece == null || piece.Type == PieceType.Normal || piece.Type == PieceType.Empty)
                return;
            if (!queued.Add(piece))
                return;

            activationQueue.Enqueue(new SpecialActivation(piece, piece.Type));
        }

        private void ClearPiece(PieceData piece, MatchResolution resolution, HashSet<PieceData> cleared)
        {
            if (piece == null || !cleared.Add(piece))
                return;

            resolution.AddClearedPiece(piece);
            if (Grid.IsValidPosition(piece.X, piece.Y) && ReferenceEquals(Grid.Get(piece.X, piece.Y), piece))
                Grid.Set(piece.X, piece.Y, null);
        }

        private List<PieceData> GetBlastArea(PieceData special, PieceType type)
        {
            var pieces = new List<PieceData>();

            if (type == PieceType.Bomb)
            {
                for (int horizontalOffset = -_configuration.BombRadius;
                     horizontalOffset <= _configuration.BombRadius;
                     horizontalOffset++)
                {
                    for (int verticalOffset = -_configuration.BombRadius;
                         verticalOffset <= _configuration.BombRadius;
                         verticalOffset++)
                    {
                        AddPieceAt(
                            special.X + horizontalOffset,
                            special.Y + verticalOffset,
                            pieces);
                    }
                }
            }
            else if (type == PieceType.HorizontalLine)
            {
                for (int x = 0; x < Grid.Width; x++)
                    AddPieceAt(x, special.Y, pieces);
            }
            else if (type == PieceType.VerticalLine)
            {
                for (int y = 0; y < Grid.Height; y++)
                    AddPieceAt(special.X, y, pieces);
            }

            return pieces;
        }

        private void AddPieceAt(int x, int y, ICollection<PieceData> pieces)
        {
            if (!Grid.IsValidPosition(x, y))
                return;

            var piece = Grid.Get(x, y);
            if (piece != null)
                pieces.Add(piece);
        }

        private List<PieceColor> GetValidInitialColors(int x, int y)
        {
            var validColors = new List<PieceColor>();
            foreach (var color in _configuration.AvailableColors)
            {
                if (IsValidInitialColor(x, y, color))
                    validColors.Add(color);
            }

            if (validColors.Count == 0)
                throw new InvalidOperationException("No valid color is available for board generation.");

            return validColors;
        }

        private PieceColor NextColor()
        {
            int index = _random.Next(_configuration.AvailableColors.Count);
            return _configuration.AvailableColors[index];
        }

        private void SwapInGrid(int x1, int y1, int x2, int y2)
        {
            var first = Grid.Get(x1, y1);
            var second = Grid.Get(x2, y2);
            Grid.Set(x1, y1, second);
            Grid.Set(x2, y2, first);
        }

        private bool IsValidInitialColor(int x, int y, PieceColor color)
        {
            if (x >= _configuration.InitialMatchLength - 1)
            {
                bool horizontalMatch = true;
                for (int offset = 1; offset < _configuration.InitialMatchLength; offset++)
                {
                    var piece = Grid.Get(x - offset, y);
                    if (piece == null || piece.Color != color)
                    {
                        horizontalMatch = false;
                        break;
                    }
                }

                if (horizontalMatch)
                    return false;
            }

            if (y >= _configuration.InitialMatchLength - 1)
            {
                bool verticalMatch = true;
                for (int offset = 1; offset < _configuration.InitialMatchLength; offset++)
                {
                    var piece = Grid.Get(x, y - offset);
                    if (piece == null || piece.Color != color)
                    {
                        verticalMatch = false;
                        break;
                    }
                }

                if (verticalMatch)
                    return false;
            }

            return true;
        }
    }
}
