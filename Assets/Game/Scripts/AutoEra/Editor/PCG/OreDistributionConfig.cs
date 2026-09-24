using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Editor.PCG
{

    /// <summary>
    /// 矿石概率配置（ArtResource PCG）：定义资源点「矿山」可出现的矿物种类及各矿物出现概率。
    /// 每个矿山资源点按 weight 加权随机选取一种矿物（暂定一个资源点只对应一种矿物），
    /// 用真正的晶簇/矿石簇对象（PCGMeshFactory 动态 mesh + 材质资产）替换占位方块。
    /// 创建：Assets/Create → AutoEra/PCG/矿石概率配置
    /// </summary>
    [CreateAssetMenu(fileName = "OreDistribution", menuName = "AutoEra/PCG/矿石概率配置")]
    public class OreDistributionConfig : ScriptableObject
    {
        [Serializable]
        public class OreEntry
        {
            [Tooltip("显示名（如 黄铁矿 / 蓝晶簇）")]
            public string DisplayName = "矿物";

            [Tooltip("true=晶簇（尖刺晶体），false=矿石簇（多矿块聚集）")]
            public bool IsCrystal = false;

            [Tooltip("矿石簇形态（IsCrystal=false 时有效）")]
            public OreType OreKind = OreType.Cubic;

            [Tooltip("材质资产（为空则用该矿物类型默认色临时材质）")]
            public Material Material;

            [Tooltip("出现概率权重（越大越常出现）")]
            public float Weight = 1f;

            [Tooltip("每个矿山该矿物的簇数（一个矿山对应 5~10 个矿石对象）")]
            public int ClusterCount = 7;

            [Header("占地大小（正方形，Unity 单位）")]
            [Tooltip("占地最小边长（米）。生成时在 min~max 间随机取值，按 mesh 实际尺寸反算缩放。")]
            public float FootprintMin = 0.5f;

            [Tooltip("占地最大边长（米）")]
            public float FootprintMax = 1f;

            /// <summary>在 FootprintMin~FootprintMax 间随机取一个占地边长（米）。</summary>
            public float RandomFootprint(System.Random rng)
            {
                float min = Mathf.Min(FootprintMin, FootprintMax);
                float max = Mathf.Max(FootprintMin, FootprintMax);
                if (max <= 0f)
                {
                    max = 1f;
                }
                return min + (float)rng.NextDouble() * (max - min);
            }
        }

        [Tooltip("矿物种类与概率列表")]
        public List<OreEntry> Entries = new List<OreEntry>();

        /// <summary>按 Weight 加权随机选取一种矿物；Entries 为空返回 null。</summary>
        public OreEntry PickRandom(System.Random rng)
        {
            if (Entries == null || Entries.Count == 0)
            {
                return null;
            }

            float total = 0f;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] != null)
                {
                    total += Mathf.Max(0f, Entries[i].Weight);
                }
            }
            if (total <= 0f)
            {
                return Entries[0];
            }

            float roll = (float)rng.NextDouble() * total;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i] == null)
                {
                    continue;
                }
                roll -= Mathf.Max(0f, Entries[i].Weight);
                if (roll <= 0f)
                {
                    return Entries[i];
                }
            }
            return Entries[Entries.Count - 1];
        }
    }
}
