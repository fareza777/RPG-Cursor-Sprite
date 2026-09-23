using UnityEngine;

namespace Emberwake
{
    /// <summary>Lightweight juice: screen shake + hit-stop + hit flash.</summary>
    public class FeelFeedback : MonoBehaviour
    {
        public static FeelFeedback Instance { get; private set; }

        Camera cam;
        Vector3 shakeOffset;
        float shakeAmp;
        float shakeTime;
        float hitStopLeft;

        void Awake()
        {
            Instance = this;
            cam = Camera.main;
        }

        void Update()
        {
            if (hitStopLeft > 0f)
            {
                hitStopLeft -= Time.unscaledDeltaTime;
                if (hitStopLeft <= 0f && Time.timeScale < 0.05f)
                    Time.timeScale = 1f;
            }
        }

        void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            if (shakeTime > 0f)
            {
                shakeTime -= Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(shakeTime);
                shakeOffset = (Vector3)(Random.insideUnitCircle * shakeAmp * t);
            }
            else shakeOffset = Vector3.zero;

            cam.transform.position += shakeOffset;
        }

        public static void Shake(float amplitude = 0.18f, float duration = 0.15f)
        {
            Ensure();
            Instance.shakeAmp = Mathf.Max(Instance.shakeAmp, amplitude);
            Instance.shakeTime = Mathf.Max(Instance.shakeTime, duration);
        }

        public static void HitStop(float seconds = 0.05f)
        {
            Ensure();
            if (seconds <= 0f) return;
            Instance.hitStopLeft = Mathf.Max(Instance.hitStopLeft, seconds);
            Time.timeScale = 0.02f;
        }

        static void Ensure()
        {
            if (Instance != null) return;
            var go = new GameObject("FeelFeedback");
            Instance = go.AddComponent<FeelFeedback>();
        }
    }

    public class SpriteFlash : MonoBehaviour
    {
        SpriteRenderer sr;
        Color baseColor = Color.white;
        float flashLeft;

        void Awake()
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) baseColor = sr.color;
        }

        void OnEnable()
        {
            var h = GetComponent<Health>();
            if (h != null) h.OnDamaged += Flash;
        }

        void OnDisable()
        {
            var h = GetComponent<Health>();
            if (h != null) h.OnDamaged -= Flash;
        }

        public void Flash()
        {
            flashLeft = 0.12f;
        }

        void Update()
        {
            if (sr == null) return;
            if (flashLeft > 0f)
            {
                flashLeft -= Time.deltaTime;
                sr.color = Color.Lerp(baseColor, Color.white, flashLeft / 0.12f);
            }
            else sr.color = baseColor;
        }
    }
}
