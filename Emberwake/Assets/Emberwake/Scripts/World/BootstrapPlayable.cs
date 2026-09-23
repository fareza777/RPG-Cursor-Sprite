using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Emberwake
{
    /// <summary>
    /// Spawns a portrait-ready Millbrook training yard when the scene is empty/minimal.
    /// Attach to an empty GameObject in Millbrook_Prototype.
    /// </summary>
    public class BootstrapPlayable : MonoBehaviour
    {
        [SerializeField] bool spawnOnAwake = true;
        [SerializeField] int slimeCount = 4;
        [SerializeField] Vector2 arenaSize = new Vector2(18f, 28f);

        void Awake()
        {
            if (spawnOnAwake) Build();
        }

        public void Build()
        {
            ForcePortrait();
            BuildManagers();
            var player = BuildPlayer();
            BuildArena();
            BuildCamera(player.transform);
            BuildHud();
            SpawnSlimes(player.transform);
            SpawnKeyItem();
            Debug.Log("[Emberwake] Portrait playable prototype ready (Millbrook yard).");
        }

        static void ForcePortrait()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        void BuildManagers()
        {
            if (GameManager.Instance != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<PlayerStats>();
            go.AddComponent<InventorySystem>();
            go.AddComponent<StoryProgress>();
            go.AddComponent<WickRankSystem>();
            go.AddComponent<SaveSystem>();
            go.AddComponent<GameManager>();
        }

        GameObject BuildPlayer()
        {
            var existing = FindFirstObjectByType<PlayerController>();
            if (existing != null) return existing.gameObject;

            var player = new GameObject("Kael");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            // Placeholder until Resources load; HeroVisual replaces sprite
            sr.sprite = CreatePixelSprite(new Color(0.3f, 0.55f, 0.95f));
            sr.transform.localScale = Vector3.one * 1.2f;

            player.AddComponent<Health>();
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerCombat>();
            visual.AddComponent<HeroVisual>();

            // Drive player hearts from GameManager stats (HUD + save)
            if (GameManager.Instance != null && GameManager.Instance.Stats != null)
            {
                // Health.Awake already ran — rebind via public helper
                player.GetComponent<Health>().BindPlayerStats(GameManager.Instance.Stats);
            }

            return player;
        }

        void BuildArena()
        {
            // Ground tint (portrait taller playfield)
            var ground = new GameObject("Ground");
            var gsr = ground.AddComponent<SpriteRenderer>();
            gsr.sprite = CreatePixelSprite(new Color(0.28f, 0.55f, 0.28f));
            gsr.sortingOrder = -20;
            ground.transform.localScale = new Vector3(arenaSize.x, arenaSize.y, 1f);

            // Soft walls
            CreateWall("WallN", new Vector2(0f, arenaSize.y * 0.5f), new Vector2(arenaSize.x, 1f));
            CreateWall("WallS", new Vector2(0f, -arenaSize.y * 0.5f), new Vector2(arenaSize.x, 1f));
            CreateWall("WallE", new Vector2(arenaSize.x * 0.5f, 0f), new Vector2(1f, arenaSize.y));
            CreateWall("WallW", new Vector2(-arenaSize.x * 0.5f, 0f), new Vector2(1f, arenaSize.y));

            // Props markers (pots stand-ins)
            for (int i = 0; i < 6; i++)
            {
                var pot = new GameObject($"Pot_{i}");
                pot.transform.position = new Vector3(Random.Range(-6f, 6f), Random.Range(-10f, 10f), 0f);
                var sr = pot.AddComponent<SpriteRenderer>();
                sr.sprite = CreatePixelSprite(new Color(0.7f, 0.45f, 0.2f));
                sr.sortingOrder = 5;
                pot.transform.localScale = Vector3.one * 0.6f;
                var col = pot.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
            }
        }

        static void CreateWall(string name, Vector2 pos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.position = pos;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            var rb = wall.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            // Portrait: taller view — show more vertical of the yard
            cam.orthographicSize = 9f;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.12f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.SetTarget(target);
            follow.SetRoomLock(true, new Vector2(-6f, -11f), new Vector2(6f, 11f));

            // URP 2D light for lantern mood
            if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
                cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            var lightGo = new GameObject("GlobalLight2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 0.85f;
        }

        void BuildHud()
        {
            if (FindFirstObjectByType<PortraitMobileHud>() != null) return;
            var hud = new GameObject("PortraitHUD");
            hud.AddComponent<PortraitMobileHud>();
        }

        void SpawnSlimes(Transform player)
        {
            for (int i = 0; i < slimeCount; i++)
            {
                var e = new GameObject($"Slime_{i}");
                e.transform.position = new Vector3(Random.Range(-5f, 5f), Random.Range(3f, 10f), 0f);
                var sr = e.AddComponent<SpriteRenderer>();
                var slimeSprite = Resources.Load<Sprite>("Battlers/SlimeA");
                sr.sprite = slimeSprite != null ? slimeSprite : CreatePixelSprite(new Color(0.3f, 0.85f, 0.35f));
                sr.sortingOrder = 8;
                e.transform.localScale = Vector3.one * (slimeSprite != null ? 1f : 0.9f);

                var rb = e.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                e.AddComponent<CircleCollider2D>().radius = 0.4f;
                var hp = e.AddComponent<Health>();
                hp.ConfigureEnemy(3f);
                e.AddComponent<EnemyChaser>();
            }
        }

        void SpawnKeyItem()
        {
            var item = new GameObject("GlovesOfLift_Pickup");
            item.transform.position = new Vector3(0f, 8f, 0f);
            var sr = item.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePixelSprite(new Color(1f, 0.9f, 0.2f));
            sr.sortingOrder = 12;
            item.transform.localScale = Vector3.one * 0.7f;
            var col = item.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            item.AddComponent<KeyItemPickup>();
        }

        static Sprite CreatePixelSprite(Color color)
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
