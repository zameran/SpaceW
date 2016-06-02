using System;

using UnityEngine;

using System.Collections.Generic;

public sealed class Atmosphere : MonoBehaviour
{
    private AtmosphereBase atmosphereBase = AtmosphereBase.Earth;
    private AtmosphereBase atmosphereBasePrev = AtmosphereBase.Earth;

    public AtmosphereBase AtmosphereBase
    {
        get { return atmosphereBase; }
        set
        {
            atmosphereBasePrev = atmosphereBase;
            atmosphereBase = value;

            if (atmosphereBasePrev != value)
                if (OnPresetChanged != null)
                    OnPresetChanged(this);
        }
    }

    public delegate void AtmosphereDelegate(Atmosphere a);
    public event AtmosphereDelegate OnPresetChanged, OnBaked;

    [Range(0.0f, 1.0f)]
    public float Density = 1.0f;

    [Tooltip("1/3 or 1/2 from Planet.TerrainMaxHeight")]
    public float TerrainRadiusHold = 0.0f;
    public float Radius = 2048f;
    public float Height = 100.0f;
    public float Scale = 1.0f;

    public int AtmosphereMeshResolution = 2;

    public float HDRExposure = 0.2f;

    public Shader SkyShader;

    public ComputeShader ReadDataCore;
    public ComputeShader WriteDataCore;

    public Material SkyMaterial;

    public Texture SunGlareTexture;
    public float SunGlareScale = 1;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset = 0;

    public RenderTexture Transmittance;
    public RenderTexture Inscatter;
    public RenderTexture Irradiance;

    public Mesh AtmosphereMesh;

    public bool RunTimeBaking = false;
    public bool LostFocusForceRebake = false;

    public AtmosphereSun Sun_1;
    public AtmosphereSun Sun_2;
    public AtmosphereSun Sun_3;
    public AtmosphereSun Sun_4;

    public List<GameObject> eclipseCasters;

    private AtmosphereParameters atmosphereParameters;

    public AtmosphereRunTimeBaker artb = null;

    public Vector3 Origin;

    private Matrix4x4 occludersMatrix1;
    private Matrix4x4 occludersMatrix2;
    private Matrix4x4 sunMatrix1;
    private Matrix4x4 worldToCamera;
    private Matrix4x4 cameraToWorld;
    private Matrix4x4 cameraToScreen;
    private Matrix4x4 screenToCamera;
    private Vector3 worldCameraPos;

    public Matrix4x4 OccludersMatrix1
    {
        get { return occludersMatrix1; }
        set { occludersMatrix1 = value; }
    }

    public Matrix4x4 OccludersMatrix2
    {
        get { return occludersMatrix2; }
        set { occludersMatrix2 = value; }
    }

    public Matrix4x4 SunMatrix1
    {
        get { return sunMatrix1; }
        set { sunMatrix1 = value; }
    }

    public Matrix4x4 WorldToCamera
    {
        get { return worldToCamera; }
        set { worldToCamera = value; }
    }

    public Matrix4x4 CameraToWorld
    {
        get { return cameraToWorld; }
        set { cameraToWorld = value; }
    }

    public Matrix4x4 CameraToScreen
    {
        get { return cameraToScreen; }
        set { cameraToScreen = value; }
    }

    public Matrix4x4 ScreenToCamera
    {
        get { return screenToCamera; }
        set { screenToCamera = value; }
    }

    public Vector3 WorldCameraPos
    {
        get { return worldCameraPos; }
        set { worldCameraPos = value; }
    }

    private void Start()
    {
        ApplyTestPresset(AtmosphereParameters.Get(atmosphereBase));

        TryBake();

        InitMisc();
        InitMaterials();
        InitTextures();
        InitMesh();
        InitSuns();

        InitSetAtmosphereUniforms();

        if (OnPresetChanged == null) //TODO: Need make some sort of Planetoid scale event manager.
            OnPresetChanged += AtmosphereOnPresetChanged;

        if (OnBaked == null)
            OnBaked += AtmosphereOnBaked;
    }

    private void ApplyTestPresset(AtmosphereParameters p)
    {
        atmosphereParameters = new AtmosphereParameters(p);

        atmosphereParameters.Rg = Radius - TerrainRadiusHold;
        atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
        atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
        atmosphereParameters.SCALE = Scale;
    }

    public void TryBake()
    {
        if (RunTimeBaking && artb != null) artb.Bake(atmosphereParameters);

        if (OnBaked != null) OnBaked(this);
    }

