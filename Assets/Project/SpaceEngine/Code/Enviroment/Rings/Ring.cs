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

using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using System.Collections.Generic;

using UnityEngine;

using ZFramework.Unity.Common.PerfomanceMonitor;

public class Ring : Node<Ring>, IUniformed<Material>, IRenderable<Ring>
{
    public CelestialBody body;

    public List<Light> Lights = new List<Light>();
    public List<Shadow> Shadows = new List<Shadow>();

    public Texture MainTex;
    public Texture NoiseTex;

    public Color Color = Color.white;

    public float Brightness = 1.0f;
    public float InnerRadius = 1.0f;
    public float OuterRadius = 2.0f;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset;

    public int SegmentCount = 8;
    public int SegmentDetail = 8;

    public float BoundsShift;

    [Range(-1.0f, 1.0f)]
    public float LightingBias = 0.5f;

    [Range(0.0f, 1.0f)]
    public float LightingSharpness = 0.5f;

    [Range(0.0f, 5.0f)]
    public float MieSharpness = 2.0f;

    [Range(0.0f, 10.0f)]
    public float MieStrength = 1.0f;

    public List<RingSegment> Segments = new List<RingSegment>();

    public static List<string> keywords = new List<string>();

    public Shader RingShader;
    public Material RingMaterial;

    public Mesh RingSegmentMesh;

    #region Node

    protected override void InitNode()
    {
        InitMesh();
        InitMaterial();

        InitUniforms(RingMaterial);
    }

    protected override void UpdateNode()
    {
        RingMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

        using (new Timer("Ring.UpdateNode()"))
        {
            Segments.RemoveAll(m => m == null);

            if (SegmentCount != Segments.Count)
            {
                Helper.ResizeArrayTo(ref Segments, SegmentCount, i => RingSegment.Create(this), null);
            }

            var angleStep = Helper.Divide(360.0f, SegmentCount);

            for (var i = SegmentCount - 1; i >= 0; i--)
            {
                var angle = angleStep * i;
                var rotation = Quaternion.Euler(0.0f, angle, 0.0f);

                Segments[i].UpdateNode(RingSegmentMesh, RingMaterial, rotation);
            }
        }

        SetUniforms(RingMaterial);
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
        Helper.Destroy(RingMaterial);
        Helper.Destroy(RingSegmentMesh);

        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            Helper.Destroy(Segments[i]);
        }

        Segments.Clear();

        base.OnDestroy();
    }

    #endregion

    #region IUniformed

    public void InitUniforms(Material target)
    {
        if (target == null) return;
    }

    public void SetUniforms(Material target)
    {
        if (target == null) return;

        SetLightsAndShadows(target);

        target.SetTexture("_DiffuseTexture", MainTex);
        target.SetTexture("_NoiseTex", NoiseTex);
        target.SetColor("_DiffuseColor", Helper.Brighten(Color, Brightness));
        target.SetFloat("_LightingBias", LightingBias);
        target.SetFloat("_LightingSharpness", LightingSharpness);

        WriteMie(MieSharpness, MieStrength, target);

        keywords.Clear();
    }

    public void InitSetUniforms()
    {
        InitUniforms(RingMaterial);
        SetUniforms(RingMaterial);
    }

    #endregion

    #region IRenderable

    public void Render(int layer = 0)
    {
        if (Segments == null) return;
        if (Segments.Count == 0) return;

        for (int i = 0; i < Segments.Count; i++)
        {
            if (Segments[i] != null)
            {
                Segments[i].Render(layer);
            }
        }
    }

    #endregion

    private void OnEnable()
    {
        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            var segment = Segments[i];

            if (segment != null)
            {
                segment.gameObject.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            var segment = Segments[i];

            if (segment != null)
            {
                segment.gameObject.SetActive(false);
            }
        }
    }

    #region Gizmos

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Helper.DrawCircle(Vector3.zero, Vector3.up, InnerRadius);
        Helper.DrawCircle(Vector3.zero, Vector3.up, OuterRadius);
    }
#endif

    #endregion

    public void SetLightsAndShadows(Material mat)
    {
        if (mat == null) return;

        var lightCount = Helper.WriteLights(Lights, 4, transform.position, null, null, mat);
        var shadowCount = Helper.WriteShadows(Shadows, 4, mat);

        WriteLightKeywords(lightCount, keywords);
        WriteShadowKeywords(shadowCount, keywords);

        keywords.Add("SCATTERING");

        Helper.SetKeywords(mat, keywords);
    }

    public void SetShadows(Material mat, List<Shadow> shadows)
    {
        if (mat == null) return;

        Helper.WriteShadows(shadows, 4, mat);
    }

    public void SetShadows(MaterialPropertyBlock block, List<Shadow> shadows)
    {
        if (block == null) return;

        Helper.WriteShadows(shadows, 4, block);
    }

    private void InitMaterial()
    {
        RingMaterial = MaterialHelper.CreateTemp(RingShader, "Ring", (int)RenderQueue);
    }

    private void InitMesh()
    {
        RingSegmentMesh = MeshFactory.SetupRingSegmentMesh(SegmentCount, SegmentDetail, InnerRadius, OuterRadius, BoundsShift);
    }

    #region Special Stuff

    public static void WriteLightKeywords(int lightCount, params List<string>[] keywordLists)
    {
        if (lightCount > 0)
        {
            var keyword = "LIGHT_" + lightCount;

            for (var i = keywordLists.Length - 1; i >= 0; i--)
            {
                var keywordList = keywordLists[i];

                if (keywordList != null)
                {
                    keywordList.Add(keyword);
                }
            }
        }
    }

    public static void WriteShadowKeywords(int shadowCount, params List<string>[] keywordLists)
    {
        if (shadowCount > 0)
        {
            var keyword = "SHADOW_" + shadowCount;

            for (var i = keywordLists.Length - 1; i >= 0; i--)
            {
                var keywordList = keywordLists[i];

                if (keywordList != null)
                {
                    keywordList.Add(keyword);
                }
            }
        }
    }

    public static void WriteMie(float sharpness, float strength, params Material[] materials)
    {
        sharpness = Mathf.Pow(10.0f, sharpness);
        strength *= (Mathf.Log10(sharpness) + 1) * 0.75f;

        //var mie  = -(1.0f - 1.0f / Mathf.Pow(10.0f, sharpness));
        //var mie4 = new Vector4(mie * 2.0f, 1.0f - mie * mie, 1.0f + mie * mie, mie / strength);

        var mie = -(1.0f - 1.0f / sharpness);
        var mie4 = new Vector4(mie * 2.0f, 1.0f - mie * mie, 1.0f + mie * mie, strength);

        for (var j = materials.Length - 1; j >= 0; j--)
        {
            var material = materials[j];

            if (material != null)
            {
                material.SetVector("_Mie", mie4);
            }
        }
    }

    #endregion
}