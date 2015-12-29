using UnityEngine;

public class QS
{
    public static int nVertsPerEdge { get { return 128; } }
    public static int nVerts { get { return nVertsPerEdge * nVertsPerEdge; } }
    public static int THREADS_PER_GROUP_X { get { return 32; } }
    public static int THREADS_PER_GROUP_Y { get { return 32; } }
    public static int THREADGROUP_SIZE_X { get { return nVertsPerEdge / THREADS_PER_GROUP_X; } }
    public static int THREADGROUP_SIZE_Y { get { return nVertsPerEdge / THREADS_PER_GROUP_Y; } }
    public static int THREADGROUP_SIZE_Z { get { return 1; } }
}

public struct GenerationConstants
{
    public float scale;
    public float noiseSeaLevel;
    public float spacing;

    public Vector4 patchCenter;

    public GenerationConstants(float scale)
    {
        this.scale = scale;
        this.noiseSeaLevel = 0;
        this.spacing = 1;

        this.patchCenter = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    }
}

public struct OutputStruct
{
    public Vector4 pos;
}

public class QuadTest : MonoBehaviour
{
    public NoiseParametersSetter Setter;

    public ComputeShader CShader;

    public Material QuadMaterial;

    public ComputeBuffer ToShaderData;

    void Start()
    {
        Dispatch();
    }

    [ContextMenu("Create Dummy Mesh")]
    public void CreateDummyMesh()
    {
        this.GetComponent<MeshFilter>().sharedMesh = SetupDummyMesh();
    }

    [ContextMenu("Displatch!")]
    public void Dispatch()
    {
        System.GC.KeepAlive(ToShaderData);

        if (ToShaderData != null)
            ToShaderData.Release();

        if (Setter != null)
        {
            Setter.LoadAndInit();
            Setter.SetUniforms(CShader);
        }

        ComputeBuffer GConstatns;
        ComputeBuffer OutData;

        GenerationConstants[] generationConstantsData = new GenerationConstants[1] { new GenerationConstants(2.0f / QS.nVertsPerEdge) };
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        GConstatns = new ComputeBuffer(1, 28);
        OutData = new ComputeBuffer(QS.nVerts, 16);
        ToShaderData = new ComputeBuffer(QS.nVerts, 16);

        Debug.Log("GConstatns Buffer count: " + GConstatns.count);
        Debug.Log("OutData Buffer count: " + OutData.count);

        GConstatns.SetData(generationConstantsData);
        CShader.SetBuffer(0, "terrainGenerationConstants", GConstatns);

        OutData.SetData(outputStructData);
        CShader.SetBuffer(0, "patchOutput", OutData);

        CShader.Dispatch(0,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        Debug.Log("Dispatched!");

        OutData.GetData(outputStructData);
        ToShaderData.SetData(outputStructData);

        Vector4 averageData = Vector4.zero;

        for (int i = 0; i < outputStructData.Length; i++)
        {
            averageData += outputStructData[i].pos;
        }

        Debug.Log("Average Output Position Data: " + averageData.ToString());

        QuadMaterial.SetBuffer("data", ToShaderData);

        GConstatns.Release();
        OutData.Release();
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

        return dummyMesh;
    }
}