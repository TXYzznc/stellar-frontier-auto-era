#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor.UiProto
{
    /// <summary>从算法工作台的公开参数页生成独立 GF UIForm 预制体。</summary>
    internal static class AlgorithmPublicParametersPrefabBuilder
    {
        private const string SourcePath = "Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab";
        private const string TargetPath = "Assets/Game/Prefabs/UI/Operations/AlgorithmPublicParametersForm.prefab";

        [MenuItem("Game Framework/AutoEra/UI/生成算法公开参数子界面", priority = 2007)]
        private static void Build()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(SourcePath);
            GameObject root = null;
            try
            {
                root = new GameObject("AlgorithmPublicParametersForm", typeof(RectTransform), typeof(CanvasGroup),
                    typeof(AutoEra.UI.AlgorithmPublicParametersForm));
                RectTransform rootRect = (RectTransform)root.transform;
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.sizeDelta = Vector2.zero;
                rootRect.anchoredPosition = Vector2.zero;

                Transform blocker = source.transform.Find("Bg_InputBlocker");
                Transform frame = source.transform.Find("Panel_Frame");
                if (blocker == null || frame == null) throw new System.InvalidOperationException("算法工作台外壳节点不完整");

                Object.Instantiate(blocker, root.transform);
                Transform clonedFrame = Object.Instantiate(frame, root.transform);
                clonedFrame.name = "Panel_Frame";
                Transform navigation = clonedFrame.Find("Grp_Navigation");
                if (navigation != null) Object.DestroyImmediate(navigation.gameObject);
                Transform editorPage = clonedFrame.Find("Grp_PageHost/Panel_PageAlgorithmEditor");
                if (editorPage != null) Object.DestroyImmediate(editorPage.gameObject);

                Transform title = clonedFrame.Find("Txt_FormTitle");
                if (title != null)
                {
                    TMPro.TMP_Text text = title.GetComponent<TMPro.TMP_Text>();
                    if (text != null) text.text = "公开参数";
                }

                SerializedObject serialized = new SerializedObject(root.GetComponent<AutoEra.UI.AlgorithmPublicParametersForm>());
                Bind(serialized, "_backButton", Find<Button>(root.transform, "Btn_FormBack"));
                Bind(serialized, "_closeButton", Find<Button>(root.transform, "Btn_FormClose"));
                Bind(serialized, "_parametersContent", Find<RectTransform>(root.transform, "Content_PublicParametersParameters"));
                Bind(serialized, "_parametersBody", Find<TMPro.TMP_Text>(root.transform, "Txt_PublicParametersParametersBody"));
                Bind(serialized, "_parametersTemplate", Find<GameObject>(root.transform, "Item_PublicParametersParametersTemplate"));
                Bind(serialized, "_impactContent", Find<RectTransform>(root.transform, "Content_PublicParametersImpact"));
                Bind(serialized, "_impactBody", Find<TMPro.TMP_Text>(root.transform, "Txt_PublicParametersImpactBody"));
                Bind(serialized, "_impactTemplate", Find<GameObject>(root.transform, "Item_PublicParametersImpactTemplate"));
                Bind(serialized, "_loadingState", Find<GameObject>(root.transform, "Grp_PublicParametersLoadingState"));
                Bind(serialized, "_emptyState", Find<GameObject>(root.transform, "Grp_PublicParametersEmptyState"));
                Bind(serialized, "_errorState", Find<GameObject>(root.transform, "Grp_PublicParametersErrorState"));
                Bind(serialized, "_successState", Find<GameObject>(root.transform, "Grp_PublicParametersSuccessState"));
                Bind(serialized, "_disabledState", Find<GameObject>(root.transform, "Grp_PublicParametersDisabledState"));
                Bind(serialized, "_changeButton", Find<Button>(root.transform, "Btn_PublicParametersChange"));
                Bind(serialized, "_defaultButton", Find<Button>(root.transform, "Btn_PublicParametersDefault"));
                Bind(serialized, "_applyButton", Find<Button>(root.transform, "Btn_PublicParametersApply"));
                Bind(serialized, "_numberValue", Find<TMPro.TMP_InputField>(root.transform, "Panel_PublicParametersNumberValue"));
                Bind(serialized, "_booleanValue", Find<Toggle>(root.transform, "Tgl_PublicParametersBooleanValue"));
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] AlgorithmPublicParametersForm generated: " + TargetPath);
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        [MenuItem("Game Framework/AutoEra/UI/从算法工作台移除公开参数页", priority = 2008)]
        private static void DetachFromEditor()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SourcePath);
            try
            {
                Transform page = root.transform.Find("Panel_Frame/Grp_PageHost/Panel_PagePublicParameters");
                if (page != null) Object.DestroyImmediate(page.gameObject);

                AutoEra.UI.AlgorithmEditorForm form = root.GetComponent<AutoEra.UI.AlgorithmEditorForm>();
                if (form != null)
                {
                    SerializedObject serialized = new SerializedObject(form);
                    SerializedProperty pages = serialized.FindProperty("_pageRoots");
                    if (pages != null && pages.isArray)
                    {
                        pages.arraySize = 1;
                        Transform editorPage = root.transform.Find("Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor");
                        pages.GetArrayElementAtIndex(0).objectReferenceValue = editorPage != null ? editorPage.gameObject : null;
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(root, SourcePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] AlgorithmEditorForm public parameters page detached.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static T Find<T>(Transform root, string name) where T : Object
        {
            Transform node = FindNode(root, name);
            if (node == null) return null;
            if (typeof(T) == typeof(GameObject)) return node.gameObject as T;
            return node.GetComponent(typeof(T)) as T;
        }

        private static Transform FindNode(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindNode(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private static void Bind(SerializedObject serialized, string field, Object value)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property != null) property.objectReferenceValue = value;
        }
    }
}
#endif
