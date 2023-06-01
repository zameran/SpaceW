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

#define SPACE_STUFF

// TODO : Blur shadows oly on specified shadow casters...
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

float ComputeMiePhase(float cosTheta, float miePhaseAnisotropy)
{
    float squareAniso = miePhaseAnisotropy * miePhaseAnisotropy;

    float Num = 1.5 * (1.0 + cosTheta * cosTheta) * (1.0 - squareAniso);
    float Den = (8.0 + squareAniso) * pow(abs(1.0 + squareAniso - 2.0 * miePhaseAnisotropy * cosTheta), 1.5);

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
float4x4 _Shadow1Matrix;
sampler2D _Shadow1Texture;
float _Shadow1Ratio;

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

float4 ShadowColor(float4x4 shadowMatrix, sampler2D shadowSampler, float shadowRatio, float4 worldPoint)
{
    float4 shadowPoint = mul(shadowMatrix, worldPoint);
    float4 shadow = 0;
    float2 shadowMag = length(shadowPoint.xy);

    shadowMag = 1.0f - (1.0f - shadowMag) * shadowRatio;

    #ifdef SHADOW_BLUR
    shadow = Blur(shadowSampler, shadowMag, 0.00015f);
    #else
		shadow = tex2Dlod(shadowSampler, float4(shadowMag.xy, 0.0, 0.0));
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
