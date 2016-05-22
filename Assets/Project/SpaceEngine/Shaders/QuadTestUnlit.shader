Shader "SpaceEngine/QuadTestUnlit"
{
	Properties
	{
		_HeightTexture("Height (RGBA)", 2D) = "white" {}
		_NormalTexture("Normal (RGBA)", 2D) = "white" {}
		_GrassTexture("GrassTexture (RGB)", 2D) = "white" {}
		_WireframeColor("Wireframe Background Color", Color) = (0, 0, 0, 1)
		_Atmosphere("Atmosphere", Range(0, 1)) = 0.0
		_Wireframe("Wireframe", Range(0, 1)) = 0.0
		_Normale("Normale", Range(0, 1)) = 0.0
		_Side("Side", Range(0, 5)) = 0.0
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

		/*
		Pass 
	    {
	        Fog { Mode Off }
			//Cull Front
			//ZTest Off
			        
			CGPROGRAM
			 
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma glsl

			#include "UnityCG.cginc"
			#include "Assets/Project/SpaceEngine/Shaders/Compute/Utils.cginc"

			float3 _Godray_WorldSunDir;

			struct appdata 
			{
			    float4 vertex : POSITION;
			    float3 normal : NORMAL;
			    float4 texcoord : TEXCOORD0;

			    uint id : SV_VertexID;
			};

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;
			uniform StructuredBuffer<OutputStruct> data;
			uniform StructuredBuffer<QuadGenerationConstants> quadGenerationConstants;

			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 depth : TEXCOORD0;
			};

			float3 LinePlaneIntersection(float3 linePoint, float3 lineVec, float3 planeNormal, float3 planePoint)
			{	
				float lineLength;
				float dotNumerator;
				float dotDenominator;
					
				float3 intersectVector;
				float3 intersection = 0;

				//calculate the distance between the linePoint and the line-plane intersection point
				dotNumerator = dot((planePoint - linePoint), planeNormal);
				dotDenominator = dot(lineVec, planeNormal);
			 
				//line and plane are not parallel
				//if(dotDenominator != 0.0f)
				//{
					lineLength =  dotNumerator / dotDenominator;
			  		intersection= (lineLength > 600.0) ? linePoint + normalize(lineVec) * (lineLength - 600) : linePoint;

					return intersection;	
				//}
				//else //output not valid
				//{
					//return false;
				//}
			}
					  
			v2f vert (appdata v) 
			{
				//v2f o;

				//float4 _LightDirWorldSpace = float4(_Godray_WorldSunDirX,_Godray_WorldSunDirY,_Godray_WorldSunDirZ,0.0);
				//float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir,0.0);
				//float3 _LightDirObjectSpace = mul(_World2Object,_LightDirWorldSpace);

				//float3 toLight=normalize(_LightDirObjectSpace);

				//float backFactor = dot( toLight, v.normal );

				//float extrude = (backFactor < 0.0) ? 1.0 : 0.0;
				//v.vertex.xyz -= toLight * (extrude * 1000000);
				//    
				//o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				//o.depth = o.pos.zw;
				//return o;

			    v2f o;

			    float noise = data[v.id].noise;
				float3 patchCenter = data[v.id].patchCenter;
				float4 vcolor = data[v.id].vcolor;
				float4 position = data[v.id].pos;

				float3 normal = tex2Dlod(_NormalTexture, v.texcoord);

				position.w = 1.0;
				position.xyz += patchCenter;

				float4 pos = position;
				float3 nor = normal;

				nor = float3(0, 0, nor.z);

				float4 _LightDirWorldSpace = float4(_Godray_WorldSunDir, 0.0);
				float3 _LightDirObjectSpace = mul(_World2Object, _LightDirWorldSpace);
			
				float3 _LightDirViewSpace = mul(UNITY_MATRIX_V, float4(_LightDirObjectSpace, 0.0)); 
				pos = mul(UNITY_MATRIX_MV, pos);  //both in view space

			    float3 toLight = normalize(_LightDirViewSpace);
			    
			    float backFactor = dot(toLight, mul(UNITY_MATRIX_MV, float4(nor, 0.0)));
			   	float backfaceFactor = dot(float3(0, 0, 1), mul(UNITY_MATRIX_MV, float4(nor, 0.0)));
			   	backfaceFactor = (backfaceFactor < 0.0) ? 1.0 : 0.0;
			   
			    float extrude = (backFactor < 0.0) ? 1.0 : 0.0;
			    
			    float towardsSunFactor = dot(toLight, float3(0, 0, 1));
			   	float projectOnNearPlane = (towardsSunFactor < 0.0) ? 1.0 : 0.0;
			   	
				//v.vertex.xyz -= toLight * (extrude * 1000000);
				//v.vertex.xyz -= toLight * (extrude  *  1000000);
				
				pos.xyz = (projectOnNearPlane * extrude > 0.0) ? LinePlaneIntersection(pos.xyz, -toLight,float3(0, 0, 1), 0) : (pos.xyz = pos.xyz - toLight * (extrude  *  1000000));
				//v.vertex.xyz = (projectOnNearPlane * extrude > 0.0) ? LinePlaneIntersection(v.vertex.xyz, -toLight,float3(0, 0, 1), float3(0, 0, 600)) : (v.vertex.xyz = v.vertex.xyz - toLight * (extrude * 1000000));

			    o.pos = mul(UNITY_MATRIX_P, pos);
			    o.depth = o.pos.zw;

			    o.pos.z = log2(max(1e-6, 10000.0 + o.pos.w)) * (2.0 / log2(_ProjectionParams.z + 1.0)) - 1.0;
				o.pos.z *= v.vertex.w;

			    return o;
			}
			 
			float4 frag(v2f i) : COLOR 
			{
			    return i.depth.x / i.depth.y;
			}

			ENDCG
	    }
	    */

		Pass
		{
			ZWrite On
			ZTest LEqual
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11
			#pragma vertex vert
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

			float3 _Godray_WorldSunDir;
		
			uniform half4 _WireframeColor;

			uniform float _Atmosphere;
			uniform float _Normale;
			uniform float _Side;

			uniform sampler2D _HeightTexture;
			uniform sampler2D _NormalTexture;
			uniform sampler2D _GrassTexture;

			uniform float3 _Rotation;

			uniform float4x4 _Globals_CameraToWorld;
			uniform float4x4 _Globals_ScreenToCamera;

			uniform float4x4 _TTW;

			uniform float3 _Sun_Position;

			uniform StructuredBuffer<OutputStruct> data;
			uniform StructuredBuffer<QuadGenerationConstants> quadGenerationConstants;

			inline float4 RGB2Reflectance(float4 inColor)
			{
				return float4(tan(1.37 * inColor.rgb) / tan(1.37), inColor.a);
			}

			float4 GroundFinalColorWithoutAtmosphere(float4 terrainColor, float3 p, float n, float3 WSD)
			{
				return terrainColor;
			}

			float4 GroundFinalColorWithAtmosphere(float4 terrainColor, float3 p, float3 n, float3 WSD, float4 WSPR, float2 uv)
			{	
				QuadGenerationConstants constants = quadGenerationConstants[0];
	
				float4 grass = tex2D(_GrassTexture, p.xyz / (constants.splitLevel));

				float3 WCP = _Globals_WorldCameraPos;

				float4 reflectance = RGB2Reflectance(terrainColor);

				float3 sunL = 0;
				float3 skyE = 0;
				float3 extinction = 0;

				float extinctionGroundFade = 0.000025;
				float cTheta = dot(n, -WSD);

				SunRadianceAndSkyIrradiance(p, n, WSD, sunL, skyE);

				float eclipse = EclipseShadow(p, WSD, WSPR.w);
				float4 inscatter = InScattering(WCP, p, WSD, extinction, 1.0);

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

			void frag(v2fg IN, out float4 outDiffuse : COLOR0)
			{		
				QuadGenerationConstants constants = quadGenerationConstants[0];

				float4 scatteringColor = IN.scatterColor;
				fixed4 terrainColor = IN.terrainColor;

				#ifdef LIGHT_1
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1, _Sun_Positions_1[0], IN.uv0) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1) : 
														  groundFinalColor1;
				#endif

				#ifdef LIGHT_2
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1, _Sun_Positions_1[0]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_2, _Sun_Positions_1[1]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2) : 
														  groundFinalColor1 + groundFinalColor2;
				#endif

				#ifdef LIGHT_3
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1, _Sun_Positions_1[0]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_2, _Sun_Positions_1[1]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor3 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_3, _Sun_Positions_1[2]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2 + groundFinalColor3) : 
														  groundFinalColor1 + groundFinalColor2 + groundFinalColor3;
				#endif

				#ifdef LIGHT_4
				float4 groundFinalColor1 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1, _Sun_Positions_1[0]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor2 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_2, _Sun_Positions_1[1]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor3 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_3, _Sun_Positions_1[2]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				float4 groundFinalColor4 = _Atmosphere > 0.0 ? 
										   GroundFinalColorWithAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_4, _Sun_Positions_1[3]) : 
										   GroundFinalColorWithoutAtmosphere(terrainColor, IN.vertex1.xyz, IN.normal0.xyz, _Sun_WorldSunDir_1);

				scatteringColor = _Atmosphere > 0.0 ? hdr(groundFinalColor1 + groundFinalColor2 + groundFinalColor3 + groundFinalColor4) : 
														  groundFinalColor1 + groundFinalColor2 + groundFinalColor3 + groundFinalColor4;
				#endif

				//IN.normal0 = mul(_TTW, IN.normal0);

				fixed3 terrainWorldNormal = IN.normal0;
				fixed3 terrainLocalNormal = CalculateSurfaceNormal_HeightMap(IN.vertex1, IN.normal0, IN.terrainColor.a);
				fixed4 outputNormal = fixed4(terrainWorldNormal, 1); //fixed4(terrainWorldNormal * terrainLocalNormal, 1);

				outDiffuse = lerp(scatteringColor, outputNormal, _Normale);
			}
			ENDCG
		}
	}
}