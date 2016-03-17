using UnityEngine;

public class Atmosphere : MonoBehaviour
{
    const int TRANSMITTANCE_W = 256;
    const int TRANSMITTANCE_H = 64;
    const int SKY_W = 64;
    const int SKY_H = 16;
    const int RES_R = 32;
    const int RES_MU = 128;
    const int RES_MU_S = 32;
    const int RES_NU = 8;

    const float AVERAGE_GROUND_REFLECTANCE = 0.1f;

    const float HR = 8.0f;
    const float HM = 1.2f;

    public float AtmosphereScale = 1.0f;
    public float Rg;
    public float Rt;
    public float Rl;

    [Range(0.0f, 0.99f)]
    public float mieG = 0.85f;

    readonly Vector3 BETA_MSca = new Vector3(4e-3f, 4e-3f, 4e-3f);
    readonly Vector3 betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);

    public int AtmosphereMeshResolution = 2;

    //public float AtmosphereHeight = 1.0f;
    public float HDRExposure = 0.2f;

    public Shader SkyShader;

    public ComputeShader ReadDataCore;
    public ComputeShader WriteDataCore;

    public Material SkyMaterial;

    public Texture SunGlareTexture;

    public RenderTexture Transmittance;
    public RenderTexture Inscatter;
    public RenderTexture Irradiance;

    public Mesh AtmosphereMesh;

    public AtmosphereSun Sun;

    public Vector3 Origin;

    string texturesPath = "/Resources/Textures/Atmosphere/";
    int waitBeforeReloadCount = 0;

    private void Start()
    {
        InitMisc();
        InitMaterials();
        InitTextures();
        InitMesh();
        InitSuns();

        InitUniforms(SkyMaterial);

        SetUniforms(SkyMaterial);
    }

    public void UpdateNode()
    {
        //Rg = AtmosphereScale;
        //Rt = (64200f / 63600f) * Rg * AtmosphereHeight;
        //Rl = (64210.0f / 63600f) * Rg;

        if (!Inscatter.IsCreated() || !Transmittance.IsCreated() || !Irradiance.IsCreated())
        {
            waitBeforeReloadCount++;

            if (waitBeforeReloadCount >= 2)
            {
                Inscatter.ReleaseAndDestroy();
                Transmittance.ReleaseAndDestroy();
                Irradiance.ReleaseAndDestroy();

                InitTextures();

                waitBeforeReloadCount = 0;
            }
        }

        Sun.Origin = Origin;

        SetUniforms(SkyMaterial);

        Graphics.DrawMesh(AtmosphereMesh, transform.localToWorldMatrix, SkyMaterial, 0);
    }

    private void OnDestroy()
    {
        if (Transmittance != null)
            Transmittance.ReleaseAndDestroy();

        if (Inscatter != null)
            Inscatter.ReleaseAndDestroy();

        if (Irradiance != null)
            Irradiance.ReleaseAndDestroy();
    }

    public void InitMaterials()
    {
        if (SkyMaterial == null)
        {
            SkyMaterial = new Material(SkyShader);
            SkyMaterial.name = "Sky" + "(Instance)" + Random.Range(float.MinValue, float.MaxValue);
        }
    }

    public void InitTextures()
    {
        ComputeBuffer buffer;

        string TransmittancePath = Application.dataPath + texturesPath + "/transmittance.raw";
        string IrradiancePath = Application.dataPath + texturesPath + "/irradiance.raw";
        string InscatterPath = Application.dataPath + texturesPath + "/inscatter.raw";

        Transmittance = RTExtensions.CreateRTexture(new Vector2(TRANSMITTANCE_W, TRANSMITTANCE_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
        Irradiance = RTExtensions.CreateRTexture(new Vector2(SKY_W, SKY_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
        Inscatter = RTExtensions.CreateRTexture(new Vector2(RES_MU_S * RES_NU, RES_MU), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp, RES_R);

        buffer = new ComputeBuffer(TRANSMITTANCE_W * TRANSMITTANCE_H, sizeof(float) * 3);
        CBUtility.WriteIntoRenderTexture(Transmittance, 3, TransmittancePath, buffer, WriteDataCore);
        buffer.Release();

        buffer = new ComputeBuffer(SKY_W * SKY_H, sizeof(float) * 3);
        CBUtility.WriteIntoRenderTexture(Irradiance, 3, IrradiancePath, buffer, WriteDataCore);
        buffer.Release();

        buffer = new ComputeBuffer(RES_MU_S * RES_NU * RES_MU * RES_R, sizeof(float) * 4);
        CBUtility.WriteIntoRenderTexture(Inscatter, 4, InscatterPath, buffer, WriteDataCore);
        buffer.Release();
    }

    public void InitMesh()
    {
        AtmosphereMesh = MeshFactory.MakePlane(AtmosphereMeshResolution, AtmosphereMeshResolution, MeshFactory.PLANE.XY, false, false, false);
        AtmosphereMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
    }

    public void InitMisc()
    {
        //Rg = AtmosphereScale;
        //Rt = (64200f / 63600f) * Rg * AtmosphereHeight;
        //Rl = (64210.0f / 63600f) * Rg;
    }

    public void InitSuns()
    {
        AtmosphereSun[] suns = FindObjectsOfType<AtmosphereSun>();

        if (Sun == null) Sun = suns[0];
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("scale", Rg / AtmosphereScale);
        mat.SetFloat("Rg", Rg);
        mat.SetFloat("Rt", Rt);
        mat.SetFloat("RL", Rl);
        mat.SetFloat("TRANSMITTANCE_W", TRANSMITTANCE_W);
        mat.SetFloat("TRANSMITTANCE_H", TRANSMITTANCE_H);
        mat.SetFloat("SKY_W", SKY_W);
        mat.SetFloat("SKY_H", SKY_H);
        mat.SetFloat("RES_R", RES_R);
        mat.SetFloat("RES_MU", RES_MU);
        mat.SetFloat("RES_MU_S", RES_MU_S);
        mat.SetFloat("RES_NU", RES_NU);
        mat.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
        mat.SetFloat("HR", HR * 1000.0f);
        mat.SetFloat("HM", HM * 1000.0f);
        mat.SetVector("betaMSca", BETA_MSca / 1000.0f);
        mat.SetVector("betaMEx", (BETA_MSca / 1000.0f) / 0.9f);
        mat.SetTexture("_Sun_Glare", SunGlareTexture);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("scale", Rg / AtmosphereScale);
        mat.SetFloat("Rg", Rg);
        mat.SetFloat("Rt", Rt);
        mat.SetFloat("RL", Rl);
        mat.SetVector("betaR", betaR / 1000.0f);
        mat.SetFloat("mieG", Mathf.Clamp(mieG, 0.0f, 0.99f));
        mat.SetTexture("_Sky_Transmittance", Transmittance);
        mat.SetTexture("_Sky_Inscatter", Inscatter);
        mat.SetTexture("_Sky_Irradiance", Irradiance);
        mat.SetTexture("_Sky_Map", null);

        mat.SetMatrix("_Globals_WorldToCamera", Camera.main.GetWorldToCamera());
        mat.SetMatrix("_Globals_CameraToWorld", Camera.main.GetCameraToWorld());
        mat.SetMatrix("_Globals_CameraToScreen", Camera.main.GetCameraToScreen());
        mat.SetMatrix("_Globals_ScreenToCamera", Camera.main.GetScreenToCamera());
        mat.SetVector("_Globals_WorldCameraPos", Camera.main.transform.position);

        mat.SetVector("_Globals_Origin", -Origin);
        mat.SetFloat("_Exposure", HDRExposure);     

        if (Sun != null)
        {
            mat.SetMatrix("_Sun_WorldToLocal", Sun.GetWorldToLocalRotation());
            Sun.SetUniforms(mat);
        }
    }

    public void SetUniformsForPlanetQuad(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("scale", Rg / AtmosphereScale);
        mat.SetFloat("Rg", Rg);
        mat.SetFloat("Rt", Rt);
        mat.SetFloat("RL", Rl);
        mat.SetVector("betaR", betaR / 1000.0f);
        mat.SetFloat("mieG", Mathf.Clamp(mieG, 0.0f, 0.99f));
        mat.SetTexture("_Sky_Transmittance", Transmittance);
        mat.SetTexture("_Sky_Inscatter", Inscatter);
        mat.SetTexture("_Sky_Irradiance", Irradiance);
        mat.SetTexture("_Sky_Map", null);

        mat.SetMatrix("_Globals_WorldToCamera", Camera.main.GetWorldToCamera());
        mat.SetMatrix("_Globals_CameraToWorld", Camera.main.GetCameraToWorld());
        mat.SetMatrix("_Globals_CameraToScreen", Camera.main.GetCameraToScreen());
        mat.SetMatrix("_Globals_ScreenToCamera", Camera.main.GetScreenToCamera());
        mat.SetVector("_Globals_WorldCameraPos", Camera.main.transform.position - Origin); // Apply origin to vector on planetoid quads. 

        mat.SetVector("_Globals_Origin", -Origin);
        mat.SetFloat("_Exposure", HDRExposure);

        if (Sun != null)
        {
            mat.SetMatrix("_Sun_WorldToLocal", Sun.GetWorldToLocalRotation());
            Sun.SetUniforms(mat);
        }
    }
}