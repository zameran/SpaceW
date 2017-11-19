using System.IO;

using UnityEditor;
using UnityEngine;

public class Reimporter
{
    [MenuItem("SpaceEngine/Reimport Shaders in Folder _%#H")]
    public static void ReImportFolder()
    {
        var realPath = Application.dataPath;

        realPath = realPath.Remove(realPath.Length - 6);

        var selectedPath = realPath + AssetDatabase.GetAssetPath(Selection.activeObject);

        ReimportFiles(realPath, selectedPath, ".compute");
        ReimportFiles(realPath, selectedPath, ".shader");

        AssetDatabase.Refresh();
    }

    private static void ReimportFiles(string dataPath, string path, string extension = ".asset")
    {
        var files = Directory.GetFiles(path, string.Format("*.{0}", extension), SearchOption.AllDirectories);

        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];

            file = file.Replace("\\", "/");
            file = file.Remove(0, dataPath.Length);

            AssetDatabase.ImportAsset(file);
        }
    }
}