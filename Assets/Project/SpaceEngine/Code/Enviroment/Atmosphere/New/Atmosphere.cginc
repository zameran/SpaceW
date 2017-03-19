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

void GetRMuMuSNuFromScatteringTextureUvwz(IN(AtmosphereParameters) atmosphere, IN(float4) uvwz, OUT(Length) r, OUT(Number) mu, OUT(Number) mu_s, OUT(Number) nu, OUT(bool) ray_r_mu_intersects_ground) 
{
	assert(uvwz.x >= 0.0 && uvwz.x <= 1.0);
	assert(uvwz.y >= 0.0 && uvwz.y <= 1.0);
	assert(uvwz.z >= 0.0 && uvwz.z <= 1.0);
	assert(uvwz.w >= 0.0 && uvwz.w <= 1.0);

	// Distance to top atmosphere boundary for a horizontal ray at ground level.
	Length H = sqrt(atmosphere.top_radius * atmosphere.top_radius - atmosphere.bottom_radius * atmosphere.bottom_radius);
	Length rho = H * GetUnitRangeFromTextureCoord(uvwz.w, SCATTERING_TEXTURE_R_SIZE); // Distance to the horizon.

	r = sqrt(rho * rho + atmosphere.bottom_radius * atmosphere.bottom_radius);

	if (uvwz.z < 0.5) 
	{
		// Distance to the ground for the ray (r,mu), and its minimum and maximum
		// values over all mu - obtained for (r,-1) and (r,mu_horizon) - from which
		// we can recover mu:
		Length d_min = r - atmosphere.bottom_radius;
		Length d_max = rho;
		Length d = d_min + (d_max - d_min) * GetUnitRangeFromTextureCoord(1.0 - 2.0 * uvwz.z, SCATTERING_TEXTURE_MU_SIZE / 2);

		mu = d == 0.0 * m ? Number(-1.0) : ClampCosine(-(rho * rho + d * d) / (2.0 * r * d));

		ray_r_mu_intersects_ground = true;
  } 
  else 
  {
		// Distance to the top atmosphere boundary for the ray (r,mu), and its
		// minimum and maximum values over all mu - obtained for (r,1) and
		// (r,mu_horizon) - from which we can recover mu:
		Length d_min = atmosphere.top_radius - r;
		Length d_max = rho + H;
		Length d = d_min + (d_max - d_min) * GetUnitRangeFromTextureCoord(2.0 * uvwz.z - 1.0, SCATTERING_TEXTURE_MU_SIZE / 2);
		mu = d == 0.0 * m ? Number(1.0) : ClampCosine((H * H - rho * rho - d * d) / (2.0 * r * d));

		ray_r_mu_intersects_ground = false;
  }

	Number x_mu_s = GetUnitRangeFromTextureCoord(uvwz.y, SCATTERING_TEXTURE_MU_S_SIZE);
	Length d_min = atmosphere.top_radius - atmosphere.bottom_radius;
	Length d_max = H;
	Number A = -2.0 * atmosphere.mu_s_min * atmosphere.bottom_radius / (d_max - d_min);
	Number a = (A - x_mu_s * A) / (1.0 + x_mu_s * A);
	Length d = d_min + min(a, A) * (d_max - d_min);

	mu_s = d == 0.0 * m ? Number(1.0) : ClampCosine((H * H - d * d) / (2.0 * atmosphere.bottom_radius * d));
	nu = ClampCosine(uvwz.x * 2.0 - 1.0);
}

