﻿#region License
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

using Random = UnityEngine.Random;

namespace SpaceEngine.Environment.Oceanic
{
    /// <summary>
    /// Extend the base class <see cref="OceanNode"/> to provide the data need to create the waves using fourier transform,
    /// which can then be applied to the projected grid handled by the <see cref="OceanNode"/>.
    /// All the fourier transforms are performed on the GPU.
    /// </summary>
    public class OceanFFT : OceanNode
    {
        private const float WAVE_CM = 0.23f; // Eq 59
        private const float WAVE_KM = 370.0f; // Eq 59
        private const float AMP = 1.0f;

        [SerializeField]
        protected Material InitSpectrumMaterial;

        [SerializeField]
        protected Material InitDisplacementMaterial;

        [SerializeField]
        protected int Aniso = 2;

        /// <summary>
        /// A higher wind speed gives greater swell to the waves.
        /// </summary>
        [SerializeField]
        private float WindSpeed = 5.0f;

        /// <summary>
        /// A lower number means the waves last longer and will build up larger waves.
        /// </summary>
        [SerializeField]
        private float WavesOmega = 0.84f;

        /// <summary>
        /// Size in meters (i.e. in spatial domain) of each grid.
        /// </summary>
        [SerializeField]
        private Vector4 GridSizes = new Vector4(5488, 392, 28, 2);

        /// <summary>
        /// Strenght of sideways displacement for each grid.
        /// </summary>
        [SerializeField]
        protected Vector4 Choppyness = new Vector4(2.3f, 2.1f, 1.3f, 0.9f);

        protected int FourierGridSize => GodManager.Instance.FourierGridSize;

        protected Shader FourierShader => GodManager.Instance.FourierShader;

        [SerializeField]
        [Range(16.0f, 128.0f)]
        protected int VarianceSize = 16;

        protected float MapSize;
        protected float MaxSlopeVariance;

        protected int IDX = 0;

        protected Vector4 InverseGridSizes;

        protected RenderTexture Spectrum01;
        protected RenderTexture Spectrum23;
        protected RenderTexture WTable;

        private RenderTexture[] FourierBuffer0;
        private RenderTexture[] FourierBuffer1;
        private RenderTexture[] FourierBuffer2;
        private RenderTexture[] FourierBuffer3;
        private RenderTexture[] FourierBuffer4;

        private RenderTexture Map0;
        private RenderTexture Map1;
        private RenderTexture Map2;
        private RenderTexture Map3;
        private RenderTexture Map4;

        private RenderTexture Variance;

        protected FourierGPU Fourier;

        public override float GetMaxSlopeVariance()
        {
            return MaxSlopeVariance;
        }

        #region OceanNode

        /// <inheritdoc />
        protected override void UpdateKeywords(Material target)
        {
            base.UpdateKeywords(target);

            Helper.ToggleKeyword(OceanMaterial, FFT_KEYWORD, WHITECAPS_KEYWORD);
        }

        #endregion

        #region Node

        public override void InitNode()
        {
            base.InitNode();

            if (InitSpectrumMaterial == null)
            {
                Debug.Log("OceanFFT: Init spectrum material is null! Trying to find it out...");
                InitSpectrumMaterial = MaterialHelper.CreateTemp(Shader.Find("SpaceEngine/Ocean/InitSpectrum"), "InitSpectrum");
            }

            if (InitDisplacementMaterial == null)
            {
                Debug.Log("OceanFFT: Init displacement material is null! Trying to find it out..");
                InitDisplacementMaterial = MaterialHelper.CreateTemp(Shader.Find("SpaceEngine/Ocean/InitDisplacement"), "InitDisplacement");
            }

            MapSize = (float)FourierGridSize;
            Offset = new Vector4(1.0f + 0.5f / MapSize, 1.0f + 0.5f / MapSize, 0, 0);

            var factor = 2.0f * Mathf.PI * MapSize;
            InverseGridSizes = new Vector4(factor / GridSizes.x, factor / GridSizes.y, factor / GridSizes.z, factor / GridSizes.w);

            Fourier = new FourierGPU(FourierGridSize, FourierShader);

            //Create the data needed to make the waves each frame
            CreateRenderTextures();
            GenerateWavesSpectrum();
            CreateWTable();

            InitSpectrumMaterial.SetTexture("_Spectrum01", Spectrum01);
            InitSpectrumMaterial.SetTexture("_Spectrum23", Spectrum23);
            InitSpectrumMaterial.SetTexture("_WTable", WTable);
            InitSpectrumMaterial.SetVector("_Offset", Offset);
            InitSpectrumMaterial.SetVector("_InverseGridSizes", InverseGridSizes);

            InitDisplacementMaterial.SetVector("_InverseGridSizes", InverseGridSizes);
        }

