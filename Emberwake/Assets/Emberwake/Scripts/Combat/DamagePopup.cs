using UnityEngine;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>Floating damage / heal numbers in world space.</summary>
    public class DamagePopup : MonoBehaviour
    {
        static Sprite white;
        Text label;
        float life = 0.7f;
        Vector3 velocity;
        Transform canvasT;
        float baseScale = 0.018f;
        float punch;

        public static void Spawn(Vector3 worldPos, float amount, bool crit = false)
        {
            var go = new GameObject("DmgPopup");
            go.transform.position = worldPos + Vector3.up * 0.6f;
            var popup = go.AddComponent<DamagePopup>();
            popup.Setup(amount, crit);
        }

        void Setup(float amount, bool crit)
        {
            EnsureWhite();
            var canvasGo = new GameObject("C", typeof(Canvas));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 120;
            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 48f);
            canvasT = canvasGo.transform;
            baseScale = crit ? 0.024f : 0.018f;
            punch = crit ? 1.7f : 1.35f;
            canvasT.localScale = Vector3.one * (baseScale * punch);

            var textGo = new GameObject("T", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(canvasGo.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            label = textGo.GetComponent<Text>();
            UiArt.StyleLabel(label, crit ? 36 : 28, crit ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.45f, 0.35f));
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.text = Mathf.CeilToInt(amount).ToString();
            textGo.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
            velocity = new Vector3(Random.Range(-0.4f, 0.4f), 1.6f, 0f);
        }

        void Update()
        {
            life -= Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            velocity.y -= 2.2f * Time.deltaTime;
            if (canvasT != null && punch > 1f)
            {
                punch = Mathf.MoveTowards(punch, 1f, Time.deltaTime * 4.5f);
                canvasT.localScale = Vector3.one * (baseScale * punch);
            }
            if (label != null)
            {
                var c = label.color;
                c.a = Mathf.Clamp01(life / 0.35f);
                label.color = c;
            }
            if (Camera.main != null)
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            if (life <= 0f) Destroy(gameObject);
        }

        static void EnsureWhite()
        {
            if (white != null) return;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }
    }
}
