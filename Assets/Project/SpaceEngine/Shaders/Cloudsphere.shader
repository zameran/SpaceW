/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
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

Shader "SpaceEngine/Atmosphere/Cloudsphere" 
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_TransmittanceColor("Transmittance Color", Color) = (1, 1, 1, 1)

		_Cloud("Cloud (RGBA)", CUBE) = "white" {}
		_Normal("Normal (RGBA)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
	
		Pass 
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest LEqual
			Offset -1, -1

			CGPROGRAM
			#include "UnityCG.cginc"		
			#include "Utility.cginc"
			#include "Atmosphere.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"

			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
					
			uniform float _TransmittanceOffset;

			uniform float4 _Color;
			uniform float4 _TransmittanceColor;

			uniform samplerCUBE _Cloud;
			uniform sampler2D _Normal;


			struct appdata_full_compute 
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 texcoord3 : TEXCOORD3;

				uint id : SV_VertexID;
			};

			struct v2f 
			{
				float4 vertex0 : POSITION0;
				float4 vertex1 : POSITION1;
				float3 normal : NORMAL;
				float3 uv : TEXCOORD0;
				float3 direction : TEXCOORD1;
				
			};

			v2f vert(appdata_full_compute v)
			{
				v2f OUT;
				
				OUT.vertex0 = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.vertex1 = v.vertex;
				OUT.normal = v.normal;
				OUT.uv = v.normal;
				OUT.direction = dot(normalize(v.normal), normalize(_Sun_Positions_1[0] - v.vertex));

				return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float4 clouds = texCUBE(_Cloud, IN.uv).aaaa;
				float4 transmittance = tex2D(_Sky_Transmittance, IN.direction + _TransmittanceOffset);

				clouds = saturate(clouds);

				transmittance /= _TransmittanceColor;

				clouds *= _Color;
				clouds *= transmittance;

				return clouds;
			}	
			ENDCG
		}
	}
}