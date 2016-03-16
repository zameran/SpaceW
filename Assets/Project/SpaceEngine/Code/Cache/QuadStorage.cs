using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QuadStorage : MonoBehaviour
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