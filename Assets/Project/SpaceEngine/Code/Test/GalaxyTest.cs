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
using SpaceEngine.Core.Utilities.Gradients;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SpaceEngine.Tests
{
    public struct GalaxyStar
    {
        public Vector3 position;
        public float size;
        public Vector4 color;
    }

    public class GalaxyTest : Node<GalaxyTest>
    {
        public Texture2D PermutationTableTexture;

        public ComputeShader Core;

        public Shader CoreShader;
        
        public int Count = 128;
        public int DrawCount = 128;

        private Material StarsMaterial;
        private ComputeBuffer StarsBuffer;

        public float Radius = 128.0f;
        public float RadiusEllipse = 1.0f;
        public float SizeBar = 0.3f;
        public float Depth = 128.0f;
        public float InverseSpiralEccentricity = 0.7f;
        public float SpiralRotation = 23.5f;

        public Vector3 Randomize = Vector3.zero;

        public Vector2 Warp1 = new Vector2(0.3f, 0.3f);     // Pre-rotation warp vector...
        public Vector2 Warp2 = new Vector2(0.01f, 0.01f);   // Post-rotation warp vector...

        public ColorMaterialTableGradientLut ColorDistribution = new ColorMaterialTableGradientLut();

        protected override void InitNode()
        {
            StarsMaterial = MaterialHelper.CreateTemp(CoreShader, "Stars");

            StarsBuffer = new ComputeBuffer(Count, Marshal.SizeOf<GalaxyStar>(), ComputeBufferType.Default);
            StarsBuffer.SetData(new GalaxyStar[Count]);

            ColorDistribution.GenerateLut();
        }

        protected override void UpdateNode()
        {
            Core.SetTexture(0, "PermutationTable", PermutationTableTexture);
            Core.SetTexture(0, "MaterialTable", ColorDistribution.Lut);

            Core.SetVector("Randomize", Randomize);
            Core.SetVector("sizeParams1", new Vector4(Radius, RadiusEllipse, SizeBar, Depth));
            Core.SetVector("warpParams1", new Vector4(Warp1.x, Warp1.y, Warp2.x, Warp2.y));
            Core.SetVector("spiralParams1", new Vector4(InverseSpiralEccentricity, SpiralRotation, 0.0f, 0.0f));

            Core.SetBuffer(0, "output", StarsBuffer);
            Core.Dispatch(0, (int)(Count / 1024.0f), 1, 1);
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            StarsBuffer.ReleaseAndDisposeBuffer();

            Helper.Destroy(StarsMaterial);

            ColorDistribution.DestroyLut();
        }

        protected void OnPostRender()
        {
            StarsMaterial.SetPass(0);
            StarsMaterial.SetBuffer("stars", StarsBuffer);

            Graphics.DrawProcedural(MeshTopology.Points, Math.Abs(DrawCount));
        }
    }
}