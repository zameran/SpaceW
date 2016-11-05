#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZFramework.Unity.Common.PerfomanceMonitor;

[Serializable]
public struct OutputStruct : IData
{
    public float noise; //4

    public Vector3 patchCenter; //12

    public Vector4 position; //16
    public Vector4 cubePosition; //16

    public int GetStride()
    {
        return 48;
    }
}

public sealed class Quad : MonoBehaviour, IQuad
{
    //NOTE : Do not TransformPoint the points on wich bounds will depend on.

    [Serializable]
    public class Id
    {
        public int LODLevel;
        public int ID;
        public int Position;

        public Id(int LODLevel, int ID, int Position)
        {
            this.LODLevel = LODLevel;
            this.ID = ID;
            this.Position = Position;
        }

        public bool Equals(Id id)
        {
            if (id == null)
            {
                return false;
            }

            return (LODLevel == id.LODLevel && ID == id.ID && Position == id.Position);
        }

        public override int GetHashCode()
        {
            return (LODLevel ^ ID ^ Position).GetHashCode();
        }

        public override string ToString()
        {
            return LODLevel + "," + ID + "," + Position;
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

    public QuadPosition Position;
    public QuadID ID;

    public Planetoid Planetoid;

    public ComputeShader CoreShader { get { return Planetoid.CoreShader; } }

    public Mesh QuadMesh;
    public Material QuadMaterial;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer PreOutDataSubBuffer;
    public ComputeBuffer OutDataBuffer;
    public ComputeBuffer QuadCornersBuffer;

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
    public bool Splitting = false;
    public bool Unsplitted = false;
    public bool Visible = false;
    public bool Uniformed = false;
    public bool GPUDataRecieved = false;
    public bool BuffersCreated = false;

    public float DistanceToLODSplit = Mathf.Infinity;

    public Vector3 topLeftCorner;
    public Vector3 bottomRightCorner;
    public Vector3 topRightCorner;
    public Vector3 bottomLeftCorner;
    public Vector3 middleNormalized;

    public QuadCorners quadCorners;
    public OutputStruct[] outputStructData;

    public QuadAABB QuadAABB = null;

    public Id RegistryID { get { return new Id(LODLevel, (int)ID, (int)Position); } }

    public Matrix4x4 RotationMatrix { get { return Matrix4x4.TRS(middleNormalized, Quaternion.Euler(middleNormalized.normalized * Mathf.Deg2Rad), Vector3.one); } }

    private void Awake()
    {
        CreateBuffers();

        HeightTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0, RenderTextureFormat.ARGB32);
        NormalTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0, RenderTextureFormat.ARGB32);

        RTUtility.ClearColor(new RenderTexture[] { HeightTexture, NormalTexture });
    }

    private void OnDestroy()
    {
        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer, QuadCornersBuffer);

        if (RenderTexture.active == HeightTexture | NormalTexture) RenderTexture.active = null;

        if (HeightTexture != null)
            HeightTexture.ReleaseAndDestroy();

        if (NormalTexture != null)
            NormalTexture.ReleaseAndDestroy();

