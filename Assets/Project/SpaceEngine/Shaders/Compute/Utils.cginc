/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
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

//-----------------------------------------------------------------------------
#if !defined (M_PI)
#define M_PI 3.14159265358
#endif

#if !defined (M_PI2)
#define M_PI2 6.28318531
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct PlanetGenerationConstants //For planet only //wip
{
	float planetRadius;
	float terrainMaxHeight;

	float4 meshSettings;
};

struct QuadGenerationConstants //For every quad
{
	float planetRadius;
	float spacing;
	float spacingreal;
	float spacingsub;
	float terrainMaxHeight;
	float lodLevel;
	float splitLevel;
	float lodOctaveModifier;
	float orientation;

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
float3 CubeCoord(QuadGenerationConstants constants, float VerticesPerSide, uint3 id, int mod, float spacing)
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

	//TODO: modifier calculation.

	float eastValue = (id.x - ((VerticesPerSide - mod) * 0.5)) * spacing;
	float northValue = (id.y - ((VerticesPerSide - mod) * 0.5)) * spacing;

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
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float3 GetSobelNormal(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	float normalStrength = 0.5 / ((constants.lodLevel / 20.0 + 1.0) * (constants.lodLevel / 20.0 + 1.0));
	
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

	float3 xnormal = float3(xdelta, ydelta, 1.0);
	float3 ynormal = float3(ydelta, xdelta, 1.0);
	float3 znormal = float3(zdelta, wdelta, 1.0);
	float3 wnormal = float3(wdelta, zdelta, 1.0);

	float xslope = 0.5 / max(dot(xnormal, float3(0.0, 1.0, 0.0)), 0.001);
	float yslope = 0.5 / max(dot(ynormal, float3(0.0, 1.0, 0.0)), 0.001);
	float zslope = 0.5 / max(dot(znormal, float3(0.0, 1.0, 0.0)), 0.001);
	float wslope = 0.5 / max(dot(wnormal, float3(0.0, 1.0, 0.0)), 0.001);

	float finalSlope = min(min(xslope, yslope), min(zslope, wslope));
	
	slope = finalSlope;

	return xnormal;
}

inline float3 GetHeightNormalFromPosition(QuadGenerationConstants constants, RWStructuredBuffer<OutputStruct> buffer, int size, uint3 id)
{
	float r = constants.planetRadius;

	float3 left	 = (buffer[(id.x + 0) + (id.y + 1) * size].position.xyz) / r;// * constants.lodLevel;
	float3 right = (buffer[(id.x + 2) + (id.y + 1) * size].position.xyz) / r;// * constants.lodLevel;
	float3 up	 = (buffer[(id.x + 1) + (id.y + 0) * size].position.xyz) / r;// * constants.lodLevel;
	float3 down  = (buffer[(id.x + 1) + (id.y + 2) * size].position.xyz) / r;// * constants.lodLevel;
	float3 curr	 = (buffer[(id.x + 1) + (id.y + 1) * size].position.xyz) / r;// * constants.lodLevel;
	
	float3 e0 = curr - left;
	float3 e1 = curr - right;
	float3 e2 = curr - up;
	float3 e3 = curr - down;

	float3 n0 = cross(e0, e2);
	float3 n1 = cross(e1, e3);

	float3 n = n0 + n1;
	float3 normal = normalize(float3(-n.x, -n.y, n.z));

	return normal;
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

	s = normalize(s);

	return s;
}
//-----------------------------------------------------------------------------