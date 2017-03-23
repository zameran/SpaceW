Shader "SpaceEngine/Terrain/CoreElevation" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "TCCommon.cginc"
		#include "TCAsteroid.cginc"
		#include "TCGasgiant.cginc"
		#include "TCPlanet.cginc"
		#include "TCSelena.cginc"
		#include "TCSun.cginc"
		#include "TCTerra.cginc"

		#include "Core.cginc"

		#define BORDER 2.0							// Tile border size

		uniform sampler2D _ResidualSampler;
		uniform float4 _ResidualOSH;

		uniform float4 _TileWSD;
		uniform float2 _TileSD;	

		uniform float _Amplitude;
		uniform float _Frequency;
		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		void vert(in VertexProducerInput v, out VertexProducerOutput o)
		{	
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv0 = v.texcoord.xy;
			o.uv1 = v.texcoord.xy * _TileWSD.x;
		}

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{			
			float u = (0.5 + BORDER) / (_TileWSD.x - 1 - BORDER * 2);
			float2 vert = (IN.uv0 * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
			//float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;

			float2 p_uv = floor(IN.uv1) * 0.5;
			float2 residual_uv = p_uv * _ResidualOSH.z + _ResidualOSH.xy;
			float residual_value = _ResidualOSH.w * tex2D(_ResidualSampler, residual_uv).x;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			float3 v = p * _Frequency;
			
			//float noise = HeightMapAsteroid(v);
			//float noise = HeightMapPlanet(v) - 1.5;
			//float noise = HeightMapSelena(v);
			//float noise = HeightMapTerra(v);

			float noise = sNoise(v);

			noise += residual_value; // Apply residual value!
			
			float height = _Amplitude * noise;
							
			output = float4(height, height, 1, noise);		
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