void GetRMuMuSNuFromScatteringTextureFragCoord(IN(AtmosphereParameters) atmosphere, IN(float3) gl_frag_coord, OUT(Length) r, OUT(Number) mu, OUT(Number) mu_s, OUT(Number) nu, OUT(bool) ray_r_mu_intersects_ground) 
{
	const float4 SCATTERING_TEXTURE_SIZE = float4(SCATTERING_TEXTURE_NU_SIZE - 1, SCATTERING_TEXTURE_MU_S_SIZE, SCATTERING_TEXTURE_MU_SIZE, SCATTERING_TEXTURE_R_SIZE);

	Number frag_coord_nu = floor(gl_frag_coord.x / Number(SCATTERING_TEXTURE_MU_S_SIZE));
	Number frag_coord_mu_s = fmod(gl_frag_coord.x, Number(SCATTERING_TEXTURE_MU_S_SIZE)); // TODO : GLSL's mod have different behaviour, rather that HLSL's fmod in negative values...

	float4 uvwz = float4(frag_coord_nu, frag_coord_mu_s, gl_frag_coord.y, gl_frag_coord.z) / SCATTERING_TEXTURE_SIZE;

	GetRMuMuSNuFromScatteringTextureUvwz(atmosphere, uvwz, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

	// Clamp nu to its valid range of values, given mu and mu_s.
	nu = clamp(nu, mu * mu_s - sqrt((1.0 - mu * mu) * (1.0 - mu_s * mu_s)), mu * mu_s + sqrt((1.0 - mu * mu) * (1.0 - mu_s * mu_s)));
}

void ComputeSingleScatteringTexture(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(float3) gl_frag_coord, OUT(IrradianceSpectrum) rayleigh, OUT(IrradianceSpectrum) mie) 
{
	Length r;
	Number mu;
	Number mu_s;
	Number nu;
	bool ray_r_mu_intersects_ground;

	GetRMuMuSNuFromScatteringTextureFragCoord(atmosphere, gl_frag_coord, r, mu, mu_s, nu, ray_r_mu_intersects_ground);
	ComputeSingleScattering(atmosphere, transmittance_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground, rayleigh, mie);
}

TEMPLATE(AbstractSpectrum)
AbstractSpectrum GetScattering(IN(AtmosphereParameters) atmosphere, IN(AbstractScatteringTexture TEMPLATE_ARGUMENT(AbstractSpectrum))scattering_texture, Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground) 
{
	float4 uvwz = GetScatteringTextureUvwzFromRMuMuSNu(atmosphere, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

	Number tex_coord_x = uvwz.x * Number(SCATTERING_TEXTURE_NU_SIZE - 1);
	Number tex_x = floor(tex_coord_x);
	Number lerp = tex_coord_x - tex_x;

	float3 uvw0 = float3((tex_x + uvwz.y) / Number(SCATTERING_TEXTURE_NU_SIZE), uvwz.z, uvwz.w);
	float3 uvw1 = float3((tex_x + 1.0 + uvwz.y) / Number(SCATTERING_TEXTURE_NU_SIZE), uvwz.z, uvwz.w);

	//return AbstractSpectrum(tex3D(scattering_texture, uvw0) * (1.0 - lerp) + tex3D(scattering_texture, uvw1) * lerp);
	return AbstractSpectrum(tex3D(scattering_texture, uvw0).rgb * (1.0 - lerp) + tex3D(scattering_texture, uvw1).rgb * lerp);
}

RadianceSpectrum GetScattering(IN(AtmosphereParameters) atmosphere, IN(ReducedScatteringTexture) single_rayleigh_scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, IN(ScatteringTexture) multiple_scattering_texture,	Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground, int scattering_order) 
{
	if (scattering_order == 1) 
	{
		IrradianceSpectrum rayleigh = GetScattering(atmosphere, single_rayleigh_scattering_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground);
		IrradianceSpectrum mie = GetScattering(atmosphere, single_mie_scattering_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

		return rayleigh * RayleighPhaseFunction(nu) + mie * MiePhaseFunction(atmosphere.mie_phase_function_g, nu);
	} 
	else 
	{
		return GetScattering(atmosphere, multiple_scattering_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground);
	}
}

IrradianceSpectrum GetIrradiance(IN(AtmosphereParameters) atmosphere, IN(IrradianceTexture) irradiance_texture,	Length r, Number mu_s);

RadianceDensitySpectrum ComputeScatteringDensity(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(ReducedScatteringTexture) single_rayleigh_scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, IN(ScatteringTexture) multiple_scattering_texture, IN(IrradianceTexture) irradiance_texture, Length r, Number mu, Number mu_s, Number nu, int scattering_order) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);
	assert(mu_s >= -1.0 && mu_s <= 1.0);
	assert(nu >= -1.0 && nu <= 1.0);
	assert(scattering_order >= 2);

	// Compute unit direction vectors for the zenith, the view direction omega and and the sun direction omega_s, such that the cosine of the view-zenith
	// angle is mu, the cosine of the sun-zenith angle is mu_s, and the cosine of the view-sun angle is nu. The goal is to simplify computations below.
	float3 zenith_direction = float3(0.0, 0.0, 1.0);
	float3 omega = float3(sqrt(1.0 - mu * mu), 0.0, mu);

	Number sun_dir_x = omega.x == 0.0 ? 0.0 : (nu - mu * mu_s) / omega.x;
	Number sun_dir_y = sqrt(max(1.0 - sun_dir_x * sun_dir_x - mu_s * mu_s, 0.0));

	float3 omega_s = float3(sun_dir_x, sun_dir_y, mu_s);

	const int SAMPLE_COUNT = 16;
	const Angle dphi = pi / Number(SAMPLE_COUNT);
	const Angle dtheta = pi / Number(SAMPLE_COUNT);

	RadianceDensitySpectrum rayleigh_mie = RadianceDensitySpectrum(0.0 * watt_per_cubic_meter_per_sr_per_nm,
																   0.0 * watt_per_cubic_meter_per_sr_per_nm,
																   0.0 * watt_per_cubic_meter_per_sr_per_nm); // TODO : Initialize with zeros.

	// Nested loops for the integral over all the incident directions omega_i.
	for (int l = 0; l < SAMPLE_COUNT; ++l) 
	{
		Angle theta = (Number(l) + 0.5) * dtheta;
		Number cos_theta = cos(theta);
		Number sin_theta = sin(theta);

		bool ray_r_theta_intersects_ground = RayIntersectsGround(atmosphere, r, cos_theta);

		// The distance and transmittance to the ground only depend on theta, so we can compute them in the outer loop for efficiency.
		Length distance_to_ground = 0.0 * m;
		DimensionlessSpectrum transmittance_to_ground = DimensionlessSpectrum(0.0, 0.0, 0.0);
		DimensionlessSpectrum ground_albedo = DimensionlessSpectrum(0.0, 0.0, 0.0);

		if (ray_r_theta_intersects_ground) 
		{
			distance_to_ground = DistanceToBottomAtmosphereBoundary(atmosphere, r, cos_theta);
			transmittance_to_ground = GetTransmittance(atmosphere, transmittance_texture, r, cos_theta, distance_to_ground, true /* ray_intersects_ground */);
			ground_albedo = atmosphere.ground_albedo;
		}

		for (int m = 0; m < 2 * SAMPLE_COUNT; ++m) 
		{
			Angle phi = (Number(m) + 0.5) * dphi;
			float3 omega_i = float3(cos(phi) * sin_theta, sin(phi) * sin_theta, cos_theta);
			SolidAngle domega_i = (dtheta / rad) * (dphi / rad) * sin(theta) * sr;

			// The radiance L_i arriving from direction omega_i after n-1 bounces is the sum of a term given by the precomputed scattering texture for the (n-1)-th order:
			Number nu1 = dot(omega_s, omega_i);
			RadianceSpectrum incident_radiance = GetScattering(atmosphere, single_rayleigh_scattering_texture, single_mie_scattering_texture, multiple_scattering_texture, r, omega_i.z, mu_s, nu1, ray_r_theta_intersects_ground, scattering_order - 1);

			// and of the contribution from the light paths with n-1 bounces and whose last bounce is on the ground. This contribution is the product of the
			// transmittance to the ground, the ground albedo, the ground BRDF, and the irradiance received on the ground after n-2 bounces.
			float3 ground_normal = normalize(zenith_direction * r + omega_i * distance_to_ground);
			IrradianceSpectrum ground_irradiance = GetIrradiance(atmosphere, irradiance_texture, atmosphere.bottom_radius, dot(ground_normal, omega_s));
			incident_radiance += transmittance_to_ground * ground_albedo * (1.0 / (M_PI * sr)) * ground_irradiance;

			// The radiance finally scattered from direction omega_i towards direction -omega is the product of the incident radiance, the scattering
			// coefficient, and the phase function for directions omega and omega_i (all this summed over all particle types, i.e. Rayleigh and Mie).
			Number nu2 = dot(omega, omega_i);
			Number rayleigh_density = exp(-(r - atmosphere.bottom_radius) / atmosphere.rayleigh_scale_height);
			Number mie_density = exp(-(r - atmosphere.bottom_radius) / atmosphere.mie_scale_height);

			rayleigh_mie += incident_radiance * (atmosphere.rayleigh_scattering * rayleigh_density * RayleighPhaseFunction(nu2) + 
												 atmosphere.mie_scattering * mie_density * MiePhaseFunction(atmosphere.mie_phase_function_g, nu2)) * domega_i;
		}
	}

	return rayleigh_mie;
}

RadianceSpectrum ComputeMultipleScattering(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture,IN(ScatteringDensityTexture) scattering_density_texture, Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);
	assert(mu_s >= -1.0 && mu_s <= 1.0);
	assert(nu >= -1.0 && nu <= 1.0);

	const int SAMPLE_COUNT = 50; // Number of intervals for the numerical integration.

	Length dx = DistanceToNearestAtmosphereBoundary(atmosphere, r, mu, ray_r_mu_intersects_ground) / Number(SAMPLE_COUNT); // The integration step, i.e. the length of each integration interval.

	RadianceSpectrum rayleigh_mie_sum = RadianceSpectrum(0.0 * watt_per_square_meter_per_sr_per_nm,
														 0.0 * watt_per_square_meter_per_sr_per_nm,
														 0.0 * watt_per_square_meter_per_sr_per_nm); // TODO : Initialize with zeros.

	// Integration loop.
	for (int i = 0; i <= SAMPLE_COUNT; ++i) 
	{
		Length d_i = Number(i) * dx;

		// The r, mu and mu_s parameters at the current integration point (see the single scattering section for a detailed explanation).
		Length r_i = ClampRadius(atmosphere, sqrt(d_i * d_i + 2.0 * r * mu * d_i + r * r));

		Number mu_i = ClampCosine((r * mu + d_i) / r_i);
		Number mu_s_i = ClampCosine((r * mu_s + d_i * nu) / r_i);

		// The Rayleigh and Mie multiple scattering at the current sample point.
		RadianceSpectrum rayleigh_mie_i = GetScattering(atmosphere, scattering_density_texture, r_i, mu_i, mu_s_i, nu, ray_r_mu_intersects_ground) *
										  GetTransmittance(atmosphere, transmittance_texture, r, mu, d_i, ray_r_mu_intersects_ground) *	dx;
		// Sample weight (from the trapezoidal rule).
		Number weight_i = (i == 0 || i == SAMPLE_COUNT) ? 0.5 : 1.0;

		rayleigh_mie_sum += rayleigh_mie_i * weight_i;
  }

  return rayleigh_mie_sum;
}

