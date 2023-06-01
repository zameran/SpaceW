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

using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Utilities.Gradients;
using SpaceEngine.Helpers;

using System.Collections.Generic;

using UnityEngine;

public sealed class TCCommonParametersSetter : MonoBehaviour, IUniformed<Material>
{
    public enum TCEngine : byte
    {
        TC_NONE = 0,
        TC_ASTEROID = 1,
        TC_PLANET = 2,
        TC_SELENA = 3,
        TC_TERRA = 4,
        TC_GASGIANT = 5,
        TC_TEST = 6
    }

    public float Lacunarity = 2.218281828459f;
    public float H = 0.5f;
    public float Offset = 0.8f;
    public float RidgeSmooth = 0.0001f;

    public float texturingHeightOffset;
    public float texturingSlopeOffset;

    public Vector3 Randomize;

    public Vector4 faceParams;
    public Vector4 scaleParams;
    public Vector4 mainParams;
    public Vector4 colorParams;
    public Vector4 climateParams;
    public Vector4 mareParams;
    public Vector4 montesParams;
    public Vector4 dunesParams;
    public Vector4 hillsParams;
    public Vector4 canyonsParams;
    public Vector4 riversParams;
    public Vector4 cracksParams;
    public Vector4 craterParams;
    public Vector4 volcanoParams1;
    public Vector4 volcanoParams2;
    public Vector4 lavaParams;
    public Vector4 textureParams;
    public Vector4 cloudsParams1;
    public Vector4 cloudsParams2;
    public Vector4 cycloneParams;

    public Vector2 InvSize;

    public bool AutoUpdate = false;

    public MaterialTableGradientLut MaterialTable = new MaterialTableGradientLut();

    public TCEngine Engine = TCEngine.TC_PLANET;

    private void Awake()
    {
        MaterialTable.GenerateLut();
    }

    private void OnDestroy()
    {
        MaterialTable.DestroyLut();
    }

    public void UpdateMaterialTable<T>(T target) where T : Material
    {
        if (!Application.isPlaying) return;

        if (MaterialTable == null)
        {
            Debug.LogWarning("TCCommonParametersSetter.UpdateMaterialTable: Trying to update lut texture, which is null! So, created...");

            MaterialTable = new MaterialTableGradientLut();

            UpdateMaterialTable<T>(target);
        }
        else
        {
            if (MaterialTable.Lut == null)
            {
                Debug.LogWarning("TCCommonParametersSetter.UpdateMaterialTable: Trying to set lut texture, which is not generated! So, generating...");

                MaterialTable.GenerateLut();
            }

            target.SetTexture("MaterialTable", MaterialTable.Lut);
        }
    }

    public void UpdateMaterialTable(MaterialPropertyBlock target)
    {
        if (!Application.isPlaying) return;

        if (MaterialTable == null)
        {
            Debug.LogWarning("TCCommonParametersSetter.UpdateMaterialTable: Trying to update lut texture, which is null! So, created...");

            MaterialTable = new MaterialTableGradientLut();

            UpdateMaterialTable(target);
        }
        else
        {
            if (MaterialTable.Lut == null)
            {
                Debug.LogWarning("TCCommonParametersSetter.UpdateMaterialTable: Trying to set lut texture, which is not generated! So, generating...");

                MaterialTable.GenerateLut();
            }

            target.SetTexture("MaterialTable", MaterialTable.Lut);
        }
    }

    public void ToggleKeywords<T>(T target) where T : Material
    {
        Helper.SetKeywords(target, new List<string>() { Engine.ToString() }, false);
    }

    public void ClearKeywords<T>(T target) where T : Material
    {
        Helper.SetKeywords(target, new List<string>() { }, false);
    }

    #region IUniformed<Material>

    public void InitUniforms(Material target)
    {
        if (target == null) return;
    }

    public void SetUniforms(MaterialPropertyBlock target)
    {
        if (target == null) return;

        UpdateMaterialTable(target);

        target.SetFloat("noiseLacunarity", Lacunarity);
        target.SetFloat("noiseH", H);
        target.SetFloat("noiseOffset", Offset);
        target.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        target.SetFloat("texturingHeightOffset", texturingHeightOffset);
        target.SetFloat("texturingSlopeOffset", texturingSlopeOffset);

        target.SetVector("Randomize", Randomize);
        target.SetVector("faceParams", faceParams); //(WIP) For SE Coloring in fragment shader work...
        target.SetVector("scaleParams", scaleParams);
        target.SetVector("mainParams", mainParams);
        target.SetVector("colorParams", colorParams);
        target.SetVector("climateParams", climateParams);
        target.SetVector("mareParams", mareParams);
        target.SetVector("montesParams", montesParams);
        target.SetVector("dunesParams", dunesParams);
        target.SetVector("hillsParams", hillsParams);
        target.SetVector("canyonsParams", canyonsParams);
        target.SetVector("riversParams", riversParams);
        target.SetVector("cracksParams", cracksParams);
        target.SetVector("craterParams", craterParams);
        target.SetVector("volcanoParams1", volcanoParams1);
        target.SetVector("volcanoParams2", volcanoParams2);
        target.SetVector("lavaParams", lavaParams);
        target.SetVector("textureParams", textureParams);
        target.SetVector("cloudsParams1", cloudsParams1);
        target.SetVector("cloudsParams2", cloudsParams2);
        target.SetVector("cycloneParams", cycloneParams);

        target.SetVector("InvSize", InvSize);
    }

    public void SetUniforms(Material target)
    {
        if (target == null) return;

        UpdateMaterialTable(target);

        target.SetFloat("noiseLacunarity", Lacunarity);
        target.SetFloat("noiseH", H);
        target.SetFloat("noiseOffset", Offset);
        target.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        target.SetFloat("texturingHeightOffset", texturingHeightOffset);
        target.SetFloat("texturingSlopeOffset", texturingSlopeOffset);

        target.SetVector("Randomize", Randomize);
        target.SetVector("faceParams", faceParams); //(WIP) For SE Coloring in fragment shader work...
        target.SetVector("scaleParams", scaleParams);
        target.SetVector("mainParams", mainParams);
        target.SetVector("colorParams", colorParams);
        target.SetVector("climateParams", climateParams);
        target.SetVector("mareParams", mareParams);
        target.SetVector("montesParams", montesParams);
        target.SetVector("dunesParams", dunesParams);
        target.SetVector("hillsParams", hillsParams);
        target.SetVector("canyonsParams", canyonsParams);
        target.SetVector("riversParams", riversParams);
        target.SetVector("cracksParams", cracksParams);
        target.SetVector("craterParams", craterParams);
        target.SetVector("volcanoParams1", volcanoParams1);
        target.SetVector("volcanoParams2", volcanoParams2);
        target.SetVector("lavaParams", lavaParams);
        target.SetVector("textureParams", textureParams);
        target.SetVector("cloudsParams1", cloudsParams1);
        target.SetVector("cloudsParams2", cloudsParams2);
        target.SetVector("cycloneParams", cycloneParams);

        target.SetVector("InvSize", InvSize);
    }

    #endregion

    #region IUniformed

    public void InitSetUniforms()
    {

    }

    #endregion
}