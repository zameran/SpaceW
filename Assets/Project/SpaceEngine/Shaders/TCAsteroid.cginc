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
float HeightMapAsteroid(float3 ppoint)
{
	// Global landscape
	float3 p = ppoint * 0.6 + Randomize;
	float height = 0.5 - Noise(p) * 2.0;

	noiseOctaves = 10;
	noiseLacunarity = 2.0;
	height += 0.05 * iqTurbulence(ppoint * 2.0 * mainFreq + Randomize, 0.35);

	// Hills
	noiseOctaves = 5;
	noiseLacunarity  = 2.218281828459;
	float hills = (0.5 + 1.5 * Fbm(p * 0.0721)) * hillsFreq;
	hills = Fbm(p * hills) * 0.15;
	noiseOctaves = 2;
	float hillsMod = smoothstep(0, 1, Fbm(p * hillsFraction) * 3.0);
	height *= 1.0 + hillsMagn * hills * hillsMod;

	// Craters
	heightFloor = -0.1;
	heightPeak  =  0.6;
	heightRim   =  0.4;
	float crater = 0.4 * CraterNoise(ppoint, craterMagn, craterFreq, craterSqrtDensity, craterOctaves);

	noiseOctaves = 10;
	noiseLacunarity = 2.0;
	crater += montesMagn * crater * iqTurbulence(ppoint * montesFreq, 0.52);	

	return height + crater;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapAsteroid(float3 ppoint, float height, float slope)
{
	height = DistFbm(ppoint * 3.7 + Randomize, 1.5, 5);

	noiseOctaves = 5.0;
	float3 p = ppoint * colorDistFreq * 2.3;
	p += Fbm3D(p * 0.5) * 1.2;
	float vary = saturate((Fbm(p) + 0.7) * 0.7);

	Surface surf = GetSurfaceColor(height, slope, vary);
	surf.color.rgb *= 0.5 + slope;
	return surf.color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapAsteroid(float3 ppoint, float height, float slope)
{
	// Thermal emission temperature (in thousand Kelvins)
	float3 p = ppoint * 600.0 + Randomize;

	float dist = 10.0 * colorDistMagn * Fbm(p * 0.2, 5);
	float globTemp = 0.95 - abs(Fbm((p + dist) * 0.01, 3)) * 0.08;
	float varyTemp = abs(Fbm(p + dist, 8));

	// Global surface melting
	float surfTemp = surfTemperature * (globTemp + varyTemp * 0.08) * saturate(2.0 * (lavaCoverage * 0.4 + 0.4 - 0.8 * height));

	return float4(UnitToColor24(log(surfTemp) * 0.188 + 0.1316), 1.0);
}
//-----------------------------------------------------------------------------