RadianceDensitySpectrum ComputeScatteringDensityTexture(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(ReducedScatteringTexture) single_rayleigh_scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, IN(ScatteringTexture) multiple_scattering_texture, IN(IrradianceTexture) irradiance_texture, IN(float3) gl_frag_coord, int scattering_order) 
{
	Length r;
	Number mu;
	Number mu_s;
	Number nu;
	bool ray_r_mu_intersects_ground;

	GetRMuMuSNuFromScatteringTextureFragCoord(atmosphere, gl_frag_coord, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

	return ComputeScatteringDensity(atmosphere, transmittance_texture, single_rayleigh_scattering_texture, single_mie_scattering_texture, multiple_scattering_texture, irradiance_texture, r, mu, mu_s, nu, scattering_order);
}

RadianceSpectrum ComputeMultipleScatteringTexture(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(ScatteringDensityTexture) scattering_density_texture, IN(float3) gl_frag_coord, OUT(Number) nu) 
{
	Length r;
	Number mu;
	Number mu_s;
	bool ray_r_mu_intersects_ground;

	GetRMuMuSNuFromScatteringTextureFragCoord(atmosphere, gl_frag_coord, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

	return ComputeMultipleScattering(atmosphere, transmittance_texture, scattering_density_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground);
}

IrradianceSpectrum ComputeDirectIrradiance(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, Length r, Number mu_s) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu_s >= -1.0 && mu_s <= 1.0);

	return atmosphere.solar_irradiance * GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r, mu_s) * max(mu_s, 0.0);
}

