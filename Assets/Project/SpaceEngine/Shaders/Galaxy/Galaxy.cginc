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
uniform float3	randomParams1;	// Randomize
uniform float4  offsetParams1;	// (offsetX,	offsetY,		offsetZ,	 UNUSED)
uniform float4	sizeParams1;	// (radius,		ellipseRadius,	barSize,	 depth)
uniform float4	warpParams1;	// (warp1.x,	warp1.y,		warp2.x,	 warp2.y)
uniform float4	spiralParams1;	// (inverseEccentricity, spiralRotation, passRotation, UNUSED)
uniform float2	dustParams1;	// (dustStrength,	dustSize,	UNUSED,		UNUSED)
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define		offsetX					offsetParams1.x;
#define		offsetY					offsetParams1.y;
#define		offsetZ					offsetParams1.z;
#define		radius					sizeParams1.x
#define		ellipseRadius			sizeParams1.y
#define		barSize					sizeParams1.z
#define		depth					sizeParams1.w
#define		warp1					warpParams1.xy
#define		warp2					warpParams1.zw
#define		inverseEccentricity		spiralParams1.x
#define		spiralRotation			spiralParams1.y
#define		passRotation			spiralParams1.z
#define		dustStrength			dustParams1.x
#define		dustSize				dustParams1.y
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct GalaxyStar
{
	float3 position;
	float4 color;
	float size;
	float temperature;
};
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 ToneMapFilmicALU(float3 color)
{
	color = max(0, color - 0.004);
	color = (color * (6.2 * color + 0.5)) / (color * (6.2 * color + 1.7) + 0.06);

	return color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 mulvq(float3 v, float4 q)
{
	float axx = q.x * 2.0;
	float ayy = q.y * 2.0;
	float azz = q.z * 2.0;
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

	result.x = ((v.x * ((1.0 - ayyy) - azzz)) + (v.y * (axyy - awzz))) + (v.z * (axzz + awyy));
	result.y = ((v.x * (axyy + awzz)) + (v.y * ((1.0 - axxx) - azzz))) + (v.z * (ayzz - awxx));
	result.z = ((v.x * (axzz - awyy)) + (v.y * (ayzz + awxx))) + (v.z * ((1.0 - axxx) - ayyy));

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
	return float(Hash(seed)) / 4294967295.0;
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
inline float NearIntersection(float3 pos, float3 ray, float distance2, float radius2)
{
	float B = 2.0 * dot(pos, ray);
	float det = max(0.0, B * B - 4.0 * (distance2 - radius2));

	return 0.5 * (-B - sqrt(det));
}

inline float FarIntersection(float3 pos, float3 ray, float distance2, float radius2)
{
	float B = 2.0 * dot(pos, ray);
	float det = max(0.0, B * B - 4.0 * (distance2 - radius2));

	return 0.5 * (-B + sqrt(det));
}

bool RaySphereIntersect(float3 s, float3 d, float r, out float ts, out float te)
{
	float r2 = r * r;
	float s2 = dot(s, s);

	if(s2 <= r2)
	{
		ts = 0.0;
		te = FarIntersection(s, d, s2, r2);
	
		return true;
	}
	
	ts = NearIntersection(s, d, s2, r2);
	te = FarIntersection(s, d, s2, r2);
	
	return te > ts && ts > 0;
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform Texture2D ColorDistributionTable;
SamplerState sampler_point_clamp_MaterialTable;

float4 GetMaterial(float value)
{
	return ColorDistributionTable.SampleLevel(sampler_point_clamp_MaterialTable, float2(value, 0.0), 0);
}
//-----------------------------------------------------------------------------