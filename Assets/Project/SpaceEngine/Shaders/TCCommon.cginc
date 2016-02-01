//-----------------------------------------------------------------------------
// tile blending method:
// 0 - hard mix (no blending)
// 1 - soft blending
// 2 - "smart" blening (tile heightmap based)
#define TILE_BLEND_MODE 1
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// tiling fix method:
// 0 - no tiling fix
// 1 - sampling texture 2 times at different scales
// 2 - voronoi random offset
// 3 - voronoi random offset and rotation
#define TILING_FIX_MODE 2
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define USESAVEPOW
#define USETEXLOD
#define PACKED_NORMALS
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define ATLAS_RES_X         8
#define ATLAS_RES_Y         16
#define ATLAS_TILE_RES      256
#define ATLAS_TILE_RES_LOG2 8
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float3    Randomize;      // Randomize
uniform float4    faceParams;     // (x0,             y0,             size,                   face)
uniform float4    scaleParams;    // (offsU,          offsV,          scale,                  tidalLock)
uniform float4    mainParams;     // (mainFreq,       terraceProb,    surfType,               snowLevel)
uniform float4    colorParams;    // (colorDistMagn,  colorDistFreq,  latIceCaps,             latTropic)
uniform float4    climateParams;  // (climatePole,    climateTropic,  climateEquator,         tropicWidth)
uniform float4    mareParams;     // (seaLevel,       mareFreq,       sqrt(mareDensity),      icecapHeight)
uniform float4    montesParams;   // (montesMagn,     montesFreq,     montesFraction,         montesSpiky)
uniform float4    dunesParams;    // (dunesMagn,      dunesFreq,      dunesDensity,           drivenDarkening)
uniform float4    hillsParams;    // (hillsMagn,      hillsFreq,      hillsDensity,           hills2Density)
uniform float4    canyonsParams;  // (canyonsMagn,    canyonsFreq,    canyonsFraction,        erosion)
uniform float4    riversParams;   // (riversMagn,     riversFreq,     riversSin,              riversOctaves)
uniform float4    cracksParams;   // (cracksMagn,     cracksFreq,     cracksOctaves,          craterRayedFactor)
uniform float4    craterParams;   // (craterMagn,     craterFreq,     sqrt(craterDensity),    craterOctaves)
uniform float4    volcanoParams1; // (volcanoMagn,    volcanoFreq,    sqrt(volcanoDensity),   volcanoOctaves)
uniform float4    volcanoParams2; // (volcanoActivity,volcanoFlows,   volcanoRadius,          volcanoTemp)
uniform float4    lavaParams;	  // (lavaCoverage,   lavaTemperature,surfTemperature,        heightTempGrad)
uniform float4    textureParams;  // (texScale,       texColorConv,   venusMagn,              venusFreq)
uniform float4    cloudsParams1;  // (cloudsFreq,     cloudsOctaves,  twistZones,             twistMagn)
uniform float4    cloudsParams2;  // (cloudsLayer,    cloudsNLayers,  cloudsStyle,            cloudsCoverage)
uniform float4    cycloneParams;  // (cycloneMagn,    cycloneFreq,    sqrt(cycloneDensity),   cycloneOctaves)
uniform float4	  radParams;	  // ()
uniform float4	  crHeightParams; // ()
uniform float4	  craterParams1;  // ()
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float2 TexCoord;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform sampler2D   NormalMap;          // normals map to calculate slope
uniform sampler2D   MaterialTable;      // material parameters table
uniform sampler1D   CloudsColorTable;   // clouds color table
uniform sampler2D   AtlasDiffSampler;   // detail texture diffuse atlas
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float noiseOctaves;// = 4;
uniform float noiseLacunarity;// = 2.218281828459;
uniform float noiseH;// = 0.5;
uniform float noiseOffset;// = 0.8;
uniform float noiseRidgeSmooth;// = 0.0001;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
const float pi = 3.14159265358;
const float pi2 = 6.28318531;
//-----------------------------------------------------------------------------

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
#define		radPeak				radParams.x
#define		radInner			radParams.y
#define		radRim				radParams.z
#define		radOuter			radParams.w
#define		heightFloor			crHeightParams.x
#define		heightPeak			crHeightParams.y
#define		heightRim			crHeightParams.z
#define		heightCrew			crHeightParams.w
#define		craterSphereRadius	craterParams1.x
#define		craterRoundDist		craterParams1.y
#define		craterDistortion	craterParams1.z
#define		craterRaysColor		craterParams1.w
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     saturate(x) clamp(x, 0.0, 1.0)
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     GetCloudsColor(height)           tex1D(CloudsColorTable, height)
#define     GetGasGiantCloudsColor(height)   tex2D(MaterialTable, float2(height, 0.0))
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float smin(float a, float b, float k)
{
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
	return lerp(b, a, h) - k * h * (1.0 - h);
}

float softExpMaxMin(float a, float b, float k)
{
	float res = exp(k * a) + exp(k * b);
	return log(res) / k;
}

float3 Rotate(float Angle, float3 Axis, float3 Vector)
{
	float cosa = cos(Angle);
	float sina = sin(Angle);

	float t = 1.0 - cosa;

	float3x3 M = float3x3
	(
		t * Axis.x * Axis.x + cosa,
		t * Axis.x * Axis.y - sina * Axis.z,
		t * Axis.x * Axis.z + sina * Axis.y,
		t * Axis.x * Axis.y + sina * Axis.z,
		t * Axis.y * Axis.y + cosa,
		t * Axis.y * Axis.z - sina * Axis.x,
		t * Axis.x * Axis.z - sina * Axis.y,
		t * Axis.y * Axis.z + sina * Axis.x,
		t * Axis.z * Axis.z + cosa
	);

	return mul(M, Vector);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 hash3(float2 p) { return frac(sin(float3(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)))) * 43758.5453); }
