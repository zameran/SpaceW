using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using ZFramework.Unity.Common.PerfomanceMonitor;

[Serializable]
public struct QuadGenerationConstants
{
    public float planetRadius; //4
    public float spacing; //4
    public float terrainMaxHeight; //4

    public Vector3 cubeFaceEastDirection; //12
    public Vector3 cubeFaceNorthDirection; //12
    public Vector3 patchCubeCenter; //12

    //12 * 3 + 4 * 3 = 36 + 12 = 48

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

    public QuadGenerationConstants quadGC;

    public Quad Parent;

    public List<Quad> Subquads = new List<Quad>();

    public int LODLevel = -1;

    public bool HaveSubQuads = false;

    public float lodUpdateInterval = 0.25f;
    public float lastLodUpdateTime = 0.00f;

    public Vector3 topLeftCorner;
    public Vector3 bottomRightCorner;
    public Vector3 topRightCorner;
    public Vector3 bottomLeftCorner;
    public Vector3 middleNormalized;

    public Quad()
    {

    }

    private void Start()
    {
        this.Dispatch();
    }

    private void Update()
    {
        if (Time.time > this.lastLodUpdateTime + this.lodUpdateInterval)
        {
            this.lastLodUpdateTime = Time.time;

            if (this.LODLevel != this.Planetoid.LODMaxLevel)
            {
                if (this.Planetoid.LODDistances[this.LODLevel + 1] > GetClosestDistance(-64))
                {
                    if (!this.HaveSubQuads)
                        this.Split();
                }
                else
                {
                    if (this.HaveSubQuads)
                        this.Unsplit();
                }
            }
        }
    }

    private void OnDestroy()
    {
        BufferHelper.ReleaseAndDisposeBuffers(this.QuadGenerationConstantsBuffer, this.PreOutDataBuffer, this.OutDataBuffer, this.ToShaderData);

        if (this.HeightTexture != null && this.HeightTexture.IsCreated())
            this.HeightTexture.Release();

        if (this.NormalTexture != null && this.NormalTexture.IsCreated())
            this.NormalTexture.Release();
    }

