using System;
using System.Threading;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    /// <summary>Pure world-coordinate data. No Unity object APIs or shared random state.</summary>
    public sealed partial class PCGWorldField
    {
        public readonly int Seed;
        public readonly float ChunkSize, HeightScale, WaterLevel;
        public readonly int Resolution, AlphaResolution;
        public readonly float LandscapeScale;
        public readonly float ReliefScale;
        public readonly float HeightOrigin;
        private readonly bool _preserveConstruction;
        private readonly Rect _constructionRegion;
        private readonly float _constructionBlend;
        private readonly float _constructionEdgeNoise;
        private readonly float _constructionRelief;
        private readonly float _constructionBaseHeight;
        private readonly float _startClimateBias;
        private const float HeightAnchor = 12f;
        public double LandscapeX(double x)=>LandscapeScale==1?x:8+(x-8)/LandscapeScale;
        public double LandscapeZ(double z)=>LandscapeScale==1?z:12+(z-12)/LandscapeScale;
        public Vector2 WorldPoint(Vector2 p)=>LandscapeScale==1?p:new Vector2(8,12)+(p-new Vector2(8,12))*LandscapeScale;
        private float WorldHeight(float h)=>HeightOrigin+HeightAnchor+(h-HeightAnchor)*ReliefScale;
        private float LandscapeHeight(float h)=>HeightAnchor+(h-HeightOrigin-HeightAnchor)/Mathf.Max(.001f,ReliefScale);
        private float ConstructionWeight(double x,double z)
        {
            if(!_preserveConstruction)return 1;
            double dx=Math.Max(Math.Max(_constructionRegion.xMin-x,x-_constructionRegion.xMax),0);
            double dz=Math.Max(Math.Max(_constructionRegion.yMin-z,z-_constructionRegion.yMax),0);
            double distance=Math.Max(dx,dz);
            if(distance<=0)return _constructionRelief;
            // 核心保持矩形平地，噪声只改变外侧恢复距离，边界处高度和一阶变化连续。
            float width=_constructionBlend*(1+(Noise(x,z,.0125,548)*2-1)*_constructionEdgeNoise);
            float edge=Smooth((float)distance/width);
            return _constructionRelief+(1-_constructionRelief)*edge;
        }
        private float ApplyConstructionHeight(double x,double z,float naturalHeight)
        {
            float weight=ConstructionWeight(x,z);
            return _constructionBaseHeight+(naturalHeight-_constructionBaseHeight)*weight;
        }
        private float ConstructionInfluence(double canonicalX,double canonicalZ)
        {
            if(!_preserveConstruction||_startClimateBias<=0)return 0;
            Vector2 world=WorldPoint(new Vector2((float)canonicalX,(float)canonicalZ));
            return (1-ConstructionWeight(world.x,world.y))*_startClimateBias;
        }
        public PCGWorldField(PCGStreamSettings settings)
        {
            Seed = settings.Seed; ChunkSize = settings.ChunkSize; HeightScale = settings.HeightScale;
            WaterLevel = settings.WaterLevel; Resolution = settings.HeightResolution; AlphaResolution = settings.AlphaResolution;
            MultiBiome = settings.MultiBiome; _biomeScale = settings.BiomeScale;
            LandscapeScale=MultiBiome?settings.LandscapeScale:1;
            ReliefScale=MultiBiome?settings.ReliefScale:1;
            HeightOrigin=settings.HeightOrigin;_preserveConstruction=settings.PreserveConstructionRegion;
            _constructionRegion=settings.ConstructionRegion;_constructionBlend=Math.Max(1,settings.ConstructionBlend);
            _constructionEdgeNoise=Mathf.Clamp01(settings.ConstructionEdgeNoise);
            _constructionRelief=Mathf.Clamp(settings.ConstructionRelief,0,1);
            _constructionBaseHeight=settings.ConstructionBaseHeight;
            _startClimateBias=Mathf.Clamp(settings.StartClimateBias,0,1);
            Version = settings.Version;
            _reservedSpaces = settings.ReservedSpaces == null ? Array.Empty<PCGReservedSpace>() :
                (PCGReservedSpace[])settings.ReservedSpaces.Clone();
        }
        public static uint Hash(long x, long z, int seed, int channel)
        {
            unchecked
            {
                ulong h = (ulong)x * 0x9E3779B185EBCA87UL ^ (ulong)z * 0xC2B2AE3D27D4EB4FUL
                    ^ (uint)seed ^ (uint)channel * 0x85EBCA6Bu;
                h ^= h >> 33; h *= 0xff51afd7ed558ccdUL; h ^= h >> 33; h *= 0xc4ceb9fe1a85ec53UL;
                return (uint)(h ^ (h >> 32));
            }
        }
        public static float Unit(long x, long z, int seed, int channel) => (Hash(x, z, seed, channel) & 0xFFFFFF) / 16777216f;
        public float Noise(double x, double z, double frequency, int channel)
        {
            x *= frequency; z *= frequency;
            long ix = (long)Math.Floor(x), iz = (long)Math.Floor(z);
            double u = x - ix, v = z - iz; u = u * u * (3 - 2 * u); v = v * v * (3 - 2 * v);
            double a = Unit(ix, iz, Seed, channel), b = Unit(ix + 1, iz, Seed, channel);
            double c = Unit(ix, iz + 1, Seed, channel), d = Unit(ix + 1, iz + 1, Seed, channel);
            return (float)((a + (b - a) * u) * (1 - v) + (c + (d - c) * u) * v);
        }
        public float Height(double x, double z)
        {
            if (MultiBiome)
            {
                float naturalHeight=WorldHeight(BiomeHeight(LandscapeX(x),LandscapeZ(z)));
                return ApplyConstructionHeight(x,z,naturalHeight);
            }
            double rolling = Noise(x, z, .006, 1) * 5 + Noise(x, z, .021, 2) * 1.8;
            double wet = Math.Max(0, (.32 - Noise(x, z, .009, 3)) / .32);
            wet *= 1 - Math.Exp(-(x*x+z*z)/22500);
            double dx = (x - 52) / 28, dz = (z - 61) / 35;
            double basin = Math.Exp(-(dx * dx + dz * dz) * 1.2) * 9.5;
            double px = (x - 30) / 16, pz = (z - 70) / 10;
            double peninsula = Math.Exp(-(px * px + pz * pz) * 1.5) * 2.1;
            return (float)Math.Max(1, Math.Min(22, 7.3 + rolling + Noise(x, z, .075, 4) * .18 - wet * 10 - basin + peninsula));
        }
        public float Forest(double x, double z) => Noise(x, z, .014, 101);
        public float GrassPatch(double x,double z)
        {
            double dx=x-8,dz=z-12;
            return Math.Max(Noise(x,z,.035,303),(float)(.9*Math.Exp(-(dx*dx+dz*dz)/1800)));
        }
        public float Slope(double x, double z)
        {
            double dx = Height(x + .5, z) - Height(x - .5, z), dz = Height(x, z + .5) - Height(x, z - .5);
            return (float)(Math.Atan(Math.Sqrt(dx * dx + dz * dz)) * 180 / Math.PI);
        }
        public Vector4 Surface(double x, double z)
        {
            float shore = 1 - Smooth((Height(x, z) - WaterLevel - .5f) / 1.6f);
            float rock = Smooth((Slope(x, z) - 17) / 17);
            float soil = .1f + Forest(x, z) * .38f;
            return new Vector4((1-shore)*(1-rock)*(1-soil), (1-shore)*(1-rock)*soil, rock*(1-shore), shore);
        }
        private static float Smooth(float x) { x = Math.Max(0, Math.Min(1, x)); return x * x * (3 - 2 * x); }
        public Vector2Int Chunk(double x, double z) => new Vector2Int((int)Math.Floor(x / ChunkSize), (int)Math.Floor(z / ChunkSize));
        public PCGChunkData Generate(Vector2Int key, CancellationToken token)
        {
            if (MultiBiome) return GenerateBiomes(key, token);
            var data = new PCGChunkData(key, Resolution, AlphaResolution)
            {Version=Version,LandscapeScale=LandscapeScale,ReliefScale=ReliefScale};
            double ox = key.x * (double)ChunkSize, oz = key.y * (double)ChunkSize;
            for (int z = 0; z < Resolution; z++)
            {
                token.ThrowIfCancellationRequested();
                for (int x = 0; x < Resolution; x++)
                    data.Heights[z,x] = Height(ox + x * (double)ChunkSize / (Resolution-1), oz + z * (double)ChunkSize / (Resolution-1)) / HeightScale;
            }
            for (int z = 0; z < AlphaResolution; z++)
            {
                token.ThrowIfCancellationRequested();
                for (int x = 0; x < AlphaResolution; x++)
                {
                    Vector4 w = Surface(ox + x * (double)ChunkSize / (AlphaResolution-1), oz + z * (double)ChunkSize / (AlphaResolution-1));
                    for (int i = 0; i < 4; i++) data.Alpha[z,x,i] = w[i];
                }
            }
            return data;
        }
    }
    public sealed class PCGChunkData
    {
        public readonly Vector2Int Key;
        public readonly float[,] Heights;
        public readonly float[,,] Alpha;
        public readonly PCGEnvironmentSample[,] Environment;
        public PCGLiquidFeature[] Liquids;
        public int Version;
        public float LandscapeScale=1;
        public float ReliefScale=1;
        public PCGChunkData(Vector2Int key, int heightResolution, int alphaResolution, int layers = 4)
        {
            Key = key; Heights = new float[heightResolution,heightResolution]; Alpha = new float[alphaResolution,alphaResolution,layers];
            if(layers == 8) Environment = new PCGEnvironmentSample[heightResolution,heightResolution];
        }
    }
}
