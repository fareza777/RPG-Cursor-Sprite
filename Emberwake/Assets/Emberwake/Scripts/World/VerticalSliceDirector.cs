using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Emberwake
{
    /// <summary>
    /// Vertical slice director: Millbrook → Hollowroot rooms → Gloves → Barkling → CLEAR.
    /// Designed for portrait APK playtest (10–20 min target feel; prototype shorter).
    /// </summary>
    public class VerticalSliceDirector : MonoBehaviour
    {
        public static VerticalSliceDirector Instance { get; private set; }

        public int RoomIndex => (int)room;
        public bool GlovesTaken => glovesTaken;
        public bool BossDead => bossDead;

        [SerializeField] bool buildOnAwake = true;

        enum RoomId { Hub, RootPath, Pressure, Reliquary, Boss, Altar }

        RoomId room = RoomId.Hub;
        Transform player;
        Text objectiveText;
        Text toastText;
        float toastTimer;
        bool glovesTaken;
        bool bossDead;
        bool cleared;
        float runTimer;
        GameObject block;
        GameObject pressurePlate;
        GameObject northGate;
        Vector2 roomCenter;
        CameraFollow camFollow;
        string statusLine = "booting...";
        string objectiveLine = "";
        string toastLine = "";
        static Material spriteMat;

        const float CamOrtho = 15.5f;
        readonly Vector2 roomSize = new Vector2(18f, 26f);

        bool gameplayActive;

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            if (buildOnAwake)
            {
                try
                {
                    BuildSlice();
                    statusLine = "title";
                    Debug.Log("[Emberwake] Vertical slice built OK");
                }
                catch (System.Exception ex)
                {
                    statusLine = "ERROR: " + ex.Message;
                    Debug.LogException(ex);
                }
            }
        }

        void OnGUI() { }

        void Update()
        {
            if (!gameplayActive || cleared) return;
            runTimer += Time.deltaTime;
            ClampPlayerInRoom();
            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f)
                {
                    toastLine = "";
                    if (toastText != null) toastText.text = "";
                }
            }

            if (room == RoomId.Pressure && block != null && pressurePlate != null)
            {
                if (Vector2.Distance(block.transform.position, pressurePlate.transform.position) < 0.65f)
                    OpenGate();
            }
        }

        public void BuildSlice()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;

            SliceArt.LogStatusOnce();
            EnsureSpriteMaterial();
            BuildManagers();
            if (FindFirstObjectByType<FeelFeedback>() == null)
                new GameObject("FeelFeedback").AddComponent<FeelFeedback>();
            if (AudioDirector.Instance == null)
                new GameObject("AudioDirector").AddComponent<AudioDirector>();

            player = BuildPlayer().transform;
            SetPlayerFrozen(true);
            BuildCamera(player);
            BuildPortraitHud();
            LoadRoom(RoomId.Hub);
            SetObjective("Pergi ke gerbang utara");
        }

        void BeginGameplay()
        {
            gameplayActive = true;
            SetPlayerFrozen(true);
            AudioDirector.Instance?.SetMusicMood("explore");
            AudioDirector.Instance?.PlayMusic();
            var save = FindFirstObjectByType<SaveSystem>();
            if (save != null && save.ConsumeContinue(out var data))
            {
                ApplyContinue(data);
                return;
            }
            SetObjective("Dengarkan Mara");
            QuestSystem.Instance?.Discover("main_wick");
            Debug.Log("[Emberwake] Gameplay started — opening dialog");

            if (DialogBox.Instance != null)
            {
                DialogBox.Instance.Play(new[]
                {
                    new DialogLine("Mara", "Kael… kau kembali. Tujuh tahun tanpa kabar."),
                    new DialogLine("Kael", "Aku… tidak ingat apa-apa. Hanya Wick yang berkedip."),
                    new DialogLine("Mara", "Lentera Millbrook sekarat. Jika padam, desa akan lupa namanya."),
                    new DialogLine("Bram", "Aku Bram, penjaga gerbang. Jalan utara sudah retak."),
                    new DialogLine("Mara", "Pergilah ke UTARA — Hollowroot. Nyalakan Wick hutan."),
                    new DialogLine("Lira", "Kak… hati-hati. Monster Ashdeep sudah merayap di jalan."),
                    new DialogLine("Sera", "Aku Sera. Jika kau jatuh, aku yang menarikmu kembali."),
                    new DialogLine("Wick", "…nyala… ingat… nama…"),
                    new DialogLine("Mara", "Di menu ▣ kau bisa lihat quest, bestiary, dan level."),
                    new DialogLine("Kael", "Aku akan jaga Wick. Tunggu aku.")
                }, () =>
                {
                    SetObjective("Masuk Hollowroot (utara)");
                    ShowToast("Ikuti jalan tanah ke UTARA · buka ▣ untuk quest");
                    SetPlayerFrozen(false);
                });
            }
            else
            {
                SetPlayerFrozen(false);
                ShowToast("Wick Millbrook sekarat — ke UTARA!");
            }
        }

        public void SetPlayerFrozenPublic(bool frozen) => SetPlayerFrozen(frozen);

        void SetPlayerFrozen(bool frozen)
        {
            if (player == null) return;
            var rb = player.GetComponent<Rigidbody2D>();
            var pc = player.GetComponent<PlayerController>();
            var combat = player.GetComponent<PlayerCombat>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = !frozen;
            }
            if (pc != null) pc.enabled = !frozen;
            if (combat != null) combat.enabled = !frozen;
            GameInput.SetMobileMove(Vector2.zero);
        }

        static void EnsureSpriteMaterial()
        {
            if (spriteMat != null) return;
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                spriteMat = new Material(shader);
                spriteMat.name = "EmberwakeSpriteUnlit";
            }
        }

        static void Paint(SpriteRenderer sr)
        {
            if (sr == null) return;
            EnsureSpriteMaterial();
            if (spriteMat != null) sr.sharedMaterial = spriteMat;
        }

        void BuildManagers()
        {
            if (GameManager.Instance != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<PlayerStats>();
            go.AddComponent<InventorySystem>();
            go.AddComponent<StoryProgress>();
            go.AddComponent<WickRankSystem>();
            go.AddComponent<LevelingSystem>();
            go.AddComponent<QuestSystem>();
            go.AddComponent<BestiarySystem>();
            go.AddComponent<SaveSystem>();
            go.AddComponent<GameManager>();
        }

        GameObject BuildPlayer()
        {
            var existing = FindFirstObjectByType<PlayerController>();
            if (existing != null) return existing.gameObject;

            var p = new GameObject("Kael");
            try { p.tag = "Player"; }
            catch (UnityException) { Debug.LogWarning("[Emberwake] Tag Player missing — continuing without tag"); }
            var rb = p.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            p.AddComponent<CircleCollider2D>().radius = 0.35f;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(p.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 20;
            sr.sprite = SliceArt.HeroIdle();
            Paint(sr);
            visual.transform.localScale = Vector3.one * 1.05f;
            GroundShadow.Attach(p.transform, 1.0f);

            // Controller BEFORE HeroVisual so Awake can bind (also re-binds in Start)
            p.AddComponent<Health>();
            p.AddComponent<PlayerController>();
            p.AddComponent<PlayerCombat>();
            p.AddComponent<SpriteFlash>();
            visual.AddComponent<HeroVisual>();
            if (GameManager.Instance != null)
                p.GetComponent<Health>().BindPlayerStats(GameManager.Instance.Stats);
            var playerHp = p.GetComponent<Health>();
            if (playerHp != null)
            {
                playerHp.OnDied -= OnPlayerDied;
                playerHp.OnDied += OnPlayerDied;
            }
            return p;
        }

        void OnPlayerDied()
        {
            if (cleared) return;
            Time.timeScale = 1f;
            SetPlayerFrozen(true);
            PortraitMobileHud.Instance?.SetControlsVisible(false);
            AudioDirector.Instance?.PlayDeath();
            AudioDirector.Instance?.SetMusicMood("title");
            ShowToast("Kael tumbang…");
            Debug.Log("[Emberwake] Player died — showing death dialog");
            if (DialogBox.Instance != null)
            {
                DialogBox.Instance.Play(new[]
                {
                    new DialogLine("Mara", "Kael! Bangun… Wick masih menunggu."),
                    new DialogLine("Kael", "Aku… masih bisa berdiri.")
                }, RespawnPlayer);
            }
            else
                RespawnPlayer();
        }

        void RespawnPlayer()
        {
            if (player == null) return;
            // Restore hearts
            if (GameManager.Instance != null && GameManager.Instance.Stats != null)
            {
                var s = GameManager.Instance.Stats;
                int missing = s.MaxHearts - s.Hearts;
                if (missing > 0) s.HealHearts(missing);
                else if (s.Hearts <= 0) s.HealHearts(s.MaxHearts);
            }
            player.position = new Vector3(0f, -2.2f, 0f);
            LoadRoom(RoomId.Hub);
            SetObjective("Masuk Hollowroot (utara)");
            ShowToast("Kembali di Millbrook");
            SetPlayerFrozen(false);
            PortraitMobileHud.Instance?.ShowGameplayHud();
        }

        void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            // Critical for URP on device — without this the screen stays black
            var urpCam = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpCam == null)
                urpCam = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = false;
            urpCam.renderType = CameraRenderType.Base;

            cam.orthographic = true;
            cam.orthographicSize = CamOrtho;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.08f);
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.nearClipPlane = -10f;
            cam.farClipPlane = 100f;

            // Global 2D light so sprites aren't lit black by URP 2D
            if (FindFirstObjectByType<Light2D>() == null)
            {
                var lightGo = new GameObject("SR_GlobalLight");
                var light = lightGo.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Global;
                light.intensity = 1.05f;
                light.color = Color.white;
            }

            camFollow = cam.GetComponent<CameraFollow>() ?? cam.gameObject.AddComponent<CameraFollow>();
            camFollow.SetTarget(target);
        }

        void BuildPortraitHud()
        {
            try
            {
                var existing = FindFirstObjectByType<PortraitMobileHud>();
                if (existing == null)
                {
                    var hud = new GameObject("PortraitHUD");
                    existing = hud.AddComponent<PortraitMobileHud>();
                }
                existing.Build(BeginGameplay);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Emberwake] HUD canvas failed: " + ex.Message);
                BeginGameplay();
            }
        }

        void ClampPlayerInRoom()
        {
            if (player == null) return;
            float pad = 0.9f;
            float minX = roomCenter.x - roomSize.x * 0.5f + pad;
            float maxX = roomCenter.x + roomSize.x * 0.5f - pad;
            float minY = roomCenter.y - roomSize.y * 0.5f + pad;
            float maxY = roomCenter.y + roomSize.y * 0.5f - pad;
            var p = player.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.y = Mathf.Clamp(p.y, minY, maxY);
            player.position = p;
        }

        void SetObjective(string msg)
        {
            objectiveLine = msg;
            statusLine = msg;
            if (PortraitMobileHud.Instance != null)
                PortraitMobileHud.Instance.SetObjective(msg);
            else if (objectiveText != null)
                objectiveText.text = msg;
        }

        public void ShowToast(string msg)
        {
            toastLine = msg;
            toastTimer = 3.5f;
            if (PortraitMobileHud.Instance != null)
                PortraitMobileHud.Instance.ShowToast(msg);
            else if (toastText != null)
                toastText.text = msg;
            Debug.Log("[Emberwake] " + msg);
        }

        void ClearWorldProps()
        {
            foreach (var n in new[] { "SliceRoom", "SliceEnemy", "SliceProp", "SliceTrigger", "SliceBoss" })
            {
                var found = GameObject.FindGameObjectsWithTag("Untagged");
                // destroy by name prefix
            }
            var all = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = all.Length - 1; i >= 0; i--)
            {
                var t = all[i];
                if (t == null) continue;
                string n = t.name;
                if (n.StartsWith("SR_") || n.StartsWith("SE_") || n.StartsWith("SP_") || n.StartsWith("ST_") || n.StartsWith("SB_") || n == "EmberFX")
                    Destroy(t.gameObject);
            }
            block = null;
            pressurePlate = null;
            northGate = null;
        }

        void LoadRoom(RoomId id)
        {
            room = id;
            ClearWorldProps();
            roomCenter = Vector2.zero;
            player.position = new Vector3(0f, -2.2f, 0f);

            string floorKey = id switch
            {
                RoomId.Hub => "hub",
                RoomId.RootPath => "root",
                RoomId.Pressure => "pressure",
                RoomId.Reliquary => "reliquary",
                RoomId.Boss => "boss",
                _ => "altar"
            };

            BuildFloor(floorKey);
            BuildWalls();
            DecorateRoom(id);
            if (id == RoomId.Hub || id == RoomId.Boss || id == RoomId.Altar)
                EmberParticles.Attach(transform, id == RoomId.Boss ? 34 : 24);
            // Lock camera so view stays mostly on floor
            float halfH = CamOrtho;
            float halfW = halfH * (9f / 16f);
            float margin = 0.2f;
            camFollow.SetRoomLock(true,
                roomCenter + new Vector2(-roomSize.x * 0.5f + halfW + margin, -roomSize.y * 0.5f + halfH + margin),
                roomCenter + new Vector2(roomSize.x * 0.5f - halfW - margin, roomSize.y * 0.5f - halfH - margin));
            if (Camera.main != null && player != null)
            {
                var p = player.position;
                float minX = roomCenter.x - roomSize.x * 0.5f + halfW + margin;
                float maxX = roomCenter.x + roomSize.x * 0.5f - halfW - margin;
                float minY = roomCenter.y - roomSize.y * 0.5f + halfH + margin;
                float maxY = roomCenter.y + roomSize.y * 0.5f - halfH - margin;
                // If lock range inverted (room too small for ortho), center on room
                if (minX > maxX) { minX = maxX = roomCenter.x; }
                if (minY > maxY) { minY = maxY = roomCenter.y; }
                p.x = Mathf.Clamp(p.x, minX, maxX);
                p.y = Mathf.Clamp(p.y, minY, maxY);
                Camera.main.transform.position = new Vector3(p.x, p.y, -10f);
            }

            switch (id)
            {
                case RoomId.Hub:
                    SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.RootPath));
                    SpawnProp(SliceArt.Torch(0), new Vector2(-1.7f, roomSize.y * 0.36f), 1.35f, 11);
                    SpawnProp(SliceArt.Torch(1), new Vector2(1.7f, roomSize.y * 0.36f), 1.35f, 11);
                    SpawnProp(SliceArt.Column(2), new Vector2(-3.4f, roomSize.y * 0.34f), 1.2f, 9);
                    SpawnProp(SliceArt.Column(3), new Vector2(3.4f, roomSize.y * 0.34f), 1.2f, 9);
                    SpawnSign(new Vector2(0f, roomSize.y * 0.28f), "UTARA → Hollowroot");
                    SpawnSign(new Vector2(2.5f, -2f), "Millbrook");
                    SetObjective("Masuk Hollowroot (utara)");
                    AudioDirector.Instance?.SetMusicMood("explore");
                    break;
                case RoomId.RootPath:
                    SpawnSlime(new Vector2(-3.2f, -1.2f));
                    SpawnSlime(new Vector2(3.1f, 1.6f));
                    SpawnSlime(new Vector2(0.4f, 5.2f));
                    SpawnEnemy("ash_wisp", new Vector2(-2.4f, 7.2f), 2f, 0.7f);
                    SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Pressure));
                    SetObjective("Bersihkan jalan akar → utara");
                    ShowToast("Root Path. Kalahkan slime, lanjut utara.");
                    AudioDirector.Instance?.SetMusicMood("battle");
                    QuestSystem.Instance?.AddProgress("side_explore");
                    QuestSystem.Instance?.Discover("main_gloves");
                    break;
                case RoomId.Pressure:
                    pressurePlate = SpawnPlate(new Vector2(2.5f, 2f));
                    block = SpawnPushBlock(new Vector2(-2.5f, -1f));
                    northGate = SpawnGate(new Vector2(0f, roomSize.y * 0.4f));
                    SpawnEnemy("root_crawler", new Vector2(0f, 1.5f), 5f, 1.0f);
                    SetObjective("Dorong peti ke plat (kanan atas)");
                    ShowToast("Pressure Chamber: dorong PETI ke PLAT.");
                    AudioDirector.Instance?.SetMusicMood("explore");
                    QuestSystem.Instance?.AddProgress("side_explore");
                    break;
                case RoomId.Reliquary:
                    SpawnEnemy("ember_moth", new Vector2(-2.5f, 1f), 2f, 0.75f);
                    SpawnEnemy("ember_moth", new Vector2(2.5f, 1.4f), 2f, 0.75f);
                    if (glovesTaken)
                    {
                        SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Boss));
                        SetObjective("Hadapi Barkling (utara)");
                        ShowToast("Sarung tangan sudah di tangan. Lanjut utara.");
                    }
                    else
                    {
                        SpawnGloves(new Vector2(0f, 2.5f));
                        SetObjective("Ambil Gloves of Lift");
                        ShowToast("Reliquary. Ambil sarung tangan emas.");
                    }
                    AudioDirector.Instance?.SetMusicMood("explore");
                    QuestSystem.Instance?.AddProgress("side_explore");
                    break;
                case RoomId.Boss:
                    if (bossDead)
                    {
                        SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Altar));
                        SetObjective("Ke altar Wick");
                        ShowToast("Barkling sudah tumbang. Altar di utara.");
                        AudioDirector.Instance?.SetMusicMood("explore");
                    }
                    else
                    {
                        SpawnBarkling(new Vector2(0f, 3f));
                        SpawnEnemy("hollow_knight", new Vector2(-3f, 1f), 8f, 1.05f);
                        SetObjective("Kalahkan Barkling!");
                        ShowToast("Barkling Nest — hati-hati slam!");
                        AudioDirector.Instance?.SetMusicMood("boss");
                        AudioDirector.Instance?.PlayBoss();
                        QuestSystem.Instance?.Discover("main_barkling");
                    }
                    QuestSystem.Instance?.AddProgress("side_explore");
                    break;
                case RoomId.Altar:
                    SpawnAltar(new Vector2(0f, 2f));
                    SetObjective("Sentuh Wick Altar");
                    ShowToast("Nyalakan Wick Millbrook!");
                    AudioDirector.Instance?.SetMusicMood("title");
                    break;
            }
        }

        void ApplyContinue(SaveData data)
        {
            glovesTaken = data.gloves != 0;
            bossDead = data.boss != 0;
            GameManager.Instance?.WickRank?.Restore(Mathf.Max(1, data.wickRank), data.essence);
            GameManager.Instance?.Inventory?.SetGold(data.gold);
            if (glovesTaken)
                GameManager.Instance?.Inventory?.GrantKeyItem(KeyItemId.GlovesOfLift);
            GameManager.Instance?.Stats?.Restore(data.hearts, data.maxHearts, data.maxStamina, data.swordPower);
            LevelingSystem.Instance?.Restore(data.level, data.xp);
            QuestSystem.Instance?.Import(data.quests);
            BestiarySystem.Instance?.ImportUnlocked(data.bestiary);
            int roomId = Mathf.Clamp(data.room, 0, (int)RoomId.Altar);
            LoadRoom((RoomId)roomId);
            string where = ((RoomId)roomId).ToString();
            if (DialogBox.Instance != null)
            {
                DialogBox.Instance.Play(new[]
                {
                    new DialogLine("Kael", $"Aku kembali. Level {data.level}, ruangan {where}."),
                    new DialogLine("Mara", "Wick masih menunggu. Lanjutkan ke utara.")
                }, () => SetPlayerFrozen(false));
            }
            else SetPlayerFrozen(false);
            ShowToast("Perjalanan dilanjutkan");
        }

        void OpenGate()
        {
            if (northGate == null) return;
            Destroy(northGate);
            northGate = null;
            SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Reliquary));
            ShowToast("Pintu terbuka!");
            SetObjective("Masuk Reliquary (utara)");
        }

        void OnGlovesTaken()
        {
            if (glovesTaken) return;
            glovesTaken = true;
            GameManager.Instance?.Inventory.GrantKeyItem(KeyItemId.GlovesOfLift);
            GameManager.Instance?.Story.SetFlag(StoryFlag.ClearedHollowroot);
            ShowToast("Gloves of Lift didapat!");
            AudioDirector.Instance?.PlayPickup();
            QuestSystem.Instance?.Discover("main_gloves");
            QuestSystem.Instance?.Complete("main_gloves");
            SetObjective("Hadapi Barkling (utara)");
            SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Boss));
        }

        void OnBossDefeated()
        {
            if (bossDead) return;
            bossDead = true;
            GameManager.Instance?.WickRank.AddEssence(80);
            LevelingSystem.Instance?.AddXp(80);
            QuestSystem.Instance?.Complete("main_barkling");
            ShowToast("Barkling tumbang!");
            SetObjective("Ke altar Wick");
            SpawnExitTrigger(new Vector2(0f, roomSize.y * 0.42f), () => LoadRoom(RoomId.Altar));
        }

        void OnAltarTouched()
        {
            if (cleared) return;
            cleared = true;
            GameManager.Instance?.Story.SetFlag(StoryFlag.VillageWickSaved);
            QuestSystem.Instance?.Complete("main_wick");
            LevelingSystem.Instance?.AddXp(50);
            AudioDirector.Instance?.PlayQuest();
            AudioDirector.Instance?.SetMusicMood("title");
            SetObjective("SLICE CLEAR");
            ShowToast($"CLEAR! Waktu {runTimer:0}s — Wick hidup lagi.");
            BuildClearOverlay();
        }

        void BuildClearOverlay()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
            var panel = new GameObject("ClearPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = panel.GetComponent<Image>();
            img.sprite = UiArt.SoftPanel();
            img.color = new Color(0f, 0f, 0f, 0.78f);
            img.raycastTarget = true;

            var go = new GameObject("ClearLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(panel.transform, false);
            var lrt = (RectTransform)go.transform;
            lrt.anchorMin = new Vector2(0.1f, 0.25f);
            lrt.anchorMax = new Vector2(0.9f, 0.75f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var label = go.GetComponent<Text>();
            UiArt.StyleLabel(label, 44, new Color(1f, 0.85f, 0.4f));
            label.text = $"EMBERWAKE\nSLICE CLEAR\n{runTimer:0} detik\n\nTerima kasih playtest!";
        }

        // ----- spawn helpers -----

        void BuildFloor(string roomKey)
        {
            // Solid grass base — never sky / landscape paintings as walkable floor
            var under = new GameObject("SR_Underlay");
            under.transform.position = roomCenter;
            var usr = under.AddComponent<SpriteRenderer>();
            usr.sprite = SliceArt.PixelColor(RoomGrassColor(roomKey));
            Paint(usr);
            usr.sortingOrder = -80;
            under.transform.localScale = new Vector3(roomSize.x * 2.2f, roomSize.y * 2.2f, 1f);

            // Proper 1x1 fill tiles — never whole 64x64 wang atlas sheets
            bool dungeon = roomKey is "pressure" or "reliquary" or "boss" or "altar";
            float tile = 1f;
            int cols = Mathf.CeilToInt(roomSize.x / tile) + 4;
            int rows = Mathf.CeilToInt(roomSize.y / tile) + 4;
            var grid = new GameObject("SR_TileGrid");
            grid.transform.position = roomCenter;

            for (int y = -rows / 2; y <= rows / 2; y++)
            for (int x = -cols / 2; x <= cols / 2; x++)
            {
                int ax = Mathf.Abs(x);
                bool onPath = !dungeon && ax <= 1;
                bool edge = !dungeon && ax == 2;
                var tileGo = new GameObject(onPath ? "SR_Path" : (edge ? "SR_Edge" : "SR_Floor"));
                tileGo.transform.SetParent(grid.transform, false);
                tileGo.transform.localPosition = new Vector3(x * tile, y * tile, 0f);
                var sr = tileGo.AddComponent<SpriteRenderer>();
                int idx = (x * 17 + y * 31) ^ (x * y);
                if (dungeon) sr.sprite = SliceArt.StoneTile(idx);
                else if (onPath) sr.sprite = SliceArt.PathTile(idx);
                else if (edge) sr.sprite = SliceArt.PathEdgeTile(idx, x > 0);
                else sr.sprite = SliceArt.GrassTile(idx);
                Paint(sr);
                sr.sortingOrder = -50;
                float sx = (((x + y) & 1) == 0) ? 1f : -1f;
                float sy = ((y & 1) == 0) ? 1f : -1f;
                if (edge)
                {
                    // Edge sprite: dirt on left / grass on right. Path is center → flip on left side.
                    sx = x > 0 ? 1f : -1f;
                    sy = 1f;
                }
                if (sr.sprite != null)
                {
                    float w = sr.sprite.bounds.size.x;
                    float h = sr.sprite.bounds.size.y;
                    if (w > 0.01f && h > 0.01f)
                        tileGo.transform.localScale = new Vector3(sx * tile / w, sy * tile / h, 1f);
                }
            }

            // Horizon painting only as thin north strip (not walkable floor)
            var bg = SliceArt.FloorBg(roomKey);
            if (bg != null)
            {
                var far = new GameObject("SR_FarBackdrop");
                far.transform.position = roomCenter + new Vector2(0f, roomSize.y * 0.5f + 1.2f);
                var fsr = far.AddComponent<SpriteRenderer>();
                fsr.sprite = bg;
                Paint(fsr);
                fsr.sortingOrder = -90;
                float wu = bg.bounds.size.x;
                float hu = bg.bounds.size.y;
                if (wu > 0.01f && hu > 0.01f)
                    far.transform.localScale = new Vector3(roomSize.x * 1.5f / wu, 3.2f / hu, 1f);
            }
        }

        static Color RoomPathColor(string roomKey)
        {
            return roomKey switch
            {
                "pressure" or "reliquary" or "boss" or "altar" => new Color(0.35f, 0.34f, 0.32f),
                "root" => new Color(0.42f, 0.32f, 0.2f),
                _ => new Color(0.48f, 0.4f, 0.28f)
            };
        }

        static Color RoomGrassColor(string roomKey)
        {
            return roomKey switch
            {
                "pressure" or "reliquary" or "boss" or "altar" => new Color(0.22f, 0.24f, 0.28f),
                "root" => new Color(0.18f, 0.38f, 0.2f),
                _ => new Color(0.28f, 0.52f, 0.28f)
            };
        }

        void BuildWalls()
        {
            float hx = roomSize.x * 0.5f;
            float hy = roomSize.y * 0.5f;
            Wall("SR_WallN", new Vector2(0f, hy), new Vector2(roomSize.x, 0.8f));
            Wall("SR_WallS", new Vector2(0f, -hy), new Vector2(roomSize.x, 0.8f));
            Wall("SR_WallE", new Vector2(hx, 0f), new Vector2(0.8f, roomSize.y));
            Wall("SR_WallW", new Vector2(-hx, 0f), new Vector2(0.8f, roomSize.y));

            // Visual rock border (sparse, not a solid grey wall of cubes)
            for (int i = -3; i <= 3; i += 2)
            {
                SpawnProp(SliceArt.Rock(Mathf.Abs(i)), new Vector2(i * 1.35f, hy - 0.2f), 1.05f, 8);
                SpawnProp(SliceArt.Rock(Mathf.Abs(i) + 1), new Vector2(i * 1.35f, -hy + 0.2f), 1.05f, 8);
            }
            for (int i = -4; i <= 4; i += 2)
            {
                SpawnProp(SliceArt.Rock(Mathf.Abs(i) + 2), new Vector2(hx - 0.2f, i * 1.4f), 1f, 8);
                SpawnProp(SliceArt.Rock(Mathf.Abs(i) + 3), new Vector2(-hx + 0.2f, i * 1.4f), 1f, 8);
            }
        }

        void DecorateRoom(RoomId id)
        {
            switch (id)
            {
                case RoomId.Hub:
                    SpawnProp(SliceArt.Barrel(0), new Vector2(-4.2f, -2.2f), 1.15f, 9);
                    SpawnProp(SliceArt.Barrel(1), new Vector2(-3.3f, -2.6f), 1.05f, 9);
                    SpawnProp(SliceArt.Barrel(2), new Vector2(-4.5f, -3.1f), 0.95f, 9);
                    SpawnProp(SliceArt.Pot(0), new Vector2(4f, -1.8f), 1.15f, 9);
                    SpawnProp(SliceArt.Pot(1), new Vector2(4.7f, -2.3f), 1.05f, 9);
                    SpawnProp(SliceArt.Crate(0), new Vector2(3.2f, -3.2f), 1.1f, 9);
                    SpawnProp(SliceArt.Column(2), new Vector2(-1.7f, 8.6f), 1.55f, 9);
                    SpawnProp(SliceArt.Column(3), new Vector2(1.7f, 8.6f), 1.55f, 9);
                    SpawnProp(SliceArt.Torch(0), new Vector2(-1.6f, 6.4f), 1.2f, 11);
                    SpawnProp(SliceArt.Torch(1), new Vector2(1.6f, 6.4f), 1.2f, 11);
                    SpawnProp(SliceArt.Torch(2), new Vector2(-1.6f, 3.2f), 1.15f, 11);
                    SpawnProp(SliceArt.Torch(3), new Vector2(1.6f, 3.2f), 1.15f, 11);
                    SpawnProp(SliceArt.Torch(0), new Vector2(-4.5f, 4.2f), 1.25f, 11);
                    SpawnProp(SliceArt.Torch(1), new Vector2(4.5f, 4.2f), 1.25f, 11);
                    SpawnProp(SliceArt.Torch(2), new Vector2(-4.5f, -4.5f), 1.15f, 11);
                    SpawnProp(SliceArt.Torch(3), new Vector2(4.5f, -4.5f), 1.15f, 11);
                    SpawnProp(SliceArt.Rock(0), new Vector2(-5.2f, 1.2f), 1.35f, 8);
                    SpawnProp(SliceArt.Rock(1), new Vector2(5.2f, 0.5f), 1.4f, 8);
                    SpawnProp(SliceArt.Column(0), new Vector2(-2.8f, 6.5f), 1.35f, 9);
                    SpawnProp(SliceArt.Column(1), new Vector2(2.8f, 6.5f), 1.35f, 9);
                    // Village wick marker
                    SpawnProp(SliceArt.Altar(), new Vector2(0f, -4.8f), 1.5f, 10);
                    break;
                case RoomId.RootPath:
                    SpawnProp(SliceArt.Rock(0), new Vector2(-3f, 0f), 1.2f, 9);
                    SpawnProp(SliceArt.Rock(2), new Vector2(3f, 1.5f), 1.3f, 9);
                    SpawnProp(SliceArt.Torch(2), new Vector2(-3.2f, 4f), 1.1f, 11);
                    break;
                case RoomId.Pressure:
                    SpawnProp(SliceArt.Column(0), new Vector2(-3.2f, 4f), 1.4f, 9);
                    SpawnProp(SliceArt.Column(1), new Vector2(3.2f, 4f), 1.4f, 9);
                    SpawnProp(SliceArt.Torch(3), new Vector2(-3f, -3f), 1.1f, 11);
                    SpawnProp(SliceArt.Torch(4), new Vector2(3f, -3f), 1.1f, 11);
                    break;
                case RoomId.Reliquary:
                    SpawnProp(SliceArt.Column(2), new Vector2(-2.8f, 3.5f), 1.3f, 9);
                    SpawnProp(SliceArt.Column(3), new Vector2(2.8f, 3.5f), 1.3f, 9);
                    SpawnProp(SliceArt.Pot(1), new Vector2(-2f, -1f), 1f, 9);
                    SpawnProp(SliceArt.Pot(2), new Vector2(2f, -1f), 1f, 9);
                    break;
                case RoomId.Boss:
                    SpawnProp(SliceArt.Rock(1), new Vector2(-3f, -2f), 1.4f, 9);
                    SpawnProp(SliceArt.Rock(3), new Vector2(3f, -1.5f), 1.4f, 9);
                    SpawnProp(SliceArt.Torch(0), new Vector2(-3.2f, 5f), 1.3f, 11);
                    SpawnProp(SliceArt.Torch(1), new Vector2(3.2f, 5f), 1.3f, 11);
                    break;
                case RoomId.Altar:
                    SpawnProp(SliceArt.Torch(2), new Vector2(-2.5f, 1f), 1.2f, 11);
                    SpawnProp(SliceArt.Torch(3), new Vector2(2.5f, 1f), 1.2f, 11);
                    SpawnProp(SliceArt.Column(4), new Vector2(-3f, 4f), 1.3f, 9);
                    SpawnProp(SliceArt.Column(0), new Vector2(3f, 4f), 1.3f, 9);
                    break;
            }
        }

        void SpawnProp(Sprite sprite, Vector2 localPos, float scale, int order)
        {
            var go = new GameObject("SP_Decor");
            go.transform.position = (Vector3)(roomCenter + localPos);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            Paint(sr);
            sr.sortingOrder = order;
            // Torches are the only props spawned at order 11 — give each a live ember glow.
            if (order == 11)
                AttachGlow(go.transform, new Vector2(0f, 0.45f / Mathf.Max(0.01f, scale)), 3.1f,
                    new Color(1f, 0.55f, 0.18f, 0.7f), 12, 4.2f, 0.3f);
        }

        static Sprite glowSprite;
        static Sprite GlowSprite()
        {
            if (glowSprite != null) return glowSprite;
            int s = 96;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            float c = (s - 1) * 0.5f;
            var px = new Color[s * s];
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (1.1f - 0.3f * d); // soft, bright core
                px[y * s + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            glowSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            return glowSprite;
        }

        /// <summary>Warm, flickering radial glow sprite — the game's signature ember light (unlit-safe).</summary>
        static void AttachGlow(Transform parent, Vector2 localOffset, float worldRadius,
            Color color, int order, float flickerSpeed = 3.4f, float flickerAmount = 0.24f)
        {
            var glow = new GameObject("SR_Glow");
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = new Vector3(localOffset.x, localOffset.y, 0f);
            // Parent may be scaled; counter it so radius is in world units.
            float inv = 1f / Mathf.Max(0.01f, parent.localScale.x);
            glow.transform.localScale = Vector3.one * (worldRadius * 2f * inv);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = GlowSprite();
            gsr.color = color;
            gsr.sortingOrder = order;
            if (spriteMat != null) gsr.sharedMaterial = spriteMat;
            glow.AddComponent<GlowFlicker>().Init(gsr, color.a, flickerSpeed, flickerAmount);
        }

        void Wall(string name, Vector2 pos, Vector2 size)
        {
            var w = new GameObject(name);
            w.transform.position = (Vector3)(roomCenter + pos);
            var col = w.AddComponent<BoxCollider2D>();
            col.size = size;
            var rb = w.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        void SpawnExitTrigger(Vector2 localPos, System.Action onEnter)
        {
            var go = new GameObject("ST_Exit");
            go.transform.position = (Vector3)(roomCenter + localPos);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.ExitMarker();
            Paint(sr);
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * 1.35f;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 1.2f);
            var t = go.AddComponent<SliceTrigger>();
            t.OnEnter = onEnter;
        }

        void SpawnSlime(Vector2 localPos)
        {
            SpawnEnemy("slime", localPos, 3f, 0.85f);
        }

        void SpawnEnemy(string id, Vector2 localPos, float hp, float scale)
        {
            var e = new GameObject("SE_" + id);
            e.transform.position = (Vector3)(roomCenter + localPos);
            var sr = e.AddComponent<SpriteRenderer>();
            var art = GeneratedArt.EnemyArt(id);
            // Prefer pack sprite for slime/boss when available
            if (id == "slime")
            {
                var pack = SliceArt.Slime();
                if (pack != null) sr.sprite = pack;
                else sr.sprite = art;
            }
            else if (id == "barkling")
            {
                var pack = SliceArt.Boss();
                if (pack != null) sr.sprite = pack;
                else sr.sprite = art;
            }
            else sr.sprite = art;
            Paint(sr);
            sr.sortingOrder = 10;
            e.transform.localScale = Vector3.one * scale;
            var rb = e.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            e.AddComponent<CircleCollider2D>().radius = 0.38f;
            var health = e.AddComponent<Health>();
            health.ConfigureEnemy(hp);
            e.AddComponent<EnemyChaser>();
            var chaser = e.GetComponent<EnemyChaser>();
            float spd = id switch
            {
                "ash_wisp" => 3.4f,
                "root_crawler" => 2.2f,
                "ember_moth" => 3.8f,
                "hollow_knight" => 2.4f,
                "barkling" => 2.1f,
                _ => 2.85f
            };
            float dmg = id == "hollow_knight" || id == "barkling" ? 1f : 1f;
            float atkR = id == "ember_moth" ? 1.25f : 1.55f;
            chaser.Configure(spd, dmg, atkR);
            e.AddComponent<SpriteFlash>();
            e.AddComponent<EnemyHpBar>().Init(health);
            e.AddComponent<EnemyIdentity>().Init(id, Mathf.RoundToInt(hp * 3f));
            GroundShadow.Attach(e.transform, Mathf.Clamp(scale * 0.85f, 0.55f, 1.2f));
        }

        GameObject SpawnPushBlock(Vector2 localPos)
        {
            var b = new GameObject("SP_Block");
            b.transform.position = (Vector3)(roomCenter + localPos);
            var sr = b.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Crate(0);
            Paint(sr);
            sr.sortingOrder = 12;
            b.transform.localScale = Vector3.one * 1.15f;
            var rb = b.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.mass = 2.5f;
            rb.linearDamping = 8f;
            b.AddComponent<BoxCollider2D>();
            return b;
        }

        GameObject SpawnPlate(Vector2 localPos)
        {
            var p = new GameObject("SP_Plate");
            p.transform.position = (Vector3)(roomCenter + localPos);
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Plate();
            Paint(sr);
            sr.sortingOrder = 4;
            p.transform.localScale = Vector3.one * 1.6f;
            p.AddComponent<CircleCollider2D>().isTrigger = true;
            return p;
        }

        GameObject SpawnGate(Vector2 localPos)
        {
            var g = new GameObject("SP_Gate");
            g.transform.position = (Vector3)(roomCenter + localPos);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Gate();
            Paint(sr);
            sr.sortingOrder = 15;
            g.transform.localScale = Vector3.one * 1.8f;
            var col = g.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.6f);
            var rb = g.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            return g;
        }

        void SpawnGloves(Vector2 localPos)
        {
            var g = new GameObject("SP_Gloves");
            g.transform.position = (Vector3)(roomCenter + localPos);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Gloves();
            Paint(sr);
            sr.color = new Color(1f, 0.92f, 0.35f, 1f);
            sr.sortingOrder = 16;
            g.transform.localScale = Vector3.one * 1.5f;
            AttachGlow(g.transform, Vector2.zero, 2.2f,
                new Color(1f, 0.85f, 0.4f, 0.6f), 15, 1.8f, 0.16f);
            var col = g.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;
            var t = g.AddComponent<SliceTrigger>();
            t.OnEnter = () =>
            {
                Destroy(g);
                OnGlovesTaken();
            };
        }

        void SpawnBarkling(Vector2 localPos)
        {
            var e = new GameObject("SB_Barkling");
            e.transform.position = (Vector3)(roomCenter + localPos);
            var sr = e.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Boss() != null ? SliceArt.Boss() : GeneratedArt.EnemyArt("barkling");
            Paint(sr);
            sr.sortingOrder = 14;
            e.transform.localScale = Vector3.one * 1.15f;
            var rb = e.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            e.AddComponent<CircleCollider2D>().radius = 0.55f;
            var hp = e.AddComponent<Health>();
            hp.ConfigureEnemy(12f);
            e.AddComponent<EnemyChaser>();
            e.AddComponent<SpriteFlash>();
            e.AddComponent<EnemyHpBar>().Init(hp);
            e.AddComponent<EnemyIdentity>().Init("barkling", 40);
            e.AddComponent<BarklingBoss>().Init(OnBossDefeated);
        }

        void SpawnAltar(Vector2 localPos)
        {
            var a = new GameObject("SP_Altar");
            a.transform.position = (Vector3)(roomCenter + localPos);
            var sr = a.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Altar();
            Paint(sr);
            sr.color = new Color(1f, 0.75f, 0.35f, 1f);
            sr.sortingOrder = 16;
            a.transform.localScale = Vector3.one * 1.9f;
            AttachGlow(a.transform, new Vector2(0f, 0.2f), 4.6f,
                new Color(1f, 0.68f, 0.28f, 0.62f), 15, 2.4f, 0.2f);
            var col = a.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            var t = a.AddComponent<SliceTrigger>();
            t.OnEnter = OnAltarTouched;
        }

        void SpawnSign(Vector2 localPos, string _)
        {
            var s = new GameObject("SP_Sign");
            s.transform.position = (Vector3)(roomCenter + localPos);
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = SliceArt.Barrel(2);
            Paint(sr);
            sr.sortingOrder = 6;
            s.transform.localScale = Vector3.one * 1.15f;
        }

        static Sprite Pixel(Color c)
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[64];
            for (int i = 0; i < 64; i++) px[i] = c;
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        }
    }

    public class SliceTrigger : MonoBehaviour
    {
        public System.Action OnEnter;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null && !other.CompareTag("Player")) return;
            OnEnter?.Invoke();
        }
    }

    public class BarklingBoss : MonoBehaviour
    {
        System.Action onDead;
        Health health;
        float slamTimer = 2.5f;
        bool dead;

        public void Init(System.Action cb) => onDead = cb;

        void Awake()
        {
            health = GetComponent<Health>();
            if (health != null) health.OnDied += HandleDead;
        }

        void Update()
        {
            slamTimer -= Time.deltaTime;
            if (slamTimer > 0f) return;
            slamTimer = 2.8f;
            // telegraph: flash + AoE damage near boss
            var player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;
            if (Vector2.Distance(transform.position, player.transform.position) < 1.6f)
            {
                var h = player.GetComponent<Health>();
                h?.TakeDamage(1f, transform.position);
            }
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }

        void HandleDead()
        {
            if (dead) return;
            dead = true;
            onDead?.Invoke();
        }
    }
}
