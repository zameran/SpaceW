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
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
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
// Creation Date: 2017.10.02
// Creation Time: 6:49 PM
// Creator: zameran
#endregion

using SpaceEngine.Core;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Utilities.Gradients;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace SpaceEngine.Tests
{
    [Serializable]
    public struct GalaxyStar
    {
        public Vector3 position;
        public Vector4 color;
        public float size;
        public float temperature;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxyGenerationParameters
    {
        public Vector3 Randomize;                   // Randomation vector...
        public Vector3 Offset;                      // Linear offset for ellipses - asymmetry factor...

        public Vector4 Warp;                        // Pre/Pos-rotation warp 2D vector packed...

        public float Radius;                        // Galaxy radius...
        public float RadiusEllipse;
        public float SizeBar;
        public float Depth;
        public float InverseSpiralEccentricity;
        public float SpiralRotation;

        public GalaxyGenerationParameters(GalaxyGenerationParameters from)
        {
            Randomize = from.Randomize;
            Offset = from.Offset;

            Warp = from.Warp;

            Radius = from.Radius;
            RadiusEllipse = from.RadiusEllipse;
            SizeBar = from.SizeBar;
            Depth = from.Depth;
            InverseSpiralEccentricity = from.InverseSpiralEccentricity;
            SpiralRotation = from.SpiralRotation;
        }

        public GalaxyGenerationParameters(Vector3 randomize, Vector3 offset, Vector4 warp,
                                          float radius, float radiusEllipse, float sizeBar, float depth, float inverseSpiralEccentricity,
                                          float spiralRotation)
        {
            Randomize = randomize;
            Offset = offset;

            Warp = warp;

            Radius = radius;
            RadiusEllipse = radiusEllipse;
            SizeBar = sizeBar;
            Depth = depth;
            InverseSpiralEccentricity = inverseSpiralEccentricity;
            SpiralRotation = spiralRotation;
        }

        public static GalaxyGenerationParameters Default
        {
            get
            {
                return new GalaxyGenerationParameters(Vector3.zero, 
                                                      new Vector3(0.0f, 0.0f, 0.03f), 
                                                      new Vector4(0.3f, 0.15f, 0.025f, 0.01f),
                                                      128.0f, 0.7f, -0.25f, 32.0f, 0.75f, 6.2832f);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxyGenerationPerPassParameters
    {
        public float PassRotation;

        public GalaxyGenerationPerPassParameters(GalaxyGenerationPerPassParameters from)
        {
            PassRotation = from.PassRotation;
        }

        public GalaxyGenerationPerPassParameters(float passRotation)
        {
            PassRotation = passRotation;
        }

        public static GalaxyGenerationPerPassParameters Default
        {
            get
            {
                return new GalaxyGenerationPerPassParameters(Mathf.PI / 2.0f);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxyParameters
    {
        public int PassCount;
        public int Count;
        public int DrawCount;

        public GalaxyParameters(GalaxyParameters from)
        {
            PassCount = from.PassCount;
            Count = from.Count;
            DrawCount = from.DrawCount;
        }

        public GalaxyParameters(int passCount, int count, int drawCount)
        {
            PassCount = passCount;
            Count = count;
            DrawCount = drawCount;
        }

        public static GalaxyParameters Default
        {
            get
            {
                return new GalaxyParameters(2, 128000, 128000);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxyRenderingParameters
    {
        // TODO : Finish him...
        public float DustStrength;
        public float DustSize;

        public GalaxyRenderingParameters(GalaxyRenderingParameters from)
        {
            DustStrength = from.DustStrength;
            DustSize = from.DustSize;
        }

        public GalaxyRenderingParameters(float dustStrength, float dustSize)
        {
            DustStrength = dustStrength;
            DustSize = dustSize;
        }

        public static GalaxyRenderingParameters Default
        {
            get
            {
                return new GalaxyRenderingParameters(0.025f, 1.0f);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GenerationSettings
    {
        internal enum GenerationType : byte
        {
            None = 0,
            Single = 1,
            Double = 2
        }

        public GenerationType Type;

        public GalaxyParameters GalaxyParameters;
        public GalaxyGenerationParameters GalaxyGenerationParameters;
        public GalaxyGenerationPerPassParameters GalaxyGenerationPerPassParameters;

        public GenerationSettings(GenerationType type, GalaxyParameters gp, GalaxyGenerationParameters ggp, GalaxyGenerationPerPassParameters ggppp)
        {
            Type = type;

            GalaxyParameters = new GalaxyParameters(gp);
            GalaxyGenerationParameters = new GalaxyGenerationParameters(ggp);
            GalaxyGenerationPerPassParameters = new GalaxyGenerationPerPassParameters(ggppp);
        }

        public GenerationSettings(GenerationSettings from)
        {
            Type = from.Type;

            GalaxyParameters = new GalaxyParameters(from.GalaxyParameters);
            GalaxyGenerationParameters = new GalaxyGenerationParameters(from.GalaxyGenerationParameters);
            GalaxyGenerationPerPassParameters = new GalaxyGenerationPerPassParameters(from.GalaxyGenerationPerPassParameters);
        }

        public static GenerationSettings Default
        {
            get
            {
                return new GenerationSettings(GenerationType.Single, GalaxyParameters.Default, 
                                                                     GalaxyGenerationParameters.Default, 
                                                                     GalaxyGenerationPerPassParameters.Default);
            }
        }

        public static string FilePrefix { get { return "Galaxy"; } }

        public static string FilePostfix { get { return string.Format("{0:yy.MM.dd-hh.mm.ss}", DateTime.Now); } }

        public static string FileExtension { get { return "json"; } }

        public static string ContainingFolder { get { return Application.dataPath + "/Resources/Output"; } }

        #region API

        internal List<string> FindSettings()
        {
            var folderPath = GenerationSettings.ContainingFolder;
            var filePathPattern = string.Format("{0}_*.{1}", GenerationSettings.FilePrefix, GenerationSettings.FileExtension);

            if (!System.IO.Directory.Exists(folderPath)) return null;

            return System.IO.Directory.GetFiles(folderPath, filePathPattern).ToList();
        }

        internal void SaveSettings()
        {
            var settings = new GenerationSettings(this);
            var jsonString = JsonUtility.ToJson(settings, true);
            var folderPath = GenerationSettings.ContainingFolder;
            var filePath = string.Format("{0}/{1}_{2}.{3}", folderPath, GenerationSettings.FilePrefix, GenerationSettings.FilePostfix, GenerationSettings.FileExtension);

            if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);

            System.IO.File.WriteAllText(filePath, jsonString);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        internal void LoadSettings(string filePath, Action<GenerationSettings> callback)
        {
            if (!System.IO.File.Exists(filePath)) return;

            var fileContent = System.IO.File.ReadAllText(filePath);
            var settings = JsonUtility.FromJson<GenerationSettings>(fileContent);

            if (callback != null) callback(settings);
        }

        internal void DeleteSettings(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return;

            System.IO.File.Delete(filePath);

            // TODO : Delete folder, if last file deleted...

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        #endregion
    }

    internal class GalaxyTest : Node<GalaxyTest>, IRenderable<GalaxyTest>
    {
        public ComputeShader Core;

        public Shader StarsShader;
        public Shader DustShader;

        private Material StarsMaterial;

        private List<List<ComputeBuffer>> StarsBuffers = new List<List<ComputeBuffer>>();
        private List<Material> DustMaterials = new List<Material>();
        private ComputeBuffer DustArgsBuffer;

        private Bounds GalaxyBounds;

        public GenerationSettings Settings = GenerationSettings.Default;

        public ColorMaterialTableGradientLut ColorDistribution = new ColorMaterialTableGradientLut();

        public bool AutoUpdate = false;

        public Mesh DustMesh = null;

        #region Galaxy

        #region Settings

        internal void ApplyPreset(GenerationSettings gs)
        {
            Settings = new GenerationSettings(gs);

            InitAndGenerateBuffers();
            InitDustMaterials();
        }

        #endregion


        #region Dust

        public void InitDustMaterials()
        {
            if (DustMaterials != null)
            {
                DestroyDustMaterials();
            }

            DustMaterials = new List<Material>((int)Settings.Type);

            for (byte materialIndex = 0; materialIndex < DustMaterials.Capacity; materialIndex++)
            {
                var material = MaterialHelper.CreateTemp(DustShader, string.Format("Dust-{0}", materialIndex));

                DustMaterials.Add(material);
            }
        }

        public void DestroyDustMaterials()
        {
            for (byte materialIndex = 0; materialIndex < DustMaterials.Capacity; materialIndex++)
            {
                Helper.Destroy(DustMaterials[materialIndex]);
            }

            DustMaterials.Clear();
        }

        #endregion

        #region Buffers

        public void InitAndGenerateBuffers()
        {
            InitBuffers();
            GenerateBuffers();
        }

        public void InitBuffers()
        {
            if (StarsBuffers != null || DustArgsBuffer != null)
            {
                DestroyBuffers();
            }

            StarsBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);

            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = new List<ComputeBuffer>(Settings.GalaxyParameters.PassCount);

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = new ComputeBuffer(Settings.GalaxyParameters.Count, Marshal.SizeOf<GalaxyStar>(), ComputeBufferType.Default);

                    buffer.SetData(new GalaxyStar[Settings.GalaxyParameters.Count]);

                    buffers.Add(buffer);
                }

                StarsBuffers.Add(buffers);
            }

            DustArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        public void GenerateBuffers()
        {
            Core.SetTexture(0, "MaterialTable", ColorDistribution.Lut);

            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];
                var perPassRotation = (generationType % 2 == 0 ? Settings.GalaxyGenerationPerPassParameters.PassRotation * generationType :
                    Settings.GalaxyGenerationPerPassParameters.PassRotation);

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    Core.SetVector("Randomize", (Settings.GalaxyGenerationParameters.Randomize + new Vector3(1.0f, 0.0f, 1.0f)) * ((bufferIndex + 1 + generationType + 1) / 10.0f));
                    Core.SetVector("offsetParams1", new Vector4(Settings.GalaxyGenerationParameters.Offset.x, Settings.GalaxyGenerationParameters.Offset.y, Settings.GalaxyGenerationParameters.Offset.z, 0.0f));
                    Core.SetVector("sizeParams1", new Vector4(Settings.GalaxyGenerationParameters.Radius, Settings.GalaxyGenerationParameters.RadiusEllipse, Settings.GalaxyGenerationParameters.SizeBar, Settings.GalaxyGenerationParameters.Depth));
                    Core.SetVector("warpParams1", Settings.GalaxyGenerationParameters.Warp);
                    Core.SetVector("spiralParams1", new Vector4(Settings.GalaxyGenerationParameters.InverseSpiralEccentricity, Settings.GalaxyGenerationParameters.SpiralRotation, perPassRotation, 0.0f));

                    Core.SetBuffer(0, "output", buffer);
                    Core.Dispatch(0, (int)(Settings.GalaxyParameters.Count / 1024.0f), 1, 1);
                }
            }
        }

        protected void DestroyBuffers()
        {
            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    buffer.ReleaseAndDisposeBuffer();
                }

                buffers.Clear();
            }

            StarsBuffers.Clear();

            DustArgsBuffer.ReleaseAndDisposeBuffer();
        }

        #endregion

        #endregion

        #region Node

        protected override void InitNode()
        {
            GalaxyBounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            if (StarsShader == null) StarsShader = Shader.Find("SpaceEngine/Galaxy/StarTest");
            StarsMaterial = MaterialHelper.CreateTemp(StarsShader, "Stars");

            if (DustShader == null) DustShader = Shader.Find("SpaceEngine/Galaxy/DustTest");
            InitDustMaterials();

            if (DustMesh == null) Debug.LogWarning("GalaxyTest.InitNode: DustMesh is null! Impossible to render dust!");

            ColorDistribution.GenerateLut();

            InitBuffers();
            GenerateBuffers();
        }

        protected override void UpdateNode()
        {
            if (AutoUpdate) GenerateBuffers();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DestroyBuffers();
            DestroyDustMaterials();

            Helper.Destroy(StarsMaterial);

            ColorDistribution.DestroyLut();
        }

        #endregion

        #region IRenderable

        public virtual void Render(int layer = 8)
        {           
            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    StarsMaterial.SetPass(0);
                    StarsMaterial.SetBuffer("stars", buffer);
                    
                    Graphics.DrawProcedural(MeshTopology.Points, Settings.GalaxyParameters.DrawCount);
                }
            }
        }

        public void RenderDust()
        {
            if (DustMesh == null) return;

            var args = new uint[5];
            args[0] = (uint)DustMesh.GetIndexCount(0);              // Index count per instance...
            args[1] = (uint)Settings.GalaxyParameters.DrawCount;    // Instance count...
            args[2] = 0;                                            // Start index location...
            args[3] = 0;                                            // Base vertex location...
            args[4] = 0;                                            // Start instance location...

            DustArgsBuffer.SetData(args);

            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];
                var material = DustMaterials[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    material.SetPass(0);
                    material.SetBuffer("stars", buffer);
                    // TODO : Calculate _Galaxy_Orientation and _Galaxy_OrientationInverse relative to camera, for a better visualization...
                    material.SetVector("_Galaxy_Position", transform.position - GodManager.Instance.View.WorldCameraPosition);
                    material.SetVector("_Galaxy_Orientation", transform.rotation.eulerAngles);
                    material.SetVector("_Galaxy_OrientationInverse", -transform.rotation.eulerAngles);

                    Graphics.DrawMeshInstancedIndirect(DustMesh, 0, material, GalaxyBounds, DustArgsBuffer, 0);
                }
            }
        }

        #endregion
    }
}