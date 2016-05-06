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

/*
 * Precomputed Atmospheric Scattering
 * Copyright (c) 2008 INRIA
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
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Eric Bruneton
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2016
 */

Shader "Proland/Atmo/Sky" 
{
	Properties
	{
		_Sun_Glare("Sun Glare", 2D) = "black" {}
		_Sun_Glare_Scale("Sun Glare Scale", Float) = 1.0
		_Sun_Glare_Color("Sun Glare Color", Color) = (1, 1, 1, 1)
	}
	SubShader 
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
	
		Pass 
		{
			ZWrite On
			ZTest Always  
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcColor

			Cull Off

			CGPROGRAM
			#include "UnityCG.cginc"		
			#include "Utility.cginc"
			#include "Atmosphere.cginc"

			#pragma multi_compile LIGHT_1 LIGHT_2

			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_Origin;
			
			uniform sampler2D _Sun_Glare;
			uniform float _Sun_Glare_Scale;
			uniform float4 _Sun_Glare_Color;
			uniform float4x4 _Sun_WorldToLocal_1;
			uniform float4x4 _Sun_WorldToLocal_2;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 dir : TEXCOORD1;

				#ifdef LIGHT_1 
					float3 relativeDir_1 : TEXCOORD2;
				#endif

				#ifdef LIGHT_2
					float3 relativeDir_1 : TEXCOORD2;
					float3 relativeDir_2 : TEXCOORD3;
				#endif
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;
				OUT.dir = (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;

				#ifdef LIGHT_1 
					float3x3 wtl_1 = _Sun_WorldToLocal_1; 
				#endif

				#ifdef LIGHT_2 
					float3x3 wtl_1 = _Sun_WorldToLocal_1;
					float3x3 wtl_2 = _Sun_WorldToLocal_2; 
				#endif
				
				// apply this rotation to view dir to get relative viewdir
				#ifdef LIGHT_1 
					OUT.relativeDir_1 = mul(wtl_1, OUT.dir); 
				#endif

				#ifdef LIGHT_2 
					OUT.relativeDir_1 = mul(wtl_1, OUT.dir); 
					OUT.relativeDir_2 = mul(wtl_2, OUT.dir); 
				#endif
	
				OUT.pos = float4(v.vertex.xy, 1.0, 1.0);
				OUT.uv = v.texcoord.xy;
				return OUT;
			}
			
			float3 OuterSunRadiance(float3 viewdir)
			{
				float3 data = viewdir.z > 0.0 ? (tex2D(_Sun_Glare, float2(0.5, 0.5) + viewdir.xy / _Sun_Glare_Scale).rgb * _Sun_Glare_Color) : float3(0, 0, 0);

				return pow(max(0, data), 2.2) * _Sun_Intensity;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float3 WCP = _Globals_WorldCameraPos;
				float3 d = normalize(IN.dir);

				#ifdef LIGHT_1
					float3 WSD_1 = _Sun_WorldSunDir_1;

					float3 sun1Color = OuterSunRadiance(IN.relativeDir_1);

					float3 extinction1;

					float3 inscatter1 = SkyRadiance(WCP + _Globals_Origin, d, WSD_1, extinction1, 0.0);

					float sunColor = sun1Color;
					float3 extinction = extinction1;
					float3 inscatter = inscatter1;
					
					float3 eclipse = ApplyEclipse(WCP, d, _Globals_Origin);

					float3 finalColor = sunColor * extinction + inscatter * eclipse;

					return float4(hdr(finalColor), 1);
				#endif

				#ifdef LIGHT_2
					float3 WSD_1 = _Sun_WorldSunDir_1;
					float3 WSD_2 = _Sun_WorldSunDir_2;

					float3 sun1Color = OuterSunRadiance(IN.relativeDir_1);
					float3 sun2Color = OuterSunRadiance(IN.relativeDir_2);

					float3 extinction1;
					float3 extinction2;

					float3 inscatter1 = SkyRadiance(WCP + _Globals_Origin, d, WSD_1, extinction1, 0.0);
					float3 inscatter2 = SkyRadiance(WCP + _Globals_Origin, d, WSD_2, extinction2, 0.0);

					float sunColor = sun1Color + sun2Color;
					float3 extinction = extinction1 + extinction2;
					float3 inscatter = inscatter1 + inscatter2;

					float3 eclipse = ApplyEclipse(WCP, d, _Globals_Origin);

					float3 finalColor = sunColor * extinction + inscatter * eclipse;

					return float4(hdr(finalColor), 1);
				#endif

				return float4(0, 0, 0, 0);
			}		
			ENDCG
		}
	}
}