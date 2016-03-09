//-----------------------------------------------------------------------------
// noise engine:
// NOISE_ENGINE_SE - space engine texture lookup
// NOISE_ENGINE_ZNE - zameran noise engine
// NOISE_ENGINE_I - space engine
//#define NOISE_ENGINE_SE
//#define NOISE_ENGINE_ZNE
#define NOISE_ENGINE_I
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// noise engine technique: (Works only with NOISE_ENGINE_I)
// 0 - classic noise - OK.
// 1 - Ken Perlin's "improved" - OK.
// 2 - fast "improved" - OK.
#define NOISE_ENGINE_TECHNIQUE 2
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// noise engine technique: (Works only with NOISE_ENGINE_I)
// 0 - space engine.
// 1 - own.
#define COLORING_TECHNIQUE 1
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// tile blending method:
// 0 - hard mix (no blending)
// 1 - soft blending
// 2 - "smart" blening (tile heightmap based)
#define TILE_BLEND_MODE 2
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// tiling fix method:
// 0 - no tiling fix
// 1 - sampling texture 2 times at different scales
// 2 - voronoi random offset
// 3 - voronoi random offset and rotation
#define TILING_FIX_MODE 3
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// color space to use:
// 0 - hsl with adjusting.
// 1 - rgb with adjusting.
// 2 - default with adjusting.
// 3 - default without adjusting.
#define COLOR_SPACE 2
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define ATLAS_RES_X         8
#define ATLAS_RES_Y         16
#define ATLAS_TILE_RES      256
#define ATLAS_TILE_RES_LOG2 8
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define USESAVEPOW
#define USETEXLOD
#define PACKED_NORMALS
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
uniform float4	  craterParams2;  // ()
uniform float4	  planetGlobalColor;	 // ()
uniform float	  texturingHeightOffset; // ()
uniform float	  texturingSlopeOffset;  // ()
uniform float2	  texturingUVAtlasOffset;// ()
uniform float2	  InvSize;				 // ()
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float2 TexCoord;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform sampler2D	PermSampler;
uniform sampler2D	PermGradSampler;
uniform sampler2D   NormalMap;          // normals map to calculate slope
uniform sampler1D   CloudsColorTable;   // clouds color table

uniform sampler2D   MaterialTable;      // material parameters table
//uniform RWTexture2D<float> MaterialTable;      // material parameters table
//uniform Texture2D<float> MaterialTable;      // material parameters table
//RWStructuredBuffer<float4> MaterialTableBuffer;

uniform sampler2D   AtlasDiffSampler;   // detail texture diffuse atlas
//uniform RWTexture2D<float> AtlasDiffSampler;   // detail texture diffuse atlas
//uniform Texture2D<float> AtlasDiffSampler;   // detail texture diffuse atlas
//RWStructuredBuffer<float4> AtlasDiffSamplerBuffer;

uniform sampler2D	ColorMap;
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
#define		craterAmplitudePerOctave	craterParams2.x;
#define		craterHeightPeakPerOctave	craterParams2.y;
#define		craterHeightFloorPerOctave	craterParams2.z;
#define		craterRadInnerPerOctave		craterParams2.w;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     saturate(x) clamp(x, 0.0, 1.0)
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     GetCloudsColor(height)           tex1D(CloudsColorTable, height)
#define     GetGasGiantCloudsColor(height)   tex2D(MaterialTable, float2(height, 0.0))
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
float2 dFdx(float2 p)
{
	return float2(p.x * p.x - p.y, p.y);
}

float2 dFdy(float2 p)
{
	return float2(p.y * p.y - p.x, p.y);
}

float2 Fwidth(float2 texCoord, float2 size)
{
	float2 pixel_step = float2(size.x, size.y);       

	float2 current = dFdx(texCoord);

	float2 dfdx = dFdx(texCoord + pixel_step.x) - current;
	float2 dfdy = dFdx(texCoord + pixel_step.y) - current;

	float2 fw = abs(dfdx) + abs(dfdy);

	return fw;
}

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

float3 hash3(float2 p) { return frac(sin(float3(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)))) * 43758.5453); }
float4 hash4(float2 p) { return frac(sin(float4(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)), dot(p, float2(398.1, 176.7)))) * 43758.5453); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline Surface Blend(Surface s0, Surface s1, float t)
{
	Surface output;

	output.color = lerp(s0.color, s1.color, t);
	output.height = lerp(s0.height, s1.height, t);

	return output;
}

inline Surface BlendSmart(Surface s0, Surface s1, float t)
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

inline float2 ruvy(float2 uv)
{
	return float2(uv.x, 1.0 - uv.y);
}

inline float3 ruvy(float3 uv)
{
	return float3(uv.x, 1.0 - uv.y, uv.z);
}

inline float4 ruvy(float4 uv)
{
	return float4(uv.x, 1.0 - uv.y, uv.z, uv.w);
}
//-----------------------------------------------------------------------------

#if (COLORING_TECHNIQUE == 0)

