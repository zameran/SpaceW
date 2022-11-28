#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

using SpaceEngine.Environment.Atmospheric;
using SpaceEngine.Tools;
using SpaceEngine.Utilities;

using System;
using System.Collections;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Atmosphere
{
    public sealed class PreProcessAtmosphere : MonoBehaviour
    {
        /// <summary>
        /// Behaviour of atmosphere baker.
        /// <see cref="NONE"/> is none!
        /// <see cref="TO_RAM"/> is runtime baking in to the RAM.
        /// <see cref="TO_HDD"/> is on call baking in to the HDD.
        /// <see cref="TO_HDD_DEBUG"/> is the same as <see cref="TO_HDD"/>, but additional debug textures will be saved.
        /// </summary>
        [Serializable]
        public enum AtmosphereBakeMode
        {
            NONE,
            TO_RAM,
            TO_HDD,
            TO_HDD_DEBUG
        }

        public AtmosphereBakeMode BakeMode = AtmosphereBakeMode.TO_RAM;
        public RenderTextureFormat Format = RenderTextureFormat.ARGBFloat;
        public TextureWrapMode WrapMode = TextureWrapMode.Clamp;

        public bool ClearAfterBake = true;
        public bool UseCoroutine = true;

        private const int NUM_THREADS = 8;
        private const int WAIT_FRAMES = 4;

        public RenderTexture transmittanceT;
        public RenderTexture irradianceT_Read, irradianceT_Write, inscatterT_Read, inscatterT_Write;
        public RenderTexture deltaET, deltaSRT, deltaSMT, deltaJT;

        public ComputeShader Precompute => GodManager.Instance.Precompute;

        int step, order;

        [HideInInspector]
        public bool finished = false;

        [SerializeField]
        string DestinationFolder = "/Resources/Preprocess/Textures/Atmosphere";

        private void Awake()
        {
            if (BakeMode == AtmosphereBakeMode.TO_HDD || BakeMode == AtmosphereBakeMode.TO_HDD_DEBUG)
            {
                Bake(AtmosphereParameters.Earth, null);
            }
        }

        public void Bake(AtmosphereParameters AP, Action callback)
        {
            if (UseCoroutine)
                StartCoroutine(DoWorkCoroutine(AP, callback));
            else
                DoWork(AP, callback);
        }

        private void Prepare(AtmosphereParameters AP)
        {
            CollectGarbage();
            CreateTextures(AP);
            SetParametersForAll(AP);
            ClearAll();
        }

        private void DoWork(AtmosphereParameters AP, Action callback)
        {
            finished = false;
            step = 0;
            order = 2;

            Prepare(AP);

            while (!finished)
            {
                Calculate(AP);
            }

            if (ClearAfterBake) CollectGarbage(false, true);

            callback?.Invoke();
        }

        private IEnumerator DoWorkCoroutine(AtmosphereParameters AP, Action callback)
        {
            finished = false;
            step = 0;
            order = 2;

            Prepare(AP);

            while (!finished)
            {
                Calculate(AP);

                for (byte i = 0; i < WAIT_FRAMES; i++)
                {
                    yield return Yielders.EndOfFrame;
                }
            }

            if (ClearAfterBake) CollectGarbage(false, true);

            callback?.Invoke();
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

            if (deltaET != null) deltaET.ReleaseAndDestroy();
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

            inscatterT_Read = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, WrapMode, AtmosphereConstants.RES_R);
            inscatterT_Write = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, WrapMode, AtmosphereConstants.RES_R);

            deltaET = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.SKY_W, AtmosphereConstants.SKY_H), 0, Format);

            deltaSRT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, WrapMode, AtmosphereConstants.RES_R);
            deltaSMT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, WrapMode, AtmosphereConstants.RES_R);
            deltaJT = RTExtensions.CreateRTexture(new Vector2(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU), 0, Format, FilterMode.Bilinear, WrapMode, AtmosphereConstants.RES_R);
        }

        public void SetParametersForAll(AtmosphereParameters AP)
        {
            SetParameters(Precompute, AP);
        }

        public void ClearAll()
        {
            RTUtility.ClearColor(transmittanceT);

            RTUtility.ClearColor(irradianceT_Read);
            RTUtility.ClearColor(irradianceT_Write);
        }

        public void Calculate(AtmosphereParameters AP)
        {
            var copyInscatter1Kernel = Precompute.FindKernel("CopyInscatter1");
            var copyInscatterNKernel = Precompute.FindKernel("CopyInscatterN");
            var copyIrradianceKernel = Precompute.FindKernel("CopyIrradiance");
            var inscatter1Kernel = Precompute.FindKernel("Inscatter1");
            var inscatterNKernel = Precompute.FindKernel("InscatterN");
            var inscatterSKernel = Precompute.FindKernel("InscatterS");
            var irradiance1Kernel = Precompute.FindKernel("Irradiance1");
            var irradianceNKernel = Precompute.FindKernel("IrradianceN");
            var transmittanceKernel = Precompute.FindKernel("Transmittance");

            if (step == 0)
            {
                // computes transmittance texture T (line 1 in algorithm 4.1)
                Precompute.SetTexture(transmittanceKernel, "transmittanceWrite", transmittanceT);
                Precompute.Dispatch(transmittanceKernel, AtmosphereConstants.TRANSMITTANCE_W / NUM_THREADS, AtmosphereConstants.TRANSMITTANCE_H / NUM_THREADS, 1);
            }
            else if (step == 1)
            {
                // computes irradiance texture deltaE (line 2 in algorithm 4.1)
                Precompute.SetTexture(irradiance1Kernel, "transmittanceRead", transmittanceT);
                Precompute.SetTexture(irradiance1Kernel, "deltaEWrite", deltaET);
                Precompute.Dispatch(irradiance1Kernel, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 2)
            {
                // computes single scattering texture deltaS (line 3 in algorithm 4.1)
                // Rayleigh and Mie separated in deltaSR + deltaSM
                Precompute.SetTexture(inscatter1Kernel, "transmittanceRead", transmittanceT);
                Precompute.SetTexture(inscatter1Kernel, "deltaSRWrite", deltaSRT);
                Precompute.SetTexture(inscatter1Kernel, "deltaSMWrite", deltaSMT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (var i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Precompute.SetInt("layer", i);
                    Precompute.Dispatch(inscatter1Kernel, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 3)
            {
                // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
                Precompute.SetFloat("k", 0.0f);
                Precompute.SetTexture(copyIrradianceKernel, "deltaERead", deltaET);
                Precompute.SetTexture(copyIrradianceKernel, "irradianceRead", irradianceT_Read);
                Precompute.SetTexture(copyIrradianceKernel, "irradianceWrite", irradianceT_Write);
                Precompute.Dispatch(copyIrradianceKernel, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 4)
            {
                // copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
                Precompute.SetTexture(copyInscatter1Kernel, "deltaSRRead", deltaSRT);
                Precompute.SetTexture(copyInscatter1Kernel, "deltaSMRead", deltaSMT);
                Precompute.SetTexture(copyInscatter1Kernel, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (var i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Precompute.SetInt("layer", i);
                    Precompute.Dispatch(copyInscatter1Kernel, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
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
                Precompute.SetInt("first", (order == 2) ? 1 : 0);
                Precompute.SetTexture(inscatterSKernel, "transmittanceRead", transmittanceT);
                Precompute.SetTexture(inscatterSKernel, "deltaERead", deltaET);
                Precompute.SetTexture(inscatterSKernel, "deltaSRRead", deltaSRT);
                Precompute.SetTexture(inscatterSKernel, "deltaSMRead", deltaSMT);
                Precompute.SetTexture(inscatterSKernel, "deltaJWrite", deltaJT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (var i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Precompute.SetInt("layer", i);
                    Precompute.Dispatch(inscatterSKernel, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 6)
            {
                // computes deltaE (line 8 in algorithm 4.1)
                Precompute.SetInt("first", (order == 2) ? 1 : 0);
                Precompute.SetTexture(irradianceNKernel, "deltaSRRead", deltaSRT);
                Precompute.SetTexture(irradianceNKernel, "deltaSMRead", deltaSMT);
                Precompute.SetTexture(irradianceNKernel, "deltaEWrite", deltaET);
                Precompute.Dispatch(irradianceNKernel, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 7)
            {
                // computes deltaS (line 9 in algorithm 4.1)
                Precompute.SetTexture(inscatterNKernel, "transmittanceRead", transmittanceT);
                Precompute.SetTexture(inscatterNKernel, "deltaJRead", deltaJT);
                Precompute.SetTexture(inscatterNKernel, "deltaSRWrite", deltaSRT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (var i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Precompute.SetInt("layer", i);
                    Precompute.Dispatch(inscatterNKernel, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 8)
            {
                // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
                Precompute.SetFloat("k", 1.0f);
                Precompute.SetTexture(copyIrradianceKernel, "deltaERead", deltaET);
                Precompute.SetTexture(copyIrradianceKernel, "irradianceRead", irradianceT_Read);
                Precompute.SetTexture(copyIrradianceKernel, "irradianceWrite", irradianceT_Write);
                Precompute.Dispatch(copyIrradianceKernel, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 9)
            {
                // adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
                Precompute.SetTexture(copyInscatterNKernel, "deltaSRead", deltaSRT);
                Precompute.SetTexture(copyInscatterNKernel, "inscatterRead", inscatterT_Read);
                Precompute.SetTexture(copyInscatterNKernel, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (var i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Precompute.SetInt("layer", i);
                    Precompute.Dispatch(copyInscatterNKernel, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
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
                if (BakeMode == AtmosphereBakeMode.TO_HDD || BakeMode == AtmosphereBakeMode.TO_HDD_DEBUG)
                {
                    var readDataShader = GodManager.Instance.ReadData;

                    RTUtility.SaveAsRaw(AtmosphereConstants.TRANSMITTANCE_W * AtmosphereConstants.TRANSMITTANCE_H, CBUtility.Channels.RGB, "/transmittance", DestinationFolder, transmittanceT, readDataShader);
                    RTUtility.SaveAsRaw(AtmosphereConstants.SKY_W * AtmosphereConstants.SKY_H, CBUtility.Channels.RGB, "/irradiance", DestinationFolder, irradianceT_Read, readDataShader);
                    RTUtility.SaveAsRaw((AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) * AtmosphereConstants.RES_MU * AtmosphereConstants.RES_R, CBUtility.Channels.RGB, "/inscatter", DestinationFolder, inscatterT_Read, readDataShader);

                    if (BakeMode == AtmosphereBakeMode.TO_HDD_DEBUG)
                    {
                        RTUtility.SaveAs8bit(AtmosphereConstants.TRANSMITTANCE_W, AtmosphereConstants.TRANSMITTANCE_H, CBUtility.Channels.RGBA, "/transmittance_debug", DestinationFolder, transmittanceT, readDataShader);
                        RTUtility.SaveAs8bit(AtmosphereConstants.SKY_W, AtmosphereConstants.SKY_H, CBUtility.Channels.RGBA, "/irradiance_debug", DestinationFolder, irradianceT_Read, readDataShader, 10.0f);
                        RTUtility.SaveAs8bit(AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU, AtmosphereConstants.RES_MU * AtmosphereConstants.RES_R, CBUtility.Channels.RGBA, "/inscater_debug", DestinationFolder, inscatterT_Read, readDataShader);
                    }
                }
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
    }
}