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
            if (GUILayout.Button("Update")) HardwareInfo.Get();

            GUILayout.Space(5);

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            GUILayout.BeginVertical();

            GUILayoutExtensions.LabelWithSpace("Device Type: " + HardwareInfo.deviceType, -8);
            GUILayoutExtensions.LabelWithSpace("Operation System: " + HardwareInfo.operatingSystem, -8);
            GUILayoutExtensions.LabelWithSpace("Unity Version: " + HardwareInfo.unityVersion, -8);
            GUILayoutExtensions.LabelWithSpace("Graphics Device: " + HardwareInfo.graphicsDeviceName, -8);
            GUILayoutExtensions.LabelWithSpace("Graphics Device API: " + HardwareInfo.graphicsDeviceVersion, -8);
            GUILayoutExtensions.LabelWithSpace("Graphics Device ID: " + HardwareInfo.graphicsDeviceID, -8);
            GUILayoutExtensions.LabelWithSpace("Graphics Memory Size: " + HardwareInfo.graphicsMemorySize, -8);
            GUILayoutExtensions.LabelWithSpace("Supported Shader Level: " + HardwareInfo.graphicsShaderLevel, -8);

            GUILayoutExtensions.LabelWithSpace("CPU: " + HardwareInfo.processorType, -8);
            GUILayoutExtensions.LabelWithSpace("CPU Cores Count (Threads Count): " + HardwareInfo.processorCount, -8);
            GUILayoutExtensions.LabelWithSpace("CPU Current Frequency: " + HardwareInfo.processorFrequency + "Hz", -8);

            GUILayoutExtensions.LabelWithSpace("RAM: " + HardwareInfo.systemMemorySize, -8);

            GUILayoutExtensions.LabelWithSpace("Maximum Texture Size: " + HardwareInfo.maxTextureSize, -8);
            GUILayoutExtensions.LabelWithSpace("Non-Power-Of-Two Texture Support: " + HardwareInfo.npotSupport, -8);

            GUILayoutExtensions.LabelWithSpace("ComputeShaders: " + HardwareInfo.supportsComputeShaders, -8);
            GUILayoutExtensions.LabelWithSpace("RenderTextures: " + true, -8);
            GUILayoutExtensions.LabelWithSpace("3DTextures: " + HardwareInfo.supports3DTextures, -8);
            GUILayoutExtensions.LabelWithSpace("Graphics Multithreading: " + HardwareInfo.graphicsMultiThreaded, -8);

            DrawSupportedFormats<RenderTextureFormat>(HardwareInfo.RenderTextureFormats, "RenderTexture");
            DrawSupportedFormats<TextureFormat>(HardwareInfo.TextureFormats, "Texture");

            GUILayout.Space(10);

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        private void DrawSupportedFormats<T>(List<T> formats, string prefix = "null") where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Only 'enum' types as T allowed!");
            }

            foreach (var format in formats)
            {
                var supports = false;
                var supportState = "NULL";

                try
                {
                    // NOTE : So, that's why i hate "bruteforce" solutions...
                    if (typeof(T) == typeof(RenderTextureFormat))
                    {
                        var f = (RenderTextureFormat)Enum.ToObject(typeof(RenderTextureFormat), format);

                        supports = SystemInfo.SupportsRenderTextureFormat(f);
                    }
                    else if (typeof(T) == typeof(TextureFormat))
                    {
                        var f = (TextureFormat)Enum.ToObject(typeof(TextureFormat), format);

                        supports = SystemInfo.SupportsTextureFormat(f);
                    }
                    else
                    {
                        throw new NotImplementedException("Unsupported format!");
                    }

                    supportState = HardwareInfo.Supports(supports);
                }
                catch (Exception ex)
                {
                    supports = false;
                    supportState = ex.GetType().Name;
                }

                GUILayoutExtensions.LabelWithSpace(string.Format("{0}.{1}: {2}", prefix, format, supportState), -8);
            }
        }

        private bool Check(Func<bool> assert)
        {
            return assert.Invoke();
        }
    }

    public static class HardwareInfo
    {
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

        public static string supportsComputeShaders;
        public static string supports3DTextures;
        public static string graphicsMultiThreaded;

        public static List<RenderTextureFormat> RenderTextureFormats;
        public static List<TextureFormat> TextureFormats;

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

            supportsComputeShaders = Supports(SystemInfo.supportsComputeShaders);
            supports3DTextures = Supports(SystemInfo.supports3DTextures);
            graphicsMultiThreaded = Supports(SystemInfo.graphicsMultiThreaded);

            RenderTextureFormats = Enum.GetValues(typeof(RenderTextureFormat)).OfType<RenderTextureFormat>().ToList();
            TextureFormats = Enum.GetValues(typeof(TextureFormat)).OfType<TextureFormat>().ToList();
        }

        public static string Supports(bool supported)
        {
            if (supported)
            {
                return "Supported";
            }
            else
            {
                return "Not Supported";
            }
        }
    }
}