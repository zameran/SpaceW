Shader "Debug/UV 1" 
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
				float4 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(appdata v) 
			{
				v2f o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = float4(v.texcoord.xy, 0, 0);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 c = frac(i.uv);

				if (any(saturate(i.uv) - i.uv))
					c.b = 0.5;

				return c;
			}
			ENDCG
		}
	}
}