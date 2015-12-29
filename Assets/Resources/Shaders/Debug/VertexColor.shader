Shader "Debug/Vertex color" 
{
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target { return i.color; }
			ENDCG
		}
	}
}