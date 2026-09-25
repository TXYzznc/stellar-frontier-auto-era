using UnityEngine;

namespace AutoEra.PCG
{
    /// <summary>
    /// 地表采样抽象：让散布 / 贴地逻辑与具体地表形态（Unity Terrain / 平面 / 其它地面）解耦。
    /// 运行时散布与矿石落位都通过本接口采样地表高度与法线，不直接依赖 Terrain。
    /// </summary>
    public interface IGroundSampler
    {
        /// <summary>采样 worldPosition 处的地表高度（世界 y 坐标）。</summary>
        float SampleHeight(Vector3 worldPosition);

        /// <summary>采样 worldPosition 处的地表法线（近似即可，用于对齐对象朝向）。</summary>
        Vector3 SampleNormal(Vector3 worldPosition);
    }
}
