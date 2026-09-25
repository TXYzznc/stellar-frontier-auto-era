// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DEC/Cutout Wind/Cutout Wind Translucency"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[Header(DEBUG SETTINGS)][Enum(Off,0,On,1)]_ZWriteMode("ZWrite Mode", Int) = 1
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)]_ColorMask("Color Mask Mode", Int) = 15
		[Enum(Off,0,On,1)]_AlphatoCoverage("Alpha to Coverage", Float) = 0
		[Header(SURFACE OPTIONS)][Enum(Front,2,Back,1,Both,0)]_RenderFace("Render Face", Int) = 2
		[EmissionFlags]_EmissionFlags("Global Illumination Emissive", Float) = 0
		[Toggle(_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF("Receive Shadows", Float) = 0
		[Toggle(_SPECULARHIGHLIGHTS_OFF)] _SPECULARHIGHLIGHTS_OFF("Receive Specular Highlights", Float) = 0
		[Toggle(_ENVIRONMENTREFLECTIONS_OFF)] _ENVIRONMENTREFLECTIONS_OFF("Receive Environment Reflections", Float) = 1
		[DE_DrawerFloatEnum(Default _Flip _Mirror)]_NormalMode("Normal Mode", Float) = 0
		[DE_DrawerToggleNoKeyword]_GlancingClipMode("Enable Clip Glancing Angle", Float) = 1
		[Header(MAIN MAPS)]_Color("Tint", Color) = (1,1,1,0)
		[DE_DrawerTextureSingleLine]_MainTex("Albedo Map", 2D) = "white" {}
		_Brightness("Brightness", Range( 0 , 2)) = 1
		_AlphaCutoffBias("Alpha Cutoff Bias", Range( 0 , 1)) = 0.5
		_AlphaCutoffBiasShadow("Alpha Cutoff Bias Shadow", Range( 0.01 , 1)) = 0.5
		_TilingX("Tiling X", Float) = 1
		_TilingY("Tiling Y", Float) = 1
		_OffsetX("Offset X", Float) = 0
		_OffsetY("Offset Y", Float) = 0
		[Normal][DE_DrawerTextureSingleLine]_BumpMap("Normal Map", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Float) = 1
		[DE_DrawerTextureSingleLine]_MetallicGlossMap("Metallic Map", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Range( 0 , 1)) = 0
		[DE_DrawerTextureSingleLine]_OcclusionMap("Occlusion Map", 2D) = "white" {}
		[DE_DrawerToggleNoKeyword]_OcclusionSource("Occlusion is Baked", Float) = 0
		_OcclusionStrengthAO("Occlusion Strength", Range( 0 , 1)) = 0
		[DE_DrawerTextureSingleLine]_SmoothnessMap("Smoothness Map", 2D) = "white" {}
		[DE_DrawerFloatEnum(Smoothness _Roughness _Geometric)]_SmoothnessSource("Smoothness Source", Float) = 0
		[DE_DrawerSliderRemap(_SmoothnessMin, _SmoothnessMax,0, 1)]_Smoothness("Smoothness", Vector) = (0,0,0,0)
		[HideInInspector]_SmoothnessMin("Smoothness Min", Range( 0 , 1)) = 0
		[HideInInspector]_SmoothnessMax("Smoothness Max", Range( 0 , 1)) = 0
		[Header(COLOR SHIFT)][DE_DrawerFloatEnum(Off _Object Space _World Space _Vertex Color _Vertex Normal)]_ColorShift_Mode("Mode", Float) = 0
		_ColorShift_ShiftVariation("Shift Variation", Range( 0 , 1)) = 0.2
		_ColorShift_ShiftVariationRGB("Shift Variation RGB", Range( -0.5 , 0.5)) = 0.2
		_ColorShift_ShiftInfluence("Shift Influence ", Range( 0 , 1)) = 0.75
		_ColorShift_ShiftSaturation("Shift Saturation", Range( 0 , 1)) = 0.85
		_ColorShift_NoiseScale("Noise Scale", Range( 0 , 2)) = 1
		[Header(COLOR SHIFT (World Space Mode))]_ColorShift_WorldSpaceDistance("Distance", Range( 0.01 , 5)) = 5
		_ColorShift_WorldSpaceOffset("Offset", Range( -1 , 1)) = 1
		_ColorShift_WorldSpaceNoiseShift("Noise Shift", Range( 1 , 5)) = 5
		[Header(COLOR SHIFT (Mask))][DE_DrawerFloatEnum(Off _Active _Active Inverted)]_ColorShift_MaskMode("Mode", Float) = 0
		[DE_DrawerTextureSingleLine]_ColorShift_MaskMap("Mask Map", 2D) = "black" {}
		_ColorShift_MaskFuzziness("Mask Fuzziness", Range( 0 , 1)) = 0.25
		[Header(TRANSLUCENCY)][DE_DrawerToggleNoKeyword]_EnableTranslucency("Enable Translucency", Float) = 0
		[DE_DrawerFloatEnum(Texture _Texture Inverted _Baked)]_TranslucencySourceFoward("Translucency Source", Float) = 0
		[HDR]_TranslucencyTint("Translucency Tint", Color) = (0.575,0.75,0,1)
		[HDR]_TransmissionTint("Transmission Tint", Color) = (0.3833333,0.5,0,1)
		[NoScaleOffset][DE_DrawerTextureSingleLine]_TranslucencyMap("Translucency Map", 2D) = "white" {}
		_TranslucencyThickness("Thickness", Range( 0 , 1.5)) = 1.466198
		_TranslucencyThicknessFeather("Thickness Feather", Range( 0 , 1)) = 1
		[Header(TRANSMISSION  URP)]_URP_TransmissionShadow("Transmission Shadow", Range( 0 , 1)) = 0.5
		[Header(TRANSLUCENCY URP)]_URP_TranslucencyStrength("Translucency Strength", Range( 1 , 50)) = 1
		_URP_TranslucencyNormalDistortion("Translucency Normal Distortion ", Range( 0 , 1)) = 0.2735869
		_URP_TranslucencyScattering("Translucency Scattering ", Range( 1 , 50)) = 2
		_URP_TranslucencyDirect("Translucency Direct ", Range( 0 , 1)) = 0.9
		_URP_TranslucencyAmbient("Translucency Ambient", Range( 0 , 1)) = 0.8339342
		_URP_TranslucencyShadow("Translucency Shadow ", Range( 0 , 1)) = 0.5
		[Header(WIND)][DE_DrawerFloatEnum(Off _Global _Local)]_WindMode("Enable Wind Mode", Float) = 0
		[DE_DrawerFloatEnum(Leaf _Palm _Grass _Simple _Ivy)]_WindType("Wind Type", Float) = 0
		[Header(WIND MODE GLOBAL)]_GlobalWindInfluenceOther("Main", Float) = 1
		_GlobalTurbulenceInfluence("Turbulence", Float) = 0.35
		[Header(WIND MODE LOCAL)]_LocalWindStrength("Main", Float) = 1
		_LocalWindTurbulence("Turbulence", Float) = 0.35
		_LocalWindPulse("Pulse Frequency", Float) = 0.1
		_LocalWindDirection("Direction", Range( 0 , 360)) = 0
		[ASEEnd]_LocalRandomWindOffset("Random Offset", Float) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry-10" "NatureRendererInstancing"="True" }
		Cull [_RenderFace]
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		AlphaToMask [_AlphatoCoverage]
		
		HLSLINCLUDE
		#pragma target 3.5

		#pragma prefer_hlslcc gles
		

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" "NatureRendererInstancing"="True" }
			
			Blend One Zero, One Zero
			ColorMask [_ColorMask]
			

			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1


			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON

			#pragma multi_compile _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _LIGHT_LAYERS
			
			#pragma multi_compile _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTERED_RENDERING

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_color : COLOR;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_ColorShift_MaskMap);
			TEXTURE2D(_BumpMap);
			TEXTURE2D(_MetallicGlossMap);
			TEXTURE2D(_SmoothnessMap);
			TEXTURE2D(_OcclusionMap);
			TEXTURE2D(_TranslucencyMap);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float ColorShift_Modefloatswitch168_g44339( float m_switch, float m_Off, float m_ObjectSpace, float m_WorldSpace, float m_VertexColor, float m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float(0);
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float4 ColorShift_Maskfloat4switch313_g44339( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float4(0,0,0,0);
			}
			
			float4 ColorShift_Modefloat4switch364_g44339( float m_switch, float4 m_Off, float4 m_ObjectSpace, float4 m_WorldSpace, float4 m_VertexColor, float4 m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float4(0,0,0,0);
			}
			
			float3 _NormalModefloat3switch( float m_switch, float3 m_Default, float3 m_Flip, float3 m_Mirror )
			{
				if(m_switch ==0)
					return m_Default;
				else if(m_switch ==1)
					return m_Flip;
				else if(m_switch ==2)
					return m_Mirror;
				else
				return float3(0,0,0);
			}
			
			float3 AdditionalLightsFlatMask( float3 WorldPosition, float4 ShadowMask )
			{
				float3 Color = 0;
				#ifdef _ADDITIONAL_LIGHTS
				int numLights = GetAdditionalLightsCount();
				for(int i = 0; i<numLights;i++)
				{
				#if ASE_SRP_VERSION >= 100000
					Light light = GetAdditionalLight(i, WorldPosition, ShadowMask);
				#else
					Light light = GetAdditionalLight(i, WorldPosition);
				#endif
					Color += light.color *(light.distanceAttenuation * light.shadowAttenuation);
					
				}
				#endif
				return Color;
			}
			
			float4 SmoothnessTypefloat4switch215_g44330( float m_switch, float4 m_Smoothness, float4 m_Roughness, float4 m_Geometric )
			{
				if(m_switch ==0)
					return m_Smoothness;
				else if(m_switch ==1)
					return m_Roughness;
				else if(m_switch ==2)
					return m_Geometric;
				else
				return float4(0,0,0,0);
			}
			
			float4 float4switch103_g44322( float m_switch, float4 m_Texture, float4 m_TextureInverted, float4 m_Baked )
			{
				if(m_switch ==0)
					return m_Texture;
				else if(m_switch ==1)
					return m_TextureInverted;
				else if(m_switch ==2)
					return m_Baked;
				else
				return float4(0,0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_texcoord9 = v.vertex;
				o.ase_color = v.ase_color;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord8.zw = v.ase_texcoord3.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				#if defined(LIGHTMAP_ON)
				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if !defined(LIGHTMAP_ON)
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord3 : TEXCOORD3;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.texcoord = v.texcoord;
				o.ase_color = v.ase_color;
				o.ase_texcoord3 = v.ase_texcoord3;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						, FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float m_switch364_g44339 = _ColorShift_Mode;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord8.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 ALBEDO_RGBA1381_g44312 = tex2DNode63_g44324;
				float3 temp_output_3_0_g44312 = ( (_Color).rgb * (ALBEDO_RGBA1381_g44312).rgb * _Brightness );
				float3 temp_output_134_0_g44339 = temp_output_3_0_g44312;
				float4 m_Off364_g44339 = float4( temp_output_134_0_g44339 , 0.0 );
				float m_switch313_g44339 = _ColorShift_MaskMode;
				float MODE176_g44339 = _ColorShift_Mode;
				float m_switch168_g44339 = MODE176_g44339;
				float m_Off168_g44339 = 0.0;
				float m_ObjectSpace168_g44339 = ( _ColorShift_NoiseScale / 3 );
				float3 p1_g44340 = ( WorldPosition * _ColorShift_WorldSpaceNoiseShift );
				float localSimpleNoise3D1_g44340 = SimpleNoise3D( p1_g44340 );
				float4 transform374_g44339 = mul(GetObjectToWorldMatrix(),IN.ase_texcoord9);
				float m_WorldSpace168_g44339 = ( ( localSimpleNoise3D1_g44340 / _ColorShift_NoiseScale ) - ( ( (transform374_g44339).w - _ColorShift_WorldSpaceOffset ) / _ColorShift_WorldSpaceDistance ) );
				float m_VertexColor168_g44339 = ( IN.ase_color.g - 0.5 );
				float m_VertexNormal168_g44339 = ( IN.ase_normal.y - 0.5 );
				float localColorShift_Modefloatswitch168_g44339 = ColorShift_Modefloatswitch168_g44339( m_switch168_g44339 , m_Off168_g44339 , m_ObjectSpace168_g44339 , m_WorldSpace168_g44339 , m_VertexColor168_g44339 , m_VertexNormal168_g44339 );
				float temp_output_112_0_g44339 = sin( ( _ColorShift_NoiseScale * PI ) );
				float3 ALBEDO_IN136_g44339 = temp_output_134_0_g44339;
				float2 appendResult120_g44339 = (float2(( (0.3 + (( 1.0 - temp_output_112_0_g44339 ) - 0.0) * (1.0 - 0.3) / (1.0 - 0.0)) * float4( ALBEDO_IN136_g44339 , 0.0 ).x ) , 0.0));
				float2 RGB146_g44339 = appendResult120_g44339;
				float3 hsvTorgb122_g44339 = RGBToHSV( float3( RGB146_g44339 ,  0.0 ) );
				float VALUE219_g44339 = temp_output_112_0_g44339;
				float3 hsvTorgb126_g44339 = HSVToRGB( float3(( ( saturate( localColorShift_Modefloatswitch168_g44339 ) * _ColorShift_ShiftVariation ) + _ColorShift_ShiftVariationRGB + hsvTorgb122_g44339 ).x,( _ColorShift_ShiftSaturation * hsvTorgb122_g44339.y ),( hsvTorgb122_g44339.z + ( VALUE219_g44339 / 40 ) )) );
				float4 m_Off313_g44339 = float4( hsvTorgb126_g44339 , 0.0 );
				float2 uv_ColorShift_MaskMap = IN.ase_texcoord8.xy * _ColorShift_MaskMap_ST.xy + _ColorShift_MaskMap_ST.zw;
				float4 transform376_g44339 = mul(GetObjectToWorldMatrix(),float4( IN.ase_texcoord9.xyz , 0.0 ));
				float4 temp_output_337_0_g44339 = saturate( ( 1.0 - ( ( SAMPLE_TEXTURE2D( _ColorShift_MaskMap, sampler_trilinear_repeat_aniso2, uv_ColorShift_MaskMap ) * transform376_g44339 ) / max( _ColorShift_MaskFuzziness , 1E-05 ) ) ) );
				float3 lerpResult314_g44339 = lerp( hsvTorgb126_g44339 , ALBEDO_IN136_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_Active313_g44339 = float4( lerpResult314_g44339 , 0.0 );
				float3 lerpResult311_g44339 = lerp( ALBEDO_IN136_g44339 , hsvTorgb126_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_ActiveInverted313_g44339 = float4( lerpResult311_g44339 , 0.0 );
				float4 localColorShift_Maskfloat4switch313_g44339 = ColorShift_Maskfloat4switch313_g44339( m_switch313_g44339 , m_Off313_g44339 , m_Active313_g44339 , m_ActiveInverted313_g44339 );
				float4 lerpResult297_g44339 = lerp( float4( temp_output_134_0_g44339 , 0.0 ) , localColorShift_Maskfloat4switch313_g44339 , _ColorShift_ShiftInfluence);
				float4 m_ObjectSpace364_g44339 = lerpResult297_g44339;
				float4 m_WorldSpace364_g44339 = lerpResult297_g44339;
				float4 m_VertexColor364_g44339 = lerpResult297_g44339;
				float4 m_VertexNormal364_g44339 = lerpResult297_g44339;
				float4 localColorShift_Modefloat4switch364_g44339 = ColorShift_Modefloat4switch364_g44339( m_switch364_g44339 , m_Off364_g44339 , m_ObjectSpace364_g44339 , m_WorldSpace364_g44339 , m_VertexColor364_g44339 , m_VertexNormal364_g44339 );
				
				float4 NORMAL_RGB1382_g44312 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 temp_output_1_0_g44332 = NORMAL_RGB1382_g44312;
				float temp_output_8_0_g44332 = _NormalStrength;
				float3 unpack52_g44332 = UnpackNormalScale( temp_output_1_0_g44332, temp_output_8_0_g44332 );
				unpack52_g44332.z = lerp( 1, unpack52_g44332.z, saturate(temp_output_8_0_g44332) );
				float3 temp_output_1519_59_g44312 = unpack52_g44332;
				float3 NORMAL_IN42_g44318 = temp_output_1519_59_g44312;
				float temp_output_50_0_g44318 = _NormalMode;
				float m_switch56_g44318 = temp_output_50_0_g44318;
				float3 m_Default56_g44318 = float3(1,1,1);
				float3 m_Flip56_g44318 = float3(-1,-1,-1);
				float3 m_Mirror56_g44318 = float3(1,1,-1);
				float3 local_NormalModefloat3switch56_g44318 = _NormalModefloat3switch( m_switch56_g44318 , m_Default56_g44318 , m_Flip56_g44318 , m_Mirror56_g44318 );
				float3 switchResult58_g44318 = (((ase_vface>0)?(NORMAL_IN42_g44318):(( NORMAL_IN42_g44318 * local_NormalModefloat3switch56_g44318 ))));
				float m_switch37_g44318 = temp_output_50_0_g44318;
				float3 m_Default37_g44318 = NORMAL_IN42_g44318;
				float3 m_Flip37_g44318 = ( NORMAL_IN42_g44318 * ase_vface );
				float3 break33_g44318 = NORMAL_IN42_g44318;
				float3 appendResult41_g44318 = (float3(break33_g44318.x , break33_g44318.y , ( break33_g44318.z * ase_vface )));
				float3 m_Mirror37_g44318 = appendResult41_g44318;
				float3 local_NormalModefloat3switch37_g44318 = _NormalModefloat3switch( m_switch37_g44318 , m_Default37_g44318 , m_Flip37_g44318 , m_Mirror37_g44318 );
				float3 temp_output_1610_30_g44312 = local_NormalModefloat3switch37_g44318;
				float3 NORMAL_OUT1491_g44312 = temp_output_1610_30_g44312;
				float3 temp_output_16_0_g44315 = NORMAL_OUT1491_g44312;
				float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				float3 tanNormal134_g44315 = temp_output_16_0_g44315;
				float3 worldNormal134_g44315 = float3(dot(tanToWorld0,tanNormal134_g44315), dot(tanToWorld1,tanNormal134_g44315), dot(tanToWorld2,tanNormal134_g44315));
				float3 NORMAL_TANGENTSPACE135_g44315 = worldNormal134_g44315;
				float3 WorldPosition63_g44315 = NORMAL_TANGENTSPACE135_g44315;
				float4 shadowMaskValue77_g44315 = float4(1,1,1,1);
				float4 ShadowMask63_g44315 = shadowMaskValue77_g44315;
				float3 localAdditionalLightsFlatMask63_g44315 = AdditionalLightsFlatMask( WorldPosition63_g44315 , ShadowMask63_g44315 );
				float3 ADDITIONAL_LIGHT1212_g44312 = localAdditionalLightsFlatMask63_g44315;
				
				float4 MASK_B1377_g44312 = SAMPLE_TEXTURE2D( _MetallicGlossMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float temp_output_1_0_g44334 = _MetallicStrength;
				
				float temp_output_223_0_g44330 = _SmoothnessSource;
				float m_switch215_g44330 = temp_output_223_0_g44330;
				float4 MASK_G158_g44312 = SAMPLE_TEXTURE2D( _SmoothnessMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 temp_cast_16 = (_SmoothnessMin).xxxx;
				float4 temp_cast_17 = (_SmoothnessMax).xxxx;
				float4 temp_output_216_0_g44330 = (temp_cast_16 + (MASK_G158_g44312 - float4( 0,0,0,0 )) * (temp_cast_17 - temp_cast_16) / (float4( 1,1,1,1 ) - float4( 0,0,0,0 )));
				float4 m_Smoothness215_g44330 = temp_output_216_0_g44330;
				float4 temp_cast_18 = (_SmoothnessMin).xxxx;
				float4 temp_cast_19 = (_SmoothnessMax).xxxx;
				float4 temp_output_214_0_g44330 = ( 1.0 - temp_output_216_0_g44330 );
				float4 m_Roughness215_g44330 = temp_output_214_0_g44330;
				float4 temp_cast_20 = (_SmoothnessMin).xxxx;
				float4 temp_cast_21 = (_SmoothnessMax).xxxx;
				float3 tanNormal1608_g44312 = NORMAL_OUT1491_g44312;
				float3 worldNormal1608_g44312 = normalize( float3(dot(tanToWorld0,tanNormal1608_g44312), dot(tanToWorld1,tanNormal1608_g44312), dot(tanToWorld2,tanNormal1608_g44312)) );
				float3 temp_output_4_0_g44330 = worldNormal1608_g44312;
				float3 temp_output_178_0_g44330 = ddx( temp_output_4_0_g44330 );
				float dotResult195_g44330 = dot( temp_output_178_0_g44330 , temp_output_178_0_g44330 );
				float3 temp_output_175_0_g44330 = ddy( temp_output_4_0_g44330 );
				float dotResult201_g44330 = dot( temp_output_175_0_g44330 , temp_output_175_0_g44330 );
				float ALBEDO_R172_g44312 = tex2DNode63_g44324.r;
				float4 temp_cast_22 = (ALBEDO_R172_g44312).xxxx;
				float4 m_Geometric215_g44330 = ( sqrt( saturate( ( temp_output_216_0_g44330 + ( ( dotResult195_g44330 + dotResult201_g44330 ) * 2.0 ) ) ) ) * ( 1.0 - temp_cast_22 ) );
				float4 localSmoothnessTypefloat4switch215_g44330 = SmoothnessTypefloat4switch215_g44330( m_switch215_g44330 , m_Smoothness215_g44330 , m_Roughness215_g44330 , m_Geometric215_g44330 );
				float4 temp_output_1635_33_g44312 = localSmoothnessTypefloat4switch215_g44330;
				float4 PBR_Occlusion1712_g44312 = SAMPLE_TEXTURE2D( _OcclusionMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float temp_output_16_0_g44316 = _OcclusionStrengthAO;
				float temp_output_65_0_g44316 = ( 1.0 - temp_output_16_0_g44316 );
				float3 appendResult69_g44316 = (float3(temp_output_65_0_g44316 , temp_output_65_0_g44316 , temp_output_65_0_g44316));
				float4 color77_g44316 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 temp_cast_25 = (IN.ase_color.a).xxxx;
				float4 lerpResult75_g44316 = lerp( color77_g44316 , temp_cast_25 , temp_output_16_0_g44316);
				float4 lerpResult83_g44316 = lerp( float4( ( ( ( (PBR_Occlusion1712_g44312).xyz - float3( 0.5,0.5,0.5 ) ) * temp_output_16_0_g44316 ) + appendResult69_g44316 ) , 0.0 ) , lerpResult75_g44316 , _OcclusionSource);
				float4 Occlusion1628_g44312 = saturate( lerpResult83_g44316 );
				
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , WorldViewDirection );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				float m_switch103_g44322 = _TranslucencySourceFoward;
				float4 color71_g44322 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				float4 temp_output_113_0_g44322 = saturate( ( SAMPLE_TEXTURE2D( _TranslucencyMap, sampler_trilinear_repeat_aniso2, OUT_UV213_g44312 ) / max( _TranslucencyThicknessFeather , 1E-05 ) ) );
				float4 lerpResult34_g44322 = lerp( color71_g44322 , temp_output_113_0_g44322 , _TranslucencyThickness);
				float4 m_Texture103_g44322 = lerpResult34_g44322;
				float temp_output_69_0_g44322 = ( 0.0 - 1.0 );
				float temp_output_22_0_g44322 = ( temp_output_113_0_g44322.r - 1.0 );
				float lerpResult66_g44322 = lerp( ( temp_output_69_0_g44322 / temp_output_22_0_g44322 ) , ( temp_output_22_0_g44322 / temp_output_69_0_g44322 ) , ( 0.7 + _TranslucencyThickness ));
				float4 temp_cast_30 = (saturate( lerpResult66_g44322 )).xxxx;
				float4 m_TextureInverted103_g44322 = temp_cast_30;
				float lerpResult30_g44322 = lerp( 0.0 , ( 5.0 - IN.ase_texcoord8.zw.x ) , _TranslucencyThickness);
				float4 temp_cast_31 = (lerpResult30_g44322).xxxx;
				float4 m_Baked103_g44322 = temp_cast_31;
				float4 localfloat4switch103_g44322 = float4switch103_g44322( m_switch103_g44322 , m_Texture103_g44322 , m_TextureInverted103_g44322 , m_Baked103_g44322 );
				float3 temp_output_90_0_g44322 = saturate( ( _MainLightColor.rgb * _MainLightColor.a ) );
				float4 lerpResult122_g44322 = lerp( float4( 0,0,0,0 ) , ( ( _TransmissionTint * _TransmissionTint.a * localfloat4switch103_g44322 ) * float4( temp_output_90_0_g44322 , 0.0 ) ) , _EnableTranslucency);
				
				float4 lerpResult123_g44322 = lerp( float4( 0,0,0,0 ) , ( ( _TranslucencyTint * _TranslucencyTint.a * localfloat4switch103_g44322 ) * float4( temp_output_90_0_g44322 , 0.0 ) ) , _EnableTranslucency);
				
				float3 Albedo = localColorShift_Modefloat4switch364_g44339.xyz;
				float3 Normal = ( switchResult58_g44318 + ADDITIONAL_LIGHT1212_g44312 );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( MASK_B1377_g44312.r * temp_output_1_0_g44334 );
				float Smoothness = ( temp_output_1635_33_g44312 * Occlusion1628_g44312 ).x;
				float Occlusion = saturate( lerpResult83_g44316 ).r;
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;
				float AlphaClipThresholdShadow = _AlphaCutoffBiasShadow;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = lerpResult122_g44322.rgb;
				float3 Translucency = lerpResult123_g44322.rgb;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif
				
				#ifdef _CLEARCOAT
				float CoatMask = 0;
				float CoatSmoothness = 0;
				#endif


				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = ShadowCoords;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif


				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				#if defined(DYNAMICLIGHTMAP_ON)
				inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
				#else
				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#endif

				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
					#endif

					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				SurfaceData surfaceData;
				surfaceData.albedo              = Albedo;
				surfaceData.metallic            = saturate(Metallic);
				surfaceData.specular            = Specular;
				surfaceData.smoothness          = saturate(Smoothness),
				surfaceData.occlusion           = Occlusion,
				surfaceData.emission            = Emission,
				surfaceData.alpha               = saturate(Alpha);
				surfaceData.normalTS            = Normal;
				surfaceData.clearCoatMask       = 0;
				surfaceData.clearCoatSmoothness = 1;


				#ifdef _CLEARCOAT
					surfaceData.clearCoatMask       = saturate(CoatMask);
					surfaceData.clearCoatSmoothness = saturate(CoatSmoothness);
				#endif

				#ifdef _DBUFFER
					ApplyDecalToSurfaceData(IN.clipPos, surfaceData, inputData);
				#endif

				half4 color = UniversalFragmentPBR( inputData, surfaceData);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _URP_TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _URP_TransmissionShadow;
					float normal = _URP_TranslucencyNormalDistortion;
					float scattering = _URP_TranslucencyScattering;
					float direct = _URP_TranslucencyDirect;
					float ambient = _URP_TranslucencyAmbient;
					float strength = _URP_TranslucencyStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			

			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);


			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif

				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif


				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord2.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;
				float AlphaClipThresholdShadow = _AlphaCutoffBiasShadow;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHONLY
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord2.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature _ EDITOR_VISUALIZATION

			#define SHADERPASS SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_POSITION
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef EDITOR_VISUALIZATION
				float4 VizUV : TEXCOORD2;
				float4 LightCoord : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_color : COLOR;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_ColorShift_MaskMap);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float ColorShift_Modefloatswitch168_g44339( float m_switch, float m_Off, float m_ObjectSpace, float m_WorldSpace, float m_VertexColor, float m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float(0);
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float4 ColorShift_Maskfloat4switch313_g44339( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float4(0,0,0,0);
			}
			
			float4 ColorShift_Modefloat4switch364_g44339( float m_switch, float4 m_Off, float4 m_ObjectSpace, float4 m_WorldSpace, float4 m_VertexColor, float4 m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float4(0,0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord4.xy = v.texcoord0.xy;
				o.ase_texcoord5 = v.vertex;
				o.ase_color = v.ase_color;
				o.ase_normal = v.ase_normal;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );

			#ifdef EDITOR_VISUALIZATION
				float2 VizUV = 0;
				float4 LightCoord = 0;
				UnityEditorVizData(v.vertex.xyz, v.texcoord0.xy, v.texcoord1.xy, v.texcoord2.xy, VizUV, LightCoord);
				o.VizUV = float4(VizUV, 0, 0);
				o.LightCoord = LightCoord;
			#endif

			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = o.clipPos;
				o.shadowCoord = GetShadowCoord( vertexInput );
			#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord0 = v.texcoord0;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord0 = patch[0].texcoord0 * bary.x + patch[1].texcoord0 * bary.y + patch[2].texcoord0 * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float m_switch364_g44339 = _ColorShift_Mode;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord4.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 ALBEDO_RGBA1381_g44312 = tex2DNode63_g44324;
				float3 temp_output_3_0_g44312 = ( (_Color).rgb * (ALBEDO_RGBA1381_g44312).rgb * _Brightness );
				float3 temp_output_134_0_g44339 = temp_output_3_0_g44312;
				float4 m_Off364_g44339 = float4( temp_output_134_0_g44339 , 0.0 );
				float m_switch313_g44339 = _ColorShift_MaskMode;
				float MODE176_g44339 = _ColorShift_Mode;
				float m_switch168_g44339 = MODE176_g44339;
				float m_Off168_g44339 = 0.0;
				float m_ObjectSpace168_g44339 = ( _ColorShift_NoiseScale / 3 );
				float3 p1_g44340 = ( WorldPosition * _ColorShift_WorldSpaceNoiseShift );
				float localSimpleNoise3D1_g44340 = SimpleNoise3D( p1_g44340 );
				float4 transform374_g44339 = mul(GetObjectToWorldMatrix(),IN.ase_texcoord5);
				float m_WorldSpace168_g44339 = ( ( localSimpleNoise3D1_g44340 / _ColorShift_NoiseScale ) - ( ( (transform374_g44339).w - _ColorShift_WorldSpaceOffset ) / _ColorShift_WorldSpaceDistance ) );
				float m_VertexColor168_g44339 = ( IN.ase_color.g - 0.5 );
				float m_VertexNormal168_g44339 = ( IN.ase_normal.y - 0.5 );
				float localColorShift_Modefloatswitch168_g44339 = ColorShift_Modefloatswitch168_g44339( m_switch168_g44339 , m_Off168_g44339 , m_ObjectSpace168_g44339 , m_WorldSpace168_g44339 , m_VertexColor168_g44339 , m_VertexNormal168_g44339 );
				float temp_output_112_0_g44339 = sin( ( _ColorShift_NoiseScale * PI ) );
				float3 ALBEDO_IN136_g44339 = temp_output_134_0_g44339;
				float2 appendResult120_g44339 = (float2(( (0.3 + (( 1.0 - temp_output_112_0_g44339 ) - 0.0) * (1.0 - 0.3) / (1.0 - 0.0)) * float4( ALBEDO_IN136_g44339 , 0.0 ).x ) , 0.0));
				float2 RGB146_g44339 = appendResult120_g44339;
				float3 hsvTorgb122_g44339 = RGBToHSV( float3( RGB146_g44339 ,  0.0 ) );
				float VALUE219_g44339 = temp_output_112_0_g44339;
				float3 hsvTorgb126_g44339 = HSVToRGB( float3(( ( saturate( localColorShift_Modefloatswitch168_g44339 ) * _ColorShift_ShiftVariation ) + _ColorShift_ShiftVariationRGB + hsvTorgb122_g44339 ).x,( _ColorShift_ShiftSaturation * hsvTorgb122_g44339.y ),( hsvTorgb122_g44339.z + ( VALUE219_g44339 / 40 ) )) );
				float4 m_Off313_g44339 = float4( hsvTorgb126_g44339 , 0.0 );
				float2 uv_ColorShift_MaskMap = IN.ase_texcoord4.xy * _ColorShift_MaskMap_ST.xy + _ColorShift_MaskMap_ST.zw;
				float4 transform376_g44339 = mul(GetObjectToWorldMatrix(),float4( IN.ase_texcoord5.xyz , 0.0 ));
				float4 temp_output_337_0_g44339 = saturate( ( 1.0 - ( ( SAMPLE_TEXTURE2D( _ColorShift_MaskMap, sampler_trilinear_repeat_aniso2, uv_ColorShift_MaskMap ) * transform376_g44339 ) / max( _ColorShift_MaskFuzziness , 1E-05 ) ) ) );
				float3 lerpResult314_g44339 = lerp( hsvTorgb126_g44339 , ALBEDO_IN136_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_Active313_g44339 = float4( lerpResult314_g44339 , 0.0 );
				float3 lerpResult311_g44339 = lerp( ALBEDO_IN136_g44339 , hsvTorgb126_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_ActiveInverted313_g44339 = float4( lerpResult311_g44339 , 0.0 );
				float4 localColorShift_Maskfloat4switch313_g44339 = ColorShift_Maskfloat4switch313_g44339( m_switch313_g44339 , m_Off313_g44339 , m_Active313_g44339 , m_ActiveInverted313_g44339 );
				float4 lerpResult297_g44339 = lerp( float4( temp_output_134_0_g44339 , 0.0 ) , localColorShift_Maskfloat4switch313_g44339 , _ColorShift_ShiftInfluence);
				float4 m_ObjectSpace364_g44339 = lerpResult297_g44339;
				float4 m_WorldSpace364_g44339 = lerpResult297_g44339;
				float4 m_VertexColor364_g44339 = lerpResult297_g44339;
				float4 m_VertexNormal364_g44339 = lerpResult297_g44339;
				float4 localColorShift_Modefloat4switch364_g44339 = ColorShift_Modefloat4switch364_g44339( m_switch364_g44339 , m_Off364_g44339 , m_ObjectSpace364_g44339 , m_WorldSpace364_g44339 , m_VertexColor364_g44339 , m_VertexNormal364_g44339 );
				
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				
				float3 Albedo = localColorShift_Modefloat4switch364_g44339.xyz;
				float3 Emission = 0;
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
			#ifdef EDITOR_VISUALIZATION
				metaInput.VizUV = IN.VizUV.xy;
				metaInput.LightCoord = IN.LightCoord;
			#endif
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ColorMask [_ColorMask]

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_2D
        
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
			
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_POSITION
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_color : COLOR;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_ColorShift_MaskMap);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float ColorShift_Modefloatswitch168_g44339( float m_switch, float m_Off, float m_ObjectSpace, float m_WorldSpace, float m_VertexColor, float m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float(0);
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float4 ColorShift_Maskfloat4switch313_g44339( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float4(0,0,0,0);
			}
			
			float4 ColorShift_Modefloat4switch364_g44339( float m_switch, float4 m_Off, float4 m_ObjectSpace, float4 m_WorldSpace, float4 m_VertexColor, float4 m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float4(0,0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.vertex;
				o.ase_color = v.ase_color;
				o.ase_normal = v.ase_normal;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float m_switch364_g44339 = _ColorShift_Mode;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord2.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 ALBEDO_RGBA1381_g44312 = tex2DNode63_g44324;
				float3 temp_output_3_0_g44312 = ( (_Color).rgb * (ALBEDO_RGBA1381_g44312).rgb * _Brightness );
				float3 temp_output_134_0_g44339 = temp_output_3_0_g44312;
				float4 m_Off364_g44339 = float4( temp_output_134_0_g44339 , 0.0 );
				float m_switch313_g44339 = _ColorShift_MaskMode;
				float MODE176_g44339 = _ColorShift_Mode;
				float m_switch168_g44339 = MODE176_g44339;
				float m_Off168_g44339 = 0.0;
				float m_ObjectSpace168_g44339 = ( _ColorShift_NoiseScale / 3 );
				float3 p1_g44340 = ( WorldPosition * _ColorShift_WorldSpaceNoiseShift );
				float localSimpleNoise3D1_g44340 = SimpleNoise3D( p1_g44340 );
				float4 transform374_g44339 = mul(GetObjectToWorldMatrix(),IN.ase_texcoord3);
				float m_WorldSpace168_g44339 = ( ( localSimpleNoise3D1_g44340 / _ColorShift_NoiseScale ) - ( ( (transform374_g44339).w - _ColorShift_WorldSpaceOffset ) / _ColorShift_WorldSpaceDistance ) );
				float m_VertexColor168_g44339 = ( IN.ase_color.g - 0.5 );
				float m_VertexNormal168_g44339 = ( IN.ase_normal.y - 0.5 );
				float localColorShift_Modefloatswitch168_g44339 = ColorShift_Modefloatswitch168_g44339( m_switch168_g44339 , m_Off168_g44339 , m_ObjectSpace168_g44339 , m_WorldSpace168_g44339 , m_VertexColor168_g44339 , m_VertexNormal168_g44339 );
				float temp_output_112_0_g44339 = sin( ( _ColorShift_NoiseScale * PI ) );
				float3 ALBEDO_IN136_g44339 = temp_output_134_0_g44339;
				float2 appendResult120_g44339 = (float2(( (0.3 + (( 1.0 - temp_output_112_0_g44339 ) - 0.0) * (1.0 - 0.3) / (1.0 - 0.0)) * float4( ALBEDO_IN136_g44339 , 0.0 ).x ) , 0.0));
				float2 RGB146_g44339 = appendResult120_g44339;
				float3 hsvTorgb122_g44339 = RGBToHSV( float3( RGB146_g44339 ,  0.0 ) );
				float VALUE219_g44339 = temp_output_112_0_g44339;
				float3 hsvTorgb126_g44339 = HSVToRGB( float3(( ( saturate( localColorShift_Modefloatswitch168_g44339 ) * _ColorShift_ShiftVariation ) + _ColorShift_ShiftVariationRGB + hsvTorgb122_g44339 ).x,( _ColorShift_ShiftSaturation * hsvTorgb122_g44339.y ),( hsvTorgb122_g44339.z + ( VALUE219_g44339 / 40 ) )) );
				float4 m_Off313_g44339 = float4( hsvTorgb126_g44339 , 0.0 );
				float2 uv_ColorShift_MaskMap = IN.ase_texcoord2.xy * _ColorShift_MaskMap_ST.xy + _ColorShift_MaskMap_ST.zw;
				float4 transform376_g44339 = mul(GetObjectToWorldMatrix(),float4( IN.ase_texcoord3.xyz , 0.0 ));
				float4 temp_output_337_0_g44339 = saturate( ( 1.0 - ( ( SAMPLE_TEXTURE2D( _ColorShift_MaskMap, sampler_trilinear_repeat_aniso2, uv_ColorShift_MaskMap ) * transform376_g44339 ) / max( _ColorShift_MaskFuzziness , 1E-05 ) ) ) );
				float3 lerpResult314_g44339 = lerp( hsvTorgb126_g44339 , ALBEDO_IN136_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_Active313_g44339 = float4( lerpResult314_g44339 , 0.0 );
				float3 lerpResult311_g44339 = lerp( ALBEDO_IN136_g44339 , hsvTorgb126_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_ActiveInverted313_g44339 = float4( lerpResult311_g44339 , 0.0 );
				float4 localColorShift_Maskfloat4switch313_g44339 = ColorShift_Maskfloat4switch313_g44339( m_switch313_g44339 , m_Off313_g44339 , m_Active313_g44339 , m_ActiveInverted313_g44339 );
				float4 lerpResult297_g44339 = lerp( float4( temp_output_134_0_g44339 , 0.0 ) , localColorShift_Maskfloat4switch313_g44339 , _ColorShift_ShiftInfluence);
				float4 m_ObjectSpace364_g44339 = lerpResult297_g44339;
				float4 m_WorldSpace364_g44339 = lerpResult297_g44339;
				float4 m_VertexColor364_g44339 = lerpResult297_g44339;
				float4 m_VertexNormal364_g44339 = lerpResult297_g44339;
				float4 localColorShift_Modefloat4switch364_g44339 = ColorShift_Modefloat4switch364_g44339( m_switch364_g44339 , m_Off364_g44339 , m_ObjectSpace364_g44339 , m_WorldSpace364_g44339 , m_VertexColor364_g44339 , m_VertexNormal364_g44339 );
				
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				
				float3 Albedo = localColorShift_Modefloat4switch364_g44339.xyz;
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				float4 worldTangent : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_BumpMap);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_MainTex);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			
			float3 _NormalModefloat3switch( float m_switch, float3 m_Default, float3 m_Flip, float3 m_Mirror )
			{
				if(m_switch ==0)
					return m_Default;
				else if(m_switch ==1)
					return m_Flip;
				else if(m_switch ==2)
					return m_Mirror;
				else
				return float3(0,0,0);
			}
			
			float3 AdditionalLightsFlatMask( float3 WorldPosition, float4 ShadowMask )
			{
				float3 Color = 0;
				#ifdef _ADDITIONAL_LIGHTS
				int numLights = GetAdditionalLightsCount();
				for(int i = 0; i<numLights;i++)
				{
				#if ASE_SRP_VERSION >= 100000
					Light light = GetAdditionalLight(i, WorldPosition, ShadowMask);
				#else
					Light light = GetAdditionalLight(i, WorldPosition);
				#endif
					Color += light.color *(light.distanceAttenuation * light.shadowAttenuation);
					
				}
				#endif
				return Color;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				o.ase_texcoord5.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 tangentWS = float4(TransformObjectToWorldDir( v.ase_tangent.xyz), v.ase_tangent.w);
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;
				o.worldTangent = tangentWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.ase_color = v.ase_color;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						, FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				float3 WorldNormal = IN.worldNormal;
				float4 WorldTangent = IN.worldTangent;

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord4.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 NORMAL_RGB1382_g44312 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 temp_output_1_0_g44332 = NORMAL_RGB1382_g44312;
				float temp_output_8_0_g44332 = _NormalStrength;
				float3 unpack52_g44332 = UnpackNormalScale( temp_output_1_0_g44332, temp_output_8_0_g44332 );
				unpack52_g44332.z = lerp( 1, unpack52_g44332.z, saturate(temp_output_8_0_g44332) );
				float3 temp_output_1519_59_g44312 = unpack52_g44332;
				float3 NORMAL_IN42_g44318 = temp_output_1519_59_g44312;
				float temp_output_50_0_g44318 = _NormalMode;
				float m_switch56_g44318 = temp_output_50_0_g44318;
				float3 m_Default56_g44318 = float3(1,1,1);
				float3 m_Flip56_g44318 = float3(-1,-1,-1);
				float3 m_Mirror56_g44318 = float3(1,1,-1);
				float3 local_NormalModefloat3switch56_g44318 = _NormalModefloat3switch( m_switch56_g44318 , m_Default56_g44318 , m_Flip56_g44318 , m_Mirror56_g44318 );
				float3 switchResult58_g44318 = (((ase_vface>0)?(NORMAL_IN42_g44318):(( NORMAL_IN42_g44318 * local_NormalModefloat3switch56_g44318 ))));
				float m_switch37_g44318 = temp_output_50_0_g44318;
				float3 m_Default37_g44318 = NORMAL_IN42_g44318;
				float3 m_Flip37_g44318 = ( NORMAL_IN42_g44318 * ase_vface );
				float3 break33_g44318 = NORMAL_IN42_g44318;
				float3 appendResult41_g44318 = (float3(break33_g44318.x , break33_g44318.y , ( break33_g44318.z * ase_vface )));
				float3 m_Mirror37_g44318 = appendResult41_g44318;
				float3 local_NormalModefloat3switch37_g44318 = _NormalModefloat3switch( m_switch37_g44318 , m_Default37_g44318 , m_Flip37_g44318 , m_Mirror37_g44318 );
				float3 temp_output_1610_30_g44312 = local_NormalModefloat3switch37_g44318;
				float3 NORMAL_OUT1491_g44312 = temp_output_1610_30_g44312;
				float3 temp_output_16_0_g44315 = NORMAL_OUT1491_g44312;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3 tanToWorld0 = float3( WorldTangent.xyz.x, ase_worldBitangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.xyz.y, ase_worldBitangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.xyz.z, ase_worldBitangent.z, WorldNormal.z );
				float3 tanNormal134_g44315 = temp_output_16_0_g44315;
				float3 worldNormal134_g44315 = float3(dot(tanToWorld0,tanNormal134_g44315), dot(tanToWorld1,tanNormal134_g44315), dot(tanToWorld2,tanNormal134_g44315));
				float3 NORMAL_TANGENTSPACE135_g44315 = worldNormal134_g44315;
				float3 WorldPosition63_g44315 = NORMAL_TANGENTSPACE135_g44315;
				float4 shadowMaskValue77_g44315 = float4(1,1,1,1);
				float4 ShadowMask63_g44315 = shadowMaskValue77_g44315;
				float3 localAdditionalLightsFlatMask63_g44315 = AdditionalLightsFlatMask( WorldPosition63_g44315 , ShadowMask63_g44315 );
				float3 ADDITIONAL_LIGHT1212_g44312 = localAdditionalLightsFlatMask63_g44315;
				
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				float3 Normal = ( switchResult58_g44318 + ADDITIONAL_LIGHT1212_g44312 );
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				
				#if defined(_GBUFFER_NORMALS_OCT)
					float2 octNormalWS = PackNormalOctQuadEncode(WorldNormal);
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
					return half4(packedNormalWS, 0.0);
				#else
					
					#if defined(_NORMALMAP)
						#if _NORMAL_DROPOFF_TS
							float crossSign = (WorldTangent.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
							float3 bitangent = crossSign * cross(WorldNormal.xyz, WorldTangent.xyz);
							float3 normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent.xyz, bitangent, WorldNormal.xyz));
						#elif _NORMAL_DROPOFF_OS
							float3 normalWS = TransformObjectToWorldNormal(Normal);
						#elif _NORMAL_DROPOFF_WS
							float3 normalWS = Normal;
						#endif
					#else
						float3 normalWS = WorldNormal;
					#endif

					return half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			
			Blend One Zero, One Zero
			ColorMask [_ColorMask]
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

			
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			
			#pragma multi_compile _ _REFLECTION_PROBE_BLENDING
			#pragma multi_compile _ _REFLECTION_PROBE_BOX_PROJECTION

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile _ _GBUFFER_NORMALS_OCT
			#pragma multi_compile _ _LIGHT_LAYERS
			#pragma multi_compile _ _RENDER_PASS_ENABLED

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"


			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_POSITION
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float2 dynamicLightmapUV : TEXCOORD7;
				#endif
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_color : COLOR;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_ColorShift_MaskMap);
			TEXTURE2D(_BumpMap);
			TEXTURE2D(_MetallicGlossMap);
			TEXTURE2D(_SmoothnessMap);
			TEXTURE2D(_OcclusionMap);
			TEXTURE2D(_TranslucencyMap);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float ColorShift_Modefloatswitch168_g44339( float m_switch, float m_Off, float m_ObjectSpace, float m_WorldSpace, float m_VertexColor, float m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float(0);
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float4 ColorShift_Maskfloat4switch313_g44339( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float4(0,0,0,0);
			}
			
			float4 ColorShift_Modefloat4switch364_g44339( float m_switch, float4 m_Off, float4 m_ObjectSpace, float4 m_WorldSpace, float4 m_VertexColor, float4 m_VertexNormal )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ObjectSpace;
				else if(m_switch ==2)
					return m_WorldSpace;
				else if(m_switch ==3)
					return m_VertexColor;
				else if(m_switch ==4)
					return m_VertexNormal;
				else
				return float4(0,0,0,0);
			}
			
			float3 _NormalModefloat3switch( float m_switch, float3 m_Default, float3 m_Flip, float3 m_Mirror )
			{
				if(m_switch ==0)
					return m_Default;
				else if(m_switch ==1)
					return m_Flip;
				else if(m_switch ==2)
					return m_Mirror;
				else
				return float3(0,0,0);
			}
			
			float3 AdditionalLightsFlatMask( float3 WorldPosition, float4 ShadowMask )
			{
				float3 Color = 0;
				#ifdef _ADDITIONAL_LIGHTS
				int numLights = GetAdditionalLightsCount();
				for(int i = 0; i<numLights;i++)
				{
				#if ASE_SRP_VERSION >= 100000
					Light light = GetAdditionalLight(i, WorldPosition, ShadowMask);
				#else
					Light light = GetAdditionalLight(i, WorldPosition);
				#endif
					Color += light.color *(light.distanceAttenuation * light.shadowAttenuation);
					
				}
				#endif
				return Color;
			}
			
			float4 SmoothnessTypefloat4switch215_g44330( float m_switch, float4 m_Smoothness, float4 m_Roughness, float4 m_Geometric )
			{
				if(m_switch ==0)
					return m_Smoothness;
				else if(m_switch ==1)
					return m_Roughness;
				else if(m_switch ==2)
					return m_Geometric;
				else
				return float4(0,0,0,0);
			}
			
			float4 float4switch103_g44322( float m_switch, float4 m_Texture, float4 m_TextureInverted, float4 m_Baked )
			{
				if(m_switch ==0)
					return m_Texture;
				else if(m_switch ==1)
					return m_TextureInverted;
				else if(m_switch ==2)
					return m_Baked;
				else
				return float4(0,0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_texcoord9 = v.vertex;
				o.ase_color = v.ase_color;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord8.zw = v.ase_texcoord3.xy;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				#if defined(DYNAMICLIGHTMAP_ON)
				o.dynamicLightmapUV.xy = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 ase_texcoord3 : TEXCOORD3;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.texcoord = v.texcoord;
				o.ase_color = v.ase_color;
				o.ase_texcoord3 = v.ase_texcoord3;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			FragmentOutput frag ( VertexOutput IN 
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								, FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#else
					ShadowCoords = float4(0, 0, 0, 0);
				#endif


	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float m_switch364_g44339 = _ColorShift_Mode;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord8.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 ALBEDO_RGBA1381_g44312 = tex2DNode63_g44324;
				float3 temp_output_3_0_g44312 = ( (_Color).rgb * (ALBEDO_RGBA1381_g44312).rgb * _Brightness );
				float3 temp_output_134_0_g44339 = temp_output_3_0_g44312;
				float4 m_Off364_g44339 = float4( temp_output_134_0_g44339 , 0.0 );
				float m_switch313_g44339 = _ColorShift_MaskMode;
				float MODE176_g44339 = _ColorShift_Mode;
				float m_switch168_g44339 = MODE176_g44339;
				float m_Off168_g44339 = 0.0;
				float m_ObjectSpace168_g44339 = ( _ColorShift_NoiseScale / 3 );
				float3 p1_g44340 = ( WorldPosition * _ColorShift_WorldSpaceNoiseShift );
				float localSimpleNoise3D1_g44340 = SimpleNoise3D( p1_g44340 );
				float4 transform374_g44339 = mul(GetObjectToWorldMatrix(),IN.ase_texcoord9);
				float m_WorldSpace168_g44339 = ( ( localSimpleNoise3D1_g44340 / _ColorShift_NoiseScale ) - ( ( (transform374_g44339).w - _ColorShift_WorldSpaceOffset ) / _ColorShift_WorldSpaceDistance ) );
				float m_VertexColor168_g44339 = ( IN.ase_color.g - 0.5 );
				float m_VertexNormal168_g44339 = ( IN.ase_normal.y - 0.5 );
				float localColorShift_Modefloatswitch168_g44339 = ColorShift_Modefloatswitch168_g44339( m_switch168_g44339 , m_Off168_g44339 , m_ObjectSpace168_g44339 , m_WorldSpace168_g44339 , m_VertexColor168_g44339 , m_VertexNormal168_g44339 );
				float temp_output_112_0_g44339 = sin( ( _ColorShift_NoiseScale * PI ) );
				float3 ALBEDO_IN136_g44339 = temp_output_134_0_g44339;
				float2 appendResult120_g44339 = (float2(( (0.3 + (( 1.0 - temp_output_112_0_g44339 ) - 0.0) * (1.0 - 0.3) / (1.0 - 0.0)) * float4( ALBEDO_IN136_g44339 , 0.0 ).x ) , 0.0));
				float2 RGB146_g44339 = appendResult120_g44339;
				float3 hsvTorgb122_g44339 = RGBToHSV( float3( RGB146_g44339 ,  0.0 ) );
				float VALUE219_g44339 = temp_output_112_0_g44339;
				float3 hsvTorgb126_g44339 = HSVToRGB( float3(( ( saturate( localColorShift_Modefloatswitch168_g44339 ) * _ColorShift_ShiftVariation ) + _ColorShift_ShiftVariationRGB + hsvTorgb122_g44339 ).x,( _ColorShift_ShiftSaturation * hsvTorgb122_g44339.y ),( hsvTorgb122_g44339.z + ( VALUE219_g44339 / 40 ) )) );
				float4 m_Off313_g44339 = float4( hsvTorgb126_g44339 , 0.0 );
				float2 uv_ColorShift_MaskMap = IN.ase_texcoord8.xy * _ColorShift_MaskMap_ST.xy + _ColorShift_MaskMap_ST.zw;
				float4 transform376_g44339 = mul(GetObjectToWorldMatrix(),float4( IN.ase_texcoord9.xyz , 0.0 ));
				float4 temp_output_337_0_g44339 = saturate( ( 1.0 - ( ( SAMPLE_TEXTURE2D( _ColorShift_MaskMap, sampler_trilinear_repeat_aniso2, uv_ColorShift_MaskMap ) * transform376_g44339 ) / max( _ColorShift_MaskFuzziness , 1E-05 ) ) ) );
				float3 lerpResult314_g44339 = lerp( hsvTorgb126_g44339 , ALBEDO_IN136_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_Active313_g44339 = float4( lerpResult314_g44339 , 0.0 );
				float3 lerpResult311_g44339 = lerp( ALBEDO_IN136_g44339 , hsvTorgb126_g44339 , temp_output_337_0_g44339.rgb);
				float4 m_ActiveInverted313_g44339 = float4( lerpResult311_g44339 , 0.0 );
				float4 localColorShift_Maskfloat4switch313_g44339 = ColorShift_Maskfloat4switch313_g44339( m_switch313_g44339 , m_Off313_g44339 , m_Active313_g44339 , m_ActiveInverted313_g44339 );
				float4 lerpResult297_g44339 = lerp( float4( temp_output_134_0_g44339 , 0.0 ) , localColorShift_Maskfloat4switch313_g44339 , _ColorShift_ShiftInfluence);
				float4 m_ObjectSpace364_g44339 = lerpResult297_g44339;
				float4 m_WorldSpace364_g44339 = lerpResult297_g44339;
				float4 m_VertexColor364_g44339 = lerpResult297_g44339;
				float4 m_VertexNormal364_g44339 = lerpResult297_g44339;
				float4 localColorShift_Modefloat4switch364_g44339 = ColorShift_Modefloat4switch364_g44339( m_switch364_g44339 , m_Off364_g44339 , m_ObjectSpace364_g44339 , m_WorldSpace364_g44339 , m_VertexColor364_g44339 , m_VertexNormal364_g44339 );
				
				float4 NORMAL_RGB1382_g44312 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 temp_output_1_0_g44332 = NORMAL_RGB1382_g44312;
				float temp_output_8_0_g44332 = _NormalStrength;
				float3 unpack52_g44332 = UnpackNormalScale( temp_output_1_0_g44332, temp_output_8_0_g44332 );
				unpack52_g44332.z = lerp( 1, unpack52_g44332.z, saturate(temp_output_8_0_g44332) );
				float3 temp_output_1519_59_g44312 = unpack52_g44332;
				float3 NORMAL_IN42_g44318 = temp_output_1519_59_g44312;
				float temp_output_50_0_g44318 = _NormalMode;
				float m_switch56_g44318 = temp_output_50_0_g44318;
				float3 m_Default56_g44318 = float3(1,1,1);
				float3 m_Flip56_g44318 = float3(-1,-1,-1);
				float3 m_Mirror56_g44318 = float3(1,1,-1);
				float3 local_NormalModefloat3switch56_g44318 = _NormalModefloat3switch( m_switch56_g44318 , m_Default56_g44318 , m_Flip56_g44318 , m_Mirror56_g44318 );
				float3 switchResult58_g44318 = (((ase_vface>0)?(NORMAL_IN42_g44318):(( NORMAL_IN42_g44318 * local_NormalModefloat3switch56_g44318 ))));
				float m_switch37_g44318 = temp_output_50_0_g44318;
				float3 m_Default37_g44318 = NORMAL_IN42_g44318;
				float3 m_Flip37_g44318 = ( NORMAL_IN42_g44318 * ase_vface );
				float3 break33_g44318 = NORMAL_IN42_g44318;
				float3 appendResult41_g44318 = (float3(break33_g44318.x , break33_g44318.y , ( break33_g44318.z * ase_vface )));
				float3 m_Mirror37_g44318 = appendResult41_g44318;
				float3 local_NormalModefloat3switch37_g44318 = _NormalModefloat3switch( m_switch37_g44318 , m_Default37_g44318 , m_Flip37_g44318 , m_Mirror37_g44318 );
				float3 temp_output_1610_30_g44312 = local_NormalModefloat3switch37_g44318;
				float3 NORMAL_OUT1491_g44312 = temp_output_1610_30_g44312;
				float3 temp_output_16_0_g44315 = NORMAL_OUT1491_g44312;
				float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				float3 tanNormal134_g44315 = temp_output_16_0_g44315;
				float3 worldNormal134_g44315 = float3(dot(tanToWorld0,tanNormal134_g44315), dot(tanToWorld1,tanNormal134_g44315), dot(tanToWorld2,tanNormal134_g44315));
				float3 NORMAL_TANGENTSPACE135_g44315 = worldNormal134_g44315;
				float3 WorldPosition63_g44315 = NORMAL_TANGENTSPACE135_g44315;
				float4 shadowMaskValue77_g44315 = float4(1,1,1,1);
				float4 ShadowMask63_g44315 = shadowMaskValue77_g44315;
				float3 localAdditionalLightsFlatMask63_g44315 = AdditionalLightsFlatMask( WorldPosition63_g44315 , ShadowMask63_g44315 );
				float3 ADDITIONAL_LIGHT1212_g44312 = localAdditionalLightsFlatMask63_g44315;
				
				float4 MASK_B1377_g44312 = SAMPLE_TEXTURE2D( _MetallicGlossMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float temp_output_1_0_g44334 = _MetallicStrength;
				
				float temp_output_223_0_g44330 = _SmoothnessSource;
				float m_switch215_g44330 = temp_output_223_0_g44330;
				float4 MASK_G158_g44312 = SAMPLE_TEXTURE2D( _SmoothnessMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float4 temp_cast_16 = (_SmoothnessMin).xxxx;
				float4 temp_cast_17 = (_SmoothnessMax).xxxx;
				float4 temp_output_216_0_g44330 = (temp_cast_16 + (MASK_G158_g44312 - float4( 0,0,0,0 )) * (temp_cast_17 - temp_cast_16) / (float4( 1,1,1,1 ) - float4( 0,0,0,0 )));
				float4 m_Smoothness215_g44330 = temp_output_216_0_g44330;
				float4 temp_cast_18 = (_SmoothnessMin).xxxx;
				float4 temp_cast_19 = (_SmoothnessMax).xxxx;
				float4 temp_output_214_0_g44330 = ( 1.0 - temp_output_216_0_g44330 );
				float4 m_Roughness215_g44330 = temp_output_214_0_g44330;
				float4 temp_cast_20 = (_SmoothnessMin).xxxx;
				float4 temp_cast_21 = (_SmoothnessMax).xxxx;
				float3 tanNormal1608_g44312 = NORMAL_OUT1491_g44312;
				float3 worldNormal1608_g44312 = normalize( float3(dot(tanToWorld0,tanNormal1608_g44312), dot(tanToWorld1,tanNormal1608_g44312), dot(tanToWorld2,tanNormal1608_g44312)) );
				float3 temp_output_4_0_g44330 = worldNormal1608_g44312;
				float3 temp_output_178_0_g44330 = ddx( temp_output_4_0_g44330 );
				float dotResult195_g44330 = dot( temp_output_178_0_g44330 , temp_output_178_0_g44330 );
				float3 temp_output_175_0_g44330 = ddy( temp_output_4_0_g44330 );
				float dotResult201_g44330 = dot( temp_output_175_0_g44330 , temp_output_175_0_g44330 );
				float ALBEDO_R172_g44312 = tex2DNode63_g44324.r;
				float4 temp_cast_22 = (ALBEDO_R172_g44312).xxxx;
				float4 m_Geometric215_g44330 = ( sqrt( saturate( ( temp_output_216_0_g44330 + ( ( dotResult195_g44330 + dotResult201_g44330 ) * 2.0 ) ) ) ) * ( 1.0 - temp_cast_22 ) );
				float4 localSmoothnessTypefloat4switch215_g44330 = SmoothnessTypefloat4switch215_g44330( m_switch215_g44330 , m_Smoothness215_g44330 , m_Roughness215_g44330 , m_Geometric215_g44330 );
				float4 temp_output_1635_33_g44312 = localSmoothnessTypefloat4switch215_g44330;
				float4 PBR_Occlusion1712_g44312 = SAMPLE_TEXTURE2D( _OcclusionMap, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float temp_output_16_0_g44316 = _OcclusionStrengthAO;
				float temp_output_65_0_g44316 = ( 1.0 - temp_output_16_0_g44316 );
				float3 appendResult69_g44316 = (float3(temp_output_65_0_g44316 , temp_output_65_0_g44316 , temp_output_65_0_g44316));
				float4 color77_g44316 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 temp_cast_25 = (IN.ase_color.a).xxxx;
				float4 lerpResult75_g44316 = lerp( color77_g44316 , temp_cast_25 , temp_output_16_0_g44316);
				float4 lerpResult83_g44316 = lerp( float4( ( ( ( (PBR_Occlusion1712_g44312).xyz - float3( 0.5,0.5,0.5 ) ) * temp_output_16_0_g44316 ) + appendResult69_g44316 ) , 0.0 ) , lerpResult75_g44316 , _OcclusionSource);
				float4 Occlusion1628_g44312 = saturate( lerpResult83_g44316 );
				
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( WorldPosition ) , ddy( WorldPosition ) ) );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , WorldViewDirection );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				float m_switch103_g44322 = _TranslucencySourceFoward;
				float4 color71_g44322 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				float4 temp_output_113_0_g44322 = saturate( ( SAMPLE_TEXTURE2D( _TranslucencyMap, sampler_trilinear_repeat_aniso2, OUT_UV213_g44312 ) / max( _TranslucencyThicknessFeather , 1E-05 ) ) );
				float4 lerpResult34_g44322 = lerp( color71_g44322 , temp_output_113_0_g44322 , _TranslucencyThickness);
				float4 m_Texture103_g44322 = lerpResult34_g44322;
				float temp_output_69_0_g44322 = ( 0.0 - 1.0 );
				float temp_output_22_0_g44322 = ( temp_output_113_0_g44322.r - 1.0 );
				float lerpResult66_g44322 = lerp( ( temp_output_69_0_g44322 / temp_output_22_0_g44322 ) , ( temp_output_22_0_g44322 / temp_output_69_0_g44322 ) , ( 0.7 + _TranslucencyThickness ));
				float4 temp_cast_30 = (saturate( lerpResult66_g44322 )).xxxx;
				float4 m_TextureInverted103_g44322 = temp_cast_30;
				float lerpResult30_g44322 = lerp( 0.0 , ( 5.0 - IN.ase_texcoord8.zw.x ) , _TranslucencyThickness);
				float4 temp_cast_31 = (lerpResult30_g44322).xxxx;
				float4 m_Baked103_g44322 = temp_cast_31;
				float4 localfloat4switch103_g44322 = float4switch103_g44322( m_switch103_g44322 , m_Texture103_g44322 , m_TextureInverted103_g44322 , m_Baked103_g44322 );
				float3 temp_output_90_0_g44322 = saturate( ( _MainLightColor.rgb * _MainLightColor.a ) );
				float4 lerpResult122_g44322 = lerp( float4( 0,0,0,0 ) , ( ( _TransmissionTint * _TransmissionTint.a * localfloat4switch103_g44322 ) * float4( temp_output_90_0_g44322 , 0.0 ) ) , _EnableTranslucency);
				
				float4 lerpResult123_g44322 = lerp( float4( 0,0,0,0 ) , ( ( _TranslucencyTint * _TranslucencyTint.a * localfloat4switch103_g44322 ) * float4( temp_output_90_0_g44322 , 0.0 ) ) , _EnableTranslucency);
				
				float3 Albedo = localColorShift_Modefloat4switch364_g44339.xyz;
				float3 Normal = ( switchResult58_g44318 + ADDITIONAL_LIGHT1212_g44312 );
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( MASK_B1377_g44312.r * temp_output_1_0_g44334 );
				float Smoothness = ( temp_output_1635_33_g44312 * Occlusion1628_g44312 ).x;
				float Occlusion = saturate( lerpResult83_g44316 ).r;
				float Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				float AlphaClipThreshold = AlphaCutoffBias33_g44319;
				float AlphaClipThresholdShadow = _AlphaCutoffBiasShadow;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = lerpResult122_g44322.rgb;
				float3 Translucency = lerpResult123_g44322.rgb;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.positionCS = IN.clipPos;
				inputData.shadowCoord = ShadowCoords;



				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
				#else
					inputData.normalWS = WorldNormal;
				#endif
					
				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = SafeNormalize( WorldViewDirection );



				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(WorldPosition, 1.0),  IN.fogFactorAndVertexLight.x);
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				

				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#else
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, IN.dynamicLightmapUV.xy, SH, inputData.normalWS);
					#else
						inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
					#endif
				#endif

				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				#if defined(DEBUG_DISPLAY)
					#if defined(DYNAMICLIGHTMAP_ON)
						inputData.dynamicLightmapUV = IN.dynamicLightmapUV.xy;
						#endif
					#if defined(LIGHTMAP_ON)
						inputData.staticLightmapUV = IN.lightmapUVOrVertexSH.xy;
					#else
						inputData.vertexSH = SH;
					#endif
				#endif

				#ifdef _DBUFFER
					ApplyDecal(IN.clipPos,
						Albedo,
						Specular,
						inputData.normalWS,
						Metallic,
						Occlusion,
						Smoothness);
				#endif

				BRDFData brdfData;
				InitializeBRDFData
				(Albedo, Metallic, Specular, Smoothness, Alpha, brdfData);

				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
				half4 color;
				MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
				color.rgb = GlobalIllumination(brdfData, inputData.bakedGI, Occlusion, inputData.positionWS, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;
				
				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif
				
				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb);
			}

			ENDHLSL
		}

		
        Pass
        {
			
            Name "SceneSelectionPass"
            Tags { "LightMode"="SceneSelectionPass" }
        
			Cull Off

			HLSLPROGRAM
        
			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1

        
			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			

			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord1.xyz = ase_worldPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif
			
			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( ase_worldPos ) , ddy( ase_worldPos ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				surfaceDescription.Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				surfaceDescription.AlphaClipThreshold = AlphaCutoffBias33_g44319;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}

			ENDHLSL
        }

		
        Pass
        {
			
            Name "ScenePickingPass"
            Tags { "LightMode"="Picking" }
        
			HLSLPROGRAM

			#define _NORMAL_DROPOFF_TS 1
			#define _TRANSMISSION_ASE 1
			#define _TRANSLUCENCY_ASE 1
			#define _ALPHATEST_SHADOW_ON 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 999999
			#define ASE_USING_SAMPLING_MACROS 1


			#pragma only_renderers d3d11 glcore gles gles3 
			#pragma vertex vert
			#pragma fragment frag

        
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY
			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#pragma instancing_options procedural:SetupNatureRenderer forwardadd
			#pragma multi_compile GPU_FRUSTUM_ON __
			#include "Nature Renderer.cginc"
			#pragma multi_compile_local _ NATURE_RENDERER
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _CLUSTERED_RENDERING
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY
			#pragma instancing_options renderinglayer


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _TranslucencyTint;
			float4 _Color;
			half4 _TransmissionTint;
			float4 _Smoothness;
			float4 _ColorShift_MaskMap_ST;
			half _ColorShift_MaskMode;
			float _ColorShift_NoiseScale;
			float _ColorShift_WorldSpaceNoiseShift;
			float _ColorShift_WorldSpaceOffset;
			float _ColorShift_WorldSpaceDistance;
			float _ColorShift_ShiftVariation;
			float _ColorShift_ShiftVariationRGB;
			float _ColorShift_ShiftSaturation;
			float _ColorShift_MaskFuzziness;
			float _ColorShift_ShiftInfluence;
			half _NormalStrength;
			half _NormalMode;
			float _MetallicStrength;
			half _SmoothnessSource;
			float _OcclusionStrengthAO;
			float _OcclusionSource;
			float _GlancingClipMode;
			half _AlphaCutoffBias;
			half _AlphaCutoffBiasShadow;
			half _TranslucencySourceFoward;
			float _TranslucencyThicknessFeather;
			float _TranslucencyThickness;
			half _Brightness;
			float _OffsetY;
			float _OffsetX;
			float _TilingY;
			int _ZWriteMode;
			float _URP_TranslucencyShadow;
			float _URP_TranslucencyDirect;
			float _AlphatoCoverage;
			float _URP_TranslucencyScattering;
			int _ColorMask;
			float _URP_TranslucencyStrength;
			float _URP_TransmissionShadow;
			float _SmoothnessMin;
			float _SmoothnessMax;
			int _RenderFace;
			float _EnableTranslucency;
			float _URP_TranslucencyAmbient;
			half _WindMode;
			half _WindType;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _GlobalTurbulenceInfluence;
			float _LocalWindTurbulence;
			half _ColorShift_Mode;
			float _TilingX;
			half _EmissionFlags;
			float _URP_TranslucencyNormalDistortion;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			int _Global_Wind_Main_Fade_Enabled;
			float _Global_Wind_Main_Intensity;
			float _Global_Wind_Main_RandomOffset;
			float _Global_Wind_Main_Pulse;
			float _Global_Wind_Main_Direction;
			float _Global_Wind_Main_Turbulence;
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);


			float4 mod289( float4 x )
			{
				return x - floor(x * (1.0 / 289.0)) * 289.0;
			}
			
			float4 perm( float4 x )
			{
				return mod289(((x * 34.0) + 1.0) * x);
			}
			
			float SimpleNoise3D( float3 p )
			{
				 float3 a = floor(p);
				    float3 d = p - a;
				    d = d * d * (3.0 - 2.0 * d);
				 float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
				    float4 k1 = perm(b.xyxy);
				 float4 k2 = perm(k1.xyxy + b.zzww);
				    float4 c = k2 + a.zzzz;
				    float4 k3 = perm(c);
				    float4 k4 = perm(c + 1.0);
				    float4 o1 = frac(k3 * (1.0 / 41.0));
				 float4 o2 = frac(k4 * (1.0 / 41.0));
				    float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
				    float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
				    return o4.y * d.y + o4.x * (1.0 - d.y);
			}
			
			float Wind_Globalfloatswitch( int m_switch, float m_Off, float m_Global, float m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float(0);
			}
			
			float2 DirectionalEquation( float _WindDirection )
			{
				float d = _WindDirection * 0.0174532924;
				float xL = cos(d) + 1 / 2;
				float zL = sin(d) + 1 / 2;
				return float2(zL,xL);
			}
			
			float3 Wind_Typefloat3switch2439_g43623( float m_switch, float3 m_Leaf, float3 m_Palm, float3 m_Grass, float3 m_Simple, float3 m_Ivy )
			{
				if(m_switch ==0)
					return m_Leaf;
				else if(m_switch ==1)
					return m_Palm;
				else if(m_switch ==2)
					return m_Grass;
				else if(m_switch ==3)
					return m_Simple;
				else if(m_switch ==4)
					return m_Ivy;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Globalfloat3switch( float m_switch, float3 m_Off, float3 m_Global, float3 m_Local )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Global;
				else if(m_switch ==2)
					return m_Local;
				else
				return float3(0,0,0);
			}
			
			float3 Wind_Fadefloat3switch3050_g43623( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_ActiveFadeOut;
				else if(m_switch ==2)
					return m_ActiveFadeIn;
				else
				return float3(0,0,0);
			}
			

        
			float4 _SelectionID;

        
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};
        
			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				int m_switch3050_g43623 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g43623 = _WindMode;
				float3 m_Off2453_g43623 = float3(0,0,0);
				float m_switch2439_g43623 = _WindType;
				float3 VERTEX_POSITION_MATRIX2352_g43623 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float WIND_MODE2462_g43623 = _WindMode;
				int m_switch2458_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2458_g43623 = 1.0;
				float m_Global2458_g43623 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g43623 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g43623 = Wind_Globalfloatswitch( m_switch2458_g43623 , m_Off2458_g43623 , m_Global2458_g43623 , m_Local2458_g43623 );
				float _WIND_STRENGHT2400_g43623 = localWind_Globalfloatswitch2458_g43623;
				int m_switch2468_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2468_g43623 = 1.0;
				float m_Global2468_g43623 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g43623 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g43623 = Wind_Globalfloatswitch( m_switch2468_g43623 , m_Off2468_g43623 , m_Global2468_g43623 , m_Local2468_g43623 );
				float4 transform3073_g43623 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g43623 = (float2(transform3073_g43623.x , transform3073_g43623.z));
				float dotResult2341_g43623 = dot( appendResult2307_g43623 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g43623 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g43623 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g43623 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g43623 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g43623 );
				float _WIND_TUBULENCE_RANDOM2274_g43623 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2312_g43623 = 1.0;
				float m_Global2312_g43623 = _Global_Wind_Main_Pulse;
				float m_Local2312_g43623 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g43623 = Wind_Globalfloatswitch( m_switch2312_g43623 , m_Off2312_g43623 , m_Global2312_g43623 , m_Local2312_g43623 );
				float _WIND_PULSE2421_g43623 = localWind_Globalfloatswitch2312_g43623;
				float FUNC_Angle2470_g43623 = ( _WIND_STRENGHT2400_g43623 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 ) - ( VERTEX_POSITION_MATRIX2352_g43623.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g43623 );
				float FUNC_Angle_SinA2424_g43623 = sin( FUNC_Angle2470_g43623 );
				float FUNC_Angle_CosA2362_g43623 = cos( FUNC_Angle2470_g43623 );
				int m_switch2456_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2456_g43623 = 1.0;
				float m_Global2456_g43623 = _Global_Wind_Main_Direction;
				float m_Local2456_g43623 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g43623 = Wind_Globalfloatswitch( m_switch2456_g43623 , m_Off2456_g43623 , m_Global2456_g43623 , m_Local2456_g43623 );
				float _WindDirection2249_g43623 = localWind_Globalfloatswitch2456_g43623;
				float2 localDirectionalEquation2249_g43623 = DirectionalEquation( _WindDirection2249_g43623 );
				float2 break2469_g43623 = localDirectionalEquation2249_g43623;
				float _WIND_DIRECTION_X2418_g43623 = break2469_g43623.x;
				float lerpResult2258_g43623 = lerp( break2265_g43623.x , ( ( break2265_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2265_g43623.x * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_X2418_g43623);
				float3 break2340_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float3 break2233_g43623 = VERTEX_POSITION_MATRIX2352_g43623;
				float _WIND_DIRECTION_Y2416_g43623 = break2469_g43623.y;
				float lerpResult2275_g43623 = lerp( break2233_g43623.z , ( ( break2233_g43623.y * FUNC_Angle_SinA2424_g43623 ) + ( break2233_g43623.z * FUNC_Angle_CosA2362_g43623 ) ) , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2235_g43623 = (float3(lerpResult2258_g43623 , ( ( break2340_g43623.y * FUNC_Angle_CosA2362_g43623 ) - ( break2340_g43623.z * FUNC_Angle_SinA2424_g43623 ) ) , lerpResult2275_g43623));
				float3 VERTEX_POSITION2282_g43623 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g43623 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 break2518_g43623 = VERTEX_POSITION2282_g43623;
				half FUNC_SinFunction2336_g43623 = sin( ( ( _WIND_RANDOM_OFFSET2244_g43623 * 200.0 * ( 0.2 + v.ase_color.g ) ) + ( v.ase_color.g * 10.0 ) + _WIND_TUBULENCE_RANDOM2274_g43623 + ( VERTEX_POSITION_MATRIX2352_g43623.z / 2.0 ) ) );
				int m_switch2326_g43623 = (int)WIND_MODE2462_g43623;
				float m_Off2326_g43623 = 1.0;
				float m_Global2326_g43623 = ( _GlobalTurbulenceInfluence * _Global_Wind_Main_Turbulence );
				float m_Local2326_g43623 = _LocalWindTurbulence;
				float localWind_Globalfloatswitch2326_g43623 = Wind_Globalfloatswitch( m_switch2326_g43623 , m_Off2326_g43623 , m_Global2326_g43623 , m_Local2326_g43623 );
				float _WIND_TUBULENCE2442_g43623 = localWind_Globalfloatswitch2326_g43623;
				float3 appendResult2480_g43623 = (float3(break2518_g43623.x , ( break2518_g43623.y + ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) ) , break2518_g43623.z));
				float3 VERTEX_LEAF2396_g43623 = appendResult2480_g43623;
				float3 m_Leaf2439_g43623 = VERTEX_LEAF2396_g43623;
				float3 VERTEX_PALM2310_g43623 = ( ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) * _WIND_TUBULENCE2442_g43623 ) + VERTEX_POSITION2282_g43623 );
				float3 m_Palm2439_g43623 = VERTEX_PALM2310_g43623;
				float3 break2486_g43623 = VERTEX_POSITION2282_g43623;
				float temp_output_2514_0_g43623 = ( FUNC_SinFunction2336_g43623 * v.ase_color.b * ( FUNC_Angle2470_g43623 + ( _WIND_STRENGHT2400_g43623 / 200.0 ) ) );
				float lerpResult2482_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_X2418_g43623);
				float lerpResult2484_g43623 = lerp( 0.0 , temp_output_2514_0_g43623 , _WIND_DIRECTION_Y2416_g43623);
				float3 appendResult2489_g43623 = (float3(( break2486_g43623.x + lerpResult2482_g43623 ) , break2486_g43623.y , ( break2486_g43623.z + lerpResult2484_g43623 )));
				float3 VERTEX_GRASS2242_g43623 = appendResult2489_g43623;
				float3 m_Grass2439_g43623 = VERTEX_GRASS2242_g43623;
				float3 m_Simple2439_g43623 = VERTEX_POSITION2282_g43623;
				float clampResult2884_g43623 = clamp( ( _WIND_STRENGHT2400_g43623 - 0.95 ) , 0.0 , 1.0 );
				float3 break2708_g43623 = VERTEX_POSITION2282_g43623;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float3 break2718_g43623 = ase_worldPos;
				float temp_output_2690_0_g43623 = ( _WIND_RANDOM_OFFSET2244_g43623 * 25.0 );
				float clampResult2691_g43623 = clamp( 25.0 , 0.2 , 0.25 );
				float2 appendResult2694_g43623 = (float2(temp_output_2690_0_g43623 , ( temp_output_2690_0_g43623 / clampResult2691_g43623 )));
				float3 appendResult2706_g43623 = (float3(break2708_g43623.x , ( break2708_g43623.y + cos( ( ( ( break2718_g43623.x + break2718_g43623.y + break2718_g43623.z ) * 2.0 ) + appendResult2694_g43623 + FUNC_Angle2470_g43623 + _WIND_TUBULENCE2442_g43623 ) ) ).x , break2708_g43623.z));
				float3 temp_output_2613_0_g43623 = ( clampResult2884_g43623 * appendResult2706_g43623 );
				float3 VERTEX_IVY997_g43623 = ( ( ( cos( temp_output_2613_0_g43623 ) + ( -0.3193 * PI ) ) * v.ase_normal * 0.2 * ( FUNC_SinFunction2336_g43623 * v.ase_color.b ) ) + ( sin( temp_output_2613_0_g43623 ) * cross( v.ase_normal , v.ase_tangent.xyz ) * 0.2 ) );
				float3 m_Ivy2439_g43623 = VERTEX_IVY997_g43623;
				float3 localWind_Typefloat3switch2439_g43623 = Wind_Typefloat3switch2439_g43623( m_switch2439_g43623 , m_Leaf2439_g43623 , m_Palm2439_g43623 , m_Grass2439_g43623 , m_Simple2439_g43623 , m_Ivy2439_g43623 );
				float3 m_Global2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 m_Local2453_g43623 = localWind_Typefloat3switch2439_g43623;
				float3 localWind_Globalfloat3switch2453_g43623 = Wind_Globalfloat3switch( m_switch2453_g43623 , m_Off2453_g43623 , m_Global2453_g43623 , m_Local2453_g43623 );
				float3 m_Off3050_g43623 = localWind_Globalfloat3switch2453_g43623;
				float temp_output_3048_0_g43623 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * ( 1.0 - temp_output_3048_0_g43623 ) );
				float3 m_ActiveFadeIn3050_g43623 = ( localWind_Globalfloat3switch2453_g43623 * temp_output_3048_0_g43623 );
				float3 localWind_Fadefloat3switch3050_g43623 = Wind_Fadefloat3switch3050_g43623( m_switch3050_g43623 , m_Off3050_g43623 , m_ActiveFadeOut3050_g43623 , m_ActiveFadeIn3050_g43623 );
				float3 temp_output_1234_0_g44312 = localWind_Fadefloat3switch3050_g43623;
				
				o.ase_texcoord1.xyz = ase_worldPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_1234_0_g44312;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_color : COLOR;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_color = v.ase_color;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;
				float2 appendResult168_g44312 = (float2(_TilingX , _TilingY));
				float2 appendResult167_g44312 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g44338 = IN.ase_texcoord.xy * appendResult168_g44312 + appendResult167_g44312;
				float2 OUT_UV213_g44312 = texCoord2_g44338;
				float2 UV40_g44324 = OUT_UV213_g44312;
				float4 tex2DNode63_g44324 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g44324 );
				float ALBEDO_A926_g44312 = tex2DNode63_g44324.a;
				float Albedo_Alpha52_g44319 = ALBEDO_A926_g44312;
				float3 ase_worldPos = IN.ase_texcoord1.xyz;
				float3 normalizeResult38_g44319 = normalize( cross( ddx( ase_worldPos ) , ddy( ase_worldPos ) ) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult35_g44319 = dot( normalizeResult38_g44319 , ase_worldViewDir );
				float temp_output_32_0_g44319 = ( 1.0 - abs( dotResult35_g44319 ) );
				float lerpResult67_g44319 = lerp( 1.0 , ( 1.0 - ( temp_output_32_0_g44319 * temp_output_32_0_g44319 ) ) , _GlancingClipMode);
				float OpacityMask42_g44319 = lerpResult67_g44319;
				
				float AlphaCutoffBias33_g44319 = _AlphaCutoffBias;
				
				surfaceDescription.Alpha = ( Albedo_Alpha52_g44319 * OpacityMask42_g44319 );
				surfaceDescription.AlphaClipThreshold = AlphaCutoffBias33_g44319;


				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;
				
				return outColor;
			}
        
			ENDHLSL
        }
		
	}
	
	CustomEditor "DE_ShaderGUI"
	Fallback " "
	
}
/*ASEBEGIN
Version=18934
71;29.5;1654.5;850.5;4923.51;4120.406;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;2875;-3943.481,-3291.7;Inherit;False;292.6667;109;DESF Utility ASE Compile Shaders;1;2876;;0,0.2047877,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2861;-4580.668,-4032.225;Inherit;False;579.3186;363.0588;URP TRANSMISSION TRANSLUCENCY ;7;2863;2867;2862;2865;2864;2866;2868;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2886;-3611.722,-4008.646;Inherit;False;255.6026;356.7736;DEBUG SETTINGS ;4;2760;2758;202;2849;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;2885;-3945.823,-4010.749;Inherit;False;328.6918;357.9233;GLOBAL SETTINGS ;2;2889;2892;;0,0,0,1;0;0
Node;AmplifyShaderEditor.FunctionNode;2871;-4677.158,-3647.136;Inherit;False;DESF Module Wind;131;;43623;b135a554f7e4d0b41bba02c61b34ae74;10,938,0,2881,0,2371,1,2454,1,2433,1,2434,1,2432,1,2457,1,2878,0,2752,0;0;1;FLOAT3;2190
Node;AmplifyShaderEditor.FunctionNode;2889;-3922.26,-3892.406;Inherit;False;DESF GUI URP Surface 12x;4;;44343;5d06ad93833bfc946bb66c05fd580bef;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2868;-4563.05,-3829.425;Inherit;False;Property;_URP_TranslucencyAmbient;Translucency Ambient;129;0;Create;False;1;;0;0;True;0;False;0.8339342;0.8339342;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;2892;-3920.51,-3965.156;Inherit;False;Property;_RenderFace;Render Face;3;2;[Header];[Enum];Create;False;1;SURFACE OPTIONS;0;1;Front,2,Back,1,Both,0;True;0;False;2;1;False;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;2879;-4458.738,-3646.505;Inherit;False;DESF Core Cutout;9;;44312;e0cdd7758f4404849b063afff4596424;20,1217,2,442,2,1509,1,1749,1,1556,1,1557,1,1333,0,1618,0,1806,0,1663,0,1650,0,1647,0,96,2,1569,0,1368,0,1415,0,830,0,1564,0,1565,0,1399,0;1;1234;FLOAT3;0,0,0;False;12;FLOAT4;38;FLOAT3;35;FLOAT;37;FLOAT4;33;COLOR;34;FLOAT;46;FLOAT;814;FLOAT;1660;COLOR;656;COLOR;657;FLOAT4;655;FLOAT3;1235
Node;AmplifyShaderEditor.RangedFloatNode;2863;-4568.166,-3984.37;Inherit;False;Property;_URP_TransmissionShadow;Transmission Shadow;124;0;Create;False;1;;0;1;UnityEngine.Rendering.CompareFunction;True;1;Header(TRANSMISSION  URP);False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;2758;-3588.695,-3895.823;Inherit;False;Property;_ColorMask;Color Mask Mode;1;1;[Enum];Create;False;1;;0;1;None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15;True;0;False;15;15;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;2866;-4288.516,-3824.825;Inherit;False;Property;_URP_TranslucencyScattering;Translucency Scattering ;127;0;Create;False;1;;0;0;True;0;False;2;2;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2864;-4291.167,-3982.641;Inherit;False;Property;_URP_TranslucencyStrength;Translucency Strength;125;0;Create;False;1;;0;0;True;1;Header(TRANSLUCENCY URP);False;1;1;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2862;-4565.05,-3907.425;Inherit;False;Property;_URP_TranslucencyDirect;Translucency Direct ;128;0;Create;False;1;;0;0;True;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2867;-4563.45,-3753.025;Inherit;False;Property;_URP_TranslucencyShadow;Translucency Shadow ;130;0;Create;False;1;;0;0;True;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;2760;-3589.124,-3968.716;Inherit;False;Property;_ZWriteMode;ZWrite Mode;0;2;[Header];[Enum];Create;False;1;DEBUG SETTINGS;0;1;Off,0,On,1;True;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-3589.089,-3824.321;Inherit;False;Constant;_MaskClipValue;Mask Clip Value;64;1;[HideInInspector];Create;True;1;;0;0;True;0;False;0.5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2865;-4290.048,-3902.427;Inherit;False;Property;_URP_TranslucencyNormalDistortion;Translucency Normal Distortion ;126;0;Create;False;1;;0;0;True;0;False;0.2735869;0.2735869;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;2876;-3934.481,-3255.7;Inherit;False;DESF Utility ASE Compile Shaders;-1;;44311;b85b01c42ba8a8a448b731b68fc0dbd9;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2849;-3589.183,-3754.175;Inherit;False;Property;_AlphatoCoverage;Alpha to Coverage;2;1;[Enum];Create;False;1;;1;Option1;0;1;Off,0,On,1;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2870;-3953.374,-3640.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;2758;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2843;-3941.374,-3630.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2842;-3941.374,-3630.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2891;-3953.374,-3560.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2841;-3953.374,-3640.745;Float;False;True;-1;2;DE_ShaderGUI;0;2;DEC/Cutout Wind/Cutout Wind Translucency;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;True;2849;True;True;2;True;2892;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=-10;NatureRendererInstancing=True;True;3;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;2758;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;2;LightMode=UniversalForward;NatureRendererInstancing=True;False;False;5;Include;;False;;Native;Pragma;instancing_options procedural:SetupNatureRenderer forwardadd;False;;Custom;Pragma;multi_compile GPU_FRUSTUM_ON __;False;;Custom;Include;Nature Renderer.cginc;False;ed9205546b797304ea7576ba0b32877e;Custom;Pragma;multi_compile_local _ NATURE_RENDERER;False;;Custom; ;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;0;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;1;0;  Transmission Shadow;0.5,True,2863;0;Translucency;1;0;  Translucency Strength;1,True,2864;0;  Normal Distortion;0.5,True,2865;0;  Scattering;2,True,2866;0;  Direct;0.9,True,2862;0;  Ambient;0.1,True,2868;0;  Shadow;0.5,True,2863;0;Cast Shadows;1;0;  Use Shadow Threshold;1;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;True;True;True;True;True;False;;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2844;-3941.374,-3630.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2890;-3953.374,-3560.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2840;-3601.374,-3640.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2869;-3953.374,-3580.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2845;-3941.374,-3630.745;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;2758;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;2879;1234;2871;2190
WireConnection;2841;0;2879;38
WireConnection;2841;1;2879;35
WireConnection;2841;3;2879;37
WireConnection;2841;4;2879;33
WireConnection;2841;5;2879;34
WireConnection;2841;6;2879;46
WireConnection;2841;7;2879;814
WireConnection;2841;16;2879;1660
WireConnection;2841;14;2879;656
WireConnection;2841;15;2879;657
WireConnection;2841;8;2879;1235
ASEEND*/
//CHKSM=FD6B96DE3B6377397BDDC3985FEE94E2FFC8EAA0