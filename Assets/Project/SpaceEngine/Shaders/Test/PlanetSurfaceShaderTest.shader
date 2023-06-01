Shader "SpaceEngine/Test/PlanetSurfaceShaderTest" 
{
	Properties 
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{
			"Queue"					= "Geometry"
			"RenderType"			= "Geometry"
			"ForceNoShadowCasting"	= "False"
			"IgnoreProjector"		= "True"
		}

		Cull Back
		ZWrite On
		ZTest On
		Fog { Mode Off }
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert exclude_path:deferred exclude_path:prepass noambient novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		#pragma target 5.0
		#pragma only_renderers d3d11 glcore metal

		struct Input 
		{
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		void vert(inout appdata_full v, out Input o) 
		{
			//v.vertex.xyz += v.normal * 0.05f;

			o.uv_MainTex = v.texcoord;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex);

			o.Albedo = color.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
		}
		ENDCG
	
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#include "UnityCG.cginc"

			struct v2f 
			{ 
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;

				//v.vertex.xyz += v.normal * 0.05f;

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}