using UnityEngine;

namespace Emberwake
{
    /// <summary>
    /// Emulator/dev/QA autopilot: skips boot, auto-advances dialogs, and drives Kael
    /// (walk + attack) so headless visual QA can traverse and fight through the slice.
    /// Only ever active on emulator / editor / when launched with -emberwakeQa.
    /// </summary>
    public class EmulatorAutoQa : MonoBehaviour
    {
        public static bool Enabled { get; private set; }

        /// <summary>QA-only: skip cosmetic scatter/glows so the headless software
        /// renderer can produce many frames (for verifying controls/combat/rooms).</summary>
        public static bool Lite { get; private set; }

        float dialogTapTimer;
        float attackTimer;
        float spinTimer;
        float wiggleTimer;
        Vector2 moveDir = Vector2.up;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            bool emu = SystemInfo.deviceModel.ToLowerInvariant().Contains("sdk")
                       || SystemInfo.deviceName.ToLowerInvariant().Contains("emulator")
                       || Application.isEditor
                       || SystemInfo.graphicsDeviceName.ToLowerInvariant().Contains("android emulator")
                       || HasArg("-emberwakeQa");
            if (!emu) return;
            Enabled = true;
            Lite = HasArg("-emberwakeLite");
            var go = new GameObject("EmulatorAutoQa");
            DontDestroyOnLoad(go);
            go.AddComponent<EmulatorAutoQa>();
            Debug.Log("[Emberwake] EmulatorAutoQa ON");
        }

        static bool HasArg(string a)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == a) return true;
            return false;
        }

        void Update()
        {
            if (!Enabled) return;
            float dt = Time.unscaledDeltaTime;
            if (HasArg("-emberwakeHub")) return; // hub opened by boot flow; stay idle

            // 1) Auto-advance any open dialog.
            if (DialogBox.Instance != null && DialogBox.Instance.IsOpen)
            {
                dialogTapTimer += dt;
                if (dialogTapTimer > 0.5f)
                {
                    DialogBox.Instance.ForceAdvance();
                    dialogTapTimer = 0f;
                }
                return;
            }
            dialogTapTimer = 0f;

            // 2) Once the HUD reports gameplay, drive Kael: walk (mostly north) + attack.
            var hud = PortraitMobileHud.Instance;
            if (hud == null || !hud.GameStarted) return;

            // Headless software rendering is ~1 fps; since this autopilot acts once per
            // rendered frame, speed up game-time so QA can traverse the whole slice.
            Time.timeScale = 3f;

            // Snake slightly left/right so we brush enemies and props while heading north.
            wiggleTimer += dt;
            float wobble = Mathf.Sin(wiggleTimer * 1.3f) * 0.55f;
            moveDir = new Vector2(wobble, 1f).normalized;
            GameInput.SetMobileMove(moveDir);

            attackTimer += dt;
            if (attackTimer > 0.55f)
            {
                GameInput.PressAttack();
                attackTimer = 0f;
            }
            spinTimer += dt;
            if (spinTimer > 2.6f)
            {
                GameInput.PressSpin();
                spinTimer = 0f;
            }
        }
    }
}
