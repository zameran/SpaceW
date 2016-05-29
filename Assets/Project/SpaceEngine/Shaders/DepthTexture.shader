Shader "SpaceEngine/Depth/DepthTexture" 
{
	SubShader 
	{
	    Tags { "RenderType" = "Opaque" }

	    Pass 
	    {
	        Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
	 
			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 depth : TEXCOORD0;
			};
	 
			v2f vert (appdata v) 
			{
			    v2f o;

			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.depth = o.pos.zw;

				//UNITY_TRANSFER_DEPTH(o.depth);

			    return o;
			}
	 
			float4 frag(v2f i) : COLOR//SV_Target
			{
			    return i.depth.x / i.depth.y;
				//UNITY_OUTPUT_DEPTH(i.depth);
			}
			ENDCG
	    }
	}
}