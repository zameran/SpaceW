#if SHADOW_1 || SHADOW_2
float4x4  _Shadow1Matrix;
sampler2D _Shadow1Texture;
float     _Shadow1Ratio;

#if SHADOW_2
float4x4  _Shadow2Matrix;
sampler2D _Shadow2Texture;
float     _Shadow2Ratio;
#endif

float4 ShadowColor(float4x4 shadowMatrix, sampler2D shadowSampler, float shadowRatio,  float4 worldPoint)
{
	float4 shadowPoint = mul(shadowMatrix, worldPoint);
	float  shadowMag   = length(shadowPoint.xy);
	
	shadowMag = 1.0f - (1.0f - shadowMag) * shadowRatio;
	
	float4 shadow = tex2D(shadowSampler, shadowMag.xx);
	
	shadow += shadowPoint.z < 0.0f;
	
	return saturate(shadow);
}

float4 ShadowColor(float4 worldPoint)
{
	float4 color = ShadowColor(_Shadow1Matrix, _Shadow1Texture, _Shadow1Ratio, worldPoint);
	
	#if SHADOW_2
		color *= ShadowColor(_Shadow2Matrix, _Shadow2Texture, _Shadow2Ratio, worldPoint);
	#endif
	
	return color;
}
#endif