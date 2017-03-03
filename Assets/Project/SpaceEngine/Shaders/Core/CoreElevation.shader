Shader "SpaceEngine/Terrain/CoreElevation" 
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
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "../TCCommon.cginc"
			#include "ImprovedPerlinNoise3D.cginc"
			
			#define BORDER 2.0 
			
			uniform float _TileSize;
			
			uniform float _Amplitude;
			uniform float4 _Offset;
			uniform float4x4 _LocalToWorld;
			 
			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f OUT;

				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.uv = v.texcoord.xy;

				return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{			
				float u = (0.5 + BORDER) / (_TileSize - 1 - BORDER * 2);

				float2 vert = IN.uv * (1.0 + u * 2.0) - u;

				vert = vert * _Offset.z + _Offset.xy;
				
				float3 P = float3(vert, _Offset.w);
				float3 p = normalize(mul(_LocalToWorld, P)).xyz;

				noiseH          = 0.5;
				noiseLacunarity = 2.218281828459;
				noiseOctaves	= 8;

				float3 position = p * _Frequency;

				float noise = 0;

				noise += Fbm(position * 0.25, 2);
				noise += Fbm(position * 0.50, 4);
				noise += Fbm(position * 0.75, 6);
				noise += Fbm(position * 1.00, 8);

				float output = _Amplitude * noise;
							
				return float4(output, output, 0.0, 0.0);
				
			}
			
			ENDCG
		}
	}
}