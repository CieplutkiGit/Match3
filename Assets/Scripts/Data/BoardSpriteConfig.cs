using UnityEngine;

namespace Match3.Data
{
    [CreateAssetMenu(fileName = "BoardSpriteConfig", menuName = "Match3/Board Sprite Config")]
    public sealed class BoardSpriteConfig : ScriptableObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite _backgroundSprite;
        [SerializeField] private Sprite _glowSprite;
        [SerializeField] private Sprite _rimSprite;
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Sprite _lightCellSprite;
        [SerializeField] private Sprite _darkCellSprite;

        [Header("Draw Modes")]
        [SerializeField] private SpriteDrawMode _backgroundDrawMode = SpriteDrawMode.Simple;
        [SerializeField] private SpriteDrawMode _glowDrawMode = SpriteDrawMode.Simple;
        [SerializeField] private SpriteDrawMode _frameDrawMode = SpriteDrawMode.Simple;
        [SerializeField] private SpriteDrawMode _cellDrawMode = SpriteDrawMode.Simple;

        [Header("Colors")]
        [SerializeField] private Color _backgroundTopColor = new Color(0.06f, 0.05f, 0.11f, 1f);
        [SerializeField] private Color _backgroundBottomColor = new Color(0.14f, 0.11f, 0.22f, 1f);
        [SerializeField] private Color _backgroundTint = Color.white;
        [SerializeField] private Color _glowColor = new Color(0.32f, 0.55f, 0.95f, 0.16f);
        [SerializeField] private Color _rimColor = new Color(0.30f, 0.36f, 0.55f, 1f);
        [SerializeField] private Color _panelColor = new Color(0.09f, 0.10f, 0.17f, 1f);
        [SerializeField] private Color _lightCellColor = new Color(0.21f, 0.24f, 0.36f, 1f);
        [SerializeField] private Color _darkCellColor = new Color(0.16f, 0.18f, 0.28f, 1f);

        [Header("Layout")]
        [SerializeField, Min(0f)] private float _fallbackBackgroundWidth = 20f;
        [SerializeField, Min(0f)] private float _fallbackBackgroundHeight = 12f;
        [SerializeField, Min(0f)] private float _backgroundScale = 1.3f;
        [SerializeField, Min(0f)] private float _glowScale = 2.4f;
        [SerializeField, Min(0f)] private float _rimPadding = 0.55f;
        [SerializeField, Min(0f)] private float _panelPadding = 0.3f;
        [SerializeField, Min(0f)] private float _cellScale = 0.92f;

        [Header("Glow Animation")]
        [SerializeField, Min(0f)] private float _glowPulseScale = 1.07f;
        [SerializeField, Min(0f)] private float _glowPulseDuration = 2.6f;

        [Header("Sorting")]
        [SerializeField] private int _backgroundSortingOrder = -100;
        [SerializeField] private int _glowSortingOrder = -50;
        [SerializeField] private int _rimSortingOrder = -8;
        [SerializeField] private int _panelSortingOrder = -7;
        [SerializeField] private int _cellSortingOrder = -6;

        public Sprite BackgroundSprite => _backgroundSprite;
        public Sprite GlowSprite => _glowSprite;
        public Sprite RimSprite => _rimSprite;
        public Sprite PanelSprite => _panelSprite;
        public Sprite LightCellSprite => _lightCellSprite;
        public Sprite DarkCellSprite => _darkCellSprite;
        public SpriteDrawMode BackgroundDrawMode => _backgroundDrawMode;
        public SpriteDrawMode GlowDrawMode => _glowDrawMode;
        public SpriteDrawMode FrameDrawMode => _frameDrawMode;
        public SpriteDrawMode CellDrawMode => _cellDrawMode;
        public Color BackgroundTopColor => _backgroundTopColor;
        public Color BackgroundBottomColor => _backgroundBottomColor;
        public Color BackgroundTint => _backgroundTint;
        public Color GlowColor => _glowColor;
        public Color RimColor => _rimColor;
        public Color PanelColor => _panelColor;
        public Color LightCellColor => _lightCellColor;
        public Color DarkCellColor => _darkCellColor;
        public float FallbackBackgroundWidth => _fallbackBackgroundWidth;
        public float FallbackBackgroundHeight => _fallbackBackgroundHeight;
        public float BackgroundScale => _backgroundScale;
        public float GlowScale => _glowScale;
        public float RimPadding => _rimPadding;
        public float PanelPadding => _panelPadding;
        public float CellScale => _cellScale;
        public float GlowPulseScale => _glowPulseScale;
        public float GlowPulseDuration => _glowPulseDuration;
        public int BackgroundSortingOrder => _backgroundSortingOrder;
        public int GlowSortingOrder => _glowSortingOrder;
        public int RimSortingOrder => _rimSortingOrder;
        public int PanelSortingOrder => _panelSortingOrder;
        public int CellSortingOrder => _cellSortingOrder;
    }
}
