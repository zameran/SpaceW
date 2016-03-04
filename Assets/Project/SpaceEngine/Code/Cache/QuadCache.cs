using System;

using UnityEngine;

[Serializable]
public class QuadCache
{
    public Quad.Id ID;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadCache(Quad.Id ID)
    {
        this.ID = ID;
    }

    public void Init()
    {
        this.HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { this.HeightTexture, this.NormalTexture });
    }

    public void TransferTo(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() => 
        {
            Graphics.Blit(this.HeightTexture, q.HeightTexture);
            Graphics.Blit(this.NormalTexture, q.NormalTexture);
        });
    }

    public void TransferFrom(Quad q)
    {
        ThreadScheduler.RunOnMainThread(() =>
        {
            Graphics.Blit(q.HeightTexture, this.HeightTexture);
            Graphics.Blit(q.NormalTexture, this.NormalTexture);
        });
    }
}