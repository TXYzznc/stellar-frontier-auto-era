using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AutoEra.Art.PCG
{
    public sealed partial class PCGChunkView
    {
        private sealed class LiquidView
        {
            public GameObject Object;
            public Material Material;
        }
        private readonly List<LiquidView> _liquids=new List<LiquidView>(4);
        private void ResetLiquids()
        {
            for(int i=0;i<_liquids.Count;i++)_liquids[i].Object.SetActive(false);
        }
        private void BuildBiomeLiquids()
        {
            var s=_world.Settings;
            for(int i=0;i<_data.Liquids.Length;i++)
            {
                var feature=_data.Liquids[i];
                Material source=feature.Kind==PCGLiquidKind.Lava?s.LavaMaterial:
                    feature.Kind==PCGLiquidKind.Marsh?s.MarshWaterMaterial:s.WaterMaterial;
                if(i>=_liquids.Count)
                {
                    var go=GameObject.CreatePrimitive(PrimitiveType.Plane);go.transform.SetParent(_root.transform,false);
                    Object.Destroy(go.GetComponent<Collider>());
                    var renderer=go.GetComponent<Renderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;
                    var material=new Material(source);renderer.sharedMaterial=material;
                    _liquids.Add(new LiquidView{Object=go,Material=material});
                }
                var view=_liquids[i];view.Material.shader=source.shader;view.Material.CopyPropertiesFromMaterial(source);
                view.Material.SetTexture("_TerrainHeight",_heightTexture);
                view.Material.SetVector("_TerrainRect",new Vector4(Key.x*s.ChunkSize,Key.y*s.ChunkSize,s.ChunkSize,s.ChunkSize));
                view.Material.SetFloat("_HeightScale",s.HeightScale);view.Material.SetFloat("_WaterLevel",feature.Level);
                view.Material.SetFloat("_TerrainBaseY",s.HeightOrigin);
                var region=s.ConstructionRegion;
                view.Material.SetVector("_ConstructionRegion",s.PreserveConstructionRegion?new Vector4(region.xMin,region.yMin,region.width,region.height):Vector4.zero);
                view.Material.SetFloat("_ConstructionBlend",s.ConstructionBlend);
                view.Material.SetVector("_Feature",new Vector4(feature.Center.x,feature.Center.y,feature.Radius,feature.Aspect));
                view.Material.SetFloat("_FeaturePhase",feature.Phase);view.Material.SetFloat("_UseFeature",1);
                view.Material.SetVector("_FeatureIdentity",new Vector4(feature.Id&65535,feature.Id>>16,0,0));
                view.Object.name=feature.Kind+" "+feature.Id;
                view.Object.transform.localPosition=new Vector3(s.ChunkSize*.5f,feature.Level,s.ChunkSize*.5f);
                view.Object.transform.localScale=new Vector3(s.ChunkSize/10,1,s.ChunkSize/10);
                view.Object.SetActive(true);
            }
        }
        private void DisposeLiquids()
        {
            for(int i=0;i<_liquids.Count;i++)Object.Destroy(_liquids[i].Material);
            _liquids.Clear();
        }
        private PCGEnvironmentSample CachedEnvironment(float x,float z,out float height)
        {
            var s=_world.Settings;int r=s.HeightResolution;
            float u=(x-Key.x*s.ChunkSize)/s.ChunkSize*(r-1),v=(z-Key.y*s.ChunkSize)/s.ChunkSize*(r-1);
            int a=Mathf.Clamp((int)u,0,r-2),b=Mathf.Clamp((int)v,0,r-2);
            height=Mathf.Lerp(Mathf.Lerp(_data.Heights[b,a],_data.Heights[b,a+1],u-a),
                Mathf.Lerp(_data.Heights[b+1,a],_data.Heights[b+1,a+1],u-a),v-b)*s.HeightScale+s.HeightOrigin;
            return _data.Environment[Mathf.Clamp(Mathf.RoundToInt(v),0,r-1),Mathf.Clamp(Mathf.RoundToInt(u),0,r-1)];
        }
        private void BuildBiomeCandidate(int cx,int cz,float spacing,int kind)
        {
            var s=_world.Settings;var f=_world.Field;
            float x=(cx+.15f+PCGWorldField.Unit(cx,cz,s.Seed,210+kind)*.7f)*spacing;
            float z=(cz+.15f+PCGWorldField.Unit(cx,cz,s.Seed,220+kind)*.7f)*spacing;
            if(x<Key.x*s.ChunkSize||x>=(Key.x+1)*s.ChunkSize||z<Key.y*s.ChunkSize||z>=(Key.y+1)*s.ChunkSize)return;
            var env=CachedEnvironment(x,z,out float h);
            float depth=env.Liquid==PCGLiquidKind.None?-100:env.LiquidLevel-h;
            float chance=PCGWorldField.Unit(cx,cz,s.Seed,230+kind),patch=f.GrassPatch(x,z);
            float yaw=PCGWorldField.Unit(cx,cz,s.Seed,240+kind)*360;
            _candidateToken=PCGWorldField.Hash(cx,cz,s.Seed,410+kind);
            Vector3 position=new Vector3(x,h-.03f,z);
            if(kind==0)
            {
                if(s.PreserveConstructionRegion&&f.IsReserved(x,z,.2f))return;
                if(depth>-.12f||env.Slope>34)return;
                float coverage=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,.72f,patch));
                float suitable=env.Cover.Meadow+env.Cover.Soil*.6f+env.Cover.Mud*.40f;
                float density=Mathf.Lerp(.015f,.98f,coverage)*suitable*(1-env.Forest*.25f);
                Material material=env.Cover.Mud>.3f?s.MarshGrassMaterial:s.GrassMaterial;
                if(env.Cover.Sand>.6f) { density=.008f; material=s.DryGrassMaterial; }
                if(env.Cover.Snow>.6f) { density=.005f; material=s.SnowGrassMaterial; }
                if(chance>density)return;
                float variation=PCGWorldField.Unit(cx,cz,s.Seed,251)*.65f+f.Noise(x,z,.09,304)*.35f;
                float height=Mathf.Lerp(s.GrassMinHeight,s.GrassMaxHeight,variation)/.94f;
                if(material==s.DryGrassMaterial || material==s.SnowGrassMaterial)height*=.65f;
                float tuftWidth=.7f+PCGWorldField.Unit(cx,cz,s.Seed,252)*.35f;
                Add(s.GrassMesh,material,0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),new Vector3(tuftWidth,height,tuftWidth)),true);
                GrassCount++;return;
            }
            if(kind==4)
            {
                if(env.Liquid==PCGLiquidKind.Lava||env.Cover.Snow>.5f||env.Slope>15||depth>1.1f||depth<-.9f)return;
                if(env.Liquid==PCGLiquidKind.None||chance>.56f)return;
                float height=1.15f+PCGWorldField.Unit(cx,cz,s.Seed,570)*1.1f;
                Add(s.ReedMesh,s.ReedMaterial,0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),new Vector3(.7f,height,.7f)),true);
                return;
            }
            // 正式世界仅消费显式对象净空；样机的中心圆形与固定通道不应留成安全带。
            bool reserved=f.IsReserved(x,z,5) || !s.PreserveConstructionRegion&&
                (x*x+z*z<18*18 || Mathf.Abs(x-8)<2.2f&&z>-10&&z<40);
            if(depth>-.2f||reserved)return;
            var surfaceSystem=_world.SurfaceResponse;
            if(kind==2&&surfaceSystem!=null&&env.Forest>.20f&&env.Cover.Snow<.2f&&env.Cover.Mud<.4f&&chance<env.Forest*.8f)
            {
                for(int leaf=0;leaf<5;leaf++)
                {
                    float lx=x+(PCGWorldField.Unit(cx,cz,s.Seed,870+leaf)-.5f)*3;
                    float lz=z+(PCGWorldField.Unit(cx,cz,s.Seed,880+leaf)-.5f)*3;
                    float lh=f.Height(lx,lz);float scaleLeaf=.4f+PCGWorldField.Unit(cx,cz,s.Seed,890+leaf)*.7f;
                    Add(surfaceSystem.Settings.LeafMesh,surfaceSystem.Settings.LeafMaterial,0,
                        Matrix4x4.TRS(new Vector3(lx,lh+.05f,lz),Quaternion.Euler(0,yaw+leaf*67,0),Vector3.one*scaleLeaf),true);
                }
            }
            if(kind==1||kind==2)
            {
                if(env.Slope>26)return;
                float probability=kind==1?env.Forest*.55f*(1-env.Cover.Mud*.8f)*(1-env.Cover.Snow*.7f):
                    .25f*(env.Cover.Meadow+env.Cover.Soil)+env.Cover.Sand*.025f;
                if(env.Cover.Volcanic>.3f||chance>probability)return;
                var pool=kind==1?(env.Cover.Snow>.4f?s.SnowTrees:s.Trees):(env.Cover.Sand>.4f?s.DryBushes:s.Bushes);
                int index=(int)(PCGWorldField.Hash(cx,cz,s.Seed,260+kind)%(uint)pool.Length);
                var prototype=pool[index];
                float height=kind==1?5.5f+PCGWorldField.Unit(cx,cz,s.Seed,270)*2.5f:.7f+PCGWorldField.Unit(cx,cz,s.Seed,271)*.6f;
                float scale=height/Mathf.Max(.1f,prototype.Height);position.y-=prototype.BaseY*scale;
                Matrix4x4 root=Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),Vector3.one*scale);
                foreach(var part in prototype.Parts)Add(part.Mesh,part.Material,part.Submesh,root*part.LocalMatrix,false);
                return;
            }
            if(kind==5)
            {
                if(chance>env.Cover.Mud*.25f+env.Cover.Volcanic*.035f || env.Slope>20)return;
                Add(s.DeadwoodMesh,s.DeadwoodMaterial,0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),
                    Vector3.one*(1.2f+PCGWorldField.Unit(cx,cz,s.Seed,575)*1.5f)),false);
                return;
            }
            float gravel=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,.76f,f.Noise(x,z,.014,581)))*env.Cover.Sand;
            float rockDensity=.04f+env.Mountain*.52f+env.Cover.Volcanic*.42f+env.Cover.Sand*.12f+env.Cover.Snow*.15f+gravel*.34f;
            if(chance>rockDensity||env.Slope>58)return;
            var mesh=s.RockMeshes[(int)(PCGWorldField.Hash(cx,cz,s.Seed,280)%(uint)s.RockMeshes.Length)];
            float group=f.Noise(x,z,.022,580);
            float width=env.Mountain>.5f?1.6f+group*5+PCGWorldField.Unit(cx,cz,s.Seed,281)*3:
                1+PCGWorldField.Unit(cx,cz,s.Seed,281)*2.4f;
            if(gravel>.4f)width*=.42f;
            float scaleRock=width/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.z);
            float verticalScale=Mathf.Min(scaleRock*(.6f+group*.8f),s.DecorationHeightEnvelope/Mathf.Max(.1f,mesh.bounds.size.y));
            position.y=h-mesh.bounds.min.y*verticalScale-width*.20f;
            int tint=env.Cover.Volcanic>.3f?3:env.Cover.Snow>.4f?4:env.Cover.Mud>.25f?2:env.Cover.Sand>.4f?1:0;
            Add(mesh,s.BiomeRockMaterials[tint],0,Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),
                new Vector3(scaleRock,verticalScale,scaleRock)),false);
            var response=_world.SurfaceResponse;
            if(response!=null)
            {
                if(env.Cover.Snow>.35f&&response.Settings.SnowCaps!=null)
                {
                    for(int capIndex=0;capIndex<s.RockMeshes.Length;capIndex++)if(s.RockMeshes[capIndex]==mesh)
                    {
                        if(capIndex<response.Settings.SnowCaps.Length)
                            Add(response.Settings.SnowCaps[capIndex],response.Settings.SurfaceMaterial,0,
                                Matrix4x4.TRS(position,Quaternion.Euler(0,yaw,0),new Vector3(scaleRock,verticalScale,scaleRock)),false);
                        break;
                    }
                }
                // Graded apron: small stones at the foot of a large rock, never a uniform ring.
                if(width>2.5f&&chance<.32f)
                    for(int pebble=0;pebble<3;pebble++)
                    {
                        float angle=yaw*.01745f+pebble*1.7f,offset=width*(.35f+.10f*pebble);
                        float px=x+Mathf.Cos(angle)*offset,pz=z+Mathf.Sin(angle)*offset;
                        float ph=f.Height(px,pz),smallScale=scaleRock*(.09f+.04f*pebble);
                        Add(mesh,s.BiomeRockMaterials[tint],0,Matrix4x4.TRS(new Vector3(px,ph-mesh.bounds.min.y*smallScale-.07f,pz),
                            Quaternion.Euler(0,yaw+pebble*71,0),Vector3.one*smallScale),false);
                    }
            }
        }
    }
}
