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

using SpaceEngine.Core.Debugging;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

[ExecutionOrder(-9997)]
[UseLogger(LoggerCategory.Core)]
public class UniformsManager : MonoSingleton<UniformsManager>, IUniformed, IUniformed<Material>, IUniformed<ComputeShader>
{
    public Texture2D PermSampler = null;
    public Texture2D PermGradSampler = null;
    public Texture2D PermSamplerGL = null;
    public Texture2D PermGradSamplerGL = null;
    public Texture2D PlanetAtlas = null;
    public Texture2D PlanetNoiseMap = null;
    public Texture2D PlanetColorMap = null;

    public Texture2D QuadTexture1 = null;
    public Texture2D QuadTexture2 = null;
    public Texture2D QuadTexture3 = null;
    public Texture2D QuadTexture4 = null;

    public bool OverrideDefaultNoiseParameters = false;

    #region IUniformed

    public void InitUniforms()
    {
        Shader.SetGlobalTexture("PermSampler", PermSampler);
        Shader.SetGlobalTexture("PermGradSampler", PermGradSampler);
        Shader.SetGlobalTexture("PermSamplerGL", PermSamplerGL);
        Shader.SetGlobalTexture("PermGradSamplerGL", PermSamplerGL);

        Shader.SetGlobalTexture("AtlasDiffSampler", PlanetAtlas);
        Shader.SetGlobalTexture("PlanetColorMap", PlanetColorMap);

        Shader.SetGlobalTexture("_PlanetNoiseMap", PlanetNoiseMap);

        Shader.SetGlobalTexture("_QuadTexture1", QuadTexture1);
        Shader.SetGlobalTexture("_QuadTexture2", QuadTexture2);
        Shader.SetGlobalTexture("_QuadTexture3", QuadTexture3);
        Shader.SetGlobalTexture("_QuadTexture4", QuadTexture4);

        if (OverrideDefaultNoiseParameters)
        {
            Shader.SetGlobalFloat("noiseOctaves", 4);
            Shader.SetGlobalFloat("noiseLacunarity", 2.218281828459f);
            Shader.SetGlobalFloat("noiseH", 0.5f);
            Shader.SetGlobalFloat("noiseOffset", 0.8f);
            Shader.SetGlobalFloat("noiseRidgeSmooth", 0.0001f);
        }
    }

    public void SetUniforms()
    {
        Shader.SetGlobalFloat("_RealTime", Time.realtimeSinceStartup);

        if (GodManager.Instance.View == null) return;

        Shader.SetGlobalMatrix("_Globals_WorldToCamera", GodManager.Instance.View.WorldToCameraMatrix.ToMatrix4x4());
        Shader.SetGlobalMatrix("_Globals_CameraToWorld", GodManager.Instance.View.CameraToWorldMatrix.ToMatrix4x4());
        Shader.SetGlobalMatrix("_Globals_CameraToScreen", GodManager.Instance.View.CameraToScreenMatrix.ToMatrix4x4());
        Shader.SetGlobalMatrix("_Globals_ScreenToCamera", GodManager.Instance.View.ScreenToCameraMatrix.ToMatrix4x4());
        Shader.SetGlobalVector("_Globals_WorldCameraPos", GodManager.Instance.View.WorldCameraPosition);
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
        target.SetTexture("PlanetColorMap", PlanetColorMap);

        target.SetTexture("_PlanetNoiseMap", PlanetNoiseMap);

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
        if (kernels == null || kernels.Length == 0) { Debug.Log("UniformsManager: Kernels array problem!"); return; }

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
        target.SetTexture(kernel, "PlanetColorMap", PlanetColorMap);
    }

    #endregion

    #region IUniformed

    public void InitSetUniforms()
    {

    }

    #endregion

    private void Awake()
    {
        Instance = this;

        LoadAndInit();

        InitUniforms();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.H)) InitUniforms();

        SetUniforms();
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
        if (PlanetColorMap == null) PlanetColorMap = LoadTextureFromResources("PlanetColorHumanityToTemp");
        if (PlanetNoiseMap == null) PlanetNoiseMap = LoadTextureFromResources("Terrain/DetailNormalmap");
    }

    public Texture2D LoadTextureFromResources(string textureName)
    {
        var path = "Textures/" + textureName;
        var textureResource = Resources.Load(path, typeof(Texture2D)) as Texture2D;

        if (textureResource == null) { Logger.Log(string.Format("UniformsManager.LoadTextureFromResources: Failed to load texture from {0}", path)); }

        return textureResource;
    }
}