        public override void UpdateNode()
        {
            if (DrawOcean == true)
            {
                InitWaveSpectrum();

                // Perform fourier transform and record what is the current index
                IDX = Fourier.PeformFFT(FourierBuffer0, FourierBuffer1, FourierBuffer2);
                Fourier.PeformFFT(FourierBuffer3, FourierBuffer4);

                // Copy the contents of the completed fourier transform to the map textures.
                // You could just use the buffer textures (FourierBuffer0,1,2,etc) to read from for the ocean shader 
                // but they need to have mipmaps and unity updates the mipmaps
                // every time the texture is renderer into. This impacts performance during fourier transform stage as mipmaps would be updated every pass
                // and there is no way to disable and then enable mipmaps on render textures in Unity at time of writting.

                Graphics.Blit(FourierBuffer0[IDX], Map0);
                Graphics.Blit(FourierBuffer1[IDX], Map1);
                Graphics.Blit(FourierBuffer2[IDX], Map2);
                Graphics.Blit(FourierBuffer3[IDX], Map3);
                Graphics.Blit(FourierBuffer4[IDX], Map4);

                OceanMaterial.SetVector("_Ocean_MapSize", new Vector2(MapSize, MapSize));
                OceanMaterial.SetVector("_Ocean_Choppyness", Choppyness);
                OceanMaterial.SetVector("_Ocean_GridSizes", GridSizes);
                OceanMaterial.SetFloat("_Ocean_HeightOffset", OceanLevel - OceanWaveLevel);
                OceanMaterial.SetTexture("_Ocean_Variance", Variance);
                OceanMaterial.SetTexture("_Ocean_Map0", Map0);
                OceanMaterial.SetTexture("_Ocean_Map1", Map1);
                OceanMaterial.SetTexture("_Ocean_Map2", Map2);
                OceanMaterial.SetTexture("_Ocean_Map3", Map3);
                OceanMaterial.SetTexture("_Ocean_Map4", Map4);
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

            if (Map0 != null) Map0.Release();
            if (Map1 != null) Map1.Release();
            if (Map2 != null) Map2.Release();
            if (Map3 != null) Map3.Release();
            if (Map4 != null) Map4.Release();

            if (Spectrum01 != null) Spectrum01.Release();
            if (Spectrum23 != null) Spectrum23.Release();

            if (WTable != null) WTable.Release();
            if (Variance != null) Variance.Release();

            for (var i = 0; i < 2; i++)
            {
                if (FourierBuffer0 != null) if (FourierBuffer0[i] != null) FourierBuffer0[i].Release();
                if (FourierBuffer1 != null) if (FourierBuffer1[i] != null) FourierBuffer1[i].Release();
                if (FourierBuffer2 != null) if (FourierBuffer2[i] != null) FourierBuffer2[i].Release();
                if (FourierBuffer3 != null) if (FourierBuffer3[i] != null) FourierBuffer3[i].Release();
                if (FourierBuffer4 != null) if (FourierBuffer4[i] != null) FourierBuffer4[i].Release();
            }
        }

        #endregion

        #region Events

        protected override void OnAtmosphereBaked(Body celestialBody, Atmosphere atmosphere)
        {
            DrawOcean = true;

            UpdateNode();

            DrawOcean = false;
        }

        #endregion

        #region IRenderable

        public override void Render(int layer = 0)
        {
            base.Render(layer);
        }

        #endregion

        /// <summary>
        /// Initializes the data to the shader that needs to have the fourier transform applied to it this frame.
        /// </summary>
        protected virtual void InitWaveSpectrum()
        {
            // Init heights (0) and slopes (1,2)
            var buffers012 = new RenderTexture[] { FourierBuffer0[1], FourierBuffer1[1], FourierBuffer2[1] };

            RTUtility.MultiTargetBlit(buffers012, InitSpectrumMaterial);

            // Init displacement (3,4)
            var buffers34 = new RenderTexture[] { FourierBuffer3[1], FourierBuffer4[1] };

            InitDisplacementMaterial.SetTexture("_Buffer1", FourierBuffer1[1]);
            InitDisplacementMaterial.SetTexture("_Buffer2", FourierBuffer2[1]);

            RTUtility.MultiTargetBlit(buffers34, InitDisplacementMaterial);
        }

        protected virtual void CreateRenderTextures()
        {
            var mapFormat = RenderTextureFormat.ARGBFloat;
            var format = RenderTextureFormat.ARGBFloat;

            // These texture hold the actual data use in the ocean renderer
            Map0 = RTExtensions.CreateRTexture(FourierGridSize, 0, mapFormat, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, Aniso);
            Map1 = RTExtensions.CreateRTexture(FourierGridSize, 0, mapFormat, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, Aniso);
            Map2 = RTExtensions.CreateRTexture(FourierGridSize, 0, mapFormat, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, Aniso);
            Map3 = RTExtensions.CreateRTexture(FourierGridSize, 0, mapFormat, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, Aniso);
            Map4 = RTExtensions.CreateRTexture(FourierGridSize, 0, mapFormat, FilterMode.Trilinear, TextureWrapMode.Repeat, true, true, Aniso);

            // These textures are used to perform the fourier transform
            CreateBuffer(out FourierBuffer0, format, "FourierHeights"); // heights
            CreateBuffer(out FourierBuffer1, format, "FourierSlopesX"); // slopes X
            CreateBuffer(out FourierBuffer2, format, "FourierSlopesY"); // slopes Y
            CreateBuffer(out FourierBuffer3, format, "FourierDisplacementX"); // displacement X
            CreateBuffer(out FourierBuffer4, format, "FourierDisplacementY"); // displacement Y

            // These textures hold the specturm the fourier transform is performed on
            Spectrum01 = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Point, TextureWrapMode.Repeat);
            Spectrum23 = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Point, TextureWrapMode.Repeat);