//-----------------------------------------------------------------------------
#if (TILING_FIX_MODE <= 1)
// Texture atlas sampling function
// height, slope defines the tile based on MaterialTable texture
// vary sets one of 4 different tiles of the same material
Surface GetSurfaceColorAtlas(float height, float slope, float vary)
{
	float4 PackFactors = float4(1.0 / ATLAS_RES_X, 1.0 / ATLAS_RES_Y, ATLAS_TILE_RES, ATLAS_TILE_RES_LOG2);
	slope = saturate(slope * 0.5);

	float4 IdScale = tex2Dlod(MaterialTable, float4(ruvy(float2(height + texturingHeightOffset, (slope + 0.5) + texturingSlopeOffset)), 0, 0));

	uint materialID = min(int(IdScale.x) + int(vary), int(ATLAS_RES_X * ATLAS_RES_Y - 1));
	float2 tileOffs = float2(materialID % ATLAS_RES_X, materialID / ATLAS_RES_X) * PackFactors.xy;

	Surface res;
	float2 tileUV = (float2(1, 1) * faceParams.z + faceParams.xy) * texScale * IdScale.y;//(TexCoord.xy * faceParams.z + faceParams.xy) * texScale * IdScale.y;
	float2 dx = Fwidth(tileUV * PackFactors.z, PackFactors.xy); //dFdx(tileUV * PackFactors.z);
	float2 dy = Fwidth(tileUV * PackFactors.z, PackFactors.xy); //dFdy(tileUV * PackFactors.z);

	float lod = 4;//clamp(0.5 * log2(max(dot(dx, dx), dot(dy, dy))), 0.0, PackFactors.w);

	float2 invSize = InvSize * PackFactors.xy; //float2(pow(2.0, lod - PackFactors.w), 0.0) * PackFactors.xy;
	float4 uv = float4(tileOffs + frac(tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize, 0, 0);
	
	#if (TILING_FIX_MODE == 0)
		res.color = tex2Dlod(AtlasDiffSampler, float4(ruvy(float2(uv.xy * texturingUVAtlasOffset.xy)).xy, 0, 0));
	#elif (TILING_FIX_MODE == 1)
		float4 uv2 = (tileOffs + frac(-0.173 * tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize, 0, 0);
		res.color = lerp(tex2Dlod(AtlasDiffSampler, ruvy(uv * texturingUVAtlasOffset)), tex2Dlod(AtlasDiffSampler, ruvy(uv2 * texturingUVAtlasOffset)), 0.5);
	#endif
	
	float4 adjust = tex2Dlod(MaterialTable, float4(ruvy(float2(height + texturingHeightOffset, slope + texturingSlopeOffset)), 0, 0));

	adjust.xyz *= texColorConv;
	
	#if (COLOR_SPACE == 0)
		float3 hsl = rgb2hsl(res.color.rgb);
		hsl.x = frac(hsl.x + adjust.x);
		hsl.yz = clamp(hsl.yz + adjust.yz, 0.0, 1.0);
		res.color.rgb = hsl2rgb(hsl);
	#elif (COLOR_SPACE == 1)
		float3 rgb = res.color.rgb;
		rgb.x = frac(rgb.x + adjust.x);
		rgb.yz = clamp(rgb.yz + adjust.yz, 0.0, 1.0);
		res.color.rgb = rgb;
	#elif (COLOR_SPACE == 2)
		float3 rgb = res.color.rgb;
		rgb.xyz += adjust.xyz;
		res.color.rgb = rgb;
	#else
		
	#endif
	
	res.color = res.color * planetGlobalColor;
	res.height = res.color.a;
	res.color.a = adjust.a;

	return  res;
}

#else

Surface GetSurfaceColorAtlas(float height, float slope, float vary)
{
	float4 PackFactors = float4(1.0 / ATLAS_RES_X, 1.0 / ATLAS_RES_Y, ATLAS_TILE_RES, ATLAS_TILE_RES_LOG2);
	slope = saturate(slope * 0.5);

	float4 IdScale = tex2Dlod(MaterialTable, float4(ruvy(float2(height + texturingHeightOffset, (slope + 0.5) + texturingSlopeOffset)), 0, 0));

	uint materialID = min(int(IdScale.x) + int(vary), int(ATLAS_RES_X * ATLAS_RES_Y - 1));
	float2 tileOffs = float2(materialID % ATLAS_RES_X, materialID / ATLAS_RES_X) * PackFactors.xy;

	float2 tileUV = (float2(1, 1) * faceParams.z + faceParams.xy) * texScale * IdScale.y;//(TexCoord.xy * faceParams.z + faceParams.xy) * texScale * IdScale.y;
	float2 dx = Fwidth(tileUV * PackFactors.z, PackFactors.xy); //dFdx(tileUV * PackFactors.z);
	float2 dy = Fwidth(tileUV * PackFactors.z, PackFactors.xy); //dFdy(tileUV * PackFactors.z);

	float lod = 4;//clamp(0.5 * log2(max(dot(dx, dx), dot(dy, dy))), 0.0, PackFactors.w);

	float2 invSize = InvSize * PackFactors.xy; //float2(pow(2.0, lod - PackFactors.w), 0.0) * PackFactors.xy;

	// Voronoi-based random offset for tile texture coordinates and rotation
	float magOffs = 1.0; // magnitude of the texture coordinates offset
	float2 uvo = tileOffs + 0.5 * invSize;
	float2 uvs = PackFactors.xy - invSize;
	float2 p = floor(tileUV);
	float2 f = frac(tileUV);
	float4 color = float4(0, 0, 0, 0);
	float weight = 0.0;

	float4 adjust = tex2Dlod(MaterialTable, float4(ruvy(float2(height + texturingHeightOffset, slope + texturingSlopeOffset)), 0, 0));

	adjust.xyz *= texColorConv;

	for(int j = -1; j <= 1; j++)
	{
		for(int i = -1; i <= 1; i++)
		{
			float2 g = float2(float(i), float(j));
			float4 o = hash4(p + g);
			float2 r = g - f + o.xy;
			float d = dot(r, r);
			float w = SavePow(1.0 - smoothstep(0.0, 2.0, d * d), 1.0 + 16.0 * magOffs);

			#if (TILING_FIX_MODE == 2)
				float2 uv = frac(tileUV + magOffs * o.zy);
			#elif (TILING_FIX_MODE == 3)
				float a = o.w * IdScale.z; // magnitude of the texture coordinates rotation (zero for sand tiles)
				float2 sc  = float2(sin(a), cos(a));
				float2x2 rot = float2x2(sc.y, sc.x, -sc.x, sc.y);
				float2 uv = frac(mul((tileUV + magOffs * o.zy), rot));
			#endif

			// color conversion must be done before summarize, because hls color space is not additive
			float4 rgb = tex2Dlod(AtlasDiffSampler, float4(ruvy(uv * uvs + uvo * texturingUVAtlasOffset), 0, 0));

			#if (COLOR_SPACE == 0)
				float3 hsl = rgb2hsl(rgb.rgb);
				hsl.x = frac(hsl.x + adjust.x);
				hsl.yz = clamp(hsl.yz + adjust.yz, 0.0, 1.0);
				rgb.rgb = hsl2rgb(hsl);
			#elif (COLOR_SPACE == 1)
				rgb.x = frac(rgb.x + adjust.x);
				rgb.yz = clamp(rgb.yz + adjust.yz, 0.0, 1.0);
			#elif (COLOR_SPACE == 2)
				rgb.xyz += adjust.xyz;
			#else
				
			#endif

			color += w * rgb;
			weight += w;
		}
	}
	
	Surface res;
	res.color = color / weight * planetGlobalColor;
	res.height = res.color.a;
	res.color.a = adjust.a;

	return  res;
}

#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#if (TILE_BLEND_MODE == 0)
// Planet surface color function (uses the texture atlas sampling function)
// height, slope defines the tile based on MaterialTable texture
// vary sets one of 4 different tiles of the same material
Surface GetSurfaceColor(float height, float slope, float vary)
{
	return GetSurfaceColorAtlas(height, slope, vary * 4.0);
}

#elif (TILE_BLEND_MODE == 1)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	height = clamp(height - 0.000625, 0.0, 1.0);
	slope  = clamp(slope  + 0.001250, 0.0, 1.0);

	float h0 = floor(height * 8.0) * 0.125;
	float h1 = h0 + 0.125;
	float dh = (height - h0) * 8.0;
	float s0 = floor(slope  * 4.0) * 0.25;
	float s1 = s0 - 0.25;
	float ds = 1.0 - (slope - s0) * 4.0;
	float v0 = floor(vary * 16.0) * 0.25;
	float v1 = v0 - 0.25;
	float dv = 1.0 - (vary * 4.0 - v0) * 4.0;

	Surface surfH0, surfH1;
	Surface surfS0, surfS1;
	Surface surfV0, surfV1;

	surfH0 = GetSurfaceColorAtlas(h0, s0, v0);
	surfH1 = GetSurfaceColorAtlas(h1, s0, v0);
	surfS0 = Blend(surfH0, surfH1, dh);

	surfH0 = GetSurfaceColorAtlas(h0, s1, v0);
	surfH1 = GetSurfaceColorAtlas(h1, s1, v0);
	surfS1 = Blend(surfH0, surfH1, dh);

	surfV0 = Blend(surfS0, surfS1, ds);

	surfH0 = GetSurfaceColorAtlas(h0, s0, v1);
	surfH1 = GetSurfaceColorAtlas(h1, s0, v1);
	surfS0 = Blend(surfH0, surfH1, dh);

	surfH0 = GetSurfaceColorAtlas(h0, s1, v1);
	surfH1 = GetSurfaceColorAtlas(h1, s1, v1);
	surfS1 = Blend(surfH0, surfH1, dh);

	surfV1 = Blend(surfS0, surfS1, ds);

	return Blend(surfV0, surfV1, dv);
}

#elif (TILE_BLEND_MODE == 2)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	height = clamp(height - 0.000625, 0.0, 1.0);
	slope  = clamp(slope  + 0.001250, 0.0, 1.0);

	float h0 = floor(height * 8.0) * 0.125;
	float h1 = h0 + 0.125;
	float dh = (height - h0) * 8.0;
	float s0 = floor(slope  * 4.0) * 0.25;
	float s1 = s0 - 0.25;
	float ds = 1.0 - (slope - s0) * 4.0;
	float v0 = floor(vary * 16.0) * 0.25;
	float v1 = v0 - 0.25;
	float dv = 1.0 - (vary * 4.0 - v0) * 4.0;

	Surface surfH0, surfH1;
	Surface surfS0, surfS1;
	Surface surfV0, surfV1;

	surfH0 = GetSurfaceColorAtlas(h0, s0, v0);
	surfH1 = GetSurfaceColorAtlas(h1, s0, v0);
	surfS0 = BlendSmart(surfH0, surfH1, dh);

	surfH0 = GetSurfaceColorAtlas(h0, s1, v0);
	surfH1 = GetSurfaceColorAtlas(h1, s1, v0);
	surfS1 = BlendSmart(surfH0, surfH1, dh);

	surfV0 = BlendSmart(surfS0, surfS1, ds);

	surfH0 = GetSurfaceColorAtlas(h0, s0, v1);
	surfH1 = GetSurfaceColorAtlas(h1, s0, v1);
	surfS0 = BlendSmart(surfH0, surfH1, dh);

	surfH0 = GetSurfaceColorAtlas(h0, s1, v1);
	surfH1 = GetSurfaceColorAtlas(h1, s1, v1);
	surfS1 = BlendSmart(surfH0, surfH1, dh);

	surfV1 = BlendSmart(surfS0, surfS1, ds);

	return BlendSmart(surfV0, surfV1, dv);
}
#endif
//-----------------------------------------------------------------------------

