// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

#define SPACESTUFF

#define SHADOW_BLUR

#if !defined (MATH)
#include "Math.cginc"
#endif

//----------------------------------------------------------------------------
#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
float4 _Light1Color;
float4 _Light1Position;
float3 _Light1Direction;

#if LIGHT_2
float4 _Light2Color;
float4 _Light2Position;
float3 _Light2Direction;
#endif

#if LIGHT_3
float4 _Light3Color;
float4 _Light3Position;
float3 _Light3Direction;
#endif

#if LIGHT_4
float4 _Light4Color;
float4 _Light4Position;
float3 _Light4Direction;
#endif

float ComputeMiePhase(float _CosTheta, float _MiePhaseAnisotropy)
{
	float squareAniso = _MiePhaseAnisotropy * _MiePhaseAnisotropy;

	float Num = 1.5 * (1.0 + _CosTheta * _CosTheta) * (1.0 - squareAniso);
	float Den = (8.0 + squareAniso) * pow( abs(1.0 + squareAniso - 2.0 * _MiePhaseAnisotropy * _CosTheta), 1.5 );
	
	return Num / Den;
}

float MiePhase(float angle, float4 mie)
{
	return ComputeMiePhase(mie.y, mie.y) / pow(mie.z - mie.x * angle, mie.w);
}
#endif
//----------------------------------------------------------------------------

//----------------------------------------------------------------------------
#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
float4x4  _Shadow1Matrix;
sampler2D _Shadow1Texture;
float     _Shadow1Ratio;

#if SHADOW_2
float4x4  _Shadow2Matrix;
sampler2D _Shadow2Texture;
float     _Shadow2Ratio;
#endif

#if SHADOW_3
float4x4  _Shadow3Matrix;
sampler2D _Shadow3Texture;
float     _Shadow3Ratio;
#endif

#if SHADOW_4
float4x4  _Shadow4Matrix;
sampler2D _Shadow4Texture;
float     _Shadow4Ratio;
#endif

float4 ShadowColor(float4x4 shadowMatrix, sampler2D shadowSampler, float shadowRatio,  float4 worldPoint)
{
	float4 shadowPoint = mul(shadowMatrix, worldPoint);
	float4 shadow = 0;
	float2 shadowMag = length(shadowPoint.xy);
	
	shadowMag = 1.0f - (1.0f - shadowMag) * shadowRatio;

	#ifdef SHADOW_BLUR
		shadow = Blur(shadowSampler, shadowMag, 0.00015f);
	#else
		shadow = tex2D(shadowSampler, shadowMag.xy);
	#endif
	
	shadow += shadowPoint.z < 0.0f ? 1.0f : 0.0f;
	shadow = saturate(shadow);

	return shadow;
}

float4 ShadowColor(float4 worldPoint)
{
	float4 color = ShadowColor(_Shadow1Matrix, _Shadow1Texture, _Shadow1Ratio, worldPoint);
	
	#if SHADOW_2
		color *= ShadowColor(_Shadow2Matrix, _Shadow2Texture, _Shadow2Ratio, worldPoint);
	#endif

	#if SHADOW_3
		color *= ShadowColor(_Shadow3Matrix, _Shadow3Texture, _Shadow3Ratio, worldPoint);
	#endif

	#if SHADOW_4
		color *= ShadowColor(_Shadow4Matrix, _Shadow4Texture, _Shadow4Ratio, worldPoint);
	#endif
	
	color.a = 1.0;

	return color;
}

float4 ShadowOuterColor(float3 d, float3 WCP, float3 origin, float Rt)
{
	float interSectPt = IntersectOuterSphereInverted(WCP, d, origin, Rt);

	return interSectPt != -1.0 ? ShadowColor(float4(WCP + d * interSectPt, 1.0)) : 1.0;
}
#endif
//----------------------------------------------------------------------------

//----------------------------------------------------------------------------
struct QuadGenerationConstants //For every quad
{
	float planetRadius;
	float spacing;
	float spacingFull;
	float terrainMaxHeight;
	float lodLevel;
	float lodOctaveModifier;

	float4 meshSettings;

	float2 borderMod;

	float3 cubeFaceEastDirection;
	float3 cubeFaceNorthDirection;
	float3 patchCubeCenter;
};

struct OutputStruct
{
	float noise;

	float3 patchCenter;

	float4 position;
	float4 cubePosition;
};

