using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class AutoEraFoundationDataSetup
    {
        [MenuItem("Game Framework/AutoEra/Generate Foundation Startup Data")]
        public static void GenerateStartupData()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            Generate();
            object[] language = { Path.GetFullPath("GameData/AIData/Languages/Foundation/Startup.json"), null };
            if (!(bool)FindEditorType("UGF.EditorTools.AILanguageAdapter").GetMethod("TryReverseJsonToExcel").Invoke(null, language))
                throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)language[1]));
            var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
            var settings = new SerializedObject(asset);
            AddValue(settings.FindProperty("mDataTables"), "Foundation/StartupMessages");
            AddValue(settings.FindProperty("mLanguages"), "Foundation/Startup");
            settings.ApplyModifiedPropertiesWithoutUndo();
            var method = FindEditorType("UGF.EditorTools.AIGameDataTableGenerator").GetMethod("ImportDataTablesFromAIJson");
            string[] paths = { Path.GetFullPath("GameData/AIData/DataTables/Foundation/StartupMessages.json") };
            CheckReport(method.Invoke(null, new object[] { paths, false, false }));
            CheckReport(method.Invoke(null, new object[] { paths, true, true }));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AddValue(SerializedProperty list, string value)
        {
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).stringValue == value) return;
            int index = list.arraySize;
            list.InsertArrayElementAtIndex(index);
            list.GetArrayElementAtIndex(index).stringValue = value;
        }

        [MenuItem("Game Framework/AutoEra/Regenerate Language Table")]
        public static void RegenerateLanguageTable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            FindEditorType("UGF.EditorTools.GameDataGenerator").GetMethod("RefreshAllDataTable")
                .Invoke(null, new object[] { new[] { Path.GetFullPath("GameData/DataTables/Core/LanguagesTable.xlsx") } });
        }
        [MenuItem("Game Framework/AutoEra/Export Startup UI Baseline")]
        public static void ExportStartupUiBaseline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            CheckReport(FindEditorType("UGF.EditorTools.AIGameDataTableGenerator").GetMethod("ExportDataTablesToAIJson")
                .Invoke(null, new object[] { new[] { Path.GetFullPath("GameData/DataTables/Core/UITable.xlsx") } }));
        }

        [MenuItem("Game Framework/AutoEra/Generate Startup UI Table")]
        public static void GenerateStartupUiTable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            var method = FindEditorType("UGF.EditorTools.AIGameDataTableGenerator").GetMethod("ImportDataTablesFromAIJson");
            string[] paths = { Path.GetFullPath("GameData/AIData/DataTables/Core/UITable.json") };
            CheckReport(method.Invoke(null, new object[] { paths, false, false }));
            CheckReport(method.Invoke(null, new object[] { paths, true, true }));
            AssetDatabase.Refresh();
        }

        private static void CheckReport(object report)
        {
            var json = JObject.FromObject(report);
            if ((int?)json["successCount"] != 1 || (int?)json["failureCount"] != 0)
                throw new InvalidOperationException(json.ToString());
            Debug.Log("[AutoEra][Data] " + json.ToString(Newtonsoft.Json.Formatting.None));
        }

        [MenuItem("Game Framework/AutoEra/Generate Foundation Runtime Config")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            const string json = "GameData/AIData/Configs/Foundation/Runtime.json";
            Type adapter = FindEditorType("UGF.EditorTools.AIConfigAdapter");
            string content = File.ReadAllText(json);
            object[] validate = { content, null, null };
            if (!(bool)adapter.GetMethod("TryParseManifest").Invoke(null, validate))
                throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)validate[2]));
            var values = JObject.Parse(content)["entries"].ToDictionary(e => (string)e["key"], e => (string)e["value"]);
            Application.AutoEraRuntimeSettings.Load(key => values.TryGetValue(key, out string value) ? value : null);
            object[] reverse = { Path.GetFullPath(json), null };
            if (!(bool)adapter.GetMethod("TryReverseJsonToExcel").Invoke(null, reverse))
                throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)reverse[1]));
            // Reverse now commits the exact runtime outputs together with workbook and JSON.
            AssetDatabase.Refresh();
            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
            if (asset == null) throw new InvalidOperationException("AppConfigs missing.");
            var settings = new SerializedObject(asset);
            SerializedProperty configs = settings.FindProperty("mConfigs");
            bool found = false;
            for (int i = 0; i < configs.arraySize; i++) found |= configs.GetArrayElementAtIndex(i).stringValue == "Foundation/Runtime";
            if (!found)
            {
                int index = configs.arraySize;
                configs.InsertArrayElementAtIndex(index);
                configs.GetArrayElementAtIndex(index).stringValue = "Foundation/Runtime";
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
        }

        // The product assembly intentionally does not reference Builtin.Editor.
        private static Type FindEditorType(string name)
            => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).Single(t => t != null);
    }
}
