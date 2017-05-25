#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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
// Creation Date: 2016.05.16
// Creation Time: 12:30
// Creator: zameran
#endregion

using SpaceEngine.Core.Debugging;

using UnityEngine;

namespace SpaceEngine.Tests
{
    [ExecuteInEditMode]
    public sealed class NoiseEngineParametersTest : MonoBehaviour
    {
        public class Uniforms
        {
            public int PermSampler;
            public int PermGradSampler;
            public int AtlasDiffSampler;
            public int MaterialTable;
            public int ColorMap;

            public int noiseLacunarity;
            public int noiseH;
            public int noiseOffset;
            public int noiseRidgeSmooth;

            public Uniforms()
            {
                PermSampler = Shader.PropertyToID("PermSampler");
                PermGradSampler = Shader.PropertyToID("PermGradSampler");
                AtlasDiffSampler = Shader.PropertyToID("AtlasDiffSampler");
                MaterialTable = Shader.PropertyToID("MaterialTable");
                ColorMap = Shader.PropertyToID("ColorMap");

                noiseLacunarity = Shader.PropertyToID("noiseLacunarity");
                noiseH = Shader.PropertyToID("noiseH");
                noiseOffset = Shader.PropertyToID("noiseOffset");
                noiseRidgeSmooth = Shader.PropertyToID("noiseRidgeSmooth");
            }
        }

        public float Lacunarity = 2.218281828459f;
        public float H = 0.5f;
        public float Offset = 0.8f;
        public float RidgeSmooth = 0.0001f;

        public Texture2D PermSampler = null;
        public Texture2D PermGradSampler = null;
        public Texture2D PlanetAtlas = null;
        public Texture2D PlanetColor = null;
        public Texture2D PlanetColorMap = null;

        public Material material;

        public Uniforms uniforms;

        private void Awake()
        {
            uniforms = new Uniforms();
        }

        private void Update()
        {
            using (new PerformanceMonitor.Timer("NoiseEngineParametersTest.Update.UpdateUniforms()"))
            {
                UpdateUniforms(material);
            }

            using (new PerformanceMonitor.Timer("NoiseEngineParametersTest.Update.UpdateUniformsWithIDs()"))
            {
                UpdateUniformsWithIDs(material, uniforms);
            }
        }

        private void OnDestroy()
        {
            //Helper.Destroy(material);
        }

        public void UpdateUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetTexture("PermSampler", PermSampler);
            mat.SetTexture("PermGradSampler", PermGradSampler);
            mat.SetTexture("AtlasDiffSampler", PlanetAtlas);
            mat.SetTexture("MaterialTable", PlanetColor);
            mat.SetTexture("ColorMap", PlanetColorMap);

            mat.SetFloat("noiseLacunarity", Lacunarity);
            mat.SetFloat("noiseH", H);
            mat.SetFloat("noiseOffset", Offset);
            mat.SetFloat("noiseRidgeSmooth", RidgeSmooth);

            mat.SetPass(0);
        }

        public void UpdateUniformsWithIDs(Material mat, Uniforms u)
        {
            if (mat == null || u == null) return;

            mat.SetTexture(u.PermSampler, PermSampler);
            mat.SetTexture(u.PermGradSampler, PermGradSampler);
            mat.SetTexture(u.AtlasDiffSampler, PlanetAtlas);
            mat.SetTexture(u.MaterialTable, PlanetColor);
            mat.SetTexture(u.ColorMap, PlanetColorMap);

            mat.SetFloat(u.noiseLacunarity, Lacunarity);
            mat.SetFloat(u.noiseH, H);
            mat.SetFloat(u.noiseOffset, Offset);
            mat.SetFloat(u.noiseRidgeSmooth, RidgeSmooth);

            mat.SetPass(0);
        }
    }
}