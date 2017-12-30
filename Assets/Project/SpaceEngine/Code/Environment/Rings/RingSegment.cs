#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

using SpaceEngine.Core.Patterns.Strategy.Renderable;

using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEngine.Environment.Rings
{
    public class RingSegment : MonoBehaviour, IRenderable<RingSegment>
    {
        public Ring Ring;

        #region IRenderable

        public void Render(int layer = 0)
        {
            if (Ring == null) return;
            if (Ring.RingSegmentMesh == null) return;
            if (Ring.RingMaterial == null) return;

            var segmentTRS = Matrix4x4.TRS(Ring.transform.position, transform.rotation, Vector3.one);

            Graphics.DrawMesh(Ring.RingSegmentMesh, segmentTRS, Ring.RingMaterial, layer, CameraHelper.Main(), 0, null, ShadowCastingMode.Off, false);
        }

        #endregion

        public void UpdateNode(Mesh mesh, Material material, Quaternion rotation)
        {
            if (Ring != null)
            {
                Helper.SetLocalRotation(transform, rotation);
            }
        }

        public static RingSegment Create(Ring ring)
        {
            var segmentGameObject = Helper.CreateGameObject("Segment", ring.transform);
            var segment = segmentGameObject.AddComponent<RingSegment>();

            segment.Ring = ring;

            return segment;
        }
    }
}