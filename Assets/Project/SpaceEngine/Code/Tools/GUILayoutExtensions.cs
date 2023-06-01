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

using UnityEngine;

namespace SpaceEngine.Tools
{
    public static class GUILayoutExtensions
    {
        public static void Space(int spacing)
        {
            GUILayout.Space(spacing);
        }

        public static void BoxedPreHeader(int spacing = 20)
        {
            Space(spacing);
        }

        public static void SpacingSeparator(int spacing = 5)
        {
            Space(spacing);
        }

        public static string Field(ref float value, string pattern = "0.0", int textFieldWidth = 75)
        {
            return GUILayout.TextField(value.ToString(pattern), GUILayout.MaxWidth(textFieldWidth));
        }

        public static string Field(ref int value, string pattern = "0", int textFieldWidth = 75)
        {
            return GUILayout.TextField(value.ToString(pattern), GUILayout.MaxWidth(textFieldWidth));
        }

        public static void DrawFieldValue(ref float value, string caption = "Float Value: ", string pattern = "0.0", int textFieldWidth = 75)
        {
            GUILayout.Label(caption);
            value = float.Parse(Field(ref value, pattern, textFieldWidth));
        }

        public static void DrawFieldValue(ref int value, string caption = "Int Value: ", string pattern = "0", int textFieldWidth = 75)
        {
            GUILayout.Label(caption);
            value = int.Parse(Field(ref value, pattern, textFieldWidth));
        }

        public static void SliderWithField(object caption, float leftValue, float rightValue, ref float value, string pattern = "0.0", int textFieldWidth = 75, bool inline = false)
        {
            if (!inline) GUILayout.Label(caption.ToString());

            GUILayout.BeginHorizontal();

            if (inline) GUILayout.Label(caption.ToString(), GUILayout.ExpandWidth(false));

            value = float.Parse(Field(ref value, pattern, textFieldWidth));
            value = GUILayout.HorizontalSlider(value, leftValue, rightValue);
            value = Mathf.Clamp(value, leftValue, rightValue);

            GUILayout.EndHorizontal();
        }

        public static void SliderWithField(object caption, int leftValue, int rightValue, ref int value, string pattern = "0", int textFieldWidth = 75, bool inline = false)
        {
            if (!inline) GUILayout.Label(caption.ToString());

            GUILayout.BeginHorizontal();

            if (inline) GUILayout.Label(caption.ToString(), GUILayout.ExpandWidth(false));

            value = int.Parse(Field(ref value, pattern, textFieldWidth));
            value = Mathf.FloorToInt(GUILayout.HorizontalSlider(value, leftValue, rightValue));
            value = Mathf.Clamp(value, leftValue, rightValue);

            GUILayout.EndHorizontal();
        }

        public static void SliderWithFieldAndControls(object caption, float leftValue, float rightValue, ref float value, string pattern = "0.0", int textFieldWidth = 75, float controlStep = 1.0f)
        {
            GUILayout.Label(caption.ToString());

            GUILayout.BeginHorizontal();

            value = float.Parse(Field(ref value, pattern, textFieldWidth));

            if (GUILayout.Button("+", GUILayout.Width(20))) { value += controlStep; }
            if (GUILayout.Button("-", GUILayout.Width(20))) { value -= controlStep; }

            value = GUILayout.HorizontalSlider(value, leftValue, rightValue);
            value = Mathf.Clamp(value, leftValue, rightValue);

            GUILayout.EndHorizontal();
        }

        public static void SliderWithFieldAndControls(object caption, int leftValue, int rightValue, ref int value, string pattern = "0", int textFieldWidth = 75, int controlStep = 1)
        {
            GUILayout.Label(caption.ToString());

            GUILayout.BeginHorizontal();

            value = int.Parse(Field(ref value, pattern, textFieldWidth));

            if (GUILayout.Button("+", GUILayout.Width(20))) { value += controlStep; }
            if (GUILayout.Button("-", GUILayout.Width(20))) { value -= controlStep; }

            value = Mathf.FloorToInt(GUILayout.HorizontalSlider(value, leftValue, rightValue));
            value = Mathf.Clamp(value, leftValue, rightValue);

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

            Space(space);
        }

