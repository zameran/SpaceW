using UnityEngine;

public static class RTExtensions
{
    public static RenderTexture CreateRTexture(int size)
    {
        RenderTexture rt;

        rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(int size, int depth)
    {
        RenderTexture rt;

        rt = new RenderTexture(size, size, depth, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(int size, int depth, RenderTextureFormat format)
    {
        RenderTexture rt;

        rt = new RenderTexture(size, size, depth, format);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, 24, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size, int depth)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size, int depth, RenderTextureFormat format)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, format);
        rt.enableRandomWrite = true;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size, int depth, RenderTextureFormat format, FilterMode fm, TextureWrapMode twm)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, format);
        rt.enableRandomWrite = true;
        rt.filterMode = fm;
        rt.wrapMode = twm;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size, int depth, RenderTextureFormat format, FilterMode fm, TextureWrapMode twm, int volumeDepth)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, format);
        rt.enableRandomWrite = true;
        rt.filterMode = fm;
        rt.wrapMode = twm;
        rt.useMipMap = false;
        rt.isVolume = true;
        rt.volumeDepth = volumeDepth;
        rt.Create();

        return rt;
    }

    public static RenderTexture CreateRTexture(Vector2 size, int depth, RenderTextureFormat format, FilterMode fm, TextureWrapMode twm, bool usemm, int al)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, format);
        rt.enableRandomWrite = true;
        rt.filterMode = fm;
        rt.wrapMode = twm;
        rt.useMipMap = usemm;
        rt.anisoLevel = al;
        rt.Create();

        return rt;
    }

    public static void ReleaseAndDestroy(this RenderTexture rt)
    {
        rt.Release();
        GameObject.DestroyImmediate(rt);
    }
}