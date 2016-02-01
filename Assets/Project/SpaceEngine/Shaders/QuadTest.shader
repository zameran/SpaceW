Shader "SpaceEngine/QuadTest" 
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_Mixing("Mixing", Range(0,1)) = 0.0
		_Glossiness("Glossiness", Range(0,1)) = 0.0
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

			uint id : SV_VertexID;
		};

		sampler2D _HeightTexture;
		sampler2D _NormalTexture;

		#ifdef SHADER_API_D3D11
		StructuredBuffer<OutputStruct> data;
		#endif

		float3 FindNormal(float2 uv, float2 u)
		{
			float ht0 = tex2D(_HeightTexture, uv + float2(-u.x, 0)).a;
			float ht1 = tex2D(_HeightTexture, uv + float2(u.x, 0)).a;
			float ht2 = tex2D(_HeightTexture, uv + float2(0, -u.y)).a;
			float ht3 = tex2D(_HeightTexture, uv + float2(0, u.y)).a;

			float3 va = normalize(float3(float2(0.1, 0.0), ht1 - ht0));
			float3 vb = normalize(float3(float2(0.0, 0.1), ht3 - ht2));

			return cross(va, vb);
		}

		float3 FindTangent(float3 normal, float epsilon)
		{
			float refVectorSign = sign(1.0 - abs(normal.x) - epsilon);

			float3 refVector = refVectorSign * float3(1.0, 0.0, 0.0);
			float3 biTangent = refVectorSign * cross(normal, refVector);

			return cross(-normal, biTangent);
		}

		struct Input 
		{
			float noise;
			
			float2 uv_HeightTexture;
			float2 uv_NormalTexture;

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
			
			v.tangent = float4(FindTangent(tex2Dlod(_NormalTexture, float4(v.texcoord.xy, 0, 0)), 0.01), 1);
			v.tangent.xyz += position;

			v.normal = tex2Dlod(_NormalTexture, v.texcoord);
			v.normal.xyz += position;

			o.noise = noise + 0.5;
			o.uv_HeightTexture = v.texcoord.xy;
			o.uv_NormalTexture = v.texcoord.xy;
			o.color = vcolor;
		}
		
		half _Mixing;
		half _Glossiness;
		half _Metallic;

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 terrainNoiseColor = fixed4(IN.noise, IN.noise, IN.noise, 1.0);
			fixed4 terrainColor = lerp(terrainNoiseColor, IN.color, _Mixing);
			fixed4 terrainTexture = lerp(tex2D(_HeightTexture, IN.uv_HeightTexture), terrainNoiseColor, _Mixing);
			fixed4 terrainNormalTexture = normalize(tex2D(_NormalTexture, IN.uv_NormalTexture));

			o.Albedo = terrainTexture.rgb;
			//o.Normal = UnpackNormal(terrainNormalTexture);
			//o.Normal = FindNormal(float4(IN.uv_NormalTexture, 0, 0), 1 / float2(240, 240));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
}