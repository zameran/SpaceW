Shader "Unlit/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_WireframeColor("Wireframe Background Color", Color) = (0, 0, 0, 1)
		_Atmosphere("Atmosphere", Range(0, 1)) = 0.0
		_Wireframe("Wireframe", Range(0, 1)) = 0.0
		_Normale("Normale", Range(0, 1)) = 0.0
		_Side("Side", Range(0, 5)) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

		Pass
		{
			ZWrite On
			ZTest LEqual  
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			//#pragma geometry geom //FPS drop down on geometry shader program.
			#pragma fragment frag

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4

			#pragma enable_d3d11_debug_symbols //RenderDoc debugging

			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/TCCommon.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Utility.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Atmosphere.cginc"

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
				float3 uv3 : TEXCOORD3;
				float3 normal0 : NORMAL0;
				float3 normal1 : NORMAL1;
				float4 vertex0 : POSITION0;
				float4 vertex1 : POSITION1;
				float depth : DEPTH;
			};
		
			uniform half4 _WireframeColor;

			uniform float _Atmosphere;
			uniform float _Wireframe;
			uniform float _Normale;
			uniform float _Side;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;

			uniform float3 _Rotation;

			uniform float3 _Origin;

			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;
			uniform float3 _Globals_Origin;

			uniform float4x4 _TTW;

			uniform float3 _Sun_Position;

			uniform StructuredBuffer<OutputStruct> data;
			uniform StructuredBuffer<QuadGenerationConstants> quadGenerationConstants;

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}

			float GroundFinalColorWithoutAtmosphere(appdata_full_compute v, float4 terrainColor, float3 p, float n)
			{
				float4 finaColor = float4(terrainColor.xyz, 1);

				return finaColor;
			}

			float4 GroundFinalColorWithAtmosphere(appdata_full_compute v, float4 terrainColor, float3 p, float3 n, float3 WSD)
			{	
				QuadGenerationConstants constants = quadGenerationConstants[0];
	
				float3 WCP = _Globals_WorldCameraPos;
				
				float3 originalPoint = p;
				float3 originalNormal = n;

				float3 rotatedPoint = Rotate(_Rotation.x, float3(1, 0, 0), 
									  Rotate(_Rotation.y, float3(0, 1, 0), 
									  Rotate(_Rotation.z, float3(0, 0, 1), originalPoint)));	

				float3 rotatedNormal = Rotate(_Rotation.x, float3(1, 0, 0), 
									   Rotate(_Rotation.y, float3(0, 1, 0), 
									   Rotate(_Rotation.z, float3(0, 0, 1), originalNormal)));	

				n = rotatedNormal;

				//n = normalize(rotatedPoint + rotatedNormal); //So. Good variant, but without normal bumping.
				//n = normalize(n + WSD); //use this for normal light intens. but disabled normals.

				//n.xy = n.xy; // - default.
				//n.xy = -n.xy; // - inverted.

				//n.z = sqrt(max(0.0, 1.0 - dot(n.xy, n.xy))); // - default.
				//n.z = sqrt(max(0.0, -1.0 + dot(n.xy, n.xy))); // - inverted.

				//n = float3(0, 0, 0); //disable normal mapping... bruuuutaal!

				//n = mul(_TTW, n);

				float4 reflectance = RGB2Reflectance(terrainColor);

				float3 sunL = 0;
				float3 skyE = 0;
				float3 extinction = 0;

				float extinctionGroundFade = 0.000025;
				float cTheta = dot(n, -WSD);

				SunRadianceAndSkyIrradiance(rotatedPoint, n, WSD, sunL, skyE);

				//float eclipse = ApplyEclipse(WCP, WCP - rotatedPoint, _Globals_Origin);
				float eclipse = EclipseShadow(rotatedPoint, WSD, _Sun_WorldSunPosRadius_1.w);
				float4 inscatter = InScattering(WCP, rotatedPoint, WSD, extinction, 1.0);

				float3 groundColor = 1.5 * reflectance.rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;

				extinction *= eclipse;
				extinction = float3(1.0, 1.0, 1.0) * extinctionGroundFade + (1 - extinctionGroundFade) * extinction;

				float4 finalColor = float4(groundColor, 1) * float4(extinction, 1) + inscatter * eclipse;
				
				return finalColor;
			}
	
			v2fg vert (in appdata_full_compute v)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				float3 normal = tex2Dlod(_NormalTexture, v.texcoord);

				position.w = 1.0;
				position.xyz += patchCenter;

				v.vertex = position;
				v.tangent = float4(FindTangent(normal, 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;

				float4 terrainColor = tex2Dlod(_HeightTexture, v.texcoord);
				float4 scatteringColor = float4(0, 0, 0, 0);

				#ifdef LIGHT_1
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_1) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1) : 
													  	  groundFinalColor1;
				#endif

				#ifdef LIGHT_2
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_1) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_2) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2) : 
														  groundFinalColor1 + groundFinalColor2;
				#endif

				#ifdef LIGHT_3
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_1) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_2) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor3 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_3) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2 + groundFinalColor3) : 
														  groundFinalColor1 + groundFinalColor2 + groundFinalColor3;
				#endif

				#ifdef LIGHT_4
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_1) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_2) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor3 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_3) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				float4 groundFinalColor4 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz, _Sun_WorldSunDir_4) : 
										   GroundFinalColorWithoutAtmosphere(v, terrainColor, v.vertex.xyz, v.normal.xyz);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2 + groundFinalColor3 + groundFinalColor4) : 
														  groundFinalColor1 + groundFinalColor2 + groundFinalColor3 + groundFinalColor4;
				#endif

				v2fg o;

				o.terrainColor = terrainColor;	
				o.scatterColor = scatteringColor;
				o.uv0 = v.texcoord;
				o.uv1 = v.texcoord1;
				o.uv2 = v.texcoord2;
				o.uv3 = v.texcoord3;
				o.normal0 = v.normal;
				o.normal1 = v.normal;
				o.vertex0 = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vertex1 = v.vertex;
				o.depth = 1;

				//Log. depth
				//o.vertex0.z = log2(max(1e-6, 1.0 + o.vertex0.w)) * (2.0 / log2(_ProjectionParams.z + 1.0)) - 1.0;
				//o.vertex0.z *= o.vertex0.w;
				//o.depth = log2(1.0 + o.vertex0.w) * (0.5 * (2.0 / log2(_ProjectionParams.z + 1.0)));

				return o;
			}

			inline v2fg PutData(v2fg FROM)
			{	
				v2fg OUT;

				OUT.terrainColor = FROM.terrainColor;
				OUT.scatterColor = FROM.scatterColor;
				OUT.uv0 = FROM.uv0;
				OUT.uv1 = FROM.uv1;
				OUT.uv2 = FROM.uv2;
				OUT.uv3 = FROM.uv3;
				OUT.normal0 = FROM.normal0;
				OUT.normal1 = FROM.normal1;
				OUT.vertex0 = FROM.vertex0;
				OUT.vertex1 = FROM.vertex1;
				OUT.depth = FROM.depth;

				return OUT;
			}

			inline v2fg PutData(v2fg FROM, float3 customUV1)
			{	
				v2fg OUT;

				OUT.terrainColor = FROM.terrainColor;
				OUT.scatterColor = FROM.scatterColor;
				OUT.uv0 = FROM.uv0;
				OUT.uv1 = customUV1;
				OUT.uv2 = FROM.uv2;
				OUT.uv3 = FROM.uv3;
				OUT.normal0 = FROM.normal0;
				OUT.normal1 = FROM.normal1;
				OUT.vertex0 = FROM.vertex0;
				OUT.vertex1 = FROM.vertex1;
				OUT.depth = FROM.depth;

				return OUT;
			}

			[maxvertexcount(16)]
			void geom(triangle v2fg IN[3], inout TriangleStream<v2fg> triStream)
			{	
				if (_Wireframe > 0)
				{
					float2 SCREEN_SCALE = float2(_ScreenParams.x / 2.0, _ScreenParams.y / 2.0);
				
					float2 p0 = SCREEN_SCALE * IN[0].vertex0.xy / IN[0].vertex0.w;
					float2 p1 = SCREEN_SCALE * IN[1].vertex0.xy / IN[1].vertex0.w;
					float2 p2 = SCREEN_SCALE * IN[2].vertex0.xy / IN[2].vertex0.w;
				
					float2 v0 = p2 - p1;
					float2 v1 = p2 - p0;
					float2 v2 = p1 - p0;

					float area = abs(v1.x * v2.y - v1.y * v2.x);
			
					triStream.Append(PutData(IN[0], float3(area / length(v0), 0, 0)));
					triStream.Append(PutData(IN[1], float3(0, area / length(v1), 0)));
					triStream.Append(PutData(IN[2], float3(0, 0, area / length(v2))));
				}
				else
				{
					triStream.Append(IN[0]);
					triStream.Append(IN[1]);
					triStream.Append(IN[2]);
				}
			}

			void frag(v2fg IN, out float4 outDiffuse : COLOR0)
			{		
				QuadGenerationConstants constants = quadGenerationConstants[0];

				float d = min(IN.uv1.x, min(IN.uv1.y, IN.uv1.z));
				float I = exp2(-4.0 * d * d);

				float4 terrainColor = IN.scatterColor;
				fixed4 wireframeColor = lerp(terrainColor, _WireframeColor, I);
				fixed4 outputColor = lerp(terrainColor, wireframeColor, _Wireframe);

				IN.normal0 = mul(_TTW, IN.normal0);

				fixed3 terrainWorldNormal = IN.normal0;
				fixed3 terrainLocalNormal = CalculateSurfaceNormal_HeightMap(IN.vertex1, IN.normal0, IN.terrainColor.a);
				fixed4 outputNormal = fixed4(terrainWorldNormal, 1); //fixed4(terrainWorldNormal * terrainLocalNormal, 1);

				outDiffuse = lerp(outputColor, outputNormal, _Normale);
			}
			ENDCG
		}
	}
	Fallback Off
}