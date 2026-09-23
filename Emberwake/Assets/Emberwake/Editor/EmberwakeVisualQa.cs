using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Emberwake.Editor
{
    /// <summary>Editor visual QA: enter playmode, auto-advance, dump screenshots.</summary>
    public static class EmberwakeVisualQa
    {
        const string ScenePath = "Assets/Emberwake/Scenes/Millbrook_Prototype.unity";
        const string OutDir = @"C:\g\out\qa";

        [MenuItem("Emberwake/6. Run Editor Visual QA")]
        public static void Run()
        {
            if (!Directory.Exists(OutDir)) Directory.CreateDirectory(OutDir);
            if (!File.Exists(ScenePath))
                EmberwakeSetupMenu.CreatePrototypeScene();
            EditorSceneManager.OpenScene(ScenePath);
            if (Object.FindFirstObjectByType<VerticalSliceDirector>() == null)
            {
                var root = new GameObject("VerticalSlice");
                root.AddComponent<VerticalSliceDirector>();
            }
            EditorApplication.isPlaying = true;
            EditorApplication.update += Tick;
            frame = 0;
            Debug.Log("[EmberwakeQA] Editor play visual QA started");
        }

        static int frame;
        static bool shotSplash, shotGame, shotMenu;

        static void Tick()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= Tick;
                return;
            }
            frame++;
            // Force QA path
            if (frame == 5)
                Debug.Log("[EmberwakeQA] playing frame=" + frame);

            if (!shotSplash && frame > 30)
            {
                Shot("editor_01_early.png");
                shotSplash = true;
            }
            if (!shotGame && frame > 120)
            {
                // skip dialogs
                var dlg = Object.FindFirstObjectByType<DialogBox>();
                if (dlg != null && dlg.IsOpen)
                {
                    var go = GameObject.Find("DialogBox");
                    if (go != null)
                    {
                        var ped = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                        UnityEngine.EventSystems.ExecuteEvents.Execute(go, ped, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
                    }
                }
            }
            if (!shotGame && frame > 200)
            {
                Shot("editor_02_gameplay.png");
                shotGame = true;
                GameMenuHub.Instance?.Open();
            }
            if (!shotMenu && frame > 260)
            {
                Shot("editor_03_menu.png");
                shotMenu = true;
                EditorApplication.isPlaying = false;
                EditorApplication.update -= Tick;
                Debug.Log("[EmberwakeQA] Editor visual QA complete → " + OutDir);
            }
        }

        static void Shot(string name)
        {
            string path = Path.Combine(OutDir, name);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[EmberwakeQA] shot " + path);
        }
    }
}
