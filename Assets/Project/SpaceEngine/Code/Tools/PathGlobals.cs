using System;
using System.IO;

using UnityEngine;

public static class PathGlobals
{
    public const string GlobalConfigFolderName = "Config";
    public const string GlobalModFolderName = "Mods";
    public const string GlobalMainLogFileName = "Log.txt";

    public static string GlobalConfigFolderPath
    {
        get
        {
            string path = Application.dataPath + "/../" + GlobalConfigFolderName;
            string outputPath = Path.GetFullPath(path);

            return outputPath;
        }
    }

    public static string GlobalModFolderPath
    {
        get
        {
            string path = Application.dataPath + "/../" + GlobalModFolderName;
            string outputPath = Path.GetFullPath(path);

            return outputPath;
        }
    }

    public static string GlobalMainLogPath
    {
        get
        {
            string path = Application.dataPath + "/../" + GlobalMainLogFileName;
            string outputPath = Path.GetFullPath(path);

            return outputPath;
        }
    }

    public static string GlobalConfigFolderPathEditor(string dataPath)
    {
        string path = dataPath + "/../" + GlobalConfigFolderName;
        string outputPath = Path.GetFullPath(path);

        return outputPath;
    }

    public static string GlobalModFolderPathEditor(string dataPath)
    {
        string path = dataPath + "/../" + GlobalModFolderName;
        string outputPath = Path.GetFullPath(path);

        return outputPath;
    }

    public static string GlobalMainLogPathEditor(string dataPath)
    {
        string path = dataPath + "/../" + GlobalMainLogFileName;
        string outputPath = Path.GetFullPath(path);

        return outputPath;
    }
}
