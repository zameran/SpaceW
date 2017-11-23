Shader "SpaceEngine/Galaxy/Star"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "PreviewType" = "Plane" }

		CGINCLUDE
		#include "Galaxy.cginc"
		
		uniform sampler2D _Particle;
		uniform float _Particle_Absolute_Size;

		uniform StructuredBuffer<GalaxyParticle> data;

		static const float2 FlickerTable[8] = 
		{
			float2(0.897907815f, -0.347608525f),
			float2(0.550299290f, 0.273586675f),
			float2(0.823885965f, 0.098853070f),
			float2(0.922739035f, -0.122108860f),
			float2(0.800630175f, -0.088956800f),
			float2(0.711673375f, 0.158864420f),
			float2(0.870537795f, 0.085484560f),
			float2(0.956022355f, -0.058114540f),
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

		inline float GetFlickerAmount(in float2 pos)
		{
			float2 hash = frac(pos.xy * 256.0f);
			float index = frac(hash.x + (hash.y + 1.0f) * (_Time.x * 2.0f + unity_DeltaTime.z));
			
			index *= 8.0f;
			
			float f = frac(index) * 2.5f;
			int i = (int)index;
			
			return FlickerTable[i].x + f * FlickerTable[i].y;
		}

		void vert(in appdata v, out v2g o)
		{
			float3 particlePosition = data[v.id].position;
			float4 particleColor = data[v.id].color;
			float particleSize = data[v.id].size;

			float magnitude = 6.5 + length(particleColor) * (-1.44f - 1.5f);
			float brightness = GetFlickerAmount(particlePosition.xy * particleSize) * pow(5.0f, (-magnitude - 1.44f) / 2.5f);

			o.vertex = UnityObjectToClipPos(float4(particlePosition, 1.0f));
			o.uv = float2(0.25f, 0.25f);
			o.size = particleSize / _Particle_Absolute_Size;
			o.color = 8.0f * brightness * particleColor * 3.0f;
		}

		void vert_debug(in appdata v, out v2g o)
		{
			float3 particlePosition = data[v.id].position;
			float4 particleColor = data[v.id].color;
			float particleSize = data[v.id].size;

			o.vertex = UnityObjectToClipPos(float4(particlePosition, 1.0f));
			o.uv = float2(0.25f, 0.25f);
			o.size = particleSize / _Particle_Absolute_Size;
			o.color = particleColor;
		}
		
		[maxvertexcount(4)]
		void geom(point v2g p[1], inout TriangleStream<v2f> triStream)
		{
			float4 particlePosition = p[0].vertex;
			float2 starUV = p[0].uv;
			
			static const float4 up = float4(0.0f, 1.0f, 0.0f, 0.0f) * -UNITY_MATRIX_P._22;
			static const float4 right = float4(1.0f, 0.0f, 0.0f, 0.0f) * UNITY_MATRIX_P._11;

			float halfSize = p[0].size / 2.0f;
			
			v2f o;
			
			o.color = p[0].color;
			
			o.vertex = particlePosition - halfSize * up;
			o.uv = float2(1.0f, 0.0f) * 0.5f + starUV;
			triStream.Append(o);
			
			o.vertex = particlePosition + halfSize * right;
			o.uv = float2(1.0f, 1.0f) * 0.5f + starUV;
			triStream.Append(o);
			
			o.vertex = particlePosition - halfSize * right;
			o.uv = float2(0.0f, 0.0f) * 0.5f + starUV;
			triStream.Append(o);
			
			o.vertex = particlePosition + halfSize * up;
			o.uv = float2(0.0f, 1.0f) * 0.5f + starUV;
			triStream.Append(o);
			
			triStream.RestartStrip();
		}
		
		void frag(in v2f i, out float4 color : SV_Target)
		{
			float4 particleColor = i.color;
			float4 particleSampler = tex2D(_Particle, i.uv / 0.5f - 0.5f).a;
			float2 particleUV = 6.5f * i.uv - 6.5f * float2(0.5f, 0.5f);
			
			half scale = exp(-dot(particleUV, particleUV));

			particleColor = float4(i.color.xyz * scale + 0.25f * i.color.w * pow(scale, 10.0f), 1.0f);

			color = particleSampler + particleColor;
			color.a = dot(color.xyz, 1.0f);
		}

		void frag_debug(in v2f i, out float4 color : SV_Target)
		{
			float4 particleColor = i.color;
			
			color = particleColor;
			color.a = dot(color.xyz / M_PI, 1.0f);
		}
		ENDCG

		Pass
		{
			Name "Star"

			Blend SrcAlpha One
			Cull Back
			ZWrite Off
			
			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}

		Pass
		{
			Name "Star (Debug)"

			Blend One One
			Cull Back
			ZWrite On
			
			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert_debug
			#pragma fragment frag_debug
			ENDCG
		}
	}
}