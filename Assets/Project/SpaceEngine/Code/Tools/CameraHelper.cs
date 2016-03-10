using UnityEngine;

public static class CameraHelper
{
    public static Matrix4x4 GetWorldToCamera(this Camera camera)
    {
        return camera.worldToCameraMatrix;
    }

    public static Matrix4x4 GetCameraToWorld(this Camera camera)
    {
        return camera.cameraToWorldMatrix;
    }

    public static Matrix4x4 GetCameraToScreen(this Camera camera)
    {
        Matrix4x4 p = camera.projectionMatrix;
        bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;

        if (d3d)
        {
            if (camera.actualRenderingPath == RenderingPath.DeferredLighting)
            {
                // Invert Y for rendering to a render texture
                for (int i = 0; i < 4; i++)
                {
                    p[1, i] = -p[1, i];
                }
            }

            // Scale and bias depth range
            for (int i = 0; i < 4; i++)
            {
                p[2, i] = p[2, i] * 0.5f + p[3, i] * 0.5f;
            }
        }

        return p;
    }

    public static Matrix4x4 GetScreenToCamera(this Camera camera)
    {
        return camera.GetCameraToScreen().inverse;
    }
}