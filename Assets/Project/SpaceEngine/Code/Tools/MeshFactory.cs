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
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = UnityEngine.Random;

public static class MeshFactory
{
    public enum PLANE { XY, XZ, YZ };

    public static Mesh MakeOceanPlane(int w, int h, float offset, float scale)
    {
        var vertices = new Vector3[w * h];
        var texcoords = new Vector2[w * h];
        var normals = new Vector3[w * h];
        var indices = new int[w * h * 6];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector2 uv = new Vector3((float)x / (float)(w - 1), (float)y / (float)(h - 1));

                uv.y *= scale;
                uv.y += offset;

                var p = Vector2.zero;

                p.x = (uv.x - 0.5f) * 2.0f;
                p.y = (uv.y - 0.5f) * 2.0f;

                var vertex = new Vector3(p.x, p.y, 0.0f);
                var normal = new Vector3(0.0f, 0.0f, 1.0f);

                texcoords[x + y * w] = uv;
                vertices[x + y * w] = vertex;
                normals[x + y * w] = normal;
            }
        }

        var num = 0;
        for (int x = 0; x < w - 1; x++)
        {
            for (int y = 0; y < h - 1; y++)
            {
                indices[num++] = x + y * w;
                indices[num++] = x + (y + 1) * w;
                indices[num++] = (x + 1) + y * w;

                indices[num++] = x + (y + 1) * w;
                indices[num++] = (x + 1) + (y + 1) * w;
                indices[num++] = (x + 1) + y * w;
            }
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.uv = texcoords;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.name = string.Format("OceanMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    public static Mesh MakePlane(int detail, PLANE plane, bool clampedUV, bool reversedIndices, bool generateBorders = false)
    {
        if (detail >= 255) { throw new ArgumentOutOfRangeException("detail", detail, "Detail can't be bigger or equal to 255!"); }

        var borders = generateBorders ? detail * 4 : 0;

        var vertices = new List<Vector3>(detail * detail + borders);
        var texcoords = new List<Vector2>(detail * detail + borders);
        var normals = new List<Vector3>(detail * detail + borders);
        var indices = new List<int>(detail * detail * 6);

        for (byte x = 0; x < detail; x++)
        {
            for (byte y = 0; y < detail; y++)
            {
                var texcoord = new Vector2((float)x / (float)(detail - 1), (float)y / (float)(detail - 1));
                var vertex = Vector3.zero;
                var normal = Vector3.zero;
                var uv = Vector2.zero;

                if (clampedUV)
                    uv = texcoord;
                else
                {
                    uv.x = (texcoord.x - 0.5f) * 2.0f;
                    uv.y = (texcoord.y - 0.5f) * 2.0f;
                }

                switch ((int)plane)
                {
                    case (int)PLANE.XY:
                        vertex = new Vector3(uv.x, uv.y, 0.0f);
                        normal = new Vector3(0.0f, 0.0f, 1.0f);
                        break;
                    case (int)PLANE.XZ:
                        vertex = new Vector3(uv.x, 0.0f, uv.y);
                        normal = new Vector3(0.0f, 1.0f, 0.0f);
                        break;
                    case (int)PLANE.YZ:
                        vertex = new Vector3(0.0f, uv.x, uv.y);
                        normal = new Vector3(1.0f, 0.0f, 0.0f);
                        break;
                }

                texcoords.Add(texcoord);
                vertices.Add(vertex);
                normals.Add(normal);
            }
        }

        for (byte x = 0; x < detail - 1; x++)
        {
            for (byte y = 0; y < detail - 1; y++)
            {
                if (reversedIndices)
                {
                    indices.Add(x + y * detail);
                    indices.Add(x + (y + 1) * detail);
                    indices.Add((x + 1) + y * detail);

                    indices.Add(x + (y + 1) * detail);
                    indices.Add((x + 1) + (y + 1) * detail);
                    indices.Add((x + 1) + y * detail);
                }
                else
                {
                    indices.Add(x + y * detail);
                    indices.Add((x + 1) + y * detail);
                    indices.Add(x + (y + 1) * detail);

                    indices.Add(x + (y + 1) * detail);
                    indices.Add((x + 1) + y * detail);
                    indices.Add((x + 1) + (y + 1) * detail);
                }
            }
        }

        indices.Reverse();

        #region Borders

        if (generateBorders)
        {
            List<int> borderIndices = new List<int>((borders - 4) * 6);

            var borderDepth = -0.625f;
            var startTriangle = detail * detail;

            for (byte col = 0; col < detail; col++)
            {
                int row = 0;

                Shift(ref vertices, ref normals, ref texcoords, startTriangle, borderDepth);

                if (col < detail - 1)
                {
                    borderIndices.Add(col * detail + row);
                    borderIndices.Add(startTriangle);
                    borderIndices.Add((col + 1) * detail + row);

                    borderIndices.Add((col + 1) * detail + row);
                    borderIndices.Add(startTriangle);
                    borderIndices.Add(startTriangle + 1);
                }

                startTriangle++;
            }

            for (byte row = 0; row < detail; row++)
            {
                int col = 0;

                Shift(ref vertices, ref normals, ref texcoords, startTriangle, borderDepth);

                if (row < detail - 1)
                {
                    borderIndices.Add(col * detail + row);
                    borderIndices.Add(col * detail + row + 1);
                    borderIndices.Add(startTriangle);

                    borderIndices.Add(startTriangle);
                    borderIndices.Add(col * detail + row + 1);
                    borderIndices.Add(startTriangle + 1);
                }

                startTriangle++;
            }

            for (byte col = 0; col < detail; col++)
            {
                int row = detail - 1;

                Shift(ref vertices, ref normals, ref texcoords, startTriangle, borderDepth);

                if (col < detail - 1)
                {
                    borderIndices.Add(col * detail + row);
                    borderIndices.Add((col + 1) * detail + row);
                    borderIndices.Add(startTriangle);

                    borderIndices.Add((col + 1) * detail + row);
                    borderIndices.Add(startTriangle + 1);
                    borderIndices.Add(startTriangle);
                }

                startTriangle++;
            }

            for (byte row = 0; row < detail; row++)
            {
                int col = detail - 1;

                Shift(ref vertices, ref normals, ref texcoords, startTriangle, borderDepth);

                if (row < detail - 1)
                {
                    borderIndices.Add(col * detail + row);
                    borderIndices.Add(startTriangle);
                    borderIndices.Add(col * detail + row + 1);

                    borderIndices.Add(startTriangle);
                    borderIndices.Add(startTriangle + 1);
                    borderIndices.Add(col * detail + row + 1);
                }

                startTriangle++;
            }

            borderIndices.Reverse();

            indices.AddRange(borderIndices);

            borderIndices.Clear();
        }

        #endregion

        var mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, texcoords);
        mesh.SetTriangles(indices, 0);
        mesh.SetNormals(normals);

        vertices.Clear();
        texcoords.Clear();
        indices.Clear();
        normals.Clear();

        mesh.name = string.Format("Plane_{1}_({0})", Random.Range(float.MinValue, float.MaxValue), plane);
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    private static void Shift(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> texcoords, int indexFrom, float depth)
    {
        vertices.Add(vertices[indexFrom] + normals[indexFrom] * depth);
        normals.Add(normals[indexFrom]);
        texcoords.Add(texcoords[indexFrom]);
    }

    public static Mesh SetupRingSegmentMesh(int SegmentCount, int SegmentDetail, float InnerRadius, float OuterRadius, float BoundsShift)
    {
        var mesh = new Mesh();

        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var angleTotal = Helper.Divide(Mathf.PI * 2.0f, SegmentCount);
        var angleStep = Helper.Divide(angleTotal, SegmentDetail);
        var coordStep = Helper.Reciprocal(SegmentDetail);

        for (var i = 0; i <= SegmentDetail; i++)
        {
            var coord = coordStep * i;
            var angle = angleStep * i;
            var sin = Mathf.Sin(angle);
            var cos = Mathf.Cos(angle);

            positions.Add(new Vector3(sin * InnerRadius, 0.0f, cos * InnerRadius));
            positions.Add(new Vector3(sin * OuterRadius, 0.0f, cos * OuterRadius));

            normals.Add(Vector3.up);
            normals.Add(Vector3.up);

            uvs.Add(new Vector2(0.0f, coord));
            uvs.Add(new Vector2(1.0f, coord));
        }

        #region Indices

        var steps = positions.Count / 2 - 1;

        for (var j = 0; j < steps; j++)
        {
            var vertexOff = j * 2;

            indices.Add(vertexOff + 0);
            indices.Add(vertexOff + 1);
            indices.Add(vertexOff + 2);
            indices.Add(vertexOff + 3);
            indices.Add(vertexOff + 2);
            indices.Add(vertexOff + 1);
        }

        #endregion

        #region Bounds

        var bounds = mesh.bounds;

        mesh.bounds = Helper.NewBoundsCenter(bounds, bounds.center + bounds.center.normalized * BoundsShift);

        #endregion

        mesh.SetVertices(positions);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(indices, 0);
        mesh.SetNormals(normals);

        positions.Clear();
        normals.Clear();
        uvs.Clear();
        indices.Clear();

        mesh.name = string.Format("RingSegmentMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    public static Mesh MakeBillboardQuad(float size)
    {
        Vector3[] Vertices =
        {
            new Vector3(1, 1, 0) * size,
            new Vector3(-1, 1, 0) * size,
            new Vector3(1, -1, 0) * size,
            new Vector3(-1, -1, 0) * size
        };

        Vector2[] uv =
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };

        var triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        var mesh = new Mesh();

        mesh.SetVertices(Vertices.ToList());
        mesh.SetUVs(0, uv.ToList());
        mesh.SetTriangles(triangles.ToList(), 0);

        mesh.RecalculateNormals();

        mesh.name = string.Format("BillboardMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    public static Vector3 SolveNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vertex1 = new Vector3(v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
        var vertex2 = new Vector3(v3.x - v2.x, v3.y - v2.y, v3.z - v2.z);

        return Vector3.Cross(vertex1, vertex2).normalized;
    }

    public static void SolveTangents(this Mesh theMesh, ref int[] indices, ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Vector2> uvs)
    {
        int vertexCount = vertices.Count;
        int triangleCount = indices.Length / 3;

        List<Vector4> tangents = new List<Vector4>(vertexCount);
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        int triangle = 0;

        for (int i = 0; i < triangleCount; i++)
        {
            int i1 = indices[triangle];
            int i2 = indices[triangle + 1];
            int i3 = indices[triangle + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
            Vector2 w1 = uvs[i1];
            Vector2 w2 = uvs[i2];
            Vector2 w3 = uvs[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;

            triangle += 3;
        }

        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            //Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize(ref n, ref t);

            tangents.Add(VectorHelper.MakeFrom(t, (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f));
        }

        theMesh.SetTangents(tangents);
    }
}