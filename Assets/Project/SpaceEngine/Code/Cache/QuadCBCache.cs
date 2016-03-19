using System;

using UnityEngine;

using ZFramework.Unity.Common.Threading;

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
        if (Owner.Multithreaded)
        {
            Dispatcher.InvokeAsync(() =>
            {

            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {

            });
        }

        base.TransferTo(q);
    }

    public override void TransferFrom(Quad q)
    {
        if (Owner.Multithreaded)
        {
            Dispatcher.InvokeAsync(() =>
            {

            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {

            });
        }

        base.TransferTo(q);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}