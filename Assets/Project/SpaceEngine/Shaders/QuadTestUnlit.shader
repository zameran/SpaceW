Shader "Unlit/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			struct v2f
			{
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			struct OutputStruct
			{
				float noise;

				float3 patchCenter;

				float4 vcolor;
				float4 pos;
				float4 cpos;
			};
			
			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;

			#ifdef SHADER_API_D3D11
			uniform StructuredBuffer<OutputStruct> data;
			#endif
		
			v2f vert (in appdata_full_compute v)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				position.w = 1.0;
				position.xyz += patchCenter;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, position);
				o.uv = v.texcoord;
				o.color = float4(noise, noise, noise, 1);
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				return fixed4(i.color.x, i.color.y, i.color.z, 1);
			}
			ENDCG
		}
	}
	Fallback Off
}