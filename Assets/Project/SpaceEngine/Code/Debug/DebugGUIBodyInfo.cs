#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

using SpaceEngine.Core.Bodies;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUIBodyInfo : DebugGUI
    {
        public Body Body { get { return GodManager.Instance.ActiveBody; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Body Info");
        }

        protected override void UI(int id)
        {
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            if (Body != null && Helper.Enabled(Body))
            {
                GUILayoutExtensions.VerticalBoxed("Body parameters: ", GUISkin, () =>
                {
                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        if (Body.TCCPS != null)
                        {
                            var materialTable = Body.TCCPS.MaterialTable;

                            if (materialTable != null && materialTable.Lut != null)
                            {
                                GUILayout.Label("Material Table: ");
                                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                                {
                                    GUILayoutExtensions.Horizontal(() =>
                                    {
                                        GUILayout.Label(materialTable.Lut);
                                    });
                                });
                            }
                        }
                    });
                });

                GUILayoutExtensions.SpacingSeparator();

                if (Body.Ocean != null && Body.OceanEnabled && Helper.Enabled(Body.Ocean))
                {
                    GUILayoutExtensions.VerticalBoxed("Ocean parameters: ", GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.SliderWithField("Level: ", 0.0f, 5.0f, ref Body.Ocean.OceanLevel);
                            GUILayoutExtensions.SliderWithField("Z Min: ", 0.0f, 50000.0f, ref Body.Ocean.ZMin);
                        });
                    });

                    GUILayoutExtensions.SpacingSeparator();
                }
                else
                {
                    GUILayoutExtensions.DrawBadHolder("Ocean parameters: ", "No Ocean!?", GUISkin);
                }
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Body parameters: ", "No Body!?", GUISkin);
            }

            GUILayout.EndScrollView();
        }
    }
}