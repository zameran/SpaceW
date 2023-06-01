Shader "SpaceEngine/Producers/CoreColor" 
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

		#define CORE_PRODUCER_ADDITIONAL_UV
		//#define BORDER 2.0

		#include "../../Core.cginc"

		uniform sampler2D _ElevationSampler;
		uniform float4 _ElevationOSL;
		uniform sampler2D _NormalsSampler;
		uniform float4 _NormalsOSL;

		uniform float _Level;

		uniform float4 _TileWSD;
		uniform float2 _TileSD;

		uniform float4 _Offset;
		uniform float4x4 _LocalToWorld;

		float4 ColorMapTempHum(float3 ppoint, float height, float slope)
		{
			noiseOctaves    = 6.0;
			noiseH          = 0.5;
			noiseLacunarity = 2.218281828459;
			noiseOffset     = 0.8;
			float climate, latitude, dist;
			float vary;
			if (tidalLock <= 0.0)
			{
				latitude = abs(normalize(ppoint).y);
				latitude += 0.15 * (Fbm(ppoint * 0.007 + Randomize) - 1.0);
				latitude = saturate(latitude);
				if (latitude < latTropic - tropicWidth)
					climate = lerp(climateTropic, climateEquator, (latTropic - tropicWidth - latitude) / latTropic);
				else if (latitude > latTropic + tropicWidth)
					climate = lerp(climateTropic, climatePole, (latitude - latTropic - tropicWidth) / (1.0 - latTropic));
				else
					climate = climateTropic;
			}
			else
			{
				latitude = 1.0 - normalize(ppoint).x;
				latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
				climate = lerp(climateTropic, climatePole, saturate(latitude));
			}

			noiseOctaves    = 5.0;
			noiseLacunarity = 3.5;
			vary = Fbm(ppoint * 17000 + Randomize);
			float snowLine   = height + 0.25 * vary * slope;
			float montHeight = saturate((height - seaLevel) / (snowLevel - seaLevel));
			climate = min(climate + 0.5 * heightTempGrad * montHeight, climatePole - 0.125);
			climate = lerp(climate, climatePole, saturate((snowLine - (1.0 - snowLevel)) * 100.0));

			float beach = saturate((height / seaLevel - 1.0) * 50.0);
			climate = lerp(0.375, climate, beach);

			float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0);
			climate = lerp(climate, climatePole, iceCap);

			float3 p = ppoint * mainFreq + Randomize;

			noiseOctaves    = 4.0;
			noiseLacunarity = 2.218281828459;
			float3  pp = (ppoint + Randomize) * (0.0005 * hillsFreq / (hillsMagn * hillsMagn));
			float fr = 0.20 * (1.5 - RidgedMultifractal(pp,         2.0)) +
					   0.05 * (1.5 - RidgedMultifractal(pp * 10.0,  2.0)) +
					   0.02 * (1.5 - RidgedMultifractal(pp * 100.0, 2.0));
			p = ppoint * (colorDistFreq * 0.005) + float3(fr, fr, fr);
			p += Fbm3D(p * 0.38) * 1.2;
			vary = Fbm(p) * 0.35 + 0.245;
			climate += vary * beach * saturate(1.0 - 3.0 * slope) * saturate(1.0 - 1.333 * climate);

			float humidity = 1.0 - climate;
			float temperature = 1.0 - (latitude + (vary * 0.75));

			temperature -= slope * 0.5;

			float4 color = tex2D(PlanetColorMap, float2(humidity, temperature));

			return color;
		}

		float3 ColorFunction(float3 ppoint, float height, float slope)
		{
			#if TC_NONE
				return 0;
			#endif

			#if TC_ASTEROID
				return ColorMapAsteroid(ppoint, height, slope);
			#endif

			#if TC_PLANET
				return ColorMapPlanet(ppoint, height, slope);
				//return ColorMapTempHum(ppoint, height, slope);
			#endif

			#if TC_SELENA
				return 0;	// TODO : Finish it!
			#endif

			#if TC_TERRA
				return 0;	// TODO : Finish it!
			#endif

			#if TC_GASGIANT
				return 0;	// TODO : Finish it!
			#endif

			#if TC_TEST
				return float3(height, slope, 1.0);
			#endif
		}

		CORE_PRODUCER_VERTEX_PROGRAM(_TileWSD.x)

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{
			//float u = (0.5 + BORDER) / (_TileWSD.x - 1 - BORDER * 2);
			//float2 vert = (IN.uv0 * (1.0 + u * 2.0) - u) * _Offset.z + _Offset.xy;
			float2 vert = (IN.uv0 * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;
				
			float3 P = float3(vert, _Offset.w);
			float3 p = normalize(mul(_LocalToWorld, P)).xyz;
			
			float4 elevationData = tex2D(_ElevationSampler, IN.uv0 + _ElevationOSL.xy);
			float4 normalData = tex2D(_NormalsSampler, IN.uv0 + _NormalsOSL.xy);

			float slope = elevationData.z;
			float height = elevationData.w;

			//slope = saturate(((slope + 1.0) * 0.5));
			slope = saturate(slope);
			height = saturate(height);

			//float3 color = ColorMapAsteroid(p, height, slope);
			//float3 color = ColorMapPlanet(p, height, slope);
			//float3 color = ColorMapSelena(p, height,  slope);
			//float3 color = ColorMapTerra(p, height, slope);

			float3 color = ColorFunction(p, height, slope);
			
			output = float4(saturate(color), 1.0);
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

			#pragma multi_compile TC_NONE TC_ASTEROID TC_PLANET TC_SELENA TC_TERRA TC_GASGIANT TC_TEST
			ENDCG
		}
	}
}