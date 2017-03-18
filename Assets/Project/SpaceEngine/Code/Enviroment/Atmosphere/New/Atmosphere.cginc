/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
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
 * Copyright (c) 2017 Eric Bruneton
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
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 */

//----------------------------------------------------------------------------------------------------------------------------
#if !defined (M_PI)
#define M_PI 3.14159265358979323846
#endif
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
#define Length						float
#define Wavelength					float
#define Angle						float
#define SolidAngle					float
#define Power						float
#define LuminousPower				float

#define Number						float
#define Area						float
#define Volume						float
#define NumberDensity				float
#define Irradiance					float
#define Radiance					float
#define SpectralPower				float
#define SpectralIrradiance			float
#define SpectralRadiance			float
#define SpectralRadianceDensity		float
#define ScatteringCoefficient		float
#define InverseSolidAngle			float
#define LuminousIntensity			float
#define Luminance					float
#define Illuminance					float

#define AbstractSpectrum			float3		// A generic function from Wavelength to some other type.
#define DimensionlessSpectrum		float3		// A function from Wavelength to Number.
#define PowerSpectrum				float3		// A function from Wavelength to SpectralPower.
#define IrradianceSpectrum			float3		// A function from Wavelength to SpectralIrradiance.
#define RadianceSpectrum			float3		// A function from Wavelength to SpectralRadiance.
#define RadianceDensitySpectrum		float3		// A function from Wavelength to SpectralRadianceDensity.
#define ScatteringSpectrum			float3		// A function from Wavelength to ScaterringCoefficient.

#define Position					float3		// A position in 3D (3 length values).
#define Direction					float3		// A unit direction vector in 3D (3 unitless values).
#define Luminance3					float3		// A vector of 3 luminance values.
#define Illuminance3				float3		// A vector of 3 illuminance values.

#define TransmittanceTexture		sampler2D
#define AbstractScatteringTexture	sampler3D
#define ReducedScatteringTexture	sampler3D
#define ScatteringTexture			sampler3D
#define ScatteringDensityTexture	sampler3D
#define IrradianceTexture			sampler2D
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
uniform const Length		m		= 1.0;
uniform const Wavelength	nm		= 1.0;
uniform const Angle			rad		= 1.0;
uniform const SolidAngle	sr		= 1.0;
uniform const Power			watt	= 1.0;
uniform const LuminousPower lm		= 1.0;
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
int TRANSMITTANCE_TEXTURE_WIDTH = 256;
int TRANSMITTANCE_TEXTURE_HEIGHT = 64;

int SCATTERING_TEXTURE_R_SIZE = 32;
int SCATTERING_TEXTURE_MU_SIZE = 128;
int SCATTERING_TEXTURE_MU_S_SIZE = 32;
int SCATTERING_TEXTURE_NU_SIZE = 8;

#define SCATTERING_TEXTURE_WIDTH SCATTERING_TEXTURE_NU_SIZE * SCATTERING_TEXTURE_MU_SIZE;
#define SCATTERING_TEXTURE_HEIGHT SCATTERING_TEXTURE_MU_SIZE;
#define SCATTERING_TEXTURE_DEPTH SCATTERING_TEXTURE_R_SIZE;

int IRRADIANCE_TEXTURE_WIDTH = 64;
int IRRADIANCE_TEXTURE_HEIGHT = 16;
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
#define	km											 1000.0 * m					//Length
#define	m2											 m * m						//Area
#define	m3											 m * m * m					//Volume
#define	pi											 M_PI * rad					//Angle
#define	deg											 pi / 180.0					//Angle
#define	watt_per_square_meter						 watt / m2					//Irradiance
#define	watt_per_square_meter_per_sr				 watt / (m2 * sr)			//Radiance
#define	watt_per_square_meter_per_nm				 watt / (m2 * nm)			//SpectralIrradiance
#define	watt_per_square_meter_per_sr_per_nm			 watt / (m2 * sr * nm)		//SpectralRadiance
#define	watt_per_cubic_meter_per_sr_per_nm			 watt / (m3 * sr * nm)		//SpectralRadianceDensity
#define	cd											 lm / sr					//LuminousIntensity
#define	kcd											 1000.0 * cd				//LuminousIntensity
#define	cd_per_square_meter							 cd / m2					//Luminance
#define	kcd_per_square_meter						 kcd / m2					//Luminance

