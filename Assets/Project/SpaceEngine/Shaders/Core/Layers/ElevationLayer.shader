Shader "SpaceEngine/Layers/Elevation" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		#include "../../Core.cginc"

		uniform sampler2D _Target;
		uniform sampler2D _Source;
		uniform float3 _Coords;
		uniform float _Height;
		
		CORE_LAYER_VERTEX_PROGRAM
			
		void frag(in VertexLayerOutput IN, out float4 output : COLOR) 
		{
			float2 uv = _Coords.xy + IN.uv * _Coords.z;
			float z = tex2Dlod(_Source, float4(uv, 0.0f, 0.0f)).x - _Height;

			float4 mask = float4(1.0f, 1.0f, 1.0f, smoothstep(4.0f, 5.0f, z));
			
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