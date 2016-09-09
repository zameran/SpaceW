#define ECLIPSES

#if !defined (M_PI)
#define M_PI 3.14159265358
#endif

#if !defined (M_PI2)
#define M_PI2 6.28318530716
#endif

#if !defined (MATH)
#include "Math.cginc"
#endif

uniform float4x4 _Sky_LightOccluders_1;
uniform float4x4 _Sky_LightOccluders_2;

uniform float _ExtinctionGroundFade;

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
	return maxPhase * smoothstep(0.0, 1.0, 1.0 - clamp((Dist-Diff) / (sumRadius-Diff), 0.0, 1.0));
}

float EclipseShadow(float3 FragPosS, float3 lightVec, float lightAngularRadius)
{
	float Shadow = 1.0;

	for (int i = 0; i < 4; ++i)
	{
		if (_Sky_LightOccluders_1[i].w <= 0.0) break;

		float3 lightCasterPos = _Sky_LightOccluders_1[i].xyz - FragPosS;

		float lightCasterInvDist  = rsqrt(dot(lightCasterPos, lightCasterPos));
		float casterAngularRadius = asin(clamp(_Sky_LightOccluders_1[i].w * lightCasterInvDist, 0.0, 1.0));
		float lightToCasterAngle  = acos(clamp(dot(lightVec, lightCasterPos * lightCasterInvDist), 0.0, 1.0));

		Shadow *= clamp(1.0 - EclipseValue(lightAngularRadius, casterAngularRadius, lightToCasterAngle), 0.0, 1.0);
	}

	for (int j = 0; j < 4; ++j)
	{
		if (_Sky_LightOccluders_2[j].w <= 0.0) break;

		float3 lightCasterPos = _Sky_LightOccluders_2[j].xyz - FragPosS;

		float lightCasterInvDist  = rsqrt(dot(lightCasterPos, lightCasterPos));
		float casterAngularRadius = asin(clamp(_Sky_LightOccluders_2[j].w * lightCasterInvDist, 0.0, 1.0));
		float lightToCasterAngle  = acos(clamp(dot(lightVec, lightCasterPos * lightCasterInvDist), 0.0, 1.0));

		Shadow *= clamp(1.0 - EclipseValue(lightAngularRadius, casterAngularRadius, lightToCasterAngle), 0.0, 1.0);
	}

	return Shadow;
}

float EclipseOuterShadow(float3 lightVec, float lightAngularRadius, float3 d, float3 WCP, float3 _Globals_Origin, float Rt)
{
	//TODO : Switch in sphere - out sphere.
	float interSectPt = IntersectOuterSphere(WCP, d, _Globals_Origin, Rt);

	return interSectPt != -1 ? EclipseShadow(WCP + d * interSectPt, lightVec, lightAngularRadius) : 1.0;
}