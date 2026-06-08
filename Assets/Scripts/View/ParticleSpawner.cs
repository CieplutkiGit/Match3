using System;
using DG.Tweening;
using UnityEngine;

namespace Match3.View
{
    public static class ParticleSpawner
    {
        public static void Fly(Vector3 from, Vector3 to, Color color, float delay, Action onArrive)
        {
            var go = new GameObject("collect");
            go.transform.position = from + (Vector3)UnityEngine.Random.insideUnitCircle * 0.2f;
            float size = UnityEngine.Random.Range(0.32f, 0.55f);
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.Circle();
            sr.color = new Color(color.r, color.g, color.b, 0.95f);
            sr.sortingOrder = 30;

            var t = go.transform;
            float dur = UnityEngine.Random.Range(0.7f, 1.0f);
            Vector3 ctrl = (from + to) * 0.5f + (Vector3)UnityEngine.Random.insideUnitCircle * 1.2f;

            DOTween.Sequence()
                .PrependInterval(delay)
                .Append(t.DOScale(size * 1.3f, 0.12f).SetEase(Ease.OutBack))
                .Append(t.DOPath(new[] { ctrl, to }, dur, PathType.CatmullRom).SetEase(Ease.InOutSine))
                .Join(t.DOScale(size * 0.4f, dur).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    onArrive?.Invoke();
                    UnityEngine.Object.Destroy(go);
                });
        }

        public static void Debris(Vector3 pos, Color color)
        {
            float bottom = WorldBottom() - 1f;
            int count = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("debris");
                go.transform.position = pos + (Vector3)UnityEngine.Random.insideUnitCircle * 0.25f;
                float size = UnityEngine.Random.Range(0.14f, 0.32f);
                go.transform.localScale = Vector3.one * size;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = ProceduralSprites.Circle();
                sr.color = color;
                sr.sortingOrder = 24;

                var t = go.transform;
                float life = UnityEngine.Random.Range(1.0f, 1.6f);
                float vx = UnityEngine.Random.Range(-1.6f, 1.6f);
                float peak = UnityEngine.Random.Range(0.3f, 0.9f);

                t.DOMoveX(pos.x + vx, life).SetEase(Ease.OutQuad);
                t.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-260f, 260f)), life, RotateMode.FastBeyond360);
                DOTween.Sequence()
                    .Append(t.DOMoveY(pos.y + peak, life * 0.25f).SetEase(Ease.OutQuad))
                    .Append(t.DOMoveY(bottom, life * 0.75f).SetEase(Ease.InQuad));

                var cSr = sr;
                var cGo = go;
                DOTween.To(() => cSr.color, c => cSr.color = c,
                           new Color(color.r, color.g, color.b, 0f), life)
                    .SetEase(Ease.InQuint)
                    .OnComplete(() => UnityEngine.Object.Destroy(cGo));
            }
        }

        public static void BackgroundFall()
        {
            var cam = Camera.main;
            if (cam == null) return;

            float depth = Mathf.Abs(cam.transform.position.z);
            Vector3 top = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.value, 1.05f, depth));
            float bottom = WorldBottom() - 1f;

            var go = new GameObject("bgfall");
            go.transform.position = new Vector3(top.x, top.y, 0f);
            float size = UnityEngine.Random.Range(0.1f, 0.3f);
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.Circle();
            sr.color = new Color(1f, 1f, 1f, UnityEngine.Random.Range(0.05f, 0.18f));
            sr.sortingOrder = -10;

            var t = go.transform;
            float fall = UnityEngine.Random.Range(2.5f, 5f);
            t.DOMoveY(bottom, fall).SetEase(Ease.InQuad);
            t.DOMoveX(top.x + UnityEngine.Random.Range(-0.6f, 0.6f), fall).SetEase(Ease.InOutSine);
            t.DORotate(new Vector3(0f, 0f, UnityEngine.Random.Range(-90f, 90f)), fall);

            var cSr = sr;
            var cGo = go;
            DOTween.To(() => cSr.color, c => cSr.color = c, new Color(1f, 1f, 1f, 0f), fall)
                .SetEase(Ease.InQuad)
                .OnComplete(() => UnityEngine.Object.Destroy(cGo));
        }

        private static float WorldBottom()
        {
            var cam = Camera.main;
            if (cam == null) return -6f;
            if (cam.orthographic) return cam.transform.position.y - cam.orthographicSize;
            return cam.transform.position.y - 6f;
        }
    }
}
