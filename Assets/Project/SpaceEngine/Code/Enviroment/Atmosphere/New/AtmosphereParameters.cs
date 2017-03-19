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

using UnityEngine;

namespace SpaceEngine.AtmosphericScatteringAcurate
{
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
    }
}