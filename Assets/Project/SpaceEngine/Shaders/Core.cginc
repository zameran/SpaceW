// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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
// CORE_PRODUCER_ADDITIONAL_UV

#define CORE

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

#if !defined (HLSL_SUPPORT_INCLUDED)
#include <HLSLSupport.cginc>
#endif

//-----------------------------------------------------------------------------
#define CORE_GLOBALS
uniform float4x4 _Globals_CameraToWorld;
uniform float4x4 _Globals_ScreenToCamera;
uniform float4x4 _Globals_CameraToScreen;
uniform float3 _Globals_WorldCameraPos;
uniform float _Globals_RadiusOffset;

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

#define CORE_SUNS

uniform float4 _Sun_Colors_1;

uniform float4x4 _Sun_WorldDirections_1;
uniform float4x4 _Sun_Positions_1;

#define CORE_BODY

uniform float3 _Body_WorldCameraPosition; // World camera position relative to body origin
uniform float3 _Body_Origin; // World body origin
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define CORE_HDR

uniform float _HDRExposure;
uniform float _HDRMode;

inline float3 hdrFunction(float3 c)
{
    UNITY_BRANCH
    if (_HDRMode == 0) // None
    {
        return c;
    }
    if (_HDRMode == 1) // Simple
    {
        c *= _HDRExposure;

        return 1.0 - exp(-c);
    }
    if (_HDRMode == 2) // SpaceEngine
    {
        c *= _HDRExposure;

        return c < 1.0 ? pow(c * 0.47, 0.6073) : 1.0 - exp(-c);
    }
    if (_HDRMode == 3) // SpaceEngineOptimized
    {
        return 1.0 - exp(-_HDRExposure * (c * 1.0));
    }
    if (_HDRMode == 4) // Proland
    {
        c *= _HDRExposure;

        return c < 1.413 ? pow(c * 0.38317, 1.0 / 2.2) : 1.0 - exp(-c);
    }
    if (_HDRMode == 5) // ProlandOptimized
    {
        c *= _HDRExposure;

        return c < 1.413 ? pow(c * 0.38317, 0.454545455) : 1.0 - exp(-c);
    }
    return c;
}

float3 hdr(float3 L)
{
    return hdrFunction(L);
}

float4 hdr(float4 L)
{
    L.rgb = hdrFunction(L.rgb);

    #if defined(CORE_HDR_LUMA)
		L.a = dot(L.rgb, float3(0.299, 0.587, 0.114));
    #else
    L.a = L.a;
    #endif

    return L;
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

    return tex2Dlod(tile, float4(uv, 0.0, 0.0));
}

float4 texTile(sampler2D tile, float2 uv, float3 tileCoords, float3 tileSize)
{
    ScaleUVToTile(uv, tileCoords, tileSize);

    return tex2D(tile, uv);
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Source: http://www.iquilezles.org/www/articles/texturerepetition/texturerepetition.htm
float4 texNoTile(sampler2D samp, float k, in float2 uv)
{
    // k variable describes variation pattern

    // compute index
    float index = k * 8.0;
    float i = floor(index);
    float f = frac(index);

    // offsets for the different virtual patterns
    float2 offa = sin(float2(3.0, 7.0) * (i + 0.0)); // can replace with any other hash
    float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash

    // compute derivatives for mip-mapping
    float2 dx = ddx(uv), dy = ddy(uv);

    // sample the two closest virtual patterns
    float4 cola = tex2Dgrad(samp, uv + offa, dx, dy);
    float4 colb = tex2Dgrad(samp, uv + offb, dx, dy);
    float4 diff = cola - colb;

    float summ = diff.x + diff.y + diff.z;

    // interpolate between the two virtual patterns
    return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * summ));
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
    float4 position : SV_POSITION;
    float2 uv0 : TEXCOORD0;

    #if defined(CORE_PRODUCER_ADDITIONAL_UV)
		float2 uv1 : TEXCOORD1;
    #endif
};

#define CORE_PRODUCER_VERTEX_PROGRAM_BODY \
	o.position = UnityObjectToClipPos(v.vertex); \
	o.uv0 = v.texcoord.xy;
#define CORE_PRODUCER_VERTEX_PROGRAM_BODY_ADDITIONAL_UV(scale) \
	o.uv1 = v.texcoord.xy * scale;
#if defined(CORE_PRODUCER_ADDITIONAL_UV)
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

//-----------------------------------------------------------------------------
struct VertexLayerInput
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;
};

struct VertexLayerOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
};

#define CORE_LAYER_VERTEX_PROGRAM_BODY \
	o.position = UnityObjectToClipPos(v.vertex); \
	o.uv = v.texcoord.xy;
