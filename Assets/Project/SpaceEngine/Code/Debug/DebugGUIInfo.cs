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
// Creation Date: 2017.03.16
// Creation Time: 2:47 PM
// Creator: zameran
#endregion

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public class DebugGUIInfo : DebugGUI
    {
        private readonly string[] Info = new string[]
        {
            "Mouse Scrollwheel to control speed.",
            "Left mouse button to orientation.",
            "Right mouse button to rotate around target.",
            "E/Q to roll axis.",
            "Left Shift for more speed.",
            "Left Control for even more speed.",
            "Left Shift + Left Control for speed of God.",
            "Left Alt for less speed.",
            "F12 to capture screenshot."
        };

        private readonly string[] InfoAdditional = new string[]
        {
            "WARNING! Float precision!"
        };

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Info");
        }

        protected override void UI(int id)
        {
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true, GUILayout.Width(debugInfoBounds.width), GUILayout.Height(debugInfoBounds.height));
            {
                GUILayoutExtensions.VerticalBoxed("Input info: ", GUISkin, () =>
                {
                    GUILayout.Space(20);

                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        DrawLabelLines(Info);
                    });
                });

                GUILayout.Space(5);

                GUILayoutExtensions.VerticalBoxed("Additional info: ", GUISkin, () =>
                {
                    GUILayout.Space(20);

                    GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                    {
                        DrawLabelLines(InfoAdditional);
                    });
                });
            }

            GUILayout.EndScrollView();
        }

        private void DrawLabelLines(string[] lines)
        {
            GUILayoutExtensions.Vertical(() =>
            {
                for (byte i = 0; i < lines.Length; i++)
                {
                    GUILayoutExtensions.LabelWithSpace(lines[i]);
                }
            });

            GUILayout.Space(8);
        }
    }
}