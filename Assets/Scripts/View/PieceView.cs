using UnityEngine;
using Match3.Model;

namespace Match3.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public PieceColor Color { get; private set; }
        public PieceType Type { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Setup(int x, int y, PieceColor color, PieceType type, Sprite sprite)
        {
            X = x;
            Y = y;
            Color = color;
            Type = type;
            _spriteRenderer.sprite = sprite;
        }

        public void UpdatePosition(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
