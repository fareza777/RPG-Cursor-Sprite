using System;
using UnityEngine;

namespace Emberwake
{
    [Serializable]
    public class SaveData
    {
        public int hearts = 3;
        public int maxHearts = 3;
        public float maxStamina = 50f;
        public int wickRank = 1;
        public int essence;
        public int gold;
        public int memoryShards;
        public int region;
        public int room;
        public int level = 1;
        public int xp;
        public float swordPower = 1f;
        public int gloves;
        public int boss;
        public float posX;
        public float posY;
        public string quests = "";
        public string bestiary = "";
        public string[] keyItems = Array.Empty<string>();
        public string[] flags = Array.Empty<string>();
    }

    public class SaveSystem : MonoBehaviour
    {
        const string SlotKey = "emberwake_save_0";

        public static bool ContinueRequested { get; private set; }

        public static void RequestContinue() => ContinueRequested = true;
        public static void ClearContinue() => ContinueRequested = false;

        public bool ConsumeContinue(out SaveData data)
        {
            data = null;
            if (!ContinueRequested) return false;
            ContinueRequested = false;
            return TryLoad(out data);
        }

        public void SaveFromManagers(Transform player)
        {
            if (GameManager.Instance == null) return;
            var gm = GameManager.Instance;
            var dir = VerticalSliceDirector.Instance;
            var data = new SaveData
            {
                hearts = gm.Stats.Hearts,
                maxHearts = gm.Stats.MaxHearts,
                maxStamina = gm.Stats.MaxStamina,
                swordPower = gm.Stats.SwordPower,
                wickRank = gm.WickRank.Rank,
                essence = gm.WickRank.Essence,
                gold = gm.Inventory.Gold,
                memoryShards = gm.Story.MemoryShardCount,
                region = (int)gm.Story.CurrentRegion,
                room = dir != null ? dir.RoomIndex : 0,
                level = LevelingSystem.Instance != null ? LevelingSystem.Instance.Level : 1,
                xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.Xp : 0,
                gloves = dir != null && dir.GlovesTaken ? 1 : 0,
                boss = dir != null && dir.BossDead ? 1 : 0,
                quests = QuestSystem.Instance != null ? QuestSystem.Instance.Export() : "",
                bestiary = BestiarySystem.Instance != null ? BestiarySystem.Instance.ExportUnlocked() : "",
                posX = player != null ? player.position.x : 0f,
                posY = player != null ? player.position.y : 0f
            };
            PlayerPrefs.SetString(SlotKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            Debug.Log("[Emberwake] Game saved.");
        }

        public bool TryLoad(out SaveData data)
        {
            data = null;
            if (!PlayerPrefs.HasKey(SlotKey)) return false;
            data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SlotKey));
            return data != null;
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SlotKey);
        }
    }
}
