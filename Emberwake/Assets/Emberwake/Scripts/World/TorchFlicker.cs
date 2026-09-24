using UnityEngine;

namespace Emberwake
{
    /// <summary>Organic flame flicker for a warm glow sprite (torches, altar, gloves).</summary>
    public class GlowFlicker : MonoBehaviour
    {
        SpriteRenderer sr;
        float baseAlpha;
        float baseScale;
        float seed;
        float pulseSpeed = 3.4f;
        float amount = 0.24f;

        public void Init(SpriteRenderer renderer, float alpha, float speed = 3.4f, float flicker = 0.24f)
        {
            sr = renderer;
            baseAlpha = alpha;
            baseScale = transform.localScale.x;
            seed = Random.value * 41f;
            pulseSpeed = speed;
            amount = flicker;
        }

        void Update()
        {
            if (sr == null) return;
            float n = Mathf.PerlinNoise(seed, Time.time * pulseSpeed);
            float f = 1f - amount + amount * 2f * n;
            var c = sr.color;
            c.a = Mathf.Clamp01(baseAlpha * f);
            sr.color = c;
            float sc = baseScale * (0.95f + 0.08f * n);
            transform.localScale = new Vector3(sc, sc, 1f);
        }
    }
}
