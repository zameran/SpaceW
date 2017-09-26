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
//    notice, this list of conditions and the following disclaimer.
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;

using UnityEngine;

namespace SpaceEngine.Enviroment.Atmospheric
{
    [Serializable]
    public struct AtmosphereParameters
    {
        //Asymmetry factor for the mie phase function
        //A higher number meands more light is scattered in the forward direction
        public float MIE_G;

        //Half heights for the atmosphere air density (HR) and particle density (HM)
        //This is the height in km that half the particles are found below
        public float HR;
        public float HM;

        //Physical settings, Mie and Rayliegh values
        public float AVERAGE_GROUND_REFLECTANCE;

        public Vector4 BETA_R;
        public Vector4 BETA_MSca;
        public Vector4 BETA_MEx;

        public float Rg;
        public float Rt;
        public float Rl;

        public float bRg;
        public float bRt;
        public float bRl;

        public float SCALE;

        public AtmosphereParameters(AtmosphereParameters from)
        {
            this.MIE_G = from.MIE_G;

            this.HR = from.HR;
            this.HM = from.HM;

            this.AVERAGE_GROUND_REFLECTANCE = from.AVERAGE_GROUND_REFLECTANCE;

            this.BETA_R = from.BETA_R;
            this.BETA_MSca = from.BETA_MSca;
            this.BETA_MEx = from.BETA_MEx;

            this.Rg = from.Rg;
            this.Rt = from.Rt;
            this.Rl = from.Rl;

            this.bRg = from.bRg;
            this.bRt = from.bRt;
            this.bRl = from.bRl;

            this.SCALE = from.SCALE;
        }

        public AtmosphereParameters(float MIE_G, float HR, float HM, float AVERAGE_GROUND_REFLECTANCE,
                                    Vector4 BETA_R,
                                    Vector4 BETA_MSca,
                                    Vector4 BETA_MEx,
                                    float Rg, float Rt, float Rl,
                                    float bRg, float bRt, float bRl,
                                    float SCALE)
        {
            this.MIE_G = MIE_G;

            this.HR = HR;
            this.HM = HM;

            this.AVERAGE_GROUND_REFLECTANCE = AVERAGE_GROUND_REFLECTANCE;

            this.BETA_R = BETA_R;
            this.BETA_MSca = BETA_MSca;
            this.BETA_MEx = BETA_MEx;

            this.Rg = Rg;
            this.Rt = Rt;
            this.Rl = Rl;

            this.bRg = bRg;
            this.bRt = bRt;
            this.bRl = bRl;

            this.SCALE = SCALE;
        }

        public static AtmosphereParameters Get(AtmosphereBase preset)
        {
            switch (preset)
            {
                case AtmosphereBase.Default: return Default;
                case AtmosphereBase.Earth: return Earth;
                case AtmosphereBase.Venus: return Venus;
                case AtmosphereBase.Mars: return Mars;
                case AtmosphereBase.Jupiter: return Jupiter;
                case AtmosphereBase.Titan: return Titan;
                case AtmosphereBase.Neptune: return Neptune;
                case AtmosphereBase.Sun: return Sun;
                case AtmosphereBase.Pluto: return Pluto;
                case AtmosphereBase.Custom: return Default;
                default: { Debug.Log(string.Format("AtmosphereParameters: Get({0}) fail!", preset)); return new AtmosphereParameters(Default); }
            }
        }

        public static AtmosphereParameters Default
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.8f, 8.0f, 2.0f, 0.1f,
                                                                   new Vector4(0.0058f, 0.0135f, 0.0331f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   6360.0f, 6420.0f, 6421.0f,
                                                                   6360.0f, 6420.0f, 6421.0f,
                                                                   6360.0f);

