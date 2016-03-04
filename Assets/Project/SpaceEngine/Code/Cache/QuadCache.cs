using System;

using UnityEngine;

[Serializable]
public abstract class QuadCache
{
    public Quad.Id ID;

    public QuadCache(Quad.Id ID)
    {
        this.ID = ID;
    }

    protected virtual void Init()
    {

    }

    protected virtual void TransferTo(Quad q)
    {

    }

    protected virtual void TransferFrom(Quad q)
    {

    }
}