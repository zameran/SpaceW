using UnityEngine;

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
        QuadComputeBuffers.TransferTo(q);
        QuadTextures.TransferTo(q);

        base.TransferTo(q);
    }

    public override void TransferFrom(Quad q)
    {
        QuadComputeBuffers.TransferFrom(q);
        QuadTextures.TransferFrom(q);

        base.TransferTo(q);
    }

    public override void OnDestroy()
    {
        QuadComputeBuffers.OnDestroy();
        QuadTextures.OnDestroy();

        base.OnDestroy();
    }
}