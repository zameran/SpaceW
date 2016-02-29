Shader "Unlit/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_WireframeBackgroundColor("Wireframe Background Color", Color) = (1,1,1,1)
		_Wireframe("Wireframe", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
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

			struct v2fg
			{
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float3 uv1 : TEXCOORD1;
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
			
			half4 _WireframeBackgroundColor;
			float _Wireframe;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;

			#ifdef SHADER_API_D3D11
			uniform StructuredBuffer<OutputStruct> data;
			#endif
		
			v2fg vert (in appdata_full_compute v)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				position.w = 1.0;
				position.xyz += patchCenter;

				v2fg o;
				o.vertex = mul(UNITY_MATRIX_MVP, position);
				o.uv = v.texcoord;
				o.uv1 = v.texcoord1;
				o.color = float4(noise, noise, noise, 1);
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2fg IN[3], inout TriangleStream<v2fg> triStream)
			{	
				float2 WIN_SCALE = float2(_ScreenParams.x / 2.0, _ScreenParams.y / 2.0);
				
				float2 p0 = WIN_SCALE * IN[0].vertex.xy / IN[0].vertex.w;
				float2 p1 = WIN_SCALE * IN[1].vertex.xy / IN[1].vertex.w;
				float2 p2 = WIN_SCALE * IN[2].vertex.xy / IN[2].vertex.w;
				
				float2 v0 = p2 - p1;
				float2 v1 = p2 - p0;
				float2 v2 = p1 - p0;

				float area = abs(v1.x * v2.y - v1.y * v2.x);
			
				v2fg OUT;
				OUT.vertex = IN[0].vertex;
				OUT.uv = IN[0].uv;
				OUT.uv1 = float3(area / length(v0), 0, 0);
				OUT.color = IN[0].color;
				triStream.Append(OUT);

				OUT.vertex = IN[1].vertex;
				OUT.uv = IN[1].uv;
				OUT.uv1 = float3(0, area / length(v1), 0);
				OUT.color = IN[1].color;
				triStream.Append(OUT);

				OUT.vertex = IN[2].vertex;
				OUT.uv = IN[2].uv;
				OUT.uv1 = float3(0, 0, area / length(v2));
				OUT.color = IN[2].color;
				triStream.Append(OUT);			
			}
			
			fixed4 frag (v2fg IN) : COLOR
			{
				float d = min(IN.uv1.x, min(IN.uv1.y, IN.uv1.z));
 				float I = exp2(-4.0 * d * d);

 				fixed4 wireframeColor = lerp(_WireframeBackgroundColor, IN.color, I);
				fixed4 terrainColor = fixed4(IN.color.x, IN.color.y, IN.color.z, 1);
				fixed4 outputColor = lerp(terrainColor, wireframeColor, _Wireframe);

 				return outputColor;		
			}
			ENDCG
		}
	}
	Fallback Off
}