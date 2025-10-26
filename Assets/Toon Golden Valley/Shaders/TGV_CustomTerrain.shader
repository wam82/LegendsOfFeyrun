// Made with Amplify Shader Editor v1.9.9.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TGV_CustomTerrain"
{
	Properties
	{
		[HideInInspector] _TerrainHolesTexture( "_TerrainHolesTexture", 2D ) = "white" {}
		[HideInInspector] _Control( "Control", 2D ) = "white" {}
		[HideInInspector] _Splat3( "Splat3", 2D ) = "white" {}
		[HideInInspector] _Splat2( "Splat2", 2D ) = "white" {}
		[HideInInspector] _Splat1( "Splat1", 2D ) = "white" {}
		_TextureRamp( "Texture Ramp", 2D ) = "white" {}
		[HideInInspector] _Splat0( "Splat0", 2D ) = "white" {}
		[HideInInspector] _Normal0( "Normal0", 2D ) = "white" {}
		[HideInInspector] _Normal1( "Normal1", 2D ) = "white" {}
		[HideInInspector] _Normal2( "Normal2", 2D ) = "white" {}
		[HideInInspector] _Normal3( "Normal3", 2D ) = "white" {}
		[HideInInspector] _Smoothness3( "Smoothness3", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _Smoothness1( "Smoothness1", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _Smoothness0( "Smoothness0", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _Smoothness2( "Smoothness2", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _Mask2( "_Mask2", 2D ) = "white" {}
		[HideInInspector] _Mask0( "_Mask0", 2D ) = "white" {}
		[HideInInspector] _Mask1( "_Mask1", 2D ) = "white" {}
		[HideInInspector] _Mask3( "_Mask3", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry-100" "TerrainCompatible"="True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
		#define ASE_VERSION 19901
		#pragma multi_compile_local __ _ALPHATEST_ON
		#pragma shader_feature_local _MASKMAP
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Mask2;
		uniform sampler2D _Mask0;
		uniform sampler2D _Mask1;
		uniform sampler2D _Mask3;
		uniform float4 _MaskMapRemapScale0;
		uniform float4 _MaskMapRemapOffset2;
		uniform float4 _MaskMapRemapScale2;
		uniform float4 _MaskMapRemapScale1;
		uniform float4 _MaskMapRemapOffset1;
		uniform float4 _MaskMapRemapScale3;
		uniform float4 _MaskMapRemapOffset3;
		uniform float4 _MaskMapRemapOffset0;
		uniform sampler2D _Control;
		uniform float4 _Control_ST;
		uniform float _Smoothness0;
		uniform sampler2D _TextureRamp;
		uniform sampler2D _Normal0;
		uniform sampler2D _Splat0;
		uniform float4 _Splat0_ST;
		uniform float _Smoothness1;
		uniform sampler2D _Normal1;
		uniform sampler2D _Splat1;
		uniform float4 _Splat1_ST;
		uniform float _Smoothness2;
		uniform sampler2D _Normal2;
		uniform sampler2D _Splat2;
		uniform float4 _Splat2_ST;
		uniform float _Smoothness3;
		uniform sampler2D _Normal3;
		uniform sampler2D _Splat3;
		uniform float4 _Splat3_ST;
		uniform sampler2D _TerrainHolesTexture;
		uniform float4 _TerrainHolesTexture_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float localCalculateTangentsStandard16_g37 = ( 0.0 );
			{
			v.tangent.xyz = cross ( v.normal, float3( 0, 0, 1 ) );
			v.tangent.w = -1;
			}
			v.vertex.xyz += localCalculateTangentsStandard16_g37;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_Control = i.uv_texcoord * _Control_ST.xy + _Control_ST.zw;
			float4 tex2DNode5_g37 = tex2D( _Control, uv_Control );
			float dotResult20_g37 = dot( tex2DNode5_g37 , float4( 1, 1, 1, 1 ) );
			float SplatWeight22_g37 = dotResult20_g37;
			float localSplatClip74_g37 = ( SplatWeight22_g37 );
			float SplatWeight74_g37 = SplatWeight22_g37;
			{
			#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
				clip(SplatWeight74_g37 == 0.0f ? -1 : 1);
			#endif
			}
			float4 SplatControl26_g37 = ( tex2DNode5_g37 / ( localSplatClip74_g37 + 0.001 ) );
			float4 temp_output_59_0_g37 = SplatControl26_g37;
			float4 appendResult33_g37 = (float4(1.0 , 1.0 , 1.0 , _Smoothness0));
			float2 uv_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			float3 tex2DNode2_g37 = UnpackNormal( tex2D( _Normal0, uv_Splat0 ) );
			float3 NormalInput0426_g37 = tex2DNode2_g37;
			float3 ase_positionWS = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_lightDirWS = 0;
			#else //aseld
			float3 ase_lightDirWS = normalize( UnityWorldSpaceLightDir( ase_positionWS ) );
			#endif //aseld
			float dotResult374_g37 = dot( (WorldNormalVector( i , NormalInput0426_g37 )) , ase_lightDirWS );
			float temp_output_363_0_g37 = (dotResult374_g37*0.49 + 0.5);
			float2 temp_cast_2 = (temp_output_363_0_g37).xx;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 ase_normalWS = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normalWSNorm = normalize( ase_normalWS );
			UnityGI gi387_g37 = gi;
			float3 diffNorm387_g37 = ase_normalWSNorm;
			gi387_g37 = UnityGI_Base( data, 1, diffNorm387_g37 );
			float3 indirectDiffuse387_g37 = gi387_g37.indirect.diffuse + diffNorm387_g37 * 0.0001;
			float3 temp_output_362_0_g37 = ( ase_lightColor.rgb + indirectDiffuse387_g37 );
			float3 temp_output_376_0_g37 = ( ase_lightAtten * ase_lightColor.rgb * ase_lightColor.a );
			float3 break375_g37 = temp_output_376_0_g37;
			float temp_output_378_0_g37 = max( max( break375_g37.x , break375_g37.y ) , break375_g37.z );
			float3 temp_cast_4 = (1.0).xxx;
			#ifdef UNITY_PASS_FORWARDBASE
				float3 staticSwitch366_g37 = temp_cast_4;
			#else
				float3 staticSwitch366_g37 = temp_output_376_0_g37;
			#endif
			float4 tex2DNode4_g37 = tex2D( _Splat0, uv_Splat0 );
			float4 Splat0301_g37 = tex2DNode4_g37;
			float3 temp_output_367_0_g37 = ( indirectDiffuse387_g37 * ase_lightColor.rgb * ase_lightColor.a );
			float4 Sp0421_g37 = ( ( ( ( tex2D( _TextureRamp, temp_cast_2 ) * float4( temp_output_362_0_g37 , 0.0 ) ) * ( temp_output_363_0_g37 * temp_output_378_0_g37 ) ) * ( float4( staticSwitch366_g37 , 0.0 ) * Splat0301_g37 ) ) + ( Splat0301_g37 * float4( temp_output_367_0_g37 , 0.0 ) ) );
			float3 _Vector1 = float3(1,1,1);
			float4 appendResult258_g37 = (float4(_Vector1 , 1.0));
			float4 tintLayer0253_g37 = appendResult258_g37;
			float4 appendResult36_g37 = (float4(1.0 , 1.0 , 1.0 , _Smoothness1));
			float2 uv_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float3 tex2DNode1_g37 = UnpackNormal( tex2D( _Normal1, uv_Splat1 ) );
			float3 NormalInput1428_g37 = tex2DNode1_g37;
			float dotResult436_g37 = dot( (WorldNormalVector( i , NormalInput1428_g37 )) , ase_lightDirWS );
			float temp_output_437_0_g37 = (dotResult436_g37*0.49 + 0.5);
			float2 temp_cast_8 = (temp_output_437_0_g37).xx;
			float4 tex2DNode3_g37 = tex2D( _Splat1, uv_Splat1 );
			float4 Splat1302_g37 = tex2DNode3_g37;
			float4 Sp1420_g37 = ( ( ( ( tex2D( _TextureRamp, temp_cast_8 ) * float4( temp_output_362_0_g37 , 0.0 ) ) * ( temp_output_437_0_g37 * temp_output_378_0_g37 ) ) * ( float4( staticSwitch366_g37 , 0.0 ) * Splat1302_g37 ) ) + ( Splat1302_g37 * float4( temp_output_367_0_g37 , 0.0 ) ) );
			float3 _Vector2 = float3(1,1,1);
			float4 appendResult261_g37 = (float4(_Vector2 , 1.0));
			float4 tintLayer1254_g37 = appendResult261_g37;
			float4 appendResult39_g37 = (float4(1.0 , 1.0 , 1.0 , _Smoothness2));
			float2 uv_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float3 tex2DNode10_g37 = UnpackNormal( tex2D( _Normal2, uv_Splat2 ) );
			float3 NormalInput2429_g37 = tex2DNode10_g37;
			float dotResult443_g37 = dot( (WorldNormalVector( i , NormalInput2429_g37 )) , ase_lightDirWS );
			float temp_output_444_0_g37 = (dotResult443_g37*0.49 + 0.5);
			float2 temp_cast_13 = (temp_output_444_0_g37).xx;
			float4 tex2DNode6_g37 = tex2D( _Splat2, uv_Splat2 );
			float4 Splat2303_g37 = tex2DNode6_g37;
			float4 Sp2419_g37 = ( ( ( ( tex2D( _TextureRamp, temp_cast_13 ) * float4( temp_output_362_0_g37 , 0.0 ) ) * ( temp_output_444_0_g37 * temp_output_378_0_g37 ) ) * ( float4( staticSwitch366_g37 , 0.0 ) * Splat2303_g37 ) ) + ( Splat2303_g37 * float4( temp_output_367_0_g37 , 0.0 ) ) );
			float3 _Vector3 = float3(1,1,1);
			float4 appendResult263_g37 = (float4(_Vector3 , 1.0));
			float4 tintLayer2255_g37 = appendResult263_g37;
			float4 appendResult42_g37 = (float4(1.0 , 1.0 , 1.0 , _Smoothness3));
			float2 uv_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float3 tex2DNode11_g37 = UnpackNormal( tex2D( _Normal3, uv_Splat3 ) );
			float3 NormalInput3430_g37 = tex2DNode11_g37;
			float dotResult450_g37 = dot( (WorldNormalVector( i , NormalInput3430_g37 )) , ase_lightDirWS );
			float temp_output_451_0_g37 = (dotResult450_g37*0.49 + 0.5);
			float2 temp_cast_18 = (temp_output_451_0_g37).xx;
			float4 tex2DNode7_g37 = tex2D( _Splat3, uv_Splat3 );
			float4 Splat3304_g37 = tex2DNode7_g37;
			float4 Sp3386_g37 = ( ( ( ( tex2D( _TextureRamp, temp_cast_18 ) * float4( temp_output_362_0_g37 , 0.0 ) ) * ( temp_output_451_0_g37 * temp_output_378_0_g37 ) ) * ( float4( staticSwitch366_g37 , 0.0 ) * Splat3304_g37 ) ) + ( Splat3304_g37 * float4( temp_output_367_0_g37 , 0.0 ) ) );
			float3 _Vector4 = float3(1,1,1);
			float4 appendResult265_g37 = (float4(_Vector4 , 1.0));
			float4 tintLayer3256_g37 = appendResult265_g37;
			float4 weightedBlendVar9_g37 = temp_output_59_0_g37;
			float4 weightedBlend9_g37 = ( weightedBlendVar9_g37.x*( appendResult33_g37 * Sp0421_g37 * tintLayer0253_g37 ) + weightedBlendVar9_g37.y*( appendResult36_g37 * Sp1420_g37 * tintLayer1254_g37 ) + weightedBlendVar9_g37.z*( appendResult39_g37 * Sp2419_g37 * tintLayer2255_g37 ) + weightedBlendVar9_g37.w*( appendResult42_g37 * Sp3386_g37 * tintLayer3256_g37 ) );
			float4 MixDiffuse28_g37 = weightedBlend9_g37;
			float4 temp_output_60_0_g37 = MixDiffuse28_g37;
			float4 localClipHoles100_g37 = ( temp_output_60_0_g37 );
			float2 uv_TerrainHolesTexture = i.uv_texcoord * _TerrainHolesTexture_ST.xy + _TerrainHolesTexture_ST.zw;
			float holeClipValue99_g37 = tex2D( _TerrainHolesTexture, uv_TerrainHolesTexture ).r;
			float Hole100_g37 = holeClipValue99_g37;
			{
			#ifdef _ALPHATEST_ON
				clip(Hole100_g37 == 0.0f ? -1 : 1);
			#endif
			}
			c.rgb = localClipHoles100_g37.xyz;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}

	Dependency "BaseMapShader"="Toon/TerrainBasePassToon"
	Dependency "AddPassShader"="Toon/TerrainAddPassToon"
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19901
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;135;-352,-304;Inherit;False;Four Splats First Pass Custom Toon;0;;37;a8603d68a48c45349a4d5b56b224d75e;2,102,1,85,0;7;59;FLOAT4;0,0,0,0;False;60;FLOAT4;0,0,0,0;False;61;FLOAT3;0,0,0;False;57;FLOAT;0;False;58;FLOAT;0;False;201;FLOAT;0;False;62;FLOAT;0;False;7;FLOAT4;0;FLOAT3;14;FLOAT;56;FLOAT;45;FLOAT;200;FLOAT;19;FLOAT3;17
Node;AmplifyShaderEditor.StandardSurfaceOutputNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;0;160,-464;Float;False;True;-1;3;;0;0;CustomLighting;Toon/TGV_CustomTerrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;-100;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;1;TerrainCompatible=True;False;2;BaseMapShader=Toon/TerrainBasePassToon;AddPassShader=Toon/TerrainAddPassToon;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;13;135;0
WireConnection;0;11;135;17
ASEEND*/
//CHKSM=7E6AAEEF0ABCDE48DEB6181491D65B2642A0B466