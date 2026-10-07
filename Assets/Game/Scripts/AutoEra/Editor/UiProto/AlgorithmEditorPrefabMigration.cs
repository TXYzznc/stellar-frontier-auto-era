using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
