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