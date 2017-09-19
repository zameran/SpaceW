// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

Shader "SpaceEngine/Planet/Terrain"
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
		ENDCG

		Pass 
		{
			Name "Planet"
			Tags 
			{
				"Queue"					= "Geometry"	// "Opaque"
				"RenderType"			= "Geometry"
				"ForceNoShadowCasting"	= "False"
				"IgnoreProjector"		= "True"

				"LightMode"				= "ForwardBase"		// "Deferred" 
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
			
			#pragma multi_compile_fwdbase noambient novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
			
			#include "SpaceStuff.cginc"
			#include "SpaceEclipses.cginc"
			#include "SpaceAtmosphere.cginc"
			#include "Ocean/OceanBRDF.cginc"
			
			struct v2f_planetTerrain
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 localVertex : TEXCOORD1;
				float3 direction : TEXCOORD2;
			};

			void vert(in a2v_planetTerrain v, out v2f_planetTerrain o)
			{	
				VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localVertex, o.texcoord);

				v.vertex = o.pos;

				o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
			}

			void frag(in v2f_planetTerrain i, out half4 outDiffuse : SV_Target)
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPO = _Atmosphere_WorldCameraPos;
				float3 WSD = _Sun_WorldDirections_1[0];
				float4 WSPR = _Sun_Positions_1[0];
				float3 position = i.localVertex;
				float2 texcoord = i.texcoord;

				float height = texTile(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).x;
				float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
				float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
				float4 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize);

				normal.xyz = DecodeNormal(normal.xyz);

				float3 V = normalize(position);
				float3 P = V * max(length(position), _Deform_Radius + _Globals_RadiusOffset);
				float3 PO = P - _Atmosphere_Origin;
				float3 v = normalize(P - WCP - _Atmosphere_Origin); // Body origin take in to account...
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
					float shadow = ShadowColor(float4(PO, 1.0));	// Body origin take in to account...
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
					float3 glowExtinction;
					float3 inscatter = 0;

					inscatter += InScattering(WCPO, P, float3(0.0, 0.0, 0.0), glowExtinction, 0.0) * _Atmosphere_GlowColor;
					inscatter += InScattering(WCPO, P, WSD, extinction, 0.0);

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
			}
			
			ENDCG
		}

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

			#include "UnityCG.cginc"
			#include "UnityStandardShadow.cginc"

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
 
			struct v2f_shadowCaster
			{
				V2F_SHADOW_CASTER;
			};
 
			v2f_shadowCaster vert(VertexInput v)
			{
				v2f_shadowCaster o;

				// Dublicate displacement work of main vertex shadeer...
				VERTEX_LOCAL_POSITION(v.vertex, v.uv0.xy, v.vertex);

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