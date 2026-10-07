#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.UiProto
{
    internal static class FieldHudMachineOverviewPrefabBuilder
    {
        private const string SourcePath = "Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab";
        private const string TargetPath = "Assets/Game/Prefabs/UI/Operations/FieldHudMachineOverviewForm.prefab";

        [MenuItem("Game Framework/AutoEra/UI/生成现场机器总览子界面", priority = 2011)]
        private static void Build()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(SourcePath);
            GameObject root = null;
            try
            {
                root = new GameObject("FieldHudMachineOverviewForm", typeof(RectTransform), typeof(CanvasGroup), typeof(AutoEra.UI.FieldHudMachineOverviewForm));
                RectTransform rect = (RectTransform)root.transform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.sizeDelta = Vector2.zero; rect.anchoredPosition = Vector2.zero;
                Transform page = FindNode(source.transform, "Panel_PageMachineOverview");
                if (page == null)
                {
                    // 机器总览已经拆为独立子 Form；父详情预制体移除页面后，
                    // 重复执行生成菜单应保持幂等，而不是把正常拆分状态记为错误。
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(TargetPath) != null)
                    {
                        Debug.Log("[AutoEraUiPrefabMigration] FieldHudMachineOverviewForm already detached and generated.");
                        return;
                    }
                    throw new System.InvalidOperationException("找不到机器总览页，且目标子界面不存在");
                }
                Transform cloned = Object.Instantiate(page, root.transform);
                cloned.name = "Panel_PageMachineOverview";
                Transform close = FindNode(source.transform, "Btn_FieldClose");
                if (close != null)
                {
                    Transform clonedClose = Object.Instantiate(close, root.transform);
                    clonedClose.name = "Btn_FieldClose";
                    RectTransform closeRect = clonedClose as RectTransform;
                    if (closeRect != null) closeRect.anchoredPosition = new Vector2(-20f, 20f);
                }
                PrefabUtility.SaveAsPrefabAsset(root, TargetPath);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] FieldHudMachineOverviewForm generated: " + TargetPath);
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        [MenuItem("Game Framework/AutoEra/UI/从现场详情移除机器总览页", priority = 2012)]
        private static void Detach()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SourcePath);
            try
            {
                Transform page = root.transform.Find("Grp_PageHost/Panel_PageMachineOverview");
                if (page != null) Object.DestroyImmediate(page.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, SourcePath);
                AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                Debug.Log("[AutoEraUiPrefabMigration] FieldHudDetailForm machine overview detached.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
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
    }
}
#endif