#define CORE_LAYER_VERTEX_PROGRAM \
	void vert(in VertexLayerInput v, out VertexLayerOutput o) \
	{ \
		CORE_LAYER_VERTEX_PROGRAM_BODY; \
	}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
static float4x4 slopexMatrix[4] =
{
    {
        0.0, 0.0, 0.0, 0.0,
        1.0, 0.0, -1.0, 0.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.0, 0.0, 0.0,
        0.5, 0.5, -0.5, -0.5,
        0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.0, 0.0, 0.0,
        0.5, 0.0, -0.5, 0.0,
        0.5, 0.0, -0.5, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.0, 0.0, 0.0,
        0.25, 0.25, -0.25, -0.25,
        0.25, 0.25, -0.25, -0.25,
        0.0, 0.0, 0.0, 0.0
    }
};

static float4x4 slopeyMatrix[4] =
{
    {
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, -1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.5, 0.5, 0.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, -0.5, -0.5, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.5, 0.0, 0.0,
        0.0, 0.5, 0.0, 0.0,
        0.0, -0.5, 0.0, 0.0,
        0.0, -0.5, 0.0, 0.0
    },
    {
        0.0, 0.25, 0.25, 0.0,
        0.0, 0.25, 0.25, 0.0,
        0.0, -0.25, -0.25, 0.0,
        0.0, -0.25, -0.25, 0.0
    }
};

static float4x4 curvatureMatrix[4] =
{
    {
        0.0, -1.0, 0.0, 0.0,
        -1.0, 4.0, -1.0, 0.0,
        0.0, -1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, -0.5, -0.5, 0.0,
        -0.5, 1.5, 1.5, -0.5,
        0.0, -0.5, -0.5, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, -0.5, 0.0, 0.0,
        -0.5, 1.5, -0.5, 0.0,
        -0.5, 1.5, -0.5, 0.0,
        0.0, -0.5, 0.0, 0.0
    },
    {
        0.0, -0.25, -0.25, 0.0,
        -0.25, 0.5, 0.5, -0.25,
        -0.25, 0.5, 0.5, -0.25,
        0.0, -0.25, -0.25, 0.0
    }
};

static float4x4 upsampleMatrix[4] =
{
    {
        0.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, 0.0, 0.0, 0.0,
        -1.0 / 16.0, 9.0 / 16.0, 9.0 / 16.0, -1.0 / 16.0,
        0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0
    },
    {
        0.0, -1.0 / 16.0, 0.0, 0.0,
        0.0, 9.0 / 16.0, 0.0, 0.0,
        0.0, 9.0 / 16.0, 0.0, 0.0,
        0.0, -1.0 / 16.0, 0.0, 0.0
    },
    {
        1.0 / 256.0, -9.0 / 256.0, -9.0 / 256.0, 1.0 / 256.0,
        -9.0 / 256.0, 81.0 / 256.0, 81.0 / 256.0, -9.0 / 256.0,
        -9.0 / 256.0, 81.0 / 256.0, 81.0 / 256.0, -9.0 / 256.0,
        1.0 / 256.0, -9.0 / 256.0, -9.0 / 256.0, 1.0 / 256.0
    }
};

float mdot(float4x4 a, float4x4 b)
{
    return dot(a[0], b[0]) + dot(a[1], b[1]) + dot(a[2], b[2]) + dot(a[3], b[3]);
}

