﻿#region License

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
// Creation Date: 2017.03.28
// Creation Time: 2:17 PM
// Creator: zameran

#endregion

using System.Collections.Generic;
using SpaceEngine.Core.Patterns.Strategy;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Enums;
using SpaceEngine.Environment.Atmospheric;
using SpaceEngine.Environment.Oceanic;
using SpaceEngine.Environment.Rings;
using SpaceEngine.Environment.Shadows;
using SpaceEngine.Environment.Sun;
using SpaceEngine.Helpers;
using SpaceEngine.Managers;
using SpaceEngine.Tools;
using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    public class Body : Node<Body>, IEventit, IBody, IUniformed<MaterialPropertyBlock>, IReanimateable, IRenderable<Body>
    {
        public Atmosphere Atmosphere;
        public OceanNode Ocean;
        public Ring Ring;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
        public int RenderQueueOffset;

        public float SizeOffset = 10.0f;

        public bool DrawGizmos;
        public bool UpdateLOD = true;

        public bool AtmosphereEnabled = true;
        public bool OceanEnabled = true;
        public bool RingEnabled = true;
        public bool TerrainEnabled = true;

        public float Amplitude = 32.0f;
        public float Frequency = 64.0f;

        public Shader ColorShader;

        public List<TileStorage> Storages = new();
        public List<TerrainNode> TerrainNodes = new(6);

        [HideInInspector]
        public double HeightZ;

        public float Size = 6360000.0f;

        public TCCommonParametersSetter TCCPS;
        public List<Sun> Suns = new(4);
        public List<Shadow> ShadowCasters = new(4);
        public List<Body> EclipseCasters = new(4);
        public List<Body> ShineCasters = new(4);
        public List<Color> ShineColors = new(4) { XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish };
        private Matrix4x4 occludersMatrix1 = Matrix4x4.zero;

        private Matrix4x4 shineColorsMatrix1 = Matrix4x4.zero;
        private Matrix4x4 shineOccludersMatrix1 = Matrix4x4.zero;
        private Matrix4x4 shineOccludersMatrix2 = Matrix4x4.zero;
        private Matrix4x4 shineParameters1 = Matrix4x4.zero;
        private Matrix4x4 sunColorsMatrix1 = Matrix4x4.zero;
        private Matrix4x4 sunDirectionsMatrix1 = Matrix4x4.zero;
        private Matrix4x4 sunPositionsMatrix1 = Matrix4x4.zero;

        public int GridResolution => GodManager.Instance.GridResolution;

        public Mesh QuadMesh => GodManager.Instance.QuadMesh;

        public Vector3 Offset { get; set; }

        public Vector3 Origin
        {
            get => transform.position;
            set => transform.position = value;
        }

        public MaterialPropertyBlock MPB { get; set; }

        public List<string> Keywords { get; set; }

        protected virtual void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus != true)
            {
                return;
            }

            Reanimate();

            if (Atmosphere != null)
            {
                Atmosphere.Reanimate();
            }

            if (Ocean != null)
            {
                Ocean.Reanimate();
            }

            if (Ring != null)
            {
                Ring.Reanimate();
            }
        }

        #region IReanimateable

        public virtual void Reanimate()
        {
            for (var i = 0; i < Mathf.Min(4, Suns.Count); i++)
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

            foreach (var terrainNode in TerrainNodes)
            {
                if (Helper.Enabled(terrainNode))
                {
                    InitUniforms(terrainNode.TerrainMaterial);
                    SetUniforms(terrainNode.TerrainMaterial);
                }
            }
        }

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 8)
        {
            if (Atmosphere != null)
            {
                if (AtmosphereEnabled)
                {
                    Atmosphere.Render();
                }
            }

            if (Ocean != null)
            {
                if (OceanEnabled)
                {
                    Ocean.Render();
                }
            }

            if (Ring != null)
            {
                if (RingEnabled)
                {
                    Ring.Render();
                }
            }

            if (TerrainEnabled)
            {
                for (var i = 0; i < TerrainNodes.Count; i++)
                {
                    if (Helper.Enabled(TerrainNodes[i]))
                    {
                        DrawTerrain(TerrainNodes[i], layer);
                    }
                }
            }
        }

        #endregion

        #region IUniformed

        public virtual void InitSetUniforms()
        {
            InitUniforms(MPB);
            SetUniforms(MPB);
        }

        #endregion

        #region Events

        private void OnSamplersChanged(Body body, TerrainNode node)
        {
            node.CollectSamplers();
            node.CollectSamplersSuitable();
        }

        #endregion

        public virtual List<string> GetKeywords()
        {
            var keywords = new List<string>();

            return keywords;
        }

        private void DrawTerrain(TerrainNode node, int layer)
        {
            // So, if doesn't have any samplers - do anything...
            if (node.Samplers.Count == 0 || node.SamplersSuitable.Count == 0)
            {
                return;
            }

            // Find all the quads in the terrain node that need to be drawn
            node.FindDrawableQuads(node.TerrainQuadRoot);

            // The draw them
            node.DrawQuads(node.TerrainQuadRoot, QuadMesh, MPB, layer);
        }

        #region Eventit

        public bool IsEventit { get; set; }

        public void Eventit()
        {
            if (IsEventit)
            {
                return;
            }

            EventManager.BodyEvents.OnSamplersChanged.OnEvent += OnSamplersChanged;

            IsEventit = true;
        }

        public void UnEventit()
        {
            if (!IsEventit)
            {
                return;
            }

            EventManager.BodyEvents.OnSamplersChanged.OnEvent -= OnSamplersChanged;

            IsEventit = false;
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            Storages = new List<TileStorage>(GetComponentsInChildren<TileStorage>(true));

            for (var storageIndex = 0; storageIndex < Storages.Count; storageIndex++)
            {
                var storage = Storages[storageIndex];

                storage.InitNode();
            }

            TerrainNodes = new List<TerrainNode>(GetComponentsInChildren<TerrainNode>(true));
            TerrainNodes.Sort(new TerrainNode.Sort());

            if (Atmosphere != null)
            {
                if (Atmosphere.ParentBody == null)
                {
                    Atmosphere.ParentBody = this;
                }

                Atmosphere.InitNode();
            }

            if (Ocean != null)
            {
                if (Ocean.ParentBody == null)
                {
                    Ocean.ParentBody = this;
                }

                Ocean.InitNode();
            }

            if (Ring != null)
            {
                if (Ring.ParentBody == null)
                {
                    Ring.ParentBody = this;
                }

                Ring.InitNode();
            }

            for (var terrainNodeIndex = 0; terrainNodeIndex < TerrainNodes.Count; terrainNodeIndex++)
            {
                var terrainNode = TerrainNodes[terrainNodeIndex];

                terrainNode.InitNode();

                for (var samplerNodeIndex = 0; samplerNodeIndex < terrainNode.Samplers.Count; samplerNodeIndex++)
                {
                    var samplerNode = terrainNode.Samplers[samplerNodeIndex];

                    samplerNode.InitNode();
                }
            }

            MPB = new MaterialPropertyBlock();

            Keywords = new List<string>();

            InitUniforms(MPB);
        }

        protected override void UpdateNode()
        {
            if (Atmosphere != null)
            {
                Atmosphere.UpdateNode();
            }

            if (Ocean != null)
            {
                Ocean.UpdateNode();
            }

            if (Ring != null)
            {
                Ring.UpdateNode();
            }

            for (var terrainNodeIndex = 0; terrainNodeIndex < TerrainNodes.Count; terrainNodeIndex++)
            {
                var terrainNode = TerrainNodes[terrainNodeIndex];

                terrainNode.UpdateNode();

                for (var samplerNodeIndex = 0; samplerNodeIndex < terrainNode.Samplers.Count; samplerNodeIndex++)
                {
                    var samplerNode = terrainNode.Samplers[samplerNodeIndex];

                    samplerNode.UpdateNode();
                }
            }

            // NOTE : Force to update tile creation tasks BEFORE rendering. Yeah. Until good multithreading is implemented...
            // As generation done in one frame with rendering, if schedular isn't updated - rendering will result the bad state...
            GodManager.Instance.UpdateSchedularWrapper();

            Keywords = GetKeywords();

            SetUniforms(MPB);

            // NOTE : Self - rendering!
            Render();
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
            base.OnDestroy();
        }

        #endregion

        #region IUniformed<Material>

        public virtual void InitUniforms(Material target)
        {
            if (target == null)
            {
                return;
            }

            Helper.SetKeywords(target, Keywords);
        }

        public virtual void SetUniforms(Material target)
        {
            if (target == null)
            {
                return;
            }

            Helper.SetKeywords(target, Keywords);
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public virtual void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null)
            {
                return;
            }

            if (Atmosphere != null)
            {
                Atmosphere.InitUniforms(target);
            }
        }

        public virtual void SetUniforms(MaterialPropertyBlock target)
        {
            // TODO : How to set these values per quad avoiding material property block and material uniforms?
            // NOTE : So, only these uniforms are variable per quad, but i don't know how to vary avoiding mpb and material uniforms, maybe instancing?
            //_Elevation_Tile
            //_Ortho_Tile
            //_Color_Tile
            //_Normals_Tile
            //_Deform_Offset
            //_Deform_Camera
            //_Deform_ScreenQuadCornerNorms
            //_Deform_ScreenQuadCorners
            //_Deform_ScreenQuadVericals
            //_Deform_TangentFrameToWorld

            if (target == null)
            {
                return;
            }

            SetEclipses(target);
            SetShine(target);
            SetSuns(target);

            target.SetFloat("_Globals_RadiusOffset", SizeOffset);

            target.SetVector("_Body_WorldCameraPosition", GodManager.Instance.View.WorldCameraPosition - Origin);
            target.SetVector("_Body_Origin", -Origin);

            if (Atmosphere != null)
            {
                Atmosphere.SetUniforms(target);
            }

            if (Ocean != null)
            {
                Ocean.SetUniforms(target);
            }

            if (Ring != null)
            {
                Ring.SetShadows(MPB, ShadowCasters);
            }

            //if (Manager.GetPlantsNode() != null)
            //    Manager.GetPlantsNode().SetUniforms(target);
        }

        #endregion

        #region AdditionalUniforms

        public void SetShine(Material mat)
        {
            if (!GodManager.Instance.Planetshine)
            {
                return;
            }

            CalculateShine(ref shineOccludersMatrix1, ref shineOccludersMatrix2, ref shineColorsMatrix1, ref shineParameters1);

            mat.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
            mat.SetMatrix("_Sky_ShineOccluders_2", shineOccludersMatrix2);
            mat.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
            mat.SetMatrix("_Sky_ShineParameters_1", shineParameters1);
        }

        public void SetShine(MaterialPropertyBlock block)
        {
            if (!GodManager.Instance.Planetshine)
            {
                return;
            }

            CalculateShine(ref shineOccludersMatrix1, ref shineOccludersMatrix2, ref shineColorsMatrix1, ref shineParameters1);

            block.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
            block.SetMatrix("_Sky_ShineOccluders_2", shineOccludersMatrix2);
            block.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
            block.SetMatrix("_Sky_ShineParameters_1", shineParameters1);
        }

        public void SetEclipses(Material mat)
        {
            if (!GodManager.Instance.Eclipses)
            {
                return;
            }

            CalculateEclipses(ref occludersMatrix1);

            mat.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        }

        public void SetEclipses(MaterialPropertyBlock block)
        {
            if (!GodManager.Instance.Eclipses)
            {
                return;
            }

            CalculateEclipses(ref occludersMatrix1);

            block.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        }

        public void SetSuns(Material mat)
        {
            if (mat == null)
            {
                return;
            }

            mat.SetFloat("_Sun_Intensity", 100.0f);

            CalculateSuns(ref sunColorsMatrix1, ref sunDirectionsMatrix1, ref sunPositionsMatrix1);

            mat.SetMatrix("_Sun_Colors_1", sunColorsMatrix1);
            mat.SetMatrix("_Sun_WorldDirections_1", sunDirectionsMatrix1);
            mat.SetMatrix("_Sun_Positions_1", sunPositionsMatrix1);
        }

        public void SetSuns(MaterialPropertyBlock block)
        {
            if (block == null)
            {
                return;
            }

            block.SetFloat("_Sun_Intensity", 100.0f);

            CalculateSuns(ref sunColorsMatrix1, ref sunDirectionsMatrix1, ref sunPositionsMatrix1);

            block.SetMatrix("_Sun_Colors_1", sunColorsMatrix1);
            block.SetMatrix("_Sun_WorldDirections_1", sunDirectionsMatrix1);
            block.SetMatrix("_Sun_Positions_1", sunPositionsMatrix1);
        }

        #endregion

        #region Calculations

        public void CalculateShine(ref Matrix4x4 soc1, ref Matrix4x4 soc2, ref Matrix4x4 sc1, ref Matrix4x4 sp1)
        {
            for (var i = 0; i < Mathf.Min(4, ShineCasters.Count); i++)
            {
                if (ShineCasters[i] == null)
                {
                    Debug.Log("Atmosphere: Shine problem!");

                    break;
                }

                var shineCaster = ShineCasters[i];

                // TODO : Planetshine distance based shine influence...
                // TODO : Planetshine distance don't gonna work correctly on screenspace, e.g Atmosphere...
                // NOTE : Distance is inversed.
                var distance = 0.0f;

                soc1.SetRow(i, VectorHelper.MakeFrom((shineCaster.transform.position - Origin).normalized, 1.0f));
                soc2.SetRow(i, VectorHelper.MakeFrom((Origin - shineCaster.transform.position).normalized, 1.0f));

                sc1.SetRow(i, (Helper.Enabled(shineCaster) ? ShineColors[i] : new Color(0, 0, 0, 0)).FromColor());

                sp1.SetRow(i, new Vector4(0.0f, 0.0f, 0.0f, distance));
            }
        }

        public void CalculateEclipses(ref Matrix4x4 occludersMatrix)
        {
            for (var i = 0; i < Mathf.Min(4, EclipseCasters.Count); i++)
            {
                if (EclipseCasters[i] == null)
                {
                    Debug.Log("Atmosphere: Eclipse caster problem!");

                    break;
                }

                if (EclipseCasters[i] as CelestialBody == null)
                {
                    Debug.Log("Atmosphere: Eclipse caster should be a planet!");

                    break;
                }

                var eclipseCaster = EclipseCasters[i];

                occludersMatrix.SetRow(i, VectorHelper.MakeFrom(eclipseCaster.Origin - Origin, Helper.Enabled(eclipseCaster) ? eclipseCaster.Size : 0.0f));
            }
        }

        public void CalculateSuns(ref Matrix4x4 sunColorsMatrix, ref Matrix4x4 sunDirectionsMatrix, ref Matrix4x4 sunPositionsMatrix)
        {
            for (var i = 0; i < Mathf.Min(4, Suns.Count); i++)
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

                sunColorsMatrix.SetRow(i, VectorHelper.MakeFrom(Vector3.one, sun.Intensity));
                sunDirectionsMatrix.SetRow(i, VectorHelper.MakeFrom(direction));
                sunPositionsMatrix.SetRow(i, VectorHelper.MakeFrom(position, radius));
                //sunPositions.SetRow(i, VectorHelper.MakeFrom(position, VectorHelper.AngularRadius(position, Origin, radius)));
            }
        }

        public float CalculateUmbraLength(float planetDiameter, float sunDiameter, float distance)
        {
            return -Mathf.Abs(planetDiameter * distance / (sunDiameter - planetDiameter));
        }

        public float CalculateUmbraSubtendedAngle(float planetDiameter, float umbraLength)
        {
            return Mathf.Asin(planetDiameter / (umbraLength * 2.0f)) * Mathf.Rad2Deg;
        }

        public Vector3 GetSunDirection(Sun sun)
        {
            return (sun.transform.position - Origin).normalized;
        }

        #endregion

        #region Gizmos

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (DrawGizmos == false)
            {
                return;
            }

            for (var i = 0; i < Mathf.Min(4, Suns.Count); i++)
            {
                var distanceToSun = Vector3.Distance(Suns[i].transform.position, Origin);
                var sunDirection = (Suns[i].transform.position - Origin) * distanceToSun;

                Gizmos.color = XKCDColors.Red;
                Gizmos.DrawRay(Origin, sunDirection);

                for (var j = 0; j < Mathf.Min(4, EclipseCasters.Count); j++)
                {
                    var distanceToEclipseCaster = Vector3.Distance(EclipseCasters[i].Origin, Origin);
                    var eclipseCasterDirection = (EclipseCasters[j].Origin - Origin) * distanceToEclipseCaster;

                    Gizmos.color = XKCDColors.Green;
                    Gizmos.DrawRay(Origin, eclipseCasterDirection);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (DrawGizmos == false)
            {
                return;
            }

            var radius = Size;

            for (var i = 0; i < Mathf.Min(4, Suns.Count); i++)
            {
                var sunRadius = Suns[i].Radius;
                var sunToPlanetDistance = Vector3.Distance(Origin, Suns[i].transform.position);
                var umbraLength = CalculateUmbraLength(radius * 2, sunRadius, sunToPlanetDistance);
                var umbraAngle = CalculateUmbraSubtendedAngle(radius * 2, umbraLength);
                var direction = GetSunDirection(Suns[i]) * umbraLength;
                var currentPosition = transform.position;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Suns[i].transform.position, sunRadius);
                Gizmos.DrawRay(Suns[i].transform.position, direction / umbraLength * -sunToPlanetDistance);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(Origin, direction);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(umbraAngle, 0, 0) * direction));
                Gizmos.DrawRay(transform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(-umbraAngle, 0, 0) * direction));
                Gizmos.DrawRay(transform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(0, umbraAngle, 0) * direction));
                Gizmos.DrawRay(transform.InverseTransformVector(Origin + direction), -(Quaternion.Euler(0, -umbraAngle, 0) * direction));

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(currentPosition + Vector3.up * radius, transform.InverseTransformVector(Origin + direction) + Vector3.up * radius);
                Gizmos.DrawLine(currentPosition + Vector3.down * radius, transform.InverseTransformVector(Origin + direction) + Vector3.down * radius);
                Gizmos.DrawLine(currentPosition + Vector3.left * radius, transform.InverseTransformVector(Origin + direction) + Vector3.left * radius);
                Gizmos.DrawLine(currentPosition + Vector3.right * radius, transform.InverseTransformVector(Origin + direction) + Vector3.right * radius);
            }
        }
        #endif

        #endregion
    }
}