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
			ZTest Always

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
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
			};

			uniform float _Freq;

			v2f vert (data v)
			{
			    v2f o;

			    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.uv = v.uv * _Freq;

			    return o;
			}

			float NoiseFunction(float3 pos)
			{
				return Noise(pos);
			}

			float3 FindNormal(float3 pos, float u)
			{
				float3 offsets[4];
				float hts[4];

				offsets[0] = pos + float3(-u, 0, 0);
				offsets[1] = pos + float3(u, 0, 0);
				offsets[2] = pos + float3(0, -u, 0);
				offsets[3] = pos + float3(0, u, 0);

				for(int i = 0; i < 4; i++)
				{
					hts[i] = NoiseFunction(offsets[i]);
				}

				float3 _step = float3(1, 0, 1);
			   
				float3 va = normalize(float3(_step.xy, hts[1] - hts[0]));
				float3 vb = normalize(float3(_step.yx, hts[3] - hts[2]));
			   
				return cross(va, vb); //you may not need to swizzle the normal
			}

			float4 frag(v2f i) : COLOR
			{
				float v = NoiseFunction(i.uv);
				
				return float4(v, v, v, 1);
				//return float4(FindNormal(i.uv, 1), 1);
			}
			ENDCG
		}
	} 
}