using System;
using UnityEngine;

namespace AutoEra.Art.PCG
{
    [Serializable]
    public struct PCGDrawPart
    {
        [InspectorName("网格")] public Mesh Mesh;
        [InspectorName("材质")] public Material Material;
        [InspectorName("子网格索引")] public int Submesh;
        [InspectorName("局部变换矩阵")] public Matrix4x4 LocalMatrix;
    }

    [Serializable]
    public sealed class PCGPlantPrototype
    {
        [InspectorName("绘制部件")] public PCGDrawPart[] Parts;
        [InspectorName("原始高度")] public float Height;
        [InspectorName("基底高度")] public float BaseY;
    }

    [CreateAssetMenu(menuName = "自动纪元/程序化环境/流式环境配置")]
    public sealed class PCGStreamSettings : ScriptableObject
    {
        public const int GeneratorVersion = 3;
        public const int MultiBiomeGeneratorVersion = 6;
        [InspectorName("启用多地貌")]
        public bool MultiBiome;
        [InspectorName("气候变化尺度"), Tooltip("控制温度、湿度和地质类型变化的水平距离；数值越小，越快遇到新地貌。")]
        public float BiomeScale = 1100;
        [InspectorName("地貌水平尺度"), Range(.15f,1f), Tooltip("只控制地貌在水平面上的展开速度；数值越小，越快遇到新地貌。")]
        public float LandscapeScale = 1;
        [InspectorName("地形起伏倍率"), Range(.35f,1.5f), Tooltip("只控制山谷和高差的垂直幅度，不改变遇到新地貌的距离。")]
        public float ReliefScale = .65f;
        [InspectorName("地形基准高度"), Tooltip("地形的基准高度。正式项目保留已有对象的地面坐标。")]
        public float HeightOrigin;
        [InspectorName("保留建设区")]
        public bool PreserveConstructionRegion;
        [InspectorName("建设区范围")]
        public Rect ConstructionRegion = new Rect(-40,-40,80,80);
        [InspectorName("建设区过渡宽度"), Min(1)]
        public float ConstructionBlend = 12;
        [InspectorName("建设区边缘自然度"), Range(0,1), Tooltip("控制建设区边缘的非规则程度。0 为规则矩形过渡，1 为更自然的边界扰动。")]
        public float ConstructionEdgeNoise = .35f;
        [InspectorName("建设区内部起伏"), Range(0,1), Tooltip("建设区内保留的自然地形比例。0 为完全平整，1 为完全使用 PCG 地形。")]
        public float ConstructionRelief = .22f;
        [InspectorName("建设区基准高度"), Tooltip("建设区内部低起伏地形围绕的世界高度；正式对象使用 0。")]
        public float ConstructionBaseHeight;
        [InspectorName("建设区气候引导"), Range(0,1), Tooltip("建设区对初始气候的引导强度；降低后，建设区周边更容易出现非草地地貌。")]
        public float StartClimateBias = .85f;
        [InspectorName("自动原点搬移")]
        public bool AutomaticRebase = true;
        [InspectorName("镜头最小正交尺寸")] public float CameraMinSize = 10;
        [InspectorName("镜头最大正交尺寸")] public float CameraMaxSize = 36;
        [InspectorName("初始镜头正交尺寸")] public float InitialCameraSize = 24;
        [InspectorName("初始镜头焦点")] public Vector2 InitialCameraFocus = new Vector2(38,32);
        [InspectorName("装饰高度包络")] public float DecorationHeightEnvelope = 14;
        [InspectorName("熔岩材质")] public Material LavaMaterial;
        [InspectorName("沼泽水面材质")] public Material MarshWaterMaterial;
        [InspectorName("干草材质")] public Material DryGrassMaterial;
        [InspectorName("沼泽草材质")] public Material MarshGrassMaterial;
        [InspectorName("雪地草材质")] public Material SnowGrassMaterial;
        [InspectorName("芦苇材质")] public Material ReedMaterial;
        [InspectorName("枯木材质")] public Material DeadwoodMaterial;
        [InspectorName("芦苇网格")] public Mesh ReedMesh;
        [InspectorName("枯木网格")] public Mesh DeadwoodMesh;
        [InspectorName("地貌岩石材质")] public Material[] BiomeRockMaterials;
        [InspectorName("雪地树木")] public PCGPlantPrototype[] SnowTrees;
        [InspectorName("干燥灌木")] public PCGPlantPrototype[] DryBushes;
        [InspectorName("地貌定位点")] public PCGBiomeLandmark[] Landmarks;
        [InspectorName("对象避让空间")] public PCGReservedSpace[] ReservedSpaces;
        [InspectorName("启用次级环境效果")] public bool SecondaryEffects = true;
        public int Version => MultiBiome ? MultiBiomeGeneratorVersion : GeneratorVersion;
        [InspectorName("随机种子")] public int Seed = 20261008;
        [InspectorName("区块尺寸")] public float ChunkSize = 64;
        [InspectorName("高度图分辨率")] public int HeightResolution = 65;
        [InspectorName("地表混合图分辨率")] public int AlphaResolution = 64;
        [InspectorName("地形垂直编码范围"), Tooltip("地形高度图的编码范围，不是直接的山体高度参数。")]
        public float HeightScale = 96;
        [InspectorName("水面高度")] public float WaterLevel = 5.5f;
        [InspectorName("显示范围外扩")] public float DisplayMargin = 20;
        [InspectorName("准备范围外扩")] public float PrepareMargin = 64;
        [InspectorName("卸载范围外扩")] public float EvictMargin = 80;
        [InspectorName("缓存保留秒数")] public float RetainSeconds = 2;
        [InspectorName("最大显示区块数")] public int MaxDisplayedChunks = 81;
        [InspectorName("最大缓存区块数")] public int MaxCachedChunks = 256;
        [InspectorName("最大并行数据任务数")] public int MaxConcurrentDataJobs = 2;
        [InspectorName("单帧构建预算毫秒")] public float FrameBudgetMs = 3;
        [InspectorName("草生成间距")] public float GrassSpacing = .28f;
        [InspectorName("草最小高度")] public float GrassMinHeight = .65f;
        [InspectorName("草最大高度")] public float GrassMaxHeight = 1.1f;
        [InspectorName("草最远显示距离")] public float GrassDrawDistance = 130;
        [InspectorName("接触恢复秒数")] public float RecoverySeconds = 12;
        [InspectorName("最大足迹单元数")] public int MaxTrailCells = 32768;
        [InspectorName("足迹纹理分辨率")] public int TrailResolution = 256;
        [InspectorName("足迹像素尺寸")] public float TrailPixelSize = 1;
        [InspectorName("地形图层")] public TerrainLayer[] TerrainLayers;
        [InspectorName("地形材质")] public Material TerrainMaterial;
        [InspectorName("水面材质")] public Material WaterMaterial;
        [InspectorName("草材质")] public Material GrassMaterial;
        [InspectorName("草网格")] public Mesh GrassMesh;
        [InspectorName("岩石材质")] public Material RockMaterial;
        [InspectorName("岩石网格")] public Mesh[] RockMeshes;
        [InspectorName("树木")] public PCGPlantPrototype[] Trees;
        [InspectorName("灌木")] public PCGPlantPrototype[] Bushes;

