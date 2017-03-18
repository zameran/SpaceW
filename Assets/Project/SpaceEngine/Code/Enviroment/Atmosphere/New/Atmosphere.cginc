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

uniform const Length		m		= 1.0;
uniform const Wavelength	nm		= 1.0;
uniform const Angle			rad		= 1.0;
uniform const SolidAngle	sr		= 1.0;
uniform const Power			watt	= 1.0;
uniform const LuminousPower lm		= 1.0;

/*
uniform const Length					km											= 1000.0 * m;
uniform const Area						m2											= m * m;
uniform const Volume					m3											= m * m * m;
uniform const Angle						pi											= PI * rad;
uniform const Angle						deg											= pi / 180.0;
uniform const Irradiance				watt_per_square_meter						= watt / m2;
uniform const Radiance					watt_per_square_meter_per_sr				= watt / (m2 * sr);
uniform const SpectralIrradiance		watt_per_square_meter_per_nm				= watt / (m2 * nm);
uniform const SpectralRadiance			watt_per_square_meter_per_sr_per_nm			= watt / (m2 * sr * nm);
uniform const SpectralRadianceDensity	watt_per_cubic_meter_per_sr_per_nm			= watt / (m3 * sr * nm);
uniform const LuminousIntensity			cd											= lm / sr;
uniform const LuminousIntensity			kcd											= 1000.0 * cd;
uniform const Luminance					cd_per_square_meter							= cd / m2;
uniform const Luminance					kcd_per_square_meter						= kcd / m2;
*/

#if !defined (M_PI)
#define M_PI 3.14159265358979323846
#endif

#define IN(x) in x
#define OUT(x) out x
#define TEMPLATE(x)
#define TEMPLATE_ARGUMENT(x)
#define assert(x)

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
	return sqrt(a); //return sqrt(max(a, 0.0 * m2));
}

Length DistanceToTopAtmosphereBoundary(IN(AtmosphereParameters) atmosphere,	Length r, Number mu) 
{
	assert(r <= atmosphere.top_radius);
	assert(mu >= -1.0 && mu <= 1.0);

	Area discriminant = r * r * (mu * mu - 1.0) + atmosphere.top_radius * atmosphere.top_radius;

	return ClampDistance(-r * mu + SafeSqrt(discriminant));
}