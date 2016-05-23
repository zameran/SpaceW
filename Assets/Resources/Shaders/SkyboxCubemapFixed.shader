Shader "Skybox/CubemapFixed" 
{
	Properties 
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		[NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
	}

	SubShader 
	{
		Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }

		Cull Off 
		ZWrite Off

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			samplerCUBE _Tex;
			half4 _Tex_HDR;
			half4 _Tint;
			half _Exposure;

			struct appdata_t 
			{
				float4 vertex : POSITION;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;

				o.vertex = float4(v.vertex.xy, 1.0, 1.0);
				o.texcoord = v.vertex.xyz;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				half4 tex = texCUBE (_Tex, i.texcoord);
				half3 c = DecodeHDR (tex, _Tex_HDR);

				c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
				c *= _Exposure;

				return half4(c, 1);
			}
			ENDCG
		}
	}
	Fallback Off
}