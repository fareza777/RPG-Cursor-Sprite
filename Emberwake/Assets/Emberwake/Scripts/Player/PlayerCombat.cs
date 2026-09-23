using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Emberwake
{
    /// <summary>Sword + spin — snappy multi-tick hits, short lunge, crisp FX.</summary>
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] PlayerController controller;
        [SerializeField] PlayerStats stats;
        [SerializeField] float attackCooldown = 0.32f;
        [SerializeField] float attackStaminaCost = 2.5f;
        [SerializeField] float spinStaminaCost = 9f;
        [SerializeField] float attackRange = 1.95f;
        [SerializeField] float attackRadius = 0.95f;
        [SerializeField] float baseDamage = 1f;
        [SerializeField] float attackAnimSeconds = 0.22f;
        [SerializeField] float lungeSpeed = 7.4f;

        float cooldownLeft;
        Rigidbody2D rb;
        Vector2 lungeDir;
        float lungeLeft;
        public bool IsAttacking { get; private set; }
        public Vector2 AttackLunge => lungeLeft > 0f ? lungeDir * lungeSpeed : Vector2.zero;

        void Awake()
        {
            if (controller == null) controller = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
            BindStats();
        }

        void Start() => BindStats();

        void BindStats()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (stats == null && GameManager.Instance != null)
                stats = GameManager.Instance.Stats;
        }

        void Update()
        {
            BindStats();
            cooldownLeft -= Time.deltaTime;
            if (lungeLeft > 0f) lungeLeft -= Time.deltaTime;
            if (cooldownLeft > 0f) return;

            if (GameInput.ConsumeAttack())
                TrySwordAttack();
            else if (GameInput.ConsumeSpin())
                TrySpinAttack();
        }

        public void TrySwordAttack()
        {
            if (cooldownLeft > 0f) return;
            stats?.TrySpendStamina(attackStaminaCost);

            cooldownLeft = attackCooldown;
            Vector2 facing = controller != null ? controller.FacingVector() : Vector2.down;
            lungeDir = facing;
            lungeLeft = 0.16f;
            StartCoroutine(AttackWindow(false, facing));
            SpawnSlashFx(false);
            FeelFeedback.Shake(0.08f, 0.07f);
            AudioDirector.Instance?.PlaySlash();
        }

        public void TrySpinAttack()
        {
            if (cooldownLeft > 0f) return;
            stats?.TrySpendStamina(spinStaminaCost);

            cooldownLeft = attackCooldown * 1.4f;
            lungeDir = Vector2.zero;
            lungeLeft = 0.08f;
            StartCoroutine(AttackWindow(true, Vector2.zero));
            SpawnSlashFx(true);
            FeelFeedback.Shake(0.16f, 0.12f);
            AudioDirector.Instance?.PlaySlash();
        }

        IEnumerator AttackWindow(bool spin, Vector2 facing)
        {
            IsAttacking = true;
            float dur = spin ? attackAnimSeconds * 1.35f : attackAnimSeconds;
            float dmg = baseDamage * (stats != null ? stats.SwordPower : 1f);
            if (spin) dmg *= 1.35f;

            var alreadyHit = new HashSet<Health>();
            int ticks = spin ? 4 : 3;
            for (int i = 0; i < ticks; i++)
            {
                float tickFacing = i / (float)Mathf.Max(1, ticks - 1);
                Vector2 sweep = spin ? Vector2.zero : Rotate(facing, Mathf.Lerp(-0.35f, 0.35f, tickFacing));
                if (spin) DealSpinDamage(dmg, alreadyHit);
                else DealArcDamage(dmg, sweep.sqrMagnitude > 0.01f ? sweep.normalized : facing, alreadyHit);
                yield return new WaitForSeconds(dur / ticks);
            }
            IsAttacking = false;
        }

        static Vector2 Rotate(Vector2 v, float radians)
        {
            float c = Mathf.Cos(radians), s = Mathf.Sin(radians);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        void DealArcDamage(float damage, Vector2 facing, HashSet<Health> alreadyHit)
        {
            if (facing.sqrMagnitude < 0.01f)
                facing = controller != null ? controller.FacingVector() : Vector2.down;
            Vector2 origin = (Vector2)transform.position + facing * 0.4f;
            var hits = Physics2D.OverlapCircleAll(origin + facing * (attackRange * 0.35f), attackRadius);
            ApplyHits(hits, damage, alreadyHit);
        }

        void DealSpinDamage(float damage, HashSet<Health> alreadyHit)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 1.25f);
            ApplyHits(hits, damage, alreadyHit);
        }

        void ApplyHits(Collider2D[] hits, float damage, HashSet<Health> alreadyHit)
        {
            var self = GetComponent<Health>();
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject == gameObject) continue;
                var h = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();
                if (h == null || h == self || h.IsDead) continue;
                if (!alreadyHit.Add(h)) continue; // full damage once per swing
                h.TakeDamage(damage, transform.position);
            }
        }

        void SpawnSlashFx(bool spin)
        {
            var go = new GameObject(spin ? "FX_Spin" : "FX_Slash");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 40;
            int size = 56;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            float cx = size * 0.5f, cy = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                bool draw = spin
                    ? (d > 16f && d < 24f)
                    : (d > 10f && d < 24f && ang > -1.0f && ang < 1.0f);
                float a = draw ? Mathf.Clamp01(1.3f - Mathf.Abs(d - 18f) * 0.12f) : 0f;
                px[y * size + x] = new Color(1f, 0.95f, 0.5f, a * 0.9f);
            }
            tex.SetPixels(px);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 28f);
            if (!spin && controller != null)
            {
                Vector2 f = controller.FacingVector();
                go.transform.position = transform.position + (Vector3)(f * 0.7f);
                go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg);
            }
            go.transform.localScale = Vector3.one * (spin ? 1.6f : 1.25f);
            StartCoroutine(FadeFx(go, sr, spin ? 0.22f : 0.16f));
        }

        IEnumerator FadeFx(GameObject go, SpriteRenderer sr, float life)
        {
            float t = 0f;
            Vector3 start = go.transform.localScale;
            while (t < life && go != null)
            {
                t += Time.deltaTime;
                float u = t / life;
                if (sr != null)
                {
                    var c = sr.color;
                    c.a = (1f - u) * 0.9f;
                    sr.color = c;
                }
                go.transform.localScale = start * (1f + u * 0.35f);
                yield return null;
            }
            if (go != null) Destroy(go);
        }
    }
}
