// AutoEra 岩石（ArtResource）：程序化纹理 + 磨砂质感的 URP PBR。
// 无贴图依赖：世界空间三平面噪声生成纹理，噪声扰动法线做凹凸，粗糙度随噪声起伏。
Shader "AutoEra/Rock"
{
    Properties
    {
        _BaseColor ("基色", Color) = (0.46, 0.44, 0.41, 1)
        _SecondaryColor ("纹理次色", Color) = (0.34, 0.32, 0.30, 1)
        _NoiseScale ("纹理尺度", Float) = 2.5
        _ColorVariation ("颜色变化强度", Range(0, 1)) = 0.75
        _Roughness ("粗糙度（磨砂）", Range(0, 1)) = 0.82
        _RoughnessVariation ("粗糙度起伏", Range(0, 1)) = 0.35
        _Metallic ("金属度", Range(0, 1)) = 0
        _NormalStrength ("凹凸强度", Range(0, 2)) = 0.7
        _MossAmount ("杂色/苔藓量", Range(0, 1)) = 0.15
        _MossColor ("杂色/苔藓色", Color) = (0.26, 0.40, 0.22, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On

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
                float4 _SecondaryColor;
                float _NoiseScale;
                float _ColorVariation;
                float _Roughness;
                float _RoughnessVariation;
                float _Metallic;
                float _NormalStrength;
                float _MossAmount;
                float4 _MossColor;
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

                // 大尺度 + 细尺度的三平面噪声
                float n = pcg_triplanar(positionWS, normalWS, _NoiseScale);
                float nDetail = pcg_triplanar(positionWS, normalWS, _NoiseScale * 4.0);

                // 基色 ↔ 次色（纹理斑驳）
                float3 albedo = lerp(_BaseColor.rgb, _SecondaryColor.rgb, n * _ColorVariation);

                // 杂色/苔藓：细噪声高区呈现苔藓色
                float moss = smoothstep(0.55, 0.9, nDetail) * _MossAmount;
                albedo = lerp(albedo, _MossColor.rgb, moss);

                // 磨砂：粗糙度随细噪声起伏（值越高越磨砂）
                float roughness = saturate(_Roughness + (nDetail - 0.5) * _RoughnessVariation);
                float smoothness = 1.0 - roughness;

                // 凹凸：噪声梯度扰动法线
                float e = 0.04;
                float h0 = pcg_triplanar(positionWS, normalWS, _NoiseScale * 3.0);
                float hx = pcg_triplanar(positionWS + float3(e, 0, 0), normalWS, _NoiseScale * 3.0);
                float hy = pcg_triplanar(positionWS + float3(0, e, 0), normalWS, _NoiseScale * 3.0);
                float hz = pcg_triplanar(positionWS + float3(0, 0, e), normalWS, _NoiseScale * 3.0);
                float3 grad = float3(hx - h0, hy - h0, hz - h0) / e;
                float3 bump = normalize(normalWS - grad * (_NormalStrength * 0.08));

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = bump;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.smoothness = smoothness;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = 0;
                surfaceData.alpha = 1.0;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
