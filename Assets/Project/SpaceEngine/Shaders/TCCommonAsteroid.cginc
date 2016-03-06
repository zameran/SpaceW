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

	//float4 c = float4(0.41, 0.41, 0.41, 1) * (0.5 + slope) + (0.0125 * height) + (0.05 * vary);

	//return c;

	Surface surf = GetSurfaceColor(height, slope, vary);

	surf.color.rgb *= 0.5 + slope;

	return surf.color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapAsteroid(float3 ppoint, float height, float slope, float cdfreq, float st, float lc)
{
	// Thermal emission temperature (in thousand Kelvins)
	noiseOctaves = 5;
	float3  p = ppoint * 600.0 + Randomize;
	float dist = 10.0 * cdfreq * Fbm(p * 0.2);
	noiseOctaves = 3;
	float globTemp = 0.95 - abs(Fbm((p + dist) * 0.01)) * 0.08;
	noiseOctaves = 8;
	float varyTemp = abs(Fbm(p + dist));

	// Global surface melting
	float surfTemp = st *
		(globTemp + varyTemp * 0.08) *
		saturate(2.0 * (lc * 0.4 + 0.4 - 0.8 * height));

	float4 outColor;
	outColor.rgb = UnitToColor24(log(surfTemp) * 0.188 + 0.1316);
	outColor.a = 1.0;
	return outColor;
}
//-----------------------------------------------------------------------------