using System.Collections.Generic;
using UnityEngine;
using Match3.Model;
using Match3.Core;
using Match3.Data;

namespace Match3.View
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private PieceView _piecePrefab;
        [SerializeField] private PieceSpriteConfig _spriteConfig;
        [SerializeField] private float _spacing = 1.0f;

        private BoardSystem _boardSystem;
        private PieceView[,] _pieceViews;

        public void Initialize(BoardSystem boardSystem)
        {
            _boardSystem = boardSystem;
            _pieceViews = new PieceView[_boardSystem.Grid.Width, _boardSystem.Grid.Height];
            
            SpawnInitialVisuals();
        }

        private void SpawnInitialVisuals()
        {
            var grid = _boardSystem.Grid;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var data = grid.Get(x, y);
                    if (data != null && data.Color != PieceColor.None)
                    {
                        SpawnPieceView(x, y, data.Color, data.Type);
                    }
                }
            }
        }

        private void SpawnPieceView(int x, int y, PieceColor color, PieceType type)
        {
            var worldPos = GetWorldPosition(x, y);
            var pieceInstance = Instantiate(_piecePrefab, worldPos, Quaternion.identity, transform);
            var sprite = _spriteConfig.GetSprite(color, type);
            pieceInstance.Setup(x, y, color, type, sprite);
            _pieceViews[x, y] = pieceInstance;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            float xPos = (x - (_boardSystem.Grid.Width - 1) * 0.5f) * _spacing;
            float yPos = (y - (_boardSystem.Grid.Height - 1) * 0.5f) * _spacing;
            return new Vector3(xPos, yPos, 0f) + transform.position;
        }

        public PieceView GetPieceView(int x, int y)
        {
            if (x >= 0 && x < _pieceViews.GetLength(0) && y >= 0 && y < _pieceViews.GetLength(1))
            {
                return _pieceViews[x, y];
            }
            return null;
        }
    }
}
