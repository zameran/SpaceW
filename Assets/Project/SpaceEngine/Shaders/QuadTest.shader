Shader "SpaceEngine/QuadTest" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)	
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#define nVerticesPerSide 128
		//#define SHOW_GRIDLINES

		#include "UnityCG.cginc"

		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert

		#pragma target 5.0

		struct OutputStruct
		{
			float noise;

			float3 patchCenter;

			float4 pos;
		};

		struct appdata_full_compute 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;

			fixed4 color : COLOR;

			uint id: SV_VertexID;
		};

		sampler2D _MainTex;
		sampler2D _HeightTexture;
		sampler2D _NormalTexture;

		#ifdef SHADER_API_D3D11
		StructuredBuffer<OutputStruct> data;
		#endif

		struct Input 
		{
			float noise;

			float2 uv_MainTex;
		};

		void vert(inout appdata_full_compute v, out Input o) 
		{
			float noise = data[v.id].noise;
			float3 patchCenter = data[v.id].patchCenter;
			float4 position = data[v.id].pos;

			position.w = 1.0;
			position.xyz += patchCenter;
			v.vertex = position;

			o.noise = noise;
			o.uv_MainTex = v.texcoord.xy;
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			#ifdef SHOW_GRIDLINES
				float2 fract = fmod(IN.uv_MainTex * (nVerticesPerSide - 1), float2(1, 1));
				fixed4 gridLine = any(step(float2(0.9, 0.9), fract));
			#else
				fixed4 gridLine = 0;
			#endif

			fixed4 terrainColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			terrainColor += fixed4(IN.noise * 4, IN.noise * 4, IN.noise * 4, 1.0);

			fixed4 c = terrainColor + gridLine;

			o.Albedo = clamp(c.rgb, fixed3(0, 0, 0), fixed3(1, 1 ,1));
			o.Normal = UnpackNormal(tex2D(_NormalTexture, IN.uv_MainTex));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
	ENDCG
	}
}