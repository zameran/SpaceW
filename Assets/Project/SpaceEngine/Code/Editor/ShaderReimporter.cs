using UnityEngine;
using UnityEditor;

public class ShaderReimporter : EditorWindow
{
    public ComputeShader copyInscatter1, copyInscatterN, copyIrradiance;
    public ComputeShader inscatter1, inscatterN, inscatterS;
    public ComputeShader irradiance1, irradianceN, transmittance;

    [MenuItem("SpaceEngine/ShaderReimporter")]
    public static void Init()
    {
        ShaderReimporter window = (ShaderReimporter)EditorWindow.GetWindow(typeof(ShaderReimporter));
        window.autoRepaintOnSceneChange = true;
    }

    public void OnGUI()
    {
        GUILayout.Label("Select shaders to reimport...");

        copyInscatter1 = EditorGUILayout.ObjectField("Copy Inscatter 1", copyInscatter1, typeof(ComputeShader), false) as ComputeShader;
        copyInscatterN = EditorGUILayout.ObjectField("Copy Inscatter N", copyInscatterN, typeof(ComputeShader), false) as ComputeShader;
        copyIrradiance = EditorGUILayout.ObjectField("Copy Irradiance ", copyIrradiance, typeof(ComputeShader), false) as ComputeShader;
        inscatter1 = EditorGUILayout.ObjectField("Inscatter 1", inscatter1, typeof(ComputeShader), false) as ComputeShader;
        inscatterN = EditorGUILayout.ObjectField("Inscatter N", inscatterN, typeof(ComputeShader), false) as ComputeShader;
        inscatterS = EditorGUILayout.ObjectField("Inscatter S", inscatterS, typeof(ComputeShader), false) as ComputeShader;
        irradiance1 = EditorGUILayout.ObjectField("Irradiance 1", irradiance1, typeof(ComputeShader), false) as ComputeShader;
        irradianceN = EditorGUILayout.ObjectField("Irradiance N", irradianceN, typeof(ComputeShader), false) as ComputeShader;
        transmittance = EditorGUILayout.ObjectField("Transmittance", transmittance, typeof(ComputeShader), false) as ComputeShader;

        if(GUILayout.Button("Reimport..."))
        {

        }
    }
}