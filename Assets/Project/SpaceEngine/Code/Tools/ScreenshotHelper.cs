using System.IO;

using UnityEngine;

using System.Collections;

[RequireComponent(typeof(Camera))]
public class ScreenshotHelper : MonoBehaviour
{
    [Range(1, 8)]
    public int SuperSize = 3;

    public bool IncludeAlpha = true;

    private bool keyPressed = false;

    private void LateUpdate()
    {
        keyPressed |= Input.GetKeyDown(KeyCode.F12);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (keyPressed)
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

            rt.ReleaseAndDestroy();

            File.WriteAllBytes(Application.dataPath + "/ScreenShot_" + System.DateTime.Now.ToString("yy.MM.dd-hh.mm.ss") + ".png", screenShot.EncodeToPNG());

            keyPressed = false;
        }

        Graphics.Blit(src, dest);
    }
}