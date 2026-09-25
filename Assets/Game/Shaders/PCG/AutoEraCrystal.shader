// AutoEra 晶簇（ArtResource）：梦幻半透明水晶，URP PBR。
// 效果：Fresnel 外表面边缘光 + 沿外表面流动的发光带 + 稀疏闪烁光点 + 自发光。
// 无需贴图，全部程序化（世界空间噪声）。
Shader "AutoEra/Crystal"
{
    Properties
    {
        _BaseColor ("晶体基色（含透明度）", Color) = (0.55, 0.60, 0.95, 0.55)
        _RimColor ("边缘光色", Color) = (0.75, 0.85, 1.0, 1)
        _RimIntensity ("边缘光强度", Range(0, 4)) = 1.8
        _RimPower ("边缘光聚拢", Range(0.5, 8)) = 3.0
        _FlowColor ("流光色", Color) = (0.6, 0.9, 1.0, 1)
        _FlowIntensity ("流光强度", Range(0, 4)) = 1.5
        _FlowScale ("流光尺度", Float) = 1.6
        _FlowSpeed ("流光速度", Float) = 0.35
        _SparkleColor ("闪光色", Color) = (1.0, 1.0, 1.0, 1)
        _SparkleIntensity ("闪光强度", Range(0, 6)) = 2.5
        _SparkleScale ("闪光尺度", Float) = 6.0
        _SparkleSharpness ("闪光锐度", Range(2, 80)) = 24.0
        _SparkleSpeed ("闪光闪烁速度", Float) = 0.8
        _Smoothness ("平滑度", Range(0, 1)) = 0.9
        _Metallic ("金属度", Range(0, 1)) = 0.05
        _SelfGlow ("自发光", Range(0, 2)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "PCGNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimIntensity;
                float _RimPower;
                float4 _FlowColor;
                float _FlowIntensity;
                float _FlowScale;
                float _FlowSpeed;
                float4 _SparkleColor;
                float _SparkleIntensity;
                float _SparkleScale;
                float _SparkleSharpness;
                float _SparkleSpeed;
                float _Smoothness;
                float _Metallic;
                float _SelfGlow;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 positionWS = IN.positionWS;
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(GetWorldSpaceNormalizeViewDir(positionWS));

                float ndv = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - ndv, _RimPower);

                // 沿外表面的流光：噪声场随时间平移，乘 Fresnel 让流光集中在边缘/表面
                float3 flowPos = positionWS * _FlowScale - float3(0.0, _Time.y * _FlowSpeed, 0.0);
                float flow = pcg_fbm3(flowPos);
                float flowGlow = pow(saturate(flow * 1.5 - 0.25), 2.0);

                // 稀疏闪烁光点（锐利阈值 + 时间闪烁）
                float sparkle = pcg_noise3(positionWS * _SparkleScale + _Time.y * _SparkleSpeed);
                float spark = pow(saturate(sparkle), _SparkleSharpness);

                float3 emission =
                    _RimColor.rgb * fresnel * _RimIntensity
                    + _FlowColor.rgb * flowGlow * (0.3 + fresnel) * _FlowIntensity
                    + _SparkleColor.rgb * spark * _SparkleIntensity
                    + _BaseColor.rgb * _SelfGlow;

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.smoothness = _Smoothness;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = emission;
                surfaceData.alpha = _BaseColor.a;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
