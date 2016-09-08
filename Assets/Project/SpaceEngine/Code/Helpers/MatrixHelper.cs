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

public static class MatrixHelper
{
    public static Matrix4x4 Rotation(Quaternion q)
    {
        return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
    }

    public static Matrix4x4 Translation(Vector3 xyz)
    {
        var matrix = Matrix4x4.identity;

        matrix.m03 = xyz.x;
        matrix.m13 = xyz.y;
        matrix.m23 = xyz.z;

        return matrix;
    }

    public static Matrix4x4 Scaling(Vector3 xyz)
    {
        var matrix = Matrix4x4.identity;

        matrix.m00 = xyz.x;
        matrix.m11 = xyz.y;
        matrix.m22 = xyz.z;

        return matrix;
    }

    public static Matrix4x4 ShearingX(Vector2 yz)
    {
        var matrix = Matrix4x4.identity;

        matrix.m01 = yz.x;
        matrix.m02 = yz.y;

        return matrix;
    }

    public static Matrix4x4 ShearingY(Vector2 xz)
    {
        var matrix = Matrix4x4.identity;

        matrix.m10 = xz.x;
        matrix.m12 = xz.y;

        return matrix;
    }

    public static Matrix4x4 ShearingZ(Vector2 xy)
    {
        var matrix = Matrix4x4.identity;

        matrix.m20 = xy.x;
        matrix.m21 = xy.y;

        return matrix;
    }
}