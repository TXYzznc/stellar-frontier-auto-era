// PCG 极光特效（ArtResource 探索）：简单带状发光 shader，噪声驱动流动，
// 叠加在天空上。第一版验证「极光特效层」做法。
Shader "PCG/Aurora"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.15, 0.85, 0.55, 1)
        _SecondaryColor ("Secondary Color", Color) = (0.55, 0.25, 0.90, 1)
        _Intensity ("Intensity", Range(0, 3)) = 1.4
        _Speed ("Speed", Float) = 0.3
        _Scale ("Scale", Float) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Aurora"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _SecondaryColor;
                float _Intensity;
                float _Speed;
                float _Scale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x),
                            lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * noise(p);
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y * _Speed;

                // 水平极光带（uv.y 越靠中间越亮，上下渐隐）
                float band = smoothstep(0.0, 0.35, uv.y) * smoothstep(1.0, 0.65, uv.y);
                // 左右边缘渐隐（让极光带两端柔和，避免矩形边缘暴露）
                float edge = smoothstep(0.0, 0.22, uv.x) * smoothstep(1.0, 0.78, uv.x);

                // 噪声流动
                float2 np = float2(uv.x * _Scale + t, uv.y * _Scale * 0.6);
                float n = fbm(np);

                // 颜色渐变 + 强度
                float3 col = lerp(_BaseColor.rgb, _SecondaryColor.rgb, n);
                float intensity = band * edge * n * _Intensity;

                return half4(col * intensity, saturate(intensity));
            }
            ENDHLSL
        }
    }
}
