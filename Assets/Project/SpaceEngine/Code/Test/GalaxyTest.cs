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
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
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
// Creation Date: 2017.10.02
// Creation Time: 6:49 PM
// Creator: zameran
#endregion

using SpaceEngine.Core;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Utilities.Gradients;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SpaceEngine.Tests
{
    [Serializable]
    public struct GalaxyStar
    {
        public Vector3 position;
        public float size;
        public Vector4 color;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GalaxyGenerationParameters
    {
        public Vector3 Randomize;                   // Randomation vector...
        public Vector3 Offset;                      // Linear offset for ellipses - asymmetry factor...

        public Vector4 Warp;                        // Pre/Pos-rotation warp 2D vector packed...

        public float Radius;                        // Galaxy radius...
        public float RadiusEllipse;
        public float SizeBar;
        public float Depth;
        public float InverseSpiralEccentricity;
        public float SpiralRotation;

        public GalaxyGenerationParameters(GalaxyGenerationParameters from)
        {
            Randomize = from.Randomize;
            Offset = from.Offset;

            Warp = from.Warp;

            Radius = from.Radius;
            RadiusEllipse = from.RadiusEllipse;
            SizeBar = from.SizeBar;
            Depth = from.Depth;
            InverseSpiralEccentricity = from.InverseSpiralEccentricity;
            SpiralRotation = from.SpiralRotation;
        }

        public GalaxyGenerationParameters(Vector3 randomize, Vector3 offset, Vector4 warp,
                                          float radius, float radiusEllipse, float sizeBar, float depth, float inverseSpiralEccentricity,
                                          float spiralRotation)
        {
            Randomize = randomize;
            Offset = offset;

            Warp = warp;

            Radius = radius;
            RadiusEllipse = radiusEllipse;
            SizeBar = sizeBar;
            Depth = depth;
            InverseSpiralEccentricity = inverseSpiralEccentricity;
            SpiralRotation = spiralRotation;
        }

        public static GalaxyGenerationParameters Default
        {
            get
            {
                return new GalaxyGenerationParameters(Vector3.zero, 
                                                      new Vector3(0.0f, 0.0f, 0.03f), 
                                                      new Vector4(0.3f, 0.15f, 0.025f, 0.01f),
                                                      128.0f, 0.7f, -0.25f, 32.0f, 0.75f, 6.2832f);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GalaxyGenerationPerPassParameters
    {
        public float PassRotation;

        public GalaxyGenerationPerPassParameters(GalaxyGenerationPerPassParameters from)
        {
            PassRotation = from.PassRotation;
        }

        public GalaxyGenerationPerPassParameters(float passRotation)
        {
            PassRotation = passRotation;
        }

        public static GalaxyGenerationPerPassParameters Default
        {
            get
            {
                GalaxyGenerationPerPassParameters ggppp = new GalaxyGenerationPerPassParameters(Mathf.PI / 2.0f);

                return ggppp;
            }
        }
    }

    public class GalaxyTest : Node<GalaxyTest>, IRenderable<GalaxyTest>
    {
        public ComputeShader Core;

        public Shader CoreShader;

        public int PassCount = 1;
        public int Count = 128;
        public int DrawCount = 128;

        private Material StarsMaterial;

        private List<ComputeBuffer> StarsBuffers = new List<ComputeBuffer>();

        public GalaxyGenerationParameters Parameters = GalaxyGenerationParameters.Default;
        public GalaxyGenerationPerPassParameters ParametersPerPass = GalaxyGenerationPerPassParameters.Default;

        public ColorMaterialTableGradientLut ColorDistribution = new ColorMaterialTableGradientLut();

        public bool AutoUpdate = false;

        #region Galaxy

        public void InitBuffers()
        {
            if (StarsBuffers != null)
            {
                if (StarsBuffers.Count > 0) DestroyBuffers();
            }

            StarsBuffers = new List<ComputeBuffer>(PassCount);

            for (var bufferIndex = 0; bufferIndex < StarsBuffers.Capacity; bufferIndex++)
            {
                var buffer = new ComputeBuffer(Count, Marshal.SizeOf<GalaxyStar>(), ComputeBufferType.Default);

                buffer.SetData(new GalaxyStar[Count]);

                StarsBuffers.Add(buffer);
            }
        }

        public void GenerateBuffers()
        {
            Core.SetTexture(0, "MaterialTable", ColorDistribution.Lut);

            for (var bufferIndex = 0; bufferIndex < StarsBuffers.Count; bufferIndex++)
            {
                var buffer = StarsBuffers[bufferIndex];
                var perPassRotation = bufferIndex % 2 == 0 ? ParametersPerPass.PassRotation * bufferIndex : ParametersPerPass.PassRotation;

                Core.SetVector("Randomize", (Parameters.Randomize + new Vector3(1.0f, 0.0f, 1.0f)) * ((bufferIndex + 1) / 10.0f));
                Core.SetVector("offsetParams1", new Vector4(Parameters.Offset.x, Parameters.Offset.y, Parameters.Offset.z, 0.0f));
                Core.SetVector("sizeParams1", new Vector4(Parameters.Radius, Parameters.RadiusEllipse, Parameters.SizeBar, Parameters.Depth));
                Core.SetVector("warpParams1", Parameters.Warp);
                Core.SetVector("spiralParams1", new Vector4(Parameters.InverseSpiralEccentricity, Parameters.SpiralRotation, perPassRotation, 0.0f));

                Core.SetBuffer(0, "output", buffer);
                Core.Dispatch(0, (int)(Count / 1024.0f), 1, 1);
            }
        }

        protected void DestroyBuffers()
        {
            for (var bufferIndex = 0; bufferIndex < StarsBuffers.Count; bufferIndex++)
            {
                var buffer = StarsBuffers[bufferIndex];

                buffer.ReleaseAndDisposeBuffer();
            }
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            StarsMaterial = MaterialHelper.CreateTemp(CoreShader, "Stars");

            ColorDistribution.GenerateLut();

            InitBuffers();
            GenerateBuffers();
        }

        protected override void UpdateNode()
        {
            if (AutoUpdate) GenerateBuffers();
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

            DestroyBuffers();

            Helper.Destroy(StarsMaterial);

            ColorDistribution.DestroyLut();
        }

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 8)
        {
            // NOTE : Render all galaxy stuff here...

            for (var bufferIndex = 0; bufferIndex < StarsBuffers.Count; bufferIndex++)
            {
                var buffer = StarsBuffers[bufferIndex];

                StarsMaterial.SetPass(0);
                StarsMaterial.SetBuffer("stars", buffer);

                Graphics.DrawProcedural(MeshTopology.Points, Math.Abs(DrawCount));
            }
        }

        #endregion
    }
}