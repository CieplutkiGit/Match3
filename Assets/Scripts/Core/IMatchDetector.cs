using System.Collections.Generic;
using Match3.Model;

namespace Match3.Core
{
    public interface IMatchDetector
    {
        List<MatchResult> FindMatches(IGrid<PieceData> grid);
    }
}
