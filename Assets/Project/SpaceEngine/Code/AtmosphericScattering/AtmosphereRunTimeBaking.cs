using UnityEngine;

public sealed class AtmosphereRunTimeBaking : MonoBehaviour
{
    //Asymmetry factor for the mie phase function
    //A higher number meands more light is scattered in the forward direction
    public float MIE_G = 0.8f;

    //Half heights for the atmosphere air density (HR) and particle density (HM)
    //This is the height in km that half the particles are found below
    public float HR = 8.0f;
    public float HM = 1.2f;

    //Physical settings, Mie and Rayliegh values
    public float AVERAGE_GROUND_REFLECTANCE = 0.1f;
    public Vector4 BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
    public Vector4 BETA_MSca = new Vector4(4e-6f, 4e-6f, 4e-6f, 0.0f);

    public float Rg = 6360.0f;
    public float Rt = 6420.0f;
    public float RL = 6421.0f;

    //Dimensions of the tables
    const int TRANSMITTANCE_W = 256;
    const int TRANSMITTANCE_H = 64;
    const int SKY_W = 64;
    const int SKY_H = 16;
    const int RES_R = 32;
    const int RES_MU = 128;
    const int RES_MU_S = 32;
    const int RES_NU = 8;
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

    private void Start()
    {
        Go();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            Go();
        }
    }

    private void Go()
    {
        finished = false;
        step = 0;
        order = 2;

        CollectGarbage();
        CreateTextures();
        SetParametersForAll();
        ClearAll();

        while (!finished)
        {
            Calculate();
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

    public void CreateTextures()
    {
        transmittanceT = RTExtensions.CreateRTexture(new Vector2(TRANSMITTANCE_W, TRANSMITTANCE_H), 0, RenderTextureFormat.ARGBFloat);

        irradianceT_Read = RTExtensions.CreateRTexture(new Vector2(SKY_W, SKY_H), 0, RenderTextureFormat.ARGBFloat);
        irradianceT_Write = RTExtensions.CreateRTexture(new Vector2(SKY_W, SKY_H), 0, RenderTextureFormat.ARGBFloat);

        inscatterT_Read = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);
        inscatterT_Write = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);

        deltaET = RTExtensions.CreateRTexture(new Vector2(SKY_W, SKY_H), 0, RenderTextureFormat.ARGBFloat);

        deltaSRT = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);
        deltaSMT = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);
        deltaJT = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);
    }

    public void SetParametersForAll()
    {
        SetParameters(copyInscatter1);
        SetParameters(copyInscatterN);
        SetParameters(copyIrradiance);
        SetParameters(inscatter1);
        SetParameters(inscatterN);
        SetParameters(inscatterS);
        SetParameters(irradiance1);
        SetParameters(irradianceN);
        SetParameters(transmittance);
    }

    public void ClearAll()
    {
        RTUtility.ClearColor(transmittanceT);

        RTUtility.ClearColor(irradianceT_Read);
        RTUtility.ClearColor(irradianceT_Write);

        RTUtility.ClearColor(deltaET);
    }

    public void Calculate()
    {
        if (step == 0)
        {
            // computes transmittance texture T (line 1 in algorithm 4.1)
            transmittance.SetTexture(0, "transmittanceWrite", transmittanceT);
            transmittance.Dispatch(0, TRANSMITTANCE_W / NUM_THREADS, TRANSMITTANCE_H / NUM_THREADS, 1);
        }
        else if (step == 1)
        {
            // computes irradiance texture deltaE (line 2 in algorithm 4.1)
            irradiance1.SetTexture(0, "transmittanceRead", transmittanceT);
            irradiance1.SetTexture(0, "deltaEWrite", deltaET);
            irradiance1.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);
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
            for (int i = 0; i < RES_R; i++)
            {
                inscatter1.SetInt("layer", i);
                inscatter1.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 3)
        {
            // copies deltaE into irradiance texture E (line 4 in algorithm 4.1)
            copyIrradiance.SetFloat("k", 0.0f);
            copyIrradiance.SetTexture(0, "deltaERead", deltaET);
            copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
            copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
            copyIrradiance.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);

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
            for (int i = 0; i < RES_R; i++)
            {
                copyInscatter1.SetInt("layer", i);
                copyInscatter1.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
            }

            //Swap inscatterT_Write - inscatterT_Read
            RTUtility.Swap(ref inscatterT_Read, ref inscatterT_Write); //!!!
        }
        else if (step == 5)
        {
            // computes deltaJ (line 7 in algorithm 4.1)
            inscatterS.SetInt("first", (order == 2) ? 1 : 0);
            inscatterS.SetTexture(0, "transmittanceRead", transmittanceT);
            inscatterS.SetTexture(0, "deltaERead", deltaET);
            inscatterS.SetTexture(0, "deltaSRRead", deltaSRT);
            inscatterS.SetTexture(0, "deltaSMRead", deltaSMT);
            inscatterS.SetTexture(0, "deltaJWrite", deltaJT);

            //The inscatter calc's can be quite demanding for some cards so process 
            //the calc's in layers instead of the whole 3D data set.
            for (int i = 0; i < RES_R; i++)
            {
                inscatterS.SetInt("layer", i);
                inscatterS.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 6)
        {
            // computes deltaE (line 8 in algorithm 4.1)
            irradianceN.SetInt("first", (order == 2) ? 1 : 0);
            irradianceN.SetTexture(0, "deltaSRRead", deltaSRT);
            irradianceN.SetTexture(0, "deltaSMRead", deltaSMT);
            irradianceN.SetTexture(0, "deltaEWrite", deltaET);
            irradianceN.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);
        }
        else if (step == 7)
        {
            // computes deltaS (line 9 in algorithm 4.1)
            inscatterN.SetTexture(0, "transmittanceRead", transmittanceT);
            inscatterN.SetTexture(0, "deltaJRead", deltaJT);
            inscatterN.SetTexture(0, "deltaSRWrite", deltaSRT);

            //The inscatter calc's can be quite demanding for some cards so process 
            //the calc's in layers instead of the whole 3D data set.
            for (int i = 0; i < RES_R; i++)
            {
                inscatterN.SetInt("layer", i);
                inscatterN.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
            }
        }
        else if (step == 8)
        {
            // adds deltaE into irradiance texture E (line 10 in algorithm 4.1)
            copyIrradiance.SetFloat("k", 1.0f);
            copyIrradiance.SetTexture(0, "deltaERead", deltaET);
            copyIrradiance.SetTexture(0, "irradianceRead", irradianceT_Read);
            copyIrradiance.SetTexture(0, "irradianceWrite", irradianceT_Write);
            copyIrradiance.Dispatch(0, SKY_W / NUM_THREADS, SKY_H / NUM_THREADS, 1);

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
            for (int i = 0; i < RES_R; i++)
            {
                copyInscatterN.SetInt("layer", i);
                copyInscatterN.Dispatch(0, (RES_MU_S * RES_NU) / NUM_THREADS, RES_MU / NUM_THREADS, 1);
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

        }
        else if (step == 11)
        {
            finished = true;
        }

        step++;
    }

    public void SetParameters(ComputeShader cs)
    {
        if (cs == null) return;

        cs.SetFloat("Rg", Rg);
        cs.SetFloat("Rt", Rt);
        cs.SetFloat("RL", RL);
        cs.SetInt("TRANSMITTANCE_W", TRANSMITTANCE_W);
        cs.SetInt("TRANSMITTANCE_H", TRANSMITTANCE_H);
        cs.SetInt("SKY_W", SKY_W);
        cs.SetInt("SKY_H", SKY_H);
        cs.SetInt("RES_R", RES_R);
        cs.SetInt("RES_MU", RES_MU);
        cs.SetInt("RES_MU_S", RES_MU_S);
        cs.SetInt("RES_NU", RES_NU);
        cs.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
        cs.SetFloat("HR", HR);
        cs.SetFloat("HM", HM);
        cs.SetVector("betaR", BETA_R);
        cs.SetVector("betaMSca", BETA_MSca);
        cs.SetVector("betaMEx", BETA_MSca / 0.9f);
        cs.SetFloat("mieG", Mathf.Clamp(MIE_G, 0.0f, 0.99f));
    }

    public void SetParameters(int kernel, ComputeShader cs)
    {
        if (cs == null) return;

        cs.SetFloat("Rg", Rg);
        cs.SetFloat("Rt", Rt);
        cs.SetFloat("RL", RL);
        cs.SetInt("TRANSMITTANCE_W", TRANSMITTANCE_W);
        cs.SetInt("TRANSMITTANCE_H", TRANSMITTANCE_H);
        cs.SetInt("SKY_W", SKY_W);
        cs.SetInt("SKY_H", SKY_H);
        cs.SetInt("RES_R", RES_R);
        cs.SetInt("RES_MU", RES_MU);
        cs.SetInt("RES_MU_S", RES_MU_S);
        cs.SetInt("RES_NU", RES_NU);
        cs.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
        cs.SetFloat("HR", HR);
        cs.SetFloat("HM", HM);
        cs.SetVector("betaR", BETA_R);
        cs.SetVector("betaMSca", BETA_MSca);
        cs.SetVector("betaMEx", BETA_MSca / 0.9f);
        cs.SetFloat("mieG", Mathf.Clamp(MIE_G, 0.0f, 0.99f));
    }
}