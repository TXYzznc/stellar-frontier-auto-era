using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>
    /// Creates a disposable scene object and saves only the approved structural template.
    /// The Builder does not create or alter a product scene.
    /// </summary>
    internal static class FunctionalRigPrototypeHierarchyTemplateBuilder
    {
        private const string PrefabFolderPath = "Assets/Game/Prefabs/FunctionalPrototypes";
        private const string PrefabPath = PrefabFolderPath + "/FunctionalRigHierarchyTemplate.prefab";

        [MenuItem("AutoEra/Functional Prototypes/Create Hierarchy Template")]
        private static void CreateHierarchyTemplate()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.LogWarning("[AutoEra.Motion] CreateHierarchyTemplate Skipped: template already exists at " + PrefabPath);
                return;
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/Game/Prefabs", "FunctionalPrototypes");
            }

            GameObject root = new GameObject("FPR_HierarchyTemplate");
            try
            {
                Transform logicRoot = CreateChild(root.transform, "LogicRoot");
                Transform rigRoot = CreateChild(root.transform, "RigRoot");
                Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
                Transform joint = CreateChild(visualRoot, "Joint_Template");
                CreateChild(joint, "VisualSlot_Template");
                Transform authorityCollisionRoot = CreateChild(root.transform, "AuthorityCollisionRoot");
                authorityCollisionRoot.gameObject.AddComponent<BoxCollider>();

                FunctionalRigPrototypeHierarchy hierarchy = root.AddComponent<FunctionalRigPrototypeHierarchy>();
                hierarchy.Configure(logicRoot, rigRoot, visualRoot, authorityCollisionRoot);
                if (!hierarchy.TryValidate(out string error))
                {
                    throw new System.InvalidOperationException(error);
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[AutoEra.Motion] CreateHierarchyTemplate Success: " + PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
    }
}
