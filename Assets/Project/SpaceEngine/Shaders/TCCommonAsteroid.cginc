#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapAsteroid(float3 ppoint, float freq, float mfreq, float hfreq, float hmagn, float hfraction, float co, float cd, float cf, float cm)
{
	// Global landscape
	float3 p = ppoint * freq + Randomize;
	float height = (Fbm(p, 2) + 0.7) * 0.7;

	// Height distortion
	float distort = 0.01 * Fbm(ppoint * mfreq + Randomize, 8);
	height += distort;

	// Hills
	float hills = (0.5 + 1.5 * Fbm(p * 0.0721, 5)) * hfreq;
	hills = Fbm(p * hills, 5) * 0.15;

	float hillsMod = smoothstep(0, 1, Fbm(p * hfraction, 2) * 3.0);
	height *= 1.0 + hmagn * hills * hillsMod;

	// Craters
	noiseOctaves = 3; // Craters roundness distortion
	float crater = CraterNoise(ppoint, cm, cf, cd, co);

	return (height + crater);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapAsteroid(float3 ppoint, float height, float slope, float cdfreq)
{
	slope = 1 - slope;
	noiseOctaves = 2.0;

	height = DistFbm((ppoint + Randomize) * 3.7, 1.5) * 0.7 + 0.5;

	noiseOctaves = 5;

	float3 p = ppoint * cdfreq * 2.3;

	p += Fbm3D(p * 0.5) * 1.2;

	float vary = saturate((Fbm(p) + 0.7) * 0.7);
	float ccc = (vary * slope) / height;

	float4 c = float4(0.41, 0.41, 0.41, 1) * (0.5 + slope) + (0.0125 * height) + (0.05 * vary);

	return c;

	//Surface surf = GetSurfaceColor(height, slope, vary);

	//surf.color.rgb *= 0.5 + slope;

	//return surf.color;
}
//-----------------------------------------------------------------------------