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

using SpaceEngine.Core.Debugging;

using System;
using System.Collections;
using System.IO;

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

[RequireComponent(typeof(Camera))]
[UseLogger(Category.Data)]
[UseLoggerFile("Log")]
public class ScreenshotHelper : MonoSingleton<ScreenshotHelper>
{
    public enum ScreenshotFormat
    {
        PNG,
        JPG
    }

    [Range(1, 8)]
    public int SuperSize = 3;

    public KeyCode Key = KeyCode.F12;
    public ScreenshotFormat Format = ScreenshotFormat.PNG;

    private bool keyPressed = false;

    private void Awake()
    {
        Instance = this;
    }

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

    private void TakeScreenShot(out Texture2D ScreenShot, RenderTexture src, int SuperSize = 1, Action OnDone = null)
    {
        ScreenShot = TakeScreenShot(src, SuperSize, OnDone);
    }

    private Texture2D TakeScreenShot(RenderTexture src, int SuperSize = 1, Action OnDone = null)
    {
        var size = new Vector2(Screen.width * SuperSize, Screen.height * SuperSize);
        var renderTextureBuffer = RTExtensions.CreateRTexture(size, 0, RenderTextureFormat.ARGB32, FilterMode.Trilinear, TextureWrapMode.Clamp, false, 6);
        var textureBuffer = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);

        RenderTexture.active = renderTextureBuffer;
        textureBuffer.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        RenderTexture.active = null;

        //Just make sure that we don't eating memory...
        renderTextureBuffer.ReleaseAndDestroy();

        if (OnDone != null)
            OnDone();

        return textureBuffer;
    }

    private void SaveScreenshot(Texture2D screenShotTexture, string name = "Screenshot")
    {
        if (screenShotTexture != null)
        {
            string file = string.Format("{0}/{1}_{2}_{3}", Application.dataPath, name, DateTime.Now.ToString("yy.MM.dd-hh.mm.ss"), (int)UnityEngine.Random.Range(0.0f, 100.0f));

            switch (Format)
            {
                case ScreenshotFormat.JPG:
                    File.WriteAllBytes(file + ".jpg", screenShotTexture.EncodeToJPG(100));
                    break;
                case ScreenshotFormat.PNG:
                    File.WriteAllBytes(file + ".png", screenShotTexture.EncodeToPNG());
                    break;
            }

            Logger.Log(string.Format("ScreenshotHelper: Screenshot Saved. {0}", file));
        }
        else
            Logger.LogError("ScreenshotHelper: screenShotTexture is null!");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //May cause driver crash on big SuperSize values, lol.
        if (keyPressed)
        {
            StartCoroutine(WaitOneFrame(() =>
            {
                var ScreenShotTexture = TakeScreenShot(source, SuperSize);

                SaveScreenshot(ScreenShotTexture);

                Destroy(ScreenShotTexture);

                keyPressed = false;
            }));
        }

        Graphics.Blit(source, destination);
    }
}