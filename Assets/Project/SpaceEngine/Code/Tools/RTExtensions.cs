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

    public static RenderTexture CreateRTexture(Vector2 size, int depth, RenderTextureFormat format, FilterMode fm, TextureWrapMode twm, bool usemm, int al, bool pot)
    {
        RenderTexture rt;

        rt = new RenderTexture((int)size.x, (int)size.y, depth, format);
        rt.enableRandomWrite = true;
        rt.filterMode = fm;
        rt.wrapMode = twm;
        rt.useMipMap = usemm;
        rt.anisoLevel = al;
        rt.isPowerOfTwo = pot;
        rt.Create();

        return rt;
    }

    public static void ReleaseAndDestroy(this RenderTexture rt)
    {
        rt.Release();
        GameObject.DestroyImmediate(rt);
    }
}