#elif (COLORING_TECHNIQUE == 1)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	Surface surf;
	surf.height = height;

	return surf;
}

#endif

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

void FAST32_hash_3D(float3 gridcell, out float4 lowz_hash, out float4 highz_hash)
{
	// g ridcell is assumed to be an integer coordinate
	const float2 OFFSET = float2(50.0, 161.0);
	const float DOMAIN = 69.0;
	const float SOMELARGEFLOAT = 635.298681;
	const float ZINC = 48.500388;

	//	truncate the domain
	gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;
	float3 gridcell_inc1 = step(gridcell, float3(DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5)) * (gridcell + 1.0);

	//	calculate the noise
	float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;
	P *= P;
	P = P.xzxz * P.yyww;
	highz_hash.xy = float2(1.0 / (SOMELARGEFLOAT + float2(gridcell.z, gridcell_inc1.z) * ZINC));
	lowz_hash  = frac(P * highz_hash.xxxx );
	highz_hash = frac(P * highz_hash.yyyy );
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 RND_M = float4(1.0, 1.0, 1.0, 1.0);
float3 OFFSET = float3(0.5, 0.5, 0.5);
float3 OFFSETOUT = float3(1.5, 1.5, 1.5);
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float4 LessThan(float4 x, float4 y)
{
	return 1.0 - step(y, x);
}
//-----------------------------------------------------------------------------

#ifdef NOISE_ENGINE_SE
//-----------------------------------------------------------------------------
// 3D Perlin noise
float Noise(float3 p)
{
	const float one = 1.0 / 256.0;

	// Find unit cube that contains point
	// Find relative x,y,z of point in cube
	float3 P = fmod(floor(p), 256.0) * one;
	p -= floor(p);

	// Compute fade curves for each of x,y,z
	float3 ff = p * p * p * (p * (p * 6.0 - 15.0) + 10.0);

	// Hash coordinates of the 8 cube corners
	float4 AA = tex2Dlod(PermSampler, float4(P.xyz, 0));

	float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
	float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1, 0, 0));
	float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0, -1, 0));
	float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1, -1, 0));
	float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0, 0, -1));
	float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1, 0, -1));
	float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0, -1, -1));
	float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1, -1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = e - a;
	float k4 = a - b - c + d;
	float k5 = a - c - e + g;
	float k6 = a - b - e + f;
	float k7 = -a + b + c - d + e - f - g + h;

	return k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// 3D Perlin noise with derivatives, returns vec4(xderiv, yderiv, zderiv, noise)
