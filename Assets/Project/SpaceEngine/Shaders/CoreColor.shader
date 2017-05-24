Shader "SpaceEngine/Terrain/CoreColor" 
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

		#define CORE_PORDUCER_ADDITIONAL_UV
		#define BORDER 2.0	// Tile border size

		#include "Core.cginc"

		uniform sampler2D _ElevationSampler;
		uniform float4 _ElevationOSL;
		uniform sampler2D _NormalsSampler;
		uniform float4 _NormalsOSL;

		uniform float _Level;

		uniform float4 _TileWSD;
		uniform float2 _TileSD;

		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		CORE_PRODUCER_VERTEX_PROGRAM(_TileWSD.x)

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{
			float u = (0.5 + BORDER) / (_TileWSD.x - 1 - BORDER * 2);
			float2 vert = (IN.uv0 * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
			//float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			
			float slope = tex2D(_NormalsSampler, IN.uv0 + _NormalsOSL.xy).w;
			float height = tex2D(_ElevationSampler, IN.uv0 + _ElevationOSL.xy).w;

			slope = saturate((2.0 * slope - 0.5) * smoothstep(4, 8, _Level)); // NOTE : Limit slope in case of very strong normals on low LOD levels...
			height = saturate(height);

			//float3 color = ColorMapAsteroid(p, height, slope);
			float3 color = ColorMapPlanet(p, height, slope);
			//float3 color = ColorMapSelena(p, height,  slope);
			//float3 color = ColorMapTerra(p, height, slope);
			
			output = float4(saturate(color), 1);
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