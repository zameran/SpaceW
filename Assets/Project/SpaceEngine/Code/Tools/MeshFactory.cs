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

using System.Collections.Generic;

using UnityEngine;

public static class MeshFactory
{
    public enum PLANE { XY, XZ, YZ };

    public static Mesh SetupQuadMesh()
    {
        int nVerts = QuadSettings.Vertices;
        int nVertsPerEdge = QuadSettings.VerticesPerSide;

        Vector3[] dummyVerts = new Vector3[nVerts];
        Vector2[] uv0 = new Vector2[nVerts];

        int[] triangles = new int[(nVertsPerEdge - 1) * (nVertsPerEdge - 1) * 2 * 3];

        float height = 0;

        for (int r = 0; r < nVertsPerEdge; r++)
        {
            int rowStartID = r * nVertsPerEdge;

            for (int c = 0; c < nVertsPerEdge; c++)
            {
                int vertID = rowStartID + c;

                dummyVerts[vertID] = new Vector3(c, height, r);

                Vector2 uv = new Vector2();

                uv.x = r / (float)(nVertsPerEdge - 1);
                uv.y = c / (float)(nVertsPerEdge - 1);

                uv0[vertID] = uv;
            }
        }

        int triangleIndex = 0;

        for (int r = 0; r < nVertsPerEdge - 1; r++)
        {
            int rowStartID = r * nVertsPerEdge;
            int rowAboveStartID = (r + 1) * nVertsPerEdge;

            for (int c = 0; c < nVertsPerEdge - 1; c++)
            {
                int vertID = rowStartID + c;
                int vertAboveID = rowAboveStartID + c;

                triangles[triangleIndex++] = vertID;
                triangles[triangleIndex++] = vertAboveID;
                triangles[triangleIndex++] = vertAboveID + 1;

                triangles[triangleIndex++] = vertID;
                triangles[triangleIndex++] = vertAboveID + 1;
                triangles[triangleIndex++] = vertID + 1;
            }
        }

        Mesh dummyMesh = new Mesh();
        dummyMesh.vertices = dummyVerts;
        dummyMesh.uv = uv0;
        dummyMesh.SetTriangles(triangles, 0);
        dummyMesh.name = string.Format("PrototypeMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
        dummyMesh.hideFlags = HideFlags.DontSave;

        return dummyMesh;
    }

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

                var position = new Vector3(p.x, p.y, 0.0f);
                var normal = new Vector3(0.0f, 0.0f, 1.0f);

                texcoords[x + y * w] = uv;
                vertices[x + y * w] = position;
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

    public static Mesh MakePlane(int w, int h, PLANE plane, bool _01, bool cw, bool invert)
    {
        Vector3[] vertices = new Vector3[w * h];
        Vector2[] texcoords = new Vector2[w * h];
        Vector3[] normals = new Vector3[w * h];
        int[] indices = new int[w * h * 6];

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Vector2 uv = new Vector3((float)x / (float)(w - 1), (float)y / (float)(h - 1));
                Vector2 p = new Vector2();

                if (_01)
                    p = uv;
                else
                {
                    p.x = (uv.x - 0.5f) * 2.0f;
                    p.y = (uv.y - 0.5f) * 2.0f;
                }

                Vector3 pos = new Vector3(), norm = new Vector3();

                switch ((int)plane)
                {
                    case (int)PLANE.XY:
                        if (!invert)
                        {
                            pos = new Vector3(p.x, p.y, 0.0f);
                            norm = new Vector3(0.0f, 0.0f, 1.0f);
                        }
                        else
                        {
                            pos = new Vector3(-p.x, -p.y, 0.0f);
                            norm = new Vector3(0.0f, 0.0f, -1.0f);
                        }
                        break;
                    case (int)PLANE.XZ:
                        if (!invert)
                        {
                            pos = new Vector3(p.x, 0.0f, p.y);
                            norm = new Vector3(0.0f, 1.0f, 0.0f);
                            break;
                        }
                        else
                        {
                            pos = new Vector3(-p.x, 0.0f, -p.y);
                            norm = new Vector3(0.0f, -1.0f, 0.0f);
                        }
                        break;
                    case (int)PLANE.YZ:
                        if (!invert)
                        {
                            pos = new Vector3(0.0f, p.x, p.y);
                            norm = new Vector3(1.0f, 0.0f, 0.0f);
                        }
                        else
                        {
                            pos = new Vector3(0.0f, -p.x, -p.y);
                            norm = new Vector3(-1.0f, 0.0f, 0.0f);
                        }
                        break;
                }

                texcoords[x + y * w] = uv;
                vertices[x + y * w] = pos;
                normals[x + y * w] = norm;
            }
        }

        int num = 0;
        for (int x = 0; x < w - 1; x++)
        {
            for (int y = 0; y < h - 1; y++)
            {
                if (cw)
                {
                    indices[num++] = x + y * w;
                    indices[num++] = x + (y + 1) * w;
                    indices[num++] = (x + 1) + y * w;

                    indices[num++] = x + (y + 1) * w;
                    indices[num++] = (x + 1) + (y + 1) * w;
                    indices[num++] = (x + 1) + y * w;
                }
                else
                {
                    indices[num++] = x + y * w;
                    indices[num++] = (x + 1) + y * w;
                    indices[num++] = x + (y + 1) * w;

                    indices[num++] = x + (y + 1) * w;
                    indices[num++] = (x + 1) + y * w;
                    indices[num++] = (x + 1) + (y + 1) * w;
                }
            }
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.uv = texcoords;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
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

        mesh.vertices = positions.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        #region Indices

        var steps = mesh.vertexCount / 2 - 1;

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

        mesh.triangles = indices.ToArray();

        #endregion

        #region Bounds

        var bounds = mesh.bounds;

        mesh.bounds = Helper.NewBoundsCenter(bounds, bounds.center + bounds.center.normalized * BoundsShift);

        #endregion

        positions.Clear();
        normals.Clear();
        uvs.Clear();
        indices.Clear();

        mesh.hideFlags = HideFlags.DontSave;

        return mesh;
    }

    public static Mesh MakeBillboardQuad(float size)
    {
        Vector3[] Vertices =
        {
            new Vector3(1, 1, 0) * size, new Vector3(-1, 1, 0) * size, new Vector3(1, -1, 0) * size, new Vector3(-1, -1, 0) * size
        };

        Vector2[] uv =
        {
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0)
        };

        var triangles = new int[6]
        {
            0, 2, 1, 2, 3, 1
        };

        var m = new Mesh
        {
            vertices = Vertices,
            uv = uv,
            triangles = triangles,
            name = string.Format("BillboardMesh_({0})", Random.Range(float.MinValue, float.MaxValue)),
            hideFlags = HideFlags.DontSave
        };

        m.RecalculateNormals();

        return m;
    }

    public static Vector3 SolveNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 vt1 = new Vector3(v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
        Vector3 vt2 = new Vector3(v3.x - v2.x, v3.y - v2.y, v3.z - v2.z);

        return Vector3.Cross(vt1, vt2).normalized;
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