float4 NoiseDeriv(float3 p)
{
	const float one = 1.0 / 256;

	// Find unit cube that contains point
	// Find relative x,y,z of point in cube
	float3 P = fmod(floor(p), 256.0) * one;
	p -= floor(p);

	// Compute fade curves for each of x,y,z
	float3 df = 30.0 * p * p * (p * (p - 2.0) + 1.0);
	float3 ff = p * p * p * (p * (p * 6.0 - 15.0) + 10.0);

	// Hash coordinates of the 8 cube corners
	float4 AA = tex2Dlod(PermSampler, float4(P.xyz, 0));

	float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
	float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1, 0, 0));
	float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0, -1, 0));
	float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1, -1, 0));
	float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0, 0, -1));
	float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1, 0, -1));
	float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0, -1, -1));
	float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1, -1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = e - a;
	float k4 = a - b - c + d;
	float k5 = a - c - e + g;
	float k6 = a - b - e + f;
	float k7 = -a + b + c - d + e - f - g + h;

	return float4(df.x * (k1 + k4 * ff.y + k6 * ff.z + k7 * ff.y * ff.z),
						  df.y * (k2 + k5 * ff.z + k4 * ff.x + k7 * ff.z * ff.x),
						  df.z * (k3 + k6 * ff.x + k5 * ff.y + k7 * ff.x * ff.y),
						  k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z);
}
//-----------------------------------------------------------------------------

#endif

#ifdef NOISE_ENGINE_ZNE

//-----------------------------------------------------------------------------
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

	float4 grad_results_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0);
	float4 grad_results_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1);

	float3 blend = Interpolation_C2(Pf);

	float4 blend2 = float4(blend.xy, float2(1.0 - blend.xy));
	float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);

	float final = dot(res0, blend2.zxzx * blend2.wwyy) * 1.1547005383792515290182975610039;

	return final;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
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

	float4 norm_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);
	float4 norm_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);

	grad_x0 *= norm_0;
	grad_y0 *= norm_0;
	grad_z0 *= norm_0;
	grad_x1 *= norm_1;
	grad_y1 *= norm_1;
	grad_z1 *= norm_1;

	//grad_x0 = mul(grad_x0, norm_0);
	//grad_y0 = mul(grad_y0, norm_0);
	//grad_z0 = mul(grad_z0, norm_0);
	//grad_x1 = mul(grad_x1, norm_1);
	//grad_y1 = mul(grad_y1, norm_1);
	//grad_z1 = mul(grad_z1, norm_1);

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

#endif

#ifdef NOISE_ENGINE_I

