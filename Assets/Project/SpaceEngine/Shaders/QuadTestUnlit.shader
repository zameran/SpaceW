Shader "Unlit/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_WireframeColor("Wireframe Background Color", Color) = (0, 0, 0, 1)
		_Wireframe("Wireframe", Range(0, 1)) = 0.0
		_Normale("Normale", Range(0, 1)) = 0.0
		_Side("Side", Range(0, 5)) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		Pass
		{
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/TCCommon.cginc"
			#include "Assets/Project/ProlandAtmosphere/Shaders/Utility.cginc"
			#include "Assets/Project/ProlandAtmosphere/Shaders/Atmosphere.cginc"

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
				float4 terrainColor : COLOR0;
				float4 scatterColor : COLOR1;
				float2 uv0 : TEXCOORD0;
				float3 uv1 : TEXCOORD1;
				float3 uv2 : TEXCOORD2;
				float3 normal0 : NORMAL0;
				float3 normal1 : NORMAL1;
				float4 vertex : POSITION;
			};
		
			uniform half4 _WireframeColor;
			uniform float _Wireframe;
			uniform float _Normale;
			uniform float _Side;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;

			uniform float4x4 _WorldToTangentFrame;
			uniform float3 _Origin;

			#ifdef SHADER_API_D3D11
			uniform StructuredBuffer<OutputStruct> data;
			#endif

			float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}

			float4 GroundFinalColor(float4 terrainColor, float3 p, float3 n)
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WSD = _Sun_WorldSunDir;
				
				float3 fn;
				//fn.xy = n.xy - default.
				//fn.xy = -n.xy - inverted.
				fn.xy = -n.xy;

				//1.0 - dot(fn.xy, fn.xy) - default.
				//-1.0 + dot(fn.xy, fn.xy) - inverted.
				fn.z = sqrt(max(0.0, -1.0 + dot(fn.xy, fn.xy)));

				//fn = float3(0, 0, 0); //disable normal mapping... bruuuutaal!
				
				float4 reflectance = RGB2Reflectance(terrainColor);

				float3 sunL;
				float3 skyE;

				//SunRadianceAndSkyIrradiance(p, fn, WSD, sunL, skyE); - default.
				//SunRadianceAndSkyIrradiance(p, p / 256, WSD, sunL, skyE); - disabled normal mapping for irradiance, but keeping color in bueaty...

				SunRadianceAndSkyIrradiance(p, fn, WSD, sunL, skyE);

				float cTheta = dot(fn, WSD); // diffuse ground color

				float3 groundColor = 1.5 * reflectance.rgb * (sunL * max(cTheta, 0.1) + skyE) / M_PI;
				float3 extinction;
				float4 inscatter = InScattering(WCP, p, WSD, extinction, 1.0);
				float4 finalColor = float4(groundColor, 1) * float4(extinction, 1) + inscatter;
				
				return finalColor;
			}
	
			v2fg vert (in appdata_full_compute v)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				float3 normal = tex2Dlod(_NormalTexture, v.texcoord).rgb;

				position.w = 1.0;
				position.xyz += patchCenter;

				v.vertex = position;
				v.tangent = float4(FindTangent(normal, 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;

				//v.tangent.xyz = mul(v.tangent.xyz, _WorldToTangentFrame);
				//v.normal.xyz = mul(v.normal.xyz, _WorldToTangentFrame);

				//v.tangent.xyz += position.xyz;
				//v.normal.xyz += position.xyz;

				//v.tangent.xyz = mul(UNITY_MATRIX_MVP, position.xyz);
				//v.normal.xyz = mul(UNITY_MATRIX_MVP, position.xyz);

				//TANGENT_SPACE_ROTATION;
				//v.tangent.xyz = mul(v.tangent.xyz, rotation);
				//v.normal.xyz = mul(v.normal.xyz, rotation);

				float4 terrainColor = tex2Dlod(_HeightTexture, v.texcoord);
				float4 groundFinalColor = GroundFinalColor(terrainColor, v.vertex.xyz, v.normal.xyz);
				float4 scatteringColor = float4(hdr(groundFinalColor.xyz), groundFinalColor.w);

				v2fg o;

				o.terrainColor = terrainColor;	
				o.scatterColor = scatteringColor;
				o.uv0 = v.texcoord;
				o.uv1 = v.texcoord1;
				o.uv2 = v.texcoord2;
				o.normal0 = v.normal;
				o.normal1 = normal;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				return o;
			}

			v2fg PutData(v2fg FROM, float3 customUV1)
			{	
				v2fg OUT;

				OUT.terrainColor = FROM.terrainColor;
				OUT.scatterColor = FROM.scatterColor;
				OUT.uv0 = FROM.uv0;
				OUT.uv1 = customUV1;
				OUT.uv2 = FROM.uv2;
				OUT.normal0 = FROM.normal0;
				OUT.normal1 = FROM.normal1;
				OUT.vertex = FROM.vertex;

				return OUT;
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
			
				triStream.Append(PutData(IN[0], float3(area / length(v0), 0, 0)));
				triStream.Append(PutData(IN[1], float3(0, area / length(v1), 0)));
				triStream.Append(PutData(IN[2], float3(0, 0, area / length(v2))));
			}

			void frag(v2fg IN, out float4 outDiffuse : COLOR0, out float4 outNormal : COLOR1)
			{		
				float d = min(IN.uv1.x, min(IN.uv1.y, IN.uv1.z));
				float I = exp2(-4.0 * d * d);

				float4 terrainColor = IN.scatterColor;
				fixed4 wireframeColor = lerp(terrainColor, _WireframeColor, I);
				fixed4 outputColor = lerp(terrainColor, wireframeColor, _Wireframe);

				fixed3 terrainWorldNormal = IN.normal0;
				//fixed3 terrainLocalNormal = CalculateSurfaceNormal_HeightMap(IN.vertex, IN.normal0, IN.terrainColor.a); //IN.normal1;
				fixed4 outputNormal = float4(terrainWorldNormal, 1); //fixed4(terrainWorldNormal * terrainLocalNormal, 1);

				outDiffuse = lerp(outputColor, outputNormal, _Normale);
				outNormal = outputNormal;	
			}
			ENDCG
		}
	}
	Fallback Off
}