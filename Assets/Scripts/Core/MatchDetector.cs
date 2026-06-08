using System.Collections.Generic;
using Match3.Model;

namespace Match3.Core
{
    public class MatchDetector : IMatchDetector
    {
        private class RawRun
        {
            public List<PieceData> Pieces { get; } = new List<PieceData>();
            public bool IsHorizontal { get; }

            public RawRun(List<PieceData> pieces, bool isHorizontal)
            {
                Pieces = pieces;
                IsHorizontal = isHorizontal;
            }
        }

        public List<MatchResult> FindMatches(IGrid<PieceData> grid)
        {
            var rawRuns = new List<RawRun>();

            for (int y = 0; y < grid.Height; y++)
            {
                int runLength = 1;
                for (int x = 0; x < grid.Width; x++)
                {
                    var current = grid.Get(x, y);
                    if (current == null || current.Color == PieceColor.None)
                    {
                        runLength = 1;
                        continue;
                    }

                    if (x < grid.Width - 1)
                    {
                        var next = grid.Get(x + 1, y);
                        if (next != null && next.Color == current.Color)
                        {
                            runLength++;
                        }
                        else
                        {
                            if (runLength >= 3)
                            {
                                var runPieces = new List<PieceData>();
                                for (int i = 0; i < runLength; i++)
                                {
                                    runPieces.Add(grid.Get(x - i, y));
                                }
                                rawRuns.Add(new RawRun(runPieces, true));
                            }
                            runLength = 1;
                        }
                    }
                    else
                    {
                        if (runLength >= 3)
                        {
                            var runPieces = new List<PieceData>();
                            for (int i = 0; i < runLength; i++)
                            {
                                runPieces.Add(grid.Get(x - i, y));
                            }
                            rawRuns.Add(new RawRun(runPieces, true));
                        }
                        runLength = 1;
                    }
                }
            }

            for (int x = 0; x < grid.Width; x++)
            {
                int runLength = 1;
                for (int y = 0; y < grid.Height; y++)
                {
                    var current = grid.Get(x, y);
                    if (current == null || current.Color == PieceColor.None)
                    {
                        runLength = 1;
                        continue;
                    }

                    if (y < grid.Height - 1)
                    {
                        var next = grid.Get(x, y + 1);
                        if (next != null && next.Color == current.Color)
                        {
                            runLength++;
                        }
                        else
                        {
                            if (runLength >= 3)
                            {
                                var runPieces = new List<PieceData>();
                                for (int i = 0; i < runLength; i++)
                                {
                                    runPieces.Add(grid.Get(x, y - i));
                                }
                                rawRuns.Add(new RawRun(runPieces, false));
                            }
                            runLength = 1;
                        }
                    }
                    else
                    {
                        if (runLength >= 3)
                        {
                            var runPieces = new List<PieceData>();
                            for (int i = 0; i < runLength; i++)
                            {
                                runPieces.Add(grid.Get(x, y - i));
                            }
                            rawRuns.Add(new RawRun(runPieces, false));
                        }
                        runLength = 1;
                    }
                }
            }

            var groups = new List<List<RawRun>>();

            foreach (var run in rawRuns)
            {
                var intersectingGroups = new List<List<RawRun>>();
                foreach (var group in groups)
                {
                    if (GroupIntersectsWithRun(group, run))
                    {
                        intersectingGroups.Add(group);
                    }
                }

                if (intersectingGroups.Count == 0)
                {
                    groups.Add(new List<RawRun> { run });
                }
                else
                {
                    var mergedGroup = new List<RawRun> { run };
                    foreach (var group in intersectingGroups)
                    {
                        mergedGroup.AddRange(group);
                        groups.Remove(group);
                    }
                    groups.Add(mergedGroup);
                }
            }

            var results = new List<MatchResult>();
            foreach (var group in groups)
            {
                var matchResult = new MatchResult();
                bool hasHorizontal = false;
                bool hasVertical = false;
                int maxHorizontalLen = 0;
                int maxVerticalLen = 0;

                foreach (var run in group)
                {
                    if (run.IsHorizontal)
                    {
                        hasHorizontal = true;
                        if (run.Pieces.Count > maxHorizontalLen) maxHorizontalLen = run.Pieces.Count;
                    }
                    else
                    {
                        hasVertical = true;
                        if (run.Pieces.Count > maxVerticalLen) maxVerticalLen = run.Pieces.Count;
                    }

                    foreach (var piece in run.Pieces)
                    {
                        matchResult.AddPiece(piece);
                    }
                }

                PieceType specialType = PieceType.Normal;
                if (hasHorizontal && hasVertical)
                {
                    specialType = PieceType.Bomb;
                }
                else if (maxHorizontalLen >= 5 || maxVerticalLen >= 5)
                {
                    specialType = PieceType.Bomb;
                }
                else if (maxHorizontalLen == 4)
                {
                    specialType = PieceType.HorizontalLine;
                }
                else if (maxVerticalLen == 4)
                {
                    specialType = PieceType.VerticalLine;
                }

                matchResult.GeneratedSpecialType = specialType;
                results.Add(matchResult);
            }

            return results;
        }

        private bool GroupIntersectsWithRun(List<RawRun> group, RawRun run)
        {
            foreach (var groupRun in group)
            {
                if (RunsIntersect(groupRun, run)) return true;
            }
            return false;
        }

        private bool RunsIntersect(RawRun r1, RawRun r2)
        {
            foreach (var p1 in r1.Pieces)
            {
                foreach (var p2 in r2.Pieces)
                {
                    if (p1 == p2) return true;
                }
            }
            return false;
        }
    }
}
