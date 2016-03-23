using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using ZFramework.Extensions;

using UnityEngine;
using UnityEngine.SceneManagement;

public class AssemblyLoader : MonoBehaviour
{
    private bool Loaded = false;

    public string AssembliesFolderName = "/Mods";

    public List<AssemblyExternal> ExternalAssemblies = new List<AssemblyExternal>();

    private void Awake()
    {
        Init();
    }

    private void OnLevelWasLoaded(int level)
    {
        Init();
    }

    private void Init()
    {
        DontDestroyOnLoad(this.gameObject);

        Debug.Log("AssemblyLoader started at scene: " + SceneManager.GetActiveScene().buildIndex);

        if (!Loaded)
        {
            LoadAssemblies();

            Dumper.DumpAssembliesExternal(ExternalAssemblies);

            Loaded = true;
        }

        FirePlugins(ExternalAssemblies);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SceneManager.LoadScene(1);
        }
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
                    SpaceAssembly ea = assembly.GetCustomAttributes(typeof(SpaceAssembly), false)[0] as SpaceAssembly;
                    List<Type> mb = GetAllSubclassesOf<Type, SpaceMonoBehaviour, MonoBehaviour>(assembly);
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

    private void FirePlugins(List<AssemblyExternal> ExternalAssemblies)
    {
        foreach (AssemblyExternal assembly in ExternalAssemblies)
        {
            foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
            {
                foreach (Type v in kvp.Value)
                {
                    FirePlugin(v);
                }
            }
        }
    }

    private void FirePlugin(Type type)
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        SpaceMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceMonoBehaviour>(type);

        if (atr != null)
        {
            if ((int)atr.Startup == currentScene)
            {
                GameObject go = new GameObject(type.Name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.AddComponent(type);
            }
        }
    }
}