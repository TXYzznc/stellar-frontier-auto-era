using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.PCG
{
    /// <summary>
    /// PCG 散布共享工具（运行时可用，无 UnityEditor 依赖）。
    /// 从 Editor 散布面板提取的纯算法：泊松盘采样、体积互斥、岩石权重选择、临时材质。
    /// </summary>
    public static class PCGScatterUtility
    {
        /// <summary>岩石种类（预设 + 权重）。散布时按权重随机选一种预设动态生成 mesh。</summary>
        [System.Serializable]
        public sealed class RockVariety
        {
            public RockPreset Preset;
            public float Weight = 1f;
        }

        /// <summary>泊松盘采样：在 width×depth 平面内生成最小间距 minDist 的均匀分布点。</summary>
        public static List<Vector2> PoissonDisk(float width, float depth, float minDist, int seed, int maxPoints)
        {
            System.Random rng = new System.Random(seed);
            float cell = minDist / Mathf.Sqrt(2f);
            int cols = Mathf.CeilToInt(width / cell) + 1;
            int rows = Mathf.CeilToInt(depth / cell) + 1;
            int[,] grid = new int[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = -1;
                }
            }

            List<Vector2> points = new List<Vector2>();
            List<int> active = new List<int>();
            Vector2 first = new Vector2((float)rng.NextDouble() * width, (float)rng.NextDouble() * depth);
            points.Add(first);
            active.Add(0);
            grid[Mathf.FloorToInt(first.y / cell), Mathf.FloorToInt(first.x / cell)] = 0;

            while (active.Count > 0 && points.Count < maxPoints)
            {
                int ai = rng.Next(active.Count);
                Vector2 p = points[active[ai]];
                bool found = false;
                for (int k = 0; k < 30; k++)
                {
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float radius = minDist * (1f + (float)rng.NextDouble());
                    Vector2 q = p + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (q.x < 0f || q.x >= width || q.y < 0f || q.y >= depth)
                    {
                        continue;
                    }
                    int qi = Mathf.FloorToInt(q.y / cell);
                    int qj = Mathf.FloorToInt(q.x / cell);
                    bool ok = true;
                    for (int di = -2; di <= 2 && ok; di++)
                    {
                        for (int dj = -2; dj <= 2; dj++)
                        {
                            int ni = qi + di;
                            int nj = qj + dj;
                            if (ni < 0 || ni >= rows || nj < 0 || nj >= cols || grid[ni, nj] == -1)
                            {
                                continue;
                            }
                            if (Vector2.Distance(points[grid[ni, nj]], q) < minDist)
                            {
                                ok = false;
                                break;
                            }
                        }
                    }
                    if (ok)
                    {
                        points.Add(q);
                        active.Add(points.Count - 1);
                        grid[qi, qj] = points.Count - 1;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    active.RemoveAt(ai);
                }
            }
            return points;
        }

        /// <summary>判断某位置是否与列表中的任一对象体积重叠（距离 &lt; 半径和）。</summary>
        public static bool OverlapAny(List<Vector2> positions, List<float> radii, float x, float z, float r)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                float dx = x - positions[i].x;
                float dz = z - positions[i].y;
                float min = r + radii[i];
                if (dx * dx + dz * dz < min * min)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>实例水平占地半径：优先碰撞体，无碰撞体时回退 renderer 包围盒。</summary>
        public static float GetOccupancyRadius(GameObject instance, out bool hasCollider)
        {
            hasCollider = false;
            float maxHalf = 0f;
            Collider[] colliders = instance.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                if (c == null)
                {
                    continue;
                }
                hasCollider = true;
                Bounds b = c.bounds;
                maxHalf = Mathf.Max(maxHalf, Mathf.Max(b.size.x, b.size.z) * 0.5f);
            }
            if (hasCollider)
            {
                return maxHalf > 0f ? maxHalf : 0.5f;
            }

            Renderer[] renderers = null;
            LODGroup lodGroup = instance.GetComponentInChildren<LODGroup>();
            if (lodGroup != null && lodGroup.GetLODs().Length > 0)
            {
                renderers = lodGroup.GetLODs()[0].renderers;
            }
            if (renderers == null || renderers.Length == 0)
            {
                renderers = instance.GetComponentsInChildren<Renderer>();
            }
            if (renderers != null)
            {
                foreach (Renderer r in renderers)
                {
                    if (r == null)
                    {
                        continue;
                    }
                    maxHalf = Mathf.Max(maxHalf, Mathf.Max(r.bounds.size.x, r.bounds.size.z) * 0.5f);
                }
            }
            return maxHalf > 0f ? maxHalf : 0.5f;
        }

        /// <summary>按权重随机选一个岩石预设；无有效种类时返回 null。</summary>
        public static RockPreset PickWeightedRock(IList<RockVariety> varieties, System.Random rng)
        {
            float total = 0f;
            for (int i = 0; i < varieties.Count; i++)
            {
                RockVariety v = varieties[i];
                if (v != null && v.Preset != null && v.Weight > 0f)
                {
                    total += v.Weight;
                }
            }
            if (total <= 0f)
            {
                return null;
            }
            float roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < varieties.Count; i++)
            {
                RockVariety v = varieties[i];
                if (v == null || v.Preset == null || v.Weight <= 0f)
                {
                    continue;
                }
                roll -= v.Weight;
                if (roll <= 0f)
                {
                    return v.Preset;
                }
            }
            for (int i = 0; i < varieties.Count; i++)
            {
                RockVariety v = varieties[i];
                if (v != null && v.Preset != null)
                {
                    return v.Preset;
                }
            }
            return null;
        }

        /// <summary>创建临时散布材质（URP Lit；透明度按 opacity 设置透明混合）。</summary>
        public static Material CreateScatterMaterial(Color color, float opacity, float smoothness, float metallic)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            Material mat = new Material(shader);
            mat.color = new Color(color.r, color.g, color.b, opacity);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (opacity < 0.999f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            }
            return mat;
        }

        /// <summary>优先用资产材质；为空则按内置颜色/透明度/平滑度/金属度生成临时材质。</summary>
        public static Material ResolveMaterial(Material asset, Color color, float opacity, float smoothness, float metallic)
        {
            if (asset != null)
            {
                return asset;
            }
            return CreateScatterMaterial(color, opacity, smoothness, metallic);
        }
    }
}
