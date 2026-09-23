using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Wick Rank soft-leveling: essence from combat/puzzles/quests → rank ups.
    /// </summary>
    public class WickRankSystem : MonoBehaviour
    {
        public enum RankBonus
        {
            Stamina,
            Sword,
            Bow,
            Crit,
            HeartFragment
        }

        [SerializeField] int essence;
        [SerializeField] int essencePerRank = 100;
        [SerializeField] int rank = 1;
        [SerializeField] int maxRank = 30;
        [SerializeField] int heartFragments;
        [SerializeField] PlayerStats stats;

        public int Essence => essence;
        public int Rank => rank;
        public int EssenceToNext => Mathf.Max(0, essencePerRank - (essence % essencePerRank));

        public event Action<int> OnRankUp;
        public event Action OnChanged;

        void Awake()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
        }

        public void Restore(int newRank, int newEssence)
        {
            rank = Mathf.Clamp(newRank, 1, maxRank);
            essence = Mathf.Max(0, newEssence);
            OnChanged?.Invoke();
        }

        public void AddEssence(int amount)
        {
            if (amount <= 0 || rank >= maxRank) return;
            essence += amount;
            OnChanged?.Invoke();
            QuestSystem.Instance?.AddProgress("side_essence", amount);

            while (rank < maxRank && essence >= rank * essencePerRank)
            {
                rank++;
                OnRankUp?.Invoke(rank);
                OnChanged?.Invoke();
            }
        }

        /// <summary>Call from UI when player picks a bonus after rank-up.</summary>
        public void ApplyRankBonus(RankBonus bonus)
        {
            if (stats == null) return;
            switch (bonus)
            {
                case RankBonus.Stamina:
                    stats.AddMaxStamina(2f);
                    break;
                case RankBonus.Sword:
                    stats.AddSwordPower(0.03f);
                    break;
                case RankBonus.Bow:
                    stats.AddBowPower(0.03f);
                    break;
                case RankBonus.Crit:
                    stats.AddCrit(0.01f);
                    break;
                case RankBonus.HeartFragment:
                    heartFragments++;
                    if (heartFragments >= 4)
                    {
                        heartFragments = 0;
                        stats.AddHeartContainer();
                    }
                    break;
            }
        }
    }
}
