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
// Creation Date: 2017.10.22
// Creation Time: 11:30 AM
// Creator: zameran
#endregion

using SpaceEngine.Galaxy;

using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUIGalaxyGenerator : DebugGUI
    {
        internal GalaxyGenerator Galaxy { get; private set; }

        private Vector2 StatisticsScrollPosition = Vector2.zero;

        private bool ShowStatistics = false;

        private Rect StatisticsInfoBounds
        {
            get
            {
                return new Rect(debugInfoBounds.x + debugInfoBounds.width + 10,
                                debugInfoBounds.y, debugInfoBounds.width, debugInfoBounds.height / 2.0f);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            Galaxy = FindObjectOfType<GalaxyGenerator>();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Galaxy Info");

            if (ShowStatistics)
            {
                GUILayout.Window(1, StatisticsInfoBounds, StatisticsUI, "Galaxy Statistics");
            }
        }

        protected override void UI(int id)
        {
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            if (Galaxy != null && Helper.Enabled(Galaxy))
            {
                GUILayoutExtensions.VerticalBoxed("Controls: ", GUISkin, () =>
                {
                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            ShowStatistics = GUILayout.Toggle(ShowStatistics, " Show Storage Contents?");
                        });

                        GUILayoutExtensions.SpacingSeparator();

                        if (GUILayout.Button("Save Preset")) Galaxy.Settings.SaveSettings();

                        GUILayoutExtensions.SpacingSeparator();

                        #region Preset Manipulation

                        // NOTE : MVC Fuckup... Patterns? Ahahahaha...

                        var presets = Galaxy.Settings.FindSettings();

                        if (presets != null && presets.Count != 0)
                        {
                            GUILayoutExtensions.VerticalBoxed("Available Presets: ", GUISkin, () =>
                            {
                                for (var presetIndex = 0; presetIndex < presets.Count; presetIndex++)
                                {
                                    var preset = presets[presetIndex];

                                    if (preset == null) break;

                                    GUILayoutExtensions.HorizontalBoxed("", GUISkin, () =>
                                    {
                                        GUILayout.Label(System.IO.Path.GetFileName(preset));

                                        if (GUILayout.Button("Load")) { Galaxy.Settings.LoadSettings(preset, settings => Galaxy.ApplyPreset(settings)); }
                                        if (GUILayout.Button("Delete")) { Galaxy.Settings.DeleteSettings(preset); }
                                    });
                                }
                            });
                        }
                        else
                        {
                            GUILayoutExtensions.DrawBadHolder("Available Presets: ", "No Presets!?", GUISkin);
                        }

                        #endregion
                    });
                });

                GUILayoutExtensions.SpacingSeparator();

                GUILayoutExtensions.VerticalBoxed("Galaxy parameters: ", GUISkin, () =>
                {
                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        var renderingCDTable = Galaxy.Settings.GalaxyRenderingParameters.DustColorDistribution;
                        var starGenerationCDTable = Galaxy.Settings.GalaxyGenerationParameters.StarsColorDistribution;
                        var dustGenerationCDTable = Galaxy.Settings.GalaxyGenerationParameters.DustColorDistribution;

                        if (renderingCDTable != null && renderingCDTable.Lut != null)
                        {
                            GUILayout.Label("Dust Color Distribution Table (Render): ");
                            GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                            {
                                GUILayoutExtensions.Horizontal(() =>
                                {
                                    GUILayout.Label(renderingCDTable.Lut);
                                });
                            });
                        }

                        GUILayoutExtensions.SpacingSeparator();

                        if (starGenerationCDTable != null && starGenerationCDTable.Lut != null)
                        {
                            GUILayout.Label("Stars Color Distribution Table (Generator): ");
                            GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                            {
                                GUILayoutExtensions.Horizontal(() =>
                                {
                                    GUILayout.Label(starGenerationCDTable.Lut);
                                });
                            });
                        }

                        GUILayoutExtensions.SpacingSeparator();

                        if (dustGenerationCDTable != null && dustGenerationCDTable.Lut != null)
                        {
                            GUILayout.Label("Dust Color Distribution Table (Generator): ");
                            GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                            {
                                GUILayoutExtensions.Horizontal(() =>
                                {
                                    GUILayout.Label(dustGenerationCDTable.Lut);
                                });
                            });
                        }
                    });

                    GUILayoutExtensions.SpacingSeparator();

                    GUILayoutExtensions.VerticalBoxed("Rendering parameters: ", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("Method: ", GUISkin, () =>
                        {
                            GalaxyRenderer.Instance.RenderMethod = (GalaxyRenderer.RenderType)GUILayout.SelectionGrid((int)GalaxyRenderer.Instance.RenderMethod, Enum.GetNames(typeof(GalaxyRenderer.RenderType)), 2);
                        });

                        GUILayoutExtensions.SpacingSeparator();

                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.SliderWithFieldAndControls("Dust Strength (Proportion): ", 0.0f, 1.0f, ref Galaxy.Settings.GalaxyRenderingParameters.DustStrength, "0.0000", 75, 0.0025f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Dust Size (Ly): ", 0.0f, 4.0f, ref Galaxy.Settings.GalaxyRenderingParameters.DustSize, "0.0000", 75, 0.25f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Dust Pass Count: ", 1, Galaxy.Settings.GalaxyParameters.PassCount, ref Galaxy.Settings.GalaxyRenderingParameters.DustPassCount, "0", 75, 1);

                            GUILayoutExtensions.SliderWithFieldAndControls("Gas Strength (Proportion): ", 0.0f, 1.0f, ref Galaxy.Settings.GalaxyRenderingParameters.GasStength, "0.0000", 75, 0.0025f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Gas Size (Ly): ", 0.0f, 4.0f, ref Galaxy.Settings.GalaxyRenderingParameters.GasSize, "0.0000", 75, 0.25f);

                            GUILayoutExtensions.SliderWithFieldAndControls("Stars Absolute Size (Mm)", 1.0f, 1024.0f, ref Galaxy.Settings.GalaxyRenderingParameters.StarAbsoluteSize, "0.0", 75, 32.0f);
                        });
                    });

                    GUILayoutExtensions.SpacingSeparator();

                    GUILayoutExtensions.VerticalBoxed("Generation methodics: ", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.SliderWithFieldAndControls("Pass Count: ", 1, 8, ref Galaxy.Settings.GalaxyParameters.PassCount, "0", 75, 1);

                            GUILayoutExtensions.VerticalBoxed("Preset: ", GUISkin, () =>
                            {
                                Galaxy.Settings.Type = (GalaxySettings.GenerationType)GUILayout.SelectionGrid((byte)Galaxy.Settings.Type, Enum.GetNames(typeof(GalaxySettings.GenerationType)), 2);
                            });

                            GUILayoutExtensions.SpacingSeparator();

                            if (GUI.changed)
                            {
                                Galaxy.InitAndGenerateBuffers();
                                Galaxy.InitDustMaterials();
                            }
                        });
                    });

                    GUILayoutExtensions.SpacingSeparator();

                    GUILayoutExtensions.VerticalBoxed("Generation parameters: ", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.DrawVectorSlidersWithField(ref Galaxy.Settings.GalaxyGenerationParameters.Randomize, -1.0f, 1.0f, GUISkin, "Randomize (Randomize Vector)", "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.DrawVectorSlidersWithField(ref Galaxy.Settings.GalaxyGenerationParameters.TemperatureRange, 0.0f, 30000.0f, GUISkin, "Temperature Range (Range)", "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.DrawVectorSlidersWithField(ref Galaxy.Settings.GalaxyGenerationParameters.Offset, -1.0f, 1.0f, GUISkin, "Offset (Linear, Assymetry Factor)", "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.DrawVectorSlidersWithField(ref Galaxy.Settings.GalaxyGenerationParameters.Warp, -1.0f, 1.0f, GUISkin, "Warp (Pre [XY] and Post [ZW] Rotation Warp)", "0.0000", textFieldWidth: 100);

                            GUILayoutExtensions.SliderWithFieldAndControls("Radius (kLy): ", 1.0f, 256.0f, ref Galaxy.Settings.GalaxyGenerationParameters.Radius, "0.0000", 75, 1.0f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Ellipse Radius (Proportion): ", -1.0f, 1.0f, ref Galaxy.Settings.GalaxyGenerationParameters.RadiusEllipse, "0.0000", 75, 0.25f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Size Bar (Proportion): ", -1.0f, 1.0f, ref Galaxy.Settings.GalaxyGenerationParameters.SizeBar, "0.0000", 75, 0.25f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Depth (kLy): ", 1.0f, 256.0f, ref Galaxy.Settings.GalaxyGenerationParameters.Depth, "0.0000", 75, 1.0f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Spiral Eccentricity (Inverse Proportion): ", -1.0f, 1.0f, ref Galaxy.Settings.GalaxyGenerationParameters.InverseSpiralEccentricity, "0.0000", 75, 0.25f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Spiral Rotation (Rad): ", -Mathf.PI * 4.0f, Mathf.PI * 4.0f, ref Galaxy.Settings.GalaxyGenerationParameters.SpiralRotation, "0.0000", 75, Mathf.PI / 4.0f);
                        });
                    });

                    GUILayoutExtensions.SpacingSeparator();

                    if (Galaxy.Settings.Type == GalaxySettings.GenerationType.Double)
                    {
                        GUILayoutExtensions.VerticalBoxed("Generation per pass parameters: ", GUISkin, () =>
                        {
                            GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                            {
                                GUILayoutExtensions.SliderWithFieldAndControls("Rotation (Rad): ", -Mathf.PI * 2.0f, Mathf.PI * 2.0f, ref Galaxy.Settings.GalaxyGenerationPerPassParameters.PassRotation, "0.0000", 75, Mathf.PI / 4.0f);
                            });
                        });
                    }
                });

                if (GUI.changed)
                {
                    Galaxy.GenerateBuffers();
                }
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Galaxy parameters: ", "No Galaxy!?", GUISkin);
            }

            GUILayout.EndScrollView();
        }

        private void StatisticsUI(int id)
        {
            StatisticsScrollPosition = GUILayout.BeginScrollView(StatisticsScrollPosition, false, false);

            if (Galaxy != null && Helper.Enabled(Galaxy))
            {
                var starsCount = Galaxy.Settings.TotalStarsCount;
                var dustCount = Galaxy.Settings.TotalDustCount;
                var octree = Galaxy.Octree;

                GUILayoutExtensions.VerticalBoxed("Render Statistics: ", GUISkin, () =>
                {
                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("Stars Count (Total): {0:N}", starsCount));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Dust Count (Total): {0:N}", dustCount));

                        GUILayoutExtensions.LabelWithSpace(string.Format("Star Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxyParticle>()));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Generation Parameters Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxyGenerationParameters>()));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Generation Per Pass Parameters Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxyGenerationPerPassParameters>()));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Parameters Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxyParameters>()));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Rendering Parameters Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxyRenderingParameters>()));
                        GUILayoutExtensions.LabelWithSpace(string.Format("Settings Struct Stride (Generator, bytes): {0}", Marshal.SizeOf<GalaxySettings>()));

                        if (octree != null)
                        {
                            GUILayoutExtensions.LabelWithSpace(string.Format("Octree Count (Total): {0:N}", octree.Count));
                            GUILayoutExtensions.LabelWithSpace(string.Format("Octree Nodes Count (Total): {0:N}", octree.NodesCount()));
                        }

                        GUILayoutExtensions.SpacingSeparator();
                    });
                });
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Galaxy statistics: ", "No Galaxy!?", GUISkin);
            }

            GUILayout.EndScrollView();
        }
    }
}