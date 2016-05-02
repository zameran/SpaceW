#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using UnityEngine;

public sealed class AssemblyOverlayGUI : MonoBehaviour
{
    public AssemblyLoader loader = null;

    private string addonList;

    private Rect position = new Rect(0.0f, 0.0f, 384.0f, 0.0f);
    private Vector2 scrollPosition;

    private bool showAddons;
    private bool useScrollView;

    private GUIStyle windowStyle;
    private GUIStyle labelGreen;
    private GUIStyle labelYellow;

    public Texture2D backgroundTex;

    private void Awake()
    {
        if (loader == null) loader = Loader.Instance as AssemblyLoader;
    }

    private void OnGUI()
    {
        if (loader == null) return;

        position = GUILayout.Window(GetInstanceID(), position, Window, string.Empty, windowStyle);
        CheckScrollViewUsage();
    }

    private void Start()
    {
        InitialiseStyles();
    }

    private void Update()
    {

    }

    private void CheckScrollViewUsage()
    {
        if (position.height < Screen.height * 0.5f || useScrollView)
        {
            return;
        }

        useScrollView = true;
        position.height = Screen.height * 0.5f;
    }

    private void DrawAddonBoxEnd()
    {
        if (useScrollView)
        {
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.EndVertical();
        }
    }

    private void DrawAddonBoxStart()
    {
        if (useScrollView)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Screen.height * 0.5f));
        }
        else
        {
            GUILayout.BeginVertical(GUI.skin.scrollView);
        }
    }

    private void DrawAddonList()
    {
        DrawAddonBoxStart();
        DrawAddons();
        DrawAddonBoxEnd();
    }

    private void DrawAddons()
    {
        addonList = string.Empty;

        foreach (var addon in loader.ExternalAssemblies)
        {
            var nameStyle = labelGreen;
            var versionStyle = labelYellow;

            addonList += Environment.NewLine + addon.Name + " - " + addon.Version;

            GUILayout.BeginHorizontal();
            GUILayout.Label(addon.Name, nameStyle);

            GUILayout.FlexibleSpace();

            GUILayout.Label(addon.Version, versionStyle);
            GUILayout.EndHorizontal();
        }
    }

    private void InitialiseStyles()
    {
        windowStyle = new GUIStyle
        {
            normal =
            {
                background = backgroundTex
            },
            border = new RectOffset(3, 3, 20, 3),
            padding = new RectOffset(10, 10, 1, 5)
        };

        labelGreen = new GUIStyle
        {
            normal =
            {
                textColor = Color.green
            }
        };

        labelYellow = new GUIStyle
        {
            normal =
            {
                textColor = Color.yellow
            }
        };
    }

    private void Window(int windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Loaded Add-ons: " + loader.TotalDetected);
        GUILayout.FlexibleSpace();

        if (GUILayout.Toggle(showAddons, "List Add-ons: ") != showAddons)
        {
            showAddons = !showAddons;
            position.height = 0.0f;
        }

        GUILayout.EndHorizontal();

        if (showAddons)
        {
            DrawAddonList();
        }
    }
}