    public List<string> GetKeywords()
    {
        //TODO: Remake this keyword shit.

        List<string> Keywords = new List<string>();

        if (Sun_1 != null)
            Keywords.Add("LIGHT_1");

        if (Sun_2 != null)
            Keywords.Add("LIGHT_2");

        if (Sun_1 != null && Sun_2 != null)
            Keywords.Remove("LIGHT_1");

        if (Sun_3 != null)
            Keywords.Add("LIGHT_3");

        if (Sun_4 != null)
            Keywords.Add("LIGHT_4");

        if (Sun_1 != null && Sun_2 != null)
            if (Sun_3 != null && Sun_4 != null)
                Keywords.Remove("LIGHT_2");

        if (Sun_3 != null && Sun_4 != null)
            Keywords.Remove("LIGHT_3");

        Keywords.Add("ATMOSPHERE");
        Keywords.Add("ECLIPSES_ON");

        return Keywords;
    }

    public void SetKeywords(Material m, List<string> keywords)
    {
        if (m != null && ArraysEqual(m.shaderKeywords, keywords) == false)
        {
            m.shaderKeywords = keywords.ToArray();
        }
    }

    public void CalculateEclipses(out Matrix4x4 oc1, out Matrix4x4 oc2, out Matrix4x4 suns)
    {
        oc1 = Matrix4x4.zero;
        oc2 = Matrix4x4.zero;
        suns = Matrix4x4.zero;

        Vector4 OccluderPlanetPos = Vector4.zero;
        Vector4 SunPosition = Vector4.zero;

        float actualRadius = 250000;

        List<AtmosphereSun> Suns = new List<AtmosphereSun>();

        if (Sun_1 != null) Suns.Add(Sun_1);
        if (Sun_2 != null) Suns.Add(Sun_2);
        if (Sun_3 != null) Suns.Add(Sun_3);
        if (Sun_4 != null) Suns.Add(Sun_4);

        for (int i = 0; i < Mathf.Min(4, Suns.Count); i++)
        {
            SunPosition = Suns[i].transform.position;
            suns.SetRow(i, new Vector4(SunPosition.x, SunPosition.y, SunPosition.z, VectorHelper.AngularRadius(SunPosition, Origin, Suns[i].Radius)));
        }

        for (int i = 0; i < Mathf.Min(4, eclipseCasters.Count); i++)
        {
            if (eclipseCasters[i] == null) { Debug.Log("Atmosphere: Eclipses problem!"); break; }

            OccluderPlanetPos = eclipseCasters[i].transform.position;
            oc1.SetRow(i, new Vector4(OccluderPlanetPos.x, OccluderPlanetPos.y, OccluderPlanetPos.z, actualRadius));
        }

        for (int i = 4; i < Mathf.Min(8, eclipseCasters.Count); i++)
        {
            if (eclipseCasters[i] == null) { Debug.Log("Atmosphere: Eclipses problem!"); break; }

            OccluderPlanetPos = eclipseCasters[i].transform.position;
            oc2.SetRow(i - 4, new Vector4(OccluderPlanetPos.x, OccluderPlanetPos.y, OccluderPlanetPos.z, actualRadius));
        }
    }

    public void SetEclipses(Material mat)
    {
        CalculateEclipses(out occludersMatrix1, out occludersMatrix2, out sunMatrix1);

        mat.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        mat.SetMatrix("_Sky_LightOccluders_2", occludersMatrix2);
        mat.SetMatrix("_Sun_Positions_1", sunMatrix1);
    }

    public void SetEclipses(MaterialPropertyBlock block)
    {
        CalculateEclipses(out occludersMatrix1, out occludersMatrix2, out sunMatrix1);

        block.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        block.SetMatrix("_Sky_LightOccluders_2", occludersMatrix2);
        block.SetMatrix("_Sun_Positions_1", sunMatrix1);
    }

