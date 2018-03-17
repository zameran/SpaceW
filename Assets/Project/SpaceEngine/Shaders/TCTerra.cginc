/* Procedural planet generator.
 *
 * Copyright (C) 2012-2015 Vladimir Romanyuk
 * All rights reserved.
 *
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Vladimir Romanyuk
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2018
 */

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

//-----------------------------------------------------------------------------
float HeightMapTerra(float3 ppoint)
{
	// Assign a climate type
	noiseOctaves = (surfClass == 1.0) ? 5.0 : 12.0;
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;
	float climate, latitude;
	if (tidalLock <= 0.0)
	{
		latitude = abs(normalize(ppoint).y);
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
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

	// Litosphere cells
	//float lithoCells = LithoCellsNoise(ppoint, climate, 1.5);

	// Global landscape
	float3 p = ppoint * mainFreq + Randomize;
	noiseOctaves = 5;
	float3  distort = 0.35 * Fbm3D(p * 0.73);
	noiseOctaves = 4;
	distort += 0.005 * (1.0 - abs(Fbm3D(p * 132.3)));
	float global = 1.0 - Cell3Noise(p + distort);

	// Venus-like structure
	float venus = 0.0;
	if (venusMagn > 0.05)
	{
		distort = Fbm3D(ppoint * 0.3, 4) * 1.5;
		venus = Fbm((ppoint + distort, 6) * venusFreq) * venusMagn;
	}

	global = (global + venus - seaLevel) * 0.5 + seaLevel;
	float shore = saturate(70.0 * (global - seaLevel));

	// Biome domains
	p = p * 2.3 + 13.5 * Fbm3D(p * 0.06, 6);
	float4  col;
	float2  cell = Cell3Noise2Color(p, col);
	float biome = col.r;

	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float terrace = col.g;
	float terraceLayers = max(col.b * 10.0 + 3.0, 3.0);
	terraceLayers += Fbm(p * 5.41);

	float montRage = saturate(DistNoise(ppoint * 22.6 + Randomize, 2.5) + 0.5);
	montRage *= montRage;
	float montBiomeScale = min(pow(2.2 * biomeScale, 2.5), 1.0) * montRage;

	float inv2montesSpiky = 1.0 /(montesSpiky*montesSpiky);
	float heightD = 0.0;
	float height = 0.0;
	float dist;

	if (biome < dunesFraction)
	{
		// Dunes
		dist = dunesFreq + Fbm(p * 1.21, 2);
		//heightD = max(Fbm(p * dist * 0.3) + 0.7, 0.0);
		//heightD = biomeScale * dunesMagn * (heightD + DunesNoise(ppoint, 3));
		heightD = 0.2 * max(Fbm(p * dist * 0.3, 2) + 0.7, 0.0);
		heightD = biomeScale * dunesMagn * (heightD + DunesNoise(ppoint, 3));
	}
	else if (biome < hillsFraction)
	{
		// "Eroded" hills
		noiseOctaves = 10.0;
		noiseH       = 1.0;
		noiseOffset  = 1.0;
		height = biomeScale * hillsMagn * (1.5 - RidgedMultifractal(ppoint * hillsFreq + Randomize, 2.0));
	}
	else if (biome < hills2Fraction)
	{
		// "Eroded" hills 2
		noiseOctaves = 10.0;
		noiseLacunarity = 2.0;
		height = biomeScale * hillsMagn * JordanTurbulence(ppoint * hillsFreq + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
	}
	else if (biome < canyonsFraction)
	{
		// Canyons
		noiseOctaves = 5.0;
		noiseH       = 0.9;
		noiseLacunarity = 4.0;
		noiseOffset  = montesSpiky;
		height = -canyonsMagn * montRage * RidgedMultifractalErodedDetail(ppoint * 4.0 * canyonsFreq * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

		//if (terrace < terraceProb)
		{
			terraceLayers *= 5.0;
			float h = height * terraceLayers;
			height = (floor(h) + smoothstep(0.1, 0.9, frac(h))) / terraceLayers;
		}
	}
	else
	{
		// Mountains
		noiseOctaves = 10.0;
		noiseH       = 1.0;
		noiseLacunarity = 2.0;
		noiseOffset  = montesSpiky;
		height = montesMagn * montRage * RidgedMultifractalErodedDetail(ppoint * montesFreq * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

		if (terrace < terraceProb)
		{
			float h = height * terraceLayers;
			height = (floor(h) + smoothstep(0.0, 1.0, frac(h))) / terraceLayers;
			height *= 0.75; // terracing made slopes too steep; reduce overall mountains height to reduce this effect
		}
	}

	// Mare
	float mare = global;
	float mareFloor = global;
	float mareSuppress = 1.0;

	if (mareSqrtDensity > 0.05)
	{
		//noiseOctaves = 2;
		//mareFloor = 0.6 * (1.0 - Cell3Noise(0.3*p));
		noiseH           = 0.5;
		noiseLacunarity  = 2.218281828459;
		noiseOffset      = 0.8;
		craterDistortion = 1.0;
		noiseOctaves     = 6.0;  // Mare roundness distortion
		mare = MareNoise(ppoint, global, 0.0, mareSuppress);
		//lithoCells *= 1.0 - saturate(20.0 * mare);
	}

	height *= saturate(20.0 * mare);        // suppress mountains, canyons and hill (but not dunes) inside mare
	height = (height + heightD) * shore;    // suppress all landforms inside seas
	//height *= lithoCells;                   // suppress all landforms inside lava seas

	// Ice caps
	float oceaniaFade = (surfClass == 1.0) ? 0.1 : 1.0;
	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0 * oceaniaFade);

	// Ice cracks
	float mask = 1.0;

	if (cracksOctaves > 0.0)
	{
		height += CrackNoise(ppoint, mask) * iceCap;
	}

	// Craters
	float crater = 0.0;
	if (craterSqrtDensity > 0.05)
	{
		heightFloor = -0.1;
		heightPeak  =  0.6;
		heightRim   =  1.0;
		crater = CraterNoise(ppoint, 0.5 * craterMagn, craterFreq, craterSqrtDensity, craterOctaves);
		noiseOctaves    = 10.0;
		noiseLacunarity = 2.0;
		crater = 0.25 * crater + 0.05 * crater * iqTurbulence(ppoint * montesFreq + Randomize, 0.55);
		//crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * montesFreq + Randomize, 2.0, erosion, 0.25 * crater);
	}

	height += mare + crater;

	// Pseudo rivers
	if (riversOctaves > 0)
	{
		noiseLacunarity = 2.218281828459;
		noiseH          = 0.5;
		noiseOffset     = 0.8;
		p = ppoint * mainFreq + Randomize;
		distort = 0.350 * Fbm3D(p * riversSin, riversOctaves) +
				  0.035 * Fbm3D(p * riversSin * 5.0, riversOctaves) +
				  0.010 * Fbm3D(p * riversSin * 25.0, riversOctaves);
		cell = Cell3Noise2(riversFreq * p + distort);
		float pseudoRivers = 1.0 - saturate(abs(cell.y - cell.x) * riversMagn);
		pseudoRivers = smoothstep(0.0, 1.0, pseudoRivers);
		pseudoRivers *= 1.0 - smoothstep(0.06, 0.10, global - seaLevel); // disable rivers inside continents
		pseudoRivers *= 1.0 - smoothstep(0.00, 0.01, seaLevel - height); // disable rivers inside oceans
		height = lerp(height, seaLevel-0.02, pseudoRivers);
	}

	// Shield volcano
	if (volcanoOctaves > 0)
	{
		height = VolcanoNoise(ppoint, global, height);
	}

	// Mountain glaciers
	/*noiseOctaves = 5.0;
	noiseLacunarity = 3.5;
	float vary = Fbm(ppoint * 1700.0 + Randomize);
	float snowLine = (height + 0.25 * vary - snowLevel) / (1.0 - snowLevel);
	height += 0.0005 * smoothstep(0.0, 0.2, snowLine);*/

	// Apply ice caps
	height = height * oceaniaFade + icecapHeight * smoothstep(0.0, 1.0, iceCap);

	return height;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
//#define VISUALIZE_BIOMES
float4 ColorMapTerra(float3 ppoint, float height, float slope)
{
	// Biome domains
	float3 p = ppoint * mainFreq + Randomize;
	float4 col;
	float3 distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06, 6);
	float2 cell = Cell3Noise2Color(distort, col);
	float biome = col.r;

	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float vary;

#ifdef VISUALIZE_BIOMES
	float4 colorOverlay;
	if (biome < dunesFraction)			// Yellow
		colorOverlay = float4(1.0, 1.0, 0.0, 0.0);
	else if (biome < hillsFraction)		// Green
		colorOverlay = float4(0.0, 1.0, 0.0, 0.0);
	else if (biome < hills2Fraction)	// Blue
		colorOverlay = float4(0.0, 0.0, 1.0, 0.0);
	else if (biome < canyonsFraction)	// Red
		colorOverlay = float4(1.0, 0.0, 0.0, 0.0);
	else								// White
		colorOverlay = float4(1.0, 1.0, 1.0, 0.0);

	return colorOverlay * biomeScale;
#endif

	Surface surf;

	// Assign a climate type
	noiseOctaves    = 6.0;
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;
	float climate, latitude, dist;
	if (tidalLock <= 0.0)
	{
		latitude = abs(normalize(ppoint).y);
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
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

	// Litosphere cells
	//float lithoCells = LithoCellsNoise(ppoint, climate, 1.5);

	// Change climate with elevation
	noiseOctaves    = 5.0;
	noiseLacunarity = 3.5;
	vary = Fbm(ppoint * 17000 + Randomize);
	float snowLine   = height + 0.25 * vary * slope;
	float montHeight = saturate((height - seaLevel) / (snowLevel - seaLevel));
	climate = min(climate + 0.5 * heightTempGrad * montHeight, climatePole - 0.125);
	climate = lerp(climate, climatePole, saturate((snowLine - snowLevel) * 100.0));

	// Beach
	float beach = saturate((height / seaLevel - 1.0) * 50.0);
	climate = lerp(0.375, climate, beach);

	// Dunes must be made of sand only
	//float dunes = step(dunesFraction, biome) * biomeScale;
	//slope *= dunes;

	// Ice caps
	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0);
	climate = lerp(climate, climatePole, iceCap);

	// Flatland climate distortion
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

	// Dunes must be made of sand only
	//climate = lerp(0.0, climate, dunes);

	// Color texture distortion
	noiseOctaves = 5.0;
	p = ppoint * colorDistFreq * 0.371;
	p += Fbm3D(p * 0.5) * 1.2;
	vary = saturate(Fbm(p) * 0.7 + 0.5);

	// Shield volcano lava
	float2 volcMask = float2(0.0, 0.0);
	if (volcanoOctaves > 0)
	{
		// Global volcano activity mask
		noiseOctaves = 3.0;
		float volcActivity = saturate((Fbm(ppoint * 1.37 + Randomize) - 1.0 + volcanoActivity) * 5.0);
		// Lava in volcano caldera and flows
		volcMask = VolcanoGlowNoise(ppoint);
		volcMask.x *= volcActivity;
	}

	// Model lava as rocks texture
	climate = lerp(climate, 0.375, volcMask.x);
	slope   = lerp(slope,   1.0,   volcMask.x);

	surf = GetSurfaceColor(climate, slope, vary);

	// Sedimentary layers
	noiseOctaves = 4.0;
	float layers = Fbm(float3(height * 168.4 + 0.17 * vary, 0.43 * (p.x + p.y), 0.43 * (p.z - p.y)));
	//layers *= smoothstep(0.75, 0.8, climate) * (1.0 - smoothstep(0.825, 0.875, climate)); // only rock texture
	layers *= smoothstep(0.5, 0.55, slope);     // only steep slopes
	layers *= step(surf.color.a, 0.01);         // do not make layers on snow
	layers *= saturate(1.0 - 5.0 * volcMask.x); // do not make layers on lava
	layers *= saturate(1.0 - 5.0 * volcMask.y); // do not make layers on volcanos
	surf.color.rgb *= float3(1.0, 1.0, 1.0) - float3(0.0, 0.5, 1.0) * layers;

	// Global albedo variations
	noiseOctaves = 8.0;
	distort = Fbm3D((ppoint + Randomize) * 0.07) * 1.5;
	noiseOctaves = 5.0;
	vary = 1.0 - Fbm((ppoint + distort) * 0.78);

	// Ice cracks
	float mask = 1.0;
	if (cracksOctaves > 0.0)
		vary *= lerp(1.0, CrackColorNoise(ppoint, mask), iceCap);

	// Apply albedo variations
	surf.color *= lerp(float4(0.67, 0.58, 0.36, 0.00), float4(1.0, 1.0, 1.0, 1.0), vary);

	if (surfClass <= 3)   // water mask for planets with oceans
		surf.color.a += saturate((seaLevel - height) * 200.0);

	return surf.color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapTerra(float3 ppoint, float height, float slope)
{
	// Assign a climate type
	noiseOctaves	= (surfClass == 1.0) ? 5.0 : 12.0;
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;

	float climate, latitude, dist;

	if (tidalLock <= 0.0)
	{
		latitude = abs(normalize(ppoint).y);
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
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

	// Litosphere cells
	//float lithoCells = LithoCellsNoise(ppoint, climate, 1.5);

	// Change climate with elevation
	float montHeight = saturate((height - seaLevel) / (snowLevel - seaLevel));
	climate = min(climate + heightTempGrad * montHeight, climatePole);

	// Ice caps
	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0);
	climate = lerp(climate, climatePole, iceCap);

	// Thermal emission temperature (in thousand Kelvins)
	float3 p = ppoint * 600.0 + Randomize;

	dist = 10.0 * colorDistMagn * Fbm(p * 0.2, 5);

	float globTemp = 0.95 - abs(Fbm((p + dist) * 0.01, 3)) * 0.08;
	float varyTemp = abs(Fbm(p + dist, 8));

	//globTemp *= 1.0 - lithoCells;

	float surfTemp = surfTemperature *
		(globTemp + varyTemp * 0.08) *
		saturate(2.0 * (lavaCoverage * 0.4 + 0.4 - 0.8 * height)) *
		saturate((lavaCoverage - 0.01) * 25.0) *
		saturate((0.875 - climate) * 50.0);

	// Shield volcano lava
	if (volcanoOctaves > 0)
	{
		// Global volcano activity mask
		float volcActivity = saturate((Fbm(ppoint * 1.37 + Randomize, 3) - 1.0 + volcanoActivity) * 5.0);

		// Lava in the volcano caldera and lava flows
		float2 volcMask = VolcanoGlowNoise(ppoint);
		volcMask.x *= (0.75 + 0.25 * varyTemp) * volcActivity * volcanoTemp;
		surfTemp = max(surfTemp, volcMask.x);
	}

	return float4(UnitToColor24(log(surfTemp) * 0.188 + 0.1316), 1.0);
}
//-----------------------------------------------------------------------------