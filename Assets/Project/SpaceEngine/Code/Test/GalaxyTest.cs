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
using SpaceEngine.Core.Octree;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;

using Random = UnityEngine.Random;

namespace SpaceEngine.Tests
{
    [Serializable]
    public struct GalaxyParticle
    {
        public Vector3 position;
        public Vector4 color;
        public float size;
        public float temperature;
        public Vector3 id;
    }

    [Serializable]
    public class GalaxyStar
    {
        public Vector3 Position { get; }

        public float Size { get; }

        public GalaxyStar()
        {
            Position = Vector3.zero;

            Size = 1.0f;
        }

        public GalaxyStar(Vector3 position)
        {
            Position = position;

            Size = 1.0f;
        }

        public GalaxyStar(Vector3 position, float size)
        {
            Position = position;

            Size = size;
        }

        #region Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("({0}, {1})", Position, Size);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var item = obj as GalaxyStar;

            if (item == null) { return false; }

            return Position.Equals(item.Position) && Size.AlmostEquals(item.Size);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Position.GetHashCode();
        }

        #endregion
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxyGenerationParameters
    {
        public Vector3 TemperatureRange;            // Temperature range... (Z component for shift)
        public Vector3 Randomize;                   // Randomation vector...
        public Vector3 Offset;                      // Linear offset for ellipses - asymmetry factor...

        public Vector4 Warp;                        // Pre/Pos-rotation warp 2D vector packed...

        public float Radius;                        // Galaxy radius...
        public float RadiusEllipse;
        public float SizeBar;
        public float Depth;
        public float InverseSpiralEccentricity;
        public float SpiralRotation;

        public ColorMaterialTableGradientLut StarsColorDistribution;
        public ColorMaterialTableGradientLut DustColorDistribution;

        public GalaxyGenerationParameters(GalaxyGenerationParameters from)
        {
            TemperatureRange = from.TemperatureRange;

            Randomize = from.Randomize;
            Offset = from.Offset;

            Warp = from.Warp;

            Radius = from.Radius;
            RadiusEllipse = from.RadiusEllipse;
            SizeBar = from.SizeBar;
            Depth = from.Depth;
            InverseSpiralEccentricity = from.InverseSpiralEccentricity;
            SpiralRotation = from.SpiralRotation;

            StarsColorDistribution = new ColorMaterialTableGradientLut();
            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public GalaxyGenerationParameters(Vector3 temperatureRange, Vector3 randomize, Vector3 offset, Vector4 warp,
                                          float radius, float radiusEllipse, float sizeBar, float depth, float inverseSpiralEccentricity,
                                          float spiralRotation)
        {
            TemperatureRange = temperatureRange;

            Randomize = randomize;
            Offset = offset;

            Warp = warp;

            Radius = radius;
            RadiusEllipse = radiusEllipse;
            SizeBar = sizeBar;
            Depth = depth;
            InverseSpiralEccentricity = inverseSpiralEccentricity;
            SpiralRotation = spiralRotation;

            StarsColorDistribution = new ColorMaterialTableGradientLut();
            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public static GalaxyGenerationParameters Default
        {
            get
            {
                return new GalaxyGenerationParameters(new Vector3(800.0f, 29200.0f, 0.0f),
                                                      new Vector3(0.0f, 0.0f, 0.0f), 
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
        public int DustCount;
        public int Count;

        public GalaxyParameters(GalaxyParameters from)
        {
            PassCount = from.PassCount;
            DustCount = from.DustCount;
            Count = from.Count;
        }

        public GalaxyParameters(int passCount, int dustCount, int count)
        {
            PassCount = passCount;
            DustCount = dustCount;
            Count = count;
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
        [Range(0.0f, 1.0f)]
        public float DustStrength;

        [Range(0.0f, 4.0f)]
        public float DustSize;

        [Range(0.0f, 1.0f)]
        public float GasStength;

        [Range(0.0f, 4.0f)]
        public float GasSize;

        [Range(1.0f, 1024.0f)]
        public float StarAbsoluteSize;

        [Range(1.0f, 4.0f)]
        public int DustPassCount;

        public ColorMaterialTableGradientLut DustColorDistribution;

        public GalaxyRenderingParameters(GalaxyRenderingParameters from)
        {
            DustStrength = from.DustStrength;
            DustSize = from.DustSize;
            GasStength = from.GasStength;
            GasSize = from.GasSize;
            StarAbsoluteSize = from.StarAbsoluteSize;

            DustPassCount = from.DustPassCount;

            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public GalaxyRenderingParameters(float dustStrength, float dustSize, float gasStrength, float gasSize, float starAbsoluteSize, int dustPassCount)
        {
            DustStrength = dustStrength;
            DustSize = dustSize;
            GasStength = gasStrength;
            GasSize = gasSize;
            StarAbsoluteSize = starAbsoluteSize;

            DustPassCount = dustPassCount;

            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public static GalaxyRenderingParameters Default
        {
            get
            {
                return new GalaxyRenderingParameters(0.0075f, 1.0f, 0.0050f, 0.5f, 64.0f, 1);
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct GalaxySettings
    {
        internal enum GenerationType : byte
        {
            None = 0,
            Single = 1,
            Double = 2
        }

        public GenerationType Type;

        public GalaxyRenderingParameters GalaxyRenderingParameters;

        public GalaxyParameters GalaxyParameters;
        public GalaxyGenerationParameters GalaxyGenerationParameters;
        public GalaxyGenerationPerPassParameters GalaxyGenerationPerPassParameters;

        /// <summary>
        /// Returns total count of stars in the whole Galaxy.
        /// </summary>
        public long TotalStarsCount { get { return GalaxyParameters.Count * GalaxyParameters.PassCount * (int)Type; } }

        /// <summary>
        /// Returns total count of dust points in the whole Galaxy.
        /// </summary>
        public long TotalDustCount { get { return GalaxyParameters.DustCount * GalaxyParameters.PassCount * (int)Type; } }

        public GalaxySettings(GenerationType type, GalaxyRenderingParameters grp, GalaxyParameters gp, GalaxyGenerationParameters ggp, GalaxyGenerationPerPassParameters ggppp)
        {
            Type = type;

            GalaxyRenderingParameters = new GalaxyRenderingParameters(grp);

            GalaxyParameters = new GalaxyParameters(gp);
            GalaxyGenerationParameters = new GalaxyGenerationParameters(ggp);
            GalaxyGenerationPerPassParameters = new GalaxyGenerationPerPassParameters(ggppp);
        }

        public GalaxySettings(GalaxySettings from)
        {
            Type = from.Type;

            GalaxyRenderingParameters = new GalaxyRenderingParameters(from.GalaxyRenderingParameters);

            GalaxyParameters = new GalaxyParameters(from.GalaxyParameters);
            GalaxyGenerationParameters = new GalaxyGenerationParameters(from.GalaxyGenerationParameters);
            GalaxyGenerationPerPassParameters = new GalaxyGenerationPerPassParameters(from.GalaxyGenerationPerPassParameters);
        }

        public static GalaxySettings Default
        {
            get
            {
                return new GalaxySettings(GenerationType.Single, GalaxyRenderingParameters.Default,
                                                                 GalaxyParameters.Default, 
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
            var folderPath = GalaxySettings.ContainingFolder;
            var filePathPattern = string.Format("{0}_*.{1}", GalaxySettings.FilePrefix, GalaxySettings.FileExtension);

            if (!System.IO.Directory.Exists(folderPath)) return null;

            return System.IO.Directory.GetFiles(folderPath, filePathPattern).ToList();
        }

        internal void SaveSettings()
        {
            var settings = new GalaxySettings(this);
            var jsonString = JsonUtility.ToJson(settings, true);
            var folderPath = GalaxySettings.ContainingFolder;
            var filePath = string.Format("{0}/{1}_{2}.{3}", folderPath, GalaxySettings.FilePrefix, GalaxySettings.FilePostfix, GalaxySettings.FileExtension);

            if (!System.IO.Directory.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);

            System.IO.File.WriteAllText(filePath, jsonString);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        internal void LoadSettings(string filePath, Action<GalaxySettings> callback)
        {
            if (!System.IO.File.Exists(filePath)) return;

            var fileContent = System.IO.File.ReadAllText(filePath);
            var settings = JsonUtility.FromJson<GalaxySettings>(fileContent);

            if (callback != null) callback(settings);
        }

        internal void DeleteSettings(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return;

            System.IO.File.Delete(filePath);

            var fileInfo = new System.IO.FileInfo(filePath);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            #region Delete Parent Folder

            if (fileInfo.Directory != null)
            {
                if (fileInfo.Directory.GetFiles().Length == 0)
                {
                    if (fileInfo.DirectoryName != null) System.IO.Directory.Delete(fileInfo.DirectoryName);
                }
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            #endregion
        }

        #endregion
    }

    internal class GalaxyTest : Node<GalaxyTest>, IRenderable<GalaxyTest>
    {
        public ComputeShader Core;

        public Shader StarsShader;
        public Shader DustShader;
        public Shader ScreenShader;
        public Shader SpriteShader;

        public Texture2D StarParticle;
        public Texture2D GasParticle;

        private Material StarsMaterial;
        private Material ScreenMaterial;

        private List<List<ComputeBuffer>> StarsBuffers = new List<List<ComputeBuffer>>();
        private List<List<ComputeBuffer>> DustBuffers = new List<List<ComputeBuffer>>();
        private List<List<ComputeBuffer>> DustAppendBuffers = new List<List<ComputeBuffer>>();
        private List<List<ComputeBuffer>> GasAppendBuffers = new List<List<ComputeBuffer>>();
        private List<Material> DustMaterials = new List<Material>();
        private List<Material> DustSpriteMaterials = new List<Material>();
        private ComputeBuffer DustArgsBuffer;
        private ComputeBuffer GasArgsBuffer;
        private ComputeBuffer SpriteArgsBuffer;

        private CommandBuffer DustCommandBuffer;

        public GalaxySettings Settings = GalaxySettings.Default;

        public Mesh VolumeMesh = null;

        [HideInInspector]
        public Mesh ScreenMesh = null;

        public bool AutoUpdate = false;
        public bool AutoLutUpdate = false;

        private RenderTexture FrameBuffer1;
        private RenderTexture FrameBuffer2;

        private float BlendFactor = 0.0f;

        public PointOctree<GalaxyStar> Octree;

        #region Galaxy

        #region Settings

        internal void ApplyPreset(GalaxySettings gs)
        {
            Settings = new GalaxySettings(gs);

            InitAndGenerateBuffers();
            InitDustMaterials();
        }

        #endregion

        #region Dust

        public void InitDustMaterials()
        {
            if (DustMaterials != null || DustSpriteMaterials != null)
            {
                DestroyDustMaterials();
            }

            DustMaterials = new List<Material>((int)Settings.Type);
            DustSpriteMaterials = new List<Material>((int)Settings.Type);

            for (byte materialIndex = 0; materialIndex < DustMaterials.Capacity; materialIndex++)
            {
                var material = MaterialHelper.CreateTemp(DustShader, string.Format("Dust-{0}", materialIndex));

                DustMaterials.Add(material);
            }

            for (byte materialIndex = 0; materialIndex < DustSpriteMaterials.Capacity; materialIndex++)
            {
                var material = MaterialHelper.CreateTemp(SpriteShader, string.Format("Dust-Sprite-{0}", materialIndex));

                DustSpriteMaterials.Add(material);
            }
        }

        public void DestroyDustMaterials()
        {
            for (byte materialIndex = 0; materialIndex < DustMaterials.Capacity; materialIndex++)
            {
                Helper.Destroy(DustMaterials[materialIndex]);
            }

            DustMaterials.Clear();

            for (byte materialIndex = 0; materialIndex < DustSpriteMaterials.Capacity; materialIndex++)
            {
                Helper.Destroy(DustSpriteMaterials[materialIndex]);
            }

            DustSpriteMaterials.Clear();
        }

        #endregion

        #region Particles

        private void GenerateParticles(ParticleSystem system, ComputeBuffer source)
        {
            if (system == null) return;

            // TODO : Finish with particles...

            var rendererModule = system.GetComponent<Renderer>();

            var buffer = source;
            var bufferSize = buffer.count;
            var stars = new GalaxyParticle[bufferSize];
            var points = new ParticleSystem.Particle[bufferSize];

            buffer.GetData(stars);

            for (var i = 0; i < bufferSize; i++)
            {
                points[i].position = stars[i].position;
                points[i].startSize = stars[i].size / 64;
                points[i].startColor = stars[i].color.ToColor();
            }

            system.SetParticles(points, bufferSize);

            rendererModule.material.SetTexture("_MainTex", Resources.Load("Textures/Galaxy/StarParticle", typeof(Texture2D)) as Texture2D);
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
            if (StarsBuffers != null || DustBuffers != null || 
                DustAppendBuffers != null || GasAppendBuffers != null || 
                DustArgsBuffer != null || GasArgsBuffer != null || SpriteArgsBuffer != null)
            {
                DestroyBuffers();
            }

            StarsBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);
            DustBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);
            DustAppendBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);
            GasAppendBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);

            InitParticleBuffers<GalaxyParticle>(ref StarsBuffers, Settings.GalaxyParameters.PassCount, Settings.GalaxyParameters.Count);
            InitParticleBuffers<GalaxyParticle>(ref DustBuffers, Settings.GalaxyParameters.PassCount, Settings.GalaxyParameters.DustCount);
            InitParticleBuffers<GalaxyParticle>(ref DustAppendBuffers, Settings.GalaxyParameters.PassCount, Settings.GalaxyParameters.Count, ComputeBufferType.Append);
            InitParticleBuffers<GalaxyParticle>(ref GasAppendBuffers, Settings.GalaxyParameters.PassCount, Settings.GalaxyParameters.Count, ComputeBufferType.Append);

            DustArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            GasArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            SpriteArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        public void GenerateBuffers()
        {
            SetAppendCounters(ref DustAppendBuffers);
            SetAppendCounters(ref GasAppendBuffers);

            var starsKernel = Core.FindKernel("Stars");
            var dustKernel = Core.FindKernel("Dust");
            var filterDustKernel = Core.FindKernel("FilterDust");
            var filterGasKernel = Core.FindKernel("FilterGas");

            Core.SetTexture(starsKernel, "ColorDistributionTable", Settings.GalaxyGenerationParameters.StarsColorDistribution.Lut);
            Core.SetTexture(dustKernel, "ColorDistributionTable", Settings.GalaxyGenerationParameters.DustColorDistribution.Lut);

            ExecuteParticleGenerator(starsKernel, ref StarsBuffers, (int)(Settings.GalaxyParameters.DustCount / 1024.0f), 1, 1);
            ExecuteParticleGenerator(dustKernel, ref DustBuffers, (int)(Settings.GalaxyParameters.DustCount / 1024.0f), 1, 1);

            ExecuteParticleFilter(filterDustKernel, ref DustBuffers, ref DustAppendBuffers, (int)(Settings.GalaxyParameters.DustCount / 1024.0f), 1, 1);
            ExecuteParticleFilter(filterGasKernel, ref DustBuffers, ref GasAppendBuffers, (int)(Settings.GalaxyParameters.DustCount / 1024.0f), 1, 1);
        }

        protected void DestroyBuffers()
        {
            DestroyParticleBuffers(ref StarsBuffers);
            DestroyParticleBuffers(ref DustBuffers);
            DestroyParticleBuffers(ref DustAppendBuffers);
            DestroyParticleBuffers(ref GasAppendBuffers);

            DustArgsBuffer.ReleaseAndDisposeBuffer();
            GasArgsBuffer.ReleaseAndDisposeBuffer();
            SpriteArgsBuffer.ReleaseAndDisposeBuffer();
        }

        public void SetAppendCounters(ref List<List<ComputeBuffer>> input, uint value = 0)
        {
            for (byte generationType = 0; generationType < input.Capacity; generationType++)
            {
                var inputBuffers = input[generationType];

                for (var bufferIndex = 0; bufferIndex < inputBuffers.Capacity; bufferIndex++)
                {
                    var inputBuffer = inputBuffers[bufferIndex];

                    inputBuffer.SetCounterValue(value);
                }
            }
        }

        public void ExecuteParticleGenerator(int kernel, ref List<List<ComputeBuffer>> input, int tgX, int tgY, int tgZ)
        {
            for (byte generationType = 0; generationType < input.Capacity; generationType++)
            {
                var inputBuffers = input[generationType];
                var perPassRotation = (generationType % 2 == 0 ? Settings.GalaxyGenerationPerPassParameters.PassRotation * generationType :
                                                                 Settings.GalaxyGenerationPerPassParameters.PassRotation);

                for (var bufferIndex = 0; bufferIndex < inputBuffers.Capacity; bufferIndex++)
                {
                    var inputBuffer = inputBuffers[bufferIndex];

                    Core.SetInt("currentPassIndex", (generationType + 1) + (bufferIndex + 1));

                    Core.SetVector("randomParams1", (Settings.GalaxyGenerationParameters.Randomize + new Vector3(1.0f, 0.0f, 1.0f)) * ((bufferIndex + 1 + generationType + 1) / 10.0f));
                    Core.SetVector("offsetParams1", new Vector4(Settings.GalaxyGenerationParameters.Offset.x, Settings.GalaxyGenerationParameters.Offset.y, Settings.GalaxyGenerationParameters.Offset.z, 0.0f));
                    Core.SetVector("sizeParams1", new Vector4(Settings.GalaxyGenerationParameters.Radius, Settings.GalaxyGenerationParameters.RadiusEllipse, Settings.GalaxyGenerationParameters.SizeBar, Settings.GalaxyGenerationParameters.Depth));
                    Core.SetVector("warpParams1", Settings.GalaxyGenerationParameters.Warp);
                    Core.SetVector("spiralParams1", new Vector4(Settings.GalaxyGenerationParameters.InverseSpiralEccentricity, Settings.GalaxyGenerationParameters.SpiralRotation, perPassRotation, 0.0f));
                    Core.SetVector("temperatureParams1", Settings.GalaxyGenerationParameters.TemperatureRange);

                    Core.SetBuffer(kernel, "particles_output", inputBuffer);
                    Core.Dispatch(kernel, tgX, tgY, tgZ);
                }
            }
        }

        public void ExecuteParticleFilter(int kernel, ref List<List<ComputeBuffer>> input, ref List<List<ComputeBuffer>> output, int tgX, int tgY, int tgZ)
        {
            for (byte generationType = 0; generationType < output.Capacity; generationType++)
            {
                var inputBuffers = input[generationType];
                var outputBuffers = output[generationType];

                for (var bufferIndex = 0; bufferIndex < inputBuffers.Capacity; bufferIndex++)
                {
                    var inputBuffer = inputBuffers[bufferIndex];
                    var outputBuffer = outputBuffers[bufferIndex];

                    Core.SetBuffer(kernel, "filter_input", inputBuffer);
                    Core.SetBuffer(kernel, "filter_output", outputBuffer);
                    Core.Dispatch(kernel, tgX, tgY, tgZ);
                }
            }
        }

        public void InitParticleBuffers<T>(ref List<List<ComputeBuffer>> input, int width, int height, ComputeBufferType type = ComputeBufferType.Default) where T : struct
        {
            for (byte generationType = 0; generationType < input.Capacity; generationType++)
            {
                var inputBuffers = new List<ComputeBuffer>(width);

                for (var bufferIndex = 0; bufferIndex < inputBuffers.Capacity; bufferIndex++)
                {
                    var inputBuffer = new ComputeBuffer(height, Marshal.SizeOf<T>(), type);

                    inputBuffer.SetData(new T[height]);

                    inputBuffers.Add(inputBuffer);
                }

                input.Add(inputBuffers);
            }
        }

        public void DestroyParticleBuffers(ref List<List<ComputeBuffer>> input)
        {
            for (byte generationType = 0; generationType < input.Capacity; generationType++)
            {
                var inputBuffers = input[generationType];

                for (var bufferIndex = 0; bufferIndex < inputBuffers.Capacity; bufferIndex++)
                {
                    var inputBuffer = inputBuffers[bufferIndex];

                    inputBuffer.ReleaseAndDisposeBuffer();
                }

                inputBuffers.Clear();
            }

            input.Clear();
        }

        #endregion

        #region Luts

        public void GenerateLuts()
        {
            Settings.GalaxyGenerationParameters.StarsColorDistribution.GenerateLut();
            Settings.GalaxyGenerationParameters.DustColorDistribution.GenerateLut();
            Settings.GalaxyRenderingParameters.DustColorDistribution.GenerateLut();
        }

        public void DestroyLuts()
        {
            Settings.GalaxyGenerationParameters.StarsColorDistribution.DestroyLut();
            Settings.GalaxyGenerationParameters.DustColorDistribution.DestroyLut();
            Settings.GalaxyRenderingParameters.DustColorDistribution.DestroyLut();
        }

        #endregion

        #region Octree

        public void InitOctree()
        {
            Octree = new PointOctree<GalaxyStar>(512, Vector3.zero, 4);
        }

        [ContextMenu("Generate Octree")]
        public void GenerateOctree()
        {
            // NOTE : WIP
            // TODO : Regenerate octree on buffers change...
            if (Octree == null) return;

            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];
                    var data = new GalaxyParticle[buffer.count];

                    buffer.GetData(data);

                    for (var starIndex = 0; starIndex < data.Length; starIndex++)
                    {
                        var star = data[starIndex];
                        var starPosition = star.position;
                        var starSize = star.size;

                        Octree.Add(new GalaxyStar(starPosition, starSize), starPosition);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Node

        protected override void InitNode()
        {
            if (StarsShader == null) StarsShader = Shader.Find("SpaceEngine/Galaxy/Star");
            StarsMaterial = MaterialHelper.CreateTemp(StarsShader, "Galaxy Stars");

            if (DustShader == null) DustShader = Shader.Find("SpaceEngine/Galaxy/Dust");
            if (SpriteShader == null) SpriteShader = Shader.Find("SpaceEngine/Galaxy/Sprite");

            InitDustMaterials();

            if (ScreenShader == null) ScreenShader = Shader.Find("SpaceEngine/Galaxy/Screen");
            ScreenMaterial = MaterialHelper.CreateTemp(ScreenShader, "Galaxy Screen Compose");

            if (StarParticle == null)
            {
                Debug.LogWarning("GalaxyTest.InitNode: StarParticle texture is null! Trying to load from Resources the default one! Impossible to render stars, if fail!");

                StarParticle = Resources.Load("Textures/Galaxy/StarParticle2", typeof(Texture2D)) as Texture2D;
            }

            if (GasParticle == null)
            {
                Debug.LogWarning("GalaxyTest.InitNode: GasParticle texture is null! Trying to load from Resources the default one! Impossible to render gas sprites, if fail!");

                GasParticle = Resources.Load("Textures/Galaxy/StarParticle1", typeof(Texture2D)) as Texture2D;
            }

            if (VolumeMesh == null) Debug.LogWarning("GalaxyTest.InitNode: VolumeMesh is null! Impossible to render volumetric stuff!");

            if (ScreenMesh == null)
            {
                ScreenMesh = MeshFactory.MakePlane(8, MeshFactory.PLANE.XY, false, false);
                ScreenMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
            }

            var hightResolution = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
            var lowResolution = new Vector2(Screen.width / 4.0f, Screen.height / 4.0f);

            FrameBuffer1 = RTExtensions.CreateRTexture(hightResolution, 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp);
            FrameBuffer2 = RTExtensions.CreateRTexture(lowResolution, 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp);

            DustCommandBuffer = new CommandBuffer();
            DustCommandBuffer.name = "Galaxy Dust Rendering";

            GenerateLuts();

            InitBuffers();
            GenerateBuffers();

            InitOctree();
            GenerateOctree();
        }

        protected override void UpdateNode()
        {
            if (AutoUpdate) GenerateBuffers();
            if (AutoLutUpdate) GenerateLuts();

            var diameter = Settings.GalaxyGenerationParameters.Radius * 2.0f;
            var plane = new Plane(Vector3.up, diameter);
            var distance = plane.GetDistanceToPoint(CameraHelper.Main().transform.position);
            var screenSize = DistanceAndDiameterToPixelSize(distance, diameter);

            BlendFactor = screenSize / (diameter * 2.0f);
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
            DestroyLuts();

            if (FrameBuffer1 != null) FrameBuffer1.ReleaseAndDestroy();
            if (FrameBuffer2 != null) FrameBuffer2.ReleaseAndDestroy();

            if (DustCommandBuffer != null) DustCommandBuffer.Release();

            Helper.Destroy(StarsMaterial);
            Helper.Destroy(ScreenMaterial);

            Helper.Destroy(ScreenMesh);
        }

        #endregion

        private float DistanceAndDiameterToPixelSize(float distance, float diameter)
        {
            return (diameter * Mathf.Rad2Deg * Screen.height) / (distance * CameraHelper.Main().fieldOfView);
        }

        public static float NextGaussianDouble(float variance)
        {
            var rand1 = -2.0f * Mathf.Log(Random.value);
            var rand2 = Random.value * (Mathf.PI * 2.0f);

            return Mathf.Sqrt(variance * rand1) * Mathf.Cos(rand2);
        }

        private Vector3 GetGaussianPosition(float variance, float size)
        {
            return new Vector3(NextGaussianDouble(variance), NextGaussianDouble(variance), NextGaussianDouble(variance)) * size;
        }

        #region IRenderable

        public virtual void Render(int layer = 8)
        {           
            // NOTE : Nothing to draw...
        }

        public void RenderAppendBuffers(ref List<List<ComputeBuffer>> collection, ref ComputeBuffer argsBuffer, int pass)
        {
            var args = new uint[] { 0, 1, 0, 0, 0 };

            argsBuffer.SetData(args);

            StarsMaterial.SetTexture("_Particle", StarParticle);
            StarsMaterial.SetFloat("_Particle_Absolute_Size", Settings.GalaxyRenderingParameters.StarAbsoluteSize);

            for (byte generationType = 0; generationType < collection.Capacity; generationType++)
            {
                var buffers = collection[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    StarsMaterial.SetPass(pass);

                    StarsMaterial.SetBuffer("data", buffer);

                    ComputeBuffer.CopyCount(buffer, argsBuffer, 0);

                    Graphics.DrawProceduralIndirect(MeshTopology.Points, argsBuffer);
                }
            }
        }

        public void RenderAppendDust(int pass)
        {
            RenderAppendBuffers(ref DustAppendBuffers, ref DustArgsBuffer, pass);
        }

        public void RenderAppendGas(int pass)
        {
            RenderAppendBuffers(ref GasAppendBuffers, ref GasArgsBuffer, pass);
        }

        public void RenderBuffers(ref List<List<ComputeBuffer>> collection, int pass, int count)
        {
            // TODO : Render fake stars maybe?

            StarsMaterial.SetTexture("_Particle", StarParticle);
            StarsMaterial.SetFloat("_Particle_Absolute_Size", Settings.GalaxyRenderingParameters.StarAbsoluteSize);

            for (byte generationType = 0; generationType < collection.Capacity; generationType++)
            {
                var buffers = collection[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    StarsMaterial.SetPass(pass);

                    StarsMaterial.SetBuffer("data", buffer);

                    Graphics.DrawProcedural(MeshTopology.Points, count);
                }
            }
        }

        public void RenderStars(int pass)
        {
            // NOTE : Pass number is hardcoded to draw anyway in debug mode.
            if (BlendFactor < 1.0 && pass != 1) return;

            RenderBuffers(ref StarsBuffers, pass, Settings.GalaxyParameters.Count);
        }

        public void RenderDebugStars()
        {
            RenderStars(1);
        }

        public void RenderDustDebug()
        {
            // NOTE : Pass number is hardcoded to draw anyway in debug mode.
            if (BlendFactor < 1.0) return;

            RenderBuffers(ref DustBuffers, 1, Settings.GalaxyParameters.Count);
        }

        public void RenderDustToScreenBuffer()
        {
            if (ScreenMesh == null) return;

            ScreenMaterial.SetTexture("_FrameBuffer", BlendFactor < 1.0f ? FrameBuffer1 : FrameBuffer2);

            ScreenMaterial.SetPass(0);

            Graphics.DrawMeshNow(ScreenMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), 8);
        }

        public void RenderDustToFrameBuffer()
        {
            if (VolumeMesh == null) return;

            DustCommandBuffer.Clear();

            if (BlendFactor < 1.0f)
            {
                DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                DustCommandBuffer.ClearRenderTarget(true, true, Color.black);
            }
            else
            {
                DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                DustCommandBuffer.ClearRenderTarget(true, true, Color.black);
            }

            // 0 - Index count per instance...
            // 1 - Instance count...
            // 2 - Start index location...
            // 3 - Base vertex location...
            // 4 - Start instance location...
            var dustArgs = new uint[] { (uint)VolumeMesh.GetIndexCount(0), 1, 0, 0, 0 };
            var gasArgs = new uint[] { (uint)VolumeMesh.GetIndexCount(0), 1, 0, 0, 0 };
            var spriteArgs = new uint[] { 0, 1, 0, 0, 0 };

            DustArgsBuffer.SetData(dustArgs);
            GasArgsBuffer.SetData(gasArgs);
            SpriteArgsBuffer.SetData(spriteArgs);

            var dustParams1 = new Vector2(Settings.GalaxyRenderingParameters.DustStrength, Settings.GalaxyRenderingParameters.DustSize);
            var gasParams1 = new Vector2(Settings.GalaxyRenderingParameters.GasStength, Settings.GalaxyRenderingParameters.GasSize);
            var galaxyPosition = transform.position - GodManager.Instance.View.WorldCameraPosition;

            var trs = Matrix4x4.identity;

            for (byte generationType = 0; generationType < DustBuffers.Capacity; generationType++)
            {
                var dustBuffers = DustAppendBuffers[generationType];
                var gasBuffers = GasAppendBuffers[generationType];
                var material = DustMaterials[generationType];
                var spriteMaterial = DustSpriteMaterials[generationType];

                material.SetVector("dustParams1", dustParams1);
                material.SetVector("gasParams1", gasParams1);
                material.SetTexture("ColorDistributionTable", Settings.GalaxyRenderingParameters.DustColorDistribution.Lut);
                material.SetVector("_Galaxy_Position", galaxyPosition);

                // NOTE : Render galaxy dust...
                for (var bufferIndex = 0; bufferIndex < Mathf.Min(Settings.GalaxyRenderingParameters.DustPassCount, dustBuffers.Capacity); bufferIndex++)
                {
                    var dustBuffer = dustBuffers[bufferIndex];

                    material.SetBuffer("dust", dustBuffer);

                    ComputeBuffer.CopyCount(dustBuffer, DustArgsBuffer, 4);

                    if (BlendFactor < 1.0f)
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 0, DustArgsBuffer);
                    }
                    else
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 0, DustArgsBuffer);
                    }
                }

                // NOTE : Render galaxy gas...
                for (var bufferIndex = 0; bufferIndex < Mathf.Min(Settings.GalaxyRenderingParameters.DustPassCount, gasBuffers.Capacity); bufferIndex++)
                {
                    var gasBuffer = gasBuffers[bufferIndex];

                    material.SetBuffer("gas", gasBuffer);

                    ComputeBuffer.CopyCount(gasBuffer, GasArgsBuffer, 4);

                    if (BlendFactor < 1.0f)
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 1, GasArgsBuffer);
                    }
                    else
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 1, GasArgsBuffer);
                    }
                }

                spriteMaterial.SetTexture("_Particle", GasParticle);
                spriteMaterial.SetFloat("_Particle_Absolute_Size", 1.0f);
                spriteMaterial.SetVector("dustParams1", dustParams1);
                spriteMaterial.SetVector("gasParams1", gasParams1);
                spriteMaterial.SetTexture("ColorDistributionTable", Settings.GalaxyRenderingParameters.DustColorDistribution.Lut);
                spriteMaterial.SetVector("_Galaxy_Position", galaxyPosition);

                for (var bufferIndex = 0; bufferIndex < Mathf.Min(Settings.GalaxyRenderingParameters.DustPassCount, gasBuffers.Capacity); bufferIndex++)
                {
                    var gasBuffer = gasBuffers[bufferIndex];

                    spriteMaterial.SetBuffer("data", gasBuffer);

                    ComputeBuffer.CopyCount(gasBuffer, SpriteArgsBuffer, 0);

                    if (BlendFactor < 1.0f)
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                        DustCommandBuffer.DrawProceduralIndirect(trs, spriteMaterial, 0, MeshTopology.Points, SpriteArgsBuffer);
                    }
                    else
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                        DustCommandBuffer.DrawProceduralIndirect(trs, spriteMaterial, 0, MeshTopology.Points, SpriteArgsBuffer);
                    }
                }
            }

            Graphics.ExecuteCommandBuffer(DustCommandBuffer);
        }

        #endregion
    }
}