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

Shader "SpaceEngine/Atmosphere/Cloudsphere" 
{
	Properties
	{
		_DiffuseColor("Diffuse Color", Color) = (0, 0, 0, 1)
	}
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"
		#include "Atmosphere.cginc"
		#include "TCCommon.cginc"
					
		uniform float _TransmittanceOffset;
		uniform float4 _DiffuseColor;

		struct a2v
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL0;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex0 : POSITION0;
			float4 vertex1 : POSITION1;
			float3 normal : NORMAL;
			float3 uv : TEXCOORD0;
			float3 direction : TEXCOORD1;
		};

		void main_Vertex(a2v i, out v2f o)
		{
			o.vertex0 = UnityObjectToClipPos(i.vertex);
			o.vertex1 = i.vertex;
			o.normal = i.normal;
			o.uv = i.normal;
			o.direction = dot(normalize(i.normal), normalize(_Sun_Positions_1[0] - i.vertex));
		}
			
		float4 main_Fragment(v2f IN) : COLOR
		{			
			float noise = Noise(IN.vertex1.xyz * 16) + Noise(IN.vertex1.xyz * 32) + Noise(IN.vertex1.xyz * 64);
			float4 clouds = float4(noise, noise, noise, noise);
			float4 transmittance = tex2D(_Sky_Transmittance, IN.direction + _TransmittanceOffset);

			float cloudsAlpha = clouds.w;

			clouds *= _DiffuseColor;
			clouds += float4(transmittance.rgb, 1);

			float4 output = float4(clouds.xyz, cloudsAlpha);

			return output;
		}	

		ENDCG
	
		Pass 
		{
			Name "Cloudsphere"
			Tags
			{ 
				"Queue" = "Transparent" 
				"RenderType" = "Transparent" 
				"IgnoreProjector" = "True" 
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest LEqual
			Offset -1, -1
			Fog 
			{ 
				Mode Off 
			}

			CGPROGRAM

			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex main_Vertex
			#pragma fragment main_Fragment

			ENDCG
		}
	}
}