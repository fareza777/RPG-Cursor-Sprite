using UnityEngine;

namespace Emberwake
{
    /// <summary>Ambient ember particles for atmosphere (title / altar / boss).</summary>
    public class EmberParticles : MonoBehaviour
    {
        struct Ember
        {
            public Vector3 pos;
            public float speed;
            public float size;
            public float life;
            public SpriteRenderer sr;
        }

        Ember[] embers;
        static Sprite soft;

        public static EmberParticles Attach(Transform parent, int count = 18)
        {
            var go = new GameObject("EmberFX");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var fx = go.AddComponent<EmberParticles>();
            fx.Init(count);
            return fx;
        }

        void Init(int count)
        {
            if (soft == null)
            {
                int s = 16;
                var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                var px = new Color[s * s];
                float c = (s - 1) * 0.5f;
                for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / (s * 0.5f);
                    float a = Mathf.Clamp01(1f - d);
                    px[y * s + x] = new Color(1f, 0.7f, 0.25f, a * a);
                }
                tex.SetPixels(px);
                tex.Apply();
                soft = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            }

            embers = new Ember[count];
            for (int i = 0; i < count; i++)
            {
                var e = new GameObject("e" + i);
                e.transform.SetParent(transform, false);
                var sr = e.AddComponent<SpriteRenderer>();
                sr.sprite = soft;
                sr.sortingOrder = 13; // above props/torch glow, below the hero (20)
                embers[i] = new Ember
                {
                    sr = sr,
                    pos = new Vector3(Random.Range(-7f, 7f), Random.Range(-9f, 9f), 0f),
                    speed = Random.Range(0.5f, 1.7f),
                    size = Random.Range(0.22f, 0.6f),
                    life = Random.Range(0f, 1f)
                };
                e.transform.localPosition = embers[i].pos;
                e.transform.localScale = Vector3.one * embers[i].size;
            }
        }

        void Update()
        {
            if (embers == null) return;
            for (int i = 0; i < embers.Length; i++)
            {
                var e = embers[i];
                e.pos.y += e.speed * Time.deltaTime;
                e.pos.x += Mathf.Sin(Time.time * 1.5f + i) * 0.15f * Time.deltaTime;
                e.life += Time.deltaTime * 0.25f;
                if (e.pos.y > 10f)
                {
                    e.pos.y = -10f;
                    e.pos.x = Random.Range(-6f, 6f);
                }
                if (e.sr != null)
                {
                    e.sr.transform.localPosition = e.pos;
                    var c = e.sr.color;
                    c.a = 0.35f + 0.6f * Mathf.Abs(Mathf.Sin(e.life * Mathf.PI));
                    e.sr.color = c;
                }
                embers[i] = e;
            }
        }
    }
}
