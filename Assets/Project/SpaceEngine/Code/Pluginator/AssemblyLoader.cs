using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using ZFramework.Extensions;

using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class AssemblyLoader : MonoBehaviour
{
    private bool Loaded = false;
    private bool ShowGUI = false;

    public int Total = 0;

    public List<AssemblyExternal> ExternalAssemblies = new List<AssemblyExternal>();

    static AssemblyLoader instance;
    public static AssemblyLoader Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.Log("AssemblyLoader Instance get fail!");

                return null;
            }

            return instance;
        }
    }

    private void Awake()
    {
        Init();
    }

    private void OnLevelWasLoaded(int level)
    {
        ShowGUI = !Loaded;

        FirePlugins(ExternalAssemblies);
    }

    private void OnGUI()
    {
        if (ShowGUI)
        {
            GUI.Box(new Rect(Screen.width / 2 - (Screen.width / 1.25f) / 2,
                             Screen.height / 1.25f,
                             Screen.width / 1.25f,
                             15), "");

            GUI.Label(new Rect(Screen.width / 2 - 50,
                               Screen.height / 1.25f - 3,
                               Screen.width / 1.25f,
                               25), string.Format("Loading {0} dll's...", Total));
        }
    }

    private void Init()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);

        Debug.Log("AssemblyLoader started at scene: " + SceneManager.GetActiveScene().buildIndex);

        ShowGUI = !Loaded;

        if (!Loaded)
        {
            DetectAndLoadAssemblies();

            Dumper.DumpAssembliesExternal(ExternalAssemblies);

            Loaded = true;
        }

        if (SceneManager.GetActiveScene().buildIndex == 0 && Loaded)
        {
            FirePlugins(ExternalAssemblies, 0);

            SceneManager.LoadScene(1);
        }
    }

    private void DetectAssembies(out List<string> allPaths)
    {
        string path = PathGlobals.GlobalModFolderPath;

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

        Total = allPaths.Count;

        Debug.Log(string.Format("Assembies Detected: {0}", allPaths.Count));
    }

    private void DetectAndLoadAssemblies()
    {
        List<string> allPaths; DetectAssembies(out allPaths);

        LoadDetectedAssemblies(allPaths);
    }

    private void LoadAssembly(string path)
    {
        try
        {
            Assembly assembly = Assembly.LoadFile(path);

            SpaceAddonAssembly[] attrbutes = assembly.GetCustomAttributes(typeof(SpaceAddonAssembly), false) as SpaceAddonAssembly[];

            if (attrbutes.Length == 0 || attrbutes == null)
            {
                Debug.Log("This is not an adddon assymbly! " + path);
            }
            else
            {
                SpaceAddonAssembly ea = attrbutes[0] as SpaceAddonAssembly;
                List<Type> mb = GetAllSubclassesOf<Type, SpaceAddonMonoBehaviour, MonoBehaviour>(assembly);
                AssemblyExternalTypes aet = new AssemblyExternalTypes(typeof(MonoBehaviour), mb);
                AssemblyExternal ae = new AssemblyExternal(path, ea.Name, ea.Version, assembly, aet);

                ExternalAssemblies.Add(ae);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Load Exception: " + ex.Message);
        }
        finally
        {

        }
    }

    private void LoadDetectedAssemblies(List<string> allPaths)
    {
        if(allPaths == null) { DetectAssembies(out allPaths); Debug.Log("Something wrong with path's array! Detecting assemblies again!"); }

        for (int i = 0; i < allPaths.Count; i++)
        {
            string path = allPaths[i];

            LoadAssembly(path);
        }
    }

    private void FirePlugins(List<AssemblyExternal> ExternalAssemblies, int level)
    {
        foreach (AssemblyExternal assembly in ExternalAssemblies)
        {
            foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
            {
                foreach (Type v in kvp.Value)
                {
                    FirePlugin(v, level);
                }
            }
        }
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

    private void FirePlugin(Type type, int level)
    {
        SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

        if (atr != null)
        {
            if ((int)atr.Startup == level)
            {
                GameObject go = new GameObject(type.Name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.AddComponent(type);
            }
        }
    }

    private void FirePlugin(Type type)
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

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
}