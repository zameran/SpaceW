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
//     notice, this list of conditions and the following disclaimer.
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

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Patterns.Strategy.Reanimator;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Samplers;
using SpaceEngine.Ocean;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    /// <summary>
    /// Class - extensions holder for a <see cref="Body"/>.
    /// </summary>
    public static class BodyExtensions
    {
        /// <summary>
        /// Get the deformation type enumerator.
        /// </summary>
        /// <param name="body">Target.</param>
        /// <returns>Returns <see cref="BodyDeformationType"/> of target body.</returns>
        public static BodyDeformationType GetBodyDeformationType(this Body body)
        {
            if (body is CelestialBody)
            {
                return BodyDeformationType.Spherical;
            }

            return BodyDeformationType.Flat;
        }
    }


    public class Body : Node<Body>, IBody, IUniformed<MaterialPropertyBlock>, IReanimateable
    {
        public Atmosphere Atmosphere;
        public OceanNode Ocean;

        public bool DrawGizmos = false;

        public bool AtmosphereEnabled = true;
        public bool OceanEnabled = true;

        public int GridResolution = 25;

        public float Amplitude = 32.0f;
        public float Frequency = 64.0f;

        public Mesh QuadMesh;

        public Shader ColorShader;

        public List<TerrainNode> TerrainNodes = new List<TerrainNode>(6);
        public List<TileSampler> TileSamplers = new List<TileSampler>();

        [HideInInspector]
        public double HeightZ = 0;

        public float Size = 6360000.0f;

        public Vector3 Offset { get; set; }
        public Vector3 Origin { get { return transform.position; } set { transform.position = value; } }

        public NoiseParametersSetter NPS = null;
        public TCCommonParametersSetter TCCPS = null;

        public MaterialPropertyBlock MPB { get; set; }

        #region Node

        protected override void InitNode()
        {
            if (Atmosphere != null)
            {
                if (Atmosphere.ParentBody == null)
                    Atmosphere.ParentBody = this;
            }

            if (Ocean != null)
            {
                if (Ocean.ParentBody == null)
                    Ocean.ParentBody = this;

                // TODO : Whhhhhhaaattaaaaaafuuuuckkk!
                StartCoroutine(Ocean.InitializationFix());
            }

            QuadMesh = MeshFactory.MakePlane(GridResolution, MeshFactory.PLANE.XY, true, false, false);
            QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            TileSamplers = new List<TileSampler>(GetComponentsInChildren<TileSampler>());
            TileSamplers.Sort(new TileSampler.Sort());

            MPB = new MaterialPropertyBlock();

            if (NPS == null) NPS = GetComponent<NoiseParametersSetter>();
            NPS.LoadAndInit();
        }

        protected override void UpdateNode()
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

            // NOTE : Update controller and the draw. This can help avoid terrain nodes jitter...
            if (GodManager.Instance.ActiveBody == this)
                GodManager.Instance.UpdateControllerWrapper();

            foreach (var tileSampler in TileSamplers)
            {
                if (Helper.Enabled(tileSampler))
                {
                    tileSampler.UpdateSampler();
                }
            }

            foreach (var terrainNode in TerrainNodes)
            {
                if (Helper.Enabled(terrainNode))
                {
                    DrawTerrain(terrainNode);
                }
            }

            ResetMPB();
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

            Helper.Destroy(QuadMesh);
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public virtual void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.InitUniforms(target);
            }
        }

        public virtual void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.SetUniforms(target);
            }
        }

        public virtual void InitSetUniforms()
        {
            InitUniforms(MPB);
            SetUniforms(MPB);
        }

        #endregion

        #region IReanimateable

        public virtual void Reanimate()
        {
            foreach (var terrainNode in TerrainNodes)
            {
                if (Helper.Enabled(terrainNode))
                {
                    if (Atmosphere != null)
                    {
                        Atmosphere.InitUniforms(terrainNode.TerrainMaterial);
                        Atmosphere.SetUniforms(terrainNode.TerrainMaterial);
                    }
                }
            }
        }

        #endregion

        protected virtual void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus != true) return;

            Reanimate();

            if (Atmosphere != null) Atmosphere.Reanimate();
            if (Ocean != null) Ocean.Reanimate();
        }

        protected virtual void ResetMPB()
        {
            MPB.Clear();

            InitSetUniforms();
        }

        public virtual List<string> GetKeywords()
        {
            var Keywords = new List<string>();

            return Keywords;
        }

        private void DrawTerrain(TerrainNode node)
        {
            // Get all the samplers attached to the terrain node. The samples contain the data need to draw the quad
            var allSamplers = node.transform.GetComponentsInChildren<TileSampler>();
            var samplers = allSamplers.Where(sampler => sampler.enabled && sampler.StoreLeaf).ToList();

            if (samplers.Count == 0) return;
            if (samplers.Count > 255) { Debug.Log(string.Format("Body: Tomuch samplers! {0}", samplers.Count)); return; }

            // Find all the quads in the terrain node that need to be drawn
            node.FindDrawableQuads(node.TerrainQuadRoot, samplers);

            // The draw them
            node.DrawQuad(node.TerrainQuadRoot, samplers, QuadMesh, MPB);
        }
    }
}