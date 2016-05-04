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
struct QuadGenerationConstants
{
	float planetRadius;
	float spacing;
	float spacingreal;
	float spacingsub;
	float terrainMaxHeight;
	float lodLevel;
	float orientation;

	float3 cubeFaceEastDirection;
	float3 cubeFaceNorthDirection;
	float3 patchCubeCenter;
};

struct OutputStruct
{
	float noise;

	float3 patchCenter;
	
	float4 vcolor;
	float4 pos;
	float4 cpos;
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

	//TODO modifier calculation.

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