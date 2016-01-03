using UnityEngine;

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
        temp.spacing = 2.0f / (QS.nVertsPerEdge - 1.0f);
        temp.terrainMaxHeight = 64.0f;

        return temp;
    }
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
    public PlanetoidTest Planetoid;

    public NoiseParametersSetter Setter;

    public ComputeShader HeightShader;

    public ComputeBuffer ToShaderData;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

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

    [ContextMenu("Displatch!")]
    public void Dispatch()
    {
        float time = Time.realtimeSinceStartup;

        if (ToShaderData != null)
            ToShaderData.Release();

        if (Setter != null)
        {
            Setter.LoadAndInit();
            Setter.SetUniforms(HeightShader);
        }

        ComputeBuffer GenerationConstantsBuffer;
        ComputeBuffer PreOutDataBuffer;
        ComputeBuffer OutDataBuffer;

        GenerationConstants[] generationConstantsData = new GenerationConstants[] { generationConstants[0], generationConstants[0] }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        GenerationConstantsBuffer = new ComputeBuffer(1, 72);
        PreOutDataBuffer = new ComputeBuffer(QS.nVerts, 32);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 32);
        ToShaderData = new ComputeBuffer(QS.nVerts, 32);

        HeightTexture = new RenderTexture(128, 128, 24);
        HeightTexture.enableRandomWrite = true;
        HeightTexture.Create();

        NormalTexture = new RenderTexture(128, 128, 24);
        NormalTexture.enableRandomWrite = true;
        NormalTexture.Create();

        GenerationConstantsBuffer.SetData(generationConstantsData);
        PreOutDataBuffer.SetData(outputStructData);
        OutDataBuffer.SetData(outputStructData);

        HeightShader.SetBuffer(0, "generationConstants", GenerationConstantsBuffer);    
        HeightShader.SetBuffer(0, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(0, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(0, "Height", HeightTexture);
        HeightShader.SetTexture(0, "Normal", NormalTexture);

        Log("Buffers for first kernel ready!");

        HeightShader.Dispatch(0,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        Log("First kernel ready!");

        HeightShader.SetBuffer(1, "generationConstants", GenerationConstantsBuffer);
        HeightShader.SetBuffer(1, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(1, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(1, "Height", HeightTexture);
        HeightShader.SetTexture(1, "Normal", NormalTexture);

        Log("Buffers for second kernel ready!");

        HeightShader.Dispatch(1,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        Log("Second kernel ready!");

        OutDataBuffer.GetData(outputStructData);
        ToShaderData.SetData(outputStructData);

        if (Setter != null)
        {
            Setter.MaterialToUpdate.SetBuffer("data", ToShaderData);
            Setter.MaterialToUpdate.SetTexture("_HeightTexture", HeightTexture);
            Setter.MaterialToUpdate.SetTexture("_NormalTexture", NormalTexture);
        }

        GenerationConstantsBuffer.Release();
        PreOutDataBuffer.Release();
        OutDataBuffer.Release();

        GenerationConstantsBuffer.Dispose();
        PreOutDataBuffer.Dispose();
        OutDataBuffer.Dispose();

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}