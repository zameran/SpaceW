using UnityEngine;

using System;

[Serializable]
public class QuadFullCache : QuadCache
{
    public QuadCBCache QuadComputeBuffers;
    public QuadTextureCache QuadTextures;

    public QuadFullCache(Quad.Id id, QuadStorage owner) : base(id, owner)
    {
        QuadComputeBuffers = new QuadCBCache(id, owner);
        QuadTextures = new QuadTextureCache(id, owner);
    }

    public override void Init()
    {
        QuadComputeBuffers.Init();
        QuadTextures.Init();

        base.Init();
    }

    public override void TransferTo(Quad q)
    {
        QuadCacheUtility.BeginTransfer();

        QuadComputeBuffers.TransferTo(q);
        QuadTextures.TransferTo(q);

        base.TransferTo(q);

        QuadCacheUtility.EndTransfer();
    }

    public override void TransferFrom(Quad q)
    {
        QuadCacheUtility.BeginTransfer();

        QuadComputeBuffers.TransferFrom(q);
        QuadTextures.TransferFrom(q);

        base.TransferTo(q);

        QuadCacheUtility.EndTransfer();
    }

    public override void OnDestroy()
    {
        QuadComputeBuffers.OnDestroy();
        QuadTextures.OnDestroy();

        base.OnDestroy();
    }
}