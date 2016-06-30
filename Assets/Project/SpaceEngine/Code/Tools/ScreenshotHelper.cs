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

using System.IO;

using UnityEngine;

using System;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ScreenshotHelper : MonoBehaviour
{
    [Range(1, 8)]
    public int SuperSize = 3;

    public bool IncludeAlpha = true;

    public KeyCode Key = KeyCode.F12;
    public ScreenshotFormat Format = ScreenshotFormat.PNG;

    private bool keyPressed = false;

    private void Start()
    {

    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        keyPressed |= Input.GetKeyDown(Key);
    }

    private IEnumerator WaitOneFrame(Action OnDone = null)
    {
        yield return Yielders.EndOfFrame;

        if (OnDone != null)
            OnDone();
    }

    private void TakeScreenShot(out Texture2D ScreenShot, RenderTexture src, int SuperSize = 1, bool IncludeAlpha = true, Action OnDone = null)
    {
        ScreenShot = TakeScreenShot(src, SuperSize, IncludeAlpha, OnDone);
    }

    private Texture2D TakeScreenShot(RenderTexture src, int SuperSize = 1, bool IncludeAlpha = true, Action OnDone = null)
    {
        Vector2 size = new Vector2(Screen.width * SuperSize, Screen.height * SuperSize);

        RenderTexture rt = RTExtensions.CreateRTexture(size, 0, RenderTextureFormat.ARGB32, FilterMode.Trilinear, TextureWrapMode.Clamp, false, 6);

        Texture2D screenShot = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);

        Graphics.Blit(src, rt);

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        RenderTexture.active = null;

        if (!IncludeAlpha)
        {
            for (int i = 0; i < screenShot.width; i++)
            {
                for (int j = 0; j < screenShot.height; j++)
                {
                    Color color = screenShot.GetPixel(i, j);
                    color.a = 1.0f;

                    screenShot.SetPixel(i, j, color);
                }
            }
        }

        //Just make sure that we don't eating memory...
        rt.ReleaseAndDestroy();

        if (OnDone != null)
            OnDone();

        return screenShot;
    }

    private void SaveScreenshot(Texture2D ScreenShotTexture, string name = "Screenshot")
    {
        if (ScreenShotTexture != null)
        {
            string file = string.Format("{0}/{1}_{2}", Application.dataPath, name, DateTime.Now.ToString("yy.MM.dd-hh.mm.ss"));

            switch (Format)
            {
                case ScreenshotFormat.JPG:
                    File.WriteAllBytes(file + ".jpg", ScreenShotTexture.EncodeToJPG(100));
                    break;
                case ScreenshotFormat.PNG:
                    File.WriteAllBytes(file + ".png", ScreenShotTexture.EncodeToPNG());
                    break;
            }
        }
        else
            Debug.Log("ScreenshotHelper.WaitOneScreenShot : ScreenShotTexture is null!");
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (keyPressed)
        {
            StartCoroutine(WaitOneFrame(() => 
            {
                Texture2D ScreenShotTexture = TakeScreenShot(src, SuperSize, IncludeAlpha);

                SaveScreenshot(ScreenShotTexture);

                Destroy(ScreenShotTexture);

                keyPressed = false;
            }));
        }

        //May cause driver crash on big SuperSize values lol.
        Graphics.Blit(src, dest);
    }
}