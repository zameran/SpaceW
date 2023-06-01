using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SpaceEngine.Tools;

namespace Core.Editor
{
    public static class Tools
    {
        [MenuItem("Assets/Tools/Force Reserialize Asset(s)")]
        private static void ForceReserializeAssets()
        {
            try
            {
                var assetPaths = GetAssetPaths().ToArray();

                AssetDatabase.StartAssetEditing();
                AssetDatabase.ForceReserializeAssets(assetPaths);

                Debug.Log($"{nameof(ForceReserializeAssets)} complete!");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static IEnumerable<string> GetAssetPaths()
        {
            var directoryPaths = new List<string>();
            var filePaths = new List<string>();

            foreach (var assetGuid in Selection.assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                if (Directory.Exists(assetPath))
                {
                    directoryPaths.Add(assetPath);
                }
                else
                {
                    filePaths.Add(assetPath);
                }
            }

            var assetPaths = new HashSet<string>();

            if (directoryPaths.Count > 0)
            {
                AssetDatabase
                    .FindAssets(string.Empty, directoryPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToHashSet(assetPaths);
            }

            filePaths.ToHashSet(assetPaths);

            foreach (var assetPath in assetPaths)
            {
                yield return assetPath;
            }

            assetPaths.Clear();
            directoryPaths.Clear();
            filePaths.Clear();
        }
    }
}