using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.PCG
{
    /// <summary>
    /// 运行时散布器：在组件为中心的矩形区域内，用泊松盘采样散布岩石（RockPreset 程序化 mesh）与植株（Prefab）。
    /// 岩石先占位、植株避开岩石；贴地与法线对齐通过 IGroundSampler 抽象，与具体地表形态解耦。
    /// 与 Editor 散布面板（PCGPanel）同源算法，运行时无需 UnityEditor。
    /// </summary>
    public sealed class RuntimeScatterer : MonoBehaviour
    {
        [Header("散布区域（相对组件中心，XZ 平面）")]
        [InspectorName("散布区域大小"), SerializeField] private Vector2 _area = new Vector2(80f, 80f);
        [InspectorName("散布密度"), SerializeField] private float _density = 0.05f;
        [InspectorName("最小间距"), SerializeField] private float _minDistance = 1.5f;
        [InspectorName("随机种子"), SerializeField] private int _seed = 20261001;

        [Header("岩石")]
        [InspectorName("岩石种类"), SerializeField] private List<PCGScatterUtility.RockVariety> _rockVarieties = new List<PCGScatterUtility.RockVariety>();
        [InspectorName("岩石占比"), SerializeField] private float _rockShare = 0.3f;
        [InspectorName("岩石最小下沉量"), SerializeField] private float _rockSinkMin = 0.1f;

        [Header("植株")]
        [InspectorName("植被预制体"), SerializeField] private List<GameObject> _vegetationPrefabs = new List<GameObject>();

        [Header("地表对齐")]
        [InspectorName("贴合地表"), SerializeField] private bool _alignToSurface = false;
        [InspectorName("最大坡度"), SerializeField] private float _maxSlope = 30f;
        [InspectorName("启动时生成"), SerializeField] private bool _buildOnStart = true;

        private readonly List<GameObject> _spawned = new List<GameObject>();

        /// <summary>由外部传入的地表采样器；为空时内部用平地采样器（高度 0）。</summary>
        public IGroundSampler Ground { get; set; }

        /// <summary>是否已经散布过对象。</summary>
        public bool IsBuilt => _spawned.Count > 0;

        private void Start()
        {
            if (_buildOnStart)
            {
                Build();
            }
        }

        /// <summary>执行散布；会先清空上一批。</summary>
        public void Build()
        {
            Clear();
            IGroundSampler ground = Ground ?? new PlaneGroundSampler(0f);
            System.Random rng = new System.Random(_seed);

            bool hasRock = HasValidRockVariety();
            bool hasVeg = _vegetationPrefabs.Count > 0;
            if (!hasRock && !hasVeg)
            {
                return;
            }

            int target = Mathf.Clamp(Mathf.RoundToInt(_density * _area.x * _area.y), 1, 30000);
            List<Vector2> points = PCGScatterUtility.PoissonDisk(_area.x, _area.y, _minDistance, _seed, 30000);
            Shuffle(points, rng);
            if (points.Count > target)
            {
                points = points.GetRange(0, target);
            }

            // 岩石位置记录（植株避开已放置岩石）
            List<Vector2> rockPos = new List<Vector2>();
            List<float> rockRadius = new List<float>();

            foreach (Vector2 point in points)
            {
                Vector3 world = LocalToWorld(point);
                world.y = ground.SampleHeight(world);

                Vector3 normal = Vector3.up;
                if (_alignToSurface)
                {
                    normal = ground.SampleNormal(world);
                    if (Vector3.Angle(Vector3.up, normal) > _maxSlope)
                    {
                        continue;
                    }
                }
                Quaternion rotation = _alignToSurface
                    ? Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0f, rng.Next(360), 0f)
                    : Quaternion.Euler(0f, rng.Next(360), 0f);

                bool asRock = hasRock && (!hasVeg || rng.NextDouble() < (double)_rockShare);

                if (asRock)
                {
                    RockPreset preset = PCGScatterUtility.PickWeightedRock(_rockVarieties, rng);
                    if (preset == null)
                    {
                        continue;
                    }
                    int variant = rng.Next(Mathf.Max(1, preset.RockCount));
                    Mesh rockMesh = preset.CreateRockMesh(variant);
                    float footprint = PCGMeshFactory.RandomFootprint(preset.FootprintRange, rng);
                    float scale = PCGMeshFactory.ScaleForFootprint(rockMesh, footprint);

                    // 下沉：下表面从「埋入 _rockSinkMin」到「中心贴地（半埋）」随机，丰富岩石姿态
                    float bottomOffset = rockMesh.bounds.extents.y - rockMesh.bounds.center.y;
                    float sinkMax = rockMesh.bounds.extents.y;
                    float sink = UnityEngine.Random.Range(Mathf.Min(_rockSinkMin, sinkMax), sinkMax);
                    bottomOffset -= sink;
                    Vector3 actualPos = world + normal * (bottomOffset * scale);

                    Material mat = PCGScatterUtility.ResolveMaterial(preset.Material, preset.Color, 1f, preset.Smoothness, preset.Metallic);
                    GameObject go = new GameObject(preset.name + "_rock");
                    go.transform.SetParent(transform, true);
                    go.transform.position = actualPos;
                    go.transform.localScale = Vector3.one * scale;
                    go.transform.rotation = rotation;
                    go.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                    go.AddComponent<MeshCollider>().sharedMesh = rockMesh;

                    float radius = PCGScatterUtility.GetOccupancyRadius(go, out _);
                    if (PCGScatterUtility.OverlapAny(rockPos, rockRadius, actualPos.x, actualPos.z, radius))
                    {
                        Destroy(go);
                        continue;
                    }
                    rockPos.Add(new Vector2(actualPos.x, actualPos.z));
                    rockRadius.Add(radius);
                    _spawned.Add(go);
                }
                else
                {
                    GameObject prefab = _vegetationPrefabs[rng.Next(_vegetationPrefabs.Count)];
                    if (prefab == null)
                    {
                        continue;
                    }
                    GameObject go = Instantiate(prefab, world, rotation, transform);
                    float radius = PCGScatterUtility.GetOccupancyRadius(go, out bool hasCollider);
                    // 有碰撞体的木本植株（树/灌木）避开已放置岩石
                    if (hasCollider && PCGScatterUtility.OverlapAny(rockPos, rockRadius, world.x, world.z, radius))
                    {
                        Destroy(go);
                        continue;
                    }
                    _spawned.Add(go);
                }
            }
        }

        /// <summary>清空已散布的对象。</summary>
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

        private Vector3 LocalToWorld(Vector2 point)
        {
            Vector3 local = new Vector3(point.x - _area.x * 0.5f, 0f, point.y - _area.y * 0.5f);
            return transform.TransformPoint(local);
        }

        private bool HasValidRockVariety()
        {
            for (int i = 0; i < _rockVarieties.Count; i++)
            {
                PCGScatterUtility.RockVariety v = _rockVarieties[i];
                if (v != null && v.Preset != null && v.Weight > 0f)
                {
                    return true;
                }
            }
            return false;
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
