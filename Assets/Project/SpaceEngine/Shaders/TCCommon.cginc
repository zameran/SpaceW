/* Procedural planet generator.
 *
 * Copyright (C) 2012-2015 Vladimir Romanyuk
 * All rights reserved.
 *
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Vladimir Romanyuk
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 */

// NOTE:
// Not all features already ported! (Early - WIP)

#define TCCOMMON

//-----------------------------------------------------------------------------
// include 2d noise engine features?
// 0 - do not include
// 1 - include.
#define INCLUDE_NOISE_ENGINE_2D 0
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// noise engine:
// NOISE_ENGINE_SE - space engine texture lookup
// NOISE_ENGINE_ZNE - zameran noise engine
// NOISE_ENGINE_I - space engine
#define NOISE_ENGINE_I
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// noise engine technique: (Works only with NOISE_ENGINE_I)
// 0 - classic noise - OK.
// 1 - Ken Perlin's "improved" - OK.
// 2 - fast "improved" - OK.
#ifdef NOISE_ENGINE_I
#define NOISE_ENGINE_TECHNIQUE 2
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// tiles blending method:
// 0 - hard mix (no blending)
// 1 - soft blending
// 2 - "smart" blening (based on atlas heightmap texture)
#define TILE_BLEND_MODE 0
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// tiling fix method:
// 0 - no tiling fix
// 1 - sampling texture 2 times at different scales
#define TILING_FIX_MODE 0
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define ATLAS_RES_X         8
#define ATLAS_RES_Y         16
#define ATLAS_TILE_RES      256
#define ATLAS_TILE_RES_LOG2 8
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define USESAVEPOW
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float3    Randomize;      // Randomize
uniform float4    faceParams;     // (x0,             y0,             size,                   face)
uniform float4    scaleParams;    // (offsU,          offsV,          scale,                  tidalLock)
uniform float4    mainParams;     // (mainFreq,       terraceProb,    surfClass,              snowLevel)
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
uniform float4    cloudsParams1;  // (cloudsFreq,     cloudsOctaves,  stripeZones,		      stripeTwist)
uniform float4    cloudsParams2;  // (cloudsLayer,    cloudsNLayers,  stripeFluct,            cloudsCoverage)
uniform float4    cycloneParams;  // (cycloneMagn,    cycloneFreq,    cycloneDensity,		  cycloneOctaves)
uniform float4	  planetGlobalColor;	 // ()
uniform float	  texturingHeightOffset; // ()
uniform float	  texturingSlopeOffset;  // ()
uniform float2	  texturingUVAtlasOffset;// ()
uniform float2	  InvSize;				 // ()
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform sampler2D	PermSampler;		// 3d
uniform sampler2D	PermGradSampler;	// 3d
uniform sampler2D	PermSamplerGL;		// 2d
uniform sampler2D	PermGradSamplerGL;	// 2d
uniform sampler1D   CloudsColorTable;   // clouds color table

uniform sampler2D   MaterialTable;      // material parameters table

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
#if !defined (MATH)
#include "Math.cginc"
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     tidalLock           scaleParams.w
#define		mainFreq			mainParams.x
#define     terraceProb         mainParams.y
#define     surfClass           mainParams.z
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
#define     volcanoDensity		volcanoParams1.z
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
#define     stripeZones         cloudsParams1.z
#define     stripeTwist         cloudsParams1.w
#define     cloudsLayer         cloudsParams2.x
#define     cloudsNLayers       cloudsParams2.y
#define     stripeFluct         cloudsParams2.z
#define     cloudsCoverage      cloudsParams2.w
#define     cycloneMagn         cycloneParams.x
#define     cycloneFreq         cycloneParams.y
#define     cycloneDensity		cycloneParams.z
#define     cycloneOctaves      cycloneParams.w
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define			saturate(x) clamp(x, 0.0, 1.0)
#define			madfrac(A, B) mad((A), (B), -floor((A) * (B)))
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define     GetCloudsColor(height)           tex2Dlod(CloudsColorTable, float2(height, 0.0)
#define     GetGasGiantCloudsColor(height)   tex2Dlod(MaterialTable, float4(height, 0.0, 0.0, 0.0))
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float SavePow(float f, float p) 
{ 
	#ifdef USESAVEPOW 
		return pow(abs(f), p);
	#else
		return pow(f, p);
	#endif
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Wrap(float value, float minimum, float maximum)
{
	float rangeSize = maximum - minimum;

	return (minimum + (value - minimum) - (rangeSize * floor((value - minimum) / rangeSize)));
}

float Wrap01(float value) { return Wrap(value, 0.0, 1.0); }
float Wrap101(float value) { return Wrap(value, -1.0, 1.0); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float smin(float a, float b, float k)
{
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);

	return lerp(b, a, h) - k * h * (1.0 - h);
}

inline float softExpMaxMin(float a, float b, float k)
{
	return log(exp(k * a) + exp(k * b)) / k;
}

inline float AngleBetween(float3 a, float3 b) 
{
	return acos(dot(a, b) / (length(a) * length(b)));
}

inline float Repeat(float t, float l)
{
	return t - floor(t / l) * l;
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

float2x2 Inverse(float2x2 m) 
{
  return float2x2(m[1][1], -m[0][1], -m[1][0], m[0][0]) / (m[0][0] * m[1][1] - m[0][1] * m[1][0]);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Normal vector blending. Source: http://blog.selfshadow.com/publications/blending-in-detail/
float3 BlendNormalUnity(float4 n1, float4 n2)
{
	const float4 size = float4(2.0, 2.0, 2.0, -2.0);
	const float4 offset = float4(-1.0, -1.0, -1.0, 1.0);

	n1 = n1.xyzz * size + offset;
	n2 = n2 * 2.0 - 1.0;

	float3 r = float3(dot(n1.zxx, n2.xyz),
					  dot(n1.yzy, n2.xyz),
					  dot(n1.xyw, -n2.xyz));

	return normalize(r);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float2 CartesianToSpherical(float3 xyz)
{
	float longitude = atan2(xyz.x, xyz.z);
	float latitude = asin(xyz.y / length(xyz));

	return float2(longitude, latitude);
}

float2 CartesianToSphericalUV(float3 xyz)
{
	float2 uv = CartesianToSpherical(xyz);

	uv.x = Repeat(0.5 - uv.x / M_PI2, 1.0);
	uv.y = 0.5 + uv.y / M_PI;

	return uv;
}

float3 SphericalToCartesian(float2 spherical)
{
	float2 Alpha = float2(sin(spherical.x), cos(spherical.x));
	float2 Delta = float2(sin(spherical.y), cos(spherical.y));

	return float3(Delta.y * Alpha.x, Delta.x, Delta.y * Alpha.y);
}

float3 GetPlanetPoint(float3 pos, float face)
{
	if (face == 6.0)	// global
	{
		float2 spherical;

		spherical.x = (pos.x * 2.0 - 0.5) * M_PI;
		spherical.y = (0.5 - pos.y) * M_PI;

		return SphericalToCartesian(spherical);
	}
	else				// cubemap
	{
		float3 p = normalize(pos);

		if (face == 0.0)
			return float3( p.z, -p.y, -p.x);  // neg_x
		else if (face == 1.0)
			return float3(-p.z, -p.y,  p.x);  // pos_x
		else if (face == 2.0)
			return float3( p.x, -p.z, -p.y);  // neg_y
		else if (face == 3.0)
			return float3( p.x,  p.z,  p.y);  // pos_y
		else if (face == 4.0)
			return float3(-p.x, -p.y, -p.z);  // neg_z
		else if (face == 5.0)
			return float3( p.x, -p.y,  p.z);  // pos_z
		else
			return float3(0.0, 0.0, 0.0);
	}
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 UnitToColor24(in float unit)
{
	const float3 factor = float3(1.0, 255.0, 65025.0);
	const float mask = 1.0 / 256.0;

	float3 color = unit * factor.rgb;

	color.gb = frac(color.gb);
	color.rg -= color.gb * mask;

	return clamp(color, 0.0, 1.0);
}

inline float ColorToUnit24(in float3 color)
{
	const float3 factor = float3(1.0, 1.0 / 255.0, 1.0 / 65025.0);

	return dot(color, factor);
}

inline float PackColor(float3 rgb) 
{
	return rgb.r + rgb.g * 256.0 + rgb.b * 256.0 * 256.0;
}

float3 UnpackColor(float f) 
{
	float3 color;

	color.b = floor(f / 256.0 / 256.0);
	color.g = floor((f - color.b * 256.0 * 256.0) / 256.0);
	color.r = floor(f - color.b * 256.0 * 256.0 - color.g * 256.0);

	return color / 255.0;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct Surface
{
	float4 color;
	float height;
};
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float2 dFdx(float2 p)
{
	return float2(p.x * p.x - p.y, p.y);
}

inline float2 dFdy(float2 p)
{
	return float2(p.y * p.y - p.x, p.x);
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
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 rgb2hsl(float3 rgb)
{
	/*
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
	*/
	
	const float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(rgb.bg, K.wz), float4(rgb.gb, K.xy), step(rgb.b, rgb.g));
	float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));

	float d = q.x - min(q.w, q.y);

	const float e = 1.0e-10;

	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsl2rgb(float3 hsl)
{
	/*
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
	*/

	const float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);

	float3 p = abs(frac(hsl.xxx + K.xyz) * 6.0 - K.www);

	return hsl.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), hsl.y);
}

float3 hue2rgb(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);

	return saturate(float3(R, G, B));
}

float3 rgb2hcv(in float3 rgb)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0 / 3.0) : float4(rgb.gb, 0.0, -1.0 / 3.0);
	float4 Q = (rgb.r < P.x) ? float4(P.xyw, rgb.r) : float4(rgb.r, P.yzx);

	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + EPSILON) + Q.z);

	return float3(H, C, Q.x);
}

float3 rgb2hsv(in float3 rgb)
{
	float3 HCV = rgb2hcv(rgb);

	float S = HCV.y / (HCV.z + EPSILON);

	return float3(HCV.x, S, HCV.z);
}

