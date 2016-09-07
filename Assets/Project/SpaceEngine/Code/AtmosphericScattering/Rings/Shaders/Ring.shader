Shader "Hidden/Ring"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Mie("Mie", Vector) = (0,0,0)
		_LightingBias("Lighting Bias", Float) = 0
		_LightingSharpness("Lighting Sharpness", Float) = 0
		
		_Light1Color("Light 1 Color", Color) = (0,0,0)
		_Light2Color("Light 2 Color", Color) = (0,0,0)
		_Light1Position("Light 1 Position", Vector) = (0,0,0)
		_Light2Position("Light 2 Position", Vector) = (0,0,0)
		
		_Shadow1Texture("Shadow 1 Texture", 2D) = "white" {}
		_Shadow1Ratio("Shadow 1 Ratio", Float) = 1
		_Shadow2Texture("Shadow 2 Texture", 2D) = "white" {}
		_Shadow2Ratio("Shadow 2 Ratio", Float) = 1
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcColor
			Cull Off
			Lighting Off
			ZWrite Off
			
			CGPROGRAM

			#include "Light.cginc"
			#include "Shadow.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile DUMMY LIGHT_0 LIGHT_1 LIGHT_2
			#pragma multi_compile DUMMY SHADOW_1 SHADOW_2
			// Scattering
			#pragma multi_compile DUMMY SGT_A
				
			// Keep under instruction limits
			#if SGT_A && LIGHT_2 && SHADOW_2
				#undef LIGHT_1
				#undef LIGHT_2
				#define LIGHT_1 1
				#define LIGHT_2 0
			#endif
				
			sampler2D _MainTex;
			float4    _Color;
			float4    _Mie;
			float     _LightingBias;
			float     _LightingSharpness;
				
			struct a2v
			{
				float4 vertex    : POSITION;
				float2 texcoord0 : TEXCOORD0;
			};
				
			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // uv
				float4 texcoord1 : TEXCOORD1; // color

				#if LIGHT_1 || LIGHT_2
					float3 texcoord2 : TEXCOORD2; // world vertex/pixel to camera
					float3 texcoord3 : TEXCOORD3; // world vertex/pixel to light 1
					#if LIGHT_2
						float3 texcoord4 : TEXCOORD4; // world vertex/pixel to light 2
					#endif
				#endif

				#if SHADOW_1 || SHADOW_2
					float4 texcoord5 : TEXCOORD7; // world vertex/pixel
				#endif
			};
				
			struct f2g
			{
				float4 color : COLOR;
			};
				
			void Vert(a2v i, out v2f o)
			{
				float4 wPos = mul(unity_ObjectToWorld, i.vertex);
				
				o.vertex    = mul(UNITY_MATRIX_MVP, i.vertex);
				o.texcoord0 = i.texcoord0;
				o.texcoord1 = 1.0f;

				#if LIGHT_0 || LIGHT_1 || LIGHT_2
					o.texcoord1 *= UNITY_LIGHTMODEL_AMBIENT * 2.0f;
				#endif

				#if LIGHT_1 || LIGHT_2
					o.texcoord2 = _WorldSpaceCameraPos - wPos.xyz;
					o.texcoord3 = _Light1Position.xyz - wPos.xyz;
					#if LIGHT_2
						o.texcoord4 = _Light2Position.xyz - wPos.xyz;
					#endif
				#endif

				#if SHADOW_1 || SHADOW_2
					o.texcoord5 = wPos;
				#endif
			}
				
			void Frag(v2f i, out f2g o)
			{
				float4 mainTex    = tex2D(_MainTex, i.texcoord0);
				float4 mainColor = mainTex * _Color;
					
				o.color = i.texcoord1 * mainColor;

				#if LIGHT_1 || LIGHT_2
					i.texcoord2 = normalize(i.texcoord2);
					i.texcoord3 = normalize(i.texcoord3);
					
					float3 shapened = i.texcoord2 * _LightingSharpness;
					float4 light1   = saturate(dot(i.texcoord3, shapened) + _LightingBias) * _Light1Color;
					float4 lighting = float4(light1.xyz, 0.0f);

					#if LIGHT_2
						i.texcoord4 = normalize(i.texcoord4);
					
						lighting.xyz += (saturate(dot(i.texcoord4, shapened) + _LightingBias) * _Light2Color).xyz;
					#endif

					#if SGT_A
						float4 scattering = MiePhase(dot(i.texcoord2, i.texcoord3), _Mie) * _Light1Color;

						#if LIGHT_2
							scattering += MiePhase(dot(i.texcoord2, i.texcoord4), _Mie) * _Light2Color;
						#endif

						scattering *= mainTex.w * (1.0f - mainTex.w); // Only scatter at the edges
				
						lighting += scattering;
					#endif

					#if SHADOW_1 || SHADOW_2
						lighting *= ShadowColor(i.texcoord5);
					#endif

					o.color += lighting * mainColor;
				#endif

				#if !LIGHT_0 && !LIGHT_1 && !LIGHT_2
					#if SHADOW_1 || SHADOW_2
						o.color.xyz *= ShadowColor(i.texcoord5).xyz;
					#endif
				#endif
			}
			ENDCG
		}
	}
}