IrradianceSpectrum ComputeIndirectIrradiance(IN(AtmosphereParameters) atmosphere, IN(ReducedScatteringTexture) single_rayleigh_scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, IN(ScatteringTexture) multiple_scattering_texture, Length r, Number mu_s, int scattering_order) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu_s >= -1.0 && mu_s <= 1.0);
	assert(scattering_order >= 1);

	const int SAMPLE_COUNT = 32;
	const Angle dphi = pi / Number(SAMPLE_COUNT);
	const Angle dtheta = pi / Number(SAMPLE_COUNT);

	IrradianceSpectrum result = IrradianceSpectrum(0.0 * watt_per_square_meter_per_nm,
												   0.0 * watt_per_square_meter_per_nm,
												   0.0 * watt_per_square_meter_per_nm); // TODO : Initialize with zeros.

	float3 omega_s = float3(sqrt(1.0 - mu_s * mu_s), 0.0, mu_s);

	for (int j = 0; j < SAMPLE_COUNT / 2; ++j) 
	{
		Angle theta = (Number(j) + 0.5) * dtheta;

		bool ray_r_theta_intersects_ground = RayIntersectsGround(atmosphere, r, cos(theta));

		for (int i = 0; i < 2 * SAMPLE_COUNT; ++i) 
		{
			Angle phi = (Number(i) + 0.5) * dphi;

			float3 omega = float3(cos(phi) * sin(theta), sin(phi) * sin(theta), cos(theta));

			SolidAngle domega = (dtheta / rad) * (dphi / rad) * sin(theta) * sr;

			Number nu = dot(omega, omega_s);

			result += GetScattering(atmosphere, single_rayleigh_scattering_texture, single_mie_scattering_texture, multiple_scattering_texture, r, omega.z, mu_s, nu, ray_r_theta_intersects_ground, scattering_order) * omega.z * domega;
		}
	}

	return result;
}

