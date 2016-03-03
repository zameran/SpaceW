using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadStorage : MonoBehaviour
{
    public List<QuadCache> Quads = new List<QuadCache>();

    public bool ShouldAdd(Quad.Id id)
    {
        foreach (QuadCache qc in Quads)
        {
            if (qc.ID == id)
                return false;
        }

        return true;
    }

    public QuadCache Add(QuadCache q)
    {
        q.Init();

        if (ShouldAdd(q.ID))
            Quads.Add(q);

        return q;
    }

    public QuadCache GetQuad(int LODLevel, int ID, int Position)
    {
        Quad.Id id = new Quad.Id(LODLevel, ID, Position);

        foreach (QuadCache q in Quads)
        {
            if (q.ID == id)
            {
                return q;
            }
            else
                return null;
        }

        return null;
    }

    public QuadCache GetQuad(Quad.Id id)
    {
        foreach (QuadCache q in Quads)
        {
            if (q.ID == id)
            {
                return q;
            }
            else
                return null;
        }

        return null;
    }

    public QuadCache Remove(QuadCache q)
    {
        if (Quads.Contains(q))
            Quads.Remove(q);

        ClearAllNull();

        return q;
    }

    public void ClearAllNull()
    {
        Quads.RemoveAll(s => s == null);
    }

    private void OnDestroy()
    {
        foreach (QuadCache q in Quads)
        {
            BufferHelper.ReleaseAndDisposeBuffers(q.QuadGenerationConstantsBuffer,
                                                  q.PreOutDataBuffer,
                                                  q.PreOutDataSubBuffer,
                                                  q.OutDataBuffer);
            if (q.HeightTexture != null)
                q.HeightTexture.ReleaseAndDestroy();

            if (q.NormalTexture != null)
                q.NormalTexture.ReleaseAndDestroy();
        }
    }
}