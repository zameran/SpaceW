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
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 */

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

//-----------------------------------------------------------------------------
float HeightMapSun(float3 ppoint)
{
	// Flows
	float3 p = ppoint * colorDistFreq + Randomize;
	float3 dist = 2.5 * Fbm3D(p * 0.5, 5);
	float flows = Fbm(p * 7.5 + dist, 3);

	// Granularity
	noiseOctaves = 5;
	p = ppoint * hillsFreq + Randomize;
	dist = dunesMagn * Fbm3D(p * 0.2);
	float2 cell = Cell3Noise2(p + dist);
	float gran = smoothstep(0.1, 1.0, sqrt(abs(cell.y - cell.x))) - 0.5;

	// Solar spots
	float botMask = 1.0;
	float filMask = 0.0;
	float filaments = 0.0;

	if (mareSqrtDensity > 0.01)
	{
		noiseOctaves = 5;
		SolarSpotsHeightNoise(ppoint, botMask, filMask, filaments);
	}

	const float surfHeight = 1.0;
	const float filHeight  = 0.6;
	const float spotHeight = 0.5;

	//return (flows * 0.1 + gran * (1.0 - filMask)) * lerp(spotHeight, surfHeight, botMask) + filMask * lerp(spotHeight, filHeight, filaments);
	//return (0.8 + flows * 0.1) * botMask + gran * 0.03 * (1.0 - filMask) + saturate(filaments) * 0.1 * filMask;
	return (0.8 + flows * 0.1) * colorDistMagn * botMask + gran * hillsMagn * (1.0 - filMask) + saturate(filaments) * 0.1 * hillsMagn * filMask;
}

float GlowMapSun(float3 ppoint)
{
	// Flows
	float3 p = ppoint * colorDistFreq + Randomize;
	float3 dist = 2.5 * Fbm3D(p * 0.5, 5);
	float flows = Fbm(p * 7.5 + dist, 3);

	// Granularity
	noiseOctaves = 5;
	p = ppoint * hillsFreq + Randomize;
	dist = dunesMagn * Fbm3D(p * 0.2);
	float2 cell = Cell3Noise2(p + dist);
	float gran = smoothstep(0.1, 1.0, sqrt(abs(cell.y - cell.x)));

	// Solar spots
	float botMask   = 1.0;
	float filMask   = 0.0;
	float filaments = 0.0;

	if (mareSqrtDensity > 0.01)
	{
		noiseOctaves = 5;
		SolarSpotsTempNoise(ppoint, botMask, filMask, filaments);
	}

	float granTopTemp = colorParams.z;
	float granBotTemp = colorParams.w;
	float surfTemp = 1.0;
	float filTemp  = granTopTemp;
	float spotTemp = granBotTemp;

	return (flows * 0.1 + lerp(granBotTemp, granTopTemp, gran) * (1.0 - filMask)) * lerp(spotTemp, surfTemp, botMask) + filMask * lerp(spotTemp, filTemp, filaments);
}
//-----------------------------------------------------------------------------