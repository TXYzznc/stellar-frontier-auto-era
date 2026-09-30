using AutoEra.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor.UI
{
    /// <summary>
    /// b35：警报页预制体的接线完整性回归围栏。
    ///
    /// 与算法编辑器同源的历史坑：契约与脚本加了字段，但预制体没回写，真实游戏里按钮静默失灵。
    /// 这里逐个断言警报页的全部绑定非空，任何一次「改了 Fields.cs 忘了预制体」都会当场暴露。
    /// </summary>
    public sealed class AlertFormBindingsEditModeTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/Operations/AlertForm.prefab";

        [Test]
        public void AllWiredFields_HaveConcreteReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null, "预制体必须存在。");
            var form = prefab.GetComponent<AlertForm>();
            Assert.That(form, Is.Not.Null, "根节点必须挂 AlertForm。");

            var serialized = new SerializedObject(form);
            string[] fields =
            {
                "_alertsFilterButton",
                "_alertsReadButton",
                "_alertsLocateButton",
                "_alertsDetailsButton",
                "_alertsListContent",
                "_alertsListTemplate",
                "_alertsDetailContent",
                "_alertsDetailTemplate",
                "_alertsListBody",
                "_alertsDetailBody",
            };

            foreach (string field in fields)
            {
                Assert.That(serialized.FindProperty(field), Is.Not.Null,
                    field + " 必须存在于窗体脚本（Fields.cs 与脚本不同步）。");
                Assert.That(serialized.FindProperty(field).objectReferenceValue, Is.Not.Null,
                    field + " 必须绑定真实对象——历史批次曾整段漏写导致界面静默失灵。");
            }
        }
    }
}
