Shader "Custom/QuadTestNormal" 
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
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "TCCommon.cginc"

			#pragma vertex vert
			#pragma fragment frag

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
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;

				uint id : SV_VertexID;
			};

			struct v2f 		
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _HeightTexture;
			sampler2D _NormalTexture;

			#ifdef SHADER_API_D3D11
			StructuredBuffer<OutputStruct> data;
			#endif

			v2f vert(appdata_full_compute v) 
			{
				v2f o;

				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				position.w = 1.0;
				position.xyz += patchCenter;

				v.vertex = position;
				v.normal = tex2Dlod(_NormalTexture, v.texcoord);
				float direction = v.normal * normalize(patchCenter);
				float nDirection = normalize(direction);
				//v.normal *= direction;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color.xyz = v.normal * 0.5 + 0.5;
				o.color.w = 1.0;

				return o;
			}
		
			fixed4 frag(v2f i) : SV_Target
			{ 
				return i.color; 
			}
			ENDCG
		}
	}
}