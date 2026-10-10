using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    public enum PCGBiome
    {
        [InspectorName("草地")] Meadow, [InspectorName("沙漠")] Desert,
        [InspectorName("山地")] Mountains, [InspectorName("水域")] Water,
        [InspectorName("沼泽")] Marsh, [InspectorName("火山")] Volcano,
        [InspectorName("雪地")] Snow
    }
    public enum PCGLiquidKind
    {
        [InspectorName("无")] None, [InspectorName("水")] Water,
        [InspectorName("沼泽水")] Marsh, [InspectorName("熔岩")] Lava
    }
    // Semantic layer order is global and independent of display chunk/palette.
    public enum PCGCover
    {
        [InspectorName("草地")] Meadow, [InspectorName("土壤")] Soil,
        [InspectorName("岩石")] Rock, [InspectorName("沙地")] Sand,
        [InspectorName("泥地")] Mud, [InspectorName("积雪")] Snow,
        [InspectorName("火山")] Volcanic, [InspectorName("岸边")] Shore
    }
    [Serializable]
    public struct PCGReservedSpace
    {
        [InspectorName("名称")] public string Name;
        [InspectorName("占地中心")] public Vector2 Center;
        [InspectorName("通道起点")] public Vector2 Start;
        [InspectorName("通道终点")] public Vector2 End;
        [InspectorName("占地半径")] public float PadRadius;
        [InspectorName("通道半宽")] public float RouteHalfWidth;
    }

    [Serializable]
    public struct PCGBiomeLandmark
    {
        [InspectorName("名称")] public string Name;
        [InspectorName("地貌类型")] public PCGBiome Biome;
        [InspectorName("定位坐标")] public Vector2 Position;
        [InspectorName("镜头正交尺寸")] public float ViewSize;
    }

    public struct PCGCoverWeights
    {
        public float Meadow, Soil, Rock, Sand, Mud, Snow, Volcanic, Shore;
        public float this[int i]
        {
            get
            {
                switch(i) { case 0:return Meadow; case 1:return Soil; case 2:return Rock; case 3:return Sand;
                    case 4:return Mud; case 5:return Snow; case 6:return Volcanic; default:return Shore; }
            }
        }
        public void Normalize()
        {
            float total = Meadow+Soil+Rock+Sand+Mud+Snow+Volcanic+Shore;
            if(total < .0001f) { Meadow=1; return; }
            Meadow/=total; Soil/=total; Rock/=total; Sand/=total; Mud/=total;
            Snow/=total; Volcanic/=total; Shore/=total;
        }
    }

    public struct PCGEnvironmentSample
    {
        public float Height, Slope, Temperature, Moisture, Mountain, Dryness, Geothermal, Forest;
        public float LiquidLevel;
        public uint LiquidId;
        public PCGLiquidKind Liquid;
        public PCGCoverWeights Cover;
        public PCGBiome Dominant
        {
            get
            {
                if(Liquid!=PCGLiquidKind.None && Height < LiquidLevel-.15f)
                    return Liquid==PCGLiquidKind.Lava?PCGBiome.Volcano:Liquid==PCGLiquidKind.Marsh?PCGBiome.Marsh:PCGBiome.Water;
                if(Cover.Volcanic>.4f)return PCGBiome.Volcano;
                if(Cover.Snow>.45f)return PCGBiome.Snow;
                if(Cover.Mud>.35f)return PCGBiome.Marsh;
                if(Mountain>.55f)return PCGBiome.Mountains;
                if(Cover.Sand>.45f)return PCGBiome.Desert;
                return PCGBiome.Meadow;
            }
        }
    }

    public struct PCGLiquidFeature
    {
        public uint Id;
        public PCGLiquidKind Kind;
        public Vector2 Center;
        public float Radius, Level, Depth, Aspect, Phase;
        public bool Valid => Kind != PCGLiquidKind.None;
    }

    public sealed partial class PCGWorldField
    {
        public readonly bool MultiBiome;
        public readonly int Version;
        private readonly float _biomeScale;
        private readonly PCGReservedSpace[] _reservedSpaces;
        private const float FeatureCell=256, MaximumFeatureRadius=110;
        private struct Climate { public float Temperature, Moisture, Mountain, Dry, Hot; }
        private static float Clamp(float value) => Math.Max(0,Math.Min(1,value));
        private static float Ramp(float value,float lo,float hi) => Smooth((value-lo)/(hi-lo));
        public bool IsReserved(float x,float z,float footprint)
        {
            for(int i=0;i<_reservedSpaces.Length;i++)
            {
                var space=_reservedSpaces[i];float dx=x-space.Center.x,dz=z-space.Center.y;
                float radius=space.PadRadius+footprint;
                if(dx*dx+dz*dz<radius*radius)return true;
                float ax=space.End.x-space.Start.x,az=space.End.y-space.Start.y;
                float length=ax*ax+az*az;
                float t=length>0?Clamp(((x-space.Start.x)*ax+(z-space.Start.y)*az)/length):0;
                dx=x-space.Start.x-ax*t;dz=z-space.Start.y-az*t;
                radius=space.RouteHalfWidth+footprint;
                if(dx*dx+dz*dz<radius*radius)return true;
            }
            return false;
        }

        private Climate Macro(double x,double z)
        {
            double wx=x+(Noise(x,z,1/1700d,510)-.5)*180;
            double wz=z+(Noise(x,z,1/1700d,511)-.5)*180;
            float temp=Noise(wx,wz,1/_biomeScale,512);
            float moist=Noise(wx,wz,1/(_biomeScale*.87),513);
            float geology=Noise(wx,wz,1/(_biomeScale*.70),514);
            float heat=Noise(wx,wz,1/(_biomeScale*.58),515);
            // 建设区气候只提供局部引导，并叠加低频扰动，避免初始区形成固定圆形气候岛。
            float open=ConstructionInfluence(x,z);
            open*=.82f+.18f*Noise(x,z,.018,516);
            temp=temp*(1-open)+.58f*open;
            moist=moist*(1-open)+.55f*open;
            float hot=Ramp(heat,.68f,.86f)*(1-open);
            return new Climate { Temperature=temp,Moisture=moist,
                Mountain=Ramp(geology,.49f,.76f)*(1-open),
                Dry=Ramp(.48f-moist,0,.19f)*Ramp(temp,.35f,.55f)*(1-hot),
                Hot=hot };
        }
        private float Skeleton(double x,double z,Climate c)
        {
            float low=Noise(x,z,.004,521),detail=Noise(x,z,.025,522);
            float ridge=1-Math.Abs(Noise(x+z*.26,z,.010,523)*2-1);
            // A low valley follows each ridge; terraces preserve usable spaces.
            float mountain=8+(float)Math.Pow(ridge,2.8)*43+Noise(x,z,.035,524)*6;
            float dunes=(float)(Math.Sin(x*.11+z*.045+Noise(x,z,.007,525)*6)*.5+.5)*5;
            float h=10+low*5+detail*.55f;
            h=h*(1-c.Mountain)+mountain*c.Mountain;
            // Dry cover does not erase the mountain skeleton: preserve desert rock ridges.
            float duneBlend=c.Dry*(1-c.Mountain*.72f);
            h=h*(1-duneBlend)+ (12+low*5+dunes)*duneBlend;
            float marsh=Ramp(c.Moisture,.66f,.86f)*(1-c.Mountain)*(1-c.Hot);
            h=h*(1-marsh)+(9+low*1.2f+detail*.35f)*marsh;
            float fissure=1-Ramp((float)Math.Abs(Math.Sin(x*.075+Noise(x,z,.018,527)*6)),.04f,.19f);
            float volcanic=16+ridge*16+Noise(x,z,.047,526)*2-fissure*2;
            h=h*(1-c.Hot)+volcanic*c.Hot;
            return h;
        }
        private bool IsNearConstruction(double canonicalX,double canonicalZ)
        {
            if(!_preserveConstruction)return false;
            Vector2 world=WorldPoint(new Vector2((float)canonicalX,(float)canonicalZ));
            double dx=Math.Max(Math.Max(_constructionRegion.xMin-world.x,world.x-_constructionRegion.xMax),0);
            double dz=Math.Max(Math.Max(_constructionRegion.yMin-world.y,world.y-_constructionRegion.yMax),0);
            float clear=MaximumFeatureRadius*LandscapeScale+_constructionBlend;
            return Math.Max(dx,dz)<clear;
        }
        // Bounded isolated feature footprints give liquids a solid separator by construction.
        // Minimum center separation 220.16m > twice maximum warped radius 109.25m.
        public PCGLiquidFeature Feature(long cx,long cz)
        {
            var f=CanonicalFeature(cx,cz);if(!f.Valid)return f;
            f.Center=WorldPoint(f.Center);f.Radius*=LandscapeScale;f.Depth*=ReliefScale;f.Level=WorldHeight(f.Level);
            return f;
        }
        private PCGLiquidFeature CanonicalFeature(long cx,long cz)
        {
            double x=(cx+.5+(Unit(cx,cz,Seed,530)-.5)*.14)*FeatureCell;
            double z=(cz+.5+(Unit(cx,cz,Seed,531)-.5)*.14)*FeatureCell;
            if(IsNearConstruction(x,z))return default;
            Climate c=Macro(x,z);
            float chance=Unit(cx,cz,Seed,532);
            PCGLiquidKind kind;
            if(c.Hot>.48f)kind=PCGLiquidKind.Lava;
            else if(c.Moisture>.73f && c.Mountain<.25f)kind=PCGLiquidKind.Marsh;
            else if(c.Moisture>.48f && c.Mountain<.38f && chance<.55f)kind=PCGLiquidKind.Water;
            else if(c.Dry>.65f && c.Mountain<.3f && chance<.12f)kind=PCGLiquidKind.Water; // Oasis
            else return default;
            float radius=kind==PCGLiquidKind.Marsh?48+chance*38:kind==PCGLiquidKind.Lava?35+chance*45:62+chance*33;
            return new PCGLiquidFeature { Id=Hash(cx,cz,Seed,540),Kind=kind,Center=new Vector2((float)x,(float)z),
                Radius=radius,Level=Skeleton(x,z,c)-(kind==PCGLiquidKind.Marsh?.4f:2),
                Depth=kind==PCGLiquidKind.Marsh?1.25f:kind==PCGLiquidKind.Lava?7:8,
                Aspect=.68f+Unit(cx,cz,Seed,541)*.25f,Phase=Unit(cx,cz,Seed,542)*6.283185f };
        }
        public float FeatureDistance(double x,double z,PCGLiquidFeature f)
        {
            double dx=x-f.Center.x,dz=(z-f.Center.y)/f.Aspect;
            double a=Math.Atan2(dz,dx);
            double shape=1+.09*Math.Sin(a*3+f.Phase)+.06*Math.Sin(a*5-f.Phase);
            return (float)(Math.Sqrt(dx*dx+dz*dz)/(f.Radius*shape));
        }
        private bool NearbyFeature(double x,double z,out PCGLiquidFeature found)
        {
            long ix=(long)Math.Floor(x/FeatureCell),iz=(long)Math.Floor(z/FeatureCell);
            for(long cz=iz-1;cz<=iz+1;cz++)for(long cx=ix-1;cx<=ix+1;cx++)
            {
                double fx=(cx+.5+(Unit(cx,cz,Seed,530)-.5)*.14)*FeatureCell;
                double fz=(cz+.5+(Unit(cx,cz,Seed,531)-.5)*.14)*FeatureCell;
                if(Math.Abs(x-fx)>MaximumFeatureRadius || Math.Abs(z-fz)>MaximumFeatureRadius)continue;
                var f=CanonicalFeature(cx,cz);
                if(f.Valid && FeatureDistance(x,z,f)<1) {found=f;return true;}
            }
            found=default;return false;
        }
        private float BiomeHeight(double x,double z)
        {
            float h=Skeleton(x,z,Macro(x,z));
            if(NearbyFeature(x,z,out var f))
            {
                float d=FeatureDistance(x,z,f);
                float carve=1-Ramp(d,.52f,1);
                float floor=f.Level-f.Depth+Noise(x,z,.055,543)*.5f;
                if(f.Kind==PCGLiquidKind.Marsh)
                    floor+=Ramp(Noise(x,z,.045,544),.5f,.72f)*2.2f; // Dry hummocks
                if(f.Kind==PCGLiquidKind.Lava)
                    floor+=Ramp(Noise(x,z,.036,545),.64f,.80f)*8; // Cold rock islands
                h=h*(1-carve)+floor*carve;
            }
            return Math.Max(2,Math.Min(76,h));
        }
        public PCGEnvironmentSample Query(double x,double z)
        {
            if(!MultiBiome)return new PCGEnvironmentSample {Height=Height(x,z),Slope=Slope(x,z),Moisture=.55f,
                Temperature=.58f,Forest=Forest(x,z),Liquid=Height(x,z)<WaterLevel?PCGLiquidKind.Water:PCGLiquidKind.None,LiquidLevel=WaterLevel};
            return Environment(x,z,Height(x,z),Slope(x,z));
        }
        private PCGEnvironmentSample Environment(double x,double z,float h,float slope)
        {
            double lx=LandscapeX(x),lz=LandscapeZ(z);
            var e=CanonicalEnvironment(lx,lz,_preserveConstruction?BiomeHeight(lx,lz):LandscapeHeight(h),slope);
            e.Height=h;if(e.Liquid!=PCGLiquidKind.None)e.LiquidLevel=WorldHeight(e.LiquidLevel);
            if(ConstructionWeight(x,z)<1){e.Liquid=PCGLiquidKind.None;e.LiquidId=0;e.LiquidLevel=0;}
            return e;
        }
        private PCGEnvironmentSample CanonicalEnvironment(double x,double z,float h,float slope)
        {
            // 覆盖分类采用规范地貌坡度，接触和放置仍使用实际世界坡度。
            float coverSlope=(float)(Math.Atan(Math.Tan(slope*Math.PI/180)*LandscapeScale/ReliefScale)*180/Math.PI);
            Climate c=Macro(x,z);
            bool has=NearbyFeature(x,z,out var liquid);
            float localHeat=has&&liquid.Kind==PCGLiquidKind.Lava?(1-Ramp(FeatureDistance(x,z,liquid),.5f,1))*.25f:0;
            float temp=c.Temperature-Math.Max(0,h-18)*.004f+c.Hot*.05f+localHeat;
            float snow=Ramp(.39f-temp,0,.13f)*(1-Ramp(coverSlope,31,49))*(1-c.Hot*.4f);
            float wet=Ramp(c.Moisture,.66f,.86f)*(1-c.Mountain)*(1-c.Hot);
            float distance=has?FeatureDistance(x,z,liquid):2;
            float shore=has?1-Ramp(Math.Abs(h-liquid.Level),.5f,2.6f):0;
            float oasis=has&&liquid.Kind==PCGLiquidKind.Water?1-Ramp(distance,.78f,1):0;
            float sand=c.Dry*(1-oasis*.9f),hot=c.Hot;
            float forest=Noise(x,z,.013,101)*Ramp(c.Moisture,.34f,.62f)*(1-sand)*(1-hot)*(1-snow*.8f);
            float rock=Ramp(coverSlope,17,36)+c.Mountain*(.58f+Ramp(h,18,40)*.20f);
            rock=Clamp(rock);
            var covers=new PCGCoverWeights { Meadow=(1-sand)*(1-hot)*(1-snow)*(1-wet)*(1-rock),
                Soil=forest*.4f*(1-snow),Rock=rock*(1-hot)*(1-snow),
                Sand=sand*(1-snow),Mud=wet*(1-snow),Snow=snow,Volcanic=hot*(1-snow),
                Shore=shore*(1-hot)*(1-wet)*(1-snow) };
            covers.Normalize();
            return new PCGEnvironmentSample {Height=h,Slope=slope,Temperature=temp,Moisture=Math.Max(c.Moisture,oasis*.85f),
                Mountain=c.Mountain,Dryness=sand,Geothermal=hot,Forest=forest,Cover=covers,
                Liquid=has?liquid.Kind:PCGLiquidKind.None,LiquidId=has?liquid.Id:0,LiquidLevel=has?liquid.Level:0};
        }
        private PCGChunkData GenerateBiomes(Vector2Int key,CancellationToken token)
        {
            var data=new PCGChunkData(key,Resolution,AlphaResolution,8)
            {Version=Version,LandscapeScale=LandscapeScale,ReliefScale=ReliefScale};
            double ox=key.x*(double)ChunkSize,oz=key.y*(double)ChunkSize,step=ChunkSize/(Resolution-1d);
            var halo=new float[Resolution+2,Resolution+2];
            for(int z=-1;z<=Resolution;z++)
            {
                token.ThrowIfCancellationRequested();
                for(int x=-1;x<=Resolution;x++)halo[z+1,x+1]=Height(ox+x*step,oz+z*step);
            }
            for(int z=0;z<Resolution;z++)
            {
                token.ThrowIfCancellationRequested();
                for(int x=0;x<Resolution;x++)
                {
                    float h=halo[z+1,x+1],dx=(halo[z+1,x+2]-halo[z+1,x])/(float)(2*step);
                    float dz=(halo[z+2,x+1]-halo[z,x+1])/(float)(2*step);
                    float slope=(float)(Math.Atan(Math.Sqrt(dx*dx+dz*dz))*180/Math.PI);
                    data.Heights[z,x]=(h-HeightOrigin)/HeightScale;data.Environment[z,x]=Environment(ox+x*step,oz+z*step,h,slope);
                }
            }
            for(int z=0;z<AlphaResolution;z++)for(int x=0;x<AlphaResolution;x++)
            {
                double px=x*(Resolution-1d)/(AlphaResolution-1),pz=z*(Resolution-1d)/(AlphaResolution-1);
                int a=(int)px,b=(int)pz,a1=Math.Min(a+1,Resolution-1),b1=Math.Min(b+1,Resolution-1);
                float u=(float)(px-a),v=(float)(pz-b);
                for(int i=0;i<8;i++)data.Alpha[z,x,i]=
                    (data.Environment[b,a].Cover[i]*(1-u)+data.Environment[b,a1].Cover[i]*u)*(1-v)+
                    (data.Environment[b1,a].Cover[i]*(1-u)+data.Environment[b1,a1].Cover[i]*u)*v;
            }
            var liquids=new List<PCGLiquidFeature>(4);
            long loX=(long)Math.Floor((LandscapeX(ox)-MaximumFeatureRadius)/FeatureCell),hiX=(long)Math.Floor((LandscapeX(ox+ChunkSize)+MaximumFeatureRadius)/FeatureCell);
            long loZ=(long)Math.Floor((LandscapeZ(oz)-MaximumFeatureRadius)/FeatureCell),hiZ=(long)Math.Floor((LandscapeZ(oz+ChunkSize)+MaximumFeatureRadius)/FeatureCell);
            for(long z=loZ;z<=hiZ;z++)for(long x=loX;x<=hiX;x++)
            {
                var f=Feature(x,z);if(!f.Valid)continue;
                float extent=f.Radius*1.15f;
                if(f.Center.x+extent>=ox&&f.Center.x-extent<=ox+ChunkSize&&f.Center.y+extent>=oz&&f.Center.y-extent<=oz+ChunkSize)
                    liquids.Add(f);
            }
            data.Liquids=liquids.ToArray();return data;
        }
    }
}
