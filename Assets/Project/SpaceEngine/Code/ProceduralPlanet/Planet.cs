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
// Creation Date: 2016.05.18
// Creation Time: 19:26
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

using Amib.Threading;

using UnityEngine;

public abstract class Planet : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public Cloudsphere Cloudsphere;

    public int DrawLayer = 8;

    public bool DebugEnabled = false;

    public bool DrawNormals = false;
    public bool DrawGizmos = false;

    public bool GenerateColliders = false;
    public bool OctaveFade = false;
    public bool Working = false;
    public bool UseLOD = true;

    public Vector3 Origin = Vector3.zero;
    public Vector3 OriginRotation = Vector3.zero;
    public Vector3 OriginScale = Vector3.one;

    public Matrix4x4 PlanetoidTRS = Matrix4x4.identity;

    public PlanetGenerationConstants GenerationConstants;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
    public int RenderQueueOffset = 0;

    public Transform LODTarget = null;

    public float LODUpdateInterval = 0.25f;
    [HideInInspector]
    public float LastLODUpdateTime = 0.00f;

    public GameObject QuadsRoot = null;

    public float PlanetRadius = 1024;

    public float TerrainMaxHeight = 64.0f;
    public float DistanceToLODTarget = 0;

    public float LODDistanceMultiplier = 1;

    public int LODMaxLevel = 12;
    public int[] LODDistances = new int[13] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1, 0 };
    public float[] LODOctaves = new float[6] { 0.5f, 0.5f, 0.5f, 0.75f, 0.75f, 1.0f };

    public bool OneSplittingQuad = true;
    public bool ExternalRendering = false;

    public SmartThreadPool stp = new SmartThreadPool();

    public QuadDistanceToClosestCornerComparer qdtccc;
    public PlanetoidDistanceToLODTargetComparer pdtltc;

    [HideInInspector]
    public Wireframe wireframeSwitcher;

    [HideInInspector]
    public Bounds PlanetBouds;

    protected virtual void Awake()
    {
        Origin = transform.position;
        OriginRotation = QuadsRoot.transform.rotation.eulerAngles;
        OriginScale = transform.localScale;

        PlanetoidTRS = Matrix4x4.TRS(Origin, Quaternion.Euler(OriginRotation), OriginScale);

        PlanetBouds = new Bounds(Origin, Vector3.one * (PlanetRadius + TerrainMaxHeight) * 2);
    }

    protected virtual void Start()
    {
        stp.Start();

        if (qdtccc == null)
            qdtccc = new QuadDistanceToClosestCornerComparer();

        if (pdtltc == null)
            pdtltc = new PlanetoidDistanceToLODTargetComparer();

        if (wireframeSwitcher == null)
            wireframeSwitcher = FindObjectOfType<Wireframe>();
    }

    protected virtual void Update()
    {
        Origin = transform.position;
        OriginRotation = QuadsRoot.transform.rotation.eulerAngles;
        OriginScale = transform.localScale;

        PlanetoidTRS = Matrix4x4.TRS(Origin, Quaternion.Euler(OriginRotation), OriginScale);
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    protected virtual void OnRenderObject()
    {

    }

    protected virtual void OnApplicationFocus(bool focusStatus)
    {

    }

    protected virtual void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(PlanetBouds.center, PlanetBouds.size);
        }
    }

    public sealed class QuadDistanceToClosestCornerComparer : IComparer<Quad>
    {
        public int Compare(Quad x, Quad y)
        {
            if (x.DistanceToClosestCorner > y.DistanceToClosestCorner)
                return 1;
            else if (x.DistanceToClosestCorner < y.DistanceToClosestCorner)
                return -1;
            else
                return 0;
        }
    }

    public sealed class PlanetoidDistanceToLODTargetComparer : IComparer<Planetoid>
    {
        public int Compare(Planetoid x, Planetoid y)
        {
            if (x.DistanceToLODTarget > y.DistanceToLODTarget)
                return 1;
            else if (x.DistanceToLODTarget < y.DistanceToLODTarget)
                return -1;
            else
                return 0;
        }
    }
}