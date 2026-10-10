using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    public sealed class PCGWindField : MonoBehaviour
    {
        public const int MaxSources = 16;
        [InspectorName("基础风向")] public Vector2 Direction = new Vector2(.82f, .57f);
        [InspectorName("基础风速")] public float Speed = 2.4f;
        [InspectorName("最大风速")] public float MaximumSpeed = 8;
        [InspectorName("风相位")] public float SeedPhase = 1.7f;
        [InspectorName("启用风场")] public bool WindEnabled = true;
        public Vector2 WorldOrigin { get; private set; }
        private readonly List<PCGWindSource> _sources = new List<PCGWindSource>(MaxSources);
        private readonly Vector4[] _positions = new Vector4[MaxSources], _directions = new Vector4[MaxSources];
        private static readonly int ActiveId=Shader.PropertyToID("_AEWindActive"), ParamsId=Shader.PropertyToID("_AEWindParams"),
            GustId=Shader.PropertyToID("_AEWindGust"), TimeId=Shader.PropertyToID("_AEWindTime"),
            OriginId=Shader.PropertyToID("_AEWorldOrigin"), CountId=Shader.PropertyToID("_AEWindSourceCount"),
            PositionsId=Shader.PropertyToID("_AEWindPositions"), DirectionsId=Shader.PropertyToID("_AEWindDirections");
        public void SetOrigin(Vector2 origin) { WorldOrigin = origin; }
        public void Register(PCGWindSource source)
        {
            if (_sources.Contains(source)) return;
            if (_sources.Count >= MaxSources) { Debug.LogError("[PCGStream] Wind source capacity exceeded.", source); return; }
            _sources.Add(source);
        }
        public void Unregister(PCGWindSource source) => _sources.Remove(source);
        public Vector2 Sample(Vector2 world, float time)
        {
            if (!WindEnabled) return Vector2.zero;
            Vector2 d=Direction.sqrMagnitude>.0001f?Direction.normalized:Vector2.right;
            float gust=.8f+.2f*Mathf.Sin(time*.42f-world.x*.014f-world.y*.020f+SeedPhase);
            float wave=.85f+.15f*Mathf.Sin(world.x*.031f+world.y*.027f-time*.7f);
            Vector2 result=d*(Speed*gust*wave);
            for(int i=0;i<_sources.Count;i++)
            {
                PCGWindSource s=_sources[i]; if(s==null||!s.isActiveAndEnabled)continue;
                Vector2 delta=world-new Vector2(s.transform.position.x+WorldOrigin.x,s.transform.position.z+WorldOrigin.y);
                float distance=delta.magnitude, radius=Mathf.Max(.01f,s.Radius);
                float weight=Mathf.Pow(Mathf.Clamp01(1-distance/radius),Mathf.Max(.1f,s.Falloff));
                Vector2 local=s.Outward?(distance>.001f?delta/distance:Vector2.zero):s.Direction.normalized;
                result+=local*(s.Strength*weight);
            }
            return Vector2.ClampMagnitude(result,MaximumSpeed);
        }
#if UNITY_EDITOR
        public long LastManagedBytes { get; private set; }
#endif
        private void LateUpdate()
        {
#if UNITY_EDITOR
            long before=System.GC.GetAllocatedBytesForCurrentThread();
#endif
            PushWind();
#if UNITY_EDITOR
            LastManagedBytes=System.GC.GetAllocatedBytesForCurrentThread()-before;
#endif
        }
        private void PushWind()
        {
            Vector2 d=Direction.sqrMagnitude>.0001f?Direction.normalized:Vector2.right;
            int count=0;
            for(int i=0;i<_sources.Count;i++)
            {
                var s=_sources[i];if(s==null||!s.isActiveAndEnabled)continue;
                Vector2 local=s.Direction.normalized;
                _positions[count]=new Vector4(s.transform.position.x+WorldOrigin.x,s.transform.position.z+WorldOrigin.y,Mathf.Max(.01f,s.Radius),s.Strength);
                _directions[count++]=new Vector4(local.x,local.y,s.Outward?1:0,Mathf.Max(.1f,s.Falloff));
            }
            Shader.SetGlobalFloat(ActiveId,1);
            Shader.SetGlobalVector(ParamsId,new Vector4(d.x,d.y,WindEnabled?Speed:0,WindEnabled?1:0));
            Shader.SetGlobalVector(GustId,new Vector4(SeedPhase,MaximumSpeed,0,0));
            Shader.SetGlobalFloat(TimeId,Time.time);
            Shader.SetGlobalVector(OriginId,new Vector4(WorldOrigin.x,WorldOrigin.y,0,0));
            Shader.SetGlobalInt(CountId,WindEnabled?count:0);
            Shader.SetGlobalVectorArray(PositionsId,_positions);Shader.SetGlobalVectorArray(DirectionsId,_directions);
        }
        private void OnDisable() { Shader.SetGlobalFloat(ActiveId,0);Shader.SetGlobalInt(CountId,0);Shader.SetGlobalVector(OriginId,Vector4.zero); }
    }
}
