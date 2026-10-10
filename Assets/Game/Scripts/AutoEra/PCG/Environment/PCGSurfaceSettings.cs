using UnityEngine;
namespace AutoEra.Art.PCG
{
    [CreateAssetMenu(menuName="自动纪元/程序化环境/地表响应配置")]
    public sealed class PCGSurfaceSettings : ScriptableObject
    {
        [InspectorName("配置版本")] public int Version=1;
        [InspectorName("接触历史容量")] public int HistoryCapacity=65536;
        [InspectorName("最大接触源数")] public int MaxSources=32;
        [InspectorName("每次更新最大采样数")] public int MaxSamplesPerTick=256;
        [InspectorName("接触纹理分辨率")] public int TextureResolution=512;
        [InspectorName("接触显示跨度")] public float DisplaySpan=128;
        [InspectorName("接触更新间隔")] public float UpdateInterval=.10f;
        [InspectorName("接地高度容差")] public float ContactTolerance=.65f;
        [InspectorName("瞬移判定距离")] public float TeleportDistance=12;
        [InspectorName("地表覆盖材质")] public Material SurfaceMaterial;
        [InspectorName("接触粒子材质")] public Material ParticleMaterial;
        [InspectorName("积雪覆盖网格")] public Mesh[] SnowCaps;
        [InspectorName("落叶网格")] public Mesh LeafMesh;
        [InspectorName("落叶材质")] public Material LeafMaterial;
        [InspectorName("雪地痕迹保留秒数")] public float SnowLife=90;
        [InspectorName("沙地痕迹保留秒数")] public float SandLife=35;
        [InspectorName("泥地痕迹保留秒数")] public float MudLife=65;
        [InspectorName("硬地痕迹保留秒数")] public float HardLife=12;
        [InspectorName("启用接触效果")] public bool ContactEffects=true;
        public void Validate()
        {
            HistoryCapacity=Mathf.Clamp(HistoryCapacity,128,131072);MaxSources=Mathf.Clamp(MaxSources,1,32);
            TextureResolution=Mathf.Clamp(Mathf.ClosestPowerOfTwo(TextureResolution),128,1024);
            DisplaySpan=Mathf.Clamp(DisplaySpan,64,256);UpdateInterval=Mathf.Clamp(UpdateInterval,.05f,.5f);
            MaxSamplesPerTick=Mathf.Clamp(MaxSamplesPerTick,16,1024);
        }
    }
}
