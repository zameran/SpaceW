#region License

// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

#endregion

using SpaceEngine.Core.Bodies;
using SpaceEngine.Environment.Atmospheric;
using SpaceEngine.Helpers;
using SpaceEngine.Tools;
using SpaceEngine.Utilities;
using UnityEngine;

namespace SpaceEngine.Environment.Oceanic
{
    /// <summary>
    ///     Extend the OceanFFT node to also generate ocean white caps.
    /// </summary>
    public class OceanWhiteCaps : OceanFFT
    {
        [SerializeField]
        protected Material InitJacobiansMaterial;

        [SerializeField]
        protected Material WhiteCapsPrecomputeMaterial;

        [SerializeField]
        [Range(0, 16)]
        protected int FoamAniso = 9;

        [SerializeField]
        [Range(-2.0f, 2.0f)]
        protected float FoamMipmapBias = -2.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        protected float WhiteCapStrength = 0.1f;

        protected RenderTexture Foam0;
        protected RenderTexture Foam1;

        protected RenderTexture[] FourierBuffer5;
        protected RenderTexture[] FourierBuffer6;
        protected RenderTexture[] FourierBuffer7;

        #region OceanNode

        /// <inheritdoc />
        protected override void UpdateKeywords(Material target)
        {
            base.UpdateKeywords(target);

            Helper.ToggleKeyword(OceanMaterial, WHITECAPS_KEYWORD, FFT_KEYWORD);
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
            CreateBuffer(out FourierBuffer5, format, "Jacobians XX"); // Jacobians XX
            CreateBuffer(out FourierBuffer6, format, "Jacobians YY"); // Jacobians YY
            CreateBuffer(out FourierBuffer7, format, "Jacobians XY"); // Jacobians XY

            // Make sure the base textures are also created
            base.CreateRenderTextures();
        }

        protected override void InitWaveSpectrum()
        {
            base.InitWaveSpectrum();

            // Init jacobians (5,6,7)
            var buffers567 = new[] { FourierBuffer5[1], FourierBuffer6[1], FourierBuffer7[1] };

            RTUtility.MultiTargetBlit(buffers567, InitJacobiansMaterial);
        }

        #region Node

        public override void InitNode()
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

        public override void UpdateNode()
        {
            if (DrawOcean)
            {
                Fourier.PeformFFT(FourierBuffer5, FourierBuffer6, FourierBuffer7);

                WhiteCapsPrecomputeMaterial.SetTexture("_Map5", FourierBuffer5[IDX]);
                WhiteCapsPrecomputeMaterial.SetTexture("_Map6", FourierBuffer6[IDX]);
                WhiteCapsPrecomputeMaterial.SetTexture("_Map7", FourierBuffer7[IDX]);
                WhiteCapsPrecomputeMaterial.SetVector("_Choppyness", Choppyness);

                var buffers = new[] { Foam0, Foam1 };

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

            if (Foam0 != null)
            {
                Foam0.Release();
            }

            if (Foam1 != null)
            {
                Foam1.Release();
            }

            for (var i = 0; i < 2; i++)
            {
                if (FourierBuffer5 != null)
                {
                    if (FourierBuffer5[i] != null)
                    {
                        FourierBuffer5[i].Release();
                    }
                }

                if (FourierBuffer6 != null)
                {
                    if (FourierBuffer6[i] != null)
                    {
                        FourierBuffer6[i].Release();
                    }
                }

                if (FourierBuffer7 != null)
                {
                    if (FourierBuffer7[i] != null)
                    {
                        FourierBuffer7[i].Release();
                    }
                }
            }
        }

        #endregion
    }
}