float4 hash4(float2 p) { return frac(sin(float4(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)), dot(p, float2(398.1, 176.7)))) * 43758.5453); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 SphericalToCartesian(float2 spherical)
{
	float2 Alpha = float2(sin(spherical.x), cos(spherical.x));
	float2 Delta = float2(sin(spherical.y), cos(spherical.y));
	return float3(Delta.y * Alpha.x, Delta.x, Delta.y * Alpha.y);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 GetSurfacePoint(float2 texcoord)
{
	float2 spherical = float2(0.0, 0.0);

	if (faceParams.w == 6.0) //global
	{
		spherical.x = (texcoord.x * 2 - 0.5) * pi;
		spherical.y = (0.5 - texcoord.y) * pi;

		float2 Alpha = float2(sin(spherical.x), cos(spherical.x));
		float2 Delta = float2(sin(spherical.y), cos(spherical.y));

		return float3(Delta.y * Alpha.x, Delta.x, Delta.y * Alpha.y);
	}
	else //cubemap
	{
		spherical = texcoord.xy * faceParams.z + faceParams.xy;

		float3 p = normalize(float3(spherical, 1.0));

		if (faceParams.w == 0.0)
			return float3(p.z, -p.y, -p.x); //neg_x
		else if (faceParams.w == 1.0)
			return float3(-p.z, -p.y, p.x); //pos_x
		else if (faceParams.w == 2.0)
			return float3(p.x, -p.z, -p.y); //neg_y
		else if (faceParams.w == 3.0)
			return float3(p.x, p.z, p.y); //pos_y
		else if (faceParams.w == 4.0)
			return float3(-p.x, -p.y, -p.z); //neg_z
		else
			return float3(p.x, -p.y, p.z); //pos_z
	}
}

float3 GetSurfacePoint()
{
	return GetSurfacePoint(TexCoord);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 rgb2hsl(float3 rgb)
{
	float Max = max(rgb.r, max(rgb.g, rgb.b));
	float Min = min(rgb.r, min(rgb.g, rgb.b));

	float3 hsl = float3(0.0, 0.0, 0.0);

	hsl.z = (Min + Max) * 0.5;

	if (hsl.z <= 0.0) return hsl;

	float delta = Max - Min;

	if (delta == 0.0)
	{
		hsl.x = 0.0; // undefined (gray color)
		hsl.y = 0.0;
	}
	else
	{
		if (hsl.z <= 0.5) hsl.y = delta / (Max + Min);
		else              hsl.y = delta / (2.0 - Max - Min);

		float3 rgb2 = (float3(Max, 0, 0) - rgb) / delta;

		if (rgb.r == Max) hsl.x = (Min == rgb.g) ? 5.0 + rgb2.b : 1.0 - rgb2.g;
		else if (rgb.g == Max) hsl.x = (Min == rgb.b) ? 1.0 + rgb2.r : 3.0 - rgb2.b;
		else                   hsl.x = (Min == rgb.r) ? 3.0 + rgb2.g : 5.0 - rgb2.r;

		hsl.x *= 1.0 / 6.0;
	}

	return hsl;
}

float3 hsl2rgb(float3 hsl)
{
	float3  rgb;
	float q = (hsl.z <= 0.5) ? (hsl.z * (1.0 + hsl.y)) : (hsl.z + hsl.y - hsl.z * hsl.y);

	if (q <= 0)
	{
		rgb = float3(hsl.z, 0, 0);
	}
	else
	{
		float p = 2.0 * hsl.z - q;
		float tr = 6.0 * frac(hsl.x + 1.0 / 3.0);
		float tg = 6.0 * frac(hsl.x);
		float tb = 6.0 * frac(hsl.x - 1.0 / 3.0);

		if (tr < 1.0) rgb.r = p + (q - p)*tr;
		else if (tr < 3.0) rgb.r = q;
		else if (tr < 4.0) rgb.r = p + (q - p)*(4.0 - tr);
		else               rgb.r = p;

		if (tg < 1.0) rgb.g = p + (q - p)*tg;
		else if (tg < 3.0) rgb.g = q;
		else if (tg < 4.0) rgb.g = p + (q - p)*(4.0 - tg);
		else               rgb.g = p;

		if (tb < 1.0) rgb.b = p + (q - p)*tb;
		else if (tb < 3.0) rgb.b = q;
		else if (tb < 4.0) rgb.b = p + (q - p)*(4.0 - tb);
		else               rgb.b = p;
	}

	return rgb;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 UnitToColor24(in float unit)
{
	float mask = 1.0 / 256.0;

	float3 factor = float3(1.0, 255.0, 65025.0);
	float3 color = unit * factor.rgb;

	color.gb = frac(color.gb);
	color.rg -= color.gb * mask;

	return clamp(color, 0.0, 1.0);
}

float ColorToUnit24(in float3 color)
{
	return dot(color, float3(1.0, 1.0 / 255.0, 1.0 / 65025.0));
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct  Surface
{
	float4 color;
	float  height;
};

Surface Blend(Surface s0, Surface s1, float t)
{
	Surface output;

	output.color = lerp(s0.color, s1.color, t);
	output.height = lerp(s0.height, s1.height, t);

	return output;
}

Surface BlendSmart(Surface s0, Surface s1, float t)
{
	float a0 = s0.height + 1.0 - t;
	float a1 = s1.height + t;
	float ma = max(a0, a1) - 0.5;
	float b0 = max(a0 - ma, 0.0);
	float b1 = max(a1 - ma, 0.0);

	ma = b0 + b1;

	Surface res;

	res.color = (s0.color  * b0 + s1.color  * b1) / ma;
	res.height = (s0.height * b0 + s1.height * b1) / ma;

	return res;
}

#ifndef PACKED_NORMALS
float GetSurfaceHeight(float2 texcoord)
{
	float2  texCoord = texcoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	return tex2Dlod(NormalMap, float4(texCoord, 0.0, 0.0)).a;
	#else
	return tex2D(NormalMap, texCoord).a;
	#endif
}

float GetSurfaceHeight()
{
	return GetSurfaceHeight(TexCoord);
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope, float2 texcoord, sampler2D normalMap)
{
	float2 texCoord = texcoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	float4 bumpData = tex2Dlod(normalMap, float4(texCoord, 0.0, 0.0));
	#else
	float4 bumpData = tex2D(normalMap, texCoord);
	#endif

	float3 norm = 2.0 * bumpData.xyz - 1.0;

	slope = clamp(1.0 - pow(norm.z, 6.0), 0.0, 1.0);
	height = bumpData.a;
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope, float2 texcoord)
{
	float2 texCoord = texcoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	float4 bumpData = tex2Dlod(NormalMap, float4(texCoord, 0.0, 0.0));
	#else
	float4 bumpData = tex2D(NormalMap, texCoord);
	#endif

	float3 norm = 2.0 * bumpData.xyz - 1.0;

	slope = clamp(1.0 - pow(norm.z, 6.0), 0.0, 1.0);
	height = bumpData.a;
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope)
{
	GetSurfaceHeightAndSlope(height, slope, TexCoord);
}

#else

float GetSurfaceHeight(float2 texcoord)
{
	float2 texCoord = texCoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	float4 bumpData = tex2Dlod(NormalMap, float4(texCoord, 0.0, 0.0));
	#else
	float4 bumpData = tex2D(NormalMap, texCoord);
	#endif

	return dot(bumpData.zw, float2(0.00390625, 1.0));
}

float GetSurfaceHeight()
{
	return GetSurfaceHeight(TexCoord);
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope, float2 texcoord, sampler2D normalMap)
{
	float2 texCoord = texcoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	float4 bumpData = tex2Dlod(normalMap, float4(texCoord, 0.0, 0.0));
	#else
	float4 bumpData = tex2D(normalMap, texCoord);
	#endif

	float2 norm = 2.0 * bumpData.xy - 1.0;

	slope = 1.0 - dot(norm.xy, norm.xy);
	slope = clamp(1.0 - pow(slope, 3.0), 0.0, 1.0);
	height = dot(bumpData.zw, float2(0.00390625, 1.0));
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope, float2 texcoord)
{
	float2 texCoord = texcoord.xy * scaleParams.z + scaleParams.xy;

	#ifdef USETEXLOD
	float4 bumpData = tex2Dlod(NormalMap, float4(texCoord, 0.0, 0.0));
	#else
	float4 bumpData = tex2D(NormalMap, texCoord);
	#endif

	float2 norm = 2.0 * bumpData.xy - 1.0;

	slope = 1.0 - dot(norm.xy, norm.xy);
	slope = clamp(1.0 - pow(slope, 3.0), 0.0, 1.0);
	height = dot(bumpData.zw, float2(0.00390625, 1.0));
}

void GetSurfaceHeightAndSlope(inout float height, inout float slope)
{
	GetSurfaceHeightAndSlope(height, slope, TexCoord);
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Texture atlas sampling function
// height, slope defines the tile based on MaterialTable texture
// vary sets one of 4 different tiles of the same material
Surface GetSurfaceColorAtlas(float height, float slope, float vary)
{
	float4 PackFactors = float4(1.0 / ATLAS_RES_X, 1.0 / ATLAS_RES_Y, ATLAS_TILE_RES, ATLAS_TILE_RES_LOG2);
	slope = saturate(slope * 0.5);

	float4  IdScale = tex2Dlod(MaterialTable, float4(height, slope + 0.5, 0, 0));
	int materialID = min(int(IdScale.x) + int(vary), int(ATLAS_RES_X * ATLAS_RES_Y - 1));
	float2  tileOffs = float2(materialID % ATLAS_RES_X, materialID / ATLAS_RES_X) * PackFactors.xy;

	Surface res;
	float2  tileUV = (TexCoord.xy * faceParams.z + faceParams.xy) * texScale * IdScale.y;
	//float2  dx = dFdx(tileUV * PackFactors.z);
	//float2  dy = dFdy(tileUV * PackFactors.z);
	float2  dx = 0.0;
	float2  dy = 0.0;
	//float lod = clamp(0.5 * log2(max(dot(dx, dx), dot(dy, dy))), 0.0, PackFactors.w);
	float lod = clamp(0.5, 0.0, PackFactors.w);
	float2  invSize = float2(pow(2.0, lod - PackFactors.w), 0.0) * PackFactors.xy;
	float4  uv = float4(tileOffs + frac(tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize, 0.0, 0.0);
	
	//#if   (TILING_FIX_MODE == 0)
	//	res.color = tex2Dlod(AtlasDiffSampler, uv);
	//#elif (TILING_FIX_MODE == 1)
	//	float4 uv2 = (tileOffs + frac(-0.173 * tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize, 0, 0);
	//	res.color = mix(tex2Dlod(AtlasDiffSampler, uv), tex2Dlod(AtlasDiffSampler, uv2), 0.5);
	//#endif

	res.color = tex2Dlod(AtlasDiffSampler, uv);
	res.height = res.color.a;

	float4 adjust = tex2Dlod(MaterialTable, float4(height, slope, 0.0, 0.0));
	adjust.xyz *= texColorConv;
	float3 hsl = rgb2hsl(res.color.rgb);
	hsl.x = frac(hsl.x + adjust.x);
	hsl.yz = clamp(hsl.yz + adjust.yz, 0.0, 1.0);
	res.color.rgb = hsl2rgb(hsl);

	res.color.a = adjust.a;

	return  res;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Planet surface color function (uses the texture atlas sampling function)
// height, slope defines the tile based on MaterialTable texture
// vary sets one of 4 different tiles of the same material
Surface GetSurfaceColor(float height, float slope, float vary)
{
	return GetSurfaceColorAtlas(height, slope, vary * 4.0);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
void FAST32_hash_3D(float3 gridcell, out float4 lowz_hash_0, out float4 lowz_hash_1, out float4 lowz_hash_2, out float4 highz_hash_0, out float4 highz_hash_1, out float4 highz_hash_2)
{
	//generates 3 random numbers for each of the 8 cell corners
	//gridcell is assumed to be an integer coordinate

	//TODO:these constants need tweaked to find the best possible noise.
	//probably requires some kind of brute force computational searching or something....
	const float2 OFFSET = float2(50.0, 161.0);
	const float DOMAIN = 69.0;
	const float3 SOMELARGEFLOATS = float3(635.298681, 682.357502, 668.926525);
	const float3 ZINC = float3(48.500388, 65.294118, 63.934599);

	//truncate the domain
	gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
	float3 gridcell_inc1 = step(gridcell, float3(DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5)) * (gridcell + 1.0);

	//calculate the noise
	float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;

	P *= P;
	P = P.xzxz * P.yyww;

	float3 lowz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz));
	float3 highz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz));

	lowz_hash_0 = frac(P * lowz_mod.xxxx);
	highz_hash_0 = frac(P * highz_mod.xxxx);
	lowz_hash_1 = frac(P * lowz_mod.yyyy);
	highz_hash_1 = frac(P * highz_mod.yyyy);
	lowz_hash_2 = frac(P * lowz_mod.zzzz);
	highz_hash_2 = frac(P * highz_mod.zzzz);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 mod(float3 x, float y) { return x - y * floor(x / y); }
float2 mod(float2 x, float y) { return x - y * floor(x / y); }

float3 Permutation(float3 x) { return mod((34.0 * x + 1.0) * x, 289.0); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float K = 0.142857142857;
float Ko = 0.428571428571;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 RND_M = float4(1.0, 1.0, 1.0, 1.0);
float3 OFFSET = float3(0.5, 0.5, 0.5);
float3 OFFSETOUT = float3(1.5, 1.5, 1.5);
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float2 iNoise(float3 P, float jitter)
{
	float3 Pi = mod(floor(P), 289.0);
	float3 Pf = frac(P);
	float3 oi = float3(-1.0, 0.0, 1.0);
	float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = Permutation(Pi.x + oi);
	float3 py = Permutation(Pi.y + oi);

	float3 p, ox, oy, oz, dx, dy, dz;
	float2 F = float2(1e6, 1e6);

	for (int i = 0; i < 3; i++)
	{
		for (int j = 0; j < 3; j++)
		{
			p = Permutation(px[i] + py[j] + Pi.z + oi); // pij1, pij2, pij3

			ox = frac(p * K) - Ko;
			oy = mod(floor(p * K), 7.0) * K - Ko;

			p = Permutation(p);

			oz = frac(p*K) - Ko;

			dx = Pf.x - of[i] + jitter*ox;
			dy = Pf.y - of[j] + jitter*oy;
			dz = Pf.z - of + jitter*oz;

			float3 d = dx * dx + dy * dy + dz * dz; // dij1, dij2 and dij3, squared

			//Find lowest and second lowest distances
			for (int n = 0; n < 3; n++)
			{
				if (d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if (d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}
	}

	return F;
}

float Noise(float3 p)
{
	float3 Pi = floor(p);
	float3 Pf = p - Pi;
	float3 Pf_min1 = Pf - 1.0;

	float hashOffset = 0.49999;

	float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;

	FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

	float4 grad_x0 = hashx0 - hashOffset;
	float4 grad_y0 = hashy0 - hashOffset;
	float4 grad_z0 = hashz0 - hashOffset;
	float4 grad_x1 = hashx1 - hashOffset;
	float4 grad_y1 = hashy1 - hashOffset;
	float4 grad_z1 = hashz1 - hashOffset;

	float4 grad_results_0 = 1 / sqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0);
	float4 grad_results_1 = 1 / sqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1);

	float3 blend = Interpolation_C2(Pf);

	float4 blend2 = float4(blend.xy, float2(1.0 - blend.xy));
	float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);

	float final = dot(res0, blend2.zxzx * blend2.wwyy) * 1.1547005383792515290182975610039;

	return final;
}

float4 NoiseDeriv(float3 p)
{
	//establish our grid cell and unit position
	float3 Pi = floor(p);
	float3 Pf = p - Pi;
	float3 Pf_min1 = Pf - 1.0;

	//calculate the hash.
	//(various hashing methods listed in order of speed)
	float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
	FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

	//calculate the gradients
	float4 grad_x0 = hashx0 - 0.49999;
	float4 grad_y0 = hashy0 - 0.49999;
	float4 grad_z0 = hashz0 - 0.49999;
	float4 grad_x1 = hashx1 - 0.49999;
	float4 grad_y1 = hashy1 - 0.49999;
	float4 grad_z1 = hashz1 - 0.49999;

	float4 norm_0 = 1 / sqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);
	float4 norm_1 = 1 / sqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);

	grad_x0 *= norm_0;
	grad_y0 *= norm_0;
	grad_z0 *= norm_0;
	grad_x1 *= norm_1;
	grad_y1 *= norm_1;
	grad_z1 *= norm_1;

	//calculate the dot products
	float4 dotval_0 = float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0;
	float4 dotval_1 = float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1;

	//NOTE:the following is based off Milo Yips derivation, but modified for parallel execution
	//http://stackoverflow.com/a/14141774

	//Convert our data to a more parallel format
	float4 dotval0_grad0 = float4(dotval_0.x, grad_x0.x, grad_y0.x, grad_z0.x);
	float4 dotval1_grad1 = float4(dotval_0.y, grad_x0.y, grad_y0.y, grad_z0.y);
	float4 dotval2_grad2 = float4(dotval_0.z, grad_x0.z, grad_y0.z, grad_z0.z);
	float4 dotval3_grad3 = float4(dotval_0.w, grad_x0.w, grad_y0.w, grad_z0.w);
	float4 dotval4_grad4 = float4(dotval_1.x, grad_x1.x, grad_y1.x, grad_z1.x);
	float4 dotval5_grad5 = float4(dotval_1.y, grad_x1.y, grad_y1.y, grad_z1.y);
	float4 dotval6_grad6 = float4(dotval_1.z, grad_x1.z, grad_y1.z, grad_z1.z);
	float4 dotval7_grad7 = float4(dotval_1.w, grad_x1.w, grad_y1.w, grad_z1.w);

	//evaluate common constants
	float4 k0_gk0 = dotval1_grad1 - dotval0_grad0;
	float4 k1_gk1 = dotval2_grad2 - dotval0_grad0;
	float4 k2_gk2 = dotval4_grad4 - dotval0_grad0;
	float4 k3_gk3 = dotval3_grad3 - dotval2_grad2 - k0_gk0;
	float4 k4_gk4 = dotval5_grad5 - dotval4_grad4 - k0_gk0;
	float4 k5_gk5 = dotval6_grad6 - dotval4_grad4 - k1_gk1;
	float4 k6_gk6 = (dotval7_grad7 - dotval6_grad6) - (dotval5_grad5 - dotval4_grad4) - k3_gk3;

	//C2 Interpolation
	float3 blend = Interpolation_C2(Pf);
	float3 blendDeriv = Interpolation_C2_Deriv(Pf);

	//calculate final noise + deriv
	float u = blend.x;
	float v = blend.y;
	float w = blend.z;

	float4 xxx = (u * (k0_gk0 + v * k3_gk3));
	float4 yyy = (v * (k1_gk1 + w * k5_gk5));
	float4 zzz = (w * (k2_gk2 + u * (k4_gk4 + v * k6_gk6)));

	float4 noiseresult = dotval0_grad0 + xxx + yyy + zzz;

	float4 result = float4(0.0, 0.0, 0.0, noiseresult.x); //ONLY noiseresult.x!!!!!!!!

	result.x += dot(float4(k0_gk0.x, k3_gk3.x * v, float2(k4_gk4.x, k6_gk6.x * v) * w), float4(blendDeriv.x, blendDeriv.x, blendDeriv.x, blendDeriv.x));
	result.y += dot(float4(k1_gk1.x, k3_gk3.x * u, float2(k5_gk5.x, k6_gk6.x * u) * w), float4(blendDeriv.y, blendDeriv.y, blendDeriv.y, blendDeriv.y));
	result.z += dot(float4(k2_gk2.x, k4_gk4.x * u, float2(k5_gk5.x, k6_gk6.x * u) * v), float4(blendDeriv.z, blendDeriv.z, blendDeriv.z, blendDeriv.z));

	//normalize and return
	return result *= 1.1547005383792515290182975610039;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 permute(float4 x) { return fmod((x * 34.0 + 1.0) * x, 289.0); }
float3 permute3(float3 x) { return fmod((x * 34.0 + 1.0) * x, 289.0); }
float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
const float3 vyd = float3(3.33, 5.71, 1.96);
const float3 vzd = float3(7.77, 2.65, 4.37);
const float3 vwd = float3(1.13, 2.73, 6.37);
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float2 NoiseVec2(float3 p) { return float2(Noise(p), Noise(p + vyd)); }
float3 NoiseVec3(float3 p) { return float3(Noise(p), Noise(p + vyd), Noise(p + vzd)); }
float4 NoiseVec4(float3 p) { return float4(Noise(p), Noise(p + vyd), Noise(p + vzd), Noise(p + vwd)); }
float NoiseU(float3 p) { return Noise(p) * 0.5 + 0.5; }
float3 NoiseUVec3(float3 p) { return NoiseVec3(p) * 0.5 + float3(0.5, 0.5, 0.5); }
float4 NoiseUVec4(float3 p) { return NoiseVec4(p) * 0.5 + float4(0.5, 0.5, 0.5, 0.5); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float DistNoise(float3 p, float d) { return Noise(p + NoiseVec3(p + 0.5) * d); }
float3 DistNoise3D(float3 p, float d) { return NoiseVec3(p + NoiseVec3(p + 0.5) * d); }
float4 DistNoise4D(float3 p, float d) { return NoiseVec4(p + NoiseVec3(p + 0.5) * d); }
float FiltNoise(float3 p, float w) { return Noise(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float3 FiltNoise3D(float3 p, float w) { return NoiseVec3(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float4 FiltNoise4D(float3 p, float w) { return NoiseVec4(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float FiltDistNoise(float3 p, float w, float d) { return DistNoise(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
float3 FiltDistNoise3D(float3 p, float w, float d) { return DistNoise3D(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
float4 FiltDistNoise4D(float3 p, float w, float d) { return DistNoise4D(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float SavePow(float f, float p) 
{ 
	#ifdef USESAVEPOW 
	return pow(abs(f), p);
	#else
	return pow(f, p);
	#endif
}

float Lacunarity() 
{ 
	#ifdef USESAVEPOW 
	return pow(abs(noiseLacunarity), -noiseH); 
	#else
	return pow(noiseLacunarity, -noiseH);
	#endif
}

float Frequency(float frequency)
{
	#ifdef USESAVEPOW 
	return pow(abs(frequency), -noiseH);
	#else
	return pow(frequency, -noiseH);
	#endif
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Fbm(float3 ppoint)
{
	float summ = 0.0;
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < noiseOctaves; ++i)
	{
		summ += Noise(ppoint) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float Fbm(float3 ppoint, float o)
{
	float summ = 0.0;
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < o; ++i)
	{
		summ += Noise(ppoint) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float FbmClouds(float3 ppoint)
{
	float summ = 0.0;
	float ampl = 1.0;

	for (int i = 0; i < cloudsOctaves; ++i)
	{
		summ += Noise(ppoint) * ampl;
		ampl *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float FbmClouds(float3 ppoint, float o)
{
	float summ = 0.0;
	float ampl = 1.0;

	for (int i = 0; i < o; ++i)
	{
		summ += Noise(ppoint) * ampl;
		ampl *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float3 Fbm3D(float3 ppoint)
{
	float3  summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < noiseOctaves; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float3 Fbm3D(float3 ppoint, float o)
{
	float3  summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < o; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float3 Fbm3DClouds(float3 ppoint)
{
	float3  summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;

	for (int i = 0; i < cloudsOctaves; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float3 Fbm3DClouds(float3 ppoint, float o)
{
	float3  summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;

	for (int i = 0; i < o; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float DistFbm(float3 ppoint, float dist)
{
	float summ = 0.0;
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < noiseOctaves; ++i)
	{
		summ += DistNoise(ppoint, dist) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float3 DistFbm3D(float3 ppoint, float dist)
{
	float3 summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < noiseOctaves; ++i)
	{
		summ += DistNoise3D(ppoint, dist) * ampl;
		ampl *= gain;
		ppoint *= noiseLacunarity;
	}

	return summ;
}

float RidgedMultifractal(float3 ppoint, float gain)
{
	float signal = 1.0;
	float summ = 0.0;
	float frequency = 1.0;
	float weight;

	for (int i = 0; i < noiseOctaves; ++i)
	{
		weight = saturate(signal * gain);
		signal = Noise(ppoint * frequency);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + signal*signal);
		signal *= signal * weight;
		summ += signal * SavePow(frequency, -noiseH);
		frequency *= noiseLacunarity;
	}

	return summ;
}

float RidgedMultifractalDetail(float3 ppoint, float gain, float firstOctaveValue)
{
	float signal = firstOctaveValue;
	float summ = firstOctaveValue;
	float frequency = noiseLacunarity;
	float weight;

	for (int i = 1; i < noiseOctaves; ++i)
	{
		weight = saturate(signal * gain);
		signal = Noise(ppoint * frequency);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + signal*signal);
		signal *= signal * weight;
		summ += signal * SavePow(frequency, -noiseH);
		frequency *= noiseLacunarity;
	}

	return summ;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Ridged multifractal with "procedural erosion" by Giliam de Carpentier
// http://www.decarpentier.nl/scape-procedural-extensions
float RidgedMultifractalEroded(float3 ppoint, float gain, float warp)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = 0.0;
	float signal = 1.0;
	float weight;
	float3  dsum = float3(0.0, 0.0, 0.0);
	float4  noiseDeriv;

	for (int i = 0; i < noiseOctaves; ++i)
	{
		noiseDeriv = NoiseDeriv((ppoint + warp * dsum) * frequency);
		weight = saturate(signal * gain);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w*noiseDeriv.w);
		signal *= signal * weight;
		amplitude = SavePow(abs(frequency), -noiseH);
		summ += signal * amplitude;
		frequency *= noiseLacunarity;
		dsum -= amplitude * noiseDeriv.xyz * noiseDeriv.w;
	}

	return summ;
}

// Ridged multifractal with "procedural erosion" by Giliam de Carpentier
// http://www.decarpentier.nl/scape-procedural-extensions
float RidgedMultifractalErodedDetail(float3 ppoint, float gain, float warp, float firstOctaveValue)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = firstOctaveValue;
	float signal = firstOctaveValue;
	float weight;
	float3  dsum = float3(0.0, 0.0, 0.0);
	float4  noiseDeriv;

	for (int i = 0; i < noiseOctaves; ++i)
	{
		noiseDeriv = NoiseDeriv((ppoint + warp * dsum) * frequency);
		weight = saturate(signal * gain);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w*noiseDeriv.w);
		signal *= signal * weight;
		amplitude = SavePow(frequency, -noiseH);
		summ += signal * amplitude;
		frequency *= noiseLacunarity;
		dsum -= amplitude * noiseDeriv.xyz * noiseDeriv.w;
	}

	return summ;
}

//-----------------------------------------------------------------------------
// "Jordan turbulence" function by Giliam de Carpentier
// http://www.decarpentier.nl/scape-procedural-extensions
float JordanTurbulence
(
	float3 ppoint,
	float gain0, float gain,
	float warp0, float warp,
	float damp0, float damp,
	float dampScale
)
{
	float4 noiseDeriv = NoiseDeriv(ppoint);
	float4 noiseDeriv2 = noiseDeriv * noiseDeriv.w;
	float summ = noiseDeriv2.w;
	float3 dsumWarp = warp0 * noiseDeriv2.xyz;
	float3 dsumDamp = damp0 * noiseDeriv2.xyz;

	float amp = gain0;
	float freq = noiseLacunarity;
	float dampedAmp = amp * gain;

	for (int i = 1; i < noiseOctaves; ++i)
	{
		noiseDeriv = NoiseDeriv(ppoint * freq + dsumWarp.xyz);
		noiseDeriv2 = noiseDeriv * noiseDeriv.w;
		summ += dampedAmp * noiseDeriv2.w;
		dsumWarp += warp * noiseDeriv2.xyz;
		dsumDamp += damp * noiseDeriv2.xyz;
		freq *= noiseLacunarity;
		amp *= gain;
		dampedAmp = amp * (1.0 - dampScale / (1.0 + dot(dsumDamp, dsumDamp)));
	}

	return summ;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Cell3NoiseF0(float3 p, int octaves, float lacunarity)
{
	float freq = 1.0, amp = 0.5;
	float sum = 0.0;
	float gain = SavePow(lacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = iNoise(p * freq, 1.0) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= lacunarity;
		amp *= gain;
	}

	return sum / 2.0;
}

float4 Cell3NoiseF0Vec(float3 p, int octaves, float lacunarity)
{
	float3 cell = floor(p);
	float freq = 1.0, amp = 0.5;
	float sum = 0.0;
	float gain = SavePow(lacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = iNoise(p * freq, 1.0) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= lacunarity;
		amp *= gain;
	}

	p = normalize(p + cell + OFFSETOUT);

	return float4(p, sum / 2.0);
}

float Cell3NoiseF1F0(float3 p, int octaves, float lacunarity)
{
	float freq = 1.0, amp = 0.5;
	float sum = 0.0;
	float gain = SavePow(lacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = iNoise(p * freq, 1.0) * amp;

		sum += 0.1 + sqrt(F[1]) - sqrt(F[0]);

		freq *= lacunarity;
		amp *= gain;
	}

	return sum / 2.0;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 NoiseRandomUVec3(float3 c)
{
	float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));

	float3 r;

	r.z = frac(512.0 * j);
	j *= 0.125;

	r.x = frac(512.0 * j);
	j *= 0.125;

	r.y = frac(512.0 * j);

	return r - 0.5;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Cell2Noise(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 rnd;
	float3 d;
	float dist;
	float distMin = 1.0e38;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;

				pos = rnd - offs;
				dist = dot(pos, pos);
				distMin = min(distMin, dist);
			}
		}
	}

	return sqrt(distMin);
}

float2 Cell2Noise2(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 rnd;
	float3 d;
	float dist;
	float distMin1 = 1.0e38;
	float distMin2 = 1.0e38;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (dist < distMin1)
				{
					distMin2 = distMin1;
					distMin1 = dist;
				}
				else
					distMin2 = min(distMin2, dist);
			}
		}
	}

	return sqrt(float2(distMin1, distMin2));
}

float4 Cell2NoiseVec(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 ppoint = float3(0.0, 0.0, 0.0);
	float3 rnd;
	float3 d;
	float distMin = 1.0e38;
	float dist;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (distMin > dist)
				{
					distMin = dist;
					ppoint = rnd;
				}
			}
		}
	}

	ppoint = normalize(ppoint + cell + OFFSETOUT);

	return float4(ppoint, sqrt(distMin));
}

float Cell2NoiseColor(float3 p, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float4 rndM = RND_M;
	float4 rnd;
	float3 d;
	float distMin = 1.0e38;
	float dist;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = float4(NoiseRandomUVec3((cell + d)), 0.0);
				pos = rnd.xyz + d - offs;
				dist = dot(pos, pos);

				if (distMin > dist)
				{
					distMin = dist;
					rndM = rnd;
				}
			}
		}
	}

	color = rndM;

	return sqrt(distMin);
}

float4 Cell2NoiseSphere(float3 p, float Radius)
{
	p *= Radius;

	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 ppoint = float3(0.0, 0.0, 0.0);
	float3 rnd;
	float3 d;
	float distMin = 1.0e38;
	float dist;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (distMin > dist)
				{
					distMin = dist;
					ppoint = rnd;
				}
			}
		}
	}

	ppoint = normalize(ppoint + cell + OFFSETOUT);

	return float4(ppoint, length(ppoint * Radius - p));
}

void Cell2Noise2Sphere(float3 p, float Radius, out float4 point1, out float4 point2)
{
	p *= Radius;

	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 rnd;
	float3 d;
	float distMin1 = 1.0e38;
	float distMin2 = 1.0e38;
	float dist;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (dist < distMin1)
				{
					distMin2 = distMin1;
					distMin1 = dist;
					point1.xyz = rnd;
				}
				else if (dist < distMin2)
				{
					distMin2 = dist;
					point2.xyz = rnd;
				}
			}
		}
	}

	point1.xyz = normalize(point1.xyz + cell + OFFSETOUT);
	point1.w = distMin1;

	point2.xyz = normalize(point2.xyz + cell + OFFSETOUT);
	point2.w = distMin2;
}

float4 Cell2NoiseVecSphere(float3 p, float Radius)
{
	p *= Radius;

	float3 cell = floor(p);
	float3 offs = p - cell - OFFSET;
	float3 pos;
	float3 ppoint = float3(0.0, 0.0, 0.0);
	float3 rnd;
	float3 d;
	float distMin = 1.0e38;
	float dist;

	for (d.z = -1.0; d.z <= 1.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 1.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 1.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (distMin > dist)
				{
					distMin = dist;
					ppoint = rnd;
				}
			}
		}
	}

	ppoint = normalize(ppoint + cell + OFFSETOUT);

	return float4(ppoint, length(ppoint * Radius - p));
}

float Cell3Noise(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float3 rnd;
	float3 d;
	float dist;
	float distMin = 1.0e38;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);
				distMin = min(distMin, dist);
			}
		}
	}

	return sqrt(distMin);
}

float Cell3NoiseSmooth(float3 p, float falloff)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float4 rnd;
	float3 d;
	float dist;
	float res = 0.0;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = float4(NoiseRandomUVec3((cell + d)), 0.0);
				pos = rnd.xyz + d - offs;
				dist = dot(pos, pos);
				res += SavePow(dist, -falloff);
			}
		}
	}

	return SavePow(res, -0.5 / falloff);
}

