/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
 * All rights reserved.
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
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 */

 /*
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2017
 */

#if !defined (ATMOSPHERE)
#include "../Atmosphere.cginc"
#endif

#if !defined (MATH)
#include "../Math.cginc"
#endif

#define OCEAN_ONLY_SPHERICAL

uniform float4x4 _Globals_WorldToScreen;

uniform float2 _Ocean_MapSize;
uniform float4 _Ocean_Choppyness;
uniform float3 _Ocean_SunDir;
uniform float4 _Ocean_GridSizes;
uniform float2 _Ocean_ScreenGridSize;
uniform float _Ocean_WhiteCapStr;
uniform sampler3D _Ocean_Variance;
uniform sampler2D _Ocean_Map0;
uniform sampler2D _Ocean_Map1;
uniform sampler2D _Ocean_Map2;
uniform sampler2D _Ocean_Map3;
uniform sampler2D _Ocean_Map4;
uniform sampler2D _Ocean_Foam0;
uniform sampler2D _Ocean_Foam1;

float MeanFresnel(float cosThetaV, float sigmaV) 
{
	return pow(1.0 - cosThetaV, 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(sigmaV, 1.5));
}

float MeanFresnel(float3 V, float3 N, float sigmaSq) 
{
	return MeanFresnel(dot(V, N), sqrt(sigmaSq));
}

// L, V, N in world space
float ReflectedSunRadiance(float3 L, float3 V, float3 N, float sigmaSq) 
{
	float3 H = normalize(L + V);

	float hn = dot(H, N);
	float p = exp(-2.0 * ((1.0 - hn * hn) / sigmaSq) / (1.0 + hn)) / (M_PI4 * sigmaSq);

	float c = 1.0 - dot(V, H);
	float c2 = c * c;
	float fresnel = 0.02 + 0.98 * c2 * c2 * c;

	float zL = dot(L, N);
	float zV = dot(V, N);
	zL = max(zL,0.01);
	zV = max(zV,0.01);

	// brdf times cos(thetaL)
	return zL <= 0.0 ? 0.0 : max(fresnel * p * sqrt(abs(zL / zV)), 0.0);
}

float2 U(float2 zeta, float3 V) 
{
	float3 F = normalize(float3(-zeta, 1.0));
	float3 R = 2.0 * dot(F, V) * F - V;

	return -R.xy / (1.0 + R.z);
}

// V, N, sunDir in world space
float3 ReflectedSkyRadiance(sampler2D skymap, float3 V, float3 N, float sigmaSq, float3 sunDir) 
{
	float3 result = float3(0,0,0);

	float2 zeta0 = -N.xy / N.z;
	float2 tau0 = U(zeta0, V);

	const float n = 1.0 / 1.1;

	float2 JX = (U(zeta0 + float2(0.01, 0.0), V) - tau0) / 0.01 * n * sqrt(sigmaSq);
	float2 JY = (U(zeta0 + float2(0.0, 0.01), V) - tau0) / 0.01 * n * sqrt(sigmaSq);
	
	result = tex2D(skymap, (tau0 * 0.5 / 1.1 + 0.5), JX, JY).rgb;

	result *= 0.02 + 0.98 * MeanFresnel(V, N, sigmaSq);

	return result;
}

float RefractedSeaRadiance(float3 V, float3 N, float sigmaSq) 
{
	return 0.98 * (1.0 - MeanFresnel(V, N, sigmaSq));
}

float erf(float x) 
{
	const float a  = 0.140012;

	float x2 = x * x;
	float ax2 = a * x2;

	return sign(x) * sqrt( 1.0 - exp(-x2*(4.0 / M_PI + ax2)/(1.0 + ax2)) );
}

float WhitecapCoverage(float epsilon, float mu, float sigma2) 
{
	return 0.5 * erf((0.5 * sqrt(2.0) * (epsilon - mu) * (1.0 / sqrt(sigma2)))) + 0.5;
}

#define OCEAN_SKY_REFLECTIONS

#ifdef OCEAN_SKY_REFLECTIONS

float3 ReflectedSky(float3 V, float3 N, float3 sunDir, float3 earthP) 
{
	float3 result = float3(0.0, 0.0, 0.0);
	float3 reflectedAngle = reflect(-V, N);

	reflectedAngle.z = max(reflectedAngle.z, 0.0);	// Hack to avoid unsightly black pixels from downwards reflections
	result = SkyRadianceSimple(earthP, reflectedAngle, sunDir);

	return result;
}

#endif

float3 OceanRadianceWithoutSkyReflection(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor) 
{
	float fresnel = MeanFresnel(V, N, sigmaSq);

	float3 Lsky = skyE * fresnel / M_PI;
	float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
	float3 Lsea = (1.0 - fresnel) * seaColor * skyE / M_PI;

	return Lsun + Lsky + Lsea;
}

float3 OceanRadianceWithSkyReflection(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor, float3 earthP) 
{
	float fresnel = MeanFresnel(V, N, sigmaSq);

	float3 Lsky = fresnel * ReflectedSky(V, N, L, earthP);
	float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
	float3 Lsea = 0.98 * (1.0 - fresnel) * seaColor * (skyE / M_PI);

	return Lsun + Lsky + Lsea;
}

float3 OceanRadiance(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor, float3 earthP) 
{
	#ifdef OCEAN_SKY_REFLECTIONS
		return OceanRadianceWithSkyReflection(L, V, N, sigmaSq, sunL, skyE, seaColor, earthP);
	#else
		return OceanRadianceWithoutSkyReflection(L, V, N, sigmaSq, sunL, skyE, seaColor);
	#endif

	return 0;
}

// [Obsolete("...")]
float3 OceanRadiance(float3 L, float3 V, float3 N, float sigmaSq, float3 sunL, float3 skyE, float3 seaColor) 
{
	float fresnel = MeanFresnel(V, N, sigmaSq);

	float3 Lsky = skyE * fresnel / M_PI;
	float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
	float3 Lsea = (1.0 - fresnel) * seaColor * skyE / M_PI;

	return Lsun + Lsky + Lsea;
}