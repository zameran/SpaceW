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

using SpaceEngine.Cameras;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Utilities;
using SpaceEngine.Enviroment.Sun;
using SpaceEngine.SciptableObjects;
using SpaceEngine.Startfield;

using System.Linq;

using UnityEngine;

[ExecutionOrder(-9998)]
public class GodManager : MonoSingleton<GodManager>
{
    public GameCamera View;

    public Mesh AtmosphereMesh;
    public Mesh[] OceanScreenMeshGrids;
    public Mesh QuadMesh;

    public Body[] Bodies;
    public Starfield[] Starfields;
    public SunGlare[] Sunglares;

    public AtmosphereHDR HDRMode = AtmosphereHDR.ProlandOptimized;

    public GenerationShadersCoreSettings GSCS;

    public Body ActiveBody { get { return Bodies.FirstOrDefault(body => Helper.Enabled(body)); } }

    public ComputeShader WriteData { get { return GSCS.WriteData; } }
    public ComputeShader ReadData { get { return GSCS.ReadData; } }

    public ComputeShader CopyInscatter1 { get { return GSCS.CopyInscatter1; } }
    public ComputeShader CopyInscatterN { get { return GSCS.CopyInscatterN; } }
    public ComputeShader CopyIrradiance { get { return GSCS.CopyIrradiance; } }
    public ComputeShader Inscatter1 { get { return GSCS.Inscatter1; } }
    public ComputeShader InscatterN { get { return GSCS.InscatterN; } }
    public ComputeShader InscatterS { get { return GSCS.InscatterS; } }
    public ComputeShader Irradiance1 { get { return GSCS.Irradiance1; } }
    public ComputeShader IrradianceN { get { return GSCS.IrradianceN; } }
    public ComputeShader Transmittance { get { return GSCS.Transmittance; } }
    public ComputeShader Variance { get { return GSCS.Variance; } }

    public Shader FourierShader { get { return GSCS.Fourier; } }

    /// <summary>
    /// This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.
    /// </summary>
    public int FourierGridSize = 64;

    /// <summary>
    /// Quad mesh resolution in vertices.
    /// </summary>
    public int GridResolution = 25;

    /// <summary>
    /// Size of each grid in the projected grid. (number of pixels on screen).
    /// </summary>
    public int OceanGridResolution = 4;

    /// <summary>
    /// The size of each tile. For tiles made of raster data, this size is the tile width in pixels (the tile height is supposed equal to the tile width).
    /// </summary>
    public int TileSize { get { return GridResolution * 4; } }

    // TODO : Make these settings switching event based. To avoid constant every-frame checkings...
    public bool Eclipses = true;
    public bool Planetshadows = true;
    public bool Planetshine = true;
    public bool OceanSkyReflections = true;
    public bool OceanDepth = true;
    public bool DelayedCalculations = false;
    public bool FloatingOrigin = false;

    public Texture2D[] NoiseTextures;

    protected GodManager() { }

    private void Awake()
    {
        UnsaveSingleton = true;
        Instance = this;

        InitOceanFourier();

        InitAtmosphereMesh();
        InitQuadMesh();
        InitOceanScreenGridMeshes();

        Bodies = FindObjectsOfType<Body>();
        Starfields = FindObjectsOfType<Starfield>();
        Sunglares = FindObjectsOfType<SunGlare>();

        CreateOrthoNoise();
    }

    private void Update()
    {
        UpdateWorldShift();
        UpdateViewMatrices();
    }

    protected override void OnDestroy()
    {
        Helper.Destroy(AtmosphereMesh);
        Helper.Destroy(QuadMesh);

        base.OnDestroy();
    }

    private void InitAtmosphereMesh()
    {
        AtmosphereMesh = MeshFactory.IcoSphere.Create();
        AtmosphereMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
    }

    private void InitQuadMesh()
    {
        QuadMesh = MeshFactory.MakePlane(GridResolution, MeshFactory.PLANE.XY, true, false, false);
        QuadMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
    }

    private void InitOceanScreenGridMeshes()
    {
        OceanGridResolution = Mathf.Max(1, OceanGridResolution);

        // The number of squares in the grid on the x and y axis
        var NX = Screen.width / OceanGridResolution;
        var NY = Screen.height / OceanGridResolution;
        var gridsCount = 1;

        const int MAX_VERTS = 65000;

        // The number of meshes need to make a grid of this resolution
        if (NX * NY > MAX_VERTS)
        {
            gridsCount += (NX * NY) / MAX_VERTS;
        }

        OceanScreenMeshGrids = new Mesh[gridsCount];

        // Make the meshes. The end product will be a grid of verts that cover the screen on the x and y axis with the z depth at 0. 
        // This grid is then projected as the ocean by the shader
        for (byte i = 0; i < gridsCount; i++)
        {
            NY = Screen.height / gridsCount / OceanGridResolution;

            OceanScreenMeshGrids[i] = MeshFactory.MakeOceanPlane(NX, NY, (float)i / (float)gridsCount, 1.0f / (float)gridsCount);
            OceanScreenMeshGrids[i].bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
        }
    }