float2 Cell3Noise2(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float3 rnd;
	float3 d;
	float dist;
	float distMin1 = 1.0e38;
	float distMin2 = 1.0e38;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (dist < distMin1)
				{
					distMin2 = distMin1;
					distMin1 = dist;
				}
				else
					distMin2 = min(distMin2, dist);
			}
		}
	}

	return sqrt(float2(distMin1, distMin2));
}

float4 Cell3NoiseVec(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float3 ppoint = float3(0.0, 0.0, 0.0);
	float3 rnd;
	float3 d;
	float dist;
	float distMin = 1.0e38;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = NoiseRandomUVec3((cell + d)).xyz + d;
				pos = rnd - offs;
				dist = dot(pos, pos);

				if (distMin > dist)
				{
					distMin = dist;
					ppoint = rnd;
				}
			}
		}
	}

	ppoint = normalize(ppoint + cell + OFFSETOUT);

	return float4(ppoint, sqrt(distMin));
}

float Cell3NoiseColor(float3 p, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float4 rnd;
	float4 rndM = RND_M;
	float3 d;
	float dist;
	float distMin = 1.0e38;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = float4(NoiseRandomUVec3((cell + d)), 0.0);
				pos = rnd.xyz + d - offs;
				dist = dot(pos, pos);

				if (dist < distMin)
				{
					distMin = dist;
					rndM = rnd;
				}
			}
		}
	}

	color = rndM;

	return sqrt(distMin);
}

