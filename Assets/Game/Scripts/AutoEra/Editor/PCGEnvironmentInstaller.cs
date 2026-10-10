using System;
using System.IO;
using AutoEra.Art.PCG;
using AutoEra.World.Region;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoEra.Editor
{
    public static class PCGEnvironmentInstaller
    {
        public const string WorldAsset="Assets/Game/ScriptableAssets/Environment/EnvironmentWorld.asset";
        public const string SurfaceAsset="Assets/Game/ScriptableAssets/Environment/EnvironmentSurface.asset";
        public const string RegionScene="Assets/Game/Scene/InitialRegion.unity";
        [MenuItem("Game Framework/AutoEra/Environment/Install Accepted Environment")]
        public static void Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode before installation.");
            foreach(var scene in Scenes())if(scene.isDirty)throw new InvalidOperationException("Preserve dirty scenes before installation: "+scene.path);
            var settings=AssetDatabase.LoadAssetAtPath<PCGStreamSettings>(WorldAsset);
            var surface=AssetDatabase.LoadAssetAtPath<PCGSurfaceSettings>(SurfaceAsset);
            if(settings==null||surface==null)throw new InvalidOperationException("Environment settings missing.");
            var original=SceneManager.GetActiveScene();var region=SceneManager.GetSceneByPath(RegionScene);bool opened=!region.isLoaded;
            string backup="Library/PCGEnvironment/InitialRegion.before-environment.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(backup));if(!File.Exists(backup))File.Copy(RegionScene,backup);
            try
            {
                if(opened)region=EditorSceneManager.OpenScene(RegionScene,OpenSceneMode.Additive);
                InitialRegionScene entry=null;
                foreach(var root in region.GetRootGameObjects())if(root.TryGetComponent(out InitialRegionScene found))entry=found;
                if(entry==null)throw new InvalidOperationException("InitialRegionScene missing.");
                var camera=entry.GetComponent<RegionCameraController>();
                if(camera==null||camera.ViewCamera==null)throw new InvalidOperationException("Region camera binding missing.");
                var data=new SerializedObject(entry);var bounds=data.FindProperty("_bounds").rectValue;
                settings.HeightOrigin=-12;settings.PreserveConstructionRegion=true;settings.ConstructionRegion=bounds;
                settings.name="EnvironmentWorld";surface.name="EnvironmentSurface";EditorUtility.SetDirty(surface);
                settings.ConstructionBlend=24;settings.ConstructionEdgeNoise=.35f;
                settings.ConstructionRelief=0;settings.ConstructionBaseHeight=0;
                settings.StartClimateBias=.70f;
                settings.ReliefScale=1.10f;settings.HeightScale=96;settings.AutomaticRebase=false;
                settings.CameraMinSize=10;settings.CameraMaxSize=20;settings.InitialCameraSize=14;
                settings.InitialCameraFocus=new Vector2(8,12);
                var seeds=data.FindProperty("_objects");var spaces=new PCGReservedSpace[seeds.arraySize];
                for(int i=0;i<spaces.Length;i++)
                {
                    var view=(RegionObjectView)seeds.GetArrayElementAtIndex(i).objectReferenceValue;
                    var viewData=new SerializedObject(view);Vector2 footprint=viewData.FindProperty("_footprint").vector2Value;
                    Vector3 p=view.transform.position;
                    spaces[i]=new PCGReservedSpace{Name=view.name,Center=new Vector2(p.x,p.z),PadRadius=footprint.magnitude*.5f+.8f};
                }
                settings.ReservedSpaces=spaces;settings.Validate();surface.Validate();EditorUtility.SetDirty(settings);
                var bridge=entry.GetComponent<RegionEnvironmentController>();
                if(bridge==null)bridge=entry.gameObject.AddComponent<RegionEnvironmentController>();
                var binding=new SerializedObject(bridge);
                binding.FindProperty("_settings").objectReferenceValue=settings;
                binding.FindProperty("_surfaceSettings").objectReferenceValue=surface;
                binding.FindProperty("_camera").objectReferenceValue=camera;binding.ApplyModifiedPropertiesWithoutUndo();
                Light sun=null;
                foreach(var root in region.GetRootGameObjects())
                    if(root.TryGetComponent(out Light light)&&light.type==LightType.Directional){sun=light;break;}
                if(sun==null)throw new InvalidOperationException("Explicit region directional light missing.");
                sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.color=new Color(1,.95f,.85f);
                sun.intensity=1.12f;sun.shadows=LightShadows.Soft;
                binding.Update();binding.FindProperty("_sun").objectReferenceValue=sun;binding.ApplyModifiedPropertiesWithoutUndo();
                data.FindProperty("_environment").objectReferenceValue=bridge;data.ApplyModifiedPropertiesWithoutUndo();
                var ground=(MeshFilter)data.FindProperty("_navigationGround").objectReferenceValue;
                if(ground==null)throw new InvalidOperationException("Explicit navigation source missing.");
                var renderer=ground.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=false;
                var scatter=entry.GetComponent<AutoEra.PCG.RuntimeScatterer>();if(scatter!=null)scatter.enabled=false;
                camera.ViewCamera.orthographic=true;camera.ViewCamera.orthographicSize=14;
                camera.ViewCamera.clearFlags=CameraClearFlags.SolidColor;camera.ViewCamera.backgroundColor=new Color(.66f,.77f,.8f);
                EditorSceneManager.MarkSceneDirty(region);EditorSceneManager.SaveScene(region);
                AssetDatabase.SaveAssetIfDirty(settings);
                AssetDatabase.SaveAssetIfDirty(surface);
                Debug.Log("[AutoEra][Environment] Installation passed: 7-biome assets, region pad, camera, navigation source preserved.");
            }
            finally
            {
                if(opened&&region.isLoaded)EditorSceneManager.CloseScene(region,true);
                if(original.IsValid()&&original.isLoaded)SceneManager.SetActiveScene(original);
            }
        }
        private static System.Collections.Generic.IEnumerable<Scene> Scenes()
        {for(int i=0;i<SceneManager.sceneCount;i++)yield return SceneManager.GetSceneAt(i);}
    }
}
