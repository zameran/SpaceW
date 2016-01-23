using UnityEngine;

[ExecuteInEditMode()]
[RequireComponent(typeof(Planetoid))]
public class GUIDrawQuadTexture : MonoBehaviour
{
    public Planetoid planetoid = null;

    public QuadPostion quadPosition = QuadPostion.Top;
    public TextureType textureType = TextureType.Height;
    public float scale = 0.5f;
    public float x = 10.0f;
    public float y = 10.0f;

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

                    if (q.Position == quadPosition)
                    {
                        GUI.DrawTexture(new Rect(x, y, 
                                                 q.HeightTexture.width * scale, 
                                                 q.HeightTexture.height * scale),
                                                 tex, ScaleMode.StretchToFill, false);
                    }
                }
            }
        }
    }
}

public enum TextureType
{
    Height,
    Normal
}