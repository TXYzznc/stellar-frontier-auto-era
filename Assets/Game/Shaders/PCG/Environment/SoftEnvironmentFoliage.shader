Shader "AutoEra/Environment/Soft Foliage"
{
    Properties
    {
        _MainTex("植物来源图集", 2D)="white"{}
        _BaseColor("柔和色调", Color)=(.48,.68,.39,1)
        _Cutoff("叶片透明裁剪", Range(0,1))=.28
        _Leaf("叶片着色（0 为树皮）", Range(0,1))=1
        _PreservePetals("保留花瓣颜色", Range(0,1))=0
        _WindStrength("风力强度", Range(0,1.5))=.45
        _WindShape("根部高度、网格高度和预留值", Vector)=(0,1,0,0)
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
        Cull Off
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "PCGSharedWind.hlsl"
        TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _BaseColor;
            float _Cutoff, _Leaf, _WindStrength, _PreservePetals;
        CBUFFER_END
        UNITY_INSTANCING_BUFFER_START(Plant)
            UNITY_DEFINE_INSTANCED_PROP(float4, _WindShape)
        UNITY_INSTANCING_BUFFER_END(Plant)
        struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
        struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; half3 normal:TEXCOORD2; half fog:TEXCOORD3; };
        float3 WindPosition(A a)
        {
            float4 shape=UNITY_ACCESS_INSTANCED_PROP(Plant,_WindShape);
            float rootWeight=saturate((a.positionOS.y-shape.x)/max(.01,shape.y));
            float3 world=TransformObjectToWorld(a.positionOS.xyz);
            float3 origin=TransformObjectToWorld(float3(0,0,0));
            if(_AEWindActive>.5)
            {
                float2 absolute=AEWorldXZ(origin);
                float2 wind=AEWind(absolute);
                float oscillation=.65+.35*sin(_AEWindTime*.82-dot(absolute,float2(.11,.07)));
                world.xz+=wind*oscillation*rootWeight*rootWeight*_WindStrength*.12;
                return world;
            }
            float t=_Time.y;
            float phase=dot(origin.xz,float2(.11,.07));
            float gust=.65+.35*sin(t*.36-phase*.45);
            float sway=sin(t*.82-phase)*gust;
            float small=sin(t*1.63+world.x*.51+world.z*.34)*.08*_Leaf;
            world.xz+=float2(.82,.57)*(sway+small)*rootWeight*rootWeight*_WindStrength*.42;
            return world;
        }
        V vert(A a)
        {
            UNITY_SETUP_INSTANCE_ID(a);
            V o; o.world=WindPosition(a);
            o.positionCS=TransformWorldToHClip(o.world);
            o.normal=TransformObjectToWorldNormal(a.normalOS);
            o.uv=TRANSFORM_TEX(a.uv,_MainTex);
            o.fog=ComputeFogFactor(o.positionCS.z);
            return o;
        }
        half4 Atlas(V i)
        {
            half4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
            clip(tex.a-_Cutoff);
            return tex;
        }
        ENDHLSL
        Pass
        {
            Name "SoftFoliage"
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            half4 frag(V i):SV_Target
            {
                half4 tex=Atlas(i);
                half luminance=dot(tex.rgb,half3(.22,.67,.11));
                half3 base=lerp(tex.rgb*_BaseColor.rgb,_BaseColor.rgb*lerp(.7,1.2,luminance),_Leaf*.78);
                half petal=smoothstep(.01,.10,max(tex.r,tex.b)-tex.g)*_PreservePetals;
                base=lerp(base,tex.rgb*1.2,petal);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                half3 n=normalize(lerp(normalize(i.normal),half3(0,1,0),_Leaf*.45));
                half d=smoothstep(-.5,.85,dot(n,sun.direction));
                half illumination=(.55+d*.45)*lerp(.64,1,sun.shadowAttenuation);
                half variation=1+sin(i.world.x*.16+i.world.z*.23)*.05;
                return half4(MixFog(base*illumination*variation,i.fog),1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"="ShadowCaster"}
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 _LightDirection, _LightPosition;
            V shadowVert(A a)
            {
                V o=vert(a);
                float3 dir=_LightDirection;
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    dir=normalize(_LightPosition-o.world);
                #endif
                o.positionCS=TransformWorldToHClip(ApplyShadowBias(o.world,normalize(o.normal),dir));
                #if UNITY_REVERSED_Z
                    o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
                #else
                    o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
                #endif
                return o;
            }
            half4 shadowFrag(V i):SV_Target { Atlas(i); return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode"="DepthOnly"}
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment depthFrag
            #pragma multi_compile_instancing
            half4 depthFrag(V i):SV_Target { Atlas(i); return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode"="DepthNormals"}
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment normalFrag
            #pragma multi_compile_instancing
            half4 normalFrag(V i):SV_Target { Atlas(i); return half4(normalize(i.normal),0); }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
