Shader "SpaceEngine/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_PlanetUVSampler("Planet UV Sampler (RGBA)", 2D) = "white" {}
		_QuadTexture1("QuadTexture 1 (RGB)", 2D) = "white" {}
		_QuadTexture2("QuadTexture 2 (RGB)", 2D) = "white" {}
		_QuadTexture3("QuadTexture 3 (RGB)", 2D) = "white" {}
		_QuadTexture4("QuadTexture 4 (RGB)", 2D) = "white" {}
		_Normale("Normale", Range(0, 1)) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

		Pass
		{
			ZWrite On
			ZTest LEqual 
			Cull Back
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile LIGHT_1 LIGHT_2 LIGHT_3 LIGHT_4
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/TCCommon.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/HDR.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Atmosphere.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/LogarithmicDepthBuffer.cginc"

			struct appdata_full_compute 
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;

				uint id : SV_VertexID;
			};

			struct v2fg
			{
				float2 uv0 : TEXCOORD0;
				float3 normal0 : NORMAL0;
				float4 vertex0 : POSITION0;
				float4 vertex1 : POSITION1;
				float4 tangent0 : TANGENT0;
				float depth : DEPTH;

				float3 direction : TEXCOORD1;
			};

			uniform float _Normale;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;
			uniform sampler2D _PlanetUVSampler;

			uniform sampler2D _QuadTexture1;
			uniform sampler2D _QuadTexture2;
			uniform sampler2D _QuadTexture3;
			uniform sampler2D _QuadTexture4;

			uniform StructuredBuffer<OutputStruct> data;
			uniform StructuredBuffer<QuadGenerationConstants> quadGenerationConstants;

			uniform float4x4 TRS;

			uniform float _LODLevel;

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}

			inline float4 GroundFinalColorWithoutAtmosphere(float4 terrainColor, float3 p, float n, float3 WSD)
			{
				float cTheta = dot(n, -WSD);

				return terrainColor * max(cTheta, 0);
			}

			inline float4 GroundFinalColorWithAtmosphere(float4 terrainColor, float3 p, float3 n, float3 d, float3 WSD, float4 WSPR)
			{	
				float3 sunL = 0;
				float3 skyE = 0;
				float3 extinction = 0;

				p += _Globals_Origin;

				float cTheta = dot(n, -WSD);
	
				SunRadianceAndSkyIrradiance(p, n, WSD, sunL, skyE);

				#ifdef ECLIPSES_ON
					float eclipse = 1;

					eclipse *= EclipseShadow(p, WSD, WSPR.w);
				#endif

				float4 inscatter = InScattering(WCPG, p, WSD, extinction, 1.0);

				#ifdef ECLIPSES_ON
					inscatter *= eclipse;
				#endif

				#ifdef SHINE_ON
					inscatter += float4(SkyShineRadiance(p, d, _Sky_ShineOccluders_1, _Sky_ShineColors_1), 0.0);
				#endif

				float3 groundColor = 1.5 * RGB2Reflectance(terrainColor).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;

				#ifdef ECLIPSES_ON
					extinction = 1 * _ExtinctionGroundFade + (1 - _ExtinctionGroundFade) * extinction * eclipse;
				#endif

				float4 finalColor = float4(groundColor, 1) * float4(extinction, 1) + inscatter;
				
				return finalColor;
			}

			void Account(in float4 terrainColor, out float4 scatteringColor, float3 p, float3 n, float3 d) //AtmosphereInToTheAccount
			{
				scatteringColor = 0;

				#ifdef LIGHT_1
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_1, _Sun_Positions_1[0]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_1);
					#endif
				#endif

				#ifdef LIGHT_2	
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_1, _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_2, _Sun_Positions_1[1]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_1);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_2);
					#endif
				#endif

				#ifdef LIGHT_3
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_1, _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_2, _Sun_Positions_1[1]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_3, _Sun_Positions_1[2]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_1);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_2);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_3);
					#endif
				#endif

				#ifdef LIGHT_4
					#ifdef ATMOSPHERE_ON
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_1, _Sun_Positions_1[0]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_2, _Sun_Positions_1[1]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_3, _Sun_Positions_1[2]));
						scatteringColor += hdr(GroundFinalColorWithAtmosphere(terrainColor, p, n, d, _Sun_WorldSunDir_4, _Sun_Positions_1[3]));
					#endif

					#ifdef ATMOSPHERE_OFF
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_1);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_2);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_3);
						scatteringColor += GroundFinalColorWithoutAtmosphere(terrainColor, p, n, _Sun_WorldSunDir_4);
					#endif
				#endif
			}

			void vert(in appdata_full_compute v, out v2fg o)
			{
				float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 position = data[v.id].position;
				float4 cubePosition = data[v.id].cubePosition;

				float3 normal = tex2Dlod(_NormalTexture, v.texcoord);

				position.w = 1.0;
				position.xyz += patchCenter;

				cubePosition.w = 1.0;
				cubePosition.xyz += patchCenter;

				v.vertex = position;
				v.tangent = float4(FindTangent(normal, 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;

				o.uv0 = v.texcoord;
				o.normal0 = v.normal;
				o.vertex0 = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vertex1 = mul(_Object2World, v.vertex); //TODO : Apply Origin vector. //NOTE : Bug here!!!!!111
				o.tangent0 = v.tangent;
				o.depth = 1;

				o.direction = normalize(((_Globals_WorldCameraPos + _Globals_Origin) - mul(_Globals_CameraToWorld, o.vertex1)).xyz);

				//Log. depth
				//o.vertex0.z = log2(max(1e-6, 1.0 + o.vertex0.w)) * (2.0 / log2(_ProjectionParams.z + 1.0)) - 1.0;
				//o.vertex0.z *= o.vertex0.w;
				//o.depth = log2(1.0 + o.vertex0.w) * (0.5 * (2.0 / log2(_ProjectionParams.z + 1.0)));

				//Log. depth new; FarPlane = _ProjectionParams.z | _ProjectionParams.w;
				//FarPlane = UNITY_MATRIX_P[2].w / (UNITY_MATRIX_P[2].z + 1.0);
				//o.vertex0.z = log2(max(1e-6, 1.0 + o.vertex0.w)) * FCoef(1e+2) - 1.0;
				//o.depth = 1.0 + o.vertex0.w;
			}

			void frag(in v2fg IN, out float4 outDiffuse : COLOR)//, out float depth : DEPTH)
			{		
				QuadGenerationConstants constants = quadGenerationConstants[0];

				float4 scatteringColor = 0;
				fixed4 terrainColor = tex2D(_HeightTexture, IN.uv0);
				fixed4 uvSamplerColor = tex2D(_PlanetUVSampler, IN.uv0);
				fixed4 outputNormal = fixed4(IN.normal0, 1);

				float height = tex2D(_HeightTexture, IN.uv0).a;
				float slope = tex2D(_NormalTexture, IN.uv0).a;

				float2 uvTest = SinACosAUV_Z(IN.vertex1.xyz);
				float4 uvTestSampler = tex2D(_PlanetUVSampler, uvTest);

				Account(terrainColor, scatteringColor, IN.vertex1.xyz, IN.normal0.xyz, IN.direction);

				//outDiffuse = FindNormal(_HeightTexture, IN.uv0); 
				//outDiffuse = uvSamplerColor;
				outDiffuse = lerp(scatteringColor, outputNormal, _Normale);
				//depth = log2(IN.depth) * (0.5 * FCoef(1e+2));
			}
			ENDCG
		}

        /*Pass
        {
            Name "ShadowCaster"
            Tags
            { 
                "LightMode" = "ShadowCaster" 
                "IgnoreProjector" = "True"
            }

            ZWrite On

            CGPROGRAM
            #pragma target 5.0

            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }*/
	}
}