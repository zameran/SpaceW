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
[UseLoggerFile("Loader")]
public sealed class AssemblyLoader : Loader
{
    private bool Loaded = false;
    private bool ShowGUI = true;

    public int TotalDetected = 0;
    public int TotalLoaded = 0;

    public List<AssemblyExternal> ExternalAssemblies = new List<AssemblyExternal>();

    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();

        Delay(2, () => { Pass(); });
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        if (ShowGUI)
        {
            GUI.Box(new Rect(Screen.width / 2 - (Screen.width / 1.25f) / 2,
                             Screen.height / 1.25f,
                             Screen.width / 1.25f,
                             15), "");

            GUI.Label(new Rect(Screen.width / 2 - 50,
                               Screen.height / 1.25f - 3,
                               Screen.width / 1.25f,
                               25), string.Format("Loading {0}/{1} dll's...", TotalLoaded, TotalDetected));
        }
    }

    protected override void OnLevelWasLoaded(int level)
    {
        base.OnLevelWasLoaded(level);

        ShowGUI = false;

        if (SceneManager.GetActiveScene().buildIndex != 0 && Loaded) FirePlugins(ExternalAssemblies);
    }

    protected override void Pass()
    {
        Logger.Log(string.Format("AssemblyLoader Initiated at scene №: {0}", SceneManager.GetActiveScene().buildIndex));

        if (!Loaded)
        {
            DetectAndLoadAssemblies();
            Loaded = true;
        }

        if (SceneManager.GetActiveScene().buildIndex == 0 && Loaded)
        {
            Delay((TotalDetected + 1) * 2, () => { SceneManager.LoadScene(1); });      
        }
    }

    #region AssemblyLoader Logic
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
            Logger.Log(string.Format("DetectAssembies Exception: {0}", ex.Message));
        }

        TotalDetected = allPaths.Count;

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
                Logger.Log(string.Format("This is not an adddon assembly! {0}", path));
            }
            else
            {
                SpaceAddonAssembly ea = attrbutes[0] as SpaceAddonAssembly;
                List<Type> mb = GetAllSubclassesOf<Type, SpaceAddonMonoBehaviour, MonoBehaviour>(assembly);
                AssemblyExternalTypes aet = new AssemblyExternalTypes(typeof(MonoBehaviour), mb);
                AssemblyExternal ae = new AssemblyExternal(path, ea.Name, ea.Version, assembly, aet);

                ExternalAssemblies.Add(ae);

                TotalLoaded++;

                FireHotPlugin(ae);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format("LoadAssembly Exception: {0}", ex.Message));
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

            Delay(0.5f, () => { LoadAssembly(path); });
        }
    }

    private void FirePlugins(List<AssemblyExternal> ExternalAssemblies, int level)
    {
        int counter = 0;

        foreach (AssemblyExternal assembly in ExternalAssemblies)
        {
            foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
            {
                foreach (Type v in kvp.Value)
                {
                    if (FirePlugin(v, level))
                        counter++;
                }
            }
        }

        Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, level));
    }

    private void FirePlugins(List<AssemblyExternal> ExternalAssemblies)
    {
        int counter = 0;

        foreach (AssemblyExternal assembly in ExternalAssemblies)
        {
            foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
            {
                foreach (Type v in kvp.Value)
                {
                    if (FirePlugin(v))
                        counter++;
                }
            }
        }

        Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, SceneManager.GetActiveScene().buildIndex));
    }

    private void FireHotPlugin(AssemblyExternal Addon)
    {
        int counter = 0;

        foreach (KeyValuePair<Type, List<Type>> kvp in Addon.Types)
        {
            foreach (Type v in kvp.Value)
            {
                if (FirePlugin(v, 0))
                    counter++;
            }
        }

        Logger.Log(string.Format("{0} plugins fired at scene №: {1}", counter, SceneManager.GetActiveScene().buildIndex));
    }

    private bool FirePlugin(Type type, int level, string msg)
    {
        SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

        if ((int)atr.EntryPoint == level)
        {
            if (atr != null)
            {
                GameObject go = new GameObject(type.Name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.AddComponent(type);

                return true;
            }
        }
        else
        {
            if (!msg.IsNullOrWhiteSpace()) Logger.Log(msg);
        }

        return false;
    }

    private bool FirePlugin(Type type, int level)
    {
        SpaceAddonMonoBehaviour atr = AttributeUtils.GetTypeAttribute<SpaceAddonMonoBehaviour>(type);

        if ((int)atr.EntryPoint == level)
        {
            if (atr != null)
            {
                GameObject go = new GameObject(type.Name);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.AddComponent(type);

                return true;
            }
        }

        return false;
    }

    private bool FirePlugin(Type type)
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

                return true;
            }
        }

        return false;
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
    #endregion
}