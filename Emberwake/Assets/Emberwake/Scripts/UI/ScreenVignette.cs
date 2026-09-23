using UnityEngine;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>Full-screen vignette + low-HP pulse for cinematic feel.</summary>
    public class ScreenVignette : MonoBehaviour
    {
        Image img;
        float pulse;

        public void Build(Transform canvas)
        {
            // Subtle dusk color-grade over the world (drawn first, so only the world
            // behind the overlay canvas is tinted — HUD/dialog stay crisp on top).
            var duskGo = new GameObject("DuskGrade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            duskGo.transform.SetParent(canvas, false);
            var drt = (RectTransform)duskGo.transform;
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.offsetMin = drt.offsetMax = Vector2.zero;
            var dusk = duskGo.GetComponent<Image>();
            dusk.sprite = UiArt.SoftPanel();
            dusk.type = Image.Type.Sliced;
            dusk.color = new Color(0.16f, 0.15f, 0.30f, 0.26f);
            dusk.raycastTarget = false;
            duskGo.transform.SetAsFirstSibling();

            var go = new GameObject("Vignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            img = go.GetComponent<Image>();
            img.sprite = MakeVignetteSprite();
            img.type = Image.Type.Simple;
            img.color = new Color(1f, 1f, 1f, 0.55f);
            img.raycastTarget = false;
            // Keep under HUD widgets but above world (overlay canvas already)
            go.transform.SetAsFirstSibling();
        }

        void Update()
        {
            if (img == null) return;
            float danger = 0f;
            var s = GameManager.Instance?.Stats;
            if (s != null && s.MaxHearts > 0 && s.Hearts <= 1)
                danger = 1f;
            else if (s != null && s.Hearts <= 2)
                danger = 0.45f;
            pulse += Time.unscaledDeltaTime * (1.6f + danger * 2f);
            float a = 0.42f + danger * (0.18f + 0.12f * Mathf.Sin(pulse));
            img.color = new Color(1f, 1f - danger * 0.35f, 1f - danger * 0.45f, a);
        }

        static Sprite MakeVignetteSprite()
        {
            int s = 128;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[s * s];
            float c = (s - 1) * 0.5f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x - c) / c;
                float ny = (y - c) / c;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01((d - 0.35f) / 0.85f);
                a = a * a;
                px[y * s + x] = new Color(0f, 0f, 0f, a);
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        }
    }
}
