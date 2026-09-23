using UnityEngine;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>Runtime-generated circular / panel sprites for polished mobile HUD.</summary>
    public static class UiArt
    {
        static Sprite whiteCircle;
        static Sprite ringOuter;
        static Sprite softPanel;

        public static Sprite WhiteCircle()
        {
            if (whiteCircle != null) return whiteCircle;
            whiteCircle = MakeCircle(128, Color.white, 0, Color.clear);
            return whiteCircle;
        }

        public static Sprite JoystickRing()
        {
            if (ringOuter != null) return ringOuter;
            ringOuter = MakeCircle(160, new Color(1f, 1f, 1f, 0.12f), 10, new Color(1f, 0.92f, 0.55f, 0.85f));
            return ringOuter;
        }

        public static Sprite SoftPanel()
        {
            if (softPanel != null) return softPanel;
            softPanel = MakeRoundedRect(64, 32, 10, new Color(1f, 1f, 1f, 1f));
            return softPanel;
        }

        static Sprite whiteQuad;
        public static Sprite WhiteQuad()
        {
            if (whiteQuad != null) return whiteQuad;
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[64];
            for (int i = 0; i < 64; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply(false, true);
            whiteQuad = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            return whiteQuad;
        }

        public static Sprite FramePanel()
        {
            // Dark fill with bright border for menu/dialog cards
            int w = 96, h = 64, r = 12, border = 4;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool inside = InRound(x, y, w, h, r);
                bool inner = InRound(x, y, w, h, r - border);
                if (!inside) px[y * w + x] = Color.clear;
                else if (!inner) px[y * w + x] = Color.white;
                else px[y * w + x] = new Color(1f, 1f, 1f, 0.92f);
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }

        static bool InRound(int x, int y, int w, int h, int radius)
        {
            if (radius <= 0) return true;
            if (x < radius && y < radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
            if (x >= w - radius && y < radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, radius)) <= radius;
            if (x < radius && y >= h - radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, h - radius - 1)) <= radius;
            if (x >= w - radius && y >= h - radius)
                return Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, h - radius - 1)) <= radius;
            return true;
        }

        static Sprite heartSprite;

        public static Sprite HeartSprite()
        {
            if (heartSprite != null) return heartSprite;
            int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // Classic heart shape via distance fields
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                ny = -ny;
                float a = nx * nx + (ny - Mathf.Sqrt(Mathf.Abs(nx))) * (ny - Mathf.Sqrt(Mathf.Abs(nx))) - 0.55f;
                // Alternate formula
                float x2 = nx * 1.15f;
                float y2 = ny + 0.15f;
                bool inside =
                    (Mathf.Pow(x2, 2f) + Mathf.Pow(y2 - 0.25f * Mathf.Sqrt(Mathf.Abs(x2)), 2f)) < 0.45f
                    || (Vector2.Distance(new Vector2(x2, y2), new Vector2(-0.35f, 0.25f)) < 0.38f)
                    || (Vector2.Distance(new Vector2(x2, y2), new Vector2(0.35f, 0.25f)) < 0.38f)
                    || (y2 < 0.15f && Mathf.Abs(x2) + (-y2) * 0.85f < 0.72f);
                px[y * size + x] = inside ? Color.white : Color.clear;
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            heartSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return heartSprite;
        }

        public static Sprite MakeCircle(int size, Color fill, int ringWidth, Color ring)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.5f - 1f;
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                Color c = Color.clear;
                if (d <= r)
                {
                    if (ringWidth > 0 && d >= r - ringWidth) c = ring;
                    else c = fill;
                    // soft edge
                    float edge = Mathf.Clamp01(r - d);
                    if (edge < 1.5f) c.a *= edge / 1.5f;
                }
                px[y * size + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite MakeRoundedRect(int w, int h, int radius, Color fill)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool inside = true;
                // corner checks
                if (x < radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
                else if (x >= w - radius && y < radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, radius)) <= radius;
                else if (x < radius && y >= h - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, h - radius - 1)) <= radius;
                else if (x >= w - radius && y >= h - radius)
                    inside = Vector2.Distance(new Vector2(x, y), new Vector2(w - radius - 1, h - radius - 1)) <= radius;

                px[y * w + x] = inside ? fill : Color.clear;
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        public static Text StyleLabel(Text t, int size, Color color, FontStyle style = FontStyle.Bold)
        {
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
