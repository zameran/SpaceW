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
    public struct GalaxyStar
    {
        public Vector3 position;
        public Vector4 color;
        public float size;
        public float temperature;
    }

    [Serializable]
    public class GalaxyRenderStar
    {
        public Vector3 Position { get; }

        public GalaxyRenderStar()
        {
            Position = Vector3.zero;
        }

        public GalaxyRenderStar(Vector3 position)
        {
            Position = position;
        }

        #region Overrides

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("({0})", Position);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var item = obj as GalaxyRenderStar;

            if (item == null) { return false; }

            return Position.Equals(item.Position);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        #endregion
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

        public ColorMaterialTableGradientLut StarsColorDistribution;
        public ColorMaterialTableGradientLut DustColorDistribution;

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

            StarsColorDistribution = new ColorMaterialTableGradientLut();
            DustColorDistribution = new ColorMaterialTableGradientLut();
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

            StarsColorDistribution = new ColorMaterialTableGradientLut();
            DustColorDistribution = new ColorMaterialTableGradientLut();
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
        public int DustCount;
        public int BulgeCount;
        public int Count;

        public GalaxyParameters(GalaxyParameters from)
        {
            PassCount = from.PassCount;
            DustCount = from.DustCount;
            BulgeCount = from.BulgeCount;
            Count = from.Count;
        }

        public GalaxyParameters(int passCount, int dustCount, int count, int bulgeCount)
        {
            PassCount = passCount;
            DustCount = dustCount;
            BulgeCount = bulgeCount;
            Count = count;
        }

        public static GalaxyParameters Default
        {
            get
            {
                return new GalaxyParameters(2, 128000, 128000, 4);
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
        public float GasDrawPercent;

        [Range(0.0f, 1.0f)]
        public float DustDrawPercent;

        [Range(0.0f, 1.0f)]
        public float StarDrawPercent;

        public float StarAbsoluteSize;

        [Range(1.0f, 4.0f)]
        public int DustPassCount;

        public float GasCenterFalloff;
        public float HDRExposure;

        public ColorMaterialTableGradientLut DustColorDistribution;

        public GalaxyRenderingParameters(GalaxyRenderingParameters from)
        {
            DustStrength = from.DustStrength;
            DustSize = from.DustSize;
            GasDrawPercent = from.GasDrawPercent;
            DustDrawPercent = from.DustDrawPercent;
            StarDrawPercent = from.StarDrawPercent;
            StarAbsoluteSize = from.StarAbsoluteSize;

            DustPassCount = from.DustPassCount;

            GasCenterFalloff = from.GasCenterFalloff;
            HDRExposure = from.HDRExposure;

            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public GalaxyRenderingParameters(float dustStrength, float dustSize, float gasDrawPercent, float dustDrawPercent, float starDrawPercent, float starAbsoluteSize, int dustPassCount, float gasCenterFalloff, float hdrExposure)
        {
            DustStrength = dustStrength;
            DustSize = dustSize;
            GasDrawPercent = gasDrawPercent;
            DustDrawPercent = dustDrawPercent;
            StarDrawPercent = starDrawPercent;
            StarAbsoluteSize = starAbsoluteSize;

            DustPassCount = dustPassCount;

            GasCenterFalloff = gasCenterFalloff;
            HDRExposure = hdrExposure;

            DustColorDistribution = new ColorMaterialTableGradientLut();
        }

        public static GalaxyRenderingParameters Default
        {
            get
            {
                return new GalaxyRenderingParameters(0.0075f, 1.0f, 0.1f, 0.1f, 1.0f, 64.0f, 1, 16.0f, 1.2f);
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

        public Texture2D StarParticle;

        private Material StarsMaterial;
        private Material ScreenMaterial;

        private List<List<ComputeBuffer>> StarsBuffers = new List<List<ComputeBuffer>>();
        private List<List<ComputeBuffer>> DustBuffers = new List<List<ComputeBuffer>>();
        private List<ComputeBuffer> BulgeBuffers = new List<ComputeBuffer>();
        private List<Material> DustMaterials = new List<Material>();
        private ComputeBuffer DustArgsBuffer;
        private ComputeBuffer GasArgsBuffer;
        private ComputeBuffer BulgeArgsBuffer;

        private CommandBuffer DustCommandBuffer;

        public GalaxySettings Settings = GalaxySettings.Default;

        public Mesh VolumeMesh = null;

        [HideInInspector]
        public Mesh ScreenMesh = null;

        public bool AutoUpdate = false;

        // TODO : Maybe bigger data type? Leave it, if you are ok to overflows :)
        public int StarDrawCount { get { return (int)(Settings.GalaxyParameters.Count * Settings.GalaxyRenderingParameters.StarDrawPercent); } }
        public int DustDrawCount { get { return (int)(Settings.GalaxyParameters.DustCount * Settings.GalaxyRenderingParameters.DustDrawPercent); } }
        public int GasDrawCount { get { return (int)(Settings.GalaxyParameters.DustCount * Settings.GalaxyRenderingParameters.GasDrawPercent); } }
        public int StarDrawCountInversed { get { return (int)(Settings.GalaxyParameters.Count * (1.0f - Settings.GalaxyRenderingParameters.StarDrawPercent)); } }
        public int DustDrawCountInversed { get { return (int)(Settings.GalaxyParameters.DustCount * (1.0f - Settings.GalaxyRenderingParameters.DustDrawPercent)); } }
        public int GasDrawCountInversed { get { return (int)(Settings.GalaxyParameters.DustCount * (1.0f - Settings.GalaxyRenderingParameters.GasDrawPercent)); } }

        private RenderTexture FrameBuffer1;
        private RenderTexture FrameBuffer2;

        private float BlendFactor = 0.0f;

        public PointOctree<GalaxyRenderStar> Octree;

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

        #region Particles

        private void GenerateParticles(ParticleSystem system, ComputeBuffer source)
        {
            if (system == null) return;

            // TODO : Finish with particles...

            var rendererModule = system.GetComponent<Renderer>();

            var buffer = source;
            var bufferSize = buffer.count;
            var stars = new GalaxyStar[bufferSize];
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
            if (StarsBuffers != null || DustBuffers != null || BulgeBuffers != null || DustArgsBuffer != null || GasArgsBuffer != null || BulgeArgsBuffer != null)
            {
                DestroyBuffers();
            }

            StarsBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);
            DustBuffers = new List<List<ComputeBuffer>>((int)Settings.Type);
            BulgeBuffers = new List<ComputeBuffer>(1);

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

            for (byte generationType = 0; generationType < DustBuffers.Capacity; generationType++)
            {
                var buffers = new List<ComputeBuffer>(Settings.GalaxyParameters.PassCount);

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = new ComputeBuffer(Settings.GalaxyParameters.DustCount, Marshal.SizeOf<GalaxyStar>(), ComputeBufferType.Default);

                    buffer.SetData(new GalaxyStar[Settings.GalaxyParameters.DustCount]);

                    buffers.Add(buffer);
                }

                DustBuffers.Add(buffers);
            }

            for (var bufferIndex = 0; bufferIndex < BulgeBuffers.Capacity; bufferIndex++)
            {
                var buffer = new ComputeBuffer(Settings.GalaxyParameters.BulgeCount, Marshal.SizeOf<GalaxyStar>(), ComputeBufferType.Default);

                buffer.SetData(new GalaxyStar[Settings.GalaxyParameters.BulgeCount]);

                BulgeBuffers.Add(buffer);
            }

            DustArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            GasArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            BulgeArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        public void GenerateBuffers()
        {
            // TODO : Moar variation of stars in galactic center...

            var starsKernel = Core.FindKernel("Stars");
            var dustKernel = Core.FindKernel("Dust");

            Core.SetTexture(starsKernel, "ColorDistributionTable", Settings.GalaxyGenerationParameters.StarsColorDistribution.Lut);
            Core.SetTexture(dustKernel, "ColorDistributionTable", Settings.GalaxyGenerationParameters.DustColorDistribution.Lut);

            for (byte generationType = 0; generationType < StarsBuffers.Capacity; generationType++)
            {
                var buffers = StarsBuffers[generationType];
                var perPassRotation = (generationType % 2 == 0 ? Settings.GalaxyGenerationPerPassParameters.PassRotation * generationType :
                                                                 Settings.GalaxyGenerationPerPassParameters.PassRotation);

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    Core.SetVector("randomParams1", (Settings.GalaxyGenerationParameters.Randomize + new Vector3(1.0f, 0.0f, 1.0f)) * ((bufferIndex + 1 + generationType + 1) / 10.0f));
                    Core.SetVector("offsetParams1", new Vector4(Settings.GalaxyGenerationParameters.Offset.x, Settings.GalaxyGenerationParameters.Offset.y, Settings.GalaxyGenerationParameters.Offset.z, 0.0f));
                    Core.SetVector("sizeParams1", new Vector4(Settings.GalaxyGenerationParameters.Radius, Settings.GalaxyGenerationParameters.RadiusEllipse, Settings.GalaxyGenerationParameters.SizeBar, Settings.GalaxyGenerationParameters.Depth));
                    Core.SetVector("warpParams1", Settings.GalaxyGenerationParameters.Warp);
                    Core.SetVector("spiralParams1", new Vector4(Settings.GalaxyGenerationParameters.InverseSpiralEccentricity, Settings.GalaxyGenerationParameters.SpiralRotation, perPassRotation, 0.0f));

                    Core.SetBuffer(starsKernel, "stars_output", buffer);
                    Core.Dispatch(starsKernel, (int)(Settings.GalaxyParameters.Count / 1024.0f), 1, 1);
                }
            }

            for (byte generationType = 0; generationType < DustBuffers.Capacity; generationType++)
            {
                var buffers = DustBuffers[generationType];
                var perPassRotation = (generationType % 2 == 0 ? Settings.GalaxyGenerationPerPassParameters.PassRotation * generationType :
                                                                 Settings.GalaxyGenerationPerPassParameters.PassRotation);

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    Core.SetVector("randomParams1", (Settings.GalaxyGenerationParameters.Randomize + new Vector3(1.0f, 0.0f, 1.0f)) * ((bufferIndex + 1 + generationType + 1) / 10.0f));
                    Core.SetVector("offsetParams1", new Vector4(Settings.GalaxyGenerationParameters.Offset.x, Settings.GalaxyGenerationParameters.Offset.y, Settings.GalaxyGenerationParameters.Offset.z, 0.0f));
                    Core.SetVector("sizeParams1", new Vector4(Settings.GalaxyGenerationParameters.Radius, Settings.GalaxyGenerationParameters.RadiusEllipse, Settings.GalaxyGenerationParameters.SizeBar, Settings.GalaxyGenerationParameters.Depth));
                    Core.SetVector("warpParams1", Settings.GalaxyGenerationParameters.Warp);
                    Core.SetVector("spiralParams1", new Vector4(Settings.GalaxyGenerationParameters.InverseSpiralEccentricity, Settings.GalaxyGenerationParameters.SpiralRotation, perPassRotation, 0.0f));

                    Core.SetBuffer(dustKernel, "dust_output", buffer);
                    Core.Dispatch(dustKernel, (int)(Settings.GalaxyParameters.DustCount / 1024.0f), 1, 1);
                }
            }

            for (var bufferIndex = 0; bufferIndex < BulgeBuffers.Capacity; bufferIndex++)
            {
                var buffer = BulgeBuffers[bufferIndex];
                var bulgeObjects = new GalaxyStar[Settings.GalaxyParameters.BulgeCount];
                var gradient = Settings.GalaxyRenderingParameters.DustColorDistribution.Gradient;

                for (byte bulgeIndex = 0; bulgeIndex < Settings.GalaxyParameters.BulgeCount; bulgeIndex++)
                {
                    //Random.InitState(bulgeIndex);
                    //var gaussianPosition = GetGaussianPosition(0.00025f, Settings.GalaxyGenerationParameters.Radius);

                    var gaussianPosition = Vector3.zero;
                    var t = gaussianPosition.sqrMagnitude / (Settings.GalaxyParameters.BulgeCount * 10);
                    var colorMultiplier = 8 / ((Settings.GalaxyParameters.BulgeCount - bulgeIndex) + 1);

                    bulgeObjects[bulgeIndex] = new GalaxyStar();
                    bulgeObjects[bulgeIndex].position = gaussianPosition;
                    bulgeObjects[bulgeIndex].color = gradient.Evaluate(t) * colorMultiplier;
                    bulgeObjects[bulgeIndex].size = Random.value + 0.75f * 32.0f;
                    bulgeObjects[bulgeIndex].temperature = 10000;
                }

                buffer.SetData(bulgeObjects);
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

            for (byte generationType = 0; generationType < DustBuffers.Capacity; generationType++)
            {
                var buffers = DustBuffers[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    buffer.ReleaseAndDisposeBuffer();
                }

                buffers.Clear();
            }

            DustBuffers.Clear();

            for (var bufferIndex = 0; bufferIndex < BulgeBuffers.Capacity; bufferIndex++)
            {
                var buffer = BulgeBuffers[bufferIndex];

                buffer.ReleaseAndDisposeBuffer();
            }

            BulgeBuffers.Clear();

            DustArgsBuffer.ReleaseAndDisposeBuffer();
            GasArgsBuffer.ReleaseAndDisposeBuffer();
            BulgeArgsBuffer.ReleaseAndDisposeBuffer();
        }

        #endregion

        #region Octree

        public void InitOctree()
        {
            Octree = new PointOctree<GalaxyRenderStar>(512, Vector3.zero, 4);
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
                    var data = new GalaxyStar[buffer.count];

                    buffer.GetData(data);

                    for (var starIndex = 0; starIndex < data.Length; starIndex++)
                    {
                        var star = data[starIndex];
                        var starPosition = star.position;

                        Octree.Add(new GalaxyRenderStar(starPosition), starPosition);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Node

        private void OnDrawGizmos()
        {
            if (Octree == null) return;

            Octree.DrawAllBounds();
        }

        protected override void InitNode()
        {
            if (StarsShader == null) StarsShader = Shader.Find("SpaceEngine/Galaxy/StarTest");
            StarsMaterial = MaterialHelper.CreateTemp(StarsShader, "Galaxy Stars");

            if (DustShader == null) DustShader = Shader.Find("SpaceEngine/Galaxy/DustTest");
            InitDustMaterials();

            if (ScreenShader == null) ScreenShader = Shader.Find("SpaceEngine/Galaxy/ScreenCompose");
            ScreenMaterial = MaterialHelper.CreateTemp(ScreenShader, "Galaxy Screen Compose");

            if (StarParticle == null)
            {
                Debug.LogWarning("GalaxyTest.InitNode: StarParticle texture is null! Trying to load from Resources the default one! Impossible to render stars, if fail!");

                StarParticle = Resources.Load("Textures/Galaxy/StarParticle", typeof(Texture2D)) as Texture2D;
            }

            if (VolumeMesh == null) Debug.LogWarning("GalaxyTest.InitNode: VolumeMesh is null! Impossible to render volumetric stuff!");

            if (ScreenMesh == null)
            {
                ScreenMesh = MeshFactory.MakePlane(8, MeshFactory.PLANE.XY, false, false);
                ScreenMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
            }

            Settings.GalaxyGenerationParameters.StarsColorDistribution.GenerateLut();
            Settings.GalaxyGenerationParameters.DustColorDistribution.GenerateLut();
            Settings.GalaxyRenderingParameters.DustColorDistribution.GenerateLut();

            FrameBuffer1 = RTExtensions.CreateRTexture(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp);
            FrameBuffer2 = RTExtensions.CreateRTexture(new Vector2(Screen.width / 4.0f, Screen.height / 4.0f), 0, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, TextureWrapMode.Clamp);

            DustCommandBuffer = new CommandBuffer();
            DustCommandBuffer.name = "Galaxy Dust Rendering";

            InitBuffers();
            GenerateBuffers();

            InitOctree();
            GenerateOctree();
        }

        protected override void UpdateNode()
        {
            if (AutoUpdate) GenerateBuffers();

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

            if (FrameBuffer1 != null) FrameBuffer1.ReleaseAndDestroy();
            if (FrameBuffer2 != null) FrameBuffer2.ReleaseAndDestroy();

            if (DustCommandBuffer != null) DustCommandBuffer.Release();

            Helper.Destroy(StarsMaterial);
            Helper.Destroy(ScreenMaterial);

            Helper.Destroy(ScreenMesh);

            Settings.GalaxyGenerationParameters.StarsColorDistribution.DestroyLut();
            Settings.GalaxyGenerationParameters.DustColorDistribution.DestroyLut();
            Settings.GalaxyRenderingParameters.DustColorDistribution.DestroyLut();
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

        public void RenderBuffers(List<List<ComputeBuffer>> collection, int pass, int count)
        {
            for (byte generationType = 0; generationType < collection.Capacity; generationType++)
            {
                var buffers = collection[generationType];

                for (var bufferIndex = 0; bufferIndex < buffers.Capacity; bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    StarsMaterial.SetPass(pass);

                    StarsMaterial.SetBuffer("data", buffer);

                    StarsMaterial.SetTexture("_Particle", StarParticle);
                    StarsMaterial.SetFloat("_Particle_Absolute_Size", Settings.GalaxyRenderingParameters.StarAbsoluteSize);

                    StarsMaterial.SetFloat("_HDRExposure", Settings.GalaxyRenderingParameters.HDRExposure);
                    StarsMaterial.SetFloat("_HDRMode", (int)GodManager.Instance.HDRMode); // NOTE : Maybe own HDR mode? I don't know...

                    Graphics.DrawProcedural(MeshTopology.Points, count);
                }
            }
        }

        public void RenderStars(int pass)
        {
            RenderBuffers(StarsBuffers, pass, StarDrawCount);
        }

        public void RenderDustPoints()
        {
            RenderBuffers(DustBuffers, 1, DustDrawCount);
        }

        public void RenderDustToScreenBuffer()
        {
            if (ScreenMesh == null) return;

            ScreenMaterial.SetTexture("_FrameBuffer1", FrameBuffer1);
            ScreenMaterial.SetTexture("_FrameBuffer2", FrameBuffer2);
            ScreenMaterial.SetFloat("_Mix", BlendFactor);
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

            DustCommandBuffer.SetRenderTarget(FrameBuffer2);
            DustCommandBuffer.ClearRenderTarget(true, true, Color.black);

            var dustArgs = new uint[5];
            dustArgs[0] = (uint)VolumeMesh.GetIndexCount(0);            // Index count per instance...
            dustArgs[1] = (uint)DustDrawCount;                          // Instance count...
            dustArgs[2] = 0;                                            // Start index location...
            dustArgs[3] = 0;                                            // Base vertex location...
            dustArgs[4] = 0;                                            // Start instance location...

            var gasArgs = new uint[5];
            gasArgs[0] = (uint)VolumeMesh.GetIndexCount(0);
            gasArgs[1] = (uint)GasDrawCount;
            gasArgs[2] = 0;
            gasArgs[3] = 0;
            gasArgs[4] = 0;

            var bulgeArgs = new uint[5];
            bulgeArgs[0] = (uint)VolumeMesh.GetIndexCount(0);
            bulgeArgs[1] = (uint)Settings.GalaxyParameters.BulgeCount;
            bulgeArgs[2] = 0;
            bulgeArgs[3] = 0;
            bulgeArgs[4] = 0;

            DustArgsBuffer.SetData(dustArgs);
            GasArgsBuffer.SetData(gasArgs);
            BulgeArgsBuffer.SetData(bulgeArgs);

            // TODO : Calculate _Galaxy_Orientation and _Galaxy_OrientationInverse relative to camera, for a better visualization...
            // TODO : Better blending handling...
            var galaxyOrientation = new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
            var galaxyOrientationInversed = -galaxyOrientation;

            for (byte generationType = 0; generationType < DustBuffers.Capacity; generationType++)
            {
                var buffers = DustBuffers[generationType];
                var material = DustMaterials[generationType];

                material.SetVector("dustParams1", new Vector2(Settings.GalaxyRenderingParameters.DustStrength, Settings.GalaxyRenderingParameters.DustSize));
                material.SetVector("gasParams1", new Vector2(Settings.GalaxyRenderingParameters.GasCenterFalloff, 0.0f));
                material.SetTexture("ColorDistributionTable", Settings.GalaxyRenderingParameters.DustColorDistribution.Lut);
                material.SetVector("_Galaxy_Position", transform.position - GodManager.Instance.View.WorldCameraPosition);
                material.SetVector("_Galaxy_Orientation", galaxyOrientation);
                material.SetVector("_Galaxy_OrientationInverse", galaxyOrientationInversed);

                // NOTE : Render galaxy bulge dust
                for (var bufferIndex = 0; bufferIndex < BulgeBuffers.Capacity; bufferIndex++)
                {
                    var buffer = BulgeBuffers[bufferIndex];

                    material.SetBuffer("bulge", buffer);

                    if (BlendFactor < 1.0f)
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 2, BulgeArgsBuffer);
                    }

                    DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                    DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 2, BulgeArgsBuffer);
                }

                // NOTE : Render galaxy dust and gas...
                for (var bufferIndex = 0; bufferIndex < Mathf.Min(Settings.GalaxyRenderingParameters.DustPassCount, buffers.Capacity); bufferIndex++)
                {
                    var buffer = buffers[bufferIndex];

                    material.SetBuffer("stars", buffer);

                    if (BlendFactor < 1.0f)
                    {
                        DustCommandBuffer.SetRenderTarget(FrameBuffer1);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 0, DustArgsBuffer);
                        DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 1, GasArgsBuffer);
                    }

                    DustCommandBuffer.SetRenderTarget(FrameBuffer2);
                    DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 0, DustArgsBuffer);
                    DustCommandBuffer.DrawMeshInstancedIndirect(VolumeMesh, 0, material, 1, GasArgsBuffer);
                }
            }

            Graphics.ExecuteCommandBuffer(DustCommandBuffer);
        }

        #endregion
    }
}