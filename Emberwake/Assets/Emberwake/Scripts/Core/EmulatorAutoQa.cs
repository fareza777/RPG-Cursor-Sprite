using UnityEngine;

namespace Emberwake
{
    /// <summary>Emulator/dev helper: skip boot after splash, auto-tap dialogs, dump markers.</summary>
    public class EmulatorAutoQa : MonoBehaviour
    {
        public static bool Enabled { get; private set; }

        float t;
        int phase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            // Enable on emulator / editor / -qa flag
            bool emu = SystemInfo.deviceModel.ToLowerInvariant().Contains("sdk")
                       || SystemInfo.deviceName.ToLowerInvariant().Contains("emulator")
                       || Application.isEditor
                       || SystemInfo.graphicsDeviceName.ToLowerInvariant().Contains("android emulator")
                       || HasArg("-emberwakeQa");
            if (!emu) return;
            Enabled = true;
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
            t += Time.unscaledDeltaTime;

            // Auto-advance dialogs
            if (DialogBox.Instance != null && DialogBox.Instance.IsOpen && t > 0.35f)
            {
                // Simulate tap via reflection of public flow — tap flag private; use SendMessage / click
                SimulateDialogTap();
                t = 0f;
            }

            // Open menu briefly for screenshot phase
            if (phase == 0 && PortraitMobileHud.Instance != null && PortraitMobileHud.Instance.GameStarted && t > 1.2f)
            {
                phase = 1;
                t = 0f;
                Debug.Log("[EmberwakeQA] gameplay_ready");
            }
            if (phase == 1 && t > 2f)
            {
                GameMenuHub.Instance?.Open();
                phase = 2;
                t = 0f;
                Debug.Log("[EmberwakeQA] menu_open");
            }
            if (phase == 2 && t > 2f)
            {
                GameMenuHub.Instance?.Close();
                phase = 3;
                Debug.Log("[EmberwakeQA] menu_closed");
            }
        }

        void SimulateDialogTap()
        {
            // Dialog advances on PointerDown; inject via EventSystem if possible
            if (UnityEngine.EventSystems.EventSystem.current == null) return;
            var go = DialogBox.Instance != null ? GameObject.Find("DialogBox") : null;
            if (go == null) return;
            var ped = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            UnityEngine.EventSystems.ExecuteEvents.Execute(go, ped, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
        }
    }
}