float Noise(float3 p)
{
	// Establish our grid cell and unit position
	float3 Pi = floor(p);
	float3 Pf = p - Pi;
	float3 Pf_min1 = Pf - 1.0;

	#if (NOISE_ENGINE_TECHNIQUE == 0)
		// Classic noise. Requires 3 random values per point.
		// With an efficent hash function will run faster than improved noise.

		// Calculate the hash
		float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
		FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

		// Calculate the gradients
		float4 grad_x0 = hashx0 - 0.49999;
		float4 grad_y0 = hashy0 - 0.49999;
		float4 grad_z0 = hashz0 - 0.49999;
		float4 grad_x1 = hashx1 - 0.49999;
		float4 grad_y1 = hashy1 - 0.49999;
		float4 grad_z1 = hashz1 - 0.49999;
		float4 grad_results_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0);
		float4 grad_results_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1);

		// Classic Perlin Interpolation
		float3 blend = Interpolation_C2(Pf);
		float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);
		float2 res1 = lerp(res0.xy, res0.zw, blend.y);
		float finalValue = lerp(res1.x, res1.y, blend.x);
		finalValue *= 1.1547005383792515290182975610039; // (optionally) scale things to a strict -1.0->1.0 rang *= 1.0/sqrt(0.75)
		return finalValue;
	#else
		// Improved noise. Requires 1 random value per point.
		// Will run faster than classic noise if a slow hashing function is used.

		// Calculate the hash
		float4 hash_lowz, hash_highz;
		FAST32_hash_3D(Pi, hash_lowz, hash_highz);

		#if (NOISE_ENGINE_TECHNIQUE == 1)
			// This will implement Ken Perlins "improved" classic noise using the 12 mid-edge gradient points.
			// NOTE: mid-edge gradients give us a nice strict -1.0->1.0 range without additional scaling.
			// [1,1,0] [-1,1,0] [1,-1,0] [-1,-1,0]
			// [1,0,1] [-1,0,1] [1,0,-1] [-1,0,-1]
			// [0,1,1] [0,-1,1] [0,1,-1] [0,-1,-1]
			hash_lowz *= 3.0;
			float4 grad_results_0_0 = lerp(float2(Pf.y, Pf_min1.y).xxyy, float2(Pf.x, Pf_min1.x).xyxy, LessThan(hash_lowz, float4(2.0, 2.0, 2.0, 2.0)));
			float4 grad_results_0_1 = lerp(Pf.zzzz, float2(Pf.y, Pf_min1.y).xxyy, LessThan(hash_lowz, float4(1.0, 1.0, 1.0, 1.0)));
			hash_lowz = frac(hash_lowz) - 0.5;
			float4 grad_results_0 = grad_results_0_0 * sign(hash_lowz) + grad_results_0_1 * sign(abs(hash_lowz) - float4(0.25, 0.25, 0.25, 0.25));

			hash_highz *= 3.0;
			float4 grad_results_1_0 = lerp(float2(Pf.y, Pf_min1.y).xxyy, float2(Pf.x, Pf_min1.x).xyxy, LessThan(hash_highz, float4(2.0, 2.0, 2.0, 2.0)));
			float4 grad_results_1_1 = lerp(Pf_min1.zzzz, float2(Pf.y, Pf_min1.y).xxyy, LessThan(hash_highz, float4(1.0, 1.0, 1.0, 1.0)));
			hash_highz = frac(hash_highz) - 0.5;
			float4 grad_results_1 = grad_results_1_0 * sign(hash_highz) + grad_results_1_1 * sign(abs(hash_highz) - float4(0.25, 0.25, 0.25, 0.25));

			// Blend the gradients and return
			float3 blend = Interpolation_C2(Pf);
			float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);
			float2 res1 = lerp(res0.xy, res0.zw, blend.y);
			return lerp(res1.x, res1.y, blend.x);
		#else
			// "Improved" noise using 8 corner gradients. Faster than the 12 mid-edge point method.
			// Ken mentions using diagonals like this can cause "clumping", but we'll live with that.
			// [1,1,1]  [-1,1,1]  [1,-1,1]  [-1,-1,1]
			// [1,1,-1] [-1,1,-1] [1,-1,-1] [-1,-1,-1]
			hash_lowz -= float4(0.5, 0.5, 0.5, 0.5);
			float4 grad_results_0_0 = float2(Pf.x, Pf_min1.x).xyxy * sign(hash_lowz);
			hash_lowz = abs(hash_lowz) - float4(0.25, 0.25, 0.25, 0.25);
			float4 grad_results_0_1 = float2(Pf.y, Pf_min1.y).xxyy * sign(hash_lowz);
			float4 grad_results_0_2 = Pf.zzzz * sign(abs(hash_lowz) - float4(0.125, 0.125, 0.125, 0.125));
			float4 grad_results_0 = grad_results_0_0 + grad_results_0_1 + grad_results_0_2;

			hash_highz -= float4(0.5, 0.5, 0.5, 0.5);
			float4 grad_results_1_0 = float2(Pf.x, Pf_min1.x).xyxy * sign(hash_highz);
			hash_highz = abs(hash_highz) - float4(0.25, 0.25, 0.25, 0.25);
			float4 grad_results_1_1 = float2(Pf.y, Pf_min1.y).xxyy * sign(hash_highz);
			float4 grad_results_1_2 = Pf_min1.zzzz * sign(abs(hash_highz) - float4(0.125, 0.125, 0.125, 0.125));
			float4 grad_results_1 = grad_results_1_0 + grad_results_1_1 + grad_results_1_2;

			// Blend the gradients and return
			float3 blend = Interpolation_C2(Pf);
			float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);
			float2 res1 = lerp(res0.xy, res0.zw, blend.y);
			return lerp(res1.x, res1.y, blend.x) * (2.0 / 3.0);   // (optionally) mult by (2.0/3.0)to scale to a strict -1.0->1.0 range
		#endif
	#endif
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 NoiseDeriv(float3 p)
{
	// Establish our grid cell and unit position
	float3 Pi = floor(p);
	float3 Pf = p - Pi;
	float3 Pf_min1 = Pf - 1.0;

	// calculate the hash
	// (various hashing methods listed in order of speed)
	float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
	FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

	// calculate the gradients
	float4 grad_x0 = hashx0 - 0.49999;
	float4 grad_y0 = hashy0 - 0.49999;
	float4 grad_z0 = hashz0 - 0.49999;
	float4 norm_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);
	grad_x0 *= norm_0;
	grad_y0 *= norm_0;
	grad_z0 *= norm_0;
	//grad_x0 = mul(grad_x0, norm_0);
	//grad_y0 = mul(grad_y0, norm_0);
	//grad_z0 = mul(grad_z0, norm_0);
	float4 grad_x1 = hashx1 - 0.49999;
	float4 grad_y1 = hashy1 - 0.49999;
	float4 grad_z1 = hashz1 - 0.49999;
	float4 norm_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);
	grad_x1 *= norm_1;
	grad_y1 *= norm_1;
	grad_z1 *= norm_1;
	//grad_x1 = mul(grad_x1, norm_1);
	//grad_y1 = mul(grad_y1, norm_1);
	//grad_z1 = mul(grad_z1, norm_1);
	float4 grad_results_0 = float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0;
	float4 grad_results_1 = float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1;

	// get lengths in the x+y plane
	float3 Pf_sq = Pf * Pf;
	float3 Pf_min1_sq = Pf_min1 * Pf_min1;
	float4 vecs_len_sq = float2(Pf_sq.x, Pf_min1_sq.x).xyxy + float2(Pf_sq.y, Pf_min1_sq.y).xxyy;

	// evaluate the surflet
	float4 m_0 = vecs_len_sq + Pf_sq.zzzz;
	m_0 = max(1.0 - m_0, 0.0);
	float4 m2_0 = m_0 * m_0;
	float4 m3_0 = m_0 * m2_0;

	float4 m_1 = vecs_len_sq + Pf_min1_sq.zzzz;
	m_1 = max(1.0 - m_1, 0.0);
	float4 m2_1 = m_1 * m_1;
	float4 m3_1 = m_1 * m2_1;

	// calculate the derivatives
	float4 temp_0 = -6.0 * m2_0 * grad_results_0;
	float xderiv_0 = dot(temp_0, float2(Pf.x, Pf_min1.x).xyxy) + dot(m3_0, grad_x0);
	float yderiv_0 = dot(temp_0, float2(Pf.y, Pf_min1.y).xxyy) + dot(m3_0, grad_y0);
	float zderiv_0 = dot(temp_0, Pf.zzzz) + dot(m3_0, grad_z0);

	float4 temp_1 = -6.0 * m2_1 * grad_results_1;
	float xderiv_1 = dot(temp_1, float2(Pf.x, Pf_min1.x).xyxy) + dot(m3_1, grad_x1);
	float yderiv_1 = dot(temp_1, float2(Pf.y, Pf_min1.y).xxyy) + dot(m3_1, grad_y1);
	float zderiv_1 = dot(temp_1, Pf_min1.zzzz) + dot(m3_1, grad_z1);

	const float FINAL_NORMALIZATION = 2.3703703703703703703703703703704;	// scales the final result to a strict (-1.0, 1.0) range
	return float4(float3(xderiv_0, yderiv_0, zderiv_0) + float3(xderiv_1, yderiv_1, zderiv_1),
				 dot(m3_0, grad_results_0) + dot(m3_1, grad_results_1)) * FINAL_NORMALIZATION;
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 permute(float4 x) { return fmod((x * 34.0 + 1.0) * x, 289.0); }
float3 permute3(float3 x) { return fmod((x * 34.0 + 1.0) * x, 289.0); }
float4 taylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