float3 hsv2rgb(in float3 hsv)
{
	float3 rgb = hue2rgb(hsv.x);

	return ((rgb - 1) * hsv.y + 1) * hsv.z;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float hash1(float p) { return frac(sin(p) * 158.5453123); }
inline float3 hash3(float2 p) { return frac(sin(float3(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)))) * 43758.5453); }
inline float4 hash4(float2 p) { return frac(sin(float4(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)), dot(p, float2(398.1, 176.7)))) * 43758.5453); }
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
	float b0 = max(a0 - ma, 0);
	float b1 = max(a1 - ma, 0);

	ma = 1.0 / (b0 + b1);
	b0 *= ma;
	b1 *= ma;

	Surface res;

	res.color  = s0.color  * b0 + s1.color  * b1;
	res.height = s0.height * b0 + s1.height * b1;

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

//-----------------------------------------------------------------------------
Surface GetSurfaceColorAtlas(float height, float slope, float vary)
{
	const float4  PackFactors = float4(1.0 / ATLAS_RES_X, 1.0 / ATLAS_RES_Y, ATLAS_TILE_RES, ATLAS_TILE_RES_LOG2);
	slope = saturate(slope * 0.5);

	float4 IdScale = tex2D(MaterialTable, float2(height + texturingHeightOffset, slope + 0.5));
	uint materialID = min(uint(IdScale.x) + uint(vary), uint(ATLAS_RES_X * ATLAS_RES_Y - 1));
	float2 tileOffs = float2(materialID % ATLAS_RES_X, materialID / ATLAS_RES_X) * PackFactors.xy;

	Surface res;

	float2 tileUV = (float2(1.0, 1.0) * faceParams.z + faceParams.xy) * texScale * IdScale.y;
	float2 invSize = InvSize * PackFactors.xy;
	float2 uv = tileOffs + frac(tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize;

	#if (TILING_FIX_MODE == 0)
		res.color = tex2D(AtlasDiffSampler, ruvy(uv));
	#elif (TILING_FIX_MODE == 1)
		float2 uv2 = tileOffs + frac(-0.173 * tileUV) * (PackFactors.xy - invSize) + 0.5 * invSize;
		res.color = lerp(tex2D(AtlasDiffSampler, ruvy(uv)), tex2D(AtlasDiffSampler, ruvy(uv2)), 0.5);
	#endif

	res.height = res.color.a;

	float4 adjust = tex2D(MaterialTable, float2(height + texturingHeightOffset, slope + texturingSlopeOffset));
	adjust.xyz *= texColorConv;

	float3 hsl = rgb2hsl(res.color.rgb);
	hsl.x  = frac(hsl.x  + adjust.x);
	hsl.yz = clamp(hsl.yz + adjust.yz, 0.0, 1.0);

	res.color.rgb = hsl2rgb(hsl);
	res.color.a = adjust.a;

	return res;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Planet surface color function (uses the texture atlas sampling function)
// height, slope defines the tile based on MaterialTable texture
// vary sets one of 4 different tiles of the same material
#if (TILE_BLEND_MODE == 0)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	return GetSurfaceColorAtlas(height, slope, vary * 4.0);
}

#elif (TILE_BLEND_MODE == 1)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	height = clamp(height - 0.0625, 0.0, 1.0);
	slope  = clamp(slope  + 0.1250, 0.0, 1.0);

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

	return   Blend(surfV0, surfV1, dv);
}

#elif (TILE_BLEND_MODE == 2)

Surface GetSurfaceColor(float height, float slope, float vary)
{
	height = clamp(height - 0.0625, 0.0, 1.0);
	slope  = clamp(slope  + 0.1250, 0.0, 1.0);

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

	return   BlendSmart(surfV0, surfV1, dv);
}

#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
void FAST32_hash_3D(float3 gridcell, out float4 lowz_hash_0, out float4 lowz_hash_1, out float4 lowz_hash_2, out float4 highz_hash_0, out float4 highz_hash_1, out float4 highz_hash_2)
{
	// Generates 3 random numbers for each of the 8 cell corners gridcell is assumed to be an integer coordinate

	// TODO: these constants need tweaked to find the best possible noise. Probably requires some kind of brute force computational searching or something....
	const float2 OFFSET = float2(50.0, 161.0);
	const float DOMAIN = 69.0;
	const float3 SOMELARGEFLOATS = float3(635.298681, 682.357502, 668.926525);
	const float3 ZINC = float3(48.500388, 65.294118, 63.934599);

	// Truncate the domain
	gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;

	float3 gridcell_inc1 = step(gridcell, float3(DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5)) * (gridcell + 1.0);

	// Calculate the noise
	float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;

	P *= P;
	P = P.xzxz * P.yyww;

	float3 lowz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz));
	float3 highz_mod = float3(1.0 / (SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz));

	lowz_hash_0  = frac(P * lowz_mod.xxxx);
	highz_hash_0 = frac(P * highz_mod.xxxx);
	lowz_hash_1  = frac(P * lowz_mod.yyyy);
	highz_hash_1 = frac(P * highz_mod.yyyy);
	lowz_hash_2  = frac(P * lowz_mod.zzzz);
	highz_hash_2 = frac(P * highz_mod.zzzz);
}

void FAST32_hash_3D(float3 gridcell, out float4 lowz_hash, out float4 highz_hash)
{
	// Gridcell is assumed to be an integer coordinate
	const float2 OFFSET = float2(50.0, 161.0);
	const float DOMAIN = 69.0;
	const float SOMELARGEFLOAT = 635.298681;
	const float ZINC = 48.500388;

	//	truncate the domain
	gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * (1.0 / DOMAIN)) * DOMAIN;

	float3 gridcell_inc1 = step(gridcell, float3(DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5)) * (gridcell + 1.0);

	// Calculate the noise
	float4 P = float4(gridcell.xy, gridcell_inc1.xy) + OFFSET.xyxy;

	P *= P;
	P = P.xzxz * P.yyww;

	highz_hash.xy = float2(1.0 / (SOMELARGEFLOAT + float2(gridcell.z, gridcell_inc1.z) * ZINC));
	lowz_hash  = frac(P * highz_hash.xxxx );
	highz_hash = frac(P * highz_hash.yyyy );
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
inline float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }

inline float CubicHermite(float A, float B, float C, float D, float t)
{
	return (-A / 2.0 + (3.0 * B) / 2.0 - (3.0 * C) / 2.0 + D / 2.0) * t * t * t + (A - (5.0 * B) / 2.0 + 2.0 * C - D / 2.0) * t * t + (-A / 2.0 + C / 2.0) * t + B;
}

