// AutoEra 矿物（ArtResource）：一个 shader 覆盖多种现实矿物，URP PBR。
// 类型（_MineralType）：0云母（珍珠彩虹/层状） 1孔雀石（同心环带） 2石英柱（半透明晶体+边缘光）
//                        3黄铁矿（金色金属） 4铁矿石（暗红褐+氧化锈斑）
// 全部程序化（世界空间三平面噪声），无贴图依赖，flat-shaded 棱面感。
Shader "AutoEra/Mineral"
{
    Properties
    {
        [Enum(Mica,0,Malachite,1,Quartz,2,Pyrite,3,IronOre,4)] _MineralType ("矿物类型", Float) = 0
        _ColorA ("主色", Color) = (0.6, 0.62, 0.66, 1)
        _ColorB ("次色/带色", Color) = (0.35, 0.55, 0.35, 1)
        _AccentColor ("强调色（锈斑/深色）", Color) = (0.45, 0.22, 0.15, 1)
        _NoiseScale ("纹理尺度", Float) = 3.0
        _BandFrequency ("环带频率（孔雀石）", Float) = 6.0
        _BandSharpness ("环带锐度", Range(0.3, 4)) = 1.0
        _Iridescence ("彩虹强度（云母）", Range(0, 2)) = 0.8
        _Smoothness ("平滑度", Range(0, 1)) = 0.5
        _Metallic ("金属度", Range(0, 1)) = 0.0
        _Opacity ("透明度（云母/石英）", Range(0, 1)) = 1.0
        _RimIntensity ("边缘光强度（石英）", Range(0, 4)) = 1.2
        [Enum(Opaque,0,Transparent,1)] _Surface ("表面类型", Float) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull Back

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
                float _MineralType;
                float4 _ColorA;
                float4 _ColorB;
                float4 _AccentColor;
                float _NoiseScale;
                float _BandFrequency;
                float _BandSharpness;
                float _Iridescence;
                float _Smoothness;
                float _Metallic;
                float _Opacity;
                float _RimIntensity;
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
                float fresnel = pow(1.0 - ndv, 3.0);

                float n = pcg_triplanar(positionWS, normalWS, _NoiseScale);
                float nDetail = pcg_triplanar(positionWS, normalWS, _NoiseScale * 3.0);

                float3 albedo = _ColorA.rgb;
                float metallic = _Metallic;
                float smoothness = _Smoothness;
                float alpha = _Opacity;
                float3 emission = 0;

                if (_MineralType < 0.5) // 云母：层状 + 珍珠彩虹
                {
                    float layers = pcg_fbm(float2(positionWS.y * 2.5, positionWS.x * _NoiseScale + positionWS.z * 0.6));
                    float iridT = pcg_noise(float2(viewDirWS.x, viewDirWS.y) * 3.0) + fresnel + n * 0.5;
                    float3 rainbow = 0.5 + 0.5 * cos(6.28318 * (iridT + float3(0.0, 0.33, 0.67)));
                    albedo = lerp(_ColorA.rgb, _ColorB.rgb, layers);
                    albedo = lerp(albedo, rainbow, saturate(_Iridescence) * (0.25 + fresnel * 0.75));
                    metallic = 0.0;
                    smoothness = max(smoothness, 0.85);
                    alpha = _Opacity;
                }
                else if (_MineralType < 1.5) // 孔雀石：同心环带（绿系）
                {
                    float2 cp = positionWS.xz;
                    float r = length(cp * (_NoiseScale * 0.35)) + n * 2.2;
                    float band = sin(r * _BandFrequency) * 0.5 + 0.5;
                    band = pow(band, _BandSharpness);
                    albedo = lerp(_ColorB.rgb, _ColorA.rgb, band);
                    metallic = 0.0;
                    smoothness = max(smoothness, 0.7);
                    alpha = 1.0;
                }
                else if (_MineralType < 2.5) // 石英柱：半透明晶体 + 边缘光
                {
                    albedo = _ColorA.rgb;
                    metallic = 0.0;
                    smoothness = max(smoothness, 0.85);
                    alpha = _Opacity;
                    emission = _ColorB.rgb * fresnel * _RimIntensity;
                }
                else if (_MineralType < 3.5) // 黄铁矿：金色金属，块状晶体
                {
                    float blocks = step(0.5, pcg_fbm(positionWS.xz * (_NoiseScale * 0.4) + floor(positionWS.y * _NoiseScale) * 0.71));
                    albedo = lerp(_ColorB.rgb, _ColorA.rgb, blocks);
                    metallic = max(metallic, 0.85);
                    smoothness = max(smoothness, 0.55);
                    alpha = 1.0;
                }
                else // 铁矿石：暗红褐 + 氧化锈斑
                {
                    albedo = lerp(_ColorA.rgb, _ColorB.rgb, n);
                    float rust = smoothstep(0.6, 0.88, nDetail);
                    albedo = lerp(albedo, _AccentColor.rgb, rust);
                    metallic = min(metallic, 0.15);
                    smoothness = min(smoothness, 0.4);
                    alpha = 1.0;
                }

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.smoothness = smoothness;
                surfaceData.occlusion = 1.0;
                surfaceData.emission = emission;
                surfaceData.alpha = alpha;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