        public static void LabelWithSpace(GUIContent content, int space = -8)
        {
            GUILayout.Label(content);

            Space(space);
        }

        public static void Horizontal(Action body)
        {
            GUILayout.BeginHorizontal();
            body?.Invoke();
            GUILayout.EndHorizontal();
        }

        public static void Vertical(Action body)
        {
            GUILayout.BeginVertical();
            body?.Invoke();
            GUILayout.EndVertical();
        }

        public static void VerticalBoxed(string caption, GUISkin skin, Action body, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(caption, skin.box, options);
            {
                if (!string.IsNullOrWhiteSpace(caption)) { BoxedPreHeader(); }

                body?.Invoke();
            }
            GUILayout.EndVertical();
        }

        public static void HorizontalBoxed(string caption, GUISkin skin, Action body, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(caption, skin.box, options);
            {
                body?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        public static void DrawWithColor(Action body, Color color)
        {
            var tempColor = GUI.color;

            GUI.color = color;

            body?.Invoke();

            GUI.color = tempColor;
        }

        public static void DrawBadHolder(string caption, string message, GUISkin skin)
        {
            VerticalBoxed(caption, skin, () =>
            {
                VerticalBoxed("", skin, () =>
                {
                    LabelWithSpace(message, -8);

                    SpacingSeparator();
                });
            });

            SpacingSeparator();
        }

        public static void DrawRGBWithSlidersAndFields(ref Color value, float leftValue, float rightValue, GUISkin skin, string caption = "Color (RGB)", string pattern = "0.0000", int textFieldWidth = 100)
        {
            var r = value.r;
            var g = value.g;
            var b = value.b;

            VerticalBoxed(caption, skin, () =>
            {
                SliderWithField("R: ", leftValue, rightValue, ref r, pattern, textFieldWidth, true);
                SliderWithField("G: ", leftValue, rightValue, ref g, pattern, textFieldWidth, true);
                SliderWithField("B: ", leftValue, rightValue, ref b, pattern, textFieldWidth, true);
            });

            SpacingSeparator();

            value = new Color(r, g, b);
        }

        public static void DrawRGBWithFields(ref Color value, float leftValue, float rightValue, GUISkin skin, string caption = "Color (RGB)", string pattern = "0.0000", int textFieldWidth = 100)
        {
            var r = value.r;
            var g = value.g;
            var b = value.b;

            VerticalBoxed(caption, skin, () =>
            {
                HorizontalBoxed("", skin, () =>
                {
                    DrawFieldValue(ref r, "R: ", pattern, textFieldWidth);
                    DrawFieldValue(ref g, "G: ", pattern, textFieldWidth);
                    DrawFieldValue(ref b, "B: ", pattern, textFieldWidth);
                });
            });

            SpacingSeparator();

            value = new Color(r, g, b);
        }

        public static void DrawRGBAWithSlidersAndFields(ref Color value, float leftValue, float rightValue, GUISkin skin, string caption = "Color (RGBA)", string pattern = "0.0000", int textFieldWidth = 100)
        {
            var r = value.r;
            var g = value.g;
            var b = value.b;
            var a = value.a;

            VerticalBoxed(caption, skin, () =>
            {
                SliderWithField("R: ", leftValue, rightValue, ref r, pattern, textFieldWidth, true);
                SliderWithField("G: ", leftValue, rightValue, ref g, pattern, textFieldWidth, true);
                SliderWithField("B: ", leftValue, rightValue, ref b, pattern, textFieldWidth, true);
                SliderWithField("A: ", leftValue, rightValue, ref a, pattern, textFieldWidth, true);
            });

            SpacingSeparator();

            value = new Vector4(r, g, b, a);
        }

        public static void DrawRGBAWithFields(ref Color value, float leftValue, float rightValue, GUISkin skin, string caption = "Color (RGB)", string pattern = "0.0000", int textFieldWidth = 100)
        {
            var r = value.r;
            var g = value.g;
            var b = value.b;
            var a = value.a;

            VerticalBoxed(caption, skin, () =>
            {
                HorizontalBoxed("", skin, () =>
                {
                    DrawFieldValue(ref r, "R: ", pattern, textFieldWidth);
                    DrawFieldValue(ref g, "G: ", pattern, textFieldWidth);
                    DrawFieldValue(ref b, "B: ", pattern, textFieldWidth);
                    DrawFieldValue(ref a, "A: ", pattern, textFieldWidth);
                });
            });

            SpacingSeparator();

            value = new Color(r, g, b, a);
        }

        public static void DrawVectorWithFields(ref Vector3 value, float leftValue, float rightValue, GUISkin skin, string caption = "Vector", string pattern = "0.0", int textFieldWidth = 100)
        {
            var x = value.x;
            var y = value.y;
            var z = value.z;

            VerticalBoxed(caption, skin, () =>
            {
                HorizontalBoxed("", skin, () =>
                {
                    DrawFieldValue(ref x, "X: ", pattern, textFieldWidth);
                    DrawFieldValue(ref y, "Y: ", pattern, textFieldWidth);
                    DrawFieldValue(ref z, "Z: ", pattern, textFieldWidth);
                });
            });

            SpacingSeparator();

            value = new Vector3(x, y, z);
        }

        public static void DrawVectorWithFields(ref Vector4 value, float leftValue, float rightValue, GUISkin skin, string caption = "Vector", string pattern = "0.0", int textFieldWidth = 100)
        {
            var x = value.x;
            var y = value.y;
            var z = value.z;
            var w = value.w;

            VerticalBoxed(caption, skin, () =>
            {
                HorizontalBoxed("", skin, () =>
                {
                    DrawFieldValue(ref x, "X: ", pattern, textFieldWidth);
                    DrawFieldValue(ref y, "Y: ", pattern, textFieldWidth);
                    DrawFieldValue(ref z, "Z: ", pattern, textFieldWidth);
                    DrawFieldValue(ref w, "W: ", pattern, textFieldWidth);
                });
            });

            SpacingSeparator();

            value = new Vector4(x, y, z, w);
        }

        public static void DrawVectorWithSlidersAndFields(ref Vector3 value, float leftValue, float rightValue, GUISkin skin, string caption = "Vector", string pattern = "0.0", int textFieldWidth = 100)
        {
            var x = value.x;
            var y = value.y;
            var z = value.z;

            VerticalBoxed(caption, skin, () =>
            {
                SliderWithField("X: ", leftValue, rightValue, ref x, pattern, textFieldWidth, true);
                SliderWithField("Y: ", leftValue, rightValue, ref y, pattern, textFieldWidth, true);
                SliderWithField("Z: ", leftValue, rightValue, ref z, pattern, textFieldWidth, true);
            });

            SpacingSeparator();

            value = new Vector3(x, y, z);
        }

        public static void DrawVectorWithSlidersAndFields(ref Vector4 value, float leftValue, float rightValue, GUISkin skin, string caption = "Vector", string pattern = "0.0", int textFieldWidth = 100)
        {
            var x = value.x;
            var y = value.y;
            var z = value.z;
            var w = value.w;

            VerticalBoxed(caption, skin, () =>
            {
                SliderWithField("X: ", leftValue, rightValue, ref x, pattern, textFieldWidth, true);
                SliderWithField("Y: ", leftValue, rightValue, ref y, pattern, textFieldWidth, true);
                SliderWithField("Z: ", leftValue, rightValue, ref z, pattern, textFieldWidth, true);
                SliderWithField("W: ", leftValue, rightValue, ref w, pattern, textFieldWidth, true);
            });

            SpacingSeparator();

            value = new Vector4(x, y, z, w);
        }
    }
}