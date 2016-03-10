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
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

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
				float4 color : COLOR0;
				float4 scatter : COLOR1;
				float2 uv : TEXCOORD0;
				float3 uv1 : TEXCOORD1;
				float3 uv2 : TEXCOORD2;
				float3 normal : NORMAL;
				float4 vertex : POSITION;
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
				float3 normal_unpack = UnpackNormal(half4(normal, 0)).rgb;

				//float3 up = normalize(v.vertex);
				//float3 right = normalize(cross(up, float3(0, 1, 0)));
				//float3 forward = normalize(cross(right, up));
 
				//float3 localNormal = float3(dot(normal, right), dot(normal, up), dot(normal, forward));
				//float3 localNormalUnpack = UnpackNormal(half4(localNormal, 0)).rgb;

				v.tangent = float4(FindTangent(normal, 0.01, float3(0, 1, 0)), 1);
				v.normal = normal;
				
				//v.tangent.xyz += position.xyz;
				//v.normal.xyz += position.xyz;

				//TANGENT_SPACE_ROTATION;
				//v.tangent.xyz = mul(v.tangent.xyz, rotation);
				//v.normal.xyz = mul(v.normal.xyz, rotation);

				float4 terrainColor = tex2Dlod(_HeightTexture, float4(v.texcoord.xy, 0, 0));
				float3 WCP = _Globals_WorldCameraPos;
				float3 WSD = _Sun_WorldSunDir;
    			float3 p = v.vertex.xyz;
				float3 fn;
				//fn.xy = normal.xy;
				fn.xy = -normal.xy; //invert z to make it work!		
				//fn.z = sqrt(max(0.0, 1.0 - dot(fn.xy, fn.xy)));
   				fn.z = sqrt(max(0.0, -1.0 + dot(fn.xy, fn.xy))); //invert z to make it work!		
    			float4 reflectance = terrainColor;
    			reflectance.rgb = tan(1.37 * reflectance.rgb) / tan(1.37); //RGB to reflectance
    			float3 sunL;
			    float3 skyE;
			    SunRadianceAndSkyIrradiance(p, fn, WSD, sunL, skyE);
		    	float cTheta = dot(fn, WSD); // diffuse ground color
			    float3 groundColor = 1.5 * reflectance.rgb * (sunL * max(cTheta, 0.0) + skyE) / 3.14159265;
			    float3 extinction;
			    float3 inscatter = InScattering(WCP, p, WSD, extinction, 0.0);
			    float3 finalColor = hdr(groundColor * extinction + inscatter);

				v2fg o;

				o.color = lerp(float4(noise, noise, noise, 1), terrainColor, 1);	
				o.scatter = float4(finalColor, 1);
				o.uv = v.texcoord;
				o.uv1 = v.texcoord1;
				o.uv2 = v.texcoord2;
				o.normal = v.normal;
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
				OUT.scatter = IN[0].scatter;
				OUT.uv = IN[0].uv;
				OUT.uv1 = float3(area / length(v0), 0, 0);
				OUT.uv2 = IN[0].uv2;
				OUT.normal = IN[0].normal;
				OUT.vertex = IN[0].vertex;
				triStream.Append(OUT);

				OUT.color = IN[1].color;
				OUT.scatter = IN[1].scatter;
				OUT.uv = IN[1].uv;
				OUT.uv1 = float3(0, area / length(v1), 0);
				OUT.uv2 = IN[1].uv2;
				OUT.normal = IN[1].normal;
				OUT.vertex = IN[1].vertex;
				triStream.Append(OUT);

				OUT.color = IN[2].color;
				OUT.scatter = IN[2].scatter;
				OUT.uv = IN[2].uv;
				OUT.uv1 = float3(0, 0, area / length(v2));
				OUT.uv2 = IN[2].uv2;
				OUT.normal = IN[2].normal;
				OUT.vertex = IN[2].vertex;	
				triStream.Append(OUT);			
			}

			void frag(v2fg IN, out float4 outDiffuse : COLOR0, out float4 outNormal : COLOR1)
			{		
				float d = min(IN.uv1.x, min(IN.uv1.y, IN.uv1.z));
				float I = exp2(-4.0 * d * d);

				float4 terrainColor = IN.scatter;
				fixed4 wireframeColor = lerp(terrainColor, _WireframeColor, I);
				fixed4 outputColor = lerp(terrainColor, wireframeColor, _Wireframe);

				fixed3 terrainNormal = UnpackNormal(tex2D(_NormalTexture, IN.uv));
				fixed4 outputNormal = fixed4(terrainNormal, 1);

				outDiffuse = outputColor;
				outNormal = outputNormal;	
			}
			ENDCG
		}
	}
	Fallback Off
}