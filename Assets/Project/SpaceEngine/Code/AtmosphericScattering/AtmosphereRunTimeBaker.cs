using System;
using System.Collections;

using UnityEngine;

public sealed class AtmosphereRunTimeBaker : MonoBehaviour
{
    const int NUM_THREADS = 8;

    public RenderTexture transmittanceT;
    public RenderTexture irradianceT_Read, irradianceT_Write, inscatterT_Read, inscatterT_Write;
    public RenderTexture deltaET, deltaSRT, deltaSMT, deltaJT;

    public ComputeShader copyInscatter1, copyInscatterN, copyIrradiance;
    public ComputeShader inscatter1, inscatterN, inscatterS;
    public ComputeShader irradiance1, irradianceN, transmittance;
    public ComputeShader readData;

    int step, order;
    bool finished = false;

    [ContextMenu("Bake default")]
    public void Bake()
    {
        Go(AtmosphereParameters.Default);
    }

    public void Bake(AtmosphereParameters AP)
    {
        Go(AP);
    }

    private void Go(AtmosphereParameters AP)
    {
        finished = false;
        step = 0;
        order = 2;

        CollectGarbage();
        CreateTextures(AP);
        SetParametersForAll(AP);
        ClearAll();

        while (!finished)
        {
            Calculate(AP);
        }
    }

    private void OnDestroy()
    {
        CollectGarbage();
    }

