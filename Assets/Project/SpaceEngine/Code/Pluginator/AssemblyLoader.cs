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

    private void Start()
    {
        List<Assembly> allAssemblies;
        List<Type> allTypes;
        List<Type> allMonoBehaviours;

        DetectAndLoadAssemblies(out allAssemblies, out allTypes, out allMonoBehaviours);

        DumpTypesNames(allTypes, "Type");
        DumpTypesNames(allMonoBehaviours, "MonoBehaviour");

        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    private void DetectAndLoadAssemblies(out List<Assembly> allAssemblies,
                                         out List<Type> allTypes,
                                         out List<Type> allMonoBehaviours)
    {
        string path = Application.dataPath + AssembliesFolderName;

        allAssemblies = new List<Assembly>();
        allTypes = new List<Type>();
        allMonoBehaviours = new List<Type>();

        try
        {
            foreach (string dll in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(dll);

                    DumpAsseblyInfo(assembly);

                    allAssemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    Debug.LogError("AssemblyLoader.DetectAssemblies() Load Exception: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("AssemblyLoader.DetectAssemblies() Get Files Exception: " + ex.Message);
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
                        allMonoBehaviours.Add(type);
                    }
                }

                allTypes.AddRange(types);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("AssemblyLoader.DetectAssemblies() Get Types Exception: " + ex.Message);
        }

        Debug.Log("AssemblyLoader.DetectAssemblies() Types count: " + allTypes.Count);
        Debug.Log("AssemblyLoader.DetectAssemblies() MonoBehaviour Types count: " + allMonoBehaviours.Count);
    }

    private void DumpAsseblyInfo(Assembly assembly)
    {
        if (assembly != null)
        {
            Debug.Log("AssemblyLoader.DumpAsseblyInfo() Full Name: " + assembly.FullName);
            Debug.Log("AssemblyLoader.DumpAsseblyInfo() Image Runtime Version: " + assembly.ImageRuntimeVersion);
        }
    }

    private void DumpTypesNames(List<Type> types, string prefix = "")
    {
        if (types != null && types.Count > 0)
        {
            foreach (Type type in types)
            {
                if (type != null)
                    if(!prefix.IsNotNullOrEmpty())
                        Debug.Log("AssemblyLoader.DumpTypesNames() Type: " + type.Name);
                    else
                        Debug.Log("AssemblyLoader.DumpTypesNames() Type: " + "(" + prefix + ")" + " " + type.Name);
            }
        }
    }
}