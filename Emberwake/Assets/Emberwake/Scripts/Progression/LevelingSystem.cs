using System;
using UnityEngine;

namespace Emberwake
{
    /// <summary>XP / level layer on top of Wick Rank essence.</summary>
    public class LevelingSystem : MonoBehaviour
    {
        public static LevelingSystem Instance { get; private set; }

        [SerializeField] int level = 1;
        [SerializeField] int xp;
        [SerializeField] int maxLevel = 30;

        PlayerStats stats;

        public int Level => level;
        public int Xp => xp;
        public int XpToNext => 40 + level * 25;
        public float XpNormalized => XpToNext > 0 ? Mathf.Clamp01(xp / (float)XpToNext) : 1f;

        public event Action<int> OnLevelUp;
        public event Action OnChanged;

        void Awake()
        {
            Instance = this;
            stats = GetComponent<PlayerStats>();
        }

        public void Restore(int newLevel, int newXp)
        {
            level = Mathf.Clamp(newLevel, 1, maxLevel);
            xp = Mathf.Max(0, newXp);
            OnChanged?.Invoke();
        }

        public void AddXp(int amount)
        {
            if (amount <= 0 || level >= maxLevel) return;
            xp += amount;
            OnChanged?.Invoke();
            while (level < maxLevel && xp >= XpToNext)
            {
                xp -= XpToNext;
                level++;
                ApplyLevelRewards();
                OnLevelUp?.Invoke(level);
                AudioDirector.Instance?.PlayLevelUp();
                PortraitMobileHud.Instance?.ShowToast($"Level {level}!");
                OnChanged?.Invoke();
            }
        }

        void ApplyLevelRewards()
        {
            if (stats == null) stats = GetComponent<PlayerStats>() ?? GameManager.Instance?.Stats;
            if (stats == null) return;
            stats.AddSwordPower(0.08f);
            stats.AddMaxStamina(4f);
            if (level % 3 == 0)
                stats.AddHeartContainer();
        }
    }
}
