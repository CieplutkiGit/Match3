using System;
using System.Collections.Generic;
using Match3.Model;
using Match3.Data;

namespace Match3.Core
{
    public class BoardSystem
    {
        public GridData Grid { get; private set; }
        private readonly LevelSettings _levelSettings;
        private readonly IMatchDetector _matchDetector;
        private readonly List<PieceData> _keptSpecialPieces = new List<PieceData>();

        public event Action<int, int, int, int> OnPiecesSwapped;
        public event Action OnBoardGenerated;

        public BoardSystem(LevelSettings levelSettings, IMatchDetector matchDetector)
        {
            _levelSettings = levelSettings;
            _matchDetector = matchDetector;
            Grid = new GridData(_levelSettings.GridWidth, _levelSettings.GridHeight);
        }

        public void FillBoard()
        {
            var random = new Random();
            var availableColors = _levelSettings.AvailableColors;

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Height; y++)
                {
                    List<PieceColor> validColors = new List<PieceColor>();

                    foreach (var color in availableColors)
                    {
                        if (IsValidInitialColor(x, y, color))
                        {
                            validColors.Add(color);
                        }
                    }

                    PieceColor chosenColor = validColors.Count > 0 
                        ? validColors[random.Next(validColors.Count)] 
                        : availableColors[random.Next(availableColors.Length)];

                    var piece = new PieceData(x, y, chosenColor, PieceType.Normal);
                    Grid.Set(x, y, piece);
                }
            }

            OnBoardGenerated?.Invoke();
        }

        public bool TrySwap(int x1, int y1, int x2, int y2, out List<MatchResult> matches)
        {
            matches = new List<MatchResult>();

            if (!Grid.IsValidPosition(x1, y1) || !Grid.IsValidPosition(x2, y2))
            {
                return false;
            }

            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);
            if ((dx == 1 && dy == 0) || (dx == 0 && dy == 1))
            {
                SwapInGrid(x1, y1, x2, y2);

                matches = _matchDetector.FindMatches(Grid);
                if (matches.Count > 0)
                {
                    OnPiecesSwapped?.Invoke(x1, y1, x2, y2);
                    return true;
                }

                SwapInGrid(x1, y1, x2, y2);
            }

            return false;
        }

        public void ClearMatches(List<MatchResult> matches, int focusX = -1, int focusY = -1)
        {
            _keptSpecialPieces.Clear();
            foreach (var match in matches)
            {
                PieceData specialSpawnPiece = null;
                if (match.GeneratedSpecialType != PieceType.Normal)
                {
                    foreach (var p in match.MatchedPieces)
                    {
                        if (p.X == focusX && p.Y == focusY)
                        {
                            specialSpawnPiece = p;
                            break;
                        }
                    }
                    if (specialSpawnPiece == null && match.MatchedPieces.Count > 0)
                    {
                        specialSpawnPiece = match.MatchedPieces[0];
                    }
                }

                foreach (var p in match.MatchedPieces)
                {
                    if (p == specialSpawnPiece)
                    {
                        p.Type = match.GeneratedSpecialType;
                        _keptSpecialPieces.Add(p);
                    }
                    else
                    {
                        Grid.Set(p.X, p.Y, null);
                    }
                }
            }
        }

        public List<PieceFallInfo> ApplyGravityAndRefill(out List<PieceData> spawnedPieces)
        {
            var fallInfoList = new List<PieceFallInfo>();
            spawnedPieces = new List<PieceData>();
            var random = new Random();
            var availableColors = _levelSettings.AvailableColors;

            for (int x = 0; x < Grid.Width; x++)
            {
                int emptySpaces = 0;
                for (int y = 0; y < Grid.Height; y++)
                {
                    var current = Grid.Get(x, y);
                    if (current == null)
                    {
                        emptySpaces++;
                    }
                    else if (emptySpaces > 0)
                    {
                        Grid.Set(x, y - emptySpaces, current);
                        Grid.Set(x, y, null);
                        current.X = x;
                        current.Y = y - emptySpaces;  // keep X/Y in sync after gravity drop
                        fallInfoList.Add(new PieceFallInfo(x, y, x, y - emptySpaces));
                    }
                }

                for (int i = 0; i < emptySpaces; i++)
                {
                    int targetY = Grid.Height - emptySpaces + i;
                    int spawnY = Grid.Height + i;
                    var chosenColor = availableColors[random.Next(availableColors.Length)];
                    var newPiece = new PieceData(x, targetY, chosenColor, PieceType.Normal);
                    Grid.Set(x, targetY, newPiece);
                    spawnedPieces.Add(newPiece);
                    fallInfoList.Add(new PieceFallInfo(x, spawnY, x, targetY));
                }
            }

            // Special pieces kept in grid during ClearMatches had their views destroyed by
            // AnimateDestroy. Add them to spawnedPieces so AnimateFall recreates their views.
            // After the gravity loop above, special.X/Y already reflect the final position.
            // If gravity moved the piece there is already a FallInfo whose ToX/ToY matches
            // the piece — AnimateFall will handle it via spawnedPieceSet.  Only add a
            // self-referencing dummy FallInfo when no such gravity entry exists (in-place case).
            foreach (var special in _keptSpecialPieces)
            {
                spawnedPieces.Add(special);

                bool gravityCoversThisPiece = false;
                foreach (var fi in fallInfoList)
                {
                    if (fi.ToX == special.X && fi.ToY == special.Y &&
                        Grid.Get(fi.ToX, fi.ToY) == special)
                    {
                        gravityCoversThisPiece = true;
                        break;
                    }
                }

                if (!gravityCoversThisPiece)
                {
                    // Piece stayed in place — self-referencing entry triggers a view spawn.
                    fallInfoList.Add(new PieceFallInfo(special.X, special.Y, special.X, special.Y));
                }
            }
            _keptSpecialPieces.Clear();

            return fallInfoList;
        }

        private void SwapInGrid(int x1, int y1, int x2, int y2)
        {
            var p1 = Grid.Get(x1, y1);
            var p2 = Grid.Get(x2, y2);

            Grid.Set(x1, y1, p2);
            Grid.Set(x2, y2, p1);

            // Keep PieceData.X/Y in sync with actual grid positions so that
            // AnimateDestroy and ClearMatches look up the correct views/cells.
            if (p1 != null) { p1.X = x2; p1.Y = y2; }
            if (p2 != null) { p2.X = x1; p2.Y = y1; }
        }

        private bool IsValidInitialColor(int x, int y, PieceColor color)
        {
            if (x >= 2)
            {
                var p1 = Grid.Get(x - 1, y);
                var p2 = Grid.Get(x - 2, y);
                if (p1 != null && p2 != null && p1.Color == color && p2.Color == color)
                {
                    return false;
                }
            }

            if (y >= 2)
            {
                var p1 = Grid.Get(x, y - 1);
                var p2 = Grid.Get(x, y - 2);
                if (p1 != null && p2 != null && p1.Color == color && p2.Color == color)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
