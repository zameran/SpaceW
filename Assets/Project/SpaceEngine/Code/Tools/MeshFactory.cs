using System;
using UnityEngine;

public static class MeshFactory
{
    public static Mesh SetupQuadMesh()
    {
        int nVerts = QS.nVerts;
        int nVertsPerEdge = QS.nVertsPerEdge;

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
        dummyMesh.RecalculateNormals(60);

        return dummyMesh;
    }

    public static Mesh SetupQuadMesh(int nVertsPerEdge)
    {
        int nVerts = nVertsPerEdge * nVertsPerEdge;

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
        dummyMesh.RecalculateNormals(60);

        return dummyMesh;
    }

    public static Mesh SetupQuadMeshExtra(int nVertsPerEdge)
    {
        nVertsPerEdge = nVertsPerEdge + 2;

        int nVerts = nVertsPerEdge * nVertsPerEdge;

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

                dummyVerts[vertID] = new Vector3(c - 1, height, r - 1);

                Vector2 uv = new Vector2();

                uv.x = r / (float)(nVertsPerEdge - 2);
                uv.y = c / (float)(nVertsPerEdge - 2);

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
        dummyMesh.RecalculateNormals(60);

        return dummyMesh;
    }

    public static void CalculateQuadNormals(int detail, int col, int row,
                                            int previousCol, int previousRow, 
                                            bool staticX, bool staticY, bool staticZ,
                                            float stepX, float stepY, float stepZ,
                                            Vector3[] vertices, out Vector3 line1, out Vector3 line2, 
                                            Vector3 topleft, Planetoid planet)
    {
        if (previousCol >= 0)
        {
            line1.x = vertices[col * detail + row].x - vertices[previousCol * detail + row].x;
            line1.y = vertices[col * detail + row].y - vertices[previousCol * detail + row].y;
            line1.z = vertices[col * detail + row].z - vertices[previousCol * detail + row].z;
        }
        else
        {
            Vector3 previous = Vector3.zero;

            if (staticX)
                previous = new Vector3(topleft.x, topleft.y - stepY, topleft.z + stepZ * row);
            if (staticY)
                previous = new Vector3(topleft.x + stepX * row, topleft.y, topleft.z - stepZ);
            if (staticZ)
                previous = new Vector3(topleft.x + stepX * row, topleft.y - stepY, topleft.z);

            previous = VectorHelper.SperifySpherePoint(previous);

            float disp = 1;

            previous += previous * disp;
            previous *= planet.PlanetRadius;

            line1.x = vertices[col * detail + row].x - previous.x;
            line1.y = vertices[col * detail + row].y - previous.y;
            line1.z = vertices[col * detail + row].z - previous.z;
        }

        if (previousRow >= 0)
        {
            line2.x = vertices[col * detail + row].x - vertices[col * detail + previousRow].x;
            line2.y = vertices[col * detail + row].y - vertices[col * detail + previousRow].y;
            line2.z = vertices[col * detail + row].z - vertices[col * detail + previousRow].z;
        }
        else
        {
            Vector3 previous = Vector3.zero;

            if (staticX)
                previous = new Vector3(topleft.x, topleft.y + stepY * col, topleft.z - stepZ);
            if (staticY)
                previous = new Vector3(topleft.x - stepX, topleft.y, topleft.z + stepZ * col);
            if (staticZ)
                previous = new Vector3(topleft.x - stepX, topleft.y + stepY * col, topleft.z);

            previous = VectorHelper.SperifySpherePoint(previous);

            float disp = 1;

            previous += previous * disp;
            previous *= planet.PlanetRadius;

            line2.x = vertices[col * detail + row].x - previous.x;
            line2.y = vertices[col * detail + row].y - previous.y;
            line2.z = vertices[col * detail + row].z - previous.z;
        }
    }

    public static Mesh GenerateQuadMesh(int detail, Planetoid planet, Quad quad)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = null;
        Color[] vertexColors = null;
        Vector2[] uv1s = null;
        Vector3[] normals = null;

        int[] indexbuffer = null;

        // Action to be called when generation has finished, applies mesh data
        Action ApplyMesh = () =>
        {
            // create mesh
            mesh.vertices = vertices;
            mesh.colors = vertexColors;
            mesh.triangles = indexbuffer;
            mesh.uv = uv1s;
            mesh.normals = normals;
            mesh.RecalculateBounds();
            mesh.Optimize();
        };

        CalculateQuadGeometry(detail, out vertices, out vertexColors, out uv1s, out normals, out indexbuffer, planet, quad);
        ApplyMesh();

