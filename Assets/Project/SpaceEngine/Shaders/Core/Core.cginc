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

#define CORE

#define CORE_ELEVATION

uniform sampler2D _Elevation_Tile;

uniform float3 _Elevation_TileSize;
uniform float3 _Elevation_TileCoords;

#define CORE_NORMALS

uniform sampler2D _Normals_Tile;

uniform float3 _Normals_TileSize;
uniform float3 _Normals_TileCoords;

#define CORE_COLOR

uniform sampler2D _Color_Tile;

uniform float3 _Color_TileSize;
uniform float3 _Color_TileCoords;

#define CORE_ORTHO

uniform sampler2D _Ortho_Tile;

uniform float3 _Ortho_TileSize;
uniform float3 _Ortho_TileCoords;

#define CORE_DEFORMATION

uniform float _Deform_Radius;

uniform float2 _Deform_Blending;

uniform float4 _Deform_Offset;
uniform float4 _Deform_Camera;
uniform float4 _Deform_ScreenQuadCornerNorms;

uniform float4x4 _Deform_LocalToWorld;
uniform float4x4 _Deform_LocalToScreen;
uniform float4x4 _Deform_ScreenQuadCorners;
uniform float4x4 _Deform_ScreenQuadVerticals;
uniform float4x4 _Deform_TangentFrameToWorld; 

float4 texTileLod(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
{
	uv = tileCoords.xy + uv * tileSize.xy;

	return tex2Dlod(tile, float4(uv, 0, 0));
}

float4 texTile(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
{
	uv = tileCoords.xy + uv * tileSize.xy;

	return tex2D(tile, uv);
}

float4 texTile(sampler2D tile, float2 uv, float2 tileCoords, float3 tileSize) 
{
	uv = tileCoords + uv * tileSize.xy;

	return tex2D(tile, uv);
}

float4 Triplanar(sampler2D topAndButtomSampler, sampler2D leftAndRightSampler, sampler2D frontAndBackSampler, float3 worldPosition, float3 worldNormal, float2 settings)
{
	half3 YSampler = tex2D(topAndButtomSampler, worldPosition.xz / settings.x);
	half3 XSampler = tex2D(leftAndRightSampler, worldPosition.zy / settings.x);
	half3 ZSampler = tex2D(frontAndBackSampler, worldPosition.xy / settings.x);

	half3 blendWeights = pow(abs(worldNormal), settings.y);

	blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

	return fixed4(XSampler * blendWeights.x + YSampler * blendWeights.y + ZSampler * blendWeights.z, 1.0);
}

struct VertexProducerInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
};

struct VertexProducerOutput
{
	float4 pos : SV_POSITION;
	float2 uv0 : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
};