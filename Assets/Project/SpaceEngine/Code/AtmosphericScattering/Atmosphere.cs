using UnityEngine;

public sealed class Atmosphere : MonoBehaviour
{
    readonly Vector3 BETA_MSca = new Vector3(4e-3f, 4e-3f, 4e-3f);
    readonly Vector3 betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);

    public int AtmosphereMeshResolution = 2;

    public float HDRExposure = 0.2f;

    public Shader SkyShader;

    public ComputeShader ReadDataCore;
    public ComputeShader WriteDataCore;

    public Material SkyMaterial;

    public Texture SunGlareTexture;
    public float SunGlareScale = 1;

    public int RenderQueue = 1999;

    public RenderTexture Transmittance;
    public RenderTexture Inscatter;
    public RenderTexture Irradiance;

    public Mesh AtmosphereMesh;

    public bool RunTimeBaking = false;
    public bool LostFocusForceRebake = false;

    public AtmosphereSun Sun;
    public AtmosphereParameters atmosphereParameters = AtmosphereParameters.Default;
    public AtmosphereRunTimeBaker artb = null;

    public Vector3 Origin;
    
    private void Start()
    {
        TryBake();

        InitMisc();
        InitMaterials();
        InitTextures();
        InitMesh();
        InitSuns();

        InitUniforms(SkyMaterial);

        SetUniforms(SkyMaterial);
    }

    [ContextMenu("TryBake")]
    public void TryBake()
    {
        if (RunTimeBaking && artb != null) artb.Bake(atmosphereParameters);
    }

    public void UpdateNode()
    {
        //Rg = AtmosphereScale;
        //Rt = (64200f / 63600f) * Rg * AtmosphereHeight;
        //Rl = (64210.0f / 63600f) * Rg;   

        Sun.Origin = Origin;
    }

    public void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus == true && LostFocusForceRebake == true)
        {
            if (Transmittance == null || Inscatter == null || Irradiance == null) return;

            if (!Transmittance.IsCreated() || !Inscatter.IsCreated() || !Irradiance.IsCreated())
            {
                Debug.Log("Atmosphere: fail with textures!");
            }
        }
    }

    public void Render(bool now)
    {
        SetUniforms(SkyMaterial);
        SkyMaterial.renderQueue = RenderQueue;
        SkyMaterial.SetPass(0);

        if (!now)
            Graphics.DrawMesh(AtmosphereMesh, transform.localToWorldMatrix, SkyMaterial, 0);
        else
            Graphics.DrawMeshNow(AtmosphereMesh, transform.localToWorldMatrix);
    }

    private void OnDestroy()
    {
        CollectGarbage();
    }

    public void CollectGarbage(bool all = true)
    {
        if (Transmittance != null) Transmittance.ReleaseAndDestroy();
        if (Inscatter != null) Inscatter.ReleaseAndDestroy();
        if (Irradiance != null) Irradiance.ReleaseAndDestroy();
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
        Transmittance = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.TRANSMITTANCE_W, atmosphereParameters.TRANSMITTANCE_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
        Irradiance = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.SKY_W, atmosphereParameters.SKY_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
        Inscatter = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.RES_MU_S * atmosphereParameters.RES_NU, atmosphereParameters.RES_MU), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp, atmosphereParameters.RES_R);

        if (RunTimeBaking && artb != null)
        {

        }
        else
        {
            string texturesPath = "/Resources/Textures/Atmosphere/";

            ComputeBuffer buffer;

            string TransmittancePath = Application.dataPath + texturesPath + "/transmittance.raw";
            string IrradiancePath = Application.dataPath + texturesPath + "/irradiance.raw";
            string InscatterPath = Application.dataPath + texturesPath + "/inscatter.raw";

            buffer = new ComputeBuffer(atmosphereParameters.TRANSMITTANCE_W * atmosphereParameters.TRANSMITTANCE_H, sizeof(float) * 3);
            CBUtility.WriteIntoRenderTexture(Transmittance, 3, TransmittancePath, buffer, WriteDataCore);
            buffer.Release();

            buffer = new ComputeBuffer(atmosphereParameters.SKY_W * atmosphereParameters.SKY_H, sizeof(float) * 3);
            CBUtility.WriteIntoRenderTexture(Irradiance, 3, IrradiancePath, buffer, WriteDataCore);
            buffer.Release();

            buffer = new ComputeBuffer(atmosphereParameters.RES_MU_S * atmosphereParameters.RES_NU * atmosphereParameters.RES_MU * atmosphereParameters.RES_R, sizeof(float) * 4);
            CBUtility.WriteIntoRenderTexture(Inscatter, 4, InscatterPath, buffer, WriteDataCore);
            buffer.Release();
        }
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

        mat.SetFloat("scale", atmosphereParameters.Rg / atmosphereParameters.SCALE);
        mat.SetFloat("Rg", atmosphereParameters.Rg);
        mat.SetFloat("Rt", atmosphereParameters.Rt);
        mat.SetFloat("RL", atmosphereParameters.Rl);
        mat.SetFloat("TRANSMITTANCE_W", atmosphereParameters.TRANSMITTANCE_W);
        mat.SetFloat("TRANSMITTANCE_H", atmosphereParameters.TRANSMITTANCE_H);
        mat.SetFloat("SKY_W", atmosphereParameters.SKY_W);
        mat.SetFloat("SKY_H", atmosphereParameters.SKY_H);
        mat.SetFloat("RES_R", atmosphereParameters.RES_R);
        mat.SetFloat("RES_MU", atmosphereParameters.RES_MU);
        mat.SetFloat("RES_MU_S", atmosphereParameters.RES_MU_S);
        mat.SetFloat("RES_NU", atmosphereParameters.RES_NU);
        mat.SetFloat("AVERAGE_GROUND_REFLECTANCE", atmosphereParameters.AVERAGE_GROUND_REFLECTANCE);
        mat.SetFloat("HR", atmosphereParameters.HR * 1000.0f);
        mat.SetFloat("HM", atmosphereParameters.HM * 1000.0f);
        mat.SetVector("betaMSca", BETA_MSca / 1000.0f);
        mat.SetVector("betaMEx", (BETA_MSca / 1000.0f) / 0.9f);
        mat.SetTexture("_Sun_Glare", SunGlareTexture);
        mat.SetFloat("_Sun_Glare_Scale", SunGlareScale);
    }

    public void InitPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            foreach (Quad q in planetoid.MainQuads)
            {
                if (q != null)
                    planetoid.Atmosphere.InitUniforms(q.QuadMaterial);
            }
        }
    }

    public void SetPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            foreach (Quad q in planetoid.MainQuads)
            {
                if (q != null)
                    planetoid.Atmosphere.SetUniformsForPlanetQuad(q.QuadMaterial);
            }
        }
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("scale", atmosphereParameters.Rg / atmosphereParameters.SCALE);
        mat.SetFloat("Rg", atmosphereParameters.Rg);
        mat.SetFloat("Rt", atmosphereParameters.Rt);
        mat.SetFloat("RL", atmosphereParameters.Rl);
        mat.SetVector("betaR", betaR / 1000.0f);
        mat.SetFloat("mieG", Mathf.Clamp(atmosphereParameters.MIE_G, 0.0f, 0.99f));
        mat.SetFloat("_Sun_Glare_Scale", SunGlareScale);

        if (RunTimeBaking && artb != null)
        {
            if (artb.transmittanceT != null) mat.SetTexture("_Sky_Transmittance", artb.transmittanceT);
            if (artb.inscatterT_Read != null) mat.SetTexture("_Sky_Inscatter", artb.inscatterT_Read);
            if (artb.irradianceT_Read != null) mat.SetTexture("_Sky_Irradiance", artb.irradianceT_Read);
        }
        else
        {
            if (Transmittance != null) mat.SetTexture("_Sky_Transmittance", Transmittance);
            if (Inscatter != null) mat.SetTexture("_Sky_Inscatter", Inscatter);
            if (Irradiance != null) mat.SetTexture("_Sky_Irradiance", Irradiance);
        }

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
            mat.SetMatrix("_Sun_WorldToLocal", Sun.WorldToLocalRotation);
            Sun.SetUniforms(mat);
        }
    }

    public void SetUniformsForPlanetQuad(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("scale", atmosphereParameters.Rg / atmosphereParameters.SCALE);
        mat.SetFloat("Rg", atmosphereParameters.Rg);
        mat.SetFloat("Rt", atmosphereParameters.Rt);
        mat.SetFloat("RL", atmosphereParameters.Rl);
        mat.SetVector("betaR", betaR / 1000.0f);
        mat.SetFloat("mieG", Mathf.Clamp(atmosphereParameters.MIE_G, 0.0f, 0.99f));

        if (RunTimeBaking && artb != null)
        {
            if (artb.transmittanceT != null) mat.SetTexture("_Sky_Transmittance", artb.transmittanceT);
            if (artb.inscatterT_Read != null) mat.SetTexture("_Sky_Inscatter", artb.inscatterT_Read);
            if (artb.irradianceT_Read != null) mat.SetTexture("_Sky_Irradiance", artb.irradianceT_Read);
        }
        else
        {
            if (Transmittance != null) mat.SetTexture("_Sky_Transmittance", Transmittance);
            if (Inscatter != null) mat.SetTexture("_Sky_Inscatter", Inscatter);
            if (Irradiance != null) mat.SetTexture("_Sky_Irradiance", Irradiance);
        }

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
            mat.SetMatrix("_Sun_WorldToLocal", Sun.WorldToLocalRotation);
            Sun.SetUniforms(mat);
        }
    }
}