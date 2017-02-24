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

using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Samplers;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    public class CelestialBody : Node<CelestialBody>, ICelestialBody
    {
        public int GridResolution = 25;

        public Mesh QuadMesh;

        public Shader ColorShader;

        public List<TerrainNode> TerrainNodes = new List<TerrainNode>(6);
        public List<TileSampler> TileSamplers = new List<TileSampler>();

        public double HeightZ = 0;

        #region ICelestialBody

        [SerializeField]
        private float radius = 2048;

        public float Radius { get { return radius; } set { radius = value; } }
        public Vector3 Origin { get; set; }
        public MaterialPropertyBlock MPB { get; set; }

        public List<string> GetKeywords()
        {
            return new List<string>();
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            QuadMesh = MeshFactory.MakePlane(GridResolution, GridResolution, MeshFactory.PLANE.XY, true, false, false);
            QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            TileSamplers = new List<TileSampler>(GetComponentsInChildren<TileSampler>());
            TileSamplers.Sort(new TileSampler.Sort());

            MPB = new MaterialPropertyBlock();

            Origin = new Vector3(0.0f, 0.0f, Radius);
        }

        protected override void UpdateNode()
        {
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

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetMatrix("_Globals_WorldToCamera", GodManager.Instance.WorldToCamera);
            mat.SetMatrix("_Globals_CameraToWorld", GodManager.Instance.CameraToWorld);
            mat.SetMatrix("_Globals_CameraToScreen", GodManager.Instance.CameraToScreen);
            mat.SetMatrix("_Globals_ScreenToCamera", GodManager.Instance.ScreenToCamera);
            mat.SetVector("_Globals_WorldCameraPos", GodManager.Instance.WorldCameraPos);
            mat.SetVector("_Globals_Origin", Origin);
            mat.SetFloat("_Exposure", 0.2f);
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

        private void DrawQuad(TerrainNode node, TerrainQuad quad, List<TileSampler> samplers)
        {
            if (!quad.IsVisible) return;
            if (!quad.Drawable) return;

            if (quad.IsLeaf)
            {
                MPB.Clear();

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
                var order = new byte[4];

                var cameraX = node.LocalCameraPosition.x;
                var cameraY = node.LocalCameraPosition.y;
                var quadX = quad.Oy + quad.Length / 2.0;
                var quadY = quad.Oy + quad.Length / 2.0;

                if (cameraY < quadY)
                {
                    if (cameraX < quadX)
                    {
                        order[0] = 0;
                        order[1] = 1;
                        order[2] = 2;
                        order[3] = 3;
                    }
                    else
                    {
                        order[0] = 1;
                        order[1] = 0;
                        order[2] = 3;
                        order[3] = 2;
                    }
                }
                else
                {
                    if (cameraX < quadX)
                    {
                        order[0] = 2;
                        order[1] = 0;
                        order[2] = 3;
                        order[3] = 1;
                    }
                    else
                    {
                        order[0] = 3;
                        order[1] = 1;
                        order[2] = 2;
                        order[3] = 0;
                    }
                }

                var done = 0;

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

                    MPB.Clear();

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