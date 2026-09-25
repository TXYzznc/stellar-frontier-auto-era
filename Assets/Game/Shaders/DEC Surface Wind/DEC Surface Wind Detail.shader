// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DEC/Surface Wind/Surface Wind Detail"
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
		[Header(MAIN MAPS)]_Color("Tint", Color) = (1,1,1,0)
		[DE_DrawerTextureSingleLine]_MainTex("Albedo Map", 2D) = "white" {}
		_Brightness("Brightness", Range( 0 , 2)) = 1
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
		[Header(DETAIL)][DE_DrawerToggleNoKeyword]_EnableDetailMap("Enable", Float) = 0
		_ColorDetail("Tint", Color) = (1,1,1,0)
		[DE_DrawerTextureSingleLine]_DetailAlbedoMap("Albedo Map", 2D) = "white" {}
		_DetailTilingXDetail("Tiling X", Float) = 1
		_DetailTilingYDetail("Tiling Y", Float) = 1
		_DetailOffsetXDetail("Offset X", Float) = 0
		_DetailOffsetYDetail("Offset Y", Float) = 0
		[Normal][DE_DrawerTextureSingleLine]_DetailNormalMap("Normal Map", 2D) = "bump" {}
		_DetailNormalMapScale("Normal Strength", Float) = 1
		_DetailBlendInfluence("Blend Influence", Range( 0 , 3)) = 0
		[Enum(Red,0,Green,1,Blue,2)]_BlendColor("Blend Vertex Color", Int) = 0
		_BlendHeight("Blend Height", Range( 0 , 1.25)) = 1
		_DetailBlendSmooth("Blend Smooth", Range( 0.01 , 0.5)) = 0.35
		[Header(DETAIL MASK)][DE_DrawerFloatEnum(Off _Enable _Enable Inverted)]_EnableDetailMask("Enable Detail Mask", Float) = 0
		[DE_DrawerTextureSingleLine]_DetailMaskMap("Mask Map", 2D) = "white" {}
		_Detail_BlendAmountMask("Blend Amount", Range( 0.001 , 1)) = 0.5
		_Detail_BlendHardnessMask("Blend Hardness", Range( 0.001 , 5)) = 1
		_Detail_BlendFalloffMask("Blend Falloff", Range( 0.001 , 0.999)) = 0.5
		_DetailTilingXDetailMask("Tiling X", Float) = 1
		_DetailTilingYDetailMask("Tiling Y", Float) = 1
		_DetailOffsetXDetailMask("Offset X", Float) = 0
		_DetailOffsetYDetailMask("Offset Y", Float) = 0
		[Header(WIND)][DE_DrawerFloatEnum(Off _Global _Local)]_WindMode("Enable Wind Mode", Float) = 0
		[Header(WIND MODE GLOBAL)]_GlobalWindInfluenceOther("Main", Float) = 1
		[Header(WIND MODE LOCAL)]_LocalWindStrength("Main", Float) = 1
		_LocalWindPulse("Pulse Frequency", Float) = 0.1
		_LocalWindDirection("Direction", Range( 0 , 360)) = 0
		[ASEEnd]_LocalRandomWindOffset("Random Offset", Float) = 0.2

		[HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
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
		#pragma target 3.0

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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_DetailAlbedoMap);
			TEXTURE2D(_DetailMaskMap);
			TEXTURE2D(_BumpMap);
			TEXTURE2D(_DetailNormalMap);
			TEXTURE2D(_MetallicGlossMap);
			TEXTURE2D(_SmoothnessMap);
			TEXTURE2D(_OcclusionMap);


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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
			
			float4 Detail_Maskfloat4switch226_g43781( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
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
			
			float Detail_BlendVCfloatswitch319_g43781( int m_switch, float m_Red, float m_Green, float m_Blue )
			{
				if(m_switch ==0)
					return m_Red;
				else if(m_switch ==1)
					return m_Green;
				else if(m_switch ==2)
					return m_Blue;
				else
				return float(0);
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
			
			float3 Detail_Maskfloat3switch221_g43781( float m_switch, float3 m_Off, float3 m_Active, float3 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float3(0,0,0);
			}
			
			float4 SmoothnessTypefloat4switch215_g43773( float m_switch, float4 m_Smoothness, float4 m_Roughness, float4 m_Geometric )
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
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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
						 ) : SV_Target
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

				float2 appendResult150_g43762 = (float2(_TilingX , _TilingY));
				float2 appendResult151_g43762 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g43772 = IN.ase_texcoord8.xy * appendResult150_g43762 + appendResult151_g43762;
				float2 OUT_UV431_g43762 = texCoord2_g43772;
				float2 UV40_g43770 = OUT_UV431_g43762;
				float4 tex2DNode63_g43770 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 OUT_ALBEDO_RGBA1177_g43762 = tex2DNode63_g43770;
				float3 temp_output_7_0_g43762 = ( (_Color).rgb * (OUT_ALBEDO_RGBA1177_g43762).rgb * _Brightness );
				float4 temp_output_39_0_g43781 = float4( temp_output_7_0_g43762 , 0.0 );
				float4 Albedo_RGBA40_g43781 = temp_output_39_0_g43781;
				float m_switch226_g43781 = _EnableDetailMask;
				float2 appendResult132_g43781 = (float2(_DetailTilingXDetail , _DetailTilingYDetail));
				float2 appendResult114_g43781 = (float2(_DetailOffsetXDetail , _DetailOffsetYDetail));
				float2 texCoord67_g43781 = IN.ase_texcoord8.xy * appendResult132_g43781 + appendResult114_g43781;
				float4 tex2DNode45_g43781 = SAMPLE_TEXTURE2D( _DetailAlbedoMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float4 ALBEDO_OUT255_g43781 = ( _ColorDetail * tex2DNode45_g43781 * _Brightness );
				float4 m_Off226_g43781 = ALBEDO_OUT255_g43781;
				float2 appendResult219_g43781 = (float2(_DetailTilingXDetailMask , _DetailTilingYDetailMask));
				float2 appendResult206_g43781 = (float2(_DetailOffsetXDetailMask , _DetailOffsetYDetailMask));
				float2 texCoord220_g43781 = IN.ase_texcoord8.xy * appendResult219_g43781 + appendResult206_g43781;
				float temp_output_15_0_g43782 = ( 1.0 - SAMPLE_TEXTURE2D( _DetailMaskMap, sampler_trilinear_repeat_aniso2, texCoord220_g43781 ).r );
				float temp_output_26_0_g43782 = _Detail_BlendAmountMask;
				float temp_output_24_0_g43782 = _Detail_BlendHardnessMask;
				float saferPower2_g43782 = abs( max( saturate( (0.0 + (temp_output_15_0_g43782 - ( 1.0 - temp_output_26_0_g43782 )) * (temp_output_24_0_g43782 - 0.0) / (1.0 - ( 1.0 - temp_output_26_0_g43782 ))) ) , 0.0 ) );
				float temp_output_22_0_g43782 = _Detail_BlendFalloffMask;
				float temp_output_403_0_g43781 = saturate( pow( saferPower2_g43782 , ( 1.0 - temp_output_22_0_g43782 ) ) );
				float4 lerpResult225_g43781 = lerp( Albedo_RGBA40_g43781 , ALBEDO_OUT255_g43781 , temp_output_403_0_g43781);
				float4 m_Active226_g43781 = lerpResult225_g43781;
				float saferPower11_g43782 = abs( max( saturate( (1.0 + (temp_output_15_0_g43782 - temp_output_26_0_g43782) * (( 1.0 - temp_output_24_0_g43782 ) - 1.0) / (0.0 - temp_output_26_0_g43782)) ) , 0.0 ) );
				float temp_output_403_21_g43781 = saturate( pow( saferPower11_g43782 , temp_output_22_0_g43782 ) );
				float4 lerpResult408_g43781 = lerp( ALBEDO_OUT255_g43781 , Albedo_RGBA40_g43781 , temp_output_403_21_g43781);
				float4 m_ActiveInverted226_g43781 = lerpResult408_g43781;
				float4 localDetail_Maskfloat4switch226_g43781 = Detail_Maskfloat4switch226_g43781( m_switch226_g43781 , m_Off226_g43781 , m_Active226_g43781 , m_ActiveInverted226_g43781 );
				float4 Mask_Albedo258_g43781 = localDetail_Maskfloat4switch226_g43781;
				float4 break48_g43781 = temp_output_39_0_g43781;
				float Albedo_RGB300_g43781 = ( break48_g43781.x + break48_g43781.y + break48_g43781.z );
				int m_switch319_g43781 = _BlendColor;
				float m_Red319_g43781 = IN.ase_color.r;
				float m_Green319_g43781 = IN.ase_color.g;
				float m_Blue319_g43781 = IN.ase_color.b;
				float localDetail_BlendVCfloatswitch319_g43781 = Detail_BlendVCfloatswitch319_g43781( m_switch319_g43781 , m_Red319_g43781 , m_Green319_g43781 , m_Blue319_g43781 );
				float clampResult47_g43781 = clamp( ( ( ( ( Albedo_RGB300_g43781 - 0.5 ) * ( _DetailBlendInfluence - 0.9 ) ) + ( localDetail_BlendVCfloatswitch319_g43781 - ( _BlendHeight - 0.4 ) ) ) / _DetailBlendSmooth ) , 0.0 , 1.0 );
				float DetailBlend43_g43781 = clampResult47_g43781;
				float4 lerpResult58_g43781 = lerp( Mask_Albedo258_g43781 , Albedo_RGBA40_g43781 , DetailBlend43_g43781);
				float4 lerpResult409_g43781 = lerp( Albedo_RGBA40_g43781 , lerpResult58_g43781 , _EnableDetailMap);
				
				float4 OUT_NORMAL1178_g43762 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 temp_output_1_0_g43778 = OUT_NORMAL1178_g43762;
				float temp_output_8_0_g43778 = _NormalStrength;
				float3 unpack52_g43778 = UnpackNormalScale( temp_output_1_0_g43778, temp_output_8_0_g43778 );
				unpack52_g43778.z = lerp( 1, unpack52_g43778.z, saturate(temp_output_8_0_g43778) );
				float3 temp_output_1478_59_g43762 = unpack52_g43778;
				float3 NORMAL_OUT314_g43762 = temp_output_1478_59_g43762;
				float3 temp_output_16_0_g43771 = NORMAL_OUT314_g43762;
				float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				float3 tanNormal134_g43771 = temp_output_16_0_g43771;
				float3 worldNormal134_g43771 = float3(dot(tanToWorld0,tanNormal134_g43771), dot(tanToWorld1,tanNormal134_g43771), dot(tanToWorld2,tanNormal134_g43771));
				float3 NORMAL_TANGENTSPACE135_g43771 = worldNormal134_g43771;
				float3 WorldPosition63_g43771 = NORMAL_TANGENTSPACE135_g43771;
				float4 shadowMaskValue77_g43771 = float4(1,1,1,1);
				float4 ShadowMask63_g43771 = shadowMaskValue77_g43771;
				float3 localAdditionalLightsFlatMask63_g43771 = AdditionalLightsFlatMask( WorldPosition63_g43771 , ShadowMask63_g43771 );
				float3 ADDITIONAL_LIGHT1342_g43762 = localAdditionalLightsFlatMask63_g43771;
				float3 temp_output_38_0_g43781 = ( temp_output_1478_59_g43762 + ADDITIONAL_LIGHT1342_g43762 );
				float3 Normal_XYZ260_g43781 = temp_output_38_0_g43781;
				float EnableDetailMask216_g43781 = _EnableDetailMask;
				float m_switch221_g43781 = EnableDetailMask216_g43781;
				float4 temp_output_1_0_g43784 = SAMPLE_TEXTURE2D( _DetailNormalMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float temp_output_8_0_g43784 = _DetailNormalMapScale;
				float3 unpack52_g43784 = UnpackNormalScale( temp_output_1_0_g43784, temp_output_8_0_g43784 );
				unpack52_g43784.z = lerp( 1, unpack52_g43784.z, saturate(temp_output_8_0_g43784) );
				float3 Detail_Normal199_g43781 = unpack52_g43784;
				float3 m_Off221_g43781 = Detail_Normal199_g43781;
				float3 lerpResult205_g43781 = lerp( Normal_XYZ260_g43781 , Detail_Normal199_g43781 , temp_output_403_0_g43781);
				float3 m_Active221_g43781 = saturate( lerpResult205_g43781 );
				float3 lerpResult406_g43781 = lerp( Detail_Normal199_g43781 , Normal_XYZ260_g43781 , temp_output_403_21_g43781);
				float3 m_ActiveInverted221_g43781 = saturate( lerpResult406_g43781 );
				float3 localDetail_Maskfloat3switch221_g43781 = Detail_Maskfloat3switch221_g43781( m_switch221_g43781 , m_Off221_g43781 , m_Active221_g43781 , m_ActiveInverted221_g43781 );
				float3 Mask_Normal222_g43781 = localDetail_Maskfloat3switch221_g43781;
				float layeredBlendVar413_g43781 = DetailBlend43_g43781;
				float3 layeredBlend413_g43781 = ( lerp( Mask_Normal222_g43781,Normal_XYZ260_g43781 , layeredBlendVar413_g43781 ) );
				float3 normalizeResult414_g43781 = normalize( layeredBlend413_g43781 );
				float3 temp_output_416_0_g43781 = BlendNormal( normalizeResult414_g43781 , Normal_XYZ260_g43781 );
				float EnebleMode122_g43781 = _EnableDetailMap;
				float3 lerpResult410_g43781 = lerp( Normal_XYZ260_g43781 , temp_output_416_0_g43781 , EnebleMode122_g43781);
				
				float4 _MASK_B1440_g43762 = SAMPLE_TEXTURE2D( _MetallicGlossMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float temp_output_1_0_g43768 = _MetallicStrength;
				
				float temp_output_223_0_g43773 = _SmoothnessSource;
				float m_switch215_g43773 = temp_output_223_0_g43773;
				float4 _MASK_G1438_g43762 = SAMPLE_TEXTURE2D( _SmoothnessMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 temp_cast_10 = (_SmoothnessMin).xxxx;
				float4 temp_cast_11 = (_SmoothnessMax).xxxx;
				float4 temp_output_216_0_g43773 = (temp_cast_10 + (_MASK_G1438_g43762 - float4( 0,0,0,0 )) * (temp_cast_11 - temp_cast_10) / (float4( 1,1,1,1 ) - float4( 0,0,0,0 )));
				float4 m_Smoothness215_g43773 = temp_output_216_0_g43773;
				float4 temp_cast_12 = (_SmoothnessMin).xxxx;
				float4 temp_cast_13 = (_SmoothnessMax).xxxx;
				float4 temp_output_214_0_g43773 = ( 1.0 - temp_output_216_0_g43773 );
				float4 m_Roughness215_g43773 = temp_output_214_0_g43773;
				float4 temp_cast_14 = (_SmoothnessMin).xxxx;
				float4 temp_cast_15 = (_SmoothnessMax).xxxx;
				float3 NORMAL_WORLD_OUT164_g43762 = temp_output_1478_59_g43762;
				float3 temp_output_4_0_g43773 = NORMAL_WORLD_OUT164_g43762;
				float3 temp_output_178_0_g43773 = ddx( temp_output_4_0_g43773 );
				float dotResult195_g43773 = dot( temp_output_178_0_g43773 , temp_output_178_0_g43773 );
				float3 temp_output_175_0_g43773 = ddy( temp_output_4_0_g43773 );
				float dotResult201_g43773 = dot( temp_output_175_0_g43773 , temp_output_175_0_g43773 );
				float4 break377_g43762 = OUT_ALBEDO_RGBA1177_g43762;
				float ALBEDO_R169_g43762 = break377_g43762.r;
				float4 temp_cast_16 = (ALBEDO_R169_g43762).xxxx;
				float4 m_Geometric215_g43773 = ( sqrt( saturate( ( temp_output_216_0_g43773 + ( ( dotResult195_g43773 + dotResult201_g43773 ) * 2.0 ) ) ) ) * ( 1.0 - temp_cast_16 ) );
				float4 localSmoothnessTypefloat4switch215_g43773 = SmoothnessTypefloat4switch215_g43773( m_switch215_g43773 , m_Smoothness215_g43773 , m_Roughness215_g43773 , m_Geometric215_g43773 );
				float4 temp_output_1556_33_g43762 = localSmoothnessTypefloat4switch215_g43773;
				float4 PBR_Occlusion1641_g43762 = SAMPLE_TEXTURE2D( _OcclusionMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float temp_output_16_0_g43767 = _OcclusionStrengthAO;
				float temp_output_65_0_g43767 = ( 1.0 - temp_output_16_0_g43767 );
				float3 appendResult69_g43767 = (float3(temp_output_65_0_g43767 , temp_output_65_0_g43767 , temp_output_65_0_g43767));
				float4 color77_g43767 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 temp_cast_19 = (IN.ase_color.a).xxxx;
				float4 lerpResult75_g43767 = lerp( color77_g43767 , temp_cast_19 , temp_output_16_0_g43767);
				float4 lerpResult83_g43767 = lerp( float4( ( ( ( (PBR_Occlusion1641_g43762).xyz - float3( 0.5,0.5,0.5 ) ) * temp_output_16_0_g43767 ) + appendResult69_g43767 ) , 0.0 ) , lerpResult75_g43767 , _OcclusionSource);
				float4 Occlusion1550_g43762 = saturate( lerpResult83_g43767 );
				
				float3 Albedo = lerpResult409_g43781.xyz;
				float3 Normal = lerpResult410_g43781;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( _MASK_B1440_g43762.r * temp_output_1_0_g43768 );
				float Smoothness = ( temp_output_1556_33_g43762 * Occlusion1550_g43762 ).x;
				float Occlusion = saturate( lerpResult83_g43767 ).r;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
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
					float shadow = _TransmissionShadow;

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
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
			#define ASE_NEEDS_FRAG_COLOR
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_DetailAlbedoMap);
			TEXTURE2D(_DetailMaskMap);


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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
			
			float4 Detail_Maskfloat4switch226_g43781( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
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
			
			float Detail_BlendVCfloatswitch319_g43781( int m_switch, float m_Red, float m_Green, float m_Blue )
			{
				if(m_switch ==0)
					return m_Red;
				else if(m_switch ==1)
					return m_Green;
				else if(m_switch ==2)
					return m_Blue;
				else
				return float(0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				o.ase_texcoord4.xy = v.texcoord0.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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

				float2 appendResult150_g43762 = (float2(_TilingX , _TilingY));
				float2 appendResult151_g43762 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g43772 = IN.ase_texcoord4.xy * appendResult150_g43762 + appendResult151_g43762;
				float2 OUT_UV431_g43762 = texCoord2_g43772;
				float2 UV40_g43770 = OUT_UV431_g43762;
				float4 tex2DNode63_g43770 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 OUT_ALBEDO_RGBA1177_g43762 = tex2DNode63_g43770;
				float3 temp_output_7_0_g43762 = ( (_Color).rgb * (OUT_ALBEDO_RGBA1177_g43762).rgb * _Brightness );
				float4 temp_output_39_0_g43781 = float4( temp_output_7_0_g43762 , 0.0 );
				float4 Albedo_RGBA40_g43781 = temp_output_39_0_g43781;
				float m_switch226_g43781 = _EnableDetailMask;
				float2 appendResult132_g43781 = (float2(_DetailTilingXDetail , _DetailTilingYDetail));
				float2 appendResult114_g43781 = (float2(_DetailOffsetXDetail , _DetailOffsetYDetail));
				float2 texCoord67_g43781 = IN.ase_texcoord4.xy * appendResult132_g43781 + appendResult114_g43781;
				float4 tex2DNode45_g43781 = SAMPLE_TEXTURE2D( _DetailAlbedoMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float4 ALBEDO_OUT255_g43781 = ( _ColorDetail * tex2DNode45_g43781 * _Brightness );
				float4 m_Off226_g43781 = ALBEDO_OUT255_g43781;
				float2 appendResult219_g43781 = (float2(_DetailTilingXDetailMask , _DetailTilingYDetailMask));
				float2 appendResult206_g43781 = (float2(_DetailOffsetXDetailMask , _DetailOffsetYDetailMask));
				float2 texCoord220_g43781 = IN.ase_texcoord4.xy * appendResult219_g43781 + appendResult206_g43781;
				float temp_output_15_0_g43782 = ( 1.0 - SAMPLE_TEXTURE2D( _DetailMaskMap, sampler_trilinear_repeat_aniso2, texCoord220_g43781 ).r );
				float temp_output_26_0_g43782 = _Detail_BlendAmountMask;
				float temp_output_24_0_g43782 = _Detail_BlendHardnessMask;
				float saferPower2_g43782 = abs( max( saturate( (0.0 + (temp_output_15_0_g43782 - ( 1.0 - temp_output_26_0_g43782 )) * (temp_output_24_0_g43782 - 0.0) / (1.0 - ( 1.0 - temp_output_26_0_g43782 ))) ) , 0.0 ) );
				float temp_output_22_0_g43782 = _Detail_BlendFalloffMask;
				float temp_output_403_0_g43781 = saturate( pow( saferPower2_g43782 , ( 1.0 - temp_output_22_0_g43782 ) ) );
				float4 lerpResult225_g43781 = lerp( Albedo_RGBA40_g43781 , ALBEDO_OUT255_g43781 , temp_output_403_0_g43781);
				float4 m_Active226_g43781 = lerpResult225_g43781;
				float saferPower11_g43782 = abs( max( saturate( (1.0 + (temp_output_15_0_g43782 - temp_output_26_0_g43782) * (( 1.0 - temp_output_24_0_g43782 ) - 1.0) / (0.0 - temp_output_26_0_g43782)) ) , 0.0 ) );
				float temp_output_403_21_g43781 = saturate( pow( saferPower11_g43782 , temp_output_22_0_g43782 ) );
				float4 lerpResult408_g43781 = lerp( ALBEDO_OUT255_g43781 , Albedo_RGBA40_g43781 , temp_output_403_21_g43781);
				float4 m_ActiveInverted226_g43781 = lerpResult408_g43781;
				float4 localDetail_Maskfloat4switch226_g43781 = Detail_Maskfloat4switch226_g43781( m_switch226_g43781 , m_Off226_g43781 , m_Active226_g43781 , m_ActiveInverted226_g43781 );
				float4 Mask_Albedo258_g43781 = localDetail_Maskfloat4switch226_g43781;
				float4 break48_g43781 = temp_output_39_0_g43781;
				float Albedo_RGB300_g43781 = ( break48_g43781.x + break48_g43781.y + break48_g43781.z );
				int m_switch319_g43781 = _BlendColor;
				float m_Red319_g43781 = IN.ase_color.r;
				float m_Green319_g43781 = IN.ase_color.g;
				float m_Blue319_g43781 = IN.ase_color.b;
				float localDetail_BlendVCfloatswitch319_g43781 = Detail_BlendVCfloatswitch319_g43781( m_switch319_g43781 , m_Red319_g43781 , m_Green319_g43781 , m_Blue319_g43781 );
				float clampResult47_g43781 = clamp( ( ( ( ( Albedo_RGB300_g43781 - 0.5 ) * ( _DetailBlendInfluence - 0.9 ) ) + ( localDetail_BlendVCfloatswitch319_g43781 - ( _BlendHeight - 0.4 ) ) ) / _DetailBlendSmooth ) , 0.0 , 1.0 );
				float DetailBlend43_g43781 = clampResult47_g43781;
				float4 lerpResult58_g43781 = lerp( Mask_Albedo258_g43781 , Albedo_RGBA40_g43781 , DetailBlend43_g43781);
				float4 lerpResult409_g43781 = lerp( Albedo_RGBA40_g43781 , lerpResult58_g43781 , _EnableDetailMap);
				
				
				float3 Albedo = lerpResult409_g43781.xyz;
				float3 Emission = 0;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
			#define ASE_NEEDS_FRAG_COLOR
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_DetailAlbedoMap);
			TEXTURE2D(_DetailMaskMap);


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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
			
			float4 Detail_Maskfloat4switch226_g43781( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
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
			
			float Detail_BlendVCfloatswitch319_g43781( int m_switch, float m_Red, float m_Green, float m_Blue )
			{
				if(m_switch ==0)
					return m_Red;
				else if(m_switch ==1)
					return m_Green;
				else if(m_switch ==2)
					return m_Blue;
				else
				return float(0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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

				float2 appendResult150_g43762 = (float2(_TilingX , _TilingY));
				float2 appendResult151_g43762 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g43772 = IN.ase_texcoord2.xy * appendResult150_g43762 + appendResult151_g43762;
				float2 OUT_UV431_g43762 = texCoord2_g43772;
				float2 UV40_g43770 = OUT_UV431_g43762;
				float4 tex2DNode63_g43770 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 OUT_ALBEDO_RGBA1177_g43762 = tex2DNode63_g43770;
				float3 temp_output_7_0_g43762 = ( (_Color).rgb * (OUT_ALBEDO_RGBA1177_g43762).rgb * _Brightness );
				float4 temp_output_39_0_g43781 = float4( temp_output_7_0_g43762 , 0.0 );
				float4 Albedo_RGBA40_g43781 = temp_output_39_0_g43781;
				float m_switch226_g43781 = _EnableDetailMask;
				float2 appendResult132_g43781 = (float2(_DetailTilingXDetail , _DetailTilingYDetail));
				float2 appendResult114_g43781 = (float2(_DetailOffsetXDetail , _DetailOffsetYDetail));
				float2 texCoord67_g43781 = IN.ase_texcoord2.xy * appendResult132_g43781 + appendResult114_g43781;
				float4 tex2DNode45_g43781 = SAMPLE_TEXTURE2D( _DetailAlbedoMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float4 ALBEDO_OUT255_g43781 = ( _ColorDetail * tex2DNode45_g43781 * _Brightness );
				float4 m_Off226_g43781 = ALBEDO_OUT255_g43781;
				float2 appendResult219_g43781 = (float2(_DetailTilingXDetailMask , _DetailTilingYDetailMask));
				float2 appendResult206_g43781 = (float2(_DetailOffsetXDetailMask , _DetailOffsetYDetailMask));
				float2 texCoord220_g43781 = IN.ase_texcoord2.xy * appendResult219_g43781 + appendResult206_g43781;
				float temp_output_15_0_g43782 = ( 1.0 - SAMPLE_TEXTURE2D( _DetailMaskMap, sampler_trilinear_repeat_aniso2, texCoord220_g43781 ).r );
				float temp_output_26_0_g43782 = _Detail_BlendAmountMask;
				float temp_output_24_0_g43782 = _Detail_BlendHardnessMask;
				float saferPower2_g43782 = abs( max( saturate( (0.0 + (temp_output_15_0_g43782 - ( 1.0 - temp_output_26_0_g43782 )) * (temp_output_24_0_g43782 - 0.0) / (1.0 - ( 1.0 - temp_output_26_0_g43782 ))) ) , 0.0 ) );
				float temp_output_22_0_g43782 = _Detail_BlendFalloffMask;
				float temp_output_403_0_g43781 = saturate( pow( saferPower2_g43782 , ( 1.0 - temp_output_22_0_g43782 ) ) );
				float4 lerpResult225_g43781 = lerp( Albedo_RGBA40_g43781 , ALBEDO_OUT255_g43781 , temp_output_403_0_g43781);
				float4 m_Active226_g43781 = lerpResult225_g43781;
				float saferPower11_g43782 = abs( max( saturate( (1.0 + (temp_output_15_0_g43782 - temp_output_26_0_g43782) * (( 1.0 - temp_output_24_0_g43782 ) - 1.0) / (0.0 - temp_output_26_0_g43782)) ) , 0.0 ) );
				float temp_output_403_21_g43781 = saturate( pow( saferPower11_g43782 , temp_output_22_0_g43782 ) );
				float4 lerpResult408_g43781 = lerp( ALBEDO_OUT255_g43781 , Albedo_RGBA40_g43781 , temp_output_403_21_g43781);
				float4 m_ActiveInverted226_g43781 = lerpResult408_g43781;
				float4 localDetail_Maskfloat4switch226_g43781 = Detail_Maskfloat4switch226_g43781( m_switch226_g43781 , m_Off226_g43781 , m_Active226_g43781 , m_ActiveInverted226_g43781 );
				float4 Mask_Albedo258_g43781 = localDetail_Maskfloat4switch226_g43781;
				float4 break48_g43781 = temp_output_39_0_g43781;
				float Albedo_RGB300_g43781 = ( break48_g43781.x + break48_g43781.y + break48_g43781.z );
				int m_switch319_g43781 = _BlendColor;
				float m_Red319_g43781 = IN.ase_color.r;
				float m_Green319_g43781 = IN.ase_color.g;
				float m_Blue319_g43781 = IN.ase_color.b;
				float localDetail_BlendVCfloatswitch319_g43781 = Detail_BlendVCfloatswitch319_g43781( m_switch319_g43781 , m_Red319_g43781 , m_Green319_g43781 , m_Blue319_g43781 );
				float clampResult47_g43781 = clamp( ( ( ( ( Albedo_RGB300_g43781 - 0.5 ) * ( _DetailBlendInfluence - 0.9 ) ) + ( localDetail_BlendVCfloatswitch319_g43781 - ( _BlendHeight - 0.4 ) ) ) / _DetailBlendSmooth ) , 0.0 , 1.0 );
				float DetailBlend43_g43781 = clampResult47_g43781;
				float4 lerpResult58_g43781 = lerp( Mask_Albedo258_g43781 , Albedo_RGBA40_g43781 , DetailBlend43_g43781);
				float4 lerpResult409_g43781 = lerp( Albedo_RGBA40_g43781 , lerpResult58_g43781 , _EnableDetailMap);
				
				
				float3 Albedo = lerpResult409_g43781.xyz;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_TANGENT
			#define ASE_NEEDS_FRAG_COLOR
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_BumpMap);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_MainTex);
			TEXTURE2D(_DetailNormalMap);
			TEXTURE2D(_DetailMaskMap);


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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
			
			float Detail_BlendVCfloatswitch319_g43781( int m_switch, float m_Red, float m_Green, float m_Blue )
			{
				if(m_switch ==0)
					return m_Red;
				else if(m_switch ==1)
					return m_Green;
				else if(m_switch ==2)
					return m_Blue;
				else
				return float(0);
			}
			
			float3 Detail_Maskfloat3switch221_g43781( float m_switch, float3 m_Off, float3 m_Active, float3 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float3(0,0,0);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				o.ase_texcoord5.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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
						 ) : SV_TARGET
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

				float2 appendResult150_g43762 = (float2(_TilingX , _TilingY));
				float2 appendResult151_g43762 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g43772 = IN.ase_texcoord4.xy * appendResult150_g43762 + appendResult151_g43762;
				float2 OUT_UV431_g43762 = texCoord2_g43772;
				float2 UV40_g43770 = OUT_UV431_g43762;
				float4 OUT_NORMAL1178_g43762 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 temp_output_1_0_g43778 = OUT_NORMAL1178_g43762;
				float temp_output_8_0_g43778 = _NormalStrength;
				float3 unpack52_g43778 = UnpackNormalScale( temp_output_1_0_g43778, temp_output_8_0_g43778 );
				unpack52_g43778.z = lerp( 1, unpack52_g43778.z, saturate(temp_output_8_0_g43778) );
				float3 temp_output_1478_59_g43762 = unpack52_g43778;
				float3 NORMAL_OUT314_g43762 = temp_output_1478_59_g43762;
				float3 temp_output_16_0_g43771 = NORMAL_OUT314_g43762;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3 tanToWorld0 = float3( WorldTangent.xyz.x, ase_worldBitangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.xyz.y, ase_worldBitangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.xyz.z, ase_worldBitangent.z, WorldNormal.z );
				float3 tanNormal134_g43771 = temp_output_16_0_g43771;
				float3 worldNormal134_g43771 = float3(dot(tanToWorld0,tanNormal134_g43771), dot(tanToWorld1,tanNormal134_g43771), dot(tanToWorld2,tanNormal134_g43771));
				float3 NORMAL_TANGENTSPACE135_g43771 = worldNormal134_g43771;
				float3 WorldPosition63_g43771 = NORMAL_TANGENTSPACE135_g43771;
				float4 shadowMaskValue77_g43771 = float4(1,1,1,1);
				float4 ShadowMask63_g43771 = shadowMaskValue77_g43771;
				float3 localAdditionalLightsFlatMask63_g43771 = AdditionalLightsFlatMask( WorldPosition63_g43771 , ShadowMask63_g43771 );
				float3 ADDITIONAL_LIGHT1342_g43762 = localAdditionalLightsFlatMask63_g43771;
				float3 temp_output_38_0_g43781 = ( temp_output_1478_59_g43762 + ADDITIONAL_LIGHT1342_g43762 );
				float3 Normal_XYZ260_g43781 = temp_output_38_0_g43781;
				float4 tex2DNode63_g43770 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 OUT_ALBEDO_RGBA1177_g43762 = tex2DNode63_g43770;
				float3 temp_output_7_0_g43762 = ( (_Color).rgb * (OUT_ALBEDO_RGBA1177_g43762).rgb * _Brightness );
				float4 temp_output_39_0_g43781 = float4( temp_output_7_0_g43762 , 0.0 );
				float4 break48_g43781 = temp_output_39_0_g43781;
				float Albedo_RGB300_g43781 = ( break48_g43781.x + break48_g43781.y + break48_g43781.z );
				int m_switch319_g43781 = _BlendColor;
				float m_Red319_g43781 = IN.ase_color.r;
				float m_Green319_g43781 = IN.ase_color.g;
				float m_Blue319_g43781 = IN.ase_color.b;
				float localDetail_BlendVCfloatswitch319_g43781 = Detail_BlendVCfloatswitch319_g43781( m_switch319_g43781 , m_Red319_g43781 , m_Green319_g43781 , m_Blue319_g43781 );
				float clampResult47_g43781 = clamp( ( ( ( ( Albedo_RGB300_g43781 - 0.5 ) * ( _DetailBlendInfluence - 0.9 ) ) + ( localDetail_BlendVCfloatswitch319_g43781 - ( _BlendHeight - 0.4 ) ) ) / _DetailBlendSmooth ) , 0.0 , 1.0 );
				float DetailBlend43_g43781 = clampResult47_g43781;
				float EnableDetailMask216_g43781 = _EnableDetailMask;
				float m_switch221_g43781 = EnableDetailMask216_g43781;
				float2 appendResult132_g43781 = (float2(_DetailTilingXDetail , _DetailTilingYDetail));
				float2 appendResult114_g43781 = (float2(_DetailOffsetXDetail , _DetailOffsetYDetail));
				float2 texCoord67_g43781 = IN.ase_texcoord4.xy * appendResult132_g43781 + appendResult114_g43781;
				float4 temp_output_1_0_g43784 = SAMPLE_TEXTURE2D( _DetailNormalMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float temp_output_8_0_g43784 = _DetailNormalMapScale;
				float3 unpack52_g43784 = UnpackNormalScale( temp_output_1_0_g43784, temp_output_8_0_g43784 );
				unpack52_g43784.z = lerp( 1, unpack52_g43784.z, saturate(temp_output_8_0_g43784) );
				float3 Detail_Normal199_g43781 = unpack52_g43784;
				float3 m_Off221_g43781 = Detail_Normal199_g43781;
				float2 appendResult219_g43781 = (float2(_DetailTilingXDetailMask , _DetailTilingYDetailMask));
				float2 appendResult206_g43781 = (float2(_DetailOffsetXDetailMask , _DetailOffsetYDetailMask));
				float2 texCoord220_g43781 = IN.ase_texcoord4.xy * appendResult219_g43781 + appendResult206_g43781;
				float temp_output_15_0_g43782 = ( 1.0 - SAMPLE_TEXTURE2D( _DetailMaskMap, sampler_trilinear_repeat_aniso2, texCoord220_g43781 ).r );
				float temp_output_26_0_g43782 = _Detail_BlendAmountMask;
				float temp_output_24_0_g43782 = _Detail_BlendHardnessMask;
				float saferPower2_g43782 = abs( max( saturate( (0.0 + (temp_output_15_0_g43782 - ( 1.0 - temp_output_26_0_g43782 )) * (temp_output_24_0_g43782 - 0.0) / (1.0 - ( 1.0 - temp_output_26_0_g43782 ))) ) , 0.0 ) );
				float temp_output_22_0_g43782 = _Detail_BlendFalloffMask;
				float temp_output_403_0_g43781 = saturate( pow( saferPower2_g43782 , ( 1.0 - temp_output_22_0_g43782 ) ) );
				float3 lerpResult205_g43781 = lerp( Normal_XYZ260_g43781 , Detail_Normal199_g43781 , temp_output_403_0_g43781);
				float3 m_Active221_g43781 = saturate( lerpResult205_g43781 );
				float saferPower11_g43782 = abs( max( saturate( (1.0 + (temp_output_15_0_g43782 - temp_output_26_0_g43782) * (( 1.0 - temp_output_24_0_g43782 ) - 1.0) / (0.0 - temp_output_26_0_g43782)) ) , 0.0 ) );
				float temp_output_403_21_g43781 = saturate( pow( saferPower11_g43782 , temp_output_22_0_g43782 ) );
				float3 lerpResult406_g43781 = lerp( Detail_Normal199_g43781 , Normal_XYZ260_g43781 , temp_output_403_21_g43781);
				float3 m_ActiveInverted221_g43781 = saturate( lerpResult406_g43781 );
				float3 localDetail_Maskfloat3switch221_g43781 = Detail_Maskfloat3switch221_g43781( m_switch221_g43781 , m_Off221_g43781 , m_Active221_g43781 , m_ActiveInverted221_g43781 );
				float3 Mask_Normal222_g43781 = localDetail_Maskfloat3switch221_g43781;
				float layeredBlendVar413_g43781 = DetailBlend43_g43781;
				float3 layeredBlend413_g43781 = ( lerp( Mask_Normal222_g43781,Normal_XYZ260_g43781 , layeredBlendVar413_g43781 ) );
				float3 normalizeResult414_g43781 = normalize( layeredBlend413_g43781 );
				float3 temp_output_416_0_g43781 = BlendNormal( normalizeResult414_g43781 , Normal_XYZ260_g43781 );
				float EnebleMode122_g43781 = _EnableDetailMap;
				float3 lerpResult410_g43781 = lerp( Normal_XYZ260_g43781 , temp_output_416_0_g43781 , EnebleMode122_g43781);
				
				float3 Normal = lerpResult410_g43781;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
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
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_trilinear_repeat_aniso2);
			TEXTURE2D(_DetailAlbedoMap);
			TEXTURE2D(_DetailMaskMap);
			TEXTURE2D(_BumpMap);
			TEXTURE2D(_DetailNormalMap);
			TEXTURE2D(_MetallicGlossMap);
			TEXTURE2D(_SmoothnessMap);
			TEXTURE2D(_OcclusionMap);


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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
			
			float4 Detail_Maskfloat4switch226_g43781( float m_switch, float4 m_Off, float4 m_Active, float4 m_ActiveInverted )
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
			
			float Detail_BlendVCfloatswitch319_g43781( int m_switch, float m_Red, float m_Green, float m_Blue )
			{
				if(m_switch ==0)
					return m_Red;
				else if(m_switch ==1)
					return m_Green;
				else if(m_switch ==2)
					return m_Blue;
				else
				return float(0);
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
			
			float3 Detail_Maskfloat3switch221_g43781( float m_switch, float3 m_Off, float3 m_Active, float3 m_ActiveInverted )
			{
				if(m_switch ==0)
					return m_Off;
				else if(m_switch ==1)
					return m_Active;
				else if(m_switch ==2)
					return m_ActiveInverted;
				else
				return float3(0,0,0);
			}
			
			float4 SmoothnessTypefloat4switch215_g43773( float m_switch, float4 m_Smoothness, float4 m_Roughness, float4 m_Geometric )
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
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				o.ase_texcoord8.xy = v.texcoord.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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
				o.ase_color = v.ase_color;
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
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
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
								 )
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

				float2 appendResult150_g43762 = (float2(_TilingX , _TilingY));
				float2 appendResult151_g43762 = (float2(_OffsetX , _OffsetY));
				float2 texCoord2_g43772 = IN.ase_texcoord8.xy * appendResult150_g43762 + appendResult151_g43762;
				float2 OUT_UV431_g43762 = texCoord2_g43772;
				float2 UV40_g43770 = OUT_UV431_g43762;
				float4 tex2DNode63_g43770 = SAMPLE_TEXTURE2D( _MainTex, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 OUT_ALBEDO_RGBA1177_g43762 = tex2DNode63_g43770;
				float3 temp_output_7_0_g43762 = ( (_Color).rgb * (OUT_ALBEDO_RGBA1177_g43762).rgb * _Brightness );
				float4 temp_output_39_0_g43781 = float4( temp_output_7_0_g43762 , 0.0 );
				float4 Albedo_RGBA40_g43781 = temp_output_39_0_g43781;
				float m_switch226_g43781 = _EnableDetailMask;
				float2 appendResult132_g43781 = (float2(_DetailTilingXDetail , _DetailTilingYDetail));
				float2 appendResult114_g43781 = (float2(_DetailOffsetXDetail , _DetailOffsetYDetail));
				float2 texCoord67_g43781 = IN.ase_texcoord8.xy * appendResult132_g43781 + appendResult114_g43781;
				float4 tex2DNode45_g43781 = SAMPLE_TEXTURE2D( _DetailAlbedoMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float4 ALBEDO_OUT255_g43781 = ( _ColorDetail * tex2DNode45_g43781 * _Brightness );
				float4 m_Off226_g43781 = ALBEDO_OUT255_g43781;
				float2 appendResult219_g43781 = (float2(_DetailTilingXDetailMask , _DetailTilingYDetailMask));
				float2 appendResult206_g43781 = (float2(_DetailOffsetXDetailMask , _DetailOffsetYDetailMask));
				float2 texCoord220_g43781 = IN.ase_texcoord8.xy * appendResult219_g43781 + appendResult206_g43781;
				float temp_output_15_0_g43782 = ( 1.0 - SAMPLE_TEXTURE2D( _DetailMaskMap, sampler_trilinear_repeat_aniso2, texCoord220_g43781 ).r );
				float temp_output_26_0_g43782 = _Detail_BlendAmountMask;
				float temp_output_24_0_g43782 = _Detail_BlendHardnessMask;
				float saferPower2_g43782 = abs( max( saturate( (0.0 + (temp_output_15_0_g43782 - ( 1.0 - temp_output_26_0_g43782 )) * (temp_output_24_0_g43782 - 0.0) / (1.0 - ( 1.0 - temp_output_26_0_g43782 ))) ) , 0.0 ) );
				float temp_output_22_0_g43782 = _Detail_BlendFalloffMask;
				float temp_output_403_0_g43781 = saturate( pow( saferPower2_g43782 , ( 1.0 - temp_output_22_0_g43782 ) ) );
				float4 lerpResult225_g43781 = lerp( Albedo_RGBA40_g43781 , ALBEDO_OUT255_g43781 , temp_output_403_0_g43781);
				float4 m_Active226_g43781 = lerpResult225_g43781;
				float saferPower11_g43782 = abs( max( saturate( (1.0 + (temp_output_15_0_g43782 - temp_output_26_0_g43782) * (( 1.0 - temp_output_24_0_g43782 ) - 1.0) / (0.0 - temp_output_26_0_g43782)) ) , 0.0 ) );
				float temp_output_403_21_g43781 = saturate( pow( saferPower11_g43782 , temp_output_22_0_g43782 ) );
				float4 lerpResult408_g43781 = lerp( ALBEDO_OUT255_g43781 , Albedo_RGBA40_g43781 , temp_output_403_21_g43781);
				float4 m_ActiveInverted226_g43781 = lerpResult408_g43781;
				float4 localDetail_Maskfloat4switch226_g43781 = Detail_Maskfloat4switch226_g43781( m_switch226_g43781 , m_Off226_g43781 , m_Active226_g43781 , m_ActiveInverted226_g43781 );
				float4 Mask_Albedo258_g43781 = localDetail_Maskfloat4switch226_g43781;
				float4 break48_g43781 = temp_output_39_0_g43781;
				float Albedo_RGB300_g43781 = ( break48_g43781.x + break48_g43781.y + break48_g43781.z );
				int m_switch319_g43781 = _BlendColor;
				float m_Red319_g43781 = IN.ase_color.r;
				float m_Green319_g43781 = IN.ase_color.g;
				float m_Blue319_g43781 = IN.ase_color.b;
				float localDetail_BlendVCfloatswitch319_g43781 = Detail_BlendVCfloatswitch319_g43781( m_switch319_g43781 , m_Red319_g43781 , m_Green319_g43781 , m_Blue319_g43781 );
				float clampResult47_g43781 = clamp( ( ( ( ( Albedo_RGB300_g43781 - 0.5 ) * ( _DetailBlendInfluence - 0.9 ) ) + ( localDetail_BlendVCfloatswitch319_g43781 - ( _BlendHeight - 0.4 ) ) ) / _DetailBlendSmooth ) , 0.0 , 1.0 );
				float DetailBlend43_g43781 = clampResult47_g43781;
				float4 lerpResult58_g43781 = lerp( Mask_Albedo258_g43781 , Albedo_RGBA40_g43781 , DetailBlend43_g43781);
				float4 lerpResult409_g43781 = lerp( Albedo_RGBA40_g43781 , lerpResult58_g43781 , _EnableDetailMap);
				
				float4 OUT_NORMAL1178_g43762 = SAMPLE_TEXTURE2D( _BumpMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 temp_output_1_0_g43778 = OUT_NORMAL1178_g43762;
				float temp_output_8_0_g43778 = _NormalStrength;
				float3 unpack52_g43778 = UnpackNormalScale( temp_output_1_0_g43778, temp_output_8_0_g43778 );
				unpack52_g43778.z = lerp( 1, unpack52_g43778.z, saturate(temp_output_8_0_g43778) );
				float3 temp_output_1478_59_g43762 = unpack52_g43778;
				float3 NORMAL_OUT314_g43762 = temp_output_1478_59_g43762;
				float3 temp_output_16_0_g43771 = NORMAL_OUT314_g43762;
				float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				float3 tanNormal134_g43771 = temp_output_16_0_g43771;
				float3 worldNormal134_g43771 = float3(dot(tanToWorld0,tanNormal134_g43771), dot(tanToWorld1,tanNormal134_g43771), dot(tanToWorld2,tanNormal134_g43771));
				float3 NORMAL_TANGENTSPACE135_g43771 = worldNormal134_g43771;
				float3 WorldPosition63_g43771 = NORMAL_TANGENTSPACE135_g43771;
				float4 shadowMaskValue77_g43771 = float4(1,1,1,1);
				float4 ShadowMask63_g43771 = shadowMaskValue77_g43771;
				float3 localAdditionalLightsFlatMask63_g43771 = AdditionalLightsFlatMask( WorldPosition63_g43771 , ShadowMask63_g43771 );
				float3 ADDITIONAL_LIGHT1342_g43762 = localAdditionalLightsFlatMask63_g43771;
				float3 temp_output_38_0_g43781 = ( temp_output_1478_59_g43762 + ADDITIONAL_LIGHT1342_g43762 );
				float3 Normal_XYZ260_g43781 = temp_output_38_0_g43781;
				float EnableDetailMask216_g43781 = _EnableDetailMask;
				float m_switch221_g43781 = EnableDetailMask216_g43781;
				float4 temp_output_1_0_g43784 = SAMPLE_TEXTURE2D( _DetailNormalMap, sampler_trilinear_repeat_aniso2, texCoord67_g43781 );
				float temp_output_8_0_g43784 = _DetailNormalMapScale;
				float3 unpack52_g43784 = UnpackNormalScale( temp_output_1_0_g43784, temp_output_8_0_g43784 );
				unpack52_g43784.z = lerp( 1, unpack52_g43784.z, saturate(temp_output_8_0_g43784) );
				float3 Detail_Normal199_g43781 = unpack52_g43784;
				float3 m_Off221_g43781 = Detail_Normal199_g43781;
				float3 lerpResult205_g43781 = lerp( Normal_XYZ260_g43781 , Detail_Normal199_g43781 , temp_output_403_0_g43781);
				float3 m_Active221_g43781 = saturate( lerpResult205_g43781 );
				float3 lerpResult406_g43781 = lerp( Detail_Normal199_g43781 , Normal_XYZ260_g43781 , temp_output_403_21_g43781);
				float3 m_ActiveInverted221_g43781 = saturate( lerpResult406_g43781 );
				float3 localDetail_Maskfloat3switch221_g43781 = Detail_Maskfloat3switch221_g43781( m_switch221_g43781 , m_Off221_g43781 , m_Active221_g43781 , m_ActiveInverted221_g43781 );
				float3 Mask_Normal222_g43781 = localDetail_Maskfloat3switch221_g43781;
				float layeredBlendVar413_g43781 = DetailBlend43_g43781;
				float3 layeredBlend413_g43781 = ( lerp( Mask_Normal222_g43781,Normal_XYZ260_g43781 , layeredBlendVar413_g43781 ) );
				float3 normalizeResult414_g43781 = normalize( layeredBlend413_g43781 );
				float3 temp_output_416_0_g43781 = BlendNormal( normalizeResult414_g43781 , Normal_XYZ260_g43781 );
				float EnebleMode122_g43781 = _EnableDetailMap;
				float3 lerpResult410_g43781 = lerp( Normal_XYZ260_g43781 , temp_output_416_0_g43781 , EnebleMode122_g43781);
				
				float4 _MASK_B1440_g43762 = SAMPLE_TEXTURE2D( _MetallicGlossMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float temp_output_1_0_g43768 = _MetallicStrength;
				
				float temp_output_223_0_g43773 = _SmoothnessSource;
				float m_switch215_g43773 = temp_output_223_0_g43773;
				float4 _MASK_G1438_g43762 = SAMPLE_TEXTURE2D( _SmoothnessMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float4 temp_cast_10 = (_SmoothnessMin).xxxx;
				float4 temp_cast_11 = (_SmoothnessMax).xxxx;
				float4 temp_output_216_0_g43773 = (temp_cast_10 + (_MASK_G1438_g43762 - float4( 0,0,0,0 )) * (temp_cast_11 - temp_cast_10) / (float4( 1,1,1,1 ) - float4( 0,0,0,0 )));
				float4 m_Smoothness215_g43773 = temp_output_216_0_g43773;
				float4 temp_cast_12 = (_SmoothnessMin).xxxx;
				float4 temp_cast_13 = (_SmoothnessMax).xxxx;
				float4 temp_output_214_0_g43773 = ( 1.0 - temp_output_216_0_g43773 );
				float4 m_Roughness215_g43773 = temp_output_214_0_g43773;
				float4 temp_cast_14 = (_SmoothnessMin).xxxx;
				float4 temp_cast_15 = (_SmoothnessMax).xxxx;
				float3 NORMAL_WORLD_OUT164_g43762 = temp_output_1478_59_g43762;
				float3 temp_output_4_0_g43773 = NORMAL_WORLD_OUT164_g43762;
				float3 temp_output_178_0_g43773 = ddx( temp_output_4_0_g43773 );
				float dotResult195_g43773 = dot( temp_output_178_0_g43773 , temp_output_178_0_g43773 );
				float3 temp_output_175_0_g43773 = ddy( temp_output_4_0_g43773 );
				float dotResult201_g43773 = dot( temp_output_175_0_g43773 , temp_output_175_0_g43773 );
				float4 break377_g43762 = OUT_ALBEDO_RGBA1177_g43762;
				float ALBEDO_R169_g43762 = break377_g43762.r;
				float4 temp_cast_16 = (ALBEDO_R169_g43762).xxxx;
				float4 m_Geometric215_g43773 = ( sqrt( saturate( ( temp_output_216_0_g43773 + ( ( dotResult195_g43773 + dotResult201_g43773 ) * 2.0 ) ) ) ) * ( 1.0 - temp_cast_16 ) );
				float4 localSmoothnessTypefloat4switch215_g43773 = SmoothnessTypefloat4switch215_g43773( m_switch215_g43773 , m_Smoothness215_g43773 , m_Roughness215_g43773 , m_Geometric215_g43773 );
				float4 temp_output_1556_33_g43762 = localSmoothnessTypefloat4switch215_g43773;
				float4 PBR_Occlusion1641_g43762 = SAMPLE_TEXTURE2D( _OcclusionMap, sampler_trilinear_repeat_aniso2, UV40_g43770 );
				float temp_output_16_0_g43767 = _OcclusionStrengthAO;
				float temp_output_65_0_g43767 = ( 1.0 - temp_output_16_0_g43767 );
				float3 appendResult69_g43767 = (float3(temp_output_65_0_g43767 , temp_output_65_0_g43767 , temp_output_65_0_g43767));
				float4 color77_g43767 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 temp_cast_19 = (IN.ase_color.a).xxxx;
				float4 lerpResult75_g43767 = lerp( color77_g43767 , temp_cast_19 , temp_output_16_0_g43767);
				float4 lerpResult83_g43767 = lerp( float4( ( ( ( (PBR_Occlusion1641_g43762).xyz - float3( 0.5,0.5,0.5 ) ) * temp_output_16_0_g43767 ) + appendResult69_g43767 ) , 0.0 ) , lerpResult75_g43767 , _OcclusionSource);
				float4 Occlusion1550_g43762 = saturate( lerpResult83_g43767 );
				
				float3 Albedo = lerpResult409_g43781.xyz;
				float3 Normal = lerpResult410_g43781;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( _MASK_B1440_g43762.r * temp_output_1_0_g43768 );
				float Smoothness = ( temp_output_1556_33_g43762 * Occlusion1550_g43762 ).x;
				float Occlusion = saturate( lerpResult83_g43767 ).r;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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


				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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
				
				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;


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
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
        
			CBUFFER_START(UnityPerMaterial)
			float4 _Smoothness;
			float4 _Color;
			half4 _ColorDetail;
			int _ZWriteMode;
			half _DetailOffsetYDetail;
			float _DetailTilingXDetailMask;
			float _DetailTilingYDetailMask;
			half _DetailOffsetXDetailMask;
			half _DetailOffsetYDetailMask;
			half _Detail_BlendAmountMask;
			half _Detail_BlendHardnessMask;
			half _Detail_BlendFalloffMask;
			half _DetailBlendInfluence;
			int _BlendColor;
			half _BlendHeight;
			half _DetailBlendSmooth;
			float _EnableDetailMap;
			half _NormalStrength;
			half _DetailNormalMapScale;
			float _MetallicStrength;
			half _SmoothnessSource;
			half _DetailOffsetXDetail;
			float _SmoothnessMax;
			float _DetailTilingYDetail;
			float _OcclusionStrengthAO;
			int _RenderFace;
			int _ColorMask;
			float _AlphatoCoverage;
			half _EmissionFlags;
			half _WindMode;
			float _GlobalWindInfluenceOther;
			float _LocalWindStrength;
			float _LocalRandomWindOffset;
			float _LocalWindPulse;
			float _LocalWindDirection;
			float _SmoothnessMin;
			float _TilingX;
			float _TilingY;
			float _OffsetX;
			float _OffsetY;
			half _Brightness;
			half _EnableDetailMask;
			float _DetailTilingXDetail;
			float _OcclusionSource;
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
			float _Global_Wind_Main_Fade_Bias;


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
			
			float3 Wind_Fadefloat3switch3050_g3439( int m_switch, float3 m_Off, float3 m_ActiveFadeOut, float3 m_ActiveFadeIn )
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


				int m_switch3050_g3439 = _Global_Wind_Main_Fade_Enabled;
				float m_switch2453_g3439 = _WindMode;
				float3 m_Off2453_g3439 = float3(0,0,0);
				float3 VERTEX_POSITION_MATRIX2352_g3439 = mul( GetObjectToWorldMatrix(), float4( v.vertex.xyz , 0.0 ) ).xyz;
				float3 break2265_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float WIND_MODE2462_g3439 = _WindMode;
				int m_switch2458_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2458_g3439 = 1.0;
				float m_Global2458_g3439 = ( _GlobalWindInfluenceOther * _Global_Wind_Main_Intensity );
				float m_Local2458_g3439 = _LocalWindStrength;
				float localWind_Globalfloatswitch2458_g3439 = Wind_Globalfloatswitch( m_switch2458_g3439 , m_Off2458_g3439 , m_Global2458_g3439 , m_Local2458_g3439 );
				float _WIND_STRENGHT2400_g3439 = localWind_Globalfloatswitch2458_g3439;
				int m_switch2468_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2468_g3439 = 1.0;
				float m_Global2468_g3439 = _Global_Wind_Main_RandomOffset;
				float m_Local2468_g3439 = _LocalRandomWindOffset;
				float localWind_Globalfloatswitch2468_g3439 = Wind_Globalfloatswitch( m_switch2468_g3439 , m_Off2468_g3439 , m_Global2468_g3439 , m_Local2468_g3439 );
				float4 transform3073_g3439 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float2 appendResult2307_g3439 = (float2(transform3073_g3439.x , transform3073_g3439.z));
				float dotResult2341_g3439 = dot( appendResult2307_g3439 , float2( 12.9898,78.233 ) );
				float lerpResult2238_g3439 = lerp( 0.8 , ( ( localWind_Globalfloatswitch2468_g3439 / 2.0 ) + 0.9 ) , frac( ( sin( dotResult2341_g3439 ) * 43758.55 ) ));
				float _WIND_RANDOM_OFFSET2244_g3439 = ( ( _TimeParameters.x * 0.05 ) * lerpResult2238_g3439 );
				float _WIND_TUBULENCE_RANDOM2274_g3439 = ( sin( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 40.0 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 15.0 ) ) ) * 0.5 );
				int m_switch2312_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2312_g3439 = 1.0;
				float m_Global2312_g3439 = _Global_Wind_Main_Pulse;
				float m_Local2312_g3439 = _LocalWindPulse;
				float localWind_Globalfloatswitch2312_g3439 = Wind_Globalfloatswitch( m_switch2312_g3439 , m_Off2312_g3439 , m_Global2312_g3439 , m_Local2312_g3439 );
				float _WIND_PULSE2421_g3439 = localWind_Globalfloatswitch2312_g3439;
				float FUNC_Angle2470_g3439 = ( _WIND_STRENGHT2400_g3439 * ( 1.0 + sin( ( ( ( ( _WIND_RANDOM_OFFSET2244_g3439 * 2.0 ) + _WIND_TUBULENCE_RANDOM2274_g3439 ) - ( VERTEX_POSITION_MATRIX2352_g3439.z / 50.0 ) ) - ( v.ase_color.r / 20.0 ) ) ) ) * sqrt( v.ase_color.r ) * _WIND_PULSE2421_g3439 );
				float FUNC_Angle_SinA2424_g3439 = sin( FUNC_Angle2470_g3439 );
				float FUNC_Angle_CosA2362_g3439 = cos( FUNC_Angle2470_g3439 );
				int m_switch2456_g3439 = (int)WIND_MODE2462_g3439;
				float m_Off2456_g3439 = 1.0;
				float m_Global2456_g3439 = _Global_Wind_Main_Direction;
				float m_Local2456_g3439 = _LocalWindDirection;
				float localWind_Globalfloatswitch2456_g3439 = Wind_Globalfloatswitch( m_switch2456_g3439 , m_Off2456_g3439 , m_Global2456_g3439 , m_Local2456_g3439 );
				float _WindDirection2249_g3439 = localWind_Globalfloatswitch2456_g3439;
				float2 localDirectionalEquation2249_g3439 = DirectionalEquation( _WindDirection2249_g3439 );
				float2 break2469_g3439 = localDirectionalEquation2249_g3439;
				float _WIND_DIRECTION_X2418_g3439 = break2469_g3439.x;
				float lerpResult2258_g3439 = lerp( break2265_g3439.x , ( ( break2265_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2265_g3439.x * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_X2418_g3439);
				float3 break2340_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float3 break2233_g3439 = VERTEX_POSITION_MATRIX2352_g3439;
				float _WIND_DIRECTION_Y2416_g3439 = break2469_g3439.y;
				float lerpResult2275_g3439 = lerp( break2233_g3439.z , ( ( break2233_g3439.y * FUNC_Angle_SinA2424_g3439 ) + ( break2233_g3439.z * FUNC_Angle_CosA2362_g3439 ) ) , _WIND_DIRECTION_Y2416_g3439);
				float3 appendResult2235_g3439 = (float3(lerpResult2258_g3439 , ( ( break2340_g3439.y * FUNC_Angle_CosA2362_g3439 ) - ( break2340_g3439.z * FUNC_Angle_SinA2424_g3439 ) ) , lerpResult2275_g3439));
				float3 VERTEX_POSITION2282_g3439 = ( mul( GetWorldToObjectMatrix(), float4( appendResult2235_g3439 , 0.0 ) ).xyz - v.vertex.xyz );
				float3 m_Global2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 m_Local2453_g3439 = VERTEX_POSITION2282_g3439;
				float3 localWind_Globalfloat3switch2453_g3439 = Wind_Globalfloat3switch( m_switch2453_g3439 , m_Off2453_g3439 , m_Global2453_g3439 , m_Local2453_g3439 );
				float3 m_Off3050_g3439 = localWind_Globalfloat3switch2453_g3439;
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float temp_output_3048_0_g3439 = saturate( pow( ( distance( _WorldSpaceCameraPos , ase_worldPos ) / _Global_Wind_Main_Fade_Bias ) , 5.0 ) );
				float3 m_ActiveFadeOut3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * ( 1.0 - temp_output_3048_0_g3439 ) );
				float3 m_ActiveFadeIn3050_g3439 = ( localWind_Globalfloat3switch2453_g3439 * temp_output_3048_0_g3439 );
				float3 localWind_Fadefloat3switch3050_g3439 = Wind_Fadefloat3switch3050_g3439( m_switch3050_g3439 , m_Off3050_g3439 , m_ActiveFadeOut3050_g3439 , m_ActiveFadeIn3050_g3439 );
				float3 temp_output_309_0_g43762 = localWind_Fadefloat3switch3050_g3439;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_309_0_g43762;
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
				
				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;


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
71;29.5;1654.5;850.5;-525.3817;1121.036;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;399;1715.297,-1052.488;Inherit;False;255.6026;356.7736;DEBUG SETTINGS ;4;405;293;291;353;;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;390;1379.371,-379.9673;Inherit;False;289.6667;108;DESF Utility ASE Compile Shaders;1;391;;0,0.2047877,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;387;628.3608,-685.3875;Inherit;False;DESF Module Wind;91;;3439;b135a554f7e4d0b41bba02c61b34ae74;10,938,0,2881,0,2371,0,2454,0,2433,0,2434,0,2432,0,2457,0,2878,0,2752,0;0;1;FLOAT3;2190
Node;AmplifyShaderEditor.CommentaryNode;398;1381.197,-1054.591;Inherit;False;328.6918;357.9233;GLOBAL SETTINGS ;2;407;410;;0,0,0,1;0;0
Node;AmplifyShaderEditor.FunctionNode;391;1386.371,-342.9673;Inherit;False;DESF Utility ASE Compile Shaders;-1;;43786;b85b01c42ba8a8a448b731b68fc0dbd9;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;405;1743.874,-794.6603;Inherit;False;Property;_AlphatoCoverage;Alpha to Coverage;2;1;[Enum];Create;False;1;;1;Option1;0;1;Off,0,On,1;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;291;1743.872,-938.2567;Inherit;False;Property;_ColorMask;Color Mask Mode;1;1;[Enum];Create;False;1;;0;1;None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15;True;0;False;15;15;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;353;1743.114,-866.8643;Inherit;False;Constant;_MaskClipValue;Mask Clip Value;14;0;Create;True;1;;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;407;1401.632,-943.036;Inherit;False;DESF GUI URP Surface 12x;4;;43785;5d06ad93833bfc946bb66c05fd580bef;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;389;1153.694,-680.6375;Inherit;False;DESF Module Detail;65;;43781;49c077198be2bdb409ab6ad879c0ca28;4,200,1,201,1,346,0,347,0;2;39;FLOAT4;0,0,0,0;False;38;FLOAT3;0,0,1;False;2;FLOAT4;73;FLOAT3;72
Node;AmplifyShaderEditor.IntNode;293;1744.378,-1006.967;Inherit;False;Property;_ZWriteMode;ZWrite Mode;0;2;[Header];[Enum];Create;False;1;DEBUG SETTINGS;0;1;Off,0,On,1;True;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;392;850.2145,-685.399;Inherit;False;DESF Core Surface;9;;43762;c3df20d62907cd04086a1eacc41e29d1;18,183,2,1352,2,1382,1,1433,1,1432,1,1434,1,1638,0,1588,0,1491,0,1446,0,1284,0,249,0,1319,0,1318,0,1407,0,1443,0,1337,0,1336,0;1;309;FLOAT3;0,0,0;False;7;FLOAT3;42;FLOAT3;39;FLOAT;0;FLOAT4;41;COLOR;43;FLOAT;55;FLOAT3;313
Node;AmplifyShaderEditor.IntNode;410;1405.382,-1016.786;Inherit;False;Property;_RenderFace;Render Face;3;2;[Header];[Enum];Create;False;1;SURFACE OPTIONS;0;1;Front,2,Back,1,Both,0;True;0;False;2;1;False;0;1;INT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;373;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;408;1374.904,-600.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;SceneSelectionPass;0;8;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;386;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;291;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;377;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;375;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;378;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;291;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;385;1374.904,-620.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;374;1374.904,-680.6909;Float;False;True;-1;2;DE_ShaderGUI;0;2;DEC/Surface Wind/Surface Wind Detail;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;19;False;False;False;False;False;False;False;False;False;False;False;False;True;0;True;405;True;True;2;True;410;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=-10;NatureRendererInstancing=True;True;2;True;18;all;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;True;291;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;2;LightMode=UniversalForward;NatureRendererInstancing=True;False;False;5;Include;;False;;Native;Pragma;instancing_options procedural:SetupNatureRenderer forwardadd;False;;Custom;Pragma;multi_compile GPU_FRUSTUM_ON __;False;;Custom;Include;Nature Renderer.cginc;False;ed9205546b797304ea7576ba0b32877e;Custom;Pragma;multi_compile_local _ NATURE_RENDERER;False;;Custom; ;0;0;Standard;40;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;0;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,-1;0;Translucency;0;0;  Translucency Strength;1,False,-1;0;  Normal Distortion;0.5,False,-1;0;  Scattering;2,False,-1;0;  Direct;0.9,False,-1;0;  Ambient;0.1,False,-1;0;  Shadow;0.5,False,-1;0;Cast Shadows;1;0;  Use Shadow Threshold;0;637772433161205684;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,True,388;0;  Type;0;0;  Tess;16,True,386;0;  Min;10,True,387;0;  Max;25,True,385;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;Debug Display;0;0;Clear Coat;0;0;0;10;False;True;True;True;True;True;True;True;True;True;False;;True;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;376;1374.904,-680.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;18;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;409;1374.904,-600.6909;Float;False;False;-1;2;UnityEditor.ShaderGraphLitGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ScenePickingPass;0;9;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;True;4;d3d11;glcore;gles;gles3;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;389;39;392;42
WireConnection;389;38;392;39
WireConnection;392;309;387;2190
WireConnection;374;0;389;73
WireConnection;374;1;389;72
WireConnection;374;3;392;0
WireConnection;374;4;392;41
WireConnection;374;5;392;43
WireConnection;374;8;392;313
ASEEND*/
//CHKSM=9D20CD85C79666681B228E0297F967A57D47365F