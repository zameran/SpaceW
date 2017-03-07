Shader "SpaceEngine/Terrain/CoreElevation" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "Core.cginc"

		#include "../TCCommon.cginc"
		#include "../TCAsteroid.cginc"
		#include "../TCPlanet.cginc"

		#define BORDER 2.0 

		uniform float _Frequency;
		uniform float _Amplitude;

		uniform float2 _TileSD;	

		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		void vert(in VertexProducerInput v, out VertexProducerOutput o)
		{	
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv0 = v.texcoord.xy;
			o.uv1 = v.texcoord.xy;
		}

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{			
			float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			float3 v = p * _Frequency;

			noiseH          = 0.5;
			noiseLacunarity = 2.218281828459;

			float noise = HeightMapPlanet(v) - 1.5;

			//noise += Fbm(v * 0.25, 2);
			//noise += Fbm(v * 0.50, 4);
			//noise += Fbm(v * 0.75, 6);
			//noise += Fbm(v * 1.00, 8);

			float color = _Amplitude * noise;
							
			output = float4(color, color, 1, noise);		
		}

		ENDCG

		Pass 
		{
			ZTest Always
			Cull Off 
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag	
			ENDCG
		}
	}
}