Shader "SpaceEngine/Forest/TreeView" 
{
	SubShader 
	{
		CGINCLUDE
		#include "UnityCG.cginc"

		#include "../TCCommon.cginc"

		uniform sampler2D colorSampler;
		uniform float3 dir;
		uniform float4x4 worldToScreen;
		
		struct a2v
		{
			float3 p : POSITION;
			float2 uv : TEXCOORD0;
			float3 vertexAO : COLOR;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float3 fp : TEXCOORD0;
			float2 fuv : TEXCOORD1;
			float3 vertexAO : COLOR;
		};

		void vert(in a2v i, out v2f o)
		{
			i.p = i.p - float3(0, 0, 1);

			o.pos = mul(worldToScreen, float4(i.p, 1.0));
			o.fp = i.p;
			o.fuv = i.uv;
			o.vertexAO = i.vertexAO;
		}
			
		void frag(in v2f i, out float4 output : COLOR) 
		{
			if (i.fp.z < -1.0) discard;
			if (tex2D(colorSampler, i.fuv * 0.25).a < 0.5) discard;

			float t = (dot(i.fp, dir) + sqrt(2.0)) / (2.0 * sqrt(2.0));
			float ao = i.vertexAO.r;

			output = float4(t, t, ao, 1.0);
		}
		ENDCG

		Pass 
		{
			ZTest Always
			Cull Front 
			ZWrite On
			ColorMask RBA 0
			Fog { Mode Off }
			Offset 1, -1

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

		Pass
		{
			ZTest Less
			Cull Front 
			ZWrite On
			ColorMask G 0
			Fog { Mode Off }
			Offset -1, 1

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}