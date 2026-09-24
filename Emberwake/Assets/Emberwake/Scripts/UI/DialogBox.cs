using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Emberwake
{
    public struct DialogLine
    {
        public string Speaker;
        public string Text;
        public DialogLine(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }
    }

    /// <summary>Bottom dialog box for story / NPCs. Tap to advance.</summary>
    public class DialogBox : MonoBehaviour
    {
        public static DialogBox Instance { get; private set; }

        GameObject root;
        Text speakerLabel;
        Text bodyLabel;
        Image portraitImage;
        Queue<DialogLine> queue = new Queue<DialogLine>();
        System.Action onComplete;
        bool tap;

        public bool IsOpen => root != null && root.activeSelf;

        /// <summary>Advance to the next line (used by tests / QA autopilot).</summary>
        public void ForceAdvance() => tap = true;

        /// <summary>Immediately close the dialog (QA screenshots).</summary>
        public void ForceClose()
        {
            queue.Clear();
            StopAllCoroutines();
            if (root != null) root.SetActive(false);
            onComplete = null;
        }

        public void Build(Transform canvas)
        {
            Instance = this;
            root = new GameObject("DialogBox", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(1f, 0.36f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var frame = root.AddComponent<Image>();
            frame.sprite = PaintedArt.Scrim();
            frame.type = Image.Type.Simple;
            frame.color = Color.white;
            frame.raycastTarget = true;

            // Warm frame behind the card (peeks out as a thin ember-gold border).
            var borderGo = new GameObject("CardBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGo.transform.SetParent(root.transform, false);
            var border = borderGo.GetComponent<Image>();
            border.sprite = UiArt.SoftPanel();
            border.type = Image.Type.Sliced;
            border.color = new Color(0.85f, 0.55f, 0.2f, 0.85f);
            border.raycastTarget = false;
            var bdr = border.rectTransform;
            bdr.anchorMin = new Vector2(0.28f, 0.1f);
            bdr.anchorMax = new Vector2(0.97f, 0.78f);
            bdr.offsetMin = new Vector2(-5f, -5f);
            bdr.offsetMax = new Vector2(5f, 5f);

            var cardGo = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cardGo.transform.SetParent(root.transform, false);
            var card = cardGo.GetComponent<Image>();
            card.sprite = UiArt.SoftPanel();
            card.type = Image.Type.Sliced;
            card.color = new Color(0.08f, 0.06f, 0.12f, 0.94f);
            card.raycastTarget = false;
            var crt = card.rectTransform;
            crt.anchorMin = new Vector2(0.28f, 0.1f);
            crt.anchorMax = new Vector2(0.97f, 0.78f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;

            var portFrameGo = new GameObject("PortraitFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portFrameGo.transform.SetParent(root.transform, false);
            var portFrame = portFrameGo.GetComponent<Image>();
            portFrame.sprite = UiArt.SoftPanel();
            portFrame.type = Image.Type.Sliced;
            portFrame.color = new Color(0.85f, 0.55f, 0.2f, 0.9f);
            portFrame.raycastTarget = false;
            var pfrt = portFrame.rectTransform;
            pfrt.anchorMin = pfrt.anchorMax = new Vector2(0.16f, 0.42f);
            pfrt.sizeDelta = new Vector2(272f, 272f);

            var portGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portGo.transform.SetParent(root.transform, false);
            portraitImage = portGo.GetComponent<Image>();
            portraitImage.sprite = GeneratedArt.PortraitKael();
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            var prt = portraitImage.rectTransform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.16f, 0.42f);
            prt.sizeDelta = new Vector2(250f, 250f);

            speakerLabel = MakeText(root.transform, "Speaker", "Kael", 30,
                new Vector2(0.32f, 0.62f), new Vector2(0.96f, 0.82f), new Color(1f, 0.86f, 0.45f));
            speakerLabel.alignment = TextAnchor.MiddleLeft;
            speakerLabel.fontStyle = FontStyle.Bold;
            speakerLabel.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);

            bodyLabel = MakeText(root.transform, "Body", "", 32,
                new Vector2(0.32f, 0.16f), new Vector2(0.96f, 0.6f),
                new Color(0.98f, 0.96f, 0.92f));
            bodyLabel.alignment = TextAnchor.UpperLeft;
            bodyLabel.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);

            var hint = MakeText(root.transform, "Hint", "ketuk ›", 22,
                new Vector2(0.72f, 0.04f), new Vector2(0.96f, 0.16f),
                new Color(1f, 0.9f, 0.7f));
            hint.alignment = TextAnchor.MiddleRight;

            var trigger = root.GetComponent<EventTrigger>() ?? root.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entry.callback.AddListener(_ =>
            {
                tap = true;
                AudioDirector.Instance?.PlayUi();
            });
            trigger.triggers.Add(entry);

            root.SetActive(false);
        }

        public void Play(IEnumerable<DialogLine> lines, System.Action complete = null)
        {
            queue.Clear();
            foreach (var l in lines) queue.Enqueue(l);
            onComplete = complete;
            if (!isActiveAndEnabled) return;
            StopAllCoroutines();
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            root.SetActive(true);
            PortraitMobileHud.Instance?.SetControlsVisible(false);
            VerticalSliceDirector.Instance?.SetPlayerFrozenPublic(true);

            while (queue.Count > 0)
            {
                var line = queue.Dequeue();
                speakerLabel.text = string.IsNullOrEmpty(line.Speaker) ? "…" : line.Speaker;
                if (portraitImage != null)
                {
                    portraitImage.sprite = GeneratedArt.PortraitFor(line.Speaker);
                }

                // Typewriter reveal — tap once to reveal instantly, tap again to advance.
                string full = line.Text ?? "";
                bodyLabel.text = "";
                tap = false;
                float revealed = 0f;
                const float charsPerSecond = 42f;
                while (revealed < full.Length)
                {
                    if (tap) { tap = false; break; }
                    revealed += Time.unscaledDeltaTime * charsPerSecond;
                    int shown = Mathf.Clamp(Mathf.FloorToInt(revealed), 0, full.Length);
                    bodyLabel.text = full.Substring(0, shown);
                    yield return null;
                }
                bodyLabel.text = full;

                tap = false;
                yield return new WaitForSecondsRealtime(0.12f);
                tap = false;
                while (!tap) yield return null;
            }

            root.SetActive(false);
            PortraitMobileHud.Instance?.SetControlsVisible(true);
            VerticalSliceDirector.Instance?.SetPlayerFrozenPublic(false);
            var cb = onComplete;
            onComplete = null;
            cb?.Invoke();
        }

        static Text MakeText(Transform parent, string name, string content, int size,
            Vector2 amin, Vector2 amax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = amin;
            rt.anchorMax = amax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            UiArt.StyleLabel(t, size, color);
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}