float2 Cell3Noise2Color(float3 p, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float4 rnd;
	float4 rndM = RND_M;
	float3 d;
	float dist;
	float distMin1 = 1.0e38;
	float distMin2 = 1.0e38;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = float4(NoiseRandomUVec3((cell + d)), 0.0);
				pos = rnd.xyz + d - offs;
				dist = dot(pos, pos);

				if (dist < distMin1)
				{
					distMin2 = distMin1;
					distMin1 = dist;
					rndM = rnd;
				}
				else
					distMin2 = min(distMin2, dist);
			}
		}
	}

	color = rndM;

	return sqrt(float2(distMin1, distMin2));
}

float Cell3NoiseSmoothColor(float3 p, float falloff, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float4 rnd;
	float4 rndM = RND_M;
	float3 d;
	float dist;
	float distMin = 1.0e38;
	float res = 0.0;

	for (d.z = -1.0; d.z <= 2.0; d.z += 1.0)
	{
		for (d.y = -1.0; d.y <= 2.0; d.y += 1.0)
		{
			for (d.x = -1.0; d.x <= 2.0; d.x += 1.0)
			{
				rnd = float4(NoiseRandomUVec3((cell + d)), 0.0);
				pos = rnd.xyz + d - offs;
				dist = dot(pos, pos);

				if (dist < distMin)
				{
					distMin = dist;
					rndM = rnd;
				}

				res += SavePow(dist, -falloff);
			}
		}
	}

	res = SavePow(res, -0.5 / falloff);
	color = rndM;

	return res;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float CraterHeightFunc(float lastlastLand, float lastLand, float height, float r)
{
	float t;

	if (r < radPeak)
	{   // central peak
		t = 1.0 - r / radPeak;
		return lastlastLand + height * heightFloor + heightPeak * craterDistortion * smoothstep(0.0, 1.0, t);
	}
	else if (r < radInner)
	{   // crater bottom
		return lastlastLand + height * heightFloor;
	}
	else if (r < radRim)
	{   // inner rim
		t = (r - radInner) / (radRim - radInner);
		return lerp(lastlastLand + height * heightFloor, lastLand + height * heightRim * craterDistortion, t * t * t);
	}
	else if (r < radOuter)
	{   // outer rim
		t = 1.0 - (r - radRim) / (radOuter - radRim);
		return lastLand + height * craterDistortion * lerp(0.1, heightRim, t * t);
	}
	else if (r < 1.0)
	{   // outer area
		t = 1.0 - (r - radOuter) / (1.0 - radOuter);
		return lastLand + 0.1 * height * craterDistortion * t;
	}
	else
		return lastLand;
}

float CraterNoise(float3 ppoint, float cratMagn, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	ppoint = (ppoint * cratFreq + Randomize) * cratSqrtDensity;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float lastlastlastLand = 0.0;
	float amplitude = 1.0;
	float cell;
	float radFactor = 1.0 / cratSqrtDensity;

	radPeak = 0.02;
	radInner = 0.15;
	radRim = 0.20;
	radOuter = 0.40;

	for (int i = 0; i < cratOctaves; i++)
	{
		cell = Cell3Noise(ppoint + craterRoundDist * Fbm3D(ppoint * 2.56));

		lastlastlastLand = lastlastLand;
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell * radFactor);
		ppoint *= 1.81818182;
		amplitude *= 0.55;
		heightPeak *= 0.25;
		heightFloor *= 1.2;
		radInner *= 0.60;
	}

	return cratMagn * newLand;
}

