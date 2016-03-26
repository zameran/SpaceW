using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using ZFramework.Extensions;
using ZFramework.Unity.Common;

using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = ZFramework.Unity.Common.Logger;

[UseLogger(Category.Data)]
[UseLoggerFile("AssemblyLoader")]
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
                Logger.Log("AssemblyLoader Instance get fail!");

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
        if (SceneManager.GetActiveScene().buildIndex != 0 && Loaded) FirePlugins(ExternalAssemblies);
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

        Logger.Log("AssemblyLoader Initiated at scene №: " + SceneManager.GetActiveScene().buildIndex);

        if (!Loaded)
        {
            DetectAndLoadAssemblies();
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
            Logger.Log("DetectAssembies Exception: " + ex.Message);
        }

        Total = allPaths.Count;

        Logger.Log(string.Format("Assembies Detected: {0}", allPaths.Count));
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
                Logger.Log("This is not an adddon assembly! " + path);
            }
            else
            {
                SpaceAddonAssembly ea = attrbutes[0] as SpaceAddonAssembly;
                List<Type> mb = GetAllSubclassesOf<Type, SpaceAddonMonoBehaviour, MonoBehaviour>(assembly);
                AssemblyExternalTypes aet = new AssemblyExternalTypes(typeof(MonoBehaviour), mb);
                AssemblyExternal ae = new AssemblyExternal(path, ea.Name, ea.Version, assembly, aet);

                FireHotPlugin(ae);

                ExternalAssemblies.Add(ae);
            }
        }
        catch (Exception ex)
        {
            Logger.Log("LoadAssembly Exception: " + ex.Message);
        }
        finally
        {

        }
    }

    private void LoadDetectedAssemblies(List<string> allPaths)
    {
        if(allPaths == null) { DetectAssembies(out allPaths); Logger.Log("Something wrong with path's array! Detecting assemblies again!"); }

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

        Logger.Log("Plugins fired at scene №: " + level);
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

        Logger.Log("Plugins fired at scene №: " + SceneManager.GetActiveScene().buildIndex);
    }

    private void FireHotPlugin(AssemblyExternal Addon)
    {
        foreach (KeyValuePair<Type, List<Type>> kvp in Addon.Types)
        {
            foreach (Type v in kvp.Value)
            {
                FirePlugin(v, -1);
            }
        }

        Logger.Log(string.Format("Hot plugin {0} fired at scene №: {1}", Addon.Name, SceneManager.GetActiveScene().buildIndex));
    }

    private void FirePlugin(Type type, int level)
    {
        SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

        if (atr != null)
        {
            if ((int)atr.EntryPoint == level)
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
            if ((int)atr.EntryPoint == currentScene)
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