#define IN(x) in x
#define OUT(x) out x
#define TEMPLATE(x)
#define TEMPLATE_ARGUMENT(x)
#define assert(x)
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
struct AtmosphereParameters 
{
	IrradianceSpectrum		solar_irradiance;		// The solar irradiance at the top of the atmosphere.
	Angle					sun_angular_radius;		// The sun's angular radius.
	Length					bottom_radius;			// The distance between the planet center and the bottom of the atmosphere.
	Length					top_radius;				// The distance between the planet center and the top of the atmosphere.
	Length					rayleigh_scale_height;	// The scale height of air molecules, meaning that their density is proportional to exp(-h / rayleigh_scale_height), with h the altitude (with the bottom of the atmosphere at altitude 0).
	ScatteringSpectrum		rayleigh_scattering;	// The scattering coefficient of air molecules at the bottom of the atmosphere, as a function of wavelength.
	Length					mie_scale_height;		// The scale height of aerosols, meaning that their density is proportional to exp(-h / mie_scale_height), with h the altitude.
	ScatteringSpectrum		mie_scattering;			// The scattering coefficient of aerosols at the bottom of the atmosphere, as a function of wavelength.
	ScatteringSpectrum		mie_extinction;			// The extinction coefficient of aerosols at the bottom of the atmosphere, as a function of wavelength.
	Number					mie_phase_function_g;	// The asymetry parameter for the Cornette-Shanks phase function for the aerosols.
	DimensionlessSpectrum	ground_albedo;			// The average albedo of the ground.

	// The cosine of the maximum Sun zenith angle for which atmospheric scattering
	// must be precomputed (for maximum precision, use the smallest Sun zenith
	// angle yielding negligible sky light radiance values. For instance, for the
	// Earth case, 102 degrees is a good choice - yielding mu_s_min = -0.2).
	Number mu_s_min;
};
//----------------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------------
Number ClampCosine(Number mu) 
{
	return clamp(mu, Number(-1.0), Number(1.0));
}

Length ClampDistance(Length d) 
{
	return max(d, 0.0 * m);
}

Length ClampRadius(IN(AtmosphereParameters) atmosphere, Length r) 
{
	return clamp(r, atmosphere.bottom_radius, atmosphere.top_radius);
}

Length SafeSqrt(Area a) 
{
	return sqrt(max(a, 0.0 * m2));
}

