using UnityEngine;
using Match3.Model;

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

        [Header("Available Pieces")]
        public PieceColor[] AvailableColors = {
            PieceColor.Red,
            PieceColor.Green,
            PieceColor.Blue,
            PieceColor.Yellow,
            PieceColor.Purple
        };
    }
}