float2 GetIrradianceTextureUvFromRMuS(IN(AtmosphereParameters) atmosphere, Length r, Number mu_s) 
{
	assert(r >= atmosphere.bottom_radius && r <= atmosphere.top_radius);
	assert(mu_s >= -1.0 && mu_s <= 1.0);

	Number x_r = (r - atmosphere.bottom_radius) / (atmosphere.top_radius - atmosphere.bottom_radius);
	Number x_mu_s = mu_s * 0.5 + 0.5;

	return float2(GetTextureCoordFromUnitRange(x_mu_s, IRRADIANCE_TEXTURE_WIDTH), GetTextureCoordFromUnitRange(x_r, IRRADIANCE_TEXTURE_HEIGHT));
}

void GetRMuSFromIrradianceTextureUv(IN(AtmosphereParameters) atmosphere, IN(float2) uv, OUT(Length) r, OUT(Number) mu_s) 
{
	assert(uv.x >= 0.0 && uv.x <= 1.0);
	assert(uv.y >= 0.0 && uv.y <= 1.0);

	Number x_mu_s = GetUnitRangeFromTextureCoord(uv.x, IRRADIANCE_TEXTURE_WIDTH);
	Number x_r = GetUnitRangeFromTextureCoord(uv.y, IRRADIANCE_TEXTURE_HEIGHT);

	r = atmosphere.bottom_radius + x_r * (atmosphere.top_radius - atmosphere.bottom_radius);
	mu_s = ClampCosine(2.0 * x_mu_s - 1.0);
}

IrradianceSpectrum ComputeDirectIrradianceTexture(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(float2) gl_frag_coord) 
{
	const float2 IRRADIANCE_TEXTURE_SIZE = float2(IRRADIANCE_TEXTURE_WIDTH, IRRADIANCE_TEXTURE_HEIGHT);

	Length r;
	Number mu_s;

	GetRMuSFromIrradianceTextureUv(atmosphere, gl_frag_coord / IRRADIANCE_TEXTURE_SIZE, r, mu_s);

	return ComputeDirectIrradiance(atmosphere, transmittance_texture, r, mu_s);
}

