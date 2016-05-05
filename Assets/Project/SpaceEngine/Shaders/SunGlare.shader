Shader "Proland/Atmo/SunFlare" 
{
	SubShader 
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
	
		Pass 
		{
			ZWrite Off
			ZTest Off
			cull off

			Blend One OneMinusSrcColor

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Atmosphere.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl

			uniform float3 _Globals_Origin;
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;

			uniform float sunGlareScale;
			
			uniform sampler2D sunSpikes;
			uniform sampler2D sunFlare;
			uniform sampler2D sunGhost1;
			uniform sampler2D sunGhost2;
																									
			uniform float3 flareSettings;
			uniform float3 spikesSettings;
			
			uniform float4x4 ghost1Settings;
			uniform float4x4 ghost2Settings;
			
			uniform float useTransmittance;
		
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

			float4 frag(v2f IN) : COLOR
			{
				float3 WSD = _Sun_WorldSunDir_1;
				float3 WCP = _Globals_WorldCameraPos;
				
				float2 toScreenCenter = sunViewPortPos.xy - 0.5;

				float3 sunColor = 0;
				float3 ghosts = 0;

				sunColor += flareSettings.x * (tex2D(sunFlare, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(aspectRatio * flareSettings.y, 1.0) * flareSettings.z * sunGlareScale + 0.5).rgb);
				sunColor += spikesSettings.x * (tex2D(sunSpikes, (IN.uv.xy - sunViewPortPos.xy) * 
							float2(aspectRatio * spikesSettings.y , 1.0) * spikesSettings.z * sunGlareScale + 0.5).rgb); 
				
				for (int i = 0; i < 4; ++i)
				{			
					ghosts += ghost1Settings[i].x * 
							  (tex2D(sunGhost1,(IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost1Settings[i].w)) * 
							  float2(aspectRatio * ghost1Settings[i].y, 1.0) * ghost1Settings[i].z + 0.5).rgb);
				}
					
				for (int j = 0; j < 4; ++j)
				{
					ghosts += ghost2Settings[j].x * 
							  (tex2D(sunGhost2,(IN.uv.xy - sunViewPortPos.xy + (toScreenCenter * ghost2Settings[j].w)) * 
							  float2(aspectRatio * ghost2Settings[j].y, 1.0) * ghost2Settings[j].z + 0.5).rgb);
				}
				
				float3 extinction = 1; //TODO: Inside atmosphere glare coloring.

				ghosts *= smoothstep(0.0, 1.0, 1.0 - length(toScreenCenter));		
				sunColor += ghosts;		
				sunColor = (useTransmittance > 0.0) ? sunColor * extinction : sunColor;

				return float4(sunColor, 1.0);				
			}			
			ENDCG
		}
	}
}