#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

using System;

using UnityEngine;

using ZFramework.Extensions;

public class QuadAABB
{
    public Vector3[] AABB { get; set; }
    public Vector3[] CullingAABB { get; set; }

    public Bounds Bounds { get; set; }

    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public QuadAABB(Vector3[] AABB, Vector3[] CullingAABB, Quad Quad, Transform OriginTransform = null)
    {
        this.AABB = new Vector3[AABB.Length];
        this.CullingAABB = new Vector3[CullingAABB.Length];

        var Max = default(Vector3);
        var Min = default(Vector3);

        this.Bounds = Quad.GetBoundFromPoints(AABB, out Max, out Min);

        this.Min = Min;
        this.Max = Max;

        Array.Copy(AABB, this.AABB, 8);
        Array.Copy(CullingAABB, this.CullingAABB, 14);

        if (OriginTransform != null)
        {
            AABB.ForEach((point) => OriginTransform.TransformPoint(point));
            CullingAABB.ForEach((point) => OriginTransform.TransformPoint(point));
        }
    }
}