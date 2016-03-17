Shader "Proland/Atmo/Sky" 
{
	Properties
	{
		_Sun_Glare("Sun Glare", 2D) = "black" {}
		_Sun_Glare_Scale("Sun Glare Scale", Float) = 1.0
		_Sun_Glare_Color("Sun Glare Color", Color) = (1, 1, 1, 1)
	}
	SubShader 
	{
		Tags { "Queue" = "Geometry-1" }
	
		Pass 
		{
			ZWrite On
			ZTest Always  
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha

			Cull Off

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
			uniform float _Sun_Glare_Scale;
			uniform float4 _Sun_Glare_Color;
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
			
			float3 OuterSunRadiance(float3 viewdir)
			{
				float3 data = viewdir.z > 0.0 ? (tex2D(_Sun_Glare, float2(0.5, 0.5) + viewdir.xy / _Sun_Glare_Scale).rgb * _Sun_Glare_Color) : float3(0, 0, 0);

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