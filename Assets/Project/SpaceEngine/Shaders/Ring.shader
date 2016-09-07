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

			#include "SpaceStuff.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile DUMMY LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_3
			#pragma multi_compile DUMMY SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
			#pragma multi_compile DUMMY SCATTERING
				
			sampler2D _MainTex;
			float4    _Color;
			float4    _Mie;
			float     _LightingBias;
			float     _LightingSharpness;
				
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
				
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0; // uv
				float4 color : TEXCOORD1; // color

				#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
					float3 relativeDirection : TEXCOORD2; // world vertex/pixel to camera
					float3 sunRelativeDirection1 : TEXCOORD3; // world vertex/pixel to light 1
					#if LIGHT_2
						float3 sunRelativeDirection2 : TEXCOORD4; // world vertex/pixel to light 2
					#endif
					#if LIGHT_3
						float3 sunRelativeDirection3 : TEXCOORD5; // world vertex/pixel to light 2
					#endif
					#if LIGHT_4
						float3 sunRelativeDirection4 : TEXCOORD6; // world vertex/pixel to light 2
					#endif
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					float4 worldPosition : TEXCOORD7; // world vertex/pixel
				#endif
			};
				
			struct f2g
			{
				float4 color : COLOR;
			};
				
			void Vert(a2v i, out v2f o)
			{
				float4 worldPosition = mul(unity_ObjectToWorld, i.vertex);
				
				o.vertex = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.uv;
				o.color = 1.0f;

				#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
					o.color *= 0.25f * 2.0f; //UNITY_LIGHTMODEL_AMBIENT * 2.0f;
				#endif

				#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
					o.relativeDirection = _WorldSpaceCameraPos - worldPosition.xyz;
					o.sunRelativeDirection1 = _Light1Position.xyz - worldPosition.xyz;
					#if LIGHT_2
						o.sunRelativeDirection2 = _Light2Position.xyz - worldPosition.xyz;
					#endif
					#if LIGHT_3
						o.sunRelativeDirection3 = _Light3Position.xyz - worldPosition.xyz;
					#endif
					#if LIGHT_4
						o.sunRelativeDirection4 = _Light4Position.xyz - worldPosition.xyz;
					#endif
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					o.worldPosition = worldPosition;
				#endif
			}
				
			void Frag(v2f i, out f2g o)
			{
				float4 mainTex = tex2D(_MainTex, i.uv);
				float4 mainColor = mainTex * _Color;
					
				o.color = i.color * mainColor;

				#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
					i.relativeDirection = normalize(i.relativeDirection);
					i.sunRelativeDirection1 = normalize(i.sunRelativeDirection1);
					
					float3 shapened = i.relativeDirection * _LightingSharpness;
					float4 light1 = saturate(dot(i.sunRelativeDirection1, shapened) + _LightingBias) * _Light1Color;
					float4 lighting = float4(light1.xyz, 0.0f);

					#if LIGHT_2
						i.sunRelativeDirection2 = normalize(i.sunRelativeDirection2);
						lighting.xyz += (saturate(dot(i.sunRelativeDirection2, shapened) + _LightingBias) * _Light2Color).xyz;
					#endif

					#if LIGHT_3
						i.sunRelativeDirection3 = normalize(i.sunRelativeDirection3);
						lighting.xyz += (saturate(dot(i.sunRelativeDirection3, shapened) + _LightingBias) * _Light3Color).xyz;
					#endif

					#if LIGHT_4
						i.sunRelativeDirection4 = normalize(i.sunRelativeDirection4);	
						lighting.xyz += (saturate(dot(i.sunRelativeDirection4, shapened) + _LightingBias) * _Light4Color).xyz;
					#endif

					#if SCATTERING
						float4 scattering = MiePhase(dot(i.relativeDirection, i.sunRelativeDirection1), _Mie) * _Light1Color;

						#if LIGHT_2
							scattering += MiePhase(dot(i.relativeDirection, i.sunRelativeDirection2), _Mie) * _Light2Color;
						#endif

						#if LIGHT_3
							scattering += MiePhase(dot(i.relativeDirection, i.sunRelativeDirection3), _Mie) * _Light3Color;
						#endif

						#if LIGHT_4
							scattering += MiePhase(dot(i.relativeDirection, i.sunRelativeDirection4), _Mie) * _Light4Color;
						#endif

						scattering *= mainTex.w * (1.0f - mainTex.w); // Only scatter at the edges
				
						lighting += scattering;
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						lighting *= ShadowColor(i.worldPosition);
					#endif

					o.color += lighting * mainColor;
				#endif

				#if !LIGHT_1 && !LIGHT_2 && !LIGHT_3 && !LIGHT_4
					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						o.color.xyz *= ShadowColor(i.worldPosition).xyz;
					#endif
				#endif
			}
			ENDCG
		}
	}
}