Length DistanceToTopAtmosphereBoundary(IN(AtmosphereParameters) atmosphere,	Length r, Number mu) 
{
	assert(r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	Area discriminant = r * r * (mu * mu - 1.0) + atmosphere.top_radius * atmosphere.top_radius;

	return ClampDistance(-r * mu + SafeSqrt(discriminant));
}

Length DistanceToBottomAtmosphereBoundary(IN(AtmosphereParameters) atmosphere, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	Area discriminant = r * r * (mu * mu - 1.0) + atmosphere.bottom_radius * atmosphere.bottom_radius;

	return ClampDistance(-r * mu - SafeSqrt(discriminant));
}

bool RayIntersectsGround(IN(AtmosphereParameters) atmosphere, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	return mu < 0.0 && r * r * (mu * mu - 1.0) + atmosphere.bottom_radius * atmosphere.bottom_radius >= 0.0 * m2;
}

Length ComputeOpticalLengthToTopAtmosphereBoundary(IN(AtmosphereParameters) atmosphere, Length scale_height, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	const int SAMPLE_COUNT = 500; // Number of intervals for the numerical integration.

	Length dx = DistanceToTopAtmosphereBoundary(atmosphere, r, mu) / Number(SAMPLE_COUNT); // The integration step, i.e. the length of each integration interval.
	Length result = 0.0 * m; // Integration loop.

	for (int i = 0; i <= SAMPLE_COUNT; ++i) 
	{
		Length d_i = Number(i) * dx;	
		Length r_i = sqrt(d_i * d_i + 2.0 * r * mu * d_i + r * r); // Distance between the current sample point and the planet center.

		Number y_i = exp(-(r_i - atmosphere.bottom_radius) / scale_height); // Number density at the current sample point (divided by the number density at the bottom of the atmosphere, yielding a dimensionless number).
		Number weight_i = i == 0 || i == SAMPLE_COUNT ? 0.5 : 1.0; // Sample weight (from the trapezoidal rule).

		result += y_i * weight_i * dx;
	}

	return result;
}

DimensionlessSpectrum ComputeTransmittanceToTopAtmosphereBoundary(IN(AtmosphereParameters) atmosphere, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	return exp(-(atmosphere.rayleigh_scattering * 
				ComputeOpticalLengthToTopAtmosphereBoundary(atmosphere, atmosphere.rayleigh_scale_height, r, mu) +
	  			atmosphere.mie_extinction *
		  		ComputeOpticalLengthToTopAtmosphereBoundary(atmosphere, atmosphere.mie_scale_height, r, mu)));
}

Number GetTextureCoordFromUnitRange(Number x, int texture_size) 
{
	return 0.5 / Number(texture_size) + x * (1.0 - 1.0 / Number(texture_size));
}

Number GetUnitRangeFromTextureCoord(Number u, int texture_size) 
{
	return (u - 0.5 / Number(texture_size)) / (1.0 - 1.0 / Number(texture_size));
}

float2 GetTransmittanceTextureUvFromRMu(IN(AtmosphereParameters) atmosphere, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	Length H = sqrt(atmosphere.top_radius * atmosphere.top_radius - atmosphere.bottom_radius * atmosphere.bottom_radius);  // Distance to top atmosphere boundary for a horizontal ray at ground level.
	Length rho = SafeSqrt(r * r - atmosphere.bottom_radius * atmosphere.bottom_radius);  // Distance to the horizon.

	// Distance to the top atmosphere boundary for the ray (r,mu), and its minimum
	// and maximum values over all mu - obtained for (r,1) and (r,mu_horizon).
	Length d = DistanceToTopAtmosphereBoundary(atmosphere, r, mu);
	Length d_min = atmosphere.top_radius - r;
	Length d_max = rho + H;

	Number x_mu = (d - d_min) / (d_max - d_min);
	Number x_r = rho / H;

  	return float2(GetTextureCoordFromUnitRange(x_mu, TRANSMITTANCE_TEXTURE_WIDTH),
			  	  GetTextureCoordFromUnitRange(x_r, TRANSMITTANCE_TEXTURE_HEIGHT));
}

void GetRMuFromTransmittanceTextureUv(IN(AtmosphereParameters) atmosphere, IN(float2) uv, OUT(Length) r, OUT(Number) mu) 
{
	assert(uv.x >= 0.0 && uv.x <= 1.0);
	assert(uv.y >= 0.0 && uv.y <= 1.0);

	Number x_mu = GetUnitRangeFromTextureCoord(uv.x, TRANSMITTANCE_TEXTURE_WIDTH);
	Number x_r = GetUnitRangeFromTextureCoord(uv.y, TRANSMITTANCE_TEXTURE_HEIGHT);

	Length H = sqrt(atmosphere.top_radius * atmosphere.top_radius - atmosphere.bottom_radius * atmosphere.bottom_radius); // Distance to top atmosphere boundary for a horizontal ray at ground level. 
	Length rho = H * x_r; // Distance to the horizon, from which we can compute r:

	r = sqrt(rho * rho + atmosphere.bottom_radius * atmosphere.bottom_radius);

	// Distance to the top atmosphere boundary for the ray (r,mu), and its minimum
	// and maximum values over all mu - obtained for (r,1) and (r,mu_horizon) -
	// from which we can recover mu:
	Length d_min = atmosphere.top_radius - r;
	Length d_max = rho + H;
	Length d = d_min + x_mu * (d_max - d_min);
	
	mu = d == 0.0 * m ? Number(1.0) : (H * H - rho * rho - d * d) / (2.0 * r * d);
	mu = ClampCosine(mu);
}

DimensionlessSpectrum ComputeTransmittanceToTopAtmosphereBoundaryTexture(IN(AtmosphereParameters) atmosphere, IN(float2) gl_frag_coord) 
{
	const float2 TRANSMITTANCE_TEXTURE_SIZE = float2(TRANSMITTANCE_TEXTURE_WIDTH, TRANSMITTANCE_TEXTURE_HEIGHT);
	
	Length r;
	Number mu;

	GetRMuFromTransmittanceTextureUv(atmosphere, gl_frag_coord / TRANSMITTANCE_TEXTURE_SIZE, r, mu);

	return ComputeTransmittanceToTopAtmosphereBoundary(atmosphere, r, mu);
}

DimensionlessSpectrum GetTransmittanceToTopAtmosphereBoundary(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, Length r, Number mu) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);

	float2 uv = GetTransmittanceTextureUvFromRMu(atmosphere, r, mu);
	
	return DimensionlessSpectrum(tex2D(transmittance_texture, uv).xyz);
}

