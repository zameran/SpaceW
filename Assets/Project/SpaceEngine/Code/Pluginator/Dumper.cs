using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;

using UnityEngine;

public static class Dumper
{
    public static void DumpStringList(List<string> items)
    {
        StringBuilder sb = new StringBuilder();

        if (items == null)
            return;

        foreach (string item in items)
        {
            if (item != null)
            {
                string message = string.Format("Path: {0};", item);

                sb.AppendLine(message);
            }
        }

        Debug.Log(sb.ToString());
    }

    public static void DumpAssembliesExternal(List<AssemblyExternal> assemblies)
    {
        if (assemblies == null)
            return;

        foreach (AssemblyExternal assembly in assemblies)
        {
            StringBuilder sb = new StringBuilder();

            if (assembly != null)
            {
                if (assembly.Assembly != null)
                {
                    sb.AppendLine(string.Format("Name: {0};", assembly.Name));
                    sb.AppendLine(string.Format("Path: {0};", assembly.Path));
                    sb.AppendLine(string.Format("Version: {0};", assembly.Version));

                    sb.AppendLine(string.Format("Types: "));

                    foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
                    {
                        StringBuilder values_sb = new StringBuilder();

                        foreach (Type v in kvp.Value)
                        {
                            values_sb.Append(string.Format("{0} ", v));
                        }

                        sb.AppendLine(string.Format("Key: {0}, Values: {1}", kvp.Key, values_sb.ToString()));
                    }
                }
            }

            Debug.Log(sb.ToString());
        }
    }

    public static void DumpAssembliesInfo(List<Assembly> assemblies)
    {
        StringBuilder sb = new StringBuilder();

        if (assemblies == null)
            return;

        foreach (Assembly assembly in assemblies)
        {
            if (assembly != null)
            {
                string message = string.Format("Full Name: {0};", assembly.FullName);

                sb.AppendLine(message);
            }
        }

        Debug.Log(sb.ToString());
    }

    public static void DumpTypesNames(List<Type> types)
    {
        StringBuilder sb = new StringBuilder();

        if (types == null)
            return;

        foreach (Type type in types)
        {
            if (type != null)
            {
                string message = string.Format("Type: {0}; Name: {1}", type.ToString(), type.Name);

                sb.AppendLine(message);
            }
        }

        Debug.Log(sb.ToString());
    }
}