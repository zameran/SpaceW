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
// Creation Date: 2017.10.21
// Creation Time: 12:21 PM
// Creator: zameran
#endregion

using UnityEngine;

namespace SpaceEngine.Galaxy
{
    [RequireComponent(typeof(Camera))]
    public class GalaxyRenderer : MonoSingleton<GalaxyRenderer>
    {
        internal enum RenderType : byte
        {
            Realistic,
            DebugStars,
            DebugDust,
            DebugFilterDust,
            DebugFilterGas,
            OnlyDust,
            Gizmos,
            None
        }

        [SerializeField]
        internal GalaxyGenerator Galaxy;

        [SerializeField]
        internal RenderType RenderMethod = RenderType.Realistic;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            
        }

        private void Update()
        {

        }

        private void OnDrawGizmos()
        {
            if (Galaxy == null || !Helper.Enabled(Galaxy)) return;

            if (RenderMethod == RenderType.Gizmos)
            {
                if (Galaxy.Octree == null) return;

                Galaxy.Octree.DrawAllBounds();

                var chunks = Galaxy.Octree.GetNearbyNodes(transform.position, 32);

                for (var chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(chunks[chunkIndex].Center, Vector3.one * chunks[chunkIndex].SideLength);
                }
            }

            Gizmos.color = Color.white;
        }

        private void OnPreRender()
        {
            if (Galaxy == null || !Helper.Enabled(Galaxy)) return;

            if (RenderMethod == RenderType.Realistic)
            {
                Galaxy.RenderDustToFrameBuffer();
            }
            else if (RenderMethod == RenderType.DebugStars)
            {
                // NOTE : Nothing to draw...
            }
            else if (RenderMethod == RenderType.DebugDust)
            {
                // NOTE : Nothing to draw...
            }
            else if (RenderMethod == RenderType.OnlyDust)
            {
                Galaxy.RenderDustToFrameBuffer();
            }
        }

        private void OnPostRender()
        {
            if (Galaxy == null || !Helper.Enabled(Galaxy)) return;

            if (RenderMethod == RenderType.Realistic)
            {
                Galaxy.RenderDustToScreenBuffer();
                Galaxy.RenderStars(0); // NOTE : Render stars with billboards...
            }
            else if (RenderMethod == RenderType.DebugStars)
            {
                Galaxy.RenderDebugStars();  // NOTE : Render stars as dots...
            }
            else if (RenderMethod == RenderType.DebugDust)
            {
                Galaxy.RenderDustDebug();   // NOTE : Render dust as dots...
            }
            else if (RenderMethod == RenderType.DebugFilterDust)
            {
                Galaxy.RenderAppendDust(1);
            }
            else if (RenderMethod == RenderType.DebugFilterGas)
            {
                Galaxy.RenderAppendGas(1);
            }
            else if (RenderMethod == RenderType.OnlyDust)
            {
                Galaxy.RenderDustToScreenBuffer();
            }
        }
    }
}