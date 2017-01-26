#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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

public sealed class Quad : Node<Quad>, IQuad
{
    //NOTE : Do not TransformPoint the points on wich bounds will depend on.

    [Serializable]
    public class Id
    {
        public int LODLevel;
        public byte ID;
        public int Position;

        public Id(int LODLevel, byte ID, int Position)
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
            return string.Format("({0}, {1}, {2})", LODLevel, ID, Position);
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

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadGenerationConstants generationConstants;

    public Quad Parent;

    public List<Quad> Subquads = new List<Quad>(4);

    public int LODLevel = -1;

    public bool HaveSubQuads = false;
    public bool Generated = false;
    public bool ShouldDraw = false;
    public bool ReadyForDispatch = false;
    public bool Splitting = false;
    public bool Unsplitted = false;
    public bool Visible = false;
    public bool Uniformed = false;
    public bool BuffersCreated = false;

    public float DistanceToLODSplit = Mathf.Infinity;

    public Vector3 middleNormalized;

    public QuadCorners quadCorners;

    public QuadAABB QuadAABB = null;

    public Id RegistryID { get { return new Id(LODLevel, (byte)ID, (int)Position); } }

    public Matrix4x4 RotationMatrix { get { return Matrix4x4.TRS(middleNormalized, Quaternion.Euler(middleNormalized.normalized * Mathf.Deg2Rad), Vector3.one); } }

    #region Node

    protected override void InitNode()
    {

    }

