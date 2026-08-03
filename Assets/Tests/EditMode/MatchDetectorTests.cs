using Match3.Model;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class MatchDetectorTests
    {
        [Test]
        public void HorizontalRunProducesHorizontalSpecialCreation()
        {
            var grid = new GridData(5, 3);
            for (int x = 0; x < TestBoardFactory.LineSpecialLength; x++)
                TestBoardFactory.SetPiece(grid, x, 1, PieceColor.Red);

            var matches = TestBoardFactory.CreateDetector().FindMatches(grid);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(TestBoardFactory.LineSpecialLength, matches[0].MatchedPieces.Count);
            Assert.AreEqual(PieceType.HorizontalLine, matches[0].SpecialCreationType);
        }

        [Test]
        public void IntersectingRunsProduceSingleBombCreation()
        {
            var grid = new GridData(5, 5);
            for (int coordinate = 1; coordinate <= TestBoardFactory.MatchLength; coordinate++)
            {
                TestBoardFactory.SetPiece(grid, coordinate, 2, PieceColor.Blue);
                TestBoardFactory.SetPiece(grid, 2, coordinate, PieceColor.Blue);
            }

            var matches = TestBoardFactory.CreateDetector().FindMatches(grid);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(5, matches[0].MatchedPieces.Count);
            Assert.AreEqual(PieceType.Bomb, matches[0].SpecialCreationType);
        }

        [Test]
        public void EmptyCellsSeparateRuns()
        {
            var grid = new GridData(5, 3);
            TestBoardFactory.SetPiece(grid, 0, 1, PieceColor.Green);
            TestBoardFactory.SetPiece(grid, 1, 1, PieceColor.Green);
            TestBoardFactory.SetPiece(grid, 3, 1, PieceColor.Green);
            TestBoardFactory.SetPiece(grid, 4, 1, PieceColor.Green);

            var matches = TestBoardFactory.CreateDetector().FindMatches(grid);

            Assert.IsEmpty(matches);
        }
    }
}
