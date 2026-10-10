using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    public sealed class PCGGrassTrails : MonoBehaviour
    {
        private struct Cell { public Vector2 Direction; public float Strength, Time; }
        [InspectorName("流式环境")] public PCGStreamWorld World;
        public int CellCount => _cells.Count;
        public int CapacityDrops { get; private set; }
        public float RecoverySeconds => World.Settings.RecoverySeconds;
        private const float CellSize=.5f;
        private readonly List<PCGGrassInfluence> _sources=new List<PCGGrassInfluence>(32);
        private readonly Dictionary<Vector2Int,Cell> _cells=new Dictionary<Vector2Int,Cell>(32768);
        private readonly List<Vector2Int> _expired=new List<Vector2Int>(32768);
        private Texture2D _texture;
        private Color[] _pixels;
        private Vector2 _textureOrigin;
        private float _nextUpload,_nextCleanup;
        private static readonly int TextureId=Shader.PropertyToID("_AEGrassTrailMap"), RectId=Shader.PropertyToID("_AEGrassTrailRect");
        public void Register(PCGGrassInfluence source)
        {
            if(_sources.Contains(source))return;
            if(_sources.Count>=32){Debug.LogError("[PCGStream] Grass influence capacity exceeded.",source);return;}
            _sources.Add(source);
        }
        public void Unregister(PCGGrassInfluence source)=>_sources.Remove(source);
        public void ClearTrails(){_cells.Clear();_nextUpload=0;CapacityDrops=0;}
        public float VisiblePressureAt(Vector2 world)
        {
            if(_pixels==null)return 0;
            int r=World.Settings.TrailResolution;float pixel=World.Settings.TrailPixelSize;
            int x=Mathf.FloorToInt((world.x-_textureOrigin.x)/pixel),z=Mathf.FloorToInt((world.y-_textureOrigin.y)/pixel);
            return x<0||z<0||x>=r||z>=r?0:_pixels[z*r+x].b;
        }
        public void StampSegment(Vector2 from,Vector2 to,float radius,float strength,Vector2 forward,float time)
        {
            if(World==null||strength<=0)return;
            Vector2 motion=to-from;float len2=motion.sqrMagnitude;
            Vector2 direction=len2>.0001f?motion.normalized:forward.normalized;
            int x0=Mathf.FloorToInt((Mathf.Min(from.x,to.x)-radius)/CellSize),x1=Mathf.CeilToInt((Mathf.Max(from.x,to.x)+radius)/CellSize);
            int z0=Mathf.FloorToInt((Mathf.Min(from.y,to.y)-radius)/CellSize),z1=Mathf.CeilToInt((Mathf.Max(from.y,to.y)+radius)/CellSize);
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {
                Vector2 p=new Vector2((x+.5f)*CellSize,(z+.5f)*CellSize);
                float u=len2>.0001f?Mathf.Clamp01(Vector2.Dot(p-from,motion)/len2):0;
                Vector2 delta=p-(from+motion*u);float distance=delta.magnitude;
                if(distance>=radius)continue;
                float amount=strength*Mathf.Pow(1-distance/radius,.65f);
                var key=new Vector2Int(x,z);
                bool exists=_cells.TryGetValue(key,out Cell cell);
                float old=exists?cell.Strength*Mathf.Clamp01(1-(time-cell.Time)/RecoverySeconds):0;
                if(amount<old)continue;
                if(!exists&&_cells.Count>=World.Settings.MaxTrailCells){CapacityDrops++;continue;}
                Vector2 away=distance>.01f?delta/distance:direction;
                Vector2 bend=(direction*.65f+away*.35f).normalized;
                _cells[key]=new Cell{Direction=bend,Strength=Mathf.Clamp01(amount),Time=time};
            }
        }
        public Vector3 Sample(Vector2 world,float time)
        {
            var key=new Vector2Int(Mathf.FloorToInt(world.x/CellSize),Mathf.FloorToInt(world.y/CellSize));
            if(!_cells.TryGetValue(key,out Cell cell))return Vector3.zero;
            float recovery=Mathf.Clamp01(1-(time-cell.Time)/RecoverySeconds);
            return new Vector3(cell.Direction.x,cell.Direction.y,cell.Strength*recovery*recovery);
        }
        public void Cleanup(float time)
        {
            _expired.Clear();
            foreach(var pair in _cells)if(time-pair.Value.Time>=RecoverySeconds)_expired.Add(pair.Key);
            for(int i=0;i<_expired.Count;i++)_cells.Remove(_expired[i]);
        }
#if UNITY_EDITOR
        public long LastManagedBytes { get; private set; }
#endif
        private void LateUpdate()
        {
#if UNITY_EDITOR
            long before=System.GC.GetAllocatedBytesForCurrentThread();
#endif
            UpdateTrails();
#if UNITY_EDITOR
            LastManagedBytes=System.GC.GetAllocatedBytesForCurrentThread()-before;
#endif
        }
        private void UpdateTrails()
        {
            if(World==null||World.Field==null)return;
            float time=Time.time;
            for(int i=0;i<_sources.Count;i++)if(_sources[i]!=null&&_sources[i].isActiveAndEnabled)_sources[i].Stamp(time);
            if(time>=_nextCleanup){Cleanup(time);_nextCleanup=time+.5f;}
            if(time<_nextUpload)return;
            _nextUpload=time+.05f;
            int r=World.Settings.TrailResolution;float pixel=World.Settings.TrailPixelSize;
            if(_texture==null)
            {
                _texture=new Texture2D(r,r,TextureFormat.RGBAFloat,false,true){name="Transient world grass pressure",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
                _pixels=new Color[r*r];
            }
            Vector2 center=World.CameraFocus;
            float ox=Mathf.Floor((center.x-r*pixel*.5f)/pixel)*pixel,oz=Mathf.Floor((center.y-r*pixel*.5f)/pixel)*pixel;
            _textureOrigin=new Vector2(ox,oz);
            System.Array.Clear(_pixels,0,_pixels.Length);
            foreach(var pair in _cells)
            {
                int x=Mathf.FloorToInt(((pair.Key.x+.5f)*CellSize-ox)/pixel),z=Mathf.FloorToInt(((pair.Key.y+.5f)*CellSize-oz)/pixel);
                if(x<0||z<0||x>=r||z>=r)continue;
                float age=Mathf.Clamp01(1-(time-pair.Value.Time)/RecoverySeconds);
                float amount=pair.Value.Strength*age*age;
                int index=z*r+x;
                if(amount>_pixels[index].b)_pixels[index]=new Color(pair.Value.Direction.x,pair.Value.Direction.y,amount,0);
            }
            _texture.SetPixelData(_pixels,0);_texture.Apply(false,false);
            Shader.SetGlobalTexture(TextureId,_texture);
            Shader.SetGlobalVector(RectId,new Vector4(ox,oz,r*pixel,r*pixel));
        }
        private void OnDisable()
        {
            Shader.SetGlobalTexture(TextureId,Texture2D.blackTexture);
            if(_texture!=null)Destroy(_texture);
            _texture=null;_pixels=null;_cells.Clear();
        }
    }
}
