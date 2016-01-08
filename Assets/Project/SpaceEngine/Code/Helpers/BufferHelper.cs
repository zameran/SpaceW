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

    public static void ReleaseAndDisposeQuadBuffers(Quad quad)
    {
        ReleaseAndDisposeBuffers(quad.QuadGenerationConstantsBuffer, quad.PreOutDataBuffer, quad.OutDataBuffer, quad.ToShaderData);
    }
}