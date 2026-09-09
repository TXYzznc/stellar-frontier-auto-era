#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    /// <summary>Runs the UIForm PlayMode test through Unity's native Test Runner and reads its reload-safe XML result.</summary>
    internal static class AutoEraOperationsUiFormNativeTestRunner
    {
        private const string TestFullName = "AutoEra.Tests.PlayMode.AutoEraOperationsUiFormPlayModeTests.OperationsForms_OpenAndCloseThroughUiExtension";
        private const string ResultFileName = "operations-uiform-playmode.xml";

        [MenuItem("Game Framework/AutoEra/QA/Run Operations UIForm PlayMode Test")]
        private static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before starting the UIForm PlayMode regression.");
            }

            string resultPath = GetResultPath();
            if (File.Exists(resultPath))
            {
                File.Delete(resultPath);
            }

            Type filterType = FindLoadedType("UnityEditor.TestTools.TestRunner.Api.Filter");
            Type executionSettingsType = FindLoadedType("UnityEditor.TestTools.TestRunner.Api.ExecutionSettings");
            Type testRunnerApiType = FindLoadedType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi");
            Type testModeType = FindLoadedType("UnityEditor.TestTools.TestRunner.Api.TestMode");
            if (filterType == null || executionSettingsType == null || testRunnerApiType == null || testModeType == null)
            {
                throw new InvalidOperationException("Unity Test Framework editor API is not loaded.");
            }

            object filter = Activator.CreateInstance(filterType);
            filterType.GetField("testMode").SetValue(filter, Enum.Parse(testModeType, "PlayMode"));
            filterType.GetField("testNames").SetValue(filter, new[] { TestFullName });

            Array filters = Array.CreateInstance(filterType, 1);
            filters.SetValue(filter, 0);
            object settings = Activator.CreateInstance(executionSettingsType, new object[] { filters });
            ScriptableObject api = ScriptableObject.CreateInstance(testRunnerApiType);
            testRunnerApiType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance, null, new[] { executionSettingsType }, null)
                .Invoke(api, new[] { settings });
        }

        [MenuItem("Game Framework/AutoEra/QA/Read Operations UIForm PlayMode Result")]
        private static void ReadResult()
        {
            string resultPath = GetResultPath();
            if (!File.Exists(resultPath))
            {
                Debug.LogWarning($"[AutoEra][QA] No native UIForm PlayMode result is available at '{resultPath}'.");
                return;
            }

            XmlDocument document = new XmlDocument();
            document.Load(resultPath);
            XmlElement result = document.DocumentElement;
            Debug.Log($"[AutoEra][QA] Native UIForm PlayMode result: {result.GetAttribute("result")}; passed={result.GetAttribute("passed")}; failed={result.GetAttribute("failed")}; skipped={result.GetAttribute("skipped")}; duration={result.GetAttribute("duration")}s.");
        }

        private static string GetResultPath()
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            return Path.Combine(projectRoot, "Temp", "AutoEraTestResults", ResultFileName);
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
#endif