    private void InitOceanFourier()
    {
        if (FourierGridSize > 256)
        {
            Debug.Log("GodManager.InitOceanFourier: Fourier grid size must not be greater than 256, changing to 256...");
            FourierGridSize = 256;
        }

        if (!Mathf.IsPowerOfTwo(FourierGridSize))
        {
            Debug.Log("GodManager.InitOceanFourier: Fourier grid size must be pow2 number, changing to nearest pow2 number...");
            FourierGridSize = Mathf.NextPowerOfTwo(FourierGridSize);
        }
    }

    public void UpdateSchedularWrapper()
    {
        Schedular.Instance.Run();
    }

    private void UpdateViewMatrices()
    {
        if (View == null) return;

        View.UpdateMatrices();
    }

    private void UpdateWorldShift()
    {
        if (FloatingOrigin == false) return;

        var cameraPosition = View.transform.position;

        if (cameraPosition.sqrMagnitude > 500000.0)
        {
            var suns = FindObjectsOfType<Sun>();
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

    private void CreateOrthoNoise()
    {
        var tileWidth = TileSize;
        var color = new Color();

        NoiseTextures = new Texture2D[6];

        var layers = new int[] { 0, 1, 3, 5, 7, 15 };
        var rand = 1234567;

        Random.InitState(0);

        for (byte nl = 0; nl < 6; ++nl)
        {
            var layer = layers[nl];

            NoiseTextures[nl] = new Texture2D(tileWidth, tileWidth, TextureFormat.ARGB32, false, true);

            // Corners
            for (int j = 0; j < tileWidth; ++j)
            {
                for (int i = 0; i < tileWidth; ++i)
                {
                    NoiseTextures[nl].SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                }
            }

            // Bottom border
            Random.InitState((layer & 1) == 0 ? 7654321 : 5647381);

            for (int v = 2; v < 4; ++v)
            {
                for (int h = 4; h < tileWidth - 4; ++h)
                {
                    for (byte c = 0; c < 4; ++c)
                    {
                        color[c] = Random.value;
                    }

                    NoiseTextures[nl].SetPixel(h, v, color);
                    NoiseTextures[nl].SetPixel(tileWidth - 1 - h, 3 - v, color);
                }
            }

            // Right border
            Random.InitState((layer & 2) == 0 ? 7654321 : 5647381);

            for (int h = tileWidth - 3; h >= tileWidth - 4; --h)
            {
                for (int v = 4; v < tileWidth - 4; ++v)
                {
                    for (byte c = 0; c < 4; ++c)
                    {
                        color[c] = Random.value;
                    }

                    NoiseTextures[nl].SetPixel(h, v, color);
                    NoiseTextures[nl].SetPixel(2 * tileWidth - 5 - h, tileWidth - 1 - v, color);
                }
            }

            // Top border
            Random.InitState((layer & 4) == 0 ? 7654321 : 5647381);

            for (int v = tileWidth - 2; v < tileWidth; ++v)
            {
                for (int h = 4; h < tileWidth - 4; ++h)
                {
                    for (byte c = 0; c < 4; ++c)
                    {
                        color[c] = Random.value;
                    }

                    NoiseTextures[nl].SetPixel(h, v, color);
                    NoiseTextures[nl].SetPixel(tileWidth - 1 - h, 2 * tileWidth - 5 - v, color);
                }
            }

            // Left border
            Random.InitState((layer & 8) == 0 ? 7654321 : 5647381);

            for (int h = 1; h >= 0; --h)
            {
                for (int v = 4; v < tileWidth - 4; ++v)
                {
                    for (byte c = 0; c < 4; ++c)
                    {
                        color[c] = Random.value;
                    }

                    NoiseTextures[nl].SetPixel(h, v, color);
                    NoiseTextures[nl].SetPixel(3 - h, tileWidth - 1 - v, color);
                }
            }

            // Center
            Random.InitState(rand);

            for (int v = 4; v < tileWidth - 4; ++v)
            {
                for (int h = 4; h < tileWidth - 4; ++h)
                {
                    for (byte c = 0; c < 4; ++c)
                    {
                        color[c] = Random.value;
                    }

                    NoiseTextures[nl].SetPixel(h, v, color);
                }
            }

            //randomize for next texture
            rand = (rand * 1103515245 + 12345) & 0x7FFFFFFF;

            NoiseTextures[nl].name = string.Format("OrthoNoise_{0}x{0}_{1}", tileWidth, nl);
            NoiseTextures[nl].Apply();
        }
    }
}