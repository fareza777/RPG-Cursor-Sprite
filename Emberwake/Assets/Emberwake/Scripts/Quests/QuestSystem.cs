using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    [Serializable]
    public class QuestDef
    {
        public string id;
        public string title;
        public string description;
        public bool isMain;
        public int targetCount = 1;
        public int rewardEssence;
        public string rewardMessage;
    }

    [Serializable]
    public class QuestState
    {
        public string id;
        public int progress;
        public bool completed;
        public bool discovered;
    }

    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        readonly Dictionary<string, QuestDef> defs = new();
        readonly Dictionary<string, QuestState> states = new();

        public event Action OnChanged;

        void Awake()
        {
            Instance = this;
            RegisterDefaults();
        }

        public void RegisterDefaults()
        {
            Add(new QuestDef
            {
                id = "main_wick", title = "Wick Hollowroot",
                description = "Masuki Hollowroot dan nyalakan Wick hutan sebelum padam.",
                isMain = true, targetCount = 1, rewardEssence = 40,
                rewardMessage = "Wick berdenyut lagi — Millbrook ingat namanya."
            });
            Add(new QuestDef
            {
                id = "main_gloves", title = "Gloves of Lift",
                description = "Temukan Gloves of Lift di Reliquary untuk membuka jalan Barkling.",
                isMain = true, targetCount = 1, rewardEssence = 25,
                rewardMessage = "Sarung tangan kuno menempel di tangan Kael."
            });
            Add(new QuestDef
            {
                id = "main_barkling", title = "Bayangan Barkling",
                description = "Kalahkan Barkling, penjaga Wick yang membusuk.",
                isMain = true, targetCount = 1, rewardEssence = 80,
                rewardMessage = "Barkling tumbang. Jalan ke altar terbuka."
            });
            Add(new QuestDef
            {
                id = "side_slime", title = "Sapu Ashdeep",
                description = "Bunuh 5 Slime Ashdeep di jalur Hollowroot.",
                isMain = false, targetCount = 5, rewardEssence = 20,
                rewardMessage = "Jalur terasa lebih aman."
            });
            Add(new QuestDef
            {
                id = "side_explore", title = "Pemeta Hollowroot",
                description = "Jelajahi 4 ruang berbeda di Hollowroot.",
                isMain = false, targetCount = 4, rewardEssence = 15,
                rewardMessage = "Peta Wick di kepala Kael semakin jelas."
            });
            Add(new QuestDef
            {
                id = "side_essence", title = "Ember Essence",
                description = "Kumpulkan total 40 Essence.",
                isMain = false, targetCount = 40, rewardEssence = 10,
                rewardMessage = "Wick Rank bergetar lebih kuat."
            });

            Discover("main_wick");
            Discover("side_slime");
            Discover("side_explore");
            Discover("side_essence");
        }

        void Add(QuestDef def)
        {
            defs[def.id] = def;
            if (!states.ContainsKey(def.id))
                states[def.id] = new QuestState { id = def.id };
        }

        public void Discover(string id)
        {
            if (!states.TryGetValue(id, out var st)) return;
            if (st.discovered) return;
            st.discovered = true;
            OnChanged?.Invoke();
            AudioDirector.Instance?.PlayQuest();
        }

        public void AddProgress(string id, int n = 1)
        {
            if (!defs.TryGetValue(id, out var def) || !states.TryGetValue(id, out var st)) return;
            if (st.completed) return;
            if (!st.discovered) st.discovered = true;
            st.progress = Mathf.Min(def.targetCount, st.progress + n);
            if (st.progress >= def.targetCount)
                Complete(id);
            else
                OnChanged?.Invoke();
        }

        public void Complete(string id)
        {
            if (!defs.TryGetValue(id, out var def) || !states.TryGetValue(id, out var st)) return;
            if (st.completed) return;
            st.completed = true;
            st.progress = def.targetCount;
            GameManager.Instance?.WickRank?.AddEssence(def.rewardEssence);
            LevelingSystem.Instance?.AddXp(def.rewardEssence);
            AudioDirector.Instance?.PlayQuest();
            PortraitMobileHud.Instance?.ShowToast(def.rewardMessage);
            OnChanged?.Invoke();
        }

        public bool IsComplete(string id) => states.TryGetValue(id, out var st) && st.completed;

        public QuestDef GetDef(string id) => defs.TryGetValue(id, out var d) ? d : null;
        public QuestState GetState(string id) => states.TryGetValue(id, out var s) ? s : null;

        public List<(QuestDef def, QuestState st)> GetActiveMain()
        {
            var list = new List<(QuestDef, QuestState)>();
            foreach (var kv in defs)
                if (kv.Value.isMain && states[kv.Key].discovered && !states[kv.Key].completed)
                    list.Add((kv.Value, states[kv.Key]));
            return list;
        }

        public List<(QuestDef def, QuestState st)> GetActiveSides()
        {
            var list = new List<(QuestDef, QuestState)>();
            foreach (var kv in defs)
                if (!kv.Value.isMain && states[kv.Key].discovered)
                    list.Add((kv.Value, states[kv.Key]));
            return list;
        }

        public IEnumerable<QuestDef> AllDefs => defs.Values;

        public string Export()
        {
            var parts = new List<string>();
            foreach (var kv in states)
            {
                var st = kv.Value;
                parts.Add($"{st.id}:{st.progress}:{(st.completed ? 1 : 0)}:{(st.discovered ? 1 : 0)}");
            }
            return string.Join("|", parts);
        }

        public void Import(string blob)
        {
            if (string.IsNullOrEmpty(blob)) return;
            foreach (var part in blob.Split('|'))
            {
                var f = part.Split(':');
                if (f.Length < 4 || !states.TryGetValue(f[0], out var st)) continue;
                int.TryParse(f[1], out st.progress);
                st.completed = f[2] == "1";
                st.discovered = f[3] == "1";
            }
            OnChanged?.Invoke();
        }
    }
}
