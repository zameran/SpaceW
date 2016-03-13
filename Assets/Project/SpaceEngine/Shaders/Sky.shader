Shader "Proland/Atmo/Sky" 
{
	Properties
	{
		_Sun_Glare("Sun Glare", 2D) = "black" {}
	}
	SubShader 
	{
		Tags { "Queue" = "Overlay" "RenderType"="Transparent" }
	
		Pass 
		{
			ZWrite Off
			//ZTest Always
			cull off

			CGPROGRAM
			#include "UnityCG.cginc"		
			#include "Utility.cginc"
			#include "Atmosphere.cginc"

			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_Origin;
			
			uniform sampler2D _Sun_Glare;
			uniform float4x4 _Sun_WorldToLocal;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 dir : TEXCOORD1;
				float3 relativeDir : TEXCOORD2;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;

				float3x3 wtl = _Sun_WorldToLocal;
				
				// apply this rotation to view dir to get relative viewdir
				OUT.relativeDir = mul(wtl, OUT.dir);
	
				OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}
			
			// assumes sundir=vec3(0.0, 0.0, 1.0)
			float3 OuterSunRadiance(float3 viewdir)
			{
				float3 data = viewdir.z > 0.0 ? tex2D(_Sun_Glare, float2(0.5, 0.5) + viewdir.xy * 4.0).rgb : float3(0, 0, 0);
				return pow(max(0, data), 2.2) * _Sun_Intensity;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float3 WSD = _Sun_WorldSunDir;
				float3 WCP = _Globals_WorldCameraPos;

				float3 d = normalize(IN.dir);

				float3 sunColor = OuterSunRadiance(IN.relativeDir);

				float3 extinction;
				float3 inscatter = SkyRadiance(WCP + _Globals_Origin, d, WSD, extinction, 0.0);

				float3 finalColor = sunColor * extinction + inscatter;
				
				return float4(hdr(finalColor), 1);

			}		
			ENDCG
		}
	}
}