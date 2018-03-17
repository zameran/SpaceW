Shader "Render Depth" 
{
	SubShader 
	{
		Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" }

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 position : SV_POSITION;
				float4 positionClip : TEXCOORD0;
				float3 positionView : TEXCOORD1;
				float2 depth : TEXCOORD2;
			};

			struct f2a
			{
				float4 color : COLOR;
				float depth : DEPTH;
			};

			v2f vert (appdata_base v) 
			{
				v2f o;

				o.position = UnityObjectToClipPos(v.vertex);
				o.positionClip = o.position;
				o.positionView = UnityObjectToViewPos(v.vertex);
				o.depth = o.position.zw;

				return o;
			}

			f2a frag(v2f i) 
			{
				f2a o;

				float C = 1;
				float offset = 2.0;
				float scale = 8.0;

				o.color = abs(i.positionView.z) / scale;
				o.depth = (log(C * i.positionClip.z + offset) / log(C * _ProjectionParams.z + offset));

				return o;
			}
			ENDCG
		}
	}
}