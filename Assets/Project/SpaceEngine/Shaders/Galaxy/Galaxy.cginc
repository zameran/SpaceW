// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

#define GALAXY

//-----------------------------------------------------------------------------
#if !defined (MATH)
#include "../Math.cginc"
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#if !defined (TCCOMMON)
#include "../TCCommon.cginc"
#endif
//-----------------------------------------------------------------------------

static const float3 gasMultiplicationColor = float3(0.52312f, 0.99695f, 0.94122f);

//-----------------------------------------------------------------------------
uniform float3	randomParams1;	// Randomize
uniform float4  offsetParams1;	// (offsetX,	offsetY,		offsetZ,	 UNUSED)
uniform float4	sizeParams1;	// (radius,		ellipseRadius,	barSize,	 depth)
uniform float4	warpParams1;	// (warp1.x,	warp1.y,		warp2.x,	 warp2.y)
uniform float4	spiralParams1;	// (inverseEccentricity, spiralRotation, passRotation, UNUSED)
uniform float2	dustParams1;	// (dustStrength,	dustSize,	UNUSED,		UNUSED)
uniform float3	gasParams1;		// (gasStrength,	gasSize,	UNUSED,		UNUSED)
uniform float3	temperatureParams1;	// (temperature min, temperature max, temperature shift)
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define		offsetX                 offsetParams1.x
#define		offsetY                 offsetParams1.y
#define		offsetZ                 offsetParams1.z
#define		radius                  sizeParams1.x
#define		ellipseRadius           sizeParams1.y
#define		barSize                 sizeParams1.z
#define		depth                   sizeParams1.w
#define		warp1                   warpParams1.xy
#define		warp2                   warpParams1.zw
#define		inverseEccentricity     spiralParams1.x
#define		spiralRotation          spiralParams1.y
#define		passRotation            spiralParams1.z
#define		dustStrength            dustParams1.x
#define		dustSize                dustParams1.y
#define		gasStrength		        gasParams1.x
#define		gasSize					gasParams1.y
#define		temperatureMin          temperatureParams1.x
#define		temperatureMax          temperatureParams1.y
#define		temperatureShift        temperatureParams1.z
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct GalaxyParticle
{
	float3 position;
	float4 color;
	float size;
	float temperature;
	float3 id;
};
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 ToneMapFilmicALU(float3 color)
{
	color = max(0.0f, color - 0.004f);
	color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f) + 0.06f);

	return color;
}

float4 ToneMapFilmicALU(float4 color)
{
	return float4(ToneMapFilmicALU(color.xyz), color.w);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 mulvq(float3 v, float4 q)
{
	float axx = q.x * 2.0f;
	float ayy = q.y * 2.0f;
	float azz = q.z * 2.0f;
	float awxx = q.a * axx;
	float awyy = q.a * ayy;
	float awzz = q.a * azz;
	float axxx = q.x * axx;
	float axyy = q.x * ayy;
	float axzz = q.x * azz;
	float ayyy = q.y * ayy;
	float ayzz = q.y * azz;
	float azzz = q.z * azz;

	float3 result;

	result.x = ((v.x * ((1.0f - ayyy) - azzz)) + (v.y * (axyy - awzz))) + (v.z * (axzz + awyy));
	result.y = ((v.x * (axyy + awzz)) + (v.y * ((1.0f - axxx) - azzz))) + (v.z * (ayzz - awxx));
	result.z = ((v.x * (axzz - awyy)) + (v.y * (ayzz + awxx))) + (v.z * ((1.0f - axxx) - ayyy));

	return result;
}

float4 qaxang(float3 axis, float angle)
{
	float ha = angle * 0.5f;
	float sha = sin(ha);

	return float4(axis.x * sha, axis.y * sha, axis.z * sha, cos(ha));
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Hash(uint s)
{
	s ^= 2747636419u;
	s *= 2654435769u;
	s ^= s >> 16;
	s *= 2654435769u;
	s ^= s >> 16;
	s *= 2654435769u;
	return s;
}

float Random1(uint seed)
{
	return float(Hash(seed)) / 4294967295.0f;
}

float2 Random2(uint seed)
{
	return float2(Random1(seed + 0), Random1(seed + 1));
}

float3 Random3(uint seed)
{
	return float3(Random2(seed), Random1(seed + 2));
}

float4 Random4(uint seed)
{
	return float4(Random3(seed), Random1(seed + 3));
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#ifndef COMPUTE_SHADER
bool RaySphereIntersect(float3 s, float3 d, float r, out float ts, out float te)
{
	float r2 = r * r;
	float s2 = dot(s, s);
	float rs2 = s2 - r2;

	float B = 2.0f * dot(s, d);
	float det = max(0.0f, B * B - 4.0f * rs2);
	
	if(s2 <= r2)
	{
		ts = 0.0;
		te = 0.5f * (-B + sqrt(det));
	
		return true;
	}
	
	ts = 0.5f * (-B - sqrt(det));
	te = 0.5f * (-B + sqrt(det));
	
	return te > ts && ts > 0.0f;
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform Texture2D ColorDistributionTable;
SamplerState sampler_point_clamp_MaterialTable;

float4 GetMaterial(float value)
{
	return normalize(ColorDistributionTable.SampleLevel(sampler_point_clamp_MaterialTable, float2(value, 0.0f), 0));
}
//-----------------------------------------------------------------------------

float ArmProfile(float fi, float r)
{
	const float fi0 = 0.2;

	//fi = floor(fi / M_PI);
	//fi = max(sin(floor(fi / M_PI)), 0.0);
	fi = (fi < fi0) ? fi / fi0 : (1.0 - fi) / (1.0 - fi0);

	return SavePow(fi, 15.0 * r + 1.0);
}

float RayFunc(float fi, float r)
{
	const float r0 = 0.15;

	float t = (r < r0) ? r / r0 : (1.0 - r) / (1.0 - r0);
	float d = ArmProfile(fi, r);

	return SavePow(t, 1.8) * d;
}

float SpiralDensity(float3 ppoint)
{
	// Compute cyclons
	float cycloneRadius = length(ppoint);
	float cycloneAmpl   = 2.3;
	float weight        = 1.0;
	float3 twistedPoint = ppoint;
	float global = 0;
	float distort = 0;
	float turbulence = 0;

	//twistedPoint += 0.15 * Fbm3D(twistedPoint * 1.5);
	
	if (cycloneRadius < 1.0)
	{
		float dist = 1.0 - cycloneRadius;
		float fi = log(cycloneRadius);

		twistedPoint = Rotate2d(cycloneAmpl * fi, twistedPoint);
	}

	noiseLacunarity = 3.0;
	distort = Fbm(ppoint * 0.7 + Randomize, 4) * 0.2;

	//twistedPoint.x *= 0.2;
	//twistedPoint.y *= 5.0;
	//global = (Fbm(twistedPoint) + 1.0, 2) * 0.7;
	//global = abs(sin(M_PI2 * twistedPoint.x));

	float r = length(twistedPoint.xy);
	float fi = atan2(twistedPoint.x, twistedPoint.y);
	fi = frac(3 * ((fi / M_PI) * 0.5 + 0.5));
	global = RayFunc(fi + distort, r) * weight;

	// Compute flow-like distortion
	//global = (Fbm(twistedPoint + distort) + 1.0, 6) * 0.7;
	//global = (global + offset) * weight;

	// Compute turbilence features
	turbulence = Fbm(ppoint * 100.0 + Randomize, 5) * 0.1;

	return global + turbulence * step(0.1, global);
}