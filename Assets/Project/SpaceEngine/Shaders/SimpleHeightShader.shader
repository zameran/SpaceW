Shader "Custom/SimpleHeightShader" 
{
	Properties 
	{
		_Freq("Frequency", Float) = 1
		_Speed("Speed", Float) = 1
	}
	SubShader 
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#include "TCCommon.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 srcPos : TEXCOORD0;
			};

			uniform float _Freq;
			uniform float _Speed;

			v2f vert(float4 objPos : POSITION)
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MVP, objPos);

				o.srcPos = mul(_Object2World, objPos).xyz;
				o.srcPos *= _Freq;
				o.srcPos.y += _Time.y * _Speed;

				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float4 color;
				float ns = Cell2NoiseColor(i.srcPos, color);
				return color;
			}
			ENDCG
		}
	} 
}