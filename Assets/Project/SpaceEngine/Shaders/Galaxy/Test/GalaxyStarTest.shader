Shader "SpaceEngine/Galaxy/StarTest"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "PreviewType" = "Plane" }

		CGINCLUDE
		#include "../../Core.cginc"
		#include "../Galaxy.cginc"
		
		uniform sampler2D _Particle;
		uniform float _Particle_Absolute_Size;

		uniform StructuredBuffer<GalaxyStar> data;

		static const float2 tab[8] = 
		{
			float2(0.897907815, -0.347608525),
			float2(0.550299290, 0.273586675),
			float2(0.823885965, 0.098853070),
			float2(0.922739035, -0.122108860),
			float2(0.800630175, -0.088956800),
			float2(0.711673375, 0.158864420),
			float2(0.870537795, 0.085484560),
			float2(0.956022355, -0.058114540),
		};
		
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

		float GetFlickerAmount(in float2 pos)
		{
			float2 hash = frac(pos.xy * 256);
			float index = frac(hash.x + (hash.y + 1) * (_Time.x * 2 + unity_DeltaTime.z));
			
			index *= 8;
			
			float f = frac(index) * 2.5;
			int i = (int)index;
			
			return tab[i].x + f * tab[i].y;
		}

		void vert(in appdata v, out v2g o)
		{
			float3 starPosition = data[v.id].position;
			float4 starColor = data[v.id].color;
			float starSize = data[v.id].size;
			float starTemperature = data[v.id].temperature;

			float magnitude = 6.5 + length(starColor) * (-1.44 - 1.5);
			float brightness = GetFlickerAmount(starPosition.xy * starSize) * pow(5.0, (-magnitude - 1.44) / 2.5);

			o.vertex = UnityObjectToClipPos(float4(starPosition, 1.0));
			o.uv = float2(0.25, 0.25);
			o.size = starSize / _Particle_Absolute_Size;
			o.color = 8 * brightness * starColor * 3;
		}

		void vert_debug(in appdata v, out v2g o)
		{
			float3 starPosition = data[v.id].position;
			float4 starColor = data[v.id].color;
			float starSize = data[v.id].size;
			float starTemperature = data[v.id].temperature;

			o.vertex = UnityObjectToClipPos(float4(starPosition, 1.0));
			o.uv = float2(0.25, 0.25);
			o.size = starSize / _Particle_Absolute_Size;
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
			float4 starSampler = tex2D(_Particle, i.uv / 0.5 - 0.5).a;
			float2 starUv = 6.5 * i.uv - 6.5 * float2(0.5, 0.5);
			
			half scale = exp(-dot(starUv, starUv));

			starColor = float4(i.color.xyz * scale + 0.25 * i.color.w * pow(scale, 10.0), 1.0);

			color = hdr(starSampler + starColor);
			color.a = dot(color.xyz, 1.0);
		}

		void frag_debug(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor = i.color;
			
			color = starColor;
			color.a = dot(color.xyz / M_PI, 1.0);
		}
		ENDCG

		Pass
		{
			Name "Star"

			Blend SrcAlpha One
			Cull Front
			ZWrite Off
			
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}

		Pass
		{
			Name "Star (Debug)"

			Blend One One
			Cull Front
			ZWrite On
			
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert_debug
			#pragma fragment frag_debug
			ENDCG
		}
	}
}