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
public class GUIDrawQuadTexture : MonoBehaviour
{
    public enum TextureType
    {
        Height,
        Normal
    }

    public Planetoid planetoid = null;

    public QuadPosition quadPosition = QuadPosition.Top;
    public TextureType textureType = TextureType.Height;

    public float angle = 0.0f;
    public float scale = 0.5f;
    public float x = 10.0f;
    public float y = 10.0f;

    public bool alphaBelnded = false;

    private void OnGUI()
    {
        //DrawBox();

        if (planetoid != null)
        {
            if (planetoid.MainQuads != null && planetoid.MainQuads.Count != 0)
            {
                foreach (Quad q in planetoid.MainQuads)
                {
                    if (q.HeightTexture == null || q.NormalTexture == null) return;

                    Vector2 size = new Vector2(QS.nVertsPerEdgeSub, QS.nVertsPerEdgeSub);

                    if (q.Position == quadPosition)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(angle, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                        y + QS.nVertsPerEdge * scale));
                        GUI.DrawTexture(new Rect(x, y, size.x * scale, size.y * scale),
                                                 GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                }
            }
        }
    }

    private void DrawBox()
    {
        Vector2 size = new Vector2(QS.nVertsPerEdgeSub * scale, QS.nVertsPerEdgeSub * scale);

        if (planetoid != null)
        {
            if (planetoid.MainQuads != null && planetoid.MainQuads.Count != 0)
            {
                foreach (Quad q in planetoid.MainQuads)
                {
                    Rect topRect = new Rect(x, y, size.x, size.y);
                    Rect leftRect = new Rect(topRect.x - topRect.width, topRect.y, size.x, size.y);
                    Rect rightRect = new Rect(topRect.x + topRect.width, topRect.y, size.x, size.y);
                    Rect backRect = new Rect(topRect.x, topRect.y - topRect.height, size.x, size.y);
                    Rect frontRect = new Rect(topRect.x, topRect.y + topRect.height, size.x, size.y);
                    Rect bottomRect = new Rect(frontRect.x, frontRect.y + frontRect.height, size.x, size.y);

                    if (q.Position == QuadPosition.Top)
                    {
                        GUI.DrawTexture(topRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                    }
                    else if (q.Position == QuadPosition.Bottom)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(180, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                      y + QS.nVertsPerEdge * scale));

                        GUI.DrawTexture(bottomRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                    else if (q.Position == QuadPosition.Left)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(-90, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                      y + QS.nVertsPerEdge * scale));

                        GUI.DrawTexture(leftRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                    else if (q.Position == QuadPosition.Right)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(90, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                     y + QS.nVertsPerEdge * scale));

                        GUI.DrawTexture(rightRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                    else if (q.Position == QuadPosition.Front)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(90, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                     y + QS.nVertsPerEdge * scale));

                        GUI.DrawTexture(frontRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                    else if (q.Position == QuadPosition.Back)
                    {
                        GUI.BeginGroup(new Rect(0, 0, Screen.width * 10, Screen.height * 10));
                        GUIUtility.RotateAroundPivot(-90, new Vector2(x + QS.nVertsPerEdge * scale,
                                                                      y + QS.nVertsPerEdge * scale));

                        GUI.DrawTexture(backRect, GetTexture(q, textureType), ScaleMode.ScaleAndCrop, alphaBelnded);
                        GUI.EndGroup();
                    }
                }
            }
        }
    }

    private RenderTexture GetTexture(Quad q, TextureType textureType)
    {
        switch (textureType)
        {
            case TextureType.Height:
                return q.HeightTexture;
            case TextureType.Normal:
                return q.NormalTexture;
            default: return null;
        }
    }
}