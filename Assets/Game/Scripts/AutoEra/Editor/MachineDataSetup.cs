using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class MachineDataSetup
    {
        [MenuItem("Game Framework/AutoEra/Generate Machine Data")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            Type generator = Find("UGF.EditorTools.AIGameDataTableGenerator");
            MethodInfo import = generator.GetMethod("ImportDataTablesFromAIJson");
            string[] logical = { "Machines/MachineDefinitions", "Machines/ComponentDefinitions", "Catalog/FirstVersionObjects" };
            string[] paths = logical.Select(p => Path.GetFullPath("GameData/AIData/DataTables/" + p + ".json")).ToArray();
            Check(import.Invoke(null, new object[] { paths, false, false }), paths.Length);
            var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
            if (asset == null) throw new InvalidOperationException("AppConfigs missing.");
            var settings = new SerializedObject(asset);
            var list = settings.FindProperty("mDataTables");
            string[] original = Enumerable.Range(0, list.arraySize).Select(i => list.GetArrayElementAtIndex(i).stringValue).ToArray();
            try
            {
                foreach (string value in logical) Add(list, value);
                settings.ApplyModifiedPropertiesWithoutUndo();
                Check(import.Invoke(null, new object[] { paths, true, true }), paths.Length);
                object[] language = { Path.GetFullPath("GameData/AIData/Languages/Machines/Hardware.json"), null };
                if (!(bool)Find("UGF.EditorTools.AILanguageAdapter").GetMethod("TryReverseJsonToExcel").Invoke(null, language))
                    throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)language[1]));
                settings.Update();
                Add(settings.FindProperty("mLanguages"), "Machines/Hardware");
                settings.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch
            {
                settings.Update(); list = settings.FindProperty("mDataTables"); list.arraySize = original.Length;
                for (int i = 0; i < original.Length; i++) list.GetArrayElementAtIndex(i).stringValue = original[i];
                settings.ApplyModifiedPropertiesWithoutUndo();
                throw;
            }
        }

        private static void Add(SerializedProperty list, string value)
        {
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).stringValue == value) return;
            int index = list.arraySize; list.InsertArrayElementAtIndex(index); list.GetArrayElementAtIndex(index).stringValue = value;
        }
        private static void Check(object report, int count)
        {
            var json = JObject.FromObject(report);
            if ((int?)json["failureCount"] != 0 || (int?)json["successCount"] != count) throw new InvalidOperationException(json.ToString());
        }
        private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).Single(t => t != null);
    }
}
