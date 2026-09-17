using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoEra.Editor
{
    public static class DiagnoseBuildingTables
    {
        [MenuItem("Game Framework/AutoEra/Diagnose Building Tables")]
        public static void Diagnose()
        {
            string[] files = {
                Path.GetFullPath("GameData/AIData/DataTables/Buildings/BuildingDefinitions.json"),
                Path.GetFullPath("GameData/AIData/DataTables/ResourcePoints/ResourcePointDefinitions.json")
            };

            foreach (var file in files)
            {
                Debug.Log($"[Diagnose] 检查文件: {file}");
                Debug.Log($"[Diagnose] 文件存在: {File.Exists(file)}");
                
                if (File.Exists(file))
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        Debug.Log($"[Diagnose] 文件大小: {content.Length} 字符");
                        
                        var manifest = JsonConvert.DeserializeObject<JObject>(content);
                        Debug.Log($"[Diagnose] tableName: {manifest["tableName"]}");
                        Debug.Log($"[Diagnose] relativePath: {manifest["relativePath"]}");
                        Debug.Log($"[Diagnose] kind: {manifest["kind"]}");
                        
                        var rows = manifest["rows"] as JArray;
                        if (rows != null)
                        {
                            Debug.Log($"[Diagnose] rows数量: {rows.Count}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Diagnose] 解析失败: {ex.Message}");
                    }
                }
            }
        }
    }
}
