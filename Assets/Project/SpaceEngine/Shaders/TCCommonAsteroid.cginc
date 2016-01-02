#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapAsteroid(float ppoint)
{
	mainFreq = 0.59;
	montesFreq = 0.789;
	hillsMagn = 0.66972;
	hillsFraction = 0.25184;

	// Global landscape
	noiseOctaves = 8.0;
	float3 p = ppoint * mainFreq + Randomize;
	float height = (Fbm(p) + 0.7) * 0.7;

	noiseOctaves = 8; // Height distortion
	float distort = 0.01 * Fbm(ppoint * montesFreq + Randomize);
	//height += distort;
	height *= distort;

	// Hills
	noiseOctaves = 5;
	float hills = (0.5 + 1.5 * Fbm(p * 0.0721)) * hillsFreq;
	hills = Fbm(p * hills) * 0.15;
	noiseOctaves = 2;
	float hillsMod = smoothstep(0, 1, Fbm(p * hillsFraction) * 3.0);
	height *= 1.0 + hillsMagn * hills * hillsMod;

	// Craters
	noiseOctaves = 4;   // Craters roundness distortion
	craterDistortion = 1.0;
	craterRoundDist = 0.03;
	heightFloor = -0.1;
	heightPeak = 0.6;
	heightRim = 1.0;
	float crater = CraterNoise(ppoint, craterMagn, craterFreq, craterSqrtDensity, craterOctaves);

	return (height + crater);
}

float HeightMapAsteroid(float3 ppoint, float freq, float mfreq, float hfreq, float hmagn, float hfraction)
{
	// Global landscape
	noiseOctaves = 2;
	float3 p = ppoint * freq + Randomize;
	float height = (Fbm(p) + 0.7) * 0.7;

	noiseOctaves = 8; // Height distortion
	float distort = 0.01 * Fbm(ppoint * mfreq + Randomize);
	//height += distort;
	height *= distort;

	// Hills
	noiseOctaves = 5;
	float hills = (0.5 + 1.5 * Fbm(p * 0.0721)) * hfreq;
	hills = Fbm(p * hills) * 0.15;

	noiseOctaves = 2;
	float hillsMod = smoothstep(0, 1, Fbm(p * hfraction) * 3.0);
	height *= 1.0 + hmagn * hills * hillsMod;

	// Craters
	noiseOctaves = 3;   // Craters roundness distortion
	craterOctaves = 4;
	craterSqrtDensity = 2;
	craterFreq = 1;
	craterMagn = 1;
	craterDistortion = 0.05;
	craterRoundDist = 0.03;
	heightFloor = -0.1;
	heightPeak = 0.6;
	heightRim = 1.0;
	float crater = CraterNoise(ppoint, craterMagn, craterFreq, craterSqrtDensity, craterOctaves);

	//return crater;
	return (height + crater);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapAsteroid(float3 ppoint, float height, float slope)
{
	noiseOctaves = 2.0;

	height = DistFbm((ppoint + Randomize) * 3.7, 1.5) * 0.7 + 0.5;

	noiseOctaves = 5;

	float3 p = ppoint * colorDistFreq * 2.3;

	p += Fbm3D(p * 0.5) * 1.2;

	float vary = saturate((Fbm(p) + 0.7) * 0.7);

	Surface surf = GetSurfaceColor(height, slope, vary);

	surf.color.rgb *= 0.5 + slope;

	return surf.color;
}

float4 GetColor()
{
	float3 ppoint = GetSurfacePoint();

	float height, slope;

	GetSurfaceHeightAndSlope(height, slope);

	return ColorMapAsteroid(ppoint, height, slope);
}
//-----------------------------------------------------------------------------