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

using SpaceEngine;
using SpaceEngine.AtmosphericScattering.Sun;
using SpaceEngine.Cameras;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Utilities;
using SpaceEngine.Startfield;

using System.Linq;

using UnityEngine;

[ExecutionOrder(-9998)]
public class GodManager : MonoSingleton<GodManager>
{
    public GameCamera View;

    public ComputeShader WriteData;
    public ComputeShader ReadData;

    public ComputeShader CopyInscatter1;
    public ComputeShader CopyInscatterN;
    public ComputeShader CopyIrradiance;
    public ComputeShader Inscatter1;
    public ComputeShader InscatterN;
    public ComputeShader InscatterS;
    public ComputeShader Irradiance1;
    public ComputeShader IrradianceN;
    public ComputeShader Transmittance;

    public Mesh AtmosphereMesh;
    public Mesh QuadMesh;

    public Body ActiveBody { get { return Bodies.FirstOrDefault(body => Helper.Enabled(body)); } }

    public Body[] Bodies;
    public Starfield[] Starfields;
    public SunGlare[] Sunglares;

    public AtmosphereHDR HDRMode = AtmosphereHDR.ProlandOptimized;

    public Matrix4x4d WorldToCamera { get { return View.WorldToCameraMatrix; } }
    public Matrix4x4d CameraToWorld { get { return View.CameraToWorldMatrix; } }
    public Matrix4x4d CameraToScreen { get { return View.CameraToScreenMatrix; } }
    public Matrix4x4d ScreenToCamera { get { return View.ScreenToCameraMatrix; } }
    public Vector3 WorldCameraPos { get { return View.WorldCameraPosition; } }

    public int GridResolution = 25;

    // TODO : Make these settings switching event based. To avoid constant every-frame checkings...
    public bool Eclipses = true;
    public bool Planetshadows = true;
    public bool Planetshine = true;
    public bool OceanSkyReflections = true;
    public bool DelayedCalculations = false;
    public bool FloatingOrigin = false;
    public bool DebugFBO = false;

    protected GodManager() { }

    private void Awake()
    {
        Instance = this;

        AtmosphereMesh = MeshFactory.IcoSphere.Create();
        AtmosphereMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

        QuadMesh = MeshFactory.MakePlane(GridResolution, MeshFactory.PLANE.XY, true, false, false);
        QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

        Bodies = FindObjectsOfType<Body>();
        Starfields = FindObjectsOfType<Starfield>();
        Sunglares = FindObjectsOfType<SunGlare>();
    }

    private void Update()
    {
        UpdateSchedular();
    }

    protected override void OnDestroy()
    {
        Helper.Destroy(AtmosphereMesh);
        Helper.Destroy(QuadMesh);

        base.OnDestroy();
    }

    private void OnGUI()
    {
        if (FrameBufferCapturer.Instance.FBOExist() && DebugFBO)
        {
            var fboDebugDrawSize = FrameBufferCapturer.Instance.DebugDrawSize;

            GUI.DrawTexture(new Rect(new Vector2(Screen.width - fboDebugDrawSize.x, 0), fboDebugDrawSize), FrameBufferCapturer.Instance.FBOTexture, ScaleMode.StretchToFill, false);
        }
    }

    private void UpdateSchedular()
    {
        Schedular.Instance.Run();
    }

    public void UpdateControllerWrapper()
    {
        UpdateWorldShift();

        View.UpdateMatrices();
    }

    private void UpdateWorldShift()
    {
        if (FloatingOrigin == false) return;

        var cameraPosition = View.transform.position;

        if (cameraPosition.sqrMagnitude > 500000.0)
        {
            var suns = FindObjectsOfType<AtmosphereSun>();
            var bodies = FindObjectsOfType<CelestialBody>();

            foreach (var sun in suns)
            {
                var sunTransform = sun.transform;

                if (sunTransform.parent == null)
                {
                    sun.transform.position -= cameraPosition;
                }
            }

            foreach (var body in bodies)
            {
                var bodyTransform = body.transform;

                if (bodyTransform.parent == null)
                {
                    body.Origin -= cameraPosition;
                }
            }

            if (View.transform.parent == null) View.transform.position -= cameraPosition;
        }
    }
}