using System;
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
    }
}
