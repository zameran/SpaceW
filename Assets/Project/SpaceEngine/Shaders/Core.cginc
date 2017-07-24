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

// NOTE : If you wanna use this file as include, please provide special defines before (defines, that provided after include doesn't taken in to the account) :
// CORE_PORDUCER_ADDITIONAL_UV

#define CORE

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

//-----------------------------------------------------------------------------
#define CORE_GLOBALS
uniform float4x4 _Globals_CameraToWorld;
uniform float4x4 _Globals_ScreenToCamera;
uniform float4x4 _Globals_CameraToScreen;
uniform float3 _Globals_WorldCameraPos;

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
uniform float4x4 _Deform_TileToTangent;
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define CORE_HDR

uniform float _Exposure;
uniform float _HDRMode;

inline float3 hdrFunction(float c)
{
	if (_HDRMode == 0) { return c; }
	else if (_HDRMode == 1) { return 1.0 - exp(-c); }
	else if (_HDRMode == 2) { return c < 1.0 ? pow(c * 0.47, 0.6073) : 1.0 - exp(-c); }
	else if (_HDRMode == 3) { return c < 1.413 ? pow(c * 0.38317, 1.0 / 2.2) : 1.0 - exp(-c); }
	else if (_HDRMode == 4) { return c < 1.413 ? pow(c * 0.38317, 0.454545455) : 1.0 - exp(-c); }
	else return c;
}

float3 hdr(float3 L) 
{
	L *= _Exposure;

	L.r = hdrFunction(L.r);
	L.g = hdrFunction(L.g);
	L.b = hdrFunction(L.b);

	return L;
}

float4 hdr(float4 L) 
{
	L *= _Exposure;

	L.r = hdrFunction(L.r);
	L.g = hdrFunction(L.g);
	L.b = hdrFunction(L.b);
	L.a = L.a;

	return L;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define CORE_LOG_DEPTH_BUFFER

float FCoef(float farPlane)
{
	return 2.0 / log2(farPlane + 1.0);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
void ScaleUVToTile(inout float2 uv, float3 tileCoords, float3 tileSize)
{
	uv = tileCoords.xy + uv * tileSize.xy;
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 texTileLod(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
{
	ScaleUVToTile(uv, tileCoords, tileSize);

	return tex2Dlod(tile, float4(uv, 0, 0));
}

float4 texTile(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize) 
{
	ScaleUVToTile(uv, tileCoords, tileSize);

	return tex2D(tile, uv);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 Triplanar(sampler2D topAndButtomSampler, sampler2D leftAndRightSampler, sampler2D frontAndBackSampler, float3 worldPosition, float3 worldNormal, float2 settings)
{
	half3 YSampler = tex2D(topAndButtomSampler, worldPosition.xz / settings.x);
	half3 XSampler = tex2D(leftAndRightSampler, worldPosition.zy / settings.x);
	half3 ZSampler = tex2D(frontAndBackSampler, worldPosition.xy / settings.x);

	half3 blendWeights = pow(abs(worldNormal), settings.y);

	blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

	return fixed4(XSampler * blendWeights.x + YSampler * blendWeights.y + ZSampler * blendWeights.z, 1.0);
}

float4 TriplanarColor(float3 worldPosition, float3 worldNormal, float2 settings)
{
	half3 YSampler = half3(0, 1, 0);
	half3 XSampler = half3(1, 0, 0);
	half3 ZSampler = half3(0, 0, 1);

	half3 blendWeights = pow(abs(worldNormal), settings.y);

	blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

	return fixed4(XSampler * blendWeights.x + YSampler * blendWeights.y + ZSampler * blendWeights.z, 1.0);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float4 RGB2Reflectance(float4 color) { return float4(tan(1.37 * color.rgb) / tan(1.37), color.a); }
inline float4 EncodeNormalAndSlope(float3 normal, float slope) { return float4(normal, slope); }
inline float3 DecodeNormal(float3 normal) { return float3(normal.xy, sqrt(max(0.0, 1.0 - dot(normal.xy, normal.xy)))); }
inline float4 DecodeNormalAndSlope(float4 normalAndSlope) { return float4(DecodeNormal(normalAndSlope.xyz), normalAndSlope.w); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct VertexProducerInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
};

struct VertexProducerOutput
{
	float4 pos : SV_POSITION;
	float2 uv0 : TEXCOORD0;
#if defined(CORE_PORDUCER_ADDITIONAL_UV)
	float2 uv1 : TEXCOORD1;
#endif
};

#define CORE_PRODUCER_VERTEX_PROGRAM_BODY \
	o.pos = UnityObjectToClipPos(v.vertex); \
	o.uv0 = v.texcoord.xy; \

#define CORE_PRODUCER_VERTEX_PROGRAM_BODY_ADDITIONAL_UV(scale) \
	o.uv1 = v.texcoord.xy * scale; \

#if defined(CORE_PORDUCER_ADDITIONAL_UV)
#define CORE_PRODUCER_VERTEX_PROGRAM(scale) \
	void vert(in VertexProducerInput v, out VertexProducerOutput o) \
	{ \
		CORE_PRODUCER_VERTEX_PROGRAM_BODY; \
		CORE_PRODUCER_VERTEX_PROGRAM_BODY_ADDITIONAL_UV(scale); \
	}
#else
#define CORE_PRODUCER_VERTEX_PROGRAM \
	void vert(in VertexProducerInput v, out VertexProducerOutput o) \
	{ \
		CORE_PRODUCER_VERTEX_PROGRAM_BODY; \
	}
#endif
//-----------------------------------------------------------------------------