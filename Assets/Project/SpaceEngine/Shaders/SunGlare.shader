Shader "SpaceEngine/Atmosphere/Sunglare"
{
	SubShader 
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	
		Pass 
		{
			ZWrite Off
			ZTest Off
			Cull Off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Atmosphere.cginc"
			#include "HDR.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float4x4 _Sun_WorldToLocal_1;
			uniform float3 _Sun_Position;

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
			
			uniform float useAtmosphereColors;
			uniform float Eclipse;
		
			uniform float3 sunViewPortPos;

			uniform float AspectRatio;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 dir : TEXCOORD0;
				float3 relativeDir : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = float4(v.vertex.xyz, 1.0);
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
				OUT.relativeDir = mul(_Sun_WorldToLocal_1, OUT.dir); 
				OUT.uv = v.texcoord.xy;

				return OUT;
			}

			float3 OuterSunGlareRadiance(float3 viewdir, float3 sunColor)
			{
				float3 data = viewdir.z > 0.0 ? sunColor : float3(0, 0, 0);

				return pow(max(0, data), 2.2) * 2;
			}

			float3 Extinction(float3 camera, float3 p)
			{
				float3 viewdir = p;

				float r = length(camera);
				float rMu = dot(camera, viewdir);
				float mu = rMu / r;
				float d = length(viewdir - camera);
				float deltaSq = SQRT(rMu * rMu - r * r + Rt * Rt, 1e30);
				float din = max(-rMu - deltaSq, 0.0);

				//viewdir = viewdir / d;

				if (din > 0.0 && din < d) 
				{
					camera += din * viewdir;
					rMu += din;
					mu = rMu / Rt;
					r = Rt;
					d -= din;
				}
 
				return (r > Rt) ? float3(1.0, 1.0, 1.0) : Transmittance(r, mu);
			}

			float4 frag(v2f IN) : COLOR
			{
				float3 WSD = _Sun_WorldSunDir_1;
				float3 WCP = _Globals_WorldCameraPos;
				float3 d = normalize(IN.dir);

				float2 toScreenCenter = sunViewPortPos.xy - 0.5;

				float r = length(WCP);

				float3 outputColor = 0;
				float3 sunColor = 0;
				float3 ghosts = 0;

				sunColor += flareSettings.x * (tex2D(sunFlare, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(AspectRatio * flareSettings.y, 1.0) * flareSettings.z * Scale + 0.5).rgb);
				sunColor += spikesSettings.x * (tex2D(sunSpikes, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(AspectRatio * spikesSettings.y, 1.0) * spikesSettings.z * Scale + 0.5).rgb); 
				
				for (int i = 0; i < 4; ++i)
				{			
					ghosts += ghost1Settings[i].x * 
							  (tex2D(sunGhost1, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost1Settings[i].w)) * 
							  float2(AspectRatio * ghost1Settings[i].y, 1.0) * ghost1Settings[i].z + 0.5).rgb);
				}
				
				for (int j = 0; j < 4; ++j)
				{
					ghosts += ghost2Settings[j].x * 
							  (tex2D(sunGhost2, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost2Settings[j].w)) * 
							  float2(AspectRatio * ghost2Settings[j].y, 1.0) * ghost2Settings[j].z + 0.5).rgb);
				}

				for (int k = 0; k < 4; ++k)
				{
					ghosts += ghost3Settings[k].x *
							  (tex2D(sunGhost3, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost3Settings[k].w)) * 
							  float2(AspectRatio * ghost3Settings[k].y, 1.0) * ghost3Settings[k].z + 0.5).rgb);
				}		

				float3 extinction = 1;//Extinction(WCP, WCP - _Sun_Positions_1[0]);
				float3 inscatter = 1;

				(useAtmosphereColors > 0.0) ? inscatter = InScattering(WCP, _Sun_Positions_1[0], WSD, extinction, 0.0) : 1;
				//inscatter = (length(inscatter) > 0) ? inscatter : 0;

				ghosts = ghosts * smoothstep(0.0, 1.0, 1.0 - length(toScreenCenter));	

				//if (r <= Rt) {  }
				
				outputColor += sunColor;
				outputColor += ghosts;
				//outputColor *= Fade;
				outputColor = (useAtmosphereColors > 0.0) ? (outputColor * extinction) : outputColor;
				//outputColor = OuterSunGlareRadiance(IN.relativeDir, outputColor);
				//outputColor = (Eclipse > 0.0) ? sunColor : outputColor;

				return float4(outputColor, 0.0);				
			}			
			ENDCG
		}
	}
}