#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System;

using UnityEngine;

[Obsolete("Don't use this with ZFramework's Logger class. Only for unity internal usage.")]
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

                    sb.AppendLine(string.Format("Assembly Types List ({0}) Start.", assembly.Types.Count));

                    foreach (KeyValuePair<Type, List<Type>> kvp in assembly.Types)
                    {
                        StringBuilder values_sb = new StringBuilder();

                        foreach (Type v in kvp.Value)
                        {
                            values_sb.Append(string.Format("{0} ", v));
                        }

                        sb.AppendLine(string.Format("Key: {0}, Values: {1}", kvp.Key, values_sb.ToString()));
                    }

                    sb.AppendLine(string.Format("Assembly Types List ({0}) End.", assembly.Types.Count));
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