            WTable = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Point, TextureWrapMode.Clamp);

            Variance = RTExtensions.CreateRTexture(VarianceSize, 0, RenderTextureFormat.RHalf, FilterMode.Bilinear, TextureWrapMode.Clamp, VarianceSize);
        }

        protected void CreateBuffer(out RenderTexture[] textures, RenderTextureFormat format, string bufferName)
        {
            textures = new RenderTexture[2];

            for (var i = 0; i < 2; i++)
            {
                textures[i] = RTExtensions.CreateRTexture(FourierGridSize, 0, format, FilterMode.Point, TextureWrapMode.Clamp);
                textures[i].SetName(bufferName);
            }
        }

        private float Sqr(float x)
        {
            return x * x;
        }

        private float Omega(float k)
        {
            return Mathf.Sqrt(9.81f * k * (1.0f + Sqr(k / WAVE_KM))); // Eq 24
        }

        private float Spectrum(float kx, float ky, bool omnispectrum)
        {
            // I know this is a big chunk of ugly math but dont worry to much about what it all means
            // It recreates a statistcally representative model of a wave spectrum in the frequency domain.

            var U10 = WindSpeed;

            // phase speed
            var k = Mathf.Sqrt(kx * kx + ky * ky);
            var c = Omega(k) / k;

            // spectral peak
            var kp = 9.81f * Sqr(WavesOmega / U10); // after Eq 3
            var cp = Omega(kp) / kp;

            // Friction velocity
            var z0 = 3.7e-5f * Sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
            var u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60

            var Lpm = Mathf.Exp(-5.0f / 4.0f * Sqr(kp / k)); // after Eq 3
            var gamma = (WavesOmega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(WavesOmega); // after Eq 3 // log10 or log?
            var sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(WavesOmega, 3.0f)); // after Eq 3
            var Gamma = Mathf.Exp(-1.0f / (2.0f * Sqr(sigma)) * Sqr(Mathf.Sqrt(k / kp) - 1.0f));
            var Jp = Mathf.Pow(gamma, Gamma); // Eq 3
            var Fp = Lpm * Jp * Mathf.Exp(-WavesOmega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
            var alphap = 0.006f * Mathf.Sqrt(WavesOmega); // Eq 34
            var Bl = 0.5f * alphap * cp / c * Fp; // Eq 31

            var alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
            var Fm = Mathf.Exp(-0.25f * Sqr(k / WAVE_KM - 1.0f)); // Eq 41
            var Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)

            Bh *= Lpm;

            if (omnispectrum) return AMP * (Bl + Bh) / (k * Sqr(k)); // Eq 30

            var a0 = Mathf.Log(2.0f) / 4.0f;
            var ap = 4.0f;
            var am = 0.13f * u_star / WAVE_CM; // Eq 59
            var Delta = (float)System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57

            var phi = Mathf.Atan2(ky, kx);

            if (kx < 0.0f) return 0.0f;

            Bl *= 2.0f;
            Bh *= 2.0f;

            // Remove waves perpendicular to wind dir
            var tweak = Mathf.Sqrt(Mathf.Max(kx / Mathf.Sqrt(kx * kx + ky * ky), 0.0f));

            return AMP * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * Sqr(Sqr(k))) * tweak; // Eq 67
        }

        private Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin)
        {
            var dk = 2.0f * Mathf.PI / lengthScale;
            var kx = i * dk;
            var ky = j * dk;

            var result = Vector2.zero;
            var rnd = Random.value;

            if (Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin)
            {
                var S = Spectrum(kx, ky, false);
                var h = Mathf.Sqrt(S / 2.0f) * dk;

                var phi = rnd * 2.0f * Mathf.PI;
                result.x = h * Mathf.Cos(phi);
                result.y = h * Mathf.Sin(phi);
            }

            return result;
        }

        private float GetSlopeVariance(float kx, float ky, Vector2 spectrumSample)
        {
            var kSquare = kx * kx + ky * ky;
            var real = spectrumSample.x;
            var img = spectrumSample.y;
            var hSquare = real * real + img * img;

            return kSquare * hSquare * 2.0f;
        }

        private void GenerateWavesSpectrum()
        {
            // Slope variance due to all waves, by integrating over the full spectrum.
            // Used by the BRDF rendering model

            var theoreticSlopeVariance = 0.0f;
            var k = 5e-3f;

            while (k < 1e3f)
            {
                var nextK = k * 1.001f;

                theoreticSlopeVariance += k * k * Spectrum(k, 0, true) * (nextK - k);

                k = nextK;
            }

            var spectrum01 = new float[FourierGridSize * FourierGridSize * 4];
            var spectrum23 = new float[FourierGridSize * FourierGridSize * 4];

            int idx;
            float i;
            float j;
            var totalSlopeVariance = 0.0f;

            var sample12XY = Vector2.zero;
            var sample12ZW = Vector2.zero;
            var sample34XY = Vector2.zero;
            var sample34ZW = Vector2.zero;

            Random.InitState(0);

            for (var x = 0; x < FourierGridSize; x++)
            {
                for (var y = 0; y < FourierGridSize; y++)
                {
                    idx = x + y * FourierGridSize;
                    i = (x >= FourierGridSize / 2) ? (float)(x - FourierGridSize) : (float)x;
                    j = (y >= FourierGridSize / 2) ? (float)(y - FourierGridSize) : (float)y;

                    sample12XY = GetSpectrumSample(i, j, GridSizes.x, Mathf.PI / GridSizes.x);
                    sample12ZW = GetSpectrumSample(i, j, GridSizes.y, Mathf.PI * MapSize / GridSizes.x);
                    sample34XY = GetSpectrumSample(i, j, GridSizes.z, Mathf.PI * MapSize / GridSizes.y);
                    sample34ZW = GetSpectrumSample(i, j, GridSizes.w, Mathf.PI * MapSize / GridSizes.z);

                    spectrum01[idx * 4 + 0] = sample12XY.x;
                    spectrum01[idx * 4 + 1] = sample12XY.y;
                    spectrum01[idx * 4 + 2] = sample12ZW.x;
                    spectrum01[idx * 4 + 3] = sample12ZW.y;

                    spectrum23[idx * 4 + 0] = sample34XY.x;
                    spectrum23[idx * 4 + 1] = sample34XY.y;
                    spectrum23[idx * 4 + 2] = sample34ZW.x;
                    spectrum23[idx * 4 + 3] = sample34ZW.y;

                    i *= 2.0f * Mathf.PI;
                    j *= 2.0f * Mathf.PI;

                    totalSlopeVariance += GetSlopeVariance(i / GridSizes.x, j / GridSizes.x, sample12XY);
                    totalSlopeVariance += GetSlopeVariance(i / GridSizes.y, j / GridSizes.y, sample12ZW);
                    totalSlopeVariance += GetSlopeVariance(i / GridSizes.z, j / GridSizes.z, sample34XY);
                    totalSlopeVariance += GetSlopeVariance(i / GridSizes.w, j / GridSizes.w, sample34ZW);
                }
            }

            //Write floating point data into render texture
            var buffer = new ComputeBuffer(FourierGridSize * FourierGridSize, sizeof(float) * 4);

            buffer.SetData(spectrum01);
            CBUtility.WriteIntoRenderTexture(Spectrum01, CBUtility.Channels.RGBA, buffer, GodManager.Instance.WriteData);

            buffer.SetData(spectrum23);
            CBUtility.WriteIntoRenderTexture(Spectrum23, CBUtility.Channels.RGBA, buffer, GodManager.Instance.WriteData);

            buffer.Release();

            var varianceShader = GodManager.Instance.Variance;

            varianceShader.SetFloat("_SlopeVarianceDelta", 0.5f * (theoreticSlopeVariance - totalSlopeVariance));
            varianceShader.SetFloat("_VarianceSize", (float)VarianceSize);
            varianceShader.SetFloat("_Size", MapSize);
            varianceShader.SetVector("_GridSizes", GridSizes);
            varianceShader.SetTexture(0, "_Spectrum01", Spectrum01);
            varianceShader.SetTexture(0, "_Spectrum23", Spectrum23);
            varianceShader.SetTexture(0, "des", Variance);

            varianceShader.Dispatch(0, VarianceSize / 4, VarianceSize / 4, VarianceSize / 4);

            // Find the maximum value for slope variance

            buffer = new ComputeBuffer(VarianceSize * VarianceSize * VarianceSize, sizeof(float));
            CBUtility.ReadFromRenderTexture(Variance, CBUtility.Channels.R, buffer, GodManager.Instance.ReadData);

            var varianceData = new float[VarianceSize * VarianceSize * VarianceSize];

            buffer.GetData(varianceData);

            MaxSlopeVariance = 0.0f;

            for (var v = 0; v < VarianceSize * VarianceSize * VarianceSize; v++)
            {
                MaxSlopeVariance = Mathf.Max(MaxSlopeVariance, varianceData[v]);
            }

            buffer.Release();
        }

        private void CreateWTable()
        {
            // Some values need for the InitWaveSpectrum function can be precomputed
            var uv = Vector2.zero;
            var st = Vector2.zero;

            float k1, k2, k3, k4, w1, w2, w3, w4;

            var table = new float[FourierGridSize * FourierGridSize * 4];

            for (var x = 0; x < FourierGridSize; x++)
            {
                for (var y = 0; y < FourierGridSize; y++)
                {
                    uv = new Vector2(x, y) / MapSize;

                    st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
                    st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

                    k1 = (st * InverseGridSizes.x).magnitude;
                    k2 = (st * InverseGridSizes.y).magnitude;
                    k3 = (st * InverseGridSizes.z).magnitude;
                    k4 = (st * InverseGridSizes.w).magnitude;

                    w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
                    w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
                    w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
                    w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));

                    table[(x + y * FourierGridSize) * 4 + 0] = w1;
                    table[(x + y * FourierGridSize) * 4 + 1] = w2;
                    table[(x + y * FourierGridSize) * 4 + 2] = w3;
                    table[(x + y * FourierGridSize) * 4 + 3] = w4;

                }
            }

            // Write floating point data into render texture
            var buffer = new ComputeBuffer(FourierGridSize * FourierGridSize, sizeof(float) * 4);

            buffer.SetData(table);
            CBUtility.WriteIntoRenderTexture(WTable, CBUtility.Channels.RGBA, buffer, GodManager.Instance.WriteData);
            buffer.ReleaseAndDisposeBuffer();
        }
    }
}