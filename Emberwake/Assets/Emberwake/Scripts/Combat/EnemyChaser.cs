using UnityEngine;

namespace Emberwake
{
    /// <summary>Chase AI — idle bob, strafe near player, reliable contact damage.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyChaser : MonoBehaviour
    {
        [SerializeField] float detectRange = 8.5f;
        [SerializeField] float attackRange = 1.28f;
        [SerializeField] float moveSpeed = 2.55f;
        [SerializeField] float attackCooldown = 0.95f;
        [SerializeField] float attackDamage = 1f;

        Rigidbody2D rb;
        Transform player;
        Health selfHp;
        Health playerHp;
        SpriteRenderer sr;
        float cooldown;
        float strafeSign = 1f;
        float bobPhase;

        public void Configure(float speed, float dmg, float range)
        {
            moveSpeed = speed;
            attackDamage = dmg;
            attackRange = range;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            selfHp = GetComponent<Health>();
            sr = GetComponent<SpriteRenderer>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
            strafeSign = Random.value > 0.5f ? 1f : -1f;
        }

        void Start() => ResolvePlayer();

        void ResolvePlayer()
        {
            if (player != null && playerHp != null) return;
            var p = FindFirstObjectByType<PlayerController>();
            if (p == null) return;
            player = p.transform;
            playerHp = p.GetComponent<Health>();
        }

        void FixedUpdate()
        {
            cooldown -= Time.fixedDeltaTime;
            if (selfHp != null && selfHp.IsDead)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            ResolvePlayer();
            if (player == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > detectRange)
            {
                // Idle wander bob
                float wx = Mathf.Sin(Time.time * 0.7f + bobPhase) * 0.35f;
                float wy = Mathf.Cos(Time.time * 0.55f + bobPhase) * 0.25f;
                rb.linearVelocity = new Vector2(wx, wy);
                return;
            }

            Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position);
            Vector2 dir = toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : Vector2.down;

            if (sr != null && Mathf.Abs(dir.x) > 0.05f)
                sr.flipX = dir.x < 0f;

            if (dist <= attackRange)
            {
                // Orbit / press-in so contact feels alive
                Vector2 tangent = new Vector2(-dir.y, dir.x) * strafeSign;
                Vector2 press = dist > 0.7f ? dir * 0.45f : -dir * 0.15f;
                Vector2 orbit = tangent * moveSpeed * 0.55f + press * moveSpeed;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, orbit, 0.4f);
                if (Time.frameCount % 90 == 0) strafeSign = -strafeSign;

                if (cooldown <= 0f)
                {
                    cooldown = attackCooldown;
                    if (playerHp != null && !playerHp.IsDead)
                        playerHp.TakeDamage(attackDamage, transform.position);
                }
                return;
            }

            // Smooth chase with slight sine weave
            Vector2 weave = new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.time * 3f + bobPhase) * 0.35f;
            Vector2 chase = (dir + weave).normalized * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, chase, 0.22f);
        }
    }
}
