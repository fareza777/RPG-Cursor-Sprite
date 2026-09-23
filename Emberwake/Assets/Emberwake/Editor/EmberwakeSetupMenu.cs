using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Emberwake.Editor
{
    public static class EmberwakeSetupMenu
    {
        const string ScenePath = "Assets/Emberwake/Scenes/Millbrook_Prototype.unity";

        [MenuItem("Emberwake/1. Create Portrait Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Emberwake/Scenes"))
                AssetDatabase.CreateFolder("Assets/Emberwake", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("VerticalSlice");
            bootstrap.AddComponent<VerticalSliceDirector>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Emberwake] Created {ScenePath}. Press Play — portrait controls + Kael yard spawn automatically.");
        }

        [MenuItem("Emberwake/2. Apply Play Store Portrait Settings")]
        public static void ApplyPortraitSettings()
        {
            PlayerSettings.companyName = "Emberwake Studio";
            PlayerSettings.productName = "Emberwake";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.emberwakestudio.emberwake");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            // Use installed platform (34/36). 35 is missing and SDK folder is read-only.
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            GenerateAndAssignAppIcon();
            Debug.Log("[Emberwake] Portrait Play Store player settings applied.");
        }

        [MenuItem("Emberwake/5. Generate App Icon")]
        public static void GenerateAndAssignAppIcon()
        {
            const string dir = "Assets/Emberwake/Art";
            if (!AssetDatabase.IsValidFolder("Assets/Emberwake/Art"))
                AssetDatabase.CreateFolder("Assets/Emberwake", "Art");
            string path = dir + "/AppIcon.png";

            int s = 512;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            float cx = s * 0.5f, cy = s * 0.52f;
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x - cx) / s, dy = (y - cy) / s;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color c = new Color(0.08f, 0.06f, 0.1f, 1f);
                float glow = Mathf.Exp(-d * 4.2f);
                c = Color.Lerp(c, new Color(0.55f, 0.22f, 0.05f), glow * 0.85f);
                float fx = (x - cx) / (s * 0.12f);
                float fy = (y - cy - s * 0.02f) / (s * 0.22f);
                float flame = fx * fx + fy * fy;
                if (flame < 1f)
                {
                    float t = 1f - flame;
                    c = Color.Lerp(new Color(0.95f, 0.35f, 0.05f), new Color(1f, 0.92f, 0.45f), t);
                }
                float ring = Mathf.Abs(d - 0.28f);
                if (ring < 0.035f) c = Color.Lerp(c, new Color(1f, 0.85f, 0.4f), 1f - ring / 0.035f);
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (icon != null)
            {
                var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Android);
                if (icons == null || icons.Length == 0)
                    icons = new Texture2D[1];
                icons[0] = icon;
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon });
            }
            Debug.Log("[Emberwake] App icon generated → " + path);
        }

        [MenuItem("Emberwake/3. Open Prototype Scene")]
        public static void OpenPrototype()
        {
            if (!System.IO.File.Exists(ScenePath))
                CreatePrototypeScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Emberwake/4b. Build Android APK (Emulator x86_64)")]
        public static void BuildAndroidApkEmulator()
        {
            ApplyPortraitSettings();

            string sdkMirror = @"C:\Pixel Game Cursor\AndroidSDK";
            string openJdk = @"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK";
            string ndk = @"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK";
            if (System.IO.Directory.Exists(sdkMirror))
                EditorPrefs.SetString("AndroidSdkRoot", sdkMirror);
            if (System.IO.Directory.Exists(openJdk))
                EditorPrefs.SetString("JdkPath", openJdk);
            if (System.IO.Directory.Exists(ndk))
                EditorPrefs.SetString("AndroidNdkRoot", ndk);

            PlayerSettings.stripEngineCode = false;
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2
            });

            if (!System.IO.File.Exists(ScenePath))
                CreatePrototypeScene();
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("VerticalSlice");
                root.AddComponent<VerticalSliceDirector>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            AddToBuildSettings(ScenePath);
            PreparePaintedTextures();
            AssetDatabase.Refresh();

            const string outDir = @"C:\g\out";
            if (!System.IO.Directory.Exists(outDir))
                System.IO.Directory.CreateDirectory(outDir);
            string apkPath = System.IO.Path.Combine(outDir, "Emberwake-Emu.apk");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;
            // Dual ABI so emulator (x86_64) and phones (arm64) both work
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
            Debug.Log("[Emberwake] targetArchitectures=" + PlayerSettings.Android.targetArchitectures);

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[Emberwake] EMU APK OK → {System.IO.Path.GetFullPath(apkPath)} size={new System.IO.FileInfo(apkPath).Length}");
            else
                Debug.LogError($"[Emberwake] EMU APK FAILED → {report.summary.result}");

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        [MenuItem("Emberwake/4. Build Android APK (Slice Tes)")]
        public static void BuildAndroidApk()
        {
            ApplyPortraitSettings();
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Point Unity at writable SDK mirror (licenses accepted; Program Files SDK is read-only)
            string sdkMirror = @"C:\Pixel Game Cursor\AndroidSDK";
            string openJdk = @"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK";
            string ndk = @"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK";
            if (System.IO.Directory.Exists(sdkMirror))
                EditorPrefs.SetString("AndroidSdkRoot", sdkMirror);
            if (System.IO.Directory.Exists(openJdk))
                EditorPrefs.SetString("JdkPath", openJdk);
            if (System.IO.Directory.Exists(ndk))
                EditorPrefs.SetString("AndroidNdkRoot", ndk);

            // GLES3 first — GameActivity+Vulkan black-screened on Xiaomi Adreno 610
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
            {
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan
            });
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;

            if (!System.IO.File.Exists(ScenePath))
                CreatePrototypeScene();
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("VerticalSlice");
                root.AddComponent<VerticalSliceDirector>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            AddToBuildSettings(ScenePath);
            PreparePaintedTextures();
            AssetDatabase.Refresh();

            const string outDir = @"C:\g\out";
            if (!System.IO.Directory.Exists(outDir))
                System.IO.Directory.CreateDirectory(outDir);

            string apkPath = System.IO.Path.Combine(outDir, "Emberwake-Slice.apk");

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                // No Development — Profiler.Dispatcher SIGILL on x86 emulator translating arm64
                options = BuildOptions.CompressWithLz4HC
            };

            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[Emberwake] APK OK → {System.IO.Path.GetFullPath(apkPath)} size={new System.IO.FileInfo(apkPath).Length}");
            else
                Debug.LogError($"[Emberwake] APK FAILED: {report.summary.result}");
        }

        static void PreparePaintedTextures()
        {
            const string dir = "Assets/Emberwake/Resources/Painted";
            if (!AssetDatabase.IsValidFolder("Assets/Emberwake/Resources"))
                AssetDatabase.CreateFolder("Assets/Emberwake", "Resources");
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets/Emberwake/Resources", "Painted");
            AssetDatabase.Refresh();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter imp) continue;
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Bilinear;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.alphaIsTransparency = true;
                imp.maxTextureSize = 2048;
                imp.isReadable = true;
                imp.SaveAndReimport();
            }
        }

        static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
                if (s.path == path) return;

            var list = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++) list[i] = scenes[i];
            list[scenes.Length] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
