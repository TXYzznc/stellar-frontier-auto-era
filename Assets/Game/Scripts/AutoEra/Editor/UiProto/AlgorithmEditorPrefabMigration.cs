using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

namespace AutoEra.Editor.UiProto
{
    /// <summary>
    /// Repairs the algorithm editor's manually polished toolbar while preserving
    /// its visual layout. The migration is intentionally idempotent and scoped to
    /// the three buttons whose Image/targetGraphic contract was lost during the
    /// visual pass; it is not a general prefab rebuild.
    /// </summary>
    internal static class AlgorithmEditorPrefabMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab";
        private const string NodePrefabPath = "Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab";
        private const string EdgePrefabPath = "Assets/Game/Prefabs/UI/Item/AlgorithmEdgeItem.prefab";

        [MenuItem("Game Framework/AutoEra/UI/修正算法工作台布局", priority = 2008)]
        private static void RepairWorkbenchLayout()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Rect(root.transform.Find("Panel_Frame"), Vector2.zero, Vector2.one, new Vector2(.5f, .5f), new Vector2(-80, -80), Vector2.zero);
                Rect(root.transform.Find("Panel_Frame/Grp_PageHost"), Vector2.zero, Vector2.one, new Vector2(0, 1), new Vector2(-112, -170), new Vector2(56, -140));
                Rect(FindChild(root.transform, "Panel_AlgorithmEditorNodes"), Vector2.zero, new Vector2(0, 1), new Vector2(0, 1), new Vector2(320, -352), new Vector2(0, -124));
                Rect(FindChild(root.transform, "Panel_AlgorithmEditorCanvas"), Vector2.zero, Vector2.one, new Vector2(0, 1), new Vector2(-680, -352), new Vector2(336, -124));
                Rect(FindChild(root.transform, "Panel_AlgorithmEditorInspector"), new Vector2(1, 0), Vector2.one, Vector2.one, new Vector2(328, -352), new Vector2(0, -124));
                Rect(FindChild(root.transform, "Panel_AlgorithmEditorProblems"), Vector2.zero, new Vector2(1, 0), Vector2.zero, new Vector2(0, 152), new Vector2(0, 60));
                Rect(FindChild(root.transform, "Panel_AlgorithmEditorToolbar"), new Vector2(0, 1), Vector2.one, new Vector2(0, 1), new Vector2(0, 72), new Vector2(0, -36));
                var tools = FindChild(root.transform, "Grp_AlgorithmDraftTools");
                var group = tools.GetComponent<HorizontalLayoutGroup>();
                group.childControlWidth = group.childControlHeight = true;
                group.childForceExpandWidth = group.childForceExpandHeight = false;
                foreach (var name in new[] { "Btn_AlgorithmUndo", "Btn_AlgorithmRedo", "Btn_AlgorithmDeleteSelected" })
                {
                    var button = FindChild(root.transform, name);
                    var element = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
                    element.preferredWidth = name == "Btn_AlgorithmDeleteSelected" ? 140 : 100;
                    element.minHeight = element.preferredHeight = 32; element.flexibleHeight = 0;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            root = PrefabUtility.LoadPrefabContents(NodePrefabPath);
            try
            {
                ((RectTransform)root.transform).sizeDelta = new Vector2(AutoEra.UI.AlgorithmGraphPortView.NodeWidth, 100);
                var title = FindChild(root.transform, "Txt_AlgorithmNodeName").GetComponent<TMP_Text>();
                Rect(title.transform, new Vector2(0, 1), Vector2.one, new Vector2(.5f, 1), new Vector2(-24, 32), new Vector2(0, -8));
                title.fontSize = 20; title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
                ConfigurePorts(root.transform, true); ConfigurePorts(root.transform, false);
                PrefabUtility.SaveAsPrefabAsset(root, NodePrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra] Workbench and independent node layout migrated.");
        }
        private static void Rect(Transform target, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 position)
        {
            if (target == null) throw new InvalidOperationException("Required workbench layout node is absent.");
            var rect = (RectTransform)target; rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot;
            rect.sizeDelta = size; rect.anchoredPosition = position;
        }
        private static void ConfigurePorts(Transform root, bool input)
        {
            string direction = input ? "Input" : "Output";
            var list = FindChild(root, "List_Algorithm" + direction + "Ports");
            Rect(list, new Vector2(input ? 0 : .5f, 0), new Vector2(input ? .5f : 1, 1), new Vector2(.5f, .5f),
                new Vector2(-20, -56), new Vector2(input ? 2 : -2, -18));
            var scroll = list.GetComponent<ScrollRect>(); scroll.horizontal = scroll.vertical = false;
            var content = FindChild(list, "Content_Algorithm" + direction + "Ports");
            var group = content.GetComponent<VerticalLayoutGroup>();
            group.spacing = AutoEra.UI.AlgorithmGraphPortView.PortSpacing; group.padding = new RectOffset();
            group.childControlWidth = group.childControlHeight = true; group.childForceExpandWidth = true; group.childForceExpandHeight = false;
            var row = FindChild(content, "Item_Algorithm" + direction + "PortTemplate");
            var element = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = AutoEra.UI.AlgorithmGraphPortView.PortHeight;
            element.minWidth = 0; element.preferredWidth = -1; element.flexibleWidth = 1;
            element.flexibleHeight = 0;
            var label = FindChild(row, "Txt_Algorithm" + direction + "Port").GetComponent<TMP_Text>();
            Rect(label.transform, Vector2.zero, Vector2.one, new Vector2(.5f, .5f), new Vector2(-32, 0), new Vector2(input ? 10 : -10, 0));
            label.margin = Vector4.zero; label.fontSize = 18; label.enableWordWrapping = false; label.overflowMode = TextOverflowModes.Ellipsis;
        }

        [MenuItem("Game Framework/AutoEra/UI/导出算法布局检查", priority = 2007)]
        private static void InspectLayout()
        {
            var report = new LayoutReport();
            foreach (var path in new[] { PrefabPath, NodePrefabPath, EdgePrefabPath })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var rect in prefab.GetComponentsInChildren<RectTransform>(true))
                {
                    string relative = AnimationUtility.CalculateTransformPath(rect, prefab.transform);
                    if (path == PrefabPath && relative.Length > 0 && !rect.name.Contains("Algorithm") && !rect.name.Contains("Panel_Frame")) continue;
                    var group = rect.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    var fitter = rect.GetComponent<ContentSizeFitter>();
                    var text = rect.GetComponent<TMP_Text>();
                    report.nodes.Add(new LayoutNode { asset = path, path = relative, anchorMin = rect.anchorMin, anchorMax = rect.anchorMax,
                        pivot = rect.pivot, sizeDelta = rect.sizeDelta, position = rect.anchoredPosition,
                        layout = group == null ? null : group.GetType().Name + ":" + group.childControlWidth + ":" + group.childControlHeight,
                        fit = fitter == null ? null : fitter.horizontalFit + ":" + fitter.verticalFit,
                        font = text == null ? 0 : text.fontSize });
                }
            }
            Directory.CreateDirectory("Temp"); File.WriteAllText("Temp/b42-layout-inspection.json", JsonUtility.ToJson(report, true));
            Debug.Log("[AutoEra] Algorithm layout inspection exported to Temp/b42-layout-inspection.json");
        }
        [Serializable] private sealed class LayoutReport { public List<LayoutNode> nodes = new List<LayoutNode>(); }
        [Serializable] private sealed class LayoutNode
        {
            public string asset, path, layout, fit;
            public Vector2 anchorMin, anchorMax, pivot, sizeDelta, position;
            public float font;
        }

        [MenuItem("Game Framework/AutoEra/UI/修复算法编辑器按钮与绑定", priority = 2006)]
        private static void Repair()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                string[] buttonNames =
                {
                    "Btn_AlgorithmUndo", "Btn_AlgorithmRedo", "Btn_AlgorithmDeleteSelected"
                };
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    RepairButton(root.transform, buttonNames[i]);
                }

                Transform diagnosis = FindChild(root.transform, "Grp_AlgorithmDiagnosisActions");
                if (diagnosis != null)
                {
                    diagnosis.gameObject.SetActive(true);
                }

                SerializedObject serialized = new SerializedObject(root.GetComponent<AutoEra.UI.AlgorithmEditorForm>());
                Bind(serialized, "_algorithmEditorNodeSearch", FindComponent<TMP_InputField>(root.transform, "Panel_AlgorithmEditorNodeSearch"));
                Bind(serialized, "_publicParametersNumberValue", FindComponent<TMP_InputField>(root.transform, "Panel_PublicParametersNumberValue"));
                Bind(serialized, "_publicParametersBooleanValue", FindComponent<Toggle>(root.transform, "Tgl_PublicParametersBooleanValue"));
                Bind(serialized, "_algorithmDiagnosisNextStepButton", FindComponent<Button>(root.transform, "Btn_AlgorithmDiagnosisNextStep"));
                Bind(serialized, "_algorithmDiagnosisPreviousStepButton", FindComponent<Button>(root.transform, "Btn_AlgorithmDiagnosisPreviousStep"));
                Bind(serialized, "_algorithmNodeItemPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(NodePrefabPath));
                Bind(serialized, "_algorithmEdgeItemPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EdgePrefabPath));
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[AutoEraUiPrefabMigration] AlgorithmEditorForm toolbar/buttons/bindings repaired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RepairButton(Transform root, string name)
        {
            Transform buttonTransform = FindChild(root, name);
            if (buttonTransform == null)
            {
                return;
            }

            Button button = buttonTransform.GetComponent<Button>();
            Image image = buttonTransform.GetComponent<Image>();
            if (image == null)
            {
                image = buttonTransform.gameObject.AddComponent<Image>();
            }

            image.raycastTarget = true;
            image.color = new Color(0.321569f, 0.498039f, 0.639216f, 1f);
            button.targetGraphic = image;

            TMP_Text label = buttonTransform.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                RectTransform rect = label.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(-8f, -4f);
            }
        }

        private static void Bind(SerializedObject serialized, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static T FindComponent<T>(Transform root, string name) where T : Component
        {
            Transform target = FindChild(root, name);
            return target == null ? null : target.GetComponent<T>();
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindChild(root.GetChild(i), name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
