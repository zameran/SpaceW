Shader "SpaceEngine/QuadTest" 
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_Mixing("Mixing", Range(0,1)) = 0.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#include "UnityCG.cginc"
		#include "TCCommon.cginc"

		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert

		#pragma target 5.0

		struct OutputStruct
		{
			float noise;

			float3 patchCenter;

			float4 vcolor;
			float4 pos;
			float4 cpos;
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

			uint id: SV_VertexID;
		};

		sampler2D _HeightTexture;
		sampler2D _NormalTexture;

		#ifdef SHADER_API_D3D11
		StructuredBuffer<OutputStruct> data;
		#endif

		struct Input 
		{
			float noise;
			
			float2 uv;

			float4 color;
		};

		void vert(inout appdata_full_compute v, out Input o) 
		{
			float noise = data[v.id].noise;
			float3 patchCenter = data[v.id].patchCenter;
			float4 vcolor = data[v.id].vcolor;
			float4 position = data[v.id].pos;

			position.w = 1.0;
			position.xyz += patchCenter;

			v.vertex = position;

			o.noise = noise + 0.5;
			o.uv = v.texcoord.xy;
			o.color = vcolor;
		}
		
		half _Mixing;
		half _Glossiness;
		half _Metallic;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 terrainColor = lerp(fixed4(IN.noise, IN.noise, IN.noise, 1.0), float4(IN.color.xyz, 1), _Mixing);

			o.Albedo = terrainColor.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
}