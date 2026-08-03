using System;
using Match3.Model;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class GridDataTests
    {
        [Test]
        public void ConstructorRejectsInvalidDimensions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridData(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridData(1, 0));
        }

        [Test]
        public void SetStoresPieceAndSynchronizesCoordinates()
        {
            var grid = new GridData(3, 4);
            var piece = new PieceData(0, 0, PieceColor.Red, PieceType.Normal);

            grid.Set(2, 3, piece);

            Assert.AreSame(piece, grid.Get(2, 3));
            Assert.AreEqual(2, piece.X);
            Assert.AreEqual(3, piece.Y);
        }

        [Test]
        public void GetOutsideGridReturnsNullAndSetThrows()
        {
            var grid = new GridData(3, 3);

            Assert.IsNull(grid.Get(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(3, 0, null));
        }
    }
}
