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
    public Vector4 cpos;
}

public class Quad : MonoBehaviour
{
    public QuadPostion Position;
    public QuadID ID;

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
    public Quad OneLODParent;

    public List<Quad> Subquads = new List<Quad>();

    public int LODLevel = -1;

    public bool HaveSubQuads = false;

    public Quad()
    {

    }

    public Vector3 CombineVectors(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return v1 + v2 + v3;
    }

    public Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

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
        if (this.Subquads.Count != 0)
            Unsplit();

        int id = 0;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++, id++)
            {
                Quad quad = Planetoid.SetupSubQuad(Position);
                quad.Parent = this;
                quad.LODLevel = quad.Parent.LODLevel + 1;
                quad.ID = (QuadID)id;

                if (quad.LODLevel == 1)
                    quad.OneLODParent = quad.Parent;
                else if (quad.LODLevel > 1)
                    quad.OneLODParent = quad.Parent.OneLODParent;

                quad.transform.parent = this.transform;
                quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;
                quad.SetupVectors(quad, id);
                quad.Dispatch();

                this.Subquads.Add(quad);
                this.ReleaseAndDisposeBuffer(ToShaderData);

                this.HaveSubQuads = true;
            }
        }
    }

    [ContextMenu("Unslpit")]
    public void Unsplit()
    {
        for (int i = 0; i < this.Subquads.Count; i++)
        {
            if(this.Subquads[i].HaveSubQuads)
            {
                this.Subquads[i].Unsplit();
            }

            if (this.Planetoid.Quads.Contains(this.Subquads[i]))
            {
                this.Planetoid.Quads.Remove(this.Subquads[i]);
            }

            if (this.Subquads[i] != null)
            {
                DestroyImmediate(this.Subquads[i].gameObject);
            }
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

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { quadGenerationConstants, quadGenerationConstants }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
        PreOutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
        ToShaderData = new ComputeBuffer(QS.nVerts, 64);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 24);

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(outputStructData);
        OutDataBuffer.SetData(outputStructData);

        int kernel1 = HeightShader.FindKernel("CSMainNoise");
        int kernel2 = HeightShader.FindKernel("CSTexturesMain");

        SetupComputeShader(kernel1);

        Log("Buffers for first kernel ready!");

        HeightShader.Dispatch(kernel1,
        QS.THREADGROUP_SIZE_X_REAL,
        QS.THREADGROUP_SIZE_Y_REAL,
        QS.THREADGROUP_SIZE_Z_REAL);

        Log("First kernel ready!");

        SetupComputeShader(kernel2);

        Log("Buffers for second kernel ready!");

        HeightShader.Dispatch(kernel2,
        QS.THREADGROUP_SIZE_X_REAL,
        QS.THREADGROUP_SIZE_Y_REAL,
        QS.THREADGROUP_SIZE_Z_REAL);

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

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Front:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Back:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
        }

        return temp;
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPostion.Left:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPostion.Right:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0.0f, -r, 0);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenter(QuadPostion quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = this.Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                temp = new Vector3(0.0f, r, 0.0f);
                break;
            case QuadPostion.Bottom:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPostion.Left:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPostion.Right:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPostion.Front:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPostion.Back:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
        }

        return temp;
    }

    public Vector3 Reposition(QuadPostion quadPosition, Vector3 vector, int LODLevel)
    {
        #region LODLVL_1
        if (LODLevel == 1)
        {
            if (quadPosition == QuadPostion.Top)
            {
                #region TOP
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.z = -vector.z;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.x = -vector.x;
                    vector.z = -vector.z;
                }
                #endregion
            }
            else if (quadPosition == QuadPostion.Bottom)
            {
                #region BOTTOM
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                    vector.z = -vector.z;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.z = -vector.z;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                }
                #endregion
            }
            else if (quadPosition == QuadPostion.Left)
            {
                #region LEFT
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.z = -vector.z;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.y = -vector.y;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.y = -vector.y;
                    vector.z = -vector.z;
                }
                #endregion
            }
            else if (quadPosition == QuadPostion.Right)
            {
                #region RIGHT
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.z = -vector.z;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.y = -vector.y;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.y = -vector.y;
                    vector.z = -vector.z;
                }
                #endregion
            }
            else if (quadPosition == QuadPostion.Front)
            {
                #region FRONT
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.x = -vector.x;
                    vector.y = -vector.y;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.y = -vector.y;
                }
                #endregion
            }
            else if (quadPosition == QuadPostion.Back)
            {
                #region BACK
                if (this.Parent.ID == QuadID.One)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;

                    vector.x = -vector.x;
                }
                else if (this.Parent.ID == QuadID.Two)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                }
                else if (this.Parent.ID == QuadID.Three)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.x = -vector.x;
                    vector.y = -vector.y;
                }
                else if (this.Parent.ID == QuadID.Four)
                {
                    if (this.ID == QuadID.One)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Two)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection * 3;
                    else if (this.ID == QuadID.Three)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection * 3
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;
                    else if (this.ID == QuadID.Four)
                        vector = vector - this.quadGenerationConstants.cubeFaceEastDirection
                                        - this.quadGenerationConstants.cubeFaceNorthDirection;

                    vector.y = -vector.y;
                }
                #endregion
            }
        }
        #endregion

        return vector;
    }

    public Vector3 GetPatchCubeCenterSplitted(QuadPostion quadPosition, int id)
    {
        Vector3 temp = Vector3.zero;

        float v = this.Planetoid.PlanetRadius;

        temp = Reposition(quadPosition, temp, this.LODLevel);

        switch (quadPosition)
        {
            case QuadPostion.Top:
                if (id == 0)
                    temp += new Vector3(-v / 2, v, v / 2);
                else if (id == 1)
                    temp += new Vector3(v / 2, v, v / 2);
                else if (id == 2)
                    temp += new Vector3(-v / 2, v, -v / 2);
                else if (id == 3)
                    temp += new Vector3(v / 2, v, -v / 2);
                break;
            case QuadPostion.Bottom:
                if (id == 0)
                    temp += new Vector3(-v / 2, -v, -v / 2);
                else if (id == 1)
                    temp += new Vector3(v / 2, -v, -v / 2);
                else if (id == 2)
                    temp += new Vector3(-v / 2, -v, v / 2);
                else if (id == 3)
                    temp += new Vector3(v / 2, -v, v / 2);
                break;
            case QuadPostion.Left:
                if (id == 0)
                    temp += new Vector3(-v, v / 2, v / 2);
                else if (id == 1)
                    temp += new Vector3(-v, v / 2, -v / 2);
                else if (id == 2)
                    temp += new Vector3(-v, -v / 2, v / 2);
                else if (id == 3)
                    temp += new Vector3(-v, -v / 2, -v / 2);
                break;
            case QuadPostion.Right:
                if (id == 0)
                    temp += new Vector3(v, v / 2, -v / 2);
                else if (id == 1)
                    temp += new Vector3(v, v / 2, v / 2);
                else if (id == 2)
                    temp += new Vector3(v, -v / 2, -v / 2);
                else if (id == 3)
                    temp += new Vector3(v, -v / 2, v / 2);
                break;
            case QuadPostion.Front:
                if (id == 0)
                    temp += new Vector3(v / 2, v / 2, v);
                else if (id == 1)
                    temp += new Vector3(-v / 2, v / 2, v);
                else if (id == 2)
                    temp += new Vector3(v / 2, -v / 2, v);
                else if (id == 3)
                    temp += new Vector3(-v / 2, -v / 2, v);
                break;
            case QuadPostion.Back:
                if (id == 0)
                    temp += new Vector3(-v / 2, v / 2, -v);
                else if (id == 1)
                    temp += new Vector3(v / 2, v / 2, -v);
                else if (id == 2)
                    temp += new Vector3(-v / 2, -v / 2, -v);
                else if (id == 3)
                    temp += new Vector3(v / 2, -v / 2, -v);
                break;
        }

        return temp;
    }

    public void SetupVectors(Quad quad, int id)
    {
        Vector3 cfed = Parent.quadGenerationConstants.cubeFaceEastDirection / 2;
        Vector3 cfnd = Parent.quadGenerationConstants.cubeFaceNorthDirection / 2;

        quad.quadGenerationConstants.cubeFaceEastDirection = cfed;
        quad.quadGenerationConstants.cubeFaceNorthDirection = cfnd;
        quad.quadGenerationConstants.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id);
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}