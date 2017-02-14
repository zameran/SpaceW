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

using SpaceEngine.AtmosphericScattering.Sun;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.AtmosphericScattering
{
    public sealed class Atmosphere : Node<Atmosphere>, IEventit
    {
        private AtmosphereBase atmosphereBase = AtmosphereBase.Earth;

        public AtmosphereBase AtmosphereBase
        {
            get { return atmosphereBase; }
            set
            {
                var changed = false;

                changed = (atmosphereBase != value);

                atmosphereBase = value;

                if (changed)
                    EventManager.PlanetoidEvents.OnAtmospherePresetChanged.Invoke(planetoid, this);
            }
        }

        public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                              new Keyframe(0.25f, 1.0f),
                                                                              new Keyframe(1.0f, 1.0f) });

        public Planetoid planetoid;

        [Range(0.0f, 1.0f)]
        public float Density = 1.0f;

        [Tooltip("1/3 or 1/2 from Planet.TerrainMaxHeight")]
        public float TerrainRadiusHold = 0.0f;
        public float Height = 100.0f;
        public float Scale = 1.0f;
        public float Fade = 1.0f;
        public float AerialPerspectiveOffset = 2000.0f;
        public float ExtinctionGroundFade = 0.000025f;

        public int AtmosphereMeshResolution = 2;

        public float HDRExposure = 0.2f;

        public Shader SkyShader;
        public Material SkyMaterial;

        [HideInInspector]
        public AtmosphereHDR HDRMode = AtmosphereHDR.Proland;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Background;
        public int RenderQueueOffset = 0;

        public Mesh AtmosphereMesh;

        public bool LostFocusForceRebake = false;
        [HideInInspector]
        public bool Eclipses = true;
        [HideInInspector]
        public bool Planetshine = true;

        public List<AtmosphereSun> Suns = new List<AtmosphereSun>();

        public List<Planet> EclipseCasters = new List<Planet>();
        public List<GameObject> ShineCasters = new List<GameObject>();

        private AtmosphereParameters atmosphereParameters;

        public AtmosphereRunTimeBaker artb = null;

        public Vector3 Origin { get { return planetoid != null ? planetoid.Origin : Vector3.zero; } }

        public Color[] shineColors = new Color[4] { XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish };

        private Matrix4x4 shineColorsMatrix1;
        private Matrix4x4 shineOccludersMatrix1;
        private Matrix4x4 occludersMatrix1;
        private Matrix4x4 sunPositionsMatrix;
        private Matrix4x4 sunDirectionsMatrix;
        private Matrix4x4 worldToCamera;
        private Matrix4x4 cameraToWorld;
        private Matrix4x4 cameraToScreen;
        private Matrix4x4 screenToCamera;

        private Vector3 worldCameraPos;

        public List<string> Keywords = new List<string>();

        public float Radius { get { return planetoid != null ? planetoid.PlanetRadius : 0.0f; } }

        #region Eventit
        public bool isEventit { get; set; }

        public void Eventit()
        {
            if (isEventit) return;

            EventManager.PlanetoidEvents.OnAtmosphereBaked.OnEvent += OnAtmosphereBaked;
            EventManager.PlanetoidEvents.OnAtmospherePresetChanged.OnEvent += OnAtmospherePresetChanged;

            isEventit = true;
        }

        public void UnEventit()
        {
            if (!isEventit) return;

            EventManager.PlanetoidEvents.OnAtmosphereBaked.OnEvent -= OnAtmosphereBaked;
            EventManager.PlanetoidEvents.OnAtmospherePresetChanged.OnEvent -= OnAtmospherePresetChanged;

            isEventit = false;
        }
        #endregion

        #region Events
        private void OnAtmosphereBaked(Planetoid planetoid, Atmosphere atmosphere)
        {
            if (planetoid == null)
            {
                Debug.Log("Atmosphere: OnAtmosphereBaked planetoid is null!");
                return;
            }

            if (atmosphere == null)
            {
                Debug.Log("Atmosphere: OnAtmosphereBaked atmosphere is null!");
                return;
            }

            atmosphere.ApplyPresset(AtmosphereParameters.Get(atmosphere.AtmosphereBase));
            atmosphere.ReanimateAtmosphereUniforms(atmosphere, planetoid);
        }

        private void OnAtmospherePresetChanged(Planetoid planetoid, Atmosphere atmosphere)
        {
            if (planetoid == null)
            {
                Debug.Log("Atmosphere: OnAtmospherePresetChanged planetoid is null!");
                return;
            }

            if (atmosphere == null)
            {
                Debug.Log("Atmosphere: OnAtmospherePresetChanged atmosphere is null!");
                return;
            }

            atmosphere.TryBake();
            planetoid.ReSetupQuads();
        }
        #endregion

        #region Node

        protected override void InitNode()
        {
            ApplyPresset(AtmosphereParameters.Get(AtmosphereBase));

            TryBake();

            InitMisc();
            InitMaterials();
            InitMesh();

            InitSetAtmosphereUniforms();
        }

        protected override void UpdateNode()
        {
            SkyMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            atmosphereParameters.Rg = Radius - TerrainRadiusHold;
            atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
            atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
            atmosphereParameters.SCALE = Scale;

            worldToCamera = CameraHelper.Main().GetWorldToCamera();
            cameraToWorld = CameraHelper.Main().GetCameraToWorld();
            cameraToScreen = CameraHelper.Main().GetCameraToScreen();
            screenToCamera = CameraHelper.Main().GetScreenToCamera();
            worldCameraPos = CameraHelper.Main().transform.position;

            var fadeValue = Mathf.Clamp01(VectorHelper.AngularRadius(Origin, planetoid.LODTarget.position, planetoid.PlanetRadius));

            Fade = FadeCurve.Evaluate(float.IsNaN(fadeValue) || float.IsInfinity(fadeValue) ? 1.0f : fadeValue);

            Keywords = planetoid.GetKeywords();
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

        #endregion

        private void ApplyPresset(AtmosphereParameters p)
        {
            atmosphereParameters = new AtmosphereParameters(p);

            atmosphereParameters.Rg = Radius - TerrainRadiusHold;
            atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
            atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
            atmosphereParameters.SCALE = Scale;
        }

        public void TryBake()
        {
            if (artb != null) artb.Bake(atmosphereParameters);

            EventManager.PlanetoidEvents.OnAtmosphereBaked.Invoke(planetoid, this);
        }

        public void CalculateShine(out Matrix4x4 soc1, out Matrix4x4 sc1)
        {
            soc1 = Matrix4x4.zero;
            sc1 = Matrix4x4.zero;

            byte index = 0;

            for (byte i = 0; i < Mathf.Min(4, ShineCasters.Count); i++)
            {
                if (ShineCasters[i] == null) { Debug.Log("Atmosphere: Shine problem!"); break; }

                var distance = shineColors[i].a; //TODO : Distance based shine power.

                soc1.SetRow(i, VectorHelper.MakeFrom((ShineCasters[i].transform.position - Origin).normalized, 1.0f));

                sc1.SetRow(index, VectorHelper.FromColor(shineColors[i], distance));

                index++;
            }
        }

        public void CalculateEclipses(out Matrix4x4 occludersMatrix)
        {
            occludersMatrix = Matrix4x4.zero;

            for (byte i = 0; i < Mathf.Min(4, EclipseCasters.Count); i++)
            {
                if (EclipseCasters[i] == null)
                {
                    Debug.Log("Atmosphere: Eclipse caster problem!");
                    break;
                }

                occludersMatrix.SetRow(i, VectorHelper.MakeFrom(EclipseCasters[i].Origin - Origin, EclipseCasters[i].PlanetRadius));
            }
        }

        public void CalculateSuns(out Matrix4x4 sunDirectionsMatrix, out Matrix4x4 sunPositionsMatrix)
        {
            sunDirectionsMatrix = Matrix4x4.zero;
            sunPositionsMatrix = Matrix4x4.zero;

            for (byte i = 0; i < Mathf.Min(4, Suns.Count); i++)
            {
                if (Suns[i] == null)
                {
                    Debug.Log("Atmosphere: Sun calculation problem!");
                    break;
                }

                var sun = Suns[i];
                var direction = GetSunDirection(sun);
                var position = sun.transform.position;
                var radius = sun.Radius;

                sunDirectionsMatrix.SetRow(i, VectorHelper.MakeFrom(direction));
                sunPositionsMatrix.SetRow(i, VectorHelper.MakeFrom(position, radius));
                //sunPositions.SetRow(i, VectorHelper.MakeFrom(position, VectorHelper.AngularRadius(position, Origin, radius)));
            }
        }

        public void SetShine(Material mat)
        {
            if (!Planetshine) return;

            CalculateShine(out shineOccludersMatrix1, out shineColorsMatrix1);

            mat.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
            mat.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
        }

        public void SetShine(MaterialPropertyBlock block)
        {
            if (!Planetshine) return;

            CalculateShine(out shineOccludersMatrix1, out shineColorsMatrix1);

            block.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
            block.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
        }

        public void SetEclipses(Material mat)
        {
            if (!Eclipses) return;

            CalculateEclipses(out occludersMatrix1);

            mat.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        }

        public void SetEclipses(MaterialPropertyBlock block)
        {
            if (!Eclipses) return;

            CalculateEclipses(out occludersMatrix1);

            block.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        }

        public void SetSuns(MaterialPropertyBlock block)
        {
            if (block == null) return;

            block.SetFloat("_Sun_Intensity", 100.0f);

            CalculateSuns(out sunDirectionsMatrix, out sunPositionsMatrix);

            block.SetMatrix("_Sun_WorldDirections_1", sunDirectionsMatrix);
            block.SetMatrix("_Sun_Positions_1", sunPositionsMatrix);
        }

        public void Render(Vector3 Origin, int drawLayer = 8)
        {
            Render(CameraHelper.Main(), Origin, drawLayer);
        }

        public void Render(Camera camera, Vector3 Origin, int drawLayer = 8)
        {
            SetUniforms(planetoid.QuadMPB, SkyMaterial, true);

            Graphics.DrawMesh(AtmosphereMesh, transform.localToWorldMatrix, SkyMaterial, drawLayer, camera, 0, planetoid.QuadMPB);
        }

        public void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus == true && LostFocusForceRebake == true)
            {
                TryBake();
            }
        }

        private void OnDestroy()
        {
            Helper.Destroy(SkyMaterial);
            Helper.Destroy(AtmosphereMesh);

            UnEventit();
        }

        private void OnDrawGizmosSelected()
        {
            if (planetoid != null)
            {
                if (planetoid.DrawGizmos == false) return;

                for (byte i = 0; i < Mathf.Min(4, Suns.Count); i++)
                {
                    var distanceToSun = Vector3.Distance(Suns[i].transform.position, Origin);
                    var sunDirection = (Suns[i].transform.position - Origin) * distanceToSun;

                    Gizmos.color = XKCDColors.Red;
                    Gizmos.DrawRay(Origin, sunDirection);

                    for (byte j = 0; j < Mathf.Min(4, EclipseCasters.Count); j++)
                    {
                        var distanceToEclipseCaster = Vector3.Distance(EclipseCasters[i].Origin, Origin); ;
                        var eclipseCasterDirection = (EclipseCasters[j].Origin - Origin) * distanceToEclipseCaster;

                        Gizmos.color = XKCDColors.Green;
                        Gizmos.DrawRay(Origin, eclipseCasterDirection);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (planetoid != null)
            {
                if (planetoid.DrawGizmos == false) return;

                for (byte i = 0; i < Mathf.Min(4, Suns.Count); i++)
                {
                    float sunRadius = Suns[i].Radius;
                    float sunToPlanetDistance = Vector3.Distance(Origin, Suns[i].transform.position);
                    float umbraLength = CalculateUmbraLength(planetoid.PlanetRadius * 2, sunRadius, sunToPlanetDistance);
                    float umbraAngle = CalculateUmbraSubtendedAngle(planetoid.PlanetRadius * 2, umbraLength);

                    Vector3 direction = GetSunDirection(Suns[i]) * umbraLength;

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(Suns[i].transform.position, sunRadius);
                    Gizmos.DrawRay(Suns[i].transform.position, (direction / umbraLength) * -sunToPlanetDistance);

                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(Origin, direction);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(planetoid.OriginTransform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(umbraAngle, 0, 0) * direction));
                    Gizmos.DrawRay(planetoid.OriginTransform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(-umbraAngle, 0, 0) * direction));
                    Gizmos.DrawRay(planetoid.OriginTransform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(0, umbraAngle, 0) * direction));
                    Gizmos.DrawRay(planetoid.OriginTransform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(0, -umbraAngle, 0) * direction));

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(planetoid.OriginTransform.position + Vector3.up * planetoid.PlanetRadius, planetoid.OriginTransform.InverseTransformVector(Origin + direction) + Vector3.up * planetoid.PlanetRadius);
                    Gizmos.DrawLine(planetoid.OriginTransform.position + Vector3.down * planetoid.PlanetRadius, planetoid.OriginTransform.InverseTransformVector(Origin + direction) + Vector3.down * planetoid.PlanetRadius);
                    Gizmos.DrawLine(planetoid.OriginTransform.position + Vector3.left * planetoid.PlanetRadius, planetoid.OriginTransform.InverseTransformVector(Origin + direction) + Vector3.left * planetoid.PlanetRadius);
                    Gizmos.DrawLine(planetoid.OriginTransform.position + Vector3.right * planetoid.PlanetRadius, planetoid.OriginTransform.InverseTransformVector(Origin + direction) + Vector3.right * planetoid.PlanetRadius);
                }
            }
        }

        private Vector3 GetSunDirection(AtmosphereSun sun)
        {
            return (sun.transform.position - Origin).normalized;
        }

        public void InitMaterials()
        {
            SkyMaterial = MaterialHelper.CreateTemp(SkyShader, "Sky");
        }

        public void InitMesh()
        {
            AtmosphereMesh = MeshFactory.MakePlane(AtmosphereMeshResolution, AtmosphereMeshResolution, MeshFactory.PLANE.XY, false, false, false);
            AtmosphereMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
        }

        public void InitMisc()
        {
            worldToCamera = CameraHelper.Main().GetWorldToCamera();
            cameraToWorld = CameraHelper.Main().GetCameraToWorld();
            cameraToScreen = CameraHelper.Main().GetCameraToScreen();
            screenToCamera = CameraHelper.Main().GetScreenToCamera();
            worldCameraPos = CameraHelper.Main().transform.position;

            Keywords = planetoid.GetKeywords();
        }

        public void InitUniforms(MaterialPropertyBlock block, Material mat, bool full = true)
        {
            if (mat != null)
            {
                Helper.SetKeywords(mat, Keywords);
            }

            if (full)
            {
                if (block == null) return;

                SetEclipses(block);
                SetShine(block);

                block.SetFloat("_Aerial_Perspective_Offset", AerialPerspectiveOffset);

                block.SetFloat("Rg", atmosphereParameters.Rg);
                block.SetFloat("Rt", atmosphereParameters.Rt);
                block.SetFloat("RL", atmosphereParameters.Rl);

                block.SetFloat("TRANSMITTANCE_W", AtmosphereConstants.TRANSMITTANCE_W);
                block.SetFloat("TRANSMITTANCE_H", AtmosphereConstants.TRANSMITTANCE_H);
                block.SetFloat("SKY_W", AtmosphereConstants.SKY_W);
                block.SetFloat("SKY_H", AtmosphereConstants.SKY_H);
                block.SetFloat("RES_R", AtmosphereConstants.RES_R);
                block.SetFloat("RES_MU", AtmosphereConstants.RES_MU);
                block.SetFloat("RES_MU_S", AtmosphereConstants.RES_MU_S);
                block.SetFloat("RES_NU", AtmosphereConstants.RES_NU);
                block.SetFloat("AVERAGE_GROUND_REFLECTANCE", atmosphereParameters.AVERAGE_GROUND_REFLECTANCE);
                block.SetFloat("HR", atmosphereParameters.HR * 1000.0f);
                block.SetFloat("HM", atmosphereParameters.HM * 1000.0f);
            }
        }

        public void InitPlanetoidUniforms(Planetoid planetoid)
        {
            if (planetoid.Atmosphere != null)
            {
                for (int i = 0; i < planetoid.Quads.Count; i++)
                {
                    if (planetoid.Quads[i] != null)
                    {
                        planetoid.Atmosphere.InitUniforms(null, planetoid.Quads[i].QuadMaterial, false);
                    }
                }

                //Just make sure that all mpb parameters are set.
                planetoid.Atmosphere.InitUniforms(planetoid.QuadMPB, null, true);
            }
        }

        public void SetPlanetoidUniforms(Planetoid planetoid)
        {
            if (planetoid.Atmosphere != null)
            {
                for (int i = 0; i < planetoid.Quads.Count; i++)
                {
                    if (planetoid.Quads[i] != null)
                    {
                        planetoid.Atmosphere.SetUniforms(null, planetoid.Quads[i].QuadMaterial, false, true);
                    }
                }

                //Just make sure that all mpb parameters are set.
                planetoid.Atmosphere.SetUniforms(planetoid.QuadMPB, null, true, true);
            }
        }

        public void InitSetPlanetoidUniforms(Planetoid planetoid)
        {
            InitPlanetoidUniforms(planetoid);
            SetPlanetoidUniforms(planetoid);
        }

        public void InitSetAtmosphereUniforms()
        {
            InitUniforms(planetoid.QuadMPB, SkyMaterial, true);
            SetUniforms(planetoid.QuadMPB, SkyMaterial, true);
        }

        public void InitSetAtmosphereUniforms(Atmosphere atmosphere)
        {
            InitUniforms(planetoid.QuadMPB, SkyMaterial, true);
            SetUniforms(planetoid.QuadMPB, SkyMaterial, true);
        }

        public void ReanimateAtmosphereUniforms(Atmosphere atmosphere, Planetoid planetoid)
        {
            if (atmosphere != null && planetoid != null)
            {
                atmosphere.InitSetPlanetoidUniforms(planetoid);
                atmosphere.InitSetAtmosphereUniforms();

                for (byte i = 0; i < Suns.Count; i++)
                {
                    if (Suns[i] != null)
                    {
                        var sunGlareComponent = Suns[i].GetComponent<SunGlare>();

                        if (sunGlareComponent != null)
                        {
                            sunGlareComponent.InitSetUniforms();
                        }
                    }
                }
            }
            else
                Debug.Log("Atmosphere: Reanimation fail!");
        }

        public void SetUniforms(MaterialPropertyBlock block, Material mat, bool full = true, bool forQuad = false)
        {
            if (artb == null) { Debug.Log("Atmosphere: ARTB is null!"); return; }

            if (mat != null)
            {
                Helper.SetKeywords(mat, Keywords);
            }

            if (full)
            {
                if (block == null) return;

                SetEclipses(block);
                SetShine(block);
                SetSuns(block);

                if (!forQuad)
                {
                    if (planetoid != null)
                    {
                        if (planetoid.Ring != null)
                        {
                            planetoid.Ring.SetShadows(block, planetoid.Shadows);
                        }
                    }
                }

                block.SetFloat("fade", Fade);
                block.SetFloat("density", Density);
                block.SetFloat("scale", atmosphereParameters.SCALE);
                block.SetFloat("Rg", atmosphereParameters.Rg);
                block.SetFloat("Rt", atmosphereParameters.Rt);
                block.SetFloat("RL", atmosphereParameters.Rl);
                block.SetVector("betaR", atmosphereParameters.BETA_R / 1000);
                block.SetVector("betaMSca", atmosphereParameters.BETA_MSca / 1000);
                block.SetVector("betaMEx", atmosphereParameters.BETA_MEx / 1000);
                block.SetFloat("mieG", Mathf.Clamp(atmosphereParameters.MIE_G, 0.0f, 0.99f));

                block.SetFloat("_Aerial_Perspective_Offset", AerialPerspectiveOffset);
                block.SetFloat("_ExtinctionGroundFade", ExtinctionGroundFade);

                if (artb.transmittanceT != null) block.SetTexture("_Sky_Transmittance", artb.transmittanceT);
                if (artb.inscatterT_Read != null) block.SetTexture("_Sky_Inscatter", artb.inscatterT_Read);
                if (artb.irradianceT_Read != null) block.SetTexture("_Sky_Irradiance", artb.irradianceT_Read);

                var WCP = forQuad == true ? worldCameraPos - Origin : worldCameraPos;

                block.SetMatrix("_Globals_WorldToCamera", worldToCamera);
                block.SetMatrix("_Globals_CameraToWorld", cameraToWorld);
                block.SetMatrix("_Globals_CameraToScreen", cameraToScreen);
                block.SetMatrix("_Globals_ScreenToCamera", screenToCamera);
                block.SetVector("_Globals_WorldCameraPos", forQuad == true ? worldCameraPos - Origin : worldCameraPos);

                block.SetVector("_Globals_Origin", -Origin);

                block.SetVector("WCPG", WCP + (-Origin));

                block.SetFloat("_Exposure", HDRExposure);
                block.SetFloat("_HDRMode", (int)HDRMode);
            }
        }

        #region ExtraAPI

        public float CalculateUmbraLength(float planetDiameter, float sunDiameter, float distance)
        {
            return -Mathf.Abs((planetDiameter * distance) / (sunDiameter - planetDiameter));
        }

        public float CalculateUmbraSubtendedAngle(float planetDiameter, float umbraLength)
        {
            return Mathf.Asin(planetDiameter / (umbraLength * 2.0f)) * Mathf.Rad2Deg;
        }

        #endregion
    }
}