using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AutoEra.Art.PCG
{
    public struct PCGPlacement { public int MeshId,Submesh;public Matrix4x4 Matrix; }
    internal sealed class PCGInstanceBatch
    {
        public Mesh Mesh;
        public Material Material;
        public int Submesh, Count;
        public readonly Matrix4x4[] Matrices;
        public readonly Vector2[] WorldPositions;
        public bool Grass;
        public PCGInstanceBatch(Mesh mesh,Material material,int submesh,bool grass)
        {
            Mesh=mesh;Material=material;Submesh=submesh;Grass=grass;Matrices=new Matrix4x4[grass?1023:64];
            WorldPositions=new Vector2[Matrices.Length];
        }
    }
    public sealed partial class PCGChunkView
    {
        public Vector2Int Key { get; private set; }
        public Terrain Terrain { get; private set; }
        public bool GroundReady { get; private set; }
        public bool Complete { get; private set; }
        public float LastWanted;
        public int GrassCount { get; private set; }
        private readonly GameObject _root,_water;
        private readonly TerrainData _terrainData;
        private readonly Material _waterMaterial;
        private readonly Texture2D _heightTexture;
        private readonly Color[] _heightPixels;
        private readonly List<PCGInstanceBatch> _batches=new List<PCGInstanceBatch>(40);
        private PCGStreamWorld _world;
        private PCGChunkData _data;
        private int _stage,_row,_grassBatchIndex=-1;
        private ulong _signature;
        private uint _candidateToken;
        private static readonly int WindShapeId=Shader.PropertyToID("_WindShape");
        private readonly MaterialPropertyBlock _plantBlock=new MaterialPropertyBlock();
        public PCGChunkView(PCGStreamWorld world)
        {
            _world=world;var s=world.Settings;
            _root=new GameObject("Stream chunk");_root.transform.SetParent(world.transform,false);
            _terrainData=new TerrainData{heightmapResolution=s.HeightResolution,alphamapResolution=s.AlphaResolution,
                baseMapResolution=128,size=new Vector3(s.ChunkSize,s.HeightScale,s.ChunkSize)};
            _terrainData.terrainLayers=s.TerrainLayers;
            var terrainGo=Terrain.CreateTerrainGameObject(_terrainData);terrainGo.transform.SetParent(_root.transform,false);
            Terrain=terrainGo.GetComponent<Terrain>();Terrain.drawInstanced=true;Terrain.heightmapPixelError=3;
            terrainGo.transform.localPosition=new Vector3(0,s.HeightOrigin,0);
            Terrain.basemapDistance=300;Terrain.materialTemplate=s.TerrainMaterial;
            _water=GameObject.CreatePrimitive(PrimitiveType.Plane);_water.name="Wind-responsive shallow water";
            _water.transform.SetParent(_root.transform,false);
            UnityEngine.Object.Destroy(_water.GetComponent<Collider>());
            _waterMaterial=new Material(s.WaterMaterial);
            _heightTexture=new Texture2D(s.HeightResolution,s.HeightResolution,TextureFormat.RFloat,false,true)
                {wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear,name="Runtime chunk water depth"};
            _heightPixels=new Color[s.HeightResolution*s.HeightResolution];
            _waterMaterial.SetTexture("_TerrainHeight",_heightTexture);
            var renderer=_water.GetComponent<Renderer>();renderer.sharedMaterial=_waterMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;
            _plantBlock.SetVector(WindShapeId,new Vector4(0,1,0,0));
            _root.SetActive(false);
        }
        public void Begin(PCGChunkData data)
        {
            if(_world.Settings.MultiBiome && data.Version != _world.Settings.Version)
                throw new InvalidOperationException("Incompatible PCG chunk generator version.");
            _data=data;Key=data.Key;_stage=0;_row=0;Complete=false;GroundReady=false;GrassCount=0;
            Terrain.heightmapPixelError=_world.Settings.MultiBiome&&_world.Settings.LandscapeScale<1?1:3;
            _signature=0;_grassBatchIndex=-1;
            for(int i=0;i<_batches.Count;i++)_batches[i].Count=0;
            _root.name="Chunk "+Key.x+","+Key.y;SetOrigin();
            ResetLiquids();
        }
        public void SetOrigin()
        {
            float size=_world.Settings.ChunkSize;
            _root.transform.localPosition=new Vector3(Key.x*size-_world.WorldOrigin.x,0,Key.y*size-_world.WorldOrigin.y);
        }
        public void ShiftBatches(Vector2 shift)
        {
            for(int i=0;i<_batches.Count;i++)for(int j=0;j<_batches[i].Count;j++)
            {
                Matrix4x4 m=_batches[i].Matrices[j];
                m.m03=_batches[i].WorldPositions[j].x-_world.WorldOrigin.x;
                m.m23=_batches[i].WorldPositions[j].y-_world.WorldOrigin.y;_batches[i].Matrices[j]=m;
            }
            SetOrigin();
        }
        public void Suspend()
        {
            _root.SetActive(false);Terrain.SetNeighbors(null,null,null,null);GroundReady=false;Complete=false;
        }
        public void BuildStep()
        {
            var s=_world.Settings;
            if(_stage==0)
            {
                _terrainData.SetHeights(0,0,_data.Heights);
                _terrainData.SetAlphamaps(0,0,_data.Alpha);
                _root.SetActive(true);_water.SetActive(false);_stage=1;return;
            }
            if(_stage==1)
            {
                int r=s.HeightResolution;
                for(int z=0;z<r;z++)for(int x=0;x<r;x++)_heightPixels[z*r+x]=new Color(_data.Heights[z,x],0,0,1);
                _heightTexture.SetPixels(_heightPixels);_heightTexture.Apply(false,false);
                _waterMaterial.SetVector("_TerrainRect",new Vector4(Key.x*s.ChunkSize,Key.y*s.ChunkSize,s.ChunkSize,s.ChunkSize));
                _waterMaterial.SetFloat("_HeightScale",s.HeightScale);_waterMaterial.SetFloat("_WaterLevel",s.WaterLevel);
                _water.transform.localPosition=new Vector3(s.ChunkSize*.5f,s.WaterLevel,s.ChunkSize*.5f);
                _water.transform.localScale=new Vector3(s.ChunkSize/10,1,s.ChunkSize/10);
                if(s.MultiBiome)BuildBiomeLiquids();
                else _water.SetActive(true);
                if(s.MultiBiome)BuildSurfaceLayer();
                GroundReady=true;_stage=2;_row=0;return;
            }
            float spacing=_stage==2?s.GrassSpacing:_stage==3?8:_stage==4?4:16;
            if(s.MultiBiome && _stage>2)spacing=_stage==3?12:_stage==4?6:_stage==5?8:_stage==6?1.15f:18;
            int x0=Mathf.FloorToInt(Key.x*s.ChunkSize/spacing),x1=Mathf.FloorToInt((Key.x+1)*s.ChunkSize/spacing);
            int z0=Mathf.FloorToInt(Key.y*s.ChunkSize/spacing),z1=Mathf.FloorToInt((Key.y+1)*s.ChunkSize/spacing);
            int zCell=z0+_row++;
            for(int xCell=x0;xCell<=x1;xCell++)BuildCandidate(xCell,zCell,spacing,_stage-2);
            if(zCell>=z1)
            {
                _row=0;_stage++;
                if(_stage>(s.MultiBiome?7:5)){Complete=true;_data=null;}
            }
        }
        private void BuildCandidate(int cx,int cz,float spacing,int kind)
        {
            if(_world.Settings.MultiBiome){BuildBiomeCandidate(cx,cz,spacing,kind);return;}
            var s=_world.Settings;var f=_world.Field;
            float x=(cx+.15f+PCGWorldField.Unit(cx,cz,s.Seed,210+kind)*.7f)*spacing;
            float z=(cz+.15f+PCGWorldField.Unit(cx,cz,s.Seed,220+kind)*.7f)*spacing;
            if(x<Key.x*s.ChunkSize||x>=(Key.x+1)*s.ChunkSize||z<Key.y*s.ChunkSize||z>=(Key.y+1)*s.ChunkSize)return;
            float forest=f.Forest(x,z),chance=PCGWorldField.Unit(cx,cz,s.Seed,230+kind);
            float patch=f.GrassPatch(x,z);
            if(kind==0)
            {
                float coverage=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,.72f,patch));
                float density=Mathf.Lerp(.015f,.98f,coverage)*(1-forest*.25f);
                if(chance>density)return;
            }
            float h=f.Height(x,z);if(h<s.WaterLevel+.5f||f.Slope(x,z)>29)return;
            float yaw=PCGWorldField.Unit(cx,cz,s.Seed,240+kind)*360;
            _candidateToken=PCGWorldField.Hash(cx,cz,s.Seed,410+kind);
            Vector3 position=new Vector3(x,h-.03f,z);
            if(kind==0)
            {
                float variation=PCGWorldField.Unit(cx,cz,s.Seed,251)*.65f+f.Noise(x,z,.09,304)*.35f;
                float height=Mathf.Lerp(s.GrassMinHeight,s.GrassMaxHeight,variation)/.94f;
                float tuftWidth=.7f+PCGWorldField.Unit(cx,cz,s.Seed,252)*.35f;
                Add(s.GrassMesh,s.GrassMaterial,0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),new Vector3(tuftWidth,height,tuftWidth)),true);
                GrassCount++;return;
            }
            if(kind==1||kind==2)
            {
                float probability=kind==1?Mathf.InverseLerp(.37f,.72f,forest)*.65f:Mathf.Clamp01(.55f-Mathf.Abs(forest-.5f));
                if(chance>probability)return;
                var pool=kind==1?s.Trees:s.Bushes;
                int index=(int)(PCGWorldField.Hash(cx,cz,s.Seed,260+kind)%(uint)pool.Length);
                var prototype=pool[index];
                float height=kind==1?5.5f+PCGWorldField.Unit(cx,cz,s.Seed,270)*2.5f:.7f+PCGWorldField.Unit(cx,cz,s.Seed,271)*.6f;
                float scale=height/Mathf.Max(.1f,prototype.Height);position.y-=prototype.BaseY*scale;
                Matrix4x4 root=Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),Vector3.one*scale);
                foreach(var part in prototype.Parts)Add(part.Mesh,part.Material,part.Submesh,root*part.LocalMatrix,false);
                return;
            }
            if(chance>.12f||patch<.45f)return;
            var mesh=s.RockMeshes[(int)(PCGWorldField.Hash(cx,cz,s.Seed,280)%(uint)s.RockMeshes.Length)];
            float width=1.2f+PCGWorldField.Unit(cx,cz,s.Seed,281)*1.8f;
            float rockScale=width/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.z);
            position.y=h-mesh.bounds.min.y*rockScale-width*.16f;
            Add(mesh,s.RockMaterial,0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),Vector3.one*rockScale),false);
        }
        private void Add(Mesh mesh,Material material,int submesh,Matrix4x4 matrix,bool grass)
        {
            unchecked{_signature+=(ulong)_candidateToken*(uint)mesh.GetInstanceID()*1099511628211UL+(uint)submesh;}
            Vector2 absolute=new Vector2(matrix.m03,matrix.m23);
            matrix.m03-= _world.WorldOrigin.x;matrix.m23-= _world.WorldOrigin.y;
            if(grass&&_grassBatchIndex>=0)
            {
                var current=_batches[_grassBatchIndex];
                if(current.Mesh==mesh&&current.Material==material&&current.Submesh==submesh&&current.Count<current.Matrices.Length)
                {current.WorldPositions[current.Count]=absolute;current.Matrices[current.Count++]=matrix;return;}
            }
            for(int i=0;i<_batches.Count;i++)
            {
                var b=_batches[i];
                if(b.Mesh==mesh&&b.Material==material&&b.Submesh==submesh&&b.Count<b.Matrices.Length)
                {if(grass)_grassBatchIndex=i;b.WorldPositions[b.Count]=absolute;b.Matrices[b.Count++]=matrix;return;}
            }
            var batch=new PCGInstanceBatch(mesh,material,submesh,grass);
            if(grass)_grassBatchIndex=_batches.Count;
            batch.WorldPositions[batch.Count]=absolute;batch.Matrices[batch.Count++]=matrix;_batches.Add(batch);
        }
        public void Draw(Camera camera)
        {
            if(!GroundReady)return;
            Vector2 center=new Vector2((Key.x+.5f)*_world.Settings.ChunkSize,(Key.y+.5f)*_world.Settings.ChunkSize);
            bool grassNear=(center-_world.CameraFocus).sqrMagnitude<_world.Settings.GrassDrawDistance*_world.Settings.GrassDrawDistance;
            for(int i=0;i<_batches.Count;i++)
            {
                var b=_batches[i];if(b.Count==0||b.Grass&&!grassNear)continue;
                Graphics.DrawMeshInstanced(b.Mesh,b.Submesh,b.Material,b.Matrices,b.Count,
                    b.Grass?null:_plantBlock,b.Grass?ShadowCastingMode.Off:ShadowCastingMode.On,true,0,camera,LightProbeUsage.Off);
            }
        }
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_root);UnityEngine.Object.Destroy(_terrainData);
            UnityEngine.Object.Destroy(_waterMaterial);UnityEngine.Object.Destroy(_heightTexture);
            DisposeLiquids();
            if(_surfaceMesh!=null)UnityEngine.Object.Destroy(_surfaceMesh);
        }
        public ulong LayoutSignature()
        {
            return _signature;
        }
        public void CopyWorldPlacements(List<PCGPlacement> placements)
        {
            foreach(var batch in _batches)for(int i=0;i<batch.Count;i++)
            {
                Matrix4x4 m=batch.Matrices[i];m.m03+=_world.WorldOrigin.x;m.m23+=_world.WorldOrigin.y;
                placements.Add(new PCGPlacement{MeshId=batch.Mesh.GetInstanceID(),Submesh=batch.Submesh,Matrix=m});
            }
        }
    }
}
