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

using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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

public sealed class Quad : MonoBehaviour, IQuad, IEventit<Quad>
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
            if ((this == null) || (id == null))
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
    public bool Cached = false;
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
    public Box3d QuadBox = null;

    public delegate void QuadDelegate(Quad q);
    public event QuadDelegate DispatchStarted, DispatchReady, GPUGetDataReady;

    public Id RegistryID { get { return new Id(LODLevel, (int)ID, (int)Position); } }

    public Matrix4x4 RotationMatrix { get { return Matrix4x4.TRS(middleNormalized, Quaternion.Euler((middleNormalized).normalized * Mathf.Deg2Rad), Vector3.one); } }

    #region Eventit
    public bool isEventit { get; set; }

    public void Eventit(Quad quad)
    {
        if (isEventit) return;

        quad.DispatchStarted += quad.QuadDispatchStarted;
        quad.DispatchReady += quad.QuadDispatchReady;
        quad.GPUGetDataReady += quad.QuadGPUGetDataReady;

        if (quad.Planetoid != null)
        {
            quad.DispatchStarted += quad.Planetoid.QuadDispatchStarted;
            quad.DispatchReady += quad.Planetoid.QuadDispatchReady;
            quad.GPUGetDataReady += quad.Planetoid.QuadGPUGetDataReady;
        }

        isEventit = true;
    }

    public void UnEventit(Quad quad)
    {
        if (!isEventit) return;

        DispatchStarted -= QuadDispatchStarted;
        DispatchReady -= QuadDispatchReady;
        GPUGetDataReady -= QuadGPUGetDataReady;

        if (quad.Planetoid != null)
        {
            DispatchStarted -= Planetoid.QuadDispatchStarted;
            DispatchReady -= Planetoid.QuadDispatchReady;
            GPUGetDataReady -= Planetoid.QuadGPUGetDataReady;
        }

        isEventit = false;
    }
    #endregion

    private void QuadDispatchStarted(Quad q)
    {
        //Debug.Log("Quad: QuadDispatchStarted() - " + q.gameObject.name);
    }

    private void QuadDispatchReady(Quad q)
    {
        //Debug.Log("Quad: QuadDispatchReady() - " + q.gameObject.name);

        if (LODLevel == 6 && Planetoid.GenerateColliders && GPUDataRecieved)
        {
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = MeshFactory.SetupQuadColliderMesh(outputStructData);
        }
    }

    private void QuadGPUGetDataReady(Quad q)
    {
        //Debug.Log("Quad: QuadGPUGetDataReady() - " + q.gameObject.name);
    }

    public Quad()
    {

    }

    private void Awake()
    {
        CreateBuffers();

        HeightTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0, RenderTextureFormat.ARGB32);
        NormalTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0, RenderTextureFormat.ARGB32);

        RTUtility.ClearColor(new RenderTexture[] { HeightTexture, NormalTexture });
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {

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

        UnEventit(this);
    }

    private void OnWillRenderObject()
    {

    }

    private void OnRenderObject()
    {

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

    private Bounds GetBoundFromPoints(Vector3[] points, out Vector3 max, out Vector3 min)
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

    private Box3d GetBoundFromPoints(Vector3[] points)
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        Box3d box = new Box3d();

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p = points[i];

            p = RotationMatrix.MultiplyPoint(p);

            if (p.x < min.x) min.x = p.x;
            if (p.y < min.y) min.y = p.y;
            if (p.z < min.z) min.z = p.z;

            if (p.x > max.x) max.x = p.x;
            if (p.y > max.y) max.y = p.y;
            if (p.z > max.z) max.z = p.z;
        }

        box.xmax = max.x;
        box.ymax = max.y;
        box.zmax = max.z;
        box.xmin = min.x;
        box.ymin = min.y;
        box.zmin = min.z;

        return box;
    }

    public void Render(int drawLayer = 8)
    {
        Render(CameraHelper.Main(), drawLayer);
    }

    public void Render(Camera camera, int drawLayer = 8)
    {
        if (ReadyForDispatch)
        {
            if (!Generated && !Planetoid.Cache.Working)
            {
                Dispatch();
            }
        }

        if (QuadAABB == null)
        {
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;

            QuadAABB = new QuadAABB(GetVolumeBox(Planetoid.TerrainMaxHeight, 0, true), false);
            QuadAABB.Bounds = GetBoundFromPoints(GetVolumeBox(Planetoid.TerrainMaxHeight, 0, false), out max, out min);
            QuadAABB.Max = max;
            QuadAABB.Min = min;
        }

        if(QuadBox == null)
        {
            QuadBox = GetBoundFromPoints(GetVolumeBox(0));
        }

        SetupBounds(this, QuadMesh);

        if (Planetoid.Atmosphere != null) Planetoid.Atmosphere.SetUniforms(null, QuadMaterial, false, true);

        //if (Planetoid.NPS != null) Planetoid.NPS.UpdateUniforms(QuadMaterial, null); //(WIP) For SE Coloring in fragment shader work...
        //if (Planetoid.tccps != null) Planetoid.tccps.UpdateUniforms(QuadMaterial); //(WIP) For SE Coloring in fragment shader work...

        if (QuadMaterial == null) { return; }

        if (!Uniformed)
        {
            QuadMaterial.SetBuffer("data", OutDataBuffer);
            QuadMaterial.SetBuffer("quadGenerationConstants", QuadGenerationConstantsBuffer);

            Uniformed = true;
        }

        QuadMaterial.SetTexture("_HeightTexture", HeightTexture);
        QuadMaterial.SetTexture("_NormalTexture", NormalTexture);
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
            if (Planetoid.CullingMethod == QuadCullingMethod.Custom)
                Visible = PlaneFrustumCheck(QuadAABB);
            else
                Visible = true;
        }
    }

    public Vector3[] GetVolumeBox(float height, float offset = 0, bool forCulling = false)
    {
        Vector3[] verts = new Vector3[forCulling ? 14 : 8];

        Vector3 tl = topLeftCorner;
        Vector3 tr = topRightCorner;
        Vector3 bl = bottomLeftCorner;
        Vector3 br = bottomRightCorner;
        Vector3 mi = middleNormalized;

        verts[0] = tl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        verts[1] = tr.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        verts[2] = bl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        verts[3] = br.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);

        verts[4] = tl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        verts[5] = tr.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        verts[6] = bl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        verts[7] = br.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

        if (forCulling)
        {
            verts[8] = verts[0] - verts[4];
            verts[9] = verts[1] - verts[5];
            verts[10] = verts[2] - verts[6];
            verts[11] = verts[3] - verts[7];

            verts[12] = mi.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
            verts[13] = mi.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        }

        return verts;
    }

    public bool PlaneFrustumCheck(QuadAABB qaabb)
    {
        if (qaabb == null) { Log("QuadAABB problem!"); return true; }

        return PlaneFrustumCheck(qaabb.AABB);
    }

    public bool PlaneFrustumCheck(Vector3[] aabb)
    {
        if (Planetoid == null) { Log("Planetoid is null!"); return true; }
        if (aabb == null || aabb.Length == 0) { Log("AABB array problem!"); return true; }
        if (Planetoid.FrustumPlanes == null || Planetoid.FrustumPlanes.Length == 0) { Log("Frustum Planes Problem!"); return true; }
        if (Parent == null || !Generated || Splitting || Planetoid.CullingMethod == (QuadCullingMethod.Unity | QuadCullingMethod.None)) { return true; }

        bool[] states = new bool[aabb.Length];

        for (int i = 0; i < states.Length; i++)
        {
            states[i] = BorderFrustumCheck(Planetoid.FrustumPlanes, aabb[i]);
        }

        return states.Contains(true);
    }

    public bool BorderFrustumCheck(Plane[] planes, Vector3 border)
    {
        float offset = 1024.0f;

        bool useOffset = true;

        for (int i = 0; i < planes.Length; i++)
        {
            if (planes[i].GetDistanceToPoint(Planetoid.transform.TransformPoint(border)) < (useOffset ? 0 - offset : 0))
            {
                return false;
            }
        }

        return true;
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
                quad.SetupParent(this);
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
        foreach (Quad q in Subquads)
        {
            q.ReadyForDispatch = true;

            for (int wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
            {
                yield return Yielders.EndOfFrame;
            }
        }

        foreach (Quad q in Subquads)
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

        for (int i = 0; i < Subquads.Count; i++)
        {
            if (Subquads[i].HaveSubQuads)
            {
                Subquads[i].Unsplit();
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
        Unsplitted = true;
        Subquads.Clear();
    }

    public void Dispatch()
    {
        StartCoroutine(DispatcheCoroutineWait());
    }

    public void DispatchNow()
    {
        StartCoroutine(DispatchCoroutine());
    }

    public IEnumerator DispatcheCoroutineWait()
    {
        yield return StartCoroutine(DispatchCoroutine());
    }

    public IEnumerator DispatchCoroutine()
    {
        if (CoreShader == null) StopCoroutine(DispatchCoroutine());

        if (DispatchStarted != null)
            DispatchStarted(this);

        generationConstants.SplitLevel = LODLevel + 2;
        generationConstants.LODLevel = (((1 << LODLevel + 2) * (Planetoid.PlanetRadius / (LODLevel + 2)) - ((Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / Planetoid.PlanetRadius);
        generationConstants.LODOctaveModifier = Planetoid.GetLODOctaveModifier(LODLevel + 1);
        generationConstants.orientation = (float)Position;

        SetupComputeShaderUniforms();

        Cached = Planetoid.Cache.ExistInTexturesCache(this);

        if (Cached) Log("Textures founded in cache!"); else Log("Textures not found in cache!");

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
        }

        //Release and dispose unnecessary buffers. Video memory, you are free!
        BufferHelper.ReleaseAndDisposeBuffers(PreOutDataBuffer, PreOutDataSubBuffer, QuadCornersBuffer);
        BuffersCreated = false;

        if (DispatchReady != null)
            DispatchReady(this);
    }

    private bool AllSubquadsGenerated()
    {
        if (Subquads.Count != 0)
        {
            return Subquads.All(s => s.Generated == true);
        }
        else
            return false;
    }

    private void SetupComputeShaderUniforms()
    {
        if (Planetoid.tccps != null)
            Planetoid.tccps.UpdateUniforms(CoreShader);
    }

    private void SetupComputeShaderKernelUniforfms(int kernel, ComputeBuffer QuadGenerationConstantsBuffer, ComputeBuffer PreOutDataBuffer, ComputeBuffer PreOutDataSubBuffer, ComputeBuffer OutDataBuffer, ComputeBuffer QuadCornersBuffer)
    {
        if (CoreShader == null) return;

        CoreShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutputSub", PreOutDataSubBuffer);
        CoreShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);
        CoreShader.SetBuffer(kernel, "quadCorners", QuadCornersBuffer);

        CoreShader.SetTexture(kernel, "Height", HeightTexture);
        CoreShader.SetTexture(kernel, "Normal", NormalTexture);

        if (Planetoid.NPS != null)
            Planetoid.NPS.UpdateUniforms(QuadMaterial, CoreShader, kernel);
    }

    private void SetupComputeShaderKernelsUniforfms(ComputeBuffer QuadGenerationConstantsBuffer, ComputeBuffer PreOutDataBuffer, ComputeBuffer PreOutDataSubBuffer, ComputeBuffer OutDataBuffer, ComputeBuffer QuadCornersBuffer, params int[] kernels)
    {
        if (kernels == null || kernels.Length == 0) { Debug.Log("Quad.SetupComputeShaderKernelsUniforfms(...) problem!"); return; }

        for (int i = 0; i < kernels.Length; i++)
        {
            SetupComputeShaderKernelUniforfms(i, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer, QuadCornersBuffer);
        }
    }

    public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 cfed = Parent.generationConstants.cubeFaceEastDirection / 2;
        Vector3 cfnd = Parent.generationConstants.cubeFaceNorthDirection / 2;

        quad.generationConstants.cubeFaceEastDirection = cfed;
        quad.generationConstants.cubeFaceNorthDirection = cfnd;
        quad.generationConstants.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
    }

    public void SetupCorners(QuadPosition pos)
    {
        float v = Planetoid.PlanetRadius / 2;

        switch (pos)
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
        mesh.bounds = GetBounds(quad);//new Bounds(generationConstants.patchCubeCenter, GetBoundsSize(quad));
    }

    public float GetDistanceToLODSplit()
    {
        float distance = Mathf.Infinity;

        switch(Planetoid.LODDistanceMethod)
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
        if (QuadAABB.AABB == null || QuadAABB.AABB.Length == 0 || QuadAABB.AABB.Length <= 4) { Debug.Log("Quad.GetClosestAABBCorner(...) QuadAABB.AABB problem!"); return Mathf.Infinity; }

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
            if (d < closestDistance) { closestCorner = QuadAABB.AABB[i]; closestDistance = d; }
        }

        d = Vector3.Distance(Planetoid.LODTarget.position, Planetoid.transform.TransformPoint(middleNormalized));
        if (d < closestDistance) { closestCorner = middleNormalized; closestDistance = d; }

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

        if (Generated)
            return closestCorner;
        else
            return new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    }

    public Bounds GetBounds(Quad quad)
    {
        if (Planetoid.CullingMethod == QuadCullingMethod.Unity)
        {
            //TODO : Well calculated, but not axis aligned.
            //https://inovaestudios.blob.core.windows.net/forumsavatars/3255860d4f86c8c8a67cdb0b79e7e8889951cc54a65a.png

            if (QuadAABB != null)
                return QuadAABB.Bounds;

            return new Bounds(quad.generationConstants.patchCubeCenter, new Vector3(9e37f, 9e37f, 9e37f));
        }
        else
        {
            return new Bounds(quad.generationConstants.patchCubeCenter, new Vector3(9e37f, 9e37f, 9e37f));
        }
    }

    public Vector3 GetCubeFaceEastDirection(QuadPosition quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = Planetoid.PlanetRadius;

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
        }

        return temp;
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPosition quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = Planetoid.PlanetRadius;

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
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenter(QuadPosition quadPosition)
    {
        Vector3 temp = Vector3.zero;

        float r = Planetoid.PlanetRadius;

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
        }

        return temp;
    }

    public Vector3 GetPatchCubeCenterSplitted(QuadPosition quadPosition, int id, bool staticX, bool staticY, bool staticZ)
    {
        Vector3 temp = Vector3.zero;

        float mod = 0.5f;
        float v = Planetoid.PlanetRadius;
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

        middle = (topLeft + bottomRight) * (1 / Mathf.Abs(LODLevel));
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

    private void Log(string msg, bool state)
    {
        if (state)
            Debug.Log(msg);
    }
}