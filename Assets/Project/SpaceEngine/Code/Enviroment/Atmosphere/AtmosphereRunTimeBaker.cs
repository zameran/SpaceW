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

using System.Collections;

using UnityEngine;

namespace SpaceEngine.AtmosphericScattering
{
    public sealed class AtmosphereRunTimeBaker : MonoBehaviour
    {
        public RenderTextureFormat Format = RenderTextureFormat.ARGBFloat;

        public bool ClearAfterBake = true;
        public bool UseCoroutine = true;

        const int NUM_THREADS = 8;

        public RenderTexture transmittanceT;
        public RenderTexture irradianceT_Read, irradianceT_Write, inscatterT_Read, inscatterT_Write;
        public RenderTexture deltaET, deltaSRT, deltaSMT, deltaJT;

        public ComputeShader copyInscatter1, copyInscatterN, copyIrradiance;
        public ComputeShader inscatter1, inscatterN, inscatterS;
        public ComputeShader irradiance1, irradianceN, transmittance;
        public ComputeShader readData;

        int step, order;

        [HideInInspector]
        public bool finished = false;

        [ContextMenu("Bake default")]
        public void Bake()
        {
            if (UseCoroutine)
                StartCoroutine(DoWorkCoroutine(AtmosphereParameters.Earth));
            else
                DoWork(AtmosphereParameters.Earth);
        }

        public void Bake(AtmosphereParameters AP)
        {
            if (UseCoroutine)
                StartCoroutine(DoWorkCoroutine(AP));
            else
                DoWork(AP);
        }

        public void PreBake(AtmosphereParameters AP)
        {
            PreGo(AP);
        }

        private void PreGo(AtmosphereParameters AP)
        {
            CollectGarbage();
            CreateTextures(AP);
            SetParametersForAll(AP);
            ClearAll();
        }

        private void DoWork(AtmosphereParameters AP)
        {
            finished = false;
            step = 0;
            order = 2;

            PreGo(AP);

            while (!finished)
            {
                Calculate(AP);
            }

            if (ClearAfterBake) CollectGarbage(false, true);
        }

        private IEnumerator DoWorkCoroutine(AtmosphereParameters AP)
        {
            finished = false;
            step = 0;
            order = 2;

            PreGo(AP);

            while (!finished)
            {
                Calculate(AP);

                for (int i = 0; i < 8; i++)
                {
                    yield return Yielders.EndOfFrame;
                }
            }

            if (ClearAfterBake) CollectGarbage(false, true);
        }

        private void OnDestroy()
        {
            CollectGarbage();
        }

        public void CollectGarbage(bool all = true, bool afterBake = false)
        {
            if (all)
            {
                if (transmittanceT != null) transmittanceT.ReleaseAndDestroy();
                if (irradianceT_Read != null) irradianceT_Read.ReleaseAndDestroy();
                if (inscatterT_Read != null) inscatterT_Read.ReleaseAndDestroy();
            }

            if (irradianceT_Write != null) irradianceT_Write.ReleaseAndDestroy();
            if (inscatterT_Write != null) inscatterT_Write.ReleaseAndDestroy();

            if (all) if (deltaET != null) deltaET.ReleaseAndDestroy();

            if (deltaSRT != null) deltaSRT.ReleaseAndDestroy();
            if (deltaSMT != null) deltaSMT.ReleaseAndDestroy();
            if (deltaJT != null) deltaJT.ReleaseAndDestroy();

            if (afterBake)
            {
                irradianceT_Write = null;
                inscatterT_Write = null;

                deltaSRT = null;
                deltaSMT = null;
                deltaJT = null;
            }
        }

        public void CreateTextures(AtmosphereParameters AP)
        {
            transmittanceT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.TRANSMITTANCE_W, AtmosphereConstants.TRANSMITTANCE_H), 0, Format);

