Shader "AutoEra/Environment/Soft Rock"
{
    Properties
    {
        _BaseColor("岩石色调", Color) = (.51,.60,.62,1)
        _TopColor("苔藓色调", Color) = (.53,.64,.48,1)
        _ShadowTint("冷色阴影", Color) = (.48,.59,.67,1)
        _Variation("大范围颜色变化", Range(0,1)) = .12
        _Moss("顶部苔藓比例", Range(0,1)) = .28
        _DetailStrength("层理与风化强度",Range(0,1))=0
        [HideInInspector] _BaseMap("Base", 2D) = "white" {}
        [HideInInspector] _Cutoff("Cutoff", Float) = .5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "PCGSharedWind.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _TopColor, _ShadowTint, _BaseMap_ST;
                float _Variation, _Moss, _Cutoff, _DetailStrength;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; half3 normal:TEXCOORD1; half fog:TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
            V vert(A a)
            {
                UNITY_SETUP_INSTANCE_ID(a);
                V o; UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.world=TransformObjectToWorld(a.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world);
                o.normal=TransformObjectToWorldNormal(a.normalOS);
                o.fog=ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 frag(V i):SV_Target
            {
                half3 n=normalize(i.normal);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                half diffuse=smoothstep(-.25,.85,dot(n,sun.direction));
                half shade=diffuse*lerp(.48,1,sun.shadowAttenuation);
                float2 world=AEWorldXZ(i.world);
                half variation=sin(world.x*.67+world.y*.31)*sin(i.world.y*.82-world.y*.42);
                half moss=smoothstep(.35,.90,n.y)*_Moss;
                half3 base=lerp(_BaseColor.rgb,_TopColor.rgb,moss)*(1+variation*_Variation);
                base*=1+sin(i.world.y*2.4+sin(world.x*.15)*.8+sin(world.y*.17)*.6)*.07*_DetailStrength;
                half3 col=base*lerp(_ShadowTint.rgb,half3(1.06,1.04,.98),shade);
                col*=lerp(half3(1,1,1),sun.color,.25);
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
