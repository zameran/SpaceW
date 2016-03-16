using UnityEngine;

[ExecuteInEditMode]
public class DebugHardwareInfoGUI : DebugGUI
{
    protected override void Awake()
    {
        base.Awake();

        SI.Get();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        GUILayout.BeginArea(debugInfoBounds);

        GUILayoutExtensions.LabelWithSpace("Device Type: " + SI.deviceType, -10);
        GUILayoutExtensions.LabelWithSpace("Operation System: " + SI.operatingSystem, -10);
        GUILayoutExtensions.LabelWithSpace("Unity Version: " + SI.unityVersion, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device: " + SI.graphicsDeviceName, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device API: " + SI.graphicsDeviceVersion, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Device ID: " + SI.graphicsDeviceID, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Memory Size: " + SI.graphicsMemorySize, -10);
        GUILayoutExtensions.LabelWithSpace("Supported Shader Level: " + SI.graphicsShaderLevel, -10);

        GUILayoutExtensions.LabelWithSpace("CPU: " + SI.processorType, -10);
        GUILayoutExtensions.LabelWithSpace("CPU Cores Count (Threads Count): " + SI.processorCount, -10);
        GUILayoutExtensions.LabelWithSpace("CPU Current Frequency: " + SI.processorFrequency + "Hz", -10);

        GUILayoutExtensions.LabelWithSpace("RAM: " + SI.systemMemorySize, -10);

        GUILayoutExtensions.LabelWithSpace("Maximum Texture Size: " + SI.maxTextureSize, -10);
        GUILayoutExtensions.LabelWithSpace("Non-Power-Of-Two Texture Support: " + SI.npotSupport, -10);

        GUILayoutExtensions.LabelWithSpace("ComputeShaders: " + SI.supportsComputeShaders, -10);
        GUILayoutExtensions.LabelWithSpace("RenderTextures: " + SI.supportsRenderTextures, -10);
        GUILayoutExtensions.LabelWithSpace("3DTextures: " + SI.supports3DTextures, -10);
        GUILayoutExtensions.LabelWithSpace("Graphics Multithreading: " + SI.graphicsMultiThreaded, -10);
        GUILayoutExtensions.LabelWithSpace("ARGBFloat: " + SI.supportsARGBFloatTextureFormat, -10);
        GUILayoutExtensions.LabelWithSpace("Depth Textures: " + SI.supportsDepthRenderTextureFormat, -10);

        GUILayout.EndArea();
    }
}

public static class SI
{
    public static string deviceType;
    public static string operatingSystem;
    public static string unityVersion;

    public static string graphicsDeviceName;
    public static string graphicsDeviceVersion;
    public static string graphicsDeviceID;
    public static string graphicsMemorySize;
    public static string graphicsShaderLevel;

    public static string processorType;
    public static string processorCount;
    public static string processorFrequency;

    public static string systemMemorySize;

    public static string maxTextureSize;
    public static string npotSupport;

    public static string supportsComputeShaders;
    public static string supportsRenderTextures;
    public static string supports3DTextures;
    public static string graphicsMultiThreaded;
    public static string supportsARGBFloatTextureFormat;
    public static string supportsDepthRenderTextureFormat;

    public static void Get()
    {
        deviceType = SystemInfo.deviceType.ToString();
        operatingSystem = SystemInfo.operatingSystem;
        unityVersion = Application.unityVersion;
        graphicsDeviceName = SystemInfo.graphicsDeviceName;
        graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
        graphicsDeviceID = SystemInfo.graphicsDeviceID.ToString();
        graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
        graphicsShaderLevel = SystemInfo.graphicsShaderLevel.ToString();

        processorType = SystemInfo.processorType;
        processorCount = SystemInfo.processorCount.ToString();
        processorFrequency = SystemInfo.processorFrequency.ToString();

        systemMemorySize = SystemInfo.systemMemorySize.ToString();

        maxTextureSize = SystemInfo.maxTextureSize.ToString();
        npotSupport = SystemInfo.npotSupport.ToString();

        supportsComputeShaders = Supports(SystemInfo.supportsComputeShaders);
        supportsRenderTextures = Supports(SystemInfo.supportsRenderTextures);
        supports3DTextures = Supports(SystemInfo.supports3DTextures);
        graphicsMultiThreaded = Supports(SystemInfo.graphicsMultiThreaded);
        supportsARGBFloatTextureFormat = Supports(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat));
        supportsDepthRenderTextureFormat = Supports(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth));
    }

    private static string Supports(bool supported)
    {
        if (supported)
        {
            return "Supported";
        }
        else
        {
            return "Not Supported";
        }
    }
}