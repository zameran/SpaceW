// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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

#define SPACE_ECLIPSES

#define ECLIPSES_METHOD 0

#if !defined (MATH)
#include "Math.cginc"
#endif

uniform float4x4 _Sky_LightOccluders_1;

uniform float _ExtinctionGroundFade;

//-----------------------------------------------------------------------------
#if (ECLIPSES_METHOD == 0)

float EclipseValue(float lightRadius, float casterRadius, float dist)
{
	float sumRadius = lightRadius + casterRadius;

	// No intersection
	//UNITY_BRANCH
	// TODO : 'use of potentially uninitialized variable (EclipseValue)'
	if (dist >= sumRadius) return 0.0;

	float minRadius;
	float maxPhase;

	UNITY_BRANCH
	if (lightRadius < casterRadius)
	{
		minRadius = lightRadius;
		maxPhase = 1.0;
	}
	else
	{
		minRadius = casterRadius;

		UNITY_BRANCH
		if (lightRadius < 0.001)
			maxPhase = (casterRadius * casterRadius) / (lightRadius * lightRadius);
		else
			maxPhase = (1.0 - cos(casterRadius)) / (1.0 - cos(lightRadius));
	}

	// Full intersection
	UNITY_BRANCH
	if (dist <= max(lightRadius, casterRadius) - minRadius) return maxPhase;

	float diff = abs(lightRadius - casterRadius);

	// Partial intersection
	return maxPhase * smoothstep(0.0, 1.0, 1.0 - clamp((dist - diff) / (sumRadius - diff), 0.0, 1.0));
}

float EclipseShadow(float3 worldPos, float3 lightVec, float lightAngularRadius)
{
	// TODO : Fix eclipse mirror around (Planet2Light-Light2Planet) plane...
	float3 lightCasterPos = 0;

	float lightCasterInvDist = 0;
	float casterAngularRadius = 0;
	float lightToCasterAngle = 0;
	float shadow = 1.0;

	UNITY_UNROLL
	for (int i = 0; i < 4; ++i)
	{
		if (_Sky_LightOccluders_1[i].w <= 0.0) { break; }

		lightCasterPos = _Sky_LightOccluders_1[i].xyz - worldPos;

		lightCasterInvDist  = rsqrt(dot(lightCasterPos, lightCasterPos));
		casterAngularRadius = asin(clamp(_Sky_LightOccluders_1[i].w * lightCasterInvDist, 0.0, 1.0));
		lightToCasterAngle  = acos(clamp(dot(lightVec, lightCasterPos * lightCasterInvDist), 0.0, 1.0));

		shadow *= clamp(1.0 - EclipseValue(lightAngularRadius, casterAngularRadius, lightToCasterAngle), 0.0, 1.0);
	}

	return shadow;
}

float EclipseOuterShadow(float3 lightVec, float lightAngularRadius, float3 d, float3 camera, float3 origin, float Rt)
{
	// TODO : Switch in sphere - out sphere.
	float interSectPt = IntersectOuterSphere(camera, d, origin, Rt);

	return interSectPt != -1 ? EclipseShadow(camera + d * interSectPt, lightVec, lightAngularRadius) : 1.0;
}
//-----------------------------------------------------------------------------

#elif (ECLIPSES_METHOD == 1)

//-----------------------------------------------------------------------------
// Source: https://en.wikibooks.org/wiki/GLSL_Programming/Unity/Soft_Shadows_of_Spheres
float EclipseValue(float3 worldPos, float3 worldLightPos,float3 occluderSpherePosition, float3 occluderSphereRadius, float3 lightSourceRadius)
{
	float3 lightDirection = float3(worldLightPos - worldPos);	// Light source direction...
	float3 lightDistance = length(lightDirection);				// Light distance...

	lightDirection = lightDirection / lightDistance;			// Post fake normalization....
			   
	// Computation of level of shadowing...
	float3 sphereDirection = float3(occluderSpherePosition - worldPos);  // Occluder sphere....
	float sphereDistance = length(sphereDirection);

	sphereDirection = sphereDirection / sphereDistance;
					
	float dd = lightDistance * (asin(min(1.0, length(cross(lightDirection, sphereDirection)))) - asin(min(1.0, occluderSphereRadius / sphereDistance)));
			
	float w = smoothstep(-1.0, 1.0, -dd / lightSourceRadius);

	w = w * smoothstep(0.0, 0.2, dot(lightDirection, sphereDirection));
					
	return (1 - w);
}

float EclipseShadow(float3 worldPos, float3 worldLightPos, float lightRadius)
{
	// TODO : Fix eclipse mirror around (Planet2Light-Light2Planet) plane...

	float shadow = 1;

	UNITY_UNROLL
	for (int i = 0; i < 4; ++i)
	{
		if (_Sky_LightOccluders_1[i].w <= 0.0) { break; }

		shadow *= EclipseValue(worldPos, worldLightPos, _Sky_LightOccluders_1[i].xyz, _Sky_LightOccluders_1[i].w, lightRadius);
	}

	return shadow;
}

float EclipseOuterShadow(float3 lightVec, float lightAngularRadius, float3 d, float3 camera, float3 origin, float Rt)
{
	// TODO : Switch in sphere - out sphere.
	float interSectPt = IntersectOuterSphere(camera, d, origin, Rt);

	return interSectPt != -1 ? EclipseShadow(camera + d * interSectPt, lightVec, lightAngularRadius) : 1.0;
}
//-----------------------------------------------------------------------------

#endif

//-----------------------------------------------------------------------------
float4 GroundFade(float fade, float4 value)
{
	return 1.0f * fade + (1.0f - fade) * value;
}

float3 GroundFade(float fade, float3 extinction, float4 value)
{
	return 1.0f * fade + (1.0f - fade) * extinction * value.xyz;
}

float4 BrightnessContrast(float brightness, float contrast, float4 value)
{
	return (value - 1.0) * contrast + 1.0 + brightness;
}
//-----------------------------------------------------------------------------