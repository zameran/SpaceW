/* Procedural planet generator.
 *
 * Copyright (C) 2015-2023 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Precomputed Atmospheric Scattering
 * Copyright (c) 2008 INRIA
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Eric Bruneton
 * Modified and ported to Unity by Justin Hawkins 2014
 * Modified by Denis Ovchinnikov 2015-2023
 */

#define SPACE_ATMOSPHERE_PREPROCESS

#define NUM_THREADS 8
 
// ---------------------------------------------------------------------------- 
// PHYSICAL MODEL PARAMETERS 
// ---------------------------------------------------------------------------- 
 
float Rg;
float Rt;
float RL;
float HR;
float HM;
float mieG;
float AVERAGE_GROUND_REFLECTANCE;
float4 betaR;
float4 betaMSca;
float4 betaMEx;

// ---------------------------------------------------------------------------- 
// CONSTANT PARAMETERS 
// ---------------------------------------------------------------------------- 

int first;
int TRANSMITTANCE_H;
int TRANSMITTANCE_W;
int SKY_W;
int SKY_H;
int RES_R;
int RES_MU;
int RES_MU_S;
int RES_NU;

// ---------------------------------------------------------------------------- 
// NUMERICAL INTEGRATION PARAMETERS 
// ---------------------------------------------------------------------------- 

//#define TRANSMITTANCE_INTEGRAL_SAMPLES 512		//500
//#define INSCATTER_INTEGRAL_SAMPLES 64				//50
//#define IRRADIANCE_INTEGRAL_SAMPLES 32			//32
//#define IRRADIANCE_INTEGRAL_SAMPLES_HALF 16		//16
//#define INSCATTER_SPHERICAL_INTEGRAL_SAMPLES 16	//16

int TRANSMITTANCE_INTEGRAL_SAMPLES;
int INSCATTER_INTEGRAL_SAMPLES;
int IRRADIANCE_INTEGRAL_SAMPLES;
int IRRADIANCE_INTEGRAL_SAMPLES_HALF;
int INSCATTER_SPHERICAL_INTEGRAL_SAMPLES;
 
#if !defined (MATH)
#include "Math.cginc"
#endif
 
// ---------------------------------------------------------------------------- 
// PARAMETERIZATION OPTIONS 
// ---------------------------------------------------------------------------- 
 
#define TRANSMITTANCE_NON_LINEAR
#define INSCATTER_NON_LINEAR
 
// ---------------------------------------------------------------------------- 
// PARAMETERIZATION FUNCTIONS 
// ---------------------------------------------------------------------------- 

Texture2D<float4> transmittanceRead;

SamplerState PointClamp;
SamplerState LinearClamp;

float4 SamplePoint(Texture3D<float4> tex, float3 uv, float3 size)
{
	uv = saturate(uv);
	uv = uv * (size - 1.0);

	return tex[uint3(uv + 0.5)];
}

float mod(float x, float y) { return x - y * floor(x/y); }
  
float2 GetTransmittanceUV(float r, float mu) 
{ 
	float uR, uMu; 

	#if defined(TRANSMITTANCE_NON_LINEAR)
		uR = sqrt((r - Rg) / (Rt - Rg)); 
		uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5; 
	#else 
		uR = (r - Rg) / (Rt - Rg); 
		uMu = (mu + 0.15) / (1.0 + 0.15); 
	#endif
 
	return float2(uMu, uR); 
} 
 
void GetTransmittanceRMu(float2 coord, out float r, out float muS) 
{ 
	r = coord.y / float(TRANSMITTANCE_H); 
	muS = coord.x / float(TRANSMITTANCE_W); 

	#if defined(TRANSMITTANCE_NON_LINEAR) 
		r = Rg + (r * r) * (Rt - Rg); 
		muS = -0.15 + tan(1.5 * muS) / tan(1.5) * (1.0 + 0.15); 
	#else 
		r = Rg + r * (Rt - Rg); 
		muS = -0.15 + muS * (1.0 + 0.15); 
	#endif 
}
 
float2 GetIrradianceUV(float r, float muS) 
{ 
	float uR = (r - Rg) / (Rt - Rg); 
	float uMuS = (muS + 0.2) / (1.0 + 0.2); 

	return float2(uMuS, uR); 
}  

void GetIrradianceRMuS(float2 coord, out float r, out float muS) 
{ 
	r = Rg + (coord.y - 0.5) / (float(SKY_H) - 1.0) * (Rt - Rg); 
	muS = -0.2 + (coord.x - 0.5) / (float(SKY_W) - 1.0) * (1.0 + 0.2); 
}  

float4 Texture4D(Texture3D tex, float r, float mu, float muS, float nu) 
{ 
	float H = sqrt(Rt * Rt - Rg * Rg); 
	float rho = sqrt(r * r - Rg * Rg); 

	#if defined(INSCATTER_NON_LINEAR) 
		float rmu = r * mu; 
		float delta = rmu * rmu - r * r + Rg * Rg; 
		float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / float(RES_MU)) : float4(-1.0, H * H, H, 0.5 + 0.5 / float(RES_MU)); 
		float uR = 0.5 / float(RES_R) + rho / H * (1.0 - 1.0 / float(RES_R)); 
		float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / float(RES_MU)); 
		// paper formula 
		//float uMuS = 0.5 / float(RES_MU_S) + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / float(RES_MU_S)); 
		// better formula 
		float uMuS = 0.5 / float(RES_MU_S) + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / float(RES_MU_S)); 
	#else 
		float uR = 0.5 / float(RES_R) + rho / H * (1.0 - 1.0 / float(RES_R)); 
		float uMu = 0.5 / float(RES_MU) + (mu + 1.0) / 2.0 * (1.0 - 1.0 / float(RES_MU)); 
		float uMuS = 0.5 / float(RES_MU_S) + max(muS + 0.2, 0.0) / 1.2 * (1.0 - 1.0 / float(RES_MU_S)); 
	#endif 

	float _lerp = (nu + 1.0) / 2.0 * (float(RES_NU) - 1.0); 
	float uNu = floor(_lerp); 
	_lerp = _lerp - uNu; 
	
	float3 size = float3(RES_MU_S * RES_NU, RES_MU, RES_R);
	
	return SamplePoint(tex, float3((uNu + uMuS) / float(RES_NU), uMu, uR), size) * (1.0 - _lerp) + 
		   SamplePoint(tex, float3((uNu + uMuS + 1.0) / float(RES_NU), uMu, uR), size) * _lerp;
} 

