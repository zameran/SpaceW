Shader "Hidden/FrameBufferProcess"
{
	Properties
	{
		_CameraFrameBufferTexture("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			uniform sampler2D _CameraFrameBufferTexture;
			float4 _CameraFrameBufferTexture_TexelSize;

			void vert(in a2v v, out v2f o)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				#if UNITY_UV_STARTS_AT_TOP
					if (_CameraFrameBufferTexture_TexelSize.y < 0)
						o.uv.y = 1 - o.uv.y;
				#endif
			}

			void frag(in v2f i, out float4 output : SV_Target)
			{
				float4 fboColor = tex2D(_CameraFrameBufferTexture, i.uv);

				fboColor.a = (fboColor.a + 1.0) * 0.5;
				fboColor.a = saturate(fboColor.a);

				output = fboColor;
			}

			ENDCG
		}
	}
}