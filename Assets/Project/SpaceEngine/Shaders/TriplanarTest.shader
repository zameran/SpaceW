Shader "Unlit/TriplanarTest"
{
	Properties
	{
		_TBTexture("Top/Buttom Diffuse Map", 2D) = "white" {}
		_LRTexture("Left/Right Diffuse Map", 2D) = "white" {}
		_FBTexture("Front/Back Diffuse Map", 2D) = "white" {}
		_TextureScale("Texture Scale", Range(0, 32)) = 0.0
		_TriplanarBlendSharpness("Triplanar Blend Sharpness", Range(0, 8)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Core/Core.cginc"

			uniform sampler2D _TBTexture;
			uniform sampler2D _LRTexture;
			uniform sampler2D _FBTexture;
			uniform float _TextureScale;
			uniform float _TriplanarBlendSharpness;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;

				float3 worldNormal : TEXCOORD1;
				float3 worldPosition : TEXCOORD2;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;

				o.worldNormal = v.normal.xyz;
				o.worldPosition = mul(UNITY_MATRIX_M, v.vertex).xyz;

				return o;
			}

			fixed4 frag (v2f IN) : SV_Target
			{
				return Triplanar(_TBTexture, _LRTexture, _FBTexture, IN.worldPosition, IN.worldNormal, float2(_TextureScale, _TriplanarBlendSharpness));
			}
			ENDCG
		}
	}
}
