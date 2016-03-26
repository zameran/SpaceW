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
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalConfigFolderName); ;
        }
    }

    public static string GlobalModFolderPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalModFolderName); ;
        }
    }

    public static string GlobalMainLogPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalMainLogFileName); ;
        }
    }

    public static string GlobalRootPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../");
        }
    }

    public static string GlobalConfigFolderPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalConfigFolderName);
    }

    public static string GlobalModFolderPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalModFolderName);
    }

    public static string GlobalMainLogPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalMainLogFileName); ;
    }
}
