Shader "SpaceEngine/Galaxy/Screen"
{
	Properties
	{
		
	}
	SubShader
	{
		Pass
		{
			Blend One Zero
			Cull Off
			ZWrite On
			ZTest Always

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _FrameBuffer;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			void vert(in appdata v, out v2f o)
			{
				o.vertex = float4(v.vertex.x, v.vertex.y, 1.0, v.vertex.w);
				o.texcoord = v.texcoord.xy;

				#if UNITY_UV_STARTS_AT_TOP
					o.texcoord.y = 1.0 - o.texcoord.y;
				#endif
			}
			
			void frag(in v2f i, out float4 color : SV_Target)
			{
				color = tex2D(_FrameBuffer, i.texcoord);
			}
			ENDCG
		}
	}
}
