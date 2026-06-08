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
            transform.localScale = Vector3.one;
        }

        public void UpdatePosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void MoveTo(Vector3 targetWorldPos, float duration, Ease ease = Ease.OutCubic, Action onComplete = null)
        {
            transform.DOMove(targetWorldPos, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
        }

        public void PlayDestroyAnimation(float duration, Action onComplete = null)
        {
            transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
        }

        public void PlaySpawnAnimation()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }
    }
}
