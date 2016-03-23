using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using ZFramework.Extensions;

using UnityEngine;
using UnityEngine.SceneManagement;

public class AssemblyLoader : MonoBehaviour
{
    public string AssembliesFolderName = "/Mods";

    private void Awake()
    {
        List<string> allPaths;
        List<Assembly> allAssemblies;
        List<Type> allTypes;
        List<Type> allMonoBehaviours;
        List<Type> allMonoBehavioursToStart;

        DetectAssembies(out allPaths);
        LoadAssemblies(out allAssemblies, out allTypes, out allMonoBehaviours, out allMonoBehavioursToStart, allPaths);

        Dumper.DumpAssembliesInfo(allAssemblies);
        Dumper.DumpTypesNames(allTypes);
    }

    private void DetectAssembies(out List<string> allPaths)
    {
        string path = Application.dataPath + AssembliesFolderName;

        allPaths = new List<string>();

        try
        {
            foreach (string dll in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                allPaths.Add(dll);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Get Files Exception: " + ex.Message);
        }

        Debug.Log(string.Format("Assembies Detected: {0}", allPaths.Count));

        Dumper.DumpStringList(allPaths);
    }

    private void LoadAssemblies(out List<Assembly> allAssemblies,
                                out List<Type> allTypes,
                                out List<Type> allMonoBehaviours,
                                out List<Type> allMonoBehavioursToStart,
                                List<string> allPaths)
    {
        allAssemblies = new List<Assembly>();
        allTypes = new List<Type>();
        allMonoBehaviours = new List<Type>();
        allMonoBehavioursToStart = new List<Type>();

        if (allPaths == null)
        {
            Debug.LogWarning("Path's array is null! Don't try to load without detect!");
            DetectAssembies(out allPaths);
        }

        try
        {
            foreach (string dll in allPaths)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(dll);

                    allAssemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Load Exception: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Get Files Exception: " + ex.Message);
        }

        try
        {
            foreach (Assembly dll in allAssemblies)
            {
                Type[] types = dll.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        ExternalMonoBehaviour atr = AttributeUtils.GetTypeAttribute<ExternalMonoBehaviour>(type);

                        if (atr != null)
                        {
                            allMonoBehavioursToStart.Add(type);
                        }

                        allMonoBehaviours.Add(type);
                    }
                }

                allTypes.AddRange(types);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Get Types Exception: " + ex.Message);
        }

        Debug.Log(string.Format("Types Count: {0}; Base MonoBehaviour Types count: {1}; Plugin MonoBehaviour Types count: {2}", allTypes.Count, allMonoBehaviours.Count, allMonoBehavioursToStart.Count));
    }

    private void FirePlugins(List<Type> types)
    {
        foreach (Type type in types)
        {
            FirePlugin(type);
        }
    }

    private void FirePlugin(Type type)
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        ExternalMonoBehaviour atr = AttributeUtils.GetTypeAttribute<ExternalMonoBehaviour>(type);

        if (atr != null)
        {
            if (currentScene == (int)atr.Startup)
            {
                GameObject go = new GameObject(type.Name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.AddComponent(type);
            }
        }
    }
}