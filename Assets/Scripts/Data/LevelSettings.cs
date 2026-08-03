using UnityEngine;
using Match3.Model;
using Match3.Core;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = "NewLevelSettings", menuName = "Match3/Level Settings")]
    public class LevelSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        [Min(3)] public int GridWidth = 8;
        [Min(3)] public int GridHeight = 8;

        [Header("Gameplay Settings")]
        public int MaxMoves = 20;
        public int TargetScore = 1000;
        public int PointsPerPiece = 10;

        [Header("Generation Settings")]
        public int RandomSeed = 12345;

        [Header("Match Rules")]
        [Min(2)] public int MinimumMatchLength = 3;
        [Min(3)] public int LineSpecialLength = 4;
        [Min(4)] public int BombSpecialLength = 5;
        [Min(1)] public int BombRadius = 1;

        [Header("Available Pieces")]
        public PieceColor[] AvailableColors = {
            PieceColor.Red,
            PieceColor.Green,
            PieceColor.Blue,
            PieceColor.Yellow,
            PieceColor.Purple
        };

        public BoardConfiguration CreateBoardConfiguration()
        {
            ValidateGameplaySettings();
            return new BoardConfiguration(
                GridWidth,
                GridHeight,
                AvailableColors,
                RandomSeed,
                MinimumMatchLength,
                BombRadius);
        }

        public MatchRules CreateMatchRules()
        {
            ValidateGameplaySettings();
            return new MatchRules(MinimumMatchLength, LineSpecialLength, BombSpecialLength);
        }

        public void ValidateGameplaySettings()
        {
            if (MaxMoves <= 0)
                throw new System.InvalidOperationException("Max moves must be greater than zero.");
            if (TargetScore <= 0)
                throw new System.InvalidOperationException("Target score must be greater than zero.");
            if (PointsPerPiece <= 0)
                throw new System.InvalidOperationException("Points per piece must be greater than zero.");
        }
    }
}
