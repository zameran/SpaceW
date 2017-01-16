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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using UnityEngine;

public sealed class NoiseParametersSetter : MonoBehaviour
{
    public Texture2D PermSampler = null;
    public Texture2D PermGradSampler = null;
    public Texture2D PermSamplerGL = null;
    public Texture2D PermGradSamplerGL = null;
    public Texture2D PlanetAtlas = null;
    public Texture2D PlanetColor = null;
    public Texture2D PlanetColorMap = null;
    public Texture2D PlanetUVSampler = null;

    public Texture2D QuadTexture1 = null;
    public Texture2D QuadTexture2 = null;
    public Texture2D QuadTexture3 = null;
    public Texture2D QuadTexture4 = null;

    private void Awake()
    {
        LoadAndInit();
    }

    private void Update()
    {

    }

    private void OnDestroy()
    {

    }

    public void UpdateUniforms(Material mat)
    {
        SetUniforms(mat);
    }

    public void UpdateUniforms(Material mat, ComputeShader cs)
    {
        SetUniforms(mat);
        SetUniforms(cs);
    }

    public void UpdateUniforms(Material mat, ComputeShader cs, int kernel)
    {
        SetUniforms(mat);
        SetUniforms(cs, kernel);
    }

    public void LoadAndInit()
    {
        if (PermSampler == null) PermSampler = LoadTextureFromResources("Noise/PerlinPerm2D");
        if (PermGradSampler == null) PermGradSampler = LoadTextureFromResources("Noise/PerlinGrad2D");
        if (PermSamplerGL == null) PermSamplerGL = LoadTextureFromResources("Noise/PerlinPerm2D_GL");
        if (PermGradSamplerGL == null) PermGradSamplerGL = LoadTextureFromResources("Noise/PerlinGrad2D_GL");
        if (PlanetAtlas == null) PlanetAtlas = LoadTextureFromResources("PlanetAtlas");
        if (PlanetColor == null) PlanetColor = LoadTextureFromResources("PlanetColorHeightGradient");
        if (PlanetColorMap == null) PlanetColorMap = LoadTextureFromResources("PlanetColorHumanityToTemp");
    }

    public Texture2D LoadTextureFromResources(string name)
    {
        Texture2D temp = Resources.Load("Textures/" + name, typeof(Texture2D)) as Texture2D;

        return temp;
    }

    private void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetTexture("PermSampler", PermSampler);
        mat.SetTexture("PermGradSampler", PermGradSampler);
        mat.SetTexture("PermSamplerGL", PermSamplerGL);
        mat.SetTexture("PermGradSamplerGL", PermSamplerGL);

        mat.SetTexture("AtlasDiffSampler", PlanetAtlas);
        mat.SetTexture("MaterialTable", PlanetColor);
        mat.SetTexture("ColorMap", PlanetColorMap);

        mat.SetTexture("_PlanetUVSampler", PlanetUVSampler);

        mat.SetTexture("_QuadTexture1", QuadTexture1);
        mat.SetTexture("_QuadTexture2", QuadTexture2);
        mat.SetTexture("_QuadTexture3", QuadTexture3);
        mat.SetTexture("_QuadTexture4", QuadTexture4);
    }

    private void SetUniforms(ComputeShader shader)
    {
        if (shader == null) return;

        shader.SetTexture(0, "PermSampler", PermSampler);
        shader.SetTexture(0, "PermGradSampler", PermGradSampler);
        shader.SetTexture(0, "PermSamplerGL", PermSamplerGL);
        shader.SetTexture(0, "PermGradSamplerGL", PermSamplerGL);

        shader.SetTexture(0, "AtlasDiffSampler", PlanetAtlas);
        shader.SetTexture(0, "MaterialTable", PlanetColor);
        shader.SetTexture(0, "ColorMap", PlanetColorMap);
    }

    private void SetUniforms(ComputeShader shader, int kernel)
    {
        if (shader == null) return;

        shader.SetTexture(kernel, "PermSampler", PermSampler);
        shader.SetTexture(kernel, "PermGradSampler", PermGradSampler);
        shader.SetTexture(kernel, "PermSamplerGL", PermSamplerGL);
        shader.SetTexture(kernel, "PermGradSamplerGL", PermSamplerGL);

        shader.SetTexture(kernel, "AtlasDiffSampler", PlanetAtlas);
        shader.SetTexture(kernel, "MaterialTable", PlanetColor);
        shader.SetTexture(kernel, "ColorMap", PlanetColorMap);
    }
}