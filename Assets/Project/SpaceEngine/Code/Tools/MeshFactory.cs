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

using System.Collections.Generic;

using UnityEngine;

public static class MeshFactory
{
    public enum PLANE { XY, XZ, YZ };

    public static Mesh SetupQuadColliderMesh(OutputStruct[] outputStructData)
    {
        int nVerts = QuadSettings.nVerts;
        int nVertsPerEdge = QuadSettings.nVertsPerEdge;

        Vector3[] dummyVerts = new Vector3[nVerts];

        int[] triangles = new int[(nVertsPerEdge - 1) * (nVertsPerEdge - 1) * 2 * 3];

        for (int r = 0; r < nVertsPerEdge; r++)
        {
            int rowStartID = r * nVertsPerEdge;

            for (int c = 0; c < nVertsPerEdge; c++)
            {
                int vertID = rowStartID + c;

                outputStructData[vertID].position.w = 1.0f;
                dummyVerts[vertID] = outputStructData[vertID].position + (Vector4)outputStructData[vertID].patchCenter;
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
        dummyMesh.triangles = triangles;
        dummyMesh.hideFlags = HideFlags.DontSave;

        return dummyMesh;
    }

    public static Mesh SetupQuadMesh()
    {
        int nVerts = QuadSettings.nVerts;
        int nVertsPerEdge = QuadSettings.nVertsPerEdge;

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

    public static Mesh SetupQuadMesh(int nVertsPerEdge, PLANE plane, bool invert)
    {
        int nVerts = nVertsPerEdge * nVertsPerEdge;

        Vector3[] dummyVerts = new Vector3[nVerts];
        Vector3[] dummyNormals = new Vector3[nVerts];
        Vector2[] uv0 = new Vector2[nVerts];

        int[] triangles = new int[(nVertsPerEdge - 1) * (nVertsPerEdge - 1) * 2 * 3];

        for (int r = 0; r < nVertsPerEdge; r++)
        {
            int rowStartID = r * nVertsPerEdge;

            for (int c = 0; c < nVertsPerEdge; c++)
            {
                int vertID = rowStartID + c;

                Vector3 pos = new Vector3(), norm = new Vector3();

                Vector2 p = new Vector2();

                Vector2 uv = new Vector2();

                //uv.x = r / (float)((float)nVertsPerEdge - 0.5f);
                //uv.y = c / (float)((float)nVertsPerEdge - 0.5f);

                uv.x = r / (float)(nVertsPerEdge - 1);
                uv.y = c / (float)(nVertsPerEdge - 1);

                p.x = (uv.x - 0.5f) * 2.0f;
                p.y = (uv.y - 0.5f) * 2.0f;

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

                dummyVerts[vertID] = pos;
                dummyNormals[vertID] = norm;
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
        dummyMesh.hideFlags = HideFlags.DontSave;

        return dummyMesh;
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

    public static Mesh MakeSphere(float radius, int divr, int divh)
    {
        float a, da, yp, ya, yda, yf;
        float U, V, dU, dV;

        if (divr < 3) divr = 3;
        if (divh < 3) divh = 3;

        int numVerts = (divh - 2) * divr + 2 + (divh - 2);
        int numInds = (divr * 2 + divr * 2 * (divh - 3)) * 3;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        int[] inds = new int[numInds];

        int vidx = 0;       //current vertex
        int fidx = 0;       //current index

        //top and bottom vertices
        vertices[vidx++] = new Vector3(0, radius, 0);
        vertices[vidx++] = new Vector3(0, -radius, 0);

        ya = 0;
        yda = Mathf.PI / (divh - 1);
        da = (2 * Mathf.PI) / divr;

        //all other vertices
        for (int y = 0; y < divh - 2; y++)
        {
            ya += yda;
            yp = Mathf.Cos(ya) * radius;
            yf = Mathf.Sin(ya) * radius;
            a = 0;

            for (int x = 0; x < divr; x++)
            {
                vertices[vidx++] = new Vector3(Mathf.Cos(a) * yf, yp, Mathf.Sin(a) * yf);

                if (x == divr - 1)
                {
                    //add an extra vertex in the end of each longitudinal circunference
                    Vector3 tmp = vertices[y * (divr + 1) + 2];
                    vertices[vidx++] = new Vector3(tmp.x, tmp.y, tmp.z);
                }

                a += da;
            }
        }

        a = 0;
        U = 0;
        dU = 1.0f / divr;
        dV = V = 1.0f / divh;

        //top indices
        for (int x = 0; x < divr; x++)
        {
            int[] v = { 0, 2 + x + 1, 2 + x };

            inds[fidx++] = v[0];
            inds[fidx++] = v[1];
            inds[fidx++] = v[2];

            uvs[v[0]].x = U;
            uvs[v[0]].y = 0;

            uvs[v[1]].x = U + dU;
            uvs[v[1]].y = V;

            uvs[v[2]].x = U;
            uvs[v[2]].y = V;

            U += dU;
        }

        da = 1.0f / (divr + 1);
        int offv = 2;

        //create main body faces
        for (int x = 0; x < divh - 3; x++)
        {
            U = 0;
            for (int y = 0; y < divr; y++)
            {
                int[] v = { offv + y, offv + (divr + 1) + y + 1, offv + (divr + 1) + y };

                inds[fidx++] = v[0];
                inds[fidx++] = v[1];
                inds[fidx++] = v[2];

                uvs[v[0]].x = U;
                uvs[v[0]].y = V;

                uvs[v[1]].x = U + dU;
                uvs[v[1]].y = V + dV;

                uvs[v[2]].x = U;
                uvs[v[2]].y = V + dV;

                int[] vv = { offv + y, offv + y + 1, offv + y + 1 + (divr + 1) };

                inds[fidx++] = vv[0];
                inds[fidx++] = vv[1];
                inds[fidx++] = vv[2];

                uvs[vv[0]].x = U;
                uvs[vv[0]].y = V;

                uvs[vv[1]].x = U + dU;
                uvs[vv[1]].y = V;

                uvs[vv[2]].x = U + dU;
                uvs[vv[2]].y = V + dV;

                U += dU;
            }

            V += dV;
            offv += divr + 1;
        }

        int s = numVerts - divr - 1;
        U = 0;

        //bottom faces
        for (int x = 0; x < divr; x++)
        {
            int[] v = { 1, s + x, s + x + 1 };

            inds[fidx++] = v[0];
            inds[fidx++] = v[1];
            inds[fidx++] = v[2];

            uvs[v[0]].x = U;
            uvs[v[0]].y = 1.0f;

            uvs[v[1]].x = U;
            uvs[v[1]].y = V;

            uvs[v[2]].x = U + dU;
            uvs[v[2]].y = V;

            U += dU;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = inds;

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mesh.hideFlags = HideFlags.DontSave;

        SolveTangents(mesh);

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

    public static void SolveTangents(Mesh theMesh)
    {
        int vertexCount = theMesh.vertexCount;

        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;

        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length / 3;

        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        int tri = 0;

        for (int i = 0; i < (triangleCount); i++)
        {
            int i1 = triangles[tri];
            int i2 = triangles[tri + 1];
            int i3 = triangles[tri + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
            Vector2 w1 = texcoords[i1];
            Vector2 w2 = texcoords[i2];
            Vector2 w3 = texcoords[i3];

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

            tri += 3;
        }

        for (int i = 0; i < (vertexCount); i++)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            //Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize(ref n, ref t);

            tangents[i].x = t.x;
            tangents[i].y = t.y;
            tangents[i].z = t.z;

            //Calculate handedness
            tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
        }

        theMesh.tangents = tangents;
    }
}