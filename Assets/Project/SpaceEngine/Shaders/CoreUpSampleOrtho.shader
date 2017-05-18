Shader "SpaceEngine/Terrain/CoreUpSampleOrtho" 
{
	SubShader 
	{
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#include "UnityCG.cginc"

			#include "TCCommon.cginc"

			#define CORE_PORDUCER_ADDITIONAL_UV

			#include "Core.cginc"

			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			
			//x - size in pixels of one tile (including borders), 
			//y - size in meters of a pixel of the elevation texture, 
			//z - (tileWidth - 2 * BORDER) / grid mesh size for display.
			uniform float4 _TileWSD;

			uniform sampler2D _CoarseLevelSampler;		// Coarse level texture
			uniform float4 _CoarseLevelOSL;				// Lower left corner of patch to upsample, one over size in pixels of coarse level texture, layer id
			
			uniform sampler2D _ResidualSampler;
			uniform float4 _ResidualOSH;
			
			uniform sampler2D _NoiseSampler;
			uniform float4 _NoiseUVLH;
			uniform float4 _NoiseColor;
			uniform float4 _NoiseRootColor;
			
			static float4 _Masks[4] = 
			{
				float4(1.0, 3.0, 3.0, 9.0),
				float4(3.0, 1.0, 9.0, 3.0),
				float4(3.0, 9.0, 1.0, 3.0),
				float4(9.0, 3.0, 3.0, 1.0)
			};

			CORE_PRODUCER_VERTEX_PROGRAM(_TileWSD.x)
			
			void frag(in VertexProducerOutput IN, out float4 output : COLOR)
			{		
				float4 result = float4(128.0, 128.0, 128.0, 128.0);
				float2 uv = floor(IN.uv1);
				
				if (_ResidualOSH.x != -1.0) 
				{
					output = tex2Dlod(_ResidualSampler, float4(uv * _ResidualOSH.z + _ResidualOSH.xy, 0.0, 0.0));
				} 
				else if (_CoarseLevelOSL.x == -1.0) 
				{
					result = _NoiseRootColor * 255.0;
				}
				
				if (_CoarseLevelOSL.x != -1.0) 
				{
					float4 mask = _Masks[int(modi(uv.x, 2.0) + 2.0 * modi(uv.y, 2.0))];
					
					float4 _offset = float4(floor((uv + 1.0) * 0.5) * _CoarseLevelOSL.z + _CoarseLevelOSL.xy, 0.0, 0.0);
					
					float4 c0 = tex2Dlod(_CoarseLevelSampler, _offset) * 255.0;
					float4 c1 = tex2Dlod(_CoarseLevelSampler, _offset + float4(_CoarseLevelOSL.z, 0.0, 0.0, 0.0)) * 255.0;
					float4 c2 = tex2Dlod(_CoarseLevelSampler, _offset + float4(0.0, _CoarseLevelOSL.z, 0.0, 0.0)) * 255.0;
					float4 c3 = tex2Dlod(_CoarseLevelSampler, _offset + float4(_CoarseLevelOSL.zz, 0.0, 0.0)) * 255.0;
					
					float4 c = floor((mask.x * c0 + mask.y * c1 + mask.z * c2 + mask.w * c3) / 16.0);

					result = (result - 128.0) * -1.0 + c;
				}
				
				float2 nuv = (uv + 0.5) / _TileWSD.x;
				float4 uvs = float4(nuv, 1.0 - nuv);
				
				float2 noiseUV = float2(uvs[int(_NoiseUVLH.x)], uvs[int(_NoiseUVLH.y)]);
				float4 noise = tex2Dlod(_NoiseSampler, float4(noiseUV, 0.0, 0.0)) * 255.0;
				
				if (_NoiseUVLH.w == 1.0) 
				{
					float3 hsv = rgb2hsv(result.rgb / 255.0);

					hsv *= float3(1.0, 1.0, 1.0) + (1.0 - smoothstep(0.4, 0.8, hsv.z)) * _NoiseColor.rgb * (noise.rgb - 128.0) / 255.0;
					hsv.x = frac(hsv.x);
					hsv.y = clamp(hsv.y, 0.0, 1.0);
					hsv.z = clamp(hsv.z, 0.0, 1.0);
					
					result = float4(hsv2rgb(hsv) * 255.0, _NoiseColor.a * (noise.a - 128.0) + result.a);
				} 
				else 
				{
					result = _NoiseColor * (noise - 128.0) + result;
				}

				output = result / 255.0;
			}		
			ENDCG
		}
	}
}