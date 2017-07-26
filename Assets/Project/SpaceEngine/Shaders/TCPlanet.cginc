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
float HeightMapPlanet(float3 ppoint)
{
	float3 p = ppoint * mainFreq + Randomize;

	float total = 0;
	float latitude = 0;

	if (tidalLock <= 0.0)
	{
		latitude = abs(normalize(ppoint).y);
		latitude += 0.15 * (Fbm(ppoint * 0.007 + Randomize) - 1.0);
		latitude = saturate(latitude);
	}
	else
	{
		latitude = 1.0 - normalize(ppoint).z;
		latitude += 0.15 * (Fbm(ppoint * 0.007 + Randomize) - 1.0);
		latitude = saturate(latitude);
	}

	float t0 = Fbm(p * 0.75, 4);
	//float height = t0 + pow(2.0, RidgedMultifractalExtra(p, 18, 1, 1.75, 0.6));
	//float height = t0 + pow(2.0, SimplexRidgedMultifractal(p, 18, 2, 0.5));
	float height = t0 + pow(4.0, SimplexRidgedMultifractal(p, 18, 2, 0.5));
	//float height = t0 + SimplexRidgedMultifractal(p, 18, 2, 0.5);

	height = height + height * saturate(Cell2Noise(p * 0.5 * FiltNoise3D(p * 1.25, 1.0)));

	float terraceLayers = max(iNoise(p, 1.0) * 4.0, 3.0);
	terraceLayers += Fbm(p * 5.41, 4);

	height = GetTerraced(height, terraceLayers);

	total = height;

	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0 * 1);
	total = total * 1 + icecapHeight * smoothstep(0.0, 1.0, iceCap);

	return total - 1.5;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapPlanet(float3 ppoint, float height, float slope)
{
	Surface surf;

	float3 p = ppoint * mainFreq + Randomize;
	float3 pp = (ppoint + Randomize) * (0.0005 * hillsFreq / (hillsMagn * hillsMagn));

	float vary = 0;
	float fr = 0.20 * (1.5 - RidgedMultifractal(pp,         2.0, 4)) +
			   0.05 * (1.5 - RidgedMultifractal(pp * 10.0,  2.0, 4)) +
			   0.02 * (1.5 - RidgedMultifractal(pp * 100.0, 2.0, 4));

	p = ppoint * (colorDistFreq * 0.005) + float3(fr, fr, fr);
	p += Fbm3D(p * 0.38, 4) * 1.2;
	p = ppoint * colorDistFreq * 0.371;
	p += Fbm3D(p * 0.5, 5) * 1.2;
	vary = (Fbm(p, 5) + 0.7) * 0.7;

	surf = GetSurfaceColor(height, slope, vary);

	return surf.color;
}
//-----------------------------------------------------------------------------