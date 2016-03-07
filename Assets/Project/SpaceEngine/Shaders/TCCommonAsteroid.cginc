#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapAsteroid_Eroded(float3 ppoint, float cm, float freq, float mfreq, float cd, float co, float eros)
{
	float height = 0;
	float crater = 0;

	noiseOctaves = 3;  // Craters roundness distortion
	craterDistortion = 1.0;
	craterRoundDist = 0.03;
	heightFloor = -0.1;
	heightPeak = 0.6;
	heightRim = 1.0;
	crater = CraterNoise(ppoint, 0.5 * cm, freq, cd, co);
	noiseOctaves = 10.0;
	noiseH = 1.0;
	noiseOffset = 1.0;
	crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * mfreq + Randomize, 2.0, eros, 0.25 * crater);

	return height + crater;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float HeightMapAsteroid(float3 ppoint, float freq, float mfreq, float hfreq, float hmagn, float hfraction, float co, float cd, float cf, float cm)
{
	// Global landscape
	float3 p = ppoint * freq + Randomize;
	float height = (Fbm(p, 2) + 0.7) * 0.7;

	// Height distortion
	float distort = 0.1 * Fbm(ppoint * mfreq + Randomize, 8);
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