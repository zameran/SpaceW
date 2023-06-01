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
// Creation Date: 2017.09.10
// Creation Time: 5:04 PM
// Creator: zameran
#endregion

using System;
using System.ComponentModel;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy;
using SpaceEngine.Environment.Atmospheric;
using SpaceEngine.Helpers;
using SpaceEngine.Managers;
using SpaceEngine.Tools;
using UnityEngine;

namespace SpaceEngine.Debugging
{
    // NOTE : Parameters copy stuff with events looks pretty messy. So. I don't give a fuck - this is a developer GUI.
    public class DebugGUIAtmosphereSetup : DebugGUI, IEventit
    {
        private readonly AtmosphereBaseProperty AtmosphereBaseProperty = new AtmosphereBaseProperty();

        private AtmosphereBase AtmosphereBase { get => AtmosphereBaseProperty.Value;
            set => AtmosphereBaseProperty.Value = value;
        }

        public Body Body => GodManager.Instance.ActiveBody;

        public Atmosphere Atmosphere => Body.Atmosphere;

        public AtmosphereParameters AtmosphereParameters = AtmosphereParameters.Default;

        public bool PresetChanged = false;

        #region Eventit

        public bool IsEventit { get; set; }

        public void Eventit()
        {
            if (IsEventit) return;

            AtmosphereBaseProperty.PropertyChanged += AtmosphereBasePropertyOnPropertyChanged;

            IsEventit = true;
        }

        public void UnEventit()
        {
            if (!IsEventit) return;

            AtmosphereBaseProperty.PropertyChanged -= AtmosphereBasePropertyOnPropertyChanged;

            IsEventit = false;
        }

        #endregion

        #region Events

        private void AtmosphereBasePropertyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PresetChanged = true;
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            Eventit();
        }

        protected virtual void OnDestroy()
        {
            UnEventit();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Atmosphere Setup");
        }

