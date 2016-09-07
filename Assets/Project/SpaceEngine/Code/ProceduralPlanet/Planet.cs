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

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.AtmosphericScattering.Clouds;
using SpaceEngine.AtmosphericScattering.Sun;

public abstract class Planet : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public Cloudsphere Cloudsphere;
    public Ring Ring;

    [Tooltip("Render planet's quads?")]
    public bool PlanetQuadsEnabled = true;
    [Tooltip("Render planet's atmosphere?")]
    public bool AtmosphereEnabled = true;
    [Tooltip("Render planet's cloudsphere?")]
    public bool CloudsphereEnabled = true;
    [Tooltip("Render planet's ring?")]
    public bool RingEnabled = true;

    public int DrawLayer = 8;

    public bool DebugEnabled = false;

    public bool DrawNormals = false;
    public bool DrawGizmos = false;

    public bool GenerateColliders = false;
    public bool OctaveFade = false;
    public bool Working = false;
    public bool UseLOD = true;

    public Transform OriginTransform { get { return transform; } private set { } }

    public Vector3 Origin { get { return OriginTransform.position; } private set { } }
    public Vector3 OriginRotation { get { if (QuadsRoot != null) return QuadsRoot.transform.rotation.eulerAngles; else return OriginTransform.rotation.eulerAngles; } private set { } }
    public Vector3 OriginScale { get { return OriginTransform.localScale; } private set { } }

    public Matrix4x4 PlanetoidTRS = Matrix4x4.identity;

    public PlanetGenerationConstants GenerationConstants;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
    public int RenderQueueOffset = 0;

    public Transform LODTarget = null;

    public GameObject QuadsRoot = null;

    public float PlanetRadius = 1024;

    public float TerrainMaxHeight = 64.0f;
    public float DistanceToLODTarget = 0;

    public float LODDistanceMultiplier = 1;
    public float LODDistanceMultiplierPerLevel = 2;
    public int LODMaxLevel = 15;
    public float[] LODDistances = new float[16];
    public float[] LODOctaves = new float[6] { 0.5f, 0.5f, 0.5f, 0.75f, 0.75f, 1.0f };

    public bool OneSplittingQuad = true;
    public bool ExternalRendering = false;

    public SmartThreadPool stp = new SmartThreadPool();

    public QuadDistanceToClosestCornerComparer qdtccc;

    [HideInInspector]
    public Wireframe wireframeSwitcher;

    [HideInInspector]
    public Bounds PlanetBouds;

    protected virtual void Awake()
    {
        PlanetoidTRS = Matrix4x4.TRS(Origin, Quaternion.Euler(OriginRotation), OriginScale);

        PlanetBouds = new Bounds(Origin, Vector3.one * (PlanetRadius + TerrainMaxHeight) * 2);
    }

    protected virtual void Start()
    {
        stp.Start();

        if (qdtccc == null)
            qdtccc = new QuadDistanceToClosestCornerComparer();

        if (wireframeSwitcher == null)
            wireframeSwitcher = FindObjectOfType<Wireframe>();
    }

    protected virtual void Update()
    {
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
            if (x.DistanceToLODSplit > y.DistanceToLODSplit)
                return 1;
            else if (x.DistanceToLODSplit < y.DistanceToLODSplit)
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