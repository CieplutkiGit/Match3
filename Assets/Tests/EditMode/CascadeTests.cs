using Match3.Model;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class CascadeTests
    {
        [Test]
        public void GravityCreatesASecondMatchAfterResolution()
        {
            var board = TestBoardFactory.CreateBoard(3, 4);
            PieceColor[,] colors =
            {
                { PieceColor.Blue, PieceColor.Red, PieceColor.Blue, PieceColor.Blue },
                { PieceColor.Green, PieceColor.Red, PieceColor.Yellow, PieceColor.Green },
                { PieceColor.Yellow, PieceColor.Red, PieceColor.Green, PieceColor.Yellow }
            };

            for (int x = 0; x < board.Grid.Width; x++)
            {
                for (int y = 0; y < board.Grid.Height; y++)
                    TestBoardFactory.SetPiece(board.Grid, x, y, colors[x, y]);
            }

            var detector = TestBoardFactory.CreateDetector();
            var firstMatches = detector.FindMatches(board.Grid);
            var resolution = board.ResolveMatches(firstMatches);
            board.ApplyGravityAndRefill(out _);
            var cascadeMatches = detector.FindMatches(board.Grid);

            Assert.AreEqual(TestBoardFactory.MatchLength, resolution.ClearedPieces.Count);
            Assert.IsTrue(ContainsVerticalBlueMatch(cascadeMatches));
        }

        private static bool ContainsVerticalBlueMatch(System.Collections.Generic.IEnumerable<MatchResult> matches)
        {
            foreach (var match in matches)
            {
                int matchingPieces = 0;
                foreach (var piece in match.MatchedPieces)
                {
                    if (piece.X == 0 && piece.Color == PieceColor.Blue)
                        matchingPieces++;
                }

                if (matchingPieces >= TestBoardFactory.MatchLength)
                    return true;
            }

            return false;
        }
    }
}