        return mesh;
    }

    public static void CalculateQuadGeometry(int detail, 
                                             out Vector3[] vertices, 
                                             out Color[] vertexColors, 
                                             out Vector2[] uv1s, 
                                             out Vector3[] normals, 
                                             out int[] indexbuffer,
                                             Planetoid planet,
                                             Quad quad)
    {
        // downward facing vertices at border of the surface to prevent seams appearing
        int borders = 0;

        // vertex array
        vertices = new Vector3[detail * detail + borders];

        // vertex colours, to store data in
        vertexColors = new Color[detail * detail + borders];

        Vector3 brc = quad.bottomRightCorner.NormalizeToRadius(planet.PlanetRadius);
        Vector3 tlc = quad.topLeftCorner.NormalizeToRadius(planet.PlanetRadius);

        // calculate interpolation between coordinates
        float stepX = (brc.x - tlc.x) / (detail - 1);
        float stepY = (brc.y - tlc.y) / (detail - 1);
        float stepZ = (brc.z - tlc.z) / (detail - 1);

        // check which axis remains stationary
        bool staticX = false, staticY = false, staticZ = false;
        if (stepX == 0)
            staticX = true;
        if (stepY == 0)
            staticY = true;
        if (stepZ == 0)
            staticZ = true;

        // normals
        normals = new Vector3[detail * detail + borders];

        Vector3 line1 = Vector3.zero;
        Vector3 line2 = Vector3.zero;

        // uvs
        uv1s = new Vector2[detail * detail + borders];

        // indices
        int indexCount = (detail - 1) * (detail - 1) * 6;

        indexbuffer = new int[indexCount];

        int index = 0;

        // plot mesh geometry
        for (int col = 0; col < detail; col++)
        {
            for (int row = 0; row < detail; row++)
            {
                // set vertex position
                if (staticX)
                    vertices[col * detail + row] = new Vector3(tlc.x, tlc.y + stepY * col, tlc.z + stepZ * row);
                if (staticY)
                    vertices[col * detail + row] = new Vector3(tlc.x + stepX * row, tlc.y, tlc.z + stepZ * col);
                if (staticZ)
                    vertices[col * detail + row] = new Vector3(tlc.x + stepX * row, tlc.y + stepY * col, tlc.z);

                // map the point on to the sphere
                vertices[col * detail + row] = VectorHelper.SperifySpherePoint(vertices[col * detail + row]);

                // calculate noise displacement
                float disp = 1;

                // displace vertex position
                vertices[col * detail + row] += vertices[col * detail + row] * disp;

                // calculate uv's
                //uv1s[col * detail + row] = UVFactory.GetSphericalUv(detail, col, row, vertices[col * detail + row], staticY, staticZ);
                uv1s[col * detail + row] = UVFactory.GetSgtSphericalUv(vertices[col * detail + row]);
                //uv1s[col * detail + row] = UVFactory.GetContinuousUV(detail, col, row, quad.uvResolution, quad.uvStartX, quad.uvStartY);
                //uv1s[col * detail + row] = UVFactory.GetSurfaceUV(detail, col, row);

                // scale to planet Radius
                vertices[col * detail + row] *= planet.PlanetRadius;

                // calculate triangle indexes
                if (col < detail - 1 && row < detail - 1)
                {
                    indexbuffer[index] = (col * detail + row);
                    index++;
                    indexbuffer[index] = (col + 1) * detail + row;
                    index++;
                    indexbuffer[index] = col * detail + row + 1;
                    index++;

                    indexbuffer[index] = (col + 1) * detail + row;
                    index++;
                    indexbuffer[index] = (col + 1) * detail + row + 1;
                    index++;
                    indexbuffer[index] = col * detail + row + 1;
                    index++;
                }

                // CALCULATE NORMALS
                int previousCol = col - 1;
                int previousRow = row - 1;

                CalculateQuadNormals(detail, col, row, previousCol, previousRow, staticX, staticY, staticZ,
                                     stepX, stepY, stepZ, vertices, out line1, out line2, tlc, planet);

                normals[col * detail + row] = Vector3.Cross(line1, line2).normalized;

                // calculate slope
                //float slope = Vector3.Dot(normals[col * detail + row], -vertices[col * detail + row].normalized) + 1.0f;

                // store data to vertex color:, r = height, g = polar, b = slope, a = unused
                //float vertexR = (displacement + 1f) / 2;
                //float vertexG = vertices[col * detail + row].normalized.y > 0 ?
                //                Mathf.Abs((vertices[col * detail + row].normalized.y - displacement / 2) + vertexR / 2) :
                //                Mathf.Abs((vertices[col * detail + row].normalized.y + displacement / 2) - vertexR / 2);
                //float vertexB = slope;
                //float vertexA = 0.0f;

                //vertexColors[col * detail + row] = new Color(vertexR, vertexG, vertexB, vertexA);
                vertexColors[col * detail + row] = Color.white;
            }
        }
    }
}