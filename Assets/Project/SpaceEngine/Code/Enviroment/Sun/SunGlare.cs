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

using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using UnityEngine;

namespace SpaceEngine.AtmosphericScattering.Sun
{
    public sealed class SunGlare : Node<SunGlare>, IUniformed<Material>, IRenderable<SunGlare>
    {
        private readonly CachedComponent<AtmosphereSun> SunCachedComponent = new CachedComponent<AtmosphereSun>();

        public AtmosphereSun SunComponent { get { return SunCachedComponent.Component; } }

        public Atmosphere Atmosphere;

        public Shader SunGlareShader;
        private Material SunGlareMaterial;

        public SunGlareSettings Settings;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
        public int RenderQueueOffset = 1000000; //NOTE : Render over all.

        public float Magnitude = 1;

        public bool InitUniformsInUpdate = true;
        public bool UseAtmosphereColors = false;
        public bool UseRadiance = true;

        private bool Eclipse = false;
        private bool UseTransmittanceOffset = false;

        private Vector3 ViewPortPosition = Vector3.zero;

        private float Scale = 1;
        private float Fade = 1;

        public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                              new Keyframe(1.0f, 1.0f),
                                                                              new Keyframe(10.0f, 0.0f) });

        private Mesh SunGlareMesh;

        private Matrix4x4 Ghost1Settings = Matrix4x4.zero;
        private Matrix4x4 Ghost2Settings = Matrix4x4.zero;
        private Matrix4x4 Ghost3Settings = Matrix4x4.zero;

        #region Node

        protected override void InitNode()
        {
            if (Settings == null) return;

            SunGlareMaterial = MaterialHelper.CreateTemp(SunGlareShader, "Sunglare", (int)RenderQueue);

            SunGlareMesh = MeshFactory.MakePlane(8, MeshFactory.PLANE.XY, false, false, false);
            SunGlareMesh.bounds = new Bounds(Vector4.zero, new Vector3(9e37f, 9e37f, 9e37f));

            for (byte i = 0; i < Settings.Ghost1SettingsList.Count; i++)
                Ghost1Settings.SetRow(i, Settings.Ghost1SettingsList[i]);

            for (byte i = 0; i < Settings.Ghost2SettingsList.Count; i++)
                Ghost2Settings.SetRow(i, Settings.Ghost2SettingsList[i]);

            for (byte i = 0; i < Settings.Ghost3SettingsList.Count; i++)
                Ghost3Settings.SetRow(i, Settings.Ghost3SettingsList[i]);

            InitUniforms(SunGlareMaterial);
        }

        protected override void UpdateNode()
        {
            if (Settings == null) return;

            SunGlareMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            var distance = (CameraHelper.Main().transform.position.normalized - SunComponent.transform.position.normalized).magnitude;

            ViewPortPosition = CameraHelper.Main().WorldToViewportPoint(SunComponent.transform.position);

            // NOTE : So, camera's projection matrix replacement is bad idea in fact of strange clip planes behaviour.
            // Instead i will invert the y component of resulting vector of WorldToViewportPoint.
            // Looks like better idea...
            if (CameraHelper.Main().IsDeferred())
            {
                ViewPortPosition.y = 1.0f - ViewPortPosition.y;
            }

            Scale = distance / Magnitude;
            Fade = FadeCurve.Evaluate(Mathf.Clamp(Scale, 0.0f, 100.0f));
            //Fade = FadeCurve.Evaluate(Mathf.Clamp01(VectorHelper.AngularRadius(SunComponent.transform.position, CameraHelper.Main().transform.position, 250000.0f)));

            //RaycastHit hit;

            Eclipse = false;

            //Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (SunComponent.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);
            //if (!Eclipse)
            //    Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (SunComponent.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

            if (InitUniformsInUpdate) InitUniforms(SunGlareMaterial);

            SetUniforms(SunGlareMaterial);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            SunCachedComponent.TryInit(this);

            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            Helper.Destroy(SunGlareMaterial);
            Helper.Destroy(SunGlareMesh);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed

        public void InitUniforms(Material target)
        {
            if (target == null) return;

            target.SetTexture("sunSpikes", Settings.SunSpikes);
            target.SetTexture("sunFlare", Settings.SunFlare);
            target.SetTexture("sunGhost1", Settings.SunGhost1);
            target.SetTexture("sunGhost2", Settings.SunGhost2);
            target.SetTexture("sunGhost3", Settings.SunGhost3);

            target.SetVector("flareSettings", Settings.FlareSettings);
            target.SetVector("spikesSettings", Settings.SpikesSettings);
            target.SetMatrix("ghost1Settings", Ghost1Settings);
            target.SetMatrix("ghost2Settings", Ghost2Settings);
            target.SetMatrix("ghost3Settings", Ghost2Settings);

            if (Atmosphere != null) Atmosphere.InitUniforms(target);
        }

        public void SetUniforms(Material target)
        {
            if (target == null) return;

            target.SetVector("sunViewPortPos", ViewPortPosition);

            target.SetFloat("AspectRatio", CameraHelper.Main().aspect);
            target.SetFloat("Scale", Scale);
            target.SetFloat("Fade", Fade);
            target.SetFloat("UseAtmosphereColors", UseAtmosphereColors ? 1.0f : 0.0f);
            target.SetFloat("UseRadiance", UseRadiance ? 1.0f : 0.0f);
            target.SetFloat("Eclipse", Eclipse ? 0.0f : 1.0f);

            if (Atmosphere != null)
            {
                // NOTE : Only on these atmospheres we don't gonna use special transmittance uv offset... Magic!
                UseTransmittanceOffset = Atmosphere.AtmosphereBase != AtmosphereBase.Earth &&
                                         Atmosphere.AtmosphereBase != AtmosphereBase.Neptune &&
                                         Atmosphere.AtmosphereBase != AtmosphereBase.Jupiter;

                target.SetFloat("UseTransmittanceOffset", UseTransmittanceOffset ? 1.0f : 0.0f);
            }

            target.renderQueue = (int)RenderQueue + RenderQueueOffset;

            if (Atmosphere != null) Atmosphere.SetUniforms(target);
        }

        public void InitSetUniforms()
        {
            InitUniforms(SunGlareMaterial);
            SetUniforms(SunGlareMaterial);
        }

        #endregion

        #region IRenderable

        public void Render(int layer = 8)
        {
            if (SunGlareMesh == null) return;

            if (ViewPortPosition.z > 0)
            {
                if (Atmosphere == null) return;

                Graphics.DrawMesh(SunGlareMesh, Vector3.zero, Quaternion.identity, SunGlareMaterial, layer, CameraHelper.Main(), 0, Atmosphere.ParentBody.MPB, false, false);
            }
        }

        #endregion
    }
}