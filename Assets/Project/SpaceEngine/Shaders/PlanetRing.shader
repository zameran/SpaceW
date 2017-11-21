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
		
	}
	SubShader
	{
		CGINCLUDE
		
		#include "Core.cginc"
		#include "SpaceStuff.cginc"
		
		uniform sampler2D DiffuseMap;
		uniform sampler2D NoiseMap;
		
		uniform float2 RingsParams;		// (Rings Width, 1 / Ring.Thickness)
		uniform float3 AmbientColor;	// (Ambient Color)
		uniform float3 LightingParams;	// (Front Bright, Back Bright, Exposure)
		uniform float4 DetailParams;	// (2D Frequency, Radial Frequency, 1 / Camera.PixelSize, Rings Density)

		#define		ringsWidth                  RingsParams.x
		#define		inversedRingsThickness      RingsParams.y
		#define		ambientColor                AmbientColor.rgb
		#define		frontBright                 LightingParams.x
		#define		backBright                  LightingParams.y
		#define		exposure                    LightingParams.z
		#define		detail2DFrequency           DetailParams.x
		#define		detailRadialFrequency       DetailParams.y
		#define		inversedCameraPixelSize     DetailParams.z
		#define		detailRingsDensity          DetailParams.w

		struct a2v_planetRing
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};
				
		struct v2f_planetRing
		{
			float4 vertex : SV_POSITION;
			float4 worldPosition : TEXCOORD0;
			float3 relativeCameraPosition : TEXCOORD1;
			
			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
				#if LIGHT_1
					float3 light1RelativePosition : TEXCOORD2;
				#endif
				#if LIGHT_2
					float3 light2RelativePosition : TEXCOORD3;
				#endif
				#if LIGHT_3
					float3 light3RelativePosition : TEXCOORD4;
				#endif
				#if LIGHT_4
					float3 light4RelativePosition : TEXCOORD5;
				#endif
			#endif

			float2 uv : TEXCOORD6;
		};
		
		void vert(in a2v_planetRing i, out v2f_planetRing o)
		{
			float4 worldPosition = mul(unity_ObjectToWorld, i.vertex);
			
			o.vertex = UnityObjectToClipPos(i.vertex);
			o.worldPosition = worldPosition;
			o.relativeCameraPosition = _WorldSpaceCameraPos - worldPosition.xyz;

			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
				#if LIGHT_1
					o.light1RelativePosition = _Light1Position.xyz - worldPosition.xyz;
				#endif
				#if LIGHT_2
					o.light2RelativePosition = _Light2Position.xyz - worldPosition.xyz;
				#endif
				#if LIGHT_3
					o.light3RelativePosition = _Light3Position.xyz - worldPosition.xyz;
				#endif
				#if LIGHT_4
					o.light4RelativePosition = _Light4Position.xyz - worldPosition.xyz;
				#endif
			#endif

			o.uv = i.uv;
		}
		
		float3 RingLighting(float3 lightPosition, 
							float3 cameraDirection, 
							float4 frontColor, 
							float4 backColor, 
							float3 frontColorMod)
		{
			const float g = -0.95;
			const float g2 = g * g;
			const float k = 1.5 / 806.202 * ((1.0 - g2) / (2.0 + g2));

			float lightDistance = length(lightPosition);
			float3 lightDirection = lightPosition / lightDistance;
			
			float theta = dot(lightDirection, cameraDirection);
			float frontLit = frontBright * (theta + 1.0) * 0.5;
			
			frontLit *= smoothstep(0.0, 1.0, abs(lightDirection.y) * M_PI); // * 100.0
			if (lightDirection.y * cameraDirection.y < 0) frontLit *= 1.0 - frontColor.a;
			
			float backLit = backBright * k * (1.0 + theta * theta) * pow(1.0 + g2 - 2.0 * g * theta, -1.5);
			
			return (frontLit * frontColorMod + backLit * backColor.rgb);
		}

		void frag(in v2f_planetRing i, out ForwardOutput o)
		{
			o.diffuse = 1.0;

			float cameraDistance = length(i.relativeCameraPosition);
			float3 cameraDirection = i.relativeCameraPosition / cameraDistance;

			float fade = smoothstep(0.0, 1.0, cameraDistance * inversedRingsThickness - 0.25);

			float pixelDistance = length(i.worldPosition.xyz);
			float3 pixelDirection = i.worldPosition.xyz / (-pixelDistance);

			float uvRadialU = (pixelDistance - 1.0) / ringsWidth;
			float2 uv = i.uv;

			float noiseValue = 1.0;
			float fadeIn = 1.0 - cameraDistance * inversedCameraPixelSize;

			if (fadeIn > 0.0)
			{
				float2 deltaPosition = i.worldPosition.xz * detail2DFrequency;
				float radial = uvRadialU * detailRadialFrequency;
				float noiseFirst = tex2D(NoiseMap, 64 * i.uv).r;

				noiseValue = texNoTile(NoiseMap, noiseFirst, deltaPosition * 0.25).r * 
							 texNoTile(NoiseMap, noiseFirst, deltaPosition * 0.15).b * 
							 tex2D(NoiseMap, float2(radial, 0.5)).b * 
							 tex2D(NoiseMap, float2(radial * 0.3, 0.5)).a * 16.0;

				noiseValue = lerp(1.0, noiseValue, clamp(fadeIn, 0.0, 1.0));
			}

			float4 frontColor = tex2D(DiffuseMap, uv);//float2(uvRadialU, 0.25));
			frontColor *= noiseValue;

			float4 backColor = tex2D(DiffuseMap, uv);//float2(uvRadialU, 0.75));
			backColor *= noiseValue;

			float3 frontColorMod = frontColor.rgb * frontColor.a;

			o.diffuse.a = clamp(frontColor.a + detailRingsDensity, 0.0, 1.0);
			o.diffuse.rgb = frontColor.a * ambientColor;

			#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
				float Shadow = 1.0;

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					Shadow = ShadowColor(i.worldPosition);
				#endif

				#if LIGHT_1
					o.diffuse.rgb += RingLighting(i.light1RelativePosition, cameraDirection, frontColor, backColor, frontColorMod) * _Light1Color;
				#endif
				#if LIGHT_2
					o.diffuse.rgb += RingLighting(i.light2RelativePosition, cameraDirection, frontColor, backColor, frontColorMod) * _Light2Color;
				#endif
				#if LIGHT_3
					o.diffuse.rgb += RingLighting(i.light3RelativePosition, cameraDirection, frontColor, backColor, frontColorMod) * _Light3Color;
				#endif
				#if LIGHT_4
					o.diffuse.rgb += RingLighting(i.light4RelativePosition, cameraDirection, frontColor, backColor, frontColorMod) * _Light4Color;
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					o.diffuse.rgb *= Shadow;
				#endif
			#endif

			o.diffuse.rgb *= exposure;
			o.diffuse *= fade;
			o.diffuse = clamp(o.diffuse, 0.0, 1.0);
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

			Blend SrcAlpha OneMinusSrcColor
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
			ENDCG
		}
	}
}