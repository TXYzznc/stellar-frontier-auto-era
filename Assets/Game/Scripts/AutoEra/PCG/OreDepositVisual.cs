using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.PCG
{
    /// <summary>
    /// 运行时矿石视觉生成器：为「地表矿脉」资源点生成矿石簇/晶簇视觉对象，替换编辑器里的 Cube 占位。
    /// 运行时按 OreDistributionConfig 加权随机选一种矿物，用 PCGMeshFactory 动态生成 mesh 并贴地落位。
    /// 地表高度与法线通过 IGroundSampler 采样，与具体地表形态解耦（当前接平地，未来可换 Terrain）。
    /// </summary>
    public sealed class OreDepositVisual : MonoBehaviour
    {
        [Header("矿石概率配置")]
        [InspectorName("矿石概率配置"), SerializeField] private OreDistributionConfig _distribution;

        [Header("占地与生成")]
        [InspectorName("占地范围"), SerializeField] private Vector2 _footprint = new Vector2(10f, 8f);
        [InspectorName("随机种子"), SerializeField] private int _seed = 20261001;
        [InspectorName("矿石数量"), SerializeField] private int _oreCount = 8;
        [InspectorName("边缘留白"), SerializeField] private float _edgeMargin = 0.5f;
        [InspectorName("启动时生成"), SerializeField] private bool _buildOnStart = true;

        private readonly List<GameObject> _spawned = new List<GameObject>();

        /// <summary>由外部传入的地表采样器；为空时内部用平地采样器（高度 0）。</summary>
        public IGroundSampler Ground { get; set; }

        /// <summary>是否已经生成过矿石对象。</summary>
        public bool IsBuilt => _spawned.Count > 0;

        public void Initialize(OreDistributionConfig distribution, Vector2 footprint, int seed, int oreCount, IGroundSampler ground)
        {
            _distribution = distribution;
            _footprint = footprint;
            _seed = seed;
            _oreCount = oreCount;
            Ground = ground;
        }

        private void Start()
        {
            if (_buildOnStart)
            {
                Build();
            }
        }

        /// <summary>按配置生成矿石对象；会先清空上一批。</summary>
        public void Build()
        {
            Clear();
            if (_distribution == null)
            {
                return;
            }

            IGroundSampler ground = Ground ?? new PlaneGroundSampler(0f);
            System.Random rng = new System.Random(_seed);
            OreDistributionConfig.OreEntry entry = _distribution.PickRandom(rng);
            if (entry == null)
            {
                return;
            }

            for (int k = 0; k < _oreCount; k++)
            {
                Vector3 localPos = RandomLocalPosition(rng);
                Vector3 worldPos = transform.TransformPoint(localPos);

                Mesh mesh = entry.IsCrystal
                    ? PCGMeshFactory.CreateCrystal(_seed + k, 2,
                        new Vector2(0.15f, 0.55f), new Vector2(1.4f, 3.2f), new Vector2(0.7f, 1.0f))
                    : PCGMeshFactory.CreateOreCluster(entry.OreKind, _seed + 100 + k, 6,
                        new Vector2(0.25f, 0.55f), new Vector2(0.5f, 0.9f));

                Material mat = entry.Material != null ? entry.Material : CreateFallbackMaterial(entry);

                float footprint = entry.RandomFootprint(rng);
                float scale = PCGMeshFactory.ScaleForFootprint(mesh, footprint);

                // 贴地：让 mesh 底部（而非中心）落到地表，再下沉一点部分埋地，避免悬空。
                float groundY = ground.SampleHeight(worldPos);
                float bottomOffset = (mesh.bounds.center.y - mesh.bounds.extents.y) * scale;
                worldPos.y = groundY - bottomOffset - 0.15f;

                GameObject go = new GameObject(entry.DisplayName + "_" + (k + 1));
                go.transform.SetParent(transform, true);
                go.transform.position = worldPos;
                go.transform.localScale = Vector3.one * scale;
                go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                _spawned.Add(go);
            }
        }

        /// <summary>清空已生成的矿石对象。</summary>
        public void Clear()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i]);
                }
            }
            _spawned.Clear();
        }

        private Vector3 RandomLocalPosition(System.Random rng)
        {
            float halfW = Mathf.Max(0f, _footprint.x * 0.5f - _edgeMargin);
            float halfD = Mathf.Max(0f, _footprint.y * 0.5f - _edgeMargin);
            return new Vector3(
                (float)(rng.NextDouble() * 2.0 - 1.0) * halfW,
                0f,
                (float)(rng.NextDouble() * 2.0 - 1.0) * halfD);
        }

        private static Material CreateFallbackMaterial(OreDistributionConfig.OreEntry entry)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material mat = new Material(shader);
            Color c = entry.IsCrystal
                ? new Color(0.5f, 0.58f, 0.9f, 0.55f)
                : PCGMeshFactory.GetDefaultOreColor(entry.OreKind);
            mat.color = c;
            return mat;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
