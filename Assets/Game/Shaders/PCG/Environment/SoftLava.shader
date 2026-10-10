Shader "AutoEra/Environment/Soft Lava"
{
    Properties
    {
        _BaseColor("冷却玄武岩颜色",Color)=(.14,.08,.075,1)
        _HotColor("高温裂隙颜色",Color)=(1,.30,.035,1)
        _TerrainHeight("地形高度",2D)="black"{}
        _TerrainRect("区块原点与水平尺寸",Vector)=(0,0,64,64)
        _Feature("特征中心、半径和长宽比",Vector)=(0,0,100,1)
        _FeaturePhase("边界扰动相位",Float)=0
        _UseFeature("启用局部液体特征",Float)=1
        _WaterLevel("液体高度",Float)=10
        _HeightScale("地形垂直编码范围",Float)=96
        _TerrainBaseY("地形基准高度",Float)=0
        _ConstructionRegion("建设区矩形范围",Vector)=(0,0,0,0)
        _ConstructionBlend("建设区过渡宽度",Float)=24
        _Motion("流动强度",Range(0,1))=.4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "PCGSharedWind.hlsl"
            #include "PCGSurfaceResponse.hlsl"
            TEXTURE2D(_TerrainHeight); SAMPLER(sampler_TerrainHeight);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor,_HotColor,_TerrainRect,_Feature;
                float4 _FeatureIdentity;
                float4 _TerrainHeight_TexelSize;
                float _FeaturePhase,_UseFeature,_WaterLevel,_HeightScale,_Motion,_TerrainBaseY,_ConstructionBlend;
                float4 _ConstructionRegion;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;};
            struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half fog:TEXCOORD1;};
            V vert(A a)
            {
                V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);
                o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            float2 cellHash(float2 p)
            {
                return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
            }
            half4 frag(V i):SV_Target
            {
                float2 p=AEWorldXZ(i.world),d=(p-_Feature.xy)/float2(1,_Feature.w);
                if(_ConstructionRegion.z>0)
                {
                    float2 delta=max(max(_ConstructionRegion.xy-p,p-_ConstructionRegion.xy-_ConstructionRegion.zw),0);
                    // 与建设区矩形合同一致，使用倒角距离，避免 length(delta) 产生近圆形安全带。
                    float chamfer=max(delta.x,delta.y)+min(delta.x,delta.y)*.35;
                    clip(chamfer-_ConstructionBlend);
                }
                float angle=atan2(d.y,d.x),shape=1+.09*sin(angle*3+_FeaturePhase)+.06*sin(angle*5-_FeaturePhase);
                clip(_Feature.z*shape-length(d));
                float2 heightUV=(p-_TerrainRect.xy)/_TerrainRect.zw;
                heightUV=saturate(heightUV)*(1-_TerrainHeight_TexelSize.xy)+.5*_TerrainHeight_TexelSize.xy;
                float ground=SAMPLE_TEXTURE2D(_TerrainHeight,sampler_TerrainHeight,heightUV).r*_HeightScale+_TerrainBaseY;
                float depth=_WaterLevel-ground;clip(depth-.03);
                // Slow independent flow; wind never controls geothermal motion.
                float t=_Time.y*_Motion*.20;
                float2 q=p*.23+float2(sin(p.y*.07+t),cos(p.x*.08-t))*.45;
                float2 tile=floor(q),local=frac(q);float nearest=10,second=10;
                [unroll]for(int y=-1;y<=1;y++)[unroll]for(int x=-1;x<=1;x++)
                {
                    float2 offset=float2(x,y),seed=cellHash(tile+offset);
                    float2 cellDelta=offset+.5+.36*sin(seed*6.283+t*.35)-local;
                    float distance=dot(cellDelta,cellDelta);
                    if(distance<nearest){second=nearest;nearest=distance;}
                    else second=min(second,distance);
                }
                float cracks=1-smoothstep(.018,.11,second-nearest);
                float cell=smoothstep(.1,.7,nearest);
                float glow=smoothstep(.25,.65,depth)*(cell*.16+cracks*.64+.12);
                half3 col=lerp(_BaseColor.rgb,_HotColor.rgb,glow);
                col+=_HotColor.rgb*cracks*.28;
                col+=_HotColor.rgb*max(0,AELiquidRipple(p,_FeatureIdentity.xy))*.13;
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