// 3D simplex noise
float sNoise(float3 v)
{
	v *= 0.25;

	float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
	float4 D = float4(0.0, 0.5, 1.0, 2.0);

	// First corner
	float3 i  = floor(v + dot(v, C.yyy));
	float3 x0 =   v - i + dot(i, C.xxx);

	// Other corners
	float3 g = step(x0.yzx, x0.xyz);
	float3 l = 1.0 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);

	//   x0 = x0 - 0.0 + 0.0 * C.xxx;
	//   x1 = x0 - i1  + 1.0 * C.xxx;
	//   x2 = x0 - i2  + 2.0 * C.xxx;
	//   x3 = x0 - 1.0 + 3.0 * C.xxx;
	float3 x1 = x0 - i1 + C.xxx;
	float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
	float3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

	// Permutations
	i = fmod(i, 289.0); 
	float4 p = permute(permute(permute(
		  i.z + float4(0.0, i1.z, i2.z, 1.0))
		+ i.y + float4(0.0, i1.y, i2.y, 1.0))
		+ i.x + float4(0.0, i1.x, i2.x, 1.0));

	// Gradients: 7x7 points over a square, mapped onto an octahedron.
	// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
	float n_ = 0.142857142857; // 1.0/7.0
	float3 ns = n_ * D.wyz - D.xzx;

	float4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

	float4 x_ = floor(j * ns.z);
	float4 y_ = floor(j - 7.0 * x_);    // mod(j,N)

	float4 x = x_ * ns.x + ns.yyyy;
	float4 y = y_ * ns.x + ns.yyyy;
	float4 h = 1.0 - abs(x) - abs(y);

	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);

	//vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
	//vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
	float4 s0 = floor(b0) * 2.0 + 1.0;
	float4 s1 = floor(b1) * 2.0 + 1.0;
	float4 sh = -step(h, float4(0, 0, 0, 0));

	float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

	float3 p0 = float3(a0.xy, h.x);
	float3 p1 = float3(a0.zw, h.y);
	float3 p2 = float3(a1.xy, h.z);
	float3 p3 = float3(a1.zw, h.w);

	//Normalise gradients
	float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
	m = m * m;
	return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}
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

