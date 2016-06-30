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
				float3 volume : TEXCOORD1;
			};

			vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 volume : TEXCOORD1)
			{
				vtx_out OUT;

				OUT.position = position;
				OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;
				OUT.volume = volume;

				return OUT;
			}

			float4 frag(vtx_out i) : COLOR
			{
				float3 p = normalize(i.volume);

				//float h = HeightMapClouds(p);
				float h = Fbm(p, 4);

				// return the heightmap
				// r = normalmap dx (not filled yet -- will be filled by the normal generator)
				// g = normalmap dy (not filled yet -- will be filled by the normal generator)
				// b = slope (not filled yet -- will be filled by the normal generator)
				// a = height
				return float4(0, 0, 0, h);
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float _UVStep;
			uniform sampler2D _Heightmap;
			uniform float _HeightScale;

			struct vtx_out
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			float2 GetSobelNormal(float2 uv, float uvStep)
			{
				float u = uv.x;
				float v = uv.y;

				float tl = tex2D(_Heightmap, float2(u - uvStep, v - uvStep)).a * _HeightScale;
				float l  = tex2D(_Heightmap, float2(u - uvStep, v)).a * _HeightScale;
				float bl = tex2D(_Heightmap, float2(u - uvStep, v + uvStep)).a * _HeightScale;
				float b  = tex2D(_Heightmap, float2(u, v + uvStep)).a * _HeightScale;
				float br = tex2D(_Heightmap, float2(u + uvStep, v + uvStep)).a * _HeightScale;
				float r  = tex2D(_Heightmap, float2(u + uvStep, v)).a * _HeightScale;
				float tr = tex2D(_Heightmap, float2(u + uvStep, v - uvStep)).a * _HeightScale;
				float t  = tex2D(_Heightmap, float2(u, v - uvStep)).a * _HeightScale;

				float dX = tr + 2.0 * r + br - tl - 2.0 * l - bl;
				float dY = bl + 2.0 * b + br - tl - 2.0 * t - tr;

				return float2(dX, dY);
			}

			vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0)
			{
				vtx_out OUT;

				OUT.position = position;
				OUT.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, uv).xy;

				return OUT;
			}

			float4 frag(vtx_out i) : COLOR
			{
				float height = tex2D(_Heightmap, i.uv).a;

				float2 d = GetSobelNormal(i.uv, _UVStep);

				float3 n = normalize(float3(-d.x, -d.y, 2.0));

				float slope = max(abs(n.x), abs(n.y));

				// return the packed heightmap with normals
				// r = normalmap dx
				// g = normalmap dy
				// b = slope
				// a = height
				return float4(n.xy, slope, height);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}