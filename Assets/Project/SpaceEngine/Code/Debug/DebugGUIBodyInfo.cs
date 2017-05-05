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

using SpaceEngine.Core.Bodies;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUIBodyInfo : DebugGUI
    {
        public Body Body;

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
                GUILayout.BeginVertical();

                GUILayout.Label("Body parameters: ", BoldLabelStyle);

                GUILayout.Label("Nothing here!");

                GUILayout.EndVertical();

                if (Body.Atmosphere != null && Helper.Enabled(Body.Atmosphere))
                {
                    GUILayout.BeginVertical();

                    GUILayout.Label("Atmosphere parameters: ", BoldLabelStyle);

                    GUILayout.Label("Preset: ");
                    Body.Atmosphere.AtmosphereBase = (AtmosphereBase)GUILayout.SelectionGrid((int)Body.Atmosphere.AtmosphereBase, System.Enum.GetNames(typeof(AtmosphereBase)), 2);

                    GUILayout.Space(10);

                    GUILayout.Label("Density: ");
                    float.TryParse(GUILayout.TextField(Body.Atmosphere.Density.ToString("0.0")), out Body.Atmosphere.Density);

                    GUILayout.Label("Height: ");
                    float.TryParse(GUILayout.TextField(Body.Atmosphere.Height.ToString("0.0")), out Body.Atmosphere.Height);

                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginVertical();

                    GUILayoutExtensions.LabelWithSpace("No Atmosphere!?", -8);

                    GUILayout.EndVertical();
                }
            }
            else
            {
                GUILayout.BeginVertical();

                GUILayoutExtensions.LabelWithSpace("No Body!?", -8);

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }
    }
}