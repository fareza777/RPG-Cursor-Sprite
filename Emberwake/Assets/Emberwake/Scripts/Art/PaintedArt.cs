using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    /// <summary>Painted PNG pack loaded from Resources/Painted.</summary>
    public static class PaintedArt
    {
        static readonly Dictionary<string, Sprite> Cache = new();
        static Sprite scrim;

        public static Sprite Load(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;
            var sprite = Resources.Load<Sprite>("Painted/" + name);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>("Painted/" + name);
                if (tex == null) return null;
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            Cache[name] = sprite;
            return sprite;
        }

        public static Sprite Scene(string id) => id switch
        {
            "forest" => Load("cine_forest") ?? Load("cine_village"),
            "altar" => Load("cine_altar") ?? Load("cine_village"),
            _ => Load("cine_village")
        };

        public static Sprite Scrim()
        {
            if (scrim != null) return scrim;
            const int h = 128;
            var tex = new Texture2D(4, h, TextureFormat.RGBA32, false);
            var px = new Color[4 * h];
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                float a = Mathf.Lerp(0.94f, 0f, Mathf.SmoothStep(0f, 1f, t));
                var c = new Color(0.02f, 0.012f, 0.015f, a);
                for (int x = 0; x < 4; x++) px[y * 4 + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply();
            scrim = Sprite.Create(tex, new Rect(0, 0, 4, h), new Vector2(0.5f, 0f), 100f);
            return scrim;
        }
    }
}
