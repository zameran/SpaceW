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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran

#endregion

using System;
using System.Collections.Generic;
using SpaceEngine.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUILeaks : DebugGUI
    {
        public Object[] Objects;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            Objects = FindObjectsByType<Object>(FindObjectsSortMode.InstanceID);
        }

        private void FixedUpdate()
        {
            if (Time.frameCount % 64 == 0)
            {
                Objects = FindObjectsByType<Object>(FindObjectsSortMode.InstanceID);
            }
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Leaks Info");
        }

        protected override void UI(int id)
        {
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true, GUILayout.Width(debugInfoBounds.width), GUILayout.Height(debugInfoBounds.height));
            {
                #region Do Magic

                if (Objects == null)
                {
                    return;
                }

                var dictionary = new Dictionary<Type, int>();

                for (var i = 0; i < Objects.Length; i++)
                {
                    var obj = Objects[i];
                    var key = obj.GetType();

                    if (dictionary.ContainsKey(key))
                    {
                        dictionary[key]++;
                    }
                    else
                    {
                        dictionary[key] = 1;
                    }
                }

                #endregion

                var entries = new List<KeyValuePair<Type, int>>(dictionary);

                entries.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));

                GUILayout.BeginVertical();

                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];

                    GUILayoutExtensions.HorizontalBoxed("", GUISkin, () => { GUILayoutExtensions.LabelWithFlexibleSpace(entry.Key.FullName, entry.Value.ToString()); });
                }

                GUILayout.Space(10);

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }
    }
}