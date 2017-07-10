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

Shader "SpaceEngine/Other/Sun Glare"
{
	SubShader 
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
	
		Pass 
		{
			ZWrite Off
			ZTest Off
			Cull Off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#include "UnityCG.cginc"

			#include "SpaceAtmosphere.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float Scale;
			uniform float Fade;
			
			uniform sampler2D sunSpikes;
			uniform sampler2D sunFlare;
			uniform sampler2D sunGhost1;
			uniform sampler2D sunGhost2;
			uniform sampler2D sunGhost3;
																									
			uniform float3 flareSettings;
			uniform float3 spikesSettings;
			
			uniform float4x4 ghost1Settings;
			uniform float4x4 ghost2Settings;
			uniform float4x4 ghost3Settings;
			
			uniform float UseAtmosphereColors;
			uniform float UseRadiance;
			uniform float Eclipse;
		
			uniform float SunIndex;
			uniform float3 SunViewPortPosition;

			uniform float AspectRatio;

			uniform sampler2D _FrameBuffer;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 dir : TEXCOORD0;
				float2 uv : TEXCOORD2;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = float4(v.vertex.xyz, 1.0);
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
				OUT.uv = v.texcoord.xy;

				return OUT;
			}

			float2 GetTransmittanceUV_SunGlare(float r, float mu) 
			{
				float uR, uMu;

				uR = sqrt((r - Rg) / (Rt - Rg));
				uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;

				return float2(uMu, uR);
			}

			float3 Transmittance_SunGlare(float r, float mu) 
			{
				float2 uv = GetTransmittanceUV_SunGlare(r, mu);

				return tex2D(_Sky_Transmittance, uv).rgb;
			}

			float3 Extinction_SunGlare(float3 camera, float3 viewdir)
			{
				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;

				float deltaSq = SQRT(rMu * rMu - r * r + Rt * Rt, 0.000001);
				float din = max(-rMu - deltaSq, 0.0);

				if (din > 0.0)
				{
					camera += din * viewdir;
					rMu += din;
					mu = rMu / Rt;
					r = Rt;
				}

				return (r > Rt) ? float3(1.0, 1.0, 1.0) : Transmittance_SunGlare(r, mu);;
			}

			float3 OuterRadiance_SunGlare(float3 sunColor)
			{
				return pow(max(0, sunColor), 2.2) * 2;
			}

			float4 frag(v2f IN) : COLOR
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WCPG = WCP + _Atmosphere_Origin; // Current camera position with offset applied...
				//float3 WSD = _Sun_WorldDirections_1[SunIndex];
				float3 WSD = normalize(_Sun_Positions_1[SunIndex] - WCPG);

				float2 toScreenCenter = SunViewPortPosition.xy - 0.5;

				float3 outputColor = 0;
				float3 sunColor = 0;
				float3 ghosts = 0;

				sunColor += flareSettings.x * (tex2D(sunFlare, (IN.uv.xy - SunViewPortPosition.xy) * float2(AspectRatio * flareSettings.y, 1.0) * flareSettings.z * Scale + 0.5).rgb);
				sunColor += spikesSettings.x * (tex2D(sunSpikes, (IN.uv.xy - SunViewPortPosition.xy) * float2(AspectRatio * spikesSettings.y, 1.0) * spikesSettings.z * Scale + 0.5).rgb); 
				
				for (int i = 0; i < 4; ++i)
				{			
					ghosts += ghost1Settings[i].x * 
							  (tex2D(sunGhost1, (IN.uv.xy - SunViewPortPosition.xy + (toScreenCenter * ghost1Settings[i].w)) * 
							  float2(AspectRatio * ghost1Settings[i].y, 1.0) * ghost1Settings[i].z + 0.5).rgb);

					ghosts += ghost2Settings[i].x * 
							  (tex2D(sunGhost2, (IN.uv.xy - SunViewPortPosition.xy + (toScreenCenter * ghost2Settings[i].w)) * 
							  float2(AspectRatio * ghost2Settings[i].y, 1.0) * ghost2Settings[i].z + 0.5).rgb);

					ghosts += ghost3Settings[i].x *
							  (tex2D(sunGhost3, (IN.uv.xy - SunViewPortPosition.xy + (toScreenCenter * ghost3Settings[i].w)) * 
							  float2(AspectRatio * ghost3Settings[i].y, 1.0) * ghost3Settings[i].z + 0.5).rgb);
				}	

				ghosts = ghosts * smoothstep(0.0, 1.0, 1.0 - length(toScreenCenter));	

				outputColor += sunColor;
				outputColor += ghosts;
				outputColor *= Fade;
				outputColor *= Eclipse;
				
				if (UseRadiance > 0.0)
				{
					outputColor = OuterRadiance_SunGlare(outputColor);
				}

				if (UseAtmosphereColors > 0.0)
				{
					outputColor *= Extinction_SunGlare(WCPG, WSD);
				}

				//float3 gray = float3(0.299, 0.587, 0.114);
				//float4 frameBuffer = tex2D(_FrameBuffer, SunViewPortPosition.xy);

				//if (dot(frameBuffer, gray) >= 1.0) {  }

				return float4(outputColor, 0.0);				
			}			
			ENDCG
		}
	}
}