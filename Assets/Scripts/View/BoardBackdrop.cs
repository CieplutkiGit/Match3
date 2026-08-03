using System;
using DG.Tweening;
using Match3.Data;
using UnityEngine;

namespace Match3.View
{
    public sealed class BoardBackdrop
    {
        public void Build(
            Transform parent,
            int width,
            int height,
            Func<int, int, Vector3> cellPosition,
            float cellSize,
            BoardSpriteConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Vector3 first = cellPosition(0, 0);
            Vector3 last = cellPosition(width - 1, height - 1);
            Vector3 center = (first + last) * 0.5f;
            float boardWidth = Mathf.Abs(last.x - first.x) + cellSize;
            float boardHeight = Mathf.Abs(last.y - first.y) + cellSize;

            BuildBackground(parent, config);
            BuildGlow(parent, center, Mathf.Max(boardWidth, boardHeight), config);
            BuildFrame(parent, center, boardWidth, boardHeight, cellSize, config);
            BuildCells(parent, width, height, cellPosition, cellSize, config);
        }

        private static void BuildBackground(Transform parent, BoardSpriteConfig config)
        {
            var camera = Camera.main;
            float height = camera != null && camera.orthographic
                ? camera.orthographicSize * 2f
                : config.FallbackBackgroundHeight;
            float width = camera != null
                ? height * camera.aspect
                : config.FallbackBackgroundWidth;
            Vector3 position = camera != null
                ? new Vector3(camera.transform.position.x, camera.transform.position.y, 0f)
                : Vector3.zero;
            Sprite sprite = config.BackgroundSprite != null
                ? config.BackgroundSprite
                : ProceduralSprites.VerticalGradient(
                    config.BackgroundTopColor,
                    config.BackgroundBottomColor);

            CreateRenderer(
                parent,
                "Background",
                sprite,
                config.BackgroundTint,
                config.BackgroundSortingOrder,
                position,
                width * config.BackgroundScale,
                height * config.BackgroundScale,
                config.BackgroundDrawMode);
        }

        private static void BuildGlow(
            Transform parent,
            Vector3 center,
            float size,
            BoardSpriteConfig config)
        {
            Sprite sprite = config.GlowSprite != null
                ? config.GlowSprite
                : ProceduralSprites.Circle();
            Transform glow = CreateRenderer(
                parent,
                "BoardGlow",
                sprite,
                config.GlowColor,
                config.GlowSortingOrder,
                center,
                size * config.GlowScale,
                size * config.GlowScale,
                config.GlowDrawMode);
            Vector3 baseScale = glow.localScale;
            glow.DOScale(baseScale * config.GlowPulseScale, config.GlowPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private static void BuildFrame(
            Transform parent,
            Vector3 center,
            float boardWidth,
            float boardHeight,
            float cellSize,
            BoardSpriteConfig config)
        {
            Sprite fallback = ProceduralSprites.RoundedSquare();
            Sprite rimSprite = config.RimSprite != null ? config.RimSprite : fallback;
            Sprite panelSprite = config.PanelSprite != null ? config.PanelSprite : fallback;

            CreateRenderer(
                parent,
                "BoardRim",
                rimSprite,
                config.RimColor,
                config.RimSortingOrder,
                center,
                boardWidth + cellSize * config.RimPadding,
                boardHeight + cellSize * config.RimPadding,
                config.FrameDrawMode);
            CreateRenderer(
                parent,
                "BoardPanel",
                panelSprite,
                config.PanelColor,
                config.PanelSortingOrder,
                center,
                boardWidth + cellSize * config.PanelPadding,
                boardHeight + cellSize * config.PanelPadding,
                config.FrameDrawMode);
        }

        private static void BuildCells(
            Transform parent,
            int width,
            int height,
            Func<int, int, Vector3> cellPosition,
            float cellSize,
            BoardSpriteConfig config)
        {
            Sprite fallback = ProceduralSprites.RoundedSquare();
            Sprite lightSprite = config.LightCellSprite != null ? config.LightCellSprite : fallback;
            Sprite darkSprite = config.DarkCellSprite != null ? config.DarkCellSprite : lightSprite;
            float size = cellSize * config.CellScale;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool lightCell = (x + y) % 2 == 0;
                    CreateRenderer(
                        parent,
                        "Cell",
                        lightCell ? lightSprite : darkSprite,
                        lightCell ? config.LightCellColor : config.DarkCellColor,
                        config.CellSortingOrder,
                        cellPosition(x, y),
                        size,
                        size,
                        config.CellDrawMode);
                }
            }
        }

        private static Transform CreateRenderer(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            int sortingOrder,
            Vector3 position,
            float width,
            float height,
            SpriteDrawMode drawMode)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.position = position;

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = drawMode;

            if (drawMode == SpriteDrawMode.Simple)
            {
                Vector3 bounds = sprite.bounds.size;
                gameObject.transform.localScale = new Vector3(
                    width / bounds.x,
                    height / bounds.y,
                    1f);
            }
            else
            {
                renderer.size = new Vector2(width, height);
            }

            return gameObject.transform;
        }
    }
}
