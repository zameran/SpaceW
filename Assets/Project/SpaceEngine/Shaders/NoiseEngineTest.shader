﻿Shader "SpaceEngine/NoiseEngineTest" 
{
	Properties 
	{
		_Freq("Frequency", Float) = 1
	}
	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "TCCommon.cginc"
			
			struct data
			{
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float3 uv : TEXCOORD0;
			};

			uniform float _Freq;

			v2f vert(data v)
			{
				v2f o;

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				o.uv = mul(_Object2World, v.uv).xyz;
				o.uv *= _Freq;
		
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float v = sNoise(i.uv);

				return float4(v, v, v, 1);
			}
			ENDCG
		}
	} 
}