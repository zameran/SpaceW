Shader "SpaceEngine/Test/PlanetSurfaceShaderTest" 
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader 
	{
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0

			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma multi_compile_shadowcaster

			#include "UnityStandardShadow.cginc"

			#pragma vertex vertShadowCasterModified
			#pragma fragment fragShadowCasterModified

			void vertShadowCasterModified(VertexInput v,
				#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
				out VertexOutputShadowCaster o,
				#endif
				out float4 opos : SV_POSITION)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v.vertex.xyz += v.normal * 0.05f;
				TRANSFER_SHADOW_CASTER_NOPOS(o,opos)

				#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
					o.tex = TRANSFORM_TEX(v.uv0, _MainTex);
				#endif
			}

			half4 fragShadowCasterModified(
				#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
				VertexOutputShadowCaster i
				#endif
				#ifdef UNITY_STANDARD_USE_DITHER_MASK
				, UNITY_VPOS_TYPE vpos : VPOS
				#endif
				) : SV_Target
			{
				#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
					half alpha = tex2D(_MainTex, i.tex).a * _Color.a;
					#if defined(_ALPHATEST_ON)
						clip (alpha - _Cutoff);
					#endif
					#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
						#if defined(UNITY_STANDARD_USE_DITHER_MASK)
							half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy * 0.25, alpha * 0.9375)).a;
							clip(alphaRef - 0.01);
						#else
							clip(alpha - _Cutoff);
						#endif
					#endif
				#endif

				SHADOW_CASTER_FRAGMENT(i)
			}	

			ENDCG
		}

		Tags { "RenderType" = "Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert

		#pragma target 3.0

		struct Input 
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;
		sampler2D _MainTex;
		half _Glossiness;
		half _Metallic;

		void vert(inout appdata_full v) 
		{
			v.vertex.xyz += v.normal * 0.05f;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = color.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = color.a;
		}
		ENDCG
	}
	//FallBack "Diffuse"
}