using UnityEngine;

[ExecuteInEditMode]
public class HardwareInfoGUI : MonoBehaviour
{
    public Rect debugInfoBounds = new Rect(10, 10, 500, 500);

    void OnGUI()
    {
        GUILayout.BeginArea(debugInfoBounds);

        GUILayoutExtensions.LabelWithSpace("Device Type: " + SystemInfo.deviceType, -10);
        GUILayoutExtensions.LabelWithSpace("Operation System: " + SystemInfo.operatingSystem, -10);
        GUILayoutExtensions.LabelWithSpace("Unity Version: " + Application.unityVersion, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device: " + SystemInfo.graphicsDeviceName, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device API: " + SystemInfo.graphicsDeviceVersion, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device ID: " + SystemInfo.graphicsDeviceID, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Memory Size: " + SystemInfo.graphicsMemorySize, -10);
        GUILayoutExtensions.LabelWithSpace("Supported Shader Level: " + SystemInfo.graphicsShaderLevel, -10);

        GUILayoutExtensions.LabelWithSpace("CPU: " + SystemInfo.processorType, -10);
        GUILayoutExtensions.LabelWithSpace("CPU Cores Count (Threads Count): " + SystemInfo.processorCount, -10);
        GUILayoutExtensions.LabelWithSpace("CPU Current Frequency: " + SystemInfo.processorFrequency + "Hz", -10);

        GUILayoutExtensions.LabelWithSpace("RAM: " + SystemInfo.systemMemorySize, -10);

        GUILayoutExtensions.LabelWithSpace("Maximum Texture Size: " + SystemInfo.maxTextureSize, -10);
        GUILayoutExtensions.LabelWithSpace("Non-Power-Of-Two Texture Support: " + SystemInfo.npotSupport, -10);

        DisplaySupport("ComputeShaders", SystemInfo.supportsComputeShaders);
        DisplaySupport("RenderTextures", SystemInfo.supportsRenderTextures);
        DisplaySupport("3DTextures", SystemInfo.supports3DTextures);
        DisplaySupport("Graphics Multithreading: ", SystemInfo.graphicsMultiThreaded);
        DisplaySupport("ARGBFloat", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat));
        DisplaySupport("Depth Textures", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth));
        GUILayout.EndArea();
    }

    private void DisplaySupport(string name, bool supported)
    {
        if (supported)
        {
            GUILayoutExtensions.LabelWithSpace(name + " Supported", -10);
        }
        else
        {
            GUILayoutExtensions.LabelWithSpace(name + " Not Supported", -10);
        }
    }
}