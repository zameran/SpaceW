#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapTerra(float3 ppoint, float vfreq, float freq, float hfreq, float cfreq, float dfreq, float mfreq, float rfreq,
									float vmagn, float dmagn, float hmagn, float cmagn, float mmagn, float rmagn,
									float slvl, float montspiky,
									float dfraction, float hfraction, float h2fraction, float cfraction,
									float md, float cd,
									float cro, float ro, float vo,
									float rsin, float cldsstyle, float latcap, float caph, float eros,
									float crato, float crm, float crf,
									float volcfreq, float vdens, float vradi, float craf)
{
	// Global landscape
	float3 p = ppoint * freq + Randomize;
	noiseOctaves = 5;
	float3 distort = 0.35 * Fbm3D(p * 2.37);
	noiseOctaves = 4;
	distort += 0.005 * (1.0 - abs(Fbm3D(p * 132.3)));
	float global = 1.0 - Cell3Noise(p + distort);

	// Venus-like structure
	float venus = 0.0;
	if (vmagn > 0.05)
	{
		noiseOctaves = 4;
		distort = Fbm3D(ppoint * 0.3) * 1.5;
		noiseOctaves = 6;
		venus = Fbm((ppoint + distort) * vfreq) * vmagn;
	}

	global = (global + venus - slvl) * 0.5 + slvl;
	float shore = saturate(70.0 * (global - slvl));

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

	float inv2montesSpiky = 1.0 / (montspiky * montspiky);
	float height = 0.0;
	float dist;

	if (biome < dfraction)
	{
		// Dunes
		noiseOctaves = 2.0;
		dist = dfreq + Fbm(p * 1.21);
		height = max(Fbm(p * dist * 0.3) + 0.7, 0.0);
		height = biomeScale * dmagn * (height + DunesNoise(ppoint, 3));
	}
	else if (biome < hfraction)
	{
		// "Eroded" hills
		noiseOctaves = 10.0;
		noiseH = 1.0;
		noiseOffset = 1.0;
		height = biomeScale * hmagn * (1.5 - RidgedMultifractal(ppoint * hfreq + Randomize, 2.0));
	}
	else if (biome < h2fraction)
	{
		// "Eroded" hills 2
		noiseOctaves = 10.0;
		noiseLacunarity = 2.0;
		height = biomeScale * hmagn * JordanTurbulence(ppoint * hfreq + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
	}
	else if (biome < cfraction)
	{
		// Canyons
		noiseOctaves = 5.0;
		noiseH = 0.9;
		noiseLacunarity = 4.0;
		noiseOffset = montspiky;
		height = -cmagn * montRage * RidgedMultifractalErodedDetail(ppoint * 4.0 * cfreq * inv2montesSpiky + Randomize, 2.0, eros, montBiomeScale);

		if (terrace < terraceProb)
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
		noiseOffset = montspiky;
		height = mmagn * montRage * RidgedMultifractalErodedDetail(ppoint * mfreq * inv2montesSpiky + Randomize, 2.0, eros, montBiomeScale);

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
	if (md > 0.05)
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
	if (cro > 0.0)
	{
		// Rim height distortion
		noiseH = 0.5;
		noiseLacunarity = 2.218281828459;
		noiseOffset = 0.8;
		noiseOctaves = 4.0;
		float dunes = 2 * cracksMagn * (0.2 + dmagn * max(Fbm(ppoint * dfreq) + 0.7, 0.0));
		noiseOctaves = 6.0;  // Cracks roundness distortion
		height += CrackNoise(ppoint, dunes, craf, cro, 1);
	}
	
	// Craters
	float crater = 0.0;
	if (cd > 0.05)
	{
		noiseOctaves = 3;  // Craters roundness distortion
		craterDistortion = 1.0;
		craterRoundDist = 0.03;
		heightFloor = -0.1;
		heightPeak = 0.6;
		heightRim = 1.0;
		crater = CraterNoise(ppoint, 0.5 * craterMagn, craterFreq, cd, craterOctaves);
		noiseOctaves = 10.0;
		noiseH = 1.0;
		noiseOffset = 1.0;
		crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * montesFreq + Randomize, 2.0, eros, 0.25 * crater);
	}

	height += mare + crater;

	// Pseudo rivers
	if (ro > 0)
	{
		noiseOctaves = ro;
		noiseLacunarity = 2.218281828459;
		noiseH = 0.5;
		noiseOffset = 0.8;
		p = ppoint * freq + Randomize;
		distort = 0.035 * Fbm3D(p * rsin * 5.0);
		distort += 0.350 * Fbm3D(p * rsin);
		cell = Cell3Noise2(rfreq * p + distort);
		float pseudoRivers = 1.0 - saturate(abs(cell.y - cell.x) * rmagn);
		pseudoRivers = smoothstep(0.0, 1.0, pseudoRivers);
		pseudoRivers *= 1.0 - smoothstep(0.06, 0.10, global - slvl); // disable rivers inside continents
		pseudoRivers *= 1.0 - smoothstep(0.00, 0.01, slvl - height); // disable rivers inside oceans
		height = lerp(height, slvl - 0.02, pseudoRivers);
	}

	// Shield volcano
	if (vo > 0)
	{
		noiseOctaves = 3;  // volcano roundness distortion
		craterRoundDist = 0.001;
		height = VolcanoNoise(ppoint, global, height); // global - 1.0 to fix flooding of the canyons and river beds with lava
	}

	/*
	//--------------------------------------------------------------------------------------------------------
	// Mare
	float mare = global;
	float mareFloor = global;
	float mareSuppress = 1.0;

	noiseH = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset = 0.8;
	craterDistortion = 1.0;
	noiseOctaves = 6.0;  // Mare roundness distortion
	mare = MareNoise(ppoint, global, 0.0, mareSuppress);
	//--------------------------------------------------------------------------------------------------------
	
	//--------------------------------------------------------------------------------------------------------
	// Ice cracks
	// Rim height distortion
	noiseH = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset = 0.8;
	noiseOctaves = 4.0;
	float dunes = 2 * cracksMagn * (0.2 + dmagn * max(Fbm(ppoint * dfreq) + 0.7, 0.0));
	noiseOctaves = 6.0;  // Cracks roundness distortion
	height += CrackNoise(ppoint, dunes, craf, cro, 1);
	//--------------------------------------------------------------------------------------------------------

	//--------------------------------------------------------------------------------------------------------
	// Craters
	float crater = 0.0;

	noiseOctaves = 3;  // Craters roundness distortion
	craterDistortion = 1.0;
	craterRoundDist = 0.03;
	heightFloor = -0.1;
	heightPeak = 0.6;
	heightRim = 1.0;
	crater = CraterNoise(ppoint, 0.5 * crm, crf, cd, crato);
	noiseOctaves = 10.0;
	noiseH = 1.0;
	noiseOffset = 1.0;
	crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * mfreq + Randomize, 2.0, eros, 0.25 * crater);	
	//--------------------------------------------------------------------------------------------------------
	
	height += mare + crater;

	//--------------------------------------------------------------------------------------------------------
	// Pseudo rivers
	noiseOctaves = ro;
	noiseLacunarity = 2.218281828459;
	noiseH = 0.5;
	noiseOffset = 0.8;
	p = ppoint * freq + Randomize;
	distort = 0.035 * Fbm3D(p * rsin * 5.0);
	distort += 0.350 * Fbm3D(p * rsin);
	cell = Cell3Noise2(rfreq * p + distort);
	float pseudoRivers = 1.0 - saturate(abs(cell.y - cell.x) * rmagn);
	pseudoRivers = smoothstep(0.0, 1.0, pseudoRivers);
	pseudoRivers *= 1.0 - smoothstep(0.06, 0.10, global - slvl); // disable rivers inside continents
	pseudoRivers *= 1.0 - smoothstep(0.00, 0.01, slvl - height); // disable rivers inside oceans
	height = lerp(height, slvl - 0.02, pseudoRivers);
	//--------------------------------------------------------------------------------------------------------
	
	//--------------------------------------------------------------------------------------------------------
	// Shield volcano
	//noiseOctaves = 3;  // volcano roundness distortion
	//craterRoundDist = 0.001;
	//height = VolcanoNoise(ppoint, global, height, volcfreq, vdens, vradi, vo); // global - 1.0 to fix flooding of the canyons and river beds with lava
	//--------------------------------------------------------------------------------------------------------
	*/

	// Assign a climate type
	noiseOctaves = (cldsstyle == 1.0) ? 5.0 : 12.0;
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
	float iceCap = saturate((latitude / latcap - 1.0) * 50.0 * cldsstyle);
	height = height * cldsstyle + caph * smoothstep(0.0, 1.0, iceCap);

	return height;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapTerra(float3 ppoint, float mfreq, float dfraction, float hfraction, float h2fraction, float cfraction)
{
	float3  p = ppoint * mfreq + Randomize;
	float4  col;
	noiseOctaves = 6;
	float3  distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06);
	float2  cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));

	float4  colorOverlay;
	if (biome < dfraction)
		colorOverlay = float4(1.0, 1.0, 0.0, 0.0);
	else if (biome < hfraction)
		colorOverlay = float4(0.0, 1.0, 0.0, 0.0);
	else if (biome < h2fraction)
		colorOverlay = float4(0.0, 1.0, 0.5, 0.0);
	else if (biome < cfraction)
		colorOverlay = float4(1.0, 0.0, 0.0, 0.0);
	else
		colorOverlay = float4(1.0, 1.0, 1.0, 0.0);

	return colorOverlay * biomeScale;
}
//-----------------------------------------------------------------------------

