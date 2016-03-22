using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.IO;

public class BuildPostprocessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string texturesPath = "/Resources/Textures/Atmosphere";

        string transmittanceName = "/transmittance.raw";
        string irradianceName = "/irradiance.raw";
        string inscatterName = "/inscatter.raw";

        string transmittancePath = Application.dataPath + texturesPath + transmittanceName;
        string irradiancePath = Application.dataPath + texturesPath + irradianceName;
        string inscatterPath = Application.dataPath + texturesPath + inscatterName;

        string exeName = Path.GetFileName(pathToBuiltProject);
        string exeNameWE = Path.GetFileNameWithoutExtension(pathToBuiltProject);

        string dataPath = pathToBuiltProject.Remove(pathToBuiltProject.Length - exeName.Length, exeName.Length) + exeNameWE + "_Data";

        string resourcesPath = dataPath + "/Resources";

        string atmosphereTexturesPath = resourcesPath + "/Textures/Atmosphere";

        Directory.CreateDirectory(atmosphereTexturesPath);
        Directory.CreateDirectory(dataPath + "/Mods");

        FileUtil.CopyFileOrDirectory(transmittancePath, atmosphereTexturesPath + transmittanceName);
        FileUtil.CopyFileOrDirectory(irradiancePath, atmosphereTexturesPath + irradianceName);
        FileUtil.CopyFileOrDirectory(inscatterPath, atmosphereTexturesPath + inscatterName);
    }
}