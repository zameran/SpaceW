using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public sealed class QuadStorage : MonoBehaviour
{
    public bool Multithreaded = false;
    public bool Working = false;

    public List<QuadTextureCache> TexturesCache = new List<QuadTextureCache>();

    public void AddToTexturesCache(Quad q)
    {
        Working = true;

        if (!ExistInTexturesCache(q))
        {
            QuadTextureCache qtc = new QuadTextureCache(q.GetId(), this);
            qtc.Init();
            qtc.TransferFrom(q);

            TexturesCache.Add(qtc);
        }

        Working = false;
    }

    public void GetFromTexturesCache(Quad q)
    {
        Working = true;

        QuadTextureCache qtc = TexturesCache.Find(s => s.ID.Equals(q.GetId()));
        qtc.TransferTo(q);

        Working = false;
    }

    public void LinkQuadToCache(QuadCBCache qcbc, Quad q)
    {

    }

    public void LinkQuadToCache(QuadTextureCache qtc, Quad q)
    {

    }

    public void LinkQuadToCache(QuadFullCache qfc, Quad q)
    {

    }

    public void LinkCacheToQuad(Quad q, QuadCBCache qcbc)
    {

    }

    public void LinkCacheToQuad(Quad q, QuadTextureCache qtc)
    {

    }

    public void LinkCacheToQuad(Quad q, QuadFullCache qfc)
    {

    }

    public bool ExistInTexturesCache(Quad q)
    {
        return TexturesCache.Any(s => s.ID.Equals(q.GetId()));
    }

    private void OnDestroy()
    {
        foreach (QuadTextureCache q in TexturesCache)
        {
            if (q != null)
                q.OnDestroy();
        }
    }
}