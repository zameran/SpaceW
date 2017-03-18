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

		#include "Core.cginc"

		#define BORDER 2.0 

		uniform float _Level;

		uniform float4 _TileWSD;
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
			// TODO : Check it out! Test it!	
			float u = (0.5 + BORDER) / (_TileWSD.x - 1 - BORDER * 2);
			float2 vert = (IN.uv0 * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
			//float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			float3 v = p;
			
			float slope = texTile(_Normals_Tile, IN.uv0.xy, _Normals_TileCoords, _Normals_TileSize).w;
			float height = texTile(_Elevation_Tile, IN.uv0.xy, _Elevation_TileCoords, _Elevation_TileSize).w;

			slope = saturate((2.0 * slope - 0.5) * smoothstep(4, 8, _Level)); // NOTE : Limit slope in case of very strong normals on low LOD levels...
			height = saturate(height);

			//float3 color = ColorMapAsteroid(v, height, slope);
			float3 color = ColorMapPlanet(v, height, slope);
			//float3 color = ColorMapSelena(v, height,  slope);
			//float3 color = ColorMapTerra(v, height, slope);
			
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