float RidgedMultifractal(float3 ppoint, float gain, float o)
{
	float signal = 1.0;
	float summ = 0.0;
	float frequency = 1.0;
	float weight;

	for (int i = 0; i < o; ++i)
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

float RidgedNoise(float3 p)
{
	float n = 1.0f - abs(Noise(p) * 2.0);
	return n * n - 0.5;
}

float RidgedMultifractalExtra(float3 ppoint, int octaves, float frequency, float lacunarity, float gain)
{
	float sum = 0;
	float ampl = 1.0;

	for(int i = 0; i < octaves; i++)
	{
		float n = RidgedNoise(ppoint * frequency);
		sum += n * ampl;
		frequency *= lacunarity;
		ampl *= gain;
	}

	return sum;
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
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w * noiseDeriv.w);
		signal *= signal * weight;
		amplitude = SavePow(abs(frequency), -noiseH);
		summ += signal * amplitude;
		frequency *= noiseLacunarity;
		dsum -= amplitude * noiseDeriv.xyz * noiseDeriv.w;
	}

	return summ;
}

float RidgedMultifractalEroded(float3 ppoint, float gain, float warp, float o)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = 0.0;
	float signal = 1.0;
	float weight;
	float3  dsum = float3(0.0, 0.0, 0.0);
	float4  noiseDeriv;

	for (int i = 0; i < o; ++i)
	{
		noiseDeriv = NoiseDeriv((ppoint + warp * dsum) * frequency);
		weight = saturate(signal * gain);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w * noiseDeriv.w);
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
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w * noiseDeriv.w);
		signal *= signal * weight;
		amplitude = SavePow(frequency, -noiseH);
		summ += signal * amplitude;
		frequency *= noiseLacunarity;
		dsum -= amplitude * noiseDeriv.xyz * noiseDeriv.w;
	}

	return summ;
}

float RidgedMultifractalErodedDetail(float3 ppoint, float gain, float warp, float firstOctaveValue, float o)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = firstOctaveValue;
	float signal = firstOctaveValue;
	float weight;
	float3  dsum = float3(0.0, 0.0, 0.0);
	float4  noiseDeriv;

	for (int i = 0; i < o; ++i)
	{
		noiseDeriv = NoiseDeriv((ppoint + warp * dsum) * frequency);
		weight = saturate(signal * gain);
		signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w * noiseDeriv.w);
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
float Cell3NoiseF0(float3 p, int octaves, float amp)
{
	float freq = 1.0;
	float sum = 0.0;
	float gain = SavePow(Lacunarity(), -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= Lacunarity();
		amp *= gain;
	}

	return sum / 2.0;
}

float4 Cell3NoiseF0Vec(float3 p, int octaves, float amp)
{
	float3 cell = floor(p);
	float freq = 1.0;
	float sum = 0.0;
	float gain = SavePow(Lacunarity(), -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= Lacunarity();
		amp *= gain;
	}

	p = normalize(p + cell + OFFSETOUT);

	return float4(p, sum / 2.0);
}

float Cell3NoiseF1F0(float3 p, int octaves, float amp)
{
	float freq = 1.0;
	float sum = 0.0;
	float gain = SavePow(Lacunarity(), -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]) - sqrt(F[1]);

		freq *= Lacunarity();
		amp *= gain;
	}

	return sum / 2.0;
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
		float id = (i + 1) * 0.25;

		cell = Cell3Noise(ppoint + craterRoundDist * Fbm3D(ppoint * 2.56));

		lastlastlastLand = lastlastLand;
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell * radFactor);
		ppoint *= 1.81818182;
		amplitude *= craterAmplitudePerOctave;
		heightPeak *= craterHeightPeakPerOctave;
		heightFloor *= craterHeightFloorPerOctave;
		radInner *= craterRadInnerPerOctave;
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
		//amplitude *= 0.55;
		//heightPeak *= 0.25;
		//heightFloor *= 1.2;
		//radInner *= 0.60;
		amplitude *= craterAmplitudePerOctave;
		heightPeak *= craterHeightPeakPerOctave;
		heightFloor *= craterHeightFloorPerOctave;
		radInner *= craterRadInnerPerOctave;
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

