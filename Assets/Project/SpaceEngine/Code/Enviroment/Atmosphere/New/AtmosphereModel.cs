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
// Creation Time: 7:38 AM
// Creator: zameran
#endregion

namespace SpaceEngine.AtmosphericScatteringAcurate
{
    public static class AtmosphereModel
    {
        public static double kPi = 3.1415926;
        public static double kSunAngularRadius = 0.00935 / 2.0;
        public static double kSunSolidAngle = kPi * kSunAngularRadius * kSunAngularRadius;
        public static double kLengthUnitInMeters = 1000.0;

        public static int kLambdaMin = 360;
        public static int kLambdaMax = 830;
        public static double[] kSolarIrradiance = new double[]
        {
            1.11776, 1.14259, 1.01249, 1.14716, 1.72765, 1.73054, 1.6887, 1.61253,
            1.91198, 2.03474, 2.02042, 2.02212, 1.93377, 1.95809, 1.91686, 1.8298,
            1.8685, 1.8931, 1.85149, 1.8504, 1.8341, 1.8345, 1.8147, 1.78158, 1.7533,
            1.6965, 1.68194, 1.64654, 1.6048, 1.52143, 1.55622, 1.5113, 1.474, 1.4482,
            1.41018, 1.36775, 1.34188, 1.31429, 1.28303, 1.26758, 1.2367, 1.2082,
            1.18737, 1.14683, 1.12362, 1.1058, 1.07124, 1.04992
        };

        public static double kConstantSolarIrradiance = 1.5;
        public static double kBottomRadius = 6360000.0;
        public static double kTopRadius = 6420000.0;
        public static double kRayleigh = 1.24062e-6;
        public static double kRayleighScaleHeight = 8000.0;
        public static double kMieScaleHeight = 1200.0;
        public static double kMieAngstromAlpha = 0.0;
        public static double kMieAngstromBeta = 5.328e-3;
        public static double kMieSingleScatteringAlbedo = 0.9;
        public static double kMiePhaseFunctionG = 0.8;
        public static double kGroundAlbedo = 0.1;
        public static double kMaxSunZenithAngle = 102.0 / 180.0 * kPi;
    }
}