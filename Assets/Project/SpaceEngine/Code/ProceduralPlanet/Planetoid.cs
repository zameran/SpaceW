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
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class PlanetoidExtensions
{
    public static List<string> GetKeywords(this Planet planet)
    {
        var Keywords = new List<string>();

        if (planet != null)
        {
            if (planet.Ring != null)
            {
                Keywords.Add(planet.RingEnabled ? "RING_ON" : "RING_OFF");
                if (planet.RingEnabled) Keywords.Add("SCATTERING");

                var shadowsCount = planet.Shadows.Count((shadow) => shadow != null);

                for (byte i = 1; i < shadowsCount + 1; i++)
                {
                    Keywords.Add("SHADOW_" + i);
                }
            }
            else
            {
                Keywords.Add("RING_OFF");
            }

            if (planet.Atmosphere != null)
            {
                if (planet.AtmosphereEnabled)
                {
                    var lightCount = planet.Atmosphere.Suns.Count((sun) => sun != null && sun.gameObject.activeInHierarchy);

                    if (lightCount != 0)
                        Keywords.Add("LIGHT_" + lightCount);

                    if (planet.Atmosphere.EclipseCasters.Count == 0)
                    {
                        Keywords.Add("ECLIPSES_OFF");
                    }
                    else
                    {
                        Keywords.Add(planet.Atmosphere.Eclipses ? "ECLIPSES_ON" : "ECLIPSES_OFF");
                    }

                    if (planet.Atmosphere.ShineCasters.Count == 0)
                    {
                        Keywords.Add("SHINE_OFF");
                    }
                    else
                    {
                        Keywords.Add(planet.Atmosphere.Planetshine ? "SHINE_ON" : "SHINE_OFF");
                    }

                    Keywords.Add("ATMOSPHERE_ON");
                }
                else
                {
                    Keywords.Add("ATMOSPHERE_OFF");
                }
            }
            else
            {
                Keywords.Add("ATMOSPHERE_OFF");
            }
        }

        return Keywords;
    }
}

public sealed class Planetoid : Planet, IPlanet
{
    public List<Quad> MainQuads = new List<Quad>();
    public List<Quad> Quads = new List<Quad>();

    public Shader ColorShader;
    public ComputeShader CoreShader;

    public int DispatchSkipFramesCount = 8;

    public NoiseParametersSetter NPS = null;

    public QuadCullingMethod CullingMethod = QuadCullingMethod.Custom;
    public QuadLODDistanceMethod LODDistanceMethod = QuadLODDistanceMethod.ClosestCorner;

    public TCCommonParametersSetter tccps;

    public MaterialPropertyBlock QuadMPB;

    public bool WaitOnSplit = false;

    protected override void Awake()
    {
        base.Awake();

        if (Atmosphere != null)
        {
            if (Atmosphere.planetoid == null)
                Atmosphere.planetoid = this;
        }

        if (Cloudsphere != null)
        {
            if (Cloudsphere.planetoid == null)
                Cloudsphere.planetoid = this;
        }

        if (Ring != null)
        {
            //TODO : RINGS
        }

        QuadMPB = new MaterialPropertyBlock();
    }

    protected override void Start()
    {
        base.Start();

        if (tccps == null)
            if (gameObject.GetComponentInChildren<TCCommonParametersSetter>() != null)
                tccps = gameObject.GetComponentInChildren<TCCommonParametersSetter>();

        if (NPS != null)
            NPS.LoadAndInit();

        if (Atmosphere != null)
        {
            Atmosphere.InitPlanetoidUniforms(this);
        }

        ReSetupQuads(); //NOTE : Force resetup on start.
    }

    protected override void Update()
    {
        base.Update();

        CheckCutoff();

        if (LODTarget != null)
            DistanceToLODTarget = PlanetBounds.SqrDistance(LODTarget.position);
        else
            DistanceToLODTarget = -1.0f;

        if (Input.GetKeyDown(KeyCode.F1))
        {
            DrawNormals = !DrawNormals;

            //NOTE : Update shader variable...
            QuadMPB.SetFloat("_DrawNormals", DrawNormals ? 1.0f : 0.0f);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (Atmosphere != null) Atmosphere.TryBake();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (Atmosphere != null)
            {
                Atmosphere.ReanimateAtmosphereUniforms(Atmosphere, this);
            }
        }

        if (Atmosphere != null) Atmosphere.SetUniforms(QuadMPB, null, false, true);

        if (!ExternalRendering)
        {
            Render();
        }

        UpdateLOD();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MainQuads.Clear();
        Quads.Clear();
    }

