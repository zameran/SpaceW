Shader "SpaceEngine/Galaxy/StarTest"
{
	Properties
	{
		
	}
	SubShader
	{
		Pass
		{
			Blend SrcAlpha One
			Cull Front
			ZWrite Off

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#include "../Galaxy.cginc"

			sampler2D _Star_Particle;
			float _Star_Absolute_Size;

			StructuredBuffer<GalaxyStar> stars;

			struct appdata
			{
				uint id : SV_VertexID;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float size : TEXCOORD1;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
			};
			
			void vert(in appdata v, out v2g o)
			{
				float3 starPosition = stars[v.id].position;
				float4 starColor = stars[v.id].color;
				float starSize = stars[v.id].size;
				float starTemperature = stars[v.id].temperature;

				o.vertex = UnityObjectToClipPos(float4(starPosition, 1.0));
				o.uv = float2(0.25, 0.25);
				o.size = starSize / _Star_Absolute_Size;
				o.color = starColor;
			}

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<v2f> triStream)
			{
				float4 starPosition = p[0].vertex;
				float2 starUV = p[0].uv;
				float4 starColor = p[0].color;

				float4 up = float4(0.0, 1.0, 0.0, 0.0) * UNITY_MATRIX_P._22;
				float4 right = float4(1.0, 0.0, 0.0, 0.0) * UNITY_MATRIX_P._11;
				float halfSize = p[0].size / 2.0;

				float4 v[4];
				v[0] = starPosition - halfSize * up;
				v[1] = starPosition + halfSize * right;
				v[2] = starPosition - halfSize * right;
				v[3] = starPosition + halfSize * up;

				v2f o;

				o.color = starColor;

				o.vertex = v[0];
				o.uv = float2(1.0, 0.0) * 0.5 + starUV;
				triStream.Append(o);

				o.vertex = v[1];
				o.uv = float2(1.0, 1.0) * 0.5 + starUV;
				triStream.Append(o);

				o.vertex = v[2];
				o.uv = float2(0.0, 0.0) * 0.5 + starUV;
				triStream.Append(o);

				o.vertex = v[3];
				o.uv = float2(0.0, 1.0) * 0.5 + starUV;
				triStream.Append(o);

				triStream.RestartStrip();
			}
			
			void frag(in v2f i, out float4 color : SV_Target)
			{
				float4 starColor = i.color;
				float4 starSampler = tex2D(_Star_Particle, i.uv).a;
				float2 starUv = i.uv;

				color = starSampler * starColor;
				color.a = dot(color.xyz, 1.0);
			}
			ENDCG
		}
	}
}
