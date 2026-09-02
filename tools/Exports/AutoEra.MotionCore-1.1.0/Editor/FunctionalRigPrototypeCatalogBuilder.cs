using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    internal static class FunctionalRigPrototypeCatalogBuilder
    {
        private const string Folder = "Assets/Game/Prefabs/FunctionalPrototypes/Catalog";
        private const string MaterialFolder = "Assets/Game/Materials/FunctionalPrototypes";

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
                ApplyPresentationIdentity(root, familyId);
                PrefabUtility.SaveAsPrefabAsset(root, Folder + "/" + familyId + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Transform Child(Transform parent, string name) { Transform child = new GameObject(name).transform; child.SetParent(parent, false); return child; }

        private static void ApplyPresentationIdentity(GameObject root, string familyId)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/Game/Materials", "FunctionalPrototypes");
            Material material = GetOrCreateMaterial(familyId, GetFamilyColor(familyId));
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true)) renderer.sharedMaterial = material;

            Transform slot = FindFirstVisualSlot(root.transform);
            if (slot == null) return;
            GameObject label = new GameObject("Label_" + familyId);
            label.transform.SetParent(slot, false);
            label.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            label.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            var text = label.AddComponent<TextMesh>();
            text.text = familyId.Replace('_', ' ').ToUpperInvariant();
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.12f;
            text.fontSize = 28;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Transform FindFirstVisualSlot(Transform root)
        {
            foreach (FunctionalRigPrototypeStableId identity in root.GetComponentsInChildren<FunctionalRigPrototypeStableId>(true))
            {
                if (identity.StableId.StartsWith("visual-slot:")) return identity.transform;
            }

            return null;
        }

        private static Material GetOrCreateMaterial(string familyId, Color color)
        {
            string path = MaterialFolder + "/" + familyId + "_prototype.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader == null ? Shader.Find("Standard") : shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            return material;
        }

        private static Color GetFamilyColor(string familyId)
        {
            switch (familyId)
            {
                case "wheeled_carrier": return new Color(0.20f, 0.48f, 0.78f);
                case "four_wheel_module": return new Color(0.34f, 0.72f, 0.40f);
                case "multi_joint_arm": return new Color(0.93f, 0.52f, 0.20f);
                case "replaceable_effector": return new Color(0.72f, 0.34f, 0.76f);
                case "sliding_door": return new Color(0.92f, 0.74f, 0.22f);
                default: return new Color(0.22f, 0.70f, 0.70f);
            }
        }
    }
}
