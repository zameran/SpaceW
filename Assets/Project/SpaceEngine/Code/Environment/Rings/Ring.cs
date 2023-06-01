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

using SpaceEngine.Core;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Enums;
using SpaceEngine.Environment.Shadows;
using SpaceEngine.Helpers;
using SpaceEngine.Tools;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Environment.Rings
{
    public class Ring : NodeSlave<Ring>, IUniformed<Material>, IReanimateable, IRenderable<Ring>
    {
        public Body ParentBody;

        public List<Light> Lights = new List<Light>();
        public List<Shadow> Shadows = new List<Shadow>();

        public Texture DiffuseTexture;
        public Texture NoiseTexture;

        public float InnerRadius = 1.0f;
        public float OuterRadius = 2.0f;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
        public int RenderQueueOffset;

        public int SegmentCount = 8;
        public int SegmentDetail = 8;
        public int RadiusDetail = 2;

        public float BoundsShift;

        public Color AmbientColor = Color.white;

        public float Thickness = 5000.0f;
        public float FrontBright = 12.8f;
        public float BackBright = 128.0f;
        public float Exposure = 0.2f;
        public float DetailFrequency = 0.01f;
        public float DetailRadialFrequency = 128.0f;
        public float InversedCameraPixelSize = 0.0000001f;
        public float Density = 1.0f;

        public List<RingSegment> Segments = new List<RingSegment>();

        public static List<string> Keywords = new List<string>();

        public Shader RingShader;
        public Material RingMaterial;

        public Mesh RingSegmentMesh;

        #region NodeSlave<Ring>

        public override void InitNode()
        {
            InitMesh();
            InitMaterial();

            InitUniforms(RingMaterial);
        }

        public override void UpdateNode()
        {
            RingMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

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

            SetUniforms(RingMaterial);
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

        #region IUniformed<Material>

        public void InitUniforms(Material target)
        {
            if (target == null) return;

            target.SetTexture("DiffuseMap", DiffuseTexture);
            target.SetTexture("NoiseMap", NoiseTexture);
        }

        public void SetUniforms(Material target)
        {
            if (target == null) return;

            SetLightsAndShadows(target);

            target.SetVector("RingsParams", new Vector2((OuterRadius - InnerRadius), 1.0f / Thickness));

            target.SetColor("AmbientColor", AmbientColor);

            target.SetVector("LightingParams", new Vector3(FrontBright, BackBright, Exposure));
            target.SetVector("DetailParams", new Vector4(DetailFrequency, DetailRadialFrequency, InversedCameraPixelSize, Density));

            Keywords.Clear();
        }

        public void InitSetUniforms()
        {
            InitUniforms(RingMaterial);
            SetUniforms(RingMaterial);
        }

        #endregion

        #region IReanimateable

        public void Reanimate()
        {
            InitUniforms(RingMaterial);
        }

        #endregion

        #region IRenderable

        public void Render(int layer = 11)
        {
            if (Segments == null) return;
            if (Segments.Count == 0) return;

            for (var i = 0; i < Segments.Count; i++)
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

            WriteLightKeywords(lightCount, Keywords);
            WriteShadowKeywords(shadowCount, Keywords);

            Helper.SetKeywords(mat, Keywords);
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
            RingSegmentMesh = MeshFactory.SetupRingSegmentMesh(SegmentCount, SegmentDetail, RadiusDetail, InnerRadius, OuterRadius, BoundsShift);
        }

        #region Special Stuff

        public static void WriteLightKeywords(int lightCount, params List<string>[] keywordLists)
        {
            if (lightCount > 0)
            {
                var keyword = $"LIGHT_{lightCount}";

                for (var i = keywordLists.Length - 1; i >= 0; i--)
                {
                    var keywordList = keywordLists[i];

                    keywordList?.Add(keyword);
                }
            }
        }

        public static void WriteShadowKeywords(int shadowCount, params List<string>[] keywordLists)
        {
            if (shadowCount > 0)
            {
                var keyword = $"SHADOW_{shadowCount}";

                for (var i = keywordLists.Length - 1; i >= 0; i--)
                {
                    var keywordList = keywordLists[i];

                    keywordList?.Add(keyword);
                }
            }
        }

        #endregion
    }
}