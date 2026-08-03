using UnityEngine;
using Match3.Model;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = "PieceSpriteConfig", menuName = "Match3/Piece Sprite Config")]
    public class PieceSpriteConfig : ScriptableObject
    {
        [Header("Layout")]
        [SerializeField, Min(0.01f)] private float _pieceSizeInCells = 0.85f;

        [System.Serializable]
        public struct PieceColorSprite
        {
            public PieceColor Color;
            public Sprite NormalSprite;
            public Sprite BombSprite;
            public Sprite HorizontalLineSprite;
            public Sprite VerticalLineSprite;
        }

        [Header("Sprites")]
        public PieceColorSprite[] ColorSprites;

        public float PieceSizeInCells => _pieceSizeInCells;

        public Sprite GetSprite(PieceColor color, PieceType type)
        {
            foreach (var item in ColorSprites)
            {
                if (item.Color == color)
                {
                    switch (type)
                    {
                        case PieceType.Bomb:
                            return item.BombSprite != null ? item.BombSprite : item.NormalSprite;
                        case PieceType.HorizontalLine:
                            return item.HorizontalLineSprite != null ? item.HorizontalLineSprite : item.NormalSprite;
                        case PieceType.VerticalLine:
                            return item.VerticalLineSprite != null ? item.VerticalLineSprite : item.NormalSprite;
                        default:
                            return item.NormalSprite;
                    }
                }
            }
            return null;
        }
    }
}