    public void CollectGarbage(bool all = true)
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
    }

    public void CreateTextures(AtmosphereParameters AP)
    {
        transmittanceT = RTExtensions.CreateRTexture(new Vector2(AP.TRANSMITTANCE_W, AP.TRANSMITTANCE_H), 0, RenderTextureFormat.ARGBFloat);

        irradianceT_Read = RTExtensions.CreateRTexture(new Vector2(AP.SKY_W, AP.SKY_H), 0, RenderTextureFormat.ARGBFloat);
        irradianceT_Write = RTExtensions.CreateRTexture(new Vector2(AP.SKY_W, AP.SKY_H), 0, RenderTextureFormat.ARGBFloat);

        inscatterT_Read = RTExtensions.CreateRTexture(new Vector2(AP.RES_MU_S * AP.RES_NU, AP.RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, AP.RES_R);
        inscatterT_Write = RTExtensions.CreateRTexture(new Vector2(AP.RES_MU_S * AP.RES_NU, AP.RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, AP.RES_R);

        deltaET = RTExtensions.CreateRTexture(new Vector2(AP.SKY_W, AP.SKY_H), 0, RenderTextureFormat.ARGBFloat);

        deltaSRT = RTExtensions.CreateRTexture(new Vector2(AP.RES_MU_S * AP.RES_NU, AP.RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, AP.RES_R);
        deltaSMT = RTExtensions.CreateRTexture(new Vector2(AP.RES_MU_S * AP.RES_NU, AP.RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, AP.RES_R);
        deltaJT = RTExtensions.CreateRTexture(new Vector2(AP.RES_MU_S * AP.RES_NU, AP.RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, AP.RES_R);
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
            transmittance.Dispatch(0, AP.TRANSMITTANCE_W / NUM_THREADS, AP.TRANSMITTANCE_H / NUM_THREADS, 1);
        }
        else if (step == 1)
        {
            // computes irradiance texture deltaE (line 2 in algorithm 4.1)
            irradiance1.SetTexture(0, "transmittanceRead", transmittanceT);
            irradiance1.SetTexture(0, "deltaEWrite", deltaET);
            irradiance1.Dispatch(0, AP.SKY_W / NUM_THREADS, AP.SKY_H / NUM_THREADS, 1);
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
            for (int i = 0; i < AP.RES_R; i++)
            {
                inscatter1.SetInt("layer", i);
                inscatter1.Dispatch(0, (AP.RES_MU_S * AP.RES_NU) / NUM_THREADS, AP.RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 3)
        {
            // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
            copyIrradiance.SetFloat("k", 0.0f);
            copyIrradiance.SetTexture(0, "deltaERead", deltaET);
            copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
            copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
            copyIrradiance.Dispatch(0, AP.SKY_W / NUM_THREADS, AP.SKY_H / NUM_THREADS, 1);

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
            for (int i = 0; i < AP.RES_R; i++)
            {
                copyInscatter1.SetInt("layer", i);
                copyInscatter1.Dispatch(0, (AP.RES_MU_S * AP.RES_NU) / NUM_THREADS, AP.RES_MU / NUM_THREADS, 1);
            }

            //Swap inscatterT_Write - inscatterT_Read
            RTUtility.Swap(ref inscatterT_Read, ref inscatterT_Write); //!!!
        }
        else if (step == 5)
        {
            //Here Nvidia GTX 430 driver will crash.
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
            for (int i = 0; i < AP.RES_R; i++)
            {
                inscatterS.SetInt("layer", i);
                inscatterS.Dispatch(0, (AP.RES_MU_S * AP.RES_NU) / NUM_THREADS, AP.RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 6) 
        {
            // computes deltaE (line 8 in algorithm 4.1)
            irradianceN.SetInt("first", (order == 2) ? 1 : 0);
            irradianceN.SetTexture(0, "deltaSRRead", deltaSRT);
            irradianceN.SetTexture(0, "deltaSMRead", deltaSMT);
            irradianceN.SetTexture(0, "deltaEWrite", deltaET);
            irradianceN.Dispatch(0, AP.SKY_W / NUM_THREADS, AP.SKY_H / NUM_THREADS, 1);
        }
        else if (step == 7)
        {
            // computes deltaS (line 9 in algorithm 4.1)
            inscatterN.SetTexture(0, "transmittanceRead", transmittanceT);
            inscatterN.SetTexture(0, "deltaJRead", deltaJT);
            inscatterN.SetTexture(0, "deltaSRWrite", deltaSRT);

            //The inscatter calc's can be quite demanding for some cards so process 
            //the calc's in layers instead of the whole 3D data set.
            for (int i = 0; i < AP.RES_R; i++)
            {
                inscatterN.SetInt("layer", i);
                inscatterN.Dispatch(0, (AP.RES_MU_S * AP.RES_NU) / NUM_THREADS, AP.RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 8)
        {
            // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
            copyIrradiance.SetFloat("k", 1.0f);
            copyIrradiance.SetTexture(0, "deltaERead", deltaET);
            copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
            copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
            copyIrradiance.Dispatch(0, AP.SKY_W / NUM_THREADS, AP.SKY_H / NUM_THREADS, 1);

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
            for (int i = 0; i < AP.RES_R; i++)
            {
                copyInscatterN.SetInt("layer", i);
                copyInscatterN.Dispatch(0, (AP.RES_MU_S * AP.RES_NU) / NUM_THREADS, AP.RES_MU / NUM_THREADS, 1);
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

        cs.SetFloat("Rg", AP.Rg);
        cs.SetFloat("Rt", AP.Rt);
        cs.SetFloat("RL", AP.Rl);
        cs.SetInt("TRANSMITTANCE_W", AP.TRANSMITTANCE_W);
        cs.SetInt("TRANSMITTANCE_H", AP.TRANSMITTANCE_H);
        cs.SetInt("SKY_W", AP.SKY_W);
        cs.SetInt("SKY_H", AP.SKY_H);
        cs.SetInt("RES_R", AP.RES_R);
        cs.SetInt("RES_MU", AP.RES_MU);
        cs.SetInt("RES_MU_S", AP.RES_MU_S);
        cs.SetInt("RES_NU", AP.RES_NU);
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
        cs.SetVector("betaMEx", AP.BETA_MSca / 0.9f);
        cs.SetFloat("mieG", Mathf.Clamp(AP.MIE_G, 0.0f, 0.99f));
    }

    public void SetParameters(int kernel, ComputeShader cs, AtmosphereParameters AP)
    {
        if (cs == null) return;

        cs.SetFloat("Rg", AP.Rg);
        cs.SetFloat("Rt", AP.Rt);
        cs.SetFloat("RL", AP.Rl);
        cs.SetInt("TRANSMITTANCE_W", AP.TRANSMITTANCE_W);
        cs.SetInt("TRANSMITTANCE_H", AP.TRANSMITTANCE_H);
        cs.SetInt("SKY_W", AP.SKY_W);
        cs.SetInt("SKY_H", AP.SKY_H);
        cs.SetInt("RES_R", AP.RES_R);
        cs.SetInt("RES_MU", AP.RES_MU);
        cs.SetInt("RES_MU_S", AP.RES_MU_S);
        cs.SetInt("RES_NU", AP.RES_NU);
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
        cs.SetVector("betaMEx", AP.BETA_MSca / 0.9f);
        cs.SetFloat("mieG", Mathf.Clamp(AP.MIE_G, 0.0f, 0.99f));
    }
}