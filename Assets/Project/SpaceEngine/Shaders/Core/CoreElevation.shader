Shader "SpaceEngine/Terrain/CoreElevation" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "../TCCommon.cginc"

		#define BORDER 2.0 

		uniform float _TileSize;	
		uniform float _Frequency;
		uniform float _Amplitude;

		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		void vert(in appdata_base v, out v2f o)
		{	
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;
		}

		void frag(in v2f IN, out float4 output : COLOR)
		{			
			float u = (0.5 + BORDER) / (_TileSize - 1 - BORDER * 2);

			float2 vert = (IN.uv * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			float3 v = p * _Frequency;

			noiseH          = 0.5;
			noiseLacunarity = 2.218281828459;
			noiseOctaves	= 8;

			float noise = 0;

			noise += Fbm(v * 0.25, 2);
			noise += Fbm(v * 0.50, 4);
			noise += Fbm(v * 0.75, 6);
			noise += Fbm(v * 1.00, 8);

			float color = _Amplitude * noise;
							
			output = float4(color, color, 0.0, 0.0);		
		}

		ENDCG

		Pass 
		{
			ZTest Always
			Cull Off 
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag	
			ENDCG
		}
	}
}