inline float CubicHermite(float4 V, float t)
{
	return CubicHermite(V.x, V.y, V.z, V.w, t);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#ifdef COMPUTE_SHADER
// Improved texture interpolation by iq. http://www.iquilezles.org/www/articles/texture/texture.htm
float4 SampleCustom(Texture2D tex, SamplerState texSampler, float2 uv, float resolution)
{
	uv = uv * resolution + 0.5;

	float2 i = floor(uv);
	float2 f = uv - i;

	f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

	uv = i + f;
	uv = (uv - 0.5) / resolution;

	return tex.Sample(texSampler, uv);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Improved bilinear interpolated texture fetch by iq. http://www.iquilezles.org/www/articles/hwinterpolation/hwinterpolation.htm
float4 SampleCustomBilinear(Texture2D tex, SamplerState texSampler, float2 uv, float resolution)
{
	float2 st = uv * resolution - 0.5;

	float2 iuv = floor(st);
	float2 fuv = frac(st);

	float4 a = tex.Sample(texSampler, (iuv + float2(0.5, 0.5)) / resolution);
	float4 b = tex.Sample(texSampler, (iuv + float2(1.5, 0.5)) / resolution);
	float4 c = tex.Sample(texSampler, (iuv + float2(0.5, 1.5)) / resolution);
	float4 d = tex.Sample(texSampler, (iuv + float2(1.5, 1.5)) / resolution);

	return lerp(lerp(a, b, fuv.x), lerp(c, d, fuv.x), fuv.y);
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Curves by iq. http://www.iquilezles.org/www/articles/functions/functions.htm
inline float impulse(float k, float x)
{
	float h = k * x;

	return h * exp(1.0 - h);
}

inline float expstep(float x, float k, float n)
{
	return exp(-k * pow(x, n));
}

inline float parabola(float x, float k)
{
	return pow(4.0 * x * (1.0 - x), k);
}
	
inline float powercurve(float x, float a, float b)
{
	return (pow(a + b, a + b) / (pow(a, a) * pow(b, b))) * pow(x, a) * pow(1.0 - x, b);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
const float4 NOISE_RND_M = float4(1.0, 1.0, 1.0, 1.0);
const float3 NOISE_OFFSET = float3(0.5, 0.5, 0.5);
const float3 NOISE_OFFSETOUT = float3(1.5, 1.5, 1.5);
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float4 LessThan(float4 x, float4 y) { return 1.0 - step(y, x); }
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
	// NOTE : I FUCKING DID IT! I FIX DAT FORMULA FOR 3D!
	// Solution is "+ P.z"
	float4 AA = tex2Dlod(PermSampler, float4(P.xy, 0, 0)) + P.z;

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
	const float one = 1.0 / 256.0;

	// Find unit cube that contains point
	// Find relative x,y,z of point in cube
	float3 P = fmod(floor(p), 256.0) * one;
	p -= floor(p);

	// Compute fade curves for each of x,y,z
	float3 df = 30.0 * p * p * (p * (p - 2.0) + 1.0);
	float3 ff = p * p * p * (p * (p * 6.0 - 15.0) + 10.0);

	// Hash coordinates of the 8 cube corners
	// NOTE : I FUCKING DID IT! I FIX DAT FORMULA FOR 3D!
	// Solution is "+ P.z"
	float4 AA = tex2Dlod(PermSampler, float4(P.xy, 0, 0)) + P.z; // <- Here!
	
	float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
	float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1, 0, 0));
	float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0, -1, 0));
	float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1, -1, 0));
	float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0, 0, -1));
	float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1, 0, -1));
	float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0, -1, -1));
	float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1, -1, -1));

	float k0 =  a;
	float k1 =  b - a;
	float k2 =  c - a;
	float k3 =  e - a;
	float k4 =  a - b - c + d;
	float k5 =  a - c - e + g;
	float k6 =  a - b - e + f;
	float k7 = -a + b + c - d + e - f - g + h;

	return float4(df.x * (k1 + k4*ff.y + k6*ff.z + k7*ff.y*ff.z),
				  df.y * (k2 + k5*ff.z + k4*ff.x + k7*ff.z*ff.x),
				  df.z * (k3 + k6*ff.x + k5*ff.y + k7*ff.x*ff.y),
				  k0 + k1*ff.x + k2*ff.y + k3*ff.z + k4*ff.x*ff.y + k5*ff.y*ff.z + k6*ff.z*ff.x + k7*ff.x*ff.y*ff.z);
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
	// Establish our grid cell and unit position
	float3 Pi = floor(p);
	float3 Pf = p - Pi;
	float3 Pf_min1 = Pf - 1.0;

	// Calculate the hash (various hashing methods listed in order of speed).
	float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
	FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

	// Calculate the gradients
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

	// Calculate the dot products
	float4 dotval_0 = float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0;
	float4 dotval_1 = float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1;

	// NOTE : the following is based off Milo Yips derivation, but modified for parallel execution http://stackoverflow.com/a/14141774

	// Convert our data to a more parallel format
	float4 dotval0_grad0 = float4(dotval_0.x, grad_x0.x, grad_y0.x, grad_z0.x);
	float4 dotval1_grad1 = float4(dotval_0.y, grad_x0.y, grad_y0.y, grad_z0.y);
	float4 dotval2_grad2 = float4(dotval_0.z, grad_x0.z, grad_y0.z, grad_z0.z);
	float4 dotval3_grad3 = float4(dotval_0.w, grad_x0.w, grad_y0.w, grad_z0.w);
	float4 dotval4_grad4 = float4(dotval_1.x, grad_x1.x, grad_y1.x, grad_z1.x);
	float4 dotval5_grad5 = float4(dotval_1.y, grad_x1.y, grad_y1.y, grad_z1.y);
	float4 dotval6_grad6 = float4(dotval_1.z, grad_x1.z, grad_y1.z, grad_z1.z);
	float4 dotval7_grad7 = float4(dotval_1.w, grad_x1.w, grad_y1.w, grad_z1.w);

	// Evaluate common constants
	float4 k0_gk0 = dotval1_grad1 - dotval0_grad0;
	float4 k1_gk1 = dotval2_grad2 - dotval0_grad0;
	float4 k2_gk2 = dotval4_grad4 - dotval0_grad0;
	float4 k3_gk3 = dotval3_grad3 - dotval2_grad2 - k0_gk0;
	float4 k4_gk4 = dotval5_grad5 - dotval4_grad4 - k0_gk0;
	float4 k5_gk5 = dotval6_grad6 - dotval4_grad4 - k1_gk1;
	float4 k6_gk6 = (dotval7_grad7 - dotval6_grad6) - (dotval5_grad5 - dotval4_grad4) - k3_gk3;

	// C2 Interpolation
	float3 blend = Interpolation_C2(Pf);
	float3 blendDeriv = Interpolation_C2_Deriv(Pf);

	// Calculate final noise + deriv
	float u = blend.x;
	float v = blend.y;
	float w = blend.z;

	float4 xxx = (u * (k0_gk0 + v * k3_gk3));
	float4 yyy = (v * (k1_gk1 + w * k5_gk5));
	float4 zzz = (w * (k2_gk2 + u * (k4_gk4 + v * k6_gk6)));

	float4 deriv = dotval0_grad0 + xxx + yyy + zzz;

	float4 result = 0;

	result.x = dot(float4(k0_gk0.x, k3_gk3.x * v, float2(k4_gk4.x, k6_gk6.x * v) * w), float4(blendDeriv.x, blendDeriv.x, blendDeriv.x, blendDeriv.x));
	result.y = dot(float4(k1_gk1.x, k3_gk3.x * u, float2(k5_gk5.x, k6_gk6.x * u) * w), float4(blendDeriv.y, blendDeriv.y, blendDeriv.y, blendDeriv.y));
	result.z = dot(float4(k2_gk2.x, k4_gk4.x * u, float2(k5_gk5.x, k6_gk6.x * u) * v), float4(blendDeriv.z, blendDeriv.z, blendDeriv.z, blendDeriv.z));
	result.w = deriv.x;

	// Normalize
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
		// Classic noise. Requires 3 random values per point. With an efficent hash function will run faster than improved noise.

		// Calculate the hash
		float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
		FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

		// Calculate the gradients
		const float4 C = float4(0.49999, 0.49999, 0.49999, 0.49999);
		float4 grad_x0 = hashx0 - C;
		float4 grad_y0 = hashy0 - C;
		float4 grad_z0 = hashz0 - C;
		float4 grad_x1 = hashx1 - C;
		float4 grad_y1 = hashy1 - C;
		float4 grad_z1 = hashz1 - C;
		float4 grad_results_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0);
		float4 grad_results_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1) * (float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1);

		// Classic Perlin Interpolation
		float3 blend = Interpolation_C2(Pf);

		float4 res0 = lerp(grad_results_0, grad_results_1, blend.z);
		float2 res1 = lerp(res0.xy, res0.zw, blend.y);

		float finalValue = lerp(res1.x, res1.y, blend.x);

		return finalValue * 1.1547005383792515290182975610039; // [optionally] Scale things to a strict [-1.0, 1.0] range; -> *= 1.0 / sqrt(0.75)
	#else
		// Improved noise. Requires 1 random value per point. Will run faster than classic noise if a slow hashing function is used.

		// Calculate the hash
		float4 hash_lowz, hash_highz;
		FAST32_hash_3D(Pi, hash_lowz, hash_highz);

		#if (NOISE_ENGINE_TECHNIQUE == 1)
			// This will implement Ken Perlins "improved" classic noise using the 12 mid-edge gradient points.
			// NOTE : mid-edge gradients give us a nice strict -1.0->1.0 range without additional scaling.
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

			return lerp(res1.x, res1.y, blend.x) * 0.66666666666; //0.66666666666 = (2.0 / 3.0) //(2.0 / 3.0); // [optionally] mult by (2.0 / 3.0) to scale to a strict [-1.0, 1.0] range.
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

			return lerp(res1.x, res1.y, blend.x) * 0.66666666666; //0.66666666666 = (2.0 / 3.0) //(2.0 / 3.0); // [optionally] multiply by (2.0 / 3.0) to scale to a strict [-1.0, 1.0] range.
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

	// Calculate the hash (various hashing methods listed in order of speed).
	float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
	FAST32_hash_3D(Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1);

	// Calculate the gradients
	const float4 C = float4(0.49999, 0.49999, 0.49999, 0.49999);
	float4 grad_x0 = hashx0 - C;
	float4 grad_y0 = hashy0 - C;
	float4 grad_z0 = hashz0 - C;
	float4 norm_0 = rsqrt(grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0);

	grad_x0 *= norm_0;
	grad_y0 *= norm_0;
	grad_z0 *= norm_0;

	float4 grad_x1 = hashx1 - C;
	float4 grad_y1 = hashy1 - C;
	float4 grad_z1 = hashz1 - C;
	float4 norm_1 = rsqrt(grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1);

	grad_x1 *= norm_1;
	grad_y1 *= norm_1;
	grad_z1 *= norm_1;

	float4 grad_results_0 = float2(Pf.x, Pf_min1.x).xyxy * grad_x0 + float2(Pf.y, Pf_min1.y).xxyy * grad_y0 + Pf.zzzz * grad_z0;
	float4 grad_results_1 = float2(Pf.x, Pf_min1.x).xyxy * grad_x1 + float2(Pf.y, Pf_min1.y).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1;

	// Get lengths in the x+y plane
	float3 Pf_sq = Pf * Pf;
	float3 Pf_min1_sq = Pf_min1 * Pf_min1;
	float4 vecs_len_sq = float2(Pf_sq.x, Pf_min1_sq.x).xyxy + float2(Pf_sq.y, Pf_min1_sq.y).xxyy;

	// Evaluate the surflet
	float4 m_0 = vecs_len_sq + Pf_sq.zzzz;
	m_0 = max(1.0 - m_0, 0.0);
	float4 m2_0 = m_0 * m_0;
	float4 m3_0 = m_0 * m2_0;

	float4 m_1 = vecs_len_sq + Pf_min1_sq.zzzz;
	m_1 = max(1.0 - m_1, 0.0);
	float4 m2_1 = m_1 * m_1;
	float4 m3_1 = m_1 * m2_1;

	// Calculate the derivatives
	float4 temp_0 = -6.0 * m2_0 * grad_results_0;
	float xderiv_0 = dot(temp_0, float2(Pf.x, Pf_min1.x).xyxy) + dot(m3_0, grad_x0);
	float yderiv_0 = dot(temp_0, float2(Pf.y, Pf_min1.y).xxyy) + dot(m3_0, grad_y0);
	float zderiv_0 = dot(temp_0, Pf.zzzz) + dot(m3_0, grad_z0);

	float4 temp_1 = -6.0 * m2_1 * grad_results_1;
	float xderiv_1 = dot(temp_1, float2(Pf.x, Pf_min1.x).xyxy) + dot(m3_1, grad_x1);
	float yderiv_1 = dot(temp_1, float2(Pf.y, Pf_min1.y).xxyy) + dot(m3_1, grad_y1);
	float zderiv_1 = dot(temp_1, Pf_min1.zzzz) + dot(m3_1, grad_z1);

	const float FINAL_NORMALIZATION = 2.3703703703703703703703703703704;	// Scale the final result to a strict [-1.0, 1.0] range.
	return float4(float3(xderiv_0, yderiv_0, zderiv_0) + float3(xderiv_1, yderiv_1, zderiv_1),
				 dot(m3_0, grad_results_0) + dot(m3_1, grad_results_1)) * FINAL_NORMALIZATION;
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#if (INCLUDE_NOISE_ENGINE_2D == 1)
float Noise2D(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	// Get weights from the coordinate fraction
	float2 w = p * p * p * (p * (p * 6 - 15) + 10);
	float4 w4 = float4(1, w.x, w.y, w.x * w.y);

	// Get the four randomly permutated indices from the noise lattice nearest to p and offset these numbers with the seed number.
	float4 perm = tex2Dlod(PermSamplerGL, float4(P, 0, 0)) + seed;

	// Permutate the four offseted indices again and get the 2D gradient for each of the four permutated coordinates-seed pairs.
	float4 g1 = tex2Dlod(PermGradSamplerGL, float4(perm.xy, 0, 0)) * 2 - 1;
	float4 g2 = tex2Dlod(PermGradSamplerGL, float4(perm.zw, 0, 0)) * 2 - 1;

	// Evaluate the four lattice gradients at p
	float a = dot(g1.xy, p);
	float b = dot(g2.xy, p + float2(-1,  0));
	float c = dot(g1.zw, p + float2( 0, -1));
	float d = dot(g2.zw, p + float2(-1, -1));

	// Bi-linearly blend between the gradients, using w4 as blend factors.
	float4 grads = float4(a, b - a, c - a, a - b - c + d);

	float n = dot(grads, w4);

	// Return the noise value, roughly normalized in the range [-1.0, 1.0]
	return n * 1.5;
}

