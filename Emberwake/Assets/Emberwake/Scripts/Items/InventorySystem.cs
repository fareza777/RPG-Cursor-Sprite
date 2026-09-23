using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Key items + simple consumable stacks for Kael's inventory.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] List<KeyItemId> keyItems = new();
        [SerializeField] int heartDrops;
        [SerializeField] int wickOil;
        [SerializeField] int ashSalt;
        [SerializeField] int gold;

        [SerializeField] int swordTier;
        [SerializeField] int bowTier;
        [SerializeField] int shieldTier;
        [SerializeField] int lanternTier = 1;

        public int Gold => gold;
        public int SwordTier => swordTier;
        public int BowTier => bowTier;
        public int ShieldTier => shieldTier;
        public int LanternTier => lanternTier;

        public event Action OnChanged;

        public bool HasKeyItem(KeyItemId id) => id != KeyItemId.None && keyItems.Contains(id);

        public void GrantKeyItem(KeyItemId id)
        {
            if (id == KeyItemId.None || keyItems.Contains(id)) return;
            keyItems.Add(id);

            switch (id)
            {
                case KeyItemId.GlovesOfLift:
                    break;
                case KeyItemId.TideBow:
                    bowTier = Mathf.Max(bowTier, 1);
                    break;
                case KeyItemId.MirageShield:
                    shieldTier = Mathf.Max(shieldTier, 1);
                    break;
                case KeyItemId.Ashbrand:
                    swordTier = Mathf.Max(swordTier, 1);
                    break;
                case KeyItemId.EchoLantern:
                    lanternTier = Mathf.Max(lanternTier, 2);
                    break;
                case KeyItemId.KingsSigil:
                    break;
                case KeyItemId.Heartwick:
                    lanternTier = Mathf.Max(lanternTier, 3);
                    break;
            }

            SyncStats();
            OnChanged?.Invoke();
        }

        public void SetGold(int amount)
        {
            gold = Mathf.Max(0, amount);
            OnChanged?.Invoke();
        }

        public void AddGold(int amount)
        {
            gold = Mathf.Max(0, gold + amount);
            OnChanged?.Invoke();
        }

        public bool TrySpendGold(int amount)
        {
            if (gold < amount) return false;
            gold -= amount;
            OnChanged?.Invoke();
            return true;
        }

        public void AddConsumable(string id, int amount = 1)
        {
            switch (id)
            {
                case "heart_drop": heartDrops = Mathf.Min(20, heartDrops + amount); break;
                case "wick_oil": wickOil = Mathf.Min(20, wickOil + amount); break;
                case "ash_salt": ashSalt = Mathf.Min(20, ashSalt + amount); break;
            }
            OnChanged?.Invoke();
        }

        public bool TryUseHeartDrop(PlayerStats stats)
        {
            if (heartDrops <= 0 || stats == null || stats.IsFullHealth) return false;
            heartDrops--;
            stats.HealHearts(1);
            OnChanged?.Invoke();
            return true;
        }

        void SyncStats()
        {
            var stats = GameManager.Instance != null ? GameManager.Instance.Stats : GetComponent<PlayerStats>();
            stats?.ApplyWeaponTier(swordTier, bowTier, shieldTier, lanternTier);
        }
    }
}
