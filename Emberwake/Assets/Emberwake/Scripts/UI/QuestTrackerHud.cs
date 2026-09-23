using UnityEngine;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>Compact quest tracker under the objective banner.</summary>
    public class QuestTrackerHud : MonoBehaviour
    {
        Text label;

        public void Build(Transform parent)
        {
            var go = new GameObject("QuestTracker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = UiArt.SoftPanel();
            img.type = Image.Type.Sliced;
            img.color = new Color(0.06f, 0.07f, 0.1f, 0.72f);
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            // Sit below the objective banner (banner ends ~264px from the top).
            rt.anchoredPosition = new Vector2(0f, -284f);
            rt.sizeDelta = new Vector2(980f, 64f);

            var icon = new GameObject("I", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(go.transform, false);
            var iimg = icon.GetComponent<Image>();
            iimg.sprite = GeneratedArt.IconQuest();
            iimg.preserveAspect = true;
            iimg.raycastTarget = false;
            var irt = icon.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0f, 0.5f);
            irt.anchorMax = new Vector2(0f, 0.5f);
            irt.anchoredPosition = new Vector2(40f, 0f);
            irt.sizeDelta = new Vector2(44f, 44f);

            var textGo = new GameObject("T", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.1f, 0f);
            trt.anchorMax = new Vector2(0.98f, 1f);
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            label = textGo.GetComponent<Text>();
            UiArt.StyleLabel(label, 22, new Color(1f, 0.9f, 0.55f));
            label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false;
        }

        void Update()
        {
            if (label == null || QuestSystem.Instance == null) return;
            var mains = QuestSystem.Instance.GetActiveMain();
            if (mains.Count > 0)
            {
                var (def, st) = mains[0];
                label.text = $"MAIN  {def.title}  ({st.progress}/{def.targetCount})";
            }
            else
            {
                var sides = QuestSystem.Instance.GetActiveSides();
                var open = sides.FindAll(x => !x.st.completed);
                if (open.Count > 0)
                {
                    var (def, st) = open[0];
                    label.text = $"SIDE  {def.title}  ({st.progress}/{def.targetCount})";
                }
                else label.text = "Semua quest aktif selesai — nyalakan Wick";
            }
        }
    }
}
