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

    public List<AssemblyExternal> ExternalAssemblies = new List<AssemblyExternal>();

    private void Awake()
    {
        LoadAssemblies();

        Dumper.DumpAssembliesExternal(ExternalAssemblies);
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

    private void LoadAssemblies()
    {
        List<string> allPaths;

        DetectAssembies(out allPaths);

        try
        {
            foreach (string dll in allPaths)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(dll);
                    ExternalAssembly ea = assembly.GetCustomAttributes(typeof(ExternalAssembly), false)[0] as ExternalAssembly;
                    List<Type> mb = GetAllSubclassesOf<Type, ExternalMonoBehaviour, MonoBehaviour>(assembly);
                    AssemblyExternalTypes aet = new AssemblyExternalTypes(typeof(MonoBehaviour), mb);
                    AssemblyExternal ae = new AssemblyExternal(dll, ea.Name, ea.Version, assembly, aet);

                    ExternalAssemblies.Add(ae);
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
    }

    public List<T> GetAllSubclassesOf<T, U, Y>(Assembly assembly) where T : Type where U : Attribute
    {
        Type[] types = assembly.GetTypes();

        List<T> output = new List<T>();

        foreach (Type type in types)
        {
            if (type.IsSubclassOf(typeof(Y)))
            {
                U atr = AttributeUtils.GetTypeAttribute<U>(type);

                if (atr != null)
                    output.Add(type as T);
            }
        }

        return output;
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