using System;
using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Hearts / stamina / defense. Soft RPG stats for 20–30h progression.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Hearts")]
        [SerializeField] int hearts = 3;
        [SerializeField] int maxHearts = 3;
        [SerializeField] int heartContainerCap = 20;

        [Header("Stamina")]
        [SerializeField] float stamina = 50f;
        [SerializeField] float maxStamina = 50f;
        [SerializeField] float staminaRegenPerSecond = 18f;

        [Header("Combat")]
        [SerializeField] float swordPower = 1f;
        [SerializeField] float bowPower = 1f;
        [SerializeField] float defensePercent;
        [SerializeField] float critChance;
        [SerializeField] float lanternRadius = 3f;

        public int Hearts => hearts;
        public int MaxHearts => maxHearts;
        public float Stamina => stamina;
        public float MaxStamina => maxStamina;
        public float SwordPower => swordPower;
        public float BowPower => bowPower;
        public float DefensePercent => defensePercent;
        public float CritChance => critChance;
        public float LanternRadius => lanternRadius;
        public bool IsAlive => hearts > 0;
        public bool IsFullHealth => hearts >= maxHearts;

        public event Action OnChanged;
        public event Action OnDeath;

        public void HealHearts(int amount)
        {
            hearts = Mathf.Min(maxHearts, hearts + Mathf.Max(0, amount));
            OnChanged?.Invoke();
        }

        public void DamageHearts(float rawHearts)
        {
            float mitigated = rawHearts * (1f - Mathf.Clamp01(defensePercent));
            int loss = Mathf.Max(1, Mathf.CeilToInt(mitigated));
            hearts = Mathf.Max(0, hearts - loss);
            OnChanged?.Invoke();
            if (hearts <= 0) OnDeath?.Invoke();
        }

        public bool TrySpendStamina(float cost)
        {
            if (stamina < cost) return false;
            stamina -= cost;
            OnChanged?.Invoke();
            return true;
        }

        public void RegenStamina(float dt)
        {
            if (stamina >= maxStamina) return;
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenPerSecond * dt);
            OnChanged?.Invoke();
        }

        public bool AddHeartContainer()
        {
            if (maxHearts >= heartContainerCap) return false;
            maxHearts++;
            hearts = maxHearts;
            OnChanged?.Invoke();
            return true;
        }

        public void AddMaxStamina(float amount)
        {
            maxStamina += amount;
            stamina = maxStamina;
            OnChanged?.Invoke();
        }

        public void Restore(int hp, int maxHp, float maxSta, float sword)
        {
            maxHearts = Mathf.Clamp(maxHp, 1, heartContainerCap);
            hearts = Mathf.Clamp(hp, 1, maxHearts);
            maxStamina = Mathf.Max(10f, maxSta);
            stamina = maxStamina;
            swordPower = Mathf.Max(0.2f, sword);
            OnChanged?.Invoke();
        }

        public void AddSwordPower(float amount) { swordPower += amount; OnChanged?.Invoke(); }
        public void AddBowPower(float amount) { bowPower += amount; OnChanged?.Invoke(); }
        public void AddDefense(float amount) { defensePercent = Mathf.Min(0.4f, defensePercent + amount); OnChanged?.Invoke(); }
        public void AddCrit(float amount) { critChance = Mathf.Min(0.15f, critChance + amount); OnChanged?.Invoke(); }
        public void SetLanternRadius(float radius) { lanternRadius = radius; OnChanged?.Invoke(); }

        public void ApplyWeaponTier(int swordTier, int bowTier, int shieldTier, int lanternTier)
        {
            swordPower = 1f + swordTier * 0.5f;
            bowPower = bowTier <= 0 ? 0f : 1f + (bowTier - 1) * 0.4f;
            defensePercent = Mathf.Min(0.4f, shieldTier * 0.1f);
            lanternRadius = 3f + lanternTier * 2f;
            OnChanged?.Invoke();
        }

        void Update()
        {
            RegenStamina(Time.deltaTime);
        }
    }
}
