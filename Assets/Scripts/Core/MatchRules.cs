using System;

namespace Match3.Core
{
    public sealed class MatchRules
    {
        public int MinimumMatchLength { get; }
        public int LineSpecialLength { get; }
        public int BombSpecialLength { get; }

        public MatchRules(int minimumMatchLength, int lineSpecialLength, int bombSpecialLength)
        {
            if (minimumMatchLength < 2)
                throw new ArgumentOutOfRangeException(nameof(minimumMatchLength));
            if (lineSpecialLength <= minimumMatchLength)
                throw new ArgumentOutOfRangeException(nameof(lineSpecialLength));
            if (bombSpecialLength <= lineSpecialLength)
                throw new ArgumentOutOfRangeException(nameof(bombSpecialLength));

            MinimumMatchLength = minimumMatchLength;
            LineSpecialLength = lineSpecialLength;
            BombSpecialLength = bombSpecialLength;
        }
    }
}
