using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EuroHaul.Editor
{
    /// <summary>Build entry point used by GameCI after selecting the target platform.</summary>
    public static class EuroHaulBuild
    {
        public static void Build()
        {
            EuroHaulSceneBootstrap.EnsureScene();
            EditorBuildSettingsScene[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .ToArray();
            if (enabledScenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes. Truck Lab scene bootstrap did not register a scene.");

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string outputDirectory;
            string outputPath;
            if (target == BuildTarget.Android)
            {
                outputDirectory = "build/Android";
                outputPath = Path.Combine(outputDirectory, "EURO-HAUL.apk");
            }
            else if (target == BuildTarget.StandaloneWindows64)
            {
                outputDirectory = "build/StandaloneWindows64";
                outputPath = Path.Combine(outputDirectory, "EURO-HAUL.exe");
            }
            else
            {
                throw new NotSupportedException("Unsupported EURO HAUL build target: " + target);
            }

            Directory.CreateDirectory(outputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes.Select(scene => scene.path).ToArray(),
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Player build failed: " + report.summary.result);
            Debug.Log("EURO HAUL build completed: " + report.summary.outputPath);
        }
    }
}