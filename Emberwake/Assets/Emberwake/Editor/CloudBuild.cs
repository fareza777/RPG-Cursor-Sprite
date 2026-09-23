using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Emberwake.Editor
{
    // TEMPORARY cloud-agent helper for headless Linux builds. Not part of the game.
    public static class CloudBuild
    {
        const string ScenePath = "Assets/Emberwake/Scenes/Millbrook_Prototype.unity";

        public static void BuildLinux()
        {
            string outPath = Environment.GetEnvironmentVariable("EMBERWAKE_OUT")
                             ?? "/opt/unity/out/EmberwakeLinux/Emberwake.x86_64";
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

            if (!File.Exists(ScenePath))
                EmberwakeSetupMenu.CreatePrototypeScene();

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outPath,
                target = BuildTarget.StandaloneLinux64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;
            Debug.Log($"[CloudBuild] result={s.result} size={s.totalSize} errors={s.totalErrors} out={Path.GetFullPath(outPath)}");
            if (s.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
            EditorApplication.Exit(0);
        }
    }
}
