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

Shader "SpaceEngine/Space/Sun Glare"
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
			
			uniform float useRadiance;
			uniform float useAtmosphereColors;
							
			uniform float3 sunPosition;
			uniform float3 sunViewPortPosition;
			uniform float3 sunViewPortPositionInversed;

			uniform float aspectRatio;
			uniform float sunGlareScale;
			uniform float sunGlareFade;

			sampler2D _CameraGBufferTexture2;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// TODO : Use built in atmosphere's stuff... From SpaceAtmosphere.cginc...
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
				return pow(max(0.0f, sunColor), 2.2f) * 2.0f;
			}

			float ObstacleSample(float2 uv, float u)
			{
				float value = 0.0f;

				value += tex2D(_CameraGBufferTexture2, uv + float2(u, 0.0f)).a;
				value += tex2D(_CameraGBufferTexture2, uv - float2(u, 0.0f)).a;
				value += tex2D(_CameraGBufferTexture2, uv - float2(0.0f, u)).a;
				value += tex2D(_CameraGBufferTexture2, uv + float2(0.0f, u)).a;

				return smoothstep(0.0f, 1.0f, 1.0f - clamp(value / 4.0f, 0.0f, 1.0f));
			}

			void vert(in appdata_base i, out v2f o)
			{
				o.pos = float4(i.vertex.xyz, 1.0);
				o.uv = i.texcoord.xy;
			}

			void frag(in v2f i, out float4 diffuse : COLOR)
			{
				// NOTE : Sample W component from GBuffer normals data. Unity's Standart shader using it as 1.0, and my planets too!
				float obstacle = ObstacleSample(sunViewPortPositionInversed, 1.0 / 512);					// Sample 4 points...
				//float obstacle = 1.0 - tex2D(_CameraGBufferTexture2, sunViewPortPositionInversed.xy).a;	// Sample 1 point...

				// Perform obstacle test...
				if (sign(obstacle) == 0.0f) discard;

				float2 toScreenCenter = sunViewPortPosition.xy - 0.5;
				float2 sceenCenterUV = i.uv.xy - sunViewPortPosition.xy;

				float3 outputColor = 0;
				float3 sunColor = 0;
				float3 ghosts = 0;

				sunColor += flareSettings.x * (tex2D(sunFlare, sceenCenterUV * float2(aspectRatio * flareSettings.y, 1.0) * flareSettings.z * sunGlareScale + 0.5).rgb);
				sunColor += spikesSettings.x * (tex2D(sunSpikes, sceenCenterUV * float2(aspectRatio * spikesSettings.y, 1.0) * spikesSettings.z * sunGlareScale + 0.5).rgb); 
				
				for (int j = 0; j < 4; ++j)
				{			
					ghosts += ghost1Settings[j].x * (tex2D(sunGhost1, (sceenCenterUV + (toScreenCenter * ghost1Settings[j].w)) * 
							  float2(aspectRatio * ghost1Settings[j].y, 1.0) * ghost1Settings[j].z + 0.5).rgb);

					ghosts += ghost2Settings[j].x * (tex2D(sunGhost2, (sceenCenterUV + (toScreenCenter * ghost2Settings[j].w)) * 
							  float2(aspectRatio * ghost2Settings[j].y, 1.0) * ghost2Settings[j].z + 0.5).rgb);

					ghosts += ghost3Settings[j].x * (tex2D(sunGhost3, (sceenCenterUV + (toScreenCenter * ghost3Settings[j].w)) * 
							  float2(aspectRatio * ghost3Settings[j].y, 1.0) * ghost3Settings[j].z + 0.5).rgb);
				}	

				ghosts = ghosts * smoothstep(0.0, 1.0, 1.0 - length(toScreenCenter));	

				outputColor += sunColor;
				outputColor += ghosts;
				outputColor *= sunGlareFade;
				outputColor *= obstacle;

				// TODO : Use keywords for that kind of settings...
				if (useRadiance > 0.0)
				{
					outputColor = OuterRadiance_SunGlare(outputColor);
				}

				if (useAtmosphereColors > 0.0)
				{
					float3 WCP = _Globals_WorldCameraPos;
					float3 WCP_A = WCP + _Atmosphere_Origin; // Current camera position with offset applied...
					float3 WSD = normalize(sunPosition - WCP_A);

					outputColor *= Extinction_SunGlare(WCP_A, WSD);
				}
				
				diffuse = float4(outputColor, 1.0);				
			}			
			ENDCG
		}
	}
}