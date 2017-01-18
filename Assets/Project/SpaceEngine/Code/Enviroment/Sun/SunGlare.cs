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

using System.Collections.Generic;
using UnityEngine;

namespace SpaceEngine.AtmosphericScattering.Sun
{
    public sealed class SunGlare : Node<SunGlare>
    {
        public Atmosphere Atmosphere;
        public AtmosphereSun Sun;

        public Shader SunGlareShader;
        private Material SunGlareMaterial;

        public Texture2D SunSpikes;
        public Texture2D SunFlare;
        public Texture2D SunGhost1;
        public Texture2D SunGhost2;
        public Texture2D SunGhost3;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
        public int RenderQueueOffset = 1000;

        public float Magnitude = 1;

        public bool InitUniformsInUpdate = true;

        private bool Eclipse = false;

        private Vector3 ViewPortPosition = Vector3.zero;

        private float Scale = 1;
        private float Fade = 1;

        public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                              new Keyframe(1.0f, 1.0f),
                                                                              new Keyframe(10.0f, 0.0f) });

        private Mesh SunGlareMesh;

        public Vector3 FlareSettings = new Vector3(0.45f, 1.0f, 0.85f);
        public Vector3 SpikesSettings = new Vector3(0.6f, 1.0f, 1.0f);

        public List<Vector4> Ghost1SettingsList = new List<Vector4>
        {
            new Vector4(0.54f, 0.65f, 2.3f, 0.5f),
            new Vector4(0.54f, 1.0f, 6.0f, 0.7f)
        };

        public List<Vector4> Ghost2SettingsList = new List<Vector4>
        {
            new Vector4(0.135f, 1.0f, 3.0f, 0.9f),
            new Vector4(0.054f, 1.0f, 8.0f, 1.1f),
            new Vector4(0.054f, 1.0f, 4.0f, 1.3f),
            new Vector4(0.054f, 1.0f, 5.0f, 1.5f)
        };

        public List<Vector4> Ghost3SettingsList = new List<Vector4>
        {
            new Vector4(0.135f, 1.0f, 3.0f, 0.9f),
            new Vector4(0.054f, 1.0f, 8.0f, 1.1f),
            new Vector4(0.054f, 1.0f, 4.0f, 1.3f),
            new Vector4(0.054f, 1.0f, 5.0f, 1.5f)
        };

        private Matrix4x4 Ghost1Settings = Matrix4x4.zero;
        private Matrix4x4 Ghost2Settings = Matrix4x4.zero;
        private Matrix4x4 Ghost3Settings = Matrix4x4.zero;

        #region Node

        protected override void InitNode()
        {
            if (Sun == null)
                if (GetComponent<AtmosphereSun>() != null)
                    Sun = GetComponent<AtmosphereSun>();

            SunGlareMaterial = MaterialHelper.CreateTemp(SunGlareShader, "Sunglare", (int)RenderQueue);

            SunGlareMesh = MeshFactory.MakePlane(8, 8, MeshFactory.PLANE.XY, false, false, false);
            SunGlareMesh.bounds = new Bounds(Vector4.zero, new Vector3(9e37f, 9e37f, 9e37f));

            for (int i = 0; i < Ghost1SettingsList.Count; i++)
                Ghost1Settings.SetRow(i, Ghost1SettingsList[i]);

            for (int i = 0; i < Ghost2SettingsList.Count; i++)
                Ghost2Settings.SetRow(i, Ghost2SettingsList[i]);

            for (int i = 0; i < Ghost3SettingsList.Count; i++)
                Ghost3Settings.SetRow(i, Ghost3SettingsList[i]);

            InitUniforms(SunGlareMaterial);
        }

        protected override void UpdateNode()
        {
            if (Sun == null) return;

            SunGlareMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            var distance = (CameraHelper.Main().transform.position.normalized - Sun.transform.position.normalized).magnitude;

            CameraHelper.WithReplacedProjection(() =>
            {
                ViewPortPosition = CameraHelper.Main().WorldToViewportPoint(Sun.transform.position);
            });

            Scale = distance / Magnitude;
            Fade = FadeCurve.Evaluate(Mathf.Clamp(Scale, 0.0f, 100.0f));
            //Fade = FadeCurve.Evaluate(Mathf.Clamp01(VectorHelper.AngularRadius(Sun.transform.position, CameraHelper.Main().transform.position, 250000.0f)));

            //RaycastHit hit;

            Eclipse = false;

            //Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);
            //if (!Eclipse)
            //    Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

            if (InitUniformsInUpdate) InitUniforms(SunGlareMaterial);

            SetUniforms(SunGlareMaterial);
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            if (ViewPortPosition.z > 0)
            {
                if (Atmosphere == null) return;

                Graphics.DrawMesh(SunGlareMesh, Vector3.zero, Quaternion.identity, SunGlareMaterial, 10, CameraHelper.Main(), 0, Atmosphere.planetoid.QuadAtmosphereMPB, false, false);
            }

            base.Update();
        }

        #endregion

        public void InitSetAtmosphereUniforms()
        {
            InitUniforms(SunGlareMaterial);
            SetUniforms(SunGlareMaterial);
        }

        public void InitUniforms(Material mat)
        {
            if (mat == null) return;

            SunGlareMaterial.SetTexture("sunSpikes", SunSpikes);
            SunGlareMaterial.SetTexture("sunFlare", SunFlare);
            SunGlareMaterial.SetTexture("sunGhost1", SunGhost1);
            SunGlareMaterial.SetTexture("sunGhost2", SunGhost2);
            SunGlareMaterial.SetTexture("sunGhost3", SunGhost3);

            SunGlareMaterial.SetVector("flareSettings", FlareSettings);
            SunGlareMaterial.SetVector("spikesSettings", SpikesSettings);
            SunGlareMaterial.SetMatrix("ghost1Settings", Ghost1Settings);
            SunGlareMaterial.SetMatrix("ghost2Settings", Ghost2Settings);
            SunGlareMaterial.SetMatrix("ghost3Settings", Ghost2Settings);

            if (Atmosphere != null) Atmosphere.InitUniforms(null, SunGlareMaterial, false);
        }

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            SunGlareMaterial.SetVector("sunViewPortPos", ViewPortPosition);

            SunGlareMaterial.SetFloat("AspectRatio", CameraHelper.Main().aspect);
            SunGlareMaterial.SetFloat("Scale", Scale);
            SunGlareMaterial.SetFloat("Fade", Fade);
            SunGlareMaterial.SetFloat("UseAtmosphereColors", 1.0f);
            SunGlareMaterial.SetFloat("Eclipse", Eclipse ? 1.0f : 0.0f);

            SunGlareMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            if (Atmosphere != null) { Atmosphere.SetUniforms(null, SunGlareMaterial, false, false); }
        }
    }
}