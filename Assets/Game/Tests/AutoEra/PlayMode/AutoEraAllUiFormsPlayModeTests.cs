using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 全场界面清单的运行期冒烟：把 UIViews 里登记过的每个 Form 都从 UITable 打开一次并关闭。
    ///
    /// 它回答的是「这份界面在真实 GF 运行时是否可用」——能否按登记路径加载、逻辑桥是否挂上、
    /// 根节点是否把缩放留给场景根 Canvas、关闭是否干净。页面内部结构与绑定路径由门 1 的
    /// 契约驱动检查负责，两者互不重复。
    /// </summary>
    public sealed class AutoEraAllUiFormsPlayModeTests
    {
        [UnityTest]
        public IEnumerator EveryConfiguredForm_OpensAndClosesThroughUiExtension()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            var failures = new List<string>();
            var views = (UIViews[])Enum.GetValues(typeof(UIViews));

            foreach (UIViews view in views)
            {
                int serialId = GF.UI.OpenUIForm(view);
                if (serialId < 0)
                {
                    failures.Add($"{view}: OpenUIForm 返回 {serialId}（UITable 路径或 UIGroup 配置有问题）");
                    continue;
                }

                yield return WaitForForm(serialId, expectedLoaded: true);

                if (!GF.UI.HasUIForm(serialId))
                {
                    failures.Add($"{view}: 打开后未进入已加载状态");
                    continue;
                }

                GameObject logic = GF.UI.GetUIForm(serialId).Logic != null
                    ? GF.UI.GetUIForm(serialId).Logic.gameObject
                    : null;
                if (logic == null)
                {
                    failures.Add($"{view}: UIForm 没有 Logic（Form 脚本未挂或被替换）");
                }
                else
                {
                    if (logic.GetComponent<CanvasScaler>() != null)
                    {
                        failures.Add($"{view}: Form 根节点自带 CanvasScaler");
                    }

                    RectTransform rect = logic.GetComponent<RectTransform>();
                    if (rect == null || rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one)
                    {
                        failures.Add($"{view}: Form 根节点不是全屏 Stretch");
                    }
                }

                GF.UI.CloseUIForm(serialId);
                yield return WaitForForm(serialId, expectedLoaded: false);

                if (GF.UI.HasUIForm(serialId))
                {
                    failures.Add($"{view}: 关闭后仍存在于 UI 管理器");
                }
            }

            Assert.That(failures, Is.Empty,
                $"界面运行期冒烟失败（{failures.Count}/{views.Length}）：\n" + string.Join("\n", failures));
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            const string launchSceneName = "Launch";
            if (SceneManager.GetActiveScene().name == launchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(launchSceneName, LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation, "Launch 必须启用并加入 Build Settings。");
            yield return loadOperation;
            Assert.AreEqual(launchSceneName, SceneManager.GetActiveScene().name);
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI 运行时或必需的 UI 数据表在 600 帧内没有就绪。");
        }

        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} 在 300 帧内没有到达 loaded={expectedLoaded}。");
        }
    }
}
