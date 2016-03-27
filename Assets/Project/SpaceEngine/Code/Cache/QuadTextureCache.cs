using System;

using UnityEngine;

using ZFramework.Unity.Common.Threading;

[Serializable]
public sealed class QuadTextureCache : QuadCache
{
    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadTextureCache(Quad.Id id, QuadStorage owner) : base(id, owner)
    {

    }

    public override void Init()
    {
        this.HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { this.HeightTexture, this.NormalTexture });

        base.Init();
    }

    public override void TransferTo(Quad q)
    {
        if (Owner.Multithreaded)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Graphics.Blit(this.HeightTexture, q.HeightTexture);
                Graphics.Blit(this.NormalTexture, q.NormalTexture);
            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                Graphics.Blit(this.HeightTexture, q.HeightTexture);
                Graphics.Blit(this.NormalTexture, q.NormalTexture);
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
                Graphics.Blit(q.HeightTexture, this.HeightTexture);
                Graphics.Blit(q.NormalTexture, this.NormalTexture);
            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                Graphics.Blit(q.HeightTexture, this.HeightTexture);
                Graphics.Blit(q.NormalTexture, this.NormalTexture);
            });
        }

        base.TransferTo(q);
    }

    public override void OnDestroy()
    {
        if (this.HeightTexture != null)
            this.HeightTexture.ReleaseAndDestroy();

        if (this.NormalTexture != null)
            this.NormalTexture.ReleaseAndDestroy();

        base.OnDestroy();
    }
}