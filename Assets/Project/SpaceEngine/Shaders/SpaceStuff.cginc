#define SHADOW_BLUR

#if !defined (M_PI)
#define M_PI 3.14159265358
#endif

#if !defined (M_PI2)
#define M_PI2 6.28318530716
#endif

#if LIGHT_1 || LIGHT_2 || LIGHT_3 || LIGHT_4
float4 _Light1Color;
float4 _Light1Position;
float3 _Light1Direction;

#if LIGHT_2
float4 _Light2Color;
float4 _Light2Position;
float3 _Light2Direction;
#endif

#if LIGHT_3
float4 _Light3Color;
float4 _Light3Position;
float3 _Light3Direction;
#endif

#if LIGHT_4
float4 _Light4Color;
float4 _Light4Position;
float3 _Light4Direction;
#endif

float ComputeMiePhase(float _CosTheta, float _MiePhaseAnisotropy)
{
	float squareAniso = _MiePhaseAnisotropy * _MiePhaseAnisotropy;

	float Num = 1.5 * (1.0 + _CosTheta * _CosTheta) * (1.0 - squareAniso);
	float Den = (8.0 + squareAniso) * pow( abs(1.0 + squareAniso - 2.0 * _MiePhaseAnisotropy * _CosTheta), 1.5 );

	return Num / Den;
}

float MiePhase(float angle, float4 mie)
{
	return ComputeMiePhase(mie.y, mie.y) / pow(mie.z - mie.x * angle, mie.w);
}

float RayleighPhase(float angle, float rayleigh)
{
	return rayleigh * angle * angle;
}

float RayleighPhaseFunction(float mu, float rayleigh) 
{
	return rayleigh * (3.0 / (16.0 * M_PI)) * (1.0 + mu * mu);
}

float MieRayleighPhase(float angle, float4 mie, float rayleigh)
{
	return MiePhase(angle, mie) + RayleighPhase(angle, rayleigh);
}

float MieRayleighPhaseFunction(float angle, float4 mie, float rayleigh) 
{
	 return MiePhase(angle, mie) + RayleighPhaseFunction(angle, rayleigh);
}
#endif

#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
float4x4  _Shadow1Matrix;
sampler2D _Shadow1Texture;
float     _Shadow1Ratio;

#if SHADOW_2
float4x4  _Shadow2Matrix;
sampler2D _Shadow2Texture;
float     _Shadow2Ratio;
#endif

#if SHADOW_3
float4x4  _Shadow3Matrix;
sampler2D _Shadow3Texture;
float     _Shadow3Ratio;
#endif

#if SHADOW_4
float4x4  _Shadow4Matrix;
sampler2D _Shadow4Texture;
float     _Shadow4Ratio;
#endif

float4 BlurShadow(sampler2D inputTexture, float2 inputUV, float inputStep = 0.0001f)
{
	float2 blurCoordinates[5];

	blurCoordinates[0] = inputUV.xy;
	blurCoordinates[1] = inputUV.xy + inputStep * 1.407333;
	blurCoordinates[2] = inputUV.xy - inputStep * 1.407333;
	blurCoordinates[3] = inputUV.xy + inputStep * 3.294215;
	blurCoordinates[4] = inputUV.xy - inputStep * 3.294215;

	float4 bluredColor = float4(0, 0, 0, 0);

	bluredColor += tex2D(inputTexture, blurCoordinates[0]) * 0.204164;
	bluredColor += tex2D(inputTexture, blurCoordinates[1]) * 0.304005;
	bluredColor += tex2D(inputTexture, blurCoordinates[2]) * 0.304005;
	bluredColor += tex2D(inputTexture, blurCoordinates[3]) * 0.093913;
	bluredColor += tex2D(inputTexture, blurCoordinates[4]) * 0.093913;

	return bluredColor;
}

float4 ShadowColor(float4x4 shadowMatrix, sampler2D shadowSampler, float shadowRatio,  float4 worldPoint)
{
	float4 shadowPoint = mul(shadowMatrix, worldPoint);
	float2 shadowMag   = length(shadowPoint.xy);
	
	shadowMag = 1.0f - (1.0f - shadowMag) * shadowRatio;
	
	float4 shadow = 0;
	
	#ifdef SHADOW_BLUR
		shadow = BlurShadow(shadowSampler, shadowMag);
	#else
		shadow = tex2D(shadowSampler, shadowMag.xy);
	#endif
	
	shadow += shadowPoint.z < 0.0f ? 1.0f : 0.0f;
	
	return saturate(shadow);
}

float4 ShadowColor(float4 worldPoint)
{
	float4 color = ShadowColor(_Shadow1Matrix, _Shadow1Texture, _Shadow1Ratio, worldPoint);
	
	#if SHADOW_2
		color *= ShadowColor(_Shadow2Matrix, _Shadow2Texture, _Shadow2Ratio, worldPoint);
	#endif

	#if SHADOW_3
		color *= ShadowColor(_Shadow3Matrix, _Shadow3Texture, _Shadow3Ratio, worldPoint);
	#endif

	#if SHADOW_4
		color *= ShadowColor(_Shadow4Matrix, _Shadow4Texture, _Shadow4Ratio, worldPoint);
	#endif
	
	return color;
}
#endif