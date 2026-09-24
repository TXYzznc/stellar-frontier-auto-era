using UnityEngine;

namespace AutoEra.Editor.PCG
{

    /// <summary>
    /// 晶簇参数预设（ScriptableObject，ArtResource 美术工具）：保存晶体 + 矿石簇的生成参数。
    /// 关键参数为「范围值」（Vector2：X=最小 Y=最大），生成时在范围内随机取值，同一预设产出多样个体。
    /// 散布时直接引用本预设，通过 PCGMeshFactory 动态生成 mesh，不依赖 Prefab 资产。
    /// 创建：Assets/Create → AutoEra/PCG/晶簇参数预设
    /// </summary>
    [CreateAssetMenu(fileName = "CrystalClusterPreset", menuName = "AutoEra/PCG/晶簇参数预设")]
    public class CrystalClusterPreset : ScriptableObject
    {
        [Header("晶体（尖刺多面体）")]
        public int CrystalCount = 6;
        public int CrystalSubdivisions = 2;
        [Tooltip("尖刺比例范围：X=最小 Y=最大")]
        public Vector2 SpikeRatioRange = new Vector2(0.15f, 0.55f);
        [Tooltip("尖刺长度范围：X=最小 Y=最大")]
        public Vector2 SpikeLenRange = new Vector2(1.4f, 3.2f);
        [Tooltip("底部半径范围：X=最小 Y=最大")]
        public Vector2 CrystalBaseRadiusRange = new Vector2(0.7f, 1.0f);
        [Tooltip("晶簇材质资产（为空则回退 CrystalColor/CrystalMetallic/CrystalSmoothness）")]
        public Material CrystalMaterial;
        public Color CrystalColor = new Color(0.50f, 0.58f, 0.90f, 0.55f);
        [Range(0f, 1f)] public float CrystalMetallic = 0.05f;
        [Range(0f, 1f)] public float CrystalSmoothness = 0.95f;
        [Header("占地大小（正方形，Unity 单位）")]
        [Tooltip("晶簇占地边长范围：X=最小 Y=最大（米）。生成时按 mesh 实际尺寸反算缩放。")]
        public Vector2 CrystalFootprintRange = new Vector2(0.5f, 1f);

        [Header("矿石簇（多矿块聚集）")]
        public int ClusterCount = 6;
        public OreType OreKind = OreType.Rubble;
        [Tooltip("块数范围：X=最小 Y=最大")]
        public Vector2 BlocksRange = new Vector2(3f, 8f);
        [Tooltip("块大小范围：X=最小 Y=最大")]
        public Vector2 BlockSizeRange = new Vector2(0.25f, 0.55f);
        [Tooltip("聚集半径范围：X=最小 Y=最大")]
        public Vector2 ClusterRadiusRange = new Vector2(0.5f, 0.9f);
        [Tooltip("矿石簇材质资产（为空则回退 OreColor/OreOpacity/OreMetallic/OreSmoothness）")]
        public Material OreMaterial;
        public Color OreColor = new Color(0.60f, 0.46f, 0.30f, 1f);
        [Range(0f, 1f)] public float OreOpacity = 1f;
        [Range(0f, 1f)] public float OreMetallic = 0.65f;
        [Range(0f, 1f)] public float OreSmoothness = 0.45f;
        [Tooltip("矿石簇占地边长范围：X=最小 Y=最大（米）。")]
        public Vector2 OreFootprintRange = new Vector2(0.5f, 1f);

        [Header("通用")]
        public int Seed = 20261002;

        /// <summary>按参数动态生成第 variantIndex 个晶体 mesh（范围值随机，不依赖 Prefab）。</summary>
        public Mesh CreateCrystalMesh(int variantIndex)
        {
            return PCGMeshFactory.CreateCrystal(Seed + variantIndex, CrystalSubdivisions,
                SpikeRatioRange, SpikeLenRange, CrystalBaseRadiusRange);
        }

        /// <summary>按参数动态生成第 variantIndex 个矿石簇 mesh（范围值随机，不依赖 Prefab）。</summary>
        public Mesh CreateClusterMesh(int variantIndex)
        {
            int span = Mathf.Max(1, (int)BlocksRange.y - (int)BlocksRange.x + 1);
            int blocks = (int)BlocksRange.x + (variantIndex % span);
            return PCGMeshFactory.CreateOreCluster(OreKind, Seed + 100 + variantIndex, blocks,
                BlockSizeRange, ClusterRadiusRange);
        }
    }
}
