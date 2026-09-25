using System;
using AutoEra.PCG;
using AutoEra.Input;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor
{
    public static class InitialRegionSceneBuilder
    {
        [MenuItem("Game Framework/AutoEra/Configure Region Selection Layer")]
        public static void ConfigureSelectionLayer()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tags.FindProperty("layers");
            int selected = LayerMask.NameToLayer("RegionSelection");
            if (selected < 0)
                for (int i = 8; i < 32; i++)
                    if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) { selected = i; break; }
            if (selected < 8) throw new InvalidOperationException("No free user layer.");
            bool[,] before = new bool[32, 32];
            for (int a = 0; a < 32; a++)
                for (int b = 0; b < 32; b++) before[a, b] = Physics.GetIgnoreLayerCollision(a, b);
            layers.GetArrayElementAtIndex(selected).stringValue = "RegionSelection";
            tags.ApplyModifiedPropertiesWithoutUndo();
            for (int i = 0; i < 32; i++) Physics.IgnoreLayerCollision(selected, i, true);
            for (int a = 0; a < 32; a++)
                for (int b = 0; b < 32; b++)
                    if (a != selected && b != selected && before[a, b] != Physics.GetIgnoreLayerCollision(a, b))
                        throw new InvalidOperationException("Unrelated collision matrix changed.");
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra][Region] Selection layer=" + selected + "; ignores 32 layers; existing pairs unchanged.");
        }

        [MenuItem("Game Framework/AutoEra/Build Initial Region Scene")]
        public static void Build()
        {
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                throw new InvalidOperationException("InitialRegion already exists; do not overwrite saved user changes.");
            int selectionLayer = LayerMask.NameToLayer("RegionSelection");
            if (selectionLayer < 0) throw new InvalidOperationException("RegionSelection layer must be configured first.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                GameObject root = new GameObject("InitialRegion");
                InitialRegionScene entry = root.AddComponent<InitialRegionScene>();
                RegionObjectView[] views =
                {
                    Create(root, selectionLayer, "基础仓库", PersistentObjectKind.Building, new Vector2(0, -14), new Vector2(8, 6), 2f, true),
                    Create(root, selectionLayer, "农田", PersistentObjectKind.ResourcePoint, new Vector2(-12, 0), new Vector2(8, 8), .12f, false),
                    Create(root, selectionLayer, "人工林", PersistentObjectKind.ResourcePoint, new Vector2(12, 0), new Vector2(8, 8), .12f, false),
                    Create(root, selectionLayer, "地表矿脉", PersistentObjectKind.ResourcePoint, new Vector2(-12, 14), new Vector2(10, 8), .12f, false),
                    Create(root, selectionLayer, "水域", PersistentObjectKind.ResourcePoint, new Vector2(12, 14), new Vector2(8, 6), .12f, false, true),
                    Create(root, selectionLayer, "生物质发电机", PersistentObjectKind.Building, new Vector2(-12, -14), new Vector2(4, 3), 1.5f, true),
                    Create(root, selectionLayer, "太阳能阵列", PersistentObjectKind.Building, new Vector2(12, -14), new Vector2(4, 4), .8f, true)
                };
                AttachPcgDecoration(root, views);
                var serialized = new SerializedObject(entry);
                SerializedProperty array = serialized.FindProperty("_objects");
                array.arraySize = views.Length;
                for (int i = 0; i < views.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = views[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "区域地面";
                ground.transform.localScale = new Vector3(8, 1, 8);
                GameObject cameraRoot = new GameObject("RegionCamera");
                Camera camera = cameraRoot.AddComponent<Camera>();
                RegionCameraController cameraControl = root.AddComponent<RegionCameraController>();
                var cameraData = new SerializedObject(cameraControl);
                cameraData.FindProperty("_camera").objectReferenceValue = camera;
                cameraData.ApplyModifiedPropertiesWithoutUndo();
                cameraControl.Focus(Vector3.zero);
                RegionInputModule input = root.AddComponent<RegionInputModule>();
                var inputData = new SerializedObject(input);
                inputData.FindProperty("_scene").objectReferenceValue = entry;
                inputData.FindProperty("_camera").objectReferenceValue = cameraControl;
                inputData.FindProperty("_selectionLayers").intValue = 1 << selectionLayer;
                inputData.ApplyModifiedPropertiesWithoutUndo();
                var lighting = new GameObject("RegionSun").AddComponent<Light>();
                lighting.type = LightType.Directional;
                lighting.transform.rotation = Quaternion.Euler(55, -30, 0);
                if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Scene save failed.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static RegionObjectView Create(GameObject parent, int layer, string name, PersistentObjectKind kind,
            Vector2 position, Vector2 size, float height, bool blocks, bool infinite = false)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent.transform, false);
            root.transform.position = new Vector3(position.x, 0, position.y);
            RegionObjectView view = root.AddComponent<RegionObjectView>();
            var data = new SerializedObject(view);
            data.FindProperty("_displayName").stringValue = name;
            data.FindProperty("_kind").intValue = (int)kind;
            data.FindProperty("_footprint").vector2Value = size;
            data.FindProperty("_blocksNavigation").boolValue = blocks;
            data.FindProperty("_infiniteResource").boolValue = infinite;
            data.ApplyModifiedPropertiesWithoutUndo();
            GameObject visual;
            if (kind == PersistentObjectKind.Machine)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab");
                if (prefab == null) throw new InvalidOperationException("Accepted wheeled carrier is missing.");
                visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(root.transform, false);
                visual.transform.localPosition = Vector3.up * height * .5f;
                visual.transform.localScale = new Vector3(size.x, height, size.y);
                if (!blocks) UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            }
            visual.name = "Visual";
            var selection = new GameObject("Selection");
            selection.transform.SetParent(root.transform, false);
            selection.layer = layer;
            BoxCollider collider = selection.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = Vector3.up * height * .5f;
            collider.size = new Vector3(size.x, Mathf.Max(height, .5f), size.y);
            return view;
        }

        /// <summary>接入 PCG 运行时装饰：地表矿脉挂矿石生成器、区域根挂散布器。运行时 Play Mode 程序化生成。</summary>
        private static void AttachPcgDecoration(GameObject root, RegionObjectView[] views)
        {
            // 1. 「地表矿脉」资源点挂 OreDepositVisual（运行时程序化生成矿石视觉，替换 Cube 占位）
            OreDistributionConfig oreConfig = AssetDatabase.LoadAssetAtPath<OreDistributionConfig>(
                "Assets/Game/ScriptableAssets/PCGConfigs/OreDistribution.asset");
            foreach (RegionObjectView view in views)
            {
                if (view == null || view.name != "地表矿脉")
                {
                    continue;
                }
                OreDepositVisual ore = view.gameObject.AddComponent<OreDepositVisual>();
                var oreData = new SerializedObject(ore);
                oreData.FindProperty("_distribution").objectReferenceValue = oreConfig;
                oreData.FindProperty("_footprint").vector2Value = new Vector2(10f, 8f);
                oreData.FindProperty("_seed").intValue = 20261001;
                oreData.FindProperty("_oreCount").intValue = 8;
                oreData.ApplyModifiedPropertiesWithoutUndo();
                // 禁用 Cube 占位：矿石成为正式视觉
                Transform visual = view.transform.Find("Visual");
                if (visual != null)
                {
                    visual.gameObject.SetActive(false);
                }
            }

            // 2. 区域根挂 RuntimeScatterer（运行时散布岩石/植株）
            RuntimeScatterer scatterer = root.AddComponent<RuntimeScatterer>();
            var scData = new SerializedObject(scatterer);
            scData.FindProperty("_area").vector2Value = new Vector2(80f, 80f);
            scData.FindProperty("_density").floatValue = 0.02f;
            scData.FindProperty("_minDistance").floatValue = 1.5f;
            scData.FindProperty("_seed").intValue = 20261001;
            scData.FindProperty("_rockShare").floatValue = 0.15f;
            SerializedProperty rocks = scData.FindProperty("_rockVarieties");
            rocks.arraySize = 2;
            ConfigureRockVariety(rocks.GetArrayElementAtIndex(0), "Assets/Game/ScriptableAssets/PCG/Rock/岩石-普通石块.asset", 1f);
            ConfigureRockVariety(rocks.GetArrayElementAtIndex(1), "Assets/Game/ScriptableAssets/PCG/Rock/岩石-圆润卵石.asset", 1f);
            string[] vegPaths =
            {
                "Assets/Game/Prefabs/植株/树/ForestTree01_Optimized.prefab",
                "Assets/Game/Prefabs/植株/灌木/ForestBush01_Optimized.prefab",
                "Assets/Game/Prefabs/植株/草/GrassPlant02.prefab",
                "Assets/Game/Prefabs/植株/花/FlowerGrass01.prefab"
            };
            SerializedProperty veg = scData.FindProperty("_vegetationPrefabs");
            veg.arraySize = vegPaths.Length;
            for (int i = 0; i < vegPaths.Length; i++)
            {
                veg.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(vegPaths[i]);
            }
            scData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRockVariety(SerializedProperty element, string path, float weight)
        {
            element.FindPropertyRelative("Preset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<RockPreset>(path);
            element.FindPropertyRelative("Weight").floatValue = weight;
        }
    }
}
