using System;
using System.IO;

using UnityEngine;

public static class PathGlobals
{
    public static string GlobalConfigFolderPath
    {
        get
        {
            string path = Application.dataPath + "/../Config/";
            string outputPath = Path.GetFullPath(path);

            return outputPath;
        }
    }

    public static string GlobalModFolderPath
    {
        get
        {
            string path = Application.dataPath + "/../Mods";
            string outputPath = Path.GetFullPath(path);

            return outputPath;
        }
    }

    public static string GlobalModFolderPathEditor(string dataPath)
    {
        string path = dataPath + "/../Mods";
        string outputPath = Path.GetFullPath(path);

        return outputPath;
    }

    public static string GlobalConfigFolderPathEditor(string dataPath)
    {
        string path = dataPath + "/../Config";
        string outputPath = Path.GetFullPath(path);

        return outputPath;
    }
}
