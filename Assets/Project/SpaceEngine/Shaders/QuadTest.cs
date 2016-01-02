using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QS
{
    public static int nRealVertsCount { get { return 128; } }
    public static int nVertsPerEdge { get { return 128; } }
    public static int nVerts { get { return nVertsPerEdge * nVertsPerEdge; } }
    public static int THREADS_PER_GROUP_X { get { return 32; } }
    public static int THREADS_PER_GROUP_Y { get { return 32; } }
    public static int THREADGROUP_SIZE_X { get { return nVertsPerEdge / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y { get { return nVertsPerEdge / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z { get { return 1; } }
}

[System.Serializable]
public struct GenerationConstants
{
    public float scale;
    public float noiseSeaLevel;    
    public float planetRadius;
    public float spacing;
    public float terrainMaxHeight;

    public Vector3 cubeFaceEastDirection;
    public Vector3 cubeFaceNorthDirection;
    public Vector3 patchCubeCenter;

    public static GenerationConstants Init()
    {
        GenerationConstants temp = new GenerationConstants();

        temp.scale = 2.0f / (QS.nVertsPerEdge);
        temp.noiseSeaLevel = 0.1f;
        temp.planetRadius = 1000.0f;
        //temp.spacing = 2.0f / (QS.nVertsPerEdge - 1.0f);
        temp.spacing = 2.0f * temp.planetRadius / QS.nVertsPerEdge;
        temp.terrainMaxHeight = 64.0f;

        temp.cubeFaceEastDirection = new Vector3(1, 0, 0);
        temp.cubeFaceNorthDirection = new Vector3(0, 1, 0);
        temp.patchCubeCenter = new Vector3(0, 0, temp.planetRadius);

        return temp;
    }
}

[System.Serializable]
public struct InputStruct
{
    public Vector2 uv1;
    public Vector2 uv2;
}

[System.Serializable]
public struct OutputStruct
{
    public float noise;

    public Vector3 patchCenter;

    public Vector4 pos;
}

public class QuadTest : MonoBehaviour
{
    public bool DebugEnabled = false;

    public NoiseParametersSetter Setter;

    public ComputeShader CShader;

    public Material QuadMaterial;

    public ComputeBuffer ToShaderData;

    public InputStruct[] inputData;

    public GenerationConstants[] generationConstants;

    void Start()
    {
        Dispatch();
    }

    void OnDestroy()
    {
        if (ToShaderData != null)
            ToShaderData.Release();
    }

    [ContextMenu("Create Dummy Mesh")]
    public void CreateDummyMesh()
    {
        this.GetComponent<MeshFilter>().sharedMesh = SetupDummyMesh();
    }

    //TODO fast data get.
    public InputStruct[] GetInputData()
    {
        InputStruct[] temp = new InputStruct[QS.nRealVertsCount * QS.nRealVertsCount];

        Mesh mesh = this.GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < temp.Length; i++)
        {
            temp[i].uv1 = mesh.uv[i];
        }

        return temp;
    }

    public GenerationConstants[] GetGenerationConstantsData()
    {
        GenerationConstants[] temp = new GenerationConstants[1];

        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = GenerationConstants.Init();
        }

        return temp;
    }

    [ContextMenu("Unbake Input Data")]
    public void UnbakeInputData()
    {
        inputData = null;
        generationConstants = null;
    }

    [ContextMenu("Bake Input Data")]
    public void BakeInputData()
    {
        inputData = GetInputData();
        generationConstants = GetGenerationConstantsData();
    }

    [ContextMenu("Displatch!")]
    public void Dispatch()
    {
        if (inputData == null ||
           inputData.Length == 0 ||
           inputData.Length < QS.nRealVertsCount * QS.nRealVertsCount ||
           generationConstants == null ||
           generationConstants.Length == 0)
        {
            BakeInputData();

            Log("Input data was null, or something wrong with it - new one was created and calculated.");
        }

        float time = Time.realtimeSinceStartup;

        if (ToShaderData != null)
            ToShaderData.Release();

        if (Setter != null)
        {
            Setter.LoadAndInit();
            Setter.SetUniforms(CShader);
        }

        ComputeBuffer GConstatns;
        ComputeBuffer InData;
        ComputeBuffer OutData;

        GenerationConstants[] generationConstantsData = generationConstants;
        InputStruct[] inputStructData = inputData;
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        GConstatns = new ComputeBuffer(1, 72);
        InData = new ComputeBuffer(QS.nVerts, 16);
        OutData = new ComputeBuffer(QS.nVerts, 32);
        ToShaderData = new ComputeBuffer(QS.nVerts, 32);

        Log("GConstants Buffer count: " + GConstatns.count);
        Log("InData Buffer count: " + InData.count);
        Log("OutData Buffer count: " + OutData.count);

        GConstatns.SetData(generationConstantsData);
        InData.SetData(inputStructData);
        OutData.SetData(outputStructData);

        CShader.SetBuffer(0, "terrainGenerationConstants", GConstatns);    
        CShader.SetBuffer(0, "patchInput", InData); 
        CShader.SetBuffer(0, "patchOutput", OutData);

        CShader.Dispatch(0,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        Log("Dispatched!");

        OutData.GetData(outputStructData);
        ToShaderData.SetData(outputStructData);

        Vector4 averageData = Vector4.zero;

        for (int i = 0; i < outputStructData.Length; i++)
        {
            averageData += outputStructData[i].pos;
        }

        Log("Average Output Position Data: " + averageData.ToString());

        QuadMaterial.SetBuffer("data", ToShaderData);

        GConstatns.Release();
        InData.Release();
        OutData.Release();

        GConstatns.Dispose();
        InData.Dispose();
        OutData.Dispose();

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private Mesh SetupDummyMesh()
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

    private void Log(string msg)
    {
        if (DebugEnabled)
            Debug.Log(msg);
    }
}

public static class NormalSolver
{
    /// <summary>
    ///     Recalculate the normals of a mesh based on an angle threshold. This takes
    ///     into account distinct vertices that have the same position.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="angle">
    ///     The smoothing angle. Note that triangles that already share
    ///     the same vertex will be smooth regardless of the angle!
    /// </param>
    public static void RecalculateNormals(this Mesh mesh, float angle)
    {
        var triangles = mesh.GetTriangles(0);
        var vertices = mesh.vertices;
        var triNormals = new Vector3[triangles.Length / 3];
        var normals = new Vector3[vertices.Length];

        angle = angle * Mathf.Deg2Rad;

        var dictionary = new Dictionary<VertexKey, VertexEntry>(vertices.Length);

        for (var i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 p1 = vertices[i2] - vertices[i1];
            Vector3 p2 = vertices[i3] - vertices[i1];
            Vector3 normal = Vector3.Cross(p1, p2).normalized;

            int triIndex = i / 3;
            triNormals[triIndex] = normal;

            VertexEntry entry;
            VertexKey key;

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i1, triIndex);

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i2, triIndex);

            if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
            {
                entry = new VertexEntry();
                dictionary.Add(key, entry);
            }
            entry.Add(i3, triIndex);
        }

        foreach (var value in dictionary.Values)
        {
            for (var i = 0; i < value.Count; ++i)
            {
                var sum = new Vector3();
                for (var j = 0; j < value.Count; ++j)
                {
                    if (value.VertexIndex[i] == value.VertexIndex[j])
                    {
                        sum += triNormals[value.TriangleIndex[j]];
                    }

                    else
                    {
                        float dot = Vector3.Dot(triNormals[value.TriangleIndex[i]],
                                                triNormals[value.TriangleIndex[j]]);

                        dot = Mathf.Clamp(dot, -0.99999f, 0.99999f);

                        float acos = Mathf.Acos(dot);

                        if (acos <= angle)
                        {
                            sum += triNormals[value.TriangleIndex[j]];
                        }
                    }
                }

                normals[value.VertexIndex[i]] = sum.normalized;
            }
        }

        mesh.normals = normals;
    }

    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;

        private const int Tolerance = 100000;

        public VertexKey(Vector3 position)
        {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }

        public override bool Equals(object obj)
        {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }

        public override int GetHashCode()
        {
            return (_x * 7 ^ _y * 13 ^ _z * 27).GetHashCode();
        }
    }

    private sealed class VertexEntry
    {
        public int[] TriangleIndex = new int[4];
        public int[] VertexIndex = new int[4];

        private int _reserved = 4;
        private int _count;

        public int Count { get { return _count; } }

        public void Add(int vertIndex, int triIndex)
        {
            if (_reserved == _count)
            {
                _reserved *= 2;
                Array.Resize(ref TriangleIndex, _reserved);
                Array.Resize(ref VertexIndex, _reserved);
            }

            TriangleIndex[_count] = triIndex;
            VertexIndex[_count] = vertIndex;
            ++_count;
        }
    }
}