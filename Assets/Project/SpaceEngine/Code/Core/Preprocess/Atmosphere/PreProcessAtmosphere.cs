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


using SpaceEngine.Enviroment.Atmospheric;

using System;
using System.Collections;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Atmospehre
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

        public ComputeShader CopyInscatter1 { get { return GodManager.Instance.CopyInscatter1; } }
        public ComputeShader CopyInscatterN { get { return GodManager.Instance.CopyInscatterN; } }
        public ComputeShader CopyIrradiance { get { return GodManager.Instance.CopyIrradiance; } }
        public ComputeShader Inscatter1 { get { return GodManager.Instance.Inscatter1; } }
        public ComputeShader InscatterN { get { return GodManager.Instance.InscatterN; } }
        public ComputeShader InscatterS { get { return GodManager.Instance.InscatterS; } }
        public ComputeShader Irradiance1 { get { return GodManager.Instance.Irradiance1; } }
        public ComputeShader IrradianceN { get { return GodManager.Instance.IrradianceN; } }
        public ComputeShader Transmittance { get { return GodManager.Instance.Transmittance; } }

        int step, order;

        [HideInInspector]
        public bool finished = false;

        [SerializeField]
        string DestinationFolder = "/Resources/Preprocess/Textures/Atmosphere";

        private void Start()
        {
            if (BakeMode == AtmosphereBakeMode.TO_HDD || BakeMode == AtmosphereBakeMode.TO_HDD_DEBUG)
            {
                Bake(AtmosphereParameters.Earth);
            }
        }

        public void Bake(AtmosphereParameters AP)
        {
            if (UseCoroutine)
                StartCoroutine(DoWorkCoroutine(AP));
            else
                DoWork(AP);
        }

        private void Prepeare(AtmosphereParameters AP)
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

            Prepeare(AP);

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

            Prepeare(AP);

            while (!finished)
            {
                Calculate(AP);

                for (byte i = 0; i < WAIT_FRAMES; i++)
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

            //if (deltaET != null) deltaET.ReleaseAndDestroy();
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
            SetParameters(CopyInscatter1, AP);
            SetParameters(CopyInscatterN, AP);
            SetParameters(CopyIrradiance, AP);
            SetParameters(Inscatter1, AP);
            SetParameters(InscatterN, AP);
            SetParameters(InscatterS, AP);
            SetParameters(Irradiance1, AP);
            SetParameters(IrradianceN, AP);
            SetParameters(Transmittance, AP);
        }

        public void ClearAll()
        {
            RTUtility.ClearColor(transmittanceT);

            RTUtility.ClearColor(irradianceT_Read);
            RTUtility.ClearColor(irradianceT_Write);
        }

        public void Calculate(AtmosphereParameters AP)
        {
            if (step == 0)
            {
                // computes transmittance texture T (line 1 in algorithm 4.1)
                Transmittance.SetTexture(0, "transmittanceWrite", transmittanceT);
                Transmittance.Dispatch(0, AtmosphereConstants.TRANSMITTANCE_W / NUM_THREADS, AtmosphereConstants.TRANSMITTANCE_H / NUM_THREADS, 1);
            }
            else if (step == 1)
            {
                // computes irradiance texture deltaE (line 2 in algorithm 4.1)
                Irradiance1.SetTexture(0, "transmittanceRead", transmittanceT);
                Irradiance1.SetTexture(0, "deltaEWrite", deltaET);
                Irradiance1.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 2)
            {
                // computes single scattering texture deltaS (line 3 in algorithm 4.1)
                // Rayleigh and Mie separated in deltaSR + deltaSM
                Inscatter1.SetTexture(0, "transmittanceRead", transmittanceT);
                Inscatter1.SetTexture(0, "deltaSRWrite", deltaSRT);
                Inscatter1.SetTexture(0, "deltaSMWrite", deltaSMT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    Inscatter1.SetInt("layer", i);
                    Inscatter1.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 3)
            {
                // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
                CopyIrradiance.SetFloat("k", 0.0f);
                CopyIrradiance.SetTexture(0, "deltaERead", deltaET);
                CopyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
                CopyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
                CopyIrradiance.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 4)
            {
                // copies deltaS into inscatter texture S (line 5 in algorithm 4.1)
                CopyInscatter1.SetTexture(0, "deltaSRRead", deltaSRT);
                CopyInscatter1.SetTexture(0, "deltaSMRead", deltaSMT);
                CopyInscatter1.SetTexture(0, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    CopyInscatter1.SetInt("layer", i);
                    CopyInscatter1.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
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
                InscatterS.SetInt("first", (order == 2) ? 1 : 0);
                InscatterS.SetTexture(0, "transmittanceRead", transmittanceT);
                InscatterS.SetTexture(0, "deltaERead", deltaET);
                InscatterS.SetTexture(0, "deltaSRRead", deltaSRT);
                InscatterS.SetTexture(0, "deltaSMRead", deltaSMT);
                InscatterS.SetTexture(0, "deltaJWrite", deltaJT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    InscatterS.SetInt("layer", i);
                    InscatterS.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 6)
            {
                // computes deltaE (line 8 in algorithm 4.1)
                IrradianceN.SetInt("first", (order == 2) ? 1 : 0);
                IrradianceN.SetTexture(0, "deltaSRRead", deltaSRT);
                IrradianceN.SetTexture(0, "deltaSMRead", deltaSMT);
                IrradianceN.SetTexture(0, "deltaEWrite", deltaET);
                IrradianceN.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);
            }
            else if (step == 7)
            {
                // computes deltaS (line 9 in algorithm 4.1)
                InscatterN.SetTexture(0, "transmittanceRead", transmittanceT);
                InscatterN.SetTexture(0, "deltaJRead", deltaJT);
                InscatterN.SetTexture(0, "deltaSRWrite", deltaSRT);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    InscatterN.SetInt("layer", i);
                    InscatterN.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
                }
            }
            else if (step == 8)
            {
                // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
                CopyIrradiance.SetFloat("k", 1.0f);
                CopyIrradiance.SetTexture(0, "deltaERead", deltaET);
                CopyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
                CopyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
                CopyIrradiance.Dispatch(0, AtmosphereConstants.SKY_W / NUM_THREADS, AtmosphereConstants.SKY_H / NUM_THREADS, 1);

                //Swap irradianceT_Read - irradianceT_Write
                RTUtility.Swap(ref irradianceT_Read, ref irradianceT_Write);
            }
            else if (step == 9)
            {
                // adds deltaS into inscatter texture S (line 11 in algorithm 4.1)
                CopyInscatterN.SetTexture(0, "deltaSRead", deltaSRT);
                CopyInscatterN.SetTexture(0, "inscatterRead", inscatterT_Read);
                CopyInscatterN.SetTexture(0, "inscatterWrite", inscatterT_Write);

                //The inscatter calc's can be quite demanding for some cards so process 
                //the calc's in layers instead of the whole 3D data set.
                for (int i = 0; i < AtmosphereConstants.RES_R; i++)
                {
                    CopyInscatterN.SetInt("layer", i);
                    CopyInscatterN.Dispatch(0, (AtmosphereConstants.RES_MU_S * AtmosphereConstants.RES_NU) / NUM_THREADS, AtmosphereConstants.RES_MU / NUM_THREADS, 1);
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