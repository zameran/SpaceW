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

using SpaceEngine.Core.Debugging;
using SpaceEngine.Tools;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUIPerfomanceMonitor : DebugGUI
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

            debugInfoDrawBounds.width = Screen.width - 20;

            using (new PerformanceMonitor.Timer("Repfomance Monitor OnGUI"))
            {
                GUI.Window(0, debugInfoDrawBounds, UI, "Perfomance Monitor (in milliseconds)");
            }
        }

        protected override void UI(int id)
        {
            var counters = PerformanceMonitor.Counters;
            if (counters == null || counters.Count == 0) { GUILayoutExtensions.DrawBadHolder("Perfomance stats: ", "No Data!?", GUISkin); return; }

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);
            {
                GUILayout.BeginVertical();

                for (int i = 0; i < counters.Count; i++)
                {
                    var counter = counters[i];

                    GUILayoutExtensions.VerticalBoxed(string.Format("{0}", counter.Name), GUISkin, () =>
                    {
                        GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                        {
                            GUILayoutExtensions.Horizontal(() =>
                            {
                                GUILayoutExtensions.LabelWithSpace(string.Format("Total: {0}", counter.Time / 1000.0f), -8);
                                GUILayoutExtensions.LabelWithSpace(string.Format("Average: {0}", counter.Average / 1000.0f), -8);
                                GUILayoutExtensions.LabelWithSpace(string.Format("Last: {0}", counter.Last / 1000.0f), -8);
                                GUILayoutExtensions.LabelWithSpace(string.Format("Max: {0}", counter.Max / 1000.0f), -8);
                                GUILayoutExtensions.LabelWithSpace(string.Format("Count: {0}", counter.Count), -8);
                            });
                        }, GUILayout.Width(debugInfoDrawBounds.width - 45));
                    }, GUILayout.Width(debugInfoDrawBounds.width - 40));
                }

                GUILayoutExtensions.SpacingSeparator();

                GUILayout.EndVertical();
            }

            GUILayoutExtensions.SpacingSeparator();

            GUILayout.EndScrollView();
        }
    }
}