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
// Creation Date: 2017.03.03
// Creation Time: 6:31 AM
// Creator: zameran
#endregion

using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Storage;

using System.Linq;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public class DebugGUICoreStorageInfo : DebugGUI
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

            GUILayout.Window(0, debugInfoBounds, UI, "Core Cache Info");
        }

        protected override void UI(int id)
        {
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            DrawStorageInfo<GPUTileStorage>("GPU Storage");
            DrawStorageInfo<CBTileStorage>("CB Storage");
            DrawStorageInfo<CPUTileStorage>("CPU Storage");

            GUILayout.EndScrollView();
        }

        protected void DrawStorageInfo<T>(string prefix = "Storage") where T : TileStorage
        {
            GUILayout.BeginVertical(prefix, GUISkin.box, GUILayout.Width(debugInfoBounds.width - 40));
            {
                var storages = GodManager.Instance.ActiveBody.transform.GetComponentsInChildren<T>().ToList();

                GUILayout.Space(20);

                GUILayout.BeginVertical("", GUISkin.box, GUILayout.Width(debugInfoBounds.width - 45));
                {
                    if (storages.Count == 0)
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("Active body doesn't have any storages of provided type {0}", typeof(T).Name));
                    }
                    else
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Count: {1}", prefix, storages.Count));
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Total Capacity: {1}", prefix, storages.Sum((storage) => storage.Capacity)));
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Total Free: {1}", prefix, storages.Sum((storage) => storage.FreeSlotsCount)));
                    }

                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
    }
}