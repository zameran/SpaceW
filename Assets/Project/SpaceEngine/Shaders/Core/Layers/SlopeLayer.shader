Shader "SpaceEngine/Layers/Slope" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "../../Core.cginc"

		uniform sampler2D _Target;
		uniform sampler2D _Source;
		uniform float3 _Coords;
		
		CORE_LAYER_VERTEX_PROGRAM
			
		void frag(in VertexLayerOutput IN, out float4 output : COLOR) 
		{
			float2 uv = _Coords.xy + IN.uv * _Coords.z;
			float2 n = tex2Dlod(_Source, float4(uv, 0.0f, 0.0f)).xy;
	
			float z = sqrt(1.0f - dot(n, n));
				
			float4 mask = float4(1.0f, 1.0f, 1.0f, smoothstep(0.85f, 0.9f, z));
			
			output = tex2D(_Target, IN.uv) * mask;
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