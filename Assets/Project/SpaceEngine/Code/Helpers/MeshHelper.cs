#region License

// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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

using System.Collections.Generic;
using UnityEngine;

namespace SpaceEngine.Helpers
{
    /// <summary>
    ///     Class - extensions holder for a <see cref="Mesh" />.
    /// </summary>
    public static class MeshHelper
    {
        public static Mesh DuplicateMesh(Mesh mesh)
        {
            return Object.Instantiate(mesh);
        }

        /// <summary>
        ///     Taken from wiki. Provides some <see cref="Mesh" />'s subdivision API.
        /// </summary>
        public static class SubdivisionHelper
        {
            private static List<Vector3> vertices;
            private static List<Vector3> normals;
            private static List<Color> colors;
            private static List<Vector2> uv1;
            private static List<Vector2> uv2;
            private static List<Vector2> uv3;

            private static List<int> indices;
            private static Dictionary<uint, int> newVectices;

            private static void InitArrays(Mesh mesh)
            {
                vertices = new List<Vector3>(mesh.vertices);
                normals = new List<Vector3>(mesh.normals);
                colors = new List<Color>(mesh.colors);
                uv1 = new List<Vector2>(mesh.uv);
                uv2 = new List<Vector2>(mesh.uv2);
                uv3 = new List<Vector2>(mesh.uv3);
                indices = new List<int>();
            }

            private static void CleanUp()
            {
                vertices = null;
                normals = null;
                colors = null;
                uv1 = null;
                uv2 = null;
                uv3 = null;
                indices = null;
            }

            #region Subdivide

            /// <summary>
            ///     This functions subdivides the mesh based on the level parameter
            ///     Note that only the 4 and 9 subdivides are supported so only those divides
            ///     are possible. [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
            ///     The function tried to approximate the desired level
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="level">
            ///     Should be a number made up of (2^x * 3^y)
            ///     [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
            /// </param>
            public static void Subdivide(Mesh mesh, int level)
            {
                if (level < 2)
                {
                    return;
                }

                while (level > 1)
                {
                    while (level % 3 == 0)
                    {
                        Subdivide9(mesh);
                        level /= 3;
                    }

                    while (level % 2 == 0)
                    {
                        Subdivide4(mesh);
                        level /= 2;
                    }

                    if (level > 3)
                    {
                        level++;
                    }
                }
            }

            #endregion Subdivide

            #region Subdivide4 (2x2)

            private static int GetNewVertex4(int i1, int i2)
            {
                var newIndex = vertices.Count;

                var t1 = ((uint)i1 << 16) | (uint)i2;
                var t2 = ((uint)i2 << 16) | (uint)i1;

                if (newVectices.ContainsKey(t2))
                {
                    return newVectices[t2];
                }

                if (newVectices.ContainsKey(t1))
                {
                    return newVectices[t1];
                }

                newVectices.Add(t1, newIndex);
                vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);

                if (normals.Count > 0)
                {
                    normals.Add((normals[i1] + normals[i2]).normalized);
                }

                if (colors.Count > 0)
                {
                    colors.Add((colors[i1] + colors[i2]) * 0.5f);
                }

                if (uv1.Count > 0)
                {
                    uv1.Add((uv1[i1] + uv1[i2]) * 0.5f);
                }

                if (uv2.Count > 0)
                {
                    uv2.Add((uv2[i1] + uv2[i2]) * 0.5f);
                }

                if (uv3.Count > 0)
                {
                    uv3.Add((uv3[i1] + uv3[i2]) * 0.5f);
                }

                return newIndex;
            }

            /// <summary>
            ///     Devides each triangles into 4. A quad(2 tris) will be splitted into 2x2 quads( 8 tris )
            /// </summary>
            /// <param name="mesh"></param>
            public static void Subdivide4(Mesh mesh)
            {
                newVectices = new Dictionary<uint, int>();

                InitArrays(mesh);

                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var i1 = triangles[i + 0];
                    var i2 = triangles[i + 1];
                    var i3 = triangles[i + 2];

                    var a = GetNewVertex4(i1, i2);
                    var b = GetNewVertex4(i2, i3);
                    var c = GetNewVertex4(i3, i1);

                    indices.Add(i1);
                    indices.Add(a);
                    indices.Add(c);
                    indices.Add(i2);
                    indices.Add(b);
                    indices.Add(a);
                    indices.Add(i3);
                    indices.Add(c);
                    indices.Add(b);
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c); // center triangle
                }

