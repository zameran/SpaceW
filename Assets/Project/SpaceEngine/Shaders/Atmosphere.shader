/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Precomputed Atmospheric Scattering
 * Copyright (c) 2008 INRIA
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Eric Bruneton
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2017
 */

Shader "SpaceEngine/Atmosphere/Atmosphere" 
{
	Properties
	{

	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
	
		Pass 
		{
			ZWrite On
			ZTest Always  
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcColor

			Cull Off

			CGPROGRAM
			#include "UnityCG.cginc"		
			#include "HDR.cginc"
			#include "Atmosphere.cginc"
			#include "SpaceStuff.cginc"
			#include "Eclipses.cginc"

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 dir : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
				OUT.uv = v.texcoord.xy;

				return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPG = WCP + _Globals_Origin; // Current camera position with offset applied...
				float3 WCPGG = WCPG + _Globals_Origin; // I HAVE NO IDEA HOW, BUT IT WORKS!

				// ORIGIN = Planet center, but inverted, aka -Planet.transform.position...
				// CAMERA + ORIGIN = Current true position for atmosphere and shadows stuff...
				// CAMERA + ORIGIN + ORIGIN = Current true position for eclipses stuff...
				// FUCK DAT SHIT I'AM OUT! ¯\_(ツ)_/¯

				// NOTE : Please don't hurt me, baby...
				// NOTE : _Globals_Origin Should be inverted for shadows stuff, aka Planet.transform.position...

				float3 d = normalize(IN.dir);

				float sunColor = 0;
				float3 extinction = 0;
				float3 inscatter = 0;

				#ifdef ECLIPSES_ON
					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						float shadow = 1.0;

						shadow = ShadowOuterColor(d, WCP, -_Globals_Origin, Rt);
						shadow = GroundFade(_ExtinctionGroundFade, shadow);
					#endif
				#endif

				#ifdef LIGHT_1
					float3 extinction1 = 0;

					#ifdef ECLIPSES_ON
						float4 WSPR0 = _Sun_Positions_1[0];

						float3 invertedLightDistance0 = rsqrt(dot(WSPR0.xyz, WSPR0.xyz));
						float3 lightPosition0 = WSPR0.xyz * invertedLightDistance0;

						float lightAngularRadius = 0;

						lightAngularRadius = asin(WSPR0.w * invertedLightDistance0);
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_2
					float3 extinction1 = 0;
					float3 extinction2 = 0;

					#ifdef ECLIPSES_ON
						float4 WSPR0 = _Sun_Positions_1[0];
						float4 WSPR1 = _Sun_Positions_1[1];

						float3 invertedLightDistance0 = rsqrt(dot(WSPR0.xyz, WSPR0.xyz));
						float3 invertedLightDistance1 = rsqrt(dot(WSPR1.xyz, WSPR1.xyz));
						float3 lightPosition0 = WSPR0.xyz * invertedLightDistance0;
						float3 lightPosition1 = WSPR1.xyz * invertedLightDistance1;

						float lightAngularRadius = 0;

						lightAngularRadius = asin(WSPR0.w * invertedLightDistance0);
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
						eclipse2 = GroundFade(_ExtinctionGroundFade, eclipse2);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[1], extinction2, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;

					#ifdef ECLIPSES_ON
						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							extinction = 1 * _ExtinctionGroundFade + (1 - _ExtinctionGroundFade) * extinction;
						#endif
					#endif

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_3
					float3 extinction1 = 0;
					float3 extinction2 = 0;
					float3 extinction3 = 0;

					#ifdef ECLIPSES_ON
						float4 WSPR0 = _Sun_Positions_1[0];
						float4 WSPR1 = _Sun_Positions_1[1];
						float4 WSPR2 = _Sun_Positions_1[2];

						float3 invertedLightDistance0 = rsqrt(dot(WSPR0.xyz, WSPR0.xyz));
						float3 invertedLightDistance1 = rsqrt(dot(WSPR1.xyz, WSPR1.xyz));
						float3 invertedLightDistance2 = rsqrt(dot(WSPR2.xyz, WSPR2.xyz));
						float3 lightPosition0 = WSPR0.xyz * invertedLightDistance0;
						float3 lightPosition1 = WSPR1.xyz * invertedLightDistance1;
						float3 lightPosition2 = WSPR2.xyz * invertedLightDistance2;

						float lightAngularRadius = 0;

						lightAngularRadius = asin(WSPR0.w * invertedLightDistance0);
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR2.w * invertedLightDistance2);
						float eclipse3 = EclipseOuterShadow(lightPosition2, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
						eclipse2 = GroundFade(_ExtinctionGroundFade, eclipse2);
						eclipse3 = GroundFade(_ExtinctionGroundFade, eclipse3);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[1], extinction2, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[2], extinction3, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;
						inscatter *= eclipse3;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				#ifdef LIGHT_4
					float3 extinction1 = 0;
					float3 extinction2 = 0;
					float3 extinction3 = 0;
					float3 extinction4 = 0;

					#ifdef ECLIPSES_ON
						float4 WSPR0 = _Sun_Positions_1[0];
						float4 WSPR1 = _Sun_Positions_1[1];
						float4 WSPR2 = _Sun_Positions_1[2];
						float4 WSPR3 = _Sun_Positions_1[3];

						float3 invertedLightDistance0 = rsqrt(dot(WSPR0.xyz, WSPR0.xyz));
						float3 invertedLightDistance1 = rsqrt(dot(WSPR1.xyz, WSPR1.xyz));
						float3 invertedLightDistance2 = rsqrt(dot(WSPR2.xyz, WSPR2.xyz));
						float3 invertedLightDistance3 = rsqrt(dot(WSPR3.xyz, WSPR3.xyz));
						float3 lightPosition0 = WSPR0.xyz * invertedLightDistance0;
						float3 lightPosition1 = WSPR1.xyz * invertedLightDistance1;
						float3 lightPosition2 = WSPR2.xyz * invertedLightDistance2;
						float3 lightPosition3 = WSPR3.xyz * invertedLightDistance3;

						float lightAngularRadius = 0;

						lightAngularRadius = asin(WSPR0.w * invertedLightDistance0);
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR2.w * invertedLightDistance2);
						float eclipse3 = EclipseOuterShadow(lightPosition2, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);
						lightAngularRadius = asin(WSPR3.w * invertedLightDistance3);
						float eclipse4 = EclipseOuterShadow(lightPosition3, lightAngularRadius, d, WCPGG, _Globals_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
						eclipse2 = GroundFade(_ExtinctionGroundFade, eclipse2);
						eclipse3 = GroundFade(_ExtinctionGroundFade, eclipse3);
						eclipse4 = GroundFade(_ExtinctionGroundFade, eclipse4);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[1], extinction2, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[2], extinction3, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[3], extinction4, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;
						inscatter *= eclipse3;
						inscatter *= eclipse4;

						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							inscatter *= shadow;
						#endif
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;
					extinction += extinction4;

					float3 finalColor = sunColor * extinction + inscatter;

					return float4(hdr(finalColor), 1) * fade;
				#endif

				return float4(0, 0, 0, 0);
			}		
			ENDCG
		}
	}
}