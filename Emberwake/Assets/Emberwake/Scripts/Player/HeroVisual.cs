using UnityEngine;

namespace Emberwake
{
    /// <summary>Animated Kael from Super Retro hero walk/attack sheets.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HeroVisual : MonoBehaviour
    {
        [SerializeField] string colorFolder = "color_1";
        [SerializeField] float walkFps = 10f;
        [SerializeField] float attackFps = 14f;

        SpriteRenderer sr;
        PlayerController controller;
        PlayerCombat combat;
        Sprite[] down, up, left, right;
        Sprite[] atkDown, atkUp, atkLeft, atkRight;
        Sprite[] idleBreath;
        float frameTimer;
        int frameIndex;
        float bobTimer;
        Vector3 baseScale;
        bool wasAttacking;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            LoadSheets();
        }

        void Start()
        {
            BindRefs();
            if (baseScale.sqrMagnitude < 0.01f)
                baseScale = transform.localScale;
        }

        void BindRefs()
        {
            if (controller == null) controller = GetComponentInParent<PlayerController>();
            if (combat == null) combat = GetComponentInParent<PlayerCombat>();
        }

        void LoadSheets()
        {
            string root = $"Hero/hero/{colorFolder}/";
            down = Resources.LoadAll<Sprite>(root + "walk/hero_walk_DOWN");
            up = Resources.LoadAll<Sprite>(root + "walk/hero_walk_UP");
            left = Resources.LoadAll<Sprite>(root + "walk/hero_walk_LEFT");
            right = Resources.LoadAll<Sprite>(root + "walk/hero_walk_RIGHT");
            atkDown = Resources.LoadAll<Sprite>(root + "attack/hero_attack_DOWN");
            atkUp = Resources.LoadAll<Sprite>(root + "attack/hero_attack_UP");
            atkLeft = Resources.LoadAll<Sprite>(root + "attack/hero_attack_LEFT");
            atkRight = Resources.LoadAll<Sprite>(root + "attack/hero_attack_RIGHT");
            idleBreath = Resources.LoadAll<Sprite>(root + "breath_idle/hero_breath_idle_DOWN");

            if ((down == null || down.Length == 0))
                Debug.LogWarning("[Emberwake] Hero walk sheets missing from Resources");
            else
            {
                sr.sprite = down[0];
                Debug.Log($"[Emberwake] HeroVisual frames walkDown={down.Length}");
            }
        }

        void Update()
        {
            if (sr == null) return;
            BindRefs();
            if (controller == null) return;

            bobTimer += Time.deltaTime;
            float bob = controller.IsMoving ? Mathf.Sin(bobTimer * 18f) * 0.04f : Mathf.Sin(bobTimer * 3.2f) * 0.025f;
            transform.localScale = baseScale + new Vector3(0f, bob, 0f);

            bool attacking = combat != null && combat.IsAttacking;
            if (attacking && !wasAttacking)
            {
                frameIndex = 0;
                frameTimer = 0f;
            }
            wasAttacking = attacking;

            if (attacking)
            {
                PlaySheet(AttackSheet(), attackFps, false);
                return;
            }

            Sprite[] walk = WalkSheet();
            if (walk == null || walk.Length == 0) return;

            if (!controller.IsMoving)
            {
                if (idleBreath != null && idleBreath.Length > 0 && controller.Facing == CardinalDir.Down)
                    PlaySheet(idleBreath, 5f, true);
                else
                {
                    sr.sprite = walk[0];
                    frameIndex = 0;
                    frameTimer = 0f;
                }
                return;
            }

            float fps = walkFps * (controller.IsRunning ? 1.45f : 1f);
            PlaySheet(walk, fps, true);
        }

        void PlaySheet(Sprite[] sheet, float fps, bool loop)
        {
            if (sheet == null || sheet.Length == 0) return;
            frameTimer += Time.deltaTime * fps;
            if (frameTimer >= 1f)
            {
                frameTimer -= 1f;
                if (loop)
                    frameIndex = (frameIndex + 1) % sheet.Length;
                else
                    frameIndex = Mathf.Min(frameIndex + 1, sheet.Length - 1);
            }
            frameIndex = Mathf.Clamp(frameIndex, 0, sheet.Length - 1);
            sr.sprite = sheet[frameIndex];
        }

        Sprite[] WalkSheet()
        {
            return controller.Facing switch
            {
                CardinalDir.Up => up,
                CardinalDir.Left => left,
                CardinalDir.Right => right,
                _ => down
            };
        }

        Sprite[] AttackSheet()
        {
            return controller.Facing switch
            {
                CardinalDir.Up => atkUp != null && atkUp.Length > 0 ? atkUp : atkDown,
                CardinalDir.Left => atkLeft != null && atkLeft.Length > 0 ? atkLeft : atkDown,
                CardinalDir.Right => atkRight != null && atkRight.Length > 0 ? atkRight : atkDown,
                _ => atkDown
            };
        }
    }
}
