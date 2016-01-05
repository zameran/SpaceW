using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct QuadGenerationConstants
{
    public float planetRadius;
    public float spacing;
    public float terrainMaxHeight;

    public Vector3 cubeFaceEastDirection;
    public Vector3 cubeFaceNorthDirection;
    public Vector3 patchCubeCenter;

    public static QuadGenerationConstants Init()
    {
        QuadGenerationConstants temp = new QuadGenerationConstants();

        temp.spacing = QS.nSpacing;
        temp.terrainMaxHeight = 64.0f;

        return temp;
    }
}

[Serializable]
public struct OutputStruct
{
    public float noise;

    public Vector3 patchCenter;

    public Vector4 vcolor;
    public Vector4 pos;
}

public class Quad : MonoBehaviour
{
    public QuadPostion Position;

    public Planetoid Planetoid;

    public NoiseParametersSetter Setter;

    public ComputeShader HeightShader;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer OutDataBuffer;
    public ComputeBuffer ToShaderData;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadGenerationConstants quadGenerationConstants;

    public Quad Parent;

    public List<Quad> Subquads = new List<Quad>();

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

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { quadGenerationConstants, quadGenerationConstants }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
        PreOutDataBuffer = new ComputeBuffer(QS.nVerts, 48);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 48);
        ToShaderData = new ComputeBuffer(QS.nVerts, 48);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(outputStructData);
        OutDataBuffer.SetData(outputStructData);

        SetupComputeShader(0);

        Log("Buffers for first kernel ready!");

        HeightShader.Dispatch(0,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        Log("First kernel ready!");

        SetupComputeShader(1);

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

        ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, OutDataBuffer);

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private void SetupComputeShader(int kernel)
    {
        HeightShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
        HeightShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(kernel, "Height", HeightTexture);
        HeightShader.SetTexture(kernel, "Normal", NormalTexture);
    }

    private void ReleaseBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
            }
        }
    }

    private void DisposeBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Dispose();
            }
        }
    }

    private void ReleaseAndDisposeBuffer(ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer.Dispose();
        }
    }

    private void ReleaseAndDisposeBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
                buffers[i].Dispose();
            }
        }
    }

    public Vector3 GetCubeFaceEastDirection(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0, 0, -1);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0, 0, -1);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0, -1, 0);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0, -1, 0);
                break;
            case QuadPostion.Front:
                temp = new Vector3(1, 0, 0);
                break;
            case QuadPostion.Back:
                temp = new Vector3(-1, 0, 0);
                break;
        }

        return temp;
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(1, 0, 0);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(-1, 0, 0);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0, 0, -1);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0, 0, 1);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0, -1, 0);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0, -1, 0);
                break;
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenter(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0, 1, 0);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0, -1, 0);
                break;
            case QuadPostion.Left:
                temp = new Vector3(-1, 0, 0);
                break;
            case QuadPostion.Right:
                temp = new Vector3(1, 0, 0);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0, 0, 1);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0, 0, -1);
                break;
        }

        return temp;
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}