struct QuadCorners
{
	float4 topLeftCorner;
	float4 topRightCorner;
	float4 bottomLeftCorner;
	float4 bottomRightCorner;
};
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float DenormalizeMaximumHeight(float noiseValue, float terrainMaxHeight)
{
	return (noiseValue * terrainMaxHeight + terrainMaxHeight) - (terrainMaxHeight * 1.35f);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 FindBiTangent(float3 normal, float epsilon, float3 dir)
{
	float refVectorSign = sign(1.0 - abs(normal.x) - epsilon);

	float3 refVector = refVectorSign * dir;
	float3 biTangent = refVectorSign * cross(normal, refVector);

	return biTangent;
}

float3 FindTangent(float3 normal, float epsilon, float3 dir)
{
	float refVectorSign = sign(1.0 - abs(normal.x) - epsilon);

	float3 refVector = refVectorSign * dir;
	float3 biTangent = refVectorSign * cross(normal, refVector);

	return cross(-normal, biTangent);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 CubeCoord(QuadGenerationConstants constants, float verticesPerSide, uint3 id, int mod, float spacing)
{
	//32 : 1;     x = 32; y = 32;   z0 = y / x + 0 + 0;
	//64 : 3;     x = 32; y = 64;   z1 = y / x + z0 + 0;
	//128 : 7;    x = 32; y = 128;  z2 = y / x + z1 + 0;
	//256 : 17;   x = 32; y = 256;  z3 = y / x + z2 + 2; = 17
	//512 : 41;   x = 32; y = 512;  z4 = y / x + z3 + 8; = 41
	//1024 : 105; x = 32; y = 1024; z5 = y / x + z4 + 32; = 105

	//Ok i figured out, that offset for 256 is wrong, but 15 is good. So. New table looks like:
	//32 : 1
	//64 : 3
	//128 : 7
	//256 : 15
	//512 : 31

	//NOTE: The mod formula is:
	//(SIZE / 16) - 1; Where SIZE is PoT [Power Of Two]

	float eastValue = (id.x - ((verticesPerSide - mod) * 0.5)) * spacing;
	float northValue = (id.y - ((verticesPerSide - mod) * 0.5)) * spacing;

	float3 cubeCoordEast = constants.cubeFaceEastDirection * eastValue;
	float3 cubeCoordNorth = constants.cubeFaceNorthDirection * northValue;

	float3 cubeCoord = cubeCoordEast + cubeCoordNorth + constants.patchCubeCenter;

	return cubeCoord;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Project the surface gradient (dhdx, dhdy) onto the surface (n, dpdx, dpdy)
float3 CalculateSurfaceGradient(float3 n, float3 dpdx, float3 dpdy, float dhdx, float dhdy) 
{
	float3 r1 = cross(dpdy, n);
	float3 r2 = cross(n, dpdx);
  
	return (r1 * dhdx + r2 * dhdy) / dot(dpdx, r1);
}
 
// Move the normal away from the surface normal in the opposite surface gradient direction
float3 PerturbNormal(float3 normal, float3 dpdx, float3 dpdy, float dhdx, float dhdy) 
{
	return normalize(normal - CalculateSurfaceGradient(normal, dpdx, dpdy, dhdx, dhdy));
}

// Calculate the surface normal using screen-space partial derivatives of the height field
float3 CalculateSurfaceNormal_HeightMap(float3 position, float3 normal, float height)
{
	float3 dpdx = ddx_fine(position);
	float3 dpdy = ddy_fine(position);
		   
	float dhdx = ddx_fine(height);
	float dhdy = ddy_fine(height);
  
	return PerturbNormal(normal, dpdx, dpdy, dhdx, dhdy);
}

inline float3x3 CotangentFrame(float3 N, float3 p, float2 uv)
{
	// get edge vectors of the pixel triangle
	float3 dp1 = ddx_fine(p);
	float3 dp2 = ddx_fine(p);

	float2 duv1 = ddx_fine(uv);
	float2 duv2 = ddx_fine(uv);
 
	// solve the linear system
	float3 dp2perp = cross(dp2, N);
	float3 dp1perp = cross(N, dp1);
	float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	float3 B = dp2perp * duv1.y + dp1perp * duv2.y;
 
	// construct a scale-invariant frame 
	float invmax = rsqrt(max(dot(T, T), dot(B, B)));

	return float3x3(T * invmax, B * invmax, N);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float GetSlope(float3 normal)
{
	return 0.5 * max(dot(normal, float3(0.0, 1.0, 0.0)), 0.001);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#if defined (COMPUTE_SHADER)
inline float3 GetSobelNormal(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	//float normalStrength = 0.5 / ((constants.lodLevel / 20.0 + 1.0) * (constants.lodLevel / 20.0 + 1.0));
	
	float tl = buffer[(id.x + 0) + (id.y + 0) * size].noise;// * constants.lodLevel;
	float  l = buffer[(id.x + 0) + (id.y + 1) * size].noise;// * constants.lodLevel;
	float bl = buffer[(id.x + 0) + (id.y + 2) * size].noise;// * constants.lodLevel;
	float  t = buffer[(id.x + 1) + (id.y + 0) * size].noise;// * constants.lodLevel;
	float  b = buffer[(id.x + 1) + (id.y + 2) * size].noise;// * constants.lodLevel;
	float tr = buffer[(id.x + 2) + (id.y + 0) * size].noise;// * constants.lodLevel;
	float  r = buffer[(id.x + 2) + (id.y + 1) * size].noise;// * constants.lodLevel;
	float br = buffer[(id.x + 2) + (id.y + 2) * size].noise;// * constants.lodLevel;

	float xdelta = tr + 2.0 * r + br - tl - 2.0 * l - bl;
	float ydelta = bl + 2.0 * b + br - tl - 2.0 * t - tr;

	float3 normal = normalize(float3(xdelta, ydelta, 2.0 * 1));

	return normal;
}

inline float3 GetHeightNormal(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id, out float slope)
{
	float left  = buffer[(id.x + 0) + (id.y + 1) * size].noise * constants.lodLevel;
	float right = buffer[(id.x + 2) + (id.y + 1) * size].noise * constants.lodLevel;
	float up    = buffer[(id.x + 1) + (id.y + 0) * size].noise * constants.lodLevel;
	float down  = buffer[(id.x + 1) + (id.y + 2) * size].noise * constants.lodLevel;

	float xdelta = ((left - right) + 1.0) * 0.5;
	float ydelta = ((up - down) + 1.0) * 0.5;
	float zdelta = ((right - left) + 1.0) * 0.5;
	float wdelta = ((up - down) + 1.0) * 0.5;

	float3 xnormal = normalize(float3(xdelta, ydelta, 1.0));
	float3 ynormal = normalize(float3(ydelta, xdelta, 1.0));
	float3 znormal = normalize(float3(zdelta, wdelta, 1.0));
	float3 wnormal = normalize(float3(wdelta, zdelta, 1.0));

	float finalSlope = min(min(GetSlope(xnormal), 
							   GetSlope(ynormal)), 
						   min(GetSlope(znormal), 
							   GetSlope(wnormal)));
	
	slope = finalSlope;

	return xnormal;
}

inline float3 GetHeightNormalFromPosition(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	float3 left	 = (buffer[(id.x + 0) + (id.y + 1) * size].position.xyz);
	float3 right = (buffer[(id.x + 2) + (id.y + 1) * size].position.xyz);
	float3 up	 = (buffer[(id.x + 1) + (id.y + 0) * size].position.xyz);
	float3 down  = (buffer[(id.x + 1) + (id.y + 2) * size].position.xyz);
	float3 curr	 = (buffer[(id.x + 1) + (id.y + 1) * size].position.xyz);

	float3 n = cross(curr - left, curr - up) + cross(curr - right, curr - down);

	return normalize(float3(-n.x, -n.y, n.z));
	//return normalize(cross(right - curr, up - curr));
}

inline float3 GetHeightNormalFromBump(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	float left	 = buffer[(id.x + 0) + (id.y + 1) * size].noise * constants.lodLevel;
	float right  = buffer[(id.x + 2) + (id.y + 1) * size].noise * constants.lodLevel;
	float up	 = buffer[(id.x + 1) + (id.y + 0) * size].noise * constants.lodLevel;
	float down   = buffer[(id.x + 1) + (id.y + 2) * size].noise * constants.lodLevel;
	float curr	 = buffer[(id.x + 1) + (id.y + 1) * size].noise * constants.lodLevel;
	
	float3 s;

	s  = normalize(float3(up - curr, curr - left, 1.0));
	s += normalize(float3(curr - down, curr - left, 1.0));
	s += normalize(float3(curr - down, right - curr, 1.0));
	s += normalize(float3(up - curr, right - curr, 1.0));

	return normalize(s);
}

inline float3 GetPackedNormal(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	float left	 = (buffer[(id.x + 0) + (id.y + 1) * size].noise) * constants.lodLevel;
	float right  = (buffer[(id.x + 2) + (id.y + 1) * size].noise) * constants.lodLevel;
	float up	 = (buffer[(id.x + 1) + (id.y + 0) * size].noise) * constants.lodLevel;
	float down   = (buffer[(id.x + 1) + (id.y + 2) * size].noise) * constants.lodLevel;
			   
	float2 dir = float2(1.0, 0.0);
			   
	return cross(normalize(float3(dir.xy, right - left)), normalize(float3(dir.yx, down - up))).xyz; 
}
#endif
//-----------------------------------------------------------------------------