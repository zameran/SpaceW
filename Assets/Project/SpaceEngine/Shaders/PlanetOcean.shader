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

Shader "SpaceEngine/Planet/Ocean"
{
	SubShader 
	{
		CGINCLUDE

		#include "SpaceAtmosphere.cginc"

		#include "Ocean/OceanBRDF.cginc"
		#include "Ocean/OceanDisplacement.cginc"

		#include "UnityCG.cginc"

		#if !defined(CORE)
			uniform float3 _Ocean_Color;
		#endif

		#if OCEAN_DEPTH_ON
			#if !defined(CORE)
				uniform float3 _Ocean_AbsorbtionTint;
				uniform float4 _Ocean_AbsorbtionRGBA;
			#endif

			sampler2D _CameraDepthTexture;
		#endif

		#if defined(CORE_WRITE_TO_DEPTH)
			#undef CORE_WRITE_TO_DEPTH	// TODO : Add support for custom depth buffer...
		#endif

		uniform float _Ocean_Wave_Level;

		uniform float4x4 _Ocean_LocalToOcean;

		struct a2v_planetOcean
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f_planetOcean
		{
			float4 position : SV_POSITION;
			float2 oceanU : TEXCOORD0;
			float3 oceanP : TEXCOORD1;
			float4 screenP : TEXCOORD2;

			#if OCEAN_DEPTH_ON
				float4 projPos : TEXCOORD3;
			#endif
		};

		void vert(in a2v_planetOcean v, out v2f_planetOcean o)
		{
			float t = 0;
			float3 cameraDir = 0;
			float3 oceanDir = 0;
			
			float4 vertex = float4(v.vertex.xy * 1.25, v.vertex.zw);

			float2 u = OceanPos(vertex, _Globals_ScreenToCamera, t, cameraDir, oceanDir);
			float2 dux = OceanPos(vertex + float4(_Ocean_ScreenGridSize.x, 0.0, 0.0, 0.0), _Globals_ScreenToCamera) - u;
			float2 duy = OceanPos(vertex + float4(0.0, _Ocean_ScreenGridSize.y, 0.0, 0.0), _Globals_ScreenToCamera) - u;
			float3 dP = float3(0.0, 0.0, _Ocean_HeightOffset);
				
			if(duy.x != 0.0 || duy.y != 0.0) 
			{
				float4 GRID_SIZES = _Ocean_GridSizes;
				float4 CHOPPYNESS = _Ocean_Choppyness;
					
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).x;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).y;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).z;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).w;
					
				dP.xy += CHOPPYNESS.x * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).xy;
				dP.xy += CHOPPYNESS.y * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).zw;
				dP.xy += CHOPPYNESS.z * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).xy;
				dP.xy += CHOPPYNESS.w * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).zw;
			}

			float tClamped = clamp(t * 0.25, 0.0, _Ocean_Wave_Level);
			dP = lerp(float3(0.0, 0.0, -0.1), dP, tClamped);

			float4 screenP = float4(t * cameraDir + mul(_Ocean_OceanToCamera, dP), 1.0);
			float3 oceanP = t * oceanDir + dP + float3(0.0, 0.0, _Ocean_CameraPos.z);
			float4 position = mul(_Globals_CameraToScreen, screenP);
			float4 computedScreenP = ComputeScreenPos(position);
			
			o.position = position;
			o.oceanU = u;
			o.oceanP = oceanP;
			o.screenP = screenP;

			#if OCEAN_DEPTH_ON
				o.projPos = computedScreenP;
			#endif
		}

		void frag(in v2f_planetOcean i, out ForwardOutput o)
		{
			float3 L = _Ocean_SunDir;
			float radius = _Ocean_Radius;
			float waveStrength = _Ocean_Wave_Level; // This value can be modulated...
			float2 u = i.oceanU;
			float3 oceanP = i.oceanP;
			float3 screenP = i.screenP.xyz;
			
			float3 earthCamera = float3(0.0, 0.0, _Ocean_CameraPos.z + radius);

			// NOTE : Vertices some times pass or lay under atmosphere 'ground terminator' and InScattering returns solid radiance color...
			#ifdef OCEAN_INSCATTER_FIX
				float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 10.0 - (_Ocean_HeightOffset * 1.25)) : oceanP;
			#else
				float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 10.0) : oceanP;
			#endif
			
			float3 oceanCamera = float3(0.0, 0.0, _Ocean_CameraPos.z);
			float3 V = normalize(oceanCamera - oceanP);
			
			float2 slopes = float2(0.0, 0.0);
			slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.x).xy;
			slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.y).zw;
			slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.z).xy;
			slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.w).zw;
			
			if (radius > 0.0) { slopes -= oceanP.xy / (radius + oceanP.z); }

			slopes *= waveStrength;
			
			float3 N = normalize(float3(-slopes.x, -slopes.y, 1.0));

			// Reflects backfacing normals
			if (dot(V, N) < 0.0) { N = reflect(N, V); }
				
			float Jxx = ddx(u.x);
			float Jxy = ddy(u.x);
			float Jyx = ddx(u.y);
			float Jyy = ddy(u.y);
			float A = Jxx * Jxx + Jyx * Jyx;
			float B = Jxx * Jxy + Jyx * Jyy;
			float C = Jxy * Jxy + Jyy * Jyy;
			
			const float SCALE = 10.0;

			float ua = pow(A / SCALE, 0.25);
			float ub = 0.5 + 0.5 * B / sqrt(A * C);
			float uc = pow(C / SCALE, 0.25);
			float sigmaSq = tex3D(_Ocean_Variance, float3(ua, ub, uc)).x;

			sigmaSq = max(sigmaSq, 2e-5);
			
			float3 sunL;
			float3 skyE;
			float3 extinction;

			SunRadianceAndSkyIrradiance(earthP, N, L, sunL, skyE);

			float fresnel = MeanFresnel(V, N, sigmaSq);

			float3 oceanColor = 0;
			float3 surfaceColor = 0;
			float surfaceAlpha = 1;

			#if OCEAN_DEPTH_ON
				float fragDepth = max(0, LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))).r) - _ProjectionParams.y);
				float oceanDepth = max(0, i.projPos.w - _ProjectionParams.y);
				float depthCoeff = ShoreCoefficient(fragDepth, oceanDepth);

				#if OCEAN_WHITECAPS
					float depthFoamCoeff = ShoreFoamCoefficient(fragDepth, oceanDepth);
				#endif
				
				float3 shoreColor = AbsorbtionColor(_Ocean_AbsorbtionRGBA, _Ocean_AbsorbtionTint, depthCoeff);

				oceanColor = saturate(lerp(_Ocean_Color, shoreColor, depthCoeff));
				surfaceAlpha = clamp(1.0 - lerp(0.0, 1.0, depthCoeff), 0.8, 1.0);
			#else
				oceanColor = _Ocean_Color;
				surfaceAlpha = 1.0;
			#endif

			float3 Lsky = 0;
			float3 Lsun = 0;
			float3 Lsea = 0;

			CalculateRadiances(V, N, L, earthP, oceanColor, sunL, skyE, sigmaSq, fresnel, Lsky, Lsun, Lsea);

			// Aerial perspective
			float3 inscatter = InScattering(earthCamera, earthP, L, extinction, 0.0);

			#if OCEAN_FFT
				surfaceColor = Lsun + Lsky + Lsea;
				surfaceAlpha = min(max(hdr(Lsun), fresnel + surfaceAlpha), 1.0);
			#endif
					
			#if OCEAN_WHITECAPS
				// Extract mean and variance of the jacobian matrix determinant
				float2 jm1 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.x).xy;
				float2 jm2 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.y).zw;
				float2 jm3 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.z).xy;
				float2 jm4 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.w).zw;
				float2 jm  = jm1 + jm2 + jm3 + jm4;

				float jSigma2 = max(jm.y - (jm1.x * jm1.x + jm2.x * jm2.x + jm3.x * jm3.x + jm4.x * jm4.x), 0.0);

				// Get coverage
				// Modulated...
				//float noiseValue = Noise(float3(u * 0.01, 0.0)) * _Ocean_WhiteCapStr;
				//float whiteCapStr = clamp((noiseValue + 1.0) * 0.5, 0.0, 1.0);
				//float W = WhitecapCoverage(whiteCapStr, jm.x, jSigma2);

				float whiteCapStr = _Ocean_WhiteCapStr;

				#if OCEAN_DEPTH_ON
					whiteCapStr = lerp(_Ocean_WhiteCapStr, 1.0, depthFoamCoeff * 2.0);
				#else
					whiteCapStr = _Ocean_WhiteCapStr;
				#endif

				// Simple...
				float W = WhitecapCoverage(whiteCapStr, jm.x, jSigma2);
				
				// Compute and add whitecap radiance
				float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE) / M_PI;
				//float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE + UNITY_LIGHTMODEL_AMBIENT.rgb * 30) / M_PI;

				float3 R_ftot = float3((W * waveStrength) * l * 0.4);

				surfaceColor = Lsun + Lsky + Lsea + R_ftot;
				surfaceAlpha = min(max(hdr(Lsun + R_ftot), fresnel + surfaceAlpha), 1.0);

				#if SHINE_ON_TODO
					// NOTE : Here light direction should be converted to ocean space...

					float3 shineL = 0;
					//float3 shineInscatter = 0;
					//float3 shineExtinction = 0;

					float3 occluderDirection = 0;
					float3 occluderOppositeDirection = 0;
					float intensity = 1;

					for (int i = 0; i < 4; ++i)
					{
						if (_Sky_ShineColors_1[i].w <= 0) break;

						shineL = mul(_Ocean_LocalToOcean, _Sky_ShineOccluders_1[i].xyz);

						SunRadianceAndSkyIrradiance(earthP, N, shineL, sunL, skyE);
						CalculateRadiances(V, N, shineL, earthP, oceanColor, sunL, skyE, sigmaSq, fresnel, Lsky, Lsun, Lsea);

						l = (sunL * (max(dot(N, shineL), 0.0)) + skyE) / M_PI;
						R_ftot = float3((W * waveStrength) * l * 0.4);

						occluderDirection = normalize(mul(_Ocean_LocalToOcean, _Sky_ShineOccluders_1[i].xyz) - earthP);			// Occluder direction with origin offset...
						occluderOppositeDirection = mul(_Ocean_LocalToOcean, _Sky_ShineOccluders_2[i].xyz);						// Occluder opposite direction with origin offset...
						intensity = 0.57 * max((dot(occluderDirection, occluderOppositeDirection) - _Sky_ShineParameters_1[i].w), 0);

						//shineInscatter = InScattering(earthCamera, earthP, shineL, shineExtinction, 0.0);
						//shineInscatter = InScatteringShine(earthCamera, earthP, shineL, shineExtinction, 0.0, 1.0);

						surfaceColor += (Lsun + Lsky + Lsea + R_ftot) * _Sky_ShineColors_1[i].xyz * _Sky_ShineColors_1[i].w * intensity;
						surfaceAlpha = min(max(hdr(Lsun + R_ftot), fresnel + surfaceAlpha), 1.0);
					}

					//inscatter += shineInscatter;
					//extinction += shineExtinction;
				#endif
			#endif

			// Final color
			float3 finalColor = surfaceColor * extinction + inscatter;

			o.diffuse = float4(hdr(finalColor), surfaceAlpha);
		}
		ENDCG

		Pass
		{	
			Name "Ocean"
			Tags 
			{
				"Queue"					= "Geometry+100"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite On
			ZTest LEqual

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile OCEAN_DEPTH_ON OCEAN_DEPTH_OFF
			#pragma multi_compile OCEAN_SKY_REFLECTIONS_ON OCEAN_SKY_REFLECTIONS_OFF
			#pragma multi_compile OCEAN_NONE OCEAN_FFT OCEAN_WHITECAPS
			
			#pragma multi_compile SHINE_ON SHINE_OFF
			ENDCG
		}
	}
}