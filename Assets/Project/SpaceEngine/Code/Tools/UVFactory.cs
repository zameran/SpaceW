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

using UnityEngine;

public static class UVFactory
{
    public static Vector2 GetSgtSphericalUV(Vector3 vertex)
    {
        return VectorHelper.CartesianToPolarUV(vertex);
    }

    public static Vector2 GetSurfaceUV(int detail, int col, int row)
    {
        return new Vector2((float)row / detail, (float)col / detail);
    }

    public static Vector2 GetContinuousUV(int detail, int col, int row, float uvResolution, float uvStartX, float uvStartY)
    {
        return new Vector2(uvStartX + ((float)row / (detail - 1)) * uvResolution,
                          (uvStartY + ((float)col / (detail - 1)) * uvResolution));
    }

    public static Vector2 GetSphericalUV(int detail, int col, int row, Vector3 vertex, bool staticX, bool staticY)
    {
        Vector2 uv = new Vector2();

        uv.x = -(Mathf.Atan2(vertex.x, vertex.z) / (2f * Mathf.PI) + 0.5f);
        uv.y = (Mathf.Asin(vertex.y) / Mathf.PI + .5f);

        if (staticX)
        {
            if (vertex.x < 0)
            {
                if ((row == detail - 1) && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 0;
                if ((row == 0) && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
            }
        }
        else if (staticY)
        {
            if (vertex.y > 0)
            {
                if ((col == detail - 1) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 0;
                if ((col == 0) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
            }
            else
            {
                if ((col == detail - 1) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
                if ((col == 0) && vertex.x < 0 && vertex.z < 0.01f && vertex.z > -0.01f) uv.x = 0;
            }
        }
        return uv;
    }
}