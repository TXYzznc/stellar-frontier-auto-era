#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    /// <summary>一次性把算法工作台的图元素模板拆成独立 UIItem 资产并回写 Form 引用。</summary>
    public static class AlgorithmEditorItemMigration
    {
        private const string FormPath = "Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab";
        private const string SourcePath = FormPath;
        private const string ItemFolder = "Assets/Game/Prefabs/UI/Item";
        private const string NodePath = ItemFolder + "/AlgorithmNodeItem.prefab";
        private const string EdgePath = ItemFolder + "/AlgorithmEdgeItem.prefab";

        [MenuItem("AutoEra/Algorithm Editor/Extract Node And Edge Items")]
        public static void Extract()
        {
            EnsureFolder();
            GameObject sourceRoot = PrefabUtility.LoadPrefabContents(SourcePath);
            try
            {
                Transform template = sourceRoot.transform.Find("Panel_Frame/Grp_PageHost/Panel_PageAlgorithmEditor/Panel_AlgorithmEditorCanvas/List_AlgorithmGraph/Viewport_AlgorithmGraph/Content_AlgorithmGraph/Item_AlgorithmGraphElementTemplate");
                if (template == null) throw new System.InvalidOperationException("找不到 Item_AlgorithmGraphElementTemplate");

                CreateItem(template, "Grp_AlgorithmEdge", "AlgorithmNodeItem", NodePath);
                CreateItem(template, "Grp_AlgorithmNode", "AlgorithmEdgeItem", EdgePath);

                Transform content = template.parent;
                Object.DestroyImmediate(template.gameObject);
                AutoEra.UI.AlgorithmEditorForm form = sourceRoot.GetComponentInChildren<AutoEra.UI.AlgorithmEditorForm>(true);
                if (form == null) throw new System.InvalidOperationException("找不到 AlgorithmEditorForm 组件");
                SerializedObject serialized = new SerializedObject(form);
                serialized.FindProperty("_algorithmGraphElementTemplate").objectReferenceValue = null;
                serialized.FindProperty("_algorithmNodeItemPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(NodePath);
                serialized.FindProperty("_algorithmEdgeItemPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(EdgePath);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(sourceRoot, FormPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[AutoEra][AlgorithmEditor] 已拆分并接入节点/连线 Item：" + NodePath + "，" + EdgePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(sourceRoot);
            }
        }

        private static void CreateItem(Transform template, string removeChild, string componentName, string savePath)
        {
            GameObject clone = Object.Instantiate(template.gameObject);
            clone.name = componentName;
            Transform unwanted = clone.transform.Find(removeChild);
            if (unwanted != null) Object.DestroyImmediate(unwanted.gameObject);
            if (componentName == "AlgorithmNodeItem") clone.AddComponent<AutoEra.UI.AlgorithmNodeItem>();
            else clone.AddComponent<AutoEra.UI.AlgorithmEdgeItem>();
            clone.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(clone, savePath);
            Object.DestroyImmediate(clone);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs/UI/Item"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs/UI")) AssetDatabase.CreateFolder("Assets/Game/Prefabs", "UI");
                AssetDatabase.CreateFolder("Assets/Game/Prefabs/UI", "Item");
            }
        }
    }
}
#endif
