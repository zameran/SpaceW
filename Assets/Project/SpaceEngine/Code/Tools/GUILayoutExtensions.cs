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

using System;

using UnityEngine;

public static class GUILayoutExtensions
{
    public static void SliderWithField(object caption, float leftValue, float rightValue, ref float value, string pattern = "0.0", int textFieldWidth = 75)
    {
        GUILayout.Label(caption.ToString());

        GUILayout.BeginHorizontal();

        GUILayout.TextField(value.ToString(pattern), GUILayout.MaxWidth(textFieldWidth));
        value = GUILayout.HorizontalSlider(value, leftValue, rightValue);

        GUILayout.EndHorizontal();
    }

    public static void SliderWithFieldAndControls(object caption, float leftValue, float rightValue, ref float value, string pattern = "0.0", int textFieldWidth = 75, float controlStep = 1.0f)
    {
        GUILayout.Label(caption.ToString());

        GUILayout.BeginHorizontal();

        GUILayout.TextField(value.ToString(pattern), GUILayout.MaxWidth(textFieldWidth));

        if (GUILayout.Button("+", GUILayout.Width(20))) { value += controlStep; }
        if (GUILayout.Button("-", GUILayout.Width(20))) { value -= controlStep; }

        value = GUILayout.HorizontalSlider(value, leftValue, rightValue);

        GUILayout.EndHorizontal();
    }

    public static void LabelWithFlexibleSpace(object text1, object text2)
    {
        GUILayout.Label(text1.ToString());
        GUILayout.FlexibleSpace();
        GUILayout.Label(text2.ToString());
    }

    public static void LabelWithSpace(string text, int space = -8)
    {
        GUILayout.Label(text);
        GUILayout.Space(space);
    }

    public static void LabelWithSpace(GUIContent content, int space = -8)
    {
        GUILayout.Label(content);
        GUILayout.Space(space);
    }

    public static void Horizontal(Action body)
    {
        GUILayout.BeginHorizontal();
        if (body != null) body();
        GUILayout.EndHorizontal();
    }

    public static void Vertical(Action body)
    {
        GUILayout.BeginVertical();
        if (body != null) body();
        GUILayout.EndVertical();
    }

    public static void VerticalBoxed(string caption, GUISkin skin, Action body, params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(caption, skin.box, options);
        {
            if (body != null) body();
        }
        GUILayout.EndVertical();
    }

    public static void HorizontalBoxed(string caption, GUISkin skin, Action body, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal(caption, skin.box, options);
        {
            if (body != null) body();
        }
        GUILayout.EndHorizontal();
    }

    public static void DrawWithColor(Action body, Color color)
    {
        var tempColor = GUI.color;

        GUI.color = color;

        if (body != null) body();
        
        GUI.color = tempColor;
    }

    public static void DrawBadHolder(string caption, string message, GUISkin skin)
    {
        VerticalBoxed(caption, skin, () =>
        {
            GUILayout.Space(20);

            VerticalBoxed("", skin, () =>
            {
                LabelWithSpace(message, -8);

                GUILayout.Space(5);
            });
        });

        GUILayout.Space(5);
    }
}