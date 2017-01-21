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

using System.Collections.Generic;

using UnityEngine;

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

            Objects = FindObjectsOfType(typeof(Object));
        }

        private void FixedUpdate()
        {
            if (Time.frameCount % 64 == 0)
                Objects = FindObjectsOfType(typeof(Object));
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Leaks Info");
        }

        protected override void UI(int id)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(debugInfoBounds.width), GUILayout.Height(debugInfoBounds.height));
            {
                #region Do Magic

                if (Objects == null) return;

                var dictionary = new Dictionary<string, int>();

                for (var i = 0; i < Objects.Length; i++)
                {
                    var obj = Objects[i];

                    var key = obj.GetType().FullName;

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

                var entryies = new List<KeyValuePair<string, int>>(dictionary);

                entryies.Sort((firstPair, nextPair) => nextPair.Value.CompareTo((firstPair.Value)));

                GUILayout.BeginVertical();

                for (int i = 0; i < entryies.Count; i++)
                {
                    var entry = entryies[i];

                    GUILayout.BeginHorizontal("box");
                    GUILayoutExtensions.LabelWithFlexibleSpace(entry.Key, entry.Value.ToString());
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
        }
    }
}