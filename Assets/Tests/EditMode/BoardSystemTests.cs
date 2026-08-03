using System;
using Match3.Core;
using Match3.Model;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class BoardSystemTests
    {
        private static readonly PieceColor[] Colors =
        {
            PieceColor.Red,
            PieceColor.Green,
            PieceColor.Blue
        };

        [Test]
        public void ValidSwapRemainsApplied()
        {
            var board = CreateSwapBoard();

            bool valid = board.TrySwap(0, 2, 1, 2, out var matches);

            Assert.IsTrue(valid);
            Assert.IsNotEmpty(matches);
            Assert.AreEqual(PieceColor.Red, board.Grid.Get(0, 2).Color);
            Assert.AreEqual(PieceColor.Blue, board.Grid.Get(1, 2).Color);
        }

        [Test]
        public void InvalidSwapRestoresOriginalPositions()
        {
            var board = CreateSwapBoard();
            var first = board.Grid.Get(1, 2);
            var second = board.Grid.Get(2, 2);

            bool valid = board.TrySwap(1, 2, 2, 2, out var matches);

            Assert.IsFalse(valid);
            Assert.IsEmpty(matches);
            Assert.AreSame(first, board.Grid.Get(1, 2));
            Assert.AreSame(second, board.Grid.Get(2, 2));
            Assert.AreEqual(1, first.X);
            Assert.AreEqual(2, second.X);
        }

        [Test]
        public void GravityCompactsPiecesAndRefillsEveryCell()
        {
            var board = TestBoardFactory.CreateBoard(3, 4);
            var lower = TestBoardFactory.SetPiece(board.Grid, 0, 1, PieceColor.Red);
            var upper = TestBoardFactory.SetPiece(
                board.Grid,
                0,
                3,
                PieceColor.Blue,
                PieceType.Bomb);

            var falls = board.ApplyGravityAndRefill(out var spawned);

            Assert.AreSame(lower, board.Grid.Get(0, 0));
            Assert.AreSame(upper, board.Grid.Get(0, 1));
            Assert.AreEqual(0, lower.Y);
            Assert.AreEqual(1, upper.Y);
            Assert.AreEqual(10, spawned.Count);
            Assert.AreEqual(12, falls.Count);
            AssertGridIsFull(board.Grid);
        }

        [Test]
        public void EqualSeedsProduceEqualBoardsAndRefills()
        {
            var first = TestBoardFactory.CreateBoard(5, 5, TestBoardFactory.RandomSeed);
            var second = TestBoardFactory.CreateBoard(5, 5, TestBoardFactory.RandomSeed);

            first.FillBoard();
            second.FillBoard();
            AssertSameColors(first.Grid, second.Grid);

            first.Grid.Set(0, 0, null);
            second.Grid.Set(0, 0, null);
            first.ApplyGravityAndRefill(out var firstSpawned);
            second.ApplyGravityAndRefill(out var secondSpawned);

            Assert.AreEqual(firstSpawned.Count, secondSpawned.Count);
            AssertSameColors(first.Grid, second.Grid);
        }

        [Test]
        public void ConfigurationRejectsDuplicateAndMissingColors()
        {
            Assert.Throws<ArgumentException>(() => new BoardConfiguration(
                3,
                3,
                new[] { PieceColor.Red, PieceColor.Red },
                TestBoardFactory.RandomSeed,
                TestBoardFactory.MatchLength,
                TestBoardFactory.BombRadius));

            Assert.Throws<ArgumentException>(() => new BoardConfiguration(
                3,
                3,
                new[] { PieceColor.Red, PieceColor.Green },
                TestBoardFactory.RandomSeed,
                TestBoardFactory.MatchLength,
                TestBoardFactory.BombRadius));
        }

        private static BoardSystem CreateSwapBoard()
        {
            var board = TestBoardFactory.CreateBoard(3, 3);
            PieceColor[,] colors =
            {
                { PieceColor.Red, PieceColor.Red, PieceColor.Blue },
                { PieceColor.Green, PieceColor.Blue, PieceColor.Red },
                { PieceColor.Blue, PieceColor.Green, PieceColor.Yellow }
            };

            for (int x = 0; x < board.Grid.Width; x++)
            {
                for (int y = 0; y < board.Grid.Height; y++)
                    TestBoardFactory.SetPiece(board.Grid, x, y, colors[x, y]);
            }

            return board;
        }

        private static void AssertGridIsFull(GridData grid)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                    Assert.IsNotNull(grid.Get(x, y));
            }
        }

        private static void AssertSameColors(GridData first, GridData second)
        {
            Assert.AreEqual(first.Width, second.Width);
            Assert.AreEqual(first.Height, second.Height);

            for (int x = 0; x < first.Width; x++)
            {
                for (int y = 0; y < first.Height; y++)
                    Assert.AreEqual(first.Get(x, y).Color, second.Get(x, y).Color);
            }
        }
    }
}