IrradianceSpectrum ComputeIndirectIrradianceTexture(IN(AtmosphereParameters) atmosphere, IN(ReducedScatteringTexture) single_rayleigh_scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, IN(ScatteringTexture) multiple_scattering_texture, IN(float2) gl_frag_coord, int scattering_order) 
{
	const float2 IRRADIANCE_TEXTURE_SIZE = float2(IRRADIANCE_TEXTURE_WIDTH, IRRADIANCE_TEXTURE_HEIGHT);

	Length r;
	Number mu_s;

	GetRMuSFromIrradianceTextureUv(atmosphere, gl_frag_coord / IRRADIANCE_TEXTURE_SIZE, r, mu_s);

	return ComputeIndirectIrradiance(atmosphere, single_rayleigh_scattering_texture, single_mie_scattering_texture, multiple_scattering_texture, r, mu_s, scattering_order);
}

IrradianceSpectrum GetIrradiance(IN(AtmosphereParameters) atmosphere, IN(IrradianceTexture) irradiance_texture, Length r, Number mu_s) 
{
  float2 uv = GetIrradianceTextureUvFromRMuS(atmosphere, r, mu_s);

  return IrradianceSpectrum(tex2D(irradiance_texture, uv).rgb);
}

#ifdef COMBINED_SCATTERING_TEXTURES
float3 GetExtrapolatedSingleMieScattering(IN(AtmosphereParameters) atmosphere, IN(vec4) scattering) 
{
	if (scattering.r == 0.0) 
	{
		return float3(0.0, 0.0, 0.0);
	}

	return scattering.rgb * scattering.a / scattering.r *	(atmosphere.rayleigh_scattering.r / atmosphere.mie_scattering.r) * (atmosphere.mie_scattering / atmosphere.rayleigh_scattering);
}
#endif

IrradianceSpectrum GetCombinedScattering(IN(AtmosphereParameters) atmosphere, IN(ReducedScatteringTexture) scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, Length r, Number mu, Number mu_s, Number nu, bool ray_r_mu_intersects_ground, OUT(IrradianceSpectrum) single_mie_scattering) 
{
	float4 uvwz = GetScatteringTextureUvwzFromRMuMuSNu(atmosphere, r, mu, mu_s, nu, ray_r_mu_intersects_ground);

	Number tex_coord_x = uvwz.x * Number(SCATTERING_TEXTURE_NU_SIZE - 1);
	Number tex_x = floor(tex_coord_x);
	Number blend = tex_coord_x - tex_x;

	float3 uvw0 = float3((tex_x + uvwz.y) / Number(SCATTERING_TEXTURE_NU_SIZE), uvwz.z, uvwz.w);
	float3 uvw1 = float3((tex_x + 1.0 + uvwz.y) / Number(SCATTERING_TEXTURE_NU_SIZE), uvwz.z, uvwz.w);

	#ifdef COMBINED_SCATTERING_TEXTURES
		float4 combined_scattering = tex3D(scattering_texture, uvw0) * (1.0 - blend) + tex3D(scattering_texture, uvw1) * blend;

		IrradianceSpectrum scattering = IrradianceSpectrum(combined_scattering);

		single_mie_scattering = GetExtrapolatedSingleMieScattering(atmosphere, combined_scattering);
	#else
		IrradianceSpectrum scattering = IrradianceSpectrum(tex3D(scattering_texture, uvw0).rgb * (1.0 - blend) + tex3D(scattering_texture, uvw1).rgb * blend);

		single_mie_scattering = IrradianceSpectrum(tex3D(single_mie_scattering_texture, uvw0).rgb * (1.0 - blend) + tex3D(single_mie_scattering_texture, uvw1).rgb * blend);
	#endif

	return scattering;
}