float RayedCraterColorFunc(float r, float fi, float rnd)
{
	if (r < radOuter)
	{
		float t = 1.0 - (r - radRim) / (radOuter - radRim);

		float d = SavePow(NoiseU(float3(70.3 * fi, rnd, rnd)), 4);

		return sqrt(t) * saturate(SavePow(d, 4) + 1.0 - smoothstep(d, d + 0.75, r));
	}
	else
		return 0.0;
}

float RayedCraterNoise(float3 ppoint, float cratMagn, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	craterSphereRadius = cratFreq * cratSqrtDensity;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float lastlastlastLand = 0.0;
	float amplitude = 1.0;
	float cell;
	float radFactor = 1.0 / cratSqrtDensity;

	radPeak = 0.002;
	radInner = 0.015;
	radRim = 0.020;
	radOuter = 0.040;

	for (int i = 0; i < cratOctaves; i++)
	{
		cell = Cell2NoiseSphere(ppoint, craterSphereRadius).w;

		lastlastlastLand = lastlastLand;
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell * radFactor);

		craterSphereRadius *= 1.81818182;
		amplitude *= 0.55;
		heightPeak *= 0.25;
		heightFloor *= 1.2;
		radInner *= 0.60;
	}

	return  cratMagn * newLand;
}

float RayedCraterColorNoise(float3 ppoint, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));

	craterSphereRadius = cratFreq * cratSqrtDensity;

	float color = 0.0;
	float fi;
	float4 cell;
	float radFactor = 1.0 / cratSqrtDensity;

	radPeak = 0.002;
	radInner = 0.015;
	radRim = 0.030;
	radOuter = 0.800;

	for (int i = 0; i < cratOctaves; i++)
	{
		cell = Cell2NoiseVec(ppoint * craterSphereRadius);
		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		color += RayedCraterColorFunc(cell.w * radFactor, fi, 48.3 * dot(cell.xyz, Randomize));
		craterSphereRadius *= 1.81818182;
		radInner *= 0.60;
	}

	return color;
}

