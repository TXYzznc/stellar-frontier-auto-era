using System;
using AutoEra.Art.PCG;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AutoEra.World.Region
{
    /// <summary>Scene-owned bridge: semantic camera input and explicit world/contact lifecycle.</summary>
    public sealed class RegionEnvironmentController : MonoBehaviour
    {
        [InspectorName("环境生成配置"),SerializeField] private PCGStreamSettings _settings;
        [InspectorName("地表响应配置"),SerializeField] private PCGSurfaceSettings _surfaceSettings;
        [InspectorName("区域镜头"),SerializeField] private RegionCameraController _camera;
        [InspectorName("环境主光"),SerializeField] private Light _sun;
        private AmbientMode _previousAmbientMode;
        private Color _previousSky,_previousEquator,_previousGround;
        private Material _previousSkybox;
        private Light _previousSun;
        private RenderPipelineAsset _previousQualityPipeline;
        private UniversalRenderPipelineAsset _environmentPipeline;
        private int _pipelineQualityLevel;
        private bool _lightingActive;
        private GameObject _runtimeRoot;
        private Action _ready;
        private Action<string> _failed;
        private float _deadline;
        public PCGStreamWorld World { get; private set; }
        public PCGGrassTrails Grass { get; private set; }
        public PCGSurfaceResponse Surface { get; private set; }
        public Vector3 FocusPosition=>World==null?Vector3.zero:new Vector3(World.CameraFocus.x,0,World.CameraFocus.y);

        public void Begin()
        {
            if(World!=null)return;
            if(_settings==null||_surfaceSettings==null||_camera==null||_camera.ViewCamera==null)
                throw new InvalidOperationException("[AutoEra][Environment] Missing scene environment binding.");
            BeginLighting();
            _runtimeRoot=new GameObject("Environment runtime");_runtimeRoot.SetActive(false);
            _runtimeRoot.transform.SetParent(transform,false);
            World=_runtimeRoot.AddComponent<PCGStreamWorld>();World.AutomaticInitialization=false;
            World.Settings=_settings;World.ViewCamera=_camera.ViewCamera;
            World.Wind=_runtimeRoot.AddComponent<PCGWindField>();
            Grass=_runtimeRoot.AddComponent<PCGGrassTrails>();Grass.World=World;
            Surface=_runtimeRoot.AddComponent<PCGSurfaceResponse>();Surface.World=World;Surface.Settings=_surfaceSettings;
            World.SurfaceResponse=Surface;
            var atmosphere=_runtimeRoot.AddComponent<PCGBiomeAtmosphere>();atmosphere.World=World;atmosphere.ParticleMaterial=_surfaceSettings.ParticleMaterial;
            _runtimeRoot.SetActive(true);World.InitializeStreaming();_camera.BindEnvironment(this);
        }

        public void WaitUntilReady(Action ready,Action<string> failed)
        {
            if(World==null||World.InitialReady){ready?.Invoke();return;}
            _ready=ready;_failed=failed;_deadline=UnityEngine.Time.realtimeSinceStartup+60;
        }
        private void Update()
        {
            if(_ready==null&&_failed==null)return;
            if(World!=null&&World.InitialReady){var callback=_ready;_ready=null;_failed=null;callback?.Invoke();}
            else if(World==null||World.FocusFailure!=null||UnityEngine.Time.realtimeSinceStartup>_deadline)
            {var callback=_failed;_ready=null;_failed=null;callback?.Invoke(World?.FocusFailure??"Environment ground preparation timed out.");}
        }
        private void LateUpdate()
        {
            // QualitySettings.SetQualityLevel can apply its pipeline after Update,
            // and it can do so even when the selected level number is unchanged.
            // Rebind in LateUpdate so the orthographic world keeps its required
            // shadow range on every quality tier.
            if(_lightingActive&&(QualitySettings.GetQualityLevel()!=_pipelineQualityLevel||GraphicsSettings.currentRenderPipeline!=_environmentPipeline))
            {ReleaseQualityPipeline();BindQualityPipeline();}
        }
        public void Apply(Vector2 pan,Vector2 orbit,float zoom,float seconds,float panSpeed,float rotationSpeed,float zoomSpeed,float horizontal,float vertical)
        {
            if(pan==Vector2.zero&&orbit==Vector2.zero&&zoom==0)return;
            float yaw=World.RequestedYaw+orbit.x*rotationSpeed*horizontal;
            float pitch=Mathf.Clamp(World.RequestedPitch-orbit.y*rotationSpeed*vertical,25,80);
            Vector3 motion=Quaternion.Euler(0,yaw,0)*new Vector3(pan.x*horizontal,0,pan.y);
            motion=Vector3.ClampMagnitude(motion,1)*panSpeed*Mathf.Max(0,seconds);
            Vector2 focus=World.RequestedFocus+new Vector2(motion.x,motion.z);
            World.RequestView(focus,World.RequestedSize-zoom*zoomSpeed*vertical,false,yaw,pitch);
        }
        public void Focus(Vector3 position)=>World.RequestView(new Vector2(position.x,position.z),World.RequestedSize,true);
        public void RegisterReceiver(GameObject receiver,float width=1.6f,PCGContactShape shape=PCGContactShape.Wheels)
        {
            if(World==null||receiver==null)return;
            var grass=receiver.GetComponent<PCGGrassInfluence>();
            if(grass==null)grass=receiver.AddComponent<PCGGrassInfluence>();
            if(grass.Trails!=null&&grass.Trails!=Grass)grass.Trails.Unregister(grass);
            grass.Trails=Grass;grass.Radius=Mathf.Clamp(width*.6f,.4f,5);grass.ResetHistory();Grass.Register(grass);
            var contact=receiver.GetComponent<PCGSurfaceContact>();
            if(contact==null)contact=receiver.AddComponent<PCGSurfaceContact>();
            if(contact.Response!=null&&contact.Response!=Surface)contact.Response.Unregister(contact);
            contact.Response=Surface;contact.Width=width;contact.Shape=shape;contact.ResetHistory(true);Surface.Register(contact);
        }
        public void End()
        {
            _ready=null;_failed=null;
            if(_camera!=null)_camera.BindEnvironment(null);
            if(World!=null)World.ShutdownStreaming();
            if(_runtimeRoot!=null){_runtimeRoot.SetActive(false);Destroy(_runtimeRoot);}
            World=null;Grass=null;Surface=null;_runtimeRoot=null;
            EndLighting();
        }
        private void BeginLighting()
        {
            _previousAmbientMode=RenderSettings.ambientMode;_previousSky=RenderSettings.ambientSkyColor;
            _previousEquator=RenderSettings.ambientEquatorColor;_previousGround=RenderSettings.ambientGroundColor;
            _previousSkybox=RenderSettings.skybox;_previousSun=RenderSettings.sun;_lightingActive=true;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.7f,.8f,.86f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.58f,.58f);RenderSettings.ambientGroundColor=new Color(.32f,.36f,.3f);
            RenderSettings.skybox=null;if(_sun!=null)RenderSettings.sun=_sun;
            // The accepted orthographic camera sits farther away than the old orbit camera.
            // Clone quality settings so shadow visibility is adequate without editing the project asset.
            BindQualityPipeline();
        }
        private void BindQualityPipeline()
        {
            _pipelineQualityLevel=QualitySettings.GetQualityLevel();
            var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(pipeline!=null)
            {
                _previousQualityPipeline=QualitySettings.renderPipeline;
                _environmentPipeline=Instantiate(pipeline);_environmentPipeline.hideFlags=HideFlags.DontSave;
                _environmentPipeline.shadowDistance=Mathf.Max(250,pipeline.shadowDistance);
                QualitySettings.renderPipeline=_environmentPipeline;
            }
        }
        private void ReleaseQualityPipeline()
        {
            if(_environmentPipeline==null)return;
            int current=QualitySettings.GetQualityLevel();
            if(QualitySettings.GetRenderPipelineAssetAt(_pipelineQualityLevel)==_environmentPipeline)
            {
                // Unity 2022.3 exposes a setter for the active quality level only.
                // Restore the previous level in the same frame, retaining the player's selected level.
                if(current!=_pipelineQualityLevel)QualitySettings.SetQualityLevel(_pipelineQualityLevel,false);
                QualitySettings.renderPipeline=_previousQualityPipeline;
                if(current!=_pipelineQualityLevel)QualitySettings.SetQualityLevel(current,false);
            }
            Destroy(_environmentPipeline);_environmentPipeline=null;
        }
        private void EndLighting()
        {
            if(!_lightingActive)return;_lightingActive=false;
            RenderSettings.ambientMode=_previousAmbientMode;RenderSettings.ambientSkyColor=_previousSky;
            RenderSettings.ambientEquatorColor=_previousEquator;RenderSettings.ambientGroundColor=_previousGround;
            RenderSettings.skybox=_previousSkybox;RenderSettings.sun=_previousSun;
            ReleaseQualityPipeline();
        }
        private void OnDestroy()=>End();
    }
}
