Shader "Unlit/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_WireframeColor("Wireframe Background Color", Color) = (0, 0, 0, 1)
		_Wireframe("Wireframe", Range(0, 1)) = 0.0
		_Side("Side", Range(0, 5)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}

		Pass
		{
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/TCCommon.cginc"

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
				float4 color : COLOR0;
				float4 light : COLOR1;
				float2 uv : TEXCOORD0;
				float3 uv1 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};
		
			uniform half4 _WireframeColor;
			uniform float _Wireframe;
			uniform float _Side;

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

				v.vertex = position;

				float3 normal = tex2Dlod(_NormalTexture, float4(v.texcoord.xy, 0, 0)).rgb;
				float3 normal_unpack = UnpackNormal(tex2Dlod(_NormalTexture, float4(v.texcoord.xy, 0, 0))).rgb;

				v.tangent = float4(FindTangent(tex2Dlod(_NormalTexture, float4(v.texcoord.xy, 0, 0)), 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;
				
				v.tangent.xyz += position.xyz;
				v.normal.xyz += position.xyz;

				//TANGENT_SPACE_ROTATION;
				//v.tangent.xyz = mul(v.tangent.xyz, rotation);
				//v.normal.xyz = mul(v.normal.xyz, rotation);

				//float3x3 tbn = TBN(normal);
				//v.tangent.xyz = mul(v.tangent.xyz, -tbn);
				//v.normal.xyz = mul(v.normal.xyz, -tbn);
				
				/*
				if (_Side == 0)//top
				{

				}
				else if (_Side == 1)//bottom
				{

				}
				else if (_Side == 2)//left
				{

				}
				else if (_Side == 3)//right
				{

				}
				else if (_Side == 4)//front
				{

				}
				else if (_Side == 5)//back
				{

				}
				*/

				float atten = 1.0;
				float3 normalDirection = normalize(mul(float4(v.normal, 0), _Object2World).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz - float3(0, 0, -8192));	// - float3(4096, 4096, 8192) || - float3(0, 0, -8192)
				float3 diffuseReflection = atten * _LightColor0.xyz * max(0, dot(normalDirection, lightDirection));
				float3 lightFinal = diffuseReflection * UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				v2fg o;

				//GLSL
				//vec3 color = mix(snowColor, slateColor, smoothstep(1.5, 1.6, slope + .2*fBm(worldSpacePosition, .4f, 1.918, 4)));
				//HLSL
				//float4 color = lerp(noiseColor, mapColor, smoothstep(1.5, 1.6, slope + 0.4 * Fbm(v.vertex * 0.0001, 4)));

				o.color = lerp(float4(noise, noise, noise, 1), tex2Dlod(_HeightTexture, v.texcoord), 1);	
				o.light = float4(lightFinal, 1);
				o.uv = v.texcoord;
				o.uv1 = v.texcoord1;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

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
				OUT.color = IN[0].color;
				OUT.light = IN[0].light;
				OUT.uv = IN[0].uv;
				OUT.uv1 = float3(area / length(v0), 0, 0);
				OUT.vertex = IN[0].vertex;
				triStream.Append(OUT);

				OUT.color = IN[1].color;
				OUT.light = IN[1].light;
				OUT.uv = IN[1].uv;
				OUT.uv1 = float3(0, area / length(v1), 0);
				OUT.vertex = IN[1].vertex;
				triStream.Append(OUT);

				OUT.color = IN[2].color;
				OUT.light = IN[2].light;
				OUT.uv = IN[2].uv;
				OUT.uv1 = float3(0, 0, area / length(v2));
				OUT.vertex = IN[2].vertex;		
				triStream.Append(OUT);			
			}

			void frag(v2fg IN, out half4 outDiffuse : COLOR0, out half4 outNormal : COLOR1)
			{
				float d = min(IN.uv1.x, min(IN.uv1.y, IN.uv1.z));
				float I = exp2(-4.0 * d * d);

				fixed4 terrainColor = fixed4(IN.color.x, IN.color.y, IN.color.z, 1);
				fixed4 wireframeColor = lerp(terrainColor, _WireframeColor, I);
				fixed4 outputColor = lerp(terrainColor, wireframeColor, _Wireframe);

				fixed3 terrainNormal = UnpackNormal(tex2D(_NormalTexture, IN.uv));
				fixed4 outputNormal = fixed4(terrainNormal, 1);

				outDiffuse = outputColor * IN.light * 2;	
				outNormal = outputNormal;	
			}
			ENDCG
		}
	}
	Fallback Off
}