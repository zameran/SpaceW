#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

[RequireComponent(typeof(Camera))]
[UseLogger(LoggerCategory.Data)]
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

    [HideInInspector]
    public bool KeyPressed = false;

    public Vector2 ScreenSize { get { return new Vector2(Screen.width, Screen.height); } }
    public Vector2 ScreenShotSize { get { return ScreenSize * SuperSize; } }

    private RenderTexture Buffer;

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
        KeyPressed |= Input.GetKeyDown(Key);
    }

    private static Texture2D GetRTPixels(RenderTexture rt)
    {
        var currentActiveRT = RenderTexture.active;

        RenderTexture.active = rt;

        var texture = new Texture2D(rt.width, rt.height);

        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);

        RenderTexture.active = currentActiveRT;

        return texture;
    }

    private void SaveScreenshot(Texture2D screenShotTexture, string fileName = "Screenshot")
    {
        if (screenShotTexture != null)
        {
            var filePath = string.Format("{0}/{1}_{2:yy.MM.dd-hh.mm.ss}_{3}", Application.dataPath, fileName, DateTime.Now, (int)UnityEngine.Random.Range(0.0f, 100.0f));

            switch (Format)
            {
                case ScreenshotFormat.JPG:
                    File.WriteAllBytes(filePath + ".jpg", screenShotTexture.EncodeToJPG(100));
                    break;
                case ScreenshotFormat.PNG:
                    File.WriteAllBytes(filePath + ".png", screenShotTexture.EncodeToPNG());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Logger.Log(string.Format("ScreenshotHelper.SaveScreenshot: Screenshot Saved. {0}", filePath));

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        else
            Logger.LogError("ScreenshotHelper.SaveScreenshot: screenShotTexture is null!");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // NOTE : May cause driver crash on big SuperSize values, lol.
        if (KeyPressed)
        {
            Buffer = RTExtensions.CreateRTexture(ScreenShotSize, 0, RenderTextureFormat.ARGB32, FilterMode.Trilinear, TextureWrapMode.Clamp, false, 6);

            Graphics.Blit(source, Buffer);

            var screenShotTexture = GetRTPixels(Buffer);

            SaveScreenshot(screenShotTexture);

            Destroy(screenShotTexture);
            Destroy(Buffer);

            KeyPressed = false;
        }

        Graphics.Blit(source, destination);
    }
}