float3 Noise2DPseudoDeriv(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 f = p * p * p * (p * (p * 6 - 15) + 10);
	float2 df = p * p * (p * (30 * p - 60) + 30);

	float4 AA = tex2Dlod(PermSamplerGL, float4(P, 0, 0)) + seed / 256;
	float4 G1 = tex2Dlod(PermGradSamplerGL, float4(AA.xy, 0, 0)) * 2 - 1;
	float4 G2 = tex2Dlod(PermGradSamplerGL, float4(AA.zw, 0, 0)) * 2 - 1;

	float a = dot(G1.xy, p);
	float b = dot(G2.xy, p + float2(-1,  0));
	float c = dot(G1.zw, p + float2( 0, -1));
	float d = dot(G2.zw, p + float2(-1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = a - b - c + d;

	float n = k0 + k1 * f.x + k2 * f.y + k3 * f.x * f.y;

	float dx = df.x * (k1 + k3 * f.y);
	float dy = df.y * (k2 + k3 * f.x);

	return float3(n, dx, dy) * 1.5;
}

float3 Noise2DDeriv(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 w = p * p * p * (p * (p * 6 - 15) + 10);
	float2 dw = p * p * (p * (p * 30 - 60) + 30);
	float2 dwp = p * p * p * (p * (p * 36 - 75) + 40);

	float4 AA = tex2Dlod(PermSamplerGL, float4(P, 0, 0)) + seed / 256;
	float4 G1 = tex2Dlod(PermGradSamplerGL, float4(AA.xy, 0, 0)) * 2 - 1;
	float4 G2 = tex2Dlod(PermGradSamplerGL, float4(AA.zw, 0, 0)) * 2 - 1;

	float k0 = G1.x * p.x + G1.y * p.y;
	float k1 = (G2.x - G1.x) * p.x + (G2.y - G1.y) * p.y - G2.x;
	float k2 = (G1.z - G1.x) * p.x + (G1.w - G1.y) * p.y - G1.w;
	float k3 = (G1.x - G2.x - G1.z + G2.z) * p.x + (G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w; // a - b - c + d

	float n = k0 + k1 * w.x + k2 * w.y + k3 * w.x * w.y;

	float dx = (G1.x + (G1.z - G1.x) * w.y) + ((G2.y - G1.y) * p.y - G2.x + ((G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w) * w.y) * dw.x + ((G2.x - G1.x) + (G1.x - G2.x - G1.z + G2.z) * w.y) * dwp.x;
	float dy = (G1.y + (G2.y - G1.y) * w.x) + ((G1.z - G1.x) * p.x - G1.w + ((G1.x - G2.x - G1.z + G2.z) * p.x + G2.x + G1.w - G2.z - G2.w) * w.x) * dw.y + ((G1.w - G1.y) + (G1.y - G2.y - G1.w + G2.w) * w.x) * dwp.y;

	return float3(n, dx, dy) * 1.5;
}

float3 Noise2DAlternativeDeriv(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 f = p * p * p * (p * (p * 6 - 15) + 10);
	float2 ddf = p * (p * (p * 120 - 180) + 60);
	float2 ddfp = p * p * (p * (p * 180 - 300) + 120);

	float4 AA = tex2Dlod(PermSamplerGL, float4(P, 0, 0)) + seed / 256;
	float4 G1 = tex2Dlod(PermGradSamplerGL, float4(AA.xy, 0, 0)) * 2 - 1;
	float4 G2 = tex2Dlod(PermGradSamplerGL, float4(AA.zw, 0, 0)) * 2 - 1;

	float k0 = G1.x * p.x + G1.y * p.y;
	float k1 = (G2.x - G1.x) * p.x + (G2.y - G1.y) * p.y - G2.x;
	float k2 = (G1.z - G1.x) * p.x + (G1.w - G1.y) * p.y - G1.w;
	float k3 = (G1.x - G2.x - G1.z + G2.z) * p.x + (G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w; // a - b - c + d

	float n = k0 + k1 * f.x + k2 * f.y + k3 * f.x * f.y;

	float ddx = ((G2.y - G1.y) * p.y - G2.x + ((G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w) * f.y) * ddf.x + ((G2.x-G1.x) + (G1.x - G2.x - G1.z + G2.z) * f.y) * ddfp.x;
	float ddy = ((G1.z - G1.x) * p.x - G1.w + ((G1.x - G2.x - G1.z + G2.z) * p.x + G2.x + G1.w - G2.z - G2.w) * f.x) * ddf.y + ((G1.w-G1.y) + (G1.y - G2.y - G1.w + G2.w) * f.x) * ddfp.y;

	return float3(n, ddx, ddy) * 1.5;
}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float4 modi(float4 x, float y) { return x - y * floor(x / y); }
inline float3 modi(float3 x, float y) { return x - y * floor(x / y); }
inline float2 modi(float2 x, float y) { return x - y * floor(x / y); }
inline float modi(float x, float y) { return x - y * floor(x / y); }
inline float4 Permutation(float4 x) { return modi((34.0 * x + 1.0) * x, 289.0); }
inline float3 Permutation(float3 x) { return modi((34.0 * x + 1.0) * x, 289.0); }
inline float4 TaylorInvSqrt(float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

#define K 0.142857142857
#define Ko 0.428571428571

float2 iNoise(float3 P, float jitter)
{			
	float3 Pi = modi(floor(P), 289.0);
	float3 Pf = frac(P);
	const float3 oi = float3(-1.0, 0.0, 1.0);
	const float3 of = float3(-0.5, 0.5, 1.5);
	float3 px = Permutation(Pi.x + oi);
	float3 py = Permutation(Pi.y + oi);

	float3 p, ox, oy, oz, dx, dy, dz;
	float2 F = 1e6;

	for(int i = 0; i < 3; i++)
	{
		for(int j = 0; j < 3; j++)
		{
			p = Permutation(px[i] + py[j] + Pi.z + oi); // pij1, pij2, pij3

			ox = frac(p * K) - Ko;
			oy = modi(floor(p * K), 7.0) * K - Ko;
			
			p = Permutation(p);
			
			oz = frac(p * K) - Ko;
		
			dx = Pf.x - of[i] + jitter * ox;
			dy = Pf.y - of[j] + jitter * oy;
			dz = Pf.z - of + jitter * oz;
			
			float3 d = dx * dx + dy * dy + dz * dz; // dij1, dij2 and dij3, squared
			
			// Find lowest and second lowest distances
			for(int n = 0; n < 3; n++)
			{
				if(d[n] < F[0])
				{
					F[1] = F[0];
					F[0] = d[n];
				}
				else if(d[n] < F[1])
				{
					F[1] = d[n];
				}
			}
		}
	}
	
	return F;
}

// 3D simplex noise
float sNoise(float3 v)
{
	const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
	const float4 D = float4(0.0, 0.5, 1.0, 2.0);

	// First corner
	float3 i  = floor(v + dot(v, C.yyy));
	float3 x0 = v - i + dot(i, C.xxx);

	// Other corners
	float3 g = step(x0.yzx, x0.xyz);
	float3 l = 1.0 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);

	// x0 = x0 - 0.0 + 0.0 * C.xxx;
	// x1 = x0 - i1  + 1.0 * C.xxx;
	// x2 = x0 - i2  + 2.0 * C.xxx;
	// x3 = x0 - 1.0 + 3.0 * C.xxx;
	float3 x1 = x0 - i1 + C.xxx;
	float3 x2 = x0 - i2 + C.yyy; // 2.0 * C.x = 1 / 3 = C.y
	float3 x3 = x0 - D.yyy;      // -1.0 + 3.0 * C.x = -0.5 = -D.y

	// Permutations
	i = modi(i, 289.0); 

	float4 p = Permutation(Permutation(Permutation(i.z + float4(0.0, i1.z, i2.z, 1.0)) + i.y + float4(0.0, i1.y, i2.y, 1.0)) + i.x + float4(0.0, i1.x, i2.x, 1.0));

	// Gradients: 7x7 points over a square, mapped onto an octahedron.
	// The ring size 17 * 17 = 289 is close to a multiple of 49 (49 * 6 = 294)

	const float n_ = 0.142857142857; // 1.0 / 7.0
	float3 ns = n_ * D.wyz - D.xzx;

	float4 j = p - 49.0 * floor(p * ns.z * ns.z);  // mod(p,7*7)

	float4 x_ = floor(j * ns.z);
	float4 y_ = floor(j - 7.0 * x_);    // mod(j,N)

	float4 x = x_ * ns.x + ns.yyyy;
	float4 y = y_ * ns.x + ns.yyyy;
	float4 h = 1.0 - abs(x) - abs(y);

	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);

	//vec4 s0 = vec4(lessThan(b0, 0.0)) * 2.0 - 1.0;
	//vec4 s1 = vec4(lessThan(b1, 0.0)) * 2.0 - 1.0;
	float4 s0 = floor(b0) * 2.0 + 1.0;
	float4 s1 = floor(b1) * 2.0 + 1.0;
	float4 sh = -step(h, float4(0, 0, 0, 0));

	float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

	float3 p0 = float3(a0.xy, h.x);
	float3 p1 = float3(a0.zw, h.y);
	float3 p2 = float3(a1.xy, h.z);
	float3 p3 = float3(a1.zw, h.w);

	// Normalise gradients
	float4 norm = TaylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));

	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);

	m = m * m;

	return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

float SimplexRidgedMultifractal(float3 position, int octaves, float frequency, float persistence) 
{
	float total = 0.0;
	float maxAmplitude = 0.0;
	float amplitude = 1.0;

	for (int i = 0; i < octaves; i++) 
	{
		total += ((1.0 - abs(sNoise(position * frequency))) * 2.0 - 1.0) * amplitude;
		frequency *= 2.0;
		maxAmplitude += amplitude;
		amplitude *= persistence;
	}

	return total / maxAmplitude;
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

float Fbm(float3 ppoint, int o)
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

float Fbm(float3 ppoint, float lacunarity, float h, int o)
{
	float summ = 0.0;
	float ampl = 1.0;
	float gain = SavePow(lacunarity, -h);

	for (int i = 0; i < o; ++i)
	{
		summ += Noise(ppoint) * ampl;
		ampl *= gain;
		ppoint *= lacunarity;
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

float FbmClouds(float3 ppoint, int o)
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

float3 FbmClouds3D(float3 ppoint)
{
	float3 summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;

	for (int i = 0; i < cloudsOctaves; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl  *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float3 Fbm3D(float3 ppoint)
{
	float3 summ = float3(0.0, 0.0, 0.0);
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

float3 Fbm3D(float3 ppoint, int o)
{
	float3 summ = float3(0.0, 0.0, 0.0);
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
	float3 summ = float3(0.0, 0.0, 0.0);
	float ampl = 1.0;

	for (int i = 0; i < cloudsOctaves; ++i)
	{
		summ += NoiseVec3(ppoint) * ampl;
		ampl *= 0.333;
		ppoint *= 3.1416;
	}

	return summ;
}

float3 Fbm3DClouds(float3 ppoint, int o)
{
	float3 summ = float3(0.0, 0.0, 0.0);
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

float DistFbm(float3 ppoint, float dist, int o)
{
	float summ = 0.0;
	float ampl = 1.0;
	float gain = SavePow(noiseLacunarity, -noiseH);

	for (int i = 0; i < o; ++i)
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

float RidgedMultifractal(float3 ppoint, float gain, int o)
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
		sum += RidgedNoise(ppoint * frequency) * ampl;
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
	float3 dsum = float3(0.0, 0.0, 0.0);
	float4 noiseDeriv;

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

float RidgedMultifractalEroded(float3 ppoint, float gain, float warp, int o)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = 0.0;
	float signal = 1.0;
	float weight;
	float3 dsum = float3(0.0, 0.0, 0.0);
	float4 noiseDeriv;

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
	float3 dsum = float3(0.0, 0.0, 0.0);
	float4 noiseDeriv;

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

float RidgedMultifractalErodedDetail(float3 ppoint, float gain, float warp, float firstOctaveValue, int o)
{
	float frequency = 1.0;
	float amplitude = 1.0;
	float summ = firstOctaveValue;
	float signal = firstOctaveValue;
	float weight;
	float3 dsum = float3(0.0, 0.0, 0.0);
	float4 noiseDeriv;

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

// "iqTurbulence" function by Inigo Quilez
// http://www.iquilezles.org, http://www.decarpentier.nl/scape-procedural-basics
float iqTurbulence(float3 ppoint, float gain)
{
	float4 n;
	float summ = 0.5;
	float freq = 1.0;
	float amp  = 1.0;
	float2 dsum = float2(0.0, 0.0);

	for (int i = 0; i < noiseOctaves; i++)
	{
		n = NoiseDeriv(ppoint * freq);
		dsum += n.yz;
		summ += amp * n.x / (1.0 + dot(dsum, dsum));
		freq *= noiseLacunarity;
		amp  *= gain;
	}

	return summ;
}

// iqTurbulence with faded octave 2 mudulates octaves oct to noiseOctaves
float iqTurbulence2(float3 ppoint, float gain, int oct)
{
	// Octave 2
	float4 n = NoiseDeriv(ppoint * noiseLacunarity * noiseLacunarity);
	float oct0 = 0.5 + n.x / (1.0 + dot(n.yz, n.yz));

	// Octaves oct to noiseOctaves
	float summ = 0.5;
	float freq = pow(noiseLacunarity, noiseOctaves - oct);
	float amp  = 1.0;
	float2 dsum = float2(0.0, 0.0);

	for (int i = oct; i < noiseOctaves; i++)
	{
		n = NoiseDeriv(ppoint * freq);
		dsum += n.yz;
		summ += amp * n.x / (1 + dot(dsum, dsum));
		freq *= noiseLacunarity;
		amp  *= gain;
	}

	// Modulate noise with octave 2
	return summ * oct0;
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

float4 NoiseRandomUVec4(float3 c)
{
	float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));

	float4 r;

	r.z = frac(512.0 * j);
	j *= 0.125;

	r.x = frac(512.0 * j);
	j *= 0.125;

	r.y = frac(512.0 * j);
	j *= 0.125;

	r.w = frac(512.0 * j);

	return r - 0.5;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Cell2Noise(float3 p)
{
	float3 cell = floor(p);
	float3 offs = p - cell - NOISE_OFFSET;
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
	float3 offs = p - cell - NOISE_OFFSET;
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
	float3 offs = p - cell - NOISE_OFFSET;
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

	ppoint = normalize(ppoint + cell + NOISE_OFFSETOUT);

	return float4(ppoint, sqrt(distMin));
}

float Cell2NoiseColor(float3 p, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell - NOISE_OFFSET;
	float3 pos;
	float4 rndM = NOISE_RND_M;
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
	float3 offs = p - cell - NOISE_OFFSET;
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

	ppoint = normalize(ppoint + cell + NOISE_OFFSETOUT);

	return float4(ppoint, length(ppoint * Radius - p));
}

void Cell2Noise2Sphere(float3 p, float Radius, out float4 point1, out float4 point2)
{
	p *= Radius;

	float3 cell = floor(p);
	float3 offs = p - cell - NOISE_OFFSET;
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

	point1.xyz = normalize(point1.xyz + cell + NOISE_OFFSETOUT);
	point1.w = distMin1;

	point2.xyz = normalize(point2.xyz + cell + NOISE_OFFSETOUT);
	point2.w = distMin2;
}

float4 Cell2NoiseVecSphere(float3 p, float Radius)
{
	p *= Radius;

	float3 cell = floor(p);
	float3 offs = p - cell - NOISE_OFFSET;
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

	ppoint = normalize(ppoint + cell + NOISE_OFFSETOUT);

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
				rnd = NoiseRandomUVec4((cell + d)).xyz + d;
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
				rnd = NoiseRandomUVec4((cell + d));
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

	ppoint = normalize(ppoint + cell + NOISE_OFFSETOUT);

	return float4(ppoint, sqrt(distMin));
}

float4 Cell3NoiseNormalizedVec(float3 p)
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

	ppoint = normalize(normalize(ppoint) + cell + NOISE_OFFSETOUT);

	return float4(ppoint, sqrt(distMin));
}

float Cell3NoiseColor(float3 p, out float4 color)
{
	float3 cell = floor(p);
	float3 offs = p - cell;
	float3 pos;
	float4 rnd;
	float4 rndM = NOISE_RND_M;
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
	float4 rndM = NOISE_RND_M;
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
	float4 rndM = NOISE_RND_M;
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
	float gain = pow(noiseLacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= pow(noiseLacunarity, -noiseH);
		amp *= gain;
	}

	return sum / 2.0;
}

float4 Cell3NoiseF0Vec(float3 p, int octaves, float amp)
{
	float3 cell = floor(p);
	float freq = 1.0;
	float sum = 0.0;
	float gain = pow(noiseLacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]);

		freq *= pow(noiseLacunarity, -noiseH);
		amp *= gain;
	}

	p = normalize(p + cell + NOISE_OFFSETOUT);

	return float4(p, sum / 2.0);
}

float Cell3NoiseF1F0(float3 p, int octaves, float amp)
{
	float freq = 1.0;
	float sum = 0.0;
	float gain = pow(noiseLacunarity, -noiseH);

	for (int i = 0; i < octaves; i++)
	{
		float2 F = Cell3Noise2(p * freq) * amp;

		sum += 0.1 + sqrt(F[0]) - sqrt(F[1]);

		freq *= pow(noiseLacunarity, -noiseH);
		amp *= gain;
	}

	return sum / 2.0;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Spherical Fibonacci Mapping
// http://lgdv.cs.fau.de/uploads/publications/spherical_fibonacci_mapping.pdf
// Optimized [WIP] by zameran.
float2 inverseSF(float3 p, float n) 
{
	float m = 1.0 - 1.0 / n;
	float phi = min(atan2(p.y, p.x), M_PI), cosTheta = p.z;
	float k = max(2, floor(log(n * M_PI * M_SQRT5 * (1 - cosTheta * cosTheta)) / log(M_PHI * M_PHI)));
	float Fk = pow(M_PHI, k) / M_SQRT5;
	float2 F = float2(round(Fk), round(Fk * M_PHI));

	float2x2 B = float2x2(M_PI2 * madfrac(F.x + 1, M_PHI - 1) - M_PI2 * (M_PHI - 1), M_PI2 * madfrac(F.y + 1, M_PHI - 1) - M_PI2 * (M_PHI - 1), -2 * F.x / n, -2 * F.y / n);
	float2x2 invB = Inverse(B);

	float2 c = floor(mul(invB, float2(phi, cosTheta - m)));

	float d = 8.0; 
	float j = 0;

	for (uint s = 0; s < 4; ++s) 
	{
		float cosTheta = dot(B[1], float2(s % 2, s / 2) + c) + m;

		cosTheta = clamp(cosTheta, -1.0, 1.0) * 2.0 - cosTheta;

		float i = floor(n * 0.5 - cosTheta * n * 0.5);
		float phi = M_PI2 * madfrac(i, M_PHI - 1.0);

		cosTheta = 1.0 - (2.0 * i + 1.0) * rcp(n);

		float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
		float3 q = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
		float squaredDistance = dot(q - p, q - p);

		if (squaredDistance < d) 
		{
			d = squaredDistance;
			j = i;
		}
	}

	return float2(j, sqrt(d));
}

float2 inverseSF(float3 p, float n, out float3 NearestPoint) 
{
	float m = 1.0 - 1.0 / n;
	float phi = min(atan2(p.y, p.x), M_PI), cosTheta = p.z;
	float k = max(2, floor(log(n * M_PI * M_SQRT5 * (1 - cosTheta * cosTheta)) / log(M_PHI * M_PHI)));
	float Fk = pow(M_PHI, k) / M_SQRT5;
	float2 F = float2(round(Fk), round(Fk * M_PHI));

	float2x2 B = float2x2(M_PI2 * madfrac(F.x + 1, M_PHI - 1) - M_PI2 * (M_PHI - 1), M_PI2 * madfrac(F.y + 1, M_PHI - 1) - M_PI2 * (M_PHI - 1), -2 * F.x / n, -2 * F.y / n);
	float2x2 invB = Inverse(B);

	float2 c = floor(mul(invB, float2(phi, cosTheta - m)));

	float d = 8.0; 
	float j = 0;

	for (uint s = 0; s < 4; ++s) 
	{
		float cosTheta = dot(B[1], float2(s % 2, s / 2) + c) + m;

		cosTheta = clamp(cosTheta, -1.0, 1.0) * 2.0 - cosTheta;

		float i = floor(n * 0.5 - cosTheta * n * 0.5);
		float phi = M_PI2 * madfrac(i, M_PHI - 1.0);

		cosTheta = 1.0 - (2.0 * i + 1.0) * rcp(n);

		float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
		float3 q = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
		float squaredDistance = dot(q - p, q - p);

		if (squaredDistance < d) 
		{
			NearestPoint = q;
			d = squaredDistance;
			j = i;
		}
	}

	return float2(j, sqrt(d));
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float radPeak;
float radInner;
float radRim;
float radOuter;
float heightFloor;
float heightPeak;
float heightRim;
float craterSphereRadius;
float craterRoundDist;
float craterDistortion;
float4 craterRaysColor;

float CraterHeightFunc(float lastlastLand, float lastLand, float height, float r)
{
	float distHeight = craterDistortion * height;

	float t = 1.0 - r / radPeak;
	float peak = heightPeak * craterDistortion * smoothstep(0.0, 1.0, t);

	t = smoothstep(0.0, 1.0, (r - radInner) / (radRim - radInner));
	float inoutMask = t * t * t;
	float innerRim = heightRim * distHeight * smoothstep(0.0, 1.0, inoutMask);

	t = smoothstep(0.0, 1.0, (radOuter - r) / (radOuter - radRim));
	float outerRim = distHeight * lerp(0.05, heightRim, t * t);

	t = saturate((1.0 - r) / (1.0 - radOuter));
	float halo = 0.05 * distHeight * t;

	return lerp(lastlastLand + height * heightFloor + peak + innerRim, lastLand + outerRim + halo, inoutMask);
}

float CraterNoise(float3 ppoint, float cratMagn, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	//craterSphereRadius = cratFreq * cratSqrtDensity;
	//ppoint *= craterSphereRadius;
	ppoint = (ppoint * cratFreq + Randomize) * cratSqrtDensity;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float lastlastlastLand = 0.0;
	float amplitude = 1.0;
	float cell;
	float radFactor = 1.0 / cratSqrtDensity;

	// Craters roundness distortion
	noiseH           = 0.5;
	noiseLacunarity  = 2.218281828459;
	noiseOffset      = 0.8;
	noiseOctaves     = 3;
	craterDistortion = 1.0;
	craterRoundDist  = 0.03;

	radPeak  = 0.03;
	radInner = 0.15;
	radRim   = 0.2;
	radOuter = 0.8;

	for (int i = 0; i < cratOctaves; i++)
	{
		lastlastlastLand = lastlastLand;
		lastlastLand = lastLand;
		lastLand = newLand;

		//float3 dist = craterRoundDist * Fbm3D(ppoint*2.56);
		//cell = Cell2NoiseSphere(ppoint + dist, craterSphereRadius, dist).w;
		//craterSphereRadius *= 1.83;
		cell = Cell3Noise(ppoint + craterRoundDist * Fbm3D(ppoint * 2.56));
		newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell * radFactor);

		//cell = inverseSF(ppoint + 0.2 * craterRoundDist * Fbm3D(ppoint*2.56), fibFreq);
		//rad = hash1(cell.x * 743.1) * 0.9 + 0.1;
		//newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell.y * radFactor / rad);
		//fibFreq   *= 1.81818182;
		//radFactor *= 1.3483997256; // = sqrt(1.81818182)

		if (cratOctaves > 1)
		{
			ppoint       *= 1.81818182;
			amplitude    *= 0.55;
			heightPeak   *= 0.25;
			heightFloor  *= 1.2;
			radInner     *= 0.60;
		}
	}

	return cratMagn * newLand;
}

float RayedCraterColorFunc(float r, float fi, float rnd)
{
	float t = saturate((radOuter - r) / (radOuter - radRim));
	float d4 = NoiseU(float3(70.3 * fi, rnd, rnd));

	d4 *= d4;
	d4 *= d4;

	float d16 = d4 * d4;

	d16 *= d16;

	return sqrt(t) * pow(saturate(/*0.001 * t * t +*/ d16 + 1.0 - smoothstep(d4, d4 + 0.75, r)), 2.5);
}

float RayedCraterNoise(float3 ppoint, float cratMagn, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	float3 rotVec = normalize(Randomize);

	// Craters roundness distortion
	noiseH           = 0.5;
	noiseLacunarity  = 2.218281828459;
	noiseOffset      = 0.8;
	noiseOctaves     = 3;
	craterDistortion = 1.0;
	craterRoundDist  = 0.03;

	float shapeDist = 1.0 + 0.5 * craterRoundDist * Fbm(ppoint * 419.54);

	radPeak  = 0.002;
	radInner = 0.015;
	radRim   = 0.03;
	radOuter = 0.8;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float lastlastlastLand = 0.0;
	float amplitude = 1.0;
	float2 cell;
	float rad;
	float radFactor = shapeDist / cratSqrtDensity;
	float fibFreq = 2.0 * cratFreq;

	for (int i=0; i<cratOctaves; i++)
	{
		lastlastlastLand = lastlastLand;
		lastlastLand = lastLand;
		lastLand = newLand;

		//cell = Cell2NoiseSphere(ppoint, craterSphereRadius).w;
		////cell = Cell2NoiseVec(ppoint * craterSphereRadius, 1.0).w;
		//newLand = CraterHeightFunc(0.0, lastLand, amplitude, cell * radFactor);

		cell    = inverseSF(ppoint, fibFreq);
		rad     = hash1(cell.x * 743.1) * 0.9 + 0.1;
		newLand = CraterHeightFunc(lastlastlastLand, lastLand, amplitude, cell.y * radFactor / rad);

		if (cratOctaves > 1)
		{
			ppoint = Rotate(M_PI2 * hash1(float(i)), rotVec, ppoint);
			fibFreq     *= 1.81818182;
			radFactor   *= 1.3483997256; // = sqrt(1.81818182)
			amplitude   *= 0.55;
			heightPeak  *= 0.25;
			heightFloor *= 1.2;
			radInner    *= 0.6;
		}
	}

	return cratMagn * newLand;
}

//-----------------------------------------------------------------------------

float RayedCraterColorNoise(float3 ppoint, float cratFreq, float cratSqrtDensity, float cratOctaves)
{
	float3 binormal = normalize(float3(-ppoint.z, 0.0, ppoint.x)); // = normalize(cross(ppoint, float3(0, 1, 0)));
	float3 rotVec = normalize(Randomize);

	// Craters roundness distortion
	noiseH           = 0.5;
	noiseLacunarity  = 2.218281828459;
	noiseOffset      = 0.8;
	noiseOctaves     = 3;
	craterDistortion = 1.0;
	craterRoundDist  = 0.03;

	float shapeDist = 1.0 + 0.5 * craterRoundDist * Fbm(ppoint * 419.54);
	float colorDist = 1.0 - 0.2 * Fbm(ppoint * 4315.16);

	float color = 0.0;
	float fi;
	float2  cell;
	float3  cellCenter = float3(0.0, 0.0, 0.0);
	float rad;
	float radFactor = shapeDist / cratSqrtDensity;
	float fibFreq = 2.0 * cratFreq;

	heightFloor = -0.5;
	heightPeak  = 0.6;
	heightRim   = 1.0;
	radPeak     = 0.002;
	radInner    = 0.015;
	radRim      = 0.03;
	radOuter    = 0.8;

	for (int i = 0; i < cratOctaves; i++)
	{
		//cell = Cell2NoiseSphere(ppoint, craterSphereRadius);
		////cell = Cell2NoiseVec(ppoint * craterSphereRadius, 1.0);
		//fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (pi*2.0);
		//color += vary * RayedCraterColorFunc(cell.w * radFactor, fi, 48.3 * dot(cell.xyz, Randomize));
		//radInner  *= 0.6;

		cell = inverseSF(ppoint, fibFreq, cellCenter);
		rad  = hash1(cell.x * 743.1) * 0.9 + 0.1;
		fi   = acos(dot(binormal, normalize(cellCenter - ppoint))) / (M_PI2 * 2.0);
		color += RayedCraterColorFunc(cell.y * radFactor / rad, fi, 48.3 * dot(cellCenter, Randomize));

		if (cratOctaves > 1)
		{
			ppoint = Rotate(M_PI2 * hash1(float(i)), rotVec, ppoint);
			fibFreq   *= 1.81818182;
			radFactor *= 1.3483997256; // = sqrt(1.81818182)
			radInner  *= 0.6;
		}
	}

	return color * colorDist;
}

float VolcanoRidges(float r, float fi1, float fi2, float rnd)
{
	float ridges1 = iqTurbulence(float3(fi1, r, rnd + 1.126), 0.55);
	float ridges2 = iqTurbulence(float3(fi2, r, rnd + 0.754), 0.55);

	return ridges1 * saturate(abs(fi2)) + ridges2 * saturate(abs(fi1));
}

float VolcanoRidges(float r, float fi1, float fi2, float rnd, int oct)
{
	float ridges1 = iqTurbulence2(float3(fi1, r, rnd + 1.126), 0.55, oct);
	float ridges2 = iqTurbulence2(float3(fi2, r, rnd + 0.754), 0.55, oct);

	return ridges1 * saturate(abs(fi2)) + ridges2 * saturate(abs(fi1));
}

float VolcanoHeightFunc(float r, float fi1, float fi2, float rnd, float size)
{
	float rs = 0.25 * r / size;
	float shape  = saturate(2.0 * size);
	float height = 0.75 + 0.75 * shape * shape;

	float cone = saturate(1.0 - pow(r, 0.5 + 0.5 * shape));

	const float calderaRadius = 0.14;
	float t = rs * (10.0 / calderaRadius) - 2.5;
	float caldera = 0.85 + lerp(0.07, 0.025, shape);
	float calderaMask = smoothstep(0.0, 1.0, t);

	noiseOctaves = 8;
	float ridges = VolcanoRidges(rs, fi1, fi2, rnd);
	cone += lerp(0.02, 0.06, shape) * saturate(2.0 * sqrt(r)) * ridges;

	return height * lerp(caldera, cone, calderaMask);
}

float VolcanoGlowFunc(float r, float fi1, float fi2, float rnd, float size)
{
	float rs = 0.25 * r / size;

	const float calderaRadius = 0.14;
	float t = rs * (10.0 / calderaRadius) - 2.5;
	float caldera = saturate(1.0 - t);

	noiseOctaves = 8;
	float ridges = VolcanoRidges(rs, fi1, fi2, rnd, 5);

	float flows = smoothstep(0.225, 0.15, rs) * pow(saturate(ridges + volcanoFlows - 1.0), 0.5);

	return max(caldera, flows);
}

float VolcanoNoise(float3 ppoint, float globalLand, float localLand)
{
	noiseLacunarity = 2.218281828459;
	noiseH          = 0.5;
	noiseOffset     = 0.8;

	float  frequency = 150.0 * volcanoFreq;
	float  density   = volcanoDensity;
	float  size      = volcanoRadius;
	float  newLand   = localLand;
	float  globLand  = globalLand - 1.0;
	float  amplitude = 2.0 * volcanoMagn;
	float2 cell;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float3 rotVec   = normalize(Randomize);
	float3 binormal = normalize(float3(-ppoint.z, 0.0, ppoint.x)); // = normalize(cross(ppoint, float3(0, 1, 0)));
	float  distFreq = 18.361 * volcanoFreq;
	float  distMagn = 0.003;

	for (int i = 0; i < volcanoOctaves; i++)
	{
		float3 p = ppoint + distMagn * Fbm3D(ppoint * distFreq, 4);

		cell = inverseSF(p, frequency, cellCenter);

		float h = hash1(cell.x);
		float r = 40.0 * cell.y;

		if ((h < density) && (r < 1.0))
		{
			float rnd = 48.3 * dot(cellCenter, Randomize);
			float3 cen = normalize(cellCenter - p);
			float a   = dot(p, cross(cen, binormal));
			float b   = dot(cen, binormal);
			float fi1 = atan2( a,  b) / M_PI;
			float fi2 = atan2(-a, -b) / M_PI;

			float volcano = globLand + amplitude * VolcanoHeightFunc(r, fi1, fi2, rnd, size);
			newLand = softExpMaxMin(newLand, volcano, 32);
		}

		if (volcanoOctaves > 1)
		{
			ppoint = Rotate(M_PI2 * hash1(float(i)), rotVec, ppoint);
			frequency *= 2.0;
			//density   *= 2.0;
			size      *= 0.5;
			amplitude *= 1.2;
			distFreq  *= 2.0;
			distMagn  *= 0.5;
		}
	}

	return newLand;
}

float2 VolcanoGlowNoise(float3 ppoint)
{
	noiseLacunarity = 2.218281828459;
	noiseH          = 0.5;
	noiseOffset     = 0.8;

	float  frequency = 150.0 * volcanoFreq;
	float  density   = volcanoDensity;
	float  size      = volcanoRadius;
	float2 volcTempMask = float2(0.0, 0.0);
	float2 cell;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float3 rotVec   = normalize(Randomize);
	float3 binormal = normalize(float3(-ppoint.z, 0.0, ppoint.x)); // = normalize(cross(ppoint, float3(0, 1, 0)));
	float  distFreq = 18.361 * volcanoFreq;
	float  distMagn = 0.003;

	for (int i=0; i<volcanoOctaves; i++)
	{
		float3 p = ppoint + distMagn * Fbm3D(ppoint * distFreq, 4);

		cell = inverseSF(p, frequency, cellCenter);

		float h = hash1(cell.x);
		float r = 40.0 * cell.y;

		if ((h < density) && (r < 1.0))
		{
			float rnd = 48.3 * dot(cellCenter, Randomize);
			float3 cen = normalize(cellCenter - p);
			float a   = dot(p, cross(cen, binormal));
			float b   = dot(cen, binormal);
			float fi1 = atan2( a,  b) / M_PI;
			float fi2 = atan2(-a, -b) / M_PI;

			volcTempMask = max(volcTempMask, float2(1.2 * VolcanoGlowFunc(r, fi1, fi2, rnd, size), 1.0 - 2.0 * r));
		}

		if (volcanoOctaves > 1)
		{
			ppoint = Rotate(M_PI2 * hash1(float(i)), rotVec, ppoint);
			frequency *= 2.0;
			size      *= 0.5;
			distFreq  *= 2.0;
			distMagn  *= 0.5;
		}
	}

	return volcTempMask;
}

float MareHeightFunc(float lastLand, float lastlastLand, float height, float r, out float mareFloor)
{
	float t;

	if (r < radInner) // Crater bottom
	{  
		mareFloor = 1.0;

		return lastlastLand + height * heightFloor;
	}
	else if (r < radRim) // Inner rim
	{   
		t = (r - radInner) / (radRim - radInner);
		t = smoothstep(0.0, 1.0, t);

		mareFloor = 1.0 - t;

		return lerp(lastlastLand + height * heightFloor, lastLand + height * heightRim * craterDistortion, t);
	}
	else if (r < radOuter) // Outer rim
	{  
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

float CrackHeightFunc(float lastLand, float lastlastLand, float height, float r, float3 p)
{
	p.x += 0.05 * r;

	float inner = smoothstep(0.0, 0.5, r);
	float outer = smoothstep(0.5, 1.0, r);
	float cracks = height * (0.4 * Noise(p * 625.7) * (1.0 - inner) + inner * (1.0 - outer));
	float land = lerp(lastLand, lastlastLand, r);

	return lerp(cracks, land, outer);
}

float CrackNoise(float3 ppoint, out float mask)
{
	ppoint = (ppoint + Randomize) * cracksFreq;

	float newLand = 0.0;
	float lastLand = 0.0;
	float lastlastLand = 0.0;
	float2 cell;
	float r;
	float ampl = 0.4 * cracksMagn;

	mask = 1.0;

	// Rim shape and height distortion
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;
	noiseOctaves    = 6.0;

	for (int i = 0; i < cracksOctaves; i++)
	{
		cell = Cell2Noise2(ppoint + 0.02 * Fbm3D(1.8 * ppoint));
		r    = smoothstep(0.0, 1.0, 250.0 * abs(cell.y - cell.x));
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CrackHeightFunc(lastlastLand, lastLand, ampl, r, ppoint);
		ppoint = ppoint * 1.2 + Randomize;
		ampl *= 0.8333;
		mask *= smoothstep(0.6, 1.0, r);
	}

	return newLand;
}

float CrackColorNoise(float3 ppoint, out float mask)
{
	ppoint = (ppoint + Randomize) * cracksFreq;

	float  newLand = 0.0;
	float  lastLand = 0.0;
	float  lastlastLand = 0.0;
	float2 cell;
	float  r;

	// Rim height and shape distortion
	noiseH          = 0.5;
	noiseLacunarity = 2.218281828459;
	noiseOffset     = 0.8;
	noiseOctaves    = 6.0;
	mask = 1.0;

	for (int i = 0; i < cracksOctaves; i++)
	{
		cell = Cell2Noise2(ppoint + 0.02 * Fbm3D(1.8 * ppoint));
		r    = smoothstep(0.0, 1.0, 250.0 * abs(cell.y - cell.x));
		lastlastLand = lastLand;
		lastLand = newLand;
		newLand = CrackHeightFunc(lastlastLand, lastLand, 1.0, r, ppoint);
		ppoint = ppoint * 1.2 + Randomize;
		mask *= smoothstep(0.6, 1.0, r);
	}

	return pow(saturate(1.0 - newLand), 2.0);
}

float DunesNoise(float3 ppoint, float octaves)
{
	float dir = sNoise(ppoint * 3.86) * 197.3 * dunesFreq;
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
		//dist = dir + Noise(p * dunesFreq * 100.0) * 1.7;
		dist = dir + sNoise(p * dunesFreq * 25.0) * 12.7 + sNoise(p * dunesFreq * 300.0) * 1.2;
		wave = frac(dist / M_PI);
		wave = cos(M_PI * wave * wave);
		fade = smoothstep(win - 0.5 * dwin, win, glob) * (1.0 - smoothstep(win + dwin, win + 1.5 * dwin, glob));
		//dunes += (1.0 - sqrt(wave * wave + 0.005)) * ampl * fade;
		dunes += (1.0 - sqrt(wave * wave + 0.005)) * (ampl + Fbm(p * dunesFreq * 150.0) * 0.05 - 0.03) * fade;
		p = p * lac + float3(3.17, 5.38, 8.79);
		dir *= lac;
		ampl /= lac;
		win += dwin;
	}

	return dunes;
}

void SolarSpotsHeightNoise(float3 ppoint, out float botMask, out float filMask, out float filaments)
{
	float3 binormal = normalize(float3(-ppoint.z, 0.0, ppoint.x));

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
		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (M_PI * 2.0);
		rnd = 48.3 * dot(cell.xyz, Randomize);

		t = saturate((cell.w * radFactor - radInner) / (radOuter - radInner));
		botmask = smoothstep(0.0, 1.0, t);
		filmask = smoothstep(0.0, 0.1, t) * (1.0 - botmask);
		filam = NoiseU(float3(montesFreq * fi, rnd, rnd));

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
	float3 binormal =  normalize(float3(-ppoint.z, 0.0, ppoint.x));
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
		fi = acos(dot(binormal, normalize(cell.xyz - ppoint))) / (M_PI * 2.0);
		rnd = 48.3 * dot(cell.xyz, Randomize);

		t = saturate((cell.w * radFactor - radInner) / (radOuter - radInner));
		botmask = smoothstep(0.0, 0.2, t);
		filmask = (1.0 - smoothstep(0.7, 1.0, t)) * smoothstep(0.0, 0.1, t) * 0.85;
		filam   = NoiseU(float3(montesFreq * fi, rnd, rnd)) * t * 0.75;

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
float GetTerraced(float value)
{
	const float W = 0.4;

	float k = floor(value / W);
	float f = (value - k * W) / W;
	float s = min(2.0 * f, 1.0);

	return (k + s) * W;
}

float GetTerraced(float value, float terraceLayers)
{
	float height = value;
	float h = height * terraceLayers;

	height = (floor(h) + smoothstep(0.0, 1.0, frac(h))) / terraceLayers;
	height *= 0.75;

	return height;
}

float GetTerraced(float value, float n, float power)
{
	float dValue = value * n;
	float f = frac(dValue);
	float i = floor(dValue);

	return (i + pow(f, power)) / n;
}

float GetTerraced(float value, float detail, float n, float power)
{
	float dValue = value * n;
	float f = frac(dValue);
	float i = floor(dValue);

	return (i + pow(f, power)) / (n / detail);
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
// Can be used to create lava planets or icebergs
// climate-dependent: gapWidth = climate, flood = 1.5
// random:            gapWidth = 0.7,     flood = 1.0
float LithoCellsNoise(float3 ppoint, float gapWidth, float flood)
{
	float gap = saturate(1.0 - 1.0 * gapWidth);
	float2 cell;
	float3 p;
	float4 col;

	noiseOctaves = 4;
	p = ppoint * 14.2 + Randomize;
	p += 0.1 * Fbm3D(p * 0.7);

	cell = Cell3Noise2Color(p * (0.5 * gap - 0.5), col);

	float lithoCells = (1.0 - gap) * sqrt(abs(cell.y - cell.x));
	lithoCells = smoothstep(0.1 * gap, 0.6 * gap, lithoCells);
	lithoCells *= step(col.r, flood - gap);

	return lithoCells;
}

float3 TurbulenceTerra(float3 ppoint)
{
	const float scale = 0.7;

	float3 twistedPoint = ppoint;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float2 cell;
	float r, fi, rnd, dist, dist2, dir;
	float strength = 5.5;
	float freq = 20.0 * scale;
	float size = 4.0 * scale;
	float dens = 0.3;

	for (int i = 0; i < 2; i++)
	{
		cell = inverseSF(ppoint, freq, cellCenter);

		rnd = hash1(cell.x);
		r = size * cell.y;

		if ((rnd < dens) && (r < 1.0))
		{
			dir = sign(0.5 * dens - rnd);
			dist = saturate(1.0 - r);
			dist2 = saturate(0.5 - r);
			fi = pow(dist, strength) * (exp(-6.0 * dist2) + 0.25);
			twistedPoint = Rotate(dir * 15.0 * sign(cellCenter.y + 0.001) * fi, cellCenter.xyz, ppoint);
		}

		freq = min(freq * 2.0, 1600.0);
		size = min(size * 1.2, 30.0);
		strength = strength * 1.5;
		ppoint = twistedPoint;
	}

	return twistedPoint;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 CycloneNoiseTerra(float3 ppoint, inout float weight, inout float coverage)
{
	float3 rotVec = normalize(Randomize);
	float3 twistedPoint = ppoint;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float2 cell;
	float r, fi, rnd, dist, w;
	float mag = -tidalLock * cycloneMagn;
	float freq = cycloneFreq * 50.0;
	float dens = cycloneDensity * 0.02;
	float size = 8.0;

	for (int i = 0; i < cycloneOctaves; i++)
	{
		cell = inverseSF(ppoint, freq, cellCenter);
		rnd = hash1(cell.x);
		r = size * cell.y;

		if ((rnd < dens) && (r < 1.0))
		{
			dist = 1.0 - r;
			fi = lerp(log(r), dist * dist * dist, r);
			twistedPoint = Rotate(mag * sign(cellCenter.y + 0.001) * fi, cellCenter.xyz, ppoint);
			w = saturate(1.0 - r * 10.0);
			weight = min(weight, 1.0 - w * w);
			coverage = lerp(coverage, 1.0, dist);
		}

		freq *= 2.0;
		dens *= 2.0;
		size *= 2.0;
		ppoint = twistedPoint;
	}

	weight = saturate(weight);

	return twistedPoint;
}

float HeightMapCloudsTerra(float3 ppoint)
{
	float zones = cos(ppoint.y * stripeZones);
	float ang = zones * stripeTwist;
	float3 twistedPoint = ppoint;
	float coverage = cloudsCoverage;
	float weight = 1.0;
	float offset = 0.0;

	// Compute the cyclons
	if (tidalLock > 0.0)
	{
		float3 cycloneCenter = float3(0.0, 1.0, 0.0);
		float r = length(cycloneCenter - ppoint);
		float mag = -tidalLock * cycloneMagn;

		if (r < 1.0)
		{
			float dist = 1.0 - r;
			float fi = lerp(log(r), dist * dist * dist, r);
			twistedPoint = Rotate(mag * fi, cycloneCenter, ppoint);
			weight = saturate(r * 40.0 - 0.05);
			weight = weight * weight;
			coverage = lerp(coverage, 1.0, dist);
		}

		weight *= smoothstep(-0.2, 0.0, ppoint.y);   // Surpress clouds on a night side
	}
	else
		twistedPoint = CycloneNoiseTerra(ppoint, weight, coverage);

	// Compute turbulence
	twistedPoint = TurbulenceTerra(twistedPoint);

	// Compute the Coriolis effect
	float sina = sin(ang);
	float cosa = cos(ang);

	twistedPoint = float3(cosa * twistedPoint.x - sina * twistedPoint.z, twistedPoint.y, sina * twistedPoint.x + cosa * twistedPoint.z);
	twistedPoint = twistedPoint * cloudsFreq + Randomize;

	// Compute the flow-like distortion
	float3 p = twistedPoint * cloudsFreq * 6.37;
	float3 q = p + FbmClouds3D(p);
	float3 r = p + FbmClouds3D(q);
	float f = FbmClouds(r) * 0.7 + coverage - 0.3;
	float global = saturate(f) * weight;

	return global;
}

float3 TurbulenceGasGiant(float3 ppoint)
{
	const float scale = 0.7;

	float3 twistedPoint = ppoint;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float2 cell;
	float r, fi, rnd, dist, dist2, dir;
	float strength = 5.5;
	float freq = 800 * scale;
	float size = 15.0 * scale;
	float dens = 0.8;

	for (int i = 0; i < 5; i++)
	{
		cell = inverseSF(ppoint, freq, cellCenter);
		rnd = hash1(cell.x);
		r = size * cell.y;

		if ((rnd < dens) && (r < 1.0))
		{
			dir = sign(0.5 * dens - rnd);
			dist = saturate(1.0 - r);
			dist2 = saturate(0.5 - r);
			fi = pow(dist, strength) * (exp(-6.0 * dist2) + 0.25);
			twistedPoint = Rotate(dir * stripeTwist * sign(cellCenter.y) * fi, cellCenter.xyz, ppoint);
		}

		freq = min(freq * 2.0, 1600.0);
		size = min(size * 1.2, 30.0);
		strength = strength * 1.5;
		ppoint = twistedPoint;
	}

	return twistedPoint;
}

float3 CycloneNoiseGasGiant(float3 ppoint, inout float offset)
{
	float3 rotVec = normalize(Randomize);
	float3 twistedPoint = ppoint;
	float3 cellCenter = float3(0.0, 0.0, 0.0);
	float2 cell;
	float r, fi, rnd, dist, dist2, dir;
	float offs = 0.6;
	float squeeze = 1.7;
	float strength = 2.5;
	float freq = cycloneFreq * 50.0;
	float dens = cycloneDensity * 0.02;
	float size = 6.0;

	for (int i = 0; i < cycloneOctaves; i++)
	{
		cell = inverseSF(float3(ppoint.x, ppoint.y * squeeze, ppoint.z), freq, cellCenter);
		rnd = hash1(cell.x);
		r = size * cell.y;

		if ((rnd < dens) && (r < 1.0))
		{
			dir = sign(0.7 * dens - rnd);
			dist = saturate(1.0 - r);
			dist2 = saturate(0.5 - r);
			fi = pow(dist, strength) * (exp(-6.0 * dist2) + 0.5);
			twistedPoint = Rotate(cycloneMagn * dir * sign(cellCenter.y + 0.001) * fi, cellCenter.xyz, ppoint);
			offset += offs * fi * dir;
		}

		freq = min(freq * 2.0, 6400.0);
		dens = min(dens * 3.5, 0.3);
		size = min(size * 1.5, 15.0);
		offs = offs * 0.85;
		squeeze = max(squeeze - 0.3, 1.0);
		strength = max(strength * 1.3, 0.5);
		ppoint = twistedPoint;
	}

	return twistedPoint;
}

float HeightMapCloudsGasGiantCore(float3 ppoint)
{
	float3 twistedPoint = ppoint;

	// Compute zones
	float zones = Noise(float3(0.0, twistedPoint.y * stripeZones * 0.5, 0.0)) * 0.6 + 0.25;
	float offset = 0.0;

	// Compute cyclons
	if (cycloneOctaves > 0.0)
		twistedPoint = CycloneNoiseGasGiant(twistedPoint, offset);

	// Compute turbulence
	twistedPoint = TurbulenceGasGiant(twistedPoint);

	// Compute stripes
	noiseOctaves = cloudsOctaves;
	float turbulence = Fbm(twistedPoint * 0.2);
	twistedPoint = twistedPoint * (0.05 * cloudsFreq) + Randomize;
	twistedPoint.y *= 100.0 + turbulence;
	float height = stripeFluct * (Fbm(twistedPoint) * 0.7 + 0.5);

	return zones + height + offset;
}
//-----------------------------------------------------------------------------