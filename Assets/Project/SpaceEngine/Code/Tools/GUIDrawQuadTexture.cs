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

                    if (tex == null) return;

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

        return newImage;
    }
}

public enum TextureType
{
    Height,
    Normal
}