using DG.Tweening;
using UnityEngine;

namespace Match3.View
{
    public class SandBar
    {
        private Transform _fill;
        private float _maxHeight;
        private float _bottomY;
        private float _x;
        private float _width;
        private float _shown;
        private float _target;
        private int _inFlight;

        public float X => _x;
        public float Width => _width;
        public float TargetTopY => _bottomY + _maxHeight * _target;

        public void Build(Transform parent, Vector3 center, float height, float width)
        {
            _maxHeight = height;
            _width = width;
            _x = center.x;
            _bottomY = center.y - height * 0.5f;

            Quad(parent, "SandOutline", new Color(0.55f, 0.4f, 0.12f, 1f), 6,
                 new Vector3(_x, center.y, 0f), new Vector3(width + 0.2f, height + 0.2f, 1f), ProceduralSprites.Square());
            Quad(parent, "SandBG", new Color(0.08f, 0.05f, 0.02f, 1f), 7,
                 new Vector3(_x, center.y, 0f), new Vector3(width, height, 1f), ProceduralSprites.Square());
            _fill = Quad(parent, "SandFill", new Color(0.95f, 0.72f, 0.18f, 1f), 8,
                 new Vector3(_x, _bottomY, 0f), new Vector3(width, 0.001f, 1f), ProceduralSprites.SquareBottom());
            Quad(parent, "SandShine", new Color(1f, 0.95f, 0.55f, 0.25f), 9,
                 new Vector3(_x - width * 0.28f, center.y, 0f), new Vector3(width * 0.22f, height, 1f), ProceduralSprites.Square());
        }

        public void SetImmediate(float t)
        {
            _shown = _target = Mathf.Clamp01(t);
            float h = Mathf.Max(0.001f, _maxHeight * _shown);
            _fill.localScale = new Vector3(_width, h, 1f);
        }

        public void SetTarget(float t)
        {
            _target = Mathf.Clamp01(t);
        }

        public void RegisterParticle()
        {
            _inFlight++;
        }

        public void OnParticleArrived()
        {
            _inFlight = Mathf.Max(0, _inFlight - 1);
            int n = _inFlight + 1;
            _shown += (_target - _shown) / n;
            if (_inFlight == 0) _shown = _target;
            Animate();
        }

        private void Animate()
        {
            float h = Mathf.Max(0.001f, _maxHeight * _shown);
            _fill.DOKill();
            _fill.localScale = new Vector3(_width, _fill.localScale.y, 1f);
            _fill.DOScaleY(h, 0.6f).SetEase(Ease.OutElastic, 1.1f, 0.4f);
            _fill.DOScaleX(_width * 1.3f, 0.1f).SetEase(Ease.OutQuad)
                 .OnComplete(() => _fill.DOScaleX(_width, 0.5f).SetEase(Ease.OutElastic, 1.1f, 0.4f));
        }

        private Transform Quad(Transform parent, string name, Color color, int order, Vector3 pos, Vector3 scale, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
