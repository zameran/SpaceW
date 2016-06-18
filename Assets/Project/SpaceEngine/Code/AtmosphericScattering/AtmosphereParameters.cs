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
    public Vector4 BETA_MEx;

    public float Rg;
    public float Rt;
    public float Rl;

    public float bRg;
    public float bRt;
    public float bRl;

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

        this.TRANSMITTANCE_W = from.TRANSMITTANCE_W;
        this.TRANSMITTANCE_H = from.TRANSMITTANCE_H;
        this.SKY_W = from.SKY_W;
        this.SKY_H = from.SKY_H;
        this.RES_R = from.RES_R;
        this.RES_MU = from.RES_MU;
        this.RES_MU_S = from.RES_MU_S;
        this.RES_NU = from.RES_NU;
    }

    public AtmosphereParameters(float MIE_G, float HR, float HM, float AVERAGE_GROUND_REFLECTANCE,
                                Vector4 BETA_R,
                                Vector4 BETA_MSca,
                                Vector4 BETA_MEx,
                                float Rg, float Rt, float Rl,
                                float bRg, float bRt, float bRl,
                                float SCALE,
                                int TRANSMITTANCE_W, int TRANSMITTANCE_H, int SKY_W, int SKY_H, int RES_R, int RES_MU, int RES_MU_S, int RES_NU)
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

        this.TRANSMITTANCE_W = TRANSMITTANCE_W;
        this.TRANSMITTANCE_H = TRANSMITTANCE_H;
        this.SKY_W = SKY_W;
        this.SKY_H = SKY_H;
        this.RES_R = RES_R;
        this.RES_MU = RES_MU;
        this.RES_MU_S = RES_MU_S;
        this.RES_NU = RES_NU;
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
            default: { Debug.Log("Atmosphere: AtmosphereParameters.Get(...) fail!"); return Default; }
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
                                                               6360.0f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               6360.0f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               6052.0f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               3387.792f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               7149.2f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               2574.91f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               6371.0f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
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
                                                               3387.792f,
                                                               256, 64, 64, 16, 32, 128, 32, 8);

            return ap;
        }

        private set { }
    }
}