#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.UiProto
{
    /// <summary>把基地中枢能源详情页提取为按需加载的独立 UIForm。</summary>
    internal static class BaseCommandEnergyPrefabBuilder
    {
        private const string SourcePath = "Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab";
        private const string TargetPath = "Assets/Game/Prefabs/UI/Operations/BaseCommandEnergyForm.prefab";

        [MenuItem("Game Framework/AutoEra/UI/生成基地中枢能源子界面", priority = 2009)]
        private static void Build()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(SourcePath);
            GameObject root = null;
            try
            {
                root = new GameObject("BaseCommandEnergyForm", typeof(RectTransform), typeof(CanvasGroup),
                    typeof(AutoEra.UI.BaseCommandEnergyForm));
                RectTransform rect = (RectTransform)root.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;

                Transform blocker = source.transform.Find("Bg_InputBlocker");
                Transform frame = source.transform.Find("Panel_Frame");
                if (blocker == null || frame == null) throw new System.InvalidOperationException("基地中枢外壳节点不完整");
                Object.Instantiate(blocker, root.transform);
                Transform clonedFrame = Object.Instantiate(frame, root.transform);
                clonedFrame.name = "Panel_Frame";

                Transform navigation = clonedFrame.Find("Grp_Navigation");
                if (navigation != null) Object.DestroyImmediate(navigation.gameObject);
                Transform host = clonedFrame.Find("Grp_PageHost");
                if (host != null)
                {
                    for (int i = host.childCount - 1; i >= 0; i--)
                    {
                        if (host.GetChild(i).name != "Panel_PageHubEnergy")
                            Object.DestroyImmediate(host.GetChild(i).gameObject);
                    }
                }

                Transform title = clonedFrame.Find("Txt_FormTitle");
                if (title != null)
                {
                    TMPro.TMP_Text text = title.GetComponent<TMPro.TMP_Text>();
                    if (text != null) text.text = "能源系统";
                }

                var form = root.GetComponent<AutoEra.UI.BaseCommandEnergyForm>();
                SerializedObject serialized = new SerializedObject(form);
                Bind(serialized, "_backButton", Find<UnityEngine.UI.Button>(root.transform, "Btn_FormBack"));
                Bind(serialized, "_closeButton", Find<UnityEngine.UI.Button>(root.transform, "Btn_FormClose"));
                Bind(serialized, "_hubEnergyConfigureButton", Find<UnityEngine.UI.Button>(root.transform, "Btn_HubEnergyConfigure"));
                Bind(serialized, "_hubEnergyChargingAllowedToggle", Find<UnityEngine.UI.Toggle>(root.transform, "Tgl_HubEnergyChargingAllowed"));
                Bind(serialized, "_hubEnergyChargeTargetSlider", Find<UnityEngine.UI.Slider>(root.transform, "Sld_HubEnergyChargeTarget"));
                Bind(serialized, "_hubEnergyHistoryButton", Find<UnityEngine.UI.Button>(root.transform, "Btn_HubEnergyHistory"));
                Bind(serialized, "_hubEnergySummaryContent", Find<RectTransform>(root.transform, "Content_HubEnergySummary"));
                Bind(serialized, "_hubEnergySummaryBody", Find<TMPro.TMP_Text>(root.transform, "Txt_HubEnergySummaryBody"));
                Bind(serialized, "_hubEnergySummaryTemplate", Find<GameObject>(root.transform, "Item_HubEnergySummaryTemplate"));
                Bind(serialized, "_hubEnergyFacilitiesContent", Find<RectTransform>(root.transform, "Content_HubEnergyFacilities"));
                Bind(serialized, "_hubEnergyFacilitiesBody", Find<TMPro.TMP_Text>(root.transform, "Txt_HubEnergyFacilitiesBody"));
                Bind(serialized, "_hubEnergyFacilitiesTemplate", Find<GameObject>(root.transform, "Item_HubEnergyFacilitiesTemplate"));
                Bind(serialized, "_hubEnergyConsumersContent", Find<RectTransform>(root.transform, "Content_HubEnergyConsumers"));
                Bind(serialized, "_hubEnergyConsumersBody", Find<TMPro.TMP_Text>(root.transform, "Txt_HubEnergyConsumersBody"));
                Bind(serialized, "_hubEnergyConsumersTemplate", Find<GameObject>(root.transform, "Item_HubEnergyConsumersTemplate"));
                Bind(serialized, "_hubEnergyLoadingState", Find<GameObject>(root.transform, "Grp_HubEnergyLoadingState"));
                Bind(serialized, "_hubEnergyEmptyState", Find<GameObject>(root.transform, "Grp_HubEnergyEmptyState"));
                Bind(serialized, "_hubEnergyErrorState", Find<GameObject>(root.transform, "Grp_HubEnergyErrorState"));
                Bind(serialized, "_hubEnergySuccessState", Find<GameObject>(root.transform, "Grp_HubEnergySuccessState"));
                Bind(serialized, "_hubEnergyDisabledState", Find<GameObject>(root.transform, "Grp_HubEnergyDisabledState"));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] BaseCommandEnergyForm generated: " + TargetPath);
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        [MenuItem("Game Framework/AutoEra/UI/从基地中枢移除能源页", priority = 2010)]
        private static void Detach()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SourcePath);
            try
            {
                Transform page = root.transform.Find("Panel_Frame/Grp_PageHost/Panel_PageHubEnergy");
                if (page != null) Object.DestroyImmediate(page.gameObject);
                var form = root.GetComponent<AutoEra.UI.BaseCommandHubForm>();
                if (form != null)
                {
                    SerializedObject serialized = new SerializedObject(form);
                    SerializedProperty pages = serialized.FindProperty("_pageRoots");
                    if (pages != null && pages.isArray)
                    {
                        pages.arraySize = 6;
                        string[] names = { "Panel_PageHubOverview", "Panel_PageHubTasks", "Panel_PageHubObjects", "Panel_PageHubRules", "Panel_PageHubStats", "Panel_PageRemoteMachine" };
                        for (int i = 0; i < names.Length; i++)
                        {
                            Transform pageRoot = root.transform.Find("Panel_Frame/Grp_PageHost/" + names[i]);
                            pages.GetArrayElementAtIndex(i).objectReferenceValue = pageRoot != null ? pageRoot.gameObject : null;
                        }
                    }
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, SourcePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] BaseCommandHubForm energy page detached.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
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
                Transform found = FindNode(root.GetChild(i), name);
                if (found != null) return found;
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
