using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    internal static class FunctionalRigPrototypeCatalogBuilder
    {
        private const string Folder = "Assets/Game/Prefabs/FunctionalPrototypes/Catalog";

        [MenuItem("AutoEra/Functional Prototypes/Build Representative Catalog")]
        private static void BuildCatalog()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Game/Prefabs/FunctionalPrototypes", "Catalog");
            foreach (string familyId in FunctionalRigPrototypeCatalog.AssetFamilyIds) BuildFamily(familyId);
            AssetDatabase.SaveAssets();
        }

        private static void BuildFamily(string familyId)
        {
            GameObject root = new GameObject("FPR_" + familyId);
            try
            {
                Transform logic = Child(root.transform, "LogicRoot");
                Transform rig = Child(root.transform, "RigRoot");
                Transform visual = Child(rig, "VisualRoot");
                Transform collision = Child(root.transform, "AuthorityCollisionRoot");
                collision.gameObject.AddComponent<BoxCollider>();
                FunctionalRigPrototypeHierarchy hierarchy = root.AddComponent<FunctionalRigPrototypeHierarchy>();
                hierarchy.Configure(logic, rig, visual, collision);
                FunctionalRigPrototypeBuilder.Build(FunctionalRigPrototypeCatalog.Create(familyId), hierarchy);
                PrefabUtility.SaveAsPrefabAsset(root, Folder + "/" + familyId + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Transform Child(Transform parent, string name) { Transform child = new GameObject(name).transform; child.SetParent(parent, false); return child; }
    }
}
