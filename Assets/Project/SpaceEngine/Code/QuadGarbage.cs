using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public struct QuadGarbage
{
    public Quad Parent;

    public List<Texture2D> Textures2D;
    public List<RenderTexture> RTextures;

    public QuadGarbage(Quad Parent)
    {
        this.Parent = Parent;

        this.Textures2D = new List<Texture2D>();
        this.RTextures = new List<RenderTexture>();
    }
}