DimensionlessSpectrum GetTransmittance(IN(AtmosphereParameters) atmosphere,	IN(TransmittanceTexture) transmittance_texture,	Length r, Number mu, Length d, bool ray_r_mu_intersects_ground) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);
	assert(d >= 0.0 * m);

	Length r_d = ClampRadius(atmosphere, sqrt(d * d + 2.0 * r * mu * d + r * r));

	Number mu_d = ClampCosine((r * mu + d) / r_d);

	if (ray_r_mu_intersects_ground) 
	{
		return min(GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r_d, -mu_d) /
				   GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r, -mu), DimensionlessSpectrum(1.0, 1.0, 1.0));
	} 
	else 
	{
		return min(GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r, mu) /
				   GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r_d, mu_d), DimensionlessSpectrum(1.0, 1.0, 1.0));
	}
}

void ComputeSingleScatteringIntegrand(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, Length r, Number mu, Number mu_s, Number nu, Length d, bool ray_r_mu_intersects_ground, OUT(DimensionlessSpectrum) rayleigh, OUT(DimensionlessSpectrum) mie) 
{
	Length r_d = ClampRadius(atmosphere, sqrt(d * d + 2.0 * r * mu * d + r * r));
	Number mu_s_d = ClampCosine((r * mu_s + d * nu) / r_d);

	if (RayIntersectsGround(atmosphere, r_d, mu_s_d)) 
	{
		rayleigh = DimensionlessSpectrum(0.0, 0.0, 0.0);
		mie = DimensionlessSpectrum(0.0, 0.0, 0.0);
	} 
	else 
	{
		DimensionlessSpectrum transmittance = GetTransmittance(atmosphere, transmittance_texture, r, mu, d,	ray_r_mu_intersects_ground) *
											  GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r_d, mu_s_d);

		rayleigh = transmittance * exp(-(r_d - atmosphere.bottom_radius) / atmosphere.rayleigh_scale_height);
		mie = transmittance * exp(-(r_d - atmosphere.bottom_radius) / atmosphere.mie_scale_height);
	}
}

Length DistanceToNearestAtmosphereBoundary(IN(AtmosphereParameters) atmosphere, Length r, Number mu, bool ray_r_mu_intersects_ground) 
{
	if (ray_r_mu_intersects_ground) 
	{
		return DistanceToBottomAtmosphereBoundary(atmosphere, r, mu);
	} 
	else 
	{
		return DistanceToTopAtmosphereBoundary(atmosphere, r, mu);
	}
}

void ComputeSingleScattering(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground, OUT(IrradianceSpectrum) rayleigh, OUT(IrradianceSpectrum) mie) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);
	assert(mu_s >= -1.0 && mu_s <= 1.0);
	assert(nu >= -1.0 && nu <= 1.0);

  	const int SAMPLE_COUNT = 50; // Number of intervals for the numerical integration.

  	Length dx = DistanceToNearestAtmosphereBoundary(atmosphere, r, mu, ray_r_mu_intersects_ground) / Number(SAMPLE_COUNT); // The integration step, i.e. the length of each integration interval.

	// Integration loop.
	DimensionlessSpectrum rayleigh_sum = DimensionlessSpectrum(0.0, 0.0, 0.0);
	DimensionlessSpectrum mie_sum = DimensionlessSpectrum(0.0, 0.0, 0.0);

  	for (int i = 0; i <= SAMPLE_COUNT; ++i) 
	{
		Length d_i = Number(i) * dx;
		
		DimensionlessSpectrum rayleigh_i; // The Rayleigh and Mie single scattering at the current sample point.
		DimensionlessSpectrum mie_i;

		ComputeSingleScatteringIntegrand(atmosphere, transmittance_texture,	r, mu, mu_s, nu, d_i, ray_r_mu_intersects_ground, rayleigh_i, mie_i);
		
		Number weight_i = (i == 0 || i == SAMPLE_COUNT) ? 0.5 : 1.0; // Sample weight (from the trapezoidal rule).
		rayleigh_sum += rayleigh_i * weight_i;
		mie_sum += mie_i * weight_i;
  	}
	
  	rayleigh = rayleigh_sum * dx * atmosphere.solar_irradiance * atmosphere.rayleigh_scattering;
  	mie = mie_sum * dx * atmosphere.solar_irradiance * atmosphere.mie_scattering;
}