                return ap;
            }
        }

        public static AtmosphereParameters Earth
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.85f, 8.0f, 1.0f, 0.1f,
                                                                   new Vector4(0.0128f, 0.0305f, 0.0731f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   6000.0f, 6056.6f, 6057.5f,
                                                                   6000.0f, 6056.6f, 6057.5f,
                                                                   0.0f);

                return ap;
            }
        }

        public static AtmosphereParameters Venus
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.6f, 12.0f, 8.0f, 0.1f,
                                                                   new Vector4(0.010f, 0.008f, 0.004f, 0.0f),
                                                                   new Vector4(0.005f, 0.004f, 0.002f, 0.0f),
                                                                   new Vector4(0.005f, 0.004f, 0.002f, 0.0f),
                                                                   6052.0f, 6132.0f, 6133.0f,
                                                                   6052.0f, 6132.0f, 6133.0f,
                                                                   6052.0f);

                return ap;
            }
        }

        public static AtmosphereParameters Mars
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.4f, 8.0f, 2.0f, 0.1f,
                                                                   new Vector4(0.0213f, 0.0168f, 0.0113f, 0.0f),
                                                                   new Vector4(0.0085f, 0.0067f, 0.0045f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   3387.792f, 3487.792f, 3488.792f,
                                                                   3387.792f, 3487.792f, 3488.792f,
                                                                   3387.792f);

                return ap;
            }
        }

        public static AtmosphereParameters Jupiter
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.8f, 12.0f, 2.0f, 0.1f,
                                                                   new Vector4(0.0117f, 0.0135f, 0.0180f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   new Vector4(0.0040f, 0.0040f, 0.0040f, 0.0f),
                                                                   7149.2f, 7209.2f, 7210.2f,
                                                                   7149.2f, 7209.2f, 7210.2f,
                                                                   7149.2f);

                return ap;
            }
        }

        public static AtmosphereParameters Titan
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.0f, 10.0f, 8.0f, 0.1f,
                                                                   new Vector4(0.0040f, 0.0040f, 0.0100f, 0.0f),
                                                                   new Vector4(0.0010f, 0.0100f, 0.0600f, 0.0f),
                                                                   new Vector4(0.0010f, 0.0100f, 0.0600f, 0.0f),
                                                                   2574.91f, 2634.91f, 2635.91f,
                                                                   2574.91f, 2634.91f, 2635.91f,
                                                                   2574.91f);

                return ap;
            }
        }

        public static AtmosphereParameters Neptune
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.6f, 8.0f, 4.5f, 0.1f,
                                                                   new Vector4(0.0058f, 0.0135f, 0.0331f, 0.0f),
                                                                   new Vector4(0.00058f, 0.0027f, 0.1f, 0.0f),
                                                                   new Vector4(0.00058f, 0.00027f, 0.005f, 0.0f),
                                                                   6371.0f, 6431.0f, 6432.0f,
                                                                   6371.0f, 6431.0f, 6432.0f,
                                                                   6371.0f);

                return ap;
            }
        }

        public static AtmosphereParameters Sun
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.6f, 10.0f, 2.0f, 0.1f,
                                                                   new Vector4(0.004f, 0.004f, 0.004f, 0.0f),
                                                                   new Vector4(0.004f, 0.004f, 0.004f, 0.0f),
                                                                   new Vector4(0.004f, 0.004f, 0.004f, 0.0f),
                                                                   3387.792f, 3487.792f, 3488.792f,
                                                                   3387.792f, 3487.792f, 3488.792f,
                                                                   3387.792f);

                return ap;
            }
        }

        public static AtmosphereParameters Pluto
        {
            get
            {
                AtmosphereParameters ap = new AtmosphereParameters(0.6f, 12.0f, 8.0f, 0.1f,
                                                                   new Vector4(0.001f, 0.001f, 0.001f),
                                                                   new Vector4(0.004f, 0.0045f, 0.006f),
                                                                   new Vector4(0.001f, 0.001f, 0.001f),
                                                                   2400.0f, 2460.0f, 2470.0f,
                                                                   2400.0f, 2460.0f, 2470.0f,
                                                                   2400.0f);
                return ap;
            }
        }
    }
}