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

Shader "SpaceEngine/Stars/Starfield" 
{
	SubShader 
	{
		Tags { "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Background" }
		Blend OneMinusDstColor OneMinusSrcAlpha
		//Blend OneMinusDstAlpha SrcAlpha
		ZWrite Off 
		Fog { Mode Off }

		Pass
		{	
			CGPROGRAM
			#include "HDR.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest	

			uniform float _StarIntensity;
			uniform float4x4 _RotationMatrix;
		
			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD;
			};
		
			struct v2f 
			{
				float4 pos : SV_POSITION;
				half4 color : COLOR;
				half2 uv : TEXCOORD0;
			};	
			
			float GetFlickerAmount(in float2 pos)
			{
				const float2 tab[8] = 
				{
					float2(0.897907815,-0.347608525), float2(0.550299290, 0.273586675), float2(0.823885965, 0.098853070), float2(0.922739035,-0.122108860),
					float2(0.800630175,-0.088956800), float2(0.711673375, 0.158864420), float2(0.870537795, 0.085484560), float2(0.956022355,-0.058114540)
				};
		
				float2 hash = frac(pos.xy * 512);
				//float index = frac(hash.x + (hash.y + 1) * (_Time.x * 2 + unity_DeltaTime.z)); // flickering speed
				float index = frac(hash.x + (hash.y + 1) * (_Time.w * 5 * unity_DeltaTime.z)); // flickering speed

				index *= 8;

				float f = frac(index) * 2.5;
				int i = (int)index;

				return tab[i].x + f * tab[i].y;
			}	
		
			v2f vert(appdata_t v)
			{
				v2f OUT;

				float3 t = mul((float3x3)_RotationMatrix, v.vertex.xyz) + _WorldSpaceCameraPos.xyz; 

				//float appMag = 6.5 + v.color.w * (-1.44 - 2.5);
				float appMag = 6.5 + v.color.w * 255 * (-1.44 - 2.5);
				float brightness = GetFlickerAmount(v.vertex.xy) * pow(5.0, (-appMag - 1.44) / 2.5);
						
				half4 starColor = _StarIntensity * float4(brightness * v.color.xyz, brightness);
			
				OUT.pos = mul(UNITY_MATRIX_MVP, float4 (t, 1));
				OUT.color = starColor;// * saturate(t.y);	
				OUT.uv = 5 * (v.texcoord.xy - float2(0.5, 0.5));
			
				return OUT;
			}

			half4 frag(v2f IN) : SV_Target
			{
				half2 distCenter = IN.uv.xy;
				half scale = exp(-dot(distCenter, distCenter));
				half3 colFinal = IN.color.xyz * scale + 5 * IN.color.w * pow(scale, 16);

				colFinal = (colFinal * colFinal) * 2;

				return half4(hdr(colFinal), 0);
			}
			ENDCG
		}
	}
}