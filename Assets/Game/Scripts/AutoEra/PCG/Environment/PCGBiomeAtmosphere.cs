using UnityEngine;

namespace AutoEra.Art.PCG
{
    /// <summary>Bounded camera-local optional dust/snow/ash. Shared wind input, no gameplay force.</summary>
    public sealed class PCGBiomeAtmosphere : MonoBehaviour
    {
        [InspectorName("流式环境")] public PCGStreamWorld World;
        [InspectorName("环境粒子材质")] public Material ParticleMaterial;
        private ParticleSystem _particles;
        private Material _material;
        private float _nextSample;
        private void Start()
        {
            var go=new GameObject("Optional biome atmosphere");go.transform.SetParent(transform,false);
            _particles=go.AddComponent<ParticleSystem>();
            _particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=_particles.main;main.maxParticles=220;main.startLifetime=6;
            main.startSpeed=.3f;main.startSize=.065f;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var shape=_particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(65,9,65);
            var emission=_particles.emission;emission.rateOverTime=0;
            _material=new Material(ParticleMaterial);
            _material.SetFloat("_Surface",1);_material.SetFloat("_Blend",0);
            _material.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _material.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _material.SetFloat("_ZWrite",0);_material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _material.renderQueue=3000;_material.SetColor("_BaseColor",Color.white);
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=_material;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            _particles.Play();
        }
        private void Update()
        {
            if(World.Field==null||!World.InitialReady||_particles==null||Time.time<_nextSample)return;
            _nextSample=Time.time+.3f;
            var sample=World.Field.Query(World.CameraFocus.x,World.CameraFocus.y);
            bool snow=sample.Cover.Snow>.55f,dust=sample.Cover.Sand>.55f,ash=sample.Cover.Volcanic>.6f;
            var emission=_particles.emission;
            emission.rateOverTime=World.Settings.SecondaryEffects?(snow?20:dust?9:ash?7:0):0;
            _particles.transform.position=new Vector3(World.CameraFocus.x-World.WorldOrigin.x,sample.Height+5,World.CameraFocus.y-World.WorldOrigin.y);
            var main=_particles.main;
            main.startColor=snow?new Color(.92f,.96f,1,.48f):dust?new Color(.78f,.66f,.43f,.12f):new Color(.35f,.28f,.23f,.20f);
            main.startSize=snow?.07f:dust?.35f:.05f;
            Vector2 wind=World.Wind.Sample(World.CameraFocus,Time.time);
            var velocity=_particles.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;
            velocity.x=wind.x*.6f;velocity.z=wind.y*.6f;velocity.y=snow?-.6f:ash?.25f:0;
            if(!World.Settings.SecondaryEffects)_particles.Clear();
        }
        private void OnDestroy(){if(_material!=null)Destroy(_material);}
    }
}
