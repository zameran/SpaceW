#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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
using SpaceEngine.Tools;
using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUIHardwareInfo : DebugGUI
    {
        protected override void Awake()
        {
            base.Awake();

            HardwareInfo.Get();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Hardware Info");
        }

        protected override void UI(int id)
        {
            GUILayoutExtensions.VerticalBoxed("Controls: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    if (GUILayout.Button("Update")) HardwareInfo.Get();
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            GUILayoutExtensions.VerticalBoxed("Overall summary: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    GUILayoutExtensions.LabelWithSpace($"Device Type: {HardwareInfo.deviceType}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Operation System: {HardwareInfo.operatingSystem}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Unity Version: {HardwareInfo.unityVersion}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Graphics Device: {HardwareInfo.graphicsDeviceName}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Graphics Device API: {HardwareInfo.graphicsDeviceVersion}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Graphics Device ID: {HardwareInfo.graphicsDeviceID}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Graphics Memory Size: {HardwareInfo.graphicsMemorySize}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supported Shader Level: {HardwareInfo.graphicsShaderLevel}", -8);

                    GUILayoutExtensions.LabelWithSpace($"CPU: {HardwareInfo.processorType}", -8);
                    GUILayoutExtensions.LabelWithSpace($"CPU Cores Count (Threads Count): {HardwareInfo.processorCount}", -8);
                    GUILayoutExtensions.LabelWithSpace($"CPU Current Frequency: {HardwareInfo.processorFrequency}Hz", -8);

                    GUILayoutExtensions.LabelWithSpace($"RAM: {HardwareInfo.systemMemorySize}", -8);

                    GUILayoutExtensions.LabelWithSpace($"Maximum Texture Size: {HardwareInfo.maxTextureSize}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Non-Power-Of-Two Texture Support: {HardwareInfo.npotSupport}", -8);

                    GUILayoutExtensions.LabelWithSpace($"RenderTextures: {true}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Graphics Multithreading: {HardwareInfo.graphicsMultiThreaded}", -8);

                    GUILayoutExtensions.LabelWithSpace($"Supports ComputeShaders: {HardwareInfo.supportsComputeShaders}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports 3DTextures: {HardwareInfo.supports3DTextures}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports 2DArrayTextures: {HardwareInfo.supports2DArrayTextures}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports 3DRenderTextures: {HardwareInfo.supports3DRenderTextures}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports CubemapArrayTextures: {HardwareInfo.supportsCubemapArrayTextures}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports RawShadowDepthSampling: {HardwareInfo.supportsRawShadowDepthSampling}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports MotionVectors: {HardwareInfo.supportsMotionVectors}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports HardwareQuadTopology: {HardwareInfo.supportsHardwareQuadTopology}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports 32bitsIndexBuffer: {HardwareInfo.supports32bitsIndexBuffer}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports SparseTextures: {HardwareInfo.supportsSparseTextures}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports AsyncCompute: {HardwareInfo.supportsAsyncCompute}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports GPUFence: {HardwareInfo.supportsGPUFence}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports AsyncGPUReadback: {HardwareInfo.supportsAsyncGPUReadback}", -8);
                    
                    GUILayoutExtensions.LabelWithSpace($"Supports Audio: {HardwareInfo.supportsAudio}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports Shadows: {HardwareInfo.supportsShadows}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports Instancing: {HardwareInfo.supportsInstancing}", -8);
                    GUILayoutExtensions.LabelWithSpace($"Supports Graphics Fence: {HardwareInfo.supportsGraphicsFence}", -8);
                    
                    GUILayoutExtensions.SpacingSeparator();
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            GUILayoutExtensions.VerticalBoxed("Render Texture support summary: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    DrawSupportedFormats<RenderTextureFormat>(HardwareInfo.RenderTextureFormats, "RenderTexture");

                    GUILayoutExtensions.SpacingSeparator();
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            GUILayoutExtensions.VerticalBoxed("Texture support summary: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    DrawSupportedFormats<TextureFormat>(HardwareInfo.TextureFormats, "Texture");

                    GUILayoutExtensions.SpacingSeparator();
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            GUILayout.EndScrollView();
        }

        private void DrawSupportedFormats<TFormatType>(Dictionary<TFormatType, HardwareInfo.SupportState> formats, string prefix = "null") where TFormatType : struct, IConvertible
        {
            if (!typeof(TFormatType).IsEnum) { throw new ArgumentException("Only 'enum' types as T allowed!"); }
            if (formats == null || formats.Count == 0)
            {
                GUILayoutExtensions.HorizontalBoxed("", GUISkin, () =>
                {
                    GUILayoutExtensions.LabelWithFlexibleSpace("No Data!", "No Info!");
                });

                return;
            }

            foreach (var kvp in formats)
            {
                var format = kvp.Key;
                var supportState = kvp.Value;

                var tempColor = GUI.color;
                var actualColor = HardwareInfo.SupportStateToColor(supportState);

                GUILayoutExtensions.DrawWithColor(() =>
                {
                    GUILayoutExtensions.HorizontalBoxed("", GUISkin, () =>
                    {
                        GUILayoutExtensions.DrawWithColor(() =>
                        {
                            GUILayoutExtensions.LabelWithFlexibleSpace($"{prefix}.{format}", supportState.ToString());
                        }, tempColor);
                    });
                }, actualColor);
            }
        }
    }

    public static class HardwareInfo
    {
        public enum SupportState
        {
            None,
            Supported,
            Unsupported,
            Obsolete,
        }

        public static string deviceType;
        public static string operatingSystem;
        public static string unityVersion;

        public static string graphicsDeviceName;
        public static string graphicsDeviceVersion;
        public static string graphicsDeviceID;
        public static string graphicsMemorySize;
        public static string graphicsShaderLevel;

        public static string processorType;
        public static string processorCount;
        public static string processorFrequency;

        public static string systemMemorySize;

        public static string maxTextureSize;
        public static string npotSupport;

        public static SupportState supportsComputeShaders;
        public static SupportState supports3DTextures;
        public static SupportState supports2DArrayTextures;
        public static SupportState supports3DRenderTextures;
        public static SupportState supportsCubemapArrayTextures;
        public static SupportState supportsRawShadowDepthSampling;
        public static SupportState supportsMotionVectors;
        public static SupportState supportsHardwareQuadTopology;
        public static SupportState supports32bitsIndexBuffer;
        public static SupportState supportsSparseTextures;
        public static SupportState supportsAsyncCompute;
        public static SupportState supportsGPUFence;
        public static SupportState supportsAsyncGPUReadback;
        public static SupportState supportsAudio;
        public static SupportState supportsShadows;
        public static SupportState supportsInstancing;
        public static SupportState supportsGraphicsFence;

        public static bool graphicsMultiThreaded;
        public static bool usesReversedZBuffer;

        public static Dictionary<RenderTextureFormat, SupportState> RenderTextureFormats;
        public static Dictionary<TextureFormat, SupportState> TextureFormats;

        public static void Get()
        {
            deviceType = SystemInfo.deviceType.ToString();
            operatingSystem = SystemInfo.operatingSystem;
            unityVersion = Application.unityVersion;
            graphicsDeviceName = SystemInfo.graphicsDeviceName;
            graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
            graphicsDeviceID = SystemInfo.graphicsDeviceID.ToString();
            graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
            graphicsShaderLevel = SystemInfo.graphicsShaderLevel.ToString();

            processorType = SystemInfo.processorType;
            processorCount = SystemInfo.processorCount.ToString();
            processorFrequency = SystemInfo.processorFrequency.ToString();

            systemMemorySize = SystemInfo.systemMemorySize.ToString();

            maxTextureSize = SystemInfo.maxTextureSize.ToString();
            npotSupport = SystemInfo.npotSupport.ToString();

            supportsComputeShaders = GetSupportState(SystemInfo.supportsComputeShaders);
            supports3DTextures = GetSupportState(SystemInfo.supports3DTextures);
            supports2DArrayTextures = GetSupportState(SystemInfo.supports2DArrayTextures);
            supports3DRenderTextures = GetSupportState(SystemInfo.supports3DRenderTextures);
            supportsCubemapArrayTextures = GetSupportState(SystemInfo.supportsCubemapArrayTextures);
            supportsRawShadowDepthSampling = GetSupportState(SystemInfo.supportsRawShadowDepthSampling);
            supportsMotionVectors = GetSupportState(SystemInfo.supportsMotionVectors);
            supportsHardwareQuadTopology = GetSupportState(SystemInfo.supportsHardwareQuadTopology);
            supports32bitsIndexBuffer = GetSupportState(SystemInfo.supports32bitsIndexBuffer);
            supportsSparseTextures = GetSupportState(SystemInfo.supportsSparseTextures);
            supportsAsyncCompute = GetSupportState(SystemInfo.supportsAsyncCompute);
            supportsGPUFence = GetSupportState(SystemInfo.supportsGraphicsFence);
            supportsAsyncGPUReadback = GetSupportState(SystemInfo.supportsAsyncGPUReadback);       
            supportsAudio = GetSupportState(SystemInfo.supportsAudio);
            supportsShadows = GetSupportState(SystemInfo.supportsShadows);
            supportsInstancing = GetSupportState(SystemInfo.supportsInstancing);
            supportsGraphicsFence = GetSupportState(SystemInfo.supportsGraphicsFence);
            
            graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded;
            usesReversedZBuffer = SystemInfo.usesReversedZBuffer;

            graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded;

            RenderTextureFormats = new Dictionary<RenderTextureFormat, SupportState>();
            TextureFormats = new Dictionary<TextureFormat, SupportState>();

            var renderTextureFormats = Enum.GetValues(typeof(RenderTextureFormat)).OfType<RenderTextureFormat>().ToList();
            var texuteFormats = Enum.GetValues(typeof(TextureFormat)).OfType<TextureFormat>().ToList();

            foreach (var format in renderTextureFormats)
            {
                var supports = SystemInfo.SupportsRenderTextureFormat(format);

                RenderTextureFormats.Add(format, GetSupportState(supports));
            }

            foreach (var format in texuteFormats)
            {
                // NOTE : Some elements of TextureFormat enum marked with ObsoleteAttribute, but Enum.GetValues returns it all...
                if (GetAttributeOfType<ObsoleteAttribute>(format) == null)
                {
                    var supports = SystemInfo.SupportsTextureFormat(format);
                    
                    // NOTE : Workaround around unity's shit...
                    if ((format == TextureFormat.ASTC_4x4 || 
                         format == TextureFormat.ASTC_5x5 ||
                         format == TextureFormat.ASTC_6x6 ||
                         format == TextureFormat.ASTC_8x8 ||
                         format == TextureFormat.ASTC_10x10 ||
                         format == TextureFormat.ASTC_12x12) && TextureFormats.ContainsKey(format))
                    {
                        
                    }
                    else
                    {
                        TextureFormats.Add(format, GetSupportState(supports));
                    }
                }
                else
                {
                    // NOTE : Double check it for a PVRTC_2BPP_RGB...
                    if (!TextureFormats.ContainsKey(format))
                    {
                        TextureFormats.Add(format, GetSupportState(null));
                    }
                }
            }
        }

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memberInfos = type.GetMember(enumVal.ToString());
            var customAttributes = memberInfos[0].GetCustomAttributes(typeof(T), false);

            return (customAttributes.Length > 0) ? (T)customAttributes[0] : null;
        }

        public static SupportState GetSupportState(bool? supported)
        {
            if (supported == null) return SupportState.Obsolete;

            if (supported.Value)
            {
                return SupportState.Supported;
            }
            else
            {
                return SupportState.Unsupported;
            }
        }

        public static Color SupportStateToColor(SupportState value)
        {
            switch (value)
            {
                case SupportState.None: return Color.black;
                case SupportState.Supported: return Color.green;
                case SupportState.Unsupported: return Color.red;
                case SupportState.Obsolete: return Color.magenta;
                default: return Color.white;
            }
        }
    }
}