#if LIGHT_1 || LIGHT_2
float4 _Light1Color;
float4 _Light1Position;
float3 _Light1Direction;
#if LIGHT_2
float4 _Light2Color;
float4 _Light2Position;
float3 _Light2Direction;
#endif

float ComputeMiePhase(float _CosTheta, float _MiePhaseAnisotropy)
{
	float Num = 1.5 * (1.0 + _CosTheta*_CosTheta) * (1.0 - _MiePhaseAnisotropy*_MiePhaseAnisotropy);
	float Den = (8.0 + _MiePhaseAnisotropy*_MiePhaseAnisotropy) * pow( abs(1.0 + _MiePhaseAnisotropy*_MiePhaseAnisotropy - 2.0 * _MiePhaseAnisotropy * _CosTheta), 1.5 );

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
	return rayleigh * (3.0 / (16.0 * 3.141592)) * (1.0 + mu * mu);
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