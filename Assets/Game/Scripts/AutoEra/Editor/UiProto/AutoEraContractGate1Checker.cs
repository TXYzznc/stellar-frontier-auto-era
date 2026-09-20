using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor.UiProto
{
    /// <summary>
    /// 门 1（原型验收）的**契约驱动**机器判定，覆盖 L1 静态卫生 / L2 契约一致 / L3 绑定语义。
    ///
    /// 遍历 UI-PrefabLayouts 下的每一份 `&lt;Form&gt;.contract.json`，对号入座检查它声明的
    /// 预制体：新增页面只要写出契约就自动进入门禁，不会因为忘了加断言而漏检。
    ///
    /// 放在 Editor 程序集而不是测试程序集，是因为契约要按 JSON 解析，而
    /// Newtonsoft 只对该程序集可见；EditMode 测试因此只做一行断言，
    /// 人工复跑则直接用菜单「门1 契约自检」。
    ///
    /// 它只**判定**，不修改任何资产。L4（编译与运行）由普通编译与 PlayMode 冒烟承担。
    /// </summary>
    public static class AutoEraContractGate1Checker
    {
        public const string ContractDirectory = AutoEraUiPrefabGenerator.ContractDirectory;

        /// <summary>03 规范的唯一前缀词表；表外前缀一律违规。</summary>
        private static readonly HashSet<string> Prefixes = new HashSet<string>(StringComparer.Ordinal)
        {
            "Bg_", "Overlay_", "Panel_", "Grp_", "Txt_", "Img_", "Icon_", "Btn_",
            "Tgl_", "Sld_", "List_", "Viewport_", "Content_", "Item_", "Bar_", "Deco_"
        };

        /// <summary>
        /// 规范**明确**要求 raycastTarget=false 的前缀：装饰件（Img_/Icon_/Deco_）与非交互文本。
        /// 依据 00-通用合同「装饰与文本 raycastTarget=false，按钮自身 Image 和拦截遮罩=true」。
        /// </summary>
        private static readonly HashSet<string> RaycastForbidden = new HashSet<string>(StringComparer.Ordinal)
        {
            "Img_", "Icon_", "Deco_", "Txt_"
        };

        /// <summary>规范明确要求 raycastTarget=true 的前缀：可交互图形与拦截遮罩。</summary>
        private static readonly HashSet<string> RaycastRequired = new HashSet<string>(StringComparer.Ordinal)
        {
            "Btn_", "Bg_", "Overlay_"
        };

        private static readonly Dictionary<string, Type> KnownTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            { "Image", typeof(Image) },
            { "RawImage", typeof(RawImage) },
            { "Button", typeof(Button) },
            { "Toggle", typeof(Toggle) },
            { "Slider", typeof(Slider) },
            { "ScrollRect", typeof(ScrollRect) },
            { "RectMask2D", typeof(RectMask2D) },
            { "Mask", typeof(Mask) },
            { "HorizontalLayoutGroup", typeof(HorizontalLayoutGroup) },
            { "VerticalLayoutGroup", typeof(VerticalLayoutGroup) },
            { "GridLayoutGroup", typeof(GridLayoutGroup) },
            { "ContentSizeFitter", typeof(ContentSizeFitter) },
            { "LayoutElement", typeof(LayoutElement) },
            { "CanvasGroup", typeof(CanvasGroup) },
            { "TextMeshProUGUI", typeof(TextMeshProUGUI) },
        };

        private static Dictionary<string, Type> _typeIndex;

        [MenuItem("Game Framework/AutoEra/UI/门1 契约自检", priority = 2100)]
        public static void RunFromMenu()
        {
            List<string> problems = CheckAll();

            // 完整清单自己落盘。控制台消息会被 unity-skills 的摘要截断（长消息只回开头几行），
            // 只靠 console 读结果会看到「未通过（34 项）」却只列出 3 行——诊断信息被吞掉一半。
            // 写文件之后，门1 的失败原因在任何读取路径下都是完整的。
            // 全限定：本文件处于 AutoEra.* 命名空间下，裸写 Application 会解析到 AutoEra.Application。
            string reportPath = Path.Combine(
                Path.GetDirectoryName(UnityEngine.Application.dataPath) ?? ".", "tools/_gate1_report.txt");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? "tools");
                File.WriteAllText(reportPath, problems.Count == 0
                    ? $"[Gate1] 通过：{ContractDirectory} 下全部契约满足 L1/L2/L3。\n"
                    : $"[Gate1] 未通过（{problems.Count} 项）：\n" + string.Join("\n", problems) + "\n");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Gate1] 报告落盘失败（不影响判定）：{ex.Message}");
            }

            if (problems.Count == 0)
            {
                Debug.Log($"[Gate1] 通过：{ContractDirectory} 下全部契约满足 L1/L2/L3。");
                return;
            }

            Debug.LogError($"[Gate1] 未通过（{problems.Count} 项）：\n" + string.Join("\n", problems));
        }

        /// <summary>检查契约目录下全部契约，返回问题清单（空表示通过）。</summary>
        public static List<string> CheckAll()
        {
            var problems = new List<string>();
            if (!Directory.Exists(ContractDirectory))
            {
                problems.Add($"契约目录不存在：{ContractDirectory}");
                return problems;
            }

            string[] contracts = Directory.GetFiles(ContractDirectory, "*.contract.json");
            if (contracts.Length == 0)
            {
                problems.Add($"未找到任何契约：{ContractDirectory}");
                return problems;
            }

            foreach (string contractPath in contracts.OrderBy(p => p, StringComparer.Ordinal))
            {
                CheckContract(contractPath.Replace('\\', '/'), problems);
            }

            return problems;
        }

        public static void CheckContract(string contractPath, List<string> errors)
        {
            JObject doc;
            try
            {
                doc = JObject.Parse(File.ReadAllText(contractPath));
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(contractPath)}: 契约无法解析：{ex.Message}");
                return;
            }

            string formName = (string)doc["form"];
            string tag = $"[{formName}]";

            string prefabPath = (string)doc["prefabPath"];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                errors.Add($"{tag} L2: 契约声明的预制体不存在：{prefabPath}");
                return;
            }

            CheckL1Root(tag, prefab, errors);
            CheckL1Naming(tag, prefab, errors);
            CheckL2Tree(tag, (JObject)doc["root"], prefab.transform, prefab.name, errors);
            CheckL3Bindings(tag, doc, prefab, errors);
        }

        // ------------------------------------------------------------------ L1

        private static void CheckL1Root(string tag, GameObject prefab, List<string> errors)
        {
            var rect = prefab.transform as RectTransform;
            if (rect == null)
            {
                errors.Add($"{tag} L1: 根节点不是 RectTransform");
                return;
            }

            if (rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one ||
                rect.sizeDelta != Vector2.zero || rect.anchoredPosition != Vector2.zero)
            {
                errors.Add($"{tag} L1: 根节点必须是全屏 Stretch 且 offsets 为 0");
            }

            foreach (Component component in prefab.GetComponents<Component>())
            {
                if (component == null)
                {
                    errors.Add($"{tag} L1: 根节点存在 Missing Script");
                    continue;
                }

                string typeName = component.GetType().Name;
                if (typeName == "Canvas" || typeName == "CanvasScaler" || typeName == "GraphicRaycaster")
                {
                    errors.Add($"{tag} L1: 根节点不得带 {typeName}");
                }
            }
        }

        private static void CheckL1Naming(string tag, GameObject prefab, List<string> errors)
        {
            var names = new List<string>();
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                string name = t.name;
                if (name != prefab.name)
                {
                    names.Add(name);
                }

                if (t == prefab.transform)
                {
                    continue;
                }

                int underscore = name.IndexOf('_');
                if (underscore <= 0)
                {
                    errors.Add($"{tag} L1: 节点名缺少前缀：{name}");
                    continue;
                }

                string prefix = name.Substring(0, underscore + 1);
                if (!Prefixes.Contains(prefix))
                {
                    errors.Add($"{tag} L1: 表外前缀 {prefix}（节点 {name}）");
                }

                var graphic = t.GetComponent<Graphic>();
                var button = t.GetComponent<Button>();
                var label = t.GetComponent<TMP_Text>();

                switch (prefix)
                {
                    case "Grp_":
                        if (graphic != null) errors.Add($"{tag} L1: Grp_ 不得挂 Graphic（{name}）");
                        break;
                    case "Txt_":
                        if (label == null) errors.Add($"{tag} L1: Txt_ 必须挂 TextMeshProUGUI（{name}）");
                        if (button != null) errors.Add($"{tag} L1: Txt_ 不得挂 Button（{name}）");
                        break;
                    case "Btn_":
                        if (button == null)
                        {
                            errors.Add($"{tag} L1: Btn_ 必须挂 Button（{name}）");
                        }
                        else if (button.targetGraphic != graphic)
                        {
                            errors.Add($"{tag} L1: Btn_ 的 targetGraphic 必须指向自身 Image（{name}）");
                        }
                        break;
                    case "Deco_":
                        if (label != null) errors.Add($"{tag} L1: Deco_ 不得承载文本（{name}）");
                        if (graphic != null && graphic.raycastTarget)
                        {
                            errors.Add($"{tag} L1: Deco_ 的 raycastTarget 必须为 false（{name}）");
                        }
                        break;
                    case "List_":
                        if (t.GetComponent<ScrollRect>() == null) errors.Add($"{tag} L1: List_ 必须挂 ScrollRect（{name}）");
                        break;
                    case "Viewport_":
                        if (t.GetComponent<RectMask2D>() == null) errors.Add($"{tag} L1: Viewport_ 必须挂 RectMask2D（{name}）");
                        break;
                    case "Content_":
                        if (t.GetComponent<LayoutGroup>() == null) errors.Add($"{tag} L1: Content_ 必须有 LayoutGroup（{name}）");
                        break;
                    case "Item_":
                        if (t.gameObject.activeSelf) errors.Add($"{tag} L1: Item_ 模板必须默认 inactive（{name}）");
                        break;
                }

                if (graphic != null && RaycastForbidden.Contains(prefix) && graphic.raycastTarget)
                {
                    errors.Add($"{tag} L1: {prefix.TrimEnd('_')} 的 raycastTarget 必须为 false（{name}）");
                }

                if (graphic != null && RaycastRequired.Contains(prefix) && !graphic.raycastTarget)
                {
                    errors.Add($"{tag} L1: {prefix.TrimEnd('_')} 的 raycastTarget 必须为 true（{name}）");
                }

                if (t.GetComponent<UnityEngine.UI.Text>() != null)
                {
                    errors.Add($"{tag} L1: 使用 Legacy Text（{name}）");
                }

                if (label is TextMeshProUGUI tmp && tmp.font == null)
                {
                    errors.Add($"{tag} L1: TMP 未指定字体（{name}）");
                }
            }

            foreach (var group in names.GroupBy(n => n).Where(g => g.Count() > 1))
            {
                errors.Add($"{tag} L1: 同一 Form 内节点名重复：{group.Key}（{group.Count()} 次）");
            }
        }

        // ------------------------------------------------------------------ L2

        private static void CheckL2Tree(string tag, JObject spec, Transform actual, string path, List<string> errors)
        {
            string expectedName = (string)spec["name"];
            if (actual.name != expectedName)
            {
                errors.Add($"{tag} L2: 节点名不一致，契约 {expectedName}，实际 {actual.name}");
                return;
            }

            if (!(actual is RectTransform rect))
            {
                errors.Add($"{tag} L2: {path} 缺少 RectTransform");
                return;
            }

            // 被 LayoutGroup 驱动的子节点由组计算 anchor/pivot/位置/尺寸，写死坐标对它没有意义
            // （00-通用合同：LayoutGroup 控制的孩子只声明 LayoutElement 优选尺寸，不与手写坐标竞争）。
            // Unity 在加载/保存时会用布局组重算这些值，拿契约坐标比对必然误报。
            // 双重判据：契约显式标记 controlledBy=group，或该节点的父级带 LayoutGroup。
            // 后者是 Unity 事实——布局组会覆写子节点的 anchor/pivot/位置/尺寸——所以即使
            // 契约漏标也不会误报；前者让契约本身把「由组驱动」这件事记录下来。
            bool declaredGroup = spec["controlledBy"] != null &&
                string.Equals((string)spec["controlledBy"], "group", StringComparison.Ordinal);
            bool parentDriven = actual.parent != null && actual.parent.GetComponent<LayoutGroup>() != null;
            if (!declaredGroup && !parentDriven)
            {
                CompareVector(tag, path, "anchorMin", rect.anchorMin, spec["anchorMin"], errors);
                CompareVector(tag, path, "anchorMax", rect.anchorMax, spec["anchorMax"], errors);
                CompareVector(tag, path, "pivot", rect.pivot, spec["pivot"], errors);
                CompareVector(tag, path, "sizeDelta", rect.sizeDelta, spec["sizeDelta"], errors);
                CompareVector(tag, path, "anchoredPosition", rect.anchoredPosition, spec["anchoredPosition"], errors);
            }

            bool expectedActive = spec["active"] == null || spec["active"].Value<bool>();
            if (actual.gameObject.activeSelf != expectedActive)
            {
                errors.Add($"{tag} L2: {path} 激活状态不一致，契约 {expectedActive}，实际 {actual.gameObject.activeSelf}");
            }

            if (spec["components"] is JArray comps)
            {
                foreach (JObject comp in comps)
                {
                    string typeName = (string)comp["type"];
                    Type type = ResolveType(typeName);
                    if (type == null)
                    {
                        errors.Add($"{tag} L2: 无法解析契约声明的组件类型 {typeName}（{path}）");
                        continue;
                    }

                    if (!typeof(Component).IsAssignableFrom(type) || actual.GetComponent(type) == null)
                    {
                        errors.Add($"{tag} L2: {path} 缺少契约声明的组件 {typeName}");
                    }
                }
            }

            var specChildren = spec["children"] as JArray ?? new JArray();
            if (actual.childCount != specChildren.Count)
            {
                errors.Add($"{tag} L2: {path} 子节点数不一致，契约 {specChildren.Count}，实际 {actual.childCount}");
                return;
            }

            for (int i = 0; i < specChildren.Count; i++)
            {
                Transform child = actual.GetChild(i);
                CheckL2Tree(tag, (JObject)specChildren[i], child, $"{path}/{child.name}", errors);
            }
        }

        private static void CompareVector(string tag, string path, string field, Vector2 actual, JToken spec, List<string> errors)
        {
            if (spec == null)
            {
                return;
            }

            var expected = new Vector2(spec[0].Value<float>(), spec[1].Value<float>());
            if (Mathf.Abs(actual.x - expected.x) > 0.01f || Mathf.Abs(actual.y - expected.y) > 0.01f)
            {
                errors.Add($"{tag} L2: {path}.{field} 不一致，契约 ({expected.x},{expected.y})，实际 ({actual.x},{actual.y})");
            }
        }

        // ------------------------------------------------------------------ L3

        private static void CheckL3Bindings(string tag, JObject doc, GameObject prefab, List<string> errors)
        {
            string formName = (string)doc["form"];
            Type formType = ResolveType(formName);
            if (formType == null)
            {
                errors.Add($"{tag} L3: 契约声明的 Form 类型不存在：{formName}");
                return;
            }

            Component form = prefab.GetComponent(formType);
            if (form == null)
            {
                errors.Add($"{tag} L3: 预制体根未挂 {formName}");
                return;
            }

            var serialized = new SerializedObject(form);

            foreach (JObject binding in doc["bindings"] as JArray ?? new JArray())
            {
                string field = (string)binding["path"];
                string declaredNode = (string)binding["node"];
                SerializedProperty property = FindProperty(serialized, field);
                if (property == null)
                {
                    errors.Add($"{tag} L3: 绑定字段不存在：{field}");
                    continue;
                }

                UnityEngine.Object target = property.objectReferenceValue;
                if (target == null)
                {
                    errors.Add($"{tag} L3: 绑定为空：{field} -> {declaredNode}");
                    continue;
                }

                Transform targetTransform = target is Component component ? component.transform
                    : target is GameObject go ? go.transform : null;
                if (targetTransform == null)
                {
                    continue;
                }

                string actualPath = RelativePath(prefab.transform, targetTransform);
                string expectedPath = TrimRoot(declaredNode, prefab.name);
                if (actualPath != expectedPath)
                {
                    errors.Add($"{tag} L3: {field} 绑到了错误节点，契约 {expectedPath}，实际 {actualPath}");
                }
            }

            CheckL3ScriptFields(tag, formType, form, doc, errors);
        }

        /// <summary>Form 声明的每个 SerializeField 都必须被契约覆盖且非空。</summary>
        private static void CheckL3ScriptFields(
            string tag, Type formType, Component form, JObject doc, List<string> errors)
        {
            var covered = new HashSet<string>(StringComparer.Ordinal);
            foreach (JObject binding in doc["bindings"] as JArray ?? new JArray())
            {
                covered.Add(((string)binding["path"]).Split('.')[0].Split('[')[0]);
            }
            // 标量（如 _initialPage、_pageBindings[0]._page）同样是契约对字段的声明。
            foreach (JObject scalar in doc["scalars"] as JArray ?? new JArray())
            {
                covered.Add(((string)scalar["path"]).Split('.')[0].Split('[')[0]);
            }
            foreach (var pair in doc["formArrays"] as JObject ?? new JObject())
            {
                covered.Add(pair.Key);
            }

            foreach (FieldInfo field in formType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                bool serialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
                if (!serialized || field.IsInitOnly || field.IsStatic)
                {
                    continue;
                }

                object value = field.GetValue(form);
                if (field.FieldType.IsArray)
                {
                    var array = value as Array;
                    if (array == null || array.Length == 0)
                    {
                        errors.Add($"{tag} L3: SerializeField 数组为空：{field.Name}");
                        continue;
                    }

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array.GetValue(i) == null)
                        {
                            errors.Add($"{tag} L3: SerializeField 数组元素为空：{field.Name}[{i}]");
                        }
                    }
                }
                else if (value == null || (value is UnityEngine.Object obj && obj == null))
                {
                    errors.Add($"{tag} L3: SerializeField 为空：{field.Name}");
                }

                if (!covered.Contains(field.Name))
                {
                    errors.Add($"{tag} L3: SerializeField 未在契约中声明：{field.Name}");
                }
            }
        }

        // -------------------------------------------------------------- 工具

        private static SerializedProperty FindProperty(SerializedObject serialized, string path)
        {
            SerializedProperty property = null;
            foreach (string segment in path.Split('.'))
            {
                string name = segment;
                int bracket = segment.IndexOf('[');
                int index = -1;
                if (bracket >= 0)
                {
                    name = segment.Substring(0, bracket);
                    index = int.Parse(segment.Substring(bracket + 1, segment.Length - bracket - 2));
                }

                property = property == null
                    ? serialized.FindProperty(name)
                    : property.FindPropertyRelative(name);
                if (property == null)
                {
                    return null;
                }

                if (index >= 0)
                {
                    if (!property.isArray || property.arraySize <= index)
                    {
                        return null;
                    }

                    property = property.GetArrayElementAtIndex(index);
                }
            }

            return property;
        }

        private static string RelativePath(Transform root, Transform target)
        {
            var parts = new List<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                parts.Insert(0, current.name);
                current = current.parent;
            }

            return current == root ? string.Join("/", parts) : "<不在该预制体内>";
        }

        private static string TrimRoot(string contractPath, string rootName)
        {
            return contractPath.StartsWith(rootName + "/", StringComparison.Ordinal)
                ? contractPath.Substring(rootName.Length + 1)
                : contractPath;
        }

        private static Type ResolveType(string name)
        {
            if (KnownTypes.TryGetValue(name, out Type known))
            {
                return known;
            }

            if (_typeIndex == null)
            {
                _typeIndex = new Dictionary<string, Type>(StringComparer.Ordinal);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = assembly.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                    catch (Exception) { continue; }

                    foreach (Type type in types)
                    {
                        if (!typeof(Component).IsAssignableFrom(type))
                        {
                            continue;
                        }

                        if (!_typeIndex.ContainsKey(type.Name))
                        {
                            _typeIndex[type.Name] = type;
                        }
                    }
                }
            }

            return _typeIndex.TryGetValue(name, out Type found) ? found : null;
        }
    }
}
