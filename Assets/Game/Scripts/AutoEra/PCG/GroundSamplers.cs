using UnityEngine;

namespace AutoEra.PCG
{
    /// <summary>平面地表采样：y = 固定高度，法线恒向上。用于「初始区域」这类平地。</summary>
    public sealed class PlaneGroundSampler : IGroundSampler
    {
        private readonly float _height;

        public PlaneGroundSampler(float height = 0f)
        {
            _height = height;
        }

        public float SampleHeight(Vector3 worldPosition) => _height;

        public Vector3 SampleNormal(Vector3 worldPosition) => Vector3.up;
    }

    /// <summary>Unity Terrain 地表采样：直接采样 Terrain 高度场与法线。未来接入山地地形时使用。</summary>
    public sealed class TerrainGroundSampler : IGroundSampler
    {
        private readonly Terrain _terrain;

        public TerrainGroundSampler(Terrain terrain)
        {
            _terrain = terrain;
        }

        public float SampleHeight(Vector3 worldPosition)
        {
            return _terrain != null ? _terrain.SampleHeight(worldPosition) : 0f;
        }

        public Vector3 SampleNormal(Vector3 worldPosition)
        {
            if (_terrain == null)
            {
                return Vector3.up;
            }

            // 用相邻采样差分近似坡度法线（与 Editor 面板 GetTerrainNormal 同思路）
            const float eps = 1f;
            float hL = _terrain.SampleHeight(worldPosition + Vector3.left * eps);
            float hR = _terrain.SampleHeight(worldPosition + Vector3.right * eps);
            float hD = _terrain.SampleHeight(worldPosition + Vector3.back * eps);
            float hU = _terrain.SampleHeight(worldPosition + Vector3.forward * eps);
            return Vector3.Normalize(new Vector3(hL - hR, 2f * eps, hD - hU));
        }
    }
}
