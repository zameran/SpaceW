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
// Creation Date: 2017.12.09
// Creation Time: 4:11 PM
// Creator: zameran
#endregion

using SpaceEngine.Galaxy;

namespace SpaceEngine.Debugging
{
    public class DebugDrawOctree : DebugDraw
    {
        private readonly int[][] ORDER = new[]
        {
            new int[] { 5, 1, 1, 7, 7, 3, 3, 5 },
            new int[] { 2, 6, 6, 4, 4, 0, 0, 2 },
            new int[] { 5, 2, 1, 6, 7, 4, 3, 0 }
        };

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnPostRender()
        {
            base.OnPostRender();
        }

        protected override void CreateLineMaterial()
        {
            base.CreateLineMaterial();
        }

        protected override void Draw()
        {
            var target = GalaxyRenderer.Instance.Galaxy.Octree;

            if (target == null) return;

            target.DrawOutline(CameraHelper.Main(), lineMaterial, ORDER);
        }
    }
}