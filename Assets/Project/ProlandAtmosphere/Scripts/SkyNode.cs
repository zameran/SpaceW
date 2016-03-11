using UnityEngine;

namespace Proland
{

    public class SkyNode : Node
    {
        public float AtmosphereHeight = 1.0f;

        private float Rg;
        private float Rt;
        private float Rl;

        //Dimensions of the tables
        const int TRANSMITTANCE_W = 256;
        const int TRANSMITTANCE_H = 64;
        const int SKY_W = 64;
        const int SKY_H = 16;
        const int RES_R = 32;
        const int RES_MU = 128;
        const int RES_MU_S = 32;
        const int RES_NU = 8;

        const float AVERAGE_GROUND_REFLECTANCE = 0.1f;

        //Half heights for the atmosphere air density (HR) and particle density (HM)
        //This is the height in km that half the particles are found below
        const float HR = 8.0f;
        const float HM = 1.2f;

        //scatter coefficient for mie
        readonly Vector3 BETA_MSca = new Vector3(4e-3f, 4e-3f, 4e-3f);

        public Shader skyShader;
        public Shader skyMapShader;

        public Texture sunGlareTexture;

        Material skyMaterial;
        Material skyMapMaterial;

        [SerializeField]
        Vector3 betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);

        //Asymmetry factor for the mie phase function
        //A higher number meands more light is scattered in the forward direction
        [SerializeField]
        float mieG = 0.85f;

        string texturesPath = "/Project/ProlandAtmosphere/Textures/";

        public bool drawSkyMap = false;

        Mesh mesh;

        RenderTexture transmittance, inscatter, irradiance, skyMap;

        private int waitBeforeReloadCount = 0;

        protected override void Start()
        {
            base.Start();

            Rg = m_manager.GetRadius();
            Rt = (64200f / 63600f) * Rg * AtmosphereHeight;
            Rl = (64210.0f / 63600f) * Rg;

            mesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            InitTextures();

            skyMaterial = new Material(skyShader);
            skyMaterial.name = "Sky" + "(Instance)" + Random.Range(float.MinValue, float.MaxValue);

            skyMapMaterial = new Material(skyMapShader);
            skyMapMaterial.name = "SkyMap" + "(Instance)" + Random.Range(float.MinValue, float.MaxValue);

            InitUniforms(skyMaterial);
            InitUniforms(skyMapMaterial);
        }

        public void InitTextures()
        {
            //The sky map is used to create a reflection of the sky for objects that need it (like the ocean)
            skyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
            skyMap.filterMode = FilterMode.Trilinear;
            skyMap.wrapMode = TextureWrapMode.Clamp;
            skyMap.anisoLevel = 9;
            skyMap.useMipMap = true;
            //m_skyMap.mipMapBias = -0.5f;
            skyMap.Create();

            //Transmittance is responsible for the change in the sun color as it moves
            //The raw file is a 2D array of 32 bit floats with a range of 0 to 1
            string path = Application.dataPath + texturesPath + "/transmittance.raw";

            transmittance = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBHalf);
            transmittance.wrapMode = TextureWrapMode.Clamp;
            transmittance.filterMode = FilterMode.Bilinear;
            transmittance.enableRandomWrite = true;
            transmittance.Create();

            ComputeBuffer buffer = new ComputeBuffer(TRANSMITTANCE_W * TRANSMITTANCE_H, sizeof(float) * 3);
            CBUtility.WriteIntoRenderTexture(transmittance, 3, path, buffer, m_manager.GetWriteData());
            buffer.Release();

            //Iirradiance is responsible for the change in the sky color as the sun moves
            //The raw file is a 2D array of 32 bit floats with a range of 0 to 1
            path = Application.dataPath + texturesPath + "/irradiance.raw";

            irradiance = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBHalf);
            irradiance.wrapMode = TextureWrapMode.Clamp;
            irradiance.filterMode = FilterMode.Bilinear;
            irradiance.enableRandomWrite = true;
            irradiance.Create();

            buffer = new ComputeBuffer(SKY_W * SKY_H, sizeof(float) * 3);
            CBUtility.WriteIntoRenderTexture(irradiance, 3, path, buffer, m_manager.GetWriteData());
            buffer.Release();

            //Inscatter is responsible for the change in the sky color as the sun moves
            //The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
            //As there is not such thing as a 4D texture the data is packed into a 3D texture 
            //and the shader manually performs the sample for the 4th dimension
            path = Application.dataPath + texturesPath + "/inscatter.raw";

            inscatter = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBHalf);
            inscatter.volumeDepth = RES_R;
            inscatter.wrapMode = TextureWrapMode.Clamp;
            inscatter.filterMode = FilterMode.Bilinear;
            inscatter.isVolume = true;
            inscatter.enableRandomWrite = true;
            inscatter.Create();

            buffer = new ComputeBuffer(RES_MU_S * RES_NU * RES_MU * RES_R, sizeof(float) * 4);
            CBUtility.WriteIntoRenderTexture(inscatter, 4, path, buffer, m_manager.GetWriteData());
            buffer.Release();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyImmediate(skyMapMaterial);
            DestroyImmediate(skyMaterial);

            transmittance.Release();
            irradiance.Release();
            inscatter.Release();
            skyMap.Release();
        }

        public void UpdateNode()
        {
            Rg = m_manager.GetRadius();
            Rt = (64200f / 63600f) * Rg * AtmosphereHeight;
            Rl = (64210.0f / 63600f) * Rg;

            SetUniforms(skyMaterial);
            SetUniforms(skyMapMaterial);

            m_manager.SetUniforms(skyMaterial);
            skyMaterial.SetMatrix("_Sun_WorldToLocal", m_manager.GetSunNode().GetWorldToLocalRotation());

            Graphics.DrawMesh(mesh, Matrix4x4.identity, skyMaterial, 0, Camera.main);

            if (!inscatter.IsCreated() || !transmittance.IsCreated() || !irradiance.IsCreated())
            {
                waitBeforeReloadCount++;

                if (waitBeforeReloadCount >= 1)
                {
                    inscatter.Release();
                    transmittance.Release();
                    irradiance.Release();

                    InitTextures();

                    waitBeforeReloadCount = 0;
                }
            }

            if (drawSkyMap && ((m_manager.GetSunNode().GetHasMoved()) || Time.frameCount == 1))
                Graphics.Blit(null, skyMap, skyMapMaterial);
        }

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetFloat("scale", Rg / m_manager.GetRadius());
            mat.SetFloat("Rg", Rg);
            mat.SetFloat("Rt", Rt);
            mat.SetFloat("RL", Rl);
            mat.SetVector("betaR", betaR / 1000.0f);
            mat.SetFloat("mieG", Mathf.Clamp(mieG, 0.0f, 0.99f));
            mat.SetTexture("_Sky_Transmittance", transmittance);
            mat.SetTexture("_Sky_Inscatter", inscatter);
            mat.SetTexture("_Sky_Irradiance", irradiance);
            mat.SetTexture("_Sky_Map", skyMap);

            m_manager.GetSunNode().SetUniforms(mat);
        }

        public void InitUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetFloat("scale", Rg / m_manager.GetRadius());
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
            mat.SetTexture("_Sun_Glare", sunGlareTexture);
        }

        void OnGUI()
        {
            if (drawSkyMap)
                GUI.DrawTexture(new Rect(0, 0, 512, 512), skyMap);
        }
    }
}