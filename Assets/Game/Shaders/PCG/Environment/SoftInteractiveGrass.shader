Shader "AutoEra/Environment/Interactive Soft Grass"
{
    Properties
    {
        _BaseColor("草地基底颜色",Color)=(.37,.60,.27,1)
        _TipColor("受光草尖颜色",Color)=(.62,.76,.39,1)
        _PressureDarken("压草变暗强度",Range(0,.65))=.35
        _WindStrength("随风弯曲强度",Range(0,2))=1
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "PCGSharedWind.hlsl"
        TEXTURE2D(_AEGrassTrailMap);SAMPLER(sampler_AEGrassTrailMap);
        float4 _AEGrassTrailRect;
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor,_TipColor;
            float _PressureDarken,_WindStrength;
        CBUFFER_END
        struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
        struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half3 normal:TEXCOORD1;half3 shape:TEXCOORD2;half fog:TEXCOORD3;};
        V vert(A a)
        {
            UNITY_SETUP_INSTANCE_ID(a);
            V o;
            float3 root=TransformObjectToWorld(float3(0,0,0));
            float2 world=AEWorldXZ(root);
            float2 uv=(world-_AEGrassTrailRect.xy)/max(float2(1,1),_AEGrassTrailRect.zw);
            float3 pressure=SAMPLE_TEXTURE2D_LOD(_AEGrassTrailMap,sampler_AEGrassTrailMap,uv,0).rgb;
            pressure*=step(0,uv.x)*step(0,uv.y)*step(uv.x,1)*step(uv.y,1);
            float weight=a.uv.y*a.uv.y;
            float2 wind=AEWind(world);
            float phase=world.x*.13+world.y*.09;
            float oscillation=.65+.35*sin(_AEWindTime*1.3-phase);
            o.world=TransformObjectToWorld(a.positionOS.xyz);
            o.world.xz+=(wind*.075*oscillation*_WindStrength+pressure.xy*pressure.z*.85)*weight;
            float height=max(.01,o.world.y-root.y);
            o.world.y-=height*pressure.z*.78;
            o.positionCS=TransformWorldToHClip(o.world);
            o.normal=TransformObjectToWorldNormal(a.normalOS);
            o.shape=half3(a.uv.y,pressure.z,.5+.5*sin(world.x*1.37+world.y*.91));
            o.fog=ComputeFogFactor(o.positionCS.z);
            return o;
        }
        ENDHLSL
        Pass
        {
            Name "SoftGrass" Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            half4 frag(V i):SV_Target
            {
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                half light=.74+.26*saturate(dot(normalize(i.normal),sun.direction)*.5+.5);
                half3 color=lerp(_BaseColor.rgb,_TipColor.rgb,i.shape.x*.75);
                color*=.94+i.shape.z*.12;
                color*=1-i.shape.y*_PressureDarken;
                color*=light*lerp(.72,1,sun.shadowAttenuation);
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster" Tags{"LightMode"="ShadowCaster"}
            ZWrite On ColorMask 0
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex shadowVert
            #pragma fragment empty
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection,_LightPosition;
            V shadowVert(A a)
            {
                V o=vert(a);float3 d=_LightDirection;
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    d=normalize(_LightPosition-o.world);
                #endif
                o.positionCS=TransformWorldToHClip(ApplyShadowBias(o.world,normalize(o.normal),d));
                #if UNITY_REVERSED_Z
                    o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
                #else
                    o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
                #endif
                return o;
            }
            half4 empty(V i):SV_Target{return 0;}
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly" Tags{"LightMode"="DepthOnly"}
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment empty
            #pragma multi_compile_instancing
            half4 empty(V i):SV_Target{return 0;}
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals" Tags{"LightMode"="DepthNormals"}
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment normals
            #pragma multi_compile_instancing
            half4 normals(V i):SV_Target{return half4(normalize(i.normal),0);}
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
