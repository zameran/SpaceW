using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using ZFramework.Unity.Common.PerfomanceMonitor;

[Serializable]
public struct QuadGenerationConstants
{
    public float planetRadius; //4
    public float spacing; //4
    public float spacingreal;
    public float spacingsub;
    public float terrainMaxHeight; //4
    public float LODLevel; //4
    public float orientation;

    public Vector3 cubeFaceEastDirection; //12
    public Vector3 cubeFaceNorthDirection; //12
    public Vector3 patchCubeCenter; //12

    //12 * 3 + 4 * 3 = 36 + 12 = 48

    public static QuadGenerationConstants Init()
    {
        QuadGenerationConstants temp = new QuadGenerationConstants();

        temp.spacing = QS.nSpacing;
        temp.spacingreal = QS.nSpacingReal;
        temp.spacingsub = QS.nSpacingSub;
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
    [Serializable]
    public class Id
    {
        public int LODLevel;
        public int ID;
        public int Position;

        public string Name;

        public Vector3 cubeFaceEastDirection;
        public Vector3 cubeFaceNorthDirection;
        public Vector3 patchCubeCenter;

        public Id(int LODLevel, int ID, int Position)
        {
            this.LODLevel = LODLevel;
            this.ID = ID;
            this.Position = Position;
        }

        public Id(int LODLevel, int ID, int Position, string Name, Vector3 cubeFaceEastDirection, Vector3 cubeFaceNorthDirection, Vector3 patchCubeCenter)
        {
            this.LODLevel = LODLevel;
            this.ID = ID;
            this.Position = Position;

            this.Name = Name;

            this.cubeFaceEastDirection = cubeFaceEastDirection;
            this.cubeFaceNorthDirection = cubeFaceNorthDirection;
            this.patchCubeCenter = patchCubeCenter;
        }

        public int Compare(Id id)
        {
            return LODLevel.CompareTo(id.LODLevel);
        }

        public bool Equals(Id id)
        {
            return (LODLevel == id.LODLevel && ID == id.ID && Position == id.Position && Name == id.Name &&
                    cubeFaceEastDirection == id.cubeFaceEastDirection &&
                    cubeFaceNorthDirection == id.cubeFaceNorthDirection &&
                    patchCubeCenter == id.patchCubeCenter);
        }

        public override int GetHashCode()
        {
            int code = LODLevel ^ ID ^ Position;
            return code.GetHashCode();
        }

        public override string ToString()
        {
            return LODLevel.ToString() + "," + ID.ToString() + "," + Position.ToString();
        }
    }

    public class EqualityComparerID : IEqualityComparer<Id>
    {
        public bool Equals(Id t1, Id t2)
        {
            return t1.Equals(t2);
        }

        public int GetHashCode(Id t)
        {
            return t.GetHashCode();
        }
    }

    public QuadPostion Position;
    public QuadID ID;

    public Planetoid Planetoid;

    public ComputeShader CoreShader;

    public Mesh QuadMesh;
    public Material QuadMaterial;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer PreOutDataSubBuffer;
    public ComputeBuffer OutDataBuffer;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadGenerationConstants generationConstants;

    public Quad Parent;

    public List<Quad> Subquads = new List<Quad>();

    public int LODLevel = -1;

    public bool HaveSubQuads = false;
    public bool Generated = false;
    public bool ShouldDraw = false;
    public bool ReadyForDispatch = false;

    public float lodUpdateInterval = 0.25f;
    public float lastLodUpdateTime = 0.00f;

    public Vector3 topLeftCorner;
    public Vector3 bottomRightCorner;
    public Vector3 topRightCorner;
    public Vector3 bottomLeftCorner;
    public Vector3 middleNormalized;

    public delegate void QuadDelegate(Quad q);
    public event QuadDelegate DispatchStarted, DispatchReady, GPUGetDataReady;

    public Id GetId()
    {
        return GetId(LODLevel, (int)ID, (int)Position, this.gameObject.name,
                     generationConstants.cubeFaceEastDirection,
                     generationConstants.cubeFaceNorthDirection,
                     generationConstants.patchCubeCenter);
    }

    public static Id GetId(int LODLevel, int ID, int Position, string Name, Vector3 cubeFaceEastDirection, Vector3 cubeFaceNorthDirection, Vector3 patchCubeCenter)
    {
        return new Id(LODLevel, ID, Position, Name, cubeFaceEastDirection, cubeFaceEastDirection, cubeFaceNorthDirection);
    }

    private void QuadDispatchStarted(Quad q)
    {
        Log("DispatchStarted event fire!");
    }

    private void QuadDispatchReady(Quad q)
    {
        Log("DispatchReady event fire!");
    }

    private void QuadGPUGetDataReady(Quad q)
    {
        Log("GPUGetDataReady event fire!");
    }

    public Quad()
    {
        DispatchStarted += QuadDispatchStarted;
        DispatchReady += QuadDispatchReady;
        GPUGetDataReady += QuadGPUGetDataReady;
    }

    private void Awake()
    {
        QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
        PreOutDataBuffer = new ComputeBuffer(QS.nVertsReal, 64);
        PreOutDataSubBuffer = new ComputeBuffer(QS.nRealVertsSub, 64);
        OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);

        HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { HeightTexture, NormalTexture });
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (Time.time > lastLodUpdateTime + lodUpdateInterval && Planetoid.UseLOD)
        {
            this.lastLodUpdateTime = Time.time;

            if (LODLevel < Planetoid.LODMaxLevel)
            {
                if (Generated && !HaveSubQuads && !Planetoid.Working)
                {
                    if (GetDistanceToClosestCorner() < Planetoid.LODDistances[LODLevel + 1])
                    {
                        StartCoroutine(Split());
                    }
                }
                else
                {
                    if (GetDistanceToClosestCorner() > Planetoid.LODDistances[LODLevel + 1])
                    {
                        Unsplit();
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer);

        if (HeightTexture != null)
            HeightTexture.ReleaseAndDestroy();

        if (NormalTexture != null)
            NormalTexture.ReleaseAndDestroy();

        if (QuadMesh != null)
            DestroyImmediate(QuadMesh);

        if (QuadMaterial != null)
            DestroyImmediate(QuadMaterial);

        if (DispatchStarted != null)
            DispatchStarted -= QuadDispatchStarted;

        if (DispatchReady != null)
            DispatchReady -= QuadDispatchReady;

        if (GPUGetDataReady != null)
            GPUGetDataReady -= QuadGPUGetDataReady;
    }

    private void OnWillRenderObject()
    {

    }

    private void OnRenderObject()
    {
        if (ReadyForDispatch)
        {
            if (!Generated)
                Dispatch();
        }

        QuadMaterial.SetPass(0);
        QuadMaterial.SetBuffer("data", OutDataBuffer);
        QuadMaterial.SetTexture("_HeightTexture", HeightTexture);
        QuadMaterial.SetTexture("_NormalTexture", NormalTexture);
        QuadMaterial.SetFloat("_Wireframe", Planetoid.DrawWireframe ? 1.0f : 0.0f);

        if (Generated && ShouldDraw)
        {
            if (QuadMesh != null)
                Graphics.DrawMeshNow(QuadMesh, transform.localToWorldMatrix, 0);
        }
    }

    public void InitCorners(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
    {
        topLeftCorner = topLeft;
        bottomRightCorner = bottmoRight;
        topRightCorner = topRight;
        bottomLeftCorner = bottomLeft;

        middleNormalized = CalculateMiddlePoint(topLeft, bottmoRight, topRight, bottmoRight);
    }

    public IEnumerator Split()
    {
        int id = 0;

        Vector3 size = bottomRightCorner - topLeftCorner;
        Vector3 step = size / 2;

        bool staticX = false, staticY = false, staticZ = false;

        if (step.x == 0)
            staticX = true;
        if (step.y == 0)
            staticY = true;
        if (step.z == 0)
            staticZ = true;

        Planetoid.Working = true;
        HaveSubQuads = true;

        for (int sY = 0; sY < 2; sY++)
        {
            for (int sX = 0; sX < 2; sX++, id++)
            {
                Vector3 subTopLeft = Vector3.zero, subBottomRight = Vector3.zero;
                Vector3 subTopRight = Vector3.zero, subBottomLeft = Vector3.zero;

                if (staticX)
                {
                    subTopLeft = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * sY, topLeftCorner.z + step.z * sX);
                    subBottomRight = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z + step.z * (sX + 1));

                    subTopRight = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * sY, topLeftCorner.z + step.z * (sX + 1));
                    subBottomLeft = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z + step.z * sX);
                }

                if (staticY)
                {
                    subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * sY);
                    subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));

