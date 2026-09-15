using System;
using System.Linq;
using AutoEra.World.Region;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor
{
    public static class InitialRegionEntityBuilder
    {
        [MenuItem("Game Framework/AutoEra/Improve Initial Region Proxies")]
        public static void ImproveProxies()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            string[] names = { "Warehouse", "Farmland", "Forest", "MineralVein", "Water", "Generator", "SolarArray" };
            Color[] tints = { new Color(.45f,.42f,.37f), new Color(.30f,.20f,.13f), new Color(.36f,.28f,.18f),
                new Color(.42f,.36f,.48f), new Color(.15f,.32f,.46f),new Color(.32f,.34f,.37f),new Color(.17f,.23f,.32f) };
            for (int i=0;i<names.Length;i++)
            {
                string path="Assets/Game/Prefabs/Entity/InitialRegion/"+names[i]+".prefab";
                GameObject root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var data=new SerializedObject(root.GetComponent<RegionObjectView>());
                    data.FindProperty("_tintProxy").boolValue=true;
                    data.FindProperty("_proxyTint").colorValue=tints[i];
                    data.ApplyModifiedPropertiesWithoutUndo();
                    // Only basic non-colliding stand-ins; they carry no tree/grid/ore quantity authority.
                    Transform visual=root.transform.Find("Visual");
                    if (visual != null && visual.Find("ProxyDetails") == null && (names[i]=="Forest" || names[i]=="MineralVein"))
                    {
                        var details=new GameObject("ProxyDetails"); details.transform.SetParent(visual,false);
                        for(int n=0;n<9;n++)
                        {
                            var marker=GameObject.CreatePrimitive(names[i]=="Forest" ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
                            marker.name="Proxy_"+n; marker.transform.SetParent(details.transform,false);
                            // Visual root is the existing scaled footprint cube: preserve its transform.
                            marker.transform.localPosition=new Vector3((n%3-1)*.28f,names[i]=="Forest"?7f:1.3f,(n/3-1)*.28f);
                            marker.transform.localScale=names[i]=="Forest"?new Vector3(.08f,7f,.08f):new Vector3(.12f,2f,.12f);
                            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        [MenuItem("Game Framework/AutoEra/Bind Initial Region Work Channels")]
        public static void BindWorkChannels()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            string[] names = { "Warehouse", "Farmland", "Forest", "MineralVein" };
            string[][] channels = { new[] { "物流" }, new[] { "照料", "综合" }, new[] { "采集" }, new[] { "开采" } };
            for (int i = 0; i < names.Length; i++)
            {
                string path = "Assets/Game/Prefabs/Entity/InitialRegion/" + names[i] + ".prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var data = new SerializedObject(root.GetComponent<RegionObjectView>());
                    var values = data.FindProperty("_workChannels");
                    values.arraySize = channels[i].Length;
                    for (int j = 0; j < channels[i].Length; j++) values.GetArrayElementAtIndex(j).stringValue = channels[i][j];
                    data.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        [MenuItem("Game Framework/AutoEra/Bind Initial Region Entities")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).path == path) throw new InvalidOperationException("Close InitialRegion before controlled binding.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var entry = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<InitialRegionScene>(true)).Single();
                // Launch owns the sole listener; additive world content must not add another one.
                foreach (GameObject sceneRoot in scene.GetRootGameObjects())
                    foreach (AudioListener listener in sceneRoot.GetComponentsInChildren<AudioListener>(true))
                        UnityEngine.Object.DestroyImmediate(listener);
                var data = new SerializedObject(entry);
                var objects = data.FindProperty("_objects");
                string[] names = { "Warehouse", "Farmland", "Forest", "MineralVein", "Water", "Generator", "SolarArray" };
                if (objects.arraySize != names.Length) throw new InvalidOperationException("Unexpected seed count.");
                const string folder = "Assets/Game/Prefabs/Entity/InitialRegion";
                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Prefabs/Entity", "InitialRegion");
                var paths = data.FindProperty("_entityPrefabs");
                paths.arraySize = names.Length;
                for (int i = 0; i < names.Length; i++)
                {
                    var seed = objects.GetArrayElementAtIndex(i).objectReferenceValue as RegionObjectView;
                    if (seed == null) throw new InvalidOperationException("Missing seed.");
                    string prefabPath = folder + "/" + names[i] + ".prefab";
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
                    {
                        var root = UnityEngine.Object.Instantiate(seed.gameObject);
                        try
                        {
                            root.name = names[i]; root.transform.position = Vector3.zero; root.transform.rotation = Quaternion.identity;
                            root.SetActive(true);
                            root.AddComponent<InitialRegionEntity>();
                            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                        }
                        finally { UnityEngine.Object.DestroyImmediate(root); }
                    }
                    paths.GetArrayElementAtIndex(i).stringValue = "InitialRegion/" + names[i];
                }
                data.ApplyModifiedPropertiesWithoutUndo();
                if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Save failed.");
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }
    }
}
