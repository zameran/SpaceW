Shader "SpaceEngine/Terrain/UpSampleOrtho" 
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

			#include "../TCCommon.cginc"

			#pragma target 4.0

			#pragma vertex vert
			#pragma fragment frag
			
			// size in pixels of one tile (including borders)
			uniform float _TileWidth;
			// coarse level texture
			uniform sampler2D _CoarseLevelSampler; 
			// lower left corner of patch to upsample, one over size in pixels of coarse level texture, layer id
			uniform float4 _CoarseLevelOSL;
			
			uniform sampler2D _ResidualSampler;
			uniform float4 _ResidualOSH;
			
			uniform sampler2D _NoiseSampler;
			uniform float4 _NoiseUVLH;
			uniform float4 _NoiseColor;
			uniform float4 _NoiseRootColor;
			
			static float4 masks[4] = 
			{
				float4(1.0, 3.0, 3.0, 9.0),
				float4(3.0, 1.0, 9.0, 3.0),
				float4(3.0, 9.0, 1.0, 3.0),
				float4(9.0, 3.0, 3.0, 1.0)
			};
		
			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
				float2  st : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.uv = v.texcoord.xy;
				OUT.st = v.texcoord.xy * _TileWidth;

				return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float4 result = float4(128.0, 128.0, 128.0, 128.0);
				float2 uv = floor(IN.st);
				
				if (_ResidualOSH.x != -1.0) 
				{
					return tex2Dlod(_ResidualSampler, float4(uv * _ResidualOSH.z + _ResidualOSH.xy, 0.0, 0.0));
				} 
				else if (_CoarseLevelOSL.x == -1.0) 
				{
					result = _NoiseRootColor * 255.0;
				}
				
				if (_CoarseLevelOSL.x != -1.0) 
				{
					float4 mask = masks[int(modi(uv.x, 2.0) + 2.0 * modi(uv.y, 2.0))];
					
					float4 _offset = float4(floor((uv + float2(1.0,1.0)) * 0.5) * _CoarseLevelOSL.z + _CoarseLevelOSL.xy, 0.0, 0.0);
					
					float4 c0 = tex2Dlod(_CoarseLevelSampler, _offset) * 255.0;
					float4 c1 = tex2Dlod(_CoarseLevelSampler, _offset + float4(_CoarseLevelOSL.z, 0.0, 0.0, 0.0)) * 255.0;
					float4 c2 = tex2Dlod(_CoarseLevelSampler, _offset + float4(0.0, _CoarseLevelOSL.z, 0.0, 0.0)) * 255.0;
					float4 c3 = tex2Dlod(_CoarseLevelSampler, _offset + float4(_CoarseLevelOSL.zz, 0.0, 0.0)) * 255.0;
					
					float4 c = floor((mask.x * c0 + mask.y * c1 + mask.z * c2 + mask.w * c3) / 16.0);

					result = (result - 128.0) * -1.0 + c;
				}
				
				float2 nuv = (uv + float2(0.5, 0.5)) / _TileWidth;
				float4 uvs = float4(nuv, float2(1.0, 1.0) - nuv);
				
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

				return result / 255.0;
			}
			
			ENDCG
		}
	}
}