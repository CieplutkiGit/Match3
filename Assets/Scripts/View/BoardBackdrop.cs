using System;
using DG.Tweening;
using UnityEngine;

namespace Match3.View
{
    public class BoardBackdrop
    {
        public void Build(Transform parent, int width, int height, Func<int, int, Vector3> cellPos, float cell)
        {
            Vector3 first = cellPos(0, 0);
            Vector3 last = cellPos(width - 1, height - 1);
            Vector3 center = (first + last) * 0.5f;
            float boardW = Mathf.Abs(last.x - first.x) + cell;
            float boardH = Mathf.Abs(last.y - first.y) + cell;

            BuildBackground(parent);
            BuildGlow(parent, center, Mathf.Max(boardW, boardH));
            BuildFrame(parent, center, boardW, boardH, cell);
            BuildCells(parent, width, height, cellPos, cell);
        }

        private void BuildBackground(Transform parent)
        {
            var cam = Camera.main;
            float h = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 12f;
            float w = cam != null ? h * cam.aspect : 20f;
            Vector3 pos = cam != null ? new Vector3(cam.transform.position.x, cam.transform.position.y, 0f) : Vector3.zero;

            Sprite sprite = ProceduralSprites.VerticalGradient(new Color(0.06f, 0.05f, 0.11f), new Color(0.14f, 0.11f, 0.22f));
            Sized(parent, "Background", sprite, Color.white, -100, pos, w * 1.3f, h * 1.3f);
        }

        private void BuildGlow(Transform parent, Vector3 center, float size)
        {
            Transform glow = Sized(parent, "BoardGlow", ProceduralSprites.Circle(),
                new Color(0.32f, 0.55f, 0.95f, 0.16f), -50, center, size * 2.4f, size * 2.4f);
            Vector3 baseScale = glow.localScale;
            glow.DOScale(baseScale * 1.07f, 2.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void BuildFrame(Transform parent, Vector3 center, float boardW, float boardH, float cell)
        {
            Sized(parent, "BoardRim", ProceduralSprites.RoundedSquare(),
                new Color(0.30f, 0.36f, 0.55f, 1f), -8, center, boardW + cell * 0.55f, boardH + cell * 0.55f);
            Sized(parent, "BoardPanel", ProceduralSprites.RoundedSquare(),
                new Color(0.09f, 0.10f, 0.17f, 1f), -7, center, boardW + cell * 0.3f, boardH + cell * 0.3f);
        }

        private void BuildCells(Transform parent, int width, int height, Func<int, int, Vector3> cellPos, float cell)
        {
            Color light = new Color(0.21f, 0.24f, 0.36f, 1f);
            Color dark = new Color(0.16f, 0.18f, 0.28f, 1f);
            float size = cell * 0.92f;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Color col = (x + y) % 2 == 0 ? light : dark;
                    Sized(parent, "Cell", ProceduralSprites.RoundedSquare(), col, -6, cellPos(x, y), size, size);
                }
        }

        private Transform Sized(Transform parent, string name, Sprite sprite, Color color, int order, Vector3 pos, float worldW, float worldH)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            Vector3 b = sprite.bounds.size;
            go.transform.localScale = new Vector3(worldW / b.x, worldH / b.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
