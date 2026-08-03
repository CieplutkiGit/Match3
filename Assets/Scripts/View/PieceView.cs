using System;
using DG.Tweening;
using UnityEngine;
using Match3.Model;

namespace Match3.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class PieceView : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Vector3 _baseScale;

        public PieceColor Color { get; private set; }
        public PieceType Type { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Color TintColor => _spriteRenderer.color;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Setup(
            int x,
            int y,
            PieceColor color,
            PieceType type,
            Sprite sprite,
            float cellSize,
            float sizeInCells)
        {
            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));

            X = x;
            Y = y;
            Color = color;
            Type = type;
            _spriteRenderer.sprite = sprite;
            float spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            float scale = cellSize * sizeInCells / spriteSize;
            _baseScale = Vector3.one * scale;
            transform.localScale = _baseScale;
            _spriteRenderer.color = type switch
            {
                PieceType.HorizontalLine => new UnityEngine.Color(0.4f, 0.8f, 1f),
                PieceType.VerticalLine => new UnityEngine.Color(1f, 0.8f, 0.2f),
                PieceType.Bomb => new UnityEngine.Color(1f, 0.3f, 0.3f),
                _ => UnityEngine.Color.white,
            };
        }

        public void UpdatePosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void ReactToShockwave(float delay, float strength)
        {
            DOTween.Sequence()
                .PrependInterval(delay)
                .Append(transform.DOPunchScale(_baseScale * strength, 0.45f, 10, 0.6f));
        }

        public void MoveTo(Vector3 targetWorldPos, float duration, Ease ease = Ease.OutCubic, Action onComplete = null)
        {
            transform.DOMove(targetWorldPos, duration).SetEase(ease).OnComplete(() =>
            {
                onComplete?.Invoke();
                Vector3 squashScale = Vector3.Scale(_baseScale, new Vector3(1.15f, 0.85f, 1f));
                transform.DOScale(squashScale, 0.07f).SetEase(Ease.OutQuad)
                    .OnComplete(() => transform.DOScale(_baseScale, 0.12f).SetEase(Ease.OutBack));
            });
        }

        public void PlayDestroyAnimation(float duration, Action onComplete = null)
        {
            transform.DOKill();
            SpawnParticles(6, duration);
            DOTween.Sequence()
                .Append(transform.DOScale(_baseScale * 1.2f, duration * 0.15f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(Vector3.zero, duration * 0.85f).SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }

        public void PlaySpecialDestroyAnimation(float duration, Action onComplete = null)
        {
            transform.DOKill();
            SpawnParticles(12, duration);
            DOTween.Sequence()
                .Append(transform.DOScale(_baseScale * 1.8f, duration * 0.2f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(Vector3.zero, duration * 0.8f).SetEase(Ease.InExpo))
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }

        public void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(_baseScale, 0.25f).SetEase(Ease.OutBack);
        }

        private void SpawnParticles(int count, float duration)
        {
            UnityEngine.Color baseColor = _spriteRenderer.color;
            Sprite sprite = _spriteRenderer.sprite;

            for (int i = 0; i < count; i++)
            {
                var particle = new GameObject();
                particle.transform.position = transform.position;
                particle.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.1f, 0.25f);

                var sr = particle.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = baseColor;
                sr.sortingLayerName = _spriteRenderer.sortingLayerName;
                sr.sortingOrder = _spriteRenderer.sortingOrder + 1;

                float angle = (360f / count) * i + UnityEngine.Random.Range(-30f, 30f);
                float dist = UnityEngine.Random.Range(0.2f, 0.6f);
                Vector3 dest = transform.position + Quaternion.Euler(0f, 0f, angle) * Vector3.right * dist;
                float dur = duration * UnityEngine.Random.Range(0.5f, 1f);

                particle.transform.DOMove(dest, dur).SetEase(Ease.OutQuad);
                particle.transform.DOScale(0f, dur).SetEase(Ease.InQuad);
                var capturedSr = sr;
                DOTween.To(() => capturedSr.color,
                           c => capturedSr.color = c,
                           new UnityEngine.Color(baseColor.r, baseColor.g, baseColor.b, 0f),
                           dur)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => Destroy(particle));
            }
        }
    }
}
