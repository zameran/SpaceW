using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Utilities;
using SpaceEngine.Enviroment.Atmospheric;

using UnityEngine;

namespace SpaceEngine.Enviroment.Oceanic
{
    /// <summary>
    /// Extend the OceanFFT node to also generate ocean white caps.
    /// </summary>
    public class OceanWhiteCaps : OceanFFT
    {
        [SerializeField]
        Material InitJacobiansMaterial;

        [SerializeField]
        Material WhiteCapsPrecomputeMaterial;

        [SerializeField]
        int FoamAniso = 9;

        [SerializeField]
        float FoamMipmapBias = -2.0f;

        [SerializeField]
        float WhiteCapStrength = 0.1f;

        RenderTexture[] FourierBuffer5;
        RenderTexture[] FourierBuffer6;
        RenderTexture[] FourierBuffer7;

        RenderTexture Foam0;
        RenderTexture Foam1;

        #region OceanNode

        protected override void InitOceanNode()
        {
            
        }

        protected override void UpdateOceanNode()
        {
            if (OceanMaterial.IsKeywordEnabled(FFT_KEYWORD)) OceanMaterial.DisableKeyword(FFT_KEYWORD);
            if (!OceanMaterial.IsKeywordEnabled(WHITECAPS_KEYWORD)) OceanMaterial.EnableKeyword(WHITECAPS_KEYWORD);

            UpdateKeywords(OceanMaterial);
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            base.InitNode();

            if (InitJacobiansMaterial == null)
            {
                Debug.Log("OceanWhiteCaps: Init jacobians material is null! Trying to find it out...");
                InitJacobiansMaterial = MaterialHelper.CreateTemp(Shader.Find("SpaceEngine/Ocean/InitJacobians"), "InitJacobians");
            }

            if (WhiteCapsPrecomputeMaterial == null)
            {
                Debug.Log("OceanWhiteCaps: White caps precompute material is null! Trying to find it out...");
                WhiteCapsPrecomputeMaterial = MaterialHelper.CreateTemp(Shader.Find("SpaceEngine/Ocean/WhiteCapsPrecompute"), "WhiteCapsPrecompute");
            }

            InitJacobiansMaterial.SetTexture("_Spectrum01", Spectrum01);
            InitJacobiansMaterial.SetTexture("_Spectrum23", Spectrum23);
            InitJacobiansMaterial.SetTexture("_WTable", WTable);
            InitJacobiansMaterial.SetVector("_Offset", Offset);
            InitJacobiansMaterial.SetVector("_InverseGridSizes", InverseGridSizes);
        }

        protected override void UpdateNode()
        {
            if (DrawOcean == true)
            {
                Fourier.PeformFFT(FourierBuffer5, FourierBuffer6, FourierBuffer7);

                WhiteCapsPrecomputeMaterial.SetTexture("_Map5", FourierBuffer5[IDX]);
                WhiteCapsPrecomputeMaterial.SetTexture("_Map6", FourierBuffer6[IDX]);
                WhiteCapsPrecomputeMaterial.SetTexture("_Map7", FourierBuffer7[IDX]);
                WhiteCapsPrecomputeMaterial.SetVector("_Choppyness", Choppyness);

                var buffers = new RenderTexture[] { Foam0, Foam1 };

                RTUtility.MultiTargetBlit(buffers, WhiteCapsPrecomputeMaterial);

                OceanMaterial.SetFloat("_Ocean_WhiteCapStr", WhiteCapStrength);
                OceanMaterial.SetTexture("_Ocean_Foam0", Foam0);
                OceanMaterial.SetTexture("_Ocean_Foam1", Foam1);
            }

            base.UpdateNode();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Foam0 != null) Foam0.Release();
            if (Foam1 != null) Foam1.Release();

            for (byte i = 0; i < 2; i++)
            {
                if (FourierBuffer5 != null) if (FourierBuffer5[i] != null) FourierBuffer5[i].Release();
                if (FourierBuffer6 != null) if (FourierBuffer6[i] != null) FourierBuffer6[i].Release();
                if (FourierBuffer7 != null) if (FourierBuffer7[i] != null) FourierBuffer7[i].Release();
            }
        }

        #endregion

        #region Events

        protected override void OnAtmosphereBaked(Body celestialBody, Atmosphere atmosphere)
        {
            DrawOcean = true;

            UpdateNode();

            DrawOcean = false;

            base.OnAtmosphereBaked(celestialBody, atmosphere);
        }

        #endregion

        #region IRenderable

        public override void Render(int layer = 0)
        {
            base.Render(layer);
        }

        #endregion

        protected override void CreateRenderTextures()
        {
            var format = RenderTextureFormat.ARGBFloat;

            // These texture hold the actual data use in the ocean renderer
            Foam0 = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, FoamAniso);
            Foam1 = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, FoamAniso);

            Foam0.mipMapBias = FoamMipmapBias;
            Foam1.mipMapBias = FoamMipmapBias;

            // These textures are used to perform the fourier transform
            CreateBuffer(out FourierBuffer5, format);// Jacobians XX
            CreateBuffer(out FourierBuffer6, format);// Jacobians YY
            CreateBuffer(out FourierBuffer7, format);// Jacobians XY

            // Make sure the base textures are also created
            base.CreateRenderTextures();
        }

        protected override void InitWaveSpectrum(float t)
        {
            base.InitWaveSpectrum(t);

            // Init jacobians (5,6,7)
            var buffers567 = new RenderTexture[] { FourierBuffer5[1], FourierBuffer6[1], FourierBuffer7[1] };

            RTUtility.MultiTargetBlit(buffers567, InitJacobiansMaterial);
        }
    }
}