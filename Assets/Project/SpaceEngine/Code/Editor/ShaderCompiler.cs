using UnityEngine;
using UnityEditor;

using System.Reflection;
using System;

public class ShaderCompiler : EditorWindow
{
    public Shader shader;

    public int mode = 1;
    public int customPlatformsMask = 262143;

    public bool includeAllVariants = true;

    [MenuItem("SpaceEngine/ShaderCompiler")]
    public static void Init()
    {
        ShaderCompiler window = (ShaderCompiler)EditorWindow.GetWindow(typeof(ShaderCompiler));
        window.autoRepaintOnSceneChange = true;
    }

    public void OnGUI()
    {
        GUILayout.Label("Select shader to compile: ");
        shader = (Shader)EditorGUILayout.ObjectField(shader, typeof(Shader), false);

        GUILayout.Label("Select shader compilation mode: ");
        mode = EditorGUILayout.IntField(mode);

        GUILayout.Label("Select shader compilation custom platforms masks: ");
        customPlatformsMask = EditorGUILayout.IntField(customPlatformsMask);

        GUILayout.Label("Should compiled shader contain all shader variants?");
        includeAllVariants = EditorGUILayout.Toggle(includeAllVariants);

        if (shader != null)
        {
            if (GUILayout.Button("Compile..."))
            {
                Type ShaderUtilType = typeof(ShaderUtil);
                MethodInfo OpenCompiledShaderMethod = ShaderUtilType.GetMethod("OpenCompiledShader", BindingFlags.Static | BindingFlags.NonPublic);

                OpenCompiledShaderMethod.Invoke(null, new object[] { shader, mode, customPlatformsMask, true });
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Nothing to compile! - Select shader to work with...", MessageType.Warning);
        }

        EditorGUILayout.HelpBox("This utility simply calls Unity's Internal Core OpenCompiledShader method...\n" +
                                "See Shader Inspector - It have the same button!", MessageType.Info);
    }
}