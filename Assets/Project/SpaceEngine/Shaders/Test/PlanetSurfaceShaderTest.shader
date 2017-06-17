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
			Tags { "LightMode"="ShadowCaster" }
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster

			#include "UnityCG.cginc"
 
			struct v2f 
			{
				V2F_SHADOW_CASTER;
			};
 
			v2f vert(appdata_base v)
			{
				v2f o;

				v.vertex.xyz += v.normal * 0.05f;

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}
 
			float4 frag(v2f i) : SV_Target
			{
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