    protected override void UpdateNode()
    {

    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    #endregion

    private void Awake()
    {
        CreateBuffers();

        HeightTexture = RTExtensions.CreateRTexture(QuadSettings.VerticesPerSideFull, 0, RenderTextureFormat.ARGB32);
        NormalTexture = RTExtensions.CreateRTexture(QuadSettings.VerticesPerSideFull, 0, RenderTextureFormat.ARGB32);

        RTUtility.ClearColor(new RenderTexture[] { HeightTexture, NormalTexture });
    }

    private void OnDestroy()
    {
        BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer);

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
            var r = Planetoid.PlanetRadius / 1000.0f;
            var bounds = GetBounds(this);

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(bounds.center, bounds.size);

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.topLeftCorner), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.topRightCorner), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.bottomLeftCorner), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.bottomRightCorner), r);

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius)), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius)), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius)), r);
            Gizmos.DrawWireSphere(Planetoid.OriginTransform.TransformPoint(quadCorners.bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius)), r);

            if (QuadAABB != null)
            {
                Gizmos.color = XKCDColors.Adobe;

                Gizmos.DrawWireCube(Planetoid.OriginTransform.TransformPoint(QuadAABB.Bounds.center), QuadAABB.Bounds.size);
            }

            Gizmos.color = XKCDColors.BabyBlue;

            Gizmos.DrawRay(Planetoid.OriginTransform.TransformPoint(middleNormalized), middleNormalized);
        }
        else
        {
            Gizmos.color = XKCDColors.Red;

            Gizmos.DrawRay(Planetoid.OriginTransform.TransformPoint(middleNormalized), generationConstants.cubeFaceEastDirection);

            Gizmos.color = XKCDColors.Green;

            Gizmos.DrawRay(Planetoid.OriginTransform.TransformPoint(middleNormalized), generationConstants.patchCubeCenter);

            Gizmos.color = XKCDColors.Blue;

            Gizmos.DrawRay(Planetoid.OriginTransform.TransformPoint(middleNormalized), generationConstants.cubeFaceNorthDirection);
        }
    }

    private void CreateBuffers()
    {
        if (!BuffersCreated)
        {
            QuadGenerationConstantsBuffer = new ComputeBuffer(1, 84);
            PreOutDataBuffer = new ComputeBuffer(QuadSettings.VerticesWithBorder, 48);
            PreOutDataSubBuffer = new ComputeBuffer(QuadSettings.VerticesWithBorderFull, 48);
            OutDataBuffer = new ComputeBuffer(QuadSettings.Vertices, 48);

            BuffersCreated = true;
        }
    }

    public void CheckLOD()
    {
        if (QuadAABB == null) return;

        DistanceToLODSplit = GetDistanceToLODSplit() + Planetoid.TerrainMaxHeight;

        if (LODLevel < Planetoid.LODMaxLevel)
        {
            var LODDistance = Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier;

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

    public Bounds GetBoundFromPoints(Vector3d[] points, out Vector3d max, out Vector3d min)
    {
        var center = points.Aggregate(Vector3d.zero, (current, t) => current + t) / 8;

        min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
        max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);

        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];

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
            QuadAABB = GetVolumeBox(Planetoid.TerrainMaxHeight, 0);
        }

        // TODO : Setup bounds only once...
        QuadMesh.bounds = GetBounds(this);

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
            QuadMaterial.SetMatrix("_TRS", RotationMatrix);
            QuadMaterial.SetFloat("_LODLevel", LODLevel + 2);

            Uniformed = true;
        }

        QuadMaterial.renderQueue = (int)Planetoid.RenderQueue + Planetoid.RenderQueueOffset;

        if (Generated && ShouldDraw && QuadMesh != null)
        {
            TryCull();

            if (Visible)
                Graphics.DrawMesh(QuadMesh, Planetoid.PlanetoidTRS, QuadMaterial, drawLayer, camera, 0, Planetoid.QuadMPB, true, true);
        }
    }

    private QuadAABB GetVolumeBox(float height, float offset = 0)
    {
        var points = new Vector3d[8];
        var cullingPoints = new Vector3d[14];

        points[0] = quadCorners.topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[1] = quadCorners.topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[2] = quadCorners.bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        points[3] = quadCorners.bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);

        points[4] = quadCorners.topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[5] = quadCorners.topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[6] = quadCorners.bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
        points[7] = quadCorners.bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

        Array.Copy(points, cullingPoints, 8);

        cullingPoints[8] = points[0] - points[4];
        cullingPoints[9] = points[1] - points[5];
        cullingPoints[10] = points[2] - points[6];
        cullingPoints[11] = points[3] - points[7];

        cullingPoints[12] = middleNormalized.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
        cullingPoints[13] = middleNormalized.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

        return new QuadAABB(points, cullingPoints, this, Planetoid.OriginTransform);
    }

    private void TryCull()
    {
        if (GodManager.Instance.UpdateFrustumPlanesNow == false) return;

        if (Planetoid.CullingMethod == QuadCullingMethod.Custom)
            Visible = PlaneFrustumCheck(QuadAABB);
        else
            Visible = true;
    }

    private bool PlaneFrustumCheck(QuadAABB qaabb)
    {
        for (byte i = 0; i < qaabb.CullingAABB.Length; i++)
        {
            if (BorderFrustumCheck(GodManager.Instance.FrustumPlanesTS, qaabb.CullingAABB[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool BorderFrustumCheck(FrustumPlane[] planes, Vector3 border)
    {
        for (byte i = 0; i < planes.Length; i++)
        {
            if (planes[i].GetDistanceToPoint(border) < 0 - 1024.0f)
            {
                return false;
            }
        }

        return true;
    }

    private void InitCorners(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
    {
        quadCorners.topLeftCorner = topLeft;
        quadCorners.bottomRightCorner = bottmoRight;
        quadCorners.topRightCorner = topRight;
        quadCorners.bottomLeftCorner = bottomLeft;

        middleNormalized = CalculateMiddlePoint();
    }

    private IEnumerator Split()
    {
        var id = 0;

        var subTopLeft = Vector3.zero;
        var subBottomRight = Vector3.zero;
        var subTopRight = Vector3.zero;
        var subBottomLeft = Vector3.zero;

        var size = quadCorners.bottomRightCorner - quadCorners.topLeftCorner;
        var step = size / 2.0f;

        bool staticX = false, staticY = false, staticZ = false;

        BrainFuckMath.DefineAxis(ref staticX, ref staticY, ref staticZ, size);

        Planetoid.Working = true;
        HaveSubQuads = true;
        Splitting = true;
        Unsplitted = false;

        for (byte sY = 0; sY < 2; sY++)
        {
            for (byte sX = 0; sX < 2; sX++, id++)
            {
                if (staticX)
                {
                    subTopLeft = new Vector3(quadCorners.topLeftCorner.x, quadCorners.topLeftCorner.y + step.y * sY, quadCorners.topLeftCorner.z + step.z * sX);
                    subBottomRight = new Vector3(quadCorners.topLeftCorner.x, quadCorners.topLeftCorner.y + step.y * (sY + 1), quadCorners.topLeftCorner.z + step.z * (sX + 1));

                    subTopRight = new Vector3(quadCorners.topLeftCorner.x, quadCorners.topLeftCorner.y + step.y * sY, quadCorners.topLeftCorner.z + step.z * (sX + 1));
                    subBottomLeft = new Vector3(quadCorners.topLeftCorner.x, quadCorners.topLeftCorner.y + step.y * (sY + 1), quadCorners.topLeftCorner.z + step.z * sX);
                }
                else if (staticY)
                {
                    subTopLeft = new Vector3(quadCorners.topLeftCorner.x + step.x * sX, quadCorners.topLeftCorner.y, quadCorners.topLeftCorner.z + step.z * sY);
                    subBottomRight = new Vector3(quadCorners.topLeftCorner.x + step.x * (sX + 1), quadCorners.topLeftCorner.y, quadCorners.topLeftCorner.z + step.z * (sY + 1));

                    subTopRight = new Vector3(quadCorners.topLeftCorner.x + step.x * (sX + 1), quadCorners.topLeftCorner.y, quadCorners.topLeftCorner.z + step.z * sY);
                    subBottomLeft = new Vector3(quadCorners.topLeftCorner.x + step.x * sX, quadCorners.topLeftCorner.y, quadCorners.topLeftCorner.z + step.z * (sY + 1));
                }
                else if (staticZ)
                {
                    subTopLeft = new Vector3(quadCorners.topLeftCorner.x + step.x * sX, quadCorners.topLeftCorner.y + step.y * sY, quadCorners.topLeftCorner.z);
                    subBottomRight = new Vector3(quadCorners.topLeftCorner.x + step.x * (sX + 1), quadCorners.topLeftCorner.y + step.y * (sY + 1), quadCorners.topLeftCorner.z);

                    subTopRight = new Vector3(quadCorners.topLeftCorner.x + step.x * (sX + 1), quadCorners.topLeftCorner.y + step.y * sY, quadCorners.topLeftCorner.z);
                    subBottomLeft = new Vector3(quadCorners.topLeftCorner.x + step.x * sX, quadCorners.topLeftCorner.y + step.y * (sY + 1), quadCorners.topLeftCorner.z);
                }

                var quad = Planetoid.SetupSubQuad(Position);
                quad.Splitting = true;
                quad.ShouldDraw = false;
                quad.InitCorners(subTopLeft, subBottomRight, subTopRight, subBottomLeft);
                quad.Parent = this;
                quad.LODLevel = quad.Parent.LODLevel + 1;
                quad.ID = (QuadID)id;
                quad.SetupVectors(quad, id, staticX, staticY, staticZ);

                if (quad.Parent.transform != null)
                    quad.transform.parent = quad.Parent.transform;

                quad.gameObject.name = string.Format("{0}_ID{1}_LOD{2}", quad.gameObject.name, id, quad.LODLevel);

                Subquads.Add(quad);

                for (var wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
                {
                    yield return Yielders.EndOfFrame;
                }
            }
        }

        //Dispatch one by one with intervals.
        for (byte i = 0; i < Subquads.Count; i++)
        {
            Subquads[i].ReadyForDispatch = true;

            for (var wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
            {
                yield return Yielders.EndOfFrame;
            }
        }

        for (byte i = 0; i < Subquads.Count; i++)
        {
            Subquads[i].Splitting = false;
            Subquads[i].ShouldDraw = true;
        }

        ShouldDraw = false;
        Splitting = false;

        Planetoid.Working = false;
    }

    private void Unsplit()
    {
        if (Unsplitted) return;

        StopAllCoroutines();

        for (byte i = 0; i < Subquads.Count; i++)
        {
            var subQuad = Subquads[i];

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
        generationConstants.lodLevel = (((1 << LODLevel + 2) * (Planetoid.PlanetRadius / (LODLevel + 2)) - ((Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / Planetoid.PlanetRadius);
        generationConstants.lodOctaveModifier = Planetoid.GetLODOctaveModifier(LODLevel + 1);

        SetupComputeShaderUniforms();

        CreateBuffers();

        QuadGenerationConstantsBuffer.SetData(new[] { generationConstants });
        PreOutDataBuffer.SetData(GodManager.Instance.PreOutputDataBuffer);
        PreOutDataSubBuffer.SetData(GodManager.Instance.PreOutputSubDataBuffer);
        OutDataBuffer.SetData(GodManager.Instance.OutputDataBuffer);

        if (CoreShader == null) return;

        EventManager.PlanetoidEvents.OnDispatchStarted.Invoke(Planetoid, this);

        int kernel1 = CoreShader.FindKernel("HeightMain");
        int kernel2 = CoreShader.FindKernel("Transfer");
        int kernel3 = CoreShader.FindKernel("HeightSub");
        int kernel4 = CoreShader.FindKernel("TexturesSub");

        SetupComputeShaderKernelsUniforfms(QuadGenerationConstantsBuffer,
                                           PreOutDataBuffer,
                                           PreOutDataSubBuffer,
                                           OutDataBuffer, new int[] { kernel1, kernel2, kernel3, kernel4 });

        CoreShader.Dispatch(kernel1, QuadSettings.THREADGROUP_SIZE_BORDER, QuadSettings.THREADGROUP_SIZE_BORDER, 1);
        CoreShader.Dispatch(kernel2, QuadSettings.THREADGROUP_SIZE, QuadSettings.THREADGROUP_SIZE, 1);
        CoreShader.Dispatch(kernel3, QuadSettings.THREADGROUP_SIZE_BORDER_FULL, QuadSettings.THREADGROUP_SIZE_BORDER_FULL, 1);
        CoreShader.Dispatch(kernel4, QuadSettings.THREADGROUP_SIZE_FULL, QuadSettings.THREADGROUP_SIZE_FULL, 1);

        Generated = true;

        // NOTE : NO DATA WILL BE RECIEVED UNTIL ASYNC GET DATA!

        //Release and dispose unnecessary buffers. Video memory, you are free!
        BufferHelper.ReleaseAndDisposeBuffers(PreOutDataBuffer, PreOutDataSubBuffer);

        BuffersCreated = false;

        EventManager.PlanetoidEvents.OnDispatchEnd.Invoke(Planetoid, this);
        EventManager.PlanetoidEvents.OnDispatchFinished.Invoke(Planetoid, this);
    }

    private void SetupComputeShaderUniforms()
    {
        if (Planetoid.tccps != null)
            Planetoid.tccps.UpdateUniforms(CoreShader);
    }

    private void SetupComputeShaderKernelUniforfms(int kernel, ComputeBuffer quadGenerationConstantsBuffer, ComputeBuffer preOutDataBuffer, ComputeBuffer preOutDataSubBuffer, ComputeBuffer outDataBuffer)
    {
        if (CoreShader == null) return;

        CoreShader.SetBuffer(kernel, "quadGenerationConstants", quadGenerationConstantsBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutput", preOutDataBuffer);
        CoreShader.SetBuffer(kernel, "patchPreOutputSub", preOutDataSubBuffer);
        CoreShader.SetBuffer(kernel, "patchOutput", outDataBuffer);

        CoreShader.SetTexture(kernel, "Height", HeightTexture);
        CoreShader.SetTexture(kernel, "Normal", NormalTexture);

        if (Planetoid.NPS != null)
            Planetoid.NPS.UpdateUniforms(QuadMaterial, CoreShader, kernel);
    }

    private void SetupComputeShaderKernelsUniforfms(ComputeBuffer quadGenerationConstantsBuffer, ComputeBuffer preOutDataBuffer, ComputeBuffer preOutDataSubBuffer, ComputeBuffer outDataBuffer, params int[] kernels)
    {
        if (kernels == null || kernels.Length == 0) { Debug.Log("Quad.SetupComputeShaderKernelsUniforfms(...) problem!"); return; }

        for (int i = 0; i < kernels.Length; i++)
        {
            SetupComputeShaderKernelUniforfms(i, quadGenerationConstantsBuffer, preOutDataBuffer, preOutDataSubBuffer, outDataBuffer);
        }
    }

    private void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
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
                quadCorners.topLeftCorner = new Vector3(-v, v, v);
                quadCorners.bottomRightCorner = new Vector3(v, v, -v);

                quadCorners.topRightCorner = new Vector3(v, v, v);
                quadCorners.bottomLeftCorner = new Vector3(-v, v, -v);
                break;
            case QuadPosition.Bottom:
                quadCorners.topLeftCorner = new Vector3(-v, -v, -v);
                quadCorners.bottomRightCorner = new Vector3(v, -v, v);

                quadCorners.topRightCorner = new Vector3(v, -v, -v);
                quadCorners.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPosition.Left:
                quadCorners.topLeftCorner = new Vector3(-v, v, v);
                quadCorners.bottomRightCorner = new Vector3(-v, -v, -v);

                quadCorners.topRightCorner = new Vector3(-v, v, -v);
                quadCorners.bottomLeftCorner = new Vector3(-v, -v, v);
                break;
            case QuadPosition.Right:
                quadCorners.topLeftCorner = new Vector3(v, v, -v);
                quadCorners.bottomRightCorner = new Vector3(v, -v, v);

                quadCorners.topRightCorner = new Vector3(v, v, v);
                quadCorners.bottomLeftCorner = new Vector3(v, -v, -v);
                break;
            case QuadPosition.Front:
                quadCorners.topLeftCorner = new Vector3(v, v, v);
                quadCorners.bottomRightCorner = new Vector3(-v, -v, v);

                quadCorners.topRightCorner = new Vector3(-v, v, v);
                quadCorners.bottomLeftCorner = new Vector3(v, -v, v);
                break;
            case QuadPosition.Back:
                quadCorners.topLeftCorner = new Vector3(-v, v, -v);
                quadCorners.bottomRightCorner = new Vector3(v, -v, -v);

                quadCorners.topRightCorner = new Vector3(v, v, -v);
                quadCorners.bottomLeftCorner = new Vector3(-v, -v, -v);
                break;
            default:
                throw new ArgumentOutOfRangeException("quadPosition", quadPosition, null);
        }

        middleNormalized = CalculateMiddlePoint();
    }

    private float GetDistanceToLODSplit()
    {
        var distance = Mathf.Infinity;

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
        var closestDistance = Mathf.Infinity;
        var distance = Mathf.Infinity;

        var closestCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        for (byte i = 0; i < 4; i++)
        {
            distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, Planetoid.OriginTransform.TransformPoint(QuadAABB.AABB[i]));

            if (distance < closestDistance)
            {
                closestCorner = QuadAABB.AABB[i];
                closestDistance = distance;
            }
        }

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, Planetoid.OriginTransform.TransformPoint(middleNormalized));

        if (distance < closestDistance)
        {
            closestCorner = middleNormalized;
            closestDistance = distance;
        }

        return Planetoid.OriginTransform.TransformPoint(closestCorner);
    }

    private Vector3 GetClosestCorner()
    {
        var closestDistance = Mathf.Infinity;
        var distance = Mathf.Infinity;

        var closestCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        var tl = Planetoid.OriginTransform.TransformPoint(quadCorners.topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
        var tr = Planetoid.OriginTransform.TransformPoint(quadCorners.topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));
        var bl = Planetoid.OriginTransform.TransformPoint(quadCorners.bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
        var br = Planetoid.OriginTransform.TransformPoint(quadCorners.bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));
        var middlePoint = Planetoid.OriginTransform.TransformPoint(middleNormalized);

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, tl);

        if (distance < closestDistance)
        {
            closestCorner = tl;
            closestDistance = distance;
        }

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, tr);

        if (distance < closestDistance)
        {
            closestCorner = tr;
            closestDistance = distance;
        }

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, middlePoint);

        if (distance < closestDistance)
        {
            closestCorner = middlePoint;
            closestDistance = distance;
        }

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, bl);

        if (distance < closestDistance)
        {
            closestCorner = bl;
            closestDistance = distance;
        }

        distance = VectorHelper.QuickDistance(Planetoid.LODTarget.position, br);

        if (distance < closestDistance)
        {
            closestCorner = br;
            closestDistance = distance;
        }

        return Generated ? closestCorner : new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    }

    private Bounds GetBounds(Quad quad)
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
        // NOTE : So, here i will construct vector with specific parameters.
        // I need unit axis vector, depending on Quad Orientation [QuadPosition] with positive or negative value [Planet Radius].
        // "Sign" will represent 'Is value negative or positive?'
        // "Axis" will represent one component of vector, which should be 'valued' [X or Y or Z]. Other vector components will be zero...
        // TOP      (0.0, 0.0, -r)      SIGN     1      AXIS    2   Z
        // BUTTOM   (0.0, 0.0, -r)      SIGN     1      AXIS    2   Z
        // LEFT     (0.0, -r, 0.0)      SIGN     1      AXIS    1   Y
        // RIGHT    (0.0, -r, 0.0)      SIGN     1      AXIS    1   Y
        // FRONT    (r, 0.0, 0.0)       SIGN     0      AXIS    0   X
        // BACK     (r, 0.0, 0.0)       SIGN     0      AXIS    0   X

        var sign = new byte[] { 1, 1, 1, 1, 0, 0 };
        var axis = new byte[] { 2, 2, 1, 1, 0, 0 };

        return BrainFuckMath.FromQuadPositionMask(Planetoid.PlanetRadius, sign, axis, quadPosition);
    }

    public Vector3 GetCubeFaceNorthDirection(QuadPosition quadPosition)
    {
        // NOTE : So, here i will construct vector with specific parameters.
        // I need unit axis vector, depending on Quad Orientation [QuadPosition] with positive or negative value [Planet Radius].
        // "Sign" will represent 'Is value negative or positive?'
        // "Axis" will represent one component of vector, which should be 'valued' [X or Y or Z]. Other vector components will be zero...
        // TOP      (r, 0.0, 0.0)       SIGN     0      AXIS    0   X
        // BUTTOM   (-r, 0.0, 0.0)      SIGN     1      AXIS    0   X
        // LEFT     (0.0, 0.0, -r)      SIGN     1      AXIS    2   Z
        // RIGHT    (0.0, 0.0, r)       SIGN     0      AXIS    2   Z
        // FRONT    (0.0, -r, 0.0)      SIGN     1      AXIS    1   Y
        // BACK     (0.0, r, 0.0)       SIGN     0      AXIS    1   Y

        var sign = new byte[] { 0, 1, 1, 0, 1, 0 };
        var axis = new byte[] { 0, 0, 2, 2, 1, 1 };

        return BrainFuckMath.FromQuadPositionMask(Planetoid.PlanetRadius, sign, axis, quadPosition);
    }

    public Vector3 GetPatchCubeCenter(QuadPosition quadPosition)
    {
        // NOTE : So, here i will construct vector with specific parameters.
        // I need unit axis vector, depending on Quad Orientation [QuadPosition] with positive or negative value [Planet Radius].
        // "Sign" will represent 'Is value negative or positive?'
        // "Axis" will represent one component of vector, which should be 'valued' [X or Y or Z]. Other vector components will be zero...
        // TOP      (0.0, r, 0.0)       SIGN     0      AXIS    0   Y
        // BUTTOM   (0.0, -r, 0.0)      SIGN     1      AXIS    0   Y
        // LEFT     (-r, 0.0, 0.0)      SIGN     1      AXIS    2   X
        // RIGHT    (r, 0.0, 0.0)       SIGN     0      AXIS    2   X
        // FRONT    (0.0, 0.0, r)       SIGN     0      AXIS    1   Z
        // BACK     (0.0, 0,0, -r)      SIGN     1      AXIS    1   Z

        var sign = new byte[] { 0, 1, 1, 0, 0, 1 };
        var axis = new byte[] { 1, 1, 0, 0, 2, 2 };

        return BrainFuckMath.FromQuadPositionMask(Planetoid.PlanetRadius, sign, axis, quadPosition);
    }

    private Vector3 GetPatchCubeCenterSplitted(QuadPosition quadPosition, int id, bool staticX, bool staticY, bool staticZ)
    {
        var temp = Vector3.zero;

        var r = Planetoid.PlanetRadius;
        var v = Planetoid.PlanetRadius / 2;

        var tempStatic = 0.0f;

        // TOP      (-v, r, v)  :0    SIGN    100     AXIS 010
        // TOP      (v, r, v)   :1    SIGN    000     AXIS 010
        // TOP      (-v, r, -v) :2    SIGN    101     AXIS 010
        // TOP      (v, r, -v)  :3    SIGN    001     AXIS 010
        // ---------------------------------------------------
        // BUTTOM   (-v, -r, -v):0    SIGN    111     AXIS 010
        // BUTTOM   (v, -r, -v) :1    SIGN    011     AXIS 010
        // BUTTOM   (-v, -r, v) :2    SIGN    110     AXIS 010
        // BUTTOM   (v, -r, v)  :3    SIGN    010     AXIS 010
        // ---------------------------------------------------
        // LEFT     (-r, v, v)  :0    SIGN    100     AXIS 100
        // LEFT     (-r, v, -v) :1    SIGN    101     AXIS 100
        // LEFT     (-r, -v, v) :2    SIGN    110     AXIS 100
        // LEFT     (-r, -v, -v):3    SIGN    111     AXIS 100
        // ---------------------------------------------------
        // RIGHT    (r, v, -v)  :0    SIGN    001     AXIS 100
        // RIGHT    (r, v, v)   :1    SIGN    000     AXIS 100
        // RIGHT    (r, -v, -v) :2    SIGN    011     AXIS 100
        // RIGHT    (r, -v, v)  :3    SIGN    010     AXIS 100
        // ---------------------------------------------------
        // FRONT    (v, v, r)   :0    SIGN    000     AXIS 001
        // FRONT    (-v, v, r)  :1    SIGN    100     AXIS 001
        // FRONT    (v, -v, r)  :2    SIGN    010     AXIS 001
        // FRONT    (-v, -v, r) :3    SIGN    110     AXIS 001
        // ---------------------------------------------------
        // BACK     (-v, v, -r) :0    SIGN    101     AXIS 001
        // BACK     (v, v, -r)  :1    SIGN    001     AXIS 001
        // BACK     (-v, -v, -r):2    SIGN    111     AXIS 001
        // BACK     (v, -v, -r) :3    SIGN    011     AXIS 001

        switch (quadPosition)
        {
            case QuadPosition.Top:
                if (id == 0)
                    temp += new Vector3(-v, r, v);
                else if (id == 1)
                    temp += new Vector3(v, r, v);
                else if (id == 2)
                    temp += new Vector3(-v, r, -v);
                else if (id == 3)
                    temp += new Vector3(v, r, -v);
                break;
            case QuadPosition.Bottom:
                if (id == 0)
                    temp += new Vector3(-v, -r, -v);
                else if (id == 1)
                    temp += new Vector3(v, -r, -v);
                else if (id == 2)
                    temp += new Vector3(-v, -r, v);
                else if (id == 3)
                    temp += new Vector3(v, -r, v);
                break;
            case QuadPosition.Left:
                if (id == 0)
                    temp += new Vector3(-r, v, v);
                else if (id == 1)
                    temp += new Vector3(-r, v, -v);
                else if (id == 2)
                    temp += new Vector3(-r, -v, v);
                else if (id == 3)
                    temp += new Vector3(-r, -v, -v);
                break;
            case QuadPosition.Right:
                if (id == 0)
                    temp += new Vector3(r, v, -v);
                else if (id == 1)
                    temp += new Vector3(r, v, v);
                else if (id == 2)
                    temp += new Vector3(r, -v, -v);
                else if (id == 3)
                    temp += new Vector3(r, -v, v);
                break;
            case QuadPosition.Front:
                if (id == 0)
                    temp += new Vector3(v, v, r);
                else if (id == 1)
                    temp += new Vector3(-v, v, r);
                else if (id == 2)
                    temp += new Vector3(v, -v, r);
                else if (id == 3)
                    temp += new Vector3(-v, -v, r);
                break;
            case QuadPosition.Back:
                if (id == 0)
                    temp += new Vector3(-v, v, -r);
                else if (id == 1)
                    temp += new Vector3(v, v, -r);
                else if (id == 2)
                    temp += new Vector3(-v, -v, -r);
                else if (id == 3)
                    temp += new Vector3(v, -v, -r);
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

    private Vector3 CalculateMiddlePoint()
    {
        return ((quadCorners.topLeftCorner + quadCorners.bottomRightCorner) * (1.0f / Mathf.Abs(LODLevel))).NormalizeToRadius(Planetoid.PlanetRadius);
    }

    private void Log(string msg)
    {
        if (Planetoid.DebugEnabled)
            Debug.Log(msg);
    }
}