float VolcanoHeightFunc(float r, float fi, float rnd, float dist, float depth)
{
	float t, d;
	float cone = SavePow(smoothstep(0.0, 0.9, 1.0 - 2.0 * r), 3);

	t = (r - radInner) / (radRim - radInner);
	float caldera = clamp(1.0 - t * t * t, 0.0, 0.9);

	t = saturate(1.0 - (r - radRim) / (radOuter - radRim));
	d = SavePow(NoiseU(float3(22.3 * fi, rnd, rnd)) - 0.25 * t * t, volcanoFlows);
	float flows = (1.0 - smoothstep(d, d + 0.05, 0.5 * r)) * clamp(t, 0.0, 0.9);

	float lava = softExpMaxMin(caldera, flows, 32);
	return cone - depth * lava + dist * SavePow(1.0 - lava, 4) * saturate((0.5 - r) * 20.0);
}

float VolcanoGlowFunc(float r, float fi, float rnd)
{
	float t, d;

	t = (r - radInner) / (radRim - radInner);
	float caldera = clamp(1.0 - t * t * t, 0.0, 1.0);

	t = saturate(1.0 - (r - radRim) / (radOuter - radRim));
	d = SavePow(NoiseU(float3(22.3 * fi, rnd, rnd)) - 0.25 * t * t, volcanoFlows);
	float flows = (1.0 - smoothstep(d, d + 0.05, 0.5 * r));

	return max(caldera, flows);
}

