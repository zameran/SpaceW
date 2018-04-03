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
// Creation Date: 2017.03.03
// Creation Time: 6:31 AM
// Creator: zameran
#endregion

using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Tools;

using System;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public class DebugGUICoreStorageInfo : DebugGUI
    {
        private Vector2 ContentsScrollPosition = Vector2.zero;

        public float ContentsWindowSize = 512.0f;

        private bool ShowContents = false;

        private Rect ContentsInfoBounds
        {
            get
            {
                return new Rect(debugInfoBounds.x + debugInfoBounds.width + 10, 
                                debugInfoBounds.y, ContentsWindowSize, ContentsWindowSize);
            }
        }

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

            if (ShowContents)
            {
                GUILayout.Window(1, ContentsInfoBounds, StorageUI, "Storage Contents");
            }
        }

        protected override void UI(int id)
        {
            GUILayoutExtensions.VerticalBoxed("Controls: ", GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    ShowContents = GUILayout.Toggle(ShowContents, " Show Storage Contents?");
                });
            });

            GUILayoutExtensions.SpacingSeparator();

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

            DrawStorageInfo<GPUTileStorage>("GPU Storage");
            DrawStorageInfo<CBTileStorage>("CB Storage");
            DrawStorageInfo<CPUTileStorage>("CPU Storage");

            GUILayoutExtensions.SpacingSeparator();

            GUILayout.EndScrollView();
        }

        protected void StorageUI(int id)
        {
            ContentsScrollPosition = GUILayout.BeginScrollView(ContentsScrollPosition, false, true);

            DrawStorageContents<GPUTileStorage>("GPU Storage");

            GUILayout.EndScrollView();
        }

        protected void DrawStorageContents<T>(string prefix = "Storage") where T : TileStorage
        {
            var body = GodManager.Instance.ActiveBody;
            if (body == null) { GUILayoutExtensions.DrawBadHolder(prefix, "No Body!?", GUISkin); return; }
            if (body.Storages == null) { GUILayoutExtensions.DrawBadHolder(prefix, "No Storages!?", GUISkin); return; }

            var tileStorages = body.Storages.Where(storage => storage is T).ToList();

            GUILayoutExtensions.VerticalBoxed(prefix, GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    if (tileStorages.Count == 0)
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("Active body doesn't have any storages of provided type {0}", typeof(T).Name));
                    }
                    else
                    {
                        var type = typeof(T);

                        if (type == typeof(GPUTileStorage))
                        {
                            for (var storageIndex = 0; storageIndex < tileStorages.Count; storageIndex++)
                            {
                                var storage = tileStorages[storageIndex];

                                if (storage != null)
                                {
                                    var slotsCount = storage.Slots.Length;
                                    var slots = ToRectangular(storage.Slots, Mathf.RoundToInt(slotsCount / 32.0f));

                                    GUILayout.BeginVertical();

                                    for (var x = 0; x < slots.GetLength(0); x++)
                                    {
                                        GUILayout.BeginHorizontal();

                                        for (var y = 0; y < slots.GetLength(1); y++)
                                        {
                                            var slot = slots[x, y] as GPUTileStorage.GPUSlot;

                                            if (slot != null) GUILayout.Label(slot.Texture, ImageLabelStyle, GUILayout.Width(64), GUILayout.Height(64));
                                        }

                                        GUILayout.EndHorizontal();
                                    }

                                    GUILayout.EndVertical();
                                }
                            }
                        }
                    }
                    GUILayoutExtensions.SpacingSeparator();
                }, GUILayout.Width(debugInfoBounds.width - 45));
            }, GUILayout.Width(debugInfoBounds.width - 40));
        }

        protected void DrawStorageInfo<T>(string prefix = "Storage") where T : TileStorage
        {
            var body = GodManager.Instance.ActiveBody;
            if (body == null) { GUILayoutExtensions.DrawBadHolder(prefix, "No Body!?", GUISkin); return; }
            if (body.Storages == null) { GUILayoutExtensions.DrawBadHolder(prefix, "No Storages!?", GUISkin); return; }

            var tileStorages = body.Storages.Where(storage => storage is T).ToList();

            GUILayoutExtensions.VerticalBoxed(prefix, GUISkin, () =>
            {
                GUILayoutExtensions.VerticalBoxed("", GUISkin, () =>
                {
                    if (tileStorages.Count == 0)
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("Active body doesn't have any storages of provided type {0}", typeof(T).Name));
                    }
                    else
                    {
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Count: {1}", prefix, tileStorages.Count));
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Total Capacity: {1}", prefix, tileStorages.Sum((storage) => storage.Capacity)));
                        GUILayoutExtensions.LabelWithSpace(string.Format("{0} Total Free: {1}", prefix, tileStorages.Sum((storage) => storage.FreeSlotsCount)));
                    }

                    GUILayoutExtensions.SpacingSeparator();
                }, GUILayout.Width(debugInfoBounds.width - 45));
            }, GUILayout.Width(debugInfoBounds.width - 40));
        }

        private static T[,] ToRectangular<T>(T[] flatArray, int width)
        {
            var height = (int)Math.Ceiling(flatArray.Length / (double)width);
            var result = new T[height, width];

            for (var index = 0; index < flatArray.Length; index++)
            {
                var rowIndex = index / width;
                var colIndex = index % width;

                result[rowIndex, colIndex] = flatArray[index];
            }

            return result;
        }
    }
}