using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy.Eventit;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Enviroment.Atmospheric;

using System;
using System.Collections;

using UnityEngine;

namespace SpaceEngine.Enviroment.Oceanic
{
    // TODO : Fix depth jumping on camera move...

    /// <summary>
    /// An AbstractTask to draw a flat or spherical ocean. This class provides the functions and data to draw a flat projected grid but nothing else.
    /// </summary>
    public abstract class OceanNode : Node<OceanNode>, IUniformed<Material>, IUniformed<MaterialPropertyBlock>, IReanimateable, IEventit, IRenderable<OceanNode>
    {
        public Body ParentBody;

        public Shader OceanShader;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
        public int RenderQueueOffset = 0;

        [SerializeField]
        protected Material OceanMaterial;

        [SerializeField]
        protected Color UpwellingColor = new Color(0.039f, 0.156f, 0.47f);
        [SerializeField]
        protected Color ShoreColor = new Color(0, 0.3411765f, 0.6235294f);

        /// <summary>
        /// Sea level in meters.
        /// </summary>
        public float OceanLevel = 5.0f;

        [Range(0.0f, 1.0f)]
        public float OceanWaveLevel = 1.0f;

        /// <summary>
        /// The maximum altitude at which the ocean must be displayed.
        /// </summary>
        [SerializeField]
        public float ZMin = 20000.0f;

        Matrix4x4d OldLocalToOcean = Matrix4x4d.identity;
        Matrix4x4d CameraToWorld = Matrix4x4d.identity;

        Vector3d ux = Vector3d.zero;
        Vector3d uy = Vector3d.zero;
        Vector3d uz = Vector3d.zero;
        Vector3d oo = Vector3d.zero;

        protected double H = 0;

        protected Vector4 Offset = Vector4.zero;

        /// <summary>
        /// If the ocean should be draw. To minimize depth fighting the ocean is not draw when the camera is far away. 
        /// Instead the terrain shader should render the ocean areas directly on the terrain
        /// </summary>
        public bool DrawOcean { get; protected set; }

        public Vector3d Origin { get { return ParentBody != null ? ParentBody.Origin : transform.position; } }

        protected const string FFT_KEYWORD = "OCEAN_FFT";
        protected const string WHITECAPS_KEYWORD = "OCEAN_WHITECAPS";

        /// <summary>
        /// Concrete classes must provide a function that returns the variance of the waves need for the BRDF rendering of waves.
        /// </summary>
        /// <returns></returns>
        public abstract float GetMaxSlopeVariance();

        #region OceanNode

        /// <summary>
        /// Update current ocean state important shader keywords. Call this somewhere per frame.
        /// </summary>
        protected virtual void UpdateKeywords(Material target)
        {
            Helper.ToggleKeyword(target, GodManager.Instance.OceanSkyReflections, "OCEAN_SKY_REFLECTIONS_ON", "OCEAN_SKY_REFLECTIONS_OFF");
            Helper.ToggleKeyword(target, GodManager.Instance.OceanDepth, "OCEAN_DEPTH_ON", "OCEAN_DEPTH_OFF");
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            OceanMaterial = MaterialHelper.CreateTemp(OceanShader, "Ocean");

            ParentBody.InitUniforms(OceanMaterial);
        }

        protected override void UpdateNode()
        {
            UpdateKeywords(OceanMaterial);

            OceanMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            // Calculates the required data for the projected grid
            CameraToWorld = GodManager.Instance.CameraToWorld;

            var oceanFrame = CameraToWorld * Vector3d.zero; // Camera in local space

            var radius = ParentBody.Size;
            var trueAltitude = Vector3d.Distance(GodManager.Instance.WorldCameraPos, Origin) - radius;

            if ((radius > 0.0 && trueAltitude > ZMin) || (radius < 0.0 && new Vector2d(oceanFrame.y, oceanFrame.z).Magnitude() < -radius - ZMin))
            {
                OldLocalToOcean = Matrix4x4d.identity;
                Offset = Vector4.zero;
                DrawOcean = false;

                return;
            }

            DrawOcean = true;
        }