RadianceSpectrum GetSkyRadiance(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(ReducedScatteringTexture) scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, Position camera, IN(Direction) view_ray, Length shadow_length, IN(Direction) sun_direction, OUT(DimensionlessSpectrum) transmittance) 
{
	// Compute the distance to the top atmosphere boundary along the view ray, assuming the viewer is in space (or NaN if the view ray does not intersect the atmosphere).
	Length r = length(camera);
	Length rmu = dot(camera, view_ray);
	Length distance_to_top_atmosphere_boundary = -rmu - sqrt(rmu * rmu - r * r + atmosphere.top_radius * atmosphere.top_radius);

	// If the viewer is in space and the view ray intersects the atmosphere, move the viewer to the top atmosphere boundary (along the view ray):
	if (distance_to_top_atmosphere_boundary > 0.0 * m) 
	{
		camera = camera + view_ray * distance_to_top_atmosphere_boundary;
		r = atmosphere.top_radius;
		rmu += distance_to_top_atmosphere_boundary;
	}

	// If the view ray does not intersect the atmosphere, simply return 0.
	if (r > atmosphere.top_radius) 
	{
		transmittance = DimensionlessSpectrum(1.0, 1.0, 1.0);

		return RadianceSpectrum(0.0 * watt_per_square_meter_per_sr_per_nm,
							    0.0 * watt_per_square_meter_per_sr_per_nm,
								0.0 * watt_per_square_meter_per_sr_per_nm); // TODO : Initialize with zeros.
	}
	
	// Compute the r, mu, mu_s and nu parameters needed for the texture lookups.
	Number mu = rmu / r;
	Number mu_s = dot(camera, sun_direction) / r;
	Number nu = dot(view_ray, sun_direction);
	bool ray_r_mu_intersects_ground = RayIntersectsGround(atmosphere, r, mu);

	transmittance = ray_r_mu_intersects_ground ? DimensionlessSpectrum(0.0, 0.0, 0.0) : GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r, mu);

	IrradianceSpectrum single_mie_scattering;
	IrradianceSpectrum scattering;

	if (shadow_length == 0.0 * m) 
	{
		scattering = GetCombinedScattering(atmosphere, scattering_texture, single_mie_scattering_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground, single_mie_scattering);
	} 
	else 
	{
		// Case of light shafts (shadow_length is the total length noted l in our
		// paper): we omit the scattering between the camera and the point at
		// distance l, by implementing Eq. (18) of the paper (shadow_transmittance
		// is the T(x,x_s) term, scattering is the S|x_s=x+lv term).
		Length d = shadow_length;
		Length r_p = ClampRadius(atmosphere, sqrt(d * d + 2.0 * r * mu * d + r * r));
		Number mu_p = (r * mu + d) / r_p;
		Number mu_s_p = (r * mu_s + d * nu) / r_p;

		scattering = GetCombinedScattering(atmosphere, scattering_texture, single_mie_scattering_texture, r_p, mu_p, mu_s_p, nu, ray_r_mu_intersects_ground, single_mie_scattering);

		DimensionlessSpectrum shadow_transmittance = GetTransmittance(atmosphere, transmittance_texture, r, mu, shadow_length, ray_r_mu_intersects_ground);

		scattering = scattering * shadow_transmittance;
		single_mie_scattering = single_mie_scattering * shadow_transmittance;
	}

	return scattering * RayleighPhaseFunction(nu) + single_mie_scattering * MiePhaseFunction(atmosphere.mie_phase_function_g, nu);
}

