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
            Debug.Log("[P0011] === 开始生成流程 ===");
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            Type generator = Find("UGF.EditorTools.AIGameDataTableGenerator");
            Debug.Log($"[P0011] 找到生成器类型: {generator}");
            MethodInfo import = generator.GetMethod("ImportDataTablesFromAIJson");
            Debug.Log($"[P0011] 找到导入方法: {import}");
            string[] logical = { 
                "Machines/MachineDefinitions", 
                "Machines/ComponentDefinitions", 
                "Catalog/FirstVersionObjects",
                "Buildings/BuildingDefinitions",
                "ResourcePoints/ResourcePointDefinitions"
            };
            Debug.Log($"[P0011] 逻辑表数量: {logical.Length}");
            string[] paths = logical.Select(p => Path.GetFullPath("GameData/AIData/DataTables/" + p + ".json")).ToArray();
            foreach (var path in paths) Debug.Log($"[P0011] 表路径: {path}");
            
            Debug.Log("[P0011] === 阶段1: Dry-run验证 ===");
            Check(import.Invoke(null, new object[] { paths, false, false }), paths.Length);
            Debug.Log("[P0011] Dry-run验证通过");
            
            var asset = AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset");
            if (asset == null) throw new InvalidOperationException("AppConfigs missing.");
            var settings = new SerializedObject(asset);
            var list = settings.FindProperty("mDataTables");
            string[] original = Enumerable.Range(0, list.arraySize).Select(i => list.GetArrayElementAtIndex(i).stringValue).ToArray();
            Debug.Log($"[P0011] 原始表数量: {original.Length}");
            
            try
            {
                Debug.Log("[P0011] === 阶段2: 注册表到AppConfigs ===");
                foreach (string value in logical) Add(list, value);
                settings.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[P0011] 已注册{logical.Length}个表，当前总数: {list.arraySize}");
                
                Debug.Log("[P0011] === 阶段3: 实际生成 ===");
                Check(import.Invoke(null, new object[] { paths, true, true }), paths.Length);
                Debug.Log("[P0011] 表生成完成");
                
                Debug.Log("[P0011] === 阶段4: 转换语言文件 ===");
                object[] language = { Path.GetFullPath("GameData/AIData/Languages/Machines/Hardware.json"), null };
                Debug.Log($"[P0011] 语言文件路径: {language[0]}");
                if (!(bool)Find("UGF.EditorTools.AILanguageAdapter").GetMethod("TryReverseJsonToExcel").Invoke(null, language))
                {
                    Debug.LogError($"[P0011] 语言文件转换失败: {string.Join("; ", (IEnumerable<string>)language[1])}");
                    throw new InvalidOperationException(string.Join("; ", (IEnumerable<string>)language[1]));
                }
                Debug.Log("[P0011] 语言文件转换成功");
                
                settings.Update();
                Add(settings.FindProperty("mLanguages"), "Machines/Hardware");
                settings.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log("[P0011] AppConfigs已保存");
                AssetDatabase.Refresh();
                Debug.Log("[P0011] === 生成流程全部完成 ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[P0011] 生成过程中发生异常: {ex.GetType().Name}: {ex.Message}");
                Debug.LogError($"[P0011] 堆栈: {ex.StackTrace}");
                settings.Update(); list = settings.FindProperty("mDataTables"); list.arraySize = original.Length;
                for (int i = 0; i < original.Length; i++) list.GetArrayElementAtIndex(i).stringValue = original[i];
                settings.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[P0011] 已回滚AppConfigs");
                throw;
            }
        }

        internal static void Add(SerializedProperty list, string value)
        {
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).stringValue == value) return;
            int index = list.arraySize; list.InsertArrayElementAtIndex(index); list.GetArrayElementAtIndex(index).stringValue = value;
        }
        internal static void Check(object report, int count)
        {
            var json = JObject.FromObject(report);
            Debug.Log($"[P0011] Check结果: successCount={json["successCount"]}, failureCount={json["failureCount"]}, 期望={count}");
            if ((int?)json["failureCount"] != 0 || (int?)json["successCount"] != count) 
            {
                Debug.LogError($"[P0011] Check失败，完整报告: {json}");
                throw new InvalidOperationException(json.ToString());
            }
        }
        private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).Single(t => t != null);
    }
}
