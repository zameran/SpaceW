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
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2023
 */

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

//-----------------------------------------------------------------------------
float HeightMapSelena(float3 ppoint)
{
	// Biome domains
	float3 p = ppoint * mainFreq + Randomize;
	float4 col;
	float3 distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06, 6);
	float2 cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));

	float montRage = saturate(DistNoise(ppoint * 22.6 + Randomize, 2.5) + 0.5);
	montRage *= montRage;
	float montBiomeScale = min(pow(2.2 * biomeScale, 2.5), 1.0) * montRage;
	float inv2montesSpiky = 1.0 / (montesSpiky * montesSpiky);

	// Global landscape
	//p = ppoint * mainFreq + Randomize; // Redutant.
	distort = 0.35 * Fbm3D(p * 2.37, 4);
	p += distort;// + 0.005 * (1.0 - abs(Fbm3D(p * 132.3)));
	float global = 0.6 * (1.0 - Cell3Noise(p));

	// Venus-like structure
	float venus = 0.0;
	if (venusMagn > 0.05)
	{
		noiseOctaves = 4;
		distort = Fbm3D(ppoint * 0.3) * 1.5;
		noiseOctaves = 6;
		venus = Fbm((ppoint + distort) * venusFreq) * venusMagn;
	}
	global += venus;

	// Mare
	float mare = global;
	float mareFloor = global;
	float mareSuppress = 1.0;
	if (mareSqrtDensity > 0.05)
	{
		noiseOctaves = 2;
		mareFloor = 0.6 * (1.0 - Cell3Noise(0.3 * p));
		craterDistortion = 1.0;
		noiseOctaves = 6;  // Mare roundness distortion
		mare = MareNoise(ppoint, global, mareFloor, mareSuppress);
	}

	// Old craters
	float crater = 0.0;
	if (craterSqrtDensity > 0.05)
	{
		heightFloor = -0.1;
		heightPeak  =  0.6;
		heightRim   =  1.0;
		crater = mareSuppress * CraterNoise(ppoint, craterMagn, craterFreq, craterSqrtDensity, craterOctaves);
		noiseOctaves    = 10.0;
		noiseLacunarity = 2.0;
		crater = 0.25 * crater + 0.05 * crater * iqTurbulence(ppoint * montesFreq + Randomize, 0.55);
	}

	float height = mare + crater;

	// Ice cracks
	float mask = 1.0;
	if (cracksOctaves > 0.0)
	{
		height += CrackNoise(ppoint, mask);
	}

	if (biome > hillsFraction)
	{
		if (biome < hills2Fraction)
		{
			// "Freckles" (structures like on Europa)
			noiseOctaves    = 10.0;
			noiseLacunarity = 2.0;
			height += 0.2 * hillsMagn * mask * biomeScale * JordanTurbulence(ppoint * hillsFreq + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
		}
		else if (biome < canyonsFraction)
		{
			// Rimae
			noiseOctaves     = 3.0;
			noiseLacunarity  = 2.218281828459;
			noiseH           = 0.9;
			noiseOffset      = 0.5;
			p = ppoint * mainFreq + Randomize;
			distort  = 0.035 * Fbm3D(p * riversSin * 5.0);
			distort += 0.350 * Fbm3D(p * riversSin);
			cell = Cell3Noise2(canyonsFreq * 0.05 * p + distort);
			float rima = 1.0 - saturate(abs(cell.y - cell.x) * 250.0 * canyonsMagn);
			rima = biomeScale * smoothstep(0.0, 1.0, rima);
			height = lerp(height, height-0.02, rima);
		}
		else
		{
			// Mountains
			noiseOctaves    = 10.0;
			noiseLacunarity = 2.0;
			height += montesMagn * montBiomeScale * iqTurbulence(ppoint * 0.5 * montesFreq + Randomize, 0.45);
		}
	}

	// Rayed craters
	if (craterSqrtDensity * craterSqrtDensity * craterRayedFactor > 0.05 * 0.05)
	{
		heightFloor = -0.5;
		heightPeak  =  0.6;
		heightRim   =  1.0;
		float craterRayedSqrtDensity = craterSqrtDensity * sqrt(craterRayedFactor);
		float craterRayedOctaves = floor(craterOctaves * craterRayedFactor);
		float craterRayedMagn = craterMagn * pow(0.62, craterOctaves - craterRayedOctaves);
		crater = RayedCraterNoise(normalize(ppoint), craterRayedMagn, craterFreq, craterRayedSqrtDensity, craterRayedOctaves);
		height += crater;
	}

	// Shield volcano
	if (volcanoOctaves > 0)
	{
		height = VolcanoNoise(normalize(ppoint), global, height);
	}

	return height;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapSelena(float3 ppoint, float height, float slope)
{
	// Biome domains
	float3 p = ppoint * mainFreq + Randomize;
	float4 col;
	float3 distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06, 6);
	float2 cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));

	// Assign a material
	noiseOctaves = 12.0;
	float mat, dist, lat, latitude;

	if (tidalLock <= 0.0)
	{
		lat = abs(normalize(ppoint).y);
		latitude = lat + 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
		latitude = saturate(latitude);
		mat = height;
	}
	else
	{
		lat = 1.0 - normalize(ppoint).x;
		latitude = lat + 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
		mat = lerp(climateTropic, climatePole, latitude);
	}

	// Color texture distortion
	noiseOctaves = 15.0;
	dist = 1.5 * floor(2.0 * DistFbm(ppoint * 0.002 * colorDistFreq, 2.0));
	mat += colorDistMagn * dist;

	// Color texture variation
	noiseOctaves = 5;
	p = ppoint * colorDistFreq * 2.3;
	p += Fbm3D(p * 0.5) * 1.2;
	float vary = saturate((Fbm(p) + 0.7) * 0.7);

	// Shield volcano lava
	if (volcanoOctaves > 0)
	{
		// Global volcano activity mask
		noiseOctaves = 3;
		float volcActivity = saturate((Fbm(ppoint * 1.37 + Randomize) - 1.0 + volcanoActivity) * 5.0);
		// Lava in volcano caldera and flows
		float2 volcMask = VolcanoGlowNoise(normalize(ppoint));
		volcMask.x *= volcActivity;
		// Model lava as rocks texture
		mat   = lerp(mat,   0.0, volcMask.x);
		slope = lerp(slope, 0.0, volcMask.x);
	}

	Surface surf = GetSurfaceColor(saturate(mat), slope, vary);

	// Global albedo variations
	noiseOctaves = 8;
	distort = Fbm3D((ppoint + Randomize) * 0.07) * 1.5;
	noiseOctaves = 5;
	float slopeMod = 1.0 - slope;
	vary = saturate(1.0 - Fbm((ppoint + distort) * 0.78) * slopeMod * slopeMod * 2.0);

	// Ice cracks
	float mask = 1.0;
	if (cracksOctaves > 0.0)
	{
		vary *= CrackColorNoise(ppoint, mask);
	}

	// "Freckles" (structures like on Europa)
	if ((biome > hillsFraction) && (biome < hills2Fraction))
	{
		noiseOctaves    = 10.0;
		noiseLacunarity = 2.0;
		vary *= 1.0 - saturate(2.0 * mask * biomeScale * JordanTurbulence(ppoint * hillsFreq + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0));
	}

	// Apply albedo variations
	surf.color *= lerp(float4(0.67, 0.58, 0.36, 0.00), float4(1.0, 1.0, 1.0, 1.0), vary);

	// Make driven hemisphere darker
	if (drivenDarkening != 0.0)
	{
		noiseOctaves = 3;
		float z = -normalize(ppoint).z * sign(drivenDarkening);
		z += 0.2 * Fbm(ppoint * 1.63);
		z = saturate(1.0 - z);
		z *= z;
		surf.color.rgb *= lerp(1.0 - abs(drivenDarkening), 1.0, z);
	}
		
	// Rayed craters
	if (craterSqrtDensity * craterSqrtDensity * craterRayedFactor > 0.05 * 0.05)
	{
		float craterRayedSqrtDensity = craterSqrtDensity * sqrt(craterRayedFactor);
		float craterRayedOctaves = floor(craterOctaves * craterRayedFactor);
		float crater = RayedCraterColorNoise(normalize(ppoint), craterFreq, craterRayedSqrtDensity, craterRayedOctaves);
		surf.color.rgb = lerp(surf.color.rgb, float3(1.0, 1.0, 1.0), crater);
	}

	// Ice caps - thin frost
	// TODO: make it only on shadowed slopes
	float iceCap = saturate((latitude - latIceCaps) * 2.0);
	surf.color.rgb = lerp(surf.color.rgb, float3(1.0, 1.0, 1.0), 0.4 * iceCap);

	return surf.color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapSelena(float3 ppoint, float height, float slope)
{
	// Thermal emission temperature (in thousand Kelvins)
	float3 p = ppoint * 600.0 + Randomize;
	float dist = 10.0 * colorDistMagn * Fbm(p * 0.2, 5);
	float globTemp = 0.95 - abs(Fbm((p + dist) * 0.01, 3)) * 0.08;
	float varyTemp = abs(Fbm(p + dist, 8));

	// Global surface melting
	float surfTemp = surfTemperature *
		(globTemp + varyTemp * 0.08) *
		saturate(2.0 * (lavaCoverage * 0.4 + 0.4 - 0.8 * height)) *
		saturate((lavaCoverage - 0.01) * 25.0);

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