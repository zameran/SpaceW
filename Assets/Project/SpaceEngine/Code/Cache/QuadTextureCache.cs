using UnityEngine;

using System.Collections;

public class QuadTextureCache : QuadCache
{
    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadTextureCache(Quad.Id id) : base(id)
    {

    }

    protected override void Init()
    {
        this.HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { this.HeightTexture, this.NormalTexture });

        base.Init();
    }

    protected override void TransferTo(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() =>
        {
            Graphics.Blit(this.HeightTexture, q.HeightTexture);
            Graphics.Blit(this.NormalTexture, q.NormalTexture);
        });

        base.TransferTo(q);
    }

    protected override void TransferFrom(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() =>
        {
            Graphics.Blit(q.HeightTexture, this.HeightTexture);
            Graphics.Blit(q.NormalTexture, this.NormalTexture);
        });

        base.TransferTo(q);
    }
}