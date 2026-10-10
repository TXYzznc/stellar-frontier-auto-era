using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AutoEra.Tests.EnergyBench
{
    public static class EnergyBenchmarkBuild
    {
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Energy domain benchmark").AddComponent<EnergyBenchmarkRunner>();
            EditorSceneManager.SaveScene(scene, "Assets/Benchmark.unity");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Benchmark.unity" },
                locationPathName = Path.GetFullPath("Build/EnergyBenchmark.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            File.WriteAllText("Build/build-summary.json", JsonUtility.ToJson(new Summary
            { result = report.summary.result.ToString(), errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings, bytes = report.summary.totalSize,
                seconds = report.summary.totalTime.TotalSeconds }, true));
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Benchmark Player build failed.");
        }
        [Serializable] private sealed class Summary
        { public string result; public int errors, warnings; public ulong bytes; public double seconds; }
    }
}
