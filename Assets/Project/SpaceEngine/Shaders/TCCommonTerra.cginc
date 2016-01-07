#include "TCCommon.cginc"

//-----------------------------------------------------------------------------
float HeightMapTerra(float3 ppoint, float vfreq, float freq, float hfreq, float cfreq, float dfreq, float mfreq, float rfreq,
									float vmagn, float dmagn, float hmagn, float cmagn, float mmagn, float rmagn,
									float slvl, float montspiky,
									float dfraction, float hfraction, float h2fraction, float cfraction,
									float md, float cd,
									float cro, float ro, float vo,
									float rsin, float cldsstyle, float latcap, float caph, float eros)
{
	//venusFreq = 0.01;
	//mainFreq = 0.37;
	//hillsFreq = 0.59;
	//canyonsFreq = 0.37;
	//dunesFreq = 0.62;
	//montesFreq = 0.41;
	//riversFreq = 0.33;
	//venusMagn = 0;
	//dunesMagn = 0.07;
	//hillsMagn = 0.08;
	//canyonsMagn = 0.09;
	//montesMagn = 0.09;
	//riversMagn = 0.07;
	//seaLevel = 0;
	//montesSpiky = 0.25;
	//dunesFraction = 0.18;
	//hillsFraction = 0.18;
	//hills2Fraction = 0.21;
	//canyonsFraction = 0.15;

	//mareSqrtDensity = 0.0;
	//craterSqrtDensity = 0.0;

	//cracksOctaves = 0;
	//riversOctaves = 0;
	//volcanoOctaves = 0;

	//riversSin = 0.5;

	//cloudsStyle = 1;

	//latIceCaps = 1.5;
	//icecapHeight = 0.1;

	//erosion = 1;

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
		noiseOffset = montspiky;
		height = mmagn * montRage * RidgedMultifractalErodedDetail(ppoint * mfreq * inv2montesSpiky + Randomize, 2.0, eros, montBiomeScale);

		if (terrace < terraceProb)
		{
			float h = height * terraceLayers;
			height = (floor(h) + smoothstep(0.1, 0.9, frac(h))) / terraceLayers;
		}
	}
	
	/*
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
		height += CrackNoise(ppoint, dunes);
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
	*/
	
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
	float iceCap = saturate((latitude / latcap - 1.0) * 50.0 * cldsstyle);
	height = height * cldsstyle + caph * smoothstep(0.0, 1.0, iceCap);

	return height;
}

