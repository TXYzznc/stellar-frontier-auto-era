Shader "AutoEra/Environment/Soft Surface Layer"
{
    Properties
    {
        _SnowColor("积雪颜色",Color)=(.89,.94,.97,1)
        _SandColor("沙地颜色",Color)=(.76,.62,.41,1)
        _MudColor("泥地颜色",Color)=(.30,.35,.23,1)
        _AshColor("火山灰颜色",Color)=(.24,.23,.27,1)
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+5"}
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "PCGSharedWind.hlsl"
            #include "PCGSurfaceResponse.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _SnowColor,_SandColor,_MudColor,_AshColor;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float4 cover:TEXCOORD2;float2 info:TEXCOORD3;float fog:TEXCOORD4;float surfaceY:TEXCOORD5;};
            V vert(A a)
            {
                UNITY_SETUP_INSTANCE_ID(a);
                V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.surfaceY=o.world.y;o.cover=a.color;o.info=a.uv;
                float2 p=AEWorldXZ(o.world);float4 shape=AESurfaceShape(p);
                if(abs(o.surfaceY-AESurfaceHeight(shape))>.20)shape=0;
                float broad=.5+.5*sin(p.x*.22+sin(p.y*.17));
                float thickness=a.color.r*(.20+broad*.15)+a.color.g*.18+a.color.b*.16;
                o.world.y+=thickness-clamp(shape.r,-.09,thickness*.90);
                o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);
                o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 frag(V i):SV_Target
            {
                clip(_AESurfaceActive-.5);
                float2 p=AEWorldXZ(i.world);float4 shape=AESurfaceShape(p),deposit=AESurfaceDeposit(p);
                float contactValid=step(abs(i.surfaceY-AESurfaceHeight(shape)),.20);
                if(contactValid<.5){shape=0;deposit=0;}
                float sum=dot(i.cover,1);clip(max(sum,max(deposit.a,shape.b))-.12);
                float4 w=i.cover/max(.001,sum);
                float3 base=_SnowColor.rgb*w.r+_SandColor.rgb*w.g+_MudColor.rgb*w.b+_AshColor.rgb*w.a;
                if(sum<.12)base=float3(.43,.47,.49);
                float ripple=sin(p.x*3+p.y*1.3+sin(p.y*.7))*.012*w.g;
                base*=1+ripple+sin(p.x*.4)*sin(p.y*.3)*.025;
                base=lerp(base,base*float3(.62,.74,.87),shape.g*w.r*.75);
                base*=1-shape.g*(w.b*.30+w.g*.13+w.a*.12);
                base=lerp(base,deposit.rgb,deposit.a);
                base*=1-shape.b*.20;
                float texel=_AESurfaceRect.z*_AESurfaceRect.w;
                float dx=(AESurfaceShape(p+float2(texel,0)).r-AESurfaceShape(p-float2(texel,0)).r)/(2*texel);
                float dz=(AESurfaceShape(p+float2(0,texel)).r-AESurfaceShape(p-float2(0,texel)).r)/(2*texel);
                float3 n=normalize(i.normal+float3(dx,0,dz)*1.8*contactValid);
                Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
                float diffuse=smoothstep(-.25,.85,dot(n,sun.direction));
                float3 col=base*lerp(float3(.57,.67,.77),float3(1.06,1.04,.98),diffuse*lerp(.55,1,sun.shadowAttenuation));
                float wet=max(shape.b,w.b*.23);
                float spec=pow(saturate(dot(n,normalize(sun.direction+GetWorldSpaceNormalizeViewDir(i.world)))),48)*wet*.24;
                // Seepage only near a valid local water surface; never on high dry ground.
                float seep=step(.5,i.info.x)*step(i.world.y,i.info.y+.13)*w.b*shape.g;
                col+=spec+seep*.025;
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
