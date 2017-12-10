Shader "SpaceEngine/Galaxy/Sprite"
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
				
		uniform float4x4 _Globals_WorldToCamera;
		uniform float4x4 _Globals_CameraToScreen;

		uniform StructuredBuffer<GalaxyParticle> data;
		
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

			uint id : SV_InstanceID;
		};
		
		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 color : TEXCOORD1;

			uint id : SV_InstanceID;
		};
		ENDCG

		Pass
		{
			Name "Particle Dust (Gas)"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend OneMinusSrcColor One
			Cull Back
			ZWrite Off
			ZTest Off

			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			void vert(in appdata v, out v2g o)
			{
				uint id = v.id;
				float3 particlePosition = data[id].position;
				float4 particleColor = data[id].color;
				float particleSize = data[id].size * (gasSize * 2.0f);
				float3 particleId = data[id].id;

				o.vertex = mul(mul(_Globals_CameraToScreen, _Globals_WorldToCamera), float4(particlePosition, 1.0f));
				o.uv = float2(0.25f, 0.25f);
				o.size = particleSize / _Particle_Absolute_Size;
				o.color = particleColor;
				o.id = particleId.x * particleId.y;
			}
		
			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<v2f> triStream)
			{
				float4 particlePosition = p[0].vertex;
				float2 starUV = p[0].uv;
			
				static const float4 up = float4(0.0f, 1.0f, 0.0f, 0.0f) * -_Globals_CameraToScreen._22;
				static const float4 right = float4(1.0f, 0.0f, 0.0f, 0.0f) * _Globals_CameraToScreen._11;

				float halfSize = p[0].size / 2.0f;
			
				v2f o;
			
				o.color = p[0].color;
				o.id = p[0].id;
			
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
				float2 samplerUV = i.uv / 0.5f - 0.5f;
				float4 particleColor = i.color;
				float4 particleSampler = tex2D(_Particle, Rotate2d(i.id, float3(samplerUV - 0.5f, 0.0f)).xy + 0.5f).a;

				particleColor = float4(i.color.xyz * gasMultiplicationColor, 1.0f);

				color = -((particleSampler * particleColor) * (gasStrength * 32.0f));
				color.a = dot(color.xyz, 1.0f);
			}
			ENDCG
		}
	}
}