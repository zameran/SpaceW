using System;

using UnityEngine;

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

    public float Rg;
    public float Rt;
    public float Rl;

    public float SCALE;

    //Dimensions of the tables
    public int TRANSMITTANCE_W;
    public int TRANSMITTANCE_H;
    public int SKY_W;
    public int SKY_H;
    public int RES_R;
    public int RES_MU;
    public int RES_MU_S;
    public int RES_NU;

    public AtmosphereParameters(float Rg, float Rt, float Rl)
    {
        MIE_G = 0.8f;

        HR = 8.0f;
        HM = 1.2f;

        AVERAGE_GROUND_REFLECTANCE = 0.1f;
        BETA_R = new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f);
        BETA_MSca = new Vector4(4e-6f, 4e-6f, 4e-6f, 0.0f);

        this.Rg = Rg;
        this.Rt = Rt;
        this.Rl = Rl;

        SCALE = Rg;

        TRANSMITTANCE_W = 256;
        TRANSMITTANCE_H = 64;
        SKY_W = 64;
        SKY_H = 16;
        RES_R = 32;
        RES_MU = 128;
        RES_MU_S = 32;
        RES_NU = 8;
    }

    public AtmosphereParameters(float MIE_G, float HR, float HM, float AVERAGE_GROUND_REFLECTANCE,
                                Vector4 BETA_R, 
                                Vector4 BETA_MSca,
                                float Rg, float Rt, float Rl, float SCALE,
                                int TRANSMITTANCE_W, int TRANSMITTANCE_H, int SKY_W, int SKY_H, int RES_R, int RES_MU, int RES_MU_S, int RES_NU)
    {
        this.MIE_G = MIE_G;

        this.HR = HR;
        this.HM = HM;

        this.AVERAGE_GROUND_REFLECTANCE = AVERAGE_GROUND_REFLECTANCE;
        this.BETA_R = BETA_R;
        this.BETA_MSca = BETA_MSca;

        this.Rg = Rg;
        this.Rt = Rt;
        this.Rl = Rl;

        this.SCALE = SCALE;

        this.TRANSMITTANCE_W = TRANSMITTANCE_W;
        this.TRANSMITTANCE_H = TRANSMITTANCE_H;
        this.SKY_W = SKY_W;
        this.SKY_H = SKY_H;
        this.RES_R = RES_R;
        this.RES_MU = RES_MU;
        this.RES_MU_S = RES_MU_S;
        this.RES_NU = RES_NU;
    }

    public static AtmosphereParameters Default
    {
        get
        {
            AtmosphereParameters ap = new AtmosphereParameters(0.8f, 8.0f, 1.2f, 0.1f,
                                                               new Vector4(5.8e-3f, 1.35e-2f, 3.31e-2f, 0.0f),
                                                               new Vector4(4e-6f, 4e-6f, 4e-6f, 0.0f),
                                                               6360.0f, 6420.0f, 6421.0f, 6360.0f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
    }
}