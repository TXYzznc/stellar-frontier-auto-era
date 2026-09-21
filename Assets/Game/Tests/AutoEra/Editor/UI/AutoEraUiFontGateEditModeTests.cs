using AutoEra.Editor.UiProto;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 字体门（EditMode 薄壳）。
    ///
    /// 判定逻辑在 Editor 程序集的 <see cref="AutoEraUiFontTool"/>——它要读 prefab 与
    /// TMP_FontAsset，只有该程序集可见。这里做两件事：
    ///   1. 资产态检查：字体单图集、预制体字符全覆盖、无残留子网格、字体引用统一；
    ///   2. 生成态检查：把每个界面预制体实例化出来强制生成网格，断言 TMP 不会拆出
    ///      <c>TMP_SubMeshUI</c> 子对象——这是「子对象真的不会被生成」的唯一硬证据，
    ///      只查资产是查不出来的（那些子对象本来就不落盘）。
    /// </summary>
    public sealed class AutoEraUiFontGateEditModeTests
    {
        [Test]
        public void UiFontAsset_And_Prefabs_PassAssetSideGate()
        {
            var problems = AutoEraUiFontTool.Check();

            Assert.That(problems, Is.Empty,
                "字体门（资产态）未通过：\n" + string.Join("\n", problems.Select(p => "  - " + p)));
        }

        [Test]
        public void UiPrefabs_GenerateMesh_WithoutTmpSubMesh()
        {
            string[] prefabs = Directory
                .GetFiles(AutoEraUiFontTool.UiPrefabRoot, "*.prefab", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p)
                .ToArray();

            Assert.That(prefabs, Is.Not.Empty, "没有找到界面预制体");

            var failures = new List<string>();
            int textCount = 0;
            int prefabCount = 0;

            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var canvasObject = new GameObject("FontGateCanvas", typeof(Canvas));
                SceneManager.MoveGameObjectToScene(canvasObject, preview);

                foreach (string path in prefabs)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset == null)
                    {
                        failures.Add($"{path} 加载失败");
                        continue;
                    }

                    prefabCount++;
                    GameObject instance = Object.Instantiate(asset, canvasObject.transform);
                    try
                    {
                        foreach (TMP_Text text in instance.GetComponentsInChildren<TMP_Text>(true))
                        {
                            textCount++;
                            // 未激活的节点也要算：状态卡片这类默认 inactive 的文本同样会拆子网格。
                            text.ForceMeshUpdate(true, false);
                        }

                        int subMeshes = instance.GetComponentsInChildren<TMP_SubMeshUI>(true).Length;
                        if (subMeshes > 0)
                        {
                            failures.Add($"{Path.GetFileName(path)} 生成了 {subMeshes} 个 TMP SubMeshUI 子对象");
                        }
                    }
                    finally
                    {
                        Object.DestroyImmediate(instance);
                    }
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }

            Assert.That(failures, Is.Empty,
                $"字体门（生成态）未通过：{prefabCount} 个预制体 / {textCount} 条文本中，" +
                $"{failures.Count} 个预制体仍会拆出子网格：\n" + string.Join("\n", failures.Take(40)));
        }
    }
}
