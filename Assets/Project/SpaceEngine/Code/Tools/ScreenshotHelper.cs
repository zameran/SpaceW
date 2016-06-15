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

    private bool keyPressed = false;

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

        RenderTexture rt = RTExtensions.CreateRTexture(size, 0, RenderTextureFormat.ARGBFloat, FilterMode.Trilinear, TextureWrapMode.Clamp, false, 6);

        Texture2D screenShot = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGBAFloat, false);

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

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (keyPressed)
        {
            StartCoroutine(WaitOneFrame(() => 
            {
                Texture2D ScreenShotTexture = TakeScreenShot(src, SuperSize, IncludeAlpha);

                if (ScreenShotTexture != null)
                    File.WriteAllBytes(Application.dataPath + "/ScreenShot_" + DateTime.Now.ToString("yy.MM.dd-hh.mm.ss") + ".png", ScreenShotTexture.EncodeToPNG());
                else
                    Debug.Log("ScreenshotHelper.WaitOneScreenShot : ScreenShotTexture is null!");

                Destroy(ScreenShotTexture);

                keyPressed = false;
            }));
        }

        //May cause driver crash on big SuperSize values lol.
        Graphics.Blit(src, dest);
    }
}