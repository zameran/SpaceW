Shader "SpaceEngine/Terrain/Planet" 
{
	Properties
	{
		_Normals_Tile("Normals", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "Queue" = "Geometry" "RenderType"="" }
		
		Pass 
		{
			Cull Back

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 4.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			
			//#define CUBE_PROJECTION
			
			#include "../SpaceStuff.cginc"
			#include "../Eclipses.cginc"
			#include "../HDR.cginc"
			#include "../Atmosphere.cginc"
			#include "../Ocean/OceanBRDF.cginc"

			#include "Deformation.cginc"
			#include "Elevation.cginc"
			#include "Normals.cginc"
			#include "Ortho.cginc"
			
			uniform float _Ocean_Sigma;
			uniform float3 _Ocean_Color;
			uniform float _Ocean_DrawBRDF;
			uniform float _Ocean_Level;
			
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
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

			v2f vert(appdata_base v)
			{		
				float2 zfc = texTileLod(_Elevation_Tile, v.texcoord.xy, _Elevation_TileCoords, _Elevation_TileSize).xy;
				
				if (zfc.x <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
				{
					zfc = float2(0, 0);
				}
					
				float2 vert = abs(_Deform_Camera.xy - v.vertex.xy);

				float _blend = clamp((max(max(vert.x, vert.y), _Deform_Camera.z) - _Deform_Blending.x) / _Deform_Blending.y, 0.0, 1.0);
				
				float4 L = _Deform_ScreenQuadCornerNorms;
				float3 P = float3(v.vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, _Deform_Radius);
				
				float4 uvUV = float4(v.vertex.xy, float2(1.0,1.0) - v.vertex.xy);
				float4 alpha = uvUV.zxzx * uvUV.wwyy;
				float4 alphaPrime = alpha * L / dot(alpha, L);
				
				float h = zfc.x * (1.0 - _blend) + zfc.y * _blend;
				float k = min(length(P) / dot(alpha, L) * 1.0000003, 1.0);
				float hPrime = (h + _Deform_Radius * (1.0 - k)) / k;
				
				v2f OUT;
	
				#ifdef CUBE_PROJECTION
					OUT.pos = mul(_Deform_LocalToScreen, float4(P + float3(0.0, 0.0, h), 1.0));
				#else
					OUT.pos = mul(_Deform_ScreenQuadCorners + hPrime * _Deform_ScreenQuadVerticals,  alphaPrime);
				#endif
				
				OUT.uv = v.texcoord.xy;
				
				float3x3 LTW = _Deform_LocalToWorld;
				
				OUT.p = (_Deform_Radius + max(h, _Ocean_Level)) * normalize(mul(LTW, P));
				
				return OUT;
			}

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}
			
			float4 frag(v2f IN) : COLOR
			{		
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPO = _Globals_WorldCameraPos_Offsetted;
				float3 WSD = _Sun_WorldDirections_1[0];
				float ht = texTile(_Elevation_Tile, IN.uv, _Elevation_TileCoords, _Elevation_TileSize).x;
				
				float3 V = normalize(IN.p);
				float3 P = V * max(length(IN.p), _Deform_Radius + 10.0);
				float3 v = normalize(P - WCP);
				float3 p = P + _Globals_Origin;
				
				float3 fn;
				fn.xyz = texTile(_Normals_Tile, IN.uv, _Normals_TileCoords, _Normals_TileSize).xyz;
				fn.z = sqrt(max(0.0, 1.0 - dot(fn.xy, fn.xy)));
				
				if (ht <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
				{
					fn = float3(0, 0, 1);
				}
	
				float3x3 TTW = _Deform_TangentFrameToWorld;
				fn = mul(TTW, fn);
				
				float cTheta = dot(fn, WSD);
				float vSun = dot(V, WSD);
				
				float4 reflectance = texTile(_Ortho_Tile, IN.uv, _Ortho_TileCoords, _Ortho_TileSize);
				
				float3 sunL = 0;
				float3 skyE = 0;
				SunRadianceAndSkyIrradiance(P, fn, WSD, sunL, skyE);

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					float shadow = ShadowColor(float4(P, 1));
				#endif
				
				// diffuse ground color
				float3 groundColor = 1.5 * RGB2Reflectance(reflectance).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;
				
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

				//return float4(finalColor, 1.0);

				return float4(finalColor, 1.0);
			}
			
			ENDCG
		}
	}
}