RadianceSpectrum GetSkyRadianceToPoint(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(ReducedScatteringTexture) scattering_texture, IN(ReducedScatteringTexture) single_mie_scattering_texture, Position camera, IN(Position) ppoint, Length shadow_length, IN(Direction) sun_direction, OUT(DimensionlessSpectrum) transmittance) 
{
  // Compute the distance to the top atmosphere boundary along the view ray, assuming the viewer is in space (or NaN if the view ray does not intersect the atmosphere).
	Direction view_ray = normalize(ppoint - camera);

	Length r = length(camera);
	Length rmu = dot(camera, view_ray);
	Length distance_to_top_atmosphere_boundary = -rmu - sqrt(rmu * rmu - r * r + atmosphere.top_radius * atmosphere.top_radius);

	// If the viewer is in space and the view ray intersects the atmosphere, move the viewer to the top atmosphere boundary (along the view ray):
	if (distance_to_top_atmosphere_boundary > 0.0 * m) 
	{
		camera = camera + view_ray * distance_to_top_atmosphere_boundary;
		r = atmosphere.top_radius;
		rmu += distance_to_top_atmosphere_boundary;
	}

	// Compute the r, mu, mu_s and nu parameters for the first texture lookup.
	Number mu = rmu / r;
	Number mu_s = dot(camera, sun_direction) / r;
	Number nu = dot(view_ray, sun_direction);
	Length d = length(ppoint - camera);
	bool ray_r_mu_intersects_ground = RayIntersectsGround(atmosphere, r, mu);

	transmittance = GetTransmittance(atmosphere, transmittance_texture, r, mu, d, ray_r_mu_intersects_ground);

	IrradianceSpectrum single_mie_scattering;
	IrradianceSpectrum scattering = GetCombinedScattering(atmosphere, scattering_texture, single_mie_scattering_texture, r, mu, mu_s, nu, ray_r_mu_intersects_ground, single_mie_scattering);

	// Compute the r, mu, mu_s and nu parameters for the second texture lookup.
	// If shadow_length is not 0 (case of light shafts), we want to ignore the
	// scattering along the last shadow_length meters of the view ray, which we
	// do by subtracting shadow_length from d (this way scattering_p is equal to
	// the S|x_s=x_0-lv term in Eq. (17) of our paper).
	d = max(d - shadow_length, 0.0 * m);
	Length r_p = ClampRadius(atmosphere, sqrt(d * d + 2.0 * r * mu * d + r * r));
	Number mu_p = (r * mu + d) / r_p;
	Number mu_s_p = (r * mu_s + d * nu) / r_p;

	IrradianceSpectrum single_mie_scattering_p;
	IrradianceSpectrum scattering_p = GetCombinedScattering(atmosphere, scattering_texture, single_mie_scattering_texture, r_p, mu_p, mu_s_p, nu, ray_r_mu_intersects_ground, single_mie_scattering_p);

	// Combine the lookup results to get the scattering between camera and point.
	DimensionlessSpectrum shadow_transmittance = transmittance;

	if (shadow_length > 0.0 * m) 
	{
		// This is the T(x,x_s) term in Eq. (17) of our paper, for light shafts.
		shadow_transmittance = GetTransmittance(atmosphere, transmittance_texture, r, mu, d, ray_r_mu_intersects_ground);
	}

	scattering = scattering - shadow_transmittance * scattering_p;
	single_mie_scattering = single_mie_scattering - shadow_transmittance * single_mie_scattering_p;

	#ifdef COMBINED_SCATTERING_TEXTURES
		single_mie_scattering = GetExtrapolatedSingleMieScattering(atmosphere, float4(scattering, single_mie_scattering.r));
	#endif

	// Hack to avoid rendering artifacts when the sun is below the horizon.
	single_mie_scattering = single_mie_scattering * smoothstep(Number(0.0), Number(0.01), mu_s);

	return scattering * RayleighPhaseFunction(nu) + single_mie_scattering * MiePhaseFunction(atmosphere.mie_phase_function_g, nu);
}

IrradianceSpectrum GetSunAndSkyIrradiance(IN(AtmosphereParameters) atmosphere, IN(TransmittanceTexture) transmittance_texture, IN(IrradianceTexture) irradiance_texture, IN(Position) ppoint, IN(Direction) normal, IN(Direction) sun_direction, OUT(IrradianceSpectrum) sky_irradiance) 
{
	Length r = length(ppoint);
	Number mu_s = dot(ppoint, sun_direction) / r;

	// Indirect irradiance (approximated if the surface is not horizontal).
	sky_irradiance = GetIrradiance(atmosphere, irradiance_texture, r, mu_s) * (1.0 + dot(normal, ppoint) / r) * 0.5;

	// Direct irradiance.
	return atmosphere.solar_irradiance * GetTransmittanceToTopAtmosphereBoundary(atmosphere, transmittance_texture, r, mu_s) * 
	smoothstep(-atmosphere.sun_angular_radius / rad,
				atmosphere.sun_angular_radius / rad, mu_s) * max(dot(normal, sun_direction), 0.0);
}
//----------------------------------------------------------------------------------------------------------------------------