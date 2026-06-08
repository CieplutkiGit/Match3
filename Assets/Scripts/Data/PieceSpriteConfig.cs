using UnityEngine;
using Match3.Model;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = "PieceSpriteConfig", menuName = "Match3/Piece Sprite Config")]
    public class PieceSpriteConfig : ScriptableObject
    {
        [System.Serializable]
        public struct PieceColorSprite
        {
            public PieceColor Color;
            public Sprite NormalSprite;
            public Sprite BombSprite;
            public Sprite HorizontalLineSprite;
            public Sprite VerticalLineSprite;
        }

        public PieceColorSprite[] ColorSprites;

        public Sprite GetSprite(PieceColor color, PieceType type)
        {
            foreach (var item in ColorSprites)
            {
                if (item.Color == color)
                {
                    switch (type)
                    {
                        case PieceType.Normal: 
                            return item.NormalSprite;
                        case PieceType.Bomb: 
                            return item.BombSprite ?? item.NormalSprite;
                        case PieceType.HorizontalLine: 
                            return item.HorizontalLineSprite ?? item.NormalSprite;
                        case PieceType.VerticalLine: 
                            return item.VerticalLineSprite ?? item.NormalSprite;
                    }
                }
            }
            return null;
        }
    }
}
