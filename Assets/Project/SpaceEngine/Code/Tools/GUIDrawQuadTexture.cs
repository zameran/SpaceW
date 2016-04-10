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

using UnityEngine;

[ExecuteInEditMode()]
[RequireComponent(typeof(Planetoid))]
public class GUIDrawQuadTexture : MonoBehaviour
{
    public Planetoid planetoid = null;

    public QuadPosition quadPosition = QuadPosition.Top;
    public TextureType textureType = TextureType.Height;
    public float scale = 0.5f;
    public float x = 10.0f;
    public float y = 10.0f;
    public float rotationAngle = 0.0f;

    public bool alphaBelnded = false;

    private void OnGUI()
    {
        if (planetoid != null)
        {
            if (planetoid.MainQuads != null && planetoid.MainQuads.Count != 0)
            {
                foreach (Quad q in planetoid.MainQuads)
                {
                    if (q.HeightTexture == null || q.NormalTexture == null) return;

                    RenderTexture tex = new RenderTexture(10, 10, 24);

                    switch (textureType)
                    {
                        case TextureType.Height:
                            tex = q.HeightTexture;
                            break;
                        case TextureType.Normal:
                            tex = q.NormalTexture;
                            break;
                    }

                    Vector2 dim = new Vector2(tex.width, tex.height);

                    if (q.Position == quadPosition)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
                        GUIUtility.RotateAroundPivot(rotationAngle, new Vector2(x + (dim.x * scale) / 2, y + (dim.y * scale) / 2));
                        GUI.DrawTexture(new Rect(x, y,
                                                 dim.x * scale,
                                                 dim.y * scale),
                                                 tex, ScaleMode.StretchToFill, alphaBelnded);
                        GUI.EndGroup();
                    }
                }
            }
        }
    }

    public Texture2D Rotate(Texture2D image, int centerX, int centerY, float angle)
    {
        var radians = (Mathf.PI / 180) * angle;

        var cos = Mathf.Cos(radians);
        var sin = Mathf.Sin(radians);

        var newImage = new Texture2D(image.width, image.height);

        for (var x = 0; x < image.width; x++)
        {
            for (var y = 0; y < image.height; y++)
            {
                var m = x - centerX;
                var n = y - centerY;
                var j = ((int)(m * cos + n * sin)) + centerX;
                var k = ((int)(n * cos - m * sin)) + centerY;

                if (j >= 0 && j < image.width && k >= 0 && k < image.height)
                {
                    newImage.SetPixel(x, y, image.GetPixel(j, k));
                }
            }
        }

        return newImage;
    }
}

public enum TextureType
{
    Height,
    Normal
}