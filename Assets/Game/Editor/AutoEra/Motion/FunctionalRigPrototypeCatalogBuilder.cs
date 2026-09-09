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
                ApplyEffectorPreviewBinding(root, familyId, visual);
                ApplyPresentationIdentity(root, familyId);
                ApplyFamilyGeometry(familyId, visual);
                string prefabPath = Folder + "/" + familyId + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                if (familyId == "conveyor") PersistConveyorMesh(prefabPath);
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

        private static void ApplyFamilyGeometry(string familyId, Transform visualRoot)
        {
            if (familyId != "conveyor") return;
            Transform geometry = FindByName(visualRoot, "Geometry_belt_loop_visual");
            if (geometry == null) return;
            MeshFilter filter = geometry.GetComponent<MeshFilter>();
            if (filter != null) Object.DestroyImmediate(filter);
            MeshRenderer renderer = geometry.GetComponent<MeshRenderer>();
            if (renderer != null) Object.DestroyImmediate(renderer);
            ConveyorLoopRigPreview preview = geometry.GetComponent<ConveyorLoopRigPreview>();
            if (preview == null) preview = geometry.gameObject.AddComponent<ConveyorLoopRigPreview>();
            geometry.localPosition = Vector3.zero;
            geometry.localRotation = Quaternion.identity;
            geometry.localScale = Vector3.one;
            preview.RebuildForEditor();
            MeshRenderer beltRenderer = geometry.GetComponent<MeshRenderer>();
            if (beltRenderer != null)
            {
                beltRenderer.sharedMaterials = new[]
                {
                    GetOrCreateMaterial("conveyor_belt", new Color(0.16f, 0.19f, 0.22f)),
                    GetOrCreateMaterial("conveyor_motion_marker", new Color(0.95f, 0.42f, 0.08f))
                };
            }
            BuildConveyorCargo(geometry);
        }

        private static void BuildConveyorCargo(Transform parent)
        {
            Material cargoMaterial = GetOrCreateMaterial("conveyor_demo_cargo", new Color(0.94f, 0.57f, 0.16f));
            for (int index = 0; index < 5; index++)
            {
                GameObject cargo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cargo.name = "DemoCargo_" + index.ToString("00");
                Object.DestroyImmediate(cargo.GetComponent<Collider>());
                cargo.transform.SetParent(parent, false);
                cargo.transform.localPosition = ConveyorLoopPresentation.EvaluateCargoPosition((float)index / 5f);
                cargo.transform.localScale = new Vector3(0.45f, 0.28f, 0.55f);
                cargo.GetComponent<MeshRenderer>().sharedMaterial = cargoMaterial;
            }
        }

        private static void PersistConveyorMesh(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            ConveyorLoopRigPreview preview = prefab.GetComponentInChildren<ConveyorLoopRigPreview>(true);
            if (preview == null) return;

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(prefabPath))
            {
                if (asset is Mesh && asset.name == "连续闭环传送带") Object.DestroyImmediate(asset, true);
            }

            Mesh mesh = preview.RebuildForEditor();
            AssetDatabase.AddObjectToAsset(mesh, prefabPath);
            EditorUtility.SetDirty(preview);
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        private static Transform FindByName(Transform root, string name)
        {
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true)) if (candidate.name == name) return candidate;
            return null;
        }

        private static void ApplyEffectorPreviewBinding(GameObject root, string familyId, Transform visualRoot)
        {
            EffectorWorkKind kind;
            switch (familyId)
            {
                case "water_sprayer": kind = EffectorWorkKind.WaterSpray; break;
                case "rotary_saw": kind = EffectorWorkKind.SawCut; break;
                case "rotary_drill": kind = EffectorWorkKind.DrillMine; break;
                case "cargo_bay": ApplyCargoPreviewBinding(root, visualRoot); return;
                default: return;
            }

            EffectorWorkRigPreview preview = root.GetComponent<EffectorWorkRigPreview>();
            if (preview == null) preview = root.AddComponent<EffectorWorkRigPreview>();
            preview.Configure(
                kind,
                FindJoint(visualRoot, "mount_yaw"),
                FindJoint(visualRoot, "nozzle_pitch"),
                FindJoint(visualRoot, "lift_rail") ?? FindJoint(visualRoot, "press_slide"),
                FindJoint(visualRoot, "feed_slide"),
                FindJoint(visualRoot, "saw_spindle") ?? FindJoint(visualRoot, "drill_rotor"),
                FindJoint(visualRoot, "flow_valve"));
            Transform effectRoot = root.transform.Find("PreviewEffects");
            if (effectRoot == null) effectRoot = Child(root.transform, "PreviewEffects");
            LineRenderer waterArc = null;
            Transform waterLandingMarker = null;
            ParticleSystem particles = null;
            if (kind == EffectorWorkKind.WaterSpray)
            {
                waterArc = effectRoot.GetComponent<LineRenderer>();
                if (waterArc == null) waterArc = effectRoot.gameObject.AddComponent<LineRenderer>();
                waterArc.useWorldSpace = true;
                waterArc.widthMultiplier = 0.06f;
                waterArc.startColor = new Color(0.3f, 0.75f, 1f, 0.9f);
                waterArc.endColor = new Color(0.3f, 0.75f, 1f, 0.15f);
                waterArc.enabled = false;
                waterLandingMarker = effectRoot.Find("WaterLandingMarker");
                if (waterLandingMarker == null)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    marker.name = "WaterLandingMarker";
                    Object.DestroyImmediate(marker.GetComponent<Collider>());
                    waterLandingMarker = marker.transform;
                    waterLandingMarker.SetParent(effectRoot, false);
                    waterLandingMarker.localScale = new Vector3(0.5f, 0.02f, 0.5f);
                }
                waterLandingMarker.gameObject.SetActive(false);
            }
            else
            {
                particles = effectRoot.GetComponent<ParticleSystem>();
                if (particles == null) particles = effectRoot.gameObject.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particles.main;
                main.startLifetime = 0.45f;
                main.startSpeed = 0.8f;
                main.startSize = 0.1f;
                main.maxParticles = 32;
                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = 18f;
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            preview.ConfigureVisualFeedback(waterArc, waterLandingMarker, particles);
        }

        private static void ApplyCargoPreviewBinding(GameObject root, Transform visualRoot)
        {
            Transform body = FindJoint(visualRoot, "bay_frame");
            Transform tray = FindJoint(visualRoot, "transfer_tray");
            Transform load = tray.Find("Preview_CargoFill");
            if (load == null)
            {
                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "Preview_CargoFill";
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                load = visual.transform;
                load.SetParent(tray, false);
            }

            CargoBayPreview preview = root.GetComponent<CargoBayPreview>();
            if (preview == null) preview = root.AddComponent<CargoBayPreview>();
            preview.Configure(FindJoint(visualRoot, "left_door"), FindJoint(visualRoot, "right_door"), tray, load);
        }

        private static Transform FindJoint(Transform visualRoot, string stableId)
        {
            string name = "Joint_" + stableId;
            foreach (Transform candidate in visualRoot.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == name) return candidate;
            }

            return null;
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
