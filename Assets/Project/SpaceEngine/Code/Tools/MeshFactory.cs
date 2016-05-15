#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

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

                outputStructData[vertID].pos.w = 1.0f;
                dummyVerts[vertID] = outputStructData[vertID].pos + (Vector4)outputStructData[vertID].patchCenter;
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

        return mesh;
    }
}