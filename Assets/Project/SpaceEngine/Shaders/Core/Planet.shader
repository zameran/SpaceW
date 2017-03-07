Shader "SpaceEngine/Terrain/Planet" 
{
	Properties
	{
		_Normals_Tile("Normals", 2D) = "white" {}
	}
	SubShader 
	{
		CGINCLUDE

		//#define CUBE_PROJECTION

		#include "Deformation.cginc"
		#include "Elevation.cginc"
		#include "Normals.cginc"
		#include "Color.cginc"
		#include "Ortho.cginc"

		uniform float _Ocean_Sigma;
		uniform float3 _Ocean_Color;
		uniform float _Ocean_DrawBRDF;
		uniform float _Ocean_Level;

		struct p2v
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
		};

		struct v2f 
		{
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			float3 p : TEXCOORD1;
		};

		float4 texTileLod(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
		{
			uv = tileCoords.xy + uv * tileSize.xy;

			return tex2Dlod(tile, float4(uv, 0, 0));
		}

		float4 texTile(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
		{
			uv = tileCoords.xy + uv * tileSize.xy;

			return tex2D(tile, uv);
		}

		float4 texTile(sampler2D tile, float2 uv, float2 tileCoords, float3 tileSize) 
		{
			uv = tileCoords + uv * tileSize.xy;

			return tex2D(tile, uv);
		}

		void VERTEX_POSITION(in p2v v, out float4 position, out float3 localPosition, out float2 uv)
		{
			float2 zfc = texTileLod(_Elevation_Tile, v.texcoord.xy, _Elevation_TileCoords, _Elevation_TileSize).xy;
				
			if (zfc.x <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) { zfc = float2(0, 0); }
			
			float4 L = _Deform_ScreenQuadCornerNorms;
			float3 P = float3(v.vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, _Deform_Radius);
				
			float4 uvUV = float4(v.vertex.xy, float2(1.0, 1.0) - v.vertex.xy);
			float4 alpha = uvUV.zxzx * uvUV.wwyy;
			float4 alphaPrime = alpha * L / dot(alpha, L);
				
			float k = min(length(P) / dot(alpha, L) * 1.0000003, 1.0);
			float hPrime = (zfc.x + _Deform_Radius * (1.0 - k)) / k;

			//position = mul(_Deform_LocalToScreen, float4(P + float3(0.0, 0.0, zfc.x), 1.0));						//CUBE PROJECTION
			position = mul(_Deform_ScreenQuadCorners + hPrime * _Deform_ScreenQuadVerticals, alphaPrime);			//SPHERICAL PROJECTION
			localPosition = (_Deform_Radius + max(zfc.x, _Ocean_Level)) * normalize(mul(_Deform_LocalToWorld, P));
			uv = v.texcoord.xy;

			v.vertex = position;
		}

		void VERTEX_PROGRAM(in p2v v, out v2f o)
		{
			VERTEX_POSITION(v, o.pos, o.p, o.uv);
		}
		ENDCG
		
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "UnityStandardShadow.cginc"

			ENDCG
		}

		Pass 
		{
			Tags { "Queue" = "Geometry" "RenderType"="" /*"LightMode" = "Deferred"*/ }

			ZTest On
			ZWrite On
			Cull Back

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 4.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			
			#include "../SpaceStuff.cginc"
			#include "../Eclipses.cginc"
			#include "../HDR.cginc"
			#include "../Atmosphere.cginc"
			#include "../Ocean/OceanBRDF.cginc"
			
			uniform sampler2D _DetailedNormal;

			void vert(in p2v v, out v2f o)
			{	
				VERTEX_PROGRAM(v, o);
			}

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}
			
			void frag(in v2f IN, 
				out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
				out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
				out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
				out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
			)
			{		
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPO = _Globals_WorldCameraPos_Offsetted;
				float3 WSD = _Sun_WorldDirections_1[0];
				float ht = texTile(_Elevation_Tile, IN.uv, _Elevation_TileCoords, _Elevation_TileSize).x;
				
				float3 V = normalize(IN.p);
				float3 P = V * max(length(IN.p), _Deform_Radius + 10.0);
				float3 v = normalize(P - WCP);
				float3 p = P + _Globals_Origin;

				float4 fn = texTile(_Normals_Tile, IN.uv, _Normals_TileCoords, _Normals_TileSize);
				fn.z = sqrt(max(0.0, 1.0 - dot(fn.xy, fn.xy)));		

				if (ht <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) {	fn = float4(0, 0, 1, 0); }
	
				float3x3 TTW = _Deform_TangentFrameToWorld;
				fn.xyz = mul(TTW, fn.xyz);
				
				float cTheta = dot(fn.xyz, WSD);
				float vSun = dot(V, WSD);
				
				float4 reflectance = texTile(_Ortho_Tile, IN.uv, _Ortho_TileCoords, _Ortho_TileSize);
				float4 color = texTile(_Color_Tile, IN.uv, _Color_TileCoords, _Color_TileSize);
				
				float3 sunL = 0;
				float3 skyE = 0;
				SunRadianceAndSkyIrradiance(P, fn.xyz, WSD, sunL, skyE);

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					float shadow = ShadowColor(float4(P, 1));
				#endif
				
				// diffuse ground color
				float3 groundColor = 1.5 * RGB2Reflectance(color).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;
				
				if (ht <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
				{
					groundColor = OceanRadiance(WSD, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color);
				}

				float3 extinction;
				float3 inscatter = InScattering(WCPO, P, WSD, extinction, 0.0);

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					inscatter *= shadow;
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					extinction = 1 * _ExtinctionGroundFade + (1 - _ExtinctionGroundFade) * extinction * shadow;
				#endif

				float3 finalColor = hdr(groundColor * extinction + inscatter);

				outDiffuse = float4(finalColor, 1.0);
				outSpecSmoothness = 1;
				outNormal = half4(fn.xyz * 0.5 + 0.5, 1);
				outEmission = 0;
				//return float4(texTile(_Elevation_Tile, IN.uv, _Elevation_TileCoords, _Elevation_TileSize).xxx * 0.002, 1.0);
				//return float4(fn.xyz, 1.0);
				//return float4(IN.uv, 1.0, 1.0);
				//return float4(ht / 10000, ht / 10000, ht / 10000, 1.0);
				//return float4(fn.www, 1);
			}
			
			ENDCG
		}
	}
}