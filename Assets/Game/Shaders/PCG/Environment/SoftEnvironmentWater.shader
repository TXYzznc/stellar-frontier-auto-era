Shader "AutoEra/Environment/Soft Shore Water"
{
    Properties
    {
        _BaseColor("浅水颜色", Color) = (.25,.66,.64,1)
        _DeepColor("深水颜色", Color) = (.065,.32,.40,1)
        _FoamColor("岸边柔光颜色", Color) = (.79,.90,.79,1)
        _TerrainHeight("烘焙地形高度（线性）", 2D) = "black" {}
        _TerrainRect("区块原点与水平尺寸", Vector) = (0,0,96,96)
        _HeightScale("地形垂直编码范围", Float) = 96
        _TerrainBaseY("地形基准高度", Float) = 0
        _ConstructionRegion("建设区矩形范围",Vector)=(0,0,0,0)
        _ConstructionBlend("建设区过渡宽度",Float)=24
        _WaterLevel("水面高度", Float) = 5.5
        _Motion("水面流动强度（0 为静止）", Range(0,1)) = .5
        _FoamIntensity("岸边高光强度", Range(0,1)) = .5
        _UseFeature("启用局部液体特征", Float) = 0
        _Feature("特征中心、半径和长宽比", Vector) = (0,0,100,1)
        _FeaturePhase("边界扰动相位", Float) = 0
        _FeatureIdentity("液体特征标识",Vector)=(0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            Name "SoftWater"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "PCGSharedWind.hlsl"
            #include "PCGSurfaceResponse.hlsl"
            TEXTURE2D(_TerrainHeight); SAMPLER(sampler_TerrainHeight);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _DeepColor, _FoamColor, _TerrainRect;
                float4 _TerrainHeight_TexelSize;
                float _HeightScale, _WaterLevel, _Motion, _FoamIntensity, _UseFeature, _FeaturePhase, _TerrainBaseY;
                float4 _Feature;
                float4 _FeatureIdentity;
                float4 _ConstructionRegion;
                float _ConstructionBlend;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; half fog:TEXCOORD1; };
            V vert(A a)
            {
                V o; o.world=TransformObjectToWorld(a.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world);
                o.fog=ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 frag(V i):SV_Target
            {
                float2 absolute=AEWorldXZ(i.world);
                if(_ConstructionRegion.z>0)
                {
                    float2 delta=max(max(_ConstructionRegion.xy-absolute,absolute-_ConstructionRegion.xy-_ConstructionRegion.zw),0);
                    // 与建设区矩形合同一致，使用倒角距离，避免 length(delta) 产生近圆形安全带。
                    float chamfer=max(delta.x,delta.y)+min(delta.x,delta.y)*.35;
                    clip(chamfer-_ConstructionBlend);
                }
                if(_UseFeature>.5)
                {
                    float2 delta=(absolute-_Feature.xy)/float2(1,_Feature.w);
                    float angle=atan2(delta.y,delta.x);
                    float shape=1+.09*sin(angle*3+_FeaturePhase)+.06*sin(angle*5-_FeaturePhase);
                    clip(_Feature.z*shape-length(delta));
                }
                float2 uv=(absolute-_TerrainRect.xy)/_TerrainRect.zw;
                // Height samples include both chunk endpoints; map them to texel centers.
                uv=saturate(uv)*(1-_TerrainHeight_TexelSize.xy)+.5*_TerrainHeight_TexelSize.xy;
                float ground=SAMPLE_TEXTURE2D(_TerrainHeight,sampler_TerrainHeight,uv).r*_HeightScale+_TerrainBaseY;
                float depth=_WaterLevel-ground;
                clip(depth-.025);
                float t=_Time.y*_Motion;
                float phase=absolute.x*.53+absolute.y*.37;
                float wave=sin(phase-t*.65)+sin(absolute.y*.81-absolute.x*.24+t*.47)*.42;
                half3 n=normalize(half3(cos(phase-t*.65)*.035,1,cos(absolute.y*.81+t*.47)*.025));
                if(_AEWindActive>.5)
                {
                    float2 wind=AEWind(absolute);
                    float speed=length(wind);
                    float2 direction=speed>.001?wind/speed:float2(1,0);
                    t=_AEWindTime*_Motion;
                    phase=dot(absolute,direction)*.53;
                    float crossPhase=dot(absolute,float2(-direction.y,direction.x))*.81;
                    wave=(sin(phase-t*(.4+speed*.22))+sin(crossPhase+t*.47)*.42)*saturate(speed*.5);
                    n=normalize(half3(direction.x*cos(phase-t*(.4+speed*.22))*.02*speed,1,direction.y*cos(phase-t*(.4+speed*.22))*.02*speed));
                }
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                half3 view=GetWorldSpaceNormalizeViewDir(i.world);
                half spec=pow(saturate(dot(n,normalize(sun.direction+view))),96)*.20;
                half deep=1-exp(-depth*.55);
                float contactWave=AELiquidRipple(absolute,_FeatureIdentity.xy);
                half3 col=lerp(_BaseColor.rgb,_DeepColor.rgb,deep);
                float shore=1-smoothstep(.10,.56+sin(phase*.6-t*.4)*.06,depth);
                float ripple=pow(saturate(wave*.45+.25),6)*.095;
                col+=ripple+spec;
                col+=contactWave*.045;
                col=lerp(col,_FoamColor.rgb,shore*_FoamIntensity);
                col*=lerp(.77,1,sun.shadowAttenuation);
                return half4(MixFog(col,i.fog),lerp(.48,.96,saturate(depth*1.2)));
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
