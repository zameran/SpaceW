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

Shader "SpaceEngine/Planet/Ring"
{
	Properties
	{
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		_DiffuseTexture("Diffuse Texture", 2D) = "white" {}
		_DiffuseColor("Diffuse Color", Color) = (1, 1, 1, 1)
		_Mie("Mie", Vector) = (0, 0, 0)
		_LightingBias("Lighting Bias", Float) = 0
		_LightingSharpness("Lighting Sharpness", Float) = 0
		_UVCutout("UV Cutout", Float) = 0
	}
	SubShader
	{
		CGINCLUDE
			
		#include "SpaceStuff.cginc"

		uniform sampler2D _NoiseTexture;
		uniform sampler2D _DiffuseTexture;
		uniform float4    _DiffuseColor;
		uniform float4    _Mie;
		uniform float3	  _DetailParameters;	// (2D Frequency, Radial Frequency, 1 / Camera.PixelSize)
		uniform float     _LightingBias;
		uniform float     _LightingSharpness;
		uniform float	  _UVCutout;
				
		struct a2v
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};
				
		struct v2f
		{
			float4 vertex : SV_POSITION;
			float4 color : COLOR0;
			float2 uv : TEXCOORD0;
			float4 worldPosition : TEXCOORD1;
		};
				
		struct f2g
		{
			float4 color : COLOR;
		};
			
		void vert(in a2v i, out v2f o)
		{
			float4 worldPosition = mul(unity_ObjectToWorld, i.vertex);
			
			o.vertex = UnityObjectToClipPos(i.vertex);
			o.color = 1.0f;

			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
				o.color *= UNITY_LIGHTMODEL_AMBIENT * 2.0f;
			#endif

			o.uv = float2(clamp(i.uv.x, _UVCutout, 1.0 - _UVCutout), i.uv.y); //NOTE : Oh shit...
			o.worldPosition = worldPosition;
		}
				
		void frag(in v2f i, out float4 color : SV_Target)
		{
			float4 mainTex = tex2D(_DiffuseTexture, i.uv);
			float4 mainColor = mainTex * _DiffuseColor;
					
			color = i.color * mainColor;

			float cameraDistance = length(_WorldSpaceCameraPos - i.worldPosition.xyz);
			float noiseValue = 1.0;
			float fadeIn = 1.0 - cameraDistance * _DetailParameters.z;
			float fadeOut = smoothstep(0.0, 1.0, (cameraDistance * 0.000225) - 0.25);
					
			if(fadeIn > 0.0)
			{
				float2 deltaPosition = i.worldPosition.xz * _DetailParameters.x;
				float radial = i.uv.x * _DetailParameters.y;

				noiseValue = tex2D(_NoiseTexture, deltaPosition).r *
							 tex2D(_NoiseTexture, deltaPosition * 0.3).g *
							 tex2D(_NoiseTexture, float2(radial, 0.5)).b *
							 tex2D(_NoiseTexture, float2(radial * 0.3, 0.5)).a * 16.0;

				noiseValue = lerp(1.0, noiseValue, clamp(fadeIn, 0.0, 1.0));
			}

			mainColor *= noiseValue;

			float3 relativeDirection = normalize(_WorldSpaceCameraPos - i.worldPosition.xyz);
			
			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
					float3 sunRelDirection_1 = normalize(_Light1Position.xyz - i.worldPosition.xyz);
					#if LIGHT_2
						float3 sunRelDirection_2 = normalize(_Light2Position.xyz - i.worldPosition.xyz);
					#endif
					#if LIGHT_3
						float3 sunRelDirection_3 = normalize(_Light3Position.xyz - i.worldPosition.xyz);
					#endif
					#if LIGHT_4
						float3 sunRelDirection_4 = normalize(_Light4Position.xyz - i.worldPosition.xyz);
					#endif
			#endif

			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
				float3 shapened = relativeDirection * _LightingSharpness;

				float4 lighting = 0;

				lighting.xyz += saturate(dot(sunRelDirection_1, shapened) + _LightingBias) * _Light1Color;

				#if LIGHT_2
					lighting.xyz += (saturate(dot(sunRelDirection_2, shapened) + _LightingBias) * _Light2Color).xyz;
				#endif

				#if LIGHT_3
					lighting.xyz += (saturate(dot(sunRelDirection_3, shapened) + _LightingBias) * _Light3Color).xyz;
				#endif

				#if LIGHT_4
					lighting.xyz += (saturate(dot(sunRelDirection_4, shapened) + _LightingBias) * _Light4Color).xyz;
				#endif

				#if SCATTERING
					float4 scattering = MiePhase(dot(relativeDirection, sunRelDirection_1), _Mie) * _Light1Color;

					#if LIGHT_2
						scattering += MiePhase(dot(relativeDirection, sunRelDirection_2), _Mie) * _Light2Color;
					#endif

					#if LIGHT_3
						scattering += MiePhase(dot(relativeDirection, sunRelDirection_3), _Mie) * _Light3Color;
					#endif

					#if LIGHT_4
						scattering += MiePhase(dot(relativeDirection, sunRelDirection_4), _Mie) * _Light4Color;
					#endif

					scattering *= mainTex.w * (1.0f - mainTex.w); // Only scatter at the edges

					if(fadeIn > 0.0)
					{
						scattering *= noiseValue;
					}
				
					lighting += scattering;
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					lighting *= ShadowColor(i.worldPosition);
				#endif

				color += lighting * mainColor;
			#endif

			#if !LIGHT_1 && !LIGHT_2 && !LIGHT_3 && !LIGHT_4
				color = mainColor;
			#endif

			color *= fadeOut;
		}
		ENDCG

		Pass
		{
			Name "Ring"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend One OneMinusSrcColor
			Cull Off
			ZWrite On
			ZTest LEqual 
			
			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			#pragma multi_compile SCATTERING			
			ENDCG
		}
	}
}