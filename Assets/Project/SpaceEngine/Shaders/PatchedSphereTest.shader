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

Shader "SpaceEngine/PatchedSphereTest"
{
	Properties
	{

	}

	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "TCCommon.cginc"

			struct vtx_out
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 volume : TEXCOORD1;
			};

			vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 volume : TEXCOORD1)
			{
				vtx_out OUT;

				OUT.position = position;
				OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;
				OUT.volume = volume;

				return OUT;
			}

			float4 frag(vtx_out i) : COLOR
			{
				float3 p = normalize(i.volume);

				//float h = HeightMapClouds(p);
				float h = Fbm(p, 4);

				// return the heightmap
				// r = normalmap dx (not filled yet -- will be filled by the normal generator)
				// g = normalmap dy (not filled yet -- will be filled by the normal generator)
				// b = slope (not filled yet -- will be filled by the normal generator)
				// a = height
				return float4(0, 0, 0, h);
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float _UVStep;
			uniform sampler2D _Heightmap;
			uniform float _HeightScale;

			struct vtx_out
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			float2 GetSobelNormal(float2 uv, float uvStep)
			{
				float u = uv.x;
				float v = uv.y;

				float tl = tex2D(_Heightmap, float2(u - uvStep, v - uvStep)).a * _HeightScale;
				float l  = tex2D(_Heightmap, float2(u - uvStep, v)).a * _HeightScale;
				float bl = tex2D(_Heightmap, float2(u - uvStep, v + uvStep)).a * _HeightScale;
				float b  = tex2D(_Heightmap, float2(u, v + uvStep)).a * _HeightScale;
				float br = tex2D(_Heightmap, float2(u + uvStep, v + uvStep)).a * _HeightScale;
				float r  = tex2D(_Heightmap, float2(u + uvStep, v)).a * _HeightScale;
				float tr = tex2D(_Heightmap, float2(u + uvStep, v - uvStep)).a * _HeightScale;
				float t  = tex2D(_Heightmap, float2(u, v - uvStep)).a * _HeightScale;

				float dX = tr + 2.0 * r + br - tl - 2.0 * l - bl;
				float dY = bl + 2.0 * b + br - tl - 2.0 * t - tr;

				return float2(dX, dY);
			}

			vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0)
			{
				vtx_out OUT;

				OUT.position = position;
				OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;

				return OUT;
			}

			float4 frag(vtx_out i) : COLOR
			{
				float height = tex2D(_Heightmap, i.uv).a;

				float2 d = GetSobelNormal(i.uv, _UVStep);

				float3 n = normalize(float3(-d.x, -d.y, 2.0));

				float slope = max(abs(n.x), abs(n.y));

				// return the packed heightmap with normals
				// r = normalmap dx
				// g = normalmap dy
				// b = slope
				// a = height
				return float4(n.xy, slope, height);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}