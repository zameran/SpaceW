using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Bodies;

using UnityEngine;

namespace SpaceEngine.Ocean
{
    /// <summary>
    /// Extend the OceanFFT node to also generate ocean white caps.
    /// </summary>
    public class OceanWhiteCaps : OceanFFT
    {
        [SerializeField]
        Material InitJacobiansMaterial;

        [SerializeField]
        Material WhiteCapsPrecomputeMat;

        [SerializeField]
        int FoamAniso = 9;

        [SerializeField]
        float FoamMipmapBias = -2.0f;

        [SerializeField]
        float WhiteCapStrength = 0.1f;

        RenderTexture[] FourierBuffer5;
        RenderTexture[] FourierBuffer6;
        RenderTexture[] FourierBuffer7;

        RenderTexture Map5;
        RenderTexture Map6;

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

            UpdateKeywords();
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            base.InitNode();

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

                WhiteCapsPrecomputeMat.SetTexture("_Map5", FourierBuffer5[IDX]);
                WhiteCapsPrecomputeMat.SetTexture("_Map6", FourierBuffer6[IDX]);
                WhiteCapsPrecomputeMat.SetTexture("_Map7", FourierBuffer7[IDX]);
                WhiteCapsPrecomputeMat.SetVector("_Choppyness", Choppyness);

                var buffers = new RenderTexture[] { Foam0, Foam1 };

                RTUtility.MultiTargetBlit(buffers, WhiteCapsPrecomputeMat);

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
            var mapFormat = RenderTextureFormat.ARGBFloat;
            var format = RenderTextureFormat.ARGBFloat;

            // These texture hold the actual data use in the ocean renderer
            CreateMap(ref Map5, mapFormat, Aniso);
            CreateMap(ref Map6, mapFormat, Aniso);

            CreateMap(ref Foam0, format, FoamAniso);
            CreateMap(ref Foam1, format, FoamAniso);

            Foam1.mipMapBias = FoamMipmapBias;

            // These textures are used to perform the fourier transform
            CreateBuffer(ref FourierBuffer5, format);// Jacobians XX
            CreateBuffer(ref FourierBuffer6, format);// Jacobians YY
            CreateBuffer(ref FourierBuffer7, format);// Jacobians XY

            // Make sure the base textures are also created
            base.CreateRenderTextures();
        }

        protected override void InitWaveSpectrum(float t)
        {
            base.InitWaveSpectrum(t);

            // Init jacobians (5,6,7)
            var buffers567 = new RenderTexture[] { FourierBuffer5[1], FourierBuffer6[1], FourierBuffer7[1] };

            InitJacobiansMaterial.SetFloat("_T", t);

            RTUtility.MultiTargetBlit(buffers567, InitJacobiansMaterial);
        }
    }
}