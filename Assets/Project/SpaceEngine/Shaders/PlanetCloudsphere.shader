// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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

Shader "SpaceEngine/Planet/Cloudsphere"
{
	Properties
	{
		_DiffuseColor("Diffuse Color", Color) = (0, 0, 0, 1)
	}
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "TCCommon.cginc"

		#include "Core.cginc"
		#include "SpaceAtmosphere.cginc"
		
		#if defined(CORE_WRITE_TO_DEPTH)
			#undef CORE_WRITE_TO_DEPTH	// TODO : Add support for custom depth buffer...
		#endif

		uniform float _TransmittanceOffset;
		uniform float4 _DiffuseColor;

		struct a2v_planetCloudsphere
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f_planetCloudsphere
		{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float3 uv : TEXCOORD0;
			float3 worldPosition : TEXCOORD1;
			float3 direction : TEXCOORD2;
		};

		void vert(a2v_planetCloudsphere i, out v2f_planetCloudsphere o)
		{
			o.vertex = UnityObjectToClipPos(i.vertex);
			o.normal = i.normal;
			o.uv = i.normal;
			o.worldPosition = i.vertex.xyz;
			o.direction = dot(normalize(i.normal), normalize(_Sun_Positions_1[0] - i.vertex));
		}
			
		void frag(in v2f_planetCloudsphere i, out ForwardOutput o)
		{			
			float noiseValue = Noise(i.worldPosition * 16) + Noise(i.worldPosition * 32) + Noise(i.worldPosition * 64);

			float4 clouds = noiseValue;
			float4 transmittance = tex2D(_Sky_Transmittance, i.direction + _TransmittanceOffset);

			float cloudsAlpha = clouds.w;

			clouds *= _DiffuseColor;
			clouds += float4(transmittance.rgb, 1.0);

			o.diffuse = float4(clouds.xyz, cloudsAlpha);;
		}	

		ENDCG
	
		Pass 
		{
			Name "Cloudsphere"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest LEqual
			Offset -1, -1

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11 glcore metal
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}