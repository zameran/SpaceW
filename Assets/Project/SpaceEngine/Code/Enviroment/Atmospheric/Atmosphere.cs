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

using SpaceEngine.Core;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.PropertyNotification;
using SpaceEngine.Core.Patterns.Strategy.Eventit;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Preprocess.Atmospehre;

using System.ComponentModel;

using UnityEngine;

namespace SpaceEngine.Enviroment.Atmospheric
{
    public sealed class AtmosphereBaseProperty : PropertyNotificationObject
    {
        private AtmosphereBase _value = AtmosphereBase.Earth;

        public AtmosphereBase Value
        {
            get
            {
                return _value;
            }
            set
            {
                var to = value;
                var from = _value;

                if (from != to)
                {
                    _value = value;

                    OnPropertyChanged(string.Format("AtmosphereBase_{0}_{1}", from, to));
                }
                else
                {
                    return;
                }
            }
        }
    }

    public sealed class Atmosphere : NodeSlave<Atmosphere>, IEventit, IUniformed<MaterialPropertyBlock>, IReanimateable, IRenderable<Atmosphere>
    {
        private readonly AtmosphereBaseProperty AtmosphereBaseProperty = new AtmosphereBaseProperty();

        public AtmosphereBase AtmosphereBase { get { return AtmosphereBaseProperty.Value; } set { AtmosphereBaseProperty.Value = value; } }

