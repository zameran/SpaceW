using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DebugDrawDepth : MonoBehaviour
{
    public Shader depthShader;

    public Transform sunTransform;

    public RenderTexture DepthTexture;

    private GameObject depthCamera;
    private Camera depthCameraComponent;

    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        DepthTexture = new RenderTexture((int)(Screen.width * 1), (int)(Screen.height * 1), 16, RenderTextureFormat.RFloat);

        DepthTexture.filterMode = FilterMode.Bilinear;
        DepthTexture.useMipMap = false;
        DepthTexture.Create();
    }

    private void Update()
    {
        Shader.SetGlobalVector("_Godray_WorldSunDir", sunTransform.position - transform.position);
    }

    private void OnGUI()
    {
        if (DepthTexture != null)
            GUI.DrawTexture(new Rect(5, 5, Screen.width * 0.35f, Screen.height * 0.35f), DepthTexture, ScaleMode.ScaleAndCrop, false);
    }

    private void OnPreRender()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!depthCamera)
        {
            depthCamera = new GameObject("CustomDepthCamera");
            depthCameraComponent = depthCamera.AddComponent<Camera>();

            depthCameraComponent.CopyFrom(CameraHelper.Main());

            depthCameraComponent.farClipPlane = CameraHelper.Main().farClipPlane;
            depthCameraComponent.nearClipPlane = CameraHelper.Main().nearClipPlane;
            depthCameraComponent.depthTextureMode = DepthTextureMode.None;

            depthCameraComponent.transform.parent = CameraHelper.Main().transform;

            depthCameraComponent.enabled = false;
        }

        depthCameraComponent.CopyFrom(CameraHelper.Main());
        depthCameraComponent.enabled = false;

        bool renderDepthBuffer = true;

        if (renderDepthBuffer)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = DepthTexture;

            GL.Clear(false, true, Color.black);

            depthCameraComponent.targetTexture = DepthTexture;
            depthCameraComponent.RenderWithShader(depthShader, "RenderType");

            RenderTexture.active = rt;
        }

        Graphics.Blit(source, destination);
    }
}