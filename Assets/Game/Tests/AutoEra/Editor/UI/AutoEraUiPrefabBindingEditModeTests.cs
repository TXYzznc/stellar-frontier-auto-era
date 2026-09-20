using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 两个 Operations 风格预制体的 **L1 静态规则**回归：命名词表、前缀↔组件职责、
    /// raycast 归属、根节点职责、列表三件套。
    ///
    /// 与契约驱动的门 1 检查（<c>AutoEraContractPrefabGate1EditModeTests</c>）是同一批规则的
    /// 独立第二实现：门 1 从契约声明出发，这里从 prefab 自身出发，两者互为交叉验证。
    /// 页面内部结构、绑定路径与运行期状态不在这里断言——那是门 1 与 PlayMode 回归的职责。
    /// </summary>
    public sealed class AutoEraUiPrefabBindingEditModeTests
    {
        private const string HubPrefabPath = "Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab";
        private const string HudPrefabPath = "Assets/Game/Prefabs/UI/Hud/FieldHudForm.prefab";

        private static readonly string[] NodePrefixes =
        {
            "Bg_", "Overlay_", "Panel_", "Grp_", "Txt_", "Img_", "Icon_", "Btn_",
            "Tgl_", "Sld_", "List_", "Viewport_", "Content_", "Item_", "Bar_", "Deco_"
        };

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void EntryPrefab_IsIndependentOfTheRetiredVisualCandidateChain(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Assert.That(PrefabUtility.IsPartOfPrefabInstance(root), Is.False, prefabPath);
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Any(transform => PrefabUtility.IsPartOfPrefabInstance(transform.gameObject)), Is.False, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            foreach (string dependency in AssetDatabase.GetDependencies(prefabPath, true))
            {
                Assert.That(dependency.StartsWith("Assets/Game/Prefabs/UI/ART006_UI/"), Is.False, dependency);
                Assert.That(dependency.StartsWith("Assets/Game/Art/UI/ART006_UI/"), Is.False, dependency);
            }
        }

        // ---------------------------------------------------------------- L1

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void NodeNamesFollowTheSingleApprovedVocabulary(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                {
                    if (node == root.transform) continue;   // the root is the Form class name
                    Assert.That(NodePrefixes.Any(prefix => node.name.StartsWith(prefix)), Is.True,
                        $"节点名不在规范词表内: {node.name}");
                    Assert.That(System.Text.RegularExpressions.Regex.IsMatch(node.name, @"_\d+($|_)"), Is.False,
                        $"节点名含无意义序号: {node.name}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void PrefixMatchesComponentDuty(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                {
                    bool hasButton = node.GetComponent<Button>() != null;
                    bool hasGraphic = node.GetComponent<Graphic>() != null;

                    if (node.name.StartsWith("Grp_"))
                    {
                        Assert.IsFalse(hasGraphic, $"Grp_ 必须是无视觉分组，却带 Graphic: {node.name}");
                    }

                    if (node.name.StartsWith("Txt_"))
                    {
                        Assert.IsFalse(hasButton, $"Txt_ 不得承载 Button: {node.name}");
                    }

                    if (node.name.StartsWith("Btn_"))
                    {
                        Assert.IsTrue(hasButton, $"Btn_ 必须带 Button: {node.name}");
                        var target = node.GetComponent<Button>().targetGraphic;
                        Assert.IsNotNull(target, $"Btn_ 缺少 targetGraphic: {node.name}");
                        Assert.AreSame(node.GetComponent<Graphic>(), target,
                            $"Btn_ 的 targetGraphic 必须指向自身 Image: {node.name}");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void DecorativeGraphicsDoNotEatRaycasts(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                {
                    bool hasButton = node.GetComponent<Button>() != null;
                    bool hasScroll = node.GetComponent<ScrollRect>() != null;
                    bool interactiveSurface = node.name.StartsWith("Bg_") || node.name.StartsWith("Overlay_");

                    foreach (var graphic in node.GetComponents<Graphic>())
                    {
                        // This test assembly does not reference Unity.TextMeshPro, so a
                        // TMP graphic is detected by type name.
                        if (graphic.GetType().Name.StartsWith("TextMeshPro"))
                        {
                            Assert.IsFalse(graphic.raycastTarget, $"非交互文本不得吃射线: {node.name}");
                            continue;
                        }

                        bool expected = hasButton || hasScroll || interactiveSurface;
                        Assert.AreEqual(expected, graphic.raycastTarget,
                            $"{node.name} 的 raycastTarget 与职责不符（期望 {expected}）");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void RootIsLifecycleOnlyAndFullScreen(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RectTransform rect = root.GetComponent<RectTransform>();
                Assert.AreEqual(Vector2.zero, rect.anchorMin);
                Assert.AreEqual(Vector2.one, rect.anchorMax);
                Assert.AreEqual(Vector2.zero, rect.sizeDelta);
                Assert.AreEqual(Vector2.zero, rect.anchoredPosition);

                // The form is instantiated under the scene's root Canvas, so the ASSET must not
                // carry its own Canvas/GraphicRaycaster/CanvasScaler (project deviation,
                // recorded in GF-UI-Standards/08 项目差异记录). GF adds a Canvas to the runtime
                // instance, which is why this assertion belongs to the asset, not the instance.
                Assert.IsNull(root.GetComponent<Canvas>(), "根节点不得带 Canvas");
                Assert.IsNull(root.GetComponent<GraphicRaycaster>(), "根节点不得带 GraphicRaycaster");
                Assert.IsNull(root.GetComponent<CanvasScaler>(), "根节点不得带 CanvasScaler");
                Assert.IsNull(root.GetComponent<Graphic>(), "根节点不得带可见 Graphic");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void EveryListHasScrollStructureAndOneInactiveTemplate(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var lists = root.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name.StartsWith("List_") || t.name.StartsWith("Panel_") && t.name.EndsWith("List"))
                    .ToArray();
                Assert.IsNotEmpty(lists, "未找到任何列表节点");

                foreach (Transform list in lists)
                {
                    if (list.GetComponent<ScrollRect>() == null) continue;   // 非滚动型列表另计
                    Transform viewport = list.Cast<Transform>()
                        .FirstOrDefault(c => c.name.StartsWith("Viewport_"));
                    Assert.IsNotNull(viewport, $"List_ 缺 Viewport_: {list.name}");
                    Assert.IsNotNull(viewport.GetComponent<RectMask2D>(), $"Viewport_ 缺 RectMask2D: {viewport.name}");

                    Transform content = viewport.Cast<Transform>()
                        .FirstOrDefault(c => c.name.StartsWith("Content_"));
                    Assert.IsNotNull(content, $"Viewport_ 缺 Content_: {viewport.name}");
                    Assert.IsNotNull(content.GetComponent<LayoutGroup>(), $"Content_ 缺 LayoutGroup: {content.name}");

                    Transform[] templates = content.Cast<Transform>()
                        .Where(c => c.name.StartsWith("Item_")).ToArray();
                    Assert.AreEqual(1, templates.Length, $"列表模板必须唯一: {list.name}");
                    Assert.IsFalse(templates[0].gameObject.activeSelf, $"Item 模板必须默认停用: {templates[0].name}");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