    public bool ArraysEqual<T>(T[] a, List<T> b)
    {
        if (a != null && b == null) return false;
        if (a == null && b != null) return false;

        if (a != null && b != null)
        {
            if (a.Length != b.Count) return false;

            var comparer = EqualityComparer<T>.Default;

            for (var i = 0; i < a.Length; i++)
            {
                if (comparer.Equals(a[i], b[i]) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void Update()
    {
        if (Sun_1 != null) Sun_1.UpdateNode();
        if (Sun_2 != null) Sun_2.UpdateNode();
        if (Sun_3 != null) Sun_3.UpdateNode();
        if (Sun_4 != null) Sun_4.UpdateNode();

        UpdateNode();
    }

    public void UpdateNode()
    {
        atmosphereParameters.Rg = Radius - TerrainRadiusHold;
        atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
        atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
        atmosphereParameters.SCALE = Scale;

        if (Sun_1 != null) Sun_1.Origin = Origin;
        if (Sun_2 != null) Sun_2.Origin = Origin;
        if (Sun_3 != null) Sun_3.Origin = Origin;
        if (Sun_4 != null) Sun_4.Origin = Origin;

        worldToCamera = CameraHelper.Main().GetWorldToCamera();
        cameraToWorld = CameraHelper.Main().GetCameraToWorld();
        cameraToScreen = CameraHelper.Main().GetCameraToScreen();
        screenToCamera = CameraHelper.Main().GetScreenToCamera();
        worldCameraPos = CameraHelper.Main().transform.position;
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

    public void Render(Vector3 Origin, int drawLayer = 8)
    {
        this.Origin = Origin;

        SetUniforms(SkyMaterial);

        SkyMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

        Graphics.DrawMesh(AtmosphereMesh, transform.localToWorldMatrix, SkyMaterial, drawLayer);
    }

    private void OnDestroy()
    {
        CollectGarbage();

        if (OnPresetChanged != null)
            OnPresetChanged -= AtmosphereOnPresetChanged;

        if (OnBaked != null)
            OnBaked -= AtmosphereOnBaked;
    }

    private void AtmosphereOnPresetChanged(Atmosphere a)
    {
        //Debug.Log("Atmosphere: AtmosphereOnPresetChanged() - " + a.gameObject.name);

        ApplyTestPresset(AtmosphereParameters.Get(AtmosphereBase));
        TryBake();
    }

    private void AtmosphereOnBaked(Atmosphere a)
    {
        //Just make sure that all Origin variables set.
        if (a.transform.parent != null)
        {
            Planetoid owner = a.GetComponentInParent<Planetoid>();

            if (owner != null)
                owner.ReSetupQuads();
        }
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
            SkyMaterial.name = "Sky" + "(Instance)" + UnityEngine.Random.Range(float.MinValue, float.MaxValue);
        }
    }

    public void InitTextures()
    {
        if (RunTimeBaking && artb != null)
        {

        }
        else
        {
            Transmittance = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.TRANSMITTANCE_W, atmosphereParameters.TRANSMITTANCE_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
            Irradiance = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.SKY_W, atmosphereParameters.SKY_H), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp);
            Inscatter = RTExtensions.CreateRTexture(new Vector2(atmosphereParameters.RES_MU_S * atmosphereParameters.RES_NU, atmosphereParameters.RES_MU), 0, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, TextureWrapMode.Clamp, atmosphereParameters.RES_R);

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
        worldToCamera = CameraHelper.Main().GetWorldToCamera();
        cameraToWorld = CameraHelper.Main().GetCameraToWorld();
        cameraToScreen = CameraHelper.Main().GetCameraToScreen();
        screenToCamera = CameraHelper.Main().GetScreenToCamera();
        worldCameraPos = CameraHelper.Main().transform.position;
    }

    public void InitSuns()
    {
        AtmosphereSun[] suns = FindObjectsOfType<AtmosphereSun>();

        if (suns.Length > 0) if (Sun_1 == null && suns[0] != null) Sun_1 = suns[0];
        if (suns.Length > 1) if (Sun_2 == null && suns[1] != null) Sun_2 = suns[1];
        if (suns.Length > 2) if (Sun_3 == null && suns[2] != null) Sun_3 = suns[2];
        if (suns.Length > 3) if (Sun_4 == null && suns[3] != null) Sun_4 = suns[3];
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        SetKeywords(mat, GetKeywords());
        SetEclipses(mat);

        mat.SetTexture("_Sun_Glare", SunGlareTexture);

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
    }

    [Obsolete]
    public void InitUniformsForPlanetQuad(MaterialPropertyBlock block, Material mat, bool full)
    {
        if (mat != null)
        {
            SetKeywords(mat, GetKeywords());
        }

        if (full)
        {
            if (block == null) return;

            SetEclipses(mat);

            block.SetTexture("_Sun_Glare", SunGlareTexture);

            block.SetFloat("Rg", atmosphereParameters.Rg);
            block.SetFloat("Rt", atmosphereParameters.Rt);
            block.SetFloat("RL", atmosphereParameters.Rl);

            block.SetFloat("TRANSMITTANCE_W", atmosphereParameters.TRANSMITTANCE_W);
            block.SetFloat("TRANSMITTANCE_H", atmosphereParameters.TRANSMITTANCE_H);
            block.SetFloat("SKY_W", atmosphereParameters.SKY_W);
            block.SetFloat("SKY_H", atmosphereParameters.SKY_H);
            block.SetFloat("RES_R", atmosphereParameters.RES_R);
            block.SetFloat("RES_MU", atmosphereParameters.RES_MU);
            block.SetFloat("RES_MU_S", atmosphereParameters.RES_MU_S);
            block.SetFloat("RES_NU", atmosphereParameters.RES_NU);
            block.SetFloat("AVERAGE_GROUND_REFLECTANCE", atmosphereParameters.AVERAGE_GROUND_REFLECTANCE);
            block.SetFloat("HR", atmosphereParameters.HR * 1000.0f);
            block.SetFloat("HM", atmosphereParameters.HM * 1000.0f);
        }
    }

    public void InitPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            for (int i = 0; i < planetoid.Quads.Count; i++)
            {
                if (planetoid.Quads[i] != null)
                {
                    planetoid.Atmosphere.InitUniforms(planetoid.Quads[i].QuadMaterial);
                }
            }
        }
    }

    public void SetPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            for (int i = 0; i < planetoid.Quads.Count; i++)
            {
                if (planetoid.Quads[i] != null)
                {
                    planetoid.Atmosphere.SetUniformsForPlanetQuad(null, planetoid.Quads[i].QuadMaterial, false);
                }
            }
        }
    }

    public void InitSetPlanetoidUniforms(Planetoid planetoid)
    {
        InitPlanetoidUniforms(planetoid);
        SetPlanetoidUniforms(planetoid);
    }

    public void InitSetAtmosphereUniforms()
    {
        InitUniforms(SkyMaterial);
        SetUniforms(SkyMaterial);
    }

    public void InitSetAtmosphereUniforms(Atmosphere atmosphere)
    {
        InitUniforms(atmosphere.SkyMaterial);
        SetUniforms(atmosphere.SkyMaterial);
    }

    public void ReanimateAtmosphereUniforms(Atmosphere atmosphere, Planetoid planetoid)
    {
        if (atmosphere != null && planetoid != null)
        {
            atmosphere.InitSetPlanetoidUniforms(planetoid);
            atmosphere.InitSetAtmosphereUniforms();

            if (Sun_1 != null)
            {
                SunGlare sg_1 = Sun_1.GetComponent<SunGlare>();

                if (sg_1 != null)
                    sg_1.InitSetAtmosphereUniforms();
            }

            if (Sun_2 != null)
            {
                SunGlare sg_2 = Sun_2.GetComponent<SunGlare>();

                if (sg_2 != null)
                    sg_2.InitSetAtmosphereUniforms();
            }

            if (Sun_3 != null)
            {
                SunGlare sg_3 = Sun_3.GetComponent<SunGlare>();

                if (sg_3 != null)
                    sg_3.InitSetAtmosphereUniforms();
            }

            if (Sun_4 != null)
            {
                SunGlare sg_4 = Sun_4.GetComponent<SunGlare>();

                if (sg_4 != null)
                    sg_4.InitSetAtmosphereUniforms();
            }
        }
        else
            Debug.Log("Atmosphere: Reanimation fail!");
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        SetKeywords(mat, GetKeywords());
        SetEclipses(mat);

        mat.SetFloat("density", Density);
        mat.SetFloat("scale", atmosphereParameters.SCALE);
        mat.SetFloat("Rg", atmosphereParameters.Rg);
        mat.SetFloat("Rt", atmosphereParameters.Rt);
        mat.SetFloat("RL", atmosphereParameters.Rl);
        mat.SetVector("betaR", atmosphereParameters.BETA_R / 1000);
        mat.SetVector("betaMSca", atmosphereParameters.BETA_MSca / 1000);
        mat.SetVector("betaMEx", atmosphereParameters.BETA_MEx / 1000);
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

        mat.SetMatrix("_Globals_WorldToCamera", worldToCamera);
        mat.SetMatrix("_Globals_CameraToWorld", cameraToWorld);
        mat.SetMatrix("_Globals_CameraToScreen", cameraToScreen);
        mat.SetMatrix("_Globals_ScreenToCamera", screenToCamera);
        mat.SetVector("_Globals_WorldCameraPos", worldCameraPos);

        mat.SetVector("_Globals_Origin", -Origin);
        mat.SetFloat("_Exposure", HDRExposure);

        if (Sun_1 != null) Sun_1.SetUniforms(mat);
        if (Sun_2 != null) Sun_2.SetUniforms(mat);
        if (Sun_3 != null) Sun_3.SetUniforms(mat);
        if (Sun_4 != null) Sun_4.SetUniforms(mat);
    }

    [Obsolete]
    public void SetUniformsForPlanetQuad(Material mat)
    {
        if (mat == null) return;

        SetKeywords(mat, GetKeywords());
        SetEclipses(mat);

        mat.SetFloat("density", Density);
        mat.SetFloat("scale", atmosphereParameters.SCALE);
        mat.SetFloat("Rg", atmosphereParameters.Rg);
        mat.SetFloat("Rt", atmosphereParameters.Rt);
        mat.SetFloat("RL", atmosphereParameters.Rl);
        mat.SetVector("betaR", atmosphereParameters.BETA_R / 1000);
        mat.SetVector("betaMSca", atmosphereParameters.BETA_MSca / 1000);
        mat.SetVector("betaMEx", atmosphereParameters.BETA_MEx / 1000);
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

        mat.SetMatrix("_Globals_WorldToCamera", worldToCamera);
        mat.SetMatrix("_Globals_CameraToWorld", cameraToWorld);
        mat.SetMatrix("_Globals_CameraToScreen", cameraToScreen);
        mat.SetMatrix("_Globals_ScreenToCamera", screenToCamera);
        mat.SetVector("_Globals_WorldCameraPos", worldCameraPos - Origin); // Apply origin to vector on planetoid quads. 

        mat.SetVector("_Globals_Origin", -Origin);
        mat.SetFloat("_Exposure", HDRExposure);

        if (Sun_1 != null) Sun_1.SetUniforms(mat);
        if (Sun_2 != null) Sun_2.SetUniforms(mat);
        if (Sun_3 != null) Sun_3.SetUniforms(mat);
        if (Sun_4 != null) Sun_4.SetUniforms(mat);
    }

    public void SetUniformsForPlanetQuad(MaterialPropertyBlock block, Material mat, bool full = true)
    {
        if (mat != null)
        {
            SetKeywords(mat, GetKeywords());
        }

        if (full)
        {
            if (block == null) return;

            SetEclipses(block);

            block.SetFloat("density", Density);
            block.SetFloat("scale", atmosphereParameters.SCALE);
            block.SetFloat("Rg", atmosphereParameters.Rg);
            block.SetFloat("Rt", atmosphereParameters.Rt);
            block.SetFloat("RL", atmosphereParameters.Rl);
            block.SetVector("betaR", atmosphereParameters.BETA_R / 1000);
            block.SetVector("betaMSca", atmosphereParameters.BETA_MSca / 1000);
            block.SetVector("betaMEx", atmosphereParameters.BETA_MEx / 1000);
            block.SetFloat("mieG", Mathf.Clamp(atmosphereParameters.MIE_G, 0.0f, 0.99f));

            if (RunTimeBaking && artb != null)
            {
                if (artb.transmittanceT != null) block.SetTexture("_Sky_Transmittance", artb.transmittanceT);
                if (artb.inscatterT_Read != null) block.SetTexture("_Sky_Inscatter", artb.inscatterT_Read);
                if (artb.irradianceT_Read != null) block.SetTexture("_Sky_Irradiance", artb.irradianceT_Read);
            }
            else
            {
                if (Transmittance != null) block.SetTexture("_Sky_Transmittance", Transmittance);
                if (Inscatter != null) block.SetTexture("_Sky_Inscatter", Inscatter);
                if (Irradiance != null) block.SetTexture("_Sky_Irradiance", Irradiance);
            }

            block.SetMatrix("_Globals_WorldToCamera", worldToCamera);
            block.SetMatrix("_Globals_CameraToWorld", cameraToWorld);
            block.SetMatrix("_Globals_CameraToScreen", cameraToScreen);
            block.SetMatrix("_Globals_ScreenToCamera", screenToCamera);
            block.SetVector("_Globals_WorldCameraPos", worldCameraPos - Origin); // Apply origin to vector on planetoid quads. 

            block.SetVector("_Globals_Origin", -Origin);
            block.SetFloat("_Exposure", HDRExposure);

            if (Sun_1 != null) Sun_1.SetUniforms(block);
            if (Sun_2 != null) Sun_2.SetUniforms(block);
            if (Sun_3 != null) Sun_3.SetUniforms(block);
            if (Sun_4 != null) Sun_4.SetUniforms(block);
        }
    }
}