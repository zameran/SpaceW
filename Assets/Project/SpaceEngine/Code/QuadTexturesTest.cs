using UnityEngine;

public class QuadTexturesTest : MonoBehaviour
{
    public RenderTexture tex;

    public ComputeShader shader;

    [ContextMenu("RunShader")]
    void RunShader()
    {
        int kernelHandle = shader.FindKernel("CSMain");

        tex = new RenderTexture(512, 512, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetTexture(kernelHandle, "Result", tex);
        shader.Dispatch(kernelHandle, 512 / 8, 512 / 8, 1);
    }
}