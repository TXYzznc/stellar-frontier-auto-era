// PCG 水体（ArtResource 探索）：程序化波浪 shader——噪声驱动流动 + 浅水深水渐变 + 波峰高光。
// 无需贴图，纯 HLSL 噪声生成水面纹理与波浪，URP 半透明。
Shader "PCG/Water"
{
    Properties
    {
        _BaseColor ("浅水色", Color) = (0.12, 0.45, 0.60, 0.75)
        _DeepColor ("深水色", Color) = (0.03, 0.18, 0.32, 0.92)
        _WaveScale ("波浪尺度", Float) = 6.0
        _WaveSpeed ("流动速度", Float) = 0.4
        _WaveStrength ("波浪强度", Range(0, 1)) = 0.45
        _Highlight ("波峰高光", Range(0, 2)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Water"
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
                float4 _DeepColor;
                float _WaveScale;
                float _WaveSpeed;
                float _WaveStrength;
                float _Highlight;
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
                float t = _Time.y * _WaveSpeed;

                // 两层不同方向/速度的流动，叠加出水面波纹
                float2 flowA = uv * _WaveScale + float2(t, t * 0.55);
                float2 flowB = uv * _WaveScale * 1.7 + float2(-t * 0.7, t * 0.3);
                float w = fbm(flowA);
                float w2 = fbm(flowB);
                float wave = w * 0.7 + w2 * 0.3;

                // 用波浪扰动 UV，模拟水面折射/纹理扭曲
                float2 disturb = float2(
                    fbm(flowA + float2(4.2, 1.3)) - 0.5,
                    fbm(flowB + float2(1.7, 5.8)) - 0.5) * _WaveStrength;
                float wd = fbm(uv * _WaveScale + disturb + float2(t, 0));

                // 浅水 ↔ 深水 渐变（按波浪噪声）
                float3 col = lerp(_BaseColor.rgb, _DeepColor.rgb, saturate(wd * 1.2));

                // 波峰高光（波浪值高处更亮）
                col += (wave - 0.5) * _Highlight * _BaseColor.rgb;

                float alpha = lerp(_BaseColor.a, _DeepColor.a, saturate(wd));

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