            irradianceT_Read = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.SKY_W, AtmosphereConstants.SKY_H), 0, Format);
            irradianceT_Write = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.SKY_W, AtmosphereConstants.SKY_H), 0, Format);

            inscatterT_Read = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, TextureWrapMode.Clamp, AtmosphereConstants.RES_R);
            inscatterT_Write = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, TextureWrapMode.Clamp, AtmosphereConstants.RES_R);

            deltaET = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.SKY_W, AtmosphereConstants.SKY_H), 0, Format);

            deltaSRT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, TextureWrapMode.Clamp, AtmosphereConstants.RES_R);
            deltaSMT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, TextureWrapMode.Clamp, AtmosphereConstants.RES_R);
            deltaJT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, TextureWrapMode.Clamp, AtmosphereConstants.RES_R);
        }

        public void SetParametersForAll(AtmosphereParameters AP)
        {
            SetParameters(copyInscatter1, AP);
            SetParameters(copyInscatterN, AP);
            SetParameters(copyIrradiance, AP);
            SetParameters(inscatter1, AP);
            SetParameters(inscatterN, AP);
            SetParameters(inscatterS, AP);
            SetParameters(irradiance1, AP);
            SetParameters(irradianceN, AP);
            SetParameters(transmittance, AP);
        }

        public void ClearAll()
        {
            RTUtility.ClearColor(transmittanceT);

            RTUtility.ClearColor(irradianceT_Read);
            RTUtility.ClearColor(irradianceT_Write);

            RTUtility.ClearColor(deltaET);
        }

        public void Calculate(AtmosphereParameters AP)
        {
            if (step == 0)
            {
                // computes transmittance texture T (line 1 in algorithm 4.1)
                transmittance.SetTexture(0, "transmittanceWrite", transmittanceT);
                transmittance.Dispatch(0, AtmosphereConstants.TRANSMITTANCE_W / NUM_THREADS, AtmosphereConstants.TRANSMITTANCE_H / NUM_THREADS, 1);
            }
            else if (step == 1)
            {
                // computes irradiance texture deltaE (line 2 in algorithm 4.1)
                irradiance1.SetTexture(0, "transmittanceRead", transmittanceT);
                irradiance1.SetTexture(0, "deltaEWrite", deltaET);
                irradiance1.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 2)
            {
                // computes single scattering texture deltaS (line 3 in algorithm 4.1)
                // Rayleigh and Mie separated in deltaSR + deltaSM
                inscatter1.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatter1.SetTexture(0, "deltaSRWrite", deltaSRT);
                inscatter1.SetTexture(0, "deltaSMWrite", deltaSMT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    inscatter1.SetInt("layer", i);
                    inscatter1.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 3)
            {
                // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
                copyIrradiance.SetFloat("k", 0.0f);
                copyIrradiance.SetTexture(0, "deltaERead", deltaET);
                copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
                copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
                copyIrradiance.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 4)
            {
                // copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
                copyInscatter1.SetTexture(0, "deltaSRRead", deltaSRT);
                copyInscatter1.SetTexture(0, "deltaSMRead", deltaSMT);
                copyInscatter1.SetTexture(0, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    copyInscatter1.SetInt("layer", i);
                    copyInscatter1.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }

                //Swap inscatterT_Write - inscatterT_Read
                RTUtility.Swap(ref inscatterT_Read, ref inscatterT_Write); //!!!
            }
            else if (step == 5)
            {
                //Here Nvidia GTX 430 or lower driver will crash.
                //If only ray1 or mie1 calculated - slow, but all is alright.
                //But if both - driver crash.
                //INSCATTER_SPHERICAL_INTEGRAL_SAMPLES = 8 - limit for GTX 430.

                // computes deltaJ (line 7 in algorithm 4.1)
                inscatterS.SetInt("first", (order == 2) ? 1 : 0);
                inscatterS.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatterS.SetTexture(0, "deltaERead", deltaET);
                inscatterS.SetTexture(0, "deltaSRRead", deltaSRT);
                inscatterS.SetTexture(0, "deltaSMRead", deltaSMT);
                inscatterS.SetTexture(0, "deltaJWrite", deltaJT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    inscatterS.SetInt("layer", i);
                    inscatterS.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 6)
            {
                // computes deltaE (line 8 in algorithm 4.1)
                irradianceN.SetInt("first", (order == 2) ? 1 : 0);
                irradianceN.SetTexture(0, "deltaSRRead", deltaSRT);
                irradianceN.SetTexture(0, "deltaSMRead", deltaSMT);
                irradianceN.SetTexture(0, "deltaEWrite", deltaET);
                irradianceN.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 7)
            {
                // computes deltaS (line 9 in algorithm 4.1)
                inscatterN.SetTexture(0, "transmittanceRead", transmittanceT);
                inscatterN.SetTexture(0, "deltaJRead", deltaJT);
                inscatterN.SetTexture(0, "deltaSRWrite", deltaSRT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    inscatterN.SetInt("layer", i);
                    inscatterN.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 8)
            {
                // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
                copyIrradiance.SetFloat("k", 1.0f);
                copyIrradiance.SetTexture(0, "deltaERead", deltaET);
                copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
                copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
                copyIrradiance.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 9)
            {
                // adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
                copyInscatterN.SetTexture(0, "deltaSRead", deltaSRT);
                copyInscatterN.SetTexture(0, "inscatterRead", inscatterT_Read);
                copyInscatterN.SetTexture(0, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    copyInscatterN.SetInt("layer", i);
                    copyInscatterN.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }

                //Swap inscatterT_Read - inscatterT_Write
                RTUtility.Swap(ref inscatterT_Read, ref inscatterT_Write);

                if (order < 4)
                {
                    step = 4;
                    order += 1;
                }
            }
            else if (step == 10)
            {
                //placeholder
            }
            else if (step == 11)
            {
                finished = true;
            }

            step++;
        }

        public void SetParameters(ComputeShader cs, AtmosphereParameters AP)
        {
            if (cs == null) return;

            cs.SetFloat("Rg", AP.bRg);
            cs.SetFloat("Rt", AP.bRt);
            cs.SetFloat("RL", AP.bRl);
            cs.SetInt("TRANSMITTANCE_W", AtmosphereConstants.TRANSMITTANCE_W);
            cs.SetInt("TRANSMITTANCE_H", AtmosphereConstants.TRANSMITTANCE_H);
            cs.SetInt("SKY_W", AtmosphereConstants.SKY_W);
            cs.SetInt("SKY_H", AtmosphereConstants.SKY_H);
            cs.SetInt("RES_R", AtmosphereConstants.RES_R);
            cs.SetInt("RES_MU", AtmosphereConstants.RES_MU);
            cs.SetInt("RES_MU_S", AtmosphereConstants.RES_MU_S);
            cs.SetInt("RES_NU", AtmosphereConstants.RES_NU);
            cs.SetFloat("AVERAGE_GROUND_REFLECTANCE", AP.AVERAGE_GROUND_REFLECTANCE);
            cs.SetFloat("HR", AP.HR);
            cs.SetFloat("HM", AP.HM);

            cs.SetInt("TRANSMITTANCE_INTEGRAL_SAMPLES", AtmosphereConstants.TRANSMITTANCE_INTEGRAL_SAMPLES);
            cs.SetInt("INSCATTER_INTEGRAL_SAMPLES", AtmosphereConstants.INSCATTER_INTEGRAL_SAMPLES);
            cs.SetInt("IRRADIANCE_INTEGRAL_SAMPLES", AtmosphereConstants.IRRADIANCE_INTEGRAL_SAMPLES);
            cs.SetInt("IRRADIANCE_INTEGRAL_SAMPLES_HALF", AtmosphereConstants.IRRADIANCE_INTEGRAL_SAMPLES_HALF);
            cs.SetInt("INSCATTER_SPHERICAL_INTEGRAL_SAMPLES", AtmosphereConstants.INSCATTER_SPHERICAL_INTEGRAL_SAMPLES);

            cs.SetVector("betaR", AP.BETA_R);
            cs.SetVector("betaMSca", AP.BETA_MSca);
            cs.SetVector("betaMEx", AP.BETA_MEx);
            cs.SetFloat("mieG", Mathf.Clamp(AP.MIE_G, 0.0f, 0.99f));
        }

        public void SetParameters(int kernel, ComputeShader cs, AtmosphereParameters AP)
        {
            if (cs == null) return;

            cs.SetFloat("Rg", AP.bRg);
            cs.SetFloat("Rt", AP.bRt);
            cs.SetFloat("RL", AP.bRl);
            cs.SetInt("TRANSMITTANCE_W", AtmosphereConstants.TRANSMITTANCE_W);
            cs.SetInt("TRANSMITTANCE_H", AtmosphereConstants.TRANSMITTANCE_H);
            cs.SetInt("SKY_W", AtmosphereConstants.SKY_W);
            cs.SetInt("SKY_H", AtmosphereConstants.SKY_H);
            cs.SetInt("RES_R", AtmosphereConstants.RES_R);
            cs.SetInt("RES_MU", AtmosphereConstants.RES_MU);
            cs.SetInt("RES_MU_S", AtmosphereConstants.RES_MU_S);
            cs.SetInt("RES_NU", AtmosphereConstants.RES_NU);
            cs.SetFloat("AVERAGE_GROUND_REFLECTANCE", AP.AVERAGE_GROUND_REFLECTANCE);
            cs.SetFloat("HR", AP.HR);
            cs.SetFloat("HM", AP.HM);

            cs.SetInt("TRANSMITTANCE_INTEGRAL_SAMPLES", AtmosphereConstants.TRANSMITTANCE_INTEGRAL_SAMPLES);
            cs.SetInt("INSCATTER_INTEGRAL_SAMPLES", AtmosphereConstants.INSCATTER_INTEGRAL_SAMPLES);
            cs.SetInt("IRRADIANCE_INTEGRAL_SAMPLES", AtmosphereConstants.IRRADIANCE_INTEGRAL_SAMPLES);
            cs.SetInt("IRRADIANCE_INTEGRAL_SAMPLES_HALF", AtmosphereConstants.IRRADIANCE_INTEGRAL_SAMPLES_HALF);
            cs.SetInt("INSCATTER_SPHERICAL_INTEGRAL_SAMPLES", AtmosphereConstants.INSCATTER_SPHERICAL_INTEGRAL_SAMPLES);

            cs.SetVector("betaR", AP.BETA_R);
            cs.SetVector("betaMSca", AP.BETA_MSca);
            cs.SetVector("betaMEx", AP.BETA_MEx);
            cs.SetFloat("mieG", Mathf.Clamp(AP.MIE_G, 0.0f, 0.99f));
        }
    }
}