float VolcanoNoise(float3 ppoint, float globalLand, float localLand)
{
	ppoint += craterRoundDist * Fbm3D(ppoint * 183.61);
	craterSphereRadius = volcanoFreq * volcanoSqrtDensity;

	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));
	float newLand = localLand;
	float amplitude = volcanoMagn;
	float radFactor = 2.0 / (volcanoSqrtDensity * volcanoRadius);
	float volcano, dist, fi, r;
	float shape = 0.7; // 1.0 - shield volcano, 0.5 - conic volcano
	float4 cell;

	radInner = 0.02;
	radRim = 0.03;
	radOuter = 0.80;

	//dist = 0.08 * JordanTurbulence(ppoint * 2400.0 + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
	//dist = 0.01 * DistFbm(ppoint * montesFreq * 23.8 + Randomize, 0.35);
	dist = 0.005 * Fbm(ppoint * montesFreq * 23.8 + Randomize);

	for (int i = 0; i < volcanoOctaves; i++)
	{
		cell = Cell2NoiseVecSphere(ppoint, craterSphereRadius);
		//cell = Cell3NoiseVec(ppoint + craterSphereRadius);

		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		r = SavePow(cell.w * radFactor, shape);
		volcano = globalLand - 1.0 + 2.0 * amplitude * VolcanoHeightFunc(r, fi, 48.3 * dot(cell.xyz, Randomize), dist, 0.1 * shape);
		newLand = softExpMaxMin(newLand, volcano, 32);

		craterSphereRadius *= 0.57;
		//craterSphereRadius *= 1.83;
		//amplitude *= 0.60;
		//radInner  *= 0.60;
		//radRim    *= 0.60;
		//radOuter  *= 0.60;
		shape = max(shape * 0.5, 0.5);
	}

	return newLand;
}

float VolcanoNoise(float3 ppoint, float globalLand, float localLand, float volcfreq, float vdens, float vradi, float volcocta)
{
	ppoint += craterRoundDist * Fbm3D(ppoint * 183.61);
	craterSphereRadius = volcfreq * vdens;

	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));
	float newLand = localLand;
	float amplitude = volcanoMagn;
	float radFactor = 2.0 / (vdens * vradi);
	float volcano, dist, fi, r;
	float shape = 0.7; // 1.0 - shield volcano, 0.5 - conic volcano
	float4 cell;

	radInner = 0.02;
	radRim = 0.03;
	radOuter = 0.80;

	//dist = 0.08 * JordanTurbulence(ppoint * 2400.0 + Randomize, 0.8, 0.5, 0.6, 0.35, 1.0, 0.8, 1.0);
	//dist = 0.01 * DistFbm(ppoint * montesFreq * 23.8 + Randomize, 0.35);
	dist = 0.005 * Fbm(ppoint * montesFreq * 23.8 + Randomize);

	for (int i = 0; i < volcocta; i++)
	{
		cell = Cell2NoiseVecSphere(ppoint, craterSphereRadius);
		//cell = Cell3NoiseVec(ppoint + craterSphereRadius);

		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		r = SavePow(cell.w * radFactor, shape);
		volcano = globalLand - 1.0 + 2.0 * amplitude * VolcanoHeightFunc(r, fi, 48.3 * dot(cell.xyz, Randomize), dist, 0.1 * shape);
		newLand = softExpMaxMin(newLand, volcano, 32);

		craterSphereRadius *= 0.57;
		//craterSphereRadius *= 1.83;
		//amplitude *= 0.60;
		//radInner  *= 0.60;
		//radRim    *= 0.60;
		//radOuter  *= 0.60;
		shape = max(shape * 0.5, 0.5);
	}

	return newLand;
}

float VolcanoGlowNoise(float3 ppoint)
{
	ppoint += craterRoundDist * Fbm3D(ppoint * 183.61);
	craterSphereRadius = volcanoFreq * volcanoSqrtDensity;

	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));
	float lavaTemp = 0.0;
	float radFactor = 2.0 / (volcanoSqrtDensity * volcanoRadius);
	float fi, r;
	float shape = 0.7; // 1.0 - shield volcano, 0.5 - conic volcano
	float4 cell;

	radInner = 0.02;
	radRim = 0.03;
	radOuter = 0.80;

	for (int i = 0; i < volcanoOctaves; i++)
	{
		cell = Cell2NoiseVecSphere(ppoint, craterSphereRadius);
		//cell = Cell3NoiseVec(ppoint + craterSphereRadius);

		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		r = SavePow(cell.w * radFactor, shape);
		lavaTemp = max(lavaTemp, VolcanoGlowFunc(r, fi, 48.3 * dot(cell.xyz, Randomize)));

		craterSphereRadius *= 0.57;
		//craterSphereRadius *= 1.83;
		//radInner  *= 0.60;
		//radRim    *= 0.60;
		//radOuter  *= 0.60;
		shape = max(shape * 0.5, 0.5);
	}

	return lavaTemp;
}

float MareHeightFunc(float lastLand, float lastlastLand, float height, float r, out float mareFloor)
{
	float t;

	if (r < radInner)
	{   // crater bottom
		mareFloor = 1.0;
		return lastlastLand + height * heightFloor;
	}
	else if (r < radRim)
	{   // inner rim
		t = (r - radInner) / (radRim - radInner);
		t = smoothstep(0.0, 1.0, t);
		mareFloor = 1.0 - t;
		return lerp(lastlastLand + height * heightFloor, lastLand + height * heightRim * craterDistortion, t);
	}
	else if (r < radOuter)
	{   // outer rim
		t = 1.0 - (r - radRim) / (radOuter - radRim);
		mareFloor = 0.0;
		return lerp(lastLand, lastLand + height * heightRim * craterDistortion, smoothstep(0.0, 1.0, t * t));
	}
	else
	{
		mareFloor = 0.0;
		return lastLand;
	}
}

float MareNoise(float3 ppoint, float globalLand, float bottomLand, out float mareFloor)
{
	ppoint = (ppoint * mareFreq + Randomize) * mareSqrtDensity;

	float amplitude = 0.7;
	float newLand = globalLand;
	float lastLand;
	float cell;
	float radFactor = 1.0 / mareSqrtDensity;

	radPeak = 0.0;
	radInner = 0.5;
	radRim = 0.6;
	radOuter = 1.0;
	heightFloor = 0.0;
	heightRim = 0.2;

	for (int i = 0; i < 3; i++)
	{
		cell = Cell2Noise(ppoint + 0.07 * Fbm3D(ppoint));
		lastLand = newLand;
		newLand = MareHeightFunc(lastLand, bottomLand, amplitude, cell * radFactor, mareFloor);
		ppoint = ppoint * 1.3 + Randomize;
		amplitude *= 0.62;
		radFactor *= 1.2;
	}

	mareFloor = 1.0 - mareFloor;

	return newLand;
}

float CrackHeightFunc(float lastLand, float lastlastLand, float height, float r)
{
	float t;

	if (r < 0.5)
	{   // inner rim
		t = 2.0 * r;
		return height - (1.5 * height - lastLand) * smoothstep(0.0, 1.0, 1.0 - t);
	}
	else
	{   // outer rim
		t = 2.0 * (r - 0.5);
		return height - (height - lastlastLand) * smoothstep(0.0, 1.0, t);
	}
}

float CrackColorFunc(float r)
{
	float t;

	if (r < 0.5)
	{   // inner rim
		t = 2.0 * r;
		return smoothstep(0.0, 1.0, 1.0 - t);
	}
	else
	{   // outer rim
		t = 2.0 * (r - 0.5);
		return smoothstep(0.0, 1.0, t);
	}
}

float CrackNoise(float3 ppoint, float distrort)
{
	ppoint = (ppoint + Randomize) * cracksFreq;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float2 cell;
	float dist;

	for (int i = 0; i < cracksOctaves; i++)
	{
		cell = Cell2Noise2(ppoint + 0.02 * Fbm3D(1.8 * ppoint));
		dist = smoothstep(0.0, 1.0, 250.0 * abs(cell.y - cell.x));
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CrackHeightFunc(lastlastLand, lastLand, distrort, dist);
		ppoint = ppoint * 1.2 + Randomize;
		distrort /= 1.2;
	}

	return newLand;
}

