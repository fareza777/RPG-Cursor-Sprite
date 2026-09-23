using UnityEngine;

namespace Emberwake
{
    /// <summary>Soft parallax shadow blob under characters for depth.</summary>
    public class GroundShadow : MonoBehaviour
    {
        Transform blob;
        static Sprite soft;

        public static void Attach(Transform host, float scale = 0.9f)
        {
            var gs = host.gameObject.AddComponent<GroundShadow>();
            gs.Build(scale);
        }

        void Build(float scale)
        {
            EnsureSprite();
            var go = new GameObject("Shadow");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            go.transform.localScale = new Vector3(scale, scale * 0.35f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = soft;
            sr.color = new Color(0f, 0f, 0f, 0.35f);
            sr.sortingOrder = 1;
            blob = go.transform;
        }

        void LateUpdate()
        {
            if (blob == null) return;
            blob.rotation = Quaternion.identity;
            // Keep shadow grounded even if parent flips
            var ls = blob.localScale;
            ls.x = Mathf.Abs(ls.x);
            blob.localScale = ls;
        }

        static void EnsureSprite()
        {
            if (soft != null) return;
            int s = 32;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[s * s];
            float c = (s - 1) * 0.5f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float nx = (x - c) / (c * 0.95f);
                float ny = (y - c) / (c * 0.55f);
                float d = nx * nx + ny * ny;
                float a = Mathf.Clamp01(1f - d);
                px[y * s + x] = new Color(1f, 1f, 1f, a * a);
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            soft = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
        }
    }
}
