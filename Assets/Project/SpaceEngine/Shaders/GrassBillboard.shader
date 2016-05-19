//http://answers.unity3d.com/questions/973067/using-shadow-texture-to-recieve-shadows-on-grass.html
Shader "Custom/GrassBillboard" 
{
	Properties 
	{
		_MainTex ("Grass Texture", 2D) = "white" {}
		_NoiseTexture ("Noise Texture", 2D) = "white" {}
		_HealthyColor ("Healthy Color", Color) = (0,1,0,1)
		_DryColor ("Dry Color", Color) = (1,1,0,1)
		_MinSize ("Min Size", float) = 1
		_MaxSize ("Max Size", float) = 2
		_MaxCameraDistance ("Max Camera Distance", float) = 250
		_Transition ("Transition", float) = 30
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
		_Freq ("Frequency", Range(0.1, 100)) = 0.1
	}

	SubShader 
	{
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
				#pragma vertex vertexShader
				#pragma fragment fragmentShader
				#pragma geometry geometryShader
				
				#pragma multi_compile_fwdbase
				#pragma multi_compile_fog

				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				struct VS_INPUT
				{
					float4 position : POSITION;
					float4 uv_Noise : TEXCOORD0;
					fixed sizeFactor : TEXCOORD1;
				};

				struct GS_INPUT
				{
					float4 worldPosition : TEXCOORD0;
					fixed2 parameters : TEXCOORD1;	// .x = noiseValue, .y = sizeFactor
				};

				struct FS_INPUT
				{
					float4 pos	: SV_POSITION;		// has to be called this way because of unity MACRO for light
					float2 uv_MainTexture : TEXCOORD0;
					float4 tint : COLOR0;
					LIGHTING_COORDS(1,2)
					UNITY_FOG_COORDS(3)
				};

				uniform sampler2D _MainTex, _NoiseTexture;

				// for billboard
				uniform fixed _Cutoff;
				uniform fixed _Freq;
				uniform float _MinSize, _MaxSize;
				uniform fixed4 _HealthyColor, _DryColor;
				uniform float _MaxCameraDistance;
				uniform float _Transition;
				
				uniform float4 _LightColor0;


				// Vertex Shader ------------------------------------------------
				GS_INPUT vertexShader(VS_INPUT vIn)
				{
					GS_INPUT vOut;
					
					// set output values
					vOut.worldPosition =  mul(_Object2World, vIn.position);
					vOut.parameters.x = tex2Dlod(_NoiseTexture, vIn.uv_Noise * _Freq).a;
					vOut.parameters.y = vIn.sizeFactor;

					return vOut;
				}


				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void geometryShader(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					// cutout trough a transition area
					float cameraDistance = length(_WorldSpaceCameraPos - p[0].worldPosition);
					
					// discard billboards that are too far away
					if (cameraDistance > _MaxCameraDistance)
						return;

					float t = (cameraDistance - (_MaxCameraDistance - _Transition)) / _Transition;
					float alpha = clamp (1, 0, lerp (1.0, 0.0, t));
					
					// billboard's normal is a projection plane's normal
					float3 viewDirection = UNITY_MATRIX_IT_MV[2].xyz;
					viewDirection.y = 0;
					viewDirection = normalize(viewDirection);
					
					float3 up = float3(0, 1, 0);
					float3 right = normalize(cross(up, viewDirection));
					
					// billboard's size and color
					fixed noiseValue = p[0].parameters.x;
					fixed sizeFactor = p[0].parameters.y;
					float size = lerp(_MinSize, _MaxSize, noiseValue * sizeFactor);
					float halfSize = 0.5f * size;
					float4 tint = lerp(_HealthyColor, _DryColor, noiseValue);
					tint.a = alpha;

					// create billboard
					float4 v[4];
					v[0] = float4(p[0].worldPosition + halfSize * right, 1.0f);
					v[1] = float4(p[0].worldPosition + halfSize * right + size * up, 1.0f);
					v[2] = float4(p[0].worldPosition - halfSize * right, 1.0f);
					v[3] = float4(p[0].worldPosition - halfSize * right + size * up, 1.0f);

					// matrix to transfer vertices from world to screen space
					float4x4 vpMatrix = mul(UNITY_MATRIX_MVP, _World2Object);
					
					FS_INPUT fIn;
					
					fIn.pos = mul(vpMatrix, v[0]);
					fIn.uv_MainTexture = float2(1.0f, 0.0f);
					fIn.tint = tint;
					TRANSFER_VERTEX_TO_FRAGMENT(fIn);
					UNITY_TRANSFER_FOG(fIn,fIn.pos);
					
					triStream.Append(fIn);

					fIn.pos =  mul(vpMatrix, v[1]);
					fIn.uv_MainTexture = float2(1.0f, 1.0f);
					fIn.tint = tint;
					TRANSFER_VERTEX_TO_FRAGMENT(fIn);
					UNITY_TRANSFER_FOG(fIn,fIn.pos);

					triStream.Append(fIn);

					fIn.pos =  mul(vpMatrix, v[2]);
					fIn.uv_MainTexture = float2(0.0f, 0.0f);
					fIn.tint = tint;
					TRANSFER_VERTEX_TO_FRAGMENT(fIn);
					UNITY_TRANSFER_FOG(fIn,fIn.pos);

					triStream.Append(fIn);

					fIn.pos =  mul(vpMatrix, v[3]);
					fIn.uv_MainTexture = float2(0.0f, 1.0f);
					fIn.tint = tint;
					TRANSFER_VERTEX_TO_FRAGMENT(fIn);
					UNITY_TRANSFER_FOG(fIn,fIn.pos);

					triStream.Append(fIn);
				}

				// Fragment Shader -----------------------------------------------
				float4 fragmentShader(FS_INPUT fIn) : COLOR
				{
					fixed4 color = tex2D(_MainTex, fIn.uv_MainTexture) * fIn.tint;
					if (color.a < _Cutoff) 
						discard;

					float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					float atten = LIGHT_ATTENUATION(fIn);

					float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
					float3 normal = float3(0,1,0);
					float3 lambert = float(max(0.0,dot(normal,lightDirection)));
					float3 lighting = (ambient + lambert * atten) * _LightColor0.rgb;

					color = fixed4 (color.rgb * lighting, 1.0f);
					
					UNITY_APPLY_FOG(fIn.fogCoord, color);

					return color;
				}

			ENDCG
		}

		// shadow caster
		Pass
		{
			//Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
	 
			Fog { Mode Off }
			ZWrite On ZTest LEqual
			
			CGPROGRAM
	
			#pragma vertex	vertexShader
			#pragma geometry geometryShader
			#pragma fragment fragmentShader
	 
			//#pragma multi_compile_shadowcaster
			//#pragma only_renderers d3d11
			//#define SHADOW_CASTER_PASS
	 
			#include "UnityCG.cginc"
			#include "HLSLSupport.cginc"
	 
			struct VS_INPUT
			{
				float4 position : POSITION;
				float4 uv_Noise : TEXCOORD0;
				fixed sizeFactor : TEXCOORD1;
			};

			struct SHADOW_VERTEX
			{
				float4 vertex : POSITION; // has to be called this way because of unity macro
			};

			struct GS_INPUT
			{
				float4 worldPosition : TEXCOORD0;
				fixed2 parameters : TEXCOORD1;	// .x = noiseValue, .y = sizeFactor
			};

			struct FS_INPUT
			{
				float2 uv_MainTexture : TEXCOORD0;
				V2F_SHADOW_CASTER;
			};

			uniform sampler2D _MainTex, _NoiseTexture;

			// for billboard
			uniform fixed _Cutoff;
			uniform fixed _Freq;
			uniform float _MinSize, _MaxSize;
			uniform float _MaxCameraDistance;
			uniform float _Transition;

		   GS_INPUT vertexShader (VS_INPUT v)
		   {
				GS_INPUT vOut;

				// set output values
				vOut.worldPosition =  mul(_Object2World, v.position);
				vOut.parameters.x = tex2Dlod(_NoiseTexture, v.uv_Noise * _Freq).a;
				vOut.parameters.y = v.sizeFactor;

				return vOut;
		   }
	 
		   // Geometry Shader
		   [maxvertexcount(4)]
		   void geometryShader(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream )
		   {
				// cutout trough a transition area
				float cameraDistance = length(_WorldSpaceCameraPos - p[0].worldPosition);
				
				// discard billboards that are too far away
				if (cameraDistance > _MaxCameraDistance)
					return;
				
				float t = (cameraDistance - (_MaxCameraDistance - _Transition)) / _Transition;
				float alpha = clamp (1, 0, lerp (1.0, 0.0, t));

				// billboard's normal is a projection plane's normal
				float3 viewDirection = UNITY_MATRIX_IT_MV[2].xyz;
				viewDirection.y = 0;
				viewDirection = normalize(viewDirection);
					
				float3 up = float3(0, 1, 0);
				float3 right = normalize(cross(up, viewDirection));
					
				// size of billboard
				fixed noiseValue = p[0].parameters.x;
				fixed sizeFactor = p[0].parameters.y;
				float size = lerp(_MinSize, _MaxSize, noiseValue * sizeFactor);
				float halfSize = 0.5f * size;

				// create billboard
				float4 vertices[4];
				vertices[0] = float4(p[0].worldPosition + halfSize * right, 1.0f);
				vertices[1] = float4(p[0].worldPosition + halfSize * right + size * up, 1.0f);
				vertices[2] = float4(p[0].worldPosition - halfSize * right, 1.0f);
				vertices[3] = float4(p[0].worldPosition - halfSize * right + size * up, 1.0f);

				FS_INPUT fIn;

				SHADOW_VERTEX v;
				v.vertex = mul (_World2Object, vertices[0]);
				fIn.uv_MainTexture = float2(1.0f, 0.0f);
				TRANSFER_SHADOW_CASTER(fIn)		// uses "v.vertex" for vertex position
					
				triStream.Append(fIn);
				
				v.vertex = mul (_World2Object, vertices[1]);
				fIn.uv_MainTexture = float2(1.0f, 1.0f);
				TRANSFER_SHADOW_CASTER(fIn)          

				triStream.Append(fIn);

				v.vertex = mul (_World2Object, vertices[2]);
				fIn.uv_MainTexture = float2(0.0f, 0.0f);
				TRANSFER_SHADOW_CASTER(fIn)          

				triStream.Append(fIn);

				v.vertex = mul (_World2Object, vertices[3]);
				fIn.uv_MainTexture = float2(0.0f, 1.0f);
				TRANSFER_SHADOW_CASTER(fIn)          

				triStream.Append(fIn);
			}
	 
			fixed4 fragmentShader (FS_INPUT fIn) : COLOR
			{
				fixed4 color = tex2D(_MainTex, fIn.uv_MainTexture);
				if (color.a < _Cutoff) 
					discard;

				SHADOW_CASTER_FRAGMENT(fIn)
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}