using UnityEngine;

namespace Match3.View
{
    public static class ProceduralSprites
    {
        private static Sprite _square;
        private static Sprite _squareBottom;
        private static Sprite _circle;

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
