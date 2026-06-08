using UnityEngine;

namespace Match3.View
{
    public static class ProceduralSprites
    {
        private static Sprite _square;
        private static Sprite _squareBottom;
        private static Sprite _circle;
        private static Sprite _rounded;

        public static Sprite Square()
        {
            if (_square != null) return _square;
            _square = MakeSquare(new Vector2(0.5f, 0.5f));
            return _square;
        }

        public static Sprite SquareBottom()
        {
            if (_squareBottom != null) return _squareBottom;
            _squareBottom = MakeSquare(new Vector2(0.5f, 0f));
            return _squareBottom;
        }

        public static Sprite Circle()
        {
            if (_circle != null) return _circle;

            const int res = 32;
            var tex = new Texture2D(res, res) { wrapMode = TextureWrapMode.Clamp };
            float r = res * 0.5f;
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    float dx = x + 0.5f - r;
                    float dy = y + 0.5f - r;
                    float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) / r);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
                }
            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
            return _circle;
        }

        public static Sprite RoundedSquare()
        {
            if (_rounded != null) return _rounded;

            const int res = 64;
            var tex = new Texture2D(res, res) { wrapMode = TextureWrapMode.Clamp };
            float half = res * 0.5f;
            float radius = res * 0.2f;
            float extent = half - 1f;
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float qx = Mathf.Abs(px) - (extent - radius);
                    float qy = Mathf.Abs(py) - (extent - radius);
                    float outX = Mathf.Max(qx, 0f);
                    float outY = Mathf.Max(qy, 0f);
                    float dist = Mathf.Sqrt(outX * outX + outY * outY) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float a = Mathf.Clamp01(0.5f - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            tex.Apply();
            _rounded = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
            return _rounded;
        }

        public static Sprite VerticalGradient(Color bottom, Color top)
        {
            const int h = 256;
            var tex = new Texture2D(2, h) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                Color c = Color.Lerp(bottom, top, y / (float)(h - 1));
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, h), new Vector2(0.5f, 0.5f), h);
        }

        private static Sprite MakeSquare(Vector2 pivot)
        {
            var tex = new Texture2D(4, 4);
            var cols = new Color[16];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), pivot, 4f);
        }
    }
}
