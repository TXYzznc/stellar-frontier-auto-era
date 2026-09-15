using System.Collections.Generic;
using AutoEra.Motion;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor.Motion
{
    internal static class FunctionalRigAcceptanceDemoSceneBuilder
    {
        private const string FormalPreviewScenePath = "Assets/Game/Scene/B08FormalEntityMotionPreview.unity";

        [MenuItem("AutoEra/Motion/Update Formal Entity Motion Preview")]
        private static void UpdateCurrentDemo()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != FormalPreviewScenePath)
            {
                Debug.LogError("[AutoEra.Motion] Open B08FormalEntityMotionPreview before updating it.");
                return;
            }

            Transform carrier = Ensure("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab", "演示_轮式载体", new Vector3(-7f, 0f, 0f));
            Transform wheel = Ensure("Assets/Game/Prefabs/Entity/Machines/WheelModule.prefab", "演示_四轮机构", new Vector3(-3f, 0f, -3f));
            Transform arm = Ensure("Assets/Game/Prefabs/Entity/Machines/MultiJointArm.prefab", "演示_多关节机械臂", new Vector3(0f, 0f, 0f));
            Remove("演示_可替换效应器");
            Transform door = Ensure("Assets/Game/Prefabs/Entity/Buildings/SlidingDoor_D24.prefab", "演示_滑动门", new Vector3(7f, 0f, 1f));
            Transform conveyor = Ensure("Assets/Game/Prefabs/Entity/Buildings/Conveyor.prefab", "演示_传送带", new Vector3(10f, 0f, -3f));
            Transform water = Ensure("Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab", "演示_水枪效应器", new Vector3(-7f, 0f, 6f));
            Transform saw = Ensure("Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab", "演示_旋转锯盘效应器", new Vector3(-2f, 0f, 6f));
            Transform drill = Ensure("Assets/Game/Prefabs/Entity/Machines/RotaryDrill.prefab", "演示_旋转钻头效应器", new Vector3(3f, 0f, 6f));
            Transform cargo = Ensure("Assets/Game/Prefabs/Entity/Machines/CargoPod.prefab", "演示_货舱", new Vector3(8f, 0f, 6f));
            Transform fixedRotary = Ensure("Assets/Game/Prefabs/Entity/Machines/FixedRotaryCarrier.prefab", "演示_固定旋转载体", new Vector3(-11f, 0f, 6f));
            BindFormalPresentationPreview(conveyor, water, saw, drill, cargo);

            FunctionalRigAcceptanceDemoDirector director = Object.FindObjectOfType<FunctionalRigAcceptanceDemoDirector>();
            if (director == null)
            {
                director = new GameObject("AcceptanceDemoDirector").AddComponent<FunctionalRigAcceptanceDemoDirector>();
            }

            Undo.RecordObject(director, "更新动作验收场");
            director.Configure(wheel, carrier, arm, door, conveyor, water, saw, drill, cargo, fixedRotary);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("AutoEra/Motion/Create Or Update Formal Entity Motion Preview")]
        private static void CreateOrUpdateFormalPreview()
        {
            if (AssetDatabase.LoadMainAssetAtPath(FormalPreviewScenePath) == null)
            {
                CreateFormalPreviewScene();
            }

            EditorSceneManager.OpenScene(FormalPreviewScenePath, OpenSceneMode.Single);
            UpdateCurrentDemo();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        private static void CreateFormalPreviewScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = new GameObject("AcceptanceCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 8f, -18f);
            camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.13f);

            GameObject lightObject = new GameObject("AcceptanceKeyLight");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            EditorSceneManager.SaveScene(scene, FormalPreviewScenePath);
        }

        private static Transform Ensure(string prefabPath, string objectName, Vector3 position)
        {
            GameObject existing = FindSceneObject(objectName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new System.InvalidOperationException("Missing formal entity prefab: " + prefabPath);
            }
            if (existing != null)
            {
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(existing);
                if (source != null && AssetDatabase.GetAssetPath(source) == prefabPath)
                {
                    if (prefabPath.EndsWith("/Conveyor.prefab", System.StringComparison.Ordinal))
                    {
                        PrefabUtility.RevertPrefabInstance(existing, InteractionMode.AutomatedAction);
                    }
                    existing.transform.position = position;
                    return existing.transform;
                }

                Object.DestroyImmediate(existing);
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = objectName;
            instance.transform.position = position;
            return instance.transform;
        }

        private static void Remove(string objectName)
        {
            GameObject existing = FindSceneObject(objectName);
            if (existing != null) Object.DestroyImmediate(existing);
        }

        private static void BindFormalPresentationPreview(Transform conveyor, Transform water, Transform saw, Transform drill, Transform cargo)
        {
            BindConveyorPreview(conveyor);
            BindEffectorPreview(water, EffectorWorkKind.WaterSpray, "mount_yaw", "nozzle_pitch", null, null, null, "flow_valve");
            BindEffectorPreview(saw, EffectorWorkKind.SawCut, "mount_yaw", null, "lift_rail", "feed_slide", "saw_spindle", null);
            BindEffectorPreview(drill, EffectorWorkKind.DrillMine, "mount_yaw", null, "press_slide", null, "drill_rotor", null);
            BindCargoPreview(cargo);
        }

        private static void BindConveyorPreview(Transform conveyor)
        {
            if (conveyor == null) return;
            Transform beltRoot = FindOrCreateChild(conveyor, "Presentation_BeltLoop");
            ConveyorLoopRigPreview preview = beltRoot.GetComponent<ConveyorLoopRigPreview>();
            if (preview == null) preview = beltRoot.gameObject.AddComponent<ConveyorLoopRigPreview>();
            preview.RebuildForEditor();

            MeshRenderer renderer = beltRoot.GetComponent<MeshRenderer>();
            Material material = FindFirstMaterial(conveyor);
            if (renderer != null && material != null) renderer.sharedMaterials = new[] { material, material };

            for (int index = 0; index < 5; index++)
            {
                Transform cargo = FindOrCreateChild(beltRoot, "DemoCargo_" + index.ToString("00"));
                cargo.localPosition = ConveyorLoopPresentation.EvaluateCargoPosition((float)index / 5f);
                cargo.localScale = new Vector3(0.45f, 0.28f, 0.55f);
                MeshRenderer cargoRenderer = cargo.GetComponent<MeshRenderer>();
                if (cargoRenderer == null)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = cargo.name;
                    cube.transform.SetParent(beltRoot, false);
                    cube.transform.localPosition = cargo.localPosition;
                    cube.transform.localScale = cargo.localScale;
                    Object.DestroyImmediate(cube.GetComponent<Collider>());
                    Object.DestroyImmediate(cargo.gameObject);
                    cargo = cube.transform;
                    cargoRenderer = cube.GetComponent<MeshRenderer>();
                }

                if (cargoRenderer != null && material != null) cargoRenderer.sharedMaterial = material;
            }
        }

        private static void BindEffectorPreview(Transform root, EffectorWorkKind kind, string yaw, string pitch, string lift, string feed, string rotor, string valve)
        {
            if (root == null) return;
            EffectorWorkRigPreview preview = root.GetComponent<EffectorWorkRigPreview>();
            if (preview == null) preview = root.gameObject.AddComponent<EffectorWorkRigPreview>();
            preview.Configure(kind, FindStableJoint(root, yaw), FindStableJoint(root, pitch), FindStableJoint(root, lift), FindStableJoint(root, feed), FindStableJoint(root, rotor), FindStableJoint(root, valve));

            Transform effects = FindOrCreateChild(root, "Presentation_Effects");
            if (kind == EffectorWorkKind.WaterSpray)
            {
                LineRenderer arc = effects.GetComponent<LineRenderer>();
                if (arc == null) arc = effects.gameObject.AddComponent<LineRenderer>();
                arc.useWorldSpace = true;
                arc.widthMultiplier = 0.06f;
                arc.startColor = new Color(0.3f, 0.75f, 1f, 0.9f);
                arc.endColor = new Color(0.3f, 0.75f, 1f, 0.15f);
                arc.enabled = false;
                Material material = FindFirstMaterial(root);
                if (material != null) arc.sharedMaterial = material;
                Transform landing = FindOrCreateChild(effects, "WaterLandingMarker");
                if (landing.GetComponent<MeshRenderer>() == null)
                {
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    marker.name = landing.name;
                    marker.transform.SetParent(effects, false);
                    Object.DestroyImmediate(marker.GetComponent<Collider>());
                    Object.DestroyImmediate(landing.gameObject);
                    landing = marker.transform;
                }

                landing.localScale = new Vector3(0.5f, 0.02f, 0.5f);
                landing.gameObject.SetActive(false);
                preview.ConfigureVisualFeedback(arc, landing, null);
            }
            else
            {
                ParticleSystem particles = effects.GetComponent<ParticleSystem>();
                if (particles == null) particles = effects.gameObject.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particles.main;
                main.startLifetime = 0.45f;
                main.startSpeed = 0.8f;
                main.startSize = 0.1f;
                main.maxParticles = 32;
                ParticleSystem.EmissionModule emission = particles.emission;
                emission.rateOverTime = 18f;
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                preview.ConfigureVisualFeedback(null, null, particles);
            }
        }

        private static void BindCargoPreview(Transform root)
        {
            if (root == null) return;
            CargoBayPreview preview = root.GetComponent<CargoBayPreview>();
            if (preview == null) preview = root.gameObject.AddComponent<CargoBayPreview>();
            Transform container = FindStableJoint(root, "cargo_container") ?? root;
            Transform load = FindOrCreateChild(container, "Preview_CargoFill");
            if (load.GetComponent<MeshRenderer>() == null)
            {
                GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fill.name = load.name;
                fill.transform.SetParent(container, false);
                Object.DestroyImmediate(fill.GetComponent<Collider>());
                Object.DestroyImmediate(load.gameObject);
                load = fill.transform;
            }

            Material material = FindFirstMaterial(root);
            MeshRenderer renderer = load.GetComponent<MeshRenderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
            preview.Configure(FindStableJoint(root, "left_door"), FindStableJoint(root, "right_door"), FindStableJoint(root, "transfer_tray"), load, 0.45f, 0.75f);
        }

        private static Transform FindStableJoint(Transform root, string stableId)
        {
            if (root == null || string.IsNullOrEmpty(stableId)) return null;
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == stableId || candidate.name == "Joint_" + stableId) return candidate;
            }

            return null;
        }

        private static Transform FindOrCreateChild(Transform parent, string objectName)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null) return existing;
            Transform child = new GameObject(objectName).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static Material FindFirstMaterial(Transform root)
        {
            MeshRenderer renderer = root.GetComponentInChildren<MeshRenderer>(true);
            return renderer == null ? null : renderer.sharedMaterial;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == objectName) return candidate.gameObject;
                }
            }

            return null;
        }
    }
}
