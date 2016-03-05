using System;

using UnityEngine;

[Serializable]
public class QuadCBCache : QuadCache
{
    public QuadCBCache(Quad.Id id, QuadStorage owner) : base(id, owner)
    {

    }

    public override void Init()
    {
        base.Init();
    }

    public override void TransferTo(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() =>
        {

        });

        base.TransferTo(q);
    }

    public override void TransferFrom(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() =>
        {

        });

        base.TransferTo(q);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}