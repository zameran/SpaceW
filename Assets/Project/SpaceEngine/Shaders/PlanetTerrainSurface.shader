Shader "SpaceEngine/Planet/Terrain (Surface)" 
{
	Properties 
	{
		[NoScaleOffset] _Elevation_Tile("Elevation", 2D) = "white" {}
		[NoScaleOffset] _Normals_Tile("Normals", 2D) = "white" {}
		[NoScaleOffset] _Color_Tile("Color", 2D) = "white" {}
		[NoScaleOffset] _Ortho_Tile("Ortho", 2D) = "white" {}

		[NoScaleOffset] _Ground_Diffuse("Ground Diffuse", 2D) = "white" {}
		[NoScaleOffset] _Ground_Normal("Ground Normal", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{
			"Queue"					= "Geometry"
			"RenderType"			= "Geometry"
			"ForceNoShadowCasting"	= "False"
			"IgnoreProjector"		= "True"
		}

		Cull Back
		ZWrite On
		ZTest On
		Fog { Mode Off }
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert exclude_path:prepass noambient novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		#pragma target 5.0
		#pragma only_renderers d3d11 glcore

		#define TCCOMMON
		
		#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		//#pragma multi_compile SHINE_ON SHINE_OFF
		//#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		#pragma multi_compile OCEAN_ON OCEAN_OFF
		//#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float4 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize);

			normal.xyz = DecodeNormal(normal.xyz);
			normal.xyz = mul(_Deform_TangentFrameToWorld, normal.xyz);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal.xyz;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 0.0));

			o.Albedo = reflectance;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			//o.Normal = normal;
		}
		ENDCG

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Cull Off
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#include "Core.cginc"

			#include "UnityCG.cginc"
			#include "UnityStandardShadow.cginc"

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
 
			struct v2f_shadowCaster
			{
				V2F_SHADOW_CASTER;
			};
 
			v2f_shadowCaster vert(VertexInput v)
			{
				v2f_shadowCaster o;

				// Let's declare some data holders...
				float4 outputVertex = 0;
				float3 outputLocalVertex = 0;
				float2 outputTexcoord = 0;

				// Dublicate displacement work of main vertex shadeer...
				VERTEX_POSITION(v.vertex, v.uv0.xy, outputVertex, outputLocalVertex, outputTexcoord);

				// Pass displaced vertex to original data...
				v.vertex = float4(outputLocalVertex, 0.0);

				// Make the magic...
				TRANSFER_SHADOW_CASTER(o)

				return o;
			}
 
			float4 frag(v2f_shadowCaster i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}