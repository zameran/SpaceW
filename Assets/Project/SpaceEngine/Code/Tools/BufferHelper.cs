using System;
using UnityEngine;

public static class BufferHelper
{
    public static void ReleaseBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
            }
        }
    }

    public static void DisposeBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Dispose();
            }
        }
    }

    public static void ReleaseAndDisposeBuffer(this ComputeBuffer buffer)
    {
        if (buffer != null)
        {
            buffer.Release();
            buffer.Dispose();
        }
    }

    public static void ReleaseAndDisposeBuffers(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
                buffers[i].Dispose();
            }
        }
    }

    public static void ReleaseAndDisposeCacheBuffers(QuadCBCache cache)
    {

    }

    public static void ReleaseAndDisposeCacheBuffers(QuadFullCache cache)
    {

    }

    public static void ReleaseAndDisposeQuadBuffers(Quad quad)
    {
        ReleaseAndDisposeBuffers(quad.QuadGenerationConstantsBuffer, quad.PreOutDataBuffer, quad.PreOutDataSubBuffer, quad.OutDataBuffer);
    }

    public static void ReleaseQuadBuffers(Quad quad)
    {
        ReleaseBuffers(quad.QuadGenerationConstantsBuffer, quad.PreOutDataBuffer, quad.PreOutDataSubBuffer, quad.OutDataBuffer);
    }

    public static void GetData(this ComputeBuffer buffer, Array data, Action onGetDataAction)
    {
        buffer.GetData(data);

        if (onGetDataAction != null)
            onGetDataAction();
    }

    public static void TransferData<T>(ComputeBuffer from, ComputeBuffer to)
    {
        T[] data = new T[from.count];
        from.GetData(data);
        to.SetData(data);
    }
}