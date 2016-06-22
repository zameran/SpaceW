Shader "SpaceEngine/PatchedSphereTest"
{
	Properties
	{

	}

	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "TCCommon.cginc"

			struct vtx_out
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 vol : TEXCOORD1;
			};

			vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1)
			{
				vtx_out OUT;

				OUT.position = position;
				OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;
				OUT.vol = float3(vol.x, vol.y, vol.z);

				return OUT;
			}

			float4 frag(vtx_out i) : COLOR
			{
				float3 p = normalize(i.vol);

				float h = Fbm(p, 4);

				// return the heightmap
				// r = normalmap dx (not filled yet -- will be filled by the normal generator)
				// g = normalmap dy (not filled yet -- will be filled by the normal generator)
				// b = slope (not filled yet -- will be filled by the normal generator)
				// a = height
				return h;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}