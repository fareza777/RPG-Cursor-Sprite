using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// 4-direction top-down movement for Kael. Keyboard + portrait virtual stick.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 6.2f;
        [SerializeField] float runSpeed = 7.6f;
        [SerializeField] float runStaminaCostPerSecond = 8f;
        [SerializeField] PlayerStats stats;
        [SerializeField] SpriteRenderer spriteRenderer;

        Rigidbody2D rb;
        PlayerCombat combat;
        Vector2 input;
        public CardinalDir Facing { get; private set; } = CardinalDir.Down;
        public bool IsMoving { get; private set; }
        public bool IsRunning { get; private set; }
        public Vector2 MoveVector => input;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            combat = GetComponent<PlayerCombat>();
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (stats == null && GameManager.Instance != null) stats = GameManager.Instance.Stats;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void Update()
        {
            input = GameInput.Move;
            IsMoving = input.sqrMagnitude > 0.01f;
            if (IsMoving)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                    Facing = input.x > 0 ? CardinalDir.Right : CardinalDir.Left;
                else
                    Facing = input.y > 0 ? CardinalDir.Up : CardinalDir.Down;
            }

            bool wantsRun = GameInput.RunHeld;
            IsRunning = wantsRun && IsMoving && stats != null && stats.Stamina > 0f;

            if (IsRunning)
                stats.TrySpendStamina(runStaminaCostPerSecond * Time.deltaTime);
            else if (wantsRun && IsMoving)
                IsRunning = false;
        }

        void FixedUpdate()
        {
            float speed = IsRunning ? runSpeed : walkSpeed;
            Vector2 desired = input * speed;
            if (combat != null)
                desired += combat.AttackLunge;
            // Soft blend keeps knockback/lunge from being hard-clipped in one frame
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, 0.78f);
        }

        public Vector2 FacingVector()
        {
            return Facing switch
            {
                CardinalDir.Up => Vector2.up,
                CardinalDir.Left => Vector2.left,
                CardinalDir.Right => Vector2.right,
                _ => Vector2.down
            };
        }
    }
}