float4x4 SampleCoarseLevelHeights(sampler2D coarseLevelSampler, float2 uv, float3 coarseLevelOSL)
{
    return float4x4
    (
        tex2Dlod(coarseLevelSampler, float4(uv + float2(0.0, 0.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(1.0, 0.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(2.0, 0.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(3.0, 0.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(0.0, 1.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(1.0, 1.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(2.0, 1.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(3.0, 1.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(0.0, 2.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(1.0, 2.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(2.0, 2.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(3.0, 2.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(0.0, 3.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(1.0, 3.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(2.0, 3.0) * coarseLevelOSL.z, 0.0, 0.0)).x,
        tex2Dlod(coarseLevelSampler, float4(uv + float2(3.0, 3.0) * coarseLevelOSL.z, 0.0, 0.0)).x
    );
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
#define CORE_LOG_DEPTH

// #define CORE_WRITE_TO_DEPTH	// NOTE : Control switch...
// Outerra approach...
// TODO : DirectX/OpenGL switch...
// TODO : Finish it...

#if defined(CORE_WRITE_TO_DEPTH)
	#define LOG_DEPTH(idx)						float logDepth : TEXCOORD##idx;
	#define TRANSFER_LOG_DEPTH(input, output)	LogarithmicInPosition(input.vertex, output.logDepth);
	#define OUTPUT_LOG_DEPTH(input, output)		LogarithmicOutDepth(input.logDepth, output.depth);
#else
#define LOG_DEPTH(idx)
#define TRANSFER_LOG_DEPTH(input, output)
#define OUTPUT_LOG_DEPTH(input, output)
#endif

#if defined(CORE_WRITE_TO_DEPTH)
	inline float FCoefficient()
	{
		return (2.0 / log2(_ProjectionParams.z + 1.0));
	}

	void LogarithmicInPosition(inout float4 position, out float depth)
	{
		position.z = log2(max(1e-6, 1.0 + position.w)) * FCoefficient() - 1.0;
		position.z *= position.w;
	
		depth = 1.0 + position.w;
	}

	void LogarithmicOutDepth(in float logDepth, out float depth)
	{
		depth = 1.0 - (log2(logDepth) * (0.5 * FCoefficient()));	// DirectX
		//depth = log2(logDepth) * (0.5 * FCoefficient());			// OpenGL
	}
#endif
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
struct ForwardOutput
{
    float4 diffuse : SV_Target; // rgb: diffuse,  a: unused

    #if defined(CORE_WRITE_TO_DEPTH)
		float depth	: SV_Depth;
    #endif
};

struct DeferredOutput
{
    float4 diffuse : SV_Target0; // rgb: diffuse,  a: occlusion
    float4 specular : SV_Target1; // rgb: specular, a: smoothness
    float4 normal : SV_Target2; // rgb: normal,   a: unused
    float4 emission : SV_Target3; // rgb: emission, a: unused

    #if defined(CORE_WRITE_TO_DEPTH)
		float depth	: SV_Depth;
    #endif
};

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
uniform float3 _Ocean_Color;

uniform float _Ocean_Sigma;
uniform float _Ocean_DrawBRDF;
uniform float _Ocean_Level;

void VERTEX_POSITION(in float4 vertex, in float2 texcoord, out float4 position, out float3 localPosition, out float2 uv)
{
    float2 zfc = texTileLod(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).xy;

    #if ATMOSPHERE_ON
    #if OCEAN_ON
			UNITY_BRANCH if (zfc.x <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) { zfc = float2(0.0, 0.0); }
    #endif
    #endif

    float4 vertexUV = float4(vertex.xy, float2(1.0, 1.0) - vertex.xy);
    float2 vertexToCamera = abs(_Deform_Camera.xy - vertex.xy);
    float vertexDistance = max(max(vertexToCamera.x, vertexToCamera.y), _Deform_Camera.z);
    float vertexBlend = clamp((vertexDistance - _Deform_Blending.x) / _Deform_Blending.y, 0.0, 1.0);

    float4 alpha = vertexUV.zxzx * vertexUV.wwyy;
    float4 alphaPrime = alpha * _Deform_ScreenQuadCornerNorms / dot(alpha, _Deform_ScreenQuadCornerNorms);

    float3 P = float3(vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, _Deform_Radius);

    float h = zfc.x * (1.0 - vertexBlend) + zfc.y * vertexBlend;
    float k = min(length(P) / dot(alpha, _Deform_ScreenQuadCornerNorms) * 1.0000003, 1.0);
    float hPrime = (h + _Deform_Radius * (1.0 - k)) / k;
    float hPre = _Deform_Radius + h;

    #if ATMOSPHERE_ON
    #if OCEAN_ON
			hPre = (_Deform_Radius + max(h, _Ocean_Level));
    #endif
    #endif

    //position = mul(_Deform_LocalToScreen, float4(P + float3(0.0, 0.0, h), 1.0));							//CUBE PROJECTION
    position = mul(_Deform_ScreenQuadCorners + hPrime * _Deform_ScreenQuadVerticals, alphaPrime); //SPHERICAL PROJECTION
    localPosition = hPre * normalize(mul(_Deform_LocalToWorld, P));
    uv = texcoord;
}

void VERTEX_LOCAL_POSITION(in float4 vertex, in float2 texcoord, out float4 position)
{
    float2 zfc = texTileLod(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).xy;

    float2 vertexToCamera = abs(_Deform_Camera.xy - vertex.xy);
    float vertexBlend = clamp((max(max(vertexToCamera.x, vertexToCamera.y), _Deform_Camera.z) - _Deform_Blending.x) / _Deform_Blending.y, 0.0, 1.0);

    float3 P = float3(vertex.xy * _Deform_Offset.z + _Deform_Offset.xy, _Deform_Radius);

    position = (_Deform_Radius + zfc.x * (1.0 - vertexBlend) + zfc.y * vertexBlend) * normalize(mul(_Deform_LocalToWorld, P));
}

//-----------------------------------------------------------------------------