void GetMuMuSNu(float2 coord, float r, float4 dhdH, out float mu, out float muS, out float nu) 
{ 
	float x = coord.x - 0.5; 
	float y = coord.y - 0.5; 

	#if defined(INSCATTER_NON_LINEAR)
		if (y < float(RES_MU) / 2.0) 
		{ 
			float d = 1.0 - y / (float(RES_MU) / 2.0 - 1.0); 
			d = min(max(dhdH.z, d * dhdH.w), dhdH.w * 0.999); 
			mu = (Rg * Rg - r * r - d * d) / (2.0 * r * d); 
			mu = min(mu, -sqrt(1.0 - (Rg / r) * (Rg / r)) - 0.001); 
		} 
		else 
		{ 
			float d = (y - float(RES_MU) / 2.0) / (float(RES_MU) / 2.0 - 1.0); 
			d = min(max(dhdH.x, d * dhdH.y), dhdH.y * 0.999); 
			mu = (Rt * Rt - r * r - d * d) / (2.0 * r * d); 
		} 

		muS = mod(x, float(RES_MU_S)) / (float(RES_MU_S) - 1.0); 
		// paper formula 
		//muS = -(0.6 + log(1.0 - muS * (1.0 -  exp(-3.6)))) / 3.0; 
		// better formula 
		muS = tan((2.0 * muS - 1.0 + 0.26) * 1.1) / tan(1.26 * 1.1); 
		nu = -1.0 + floor(x / float(RES_MU_S)) / (float(RES_NU) - 1.0) * 2.0; 
	#else 
		mu = -1.0 + 2.0 * y / (float(RES_MU) - 1.0); 
		muS = mod(x, float(RES_MU_S)) / (float(RES_MU_S) - 1.0); 
		muS = -0.2 + muS * 1.2; 
		nu = -1.0 + floor(x / float(RES_MU_S)) / (float(RES_NU) - 1.0) * 2.0; 
	#endif 
} 

void GetLayer(int layer, out float r, out float4 dhdH)
{
	r = float(layer) / (RES_R - 1.0);
	r = r * r;
	r = sqrt(Rg * Rg + r * (Rt * Rt - Rg * Rg)) + (layer == 0 ? 0.01 : (layer == RES_R - 1 ? -0.001 : 0.0));
	
	float dmin = Rt - r;
	float dmax = sqrt(r * r - Rg * Rg) + sqrt(Rt * Rt - Rg * Rg);
	float dminp = r - Rg;
	float dmaxp = sqrt(r * r - Rg * Rg);

	dhdH = float4(dmin, dmax, dminp, dmaxp);	
}
 
// ---------------------------------------------------------------------------- 
// UTILITY FUNCTIONS 
// ---------------------------------------------------------------------------- 

// nearest intersection of ray r,mu with ground or top atmosphere boundary 
// mu=cos(ray zenith angle at ray origin) 
float Limit(float r, float mu) 
{ 
	float dout = -r * mu + sqrt(r * r * (mu * mu - 1.0) + RL * RL); 
	float delta2 = r * r * (mu * mu - 1.0) + Rg * Rg; 
	
	if (delta2 >= 0.0) 
	{ 
		float din = -r * mu - sqrt(delta2);

		if (din >= 0.0) 
		{ 
			dout = min(dout, din); 
		} 
	} 
	
	return dout; 
}

// transmittance(=transparency) of atmosphere for infinite ray (r,mu) 
// (mu=cos(view zenith angle)), intersections with ground ignored 
float3 Transmittance(float r, float mu) 
{ 
	float2 uv = GetTransmittanceUV(r, mu);

	return transmittanceRead.SampleLevel(LinearClamp, uv, 0).rgb; 
} 

// transmittance(=transparency) of atmosphere between x and x0 
// assume segment x,x0 not intersecting ground 
// d = distance between x and x0, mu=cos(zenith angle of [x,x0) ray at x) 
float3 Transmittance(float r, float mu, float d)
{ 
	float3 result; 
	float r1 = sqrt(r * r + d * d + 2.0 * r * mu * d); 
	float mu1 = (r * mu + d) / r1; 

	if (mu > 0.0) 
	{ 
		result = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0); 
	} 
	else 
	{ 
		result = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0); 
	} 

	return result; 
} 

float3 Irradiance(Texture2D<float4> tex, float r, float muS) 
{ 
	float2 uv = GetIrradianceUV(r, muS); 

	return tex.SampleLevel(LinearClamp, uv, 0).rgb;
}  

// Rayleigh phase function 
float PhaseFunctionR(float mu) 
{ 
	return (3.0 / M_PI16) * (1.0 + mu * mu); 
} 

// Mie phase function 
float PhaseFunctionM(float mu) 
{ 
	return 1.5 * 1.0 / M_PI4 * (1.0 - mieG * mieG) * pow(max(0.0, 1.0 + (mieG * mieG) - 2.0 * mieG * mu), -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG * mieG); 
}