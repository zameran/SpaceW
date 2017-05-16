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

using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using UnityEngine;

// TODO : Special 'framework' for a global uniforms.
// TODO : PlanetColor move to body. Generate this lut from a gradient.

public class UniformsManager : MonoSingleton<UniformsManager>, IUniformed, IUniformed<Material>, IUniformed<ComputeShader>
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

    #region IUniformed

    public void InitUniforms()
    {

    }

    public void SetUniforms()
    {
        Shader.SetGlobalTexture("PermSampler", PermSampler);
        Shader.SetGlobalTexture("PermGradSampler", PermGradSampler);
        Shader.SetGlobalTexture("PermSamplerGL", PermSamplerGL);
        Shader.SetGlobalTexture("PermGradSamplerGL", PermSamplerGL);

        Shader.SetGlobalTexture("AtlasDiffSampler", PlanetAtlas);
        Shader.SetGlobalTexture("MaterialTable", PlanetColor);
        Shader.SetGlobalTexture("ColorMap", PlanetColorMap);

        Shader.SetGlobalTexture("_PlanetUVSampler", PlanetUVSampler);

        Shader.SetGlobalTexture("_QuadTexture1", QuadTexture1);
        Shader.SetGlobalTexture("_QuadTexture2", QuadTexture2);
        Shader.SetGlobalTexture("_QuadTexture3", QuadTexture3);
        Shader.SetGlobalTexture("_QuadTexture4", QuadTexture4);
    }

    #endregion

    #region IUniformed<Material>

    public void InitUniforms(Material target)
    {
        if (target == null) return;
    }

    public void SetUniforms(Material target)
    {
        if (target == null) return;

        target.SetTexture("PermSampler", PermSampler);
        target.SetTexture("PermGradSampler", PermGradSampler);
        target.SetTexture("PermSamplerGL", PermSamplerGL);
        target.SetTexture("PermGradSamplerGL", PermSamplerGL);

        target.SetTexture("AtlasDiffSampler", PlanetAtlas);
        target.SetTexture("MaterialTable", PlanetColor);
        target.SetTexture("ColorMap", PlanetColorMap);

        target.SetTexture("_PlanetUVSampler", PlanetUVSampler);

        target.SetTexture("_QuadTexture1", QuadTexture1);
        target.SetTexture("_QuadTexture2", QuadTexture2);
        target.SetTexture("_QuadTexture3", QuadTexture3);
        target.SetTexture("_QuadTexture4", QuadTexture4);
    }

    #endregion

    #region IUniformed<ComputeShader>

    public void InitUniforms(ComputeShader target)
    {
        if (target == null) return;
    }

    public void SetUniforms(ComputeShader target)
    {
        if (target == null) return;
    }

    public void SetUniforms(ComputeShader target, params int[] kernels)
    {
        if (target == null) return;
        if (kernels == null || kernels.Length == 0) { Debug.Log("Quad: SetupComputeShaderKernelsUniforfms(...) problem!"); return; }

        for (int i = 0; i < kernels.Length; i++)
        {
            SetUniforms(target, i);
        }
    }

    public void SetUniforms(ComputeShader target, int kernel)
    {
        if (target == null) return;

        target.SetTexture(kernel, "PermSampler", PermSampler);
        target.SetTexture(kernel, "PermGradSampler", PermGradSampler);
        target.SetTexture(kernel, "PermSamplerGL", PermSamplerGL);
        target.SetTexture(kernel, "PermGradSamplerGL", PermSamplerGL);

        target.SetTexture(kernel, "AtlasDiffSampler", PlanetAtlas);
        target.SetTexture(kernel, "MaterialTable", PlanetColor);
        target.SetTexture(kernel, "ColorMap", PlanetColorMap);
    }

    #endregion

    #region IUniformed

    public void InitSetUniforms()
    {

    }

    #endregion

    private void Awake()
    {
        LoadAndInit();

        SetUniforms();
    }

    private void Update()
    {

    }

    public void UpdateUniforms(Material mat)
    {
        SetUniforms(mat);
    }

    public void UpdateUniforms(ComputeShader cs, int kernel)
    {
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
        return Resources.Load("Textures/" + name, typeof(Texture2D)) as Texture2D;
    }
}