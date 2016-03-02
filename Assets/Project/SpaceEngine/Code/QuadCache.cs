using System;

using UnityEngine;

[Serializable]
public struct QuadCache
{
    public Quad Parent;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer PreOutDataSubBuffer;
    public ComputeBuffer OutDataBuffer;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadCache(Quad Parent)
    {
        this.Parent = Parent;

        this.QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
        this.PreOutDataBuffer = new ComputeBuffer(QS.nVertsReal, 64);
        this.PreOutDataSubBuffer = new ComputeBuffer(QS.nRealVertsSub, 64);
        this.OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);

        this.HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
    }
}