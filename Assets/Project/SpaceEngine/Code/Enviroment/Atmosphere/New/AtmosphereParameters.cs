#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.03.20
// Creation Time: 12:15 AM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

using UnityEngine;

using Model = SpaceEngine.AtmosphericScatteringAcurate.AtmosphereModel;

namespace SpaceEngine.AtmosphericScatteringAcurate
{
    [Serializable]
    public struct AtmosphereParameters
    {
        /// <summary>
        /// The solar irradiance at the top of the atmosphere.
        /// </summary>
        public Vector3 solar_irradiance;

        /// <summary>
        /// he sun's angular radius.
        /// </summary>
        public float sun_angular_radius;

        /// <summary>
        /// The distance between the planet center and the bottom of the atmosphere.
        /// </summary>
        public float bottom_radius;

        /// <summary>
        /// The distance between the planet center and the top of the atmosphere.
        /// </summary>
        public float top_radius;

        /// <summary>
        /// The scale height of air molecules, meaning that their density is proportional to exp(-h / rayleigh_scale_height), with h the altitude (with the bottom of the atmosphere at altitude 0).
        /// </summary>
        public float rayleigh_scale_height;

        /// <summary>
        /// The scattering coefficient of air molecules at the bottom of the atmosphere, as a function of wavelength.
        /// </summary>
        public Vector3 rayleigh_scattering;

        /// <summary>
        /// The scale height of aerosols, meaning that their density is proportional to exp(-h / mie_scale_height), with h the altitude.
        /// </summary>
        public float mie_scale_height;

        /// <summary>
        /// The scattering coefficient of aerosols at the bottom of the atmosphere, as a function of wavelength.
        /// </summary>
        public Vector3 mie_scattering;

        /// <summary>
        /// The extinction coefficient of aerosols at the bottom of the atmosphere, as a function of wavelength.
        /// </summary>
        public Vector3 mie_extinction;

        /// <summary>
        /// The asymetry parameter for the Cornette-Shanks phase function for the aerosols.
        /// </summary>
        public float mie_phase_function_g;

        /// <summary>
        /// The average albedo of the ground.
        /// </summary>
        public Vector3 ground_albedo;

        /// <summary>
        /// The cosine of the maximum Sun zenith angle for which atmospheric scattering
        /// must be precomputed (for maximum precision, use the smallest Sun zenith
        /// angle yielding negligible sky light radiance values. For instance, for the
        /// Earth case, 102 degrees is a good choice - yielding mu_s_min = -0.2).
        /// </summary>
        public float mu_s_min;

        public AtmosphereParameters(Vector3 solarIrradiance, float sunAngularRadius, float bottomRadius, float topRadius, float rayleighScaleHeight, Vector3 rayleighScattering, float mieScaleHeight, Vector3 mieScattering, Vector3 mieExtinction, float miePhaseFunctionG, Vector3 groundAlbedo, float muSMin)
        {
            solar_irradiance = solarIrradiance;
            sun_angular_radius = sunAngularRadius;
            bottom_radius = bottomRadius;
            top_radius = topRadius;
            rayleigh_scale_height = rayleighScaleHeight;
            rayleigh_scattering = rayleighScattering;
            mie_scale_height = mieScaleHeight;
            mie_scattering = mieScattering;
            mie_extinction = mieExtinction;
            mie_phase_function_g = miePhaseFunctionG;
            ground_albedo = groundAlbedo;
            mu_s_min = muSMin;
        }

        public static AtmosphereParameters Default(bool useConstantSolarSpectrum)
        {
            List<double> _wavelengths = new List<double>();
            List<double> _solar_irradiance = new List<double>();
            List<double> _rayleigh_scattering = new List<double>();
            List<double> _mie_scattering = new List<double>();
            List<double> _mie_extinction = new List<double>();
            List<double> _ground_albedo = new List<double>();

            for (uint l = AtmosphereUtils.kLambdaMin; l < AtmosphereUtils.kLambdaMax; l += 10)
            {
                var lambda = (double)l * 1e-3;  // Micro-meters...
                var mie = Model.kMieAngstromBeta / Model.kMieScaleHeight * Math.Pow(lambda, -Model.kMieAngstromAlpha);

                _wavelengths.Add(l);
                _solar_irradiance.Add(useConstantSolarSpectrum ? Model.kConstantSolarIrradiance : Model.kSolarIrradiance[(l - Model.kLambdaMin) / 10]);
                _rayleigh_scattering.Add(Model.kRayleigh * Math.Pow(lambda, -4));
                _mie_scattering.Add(mie * Model.kMieSingleScatteringAlbedo);
                _mie_extinction.Add(mie);
                _ground_albedo.Add(Model.kGroundAlbedo);
            }

            double sky_k_r = 0;
            double sky_k_g = 0;
            double sky_k_b = 0;
            AtmosphereUtils.ComputeSpectralRadianceToLuminanceFactors(_wavelengths.ToArray(), _solar_irradiance.ToArray(), -3, ref sky_k_r, ref sky_k_g, ref sky_k_b);

            double sun_k_r = 0;
            double sun_k_g = 0;
            double sun_k_b = 0;
            AtmosphereUtils.ComputeSpectralRadianceToLuminanceFactors(_wavelengths.ToArray(), _solar_irradiance.ToArray(), 0, ref sun_k_r, ref sun_k_g, ref sun_k_b);

            Vector3 solarIrradiance = AtmosphereUtils.ToVector(_wavelengths.ToArray(), _solar_irradiance.ToArray(), 1.0);
            Vector3 rayLeighScattering = AtmosphereUtils.ToVector(_wavelengths.ToArray(), _rayleigh_scattering.ToArray(), Model.kLengthUnitInMeters);
            Vector3 mieScattering = AtmosphereUtils.ToVector(_wavelengths.ToArray(), _mie_scattering.ToArray(), Model.kLengthUnitInMeters);
            Vector3 mieExtinction = AtmosphereUtils.ToVector(_wavelengths.ToArray(), _mie_extinction.ToArray(), Model.kLengthUnitInMeters);
            Vector3 groundAlbedo = AtmosphereUtils.ToVector(_wavelengths.ToArray(), _ground_albedo.ToArray(), 1.0);
            Vector3 skyK = new Vector3((float)sky_k_r, (float)sky_k_g, (float)sky_k_b);
            Vector3 sunK = new Vector3((float)sun_k_r, (float)sun_k_g, (float)sun_k_b);

            double maxSunZenithAngle = Math.Cos(Model.kMaxSunZenithAngle);

            return new AtmosphereParameters(solarIrradiance, (float)Model.kSunAngularRadius, (float)(Model.kBottomRadius / Model.kLengthUnitInMeters),
                                                                                (float)(Model.kTopRadius / Model.kLengthUnitInMeters),
                                                                                (float)(Model.kRayleighScaleHeight / Model.kLengthUnitInMeters),
                                                                                rayLeighScattering,
                                                                                (float)(Model.kMieScaleHeight / Model.kLengthUnitInMeters),
                                                                                mieScattering,
                                                                                mieExtinction,
                                                                                (float)Model.kMiePhaseFunctionG,
                                                                                groundAlbedo,
                                                                                (float)maxSunZenithAngle);
        }
    }
}