        protected override void UI(int id)
        {
            GUILayoutExtensions.VerticalBoxed("Controls: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    DrawApplyButton(() => { if (Body != null && Atmosphere != null) Atmosphere.Bake(); });
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            if (Body != null && Helper.Enabled(Body))
            {
                if (Atmosphere != null && Helper.Enabled(Body.Atmosphere) && Body.AtmosphereEnabled)
                {
                    GUILayoutExtensions.VerticalBoxed("Realtime parameters: ", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.VerticalBoxed("Preset: ", GUISkin, () =>
                            {
                                Atmosphere.AtmosphereBase = (AtmosphereBase)GUILayout.SelectionGrid((int)Atmosphere.AtmosphereBase, Enum.GetNames(typeof(AtmosphereBase)), 2);
                            });

                            GUILayoutExtensions.SpacingSeparator();

                            GUILayoutExtensions.VerticalBoxed("Artifact fixers: ", GUISkin, () =>
                            {
                                GUILayoutExtensions.SliderWithField("Radius Hold (Terrain Radius)", 0.0f, 2048.0f, ref Atmosphere.RadiusHold, "0.00", 75);
                                GUILayoutExtensions.SliderWithField("Aerial Radius (Perspective Offset)", 0.0f, 4096.0f, ref Atmosphere.AerialPerspectiveOffset, "0.00", 75);
                                GUILayoutExtensions.SliderWithFieldAndControls("Horizon Fix Eps", 0.0f, 1.0f, ref Atmosphere.HorizonFixEps, "0.00000", 75, 0.00025f);
                                GUILayoutExtensions.SliderWithFieldAndControls("Mie Fade Fix", 0.0f, 1.0f, ref Atmosphere.MieFadeFix, "0.0000", 75, 0.0025f);
                            });

                            GUILayoutExtensions.SpacingSeparator();

                            GUILayoutExtensions.SliderWithField("Density: ", 0.0f, 1.0f, ref Atmosphere.Density);
                            GUILayoutExtensions.SliderWithField("Scale", 0.01f, 16.0f, ref Atmosphere.Scale, "0.000");
                            GUILayoutExtensions.SliderWithField("Height: ", 0.0f, Body.Size, ref Atmosphere.Height);
                            GUILayoutExtensions.SliderWithField("Extinction Ground Fade", 0.000025f, 0.1f, ref Atmosphere.ExtinctionGroundFade, "0.000000");
                            GUILayoutExtensions.SliderWithField("HDR Exposure", 0.0f, 1.0f, ref Atmosphere.HDRExposure, "0.00");
                        });
                    });

                    GUILayoutExtensions.SpacingSeparator();

                    if (Atmosphere.AtmosphereBase == AtmosphereBase.Custom)
                    {
                        GUILayoutExtensions.VerticalBoxed("Bake parameters: ", GUISkin, () =>
                        {
                            GUILayoutExtensions.VerticalBoxed("Copy from preset: ", GUISkin, () =>
                            {
                                AtmosphereBase = (AtmosphereBase)GUILayout.SelectionGrid((int)AtmosphereBase, Enum.GetNames(typeof(AtmosphereBase)), 2);
                            });

                            GUILayoutExtensions.SpacingSeparator();

                            var parameters = PresetChanged ? AtmosphereParameters.Get(AtmosphereBase) : new AtmosphereParameters(AtmosphereParameters);

                            var mieG = parameters.MIE_G;
                            var hr = parameters.HR;
                            var hm = parameters.HM;
                            var agr = parameters.AVERAGE_GROUND_REFLECTANCE;
                            var betaR = parameters.BETA_R;
                            var betaM = parameters.BETA_MSca;
                            var betaE = parameters.BETA_MEx;
                            var rg = parameters.Rg;
                            var rt = parameters.Rt;
                            var rl = parameters.Rl;

                            PresetChanged = false;

                            GUILayoutExtensions.SliderWithField("Mie G: ", 0.0f, 1.0f, ref mieG, "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.SliderWithFieldAndControls("Air density (HR At half-height in KM): ", 0.0f, 256.0f, ref hr, "0.00", textFieldWidth: 100, controlStep: 1.0f);
                            GUILayoutExtensions.SliderWithFieldAndControls("Particle density (HM At half-height in KM): ", 0.0f, 256.0f, ref hm, "0.00", textFieldWidth: 100, controlStep: 1.0f);
                            GUILayoutExtensions.SliderWithField("Average Ground Reflectance: ", 0.0f, 1.0f, ref agr, "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.SliderWithField("Rg (Planet Radius in KM): ", 0.0f, 63600.0f, ref rg, "0.00000", textFieldWidth: 100);
                            GUILayoutExtensions.SliderWithField("Rt (Atmosphere Top Radius in KM): ", rg, 63600.0f, ref rt, "0.00000", textFieldWidth: 100);
                            GUILayoutExtensions.SliderWithField("Rl (Planet Bottom Radius in KM): ", rt, 63600.0f, ref rl, "0.00000", textFieldWidth: 100);

                            GUILayoutExtensions.DrawVectorWithSlidersAndFields(ref betaR, 0.0f, 1.0f, GUISkin, "Beta R (Rayliegh Scattering)", "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.DrawVectorWithSlidersAndFields(ref betaM, 0.0f, 1.0f, GUISkin, "Beta M (Mie Scattering)", "0.0000", textFieldWidth: 100);
                            GUILayoutExtensions.DrawVectorWithSlidersAndFields(ref betaE, 0.0f, 1.0f, GUISkin, "Beta E (Extinction Scattering)", "0.0000", textFieldWidth: 100);

                            parameters = new AtmosphereParameters(mieG, hr, hm, agr, betaR, betaM, betaE, rg, rt, rl, rg, rt, rl, SCALE: 1.0f);

                            AtmosphereParameters = new AtmosphereParameters(parameters);
                            Atmosphere.PushPreset(parameters);
                        });
                    }
                    else
                    {
                        GUILayoutExtensions.DrawBadHolder("Atmosphere Bake parameters: ", "Use 'Custom' preset please...", GUISkin);
                    }
                }
                else
                {
                    GUILayoutExtensions.DrawBadHolder("Atmosphere parameters: ", "No Atmosphere!?", GUISkin);
                }
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Atmosphere parameters: ", "No Body!?", GUISkin);
            }

            GUILayout.EndScrollView();
        }

        protected void DrawApplyButton(Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button("Apply", options))
            {
                action?.Invoke();
            }
        }
    }
}