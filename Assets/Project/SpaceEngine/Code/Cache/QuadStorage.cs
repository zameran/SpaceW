using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class QuadStorage : MonoBehaviour
{
    public List<QuadCache> Cache = new List<QuadCache>();

    public int CacheCurrentSize { get { return Cache.Count; } }

    public bool ShouldAddToCache(Quad.Id id)
    {
        return !Cache.Any(item => item.ID.Equals(id));
    }

    public QuadCache AddToCache(QuadCache q)
    {
        q.Init();

        //if (ShouldAddToCache(q.ID))
            Cache.Add(q);

            //Cache = Cache.Distinct().ToList();

        return q;
    }

    public bool HaveDataInCache(Quad.Id id)
    {
        for (int i = 0; i < Cache.Count; i++)
        {
            if (Cache[i].ID.Equals(id))
                return false;
        }

        return true;
    }

    public QuadCache TakeCache(Quad.Id id)
    {
        return Cache.Find(s => s.ID.Equals(id));
    }

    public QuadCache RemoveFromCache(QuadCache q)
    {
        if (Cache.Contains(q))
            Cache.Remove(q);

        ClearAllNullCache();

        return q;
    }

    public void ClearAllNullCache()
    {
        Cache.RemoveAll(s => s == null);
    }

    private void OnDestroy()
    {
        foreach (QuadCache q in Cache)
        {
            if (q.HeightTexture != null)
                q.HeightTexture.ReleaseAndDestroy();

            if (q.NormalTexture != null)
                q.NormalTexture.ReleaseAndDestroy();
        }
    }
}