float CrackNoise(float3 ppoint, float distrort, float freq, float mod)
{
	ppoint = (ppoint + Randomize) * freq;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float2 cell;
	float dist;

	for (int i = 0; i < cracksOctaves; i++)
	{
		cell = Cell2Noise2(ppoint + 0.02 * Fbm3D(1.8 * ppoint));
		dist = smoothstep(0.0, 1.0, 250.0 * abs(cell.y - cell.x));
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CrackHeightFunc(lastlastLand, lastLand, distrort, dist);
		ppoint = ppoint * 1.2 + Randomize;
		distrort /= 1.2;
	}

	return newLand * mod;
}

float CrackColorNoise(float3 ppoint)
{
	ppoint = (ppoint + Randomize) * cracksFreq;

	float color = 0.0;
	float dist;
	float2 cell;

	for (int i = 0; i < cracksOctaves; i++)
	{
		cell = Cell2Noise2(ppoint + 0.02 * Fbm3D(1.8 * ppoint));
		dist = smoothstep(0.0, 1.0, 250.0 * abs(cell.y - cell.x));
		color += 1.0 - CrackColorFunc(dist);
		ppoint = ppoint * 1.2 + Randomize;
	}

	return SavePow(saturate(1.0 - color), 2.0);
}

float DunesNoise(float3 ppoint, float octaves)
{
	float dir = Noise(ppoint * 3.86) * 197.3 * dunesFreq;
	float3 p = ppoint;

	float glob = saturate(Fbm(p * 7.21) + 0.3);
	float dwin = 1.0 / (octaves - 1.0);
	float win = 0.0;

	float wave, fade, dist;
	float dunes = 0.0;
	float ampl = 0.05;
	float lac = 1.17;

	for (int i = 0; i < octaves; i++)
	{
		dist = dir + Noise(p * dunesFreq * 100.0) * 1.7;
		wave = frac(dist / 3.1415926);
		wave = cos(3.1415926 * wave * wave);
		fade = smoothstep(win - 0.5 * dwin, win, glob) * (1.0 - smoothstep(win + dwin, win + 1.5 * dwin, glob));
		dunes += (1.0 - sqrt(wave * wave + 0.005)) * ampl * fade;
		p = p * lac + float3(3.17, 5.38, 8.79);
		dir *= lac;
		ampl /= lac;
		win += dwin;
	}

	return dunes;
}

float DunesNoise(float3 ppoint, float octaves, float freq, float mod)
{
	float dir = Noise(ppoint * 3.86) * 197.3 * freq;
	float3 p = ppoint;

	float glob = saturate(Fbm(p * 7.21) + 0.3);
	float dwin = 1.0 / (octaves - 1.0);
	float win = 0.0;

	float wave, fade, dist;
	float dunes = 0.0;
	float ampl = 0.05;
	float lac = 1.17;

	for (int i = 0; i < octaves; i++)
	{
		dist = dir + Noise(p * freq * 100.0) * 1.7;
		wave = frac(dist / 3.1415926);
		wave = cos(3.1415926 * wave * wave);
		fade = smoothstep(win - 0.5 * dwin, win, glob) * (1.0 - smoothstep(win + dwin, win + 1.5 * dwin, glob));
		dunes += (1.0 - sqrt(wave * wave + 0.005)) * ampl * fade;
		p = p * lac + float3(3.17, 5.38, 8.79);
		dir *= lac;
		ampl /= lac;
		win += dwin;
	}

	return dunes * mod;
}

void SolarSpotsHeightNoise(float3 ppoint, out float botMask, out float filMask, out float filaments)
{
	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));

	craterSphereRadius = mareFreq * mareSqrtDensity;

	botMask = 1.0;
	filMask = 1.0;
	filaments = 0.0;

	float filam, botmask, filmask;
	float fi, rnd, t;
	float4 cell;
	float radFactor = 2.0 / mareSqrtDensity;

	radInner = 0.4;
	radOuter = 1.0;

	float3 dist = 0.01 * Fbm3D(ppoint * 7.6);
	ppoint += dist * 0.5;

	//for (int i=0; i<3; i++)
	{
		cell = Cell2NoiseSphere(ppoint, craterSphereRadius);
		//cell = Cell2NoiseVec(ppoint * craterSphereRadius);
		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		rnd = 48.3 * dot(cell.xyz, Randomize);

		t = saturate((cell.w * radFactor - radInner) / (radOuter - radInner));
		botmask = smoothstep(0.0, 1.0, t);
		filmask = smoothstep(0.0, 0.1, t) * (1.0 - botmask);
		filam = NoiseU(float3(530.7 * fi, rnd, rnd));

		filaments += filam;
		filMask *= filmask;
		botMask *= botmask;

		craterSphereRadius *= 1.83;
		radInner *= 0.60;
		radOuter = 0.60;
	}
}

void SolarSpotsTempNoise(float3 ppoint, out float botMask, out float filMask, out float filaments)
{
	float3 binormal = normalize(cross(ppoint, float3(0.0, 1.0, 0.0)));
	craterSphereRadius = mareFreq * mareSqrtDensity;

	botMask = 1.0;
	filMask = 1.0;
	filaments = 0.0;

	float filam, botmask, filmask;
	float fi, rnd, t;
	float4 cell;
	float radFactor = 2.0 / mareSqrtDensity;

	radInner = 0.4;
	radOuter = 1.0;

	float3 dist = 0.01 * Fbm3D(ppoint * 7.6);
	ppoint += dist * 0.5;

	//for (int i=0; i<3; i++)
	//{
		cell = Cell2NoiseSphere(ppoint, craterSphereRadius);
		//cell = Cell2NoiseVec(ppoint * craterSphereRadius);
		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi * 2.0);
		rnd = 48.3 * dot(cell.xyz, Randomize);

		t = saturate((cell.w * radFactor - radInner) / (radOuter - radInner));
		botmask = smoothstep(0.9, 1.0, t);
		filmask = smoothstep(0.0, 0.1, t) * (1.0 - botmask);
		filam = NoiseU(float3(530.7 * fi, rnd, rnd)) * (1.0 - 0.5 * t);

		filaments += filam;
		filMask *= filmask;
		botMask *= botmask;

		craterSphereRadius *= 1.83;
		radInner *= 0.60;
		radOuter = 0.60;
	//}
}
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
//TODO Fix dat.
float HeightMapClouds(float3 ppoint)
{
	float zone = cos(ppoint.y * twistZones);
	float ang = zone * twistMagn;
	float3 twistedPoint = ppoint;
	float coverage = cloudsCoverage;
	float weight = 1.0;

	// Compute the cyclons
	float3 cycloneCenter = (tidalLock > 0.0) ? float3(0.0, 1.0, 0.0) : Cell3NoiseVec((ppoint + Randomize) * cycloneFreq).xyz;
	float cycloneRadius = length(cycloneCenter - ppoint) / cycloneSqrtDensity;
	float cycloneAmpl = -tidalLock * cycloneMagn * sign(cycloneCenter.y);

	if (cycloneRadius < 1.0)
	{
		float dist = 1.0 - cycloneRadius;
		float fi = lerp(log(cycloneRadius), SavePow(dist, 3.0), cycloneRadius);
		twistedPoint = Rotate(cycloneAmpl * fi, cycloneCenter, ppoint);
		weight = saturate(1.0 - cycloneRadius / 0.05);
		weight = (1.0 - weight * weight) * (1.0 + dist);
		coverage = lerp(coverage, 1.0, dist);
	}

	// Compute the Coriolis effect
	float sina = sin(ang);
	float cosa = cos(ang);

	twistedPoint = float3(cosa * twistedPoint.x - sina * twistedPoint.z, twistedPoint.y, sina * twistedPoint.x + cosa * twistedPoint.z);
	twistedPoint = twistedPoint * cloudsFreq + Randomize;

	// Compute the flow-like distortion
	float3 p = twistedPoint * cloudsFreq * 6.37;
	float3 q = p + Fbm3DClouds(p);
	float3 r = p + Fbm3DClouds(q);
	float f = FbmClouds(r) * 0.7 + coverage - 0.3;
	float global = saturate(f) * weight;

	// Compute turbilence features
	//noiseOctaves = cloudsOctaves;
	//float turbulence = (Fbm(point * 100.0 * cloudsFreq + Randomize) + 1.5);// * smoothstep(0.0, 0.05, global);

	return global;
}
//-----------------------------------------------------------------------------