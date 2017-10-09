Shader "SpaceEngine/Producers/CoreElevation" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "../../TCCommon.cginc"
		#include "../../TCAsteroid.cginc"
		#include "../../TCGasgiant.cginc"
		#include "../../TCPlanet.cginc"
		#include "../../TCSelena.cginc"
		#include "../../TCSun.cginc"
		#include "../../TCTerra.cginc"

		#define CORE_PORDUCER_ADDITIONAL_UV
		#define BORDER 2.0

		#include "../../Core.cginc"

		uniform sampler2D _CoarseLevelSampler;		// Coarse level texture
		uniform float4 _CoarseLevelOSL;				// Lower left corner of patch to upsample, one over size in pixels of coarse level texture, layer id

		uniform float4 _TileWSD;
		uniform float2 _TileSD;

		uniform float _Amplitude;
		uniform float _Frequency;
		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		CORE_PRODUCER_VERTEX_PROGRAM(_TileWSD.x)

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{			
			//float u = (0.5 + BORDER) / (_TileWSD.x - 1 - BORDER * 2);
			//float2 vert = (IN.uv0 * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
			float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;

			float2 p_uv = floor(IN.uv1) * 0.5;
			float2 uv = (p_uv - frac(p_uv) + float2(0.5, 0.5)) * _CoarseLevelOSL.z + _CoarseLevelOSL.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			float3 v = p * _Frequency;
			
			//float noise = HeightMapAsteroid(v);
			float noise = HeightMapPlanet(v);
			//float noise = HeightMapSelena(v);
			//float noise = HeightMapTerra(v);

			//float noise = sNoise(v);
			
			float height = _Amplitude * noise;
			float preHeight = height;

			float4x4 coarseLevelHeights = SampleCoarseLevelHeights(_CoarseLevelSampler, uv, _CoarseLevelOSL);
			int i = int(dot(frac(p_uv), float2(2.0, 4.0)));
			float3 n = float3(mdot(coarseLevelHeights, slopexMatrix[i]), mdot(coarseLevelHeights, slopeyMatrix[i]), 2.0 * _TileWSD.y);
			float slope = length(n.xy) / n.z;
			//float curvature = mdot(coarseLevelHeights, curvatureMatrix[i]) / _TileWSD.y;
			//float noiseAmp = max(clamp(4.0 * curvature, 0.0, 1.5), clamp(2.0 * slope - 0.5, 0.1, 4.0));

			if (_CoarseLevelOSL.x != -1.0)
			{
				float4 uvc = float4(BORDER + 0.5, BORDER + 0.5, BORDER + 0.5, BORDER + 0.5);

				uvc += _TileWSD.z * floor((floor(IN.uv1 - float2(BORDER, BORDER)) / (2.0 * _TileWSD.z)).xyxy + float4(0.5, 0.0, 0.0, 0.5));
					
				float zc1 = tex2Dlod(_CoarseLevelSampler, float4(uvc.xy * _CoarseLevelOSL.z + _CoarseLevelOSL.xy, 0.0, 0.0)).x;
				float zc3 = tex2Dlod(_CoarseLevelSampler, float4(uvc.zw * _CoarseLevelOSL.z + _CoarseLevelOSL.xy, 0.0, 0.0)).x;
					
				preHeight = (zc1 + zc3) * 0.5;
			}
							
			output = float4(height, preHeight, 2.0 * slope - 0.5, noise);		
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