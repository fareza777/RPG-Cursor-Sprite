using UnityEngine;

namespace Emberwake
{
    /// <summary>Lightweight impact VFX for hits (spark pop + quick flash).</summary>
    public static class CombatVfx
    {
        static Sprite spark;

        static Sprite SparkSprite()
        {
            if (spark != null) return spark;
            int s = 48;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float c = (s - 1) * 0.5f;
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x - c) / c, dy = (y - c) / c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // 4-point star-ish spark: bright center + soft falloff
                float star = Mathf.Max(0f, 1f - Mathf.Min(Mathf.Abs(dx), Mathf.Abs(dy)) * 6f);
                float core = Mathf.Clamp01(1f - d);
                float a = Mathf.Clamp01(core * core + star * (1f - d));
                px[y * s + x] = new Color(1f, 1f, 1f, a);
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            spark = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            return spark;
        }

        /// <summary>Death burst: expanding shockwave + a scatter of sparks.</summary>
        public static void Burst(Vector3 pos, Color color)
        {
            var wave = new GameObject("FX_DeathWave");
            wave.transform.position = pos;
            var wsr = wave.AddComponent<SpriteRenderer>();
            wsr.sprite = SparkSprite();
            wsr.color = new Color(color.r, color.g, color.b, 0.85f);
            wsr.sortingOrder = 44;
            wave.AddComponent<HitSparkFx>().Init(2.2f);
            int n = 6;
            for (int i = 0; i < n; i++)
            {
                float ang = (i / (float)n) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
                var off = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * Random.Range(0.25f, 0.6f);
                Spark(pos + off, color, Random.Range(0.5f, 0.9f));
            }
        }

        public static void Spark(Vector3 pos, Color color, float size = 1.0f)
        {
            var go = new GameObject("FX_HitSpark");
            go.transform.position = pos + new Vector3(Random.Range(-0.12f, 0.12f), Random.Range(-0.12f, 0.12f), 0f);
            go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 90f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SparkSprite();
            sr.color = color;
            sr.sortingOrder = 45;
            go.AddComponent<HitSparkFx>().Init(size);
        }
    }

    /// <summary>Quick scale-up + fade for a hit spark, then self-destruct.</summary>
    public class HitSparkFx : MonoBehaviour
    {
        float life = 0.16f;
        float t;
        float size;
        SpriteRenderer sr;

        public void Init(float s)
        {
            size = s;
            sr = GetComponent<SpriteRenderer>();
            transform.localScale = Vector3.one * (size * 0.5f);
        }

        void Update()
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / life);
            transform.localScale = Vector3.one * (size * Mathf.Lerp(0.6f, 1.3f, u));
            if (sr != null)
            {
                var c = sr.color;
                c.a = 1f - u;
                sr.color = c;
            }
            if (t >= life) Destroy(gameObject);
        }
    }
}
