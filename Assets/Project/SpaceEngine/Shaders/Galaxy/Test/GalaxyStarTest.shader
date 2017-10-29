Shader "SpaceEngine/Galaxy/StarTest"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#include "../Galaxy.cginc"

			StructuredBuffer<GalaxyStar> stars;

			struct appdata
			{
				uint id : SV_VertexID;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
			};
			
			void vert(in appdata v, out v2f o)
			{
				float3 starPosition = stars[v.id].position;
				float4 starColor = stars[v.id].color;
				float starSize = stars[v.id].size;
				float starTemperature = stars[v.id].temperature;

				o.vertex = UnityObjectToClipPos(float4(starPosition, 1.0));
				o.color = starColor;
			}
			
			void frag(in v2f i, out float4 color : SV_Target)
			{
				float4 starColor = i.color;

				color = starColor;
			}
			ENDCG
		}
	}
}
