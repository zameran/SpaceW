#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapTerra(float3 ppoint)
{
	erosion = 1;

	mainFreq = 0.37;
	hillsFreq = 0.59;
	canyonsFreq = 0.37;
	dunesFreq = 0.62;
	montesFreq = 0.41;
	riversFreq = 0.33;
	venusMagn = 0;
	dunesMagn = 0.07;
	hillsMagn = 0.08;
	canyonsMagn = 0.09;
	montesMagn = 0.09;
	riversMagn = 0.07;
	seaLevel = 0;
	montesSpiky = 0.25;
	dunesFraction = 0.18;
	hillsFraction = 0.18;
	hills2Fraction = 0.21;
	canyonsFraction = 0.15;

	mareSqrtDensity = 0.0;
	craterSqrtDensity = 0.0;

	cracksOctaves = 0;
	riversOctaves = 0;
	volcanoOctaves = 0;

	riversSin = 0.5;

	cloudsStyle = 1;

	latIceCaps = 1.5;
	icecapHeight = 0.1;

	// Global landscape
	float3 p = ppoint * mainFreq + Randomize;
	noiseOctaves = 5;
	float3 distort = 0.35 * Fbm3D(p * 2.37);
	noiseOctaves = 4;
	distort += 0.005 * (1.0 - abs(Fbm3D(p * 132.3)));
	float global = 1.0 - Cell3Noise(p + distort);

	// Venus-like structure
	float venus = 0.0;
	if (venusMagn > 0.05)
	{
		noiseOctaves = 4;
		distort = Fbm3D(ppoint * 0.3) * 1.5;
		noiseOctaves = 6;
		venus = Fbm((ppoint + distort) * venusFreq) * venusMagn;
	}

	global = (global + venus - seaLevel) * 0.5 + seaLevel;
	float shore = saturate(70.0 * (global - seaLevel));

	float4  col;
	noiseOctaves = 6;
	p = p * 2.3 + 13.5 * Fbm3D(p * 0.06);
	float2  cell = Cell3Noise2Color(p, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float terrace = col.g;
	float terraceLayers = max(col.b * 10.0 + 3.0, 3.0);
	terraceLayers += Fbm(p * 5.41);

	float montRage = saturate(DistNoise(ppoint * 22.6 + Randomize, 2.5) + 0.5);
	montRage *= montRage;
	float montBiomeScale = min(pow(2.2 * biomeScale, 2.5), 1.0) * montRage;

	float inv2montesSpiky = 1.0 / (montesSpiky*montesSpiky);
	float height = 0.0;
	float dist;

	if (biome < dunesFraction)
	{
		// Dunes
		noiseOctaves = 2.0;
		dist = dunesFreq + Fbm(p * 1.21);
		height = max(Fbm(p * dist * 0.3) + 0.7, 0.0);
		height = biomeScale * dunesMagn * (height + DunesNoise(ppoint, 3));
	}
	else if (biome < hillsFraction)
	{
		// "Eroded" hills
		noiseOctaves = 10.0;
		noiseH = 1.0;
		noiseOffset = 1.0;
		height = biomeScale * hillsMagn * (1.5 - RidgedMultifractal(ppoint * hillsFreq + Randomize, 2.0));
	}
	else if (biome < hills2Fraction)
	{
		// "Eroded" hills 2
		noiseOctaves = 10.0;
		noiseLacunarity = 2.0;
		height = biomeScale * hillsMagn * JordanTurbulence(ppoint * hillsFreq + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
	}
	else if (biome < canyonsFraction)
	{
		// Canyons
		noiseOctaves = 5.0;
		noiseH = 0.9;
		noiseLacunarity = 4.0;
		noiseOffset = montesSpiky;
		height = -canyonsMagn * montRage * RidgedMultifractalErodedDetail(ppoint * 4.0 * canyonsFreq * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

		//if (terrace < terraceProb)
		{
			terraceLayers *= 5.0;
			float h = height * terraceLayers;
			height = (floor(h) + smoothstep(0.1, 0.9, frac(h))) / terraceLayers;
		}
	}
	else
	{
		// Mountains
		noiseOctaves = 10.0;
		noiseH = 1.0;
		noiseOffset = montesSpiky;
		height = montesMagn * montRage * RidgedMultifractalErodedDetail(ppoint * montesFreq * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

		if (terrace < terraceProb)
		{
			float h = height * terraceLayers;
			height = (floor(h) + smoothstep(0.1, 0.9, frac(h))) / terraceLayers;
		}
	}

	height *= shore;

	// Mare
	float mare = global;
	float mareFloor = global;
	float mareSuppress = 1.0;
	if (mareSqrtDensity > 0.05)
	{
		//noiseOctaves = 2;
		//mareFloor = 0.6 * (1.0 - Cell3Noise(0.3*p));
		noiseH = 0.5;
		noiseLacunarity = 2.218281828459;
		noiseOffset = 0.8;
		craterDistortion = 1.0;
		noiseOctaves = 6.0;  // Mare roundness distortion
		mare = MareNoise(ppoint, global, 0.0, mareSuppress);
	}

	// Ice cracks
	if (cracksOctaves > 0.0)
	{
		// Rim height distortion
		noiseH = 0.5;
		noiseLacunarity = 2.218281828459;
		noiseOffset = 0.8;
		noiseOctaves = 4.0;
		float dunes = 2 * cracksMagn * (0.2 + dunesMagn * max(Fbm(ppoint * dunesFreq) + 0.7, 0.0));
		noiseOctaves = 6.0;  // Cracks roundness distortion
		height += CrackNoise(ppoint, dunes);
	}

	// Craters
	float crater = 0.0;
	if (craterSqrtDensity > 0.05)
	{
		noiseOctaves = 3;  // Craters roundness distortion
		craterDistortion = 1.0;
		craterRoundDist = 0.03;
		heightFloor = -0.1;
		heightPeak = 0.6;
		heightRim = 1.0;
		crater = CraterNoise(ppoint, 0.5 * craterMagn, craterFreq, craterSqrtDensity, craterOctaves);
		noiseOctaves = 10.0;
		noiseH = 1.0;
		noiseOffset = 1.0;
		crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * montesFreq + Randomize, 2.0, erosion, 0.25 * crater);
	}

	height += mare + crater;

	// Pseudo rivers
	if (riversOctaves > 0)
	{
		noiseOctaves = riversOctaves;
		noiseLacunarity = 2.218281828459;
		noiseH = 0.5;
		noiseOffset = 0.8;
		p = ppoint * mainFreq + Randomize;
		distort = 0.035 * Fbm3D(p * riversSin * 5.0);
		distort += 0.350 * Fbm3D(p * riversSin);
		cell = Cell3Noise2(riversFreq * p + distort);
		float pseudoRivers = 1.0 - saturate(abs(cell.y - cell.x) * riversMagn);
		pseudoRivers = smoothstep(0.0, 1.0, pseudoRivers);
		pseudoRivers *= 1.0 - smoothstep(0.06, 0.10, global - seaLevel); // disable rivers inside continents
		pseudoRivers *= 1.0 - smoothstep(0.00, 0.01, seaLevel - height); // disable rivers inside oceans
		height = lerp(height, seaLevel - 0.02, pseudoRivers);
	}

	// Shield volcano
	if (volcanoOctaves > 0)
	{
		noiseOctaves = 3;  // volcano roundness distortion
		craterRoundDist = 0.001;
		height = VolcanoNoise(ppoint, global, height); // global - 1.0 to fix flooding of the canyons and river beds with lava
	}

	// Assign a climate type
	noiseOctaves = (cloudsStyle == 1.0) ? 5.0 : 12.0;
	noiseH = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset = 0.8;
	float latitude;
	if (tidalLock <= 0.0)
	{
		latitude = abs(ppoint.y);
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
		latitude = saturate(latitude);
	}
	else
	{
		latitude = 1.0 - ppoint.x;
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
	}

	// Ice caps;
	// cloudsStyle = 0.1 for oceania, 1.0 for other planets
	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0 * cloudsStyle);
	height = height * cloudsStyle + icecapHeight * smoothstep(0.0, 1.0, iceCap);

	return height;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapTerra(float3 ppoint)
{
	float3  p = ppoint * mainFreq + Randomize;
	float4  col;
	noiseOctaves = 6;
	float3  distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06);
	float2  cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float vary;

	float4  colorOverlay;
	if (biome < dunesFraction)
		colorOverlay = float4(1.0, 1.0, 0.0, 0.0);
	else if (biome < hillsFraction)
		colorOverlay = float4(0.0, 1.0, 0.0, 0.0);
	else if (biome < hills2Fraction)
		colorOverlay = float4(0.0, 1.0, 0.5, 0.0);
	else if (biome < canyonsFraction)
		colorOverlay = float4(1.0, 0.0, 0.0, 0.0);
	else
		colorOverlay = float4(1.0, 1.0, 1.0, 0.0);

	return colorOverlay * biomeScale;
}
//-----------------------------------------------------------------------------