                mesh.vertices = vertices.ToArray();

                if (normals.Count > 0)
                {
                    mesh.normals = normals.ToArray();
                }

                if (colors.Count > 0)
                {
                    mesh.colors = colors.ToArray();
                }

                if (uv1.Count > 0)
                {
                    mesh.uv = uv1.ToArray();
                }

                if (uv2.Count > 0)
                {
                    mesh.uv2 = uv2.ToArray();
                }

                if (uv3.Count > 0)
                {
                    mesh.uv3 = uv3.ToArray();
                }

                mesh.triangles = indices.ToArray();

                CleanUp();
            }

            #endregion Subdivide4 (2x2)

            #region Subdivide9 (3x3)

            private static int GetNewVertex9(int i1, int i2, int i3)
            {
                var newIndex = vertices.Count;

                if (i3 == i1 || i3 == i2)
                {
                    var t1 = ((uint)i1 << 16) | (uint)i2;

                    if (newVectices.ContainsKey(t1))
                    {
                        return newVectices[t1];
                    }

                    newVectices.Add(t1, newIndex);
                }

                vertices.Add((vertices[i1] + vertices[i2] + vertices[i3]) / 3.0f);

                if (normals.Count > 0)
                {
                    normals.Add((normals[i1] + normals[i2] + normals[i3]).normalized);
                }

                if (colors.Count > 0)
                {
                    colors.Add((colors[i1] + colors[i2] + colors[i3]) / 3.0f);
                }

                if (uv1.Count > 0)
                {
                    uv1.Add((uv1[i1] + uv1[i2] + uv1[i3]) / 3.0f);
                }

                if (uv2.Count > 0)
                {
                    uv2.Add((uv2[i1] + uv2[i2] + uv2[i3]) / 3.0f);
                }

                if (uv3.Count > 0)
                {
                    uv3.Add((uv3[i1] + uv3[i2] + uv3[i3]) / 3.0f);
                }

                return newIndex;
            }

            /// <summary>
            ///     Devides each triangles into 9. A quad(2 tris) will be splitted into 3x3 quads( 18 tris )
            /// </summary>
            /// <param name="mesh"></param>
            public static void Subdivide9(Mesh mesh)
            {
                newVectices = new Dictionary<uint, int>();

                InitArrays(mesh);

                var triangles = mesh.triangles;
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    var i1 = triangles[i + 0];
                    var i2 = triangles[i + 1];
                    var i3 = triangles[i + 2];

                    var a1 = GetNewVertex9(i1, i2, i1);
                    var a2 = GetNewVertex9(i2, i1, i2);
                    var b1 = GetNewVertex9(i2, i3, i2);
                    var b2 = GetNewVertex9(i3, i2, i3);
                    var c1 = GetNewVertex9(i3, i1, i3);
                    var c2 = GetNewVertex9(i1, i3, i1);

                    var d = GetNewVertex9(i1, i2, i3);

                    indices.Add(i1);
                    indices.Add(a1);
                    indices.Add(c2);
                    indices.Add(i2);
                    indices.Add(b1);
                    indices.Add(a2);
                    indices.Add(i3);
                    indices.Add(c1);
                    indices.Add(b2);
                    indices.Add(d);
                    indices.Add(a1);
                    indices.Add(a2);
                    indices.Add(d);
                    indices.Add(b1);
                    indices.Add(b2);
                    indices.Add(d);
                    indices.Add(c1);
                    indices.Add(c2);
                    indices.Add(d);
                    indices.Add(c2);
                    indices.Add(a1);
                    indices.Add(d);
                    indices.Add(a2);
                    indices.Add(b1);
                    indices.Add(d);
                    indices.Add(b2);
                    indices.Add(c1);
                }

                mesh.vertices = vertices.ToArray();

                if (normals.Count > 0)
                {
                    mesh.normals = normals.ToArray();
                }

                if (colors.Count > 0)
                {
                    mesh.colors = colors.ToArray();
                }

                if (uv1.Count > 0)
                {
                    mesh.uv = uv1.ToArray();
                }

                if (uv2.Count > 0)
                {
                    mesh.uv2 = uv2.ToArray();
                }

                if (uv3.Count > 0)
                {
                    mesh.uv3 = uv3.ToArray();
                }

                mesh.triangles = indices.ToArray();

                CleanUp();
            }

            #endregion Subdivide9 (3x3)
        }
    }
}