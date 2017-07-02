Shader "SpaceEngine/Terrain/Planet" 
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
		CGINCLUDE
		#include "Core.cginc"

		uniform float _Ocean_Sigma;
		uniform float3 _Ocean_Color;
		uniform float _Ocean_DrawBRDF;
		uniform float _Ocean_Level;

		struct a2v
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
		};

		struct v2f 
		{
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
			float3 localVertex : TEXCOORD1;
			float3 direction : TEXCOORD2;
		};

		void VERTEX_POSITION(in float4 vertex, in float2 texcoord, out float4 position, out float3 localPosition, out float2 uv)
		{
			float2 zfc = texTileLod(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).xy;

			#if ATMOSPHERE_ON
				#if OCEAN_ON
					if (zfc.x <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) { zfc = float2(0, 0); }
				#endif
			#endif
			
			float4 vertexUV = float4(vertex.xy, float2(1.0, 1.0) - vertex.xy);
			float2 vertexToCamera = abs(_Deform_Camera.xy - vertex.xy);
			float vertexDistance = max(max(vertexToCamera.x, vertexToCamera.y), _Deform_Camera.z);
			float vertexBlend = clamp((vertexDistance - _Deform_Blending.x) / _Deform_Blending.y, 0.0, 1.0);
				
			float4 alpha = vertexUV.zxzx * vertexUV.wwyy;
			float4 alphaPrime = alpha * _Deform_ScreenQuadCornerNorms / dot(alpha, _Deform_ScreenQuadCornerNorms);

			float3 P = float3(vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, _Deform_Radius);
				
			float h = zfc.x * (1.0 - vertexBlend) + zfc.y * vertexBlend;
			float k = min(length(P) / dot(alpha, _Deform_ScreenQuadCornerNorms) * 1.0000003, 1.0);
			float hPrime = (h + _Deform_Radius * (1.0 - k)) / k;

			//position = mul(_Deform_LocalToScreen, float4(P + float3(0.0, 0.0, h), 1.0));							//CUBE PROJECTION
			position = mul(_Deform_ScreenQuadCorners + hPrime * _Deform_ScreenQuadVerticals, alphaPrime);			//SPHERICAL PROJECTION
			localPosition = (_Deform_Radius + max(h, _Ocean_Level)) * normalize(mul(_Deform_LocalToWorld, P));
			uv = texcoord;
		}

		void VERTEX_PROGRAM(in a2v v, out v2f o)
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.vertex, o.localVertex, o.texcoord);

			o.direction = 0;

			v.vertex = o.vertex; // Assign calculated vertex position to our data...
		}
		ENDCG

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

			#include "UnityStandardShadow.cginc"

			#pragma multi_compile_shadowcaster
 
			struct v2f_shadowCaster
			{
				V2F_SHADOW_CASTER;
			};
 
			v2f_shadowCaster vert(VertexInput v)
			{
				v2f_shadowCaster o;

				//-----------------------------------------------------------------------------
				float4 outputVertex = 0;
				float3 outputLocalVertex = 0;
				float2 outputTexcoord = 0;

				VERTEX_POSITION(v.vertex, v.uv0.xy, outputVertex, outputLocalVertex, outputTexcoord);

				v.vertex = float4(outputLocalVertex, 1.0);
				//-----------------------------------------------------------------------------

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}
 
			float4 frag(v2f_shadowCaster i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}

		Pass 
		{
			Name "Planet"
			Tags 
			{
				"Queue"					= "Geometry"	// "Opaque"
				"RenderType"			= "Geometry"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"		// "Deferred" 
			}

			Cull Back
			ZWrite On
			ZTest On
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 4.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			
			#include "SpaceStuff.cginc"
			#include "Eclipses.cginc"
			#include "HDR.cginc"
			#include "Atmosphere.cginc"
			#include "Ocean/OceanBRDF.cginc"
			
			uniform sampler2D _Ground_Diffuse;
			uniform sampler2D _Ground_Normal;
			uniform sampler2D _DetailedNormal;

			void vert(in a2v v, out v2f o)
			{	
				VERTEX_PROGRAM(v, o);

				//o.direction = ((_Globals_WorldCameraPos_Offsetted + _Globals_Origin) - mul(_Globals_CameraToWorld, o.vertex)).xyz;
				o.direction = (_Globals_WorldCameraPos_Offsetted + _Globals_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
			}

			void frag(in v2f i, 
				out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
				out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
				out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a) 
				out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
			)
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPO = _Globals_WorldCameraPos_Offsetted;
				float3 WSD = _Sun_WorldDirections_1[0];
				float4 WSPR = _Sun_Positions_1[0];
				float3 position = i.localVertex;
				float2 texcoord = i.texcoord;

				float height = texTile(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).x;
				float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
				float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
				float4 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize);

				normal.xyz = DecodeNormal(normal.xyz);
				
				//float4 triplanarDiffuse = Triplanar(_Ground_Diffuse, _Ground_Diffuse, _Ground_Diffuse, P, normal.xyz, float2(128, 4));
				//float4 triplanarNormal = Triplanar(_Ground_Normal, _Ground_Normal, _Ground_Normal, P, normal.xyz, float2(128, 4));	

				float3 V = normalize(position);
				float3 P = V * max(length(position), _Deform_Radius + 10.0);
				float3 PO = P - _Globals_Origin;
				float3 v = normalize(P - WCP - _Globals_Origin); // Body origin take in to account...
				float3 d = normalize(i.direction);

				#if ATMOSPHERE_ON
					#if OCEAN_ON
						if (height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) {	normal = float4(0.0, 0.0, 1.0, 0.0); }
					#endif
				#endif
				
				normal.xyz = mul(_Deform_TangentFrameToWorld, normal.xyz);

				float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

				float cTheta = dot(normal.xyz, WSD);

				#ifdef ECLIPSES_ON
					float eclipse = 1;

					float3 invertedLightDistance = rsqrt(dot(WSPR.xyz, WSPR.xyz));
					float3 lightPosition = WSPR.xyz * invertedLightDistance;

					float lightAngularRadius = asin(WSPR.w * invertedLightDistance);

					eclipse *= EclipseShadow(P, lightPosition, lightAngularRadius);
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					float shadow = ShadowColor(float4(PO, 1));	// Body origin take in to account...
				#endif
				
				#if ATMOSPHERE_ON
					float3 sunL = 0.0;
					float3 skyE = 0.0;
					SunRadianceAndSkyIrradiance(P, normal.xyz, WSD, sunL, skyE);

					float3 groundColor = 1.5 * RGB2Reflectance(reflectance).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;
					
					#if OCEAN_ON
						if (height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
						{	
							groundColor = OceanRadiance(WSD, -v, V, _Ocean_Sigma, sunL, skyE, _Ocean_Color, P);
						}
					#endif

					float darknessAccumulation = 1.0;
					float3 extinction;
					float3 inscatter = InScattering(WCPO, P, WSD, extinction, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse;
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(P, d);
					#endif

					#ifdef ECLIPSES_ON
						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							darknessAccumulation = eclipse * shadow;
						#else
							darknessAccumulation = eclipse;
						#endif
					#else
						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							darknessAccumulation = shadow;
						#endif
					#endif

					extinction = GroundFade(_ExtinctionGroundFade, extinction, darknessAccumulation);

					float3 finalColor = hdr(groundColor * extinction + inscatter);
				#elif ATMOSPHERE_OFF
					float3 finalColor = 1.5 * reflectance * max(cTheta, 0);
				#endif

				outDiffuse = float4(finalColor, 1.0);
				outSpecSmoothness = 1.0;
				outNormal = half4(normal.xyz * 0.5 + 0.5, 1.0);
				outEmission = half4(exp(-finalColor / 2), 1.0);
			}
			
			ENDCG
		}
	}
}