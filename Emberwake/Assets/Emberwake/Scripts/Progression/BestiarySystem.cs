using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    [Serializable]
    public class BestiaryEntry
    {
        public string id;
        public string name;
        public string lore;
        public float maxHp;
        public int danger = 1;
        public string spriteKey;
        public bool unlocked;
    }

    public class BestiarySystem : MonoBehaviour
    {
        public static BestiarySystem Instance { get; private set; }

        readonly List<BestiaryEntry> entries = new();
        public event Action OnChanged;

        void Awake()
        {
            Instance = this;
            RegisterDefaults();
        }

        void RegisterDefaults()
        {
            entries.Add(new BestiaryEntry
            {
                id = "slime", name = "Slime Ashdeep", maxHp = 3, danger = 1, spriteKey = "slime",
                lore = "Gumpalan memori basah. Menyimpan nama yang hampir dilupakan desa."
            });
            entries.Add(new BestiaryEntry
            {
                id = "ash_wisp", name = "Ash Wisp", maxHp = 2, danger = 2, spriteKey = "ash_wisp",
                lore = "Bara Wick yang lepas. Mengincar lentera hidup."
            });
            entries.Add(new BestiaryEntry
            {
                id = "root_crawler", name = "Root Crawler", maxHp = 5, danger = 2, spriteKey = "root_crawler",
                lore = "Akar berlari. Menjaga Pressure Chamber dari tangan asing."
            });
            entries.Add(new BestiaryEntry
            {
                id = "ember_moth", name = "Ember Moth", maxHp = 2, danger = 1, spriteKey = "ember_moth",
                lore = "Ngengat dari abu altar. Lemah, tapi datang bergerombol."
            });
            entries.Add(new BestiaryEntry
            {
                id = "hollow_knight", name = "Hollow Knight", maxHp = 8, danger = 4, spriteKey = "hollow_knight",
                lore = "Baju zirah kosong yang masih mengingat sumpah penjaga Wick."
            });
            entries.Add(new BestiaryEntry
            {
                id = "barkling", name = "Barkling", maxHp = 12, danger = 5, spriteKey = "barkling",
                lore = "Penjaga Wick Hollowroot yang membusuk. Kayu dan dendam."
            });
        }

        public void Unlock(string id)
        {
            var e = Find(id);
            if (e == null || e.unlocked) return;
            e.unlocked = true;
            OnChanged?.Invoke();
            AudioDirector.Instance?.PlayUi();
            PortraitMobileHud.Instance?.ShowToast($"Bestiary: {e.name}");
        }

        public bool IsUnlocked(string id)
        {
            var e = Find(id);
            return e != null && e.unlocked;
        }

        public BestiaryEntry Find(string id)
        {
            foreach (var e in entries)
                if (e.id == id) return e;
            return null;
        }

        public IReadOnlyList<BestiaryEntry> AllEntries => entries;

        public string ExportUnlocked()
        {
            var ids = new List<string>();
            foreach (var e in entries)
                if (e.unlocked) ids.Add(e.id);
            return string.Join("|", ids);
        }

        public void ImportUnlocked(string blob)
        {
            if (string.IsNullOrEmpty(blob)) return;
            foreach (var id in blob.Split('|'))
            {
                var e = Find(id);
                if (e != null) e.unlocked = true;
            }
            OnChanged?.Invoke();
        }
    }
}
