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
            GameObject root = new GameObject(GetPrototypeObjectName(familyId));
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

        }

        private static string GetPrototypeObjectName(string familyId)
        {
            switch (familyId)
            {
                case "wheeled_carrier": return "原型_轮式载体";
                case "four_wheel_module": return "原型_四轮机构";
                case "multi_joint_arm": return "原型_多关节机械臂";
                case "replaceable_effector": return "原型_可替换效应器";
                case "sliding_door": return "原型_滑动门";
                case "conveyor": return "原型_传送带";
                case "water_sprayer": return "原型_水枪效应器";
                case "rotary_saw": return "原型_旋转锯盘效应器";
                case "rotary_drill": return "原型_旋转钻头效应器";
                case "cargo_bay": return "原型_货舱";
                case "fixed_rotary_carrier": return "原型_固定旋转载体";
                default: return familyId;
            }
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
                case "water_sprayer": return new Color(0.18f, 0.58f, 0.90f);
                case "rotary_saw": return new Color(0.88f, 0.30f, 0.20f);
                case "rotary_drill": return new Color(0.42f, 0.42f, 0.48f);
                case "cargo_bay": return new Color(0.58f, 0.36f, 0.20f);
                case "fixed_rotary_carrier": return new Color(0.24f, 0.62f, 0.55f);
                default: return new Color(0.22f, 0.70f, 0.70f);
            }
        }
    }
}
