using System;
using UnityEngine;

namespace Emberwake
{
    /// <summary>Generic HP for player or enemies (hearts or float HP).</summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] float maxHp = 3f;
        [SerializeField] float hp = 3f;
        [SerializeField] float knockbackForce = 3.2f;
        [SerializeField] float iFrameSeconds = 0.42f;
        [SerializeField] bool usePlayerHearts;

        PlayerStats playerStats;
        float iFrameLeft;
        Rigidbody2D rb;

        public float Hp => usePlayerHearts && playerStats != null ? playerStats.Hearts : hp;
        public float MaxHp => usePlayerHearts && playerStats != null ? playerStats.MaxHearts : maxHp;
        public bool IsDead => Hp <= 0f;

        public event Action OnDamaged;
        public event Action OnDied;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            // Do NOT auto-bind GameManager.Stats here — enemies would steal player hearts.
            // Player calls BindPlayerStats explicitly.
            playerStats = GetComponent<PlayerStats>();
            if (playerStats != null) usePlayerHearts = true;
            if (!usePlayerHearts) hp = maxHp;
        }

        void HandleDeath()
        {
            OnDied?.Invoke();
        }

        public void BindPlayerStats(PlayerStats stats)
        {
            if (playerStats != null) playerStats.OnDeath -= HandleDeath;
            playerStats = stats;
            usePlayerHearts = stats != null;
            iFrameSeconds = 0.4f;
            knockbackForce = 2.6f;
            if (playerStats != null) playerStats.OnDeath += HandleDeath;
        }

        public void ConfigureEnemy(float max)
        {
            if (usePlayerHearts && playerStats != null)
                playerStats.OnDeath -= HandleDeath;
            usePlayerHearts = false;
            playerStats = null;
            maxHp = max;
            hp = max;
            // Short i-frames so multi-tick swings and repeated hits register.
            iFrameSeconds = 0.16f;
            knockbackForce = 2.2f;
        }

        void Update()
        {
            if (iFrameLeft > 0f) iFrameLeft -= Time.deltaTime;
        }

        public void TakeDamage(float amount, Vector2 from)
        {
            if (IsDead || iFrameLeft > 0f) return;
            iFrameLeft = iFrameSeconds;

            if (usePlayerHearts && playerStats != null)
                playerStats.DamageHearts(Mathf.Max(1f, amount));
            else
            {
                hp = Mathf.Max(0f, hp - amount);
                if (hp <= 0f) OnDied?.Invoke();
            }

            DamagePopup.Spawn(transform.position, usePlayerHearts ? Mathf.Max(1f, amount) : amount, amount >= 1.4f);
            CombatVfx.Spark(transform.position,
                usePlayerHearts ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.92f, 0.55f),
                amount >= 1.4f ? 1.35f : 1f);
            FeelFeedback.HitStop(usePlayerHearts ? 0.06f : 0.035f);

            if (rb != null && rb.simulated)
            {
                Vector2 dir = ((Vector2)transform.position - from).normalized;
                if (dir.sqrMagnitude < 0.01f) dir = Vector2.up;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }

            OnDamaged?.Invoke();
            AudioDirector.Instance?.PlayHit();
            if (usePlayerHearts)
                FeelFeedback.Shake(0.22f, 0.18f);
            else
                FeelFeedback.Shake(0.1f, 0.08f);

            if (!usePlayerHearts && hp <= 0f)
            {
                if (GameManager.Instance != null && GameManager.Instance.WickRank != null)
                    GameManager.Instance.WickRank.AddEssence(8);
                Destroy(gameObject, 0.05f);
            }
        }
    }
}