        public void UpdateMatrices()
        {
            var worldToLocal = new Matrix4x4d(1.0, 0.0, 0.0, -Origin.x,
                                              0.0, 1.0, 0.0, -Origin.y,
                                              0.0, 0.0, 1.0, -Origin.z,
                                              0.0, 0.0, 0.0, 1.0);
            var cameraToLocal = worldToLocal * CameraToWorld;
            var localToCamera = cameraToLocal.Inverse();
            var oceanFrame = cameraToLocal * Vector3d.zero;
            var radius = ParentBody.Size;

            uz = oceanFrame.Normalized();

            ux = OldLocalToOcean != Matrix4x4d.identity ? new Vector3d(OldLocalToOcean.m[1, 0], OldLocalToOcean.m[1, 1], OldLocalToOcean.m[1, 2]).Cross(uz).Normalized() : 
                                                              Vector3d.forward.Cross(uz).Normalized();

            uy = uz.Cross(ux);
            oo = (uz * radius);

            var localToOcean = new Matrix4x4d(ux.x, ux.y, ux.z, -ux.Dot(oo),
                                              uy.x, uy.y, uy.z, -uy.Dot(oo),
                                              uz.x, uz.y, uz.z, -uz.Dot(oo),
                                              0.0, 0.0, 0.0, 1.0);
            var cameraToOcean = localToOcean * cameraToLocal;
            //var worldToOcean = localToOcean * worldToLocal;

            if (OldLocalToOcean != Matrix4x4d.identity)
            {
                var delta = localToOcean * (OldLocalToOcean.Inverse() * Vector3d.zero);

                Offset += VectorHelper.MakeFrom(delta, 0.0f);
            }

            if (Mathf.Max(Mathf.Abs(Offset.x), Mathf.Abs(Offset.y)) > 20000.0f)
            {
                Offset.x = 0.0f;
                Offset.y = 0.0f;
            }

            OldLocalToOcean = localToOcean;

            var oc = cameraToOcean * Vector3d.zero;

            H = oc.z;

            if (double.IsNaN(H)) { H = 1.0; }

            var offset = new Vector3d(-Offset.x, -Offset.y, H);

            // TODO : Up to four light sources support...
            var sunDirection = ParentBody.GetSunDirection(ParentBody.Suns[0]);
            var oceanSunDirection = localToOcean.ToMatrix3x3d() * sunDirection;

            var sphereDirection = (localToCamera * Vector3d.zero).Normalized();   // Direction to center of planet			
            var OHL = (localToCamera * Vector3d.zero).Magnitude();                // Distance to center of planet
            var rHorizon = Math.Sqrt(OHL * OHL - radius * radius);                // Distance to the horizon, i.e distance to ocean sphere tangent

            // Theta equals angle to horizon, now all that is left to do is check the view direction against this angle
            var cosTheta = rHorizon / OHL;
            var sinTheta = Math.Sqrt(1.0 - cosTheta * cosTheta);
            var oceanGridResolution = GodManager.Instance.OceanGridResolution;

            OceanMaterial.SetVector("_SphereDirection", sphereDirection.ToVector3());
            OceanMaterial.SetFloat("_CosTheta", (float)cosTheta);
            OceanMaterial.SetFloat("_SinTheta", (float)sinTheta);

            OceanMaterial.SetVector("_Ocean_SunDir", oceanSunDirection.ToVector3());
            OceanMaterial.SetMatrix("_Ocean_CameraToOcean", cameraToOcean.ToMatrix4x4());
            OceanMaterial.SetMatrix("_Ocean_OceanToCamera", cameraToOcean.Inverse().ToMatrix4x4());
            OceanMaterial.SetMatrix("_Ocean_WorldToLocal", worldToLocal.ToMatrix4x4());
            OceanMaterial.SetVector("_Ocean_CameraPos", offset.ToVector3());
            OceanMaterial.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            OceanMaterial.SetVector("_Ocean_Shore_Color", ShoreColor);
            OceanMaterial.SetVector("_Ocean_ScreenGridSize", new Vector2((float)oceanGridResolution / (float)Screen.width, (float)oceanGridResolution / (float)Screen.height));
            OceanMaterial.SetFloat("_Ocean_Radius", radius);
            OceanMaterial.SetFloat("_Ocean_Wave_Level", OceanWaveLevel);

            // TODO : Complete ocean matrices calculation for a cyllindrical worlds...
        }

        protected override void Awake()
        {
            base.Awake();
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

            Helper.Destroy(OceanMaterial);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed<Material>

        public virtual void InitUniforms(Material target)
        {
            if (target == null) return;
        }

        public virtual void SetUniforms(Material target)
        {
            if (target == null) return;

            target.SetFloat("_Ocean_Sigma", GetMaxSlopeVariance());
            target.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            target.SetVector("_Ocean_Shore_Color", ShoreColor);
            target.SetFloat("_Ocean_DrawBRDF", DrawOcean ? 0.0f : 1.0f);
            target.SetFloat("_Ocean_Level", OceanLevel);
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public virtual void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;
        }

        public virtual void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            target.SetFloat("_Ocean_Sigma", GetMaxSlopeVariance());
            target.SetVector("_Ocean_Color", UpwellingColor * 0.1f);
            target.SetVector("_Ocean_Shore_Color", ShoreColor);
            target.SetFloat("_Ocean_DrawBRDF", DrawOcean ? 0.0f : 1.0f);
            target.SetFloat("_Ocean_Level", OceanLevel);
        }

        #endregion

        #region IUniformed

        public virtual void InitSetUniforms()
        {
            InitUniforms(OceanMaterial);
            SetUniforms(OceanMaterial);
        }

        #endregion

        #region IReanimateable

        public void Reanimate()
        {
            InitNode();
        }

        #endregion

        #region Eventit

        public bool isEventit { get; set; }

        public void Eventit()
        {
            if (isEventit) return;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent += OnAtmosphereBaked;

            isEventit = true;
        }

        public void UnEventit()
        {
            if (!isEventit) return;

            EventManager.BodyEvents.OnAtmosphereBaked.OnEvent -= OnAtmosphereBaked;

            isEventit = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// This event must be overridden in every inherited class, at any level of inheritance.
        /// Should provide <see cref="UpdateNode"/> call.
        /// </summary>
        /// <param name="celestialBody">Celestial body.</param>
        /// <param name="atmosphere">Atmosphere.</param>
        protected abstract void OnAtmosphereBaked(Body celestialBody, Atmosphere atmosphere);

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 10)
        {
            if (DrawOcean == false) return;

            var oceanScreenMeshGrids = GodManager.Instance.OceanScreenMeshGrids;

            foreach (var mesh in oceanScreenMeshGrids)
            {
                if (mesh == null) break;

                Graphics.DrawMesh(mesh, Matrix4x4.identity, OceanMaterial, layer, CameraHelper.Main(), 0, ParentBody.MPB);

                UpdateMatrices();
            }
        }

        #endregion

        /// <summary>
        /// Inverting <see cref="ZMin"/> with interval to switch the matrices for rendering.
        /// </summary>
        /// <returns><see cref="Yielders.EndOfFrame"/></returns>
        [Obsolete("Don't use it! Looks like a useless bugfix.")]
        public IEnumerator InitializationFix()
        {
            ZMin *= -1.0f;
            yield return Yielders.EndOfFrame;
            ZMin *= -1.0f;
        }
    }
}