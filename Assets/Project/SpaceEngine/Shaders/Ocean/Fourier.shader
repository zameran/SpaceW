Shader "Math/Fourier" 
{
	CGINCLUDE

	#include "../Math.cginc"
			
	uniform sampler2D _ReadBuffer0;
	uniform sampler2D _ReadBuffer1;
	uniform sampler2D _ReadBuffer2;
	uniform sampler2D _ButterFlyLookUp;

	uniform float _Size;

	struct a2v
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f 
	{
		float4  pos : SV_POSITION;
		float2  uv : TEXCOORD0;
	};
	
	struct f2a_1
	{
		float4 col0 : COLOR0;
	};
	
	struct f2a_2
	{
		float4 col0 : COLOR0;
		float4 col1 : COLOR1;
	};
	
	struct f2a_3
	{
		float4 col0 : COLOR0;
		float4 col1 : COLOR1;
		float4 col2 : COLOR2;
	};
	
	//Performs two FFTs on two complex numbers packed in a vector4
	float4 FFT(float2 w, float4 input1, float4 input2) 
	{
		float rx = w.x * input2.x - w.y * input2.y;
		float ry = w.y * input2.x + w.x * input2.y;
		float rz = w.x * input2.z - w.y * input2.w;
		float rw = w.y * input2.z + w.x * input2.w;

		return input1 + float4(rx, ry, rz, rw);
	}

	inline float2 CalculateW(float4 lookUp)
	{
		return float2(cos(M_PI2 * lookUp.z / _Size), sin(M_PI2 * lookUp.z / _Size));
	}

	inline float4 CalculateLookUp(float uv, out float2 w)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(uv, 0));
		
		lookUp.xyz *= 255.0;
		lookUp.xy /= _Size - 1.0;
		
		w = CalculateW(lookUp);
		
		if(lookUp.w > 0.5) w *= -1.0;

		return lookUp;
	}
	
	void vert(in a2v i, out v2f o)
	{
		o.pos = UnityObjectToClipPos(i.vertex);
		o.uv = i.texcoord;
	}

	void fragX_1(in v2f IN, out f2a_1 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.x, w);
		
		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
	}
	
	void fragY_1(in v2f IN, out f2a_1 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.y, w);
		
		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
	}
	
	void fragX_2(in v2f IN, out f2a_2 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.x, w);
		
		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
	}
	
	void fragY_2(in v2f IN, out f2a_2 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.y, w);
		
		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
	}
	
	void fragX_3(in v2f IN, out f2a_3 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.x, w);
		
		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));
	}
	
	void fragY_3(in v2f IN, out f2a_3 OUT)
	{
		float2 w = 0;
		float4 lookUp = CalculateLookUp(IN.uv.y, w);
		
		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);
		
		OUT.col0 = FFT(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));
	}
	
	ENDCG
			
	SubShader 
	{
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_1
			ENDCG
		}
		
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_1
			ENDCG
		}
		
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_2
			ENDCG
		}
		
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_2
			ENDCG
		}
		
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_3
			ENDCG
		}
		
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_3
			ENDCG
		}
	}
}