        if (QuadMaterial != null)
            DestroyImmediate(QuadMaterial);
    }

    private void OnDrawGizmos()
    {
        if (Planetoid.DrawGizmos)
        {
            Bounds bounds = GetBounds(this);

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(bounds.center, bounds.size);

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(topLeftCorner), 1000);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(topRightCorner), 1000);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(bottomLeftCorner), 1000);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(bottomRightCorner), 1000);

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius)), 1750);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius)), 1750);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius)), 1750);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius)), 1750);

            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(quadCorners.topLeftCorner), 1500);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(quadCorners.topRightCorner), 1500);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(quadCorners.bottomLeftCorner), 1500);
            Gizmos.DrawWireSphere(Planetoid.transform.TransformPoint(quadCorners.bottomRightCorner), 1500);

            if (QuadAABB != null)
            {
                Gizmos.color = XKCDColors.Adobe;

                Gizmos.DrawWireCube(Planetoid.transform.TransformPoint(QuadAABB.Bounds.center), QuadAABB.Bounds.size);
            }

            Gizmos.color = XKCDColors.BabyBlue;

            Gizmos.DrawRay(Planetoid.transform.TransformPoint(middleNormalized), middleNormalized);
        }
    }

    public void CreateBuffers()
    {
        if (!BuffersCreated)
        {
            QuadGenerationConstantsBuffer = new ComputeBuffer(1, 96);
            PreOutDataBuffer = new ComputeBuffer(QuadSettings.nVertsReal, 48);
            PreOutDataSubBuffer = new ComputeBuffer(QuadSettings.nVertsSubReal, 48);
            OutDataBuffer = new ComputeBuffer(QuadSettings.nVerts, 48);
            QuadCornersBuffer = new ComputeBuffer(1, 48);

            BuffersCreated = true;
        }
    }

    public void CheckLOD()
    {
        if (QuadAABB == null) return;

        DistanceToLODSplit = GetDistanceToLODSplit() + Planetoid.TerrainMaxHeight;

        if (LODLevel < Planetoid.LODMaxLevel)
        {
            float LODDistance = Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier;

            if (!Planetoid.OneSplittingQuad)
            {
                if (Generated && !HaveSubQuads)
                {
                    if (DistanceToLODSplit < LODDistance && !Splitting)
                    {
                        StartCoroutine(Split());
                        Log("Split Call");
                    }
                }
                else
                {
                    if (DistanceToLODSplit > LODDistance && !Splitting)
                    {
                        Unsplit();
                        Log("Unsplit Call");
                    }
                }
            }
            else
            {
                if (Generated && !HaveSubQuads && !Planetoid.Working)
                {
                    if (DistanceToLODSplit < LODDistance && !Splitting)
                    {
                        StartCoroutine(Split());
                        Log("Split Call");
                    }
                }
                else
                {
                    if (DistanceToLODSplit > LODDistance && !Splitting)
                    {
                        Unsplit();
                        Log("Unsplit Call");
                    }
                }
            }
        }
    }

    public Bounds GetBoundFromPoints(Vector3[] points, out Vector3 max, out Vector3 min)
    {
        var center = points.Aggregate(Vector3.zero, (current, t) => current + t) / 8;

        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p = points[i];

            p = RotationMatrix.MultiplyVector(p);

            if (p.x < min.x) min.x = p.x;
            if (p.y < min.y) min.y = p.y;
            if (p.z < min.z) min.z = p.z;

            if (p.x > max.x) max.x = p.x;
            if (p.y > max.y) max.y = p.y;
            if (p.z > max.z) max.z = p.z;
        }

        var size = max - min;

        return new Bounds(center, size);
    }

    public void Render(int drawLayer = 8)
    {
        Render(CameraHelper.Main(), drawLayer);
    }

    public void Render(Camera camera, int drawLayer = 8)
    {
        if (ReadyForDispatch)
        {
            if (!Generated)
            {
                Dispatch();
            }
        }

        if (QuadAABB == null)
        {
            QuadAABB = GetQuadAABB();
        }

        SetupBounds(this, QuadMesh);

        if (Planetoid.Atmosphere != null) Planetoid.Atmosphere.SetUniforms(null, QuadMaterial, false, true);
        //if (Planetoid.Ring != null) Planetoid.Ring.SetShadows(QuadMaterial, Planetoid.Shadows);
        //if (Planetoid.NPS != null) Planetoid.NPS.UpdateUniforms(QuadMaterial, null); //(WIP) For SE Coloring in fragment shader work...
        //if (Planetoid.tccps != null) Planetoid.tccps.UpdateUniforms(QuadMaterial); //(WIP) For SE Coloring in fragment shader work...

        if (QuadMaterial == null) { return; }

        if (!Uniformed)
        {
            QuadMaterial.SetBuffer("data", OutDataBuffer);
            QuadMaterial.SetBuffer("quadGenerationConstants", QuadGenerationConstantsBuffer);
            QuadMaterial.SetTexture("_HeightTexture", HeightTexture);
            QuadMaterial.SetTexture("_NormalTexture", NormalTexture);

            Uniformed = true;
        }

        QuadMaterial.SetFloat("_Atmosphere", (Planetoid.Atmosphere != null) ? 1.0f : 0.0f);
        QuadMaterial.SetFloat("_Normale", Planetoid.DrawNormals ? 1.0f : 0.0f);
        QuadMaterial.SetMatrix("_TRS", RotationMatrix);
        QuadMaterial.SetFloat("_LODLevel", LODLevel + 2);

        QuadMaterial.renderQueue = (int)Planetoid.RenderQueue + Planetoid.RenderQueueOffset;

        if (Generated && ShouldDraw && QuadMesh != null)
        {
            if (Planetoid.DrawAndCull == QuadDrawAndCull.CullBeforeDraw || Planetoid.DrawAndCull == QuadDrawAndCull.Both)
                TryCull();

            if (Visible)
                Graphics.DrawMesh(QuadMesh, Planetoid.PlanetoidTRS, QuadMaterial, drawLayer, camera, 0, Planetoid.QuadAtmosphereMPB, true, true);

            if (Planetoid.DrawAndCull == QuadDrawAndCull.CullAfterDraw || Planetoid.DrawAndCull == QuadDrawAndCull.Both)
                TryCull();
        }
    }

    public void TryCull()
    {
        using (new Timer("Quad.TryCull"))
        {
            if (GodManager.Instance.UpdateFrustumPlanes == false) return;

            if (Planetoid.CullingMethod == QuadCullingMethod.Custom)
                Visible = PlaneFrustumCheck(QuadAABB);
            else
                Visible = true;
        }
    }

    public QuadAABB GetVolumeBox(float height, float offset = 0)
    {
        Vector3[] points = new Vector3[8];
        Vector3[] cullingPoints = new Vector3[14];

        Vector3 tl = topLeftCorner;
        Vector3 tr = topRightCorner;
        Vector3 bl = bottomLeftCorner;
        Vector3 br = bottomRightCorner;
        Vector3 mi = middleNormalized;

        points[0] = tl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[1] = tr.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[2] = bl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[3] = br.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);

        points[4] = tl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[5] = tr.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[6] = bl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[7] = br.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

        Array.Copy(points, cullingPoints, 8);

        cullingPoints[8] = points[0] - points[4];
        cullingPoints[9] = points[1] - points[5];
        cullingPoints[10] = points[2] - points[6];
        cullingPoints[11] = points[3] - points[7];

        cullingPoints[12] = mi.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        cullingPoints[13] = mi.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

        return new QuadAABB(points, cullingPoints, this);
    }

    public bool PlaneFrustumCheck(QuadAABB qaabb)
    {
        if (qaabb == null) { Log("QuadAABB problem!"); return true; }

        //return GeometryUtility.TestPlanesAABB(GodManager.Instance.FrustumPlanes, QuadAABB.Bounds);
        return PlaneFrustumCheck(qaabb.CullingAABB);
    }

    public bool PlaneFrustumCheck(Vector3[] points)
    {
        if (Parent == null || Splitting || (Planetoid.CullingMethod == QuadCullingMethod.Unity || Planetoid.CullingMethod == QuadCullingMethod.None)) { return true; }

        return points.Any(pos => BorderFrustumCheck(GodManager.Instance.FrustumPlanes, pos) == true);
    }

    public bool BorderFrustumCheck(Plane[] planes, Vector3 border)
    {
        return planes.All(plane => (plane.GetDistanceToPoint(Planetoid.transform.TransformPoint(border)) < 0 - 1024.0f) == false);
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
        Splitting = true;
        Unsplitted = false;

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
                else if (staticY)
                {
                    subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * sY);
                    subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));

                    subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * sY);
                    subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));
                }
                else if (staticZ)
                {
                    subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * sY, topLeftCorner.z);
                    subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);

                    subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * sY, topLeftCorner.z);
                    subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);
                }

                Quad quad = Planetoid.SetupSubQuad(Position);
                quad.Splitting = true;
                quad.ShouldDraw = false;
                quad.InitCorners(subTopLeft, subBottomRight, subTopRight, subBottomLeft);
                quad.SetupParent(quad, this);
                quad.SetupLODLevel(quad);
                quad.SetupID(quad, id);
                quad.SetupVectors(quad, id, staticX, staticY, staticZ);

                if (quad.Parent.transform != null)
                    quad.transform.parent = quad.Parent.transform;

                quad.transform.position = Vector3.zero;
                quad.transform.rotation = Quaternion.identity;
                quad.transform.localPosition = Vector3.zero;
                quad.transform.localRotation = Quaternion.identity;

                quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;

                Subquads.Add(quad);

                for (int wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
                {
                    yield return Yielders.EndOfFrame;
                }
            }
        }

        //Dispatch one by one with intervals.
        foreach (var q in Subquads)
        {
            q.ReadyForDispatch = true;

            for (int wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
            {
                yield return Yielders.EndOfFrame;
            }
        }

        foreach (var q in Subquads)
        {
            q.Splitting = false;
            q.ShouldDraw = true;
        }

        ShouldDraw = false;
        Splitting = false;

        Planetoid.Working = false;
    }

    public void Unsplit()
    {
        if (Unsplitted) return;

        StopAllCoroutines();

        foreach (var subQuad in Subquads)
        {
            if (subQuad.HaveSubQuads)
            {
                subQuad.Unsplit();
            }

            if (Planetoid.Quads.Contains(subQuad))
            {
                Planetoid.Quads.Remove(subQuad);
            }

            DestroyImmediate(subQuad.gameObject);
        }

        if (HaveSubQuads) ShouldDraw = true;

        HaveSubQuads = false;
        Unsplitted = true;
        Subquads.Clear();
    }

    public void Dispatch()
    {
        StartCoroutine(DispatcheCoroutineWait());
    }

    public IEnumerator DispatcheCoroutineWait()
    {
        yield return StartCoroutine(DispatchCoroutine());
    }

    public IEnumerator DispatchCoroutine()
    {
        if (CoreShader == null) StopCoroutine(DispatchCoroutine());

        EventManager.PlanetoidEvents.OnDispatchStarted.Invoke(Planetoid, this);

        generationConstants.SplitLevel = LODLevel + 2;
        generationConstants.LODLevel = (((1 << LODLevel + 2) * (Planetoid.PlanetRadius / (LODLevel + 2)) - ((Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / Planetoid.PlanetRadius);
        generationConstants.LODOctaveModifier = Planetoid.GetLODOctaveModifier(LODLevel + 1);
        generationConstants.orientation = (float)Position;

        SetupComputeShaderUniforms();

        QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { generationConstants };
        OutputStruct[] preOutputStructData = new OutputStruct[QuadSettings.nVertsReal];
        OutputStruct[] preOutputSubStructData = new OutputStruct[QuadSettings.nVertsSubReal];
        OutputStruct[] outputStructData = new OutputStruct[QuadSettings.nVerts];
        QuadCorners[] quadCorners = new QuadCorners[] { new QuadCorners() };

        CreateBuffers();

        QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
        PreOutDataBuffer.SetData(preOutputStructData);
        PreOutDataSubBuffer.SetData(preOutputSubStructData);
        OutDataBuffer.SetData(outputStructData);
        QuadCornersBuffer.SetData(quadCorners);

        int kernel1 = CoreShader.FindKernel("HeightMain");
        int kernel2 = CoreShader.FindKernel("Transfer");
        int kernel3 = CoreShader.FindKernel("HeightSub");
        int kernel4 = CoreShader.FindKernel("TexturesSub");
        int kernel5 = CoreShader.FindKernel("GetCorners");

        SetupComputeShaderKernelsUniforfms(QuadGenerationConstantsBuffer,
                                           PreOutDataBuffer,
                                           PreOutDataSubBuffer,
                                           OutDataBuffer,
                                           QuadCornersBuffer, new int[] { kernel1, kernel2, kernel3, kernel4, kernel5 });

        CoreShader.Dispatch(kernel1,
        QuadSettings.THREADGROUP_SIZE_X_REAL,
        QuadSettings.THREADGROUP_SIZE_Y_REAL,
        QuadSettings.THREADGROUP_SIZE_Z_REAL);

        CoreShader.Dispatch(kernel2,
        QuadSettings.THREADGROUP_SIZE_X,
        QuadSettings.THREADGROUP_SIZE_Y,
        QuadSettings.THREADGROUP_SIZE_Z);

        CoreShader.Dispatch(kernel3,
        QuadSettings.THREADGROUP_SIZE_X_SUB_REAL,
        QuadSettings.THREADGROUP_SIZE_Y_SUB_REAL,
        QuadSettings.THREADGROUP_SIZE_Z_SUB_REAL);

        CoreShader.Dispatch(kernel4,
        QuadSettings.THREADGROUP_SIZE_X_SUB,
        QuadSettings.THREADGROUP_SIZE_Y_SUB,
        QuadSettings.THREADGROUP_SIZE_Z_SUB);

        CoreShader.Dispatch(kernel5,
        QuadSettings.THREADGROUP_SIZE_X_UNIT,
        QuadSettings.THREADGROUP_SIZE_Y_UNIT,
        QuadSettings.THREADGROUP_SIZE_Z_UNIT);

        Generated = true;

        if (LODLevel == -1)
        {
            yield return null;
        }
        else
        {
            for (int i = 0; i < (Planetoid.DispatchSkipFramesCount / 2); i++)
            {
                yield return Yielders.EndOfFrame;
            }
        }

        if (Planetoid.GetData)
        {
            QuadCornersBuffer.GetData(quadCorners);
            OutDataBuffer.GetData(outputStructData);

            this.quadCorners = quadCorners[0];
            this.outputStructData = outputStructData;

            this.GPUDataRecieved = true;

            EventManager.PlanetoidEvents.OnDispatchFinished.Invoke(Planetoid, this);
        }

        //Release and dispose unnecessary buffers. Video memory, you are free!
        BufferHelper.ReleaseAndDisposeBuffers(PreOutDataBuffer, PreOutDataSubBuffer, QuadCornersBuffer);

        BuffersCreated = false;

        EventManager.PlanetoidEvents.OnDispatchEnd.Invoke(Planetoid, this);
    }

    private void SetupComputeShaderUniforms()
    {
        if (Planetoid.tccps != null)
            Planetoid.tccps.UpdateUniforms(CoreShader);
    }

    private void SetupComputeShaderKernelUniforfms(int kernel, ComputeBuffer quadGenerationConstantsBuffer, ComputeBuffer preOutDataBuffer, ComputeBuffer preOutDataSubBuffer, ComputeBuffer outDataBuffer, ComputeBuffer quadCornersBuffer)
    {
        if (CoreShader == null) return;

        CoreShader.SetBuffer(kernel, "quadGenerationConstants", quadGenerationConstantsBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutput", preOutDataBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutputSub", preOutDataSubBuffer);
        CoreShader.SetBuffer(kernel, "patchOutput", outDataBuffer);
        CoreShader.SetBuffer(kernel, "quadCorners", quadCornersBuffer);

        CoreShader.SetTexture(kernel, "Height", HeightTexture);
        CoreShader.SetTexture(kernel, "Normal", NormalTexture);

        if (Planetoid.NPS != null)
            Planetoid.NPS.UpdateUniforms(QuadMaterial, CoreShader, kernel);
    }

    private void SetupComputeShaderKernelsUniforfms(ComputeBuffer quadGenerationConstantsBuffer, ComputeBuffer preOutDataBuffer, ComputeBuffer preOutDataSubBuffer, ComputeBuffer outDataBuffer, ComputeBuffer quadCornersBuffer, params int[] kernels)
    {
        if (kernels == null || kernels.Length == 0) { Debug.Log("Quad.SetupComputeShaderKernelsUniforfms(...) problem!"); return; }

        for (int i = 0; i < kernels.Length; i++)
        {
            SetupComputeShaderKernelUniforfms(i, quadGenerationConstantsBuffer, preOutDataBuffer, preOutDataSubBuffer, outDataBuffer, quadCornersBuffer);
        }
    }

    public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
    {
        var cfed = Parent.generationConstants.cubeFaceEastDirection / 2.0f;
        var cfnd = Parent.generationConstants.cubeFaceNorthDirection / 2.0f;

        quad.generationConstants.cubeFaceEastDirection = cfed;
        quad.generationConstants.cubeFaceNorthDirection = cfnd;
        quad.generationConstants.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
    }

    public void SetupCorners(QuadPosition quadPosition)
    {
        var v = Planetoid.PlanetRadius / 2;

        switch (quadPosition)
        {
            case QuadPosition.Top:
                topLeftCorner = new Vector3(-v, v, v);
                bottomRightCorner = new Vector3(v, v, -v);

                topRightCorner = new Vector3(v, v, v);
                bottomLeftCorner = new Vector3(-v, v, -v);
                break;
            case QuadPosition.Bottom:
                topLeftCorner = new Vector3(-v, -v, -v);
                bottomRightCorner = new Vector3(v, -v, v);

                topRightCorner = new Vector3(v, -v, -v);
                bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPosition.Left:
                topLeftCorner = new Vector3(-v, v, v);
                bottomRightCorner = new Vector3(-v, -v, -v);

                topRightCorner = new Vector3(-v, v, -v);
                bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPosition.Right:
                topLeftCorner = new Vector3(v, v, -v);
                bottomRightCorner = new Vector3(v, -v, v);

                topRightCorner = new Vector3(v, v, v);
                bottomLeftCorner = new Vector3(v, -v, -v);
                break;
            case QuadPosition.Front:
                topLeftCorner = new Vector3(v, v, v);
                bottomRightCorner = new Vector3(-v, -v, v);

                topRightCorner = new Vector3(-v, v, v);
                bottomLeftCorner = new Vector3(v, -v, v);
                break;
            case QuadPosition.Back:
                topLeftCorner = new Vector3(-v, v, -v);
                bottomRightCorner = new Vector3(v, -v, -v);

                topRightCorner = new Vector3(v, v, -v);
                bottomLeftCorner = new Vector3(-v, -v, -v);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        middleNormalized = CalculateMiddlePoint(topLeftCorner, bottomRightCorner, topRightCorner, bottomLeftCorner);
    }

    public void SetupParent(Quad quad, Quad parent)
    {
        quad.Parent = parent;
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
        mesh.bounds = GetBounds(quad); //new Bounds(generationConstants.patchCubeCenter, GetBoundsSize(quad));
    }

    public float GetDistanceToLODSplit()
    {
        float distance = Mathf.Infinity;

        switch (Planetoid.LODDistanceMethod)
        {
            case QuadLODDistanceMethod.ClosestCorner:
                distance = GetDistanceToClosestCorner();
                break;
            case QuadLODDistanceMethod.ClosestAABBCorner:
                distance = GetDistanceToClosestAABBCorner();
                break;
            default:
                distance = GetDistanceToClosestCorner();
                break;
        }

        return distance;
    }

    private float GetDistanceToClosestCorner()
    {
        return Vector3.Distance(Planetoid.LODTarget.position, GetClosestCorner());
    }

    private float GetDistanceToClosestAABBCorner()
    {
        if (QuadAABB.AABB == null || QuadAABB.AABB.Length == 0 || QuadAABB.AABB.Length <= 4)
        {
            Debug.Log("Quad.GetClosestAABBCorner(...) QuadAABB.AABB problem!");
            return Mathf.Infinity;
        }

        return Vector3.Distance(Planetoid.LODTarget.position, GetClosestAABBCorner());
    }

    private Vector3 GetClosestAABBCorner()
    {
        float closestDistance = Mathf.Infinity;
        float d;

        Vector3 closestCorner = Vector3.zero;

        for (int i = 0; i < 4; i++)
        {
            d = Vector3.Distance(Planetoid.LODTarget.position, Planetoid.transform.TransformPoint(QuadAABB.AABB[i]));
            if (d < closestDistance)
            {
                closestCorner = QuadAABB.AABB[i];
                closestDistance = d;
            }
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, Planetoid.transform.TransformPoint(middleNormalized));
        if (d < closestDistance)
        {
            closestCorner = middleNormalized;
            closestDistance = d;
        }

        return Planetoid.transform.TransformPoint(closestCorner);
    }

    private Vector3 GetClosestCorner()
    {
        float closestDistance = Mathf.Infinity;
        float d;

        Vector3 closestCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        Vector3 tl = Vector3.zero;
        Vector3 tr = Vector3.zero;
        Vector3 middlePoint = Planetoid.transform.TransformPoint(middleNormalized);
        Vector3 bl = Vector3.zero;
        Vector3 br = Vector3.zero;

        if (Planetoid.GetData && GPUDataRecieved)
        {
            tl = Planetoid.transform.TransformPoint(quadCorners.topLeftCorner);
            tr = Planetoid.transform.TransformPoint(quadCorners.topRightCorner);
            bl = Planetoid.transform.TransformPoint(quadCorners.bottomLeftCorner);
            br = Planetoid.transform.TransformPoint(quadCorners.bottomRightCorner);
        }
        else
        {
            tl = Planetoid.transform.TransformPoint(topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
            tr = Planetoid.transform.TransformPoint(topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));
            bl = Planetoid.transform.TransformPoint(bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
            br = Planetoid.transform.TransformPoint(bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, tl);

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

        return Generated ? closestCorner : new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    }

    public QuadAABB GetQuadAABB()
    {
        return GetVolumeBox(Planetoid.TerrainMaxHeight, 0);
    }

    public Bounds GetBounds(Quad quad)
    {
        if (Planetoid.CullingMethod == QuadCullingMethod.Unity)
        {
            //NOTE : https://inovaestudios.blob.core.windows.net/forumsavatars/3255860d4f86c8c8a67cdb0b79e7e8889951cc54a65a.png

            if (QuadAABB != null)
                return QuadAABB.Bounds;
            else
                return new Bounds(quad.generationConstants.patchCubeCenter, new Vector3(9e37f, 9e37f, 9e37f));
        }
        else
            return new Bounds(quad.generationConstants.patchCubeCenter, new Vector3(9e37f, 9e37f, 9e37f));
    }

    public Vector3 GetCubeFaceEastDirection(QuadPosition quadPosition)
    {
        var temp = Vector3.zero;
        var r = Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPosition.Top:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPosition.Bottom:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPosition.Left:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPosition.Right:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPosition.Front:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPosition.Back:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        return temp;
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPosition quadPosition)
    {
        var temp = Vector3.zero;
        var r = Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPosition.Top:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPosition.Bottom:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPosition.Left:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            case QuadPosition.Right:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPosition.Front:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPosition.Back:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenter(QuadPosition quadPosition)
    {
        var temp = Vector3.zero;
        var r = Planetoid.PlanetRadius;

        switch (quadPosition)
        {
            case QuadPosition.Top:
                temp = new Vector3(0.0f, r, 0.0f);
                break;
            case QuadPosition.Bottom:
                temp = new Vector3(0.0f, -r, 0.0f);
                break;
            case QuadPosition.Left:
                temp = new Vector3(-r, 0.0f, 0.0f);
                break;
            case QuadPosition.Right:
                temp = new Vector3(r, 0.0f, 0.0f);
                break;
            case QuadPosition.Front:
                temp = new Vector3(0.0f, 0.0f, r);
                break;
            case QuadPosition.Back:
                temp = new Vector3(0.0f, 0.0f, -r);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenterSplitted(QuadPosition quadPosition, int id, bool staticX, bool staticY, bool staticZ)
    {
        var temp = Vector3.zero;
        var mod = 0.5f;
        var v = Planetoid.PlanetRadius;

        float tempStatic = 0;

        switch (quadPosition)
        {
            case QuadPosition.Top:
                if (id == 0)
                    temp += new Vector3(-v * mod, v, v * mod);
                else if (id == 1)
                    temp += new Vector3(v * mod, v, v * mod);
                else if (id == 2)
                    temp += new Vector3(-v * mod, v, -v * mod);
                else if (id == 3)
                    temp += new Vector3(v * mod, v, -v * mod);
                break;
            case QuadPosition.Bottom:
                if (id == 0)
                    temp += new Vector3(-v * mod, -v, -v * mod);
                else if (id == 1)
                    temp += new Vector3(v * mod, -v, -v * mod);
                else if (id == 2)
                    temp += new Vector3(-v * mod, -v, v * mod);
                else if (id == 3)
                    temp += new Vector3(v * mod, -v, v * mod);
                break;
            case QuadPosition.Left:
                if (id == 0)
                    temp += new Vector3(-v, v * mod, v * mod);
                else if (id == 1)
                    temp += new Vector3(-v, v * mod, -v * mod);
                else if (id == 2)
                    temp += new Vector3(-v, -v * mod, v * mod);
                else if (id == 3)
                    temp += new Vector3(-v, -v * mod, -v * mod);
                break;
            case QuadPosition.Right:
                if (id == 0)
                    temp += new Vector3(v, v * mod, -v * mod);
                else if (id == 1)
                    temp += new Vector3(v, v * mod, v * mod);
                else if (id == 2)
                    temp += new Vector3(v, -v * mod, -v * mod);
                else if (id == 3)
                    temp += new Vector3(v, -v * mod, v * mod);
                break;
            case QuadPosition.Front:
                if (id == 0)
                    temp += new Vector3(v * mod, v * mod, v);
                else if (id == 1)
                    temp += new Vector3(-v * mod, v * mod, v);
                else if (id == 2)
                    temp += new Vector3(v * mod, -v * mod, v);
                else if (id == 3)
                    temp += new Vector3(-v * mod, -v * mod, v);
                break;
            case QuadPosition.Back:
                if (id == 0)
                    temp += new Vector3(-v * mod, v * mod, -v);
                else if (id == 1)
                    temp += new Vector3(v * mod, v * mod, -v);
                else if (id == 2)
                    temp += new Vector3(-v * mod, -v * mod, -v);
                else if (id == 3)
                    temp += new Vector3(v * mod, -v * mod, -v);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        BrainFuckMath.LockAxis(ref tempStatic, ref temp, staticX, staticY, staticZ);
        BrainFuckMath.CalculatePatchCubeCenter(LODLevel, Parent.generationConstants.patchCubeCenter, ref temp);
        BrainFuckMath.UnlockAxis(ref temp, ref tempStatic, staticX, staticY, staticZ);

        //Just make sure that our vector values is rounded...
        //if(Planetoid.PlanetRadius % 2 == 0) temp = temp.RoundToInt();
        //NOTE : FLOATING POINT PRECISION ANYWAY!

        return temp;
    }

    public Vector3 CalculateMiddlePoint(Vector3 topLeft, Vector3 bottomRight, Vector3 topRight, Vector3 bottomLeft)
    {
        Vector3 size = bottomLeft - topLeft;
        Vector3 middle = Vector3.zero;

        bool staticX = false, staticY = false, staticZ = false;

        float tempStatic = 0;

        BrainFuckMath.DefineAxis(ref staticX, ref staticY, ref staticZ, size);

        middle = (topLeft + bottomRight) * (1.0f / Mathf.Abs(LODLevel));
        middle = middle.NormalizeToRadius(Planetoid.PlanetRadius);

        BrainFuckMath.LockAxis(ref tempStatic, ref middle, staticX, staticY, staticZ);
        BrainFuckMath.UnlockAxis(ref middle, ref tempStatic, staticX, staticY, staticZ);

        return middle;
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}