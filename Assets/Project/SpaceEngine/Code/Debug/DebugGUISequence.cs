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
// Creation Date: 2017.03.06
// Creation Time: 12:51 AM
// Creator: zameran
#endregion

using SpaceEngine.Managers;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public class DebugGUISequence : DebugGUI
    {
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

            GUI.Window(0, debugInfoBounds, UI, "Sequence Debugger");
        }

        protected override void UI(int id)
        {
            if (DebugSequenceManager.Instance == null) return;

            var sequence = DebugSequenceManager.Instance.Sequence;

            if (sequence == null) return;

            if (sequence.Count != 0)
            {
                ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);
                {
                    GUILayout.BeginVertical();

                    for (int i = 0; i < sequence.Count; i++)
                    {
                        var entry = sequence[i];

                        GUILayoutExtensions.VerticalBoxed(string.Format("{0}", entry.Name), GUISkin, () =>
                        {
                            if (true)
                            {
                                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                                {
                                    GUILayoutExtensions.Vertical(() =>
                                    {
                                        GUILayoutExtensions.Horizontal(() =>
                                        {
                                            GUILayoutExtensions.LabelWithFlexibleSpace("Time: ", entry.Time);
                                        });

                                        GUILayoutExtensions.Horizontal(() =>
                                        {
                                            GUILayoutExtensions.LabelWithFlexibleSpace("Time Since Startup: ", entry.TimeSinceStartup);
                                        });

                                        GUILayoutExtensions.Horizontal(() =>
                                        {
                                            GUILayoutExtensions.LabelWithFlexibleSpace("Frame: ", entry.Frame);
                                        });
                                    });
                                }, GUILayout.Width(debugInfoBounds.width - 45));
                            }
                        }, GUILayout.Width(debugInfoBounds.width - 40));
                    }

                    GUILayoutExtensions.SpacingSeparator();

                    GUILayout.EndVertical();
                }

                GUILayoutExtensions.SpacingSeparator();

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayoutExtensions.DrawBadHolder("Sequence stats: ", "No Debug Sequence!?", GUISkin);
            }
        }
    }
}