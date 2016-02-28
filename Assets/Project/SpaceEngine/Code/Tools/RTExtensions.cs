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

    public static void ReleaseAndDestroy(this RenderTexture rt)
    {
        rt.Release();
        GameObject.DestroyImmediate(rt);
    }
}