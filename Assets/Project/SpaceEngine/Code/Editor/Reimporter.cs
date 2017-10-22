using System.IO;

using UnityEditor;
using UnityEngine;

public class Reimporter
{
    [MenuItem("SpaceEngine/Reimport Folder _%#B")]
    public static void ReImportFolder()
    {
        var realPath = Application.dataPath;

        realPath = realPath.Remove(realPath.Length - 6);

        var selectedPath = realPath + AssetDatabase.GetAssetPath(Selection.activeObject);
        var files = Directory.GetFiles(selectedPath, "*.compute", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];

            file = file.Replace("\\", "/");
            file = file.Remove(0, realPath.Length);

            AssetDatabase.ImportAsset(file);
        }
    }
}