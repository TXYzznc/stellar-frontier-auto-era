using System;
using System.Collections.Generic;
using UnityEngine;
namespace AutoEra.Art.PCG
{
    public sealed class PCGSurfaceResponse : MonoBehaviour
    {
        [InspectorName("流式环境")] public PCGStreamWorld World;
        [InspectorName("地表响应配置")] public PCGSurfaceSettings Settings;
        public PCGSurfaceHistory History {get;private set;}
        public long LastManagedBytes {get;private set;}
        public double LastUpdateMs {get;private set;}
        public int DroppedSources {get;private set;}
        public long DroppedSamples {get;private set;}
        [InspectorName("启用地表响应")] public bool ResponseEnabled=true;
        public Texture2D ShapeTexture {get;private set;}
        public Texture2D DepositTexture {get;private set;}
        public Vector2 TextureOrigin {get;private set;}
        private Color[] _shape,_deposit;
        private readonly List<PCGSurfaceContact> _sources=new List<PCGSurfaceContact>(32);
        private readonly Vector4[] _rings=new Vector4[32],_ringMeta=new Vector4[32],_ringMotion=new Vector4[32];
        private int _ringIndex,_samples,_seed,_version,_generatorVersion;
        private float _next;
        private ParticleSystem _particles;
        private PCGSurfaceSettings _asset;
        private PCGGrassTrails _grass;
        private readonly System.Diagnostics.Stopwatch _timer=new System.Diagnostics.Stopwatch();
        public void Register(PCGSurfaceContact source)
        {if(_sources.Contains(source))return;if(_sources.Count>=Mathf.Min(32,Settings.MaxSources)){DroppedSources++;return;}_sources.Add(source);}
        public void Unregister(PCGSurfaceContact source)=>_sources.Remove(source);
        private void Awake()
        {
            if(Settings==null||World==null){enabled=false;return;}
            _asset=Settings;Settings=Instantiate(Settings);Settings.hideFlags=HideFlags.DontSave;Settings.Validate();
            History=new PCGSurfaceHistory(Settings.HistoryCapacity);int n=Settings.TextureResolution;
            _grass=GetComponent<PCGGrassTrails>();
            ShapeTexture=new Texture2D(n,n,TextureFormat.RGBAHalf,false,true){name="Surface depth pressure wetness",wrapMode=TextureWrapMode.Clamp};
            DepositTexture=new Texture2D(n,n,TextureFormat.RGBA32,false,true){name="Surface carried material",wrapMode=TextureWrapMode.Clamp};
            _shape=new Color[n*n];_deposit=new Color[n*n];CreateParticles();
        }
        private void CreateParticles()
        {
            var go=new GameObject("Bounded contact powder and leaf fragments");go.transform.SetParent(transform,false);
            _particles=go.AddComponent<ParticleSystem>();var main=_particles.main;main.loop=false;main.playOnAwake=false;
            main.maxParticles=256;main.startLifetime=.8f;main.startSize=.10f;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            main.gravityModifier=.55f;var emission=_particles.emission;emission.enabled=false;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Settings.ParticleMaterial;
            _particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void ClearHistory()
        {History.Clear();Array.Clear(_rings,0,32);Array.Clear(_ringMeta,0,32);for(int i=0;i<_sources.Count;i++)_sources[i].ResetHistory(true);_next=0;}
        public PCGSurfaceProfile Profile(PCGEnvironmentSample e,Vector2 p)
        {
            float patch=World.Field.Noise(p.x,p.y,.075,801);
            var profile=PCGSurfaceProfile.From(e,patch);
            profile.Life=profile.Kind==PCGSurfaceKind.Snow?Settings.SnowLife:profile.Kind==PCGSurfaceKind.Sand?Settings.SandLife:
                profile.Kind==PCGSurfaceKind.Mud?Settings.MudLife:Settings.HardLife;
            return profile;
        }
        public void Contact(Vector2 p,Vector2 forward,Vector2 radius,float distance,float now,ref PCGSurfaceCarry carry,bool firstContact=false)
        {
            if(!ResponseEnabled)return;
            if(_samples++>=Settings.MaxSamplesPerTick){DroppedSamples++;return;}
            var e=World.Field.Query(p.x,p.y);var profile=Profile(e,p);
            carry.Step(profile.Kind,distance,now,true);
            if(profile.Kind==PCGSurfaceKind.Water||profile.Kind==PCGSurfaceKind.Lava)
            {
                if(distance>.015f||firstContact)
                {
                    _rings[_ringIndex]=new Vector4(p.x,p.y,now,Mathf.Clamp01(.35f+distance));
                    _ringMeta[_ringIndex]=new Vector4(e.LiquidId&65535,e.LiquidId>>16,e.LiquidLevel,profile.Kind==PCGSurfaceKind.Lava?1:0);
                    _ringMotion[_ringIndex]=new Vector4(forward.x,forward.y,distance,0);
                    _ringIndex=(_ringIndex+1)%32;
                }
                return;
            }
            float depositLife=carry.Solid==PCGSurfaceKind.Snow&&e.Temperature>.4f?12:carry.Solid==PCGSurfaceKind.Sand?18:25;
            History.Stamp(p,forward,radius,profile,carry.Deposition(profile.Kind),carry.Wet,now,e.Height,depositLife);
            if(_grass!=null)_grass.StampSegment(p,p,Mathf.Max(radius.x,radius.y),.8f,forward,now);
            if(Settings.ContactEffects&&World.Settings.SecondaryEffects&&distance>.08f&&((_samples&7)==0)&&
                (p-World.CameraFocus).sqrMagnitude<Settings.DisplaySpan*Settings.DisplaySpan*.20f)
            {
                PCGSurfaceKind powderKind=profile.Kind;
                if(profile.Kind==PCGSurfaceKind.Hard&&carry.Amount>.02f)powderKind=carry.Solid;
                else if(profile.Kind==PCGSurfaceKind.Hard&&carry.Wet>.4f)powderKind=PCGSurfaceKind.Water;
                Color color=powderKind==PCGSurfaceKind.Snow?new Color(.88f,.94f,1):powderKind==PCGSurfaceKind.Sand?new Color(.73f,.60f,.38f):
                    powderKind==PCGSurfaceKind.Mud?new Color(.24f,.29f,.18f):powderKind==PCGSurfaceKind.Water?new Color(.60f,.77f,.83f,.65f):new Color(.46f,.43f,.27f);
                Vector2 wind=World.Wind!=null?World.Wind.Sample(p,now):Vector2.zero;
                var emit=new ParticleSystem.EmitParams{position=new Vector3(p.x,e.Height+.23f,p.y),
                    velocity=new Vector3(wind.x*.18f,profile.Kind==PCGSurfaceKind.Mud?.7f:1.1f,wind.y*.18f),startColor=color,startSize=.06f+radius.x*.08f};
                // Local simulation uses absolute XZ; moving the root applies origin shifts to living particles.
                _particles.Emit(emit,2);
            }
        }
        private void LateUpdate()
        {
            if(History==null||World.Field==null)return;
            long before=GC.GetAllocatedBytesForCurrentThread();_timer.Restart();
            if(_seed!=World.Settings.Seed||_version!=Settings.Version||_generatorVersion!=World.Settings.Version)
            {ClearHistory();_seed=World.Settings.Seed;_version=Settings.Version;_generatorVersion=World.Settings.Version;}
            Shader.SetGlobalFloat("_AESurfaceActive",ResponseEnabled?1:0);
            if(Time.time>=_next&&ResponseEnabled)
            {
                _next=Time.time+Settings.UpdateInterval;_samples=0;
                for(int i=0;i<_sources.Count;i++)if(_sources[i]!=null&&_sources[i].isActiveAndEnabled)_sources[i].Sample(Time.time);
                History.Clean(Time.time,512);PushTextures(Time.time);
            }
            _particles.transform.localPosition=new Vector3(-World.WorldOrigin.x,0,-World.WorldOrigin.y);
            Shader.SetGlobalFloat("_AESurfaceTime",Time.time);
            Shader.SetGlobalVectorArray("_AERings",_rings);Shader.SetGlobalVectorArray("_AERingMeta",_ringMeta);
            Shader.SetGlobalVectorArray("_AERingMotion",_ringMotion);
            _timer.Stop();LastUpdateMs=_timer.Elapsed.TotalMilliseconds;LastManagedBytes=GC.GetAllocatedBytesForCurrentThread()-before;
        }
        public void PushTextures(float now)
        {
            int n=Settings.TextureResolution;float span=Settings.DisplaySpan,step=span/n;
            TextureOrigin=new Vector2(Mathf.Floor((World.CameraFocus.x-span*.5f)/step)*step,Mathf.Floor((World.CameraFocus.y-span*.5f)/step)*step);
            Array.Clear(_shape,0,_shape.Length);Array.Clear(_deposit,0,_deposit.Length);
            for(int i=0;i<History.Count;i++)
            {
                var cell=History.At(i);float r=PCGSurfaceHistory.Remaining(cell,now),d=PCGSurfaceHistory.DepositRemaining(cell,now);if(r<=0&&d<=0)continue;
                int x=Mathf.FloorToInt(((cell.Key.x+.5f)*PCGSurfaceHistory.CellSize-TextureOrigin.x)/step);
                int z=Mathf.FloorToInt(((cell.Key.y+.5f)*PCGSurfaceHistory.CellSize-TextureOrigin.y)/step);
                if(x<0||z<0||x>=n||z>=n)continue;int index=z*n+x;
                Color deposit=cell.Deposit;deposit.a*=d;
                float depth=cell.Depth*r*r,pressure=Mathf.Max(cell.Pressure*r,deposit.a*.15f),wet=cell.Wet*r;
                float heightWeight=Mathf.Max(Mathf.Max(pressure,wet),Mathf.Abs(depth)*4);
                _shape[index]=new Color(depth,pressure,wet,cell.Height*heightWeight);
                _deposit[index]=deposit;
            }
            ShapeTexture.SetPixels(_shape);ShapeTexture.Apply(false,false);DepositTexture.SetPixels(_deposit);DepositTexture.Apply(false,false);
            Shader.SetGlobalTexture("_AESurfaceShape",ShapeTexture);Shader.SetGlobalTexture("_AESurfaceDeposit",DepositTexture);
            Shader.SetGlobalVector("_AESurfaceRect",new Vector4(TextureOrigin.x,TextureOrigin.y,span,1f/n));
        }
        private void OnDisable(){Shader.SetGlobalFloat("_AESurfaceActive",0);}
        private void OnDestroy()
        {Destroy(ShapeTexture);Destroy(DepositTexture);if(_asset!=null){Destroy(Settings);Settings=_asset;}}
    }
}