//#define VISUALIZE_BIOMES
float4 ColorMapTerra(float3 ppoint, float height, float slope)
{
	float3  p = ppoint * mainFreq + Randomize;
	float4  col;
	noiseOctaves = 6;
	float3  distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06);
	float2  cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float vary;

	#ifdef VISUALIZE_BIOMES
	float4  colorOverlay;
	if (biome < dunesFraction)
		colorOverlay = float4(1.0, 1.0, 0.0, 0.0);
	else if (biome < hillsFraction)
		colorOverlay = float4(0.0, 1.0, 0.0, 0.0);
	else if (biome < hills2Fraction)
		colorOverlay = float4(0.0, 1.0, 0.5, 0.0);
	else if (biome < canyonsFraction)
		colorOverlay = vec4float4(1.0, 0.0, 0.0, 0.0);
	else
		colorOverlay = float4(1.0, 1.0, 1.0, 0.0);
	#endif

	noiseOctaves = 2;
	distort  = 0.035 * Fbm3D(p * 53.1);
	distort += 0.350 * Fbm3D(p *  5.8);
	cell = Cell3Noise2(3.1 * p + distort);
	float pseudoRivers = saturate(1.0 - abs(cell.y - cell.x) * 60.0);
	pseudoRivers *= 1.0 - saturate((height / seaLevel - 1.0) * 2.0);

	Surface surf;

	// Assign a climate type
	noiseOctaves    = 6.0;
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;

	float climate, latitude, dist;

	if (tidalLock <= 0.0)
	{
		latitude = abs(ppoint.y);
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
		latitude = saturate(latitude);
		if (latitude < latTropic - tropicWidth)
			climate = lerp(climateTropic, climateEquator, (latTropic - tropicWidth - latitude) / latTropic);
		else if (latitude > latTropic + tropicWidth)
			climate = lerp(climateTropic, climatePole, (latitude - latTropic - tropicWidth) / (1.0 - latTropic));
		else
			climate = climateTropic;
	}
	else
	{
		latitude = 1.0 - ppoint.x;
		latitude += 0.15 * (Fbm(ppoint * 0.7 + Randomize) - 1.0);
		climate = lerp(climateTropic, climatePole, saturate(latitude));
	}

	// Change climate with elevation
	float montHeight = saturate((height - seaLevel) / (snowLevel - seaLevel));
	vary = 0.125 * montHeight * Fbm(ppoint * 1700.0 + Randomize);
	climate = min(climate + heightTempGrad * montHeight + vary, climatePole);

	// Beach
	float beach = saturate((height / seaLevel - 1.0) * 50.0);
	climate = lerp(0.375, climate, beach);

	// Dunes must be made of sand only
	float dunes = step(dunesFraction, biome) * biomeScale;
	slope *= dunes;

	// Ice caps
	float iceCap = saturate((latitude / latIceCaps - 1.0) * 50.0);
	climate = lerp(climate, climatePole, iceCap);

	// Flatland climate distortion
	noiseOctaves = 4;
	float3  pp = (ppoint + Randomize) * (0.0005 * hillsFreq / (hillsMagn * hillsMagn));
	float fr = 0.20 * (1.5 - RidgedMultifractal(pp,         2.0)) +
			   0.05 * (1.5 - RidgedMultifractal(pp * 10.0,  2.0)) +
			   0.02 * (1.5 - RidgedMultifractal(pp * 100.0, 2.0));
	p = ppoint * (colorDistFreq * 0.005) + float3(fr, fr, fr);
	p += Fbm3D(p * 0.38) * 1.2;
	vary = (Fbm(p) + 0.7) * 0.7 * 0.5;
	climate += vary * beach * saturate(1.0 - 3.0 * slope) * saturate(1.0 - 1.333 * climate);

	// Dunes must be made of sand only
	//climate = lerp(0.0, climate, dunes);

	// Color texture distortion
	noiseOctaves = 5;
	p = ppoint * colorDistFreq * 0.371;
	p += Fbm3D(p * 0.5) * 1.2;
	vary = saturate((Fbm(p) + 0.7) * 0.7);

	// Shield volcano lava
	float lavaMask = 0.0;
	if (volcanoOctaves > 0)
	{
		// Global volcano activity mask
		noiseOctaves = 3;
		float volcActivity = saturate((Fbm(ppoint * 1.37 + Randomize) - 1.0 + volcanoActivity) * 5.0);
		// Lava in volcano caldera and flows
		noiseOctaves = 3;  // volcano roundness distortion
		craterRoundDist = 0.001;
		lavaMask = VolcanoGlowNoise(ppoint) * volcActivity;
		// Model lava as rocks texture
		climate = lerp(climate, 0.375, lavaMask);
		slope   = lerp(slope,   1.0,   lavaMask);
	}

	surf = GetSurfaceColor(climate, slope, vary);

	// Sedimentary layers
	noiseOctaves = 4;
	float layers = Fbm(float3(height * 168.4 + 0.17 * vary, 0.43 * (p.x + p.y), 0.43 * (p.z - p.y)));
	//layers *= smoothstep(0.75, 0.8, climate) * (1.0 - smoothstep(0.825, 0.875, climate)); // only rock texture
	layers *= smoothstep(0.5, 0.55, slope);     // only steep slopes
	layers *= step(surf.color.a, 0.01);         // do not make layers on snow
	layers *= saturate(1.0 - 5.0 * lavaMask);   // do not make layers on lava
	surf.color.rgb *= float3(1.0, 1.0, 1.0) - float3(0.0, 0.5, 1.0) * layers;

	// Albedo variations
	noiseOctaves = 4;
	distort = Fbm3D((ppoint + Randomize) * 0.07) * 1.5;
	noiseOctaves = 5;
	vary = Fbm((ppoint + distort) * 0.78);
	surf.color *= 1.0 - 0.5 * vary;

	#ifdef VISUALIZE_BIOMES
		surf.color = lerp(surf.color, colorOverlay * biomeScale, 0.25);
	#endif

	if (surfType <= 3)   // water mask for planets with oceans
		surf.color.a += saturate((seaLevel - height) * 200.0);

	return surf.color;
}