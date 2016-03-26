using System.IO;

using UnityEngine;

public sealed class Cleaner : MonoBehaviour
{
    private void Awake()
    {
        foreach (string file in Directory.GetFiles(PathGlobals.GlobalRootPath, "*Log*.txt"))
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}