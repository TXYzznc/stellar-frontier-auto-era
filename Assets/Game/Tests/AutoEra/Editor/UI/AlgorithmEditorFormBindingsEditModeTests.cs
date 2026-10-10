using AutoEra.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor.UI
{
    /// <summary>
    /// b33：算法编辑器预制体的接线完整性回归围栏。
    ///
    /// 历史坑：b26/b29/b30 批次改了窗体脚本但**没有把新增序列化字段回写进预制体**，
    /// 造成画布、搜索、应用按钮在真实游戏里静默失灵（编辑器里一切正常的假象）。
    /// 这里把契约要求的全部绑定逐个断言非空，任何一次「改了 Fields.cs 忘了预制体」
    /// 都会在 CI 里当场暴露，而不是等玩家点不动按钮。
    /// </summary>
    public sealed class AlgorithmEditorFormBindingsEditModeTests
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab";
        private const string ToolbarPath =
            "Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorToolbar/Grp_AlgorithmDraftTools";

        [Test]
        public void Workbench_UsesInheritedCanvasAndSingleAxisSizingOwners()
        {
            var form = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = (RectTransform)form.transform;
            Assert.That(root.anchorMin, Is.EqualTo(Vector2.zero)); Assert.That(root.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(form.GetComponentsInChildren<CanvasScaler>(true), Is.Empty, "Scaling belongs to GF's shared canvas.");
            var frame = (RectTransform)root.Find("Panel_Frame");
            Assert.That(frame.anchorMin, Is.EqualTo(Vector2.zero)); Assert.That(frame.anchorMax, Is.EqualTo(Vector2.one));
            var tools = root.Find(ToolbarPath).GetComponent<HorizontalLayoutGroup>();
            Assert.That(tools.childControlHeight, Is.True);
            foreach (Transform button in tools.transform)
            {
                var element = button.GetComponent<LayoutElement>();
                Assert.That(element, Is.Not.Null);
                Assert.That(element.preferredHeight, Is.LessThanOrEqualTo(((RectTransform)tools.transform).sizeDelta.y));
                Assert.That(button.GetComponent<ContentSizeFitter>(), Is.Null, "The toolbar owns button dimensions.");
            }
            var node = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab");
            Assert.That(node.GetComponent<ContentSizeFitter>(), Is.Null);
            foreach (var fitter in node.GetComponentsInChildren<ContentSizeFitter>(true))
            {
                Assert.That(fitter.horizontalFit, Is.EqualTo(ContentSizeFitter.FitMode.Unconstrained));
                Assert.That(fitter.verticalFit, Is.EqualTo(ContentSizeFitter.FitMode.PreferredSize));
                var parent = fitter.transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
                Assert.That(parent == null || !parent.childControlHeight, Is.True, "A parent must not also control fitted height.");
                var group = fitter.GetComponent<VerticalLayoutGroup>();
                Assert.That(group.childControlHeight, Is.True); Assert.That(group.childForceExpandHeight, Is.False);
            }
        }

        [Test]
        public void MultiPortNode_ContainsEveryPortWithoutNestedScrolling()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab");
            var instance = Object.Instantiate(prefab); instance.SetActive(true);
            try
            {
                var rect = (RectTransform)instance.transform;
                rect.sizeDelta = new Vector2(AlgorithmGraphPortView.NodeWidth, AlgorithmGraphPortView.Height(2, 14));
                foreach (var scroll in instance.GetComponentsInChildren<ScrollRect>(true))
                {
                    Assert.That(scroll.horizontal || scroll.vertical, Is.False);
                    var template = scroll.content.GetChild(0).gameObject;
                    for (int i = 0; i < 14; i++) { var row = Object.Instantiate(template, scroll.content); row.SetActive(true); }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                    Assert.That(scroll.content.rect.height, Is.LessThanOrEqualTo(scroll.viewport.rect.height + .1f), "All port rows must fit the viewport.");
                    var title = instance.GetComponentInChildren<TMPro.TMP_Text>();
                    Assert.That(title, Is.Not.Null);
                }
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void AllWiredFields_HaveConcreteReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null, "预制体必须存在。");
            var form = prefab.GetComponent<AlgorithmEditorForm>();
            Assert.That(form, Is.Not.Null, "根节点必须挂 AlgorithmEditorForm。");

            var serialized = new SerializedObject(form);
            string[] fields =
            {
                "_algorithmEditorApplyButton",
                "_algorithmEditorAddButton",
                "_algorithmEditorNodeSearch",
                "_algorithmGraphContent",
                "_algorithmNodeItemPrefab",
                "_algorithmEdgeItemPrefab",
                "_algorithmUndoButton",
                "_algorithmRedoButton",
                "_algorithmDeleteSelectedButton",
            };

            foreach (string field in fields)
            {
                Assert.That(serialized.FindProperty(field), Is.Not.Null,
                    field + " 必须存在于窗体脚本（Fields.cs 与脚本不同步）。");
                Assert.That(serialized.FindProperty(field).objectReferenceValue, Is.Not.Null,
                    field + " 必须绑定真实对象——历史批次曾整段漏写导致界面静默失灵。");
            }
        }

        [Test]
        public void DraftTools_ExistWithUndoRedoDisabled()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Transform tools = prefab.transform.Find(ToolbarPath);
            Assert.That(tools, Is.Not.Null, "草稿工具条组 Grp_AlgorithmDraftTools 必须存在。");

            Button undo = tools.Find("Btn_AlgorithmUndo").GetComponent<Button>();
            Button redo = tools.Find("Btn_AlgorithmRedo").GetComponent<Button>();
            Button delete = tools.Find("Btn_AlgorithmDeleteSelected").GetComponent<Button>();
            Assert.That(undo, Is.Not.Null, "撤销按钮必须存在。");
            Assert.That(redo, Is.Not.Null, "重做按钮必须存在。");
            Assert.That(delete, Is.Not.Null, "删除选中按钮必须存在。");
            Assert.That(undo.interactable, Is.False, "撤销是占位（命令历史属后续批次），初始必须禁用。");
            Assert.That(redo.interactable, Is.False, "重做是占位（命令历史属后续批次），初始必须禁用。");
        }

        [Test]
        public void IndependentItems_HaveExpectedStructureAndInactiveRoots()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            var form = prefab.GetComponent<AlgorithmEditorForm>();
            var serialized = new SerializedObject(form);
            GameObject node = serialized.FindProperty("_algorithmNodeItemPrefab").objectReferenceValue as GameObject;
            GameObject edge = serialized.FindProperty("_algorithmEdgeItemPrefab").objectReferenceValue as GameObject;
            Assert.That(node, Is.Not.Null, "节点 Item 必须绑定独立预制体。");
            Assert.That(edge, Is.Not.Null, "连线 Item 必须绑定独立预制体。");
            Assert.That(AssetDatabase.GetAssetPath(node), Does.Contain("Assets/Game/Prefabs/UI/Item/"));
            Assert.That(AssetDatabase.GetAssetPath(edge), Does.Contain("Assets/Game/Prefabs/UI/Item/"));
            Assert.That(node.activeSelf, Is.False, "节点 Item 根必须默认 inactive。");
            Assert.That(edge.activeSelf, Is.False, "连线 Item 根必须默认 inactive。");
            Assert.That(node.GetComponent<AlgorithmNodeItem>(), Is.Not.Null);
            Assert.That(edge.GetComponent<AlgorithmEdgeItem>(), Is.Not.Null);
            Assert.That(node.transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect"), Is.Not.Null);
            Assert.That(edge.transform.Find("Grp_AlgorithmEdge"), Is.Null,
                "连线 Item 不应再包含覆盖整条边的矩形容器；曲线 Graphic 运行时创建并负责自身命中。");
            Assert.That(edge.transform.childCount, Is.EqualTo(0),
                "连线预制体只保留透明 Item 壳，避免矩形占位物参与布局和命中。");
        }
    }
}
