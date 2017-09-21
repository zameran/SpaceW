/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
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
 
/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 */

 /*
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2017
 */

#define OCEAN_DISPLACEMENT

uniform float _Ocean_Radius;
uniform float _Ocean_HeightOffset;
uniform float3 _Ocean_CameraPos;
uniform float4x4 _Ocean_OceanToCamera;
uniform float4x4 _Ocean_CameraToOcean;

#ifdef OCEAN_ONLY_SPHERICAL

uniform float3 _SphereDirection;
uniform float _CosTheta;
uniform float _SinTheta;

float2 OceanPos(float4 vert, float4x4 stoc, out float t, out float3 cameraDir, out float3 oceanDir) 
{
	float h = _Ocean_CameraPos.z;
	float4 v = float4(vert.x, vert.y, 0.0, 1.0);

	cameraDir = normalize(mul(stoc, v).xyz);											// Direction in camera space
	
	float3 n1 = cross(_SphereDirection, cameraDir);										// Normal to plane containing direction to planet and vertex view direction
	float3 n2 = normalize(cross(n1, _SphereDirection));									// Upwards vector in plane space, plane containing CamO and cameraDir

	float3 hor = _CosTheta * _SphereDirection + _SinTheta * n2;
 
	cameraDir = ((dot(n1, cross(hor, cameraDir)) > 0.0) && (h > 0)) ? hor : cameraDir;	// Checking if view direction is above the horizon
	oceanDir = mul(_Ocean_CameraToOcean, float4(cameraDir, 0.0)).xyz;

	float dz = oceanDir.z;
	float radius = _Ocean_Radius;
	
	float b = dz * (h + radius);
	float c = h * (h + 2.0 * radius);
	float tSphere = - b - sqrt(max(b * b - c, 0.0));
	float tApprox = - h / dz * (1.0 + h / (2.0 * radius) * (1.0 - dz * dz));

	t = abs((tApprox - tSphere) * dz) < 1.0 ? tApprox : tSphere;

	return _Ocean_CameraPos.xy + t * oceanDir.xy;
}

float2 OceanPos(float4 vert, float4x4 stoc) 
{
	float t;
	float3 cameraDir;
	float3 oceanDir;

	return OceanPos(vert, stoc, t, cameraDir, oceanDir);
}

#else

uniform float3 _Ocean_Horizon1;
uniform float3 _Ocean_Horizon2;

float2 OceanPos(float4 vert, float4x4 stoc, out float t, out float3 cameraDir, out float3 oceanDir) 
{
	float horizon = _Ocean_Horizon1.x + _Ocean_Horizon1.y * vert.x;
	
	horizon -= sqrt(_Ocean_Horizon2.x + (_Ocean_Horizon2.y + _Ocean_Horizon2.z * vert.x) * vert.x);
	
	float4 v = float4(vert.x, min(vert.y, horizon), 0.0, 1.0);

	cameraDir = normalize(mul(stoc, v).xyz);
	oceanDir = mul(_Ocean_CameraToOcean, float4(cameraDir, 0.0)).xyz;
	
	float cz = _Ocean_CameraPos.z;
	float dz = oceanDir.z;
	float radius = _Ocean_Radius;
	
	// NOTE : Body shape dependent...
	float b = dz * (cz + radius);
	float c = cz * (cz + 2.0 * radius);
	float tSphere = - b - sqrt(max(b * b - c, 0.0));
	float tApprox = - cz / dz * (1.0 + cz / (2.0 * radius) * (1.0 - dz * dz));

	t = abs((tApprox - tSphere) * dz) < 1.0 ? tApprox : tSphere;

	return _Ocean_CameraPos.xy + t * oceanDir.xy;
}

float2 OceanPos(float4 vert, float4x4 stoc) 
{
	float t;
	float3 cameraDir;
	float3 oceanDir;

	return OceanPos(vert, stoc, t, cameraDir, oceanDir);
}

#endif

float4 Tex2DGrad(sampler2D tex, float2 uv, float2 dx, float2 dy, float2 texSize)
{
	//Sampling a texture by derivatives is unsupported in vert shaders in Unity but if you
	//can manually calculate the derivates you can reproduce its effect using tex2Dlod 
	float2 px = texSize.x * dx;
	float2 py = texSize.y * dy;

	float lod = 0.5 * log2(max(dot(px, px), dot(py, py)));

	return tex2Dlod(tex, float4(uv, 0.0, lod));
}