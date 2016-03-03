using System;

using UnityEngine;

[Serializable]
public class QuadCache
{
    public Quad.Id ID;

    public ComputeBuffer QuadGenerationConstantsBuffer;
    public ComputeBuffer PreOutDataBuffer;
    public ComputeBuffer PreOutDataSubBuffer;
    public ComputeBuffer OutDataBuffer;

    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadCache(Quad.Id ID)
    {
        this.ID = ID;
    }

    public void Init()
    {
        this.QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
        this.PreOutDataBuffer = new ComputeBuffer(QS.nVertsReal, 64);
        this.PreOutDataSubBuffer = new ComputeBuffer(QS.nRealVertsSub, 64);
        this.OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);

        this.HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { this.HeightTexture, this.NormalTexture });
    }

    public void TransferTo(Quad q)
    {
        //q.QuadGenerationConstantsBuffer = this.QuadGenerationConstantsBuffer;
        //q.PreOutDataBuffer = this.PreOutDataBuffer;
        //q.PreOutDataSubBuffer = this.PreOutDataSubBuffer;
        //q.OutDataBuffer = this.OutDataBuffer;

        //q.QuadGenerationConstantsBuffer = this.QuadGenerationConstantsBuffer;
        //q.PreOutDataBuffer = this.PreOutDataBuffer;
        //q.PreOutDataSubBuffer = this.PreOutDataSubBuffer;
        //q.OutDataBuffer = this.OutDataBuffer;

        ThreadScheduler.RunOnMainThread(() => 
        {
            //BufferHelper.TransferData<QuadGenerationConstants>(q.QuadGenerationConstantsBuffer, this.QuadGenerationConstantsBuffer);
            //BufferHelper.TransferData<OutputStruct>(q.PreOutDataBuffer, this.PreOutDataBuffer);
            //BufferHelper.TransferData<OutputStruct>(q.PreOutDataSubBuffer, this.PreOutDataSubBuffer);
            //BufferHelper.TransferData<OutputStruct>(q.OutDataBuffer, this.OutDataBuffer);

            Graphics.Blit(this.HeightTexture, q.HeightTexture);
            Graphics.Blit(this.NormalTexture, q.NormalTexture);
        });
    }

    public void TransferFrom(Quad q)
    {
        //this.QuadGenerationConstantsBuffer = q.QuadGenerationConstantsBuffer;
        //this.PreOutDataBuffer = q.PreOutDataBuffer;
        //this.PreOutDataSubBuffer = q.PreOutDataSubBuffer;
        //this.OutDataBuffer = q.OutDataBuffer;

        ThreadScheduler.RunOnMainThread(() =>
        {
            //BufferHelper.TransferData<QuadGenerationConstants>(this.QuadGenerationConstantsBuffer, q.QuadGenerationConstantsBuffer);
            //BufferHelper.TransferData<OutputStruct>(this.PreOutDataBuffer, q.PreOutDataBuffer);
            //BufferHelper.TransferData<OutputStruct>(this.PreOutDataSubBuffer, q.PreOutDataSubBuffer);
            //BufferHelper.TransferData<OutputStruct>(this.OutDataBuffer, q.OutDataBuffer);

            Graphics.Blit(q.HeightTexture, this.HeightTexture);
            Graphics.Blit(q.NormalTexture, this.NormalTexture);
        });
    }
}