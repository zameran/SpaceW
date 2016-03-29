using System.IO;

using ZFramework.Extensions;
using ZFramework.Unity.Common;

using UnityEngine;

using Logger = ZFramework.Unity.Common.Logger;

[UseLogger(Category.Data)]
[UseLoggerFile("Misc")]
public sealed class Cleaner : MonoBehaviour
{
    private void Awake()
    {
        foreach (string file in Directory.GetFiles(PathGlobals.GlobalRootPath, "*Log*.txt"))
        {
            if (File.Exists(file))
                File.Delete(file);

            Logger.Log(string.Format("Redutant log file deleted at {0}", file));
        }
    }
}