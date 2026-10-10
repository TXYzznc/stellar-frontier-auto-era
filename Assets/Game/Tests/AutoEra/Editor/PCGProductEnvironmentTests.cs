using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AutoEra.Art.PCG;
using AutoEra.Editor;
using AutoEra.Machines;
using AutoEra.Procedures;
using AutoEra.UI;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    public sealed class PCGProductEnvironmentTests
    {
        [Test]
        public void NativeDependenciesAndShadersResolve()
        {
            var settings=AssetDatabase.LoadAssetAtPath<PCGStreamSettings>(PCGEnvironmentInstaller.WorldAsset);
            var surface=AssetDatabase.LoadAssetAtPath<PCGSurfaceSettings>(PCGEnvironmentInstaller.SurfaceAsset);
            Assert.That(settings,Is.Not.Null);Assert.That(surface,Is.Not.Null);settings.Validate();surface.Validate();
            var dependencies=AssetDatabase.GetDependencies(new[]{PCGEnvironmentInstaller.WorldAsset,PCGEnvironmentInstaller.SurfaceAsset},true);
            foreach(string path in dependencies)
            {
                Assert.That(AssetDatabase.LoadMainAssetAtPath(path),Is.Not.Null,path);
                Assert.That(path.Contains("Assets/Art/")||path.Contains("Preview"),Is.False,path);
                if(path.EndsWith(".shader"))Assert.That(ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>(path)),Is.False,path);
            }
        }

        [Test]
        public void ConstructionAreaKeepsLowReliefAndNeighbourSamples()
        {
            var settings=AssetDatabase.LoadAssetAtPath<PCGStreamSettings>(PCGEnvironmentInstaller.WorldAsset);
            var field=new PCGWorldField(settings);
            for(int z=-40;z<=40;z+=4)for(int x=-40;x<=40;x+=4)
                Assert.That(field.Height(x,z),Is.EqualTo(settings.ConstructionBaseHeight).Within(.001f),$"construction height {x},{z}");
            Assert.That(Mathf.Abs(field.Height(48,0)-settings.ConstructionBaseHeight),Is.GreaterThan(.001f),"construction transition must leave the flat zone");
            var a=field.Generate(new Vector2Int(-1,0),CancellationToken.None);
            var b=field.Generate(Vector2Int.zero,CancellationToken.None);
            for(int z=0;z<65;z++)Assert.That(a.Heights[z,64],Is.EqualTo(b.Heights[z,0]),"negative chunk seam");
            for(int z=0;z<65;z++)for(int x=0;x<65;x++)
            {Assert.That(b.Heights[z,x],Is.InRange(0f,1f));Assert.That(b.Environment[z,x].Height,Is.EqualTo(b.Heights[z,x]*settings.HeightScale+settings.HeightOrigin).Within(.00001f));}
            var clone=UnityEngine.Object.Instantiate(settings);
            try
            {
                Vector2 reserved=clone.ReservedSpaces[0].Center;float radius=clone.ReservedSpaces[0].PadRadius;
                clone.SetLandscapeScale(.5f);
                Assert.That(clone.ReservedSpaces[0].Center,Is.EqualTo(reserved));Assert.That(clone.ReservedSpaces[0].PadRadius,Is.EqualTo(radius));
                var resized=new PCGWorldField(clone);Assert.That(Mathf.Abs(resized.Height(39,39)),Is.LessThan(20));
            }
            finally{UnityEngine.Object.DestroyImmediate(clone);}
        }

        [UnityTest,Timeout(600000)]
        public IEnumerator FormalMenuWorldStreamingMachineAndReturn()
        {
            const string launch="Assets/Game/Scene/Launch.unity";
            if(!SceneManager.GetSceneByPath(launch).isLoaded)
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch,OpenSceneMode.Additive));
            yield return new EnterPlayMode();
            // Create captured test state after the Play Mode domain reload.
            yield return VerifyFormalWorld();
            yield return new ExitPlayMode();
        }
        private static IEnumerator VerifyFormalWorld()
        {
            UnityGameFramework.Runtime.UIComponent ui=null;
            yield return Wait(()=>{ui=UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();return ui!=null;},30,"Framework UI ready");
            MainMenuForm menu=null;
            yield return Wait(()=>{menu=Menu(ui);return menu!=null&&menu.EnterButton.interactable;},90,"Main menu ready");
            int originalQuality=QualitySettings.GetQualityLevel();
            var originalPipelines=new UnityEngine.Rendering.RenderPipelineAsset[QualitySettings.names.Length];
            for(int i=0;i<originalPipelines.Length;i++)originalPipelines[i]=QualitySettings.GetRenderPipelineAssetAt(i);
            menu.EnterButton.onClick.Invoke();
            InitialRegionScene entry=null;
            yield return Wait(()=>{entry=Entry();return entry!=null&&entry.Environment!=null&&entry.Environment.World!=null&&entry.Environment.World.InitialReady;},90,"Formal world PCG ready");
            yield return Wait(()=>HasHud(ui),30,"Formal HUD ready");
            Assert.That(entry.Region.Count,Is.EqualTo(7));Assert.That(entry.Navigation,Is.Not.Null);
            var world=entry.Environment.World;var camera=entry.GetComponent<RegionCameraController>();
            yield return Wait(()=>world.DisplaysComplete,60,"Initial dressing ready");
            var placements=new List<PCGPlacement>();
            for(int z=-1;z<=0;z++)for(int x=-1;x<=0;x++)
            {
                world.CopyPlacements(new Vector2Int(x,z),placements);
                foreach(var placement in placements)
                    if(placement.MeshId==world.Settings.GrassMesh.GetInstanceID())
                        Assert.That(world.Field.IsReserved(placement.Matrix.m03,placement.Matrix.m23,.2f),Is.False,"Grass overlaps authored object reserve.");
            }
            Assert.That(camera.ViewCamera.orthographic,Is.True);Assert.That(world.Settings.AutomaticRebase,Is.False);
            var originalSettings=AssetDatabase.LoadAssetAtPath<PCGStreamSettings>(PCGEnvironmentInstaller.WorldAsset);
            Assert.That(world.Settings,Is.Not.SameAs(originalSettings));
            for(int i=0;i<Mathf.Min(3,originalPipelines.Length);i++)
            {
                QualitySettings.SetQualityLevel(i,true);
                yield return Wait(()=>
                {
                    var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
                    return pipeline!=null&&pipeline.shadowDistance>=250;
                },5,"Runtime quality pipeline rebind");
                Assert.That(((UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline).shadowDistance,Is.GreaterThanOrEqualTo(250));
            }
            QualitySettings.SetQualityLevel(originalQuality,true);yield return null;yield return null;
            Assert.That(MachineCatalog.FromLoadedGameData().TryGetMachine(10011,out var definition),Is.True);
            var machine=entry.Session.Machines.Create(definition);
            Assert.That(entry.Region.DeployMachine(machine.Id,new Vector2(20,-25),definition.Footprint,out _),Is.EqualTo(RegionMachineDeploymentResult.Bound));
            Assert.That(entry.TrySpawnMachine(machine.Id,out var reason),Is.True,reason);
            RegionObjectView machineView=null;
            yield return Wait(()=>{machineView=entry.FindMachineView(machine.Id);return machineView!=null;},30,"Formal machine view");
            Assert.That(machineView.GetComponent<PCGGrassInfluence>().Trails,Is.SameAs(entry.Environment.Grass));
            Assert.That(machineView.GetComponent<PCGSurfaceContact>().Response,Is.SameAs(entry.Environment.Surface));
            yield return null;yield return null;
            Assert.That(entry.Environment.Grass.CellCount,Is.GreaterThan(0),"Real machine must stamp grass without preview proxy.");
            var seen=new HashSet<PCGBiome>();
            Directory.CreateDirectory("Docs/ArtPipeline/Evidence/PCGProductIntegration");
            foreach(var point in world.Settings.Landmarks)
            {
                camera.Focus(new Vector3(point.Position.x,0,point.Position.y));
                yield return Wait(()=>!world.FocusPending&&world.CameraFocus==point.Position&&world.DisplaysComplete,75,"Biome "+point.Biome);
                bool first=seen.Add(world.Field.Query(point.Position.x,point.Position.y).Dominant);
                Assert.That(world.GroundCoverage(world.ViewBounds(world.CameraFocus,world.CameraSize,0)),Is.True);
                Assert.That(world.DisplayCount,Is.LessThanOrEqualTo(world.Settings.MaxDisplayedChunks));
                Assert.That(world.CacheCount,Is.LessThanOrEqualTo(world.Settings.MaxCachedChunks));
                if(first)Capture(camera.ViewCamera,"Docs/ArtPipeline/Evidence/PCGProductIntegration/"+point.Biome+".png");
            }
            Assert.That(seen.Count,Is.EqualTo(7));
            var target=new Vector2(-220,-170);camera.Focus(new Vector3(target.x,0,target.y));
            yield return Wait(()=>!world.FocusPending&&world.CameraFocus==target,75,"Negative coordinates");
            world.RequestView(target,100,true,137,25);
            yield return Wait(()=>!world.FocusPending&&Mathf.Abs(world.CameraYaw-137)<.01f&&Mathf.Abs(world.CameraPitch-25)<.01f,75,"Rotated terrain envelope");
            Assert.That(world.CameraSize,Is.EqualTo(20));Assert.That(world.GroundCoverage(world.ViewBounds(target,20,0)),Is.True);
            world.RequestView(target,1,true,320,80);
            yield return Wait(()=>!world.FocusPending&&Mathf.Abs(world.CameraYaw-320)<.01f,75,"Near zoom");
            Assert.That(world.CameraSize,Is.EqualTo(10));
            var procedures=UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.ProcedureComponent>();
            var procedure=(AutoEraWorldProcedure)procedures.GetProcedure<AutoEraWorldProcedure>();procedure.RequestReturnToMenu();
            yield return Wait(()=>{menu=Menu(ui);return menu!=null&&menu.EnterButton.interactable;},45,"Return to formal menu");
            Assert.That(world==null||world.PendingCount==0,Is.True);
            for(int i=0;i<originalPipelines.Length;i++)
                Assert.That(QualitySettings.GetRenderPipelineAssetAt(i),Is.SameAs(originalPipelines[i]),"Quality asset not restored at "+i);
            menu.EnterButton.onClick.Invoke();
            yield return Wait(()=>{entry=Entry();return entry!=null&&entry.Environment?.World!=null&&entry.Environment.World.InitialReady;},90,"Re-enter formal world");
            Assert.That(entry.Environment.Grass.CellCount,Is.Zero);Assert.That(entry.Environment.Surface.History.Count,Is.Zero);
            yield return Wait(()=>entry.Environment.World.DisplaysComplete,60,"Re-enter dressing ready");
            Capture(entry.GetComponent<RegionCameraController>().ViewCamera,"Docs/ArtPipeline/Evidence/PCGProductIntegration/FormalRegion.png");
            File.WriteAllText("Docs/ArtPipeline/Evidence/PCGProductIntegration/runtime-result.txt","PASS: formal menu/HUD, 7 biomes, actual machine contacts, negative coordinates, yaw/pitch coverage, zoom 10..20, exit/re-enter cleanup.");
            ((AutoEraWorldProcedure)procedures.GetProcedure<AutoEraWorldProcedure>()).RequestReturnToMenu();
            yield return Wait(()=>Menu(ui)!=null,45,"Final return");
        }
        private static MainMenuForm Menu(UnityGameFramework.Runtime.UIComponent ui)
        {foreach(var form in ui.GetAllLoadedUIForms())if(form.Logic is MainMenuForm menu)return menu;return null;}
        private static bool HasHud(UnityGameFramework.Runtime.UIComponent ui)
        {foreach(var form in ui.GetAllLoadedUIForms())if(form.Logic is FieldHudForm)return true;return false;}
        private static InitialRegionScene Entry()
        {
            var scene=SceneManager.GetSceneByPath(PCGEnvironmentInstaller.RegionScene);if(!scene.IsValid()||!scene.isLoaded)return null;
            foreach(var root in scene.GetRootGameObjects())if(root.TryGetComponent(out InitialRegionScene entry))return entry;return null;
        }
        private static IEnumerator Wait(Func<bool> predicate,float seconds,string label)
        {
            double until=Time.realtimeSinceStartupAsDouble+seconds;
            while(!predicate()&&Time.realtimeSinceStartupAsDouble<until)yield return null;
            Assert.That(predicate(),Is.True,label);
        }
        private static void Capture(Camera camera,string path)
        {
            var target=RenderTexture.GetTemporary(1280,720,24);var previous=camera.targetTexture;var active=RenderTexture.active;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{camera.targetTexture=previous;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.Destroy(texture);}
        }
    }
}
