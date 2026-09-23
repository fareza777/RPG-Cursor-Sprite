using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>In-game hub: Character / Leveling / Quests / Bestiary / Save.</summary>
    public class GameMenuHub : MonoBehaviour
    {
        public static GameMenuHub Instance { get; private set; }

        GameObject root;
        Text titleLabel;
        Text bodyLabel;
        Image iconImage;
        string page = "home";
        bool open;

        public bool IsOpen => open;

        public void Build(Transform canvas)
        {
            Instance = this;
            root = new GameObject("GameMenuHub", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var dim = MakeImage(root.transform, "Dim", UiArt.WhiteQuad(),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.72f), true);
            Stretch(dim.rectTransform);

            // Ember-gold frame behind the panel (consistent UI language).
            var frame = MakeImage(root.transform, "Frame", UiArt.SoftPanel(),
                new Vector2(0.055f, 0.11f), new Vector2(0.945f, 0.91f), Vector2.zero, Vector2.zero,
                new Color(0.82f, 0.53f, 0.2f, 0.9f), false);
            frame.type = Image.Type.Sliced;

            var panel = MakeImage(root.transform, "Panel", UiArt.FramePanel(),
                new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.9f), Vector2.zero, Vector2.zero,
                new Color(0.09f, 0.07f, 0.11f, 0.99f), true);
            panel.type = Image.Type.Sliced;

            iconImage = MakeImage(panel.transform, "Icon", GeneratedArt.IconWick(),
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(96f, 96f),
                Color.white, false);

            titleLabel = MakeText(panel.transform, "Title", "MENU", 44,
                new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(800f, 60f),
                new Color(1f, 0.85f, 0.35f));
            titleLabel.fontStyle = FontStyle.Bold;
            titleLabel.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.1f, 0f, 0.9f);

            bodyLabel = MakeText(panel.transform, "Body", "", 26,
                new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(820f, 760f),
                new Color(0.92f, 0.9f, 0.84f));
            bodyLabel.alignment = TextAnchor.UpperLeft;

            float y = 0.08f;
            MakeNav(panel.transform, "Karakter", GeneratedArt.IconSword(), () => ShowPage("character"), new Vector2(0.18f, y));
            MakeNav(panel.transform, "Level", GeneratedArt.IconStar(), () => ShowPage("level"), new Vector2(0.38f, y));
            MakeNav(panel.transform, "Quest", GeneratedArt.IconQuest(), () => ShowPage("quests"), new Vector2(0.58f, y));
            MakeNav(panel.transform, "Bestiar", GeneratedArt.IconBook(), () => ShowPage("bestiary"), new Vector2(0.78f, y));

            var saveBtn = MakeButton(panel.transform, "SaveBtn", "SAVE", GeneratedArt.SoftButton(),
                new Vector2(0.28f, 0.02f), new Vector2(220f, 70f), () => ShowPage("save"));
            var closeBtn = MakeButton(panel.transform, "CloseBtn", "TUTUP", GeneratedArt.SoftButtonDark(),
                new Vector2(0.72f, 0.02f), new Vector2(220f, 70f), Close);

            root.SetActive(false);
            open = false;
        }

        void MakeNav(Transform parent, string label, Sprite icon, System.Action act, Vector2 anchor)
        {
            var img = MakeImage(parent, label, GeneratedArt.IconFrame(),
                anchor, anchor, Vector2.zero, new Vector2(110f, 110f), Color.white, true);
            var ic = MakeImage(img.transform, "I", icon,
                new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(56f, 56f), Color.white, false);
            MakeText(img.transform, "L", label, 18, new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(100f, 28f),
                new Color(0.95f, 0.9f, 0.75f));
            AddTap(img.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                act?.Invoke();
            });
        }

        Image MakeButton(Transform parent, string name, string text, Sprite spr, Vector2 anchor, Vector2 size, System.Action act)
        {
            var img = MakeImage(parent, name, spr, anchor, anchor, Vector2.zero, size, Color.white, true);
            img.type = Image.Type.Sliced;
            MakeText(img.transform, "T", text, 28, new Vector2(0.5f, 0.5f), Vector2.zero, size,
                Color.white).fontStyle = FontStyle.Bold;
            AddTap(img.gameObject, () =>
            {
                AudioDirector.Instance?.PlayUi();
                act?.Invoke();
            });
            return img;
        }

        public void Toggle()
        {
            if (open) Close();
            else Open();
        }

        public void Open()
        {
            if (root == null) return;
            open = true;
            root.SetActive(true);
            VerticalSliceDirector.Instance?.SetPlayerFrozenPublic(true);
            PortraitMobileHud.Instance?.SetControlsVisible(false);
            AudioDirector.Instance?.PlayMenuOpen();
            ShowPage("home");
        }

        public void Close()
        {
            if (root == null) return;
            open = false;
            root.SetActive(false);
            VerticalSliceDirector.Instance?.SetPlayerFrozenPublic(false);
            PortraitMobileHud.Instance?.SetControlsVisible(true);
        }

        void ShowPage(string id)
        {
            page = id;
            switch (id)
            {
                case "character":
                    titleLabel.text = "KARAKTER";
                    iconImage.sprite = GeneratedArt.PortraitKael();
                    bodyLabel.text = CharacterText();
                    break;
                case "level":
                    titleLabel.text = "LEVELING";
                    iconImage.sprite = GeneratedArt.IconStar();
                    bodyLabel.text = LevelText();
                    break;
                case "quests":
                    titleLabel.text = "QUEST LOG";
                    iconImage.sprite = GeneratedArt.IconQuest();
                    bodyLabel.text = QuestText();
                    break;
                case "bestiary":
                    titleLabel.text = "BESTIARY";
                    iconImage.sprite = GeneratedArt.IconBook();
                    bodyLabel.text = BestiaryText();
                    break;
                case "save":
                    titleLabel.text = "SAVE / LOAD";
                    iconImage.sprite = GeneratedArt.IconSave();
                    bodyLabel.text = "Ketuk SAVE untuk menyimpan ruangan, HP, level, quest, dan bestiary.\n\nLanjutkan dari menu utama memuat slot ini.";
                    DoSave();
                    break;
                default:
                    titleLabel.text = "EMBERWAKE";
                    iconImage.sprite = GeneratedArt.IconWick();
                    bodyLabel.text = HomeText();
                    break;
            }
        }

        void DoSave()
        {
            var save = FindFirstObjectByType<SaveSystem>();
            var player = FindFirstObjectByType<PlayerController>();
            save?.SaveFromManagers(player != null ? player.transform : null);
            PortraitMobileHud.Instance?.ShowToast("Game tersimpan");
        }

        static string HomeText()
        {
            var s = GameManager.Instance?.Stats;
            var w = GameManager.Instance?.WickRank;
            var lv = LevelingSystem.Instance;
            string status = s != null
                ? $"Kael — Penjaga Wick\nHP {s.Hearts}/{s.MaxHearts}   ·   Lv {(lv != null ? lv.Level : 1)}   ·   Wick {(w != null ? w.Rank : 1)}\n\n"
                : "";
            return status +
                   "Pilih tab di bawah:\n\n" +
                   "• Karakter — status & kisah Kael\n" +
                   "• Level — XP & Wick rank\n" +
                   "• Quest — misi utama & sampingan\n" +
                   "• Bestiar — musuh yang terungkap\n" +
                   "• Save — simpan perjalanan\n\n" +
                   "Wick menunggu. Jangan biarkan padam.";
        }

        static string CharacterText()
        {
            var s = GameManager.Instance?.Stats;
            var w = GameManager.Instance?.WickRank;
            if (s == null) return "—";
            return $"Kael — Penjaga Wick yang lupa\n\n" +
                   $"HP          {s.Hearts}/{s.MaxHearts}\n" +
                   $"Stamina     {s.Stamina:0}/{s.MaxStamina:0}\n" +
                   $"Sword       {s.SwordPower:0.00}\n" +
                   $"Defense     {s.DefensePercent * 100f:0}%\n" +
                   $"Wick Rank   {w?.Rank ?? 1}\n" +
                   $"Essence     {w?.Essence ?? 0}\n\n" +
                   "\"Aku tidak ingat tujuh tahun itu.\nHanya nyala Wick yang mengenaliku.\"";
        }

        static string LevelText()
        {
            var lv = LevelingSystem.Instance;
            var w = GameManager.Instance?.WickRank;
            if (lv == null) return "Leveling belum siap.";
            int bar = Mathf.RoundToInt(lv.XpNormalized * 12f);
            string meter = new string('█', bar) + new string('░', 12 - bar);
            return $"Level {lv.Level}\nXP  {lv.Xp}/{lv.XpToNext}\n[{meter}]\n\n" +
                   $"Wick Rank {w?.Rank ?? 1} · Essence ke next: {w?.EssenceToNext ?? 0}\n\n" +
                   "Setiap 3 level: +1 Heart Container\nSetiap level: +Sword & Stamina\n\nBunuh musuh, selesaikan quest,\nnyalakan Wick — naik level.";
        }

        static string QuestText()
        {
            var q = QuestSystem.Instance;
            if (q == null) return "—";
            var sb = new StringBuilder();
            sb.AppendLine("══ MAIN ══");
            foreach (var (def, st) in q.GetActiveMain())
                sb.AppendLine($"• {def.title}  ({st.progress}/{def.targetCount})\n  {def.description}\n");
            foreach (var def in q.AllDefs)
                if (def.isMain && q.IsComplete(def.id))
                    sb.AppendLine($"✓ {def.title}  (selesai)\n");
            sb.AppendLine("══ SIDE ══");
            foreach (var (def, st) in q.GetActiveSides())
            {
                string mark = st.completed ? "✓" : "•";
                sb.AppendLine($"{mark} {def.title}  ({st.progress}/{def.targetCount})");
            }
            return sb.ToString();
        }

        static string BestiaryText()
        {
            var b = BestiarySystem.Instance;
            if (b == null) return "—";
            var sb = new StringBuilder();
            foreach (var e in b.AllEntries)
            {
                if (e.unlocked)
                    sb.AppendLine($"• {e.name}  ★{e.danger}  HP{e.maxHp}\n  {e.lore}\n");
                else
                    sb.AppendLine($"• ???  (belum terungkap)\n");
            }
            return sb.ToString();
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Image MakeImage(Transform parent, string name, Sprite spr, Vector2 amin, Vector2 amax,
            Vector2 pos, Vector2 size, Color color, bool raycast)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = amin;
            rt.anchorMax = amax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            if ((amax - amin).sqrMagnitude > 0.9f) { rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
            var img = go.GetComponent<Image>();
            img.sprite = spr;
            img.color = color;
            img.raycastTarget = raycast;
            img.preserveAspect = true;
            return img;
        }

        static Text MakeText(Transform parent, string name, string content, int size, Vector2 anchor,
            Vector2 pos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var text = go.GetComponent<Text>();
            UiArt.StyleLabel(text, size, color);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static void AddTap(GameObject go, System.Action onPress)
        {
            var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ =>
            {
                go.transform.localScale = Vector3.one * 0.92f;
                onPress?.Invoke();
            });
            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => go.transform.localScale = Vector3.one);
            trigger.triggers.Add(down);
            trigger.triggers.Add(up);
        }
    }
}