    protected override void OnRenderObject()
    {
        base.OnRenderObject();
    }

    protected override void OnApplicationFocus(bool focusStatus)
    {
        base.OnApplicationFocus(focusStatus);

        if (focusStatus != true) return;

        //NOTE : So, when unity recompiles shaders or scripts from editor 
        //while playing - quads not draws properly. 
        //1) Reanimation of uniforms/mpb can't help.
        //2) MaterialPropertyBlock.Clear() in Reanimation can't help.
        //3) mpb = null; in Reanimation can't help.
        //4) All parameters are ok in mpb.
        //5) Problem not in MainRenderer.
        //I think i've lost something...
        //This ussue take effect only with mpb, so dirty fix is:
        //ReSetupQuads();
        //NOTE : Fixed. Buffers setted 1 time. Need to update when focus losted.

        ReanimateQuadsBuffers(false);

        if (Atmosphere != null)
        {
            Atmosphere.ReanimateAtmosphereUniforms(Atmosphere, this);
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }

    public void ReanimateQuadsBuffers(bool resetup = false)
    {
        if (resetup) { ReSetupQuads(); return; }

        if (Quads != null)
        {
            if (Quads.Count != 0)
            {
                for (int i = 0; i < Quads.Count; i++)
                {
                    Quads[i].Uniformed = false;
                }
            }
        }
    }

    public void UpdateLOD()
    {
        if (UseLOD == false) return;

        for (int i = 0; i < Quads.Count; i++)
        {
            Quads[i].CheckLOD();
        }
    }

    public void Render(Camera camera)
    {
        if (Quads != null)
        {
            if (PlanetQuadsEnabled)
            {
                for (int i = 0; i < Quads.Count; i++)
                {
                    if (Quads[i] != null && Quads[i].gameObject.activeInHierarchy)
                        Quads[i].Render(camera, DrawLayer);
                }
            }
        }

        if (Atmosphere != null)
        {
            if (AtmosphereEnabled)
            {
                Atmosphere.Render(camera, Origin, DrawLayer);
            }
        }

        if (Cloudsphere != null)
        {
            if (CloudsphereEnabled)
            {
                Cloudsphere.Render(camera, DrawLayer);
            }
        }

        if (Ring != null)
        {
            if (RingEnabled)
            {
                Ring.Render(camera, DrawLayer);
            }
        }
    }

    public void Render()
    {
        Render(CameraHelper.Main());
    }

    public void CheckCutoff()
    {
        //Prevent fast jumping of lod distances check and working state.
        if (Vector3.Distance(LODTarget.transform.position, transform.position) > PlanetRadius * 2 + LODDistances[0])
        {
            for (int i = 0; i < Quads.Count; i++)
            {
                if (Quads[i] != null)
                {
                    Quads[i].StopAllCoroutines();
                }
            }

            Working = false;
        }
    }

    [ContextMenu("DestroyQuads")]
    public void DestroyQuads()
    {
        for (int i = 0; i < Quads.Count; i++)
        {
            if (Quads[i] != null)
                DestroyImmediate(Quads[i].gameObject);
        }

        Quads.Clear();
        MainQuads.Clear();

        if (QuadsRoot != null) DestroyImmediate(QuadsRoot);
    }

    public void UpdateLODDistances()
    {
        LODDistances = new float[LODMaxLevel + 1];

        for (byte i = 0; i < LODDistances.Length; i++)
        {
            if (i == 0)
                LODDistances[i] = PlanetRadius;
            else
            {
                LODDistances[i] = LODDistances[i - 1] / LODDistanceMultiplierPerLevel;
            }
        }
    }

    public float GetLODOctaveModifier(int LODLevel, bool invert = false)
    {
        if (OctaveFade)
        {
            var id = invert
                ? (LODDistances.Length / (LODLevel + 2 + ((LODDistances.Length - LODOctaves.Length) / LODOctaves.Length)))
                : LODOctaves.Length - (LODDistances.Length / (LODLevel + 2 + ((LODDistances.Length - LODOctaves.Length) / LODOctaves.Length)));

            id -= 1;

            if (LODOctaves != null && LODOctaves.Length > 1 && !(id > LODOctaves.Length))
            {
                return LODOctaves[id];
            }
        }

        return 1.0f;
    }

    public int GetCulledQuadsCount()
    {
        return Quads.Count(x => !x.Visible);
    }

    public Quad GetMainQuad(QuadPosition position)
    {
        return MainQuads.FirstOrDefault(q => q.Position == position);
    }

    public void SetupQuads()
    {
        if (Quads.Count > 0)
            return;

        if (tccps == null)
            if (gameObject.GetComponentInChildren<TCCommonParametersSetter>() != null)
                tccps = gameObject.GetComponentInChildren<TCCommonParametersSetter>();

        SetupRoot();
        UpdateLODDistances();

        if (GodManager.Instance.PrototypeMesh == null) return;

        var sides = Enum.GetValues(typeof(QuadPosition));

        foreach (QuadPosition side in sides)
        {
            SetupMainQuad(side);
        }

        if (NPS != null)
            NPS.LoadAndInit();
    }

    public void ReSetupQuads()
    {
        DestroyQuads();
        SetupQuads();
    }

    public void SetupRoot()
    {
        if (QuadsRoot == null)
        {
            QuadsRoot = new GameObject("Quads_Root");
            QuadsRoot.transform.position = transform.position;
            QuadsRoot.transform.rotation = transform.rotation;
            QuadsRoot.transform.parent = transform;
        }
    }

    private void SetupMainQuad(QuadPosition quadPosition)
    {
        var go = new GameObject(string.Format("Quad_{0}", quadPosition));
        go.transform.parent = QuadsRoot.transform;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var mesh = GodManager.Instance.PrototypeMesh;
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        var quadComponent = go.AddComponent<Quad>();
        quadComponent.Planetoid = this;
        quadComponent.QuadMesh = mesh;

        if (Atmosphere != null) Atmosphere.InitUniforms(null, quadComponent.QuadMaterial, false);

        var gc = QuadGenerationConstants.Init(TerrainMaxHeight);
        gc.planetRadius = PlanetRadius;

        gc.cubeFaceEastDirection = quadComponent.GetCubeFaceEastDirection(quadPosition);
        gc.cubeFaceNorthDirection = quadComponent.GetCubeFaceNorthDirection(quadPosition);
        gc.patchCubeCenter = quadComponent.GetPatchCubeCenter(quadPosition);

        quadComponent.Position = quadPosition;
        quadComponent.ID = QuadID.One;
        quadComponent.generationConstants = gc;
        quadComponent.SetupCorners(quadPosition);
        quadComponent.ShouldDraw = true;
        quadComponent.ReadyForDispatch = true;

        Quads.Add(quadComponent);
        MainQuads.Add(quadComponent);
    }

    public Quad SetupSubQuad(QuadPosition quadPosition)
    {
        var go = new GameObject(string.Format("Quad_{0}", quadPosition));

        var mesh = GodManager.Instance.PrototypeMesh;
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        var quadComponent = go.AddComponent<Quad>();
        quadComponent.Planetoid = this;
        quadComponent.QuadMesh = mesh;
        quadComponent.SetupCorners(quadPosition);

        if (Atmosphere != null) Atmosphere.InitUniforms(null, quadComponent.QuadMaterial, false);

        var gc = QuadGenerationConstants.Init(TerrainMaxHeight);
        gc.planetRadius = PlanetRadius;

        quadComponent.Position = quadPosition;
        quadComponent.generationConstants = gc;
        quadComponent.ShouldDraw = false;

        if (qdtccc == null)
            qdtccc = new QuadDistanceToClosestCornerComparer();

        Quads.Add(quadComponent);
        Quads.Sort(qdtccc);

        return quadComponent;
    }
}