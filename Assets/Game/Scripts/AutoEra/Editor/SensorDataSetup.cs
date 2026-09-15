using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace AutoEra.Editor
{
    public static class SensorDataSetup
    {
        [MenuItem("Game Framework/AutoEra/Generate Sensor Data")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UGF.EditorTools.AIGameDataTableGenerator", false)).Single(t => t != null);
            var import = type.GetMethod("ImportDataTablesFromAIJson");
            string[] paths = { Path.GetFullPath("GameData/AIData/DataTables/Sensors/SensorDefinitions.json"),
                Path.GetFullPath("GameData/AIData/DataTables/Machines/ComponentDefinitions.json") };
            Check(import.Invoke(null, new object[] { paths, false, false }), paths.Length);
            var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
            if (asset == null) throw new InvalidOperationException("AppConfigs missing.");
            var settings = new SerializedObject(asset);
            var list = settings.FindProperty("mDataTables");
            if (list == null || !list.isArray) throw new InvalidOperationException("DataTable registry missing.");
            const string logical = "Sensors/SensorDefinitions";
            bool registered = false;
            for (int i = 0; i < list.arraySize; i++) registered |= list.GetArrayElementAtIndex(i).stringValue == logical;
            int originalSize = list.arraySize;
            try
            {
                if (!registered)
                {
                    Undo.RecordObject(asset, "Register sensor table");
                    list.InsertArrayElementAtIndex(originalSize);
                    list.GetArrayElementAtIndex(originalSize).stringValue = logical;
                    settings.ApplyModifiedProperties();
                }
                // Existing pipeline owns logical fingerprint checks, workbook backup and generation.
                Check(import.Invoke(null, new object[] { paths, true, true }), paths.Length);
                AssetDatabase.SaveAssetIfDirty(asset);
                AssetDatabase.Refresh();
            }
            catch
            {
                if (!registered)
                {
                    settings.Update(); settings.FindProperty("mDataTables").arraySize = originalSize;
                    settings.ApplyModifiedPropertiesWithoutUndo();
                }
                throw;
            }
        }
        private static void Check(object report, int count)
        {
            var json = JObject.FromObject(report);
            if ((int?)json["failureCount"] != 0 || (int?)json["successCount"] != count)
                throw new InvalidOperationException(json.ToString());
            UnityEngine.Debug.Log("[SensorDataSetup] " + json);
        }
    }
}
