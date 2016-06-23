#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

public sealed class DebugGUILeaks : DebugGUI
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

        GUILayout.Window(0, debugInfoBounds, UI, "Leaks Info");
    }

    private void UI(int id)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(debugInfoBounds.width), GUILayout.Height(debugInfoBounds.height));
        {
            Object[] objects = FindObjectsOfType(typeof(Object));

            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            foreach (Object obj in objects)
            {
                string key = obj.GetType().ToString();

                if (dictionary.ContainsKey(key))
                {
                    dictionary[key]++;
                }
                else
                {
                    dictionary[key] = 1;
                }
            }

            List<KeyValuePair<string, int>> myList = new List<KeyValuePair<string, int>>(dictionary);

            myList.Sort(delegate (KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair)
            {
                return nextPair.Value.CompareTo((firstPair.Value));
            });

            GUILayout.BeginVertical();

            for (int i = 0; i < myList.Count; i++)
            {
                KeyValuePair<string, int> entry = myList[i];

                GUILayoutExtensions.LabelWithSpace(entry.Key + " : " + entry.Value, -8);
            }

            GUILayout.Space(10);

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();
    }
}