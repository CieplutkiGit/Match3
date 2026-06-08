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

        private void SwapInGrid(int x1, int y1, int x2, int y2)
        {
            var p1 = Grid.Get(x1, y1);
            var p2 = Grid.Get(x2, y2);

            Grid.Set(x1, y1, p2);
            Grid.Set(x2, y2, p1);
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
