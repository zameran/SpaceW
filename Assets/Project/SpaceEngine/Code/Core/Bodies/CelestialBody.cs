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
// Creation Date: 2017.02.20
// Creation Time: 10:45 PM
// Creator: zameran
// 
#endregion

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Samplers;
using SpaceEngine.Core.Utilities;
using SpaceEngine.Ocean;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    public class CelestialBody : Node<CelestialBody>, ICelestialBody, IUniformed<MaterialPropertyBlock>
    {
        public Atmosphere Atmosphere;
        public OceanNode Ocean;
        public Ring Ring;

        public List<Shadow> Shadows = new List<Shadow>();

        public int GridResolution = 25;

        public bool DrawGizmos = false;

        public bool AtmosphereEnabled = true;
        public bool RingEnabled = true;

        public float Amplitude = 32.0f;
        public float Frequency = 64.0f;

        public Mesh QuadMesh;

        public Shader ColorShader;

        public List<TerrainNode> TerrainNodes = new List<TerrainNode>(6);
        public List<TileSampler> TileSamplers = new List<TileSampler>();

        [HideInInspector]
        public double HeightZ = 0;

        public NoiseParametersSetter NPS = null;
        public TCCommonParametersSetter TCCPS = null;

        public Texture2D DetailedNormal;

        public Vector3 Offset { get; set; }

        #region ICelestialBody

        [SerializeField]
        private float radius = 2048;

        public float Radius { get { return radius; } set { radius = value; } }
        public Vector3 Origin { get; set; }
        public MaterialPropertyBlock MPB { get; set; }

        public List<string> GetKeywords()
        {
            var Keywords = new List<string>();

            if (Ring != null)
            {
                Keywords.Add(RingEnabled ? "RING_ON" : "RING_OFF");
                if (RingEnabled) Keywords.Add("SCATTERING");

                var shadowsCount = Shadows.Count((shadow) => shadow != null && Helper.Enabled(shadow));

                if (shadowsCount > 0)
                {
                    for (byte i = 0; i < shadowsCount; i++)
                    {
                        Keywords.Add("SHADOW_" + (i + 1));
                    }
                }
                else
                {
                    Keywords.Add("SHADOW_0");
                }
            }
            else
            {
                Keywords.Add("RING_OFF");
            }

            if (Atmosphere != null)
            {
                if (AtmosphereEnabled)
                {
                    var lightCount = Atmosphere.Suns.Count((sun) => sun != null && sun.gameObject.activeInHierarchy);

                    if (lightCount != 0)
                        Keywords.Add("LIGHT_" + lightCount);

                    if (Atmosphere.EclipseCasters.Count == 0)
                    {
                        Keywords.Add("ECLIPSES_OFF");
                    }
                    else
                    {
                        Keywords.Add(GodManager.Instance.Eclipses ? "ECLIPSES_ON" : "ECLIPSES_OFF");
                    }

                    if (Atmosphere.ShineCasters.Count == 0)
                    {
                        Keywords.Add("SHINE_OFF");
                    }
                    else
                    {
                        Keywords.Add(GodManager.Instance.Planetshine ? "SHINE_ON" : "SHINE_OFF");
                    }

                    Keywords.Add("ATMOSPHERE_ON");
                }
                else
                {
                    Keywords.Add("ATMOSPHERE_OFF");
                }

                if (Ocean != null)
                {
                    Keywords.Add("OCEAN_ON");
                }
                else
                {
                    Keywords.Add("OCEAN_OFF");
                }
            }
            else
            {
                Keywords.Add("ATMOSPHERE_OFF");
                Keywords.Add("OCEAN_OFF");
            }

            return Keywords;
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public void InitUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.InitUniforms(target);
            }
        }

        public void SetUniforms(MaterialPropertyBlock target)
        {
            if (target == null) return;

            if (Atmosphere != null)
            {
                Atmosphere.SetUniforms(target);
            }

            if (Ring != null)
            {
                Ring.SetShadows(MPB, Shadows);
            }
        }

        public void InitSetUniforms()
        {
            InitUniforms(MPB);
            SetUniforms(MPB);
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            if (Atmosphere != null)
            {
                if (Atmosphere.body == null)
                    Atmosphere.body = this;
            }

            if (Ocean != null)
            {
                if (Ocean.body == null)
                    Ocean.body = this;

                // TODO : Whhhhhhaaattaaaaaafuuuuckkk!
                StartCoroutine(Ocean.InitializationFix());
            }

            if (Ring != null)
            {
                if (Ring.body == null)
                    Ring.body = this;
            }

            // TODO : AAAAAAAAA CRAZY STUFF!
            var view = GodManager.Instance.View as PlanetView;
            if (view != null)
                view.Radius = Radius;

            QuadMesh = MeshFactory.MakePlane(GridResolution, GridResolution, MeshFactory.PLANE.XY, true, false, false);
            QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            TileSamplers = new List<TileSampler>(GetComponentsInChildren<TileSampler>());
            TileSamplers.Sort(new TileSampler.Sort());

            MPB = new MaterialPropertyBlock();

            Offset = new Vector3(0.0f, 0.0f, Radius);

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
                Ocean.Render();
            }

            if (Ring != null)
            {
                Ring.Render();
            }

            // NOTE : Update controller and the draw. This can help avoid terrain nodes jitter...
            GodManager.Instance.Controller.UpdateController();

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

            // TODO : PROFILE DAT SHIT!
            ReSetMPB();
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

        protected void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus != true) return;

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

            if (Atmosphere != null) Atmosphere.Reanimate();
            if (Ocean != null) Ocean.Reanimate();
        }

        private void DrawTerrain(TerrainNode node)
        {
            // Get all the samplers attached to the terrain node. The samples contain the data need to draw the quad
            var allSamplers = node.transform.GetComponentsInChildren<TileSampler>();
            var samplers = allSamplers.Where(sampler => sampler.enabled && sampler.StoreLeaf).ToList();

            if (samplers.Count == 0) return;

            // Find all the quads in the terrain node that need to be drawn
            FindDrawableQuads(node.TerrainQuadRoot, samplers);

            // The draw them
            DrawQuad(node, node.TerrainQuadRoot, samplers);

        }

        private bool FindDrawableSamplers(TerrainQuad quad, List<TileSampler> samplers)
        {
            for (short i = 0; i < samplers.Count; ++i)
            {
                var producer = samplers[i].Producer;

                if (producer.HasTile(quad.Level, quad.Tx, quad.Ty) && producer.FindTile(quad.Level, quad.Tx, quad.Ty, false, true) == null)
                {
                    return true;
                }
            }

            return false;
        }

        private void FindDrawableQuads(TerrainQuad quad, List<TileSampler> samplers)
        {
            quad.Drawable = false;

            if (!quad.IsVisible)
            {
                quad.Drawable = true;

                return;
            }

            if (quad.IsLeaf)
            {
                if (FindDrawableSamplers(quad, samplers)) return;
            }
            else
            {
                byte drawableCount = 0;

                for (byte i = 0; i < 4; ++i)
                {
                    FindDrawableQuads(quad.GetChild(i), samplers);

                    if (quad.GetChild(i).Drawable)
                    {
                        ++drawableCount;
                    }
                }

                if (drawableCount < 4)
                {
                    if (FindDrawableSamplers(quad, samplers)) return;
                }
            }

            quad.Drawable = true;
        }

        private void DrawNode(TerrainNode node)
        {
            // TODO : use mesh of appropriate resolution for non-leaf quads
            Graphics.DrawMesh(QuadMesh, Matrix4x4.identity, node.TerrainMaterial, 0, CameraHelper.Main(), 0, MPB);
        }

        private void ReSetMPB()
        {
            MPB.Clear();

            InitSetUniforms();
        }

        private void DrawQuad(TerrainNode node, TerrainQuad quad, List<TileSampler> samplers)
        {
            if (!quad.IsVisible) return;
            if (!quad.Drawable) return;

            if (quad.IsLeaf)
            {
                //ReSetMPB();

                for (int i = 0; i < samplers.Count; ++i)
                {
                    // Set the unifroms needed to draw the texture for this sampler
                    samplers[i].SetTile(MPB, quad.Level, quad.Tx, quad.Ty);
                }

                // Set the uniforms unique to each quad
                node.SetPerQuadUniforms(quad, MPB);

                DrawNode(node);
            }
            else
            {
                // Draw quads in a order based on distance to camera
                var done = 0;

                var order = quad.CalculateOrder(node.LocalCameraPosition.x, node.LocalCameraPosition.y, quad.Ox + quad.Length / 2.0, quad.Oy + quad.Length / 2.0);

                for (byte i = 0; i < 4; ++i)
                {
                    if (quad.GetChild(order[i]).Visibility == Frustum.VISIBILITY.INVISIBLE)
                    {
                        done |= (1 << order[i]);
                    }
                    else if (quad.GetChild(order[i]).Drawable)
                    {
                        DrawQuad(node, quad.GetChild(order[i]), samplers);

                        done |= (1 << order[i]);
                    }
                }

                if (done < 15)
                {
                    // If the a leaf quad needs to be drawn but its tiles are not ready then this will draw the next parent tile instead that is ready.
                    // Because of the current set up all tiles always have there tasks run on the frame they are generated so this section of code is never reached.

                    //ReSetMPB();

                    for (int i = 0; i < samplers.Count; ++i)
                    {
                        // Set the unifroms needed to draw the texture for this sampler
                        samplers[i].SetTile(MPB, quad.Level, quad.Tx, quad.Ty);
                    }

                    // Set the uniforms unique to each quad
                    node.SetPerQuadUniforms(quad, MPB);

                    DrawNode(node);
                }
            }
        }
    }
}