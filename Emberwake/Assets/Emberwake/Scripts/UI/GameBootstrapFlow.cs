using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Emberwake
{
    /// <summary>
    /// Portrait boot sequence: Splash → Cinematic → Onboarding → Main Menu → Play.
    /// </summary>
    public class GameBootstrapFlow : MonoBehaviour
    {
        public static GameBootstrapFlow Instance { get; private set; }

        Transform canvasRoot;
        System.Action onStartGame;
        GameObject layer;
        bool skipRequested;
        bool seenOnboarding;

        public void Begin(Transform canvas, System.Action startGame)
        {
            Instance = this;
            canvasRoot = canvas;
            onStartGame = startGame;
            seenOnboarding = PlayerPrefs.GetInt("ew_onboard", 0) == 1;
            StartCoroutine(RunFlow());
        }

        IEnumerator RunFlow()
        {
            if (EmulatorAutoQa.Enabled)
            {
                // Fast path for visual QA on emulator
                yield return Splash();
                ClearLayer();
                onStartGame?.Invoke();
                yield break;
            }
            yield return Splash();
            yield return Cinematic();
            if (!seenOnboarding)
                yield return Onboarding();
            yield return MainMenu();
        }

        IEnumerator Splash()
        {
            ClearLayer();
            layer = FullLayer("Splash");
            var bg = Panel(layer.transform, "Bg", Color.black, true);
            Stretch(bg.rectTransform);
            AudioDirector.Instance?.SetMusicMood("title");

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(layer.transform, false);
            var irt = iconGo.GetComponent<RectTransform>();
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.62f);
            irt.sizeDelta = new Vector2(260f, 260f);
            var iimg = iconGo.GetComponent<Image>();
            iimg.sprite = GeneratedArt.AppIcon512();
            iimg.preserveAspect = true;
            iimg.color = new Color(1f, 1f, 1f, 0f);

            var logo = Label(layer.transform, "Logo", "EMBERWAKE", 78,
                new Vector2(0.5f, 0.42f), new Color(1f, 0.82f, 0.35f));
            logo.fontStyle = FontStyle.Bold;
            logo.color = new Color(1f, 0.82f, 0.35f, 0f);

            var sub = Label(layer.transform, "Studio", "Emberwake Studio", 28,
                new Vector2(0.5f, 0.34f), new Color(0.75f, 0.72f, 0.65f, 0f));

            float t = 0f;
            float splashDur = EmulatorAutoQa.Enabled ? 0.45f : 1.2f;
            while (t < splashDur)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / Mathf.Max(0.2f, splashDur * 0.6f));
                iimg.color = new Color(1f, 1f, 1f, a);
                logo.color = new Color(1f, 0.82f, 0.35f, a);
                sub.color = new Color(0.75f, 0.72f, 0.65f, a * 0.9f);
                yield return null;
            }
            if (EmulatorAutoQa.Enabled) yield break;
            yield return new WaitForSecondsRealtime(0.9f);
            t = 0f;
            while (t < 0.55f)
            {
                t += Time.unscaledDeltaTime;
                float a = 1f - Mathf.Clamp01(t / 0.55f);
                iimg.color = new Color(1f, 1f, 1f, a);
                logo.color = new Color(1f, 0.82f, 0.35f, a);
                sub.color = new Color(0.75f, 0.72f, 0.65f, a * 0.9f);
                yield return null;
            }
        }

        IEnumerator Cinematic()
        {
            ClearLayer();
            layer = FullLayer("Cinematic");
            var bg = Panel(layer.transform, "Bg", Color.white, true);
            Stretch(bg.rectTransform);
            bg.sprite = PaintedArt.Scene("village") ?? GeneratedArt.VillageNight();
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;

            var scrim = Panel(layer.transform, "Scrim", Color.white, false);
            scrim.sprite = PaintedArt.Scrim();
            scrim.type = Image.Type.Simple;
            var srt = scrim.rectTransform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = new Vector2(1f, 0.46f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;

            var kicker = Label(layer.transform, "Kicker", "PROLOG", 28,
                new Vector2(0.5f, 0.93f), new Color(1f, 0.86f, 0.55f));
            kicker.fontStyle = FontStyle.Bold;
            kicker.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

            var portGo = new GameObject("CinePort", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portGo.transform.SetParent(layer.transform, false);
            var port = portGo.GetComponent<Image>();
            port.preserveAspect = true;
            port.raycastTarget = false;
            var prt = port.rectTransform;
            prt.anchorMin = prt.anchorMax = new Vector2(0.16f, 0.2f);
            prt.sizeDelta = new Vector2(210f, 210f);

            var skip = Panel(layer.transform, "Skip", new Color(0.15f, 0.15f, 0.18f, 0.85f), true);
            skip.rectTransform.anchorMin = skip.rectTransform.anchorMax = new Vector2(1f, 1f);
            skip.rectTransform.pivot = new Vector2(1f, 1f);
            skip.rectTransform.anchoredPosition = new Vector2(-28f, -48f);
            skip.rectTransform.sizeDelta = new Vector2(160f, 64f);
            Label(skip.transform, "SkipTxt", "LEWATI", 26,
                new Vector2(0.5f, 0.5f), Color.white);
            skipRequested = false;
            AddTap(skip.gameObject, () => skipRequested = true);

            (string scene, string speaker, string line)[] lines =
            {
                ("village", "", "Di Virelia, setiap desa hidup dari Wick…"),
                ("village", "", "Api suci yang menjaga memori, musim, dan nama."),
                ("altar", "Wick", "Tapi Wick padam satu per satu — seperti bintang jatuh."),
                ("village", "Mara", "Di Millbrook, lentera terakhir hampir mati."),
                ("village", "Kael", "Kael terbangun di debu jalan tanpa bayangan —\ndan tanpa tujuh tahun terakhirnya."),
                ("altar", "Mara", "Jika Wick padam…\ndunia akan lupa namanya sendiri."),
                ("forest", "Kael", "Jadi Kael berjalan ke utara.\nKe Hollowroot. Ke api yang masih bisa diselamatkan.")
            };

            var who = Label(layer.transform, "Who", "", 30,
                new Vector2(0.62f, 0.3f), new Color(1f, 0.86f, 0.45f));
            who.fontStyle = FontStyle.Bold;
            who.alignment = TextAnchor.MiddleLeft;
            who.rectTransform.sizeDelta = new Vector2(640f, 48f);
            who.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);

            var body = Label(layer.transform, "Body", "", 30,
                new Vector2(0.62f, 0.16f), new Color(0.97f, 0.95f, 0.9f));
            body.rectTransform.sizeDelta = new Vector2(680f, 160f);
            body.alignment = TextAnchor.UpperLeft;
            body.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);

            var hint = Label(layer.transform, "Hint", "ketuk LEWATI untuk lanjut", 22,
                new Vector2(0.5f, 0.12f), new Color(0.55f, 0.55f, 0.5f));

            foreach (var beat in lines)
            {
                if (skipRequested) break;
                var scene = PaintedArt.Scene(beat.scene);
                if (scene != null) bg.sprite = scene;
                who.text = beat.speaker;
                port.sprite = string.IsNullOrEmpty(beat.speaker)
                    ? GeneratedArt.AppIcon512()
                    : GeneratedArt.PortraitFor(beat.speaker);
                port.gameObject.SetActive(!string.IsNullOrEmpty(beat.speaker));
                port.color = Color.white;
                body.text = "";
                body.color = new Color(0.95f, 0.92f, 0.82f, 0f);
                float t = 0f;
                body.text = beat.line;
                while (t < 0.45f && !skipRequested)
                {
                    t += Time.unscaledDeltaTime;
                    body.color = new Color(0.95f, 0.92f, 0.82f, Mathf.Clamp01(t / 0.45f));
                    yield return null;
                }
                float hold = 0f;
                while (hold < 1.65f && !skipRequested)
                {
                    hold += Time.unscaledDeltaTime;
                    yield return null;
                }
                t = 0f;
                while (t < 0.35f && !skipRequested)
                {
                    t += Time.unscaledDeltaTime;
                    body.color = new Color(0.95f, 0.92f, 0.82f, 1f - Mathf.Clamp01(t / 0.35f));
                    yield return null;
                }
            }
        }

        IEnumerator Onboarding()
        {
            ClearLayer();
            layer = FullLayer("Onboard");
            var bg = Panel(layer.transform, "Bg", Color.white, true);
            Stretch(bg.rectTransform);
            var paintedMenu = PaintedArt.Load("menu_bg");
            if (paintedMenu != null)
            {
                bg.sprite = paintedMenu;
                bg.type = Image.Type.Simple;
            }
            var veil = Panel(layer.transform, "Veil", new Color(0.02f, 0.03f, 0.05f, 0.55f), false);
            Stretch(veil.rectTransform);

            Label(layer.transform, "Title", "CARA BERMAIN", 48,
                new Vector2(0.5f, 0.86f), new Color(1f, 0.85f, 0.4f)).fontStyle = FontStyle.Bold;

            string[] tips =
            {
                "1  D-pad kiri — gerak Kael (8 arah)",
                "2  ATK kanan atas — tebas musuh",
                "3  SPIN kanan bawah — serangan putar",
                "4  Ikuti misi kuning di atas layar",
                "5  Ketuk dialog untuk lanjut cerita"
            };
            for (int i = 0; i < tips.Length; i++)
            {
                Label(layer.transform, "Tip" + i, tips[i], 30,
                    new Vector2(0.5f, 0.72f - i * 0.08f), new Color(0.92f, 0.9f, 0.84f));
            }

            bool done = false;
            var btn = FramedButton(layer.transform, "BtnOk",
                new Color(0.78f, 0.45f, 0.12f, 1f), new Color(1f, 0.85f, 0.45f, 1f));
            btn.rectTransform.anchorMin = btn.rectTransform.anchorMax = new Vector2(0.5f, 0.14f);
            btn.rectTransform.sizeDelta = new Vector2(460f, 114f);
            Label(btn.transform, "OkTxt", "MENGERTI", 40,
                new Vector2(0.5f, 0.5f), Color.white).fontStyle = FontStyle.Bold;
            AddTap(btn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                done = true;
            });

            while (!done) yield return null;
            PlayerPrefs.SetInt("ew_onboard", 1);
            PlayerPrefs.Save();
            seenOnboarding = true;
        }

        IEnumerator MainMenu()
        {
            ClearLayer();
            layer = FullLayer("MainMenu");

            var bg = Panel(layer.transform, "Bg", Color.white, true);
            Stretch(bg.rectTransform);
            var menuArt = PaintedArt.Load("menu_bg");
            if (menuArt != null)
            {
                bg.sprite = menuArt;
                bg.type = Image.Type.Simple;
            }
            var scrim = Panel(layer.transform, "Scrim", Color.white, false);
            scrim.sprite = PaintedArt.Scrim();
            scrim.type = Image.Type.Simple;
            var ms = scrim.rectTransform;
            ms.anchorMin = Vector2.zero;
            ms.anchorMax = new Vector2(1f, 0.72f);
            ms.offsetMin = ms.offsetMax = Vector2.zero;

            // Brand hero
            var brand = Label(layer.transform, "Brand", "EMBERWAKE", 86,
                new Vector2(0.5f, 0.72f), new Color(1f, 0.8f, 0.32f));
            brand.fontStyle = FontStyle.Bold;
            brand.rectTransform.sizeDelta = new Vector2(980f, 140f);

            Label(layer.transform, "Tag", "Ketika lentera terakhir padam,\ndunia lupa namanya sendiri.", 26,
                new Vector2(0.5f, 0.62f), new Color(0.9f, 0.88f, 0.8f));

            string page = "home";
            bool play = false;
            var body = Label(layer.transform, "PageBody", "", 24,
                new Vector2(0.5f, 0.46f), new Color(0.92f, 0.9f, 0.84f));
            body.rectTransform.sizeDelta = new Vector2(860f, 280f);

            var playBtn = FramedButton(layer.transform, "Play",
                new Color(0.82f, 0.42f, 0.1f, 1f), new Color(1f, 0.85f, 0.4f, 1f));
            playBtn.rectTransform.anchorMin = playBtn.rectTransform.anchorMax = new Vector2(0.5f, 0.34f);
            playBtn.rectTransform.sizeDelta = new Vector2(520f, 100f);
            var playTxt = Label(playBtn.transform, "PlayTxt", "GAME BARU", 32,
                new Vector2(0.5f, 0.5f), Color.white);
            playTxt.fontStyle = FontStyle.Bold;

            var contBtn = FramedButton(layer.transform, "Continue",
                new Color(0.18f, 0.22f, 0.28f, 1f), new Color(0.65f, 0.7f, 0.55f, 0.9f));
            contBtn.rectTransform.anchorMin = contBtn.rectTransform.anchorMax = new Vector2(0.5f, 0.25f);
            contBtn.rectTransform.sizeDelta = new Vector2(480f, 84f);
            Label(contBtn.transform, "ContTxt", "LANJUTKAN", 26,
                new Vector2(0.5f, 0.5f), new Color(0.92f, 0.95f, 0.85f));

            float rowY = 0.155f;
            var setBtn = SmallBtn(layer.transform, "Settings", "SETTING", new Vector2(0.22f, rowY));
            var aboutBtn = SmallBtn(layer.transform, "About", "TENTANG", new Vector2(0.42f, rowY));
            var shareBtn = SmallBtn(layer.transform, "Share", "BAGIKAN", new Vector2(0.62f, rowY));
            var rateBtn = SmallBtn(layer.transform, "Rate", "NILAI", new Vector2(0.82f, rowY));

            void ShowHome()
            {
                page = "home";
                body.text = "Quest: Wick Hollowroot → Gloves → Barkling → Altar\n\nNew Game mulai dari Millbrook.\nLanjutkan memuat save terakhir.";
                playTxt.text = "GAME BARU";
                playBtn.gameObject.SetActive(true);
                contBtn.gameObject.SetActive(true);
            }

            AddTap(playBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayStart();
                if (page == "home")
                {
                    SaveSystem.ClearContinue();
                    play = true;
                }
                else ShowHome();
            });
            AddTap(contBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                var save = Object.FindFirstObjectByType<SaveSystem>();
                if (save != null && save.TryLoad(out _))
                {
                    SaveSystem.RequestContinue();
                    PortraitMobileHud.Instance?.ShowToast("Save dimuat");
                    play = true;
                }
                else
                {
                    SaveSystem.ClearContinue();
                    PortraitMobileHud.Instance?.ShowToast("Belum ada save — mulai baru");
                }
            });
            AddTap(setBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                page = "settings";
                playBtn.gameObject.SetActive(false);
                contBtn.gameObject.SetActive(false);
                body.text = SettingsText();
                playBtn.gameObject.SetActive(true);
                playTxt.text = "KEMBALI";
            });
            AddTap(aboutBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                page = "about";
                contBtn.gameObject.SetActive(false);
                playBtn.gameObject.SetActive(true);
                playTxt.text = "KEMBALI";
                body.text = "EMBERWAKE\nEmberwake Studio · v0.2\n\nAction RPG portrait.\nKael menjaga Wick terakhir Virelia.\n\nMillbrook → Hollowroot → Altar.";
            });
            AddTap(shareBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                GameSettings.ShareGame();
            });
            AddTap(rateBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                GameSettings.RateOnStore();
            });

            // Settings adjust while settings page open
            var hitL = Panel(layer.transform, "HitL", new Color(0, 0, 0, 0.01f), true);
            hitL.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            hitL.rectTransform.anchorMax = new Vector2(0.5f, 0.58f);
            hitL.rectTransform.offsetMin = hitL.rectTransform.offsetMax = Vector2.zero;
            var hitR = Panel(layer.transform, "HitR", new Color(0, 0, 0, 0.01f), true);
            hitR.rectTransform.anchorMin = new Vector2(0.5f, 0.4f);
            hitR.rectTransform.anchorMax = new Vector2(1f, 0.58f);
            hitR.rectTransform.offsetMin = hitR.rectTransform.offsetMax = Vector2.zero;
            AddTap(hitL.gameObject, () =>
            {
                if (page != "settings") return;
                GameSettings.MusicVolume = Mathf.Clamp01(GameSettings.MusicVolume - 0.1f);
                body.text = SettingsText();
            });
            AddTap(hitR.gameObject, () =>
            {
                if (page != "settings") return;
                GameSettings.MusicVolume = Mathf.Clamp01(GameSettings.MusicVolume + 0.1f);
                body.text = SettingsText();
            });
            var hitU = Panel(layer.transform, "HitU", new Color(0, 0, 0, 0.01f), true);
            hitU.rectTransform.anchorMin = new Vector2(0.15f, 0.58f);
            hitU.rectTransform.anchorMax = new Vector2(0.85f, 0.7f);
            hitU.rectTransform.offsetMin = hitU.rectTransform.offsetMax = Vector2.zero;
            var hitD = Panel(layer.transform, "HitD", new Color(0, 0, 0, 0.01f), true);
            hitD.rectTransform.anchorMin = new Vector2(0.15f, 0.38f);
            hitD.rectTransform.anchorMax = new Vector2(0.85f, 0.5f);
            hitD.rectTransform.offsetMin = hitD.rectTransform.offsetMax = Vector2.zero;
            AddTap(hitU.gameObject, () =>
            {
                if (page != "settings") return;
                GameSettings.SfxVolume = Mathf.Clamp01(GameSettings.SfxVolume + 0.1f);
                body.text = SettingsText();
            });
            AddTap(hitD.gameObject, () =>
            {
                if (page != "settings") return;
                GameSettings.SfxVolume = Mathf.Clamp01(GameSettings.SfxVolume - 0.1f);
                body.text = SettingsText();
            });

            string SettingsText() =>
                "SETTING\n\nMusik  " + Mathf.RoundToInt(GameSettings.MusicVolume * 100) +
                "%\nSFX    " + Mathf.RoundToInt(GameSettings.SfxVolume * 100) +
                "%\n\nKiri / kanan = musik\nAtas / bawah = SFX";

            ShowHome();

            var guideBtn = FramedButton(layer.transform, "Guide",
                new Color(0.14f, 0.16f, 0.22f, 1f), new Color(0.55f, 0.55f, 0.5f, 0.9f));
            guideBtn.rectTransform.anchorMin = guideBtn.rectTransform.anchorMax = new Vector2(0.5f, 0.075f);
            guideBtn.rectTransform.sizeDelta = new Vector2(360f, 70f);
            Label(guideBtn.transform, "GuideTxt", "ULANG TUTORIAL", 22,
                new Vector2(0.5f, 0.5f), new Color(0.92f, 0.9f, 0.84f));
            AddTap(guideBtn.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                PlayerPrefs.SetInt("ew_onboard", 0);
                PlayerPrefs.Save();
                seenOnboarding = false;
                play = false;
                StopAllCoroutines();
                StartCoroutine(RestartFromOnboard());
            });

            while (!play) yield return null;

            ClearLayer();
            onStartGame?.Invoke();
        }

        static Image SmallBtn(Transform parent, string name, string text, Vector2 anchor)
        {
            var btn = FramedButton(parent, name,
                new Color(0.12f, 0.13f, 0.18f, 1f), new Color(0.7f, 0.6f, 0.35f, 0.85f));
            btn.rectTransform.anchorMin = btn.rectTransform.anchorMax = anchor;
            btn.rectTransform.sizeDelta = new Vector2(180f, 72f);
            Label(btn.transform, "T", text, 20, new Vector2(0.5f, 0.5f), new Color(0.95f, 0.9f, 0.75f));
            return btn;
        }

        IEnumerator RestartFromOnboard()
        {
            yield return Onboarding();
            yield return MainMenu();
        }

        void ClearLayer()
        {
            if (layer != null) Destroy(layer);
            layer = null;
        }

        GameObject FullLayer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvasRoot, false);
            Stretch((RectTransform)go.transform);
            return go;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Image Panel(Transform parent, string name, Color color, bool raycast)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = UiArt.SoftPanel();
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        static Image FramedButton(Transform parent, string name, Color fill, Color border)
        {
            var outer = Panel(parent, name, border, true);
            var inner = Panel(outer.transform, "Inner", fill, false);
            var irt = inner.rectTransform;
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(5f, 5f);
            irt.offsetMax = new Vector2(-5f, -5f);
            return outer;
        }

        static Text Label(Transform parent, string name, string content, int size, Vector2 anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900f, 120f);
            rt.anchoredPosition = Vector2.zero;
            var text = go.GetComponent<Text>();
            UiArt.StyleLabel(text, size, color);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static void AddTap(GameObject go, System.Action onPress)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ =>
            {
                go.transform.localScale = Vector3.one * 0.94f;
                onPress?.Invoke();
            });
            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => go.transform.localScale = Vector3.one);
            trigger.triggers.Add(down);
            trigger.triggers.Add(up);
        }
    }
}
