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

#define ECLIPSES

#if !defined (MATH)
#include "Math.cginc"
#endif

uniform float4x4 _Sky_LightOccluders_1;

uniform float _ExtinctionGroundFade;

//-----------------------------------------------------------------------------
float EclipseValue(float lightRadius, float casterRadius, float Dist)
{
	float sumRadius = lightRadius + casterRadius;

	// No intersection
	if (Dist >= sumRadius) return 0.0;

	float minRadius;
	float maxPhase;

	if (lightRadius < casterRadius)
	{
		minRadius = lightRadius;
		maxPhase = 1.0;
	}
	else
	{
		minRadius = casterRadius;

		if (lightRadius < 0.001)
			maxPhase = (casterRadius * casterRadius) / (lightRadius * lightRadius);
		else
			maxPhase = (1.0 - cos(casterRadius)) / (1.0 - cos(lightRadius));
	}

	// Full intersection
	if (Dist <= max(lightRadius, casterRadius) - minRadius) return maxPhase;

	float Diff = abs(lightRadius - casterRadius);

	// Partial intersection
	return maxPhase * smoothstep(0.0, 1.0, 1.0 - clamp((Dist-Diff)/(sumRadius-Diff), 0.0, 1.0));
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float EclipseShadow(float3 position, float3 lightVec, float lightAngularRadius)
{
	float Shadow = 1.0;

	for (int i = 0; i < 4; ++i)
	{
		if (_Sky_LightOccluders_1[i].w <= 0.0) { break; }

		float3 lightCasterPos = _Sky_LightOccluders_1[i].xyz - position;

		float lightCasterInvDist  = rsqrt(dot(lightCasterPos, lightCasterPos));
		float casterAngularRadius = asin(clamp(_Sky_LightOccluders_1[i].w * lightCasterInvDist, 0.0, 1.0));
		float lightToCasterAngle  = acos(clamp(dot(lightVec, lightCasterPos * lightCasterInvDist), 0.0, 1.0));

		Shadow *= clamp(1.0 - EclipseValue(lightAngularRadius, casterAngularRadius, lightToCasterAngle), 0.0, 1.0);
	}

	return Shadow;
}

float EclipseOuterShadow(float3 lightVec, float lightAngularRadius, float3 d, float3 camera, float3 _Globals_Origin, float Rt)
{
	// TODO : Switch in sphere - out sphere.
	float interSectPt = IntersectOuterSphere(camera, d, _Globals_Origin, Rt);

	return interSectPt != -1 ? EclipseShadow(camera + d * interSectPt, lightVec, lightAngularRadius) : 1.0;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GroundFade(float fade, float4 value)
{
	return 1.0f * fade + (1.0f - fade) * value;
}

float4 BrightnessContrast(float brightness, float contrast, float4 value)
{
	return (value - 1.0) * contrast + 1.0 + brightness;
}
//-----------------------------------------------------------------------------