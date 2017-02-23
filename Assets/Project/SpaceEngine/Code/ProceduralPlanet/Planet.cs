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
// Creation Date: 2016.05.18
// Creation Time: 19:26
// Creator: zameran
#endregion

using SpaceEngine.AtmosphericScattering;
using SpaceEngine.AtmosphericScattering.Cloudsphere;
using SpaceEngine.Ocean;

using System.Collections.Generic;

using UnityEngine;

public abstract class Planet : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public OceanNode Ocean;
    public Cloudsphere Cloudsphere;
    public Ring Ring;

    public List<Shadow> Shadows = new List<Shadow>();

    public bool PlanetQuadsEnabled = true;
    public bool AtmosphereEnabled = true;
    public bool CloudsphereEnabled = true;
    public bool RingEnabled = true;

    public int DrawLayer = 8;

    public bool DrawNormals = false;
    public bool DrawQuadTree = false;
    public bool DrawGizmos = false;

    public Transform OriginTransform { get { return transform; } }
    public Vector3 Origin { get { return OriginTransform.position; } }
    public Vector3 OriginRotation { get { if (QuadsRoot != null) return QuadsRoot.transform.rotation.eulerAngles; return OriginTransform.rotation.eulerAngles; } }
    public Vector3 OriginScale { get { return OriginTransform.localScale; } }
    public Matrix4x4 PlanetoidTRS { get; set; }

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Geometry;
    public int RenderQueueOffset = 0;

    public GameObject QuadsRoot = null;

    public float PlanetRadius = 1024;
    public float TerrainMaxHeight = 64.0f;

    public bool OneSplittingQuad { get; set; }

    public Bounds PlanetBounds { get; set; }

    protected virtual void Awake()
    {
        PlanetoidTRS = Matrix4x4.TRS(Origin, Quaternion.Euler(OriginRotation), OriginScale);

        PlanetBounds = new Bounds(Origin, Ring == null ? (Vector3.one * (PlanetRadius + TerrainMaxHeight) * 2) : (Vector3.one * Ring.OuterRadius * 2));
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        PlanetoidTRS = Matrix4x4.TRS(Origin, Quaternion.Euler(OriginRotation), OriginScale);

        PlanetBounds = new Bounds(Origin, Ring == null ? (Vector3.one * (PlanetRadius + TerrainMaxHeight) * 2) : (Vector3.one * Ring.OuterRadius * 2));
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

    #region Gizmos

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(PlanetBounds.center, PlanetBounds.size);
        }
    }
#endif

    #endregion
}