        public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                              new Keyframe(0.25f, 1.0f),
                                                                              new Keyframe(1.0f, 1.0f) });

        public Body ParentBody;

        public Color GlowColor = Color.black;

        [Range(0.0f, 1.0f)]
        public float Density = 1.0f;

        [Tooltip("1/3 or 1/2 from Planet.TerrainMaxHeight")]
        public float RadiusHold = 0.0f;
        public float Height = 100.0f;
        public float Scale = 1.0f;
        public float Fade = 1.0f;
        public float AerialPerspectiveOffset = 2000.0f;
        public float HorizonFixEps = 0.004f;
        public float MieFadeFix = 0.02f;

        [Range(0.000025f, 0.1f)]
        public float ExtinctionGroundFade = 0.000025f;

        public float HDRExposure = 0.2f;

        public Shader SkyShader;
        public Material SkyMaterial;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
        public int RenderQueueOffset = 0;

        public Mesh AtmosphereMesh { get { return GodManager.Instance.AtmosphereMesh; } }

        public bool LostFocusForceRebake = false;

        private AtmosphereParameters AtmosphereParameters;

        public PreProcessAtmosphere AtmosphereBaker = null;

        public Vector3 Origin { get { return ParentBody != null ? ParentBody.transform.position : Vector3.zero; } }
        public float Radius { get { return ParentBody != null ? ParentBody.Size : 0.0f; } }

        #region Eventit

        public bool isEventit { get; set; }

        public void Eventit()
        {
            if (isEventit) return;

            AtmosphereBaseProperty.PropertyChanged += AtmosphereBasePropertyOnPropertyChanged;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent += OnAtmosphereBaked;
            EventManager.BodyEvents.OnAtmospherePresetChanged.OnEvent += OnAtmospherePresetChanged;

            isEventit = true;
        }

        public void UnEventit()
        {
            if (!isEventit) return;

            AtmosphereBaseProperty.PropertyChanged -= AtmosphereBasePropertyOnPropertyChanged;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent -= OnAtmosphereBaked;
            EventManager.BodyEvents.OnAtmospherePresetChanged.OnEvent -= OnAtmospherePresetChanged;

            isEventit = false;
        }

        #endregion

        #region Events

        private void AtmosphereBasePropertyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EventManager.BodyEvents.OnAtmospherePresetChanged.Invoke(ParentBody, this, AtmosphereBase);
        }

        private void OnAtmosphereBaked(Body body, Atmosphere atmosphere)
        {
            if (body == null)
            {
                Debug.Log("Atmosphere: OnAtmosphereBaked body is null!");
                return;
            }

            if (atmosphere == null)
            {
                Debug.Log("Atmosphere: OnAtmosphereBaked atmosphere is null!");
                return;
            }

            atmosphere.PushPreset(AtmosphereParameters.Get(atmosphere.AtmosphereBase));
            atmosphere.Reanimate();
        }

        private void OnAtmospherePresetChanged(Body body, Atmosphere atmosphere, AtmosphereBase atmosphereBase)
        {
            if (body == null)
            {
                Debug.Log("Atmosphere: OnAtmospherePresetChanged body is null!");
                return;
            }

            if (atmosphere == null)
            {
                Debug.Log("Atmosphere: OnAtmospherePresetChanged atmosphere is null!");
                return;
            }

            atmosphere.PushPreset(AtmosphereParameters.Get(atmosphereBase));
            atmosphere.Bake();
        }

        #endregion

        #region NodeSlave<Atmosphere>

        public override void InitNode()
        {
            PushPreset(AtmosphereParameters.Get(AtmosphereBase));

            Bake();

            InitMaterials();

            ParentBody.InitUniforms(SkyMaterial);
        }

        public override void UpdateNode()
        {
            SkyMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            AtmosphereParameters.Rg = Radius - RadiusHold;
            AtmosphereParameters.Rt = (Radius + Height) - RadiusHold;
            AtmosphereParameters.Rl = (Radius + Height * 1.05f) - RadiusHold;
            AtmosphereParameters.SCALE = Scale;

            var fadeValue = Mathf.Clamp01(VectorHelper.AngularRadius(Origin, GodManager.Instance.View.WorldCameraPosition, Radius));

            Fade = FadeCurve.Evaluate(float.IsNaN(fadeValue) || float.IsInfinity(fadeValue) ? 1.0f : fadeValue);

            ParentBody.SetUniforms(SkyMaterial);
        }

        protected override void Awake()
        {
            base.Awake();

            if (AtmosphereBaker == null) AtmosphereBaker = GetComponentInChildren<PreProcessAtmosphere>();
        }

        protected override void Start()
        {
            Eventit();

            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            UnEventit();

            Helper.Destroy(SkyMaterial);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            target.SetFloat("TRANSMITTANCE_W", AtmosphereConstants.TRANSMITTANCE_W);
            target.SetFloat("TRANSMITTANCE_H", AtmosphereConstants.TRANSMITTANCE_H);
            target.SetFloat("SKY_W", AtmosphereConstants.SKY_W);
            target.SetFloat("SKY_H", AtmosphereConstants.SKY_H);
            target.SetFloat("RES_R", AtmosphereConstants.RES_R);
            target.SetFloat("RES_MU", AtmosphereConstants.RES_MU);
            target.SetFloat("RES_MU_S", AtmosphereConstants.RES_MU_S);
            target.SetFloat("RES_NU", AtmosphereConstants.RES_NU);
            target.SetFloat("AVERAGE_GROUND_REFLECTANCE", AtmosphereParameters.AVERAGE_GROUND_REFLECTANCE);
            target.SetFloat("HR", AtmosphereParameters.HR * 1000.0f);
            target.SetFloat("HM", AtmosphereParameters.HM * 1000.0f);
        }

        public void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            target.SetFloat("fade", Fade);
            target.SetFloat("density", Density);
            target.SetFloat("scale", AtmosphereParameters.SCALE);
            target.SetFloat("Rg", AtmosphereParameters.Rg);
            target.SetFloat("Rt", AtmosphereParameters.Rt);
            target.SetFloat("RL", AtmosphereParameters.Rl);
            target.SetVector("betaR", AtmosphereParameters.BETA_R / 1000);
            target.SetVector("betaMSca", AtmosphereParameters.BETA_MSca / 1000);
            target.SetVector("betaMEx", AtmosphereParameters.BETA_MEx / 1000);
            target.SetFloat("mieG", Mathf.Clamp(AtmosphereParameters.MIE_G, 0.0f, 0.99f));

            target.SetFloat("_Sky_HorizonFixEps", HorizonFixEps);
            target.SetFloat("_Sky_MieFadeFix", MieFadeFix);

            target.SetFloat("_Aerial_Perspective_Offset", AerialPerspectiveOffset);
            target.SetFloat("_ExtinctionGroundFade", ExtinctionGroundFade);

            if (AtmosphereBaker.transmittanceT != null) target.SetTexture("_Sky_Transmittance", AtmosphereBaker.transmittanceT);
            if (AtmosphereBaker.inscatterT_Read != null) target.SetTexture("_Sky_Inscatter", AtmosphereBaker.inscatterT_Read);
            if (AtmosphereBaker.irradianceT_Read != null) target.SetTexture("_Sky_Irradiance", AtmosphereBaker.irradianceT_Read);

            target.SetVector("_Atmosphere_WorldCameraPos", GodManager.Instance.View.WorldCameraPosition - Origin);
            target.SetVector("_Atmosphere_Origin", -Origin);
            target.SetVector("_Atmosphere_GlowColor", GlowColor);

            target.SetFloat("_Exposure", HDRExposure);
            target.SetFloat("_HDRMode", (int)GodManager.Instance.HDRMode);
        }

        #endregion

        #region IUniformed

        public void InitSetUniforms()
        {
            // ...
        }

        #endregion

        #region IReanimateable

        public void Reanimate()
        {
            if (ParentBody != null)
            {
                InitUniforms(ParentBody.MPB);
                SetUniforms(ParentBody.MPB);

                ParentBody.InitUniforms(SkyMaterial);
                ParentBody.SetUniforms(SkyMaterial);
            }
            else
                Debug.Log("Atmosphere: Reanimation fail!");
        }

        #endregion

        #region IRenderable

        public void Render(int layer = 9)
        {
            if (AtmosphereMesh == null) return;

            var atmosphereTRS = Matrix4x4.TRS(ParentBody.transform.position, transform.rotation, Vector3.one * (Radius + Height));

            Graphics.DrawMesh(AtmosphereMesh, atmosphereTRS, SkyMaterial, layer, CameraHelper.Main(), 0, ParentBody.MPB);
        }

        #endregion

        public AtmosphereParameters PopPreset()
        {
            return AtmosphereParameters;
        }

        public void PushPreset(AtmosphereParameters ap)
        {
            AtmosphereParameters = new AtmosphereParameters(ap);

            AtmosphereParameters.Rg = Radius - RadiusHold;
            AtmosphereParameters.Rt = (Radius + Height) - RadiusHold;
            AtmosphereParameters.Rl = (Radius + Height * 1.05f) - RadiusHold;
            AtmosphereParameters.SCALE = Scale;
        }

        public void Bake()
        {
            AtmosphereBaker.Bake(AtmosphereParameters, () =>
            {
                EventManager.BodyEvents.OnAtmosphereBaked.Invoke(ParentBody, this);
            });
        }

        public void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus == true && LostFocusForceRebake == true)
            {
                Bake();
            }
        }

        public void InitMaterials()
        {
            SkyMaterial = MaterialHelper.CreateTemp(SkyShader, "Sky");
        }
    }
}