        public void SetLandscapeScale(float scale)
        {
            if(float.IsNaN(scale)||float.IsInfinity(scale)||scale<.15f||scale>1)
                throw new ArgumentOutOfRangeException(nameof(scale));
            if(scale==LandscapeScale)return;
            float ratio=scale/LandscapeScale;Vector2 anchor=new Vector2(8,12);
            if(Landmarks!=null)
            {
                var points=(PCGBiomeLandmark[])Landmarks.Clone();
                for(int i=0;i<points.Length;i++)points[i].Position=anchor+(points[i].Position-anchor)*ratio;
                Landmarks=points;
            }
            if(ReservedSpaces!=null&&!PreserveConstructionRegion)
            {
                var spaces=(PCGReservedSpace[])ReservedSpaces.Clone();
                for(int i=0;i<spaces.Length;i++)
                {
                    spaces[i].Center=anchor+(spaces[i].Center-anchor)*ratio;
                    spaces[i].Start=anchor+(spaces[i].Start-anchor)*ratio;
                    spaces[i].End=anchor+(spaces[i].End-anchor)*ratio;
                    spaces[i].PadRadius*=ratio;spaces[i].RouteHalfWidth*=ratio;
                }
                ReservedSpaces=spaces;
            }
            LandscapeScale=scale;
        }

        public void Validate()
        {
            if (ChunkSize < 32 || ChunkSize > 96 || HeightResolution != 65 || AlphaResolution != 64
                || float.IsNaN(LandscapeScale) || LandscapeScale<.15f || LandscapeScale>1
                || float.IsNaN(ReliefScale) || ReliefScale<.35f || ReliefScale>1.5f
                || ConstructionBlend < 1 || ConstructionEdgeNoise < 0 || ConstructionEdgeNoise > 1
                || ConstructionRelief < 0 || ConstructionRelief > 1
                || StartClimateBias < 0 || StartClimateBias > 1
                || CameraMinSize<10 || CameraMaxSize>36 || CameraMinSize>CameraMaxSize
                || InitialCameraSize<CameraMinSize || InitialCameraSize>CameraMaxSize
                || HeightScale <= 0 || DisplayMargin < 0 || PrepareMargin < ChunkSize || EvictMargin <= DisplayMargin
                || MaxDisplayedChunks < 49 || MaxCachedChunks < MaxDisplayedChunks + 32
                || MaxConcurrentDataJobs < 1 || MaxConcurrentDataJobs > 4 || FrameBudgetMs <= 0
                || GrassSpacing < .25f || GrassMinHeight <= 0 || GrassMaxHeight <= GrassMinHeight || GrassMaxHeight > 2
                || RecoverySeconds <= 0 || MaxTrailCells < 4096
                || TrailResolution != 256 || TrailPixelSize < 1)
                throw new ArgumentException("Streaming settings outside supported prototype envelope.");
            if (TerrainLayers == null || TerrainLayers.Length != (MultiBiome ? 8 : 4) || TerrainMaterial == null || WaterMaterial == null
                || GrassMaterial == null || GrassMesh == null || Trees == null || Trees.Length == 0
                || Bushes == null || Bushes.Length == 0 || RockMeshes == null || RockMeshes.Length == 0)
                throw new ArgumentException("Streaming asset references are incomplete.");
            if (MultiBiome && (HeightScale < 80 || BiomeScale < 512 || BiomeScale > 2048
                || LavaMaterial == null || MarshWaterMaterial == null || ReedMesh == null || DeadwoodMesh == null
                || ReedMaterial == null || DeadwoodMaterial == null || DryGrassMaterial == null
                || MarshGrassMaterial == null || SnowGrassMaterial == null || BiomeRockMaterials == null
                || BiomeRockMaterials.Length != 5 || SnowTrees == null || SnowTrees.Length == 0
                || DryBushes == null || DryBushes.Length == 0))
                throw new ArgumentException("Multi-biome configuration is incomplete or outside its height/scale envelope.");
        }
    }
}
