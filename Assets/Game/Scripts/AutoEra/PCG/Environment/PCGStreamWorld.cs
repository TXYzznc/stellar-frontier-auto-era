using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Debug=UnityEngine.Debug;

namespace AutoEra.Art.PCG
{
    public sealed partial class PCGStreamWorld : MonoBehaviour
    {
        private sealed class Cached { public PCGChunkData Data; public float Used; }
        private sealed class Pending { public Task<PCGChunkData> Task; public CancellationTokenSource Cancel; public bool Injected; }
        private struct Candidate { public Vector2Int Key; public float Priority; }
        private sealed class CandidateOrder : IComparer<Candidate>
        {
            public static readonly CandidateOrder Instance=new CandidateOrder();
            public int Compare(Candidate a,Candidate b)=>a.Priority.CompareTo(b.Priority);
        }
        [InspectorName("环境生成配置")] public PCGStreamSettings Settings;
        [InspectorName("观察镜头")] public Camera ViewCamera;
        [InspectorName("风场")] public PCGWindField Wind;
        [InspectorName("地表响应")] public PCGSurfaceResponse SurfaceResponse;
        [InspectorName("自动初始化")] public bool AutomaticInitialization = true;
        public float CameraYaw { get; private set; }
        public float CameraPitch { get; private set; }=50;
        public float RequestedYaw=>_hasRequest?_requestedYaw:CameraYaw;
        public float RequestedPitch=>_hasRequest?_requestedPitch:CameraPitch;
        public PCGWorldField Field { get; private set; }
        public Vector2 WorldOrigin { get; private set; }
        public Vector2 CameraFocus { get; private set; }=new Vector2(38,32);
        public float CameraSize { get; private set; }=24;
        public bool InitialReady { get; private set; }
        public bool FocusPending { get; private set; }
        public Vector2 RequestedFocus=>_hasRequest?_requestedFocus:CameraFocus;
        public float RequestedSize=>_hasRequest?_requestedSize:CameraSize;
        public string FocusFailure { get; private set; }
        public int DisplayCount=>_views.Count;
        public int CacheCount=>_cache.Count;
        public int PendingCount=>_pending.Count;
        public int PoolCount=>_pool.Count;
        public int CompletedJobs { get; private set; }
        public int EvictedViews { get; private set; }
        public int DiscardedJobs { get; private set; }
        public bool DisplaysComplete
        {
            get { foreach(var v in _views.Values)if(!v.Complete)return false;return true; }
        }
        public double LastBuildMs { get; private set; }
        public double MaximumBuildMs { get; private set; }
        public double MaximumDataMs { get; private set; }
        [InspectorName("数据准备延迟秒数")] public float DataDelaySeconds;
        [InspectorName("注入准备失败（测试）")] public bool InjectPreparationFailure;
        private readonly Dictionary<Vector2Int,Cached> _cache=new Dictionary<Vector2Int,Cached>(256);
        private readonly Dictionary<Vector2Int,Pending> _pending=new Dictionary<Vector2Int,Pending>(8);
        private readonly Dictionary<Vector2Int,PCGChunkView> _views=new Dictionary<Vector2Int,PCGChunkView>(128);
        private readonly Stack<PCGChunkView> _pool=new Stack<PCGChunkView>(8);
        private readonly List<Vector2Int> _remove=new List<Vector2Int>(256);
        private readonly List<Candidate> _candidates=new List<Candidate>(512);
        private readonly Stopwatch _budget=new Stopwatch();
        private PCGChunkView _building;
        private Rect _display,_prepare,_retain,_requestedDisplay,_requestedPrepare;
        private Vector2 _requestedFocus;
        private float _requestedSize,_nextPlan;
        private bool _hasRequest,_started;
        private PCGStreamSettings _sourceSettings;
        private float _requestedYaw,_requestedPitch;
        private int _cameraMask;
        private void Start()
        {if(AutomaticInitialization)InitializeStreaming();}
        public void InitializeStreaming()
        {
            if(_started)return;
            if(Settings==null||ViewCamera==null)throw new InvalidOperationException("Environment configuration or camera missing.");
            Settings.Validate();_sourceSettings=Settings;
            // Runtime toggles must not mutate the persistent configuration asset.
            Settings=Instantiate(Settings);Settings.hideFlags=HideFlags.DontSave;
            Field=new PCGWorldField(Settings);_started=true;
            _cameraMask=ViewCamera.cullingMask;WorldOrigin=Vector2.zero;
            InitialReady=false;_hasRequest=false;_building=null;_nextPlan=0;
            CameraFocus=Settings.InitialCameraFocus;CameraSize=Settings.InitialCameraSize;
            UpdateCamera();RequestView(CameraFocus,CameraSize,false);
        }
        public Rect ViewBounds(Vector2 focus,float size,float margin)
            =>ViewBounds(focus,size,margin,CameraYaw,CameraPitch);
        private Rect ViewBounds(Vector2 focus,float size,float margin,float yaw,float pitch)
        {
            float halfX=size*Mathf.Max(.1f,ViewCamera.aspect);
            float halfZ=size/Mathf.Sin(pitch*Mathf.Deg2Rad),cot=1/Mathf.Tan(pitch*Mathf.Deg2Rad);
            float centerHeight=FocusHeight(focus),bottom=Settings.HeightOrigin;
            float low=-halfZ-(bottom+Settings.HeightScale+Settings.DecorationHeightEnvelope-centerHeight)*cot;
            float high=halfZ+(centerHeight-bottom)*cot;
            float sin=Mathf.Sin(yaw*Mathf.Deg2Rad),cos=Mathf.Cos(yaw*Mathf.Deg2Rad);
            float minX=float.MaxValue,minZ=float.MaxValue,maxX=float.MinValue,maxZ=float.MinValue;
            for(int i=0;i<4;i++)
            {
                float x=(i&1)==0?-halfX:halfX,z=(i&2)==0?low:high;
                float rx=x*cos+z*sin,rz=-x*sin+z*cos;
                minX=Mathf.Min(minX,rx);maxX=Mathf.Max(maxX,rx);minZ=Mathf.Min(minZ,rz);maxZ=Mathf.Max(maxZ,rz);
            }
            return Rect.MinMaxRect(focus.x+minX-margin,focus.y+minZ-margin,focus.x+maxX+margin,focus.y+maxZ+margin);
        }
        private float FocusHeight(Vector2 focus)
        {
            var e=Field.Query(focus.x,focus.y);
            return e.Liquid==PCGLiquidKind.None?e.Height:Mathf.Max(e.Height,e.LiquidLevel);
        }
        public void RequestView(Vector2 focus,float size,bool remote)
            =>RequestView(focus,size,remote,RequestedYaw,RequestedPitch);
        public void RequestView(Vector2 focus,float size,bool remote,float yaw,float pitch)
        {
            if(!_started)return;
            if(float.IsNaN(focus.x)||float.IsNaN(focus.y)||float.IsNaN(size)||float.IsNaN(yaw)||float.IsNaN(pitch))return;
            size=ClampCameraSize(size);
            if(Mathf.Abs(focus.x)>1000000||Mathf.Abs(focus.y)>1000000)
            {FocusFailure="Target exceeds tested +/- 1,000,000m prototype range.";return;}
            _requestedFocus=focus;_requestedSize=size;_hasRequest=true;FocusPending=remote;
            _requestedYaw=Mathf.Repeat(yaw,360);_requestedPitch=Mathf.Clamp(pitch,25,80);
            FocusFailure=null;_nextPlan=0;
        }
        public bool GroundCoverage(Rect bounds)
        {
            Vector2Int lo=Field.Chunk(bounds.xMin,bounds.yMin),hi=Field.Chunk(bounds.xMax,bounds.yMax);
            for(int z=lo.y;z<=hi.y;z++)for(int x=lo.x;x<=hi.x;x++)
                if(!_views.TryGetValue(new Vector2Int(x,z),out var view)||!view.GroundReady)return false;
            return true;
        }
        public void CancelFocus()
        {
            _hasRequest=false;FocusPending=false;_nextPlan=0;
        }
        public bool TryGetLayoutSignature(Vector2Int key,out ulong signature)
        {
            signature=0;
            if(!_views.TryGetValue(key,out var view)||!view.Complete)return false;
            signature=view.LayoutSignature();return true;
        }
        public void CopyPlacements(Vector2Int key,List<PCGPlacement> placements)
        {
            placements.Clear();if(_views.TryGetValue(key,out var view))view.CopyWorldPlacements(placements);
        }
        private void OnEnable(){RenderPipelineManager.beginCameraRendering+=DrawCamera;}
        private void OnDisable(){RenderPipelineManager.beginCameraRendering-=DrawCamera;}
#if UNITY_EDITOR
        public long LastManagedBytes { get; private set; }
        public long DrawManagedBytes { get; private set; }
#endif
        private void Update()
        {
#if UNITY_EDITOR
            long before=GC.GetAllocatedBytesForCurrentThread();
#endif
            TickStreaming();
#if UNITY_EDITOR
            LastManagedBytes=GC.GetAllocatedBytesForCurrentThread()-before;
#endif
        }
        private void TickStreaming()
        {
            if(!_started)return;
            CollectJobs();
            if(Time.time>=_nextPlan){Plan();_nextPlan=Time.time+.12f;}
            _budget.Restart();
            if(_building!=null)
            {
                while(_building!=null&&_budget.Elapsed.TotalMilliseconds<Settings.FrameBudgetMs)
                {
                    bool groundWasReady=_building.GroundReady;_building.BuildStep();
                    if(!groundWasReady&&_building.GroundReady)LinkNeighbors();
                    if(_building.Complete){_building=null;LinkNeighbors();}
                }
            }
            LastBuildMs=_budget.Elapsed.TotalMilliseconds;MaximumBuildMs=Math.Max(MaximumBuildMs,LastBuildMs);
            if(_hasRequest&&GroundCoverage(_requestedDisplay))
            {
                CameraFocus=_requestedFocus;CameraSize=_requestedSize;CameraYaw=_requestedYaw;CameraPitch=_requestedPitch;
                _hasRequest=false;FocusPending=false;InitialReady=true;
                if(Settings.AutomaticRebase&&(CameraFocus-WorldOrigin).sqrMagnitude>1024*1024)Rebase(CameraFocus);
                UpdateCamera();_nextPlan=0;
            }
        }
        private void DrawCamera(ScriptableRenderContext context,Camera camera)
        {
            if(!_started||!InitialReady||camera!=ViewCamera)return;
#if UNITY_EDITOR
            long before=GC.GetAllocatedBytesForCurrentThread();
#endif
            foreach(var view in _views.Values)view.Draw(ViewCamera);
#if UNITY_EDITOR
            DrawManagedBytes=GC.GetAllocatedBytesForCurrentThread()-before;
#endif
        }
        private void CollectJobs()
        {
            _remove.Clear();
            foreach(var pair in _pending)
            {
                var p=pair.Value;if(!p.Task.IsCompleted)continue;
                if(p.Task.Status==TaskStatus.RanToCompletion)
                {
                    if(p.Task.Result.Version==Field.Version&&p.Task.Result.LandscapeScale==Field.LandscapeScale
                        &&p.Task.Result.ReliefScale==Field.ReliefScale)
                    {_cache[pair.Key]=new Cached{Data=p.Task.Result,Used=Time.time};CompletedJobs++;}
                    else DiscardedJobs++;
                }
                else if(p.Task.IsFaulted)
                {
                    if(p.Injected)Debug.LogWarning("[PCGStream] Expected test preparation failure.");
                    else Debug.LogError("[PCGStream] Data generation failed: "+p.Task.Exception);
                    FocusFailure="Data preparation failed.";FocusPending=false;_hasRequest=false;DiscardedJobs++;
                }
                else DiscardedJobs++;
                p.Cancel.Dispose();_remove.Add(pair.Key);
            }
            for(int i=0;i<_remove.Count;i++)_pending.Remove(_remove[i]);
        }
        private bool InBounds(Vector2Int key,Rect bounds)
        {
            float s=Settings.ChunkSize;
            return key.x*s<bounds.xMax&&(key.x+1)*s>bounds.xMin&&key.y*s<bounds.yMax&&(key.y+1)*s>bounds.yMin;
        }
        private void Plan()
        {
            _display=ViewBounds(CameraFocus,CameraSize,Settings.DisplayMargin);
            _prepare=ViewBounds(CameraFocus,CameraSize,Settings.DisplayMargin+Settings.PrepareMargin);
            _retain=ViewBounds(CameraFocus,CameraSize,Settings.EvictMargin);
            _requestedDisplay=_hasRequest?ViewBounds(_requestedFocus,_requestedSize,Settings.DisplayMargin,_requestedYaw,_requestedPitch):_display;
            _requestedPrepare=_hasRequest?ViewBounds(_requestedFocus,_requestedSize,Settings.DisplayMargin+Settings.PrepareMargin,_requestedYaw,_requestedPitch):_prepare;
            _remove.Clear();
            foreach(var pair in _views)
            {
                if(InBounds(pair.Key,_display)||_hasRequest&&InBounds(pair.Key,_requestedDisplay))pair.Value.LastWanted=Time.time;
                if(!InBounds(pair.Key,_retain)&&!(_hasRequest&&InBounds(pair.Key,_requestedDisplay))
                    &&Time.time-pair.Value.LastWanted>=Settings.RetainSeconds&&pair.Value!=_building)_remove.Add(pair.Key);
            }
            for(int i=0;i<_remove.Count;i++)
            {
                var v=_views[_remove[i]];v.Suspend();_views.Remove(_remove[i]);EvictedViews++;
                if(_pool.Count<4)_pool.Push(v);else v.Dispose();
            }
            if(_remove.Count>0)LinkNeighbors();
            foreach(var pair in _pending)
                if(!InBounds(pair.Key,_prepare)&&!(_hasRequest&&InBounds(pair.Key,_requestedPrepare)))pair.Value.Cancel.Cancel();
            _candidates.Clear();AddCandidates(_prepare,CameraFocus);if(_hasRequest)AddCandidates(_requestedPrepare,_requestedFocus);
            _candidates.Sort(CandidateOrder.Instance);
            for(int i=0;i<_candidates.Count;i++)
            {
                var key=_candidates[i].Key;
                bool wanted=InBounds(key,_display)||_hasRequest&&InBounds(key,_requestedDisplay);
                if(_cache.TryGetValue(key,out Cached cached))
                {
                    cached.Used=Time.time;
                    if(wanted&&!_views.ContainsKey(key)&&(_building==null||_building.GroundReady)&&(_pool.Count>0||_views.Count<Settings.MaxDisplayedChunks))
                    {
                        _building=_pool.Count>0?_pool.Pop():new PCGChunkView(this);
                        _building.Begin(cached.Data);_building.LastWanted=Time.time;_views.Add(key,_building);
                        // Only one new view per plan; otherwise several unfinished ground views
                        // were replaced before BuildStep, creating unbounded initial latency.
                        if(Settings.MultiBiome)break;
                    }
                    else if(wanted&&_building==null&&_views.TryGetValue(key,out var unfinished)&&!unfinished.Complete)
                        _building=unfinished;
                }
                else if(!_pending.ContainsKey(key)&&_pending.Count<Settings.MaxConcurrentDataJobs&&_cache.Count+_pending.Count<Settings.MaxCachedChunks)
                {
                    var cts=new CancellationTokenSource();var field=Field;var token=cts.Token;float delay=DataDelaySeconds;
                    bool injected=InjectPreparationFailure;InjectPreparationFailure=false;
                    var task=Task.Run(()=>
                    {
                        if(delay>0&&token.WaitHandle.WaitOne((int)(delay*1000)))token.ThrowIfCancellationRequested();
                        if(injected)throw new InvalidOperationException("Injected PCG preparation failure.");
                        var watch=Stopwatch.StartNew();var data=field.Generate(key,token);
                        lock(_budget){MaximumDataMs=Math.Max(MaximumDataMs,watch.Elapsed.TotalMilliseconds);}
                        return data;
                    },token);
                    _pending.Add(key,new Pending{Cancel=cts,Task=task,Injected=injected});
                }
            }
            if(_building==null)
                foreach(var view in _views.Values)if(!view.Complete){_building=view;break;}
            while(_cache.Count+_pending.Count>=Settings.MaxCachedChunks)
            {
                Vector2Int oldest=default;float time=float.MaxValue;bool found=false;
                foreach(var pair in _cache)
                    if(!_views.ContainsKey(pair.Key)&&!InBounds(pair.Key,_prepare)&&!(_hasRequest&&InBounds(pair.Key,_requestedPrepare))&&pair.Value.Used<time)
                    {time=pair.Value.Used;oldest=pair.Key;found=true;}
                if(!found)break;
                _cache.Remove(oldest);
            }
        }
        private void AddCandidates(Rect rect,Vector2 focus)
        {
            Vector2Int lo=Field.Chunk(rect.xMin,rect.yMin),hi=Field.Chunk(rect.xMax,rect.yMax);
            for(int z=lo.y;z<=hi.y;z++)for(int x=lo.x;x<=hi.x;x++)
            {
                var key=new Vector2Int(x,z);bool duplicate=false;
                for(int i=0;i<_candidates.Count;i++)if(_candidates[i].Key==key){duplicate=true;break;}
                if(duplicate)continue;
                Vector2 p=new Vector2((x+.5f)*Settings.ChunkSize,(z+.5f)*Settings.ChunkSize);
                bool visible=InBounds(key,_display)||_hasRequest&&InBounds(key,_requestedDisplay);
                _candidates.Add(new Candidate{Key=key,Priority=(p-focus).sqrMagnitude+(visible?0:1000000)});
            }
        }
        private void LinkNeighbors()
        {
            foreach(var pair in _views)
            {
                Vector2Int k=pair.Key;
                pair.Value.Terrain.SetNeighbors(GetTerrain(k+Vector2Int.left),GetTerrain(k+Vector2Int.up),GetTerrain(k+Vector2Int.right),GetTerrain(k+Vector2Int.down));
            }
        }
        private Terrain GetTerrain(Vector2Int key)=>_views.TryGetValue(key,out var view)&&view.GroundReady?view.Terrain:null;
        public void Rebase(Vector2 newOrigin)
        {
            Vector2 shift=newOrigin-WorldOrigin;WorldOrigin=newOrigin;
            foreach(var v in _views.Values)v.ShiftBatches(shift);
            foreach(var v in _pool)v.SetOrigin();
            if(Wind!=null)Wind.SetOrigin(WorldOrigin);
            // World-bound actors and wind sources are direct children of this root.
            foreach(Transform child in transform)
                if(child.GetComponent<PCGGrassInfluence>()!=null||child.GetComponent<PCGWindSource>()!=null||child.GetComponent<PCGSurfaceContact>()!=null)
                    child.position-=new Vector3(shift.x,0,shift.y);
            UpdateCamera();
        }
        private void UpdateCamera()
        {
            float h=Settings.MultiBiome&&Field!=null?FocusHeight(CameraFocus):10;
            Vector3 focus=new Vector3(CameraFocus.x-WorldOrigin.x,h,CameraFocus.y-WorldOrigin.y);
            ViewCamera.orthographic=true;ViewCamera.orthographicSize=CameraSize;
            ViewCamera.transform.rotation=Quaternion.Euler(CameraPitch,CameraYaw,0);
            ViewCamera.transform.position=focus-ViewCamera.transform.forward*(Settings.MultiBiome?160:100);
            if(Settings.MultiBiome)ViewCamera.farClipPlane=Mathf.Max(500,Settings.HeightScale*3+200);
            ViewCamera.cullingMask=InitialReady?_cameraMask:0;
        }
        private void OnDestroy()
            =>ShutdownStreaming();
        public void ShutdownStreaming()
        {
            if(!_started)return;
            CancelPreparation();
            foreach(var v in _views.Values)v.Dispose();
            foreach(var v in _pool)v.Dispose();
            _pending.Clear();_views.Clear();_pool.Clear();_cache.Clear();_started=false;
            _building=null;Field=null;InitialReady=false;_hasRequest=false;FocusPending=false;
            if(ViewCamera!=null)ViewCamera.cullingMask=_cameraMask;
            if(_sourceSettings!=null){Destroy(Settings);Settings=_sourceSettings;}
        }
    }
}