float HeightMapTerra(float3 ppoint)
{
	//venusFreq = 0.01;
	//mainFreq = 0.37;
	//hillsFreq = 0.59;
	//canyonsFreq = 0.37;
	//dunesFreq = 0.62;
	//montesFreq = 0.41;
	//riversFreq = 0.33;
	//venusMagn = 0;
	//dunesMagn = 0.07;
	//hillsMagn = 0.08;
	//canyonsMagn = 0.09;
	//montesMagn = 0.09;
	//riversMagn = 0.07;
	//seaLevel = 0;
	//montesSpiky = 0.25;
	//dunesFraction = 0.18;
	//hillsFraction = 0.18;
	//hills2Fraction = 0.21;
	//canyonsFraction = 0.15;

	mareSqrtDensity = 0.0;
	craterSqrtDensity = 0.0;

	cracksOctaves = 0;
	riversOctaves = 0;
	volcanoOctaves = 0;

	riversSin = 0.5;
	
	cloudsStyle = 1;

	latIceCaps = 1.5;
	icecapHeight = 0.1;

	erosion = 1;
	
	//-----------------------------------------------------------------------------
	#define     tidalLock           scaleParams.w
	#define		mainFreq			mainParams.x
	#define     terraceProb         mainParams.y
	#define     surfType            mainParams.z
	#define     snowLevel           mainParams.w
	#define     colorDistMagn       colorParams.x
	#define     colorDistFreq       colorParams.y
	#define     latIceCaps          colorParams.z
	#define     latTropic           colorParams.w
	#define     climatePole         climateParams.x
	#define     climateTropic       climateParams.y
	#define     climateEquator      climateParams.z
	#define     tropicWidth         climateParams.w
	#define     seaLevel            mareParams.x
	#define     mareFreq            mareParams.y
	#define     mareSqrtDensity     mareParams.z
	#define     icecapHeight        mareParams.w
	#define     montesMagn          montesParams.x
	#define     montesFreq          montesParams.y
	#define     montesFraction      montesParams.z
	#define     montesSpiky         montesParams.w
	#define     dunesMagn           dunesParams.x
	#define     dunesFreq           dunesParams.y
	#define     dunesFraction       dunesParams.z
	#define     drivenDarkening     dunesParams.w
	#define     hillsMagn           hillsParams.x
	#define     hillsFreq           hillsParams.y
	#define     hillsFraction       hillsParams.z
	#define     hills2Fraction      hillsParams.w
	#define     canyonsMagn         canyonsParams.x
	#define     canyonsFreq         canyonsParams.y
	#define     canyonsFraction     canyonsParams.z
	#define     erosion             canyonsParams.w
	#define     riversMagn          riversParams.x
	#define     riversFreq          riversParams.y
	#define     riversSin           riversParams.z
	#define     riversOctaves       riversParams.w
	#define     cracksMagn          cracksParams.x
	#define     cracksFreq          cracksParams.y
	#define     cracksOctaves       cracksParams.z
	#define     craterRayedFactor   cracksParams.w
	#define     craterMagn          craterParams.x
	#define     craterFreq          craterParams.y
	#define     craterSqrtDensity   craterParams.z
	#define     craterOctaves       craterParams.w
	#define     volcanoMagn         volcanoParams1.x
	#define     volcanoFreq         volcanoParams1.y
	#define     volcanoSqrtDensity  volcanoParams1.z
	#define     volcanoOctaves      volcanoParams1.w
	#define     volcanoActivity     volcanoParams2.x
	#define     volcanoFlows        volcanoParams2.y
	#define     volcanoRadius       volcanoParams2.z
	#define     volcanoTemp         volcanoParams2.w
	#define     lavaCoverage        lavaParams.x
	#define     lavaTemperature     lavaParams.y
	#define     surfTemperature     lavaParams.z
	#define     heightTempGrad      lavaParams.w
	#define     texScale            textureParams.x
	#define     texColorConv        textureParams.y
	#define     venusMagn           textureParams.z
	#define     venusFreq           textureParams.w
	#define     cloudsFreq          cloudsParams1.x
	#define     cloudsOctaves       cloudsParams1.y
	#define     twistZones          cloudsParams1.z
	#define     twistMagn           cloudsParams1.w
	#define     cloudsLayer         cloudsParams2.x
	#define     cloudsNLayers       cloudsParams2.y
	#define     cloudsStyle         cloudsParams2.z
	#define     cloudsCoverage      cloudsParams2.w
	#define     cycloneMagn         cycloneParams.x
	#define     cycloneFreq         cycloneParams.y
	#define     cycloneSqrtDensity  cycloneParams.z
	#define     cycloneOctaves      cycloneParams.w
	//-----------------------------------------------------------------------------
	
    // Global landscape
    float3  p = ppoint * mainParams.x + Randomize;
    noiseOctaves = 5;
    float3  distort = 0.35 * Fbm3D(p * 2.37);
    noiseOctaves = 4;
    distort += 0.005 * (1.0 - abs(Fbm3D(p * 132.3)));
    float global = 1.0 - Cell3Noise(p + distort);

    // Venus-like structure
    float venus = 0.0;
    if (textureParams.z > 0.05)
    {
        noiseOctaves = 4;
        distort = Fbm3D(ppoint * 0.3) * 1.5;
        noiseOctaves = 6;
        venus = Fbm((ppoint + distort) * textureParams.w) * textureParams.z;
    }

    global = (global + venus - mareParams.x) * 0.5 + mareParams.x;
    float shore = saturate(70.0 * (global - mareParams.x));

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

    float inv2montesSpiky = 1.0 /(montesParams.w*montesParams.w);
    float height = 0.0;
    float dist;

    if (biome < dunesParams.z)
    {
        // Dunes
        noiseOctaves = 2.0;
        dist = dunesParams.y + Fbm(p * 1.21);
        height = max(Fbm(p * dist * 0.3) + 0.7, 0.0);
        height = biomeScale * dunesParams.x * (height + DunesNoise(ppoint, 3));
    }
    else if (biome < hillsParams.z)
    {
        // "Eroded" hills
        noiseOctaves = 10.0;
        noiseH       = 1.0;
        noiseOffset  = 1.0;
        height = biomeScale * hillsParams.x * (1.5 - RidgedMultifractal(ppoint * hillsParams.y + Randomize, 2.0));
    }
    else if (biome < hillsParams.w)
    {
        // "Eroded" hills 2
        noiseOctaves = 10.0;
        noiseLacunarity = 2.0;
        height = biomeScale * hillsParams.x * JordanTurbulence(ppoint * hillsParams.y + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
    }
    else if (biome < canyonsParams.z)
    {
        // Canyons
        noiseOctaves = 5.0;
        noiseH       = 0.9;
        noiseLacunarity = 4.0;
        noiseOffset  = montesParams.w;
        height = -canyonsParams.x * montRage * RidgedMultifractalErodedDetail(ppoint * 4.0 * canyonsParams.y * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

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
        noiseH       = 1.0;
        noiseOffset  = montesParams.w;
        height = montesParams.x * montRage * RidgedMultifractalErodedDetail(ppoint * montesParams.y * inv2montesSpiky + Randomize, 2.0, erosion, montBiomeScale);

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
        noiseH           = 0.5;
        noiseLacunarity  = 2.218281828459;
        noiseOffset      = 0.8;
        craterDistortion = 1.0;
        noiseOctaves     = 6.0;  // Mare roundness distortion
        mare = MareNoise(ppoint, global, 0.0, mareSuppress);
    }

    // Ice cracks
    if (cracksOctaves > 0.0)
    {
        // Rim height distortion
        noiseH          = 0.5;
        noiseLacunarity = 2.218281828459;
        noiseOffset     = 0.8;
        noiseOctaves    = 4.0;
        float dunes = 2 * cracksMagn * (0.2 + dunesParams.x * max(Fbm(ppoint * dunesParams.y) + 0.7, 0.0));
        noiseOctaves    = 6.0;  // Cracks roundness distortion
        height += CrackNoise(ppoint, dunes);
    }

    // Craters
    float crater = 0.0;
    if (craterSqrtDensity > 0.05)
    {
        noiseOctaves = 3;  // Craters roundness distortion
        craterDistortion = 1.0;
        craterRoundDist  = 0.03;
        heightFloor = -0.1;
        heightPeak  = 0.6;
        heightRim   = 1.0;
        crater = CraterNoise(ppoint, 0.5 * craterMagn, craterFreq, craterSqrtDensity, craterOctaves);
        noiseOctaves = 10.0;
        noiseH       = 1.0;
        noiseOffset  = 1.0;
        crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * montesParams.y + Randomize, 2.0, erosion, 0.25 * crater);
    }

    height += mare + crater;

    // Pseudo rivers
    if (riversOctaves > 0)
    {
        noiseOctaves     = riversOctaves;
        noiseLacunarity  = 2.218281828459;
        noiseH           = 0.5;
        noiseOffset      = 0.8;
        p = ppoint * mainFreq + Randomize;
        distort  = 0.035 * Fbm3D(p * riversSin * 5.0);
        distort += 0.350 * Fbm3D(p * riversSin);
        cell = Cell3Noise2(riversParams.y * p + distort);
        float pseudoRivers = 1.0 - saturate(abs(cell.y - cell.x) * riversParams.x);
        pseudoRivers = smoothstep(0.0, 1.0, pseudoRivers);
        pseudoRivers *= 1.0 - smoothstep(0.06, 0.10, global - mareParams.x); // disable rivers inside continents
        pseudoRivers *= 1.0 - smoothstep(0.00, 0.01, mareParams.x - height); // disable rivers inside oceans
        height = lerp(height, mareParams.x-0.02, pseudoRivers);
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
    noiseH          = 0.5;
    noiseLacunarity = 2.218281828459;
    noiseOffset     = 0.8;
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
float4 ColorMapTerra(float3 ppoint, float mfreq, float dfraction, float hfraction, float h2fraction, float cfraction)
{
	float3  p = ppoint * mfreq + Randomize;
	float4  col;
	noiseOctaves = 6;
	float3  distort = p * 2.3 + 13.5 * Fbm3D(p * 0.06);
	float2  cell = Cell3Noise2Color(distort, col);
	float biome = col.r;
	float biomeScale = saturate(2.0 * (pow(abs(cell.y - cell.x), 0.7) - 0.05));
	float vary;

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