                    subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * sY);
                    subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));
                }

                if (staticZ)
                {
                    subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * sY, topLeftCorner.z);
                    subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);

                    subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * sY, topLeftCorner.z);
                    subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);
                }

                Quad quad = Planetoid.SetupSubQuad(Position);
                quad.ShouldDraw = false;
                quad.InitCorners(subTopLeft, subBottomRight, subTopRight, subBottomLeft);
                quad.SetupParent(this);
                quad.SetupLODLevel(quad);
                quad.SetupID(quad, id);
                quad.SetupVectors(quad, id, staticX, staticY, staticZ);

                quad.transform.parent = transform;
                quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;

                quad.ReadyForDispatch = true;

                Subquads.Add(quad);

                for (int wait = 0; wait < 8; wait++)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        foreach (Quad q in Subquads)
        {
            q.ShouldDraw = true;
        }

        ShouldDraw = false;

        Planetoid.Working = false;
    }

    public void Unsplit()
    {
        StopAllCoroutines();

        for (int i = 0; i < Subquads.Count; i++)
        {
            if (Subquads[i].HaveSubQuads)
            {
                //Subquads[i].Unsplit();
            }

            if (Planetoid.Quads.Contains(Subquads[i]))
            {
                Planetoid.Quads.Remove(Subquads[i]);
            }

            if (Subquads[i] != null)
            {
                DestroyImmediate(Subquads[i].gameObject);
            }
        }

        if (HaveSubQuads == true) ShouldDraw = true;
        HaveSubQuads = false;
        Subquads.Clear();
        //Planetoid.Working = false;
    }

    public void Dispatch()
    {
        if (DispatchStarted != null)
            DispatchStarted(this);

        Planetoid.NPS.UpdateUniforms(QuadMaterial, CoreShader);

        generationConstants.LODLevel = (((1 << LODLevel + 2) * (Planetoid.PlanetRadius / (LODLevel + 2)) - ((Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / Planetoid.PlanetRadius);
        generationConstants.orientation = (float)Position;

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { generationConstants };
        OutputStruct[] preOutputStructData = new OutputStruct[QS.nVertsReal];
        OutputStruct[] preOutputSubStructData = new OutputStruct[QS.nRealVertsSub];
        OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(preOutputStructData);
        PreOutDataSubBuffer.SetData(preOutputSubStructData);
        OutDataBuffer.SetData(outputStructData);

        int kernel1 = CoreShader.FindKernel("HeightMain");
        int kernel2 = CoreShader.FindKernel("Transfer");
        int kernel3 = CoreShader.FindKernel("HeightSub");
        int kernel4 = CoreShader.FindKernel("TexturesSub");

        SetupComputeShader(kernel1, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for first kernel ready!");

        CoreShader.Dispatch(kernel1,
        QS.THREADGROUP_SIZE_X_REAL,
        QS.THREADGROUP_SIZE_Y_REAL,
        QS.THREADGROUP_SIZE_Z_REAL); Log("First kernel ready!");

        SetupComputeShader(kernel2, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for second kernel ready!");

        CoreShader.Dispatch(kernel2,
        QS.THREADGROUP_SIZE_X,
        QS.THREADGROUP_SIZE_Y,
        QS.THREADGROUP_SIZE_Z); Log("Second kernel ready!");

        SetupComputeShader(kernel3, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for third kernel ready!");

        CoreShader.Dispatch(kernel3,
        QS.THREADGROUP_SIZE_X_SUB_REAL,
        QS.THREADGROUP_SIZE_Y_SUB_REAL,
        QS.THREADGROUP_SIZE_Z_SUB_REAL); Log("Third kernel ready!");

        SetupComputeShader(kernel4, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for fourth kernel ready!");

        CoreShader.Dispatch(kernel4,
        QS.THREADGROUP_SIZE_X_SUB,
        QS.THREADGROUP_SIZE_Y_SUB,
        QS.THREADGROUP_SIZE_Z_SUB); Log("Fourth kernel ready!");

        Generated = true;

        if (DispatchReady != null)
            DispatchReady(this);
    }

    private bool AllSubquadsGenerated()
    {
        if (Subquads.Count != 0)
        {
            var state = true;
            return Subquads.All(s => s.Generated == state);
        }
        else
            return false;
    }

    private void SetupComputeShader(int kernel, ComputeBuffer QuadGenerationConstantsBuffer, ComputeBuffer PreOutDataBuffer, ComputeBuffer PreOutDataSubBuffer, ComputeBuffer OutDataBuffer)
    {
        if (CoreShader == null) return;

        CoreShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutputSub", PreOutDataSubBuffer);
        CoreShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        CoreShader.SetTexture(kernel, "Height", HeightTexture);
        CoreShader.SetTexture(kernel, "Normal", NormalTexture);

        Planetoid.NPS.SetUniforms(CoreShader, kernel);
    }

    public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 cfed = Parent.generationConstants.cubeFaceEastDirection / 2;
        Vector3 cfnd = Parent.generationConstants.cubeFaceNorthDirection / 2;

        quad.generationConstants.cubeFaceEastDirection = cfed;
        quad.generationConstants.cubeFaceNorthDirection = cfnd;
        quad.generationConstants.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
    }

    public void SetupCorners(QuadPostion pos)
    {
        float v = Planetoid.PlanetRadius / 2;

        switch (pos)
        {
            case QuadPostion.Top:
                topLeftCorner = new Vector3(-v, v, v);
                bottomRightCorner = new Vector3(v, v, -v);

                topRightCorner = new Vector3(v, v, v);
                bottomLeftCorner = new Vector3(-v, v, -v);
                break;
            case QuadPostion.Bottom:
                topLeftCorner = new Vector3(-v, -v, -v);
                bottomRightCorner = new Vector3(v, -v, v);

                topRightCorner = new Vector3(v, -v, -v);
                bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Left:
                topLeftCorner = new Vector3(-v, v, v);
                bottomRightCorner = new Vector3(-v, -v, -v);

                topRightCorner = new Vector3(-v, v, -v);
                bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPostion.Right:
                topLeftCorner = new Vector3(v, v, -v);
                bottomRightCorner = new Vector3(v, -v, v);

                topRightCorner = new Vector3(v, v, v);
                bottomLeftCorner = new Vector3(v, -v, -v);
                break;
            case QuadPostion.Front:
                topLeftCorner = new Vector3(v, v, v);
                bottomRightCorner = new Vector3(-v, -v, v);

                topRightCorner = new Vector3(-v, v, v);
                bottomLeftCorner = new Vector3(v, -v, v);
                break;
            case QuadPostion.Back:
                topLeftCorner = new Vector3(-v, v, -v);
                bottomRightCorner = new Vector3(v, -v, -v);

                topRightCorner = new Vector3(v, v, -v);
                bottomLeftCorner = new Vector3(-v, -v, -v);
                break;
        }

        middleNormalized = CalculateMiddlePoint(topLeftCorner,
                                                bottomRightCorner,
                                                topRightCorner,
                                                bottomLeftCorner);
    }

    public void SetupParent(Quad parent)
    {
        Parent = parent;
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

    public float GetDistanceToClosestCorner()
    {
        return Vector3.Distance(Planetoid.LODTarget.position, GetClosestCorner());
    }

    public Vector3 GetClosestCorner()
    {
        float closestDistance = Mathf.Infinity;

        Vector3 closestCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        Vector3 tl = topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 tr = topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 middlePoint = middleNormalized;
        Vector3 bl = bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 br = bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);

        float d = Vector3.Distance(Planetoid.LODTarget.position, tl);

        if (d < closestDistance)
        {
            closestCorner = tl;
            closestDistance = d;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, tr);

        if (d < closestDistance)
        {
            closestCorner = tr;
            closestDistance = d;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, middlePoint);

        if (d < closestDistance)
        {
            closestCorner = middlePoint;
            closestDistance = d;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, bl);

        if (d < closestDistance)
        {
            closestCorner = bl;
            closestDistance = d;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, br);

        if (d < closestDistance)
        {
            closestCorner = br;
            closestDistance = d;
        }

        if (Generated)
            return closestCorner;
        else
            return new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    }

    public float GetClosestDistance(float offset)
    {
        float closestDistance = Mathf.Infinity;

        Vector3 tl = topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 tr = topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 middlePoint = middleNormalized;
        Vector3 bl = bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
        Vector3 br = bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);

        float d = Vector3.Distance(Planetoid.LODTarget.position, tl);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, tr);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, middlePoint);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, bl);

        if (d < closestDistance)
        {
            closestDistance = d + offset;
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, br);

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

        float r = Planetoid.PlanetRadius;

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

        float r = Planetoid.PlanetRadius;

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

        float r = Planetoid.PlanetRadius;

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

        float v = Planetoid.PlanetRadius;

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

        //TODO : Make a formula!
        //So. We have exponential modifier... WTF!?
        //Fuck dat shit. 7 LOD level more than i need. fuck. dat.

        //WARNING!!! Magic! Ya, it works...
        if (LODLevel >= 1)
        {
            if (LODLevel == 1)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 7.5f), 0.5f); //0.5f
            else if (LODLevel == 2)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 11.25f), 0.75f); //0.5f + 0.5f / 2.0f
            else if (LODLevel == 3)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 13.125f), 0.875f); //0.75f + ((0.5f / 2.0f) / 2.0f)
            else if (LODLevel == 4)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.0625f), 0.9375f); //0.875f + (((0.5f / 2.0f) / 2.0f) / 2.0f)
            else if (LODLevel == 5)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.53125f), 0.96875f); //0.9375f + ((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (LODLevel == 6)
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.765625f), 0.984375f); //0.96875f + (((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (LODLevel == 7) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.8828125f), 0.9921875f); //0.984375f + ((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (LODLevel == 8) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.94140625f), 0.99609375f); //0.9921875f + (((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
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

        middle = (topLeft + bottmoRight) * (1 / Mathf.Abs(LODLevel));
        middle = middle.NormalizeToRadius(Planetoid.PlanetRadius);

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

    private void Log(string msg, bool state)
    {
        if (state)
            Debug.Log(msg);
    }
}