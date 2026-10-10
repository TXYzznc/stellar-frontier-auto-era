using UnityEngine;
using UnityEngine.Serialization;

namespace AutoEra.PCG
{

    /// <summary>
    /// 岩石参数预设（ScriptableObject，ArtResource 美术工具）：保存岩石的生成参数。
    /// 关键参数为「范围值」（Vector2：X=最小 Y=最大），生成时在范围内随机取值。
    /// 散布时直接引用本预设，通过 PCGMeshFactory 动态生成 mesh，不依赖 Prefab 资产。
    /// 创建：Assets/Create → AutoEra/PCG/岩石参数预设
    /// </summary>
    [CreateAssetMenu(fileName = "RockPreset", menuName = "AutoEra/PCG/岩石参数预设")]
    public class RockPreset : ScriptableObject
    {
        [Header("岩石（噪声位移 + Y 压扁）")]
        [FormerlySerializedAs("rockCount")]
        [InspectorName("岩石变体数量")] public int RockCount = 8;
        [FormerlySerializedAs("subdivisions")]
        [InspectorName("细分级数")] public int Subdivisions = 3;
        [Tooltip("噪声强度范围：X=最小 Y=最大")]
        [FormerlySerializedAs("noiseRange")]
        [InspectorName("噪声强度范围")] public Vector2 NoiseRange = new Vector2(0.4f, 0.9f);
        [Tooltip("Y 压扁范围：X=最小 Y=最大")]
        [FormerlySerializedAs("squashRange")]
        [InspectorName("纵向压扁范围")] public Vector2 SquashRange = new Vector2(0.5f, 0.85f);
        [Header("材质（可选）")]
        [Tooltip("直接引用材质资产（颜色/金属度/平滑度/Shader 都由材质决定）；为空则回退用下方 color/metallic/smoothness 生成临时材质")]
        [FormerlySerializedAs("material")]
        [InspectorName("材质")] public Material Material;
        [FormerlySerializedAs("color")]
        [InspectorName("颜色")] public Color Color = new Color(0.46f, 0.44f, 0.41f, 1f);
        [Range(0f, 1f)] [FormerlySerializedAs("metallic")]
        [InspectorName("金属度")] public float Metallic = 0f;
        [Range(0f, 1f)] [FormerlySerializedAs("smoothness")]
        [InspectorName("光滑度")] public float Smoothness = 0.18f;
        [Header("占地大小（正方形，Unity 单位）")]
        [Tooltip("占地边长范围：X=最小 Y=最大（米）。生成时按 mesh 实际尺寸反算缩放。")]
        [FormerlySerializedAs("footprintRange")]
        [InspectorName("占地边长范围")] public Vector2 FootprintRange = new Vector2(1f, 2f);
        [FormerlySerializedAs("seed")]
        [InspectorName("随机种子")] public int Seed = 20260930;

        /// <summary>按参数动态生成第 variantIndex 个岩石 mesh（范围值随机，不依赖 Prefab）。</summary>
        public Mesh CreateRockMesh(int variantIndex)
        {
            return PCGMeshFactory.CreateRock(Seed + variantIndex, Subdivisions, NoiseRange, SquashRange);
        }
    }
}
