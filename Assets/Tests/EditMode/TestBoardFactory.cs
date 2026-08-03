using Match3.Core;
using Match3.Model;

namespace Match3.Tests.EditMode
{
    internal static class TestBoardFactory
    {
        internal const int MatchLength = 3;
        internal const int LineSpecialLength = 4;
        internal const int BombSpecialLength = 5;
        internal const int BombRadius = 1;
        internal const int RandomSeed = 24680;

        private static readonly PieceColor[] Colors =
        {
            PieceColor.Red,
            PieceColor.Green,
            PieceColor.Blue,
            PieceColor.Yellow
        };

        internal static MatchDetector CreateDetector()
        {
            return new MatchDetector(
                new MatchRules(MatchLength, LineSpecialLength, BombSpecialLength));
        }

        internal static BoardSystem CreateBoard(int width, int height, int seed = RandomSeed)
        {
            var configuration = new BoardConfiguration(
                width,
                height,
                Colors,
                seed,
                MatchLength,
                BombRadius);
            return new BoardSystem(configuration, CreateDetector());
        }

        internal static PieceData SetPiece(
            GridData grid,
            int x,
            int y,
            PieceColor color,
            PieceType type = PieceType.Normal)
        {
            var piece = new PieceData(x, y, color, type);
            grid.Set(x, y, piece);
            return piece;
        }

        internal static void FillPattern(GridData grid)
        {
            var colors = new[] { PieceColor.Red, PieceColor.Green, PieceColor.Blue };
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    int colorIndex = (x + y * 2) % colors.Length;
                    SetPiece(grid, x, y, colors[colorIndex]);
                }
            }
        }
    }
}
