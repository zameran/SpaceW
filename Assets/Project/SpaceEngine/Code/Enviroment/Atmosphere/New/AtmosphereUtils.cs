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
// Creation Date: 2017.03.19
// Creation Time: 9:44 PM
// Creator: zameran
#endregion

#region Original License
/**
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
#endregion

using System;

using UnityEngine.Assertions;

namespace SpaceEngine.AtmosphericScatteringAcurate
{
    public static class AtmosphereUtils
    {
        public const int kLambdaMin = 360;
        public const int kLambdaMax = 830;
        public const double kLambdaR = 680.0;
        public const double kLambdaG = 550.0;
        public const double kLambdaB = 440.0;

        public static double Interpolate(double[] wavelengths, double[] wavelength_function, double wavelength)
        {
            Assert.IsTrue(wavelength_function.Length == wavelengths.Length);

            if (wavelength < wavelengths[0])
            {
                return wavelength_function[0];
            }

            for (uint i = 0; i < wavelengths.Length - 1; i++)
            {
                if (wavelength < wavelengths[i + 1])
                {
                    var u = (wavelength - wavelengths[i]) / (wavelengths[i + 1] - wavelengths[i]);

                    return wavelength_function[i] * (1.0 - u) + wavelength_function[i + 1] * u;
                }
            }

            return wavelength_function[wavelength_function.Length - 1];
        }

        public static double CieColorMatchingFunctionTableValue(double wavelength, int column)
        {
            if (wavelength <= kLambdaMin || wavelength >= kLambdaMax) { return 0.0; }

            var u = (wavelength - kLambdaMin) / 5.0;
            var row = (int)Math.Floor(u);

            Assert.IsTrue(row >= 0 && row + 1 < 95);
            Assert.IsTrue(AtmosphereConstants.CIE_2_DEG_COLOR_MATCHING_FUNCTIONS[4 * row] <= wavelength && AtmosphereConstants.CIE_2_DEG_COLOR_MATCHING_FUNCTIONS[4 * (row + 1)] >= wavelength);

            u -= row;

            return AtmosphereConstants.CIE_2_DEG_COLOR_MATCHING_FUNCTIONS[4 * row + column] * (1.0 - u) + AtmosphereConstants.CIE_2_DEG_COLOR_MATCHING_FUNCTIONS[4 * (row + 1) + column] * u;
        }

        public static void ConvertSpectrumToLinearSrgb(double[] wavelengths, double[] spectrum, ref double r, ref double g, ref double b)
        {
            double x = 0.0;
            double y = 0.0;
            double z = 0.0;

            int dlambda = 1;

            for (var lambda = kLambdaMin; lambda < kLambdaMax; lambda += dlambda)
            {
                var value = Interpolate(wavelengths, spectrum, lambda);

                x += CieColorMatchingFunctionTableValue(lambda, 1) * value;
                y += CieColorMatchingFunctionTableValue(lambda, 2) * value;
                z += CieColorMatchingFunctionTableValue(lambda, 3) * value;
            }

            r = AtmosphereConstants.MAX_LUMINOUS_EFFICACY * (AtmosphereConstants.XYZ_TO_SRGB[0] * x + AtmosphereConstants.XYZ_TO_SRGB[1] * y + AtmosphereConstants.XYZ_TO_SRGB[2] * z) * dlambda;
            g = AtmosphereConstants.MAX_LUMINOUS_EFFICACY * (AtmosphereConstants.XYZ_TO_SRGB[3] * x + AtmosphereConstants.XYZ_TO_SRGB[4] * y + AtmosphereConstants.XYZ_TO_SRGB[5] * z) * dlambda;
            b = AtmosphereConstants.MAX_LUMINOUS_EFFICACY * (AtmosphereConstants.XYZ_TO_SRGB[6] * x + AtmosphereConstants.XYZ_TO_SRGB[7] * y + AtmosphereConstants.XYZ_TO_SRGB[8] * z) * dlambda;
        }

        public static void ComputeSpectralRadianceToLuminanceFactors(double[] wavelengths, double[] solar_irradiance, double lambda_power, ref double k_r, ref double k_g, ref double k_b)
        {
            k_r = 0.0;
            k_g = 0.0;
            k_b = 0.0;

            var solar_r = Interpolate(wavelengths, solar_irradiance, kLambdaR);
            var solar_g = Interpolate(wavelengths, solar_irradiance, kLambdaG);
            var solar_b = Interpolate(wavelengths, solar_irradiance, kLambdaB);

            int dlambda = 1;

            for (var lambda = kLambdaMin; lambda < kLambdaMax; lambda += dlambda)
            {
                var x_bar = CieColorMatchingFunctionTableValue(lambda, 1);
                var y_bar = CieColorMatchingFunctionTableValue(lambda, 2);
                var z_bar = CieColorMatchingFunctionTableValue(lambda, 3);

                var r_bar = AtmosphereConstants.XYZ_TO_SRGB[0] * x_bar + AtmosphereConstants.XYZ_TO_SRGB[1] * y_bar + AtmosphereConstants.XYZ_TO_SRGB[2] * z_bar;
                var g_bar = AtmosphereConstants.XYZ_TO_SRGB[3] * x_bar + AtmosphereConstants.XYZ_TO_SRGB[4] * y_bar + AtmosphereConstants.XYZ_TO_SRGB[5] * z_bar;
                var b_bar = AtmosphereConstants.XYZ_TO_SRGB[6] * x_bar + AtmosphereConstants.XYZ_TO_SRGB[7] * y_bar + AtmosphereConstants.XYZ_TO_SRGB[8] * z_bar;

                var irradiance = Interpolate(wavelengths, solar_irradiance, lambda);

                k_r += r_bar * irradiance / solar_r * Math.Pow(lambda / kLambdaR, lambda_power);
                k_g += g_bar * irradiance / solar_g * Math.Pow(lambda / kLambdaG, lambda_power);
                k_b += b_bar * irradiance / solar_b * Math.Pow(lambda / kLambdaB, lambda_power);
            }

            k_r *= AtmosphereConstants.MAX_LUMINOUS_EFFICACY * dlambda;
            k_g *= AtmosphereConstants.MAX_LUMINOUS_EFFICACY * dlambda;
            k_b *= AtmosphereConstants.MAX_LUMINOUS_EFFICACY * dlambda;
        }
    }
}