float CrackNoise(float3 ppoint, float distrort, float freq, float o, float mod)
{
	ppoint = (ppoint + Randomize) * freq;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float2 cell;
	float dist;

	for (int i = 0; i < o; i++)
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
float GetTerraced(float value, float n, float power)
{
	float dValue = value * n;
	float f = frac(dValue);
	float i = floor(dValue);

	return (i + pow(f, power)) / n;
}

float RidgedMultifractalTerraced(float3 ppoint, float n, float power)
{
	float total = 0.0;
	float modifier = 1.5;

	float value = RidgedMultifractal(ppoint + Randomize, 1, 12);
	value = value * value;
	total += value * modifier;
	modifier *= 0.5;

	total = GetTerraced(total, n, power);

	return total;
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

//-----------------------------------------------------------------------------
float HeightMapAsteroid(float3 ppoint)
{
	// Global landscape
	noiseOctaves = 2.0;
	float3  p = ppoint * mainFreq + Randomize;
	float height = (Fbm(p) + 0.7) * 0.7;

	noiseOctaves = 8; // Height distortion
	float distort = 0.01 * Fbm(ppoint * montesFreq + Randomize);
	height += distort;

	// Hills
	noiseOctaves = 5;
	float hills = (0.5 + 1.5 * Fbm(p * 0.0721)) * hillsFreq;
	hills = Fbm(p * hills) * 0.15;
	noiseOctaves = 2;
	float hillsMod = smoothstep(0, 1, Fbm(p * hillsFraction) * 3.0);
	height *= 1.0 + hillsMagn * hills * hillsMod;

	// Craters
	noiseOctaves = 3;
	craterDistortion = 1.0;
	craterRoundDist  = 0.03;
	heightFloor = -0.1;
	heightPeak  = 0.6;
	heightRim   = 1.0;
	float crater = CraterNoise(ppoint, craterMagn, craterFreq, craterSqrtDensity, craterOctaves);

	return height + crater;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 ColorMapAsteroid(float3 ppoint, float height, float slope)
{
	noiseOctaves = 2.0;
	//height = DistFbm((ppoint + Randomize) * 3.7, 1.5) * 0.7 + 0.5;

	noiseOctaves = 5;
	float3 p = ppoint * colorDistFreq * 2.3;
	p += Fbm3D(p * 0.5) * 1.2;
	float vary = saturate((Fbm(p) + 0.7) * 0.7);

	//float4 c = float4(0.41, 0.41, 0.41, 1) * (0.5 + slope) + (0.0125 * height) + (0.05 * vary);
	//return c;

	Surface surf = GetSurfaceColor(height, slope, vary);
	surf.color.rgb *= 0.5 + slope;
	return surf.color;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapAsteroid(float3 ppoint, float height, float slope)
{
	// Thermal emission temperature (in thousand Kelvins)
	noiseOctaves = 5;
	float3  p = ppoint * 600.0 + Randomize;
	float dist = 10.0 * colorDistMagn * Fbm(p * 0.2);
	noiseOctaves = 3;
	float globTemp = 0.95 - abs(Fbm((p + dist) * 0.01)) * 0.08;
	noiseOctaves = 8;
	float varyTemp = abs(Fbm(p + dist));

	// Global surface melting
	float surfTemp = surfTemperature *
		(globTemp + varyTemp * 0.08) *
		saturate(2.0 * (lavaCoverage * 0.4 + 0.4 - 0.8 * height));

	float4 outColor;
	outColor.rgb = UnitToColor24(log(surfTemp) * 0.188 + 0.1316);
	outColor.a = 1.0;
	return outColor;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float HeightMapTerra(float3 ppoint)
{
	// Global landscape
	float3  p = ppoint * mainFreq + Randomize;
	noiseOctaves = 5;
	float3  distort = 0.35 * Fbm3D(p * 2.37);
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

	float inv2montesSpiky = 1.0 / (montesSpiky * montesSpiky);
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
		noiseH       = 1.0;
		noiseOffset  = 1.0;
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
		noiseH       = 0.9;
		noiseLacunarity = 4.0;
		noiseOffset  = montesSpiky;
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
		noiseH       = 1.0;
		noiseOffset  = montesSpiky;
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
		float dunes = 2 * cracksMagn * (0.2 + dunesMagn * max(Fbm(ppoint * dunesFreq) + 0.7, 0.0));
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
		crater = RidgedMultifractalErodedDetail(ppoint * 0.3 * montesFreq + Randomize, 2.0, erosion, 0.25 * crater);
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
		colorOverlay = float4(1.0, 0.0, 0.0, 0.0);
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
	vary = 0.125 * montHeight; // * Fbm(ppoint * 1700.0 + Randomize);
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
	//vary = (Fbm(p) + 0.7) * 0.7 * 0.5;
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

	//surf = GetSurfaceColor(climate, slope, vary);
	surf = GetSurfaceColor(height, slope, vary);

	// Sedimentary layers
	noiseOctaves = 4;
	float layers = Fbm(float3(height * (168.4 / 100) + 0.17 * vary, 0.43 * (p.x + p.y), 0.43 * (p.z - p.y)));
	//layers *= smoothstep(0.75, 0.8, climate) * (1.0 - smoothstep(0.825, 0.875, climate)); // only rock texture
	layers *= smoothstep(0.0, 1.0, slope);     // only steep slopes
	//layers *= step(surf.color.a, 0.01);         // do not make layers on snow
	//layers *= saturate(1.0 - 5.0 * lavaMask);   // do not make layers on lava
	surf.color.rgb *= float3(1.0, 1.0, 1.0) - float3(0.0, 0.5, 1.0) * layers;

	// Albedo variations
	noiseOctaves = 4;
	distort = Fbm3D((ppoint + Randomize) * 0.07) * 1.5;
	noiseOctaves = 5;
	//vary = Fbm((ppoint + distort) * 0.78);
	//surf.color *= 1.0 - 0.5 * vary;

	#ifdef VISUALIZE_BIOMES
		surf.color = lerp(surf.color, colorOverlay * biomeScale, 0.25);
	#endif

	if (surfType <= 3)   // water mask for planets with oceans
		surf.color.a += saturate((seaLevel - height) * 200.0);

	return surf.color;
}
//-----------------------------------------------------------------------------