InverseSolidAngle RayleighPhaseFunction(Number nu) 
{
	InverseSolidAngle k = 3.0 / (16.0 * M_PI * sr);

  	return k * (1.0 + nu * nu);
}

InverseSolidAngle MiePhaseFunction(Number g, Number nu) 
{
	InverseSolidAngle k = 3.0 / (8.0 * M_PI * sr) * (1.0 - g * g) / (2.0 + g * g);
	
	return k * (1.0 + nu * nu) / pow(1.0 + g * g - 2.0 * g * nu, 1.5);
}

float4 GetScatteringTextureUvwzFromRMuMuSNu(IN(AtmosphereParameters) atmosphere, Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);
	assert(mu_s >= -1.0 && mu_s <= 1.0);
	assert(nu >= -1.0 && nu <= 1.0);

	// Distance to top atmosphere boundary for a horizontal ray at ground level.
	Length H = sqrt(atmosphere.top_radius * atmosphere.top_radius - atmosphere.bottom_radius * atmosphere.bottom_radius);
	Length rho = SafeSqrt(r * r - atmosphere.bottom_radius * atmosphere.bottom_radius);   // Distance to the horizon.
	Number u_r = GetTextureCoordFromUnitRange(rho / H, SCATTERING_TEXTURE_R_SIZE);

	// Discriminant of the quadratic equation for the intersections of the ray (r,mu) with the ground (see RayIntersectsGround).
	Length r_mu = r * mu;
	Area discriminant = r_mu * r_mu - r * r + atmosphere.bottom_radius * atmosphere.bottom_radius;
	Number u_mu;

	if (ray_r_mu_intersects_ground) 
	{
		// Distance to the ground for the ray (r,mu), and its minimum and maximum values over all mu - obtained for (r,-1) and (r,mu_horizon).
		Length d = -r_mu - SafeSqrt(discriminant);
		Length d_min = r - atmosphere.bottom_radius;
		Length d_max = rho;
		u_mu = 0.5 - 0.5 * GetTextureCoordFromUnitRange(d_max == d_min ? 0.0 : (d - d_min) / (d_max - d_min), SCATTERING_TEXTURE_MU_SIZE / 2);
	} 
	else 
	{
		// Distance to the top atmosphere boundary for the ray (r,mu), and its minimum and maximum values over all mu - obtained for (r,1) and (r,mu_horizon).
		Length d = -r_mu + SafeSqrt(discriminant + H * H);
		Length d_min = atmosphere.top_radius - r;
		Length d_max = rho + H;

		u_mu = 0.5 + 0.5 * GetTextureCoordFromUnitRange((d - d_min) / (d_max - d_min), SCATTERING_TEXTURE_MU_SIZE / 2);
	}

	Length d = DistanceToTopAtmosphereBoundary( atmosphere, atmosphere.bottom_radius, mu_s);
	Length d_min = atmosphere.top_radius - atmosphere.bottom_radius;
	Length d_max = H;

	Number a = (d - d_min) / (d_max - d_min);
	Number A = -2.0 * atmosphere.mu_s_min * atmosphere.bottom_radius / (d_max - d_min);
	Number u_mu_s = GetTextureCoordFromUnitRange(max(1.0 - a / A, 0.0) / (1.0 + a), SCATTERING_TEXTURE_MU_S_SIZE);

	Number u_nu = (nu + 1.0) / 2.0;

	return float4(u_nu, u_mu_s, u_mu, u_r);
}

//To be continued...
//----------------------------------------------------------------------------------------------------------------------------