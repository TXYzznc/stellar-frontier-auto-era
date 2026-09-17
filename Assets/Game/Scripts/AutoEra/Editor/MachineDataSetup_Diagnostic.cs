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
    public static class MachineDataSetup_Diagnostic
    {
        [MenuItem("Game Framework/AutoEra/Generate Machine Data (Diagnostic)")]
        public static void GenerateDiagnostic()
        {
            Debug.Log("[P0011-Diag] Starting diagnostic generation...");
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
                
                Debug.Log("[P0011-Diag] Step 1: Finding generator type...");
                Type generator = Find("UGF.EditorTools.AIGameDataTableGenerator");
                MethodInfo import = generator.GetMethod("ImportDataTablesFromAIJson");
                Debug.Log($"[P0011-Diag] Generator found: {generator.FullName}");
                
                string[] logical = { 
                    "Machines/MachineDefinitions", 
                    "Machines/ComponentDefinitions", 
                    "Catalog/FirstVersionObjects",
                    "Buildings/BuildingDefinitions",
                    "ResourcePoints/ResourcePointDefinitions"
                };
                string[] paths = logical.Select(p => Path.GetFullPath("GameData/AIData/DataTables/" + p + ".json")).ToArray();
                Debug.Log($"[P0011-Diag] Step 2: Dry-run validation for {paths.Length} tables...");
                foreach (var p in paths) Debug.Log($"[P0011-Diag]   - {p}");
                
                var dryRunResult = import.Invoke(null, new object[] { paths, false, false });
                Debug.Log($"[P0011-Diag] Dry-run result: {JObject.FromObject(dryRunResult).ToString()}");
                MachineDataSetup.Check(dryRunResult, paths.Length);
                Debug.Log("[P0011-Diag] Dry-run passed.");
                
                Debug.Log("[P0011-Diag] Step 3: Loading AppConfigs...");
                var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
                if (asset == null) throw new InvalidOperationException("AppConfigs missing.");
                var settings = new SerializedObject(asset);
                var list = settings.FindProperty("mDataTables");
                string[] original = Enumerable.Range(0, list.arraySize).Select(i => list.GetArrayElementAtIndex(i).stringValue).ToArray();
                Debug.Log($"[P0011-Diag] Original mDataTables count: {original.Length}");
                
                try
                {
                    Debug.Log("[P0011-Diag] Step 4: Registering tables in AppConfigs...");
                    foreach (string value in logical)
                    {
                        MachineDataSetup.Add(list, value);
                        Debug.Log($"[P0011-Diag]   Added: {value}");
                    }
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log($"[P0011-Diag] New mDataTables count: {list.arraySize}");
                    
                    Debug.Log("[P0011-Diag] Step 5: Actual generation (dryRun=false, generateCode=true)...");
                    var generateResult = import.Invoke(null, new object[] { paths, true, true });
                    Debug.Log($"[P0011-Diag] Generation result: {JObject.FromObject(generateResult).ToString()}");
                    MachineDataSetup.Check(generateResult, paths.Length);
                    Debug.Log("[P0011-Diag] Generation passed.");
                    
                    Debug.Log("[P0011-Diag] Step 6: Language file conversion...");
                    object[] language = { Path.GetFullPath("GameData/AIData/Languages/Machines/Hardware.json"), null };
                    Debug.Log($"[P0011-Diag] Language path: {language[0]}");
                    if (!(bool)Find("UGF.EditorTools.AILanguageAdapter").GetMethod("TryReverseJsonToExcel").Invoke(null, language))
                    {
                        Debug.LogError($"[P0011-Diag] Language conversion failed: {string.Join("; ", (IEnumerable<string>)language[1])}");
                        throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)language[1]));
                    }
                    Debug.Log("[P0011-Diag] Language conversion passed.");
                    
                    Debug.Log("[P0011-Diag] Step 7: Registering language...");
                    settings.Update();
                    MachineDataSetup.Add(settings.FindProperty("mLanguages"), "Machines/Hardware");
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    
                    Debug.Log("[P0011-Diag] Step 8: Saving assets...");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("[P0011-Diag] ✓ Generation completed successfully!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[P0011-Diag] Exception during generation: {ex.GetType().Name}: {ex.Message}");
                    Debug.LogError($"[P0011-Diag] Stack trace: {ex.StackTrace}");
                    Debug.Log("[P0011-Diag] Rolling back AppConfigs...");
                    settings.Update(); 
                    list = settings.FindProperty("mDataTables"); 
                    list.arraySize = original.Length;
                    for (int i = 0; i < original.Length; i++) list.GetArrayElementAtIndex(i).stringValue = original[i];
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[P0011-Diag] Rollback complete.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[P0011-Diag] FATAL: {ex.GetType().Name}: {ex.Message}");
                Debug.LogError($"[P0011-Diag] Stack: {ex.StackTrace}");
            }
        }

        private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).Single(t => t != null);
    }
}
