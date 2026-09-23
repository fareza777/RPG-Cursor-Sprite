using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Emberwake
{
    /// <summary>
    /// Portrait HUD: boot flow + polished top bar + D-pad + action buttons + dialog.
    /// </summary>
    public class PortraitMobileHud : MonoBehaviour
    {
        public static PortraitMobileHud Instance { get; private set; }

        public bool GameStarted { get; private set; }

        Transform heartsRow;
        Image stamFill;
        Image hpFill;
        Text stamLabel;
        Text rankLabel;
        Text hpLabel;
        Text objectiveLabel;
        Text toastLabel;
        GameObject hudRoot;
        GameObject controlsRoot;
        Transform canvasTransform;
        float toastTimer;
        Image[] heartIcons = new Image[8];
        System.Action onPlay;

        void Awake() => Instance = this;

        public void Build(System.Action playCallback)
        {
            onPlay = playCallback;
            EnsureEventSystem();

            var canvasGo = new GameObject("PortraitCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasTransform = canvasGo.transform;
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.7f;

            hudRoot = new GameObject("HUD", typeof(RectTransform));
            hudRoot.transform.SetParent(canvasTransform, false);
            StretchFull((RectTransform)hudRoot.transform);
            BuildTopHud(hudRoot.transform);
            BuildObjectiveBanner(hudRoot.transform);
            hudRoot.AddComponent<QuestTrackerHud>().Build(hudRoot.transform);

            controlsRoot = new GameObject("Controls", typeof(RectTransform));
            controlsRoot.transform.SetParent(hudRoot.transform, false);
            StretchFull((RectTransform)controlsRoot.transform);
            BuildJoystick(controlsRoot.transform);
            BuildActionButtons(controlsRoot.transform);
            BuildMenuButton(controlsRoot.transform);

            hudRoot.SetActive(false);
            GameStarted = false;

            var dialogHost = new GameObject("DialogHost");
            dialogHost.transform.SetParent(transform, false);
            var dialog = dialogHost.AddComponent<DialogBox>();
            dialog.Build(canvasTransform);

            var menuHost = new GameObject("MenuHub");
            menuHost.transform.SetParent(transform, false);
            menuHost.AddComponent<GameMenuHub>().Build(canvasTransform);

            canvasGo.AddComponent<ScreenVignette>().Build(canvasTransform);

            var flowHost = new GameObject("BootstrapFlow");
            flowHost.transform.SetParent(transform, false);
            var flow = flowHost.AddComponent<GameBootstrapFlow>();
            flow.Begin(canvasTransform, OnBootPlayPressed);
        }

        void BuildMenuButton(Transform parent)
        {
            var img = CreateImage(parent, "BtnMenu", GeneratedArt.IconMenu(),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-72f, -220f), new Vector2(88f, 88f),
                new Color(1f, 0.92f, 0.75f, 0.95f), true);
            img.type = Image.Type.Simple;
            AddPress(img.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                GameMenuHub.Instance?.Toggle();
            });
        }

        void OnBootPlayPressed()
        {
            ShowGameplayHud();
            onPlay?.Invoke();
        }

        public void ShowGameplayHud()
        {
            if (hudRoot != null) hudRoot.SetActive(true);
            SetControlsVisible(true);
            GameStarted = true;
            RefreshBars();
        }

        public void SetControlsVisible(bool visible)
        {
            if (controlsRoot != null) controlsRoot.SetActive(visible);
        }

        public void SetObjective(string msg)
        {
            if (objectiveLabel != null) objectiveLabel.text = msg ?? "";
        }

        public void ShowToast(string msg, float seconds = 2.2f)
        {
            if (toastLabel == null) return;
            toastLabel.text = msg ?? "";
            toastTimer = seconds;
            toastLabel.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        }

        void Update()
        {
            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f && toastLabel != null)
                {
                    toastLabel.text = "";
                    toastLabel.gameObject.SetActive(false);
                }
            }
            if (GameStarted) RefreshBars();
        }

        void RefreshBars()
        {
            if (GameManager.Instance == null) return;
            var s = GameManager.Instance.Stats;
            var w = GameManager.Instance.WickRank;
            if (s == null) return;

            int max = Mathf.Clamp(s.MaxHearts, 1, heartIcons.Length);
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] == null) continue;
                bool show = i < max;
                heartIcons[i].gameObject.SetActive(show);
                if (show)
                {
                    heartIcons[i].enabled = true;
                    heartIcons[i].color = i < s.Hearts
                        ? new Color(1f, 0.28f, 0.35f, 1f)
                        : new Color(0.25f, 0.2f, 0.22f, 0.9f);
                }
            }

            if (hpLabel != null)
                hpLabel.text = $"HP {s.Hearts}/{s.MaxHearts}";

            if (hpFill != null)
            {
                hpFill.enabled = true;
                hpFill.fillAmount = s.MaxHearts > 0 ? Mathf.Clamp01(s.Hearts / (float)s.MaxHearts) : 0f;
            }
            if (stamFill != null)
            {
                stamFill.enabled = true;
                stamFill.fillAmount = s.MaxStamina > 0 ? Mathf.Clamp01(s.Stamina / s.MaxStamina) : 0f;
            }
            if (stamLabel != null)
                stamLabel.text = $"ST {Mathf.CeilToInt(s.Stamina)}";
            if (rankLabel != null)
            {
                var lv = LevelingSystem.Instance;
                string lvTxt = lv != null ? $"Lv{lv.Level}" : "Lv1";
                rankLabel.text = w != null ? $"{lvTxt} · Wick {w.Rank}" : lvTxt;
            }
        }

        void BuildTopHud(Transform parent)
        {
            float topPad = Mathf.Max(28f, Screen.height * 0.025f);

            // Full-width status strip — always visible
            var strip = new GameObject("StatusStrip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            strip.transform.SetParent(parent, false);
            var stripImg = strip.GetComponent<Image>();
            stripImg.sprite = UiArt.SoftPanel();
            stripImg.type = Image.Type.Sliced;
            stripImg.color = new Color(0.1f, 0.06f, 0.08f, 0.92f);
            stripImg.raycastTarget = false;
            var srt = strip.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 1f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0f, -topPad);
            srt.sizeDelta = new Vector2(-32f, 132f);

            // Hearts row
            var rowGo = new GameObject("HeartsRow", typeof(RectTransform));
            rowGo.transform.SetParent(strip.transform, false);
            heartsRow = rowGo.transform;
            var rowRt = (RectTransform)heartsRow;
            rowRt.anchorMin = new Vector2(0f, 0.5f);
            rowRt.anchorMax = new Vector2(0.55f, 1f);
            rowRt.offsetMin = new Vector2(16f, 4f);
            rowRt.offsetMax = new Vector2(-8f, -8f);

            for (int i = 0; i < heartIcons.Length; i++)
            {
                var h = CreateImage(heartsRow, "H" + i, UiArt.HeartSprite(),
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(28f + i * 52f, 0f), new Vector2(46f, 42f),
                    new Color(1f, 0.28f, 0.35f, 1f), true);
                h.type = Image.Type.Simple;
                h.raycastTarget = false;
                heartIcons[i] = h;
            }

            // HP numeric (guaranteed readable even if heart sprites fail)
            hpLabel = CreateAnchoredText(strip.transform, "HpTxt", "HP 3/3", 24,
                TextAnchor.MiddleLeft, new Vector2(0f, 0.55f), new Vector2(0f, 0.55f),
                new Vector2(190f, 0f), new Vector2(180f, 40f), new Color(1f, 0.75f, 0.75f));

            var hpBg = CreateImage(strip.transform, "HpBg", UiArt.SoftPanel(),
                new Vector2(0f, 0.28f), new Vector2(0.98f, 0.5f),
                Vector2.zero, Vector2.zero,
                new Color(0.22f, 0.08f, 0.1f, 1f));
            hpBg.raycastTarget = false;
            var hbrt = hpBg.rectTransform;
            hbrt.offsetMin = new Vector2(16f, 2f);
            hbrt.offsetMax = new Vector2(-16f, -2f);
            hpFill = CreateImage(hpBg.transform, "HpFill", UiArt.WhiteQuad(),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.92f, 0.22f, 0.28f, 1f));
            hpFill.raycastTarget = false;
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            hpFill.fillAmount = 1f;
            var hfrt = hpFill.rectTransform;
            hfrt.offsetMin = new Vector2(3f, 3f);
            hfrt.offsetMax = new Vector2(-3f, -3f);

            // Stamina bar — solid white quad fill (works with Image.Filled)
            var stamBg = CreateImage(strip.transform, "StamBg", UiArt.SoftPanel(),
                new Vector2(0f, 0.04f), new Vector2(0.98f, 0.26f),
                Vector2.zero, Vector2.zero,
                new Color(0.12f, 0.14f, 0.18f, 1f));
            stamBg.raycastTarget = false;
            var sbrt = stamBg.rectTransform;
            sbrt.offsetMin = new Vector2(16f, 2f);
            sbrt.offsetMax = new Vector2(-16f, -2f);

            stamFill = CreateImage(stamBg.transform, "StamFill", UiArt.WhiteQuad(),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.25f, 0.82f, 1f, 1f));
            stamFill.raycastTarget = false;
            stamFill.type = Image.Type.Filled;
            stamFill.fillMethod = Image.FillMethod.Horizontal;
            stamFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            stamFill.fillAmount = 1f;
            var frt = stamFill.rectTransform;
            frt.offsetMin = new Vector2(3f, 3f);
            frt.offsetMax = new Vector2(-3f, -3f);

            stamLabel = CreateAnchoredText(stamBg.transform, "StamTxt", "ST 50", 20,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(140f, 36f), Color.white);

            rankLabel = CreateAnchoredText(strip.transform, "Rank", "Lv1 · Wick 1", 22,
                TextAnchor.MiddleRight, new Vector2(1f, 0.55f), new Vector2(1f, 0.55f),
                new Vector2(-16f, 0f), new Vector2(340f, 48f), new Color(1f, 0.86f, 0.4f));
            rankLabel.fontStyle = FontStyle.Bold;
        }

        void BuildObjectiveBanner(Transform parent)
        {
            float topPad = Screen.height > 2000 ? 200f : 168f;
            var banner = CreateImage(parent, "ObjectiveBanner", UiArt.SoftPanel(),
                new Vector2(0.08f, 1f), new Vector2(0.92f, 1f),
                new Vector2(0f, -topPad), new Vector2(0f, 64f),
                new Color(0.16f, 0.1f, 0.05f, 0.88f));
            banner.raycastTarget = false;
            var brt = banner.rectTransform;
            brt.pivot = new Vector2(0.5f, 1f);
            brt.offsetMin = new Vector2(0f, brt.offsetMin.y);
            brt.offsetMax = new Vector2(0f, -topPad);
            brt.sizeDelta = new Vector2(brt.sizeDelta.x, 64f);

            objectiveLabel = CreateAnchoredText(banner.transform, "Objective", "", 26,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(860f, 54f), new Color(1f, 0.93f, 0.68f));

            toastLabel = CreateAnchoredText(parent, "Toast", "", 24,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -(topPad + 160f)), new Vector2(900f, 54f), new Color(1f, 0.85f, 0.4f));
            toastLabel.gameObject.SetActive(false);
        }

        void BuildJoystick(Transform parent)
        {
            float pad = 300f;
            float cx = 200f;
            float cy = 280f;

            var plate = CreateImage(parent, "DpadPlate", UiArt.SoftPanel(),
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(cx, cy), new Vector2(pad + 48f, pad + 48f),
                new Color(0.04f, 0.05f, 0.07f, 0.62f));
            plate.raycastTarget = false;

            var hit = CreateImage(parent, "DpadHit", UiArt.WhiteCircle(),
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(cx, cy), new Vector2(pad + 48f, pad + 48f),
                new Color(1f, 1f, 1f, 0.01f), true);
            hit.type = Image.Type.Simple;

            var joyBg = CreateImage(hit.transform, "DpadRing", UiArt.JoystickRing(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(pad, pad),
                new Color(1f, 1f, 1f, 0.98f), true);
            joyBg.type = Image.Type.Simple;
            joyBg.raycastTarget = false;

            // Clearer D-pad cross
            CreateImage(joyBg.transform, "ArmH", UiArt.SoftPanel(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(pad * 0.78f, 64f),
                new Color(1f, 1f, 1f, 0.18f)).raycastTarget = false;
            CreateImage(joyBg.transform, "ArmV", UiArt.SoftPanel(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(64f, pad * 0.78f),
                new Color(1f, 1f, 1f, 0.18f)).raycastTarget = false;

            CreateAnchoredText(joyBg.transform, "N", "▲", 28,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, pad * 0.32f), new Vector2(60f, 40f), new Color(1f, 1f, 1f, 0.45f));
            CreateAnchoredText(joyBg.transform, "S", "▼", 28,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -pad * 0.32f), new Vector2(60f, 40f), new Color(1f, 1f, 1f, 0.45f));
            CreateAnchoredText(joyBg.transform, "W", "◀", 28,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-pad * 0.32f, 0f), new Vector2(60f, 40f), new Color(1f, 1f, 1f, 0.45f));
            CreateAnchoredText(joyBg.transform, "E", "▶", 28,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(pad * 0.32f, 0f), new Vector2(60f, 40f), new Color(1f, 1f, 1f, 0.45f));

            var handle = CreateImage(joyBg.transform, "Handle", UiArt.WhiteCircle(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(108f, 108f),
                new Color(1f, 0.86f, 0.28f, 1f), true);
            handle.type = Image.Type.Simple;
            handle.raycastTarget = false;

            var joy = hit.gameObject.AddComponent<VirtualJoystick>();
            joy.Setup(joyBg.rectTransform, handle.rectTransform, pad * 0.38f);
        }

        void BuildActionButtons(Transform parent)
        {
            // Right cluster: ATK primary, SPIN secondary — clear thumb gap from D-pad
            CreateRoundButton(parent, "BtnAttack", "ATK",
                new Vector2(-170f, 360f), 200f,
                new Color(0.9f, 0.26f, 0.2f, 0.97f),
                () => GameInput.PressAttack());
            CreateRoundButton(parent, "BtnSpin", "SPIN",
                new Vector2(-360f, 175f), 150f,
                new Color(0.58f, 0.3f, 0.86f, 0.97f),
                () => GameInput.PressSpin());
        }

        void CreateRoundButton(Transform parent, string name, string label, Vector2 pos, float size, Color color, System.Action onPress)
        {
            var ring = CreateImage(parent, name + "Ring", UiArt.WhiteCircle(),
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                pos, new Vector2(size + 34f, size + 34f),
                new Color(1f, 0.95f, 0.7f, 0.32f), true);
            ring.type = Image.Type.Simple;
            ring.raycastTarget = false;

            var img = CreateImage(parent, name, UiArt.WhiteCircle(),
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                pos, new Vector2(size, size), color, true);
            img.type = Image.Type.Simple;

            var inner = CreateImage(img.transform, "Inner", UiArt.WhiteCircle(),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 8f), new Vector2(size * 0.52f, size * 0.38f),
                new Color(1f, 1f, 1f, 0.2f), true);
            inner.type = Image.Type.Simple;
            inner.raycastTarget = false;

            CreateAnchoredText(img.transform, "Label", label, Mathf.RoundToInt(size * 0.2f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(size * 0.9f, 56f), Color.white);

            AddPress(img.gameObject, onPress);
        }

        static void AddPress(GameObject go, System.Action onPress)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ =>
            {
                go.transform.localScale = Vector3.one * 0.88f;
                onPress?.Invoke();
            });
            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => go.transform.localScale = Vector3.one);
            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => go.transform.localScale = Vector3.one);
            trigger.triggers.Add(down);
            trigger.triggers.Add(up);
            trigger.triggers.Add(exit);
        }

        static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Image CreateImage(Transform parent, string name, Sprite sprite,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color,
            bool preserveAspect = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.preserveAspect = preserveAspect;
            img.color = color;
            img.raycastTarget = true;
            return img;
        }

        static Text CreateAnchoredText(Transform parent, string name, string content, int size,
            TextAnchor align, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = pos;
            var text = go.GetComponent<Text>();
            UiArt.StyleLabel(text, size, color);
            text.alignment = align;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