    private void OnDrawGizmosSelected()
    {
        if (this.Planetoid.DrawGizmos)
        {
            //Gizmos.color = Color.cyan;
            //Gizmos.DrawWireCube(this.middleNormalized, GetBoundsSize(this));

            if (!this.HaveSubQuads)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(this.topLeftCorner.NormalizeToRadius(this.Planetoid.PlanetRadius), 100);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(this.topRightCorner.NormalizeToRadius(this.Planetoid.PlanetRadius), 100);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(this.bottomLeftCorner.NormalizeToRadius(this.Planetoid.PlanetRadius), 100);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(this.bottomRightCorner.NormalizeToRadius(this.Planetoid.PlanetRadius), 100);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(this.middleNormalized.NormalizeToRadius(this.Planetoid.PlanetRadius), 100);
            }
        }
    }

    public void InitCorners(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
    {
        this.topLeftCorner = topLeft;
        this.bottomRightCorner = bottmoRight;
        this.topRightCorner = topRight;
        this.bottomLeftCorner = bottomLeft;

        this.middleNormalized = this.CalculateMiddlePoint(topLeft, bottmoRight, topRight, bottmoRight);
    }

    [ContextMenu("Split")]
    public void Split()
    {
        if (this.Subquads.Count != 0)
            Unsplit();

        int id = 0;

        int subdivisions = 2;

        Vector3 size = this.bottomRightCorner - this.topLeftCorner;
        Vector3 step = size / subdivisions;

        Log("Size: " + size.ToString());

        bool staticX = false, staticY = false, staticZ = false;

        if (step.x == 0)
            staticX = true;
        if (step.y == 0)
            staticY = true;
        if (step.z == 0)
            staticZ = true;

        for (int sY = 0; sY < subdivisions; sY++)
        {
            for (int sX = 0; sX < subdivisions; sX++, id++)
            {
                Vector3 subTopLeft = Vector3.zero, subBottomRight = Vector3.zero;
                Vector3 subTopRight = Vector3.zero, subBottomLeft = Vector3.zero;

                if (staticX)
                {
                    subTopLeft = new Vector3(this.topLeftCorner.x, this.topLeftCorner.y + step.y * sY, this.topLeftCorner.z + step.z * sX);
                    subBottomRight = new Vector3(this.topLeftCorner.x, this.topLeftCorner.y + step.y * (sY + 1), this.topLeftCorner.z + step.z * (sX + 1));

                    subTopRight = new Vector3(this.topLeftCorner.x, this.topLeftCorner.y + step.y * sY, this.topLeftCorner.z + step.z * (sX + 1));
                    subBottomLeft = new Vector3(this.topLeftCorner.x, this.topLeftCorner.y + step.y * (sY + 1), this.topLeftCorner.z + step.z * sX);
                }
                if (staticY)
                {
                    subTopLeft = new Vector3(this.topLeftCorner.x + step.x * sX, this.topLeftCorner.y, this.topLeftCorner.z + step.z * sY);
                    subBottomRight = new Vector3(this.topLeftCorner.x + step.x * (sX + 1), this.topLeftCorner.y, this.topLeftCorner.z + step.z * (sY + 1));

                    subTopRight = new Vector3(this.topLeftCorner.x + step.x * (sX + 1), this.topLeftCorner.y, this.topLeftCorner.z + step.z * sY);
                    subBottomLeft = new Vector3(this.topLeftCorner.x + step.x * sX, this.topLeftCorner.y, this.topLeftCorner.z + step.z * (sY + 1));
                }
                if (staticZ)
                {
                    subTopLeft = new Vector3(this.topLeftCorner.x + step.x * sX, this.topLeftCorner.y + step.y * sY, this.topLeftCorner.z);
                    subBottomRight = new Vector3(this.topLeftCorner.x + step.x * (sX + 1), this.topLeftCorner.y + step.y * (sY + 1), this.topLeftCorner.z);

                    subTopRight = new Vector3(this.topLeftCorner.x + step.x * (sX + 1), this.topLeftCorner.y + step.y * sY, this.topLeftCorner.z);
                    subBottomLeft = new Vector3(this.topLeftCorner.x + step.x * sX, this.topLeftCorner.y + step.y * (sY + 1), this.topLeftCorner.z);
                }

                Quad quad = Planetoid.SetupSubQuad(Position);
                quad.InitCorners(subTopLeft, subBottomRight, subTopRight, subBottomLeft);
                quad.SetupParent(this);
                quad.SetupLODLevel(quad);
                quad.SetupID(quad, id);
                quad.SetupVectors(quad, id, staticX, staticY, staticZ);

                quad.transform.parent = this.transform;
                quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;

                this.Subquads.Add(quad);
                this.HaveSubQuads = true;

                quad.Dispatch();

                BufferHelper.ReleaseAndDisposeQuadBuffers(this);
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

        this.HaveSubQuads = false;
        this.Subquads.Clear();
        this.Dispatch();
    }

    [ContextMenu("Displatch!")]
    public void Dispatch()
    {
        float time = Time.realtimeSinceStartup;

        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, OutDataBuffer, ToShaderData);

        Setter.LoadAndInit();

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { quadGC, quadGC }; //Here we add 2 equal elements in to the buffer data, and nex we will set buffer size to 1. Bugfix. Idk.
        OutputStruct[] preOutputStructData = new OutputStruct[QS.nRealVerts];
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        QuadGenerationConstantsBuffer = new ComputeBuffer(1, 48);
        PreOutDataBuffer = new ComputeBuffer(QS.nRealVerts, 64);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
        ToShaderData = new ComputeBuffer(QS.nVerts, 64);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 0);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdge, 0);

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(preOutputStructData);
        OutDataBuffer.SetData(outputStructData);

        int kernel1 = HeightShader.FindKernel("HeightMain");
        int kernel2 = HeightShader.FindKernel("TexturesMain");
        //int kernel3 = HeightShader.FindKernel("Simple");

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
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z);

        OutDataBuffer.GetData(outputStructData);
        ToShaderData.SetData(outputStructData);

        Log("Second kernel ready!");

        Setter.MaterialToUpdate.SetBuffer("data", ToShaderData);
        Setter.MaterialToUpdate.SetTexture("_HeightTexture", HeightTexture);
        Setter.MaterialToUpdate.SetTexture("_NormalTexture", NormalTexture);

        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, OutDataBuffer);

        Log("Dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
    }

    private void SetupComputeShader(int kernel)
    {
        HeightShader.SetInt("FaceID", (int)Position);
        HeightShader.SetFloat("LODLevel", ((1 << LODLevel + 2) * (this.Planetoid.PlanetRadius / (LODLevel + 2)) - ((this.Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / this.Planetoid.PlanetRadius);
        HeightShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
        HeightShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        HeightShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        HeightShader.SetTexture(kernel, "Height", HeightTexture);
        HeightShader.SetTexture(kernel, "Normal", NormalTexture);

        Setter.SetUniforms(HeightShader, kernel);
    }

    public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 cfed = Parent.quadGC.cubeFaceEastDirection / 2;
        Vector3 cfnd = Parent.quadGC.cubeFaceNorthDirection / 2;

        quad.quadGC.cubeFaceEastDirection = cfed;
        quad.quadGC.cubeFaceNorthDirection = cfnd;
        quad.quadGC.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
    }

    public void SetupCorners(QuadPostion pos)
    {
        float v = this.Planetoid.PlanetRadius / 2;

        switch (pos)
        {
            case QuadPostion.Top:
                this.topLeftCorner = new Vector3(-v, v, v);
                this.bottomRightCorner = new Vector3(v, v, -v);

                this.topRightCorner = new Vector3(v, v, v);
                this.bottomLeftCorner = new Vector3(-v, v, -v);
                break;
            case QuadPostion.Bottom:
                this.topLeftCorner = new Vector3(-v, -v, -v);
                this.bottomRightCorner = new Vector3(v, -v, v);

                this.topRightCorner = new Vector3(v, -v, -v);
                this.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Left:
                this.topLeftCorner = new Vector3(-v, v, v);
                this.bottomRightCorner = new Vector3(-v, -v, -v);

                this.topRightCorner = new Vector3(-v, v, -v);
                this.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Right:
                this.topLeftCorner = new Vector3(v, v, -v);
                this.bottomRightCorner = new Vector3(v, -v, v);

                this.topRightCorner = new Vector3(v, v, v);
                this.bottomLeftCorner = new Vector3(v, -v, -v);
                break;
            case QuadPostion.Front:
                this.topLeftCorner = new Vector3(v, v, v);
                this.bottomRightCorner = new Vector3(-v, -v, v);

                this.topRightCorner = new Vector3(-v, v, v);
                this.bottomLeftCorner = new Vector3(v, -v, v);
                break;
            case QuadPostion.Back:
                this.topLeftCorner = new Vector3(-v, v, -v);
                this.bottomRightCorner = new Vector3(v, -v, -v);

                this.topRightCorner = new Vector3(v, v, -v);
                this.bottomLeftCorner = new Vector3(-v, -v, -v);
                break;
        }

        this.middleNormalized = this.CalculateMiddlePoint(this.topLeftCorner,
                                                          this.bottomRightCorner,
                                                          this.topRightCorner,
                                                          this.bottomLeftCorner);
    }

    public void SetupParent(Quad parent)
    {
        this.Parent = parent;
    }

    public void SetupLODLevel(Quad quad)
    {
        quad.LODLevel = quad.Parent.LODLevel + 1;
    }

    public void SetupID(Quad quad, int id)
    {
        quad.ID = (QuadID)id;
    }

    public void SetupBounds(Quad quad, Mesh mesh)
    {
        Vector3 middle = quad.middleNormalized;

        mesh.bounds = new Bounds(middle, GetBoundsSize(quad));
    }

    public float GetClosestDistance(float offset)
    {
        float closestDistance = Mathf.Infinity;

        Vector3 topLeftCorner = this.topLeftCorner.NormalizeToRadius(this.Planetoid.PlanetRadius);
        Vector3 topRightCorner = this.topRightCorner.NormalizeToRadius(this.Planetoid.PlanetRadius);
        Vector3 middlePoint = this.middleNormalized;
        Vector3 bottomLeftCorner = this.bottomLeftCorner.NormalizeToRadius(this.Planetoid.PlanetRadius);
        Vector3 bottomRightCorner = this.bottomRightCorner.NormalizeToRadius(this.Planetoid.PlanetRadius);

        float d = Vector3.Distance(this.Planetoid.LODTarget.position, topLeftCorner);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(this.Planetoid.LODTarget.position, topRightCorner);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(this.Planetoid.LODTarget.position, middlePoint);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(this.Planetoid.LODTarget.position, bottomLeftCorner);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(this.Planetoid.LODTarget.position, bottomRightCorner);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        return closestDistance;
    }

    public Vector3 GetBoundsSize(Quad quad)
    {
        Vector3 tlc = quad.topLeftCorner;

        tlc = tlc.Abs();
        tlc = tlc * 2;

        return tlc;
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

    public Vector3 GetPatchCubeCenterSplitted(QuadPostion quadPosition, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 temp = Vector3.zero;

        float v = this.Planetoid.PlanetRadius;

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

        float tempStatic = 0;

        if (staticX)
            tempStatic = temp.x;
        if (staticY)
            tempStatic = temp.y;
        if (staticZ)
            tempStatic = temp.z;

        //WARNING!!! Magic! Ya, it works...
        if (this.LODLevel >= 1)
        {
            if(this.LODLevel == 1)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * 2.0f, 0.5f); //0.5f
            else if (this.LODLevel == 2)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * 1.33333333333f, 0.75f); //0.5f + 0.5f / 2.0f
            else if (this.LODLevel == 3)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 13.125f), 0.875f); //0.75f + ((0.5f / 2.0f) / 2.0f)
            else if (this.LODLevel == 4)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.0625f), 0.9375f); //0.875f + (((0.5f / 2.0f) / 2.0f) / 2.0f)
            else if (this.LODLevel == 5)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.53125f), 0.96875f); //0.9375f + ((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (this.LODLevel == 6)
                temp = Vector3.Lerp(temp, this.Parent.quadGC.patchCubeCenter * (15.0f / 14.765625f), 0.984375f); //0.96875f + (((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
        }
        //End of magic here.

        if (staticX)
            temp.x = tempStatic;
        if (staticY)
            temp.y = tempStatic;
        if (staticZ)
            temp.z = tempStatic;

        temp = new Vector3(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y), Mathf.RoundToInt(temp.z)); //Just make sure that our values is rounded...

        return temp;
    }

    public Vector3 CalculateMiddlePoint(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
    {
        Vector3 size = bottomLeft - topLeft;
        Vector3 middle = Vector3.zero;

        bool staticX = false, staticY = false, staticZ = false;

        if (size.x == 0)
            staticX = true;
        else if (size.y == 0)
            staticY = true;
        else if (size.z == 0)
            staticZ = true;

        float tempStatic = 0;

        middle = (topLeft + bottmoRight) * (1 / Mathf.Abs(this.LODLevel));
        middle = middle.NormalizeToRadius(this.Planetoid.PlanetRadius);

        if (staticX)
            tempStatic = middle.x;
        if (staticY)
            tempStatic = middle.y;
        if (staticZ)
            tempStatic = middle.z;

        if (staticX)
            middle.x = tempStatic;
        if (staticY)
            middle.y = tempStatic;
        if (staticZ)
            middle.z = tempStatic;

        return middle;
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}