// PCG 程序化材质共享噪声库（ArtResource）：hash / value noise / fbm / 三平面投影。
// 所有 PCG mesh 无 UV，纹理统一用「世界空间三平面投影」生成，避免贴图映射依赖。
#ifndef PCG_NOISE_INCLUDED
#define PCG_NOISE_INCLUDED

// ---- 2D hash / noise ----
float pcg_hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float pcg_noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = pcg_hash12(i);
    float b = pcg_hash12(i + float2(1.0, 0.0));
    float c = pcg_hash12(i + float2(0.0, 1.0));
    float d = pcg_hash12(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float pcg_fbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 5; i++)
    {
        v += a * pcg_noise(p);
        p = p * 2.03 + 19.19;
        a *= 0.5;
    }
    return v;
}

// ---- 3D hash / noise / fbm（用于闪光、体积纹理，无投影接缝）----
float pcg_hash13(float3 p3)
{
    p3 = frac(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return frac((p3.x + p3.y) * p3.z);
}

float pcg_noise3(float3 p)
{
    float3 i = floor(p);
    float3 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float n000 = pcg_hash13(i);
    float n100 = pcg_hash13(i + float3(1.0, 0.0, 0.0));
    float n010 = pcg_hash13(i + float3(0.0, 1.0, 0.0));
    float n110 = pcg_hash13(i + float3(1.0, 1.0, 0.0));
    float n001 = pcg_hash13(i + float3(0.0, 0.0, 1.0));
    float n101 = pcg_hash13(i + float3(1.0, 0.0, 1.0));
    float n011 = pcg_hash13(i + float3(0.0, 1.0, 1.0));
    float n111 = pcg_hash13(i + float3(1.0, 1.0, 1.0));
    float nx00 = lerp(n000, n100, f.x);
    float nx10 = lerp(n010, n110, f.x);
    float nx01 = lerp(n001, n101, f.x);
    float nx11 = lerp(n011, n111, f.x);
    float nxy0 = lerp(nx00, nx10, f.y);
    float nxy1 = lerp(nx01, nx11, f.y);
    return lerp(nxy0, nxy1, f.z);
}

float pcg_fbm3(float3 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 4; i++)
    {
        v += a * pcg_noise3(p);
        p = p * 2.03 + 19.19;
        a *= 0.5;
    }
    return v;
}

// ---- 三平面投影（世界空间）：无 UV 网格的通用程序化纹理 ----
float pcg_triplanar(float3 p, float3 n, float scale)
{
    float3 w = abs(n);
    float wsum = w.x + w.y + w.z;
    w = w / max(wsum, 0.0001);
    float nx = pcg_fbm(p.yz * scale);
    float ny = pcg_fbm(p.xz * scale);
    float nz = pcg_fbm(p.xy * scale);
    return nx * w.x + ny * w.y + nz * w.z;
}

#endif
