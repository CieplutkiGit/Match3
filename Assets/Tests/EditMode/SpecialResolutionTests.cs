using Match3.Model;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialResolutionTests
    {
        [Test]
        public void CreatedSpecialIsKeptWithoutBeingActivated()
        {
            var board = TestBoardFactory.CreateBoard(5, 3);
            for (int x = 0; x < TestBoardFactory.LineSpecialLength; x++)
                TestBoardFactory.SetPiece(board.Grid, x, 1, PieceColor.Red);

            var matches = TestBoardFactory.CreateDetector().FindMatches(board.Grid);
            var focus = new GridPosition(2, 1);
            var resolution = board.ResolveMatches(matches, focus);

            Assert.AreEqual(1, resolution.CreatedSpecials.Count);
            Assert.AreEqual(0, resolution.ActivatedSpecials.Count);
            Assert.AreEqual(TestBoardFactory.LineSpecialLength - 1, resolution.ClearedPieces.Count);
            Assert.AreEqual(PieceType.HorizontalLine, resolution.CreatedSpecials[0].Type);
            Assert.AreSame(resolution.CreatedSpecials[0].Piece, board.Grid.Get(focus.X, focus.Y));
        }

        [Test]
        public void ChainedSpecialsActivateInQueueOrder()
        {
            var board = TestBoardFactory.CreateBoard(5, 5);
            TestBoardFactory.FillPattern(board.Grid);

            TestBoardFactory.SetPiece(
                board.Grid,
                0,
                2,
                PieceColor.Red,
                PieceType.HorizontalLine);
            TestBoardFactory.SetPiece(board.Grid, 1, 2, PieceColor.Red);
            TestBoardFactory.SetPiece(board.Grid, 2, 2, PieceColor.Red);
            TestBoardFactory.SetPiece(
                board.Grid,
                3,
                2,
                PieceColor.Blue,
                PieceType.VerticalLine);
            TestBoardFactory.SetPiece(
                board.Grid,
                3,
                4,
                PieceColor.Green,
                PieceType.Bomb);

            var matches = TestBoardFactory.CreateDetector().FindMatches(board.Grid);
            var resolution = board.ResolveMatches(matches);

            Assert.AreEqual(3, resolution.ActivatedSpecials.Count);
            Assert.AreEqual(PieceType.HorizontalLine, resolution.ActivatedSpecials[0].Type);
            Assert.AreEqual(PieceType.VerticalLine, resolution.ActivatedSpecials[1].Type);
            Assert.AreEqual(PieceType.Bomb, resolution.ActivatedSpecials[2].Type);
            Assert.AreEqual(0, resolution.CreatedSpecials.Count);
            Assert.IsNull(board.Grid.Get(4, 4));
        }
    }
}
