Shader "SpaceEngine/Terrain/CoreNormals" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "TCCommon.cginc"

		#define CORE_PORDUCER_ADDITIONAL_UV
		//#define BORDER 2.0	// Tile border size

		#include "Core.cginc"

		uniform sampler2D _ElevationSampler; 

		uniform float3 _TileSD;

		uniform float4 _ElevationOSL; 			
		uniform float4 _Deform;
		uniform float4 _PatchCornerNorms;

		uniform float4x4 _PatchCorners;
		uniform float4x4 _PatchVerticals;
		uniform float4x4 _WorldToTangentFrame;

		uniform float _Level;

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 st : TEXCOORD1;
		};

		float3 GetWorldPosition(float2 uv, float h) 
		{
			uv = uv / (_TileSD.x - 1.0);
				
			// NOTE : Body shape dependent...
			float R = _Deform.w;

			float4 L = _PatchCornerNorms;
			float4 uvUV = float4(uv, float2(1.0, 1.0) - uv);
			float4 aL = uvUV.zxzx * uvUV.wwyy;
			float4 alphaPrime = aL * L / dot(aL, L);
			float4 up = mul(_PatchVerticals, alphaPrime);

			float k = lerp(length(up.xyz), 1.0, smoothstep(R / 32.0, R / 64.0, _Deform.z));
			float hPrime = (h + R * (1.0 - k)) / k;
				
			return (mul(_PatchCorners, alphaPrime) + hPrime * up).xyz;
		}

		inline float4 SampleElevationData(sampler2D elevationSampler, float2 uv)
		{
			return tex2D(elevationSampler, uv);
		}

		inline float GetHeight(sampler2D elevationSampler, float2 uv)
		{
			return SampleElevationData(elevationSampler, uv).x;
		}

		inline float GetNoise(sampler2D elevationSampler, float2 uv)
		{
			return SampleElevationData(elevationSampler, uv).w;
		}

		float3 CalculateNormal(float2 uv)
		{
			uv = floor(uv);

			const float4 OFFSET_H = float4(-1.0, 0.0, 1.0, 0.0);
			const float4 OFFSET_V = float4(0.0, -1.0, 0.0, 1.0);

			float4 uv0 = floor(uv.xyxy + OFFSET_H) * _ElevationOSL.z + _ElevationOSL.xyxy;
			float4 uv1 = floor(uv.xyxy + OFFSET_V) * _ElevationOSL.z + _ElevationOSL.xyxy;

			float4 Z = float4(GetHeight(_ElevationSampler, uv0.xy),
							  GetHeight(_ElevationSampler, uv0.zw),
							  GetHeight(_ElevationSampler, uv1.xy),
							  GetHeight(_ElevationSampler, uv1.zw));

			float3 p0 = GetWorldPosition(uv + OFFSET_H.xy, Z.x).xyz;
			float3 p1 = GetWorldPosition(uv + OFFSET_H.zw, Z.y).xyz;
			float3 p2 = GetWorldPosition(uv + OFFSET_V.xy, Z.z).xyz;
			float3 p3 = GetWorldPosition(uv + OFFSET_V.zw, Z.w).xyz;

			return (mul((float3x3)_WorldToTangentFrame, normalize(cross(p1 - p0, p3 - p2)))).xyz;
		}

		float CalculateSlope(float3 normal)
		{
			// NOTE : First of all we need to take in to the account actual normal vector.
			normal = DecodeNormal(normal);

			return clamp(1.0 - pow(normal.z, 6.0), 0.0, 1.0);
		}

		float CalculateSteepness(float2 uv)
		{
			uv = floor(uv);

			const float4 OFFSET_H = float4(-1.0, 0.0, 1.0, 0.0);
			const float4 OFFSET_V = float4(0.0, -1.0, 0.0, 1.0);

			float4 uv0 = floor(uv.xyxy + OFFSET_H) * _ElevationOSL.z + _ElevationOSL.xyxy;
			float4 uv1 = floor(uv.xyxy + OFFSET_V) * _ElevationOSL.z + _ElevationOSL.xyxy;

			float dfdu = (GetNoise(_ElevationSampler, uv0.zw) - GetNoise(_ElevationSampler, uv0.xy)) / 2.0;
			float dfdv = (GetNoise(_ElevationSampler, uv1.zw) - GetNoise(_ElevationSampler, uv1.xy)) / 2.0;
			
			float steepness = abs(dfdu) + abs(dfdv);
			//float steepness = sqrt(dfdu * dfdu + dfdv * dfdv);

			// TODO : Value degenerating along LOD subdivision depth...
			return saturate(steepness * (_Level + 2.0) / 2.0);
		}

		CORE_PRODUCER_VERTEX_PROGRAM(_TileSD.x)

		void frag(in VertexProducerOutput IN, out float4 output : COLOR)
		{
			float3 normal = CalculateNormal(IN.uv1);
			//float slope = CalculateSlope(normal);
			//float steepness = CalculateSteepness(IN.uv1);

			output = EncodeNormalAndSlope(normal, 1.0);
		}
		ENDCG

		Pass
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag			
			ENDCG
		}
	}
}