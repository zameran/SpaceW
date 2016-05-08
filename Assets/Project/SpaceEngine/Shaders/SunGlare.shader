Shader "Proland/Atmo/SunFlare" 
{
	SubShader 
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
		Pass 
		{
			ZWrite Off
			ZTest Off
			Cull Off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Atmosphere.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			uniform float3 _Globals_Origin;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;

			uniform float sunGlareScale;
			uniform float sunGlareFade;
			
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
			
			uniform float useTransmittance;
			uniform float eclipse;
		
			uniform float3 sunViewPortPos;
			uniform float aspectRatio;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
				OUT.uv = v.texcoord.xy;

				return OUT;
			}

 			float3 Extinction(float3 camera, float3 viewdir)
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
 
     			return (r > Rt) ? float3(1.0, 1.0, 1.0) : Transmittance(r, mu);
     		}

			float4 frag(v2f IN) : COLOR
			{
				float3 WSD = _Sun_WorldSunDir_1;
				float3 WCP = _Globals_WorldCameraPos;
				
				float2 toScreenCenter = sunViewPortPos.xy - 0.5;

				float3 outputColor = 0;
				float3 sunColor = 0;
				float3 ghosts = 0;

				sunColor += flareSettings.x * (tex2D(sunFlare, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(aspectRatio * flareSettings.y, 1.0) * flareSettings.z * sunGlareScale + 0.5).rgb);
				sunColor += spikesSettings.x * (tex2D(sunSpikes, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(aspectRatio * spikesSettings.y, 1.0) * spikesSettings.z * sunGlareScale + 0.5).rgb); 
				
				for (int i = 0; i < 4; ++i)
				{			
					ghosts += ghost1Settings[i].x * 
							  (tex2D(sunGhost1, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost1Settings[i].w)) * 
							  float2(aspectRatio * ghost1Settings[i].y, 1.0) * ghost1Settings[i].z + 0.5).rgb);
				}
				
				for (int j = 0; j < 4; ++j)
				{
					ghosts += ghost2Settings[j].x * 
							  (tex2D(sunGhost2, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost2Settings[j].w)) * 
							  float2(aspectRatio * ghost2Settings[j].y, 1.0) * ghost2Settings[j].z + 0.5).rgb);
				}

				for (int k = 0; k < 4; ++k)
				{
					ghosts += ghost3Settings[k].x *
							  (tex2D(sunGhost3, (IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost3Settings[k].w)) * 
							  float2(aspectRatio * ghost3Settings[k].y, 1.0) * ghost3Settings[k].z + 0.5).rgb);
				}
				
				float3 extinction = Extinction(WCP, WSD);

				ghosts = ghosts * smoothstep(0.0, 1.0, 1.0 - length(toScreenCenter));		
				
				outputColor += sunColor;
				outputColor += ghosts;
				outputColor *= sunGlareFade;
				outputColor = (useTransmittance > 0.0) ? outputColor * extinction : outputColor;
				//outputColor = (eclipse > 0.0) ? sunColor : outputColor;

				return float4(outputColor, 1.0);				
			}			
			ENDCG
		}
	}
}