using System;
using System.Collections.Generic;
using Match3.Model;

namespace Match3.Core
{
    public sealed class BoardConfiguration
    {
        private const int MinimumColorCount = 3;

        private readonly List<PieceColor> _availableColors;

        public int Width { get; }
        public int Height { get; }
        public int RandomSeed { get; }
        public int InitialMatchLength { get; }
        public int BombRadius { get; }
        public IReadOnlyList<PieceColor> AvailableColors => _availableColors;

        public BoardConfiguration(
            int width,
            int height,
            IEnumerable<PieceColor> availableColors,
            int randomSeed,
            int initialMatchLength,
            int bombRadius)
        {
            if (width < initialMatchLength)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < initialMatchLength)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (initialMatchLength < 2)
                throw new ArgumentOutOfRangeException(nameof(initialMatchLength));
            if (bombRadius < 1)
                throw new ArgumentOutOfRangeException(nameof(bombRadius));
            if (availableColors == null)
                throw new ArgumentNullException(nameof(availableColors));

            _availableColors = new List<PieceColor>();
            var uniqueColors = new HashSet<PieceColor>();
            foreach (var color in availableColors)
            {
                if (color == PieceColor.None)
                    throw new ArgumentException("Available colors cannot contain None.", nameof(availableColors));
                if (!uniqueColors.Add(color))
                    throw new ArgumentException("Available colors must be unique.", nameof(availableColors));

                _availableColors.Add(color);
            }

            if (_availableColors.Count < MinimumColorCount)
                throw new ArgumentException("At least three colors are required.", nameof(availableColors));

            Width = width;
            Height = height;
            RandomSeed = randomSeed;
            InitialMatchLength = initialMatchLength;
            BombRadius = bombRadius;
        }
    }
}
