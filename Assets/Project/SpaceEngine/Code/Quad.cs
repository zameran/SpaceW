using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
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

[Serializable]
public struct OutputStruct
{
    public float noise;

    public Vector3 patchCenter;

    public Vector4 pos;
}

public class Quad : MonoBehaviour
{
    public QuadPostion Position;
    public QuadLODType LODType;

    public Planetoid Planetoid;

    public NoiseParametersSetter Setter;

    public ComputeShader HeightShader;

    public ComputeBuffer GenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer OutDataBuffer;
    public ComputeBuffer ToShaderData;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public GenerationConstants generationConstants;

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

    [ContextMenu("Split")]
    public void Split()
    {
        if (LODType == QuadLODType.Sub)
            return;

        int id = 0;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++, id++)
            {
                int x = i + 1;
                int y = j + 1;

                Quad quad = Planetoid.SetupSubQuad(Position, QuadLODType.Sub);
                quad.Parent = this;
                quad.transform.parent = this.transform;
                quad.gameObject.name += "_" + x + "_" + y + "_" + id;

                quad.SetupVectors(quad, Position, LODType, x, y, id);
                quad.Dispatch();

                this.Subquads.Add(quad);
            }
        }

        this.ReleaseAndDisposeBuffer(ToShaderData);
    }

    [ContextMenu("Unsplit")]
    public void Unsplit()
    {
        for (int i = 0; i < Subquads.Count; i++)
        {
            this.Planetoid.Quads.Remove(Subquads[i]);
            DestroyImmediate(Subquads[i].gameObject);
        }

        this.Subquads.Clear();
        this.Dispatch();
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

        GenerationConstants[] generationConstantsData = new GenerationConstants[] { generationConstants, generationConstants }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        GenerationConstantsBuffer = new ComputeBuffer(1, 72);
        PreOutDataBuffer = new ComputeBuffer(QS.nVerts, 32);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 32);
        ToShaderData = new ComputeBuffer(QS.nVerts, 32);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);

        GenerationConstantsBuffer.SetData(generationConstantsData);
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

        ReleaseAndDisposeBuffers(GenerationConstantsBuffer, PreOutDataBuffer, OutDataBuffer);

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private void SetupComputeShader(int kernel)
    {
        HeightShader.SetBuffer(kernel, "generationConstants", GenerationConstantsBuffer);
        HeightShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(kernel, "Height", HeightTexture);
        HeightShader.SetTexture(kernel, "Normal", NormalTexture);
    }

    private void ReleaseBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            buffers[i].Release();
        }
    }

    private void DisposeBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            buffers[i].Dispose();
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

    public Vector3 GetPatchCubeCenterSplitted(QuadPostion quadPosition, QuadLODType lodType, int id)
    {
        Vector3 temp = Vector3.zero;

        float v = 0.0f;

        if (Parent != null)
            if (Parent.LODType == QuadLODType.Main)
                v = 1.0f;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                if (id == 0)
                    temp = new Vector3(-v / 2, 1.0f, v / 2);
                else if (id == 1)
                    temp = new Vector3(v / 2, 1.0f, v / 2);
                else if (id == 2)
                    temp = new Vector3(-v / 2, 1.0f, -v / 2);
                else if (id == 3)
                    temp = new Vector3(v / 2, 1.0f, -v / 2);
                break;
            case QuadPostion.Bottom:
                if (id == 0)
                    temp = new Vector3(-1.0f / 2, -1.0f, -1.0f / 2);
                else if (id == 1)
                    temp = new Vector3(1.0f / 2, -1.0f, -1.0f / 2);
                else if (id == 2)
                    temp = new Vector3(-1.0f / 2, -1.0f, 1.0f / 2);
                else if (id == 3)
                    temp = new Vector3(1.0f / 2, -1.0f, 1.0f / 2);
                break;
            case QuadPostion.Left:
                if (id == 0)
                    temp = new Vector3(-1.0f, 1.0f / 2, 1.0f / 2);
                else if (id == 1)
                    temp = new Vector3(-1.0f, 1.0f / 2, -1.0f / 2);
                else if (id == 2)
                    temp = new Vector3(-1.0f, -1.0f / 2, 1.0f / 2);
                else if (id == 3)
                    temp = new Vector3(-1.0f, -1.0f / 2, -1.0f / 2);
                break;
            case QuadPostion.Right:
                if (id == 0)
                    temp = new Vector3(1.0f, 1.0f / 2, -1.0f / 2);
                else if (id == 1)
                    temp = new Vector3(1.0f, 1.0f / 2, 1.0f / 2);
                else if (id == 2)
                    temp = new Vector3(1.0f, -1.0f / 2, -1.0f / 2);
                else if (id == 3)
                    temp = new Vector3(1.0f, -1.0f / 2, 1.0f / 2);
                break;
            case QuadPostion.Front:
                if (id == 0)
                    temp = new Vector3(1.0f / 2, 1.0f / 2, 1.0f);
                else if (id == 1)
                    temp = new Vector3(-1.0f / 2, 1.0f / 2, 1.0f);
                else if (id == 2)
                    temp = new Vector3(1.0f / 2, -1.0f / 2, 1.0f);
                else if (id == 3)
                    temp = new Vector3(-1.0f / 2, -1.0f / 2, 1.0f);
                break;
            case QuadPostion.Back:
                if (id == 0)
                    temp = new Vector3(-1.0f / 2, 1.0f / 2, -1.0f);
                else if (id == 1)
                    temp = new Vector3(1.0f / 2, 1.0f / 2, -1.0f);
                else if (id == 2)
                    temp = new Vector3(-1.0f / 2, -1.0f / 2, -1.0f);
                else if (id == 3)
                    temp = new Vector3(1.0f / 2, -1.0f / 2, -1.0f);
                break;
        }

        return temp;
    }

    public void SetupVectors(Quad quad, QuadPostion quadPosition, QuadLODType lodType, int x, int y, int id)
    {
        Vector3 cubeFaceEastDirection = quad.Parent.generationConstants.cubeFaceEastDirection / 2.0f;
        Vector3 cubeFaceNorthDirection = quad.Parent.generationConstants.cubeFaceNorthDirection / 2.0f;

        quad.generationConstants.cubeFaceEastDirection = cubeFaceEastDirection;
        quad.generationConstants.cubeFaceNorthDirection = cubeFaceNorthDirection;

        Vector3 patchCubeCenter = quad.GetPatchCubeCenterSplitted(quadPosition, lodType, id);

        quad.generationConstants.patchCubeCenter = patchCubeCenter;
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}