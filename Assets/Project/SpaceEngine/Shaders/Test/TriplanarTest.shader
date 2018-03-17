// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

Shader "SpaceEngine/Test/TriplanarTest"
{
	Properties
	{
		_TBTexture("Top/Buttom Diffuse Map", 2D) = "white" {}
		_LRTexture("Left/Right Diffuse Map", 2D) = "white" {}
		_FBTexture("Front/Back Diffuse Map", 2D) = "white" {}
		_TextureScale("Texture Scale", Range(0, 32)) = 0.0
		_TriplanarBlendSharpness("Triplanar Blend Sharpness", Range(0, 8)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "../Core.cginc"

			uniform sampler2D _TBTexture;
			uniform sampler2D _LRTexture;
			uniform sampler2D _FBTexture;
			uniform float _TextureScale;
			uniform float _TriplanarBlendSharpness;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				float3 worldNormal : TEXCOORD1;
				float3 worldPosition : TEXCOORD2;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;

				o.worldNormal = v.normal.xyz;
				o.worldPosition = mul(UNITY_MATRIX_M, v.vertex).xyz;

				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				return Triplanar(_TBTexture, _LRTexture, _FBTexture, IN.worldPosition, IN.worldNormal, float2(_TextureScale, _TriplanarBlendSharpness));
			}
			ENDCG
		}
	}
}
