using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Editor.PCG
{

    /// <summary>
    /// 矿石类型（决定矿石簇的外观与结构，对应现实中不同矿物的晶体形态）。
    /// </summary>
    public enum OreType
    {
        Rubble = 0,     // 不规则碎石（通用矿石 / 铁矿碎块）
        Cubic = 1,      // 立方块（黄铁矿、方铅矿）
        Columnar = 2,   // 柱状晶簇（石英、电气石）
        Platy = 3,      // 片状堆叠（云母、石墨）
        Botryoidal = 4  // 葡萄串（赤铁矿、孔雀石、玛瑙）
    }

    /// <summary>
    /// PCG 共享 mesh 生成工厂（ArtResource）：岩石 / 晶体 / 矿石簇的程序化生成算法。
    /// 关键参数均为「范围值」（Vector2：x=最小 y=最大），生成每个变体时在范围内随机取值，
    /// 从而让同一预设产出多样化的个体。矿石簇按 OreType 区分晶体形态与聚集结构。
    /// </summary>
    public static class PCGMeshFactory
    {
        private static float RandomRange(System.Random rng, Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, (float)rng.NextDouble());
        }

        /// <summary>种子散列：把相邻 seed（seed+0, seed+1, ...）打散成均匀分布的整数，
        /// 避免 System.Random 相邻种子产生的随机序列高度相关（变体趋同的主因）。</summary>
        private static int HashSeed(int seed)
        {
            unchecked
            {
                uint x = (uint)seed + 0x9E3779B9u;
                x = (x ^ (x >> 16)) * 0x85EBCA6Bu;
                x = (x ^ (x >> 13)) * 0xC2B2AE35u;
                x ^= x >> 16;
                return (int)x;
            }
        }

        /// <summary>把 seed 散列成 [0,1) 均匀小数，用于 PerlinNoise 采样偏移（避免巨大整数进入 256 周期循环）。</summary>
        private static float Hash01(int seed)
        {
            return (HashSeed(seed) & 0xFFFFFF) / 16777215f;
        }

        // ============ Ico Sphere（共享顶点 + 细分） ============

        public static Mesh CreateIcoSphere(int subdivisions)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            Vector3[] baseVerts = new Vector3[12]
            {
                new Vector3(-1, t, 0).normalized, new Vector3(1, t, 0).normalized,
                new Vector3(-1, -t, 0).normalized, new Vector3(1, -t, 0).normalized,
                new Vector3(0, -1, t).normalized, new Vector3(0, 1, t).normalized,
                new Vector3(0, -1, -t).normalized, new Vector3(0, 1, -t).normalized,
                new Vector3(t, 0, -1).normalized, new Vector3(t, 0, 1).normalized,
                new Vector3(-t, 0, -1).normalized, new Vector3(-t, 0, 1).normalized
            };
            int[] baseTris = new int[60]
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };

            List<Vector3> verts = new List<Vector3>(baseVerts);
            List<int> tris = new List<int>(baseTris);
            Dictionary<long, int> cache = new Dictionary<long, int>();

            for (int s = 0; s < subdivisions; s++)
            {
                List<int> newTris = new List<int>(tris.Count * 4);
                cache.Clear();
                for (int i = 0; i < tris.Count; i += 3)
                {
                    int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                    int ab = GetMid(a, b, verts, cache);
                    int bc = GetMid(b, c, verts, cache);
                    int ca = GetMid(c, a, verts, cache);
                    newTris.Add(a); newTris.Add(ab); newTris.Add(ca);
                    newTris.Add(b); newTris.Add(bc); newTris.Add(ab);
                    newTris.Add(c); newTris.Add(ca); newTris.Add(bc);
                    newTris.Add(ab); newTris.Add(bc); newTris.Add(ca);
                }
                tris = newTris;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static int GetMid(int a, int b, List<Vector3> verts, Dictionary<long, int> cache)
        {
            long key = (long)Mathf.Min(a, b) * 1000000 + Mathf.Max(a, b);
            if (cache.TryGetValue(key, out int idx))
            {
                return idx;
            }
            Vector3 mid = ((verts[a] + verts[b]) * 0.5f).normalized;
            idx = verts.Count;
            verts.Add(mid);
            cache[key] = idx;
            return idx;
        }

        // ============ 晶体（尖刺多面体） ============

        /// <summary>尖刺晶体：尖刺比例/长度/底部半径均在范围内随机取值。</summary>
        public static Mesh CreateCrystal(int seed, int subdivisions,
            Vector2 spikeRatioRange, Vector2 spikeLenRange, Vector2 baseRadiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            float spikeRatio = RandomRange(rng, spikeRatioRange);
            float spikeLen = RandomRange(rng, spikeLenRange);
            float baseRadius = RandomRange(rng, baseRadiusRange);

            Mesh mesh = CreateIcoSphere(subdivisions);
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 d = verts[i].normalized;
                if (rng.NextDouble() < spikeRatio)
                {
                    float len = spikeLen * (0.85f + (float)rng.NextDouble() * 0.3f);
                    verts[i] = d * Mathf.Max(1.05f, len);
                }
                else
                {
                    verts[i] = d * (baseRadius + (float)rng.NextDouble() * 0.2f);
                }
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // ============ 岩石（噪声位移 + Y 压扁） ============

        /// <summary>岩石：噪声强度与 Y 压扁在范围内随机取值。</summary>
        public static Mesh CreateRock(int seed, int subdivisions, Vector2 noiseRange, Vector2 squashRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            float noise = RandomRange(rng, noiseRange);
            float squash = RandomRange(rng, squashRange);

            Mesh ico = CreateIcoSphere(subdivisions);
            Vector3[] verts = ico.vertices;
            float off = Hash01(seed) * 100f;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 d = verts[i].normalized;
                float n = Mathf.PerlinNoise(d.x * 2f + off, d.y * 2f + off + 3.7f);
                n = (n - 0.5f) * 2f;
                Vector3 v = d * (0.7f + n * noise);
                v.y *= squash;
                verts[i] = v;
            }
            ico.vertices = verts;
            ico.RecalculateNormals();
            ico.RecalculateBounds();
            return ico;
        }

        // ============ 矿石块形态（不同矿物） ============

        /// <summary>不规则碎石块：Ico Sphere + Perlin 噪声位移，模拟破碎矿石。</summary>
        public static Mesh CreateRubbleBlock(int seed, float size)
        {
            Mesh ico = CreateIcoSphere(2);
            Vector3[] verts = ico.vertices;
            float off = Hash01(seed) * 100f;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 d = verts[i].normalized;
                float n = Mathf.PerlinNoise(d.x * 3f + off, d.y * 3f + off + 5.2f);
                n = (n - 0.5f) * 2f;
                verts[i] = d * (0.8f + n * 0.4f) * size;
            }
            ico.vertices = verts;
            ico.RecalculateNormals();
            ico.RecalculateBounds();
            return ico;
        }

        /// <summary>立方矿块：立方体 + 顶点随机微调，模拟黄铁矿/方铅矿的立方晶体。</summary>
        public static Mesh CreateCubicBlock(int seed, float size)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            Vector3[] verts = new Vector3[8]
            {
                new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f),
                new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f),
                new Vector3(-0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,0.5f),
                new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f)
            };
            int[] tris = new int[36]
            {
                0,2,1, 0,3,2,   // -Z
                4,5,6, 4,6,7,   // +Z
                4,7,3, 4,3,0,   // -X
                1,2,6, 1,6,5,   // +X
                3,7,6, 3,6,2,   // +Y
                0,1,5, 0,5,4    // -Y
            };
            for (int i = 0; i < 8; i++)
            {
                Vector3 jitter = new Vector3(
                    (float)(rng.NextDouble() - 0.5) * 0.18f,
                    (float)(rng.NextDouble() - 0.5) * 0.18f,
                    (float)(rng.NextDouble() - 0.5) * 0.18f);
                verts[i] = (verts[i] + jitter) * size;
            }
            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// 柱状晶体：有明确棱面、晶柱与尖端的六棱柱，而不是拉长球。
        /// 默认向上生成，供柱状晶簇以「主晶体 V 形 + 短晶体基座」组合。
        /// </summary>
        public static Mesh CreateColumnBlock(int seed, float size, float length)
        {
            const int sides = 6;
            System.Random rng = new System.Random(HashSeed(seed));
            float height = size * length;
            float baseRadius = size * (0.82f + (float)rng.NextDouble() * 0.16f);
            float shoulderRadius = baseRadius * (0.74f + (float)rng.NextDouble() * 0.10f);
            float shoulderY = height * (0.68f + (float)rng.NextDouble() * 0.10f);
            float elliptic = 0.78f + (float)rng.NextDouble() * 0.18f;

            Vector3[] verts = new Vector3[sides * 2 + 2];
            int baseCenter = sides * 2;
            int tip = baseCenter + 1;
            for (int i = 0; i < sides; i++)
            {
                float angle = Mathf.PI * 2f * i / sides + Mathf.PI / 6f;
                float irregular = 0.92f + (float)rng.NextDouble() * 0.14f;
                float x = Mathf.Cos(angle) * irregular;
                float z = Mathf.Sin(angle) * elliptic * irregular;
                verts[i] = new Vector3(x * baseRadius, 0f, z * baseRadius);
                verts[sides + i] = new Vector3(x * shoulderRadius, shoulderY, z * shoulderRadius);
            }
            verts[baseCenter] = Vector3.zero;
            verts[tip] = new Vector3((float)(rng.NextDouble() - 0.5) * size * 0.12f,
                height, (float)(rng.NextDouble() - 0.5) * size * 0.12f);

            // 每个晶面独立顶点，避免 Unity 平滑法线把六棱晶柱渲染成圆柱。
            List<Vector3> flatVerts = new List<Vector3>(sides * 16);
            List<int> flatTris = new List<int>(sides * 12);
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                // 底面、晶柱六个宽面、顶部六角锥。
                AddFlatTriangle(flatVerts, flatTris, verts[baseCenter], verts[next], verts[i]);
                AddFlatQuad(flatVerts, flatTris, verts[i], verts[next], verts[sides + next], verts[sides + i]);
                AddFlatTriangle(flatVerts, flatTris, verts[sides + i], verts[sides + next], verts[tip]);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = flatVerts.ToArray();
            mesh.triangles = flatTris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddFlatTriangle(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c)
        {
            int index = verts.Count;
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            tris.Add(index);
            tris.Add(index + 1);
            tris.Add(index + 2);
        }

        private static void AddFlatQuad(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddFlatTriangle(verts, tris, a, b, c);
            AddFlatTriangle(verts, tris, a, c, d);
        }

        /// <summary>片状矿块：Ico Sphere 压扁 + 边缘噪声，模拟云母薄片。</summary>
        public static Mesh CreatePlateBlock(int seed, float size)
        {
            Mesh ico = CreateIcoSphere(2);
            Vector3[] verts = ico.vertices;
            float off = Hash01(seed) * 100f;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 d = verts[i].normalized;
                float n = Mathf.PerlinNoise(d.x * 3f + off, d.z * 3f + off + 2.7f);
                n = (n - 0.5f) * 2f;
                Vector3 v = d;
                v.y *= 0.22f;            // 压扁成片
                v.x += n * 0.25f;         // 边缘不规则
                v.z += n * 0.25f;
                verts[i] = v * size;
            }
            ico.vertices = verts;
            ico.RecalculateNormals();
            ico.RecalculateBounds();
            return ico;
        }

        // ============ 矿石簇（按类型分簇） ============

        /// <summary>按类型生成矿石簇；块大小与聚集半径均为范围值，每块随机取值。</summary>
        public static Mesh CreateOreCluster(OreType type, int seed, int blockCount,
            Vector2 blockSizeRange, Vector2 clusterRadiusRange)
        {
            switch (type)
            {
                case OreType.Cubic:
                    return CreateCubicCluster(seed, blockCount, blockSizeRange, clusterRadiusRange);
                case OreType.Columnar:
                    return CreateColumnarCluster(seed, blockCount, blockSizeRange, clusterRadiusRange);
                case OreType.Platy:
                    return CreatePlatyCluster(seed, blockCount, blockSizeRange, clusterRadiusRange);
                case OreType.Botryoidal:
                    return CreateBotryoidalCluster(seed, blockCount, blockSizeRange, clusterRadiusRange);
                case OreType.Rubble:
                default:
                    return CreateRubbleCluster(seed, blockCount, blockSizeRange, clusterRadiusRange);
            }
        }

        private static Vector3 RandomPointInSphere(System.Random rng, float radius)
        {
            float theta = (float)(rng.NextDouble() * Mathf.PI * 2.0);
            float phi = (float)(rng.NextDouble() * Mathf.PI);
            float r = radius * Mathf.Pow((float)rng.NextDouble(), 1f / 3f);
            return new Vector3(
                r * Mathf.Sin(phi) * Mathf.Cos(theta),
                r * Mathf.Sin(phi) * Mathf.Sin(theta),
                r * Mathf.Cos(phi));
        }

        private static Quaternion RandomRotation(System.Random rng)
        {
            return Quaternion.Euler(
                (float)(rng.NextDouble() * 360.0),
                (float)(rng.NextDouble() * 360.0),
                (float)(rng.NextDouble() * 360.0));
        }

        private static Mesh MergeBlocks(List<Mesh> blocks, List<Vector3> positions, List<Quaternion> rotations)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            for (int i = 0; i < blocks.Count; i++)
            {
                int baseIndex = verts.Count;
                Matrix4x4 m = Matrix4x4.TRS(positions[i], rotations[i], Vector3.one);
                Vector3[] bv = blocks[i].vertices;
                int[] bt = blocks[i].triangles;
                for (int k = 0; k < bv.Length; k++)
                {
                    verts.Add(m.MultiplyPoint3x4(bv[k]));
                }
                for (int k = 0; k < bt.Length; k++)
                {
                    tris.Add(baseIndex + bt[k]);
                }
            }
            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>不规则碎石堆：碎石块在球体内随机聚集 + 随机旋转。</summary>
        private static Mesh CreateRubbleCluster(int seed, int blockCount, Vector2 blockSizeRange, Vector2 radiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            List<Mesh> blocks = new List<Mesh>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            for (int j = 0; j < blockCount; j++)
            {
                float size = RandomRange(rng, blockSizeRange);
                blocks.Add(CreateRubbleBlock(seed + j * 37, size));
                positions.Add(RandomPointInSphere(rng, RandomRange(rng, radiusRange)));
                rotations.Add(RandomRotation(rng));
            }
            return MergeBlocks(blocks, positions, rotations);
        }

        /// <summary>立方块堆：立方晶体块随机聚集 + 随机旋转（黄铁矿/方铅矿）。</summary>
        private static Mesh CreateCubicCluster(int seed, int blockCount, Vector2 blockSizeRange, Vector2 radiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            List<Mesh> blocks = new List<Mesh>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            for (int j = 0; j < blockCount; j++)
            {
                float size = RandomRange(rng, blockSizeRange);
                blocks.Add(CreateCubicBlock(seed + j * 37, size));
                positions.Add(RandomPointInSphere(rng, RandomRange(rng, radiusRange)));
                rotations.Add(RandomRotation(rng));
            }
            return MergeBlocks(blocks, positions, rotations);
        }

        /// <summary>
        /// 柱状晶簇：先放置两根偏向左右的主晶体形成 V 形轮廓，再以短晶体围成紧密底座。
        /// 这是参考图中的“宽面晶柱簇”结构，不采用随机球面放射，避免长成刺球。
        /// </summary>
        private static Mesh CreateColumnarCluster(int seed, int blockCount, Vector2 blockSizeRange, Vector2 radiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            blockCount = Mathf.Max(3, blockCount);
            List<Mesh> blocks = new List<Mesh>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            float baseRadius = RandomRange(rng, radiusRange);

            for (int j = 0; j < blockCount; j++)
            {
                bool mainCrystal = j < 2;
                float size = RandomRange(rng, blockSizeRange) * (mainCrystal ? 1.32f : (0.72f + (float)rng.NextDouble() * 0.32f));
                float length = mainCrystal
                    ? 2.75f + (float)rng.NextDouble() * 1.05f
                    : 1.25f + (float)rng.NextDouble() * 1.65f;
                blocks.Add(CreateColumnBlock(seed + j * 37, size, length));

                Vector3 dir;
                Vector3 position;
                if (mainCrystal)
                {
                    // 两根高晶体朝左右分开，形成读取性最强的 V 形顶轮廓。
                    float side = j == 0 ? -1f : 1f;
                    dir = new Vector3(side * (0.32f + (float)rng.NextDouble() * 0.13f),
                        1f, (float)(rng.NextDouble() - 0.5) * 0.20f).normalized;
                    position = new Vector3(side * baseRadius * 0.18f, 0f,
                        (float)(rng.NextDouble() - 0.5) * baseRadius * 0.20f);
                }
                else
                {
                    // 短晶体贴近底座并保持向上，只做有限倾斜，形成层叠而不是刺球。
                    float theta = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                    float radial = baseRadius * (0.15f + (float)rng.NextDouble() * 0.62f);
                    position = new Vector3(Mathf.Cos(theta) * radial, 0f, Mathf.Sin(theta) * radial * 0.72f);
                    dir = new Vector3(Mathf.Cos(theta) * (0.10f + (float)rng.NextDouble() * 0.22f),
                        1f, Mathf.Sin(theta) * (0.10f + (float)rng.NextDouble() * 0.22f)).normalized;
                }
                positions.Add(position);
                rotations.Add(Quaternion.FromToRotation(Vector3.up, dir) *
                              Quaternion.Euler(0f, (float)(rng.NextDouble() * 360.0), 0f));
            }
            return MergeBlocks(blocks, positions, rotations);
        }

        /// <summary>片状堆叠：薄片随机叠放（云母/石墨）。</summary>
        private static Mesh CreatePlatyCluster(int seed, int blockCount, Vector2 blockSizeRange, Vector2 radiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            List<Mesh> blocks = new List<Mesh>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            for (int j = 0; j < blockCount; j++)
            {
                float size = RandomRange(rng, blockSizeRange);
                blocks.Add(CreatePlateBlock(seed + j * 37, size));
                positions.Add(RandomPointInSphere(rng, RandomRange(rng, radiusRange)));
                rotations.Add(Quaternion.Euler(
                    (float)(rng.NextDouble() - 0.5) * 40.0f,
                    (float)(rng.NextDouble() * 360.0),
                    (float)(rng.NextDouble() - 0.5) * 40.0f));
            }
            return MergeBlocks(blocks, positions, rotations);
        }

        /// <summary>葡萄串：小球紧密融合成圆润团块（赤铁矿/孔雀石/玛瑙）。</summary>
        private static Mesh CreateBotryoidalCluster(int seed, int blockCount, Vector2 blockSizeRange, Vector2 radiusRange)
        {
            System.Random rng = new System.Random(HashSeed(seed));
            List<Mesh> blocks = new List<Mesh>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            for (int j = 0; j < blockCount; j++)
            {
                float size = RandomRange(rng, blockSizeRange);
                Mesh b = CreateIcoSphere(1);
                Vector3[] bv = b.vertices;
                for (int k = 0; k < bv.Length; k++)
                {
                    bv[k] *= size;
                }
                b.vertices = bv;
                b.RecalculateBounds();
                blocks.Add(b);
                positions.Add(RandomPointInSphere(rng, RandomRange(rng, radiusRange) * 0.8f));
                rotations.Add(Quaternion.identity);
            }
            return MergeBlocks(blocks, positions, rotations);
        }

        /// <summary>每种矿石类型的默认颜色（参考现实矿物）。</summary>
        public static Color GetDefaultOreColor(OreType type)
        {
            switch (type)
            {
                case OreType.Cubic: return new Color(0.85f, 0.70f, 0.30f);      // 黄铁矿金黄
                case OreType.Columnar: return new Color(0.80f, 0.76f, 0.92f);   // 石英淡紫
                case OreType.Platy: return new Color(0.72f, 0.74f, 0.80f);      // 云母银白
                case OreType.Botryoidal: return new Color(0.15f, 0.55f, 0.35f); // 孔雀石绿
                case OreType.Rubble:
                default: return new Color(0.55f, 0.48f, 0.40f);                 // 铁矿灰棕
            }
        }

        /// <summary>在占地范围 [min,max] 内随机取一个占地边长（米，正方形）。</summary>
        public static float RandomFootprint(Vector2 range, System.Random rng)
        {
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);
            if (max <= 0f)
            {
                max = 1f;
            }
            return min + (float)rng.NextDouble() * (max - min);
        }

        /// <summary>根据占地大小（正方形边长，米）反算 uniform scale，使 mesh 水平占地 = footprint。</summary>
        public static float ScaleForFootprint(Mesh mesh, float footprint)
        {
            float h = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.z);
            return h > 0.0001f ? footprint / h : 1f;
        }
    }
}
