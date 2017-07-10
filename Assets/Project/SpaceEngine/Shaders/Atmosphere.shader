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
		Pass 
		{
			Name "Atmosphere"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend SrcAlpha OneMinusSrcColor
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest LEqual
			Offset -1, -1
			Fog { Mode Off }

			CGPROGRAM
			#include "HDR.cginc"
			#include "Atmosphere.cginc"
			#include "SpaceStuff.cginc"
			#include "Eclipses.cginc"

			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f 
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 direction : TEXCOORD1;
			};

			struct f2g
			{
				float4 color : COLOR;
			};

			void vert(in a2v i, out v2f o)
			{
				//o.position = UnityObjectToClipPos(float4(i.vertex.xy, 1.0, 1.0));
				o.position = UnityObjectToClipPos(i.vertex);
				o.uv = i.uv.xy;
				//o.direction = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, i.vertex)).xyz, 0.0))).xyz;
				o.direction = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, o.position)).xyz, 0.0))).xyz;
			}
			
			void frag(in v2f i, out f2g o)
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPG = WCP + _Atmosphere_Origin; // Current camera position with offset applied...
				float3 WCPGG = WCPG + _Atmosphere_Origin; // I HAVE NO IDEA HOW, BUT IT WORKS!

				// ORIGIN = Planet center, but inverted, aka -Planet.transform.position...
				// CAMERA + ORIGIN = Current true position for atmosphere and shadows stuff...
				// CAMERA + ORIGIN + ORIGIN = Current true position for eclipses stuff...
				// FUCK DAT SHIT I'AM OUT! ¯\_(ツ)_/¯

				// NOTE : Please don't hurt me, baby...
				// NOTE : _Atmosphere_Origin Should be inverted for shadows stuff, aka Planet.transform.position...

				float3 d = normalize(i.direction);

				float sunColor = 0;
				float3 extinction = 0;
				float3 inscatter = 0;

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					float4 shadow = 1.0;

					shadow = ShadowOuterColor(d, WCP, -_Atmosphere_Origin, Rt);
					shadow = GroundFade(_ExtinctionGroundFade, shadow);
				#endif

				#ifdef LIGHT_1
					float3 extinction1 = 0;

					#ifdef ECLIPSES_ON
						float4 WSPR0 = _Sun_Positions_1[0];

						float3 invertedLightDistance0 = rsqrt(dot(WSPR0.xyz, WSPR0.xyz));
						float3 lightPosition0 = WSPR0.xyz * invertedLightDistance0;

						float lightAngularRadius = 0;

						lightAngularRadius = asin(WSPR0.w * invertedLightDistance0);
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d);
					#endif

					extinction += extinction1;

					float3 finalColor = sunColor * extinction + inscatter;

					finalColor = hdr(finalColor);

					// NOTE : Hm, if i pass this value to w component of return state - looks like cutoff. Maybe thi is a fix for a Unity 5.5.
					// TODO : Test Opacity cutout in newer versions of Unity!
					//float opacity = dot(normalize(finalColor), float3(1.0, 1.0, 1.0));

					o.color = float4(finalColor, 1.0) * fade;
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
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);

						eclipse1 = GroundFade(_ExtinctionGroundFade, eclipse1);
						eclipse2 = GroundFade(_ExtinctionGroundFade, eclipse2);
					#endif

					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[0], extinction1, 0.0);
					inscatter += SkyRadiance(WCPG, d, _Sun_WorldDirections_1[1], extinction2, 0.0);

					#ifdef ECLIPSES_ON
						inscatter *= eclipse1;
						inscatter *= eclipse2;
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d);
					#endif

					extinction += extinction1;
					extinction += extinction2;

					float3 finalColor = sunColor * extinction + inscatter;

					finalColor = hdr(finalColor);

					o.color = float4(finalColor, 1.0) * fade;
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
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR2.w * invertedLightDistance2);
						float eclipse3 = EclipseOuterShadow(lightPosition2, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);

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
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;

					float3 finalColor = sunColor * extinction + inscatter;

					finalColor = hdr(finalColor);

					o.color = float4(finalColor, 1.0) * fade;
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
						float eclipse1 = EclipseOuterShadow(lightPosition0, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR1.w * invertedLightDistance1);
						float eclipse2 = EclipseOuterShadow(lightPosition1, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR2.w * invertedLightDistance2);
						float eclipse3 = EclipseOuterShadow(lightPosition2, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);
						lightAngularRadius = asin(WSPR3.w * invertedLightDistance3);
						float eclipse4 = EclipseOuterShadow(lightPosition3, lightAngularRadius, d, WCPGG, _Atmosphere_Origin, Rt);

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
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#ifdef SHINE_ON
						inscatter += SkyShineRadiance(WCPG, d);
					#endif

					extinction += extinction1;
					extinction += extinction2;
					extinction += extinction3;
					extinction += extinction4;

					float3 finalColor = sunColor * extinction + inscatter;

					finalColor = hdr(finalColor);

					o.color = float4(finalColor, 1.0) * fade;
				#endif
			}
			ENDCG
		}
	}
}