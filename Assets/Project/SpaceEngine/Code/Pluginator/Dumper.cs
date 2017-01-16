#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using UnityEngine;

namespace SpaceEngine.Pluginator
{
    [Obsolete("Don't use this with ZFramework's Logger class. Only for unity internal usage.")]
    public static class Dumper
    {
        public static void DumpStringList(List<string> items)
        {
            var sb = new StringBuilder();

            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item != null)
                {
                    var message = string.Format("Path: {0};", item);

                    sb.AppendLine(message);
                }
            }

            Debug.Log(sb.ToString());
        }

        public static void DumpAssembliesExternal(List<AssemblyExternal> assemblies)
        {
            if (assemblies == null)
                return;

            foreach (var assembly in assemblies)
            {
                var sb = new StringBuilder();

                if (assembly != null)
                {
                    if (assembly.Assembly != null)
                    {
                        sb.AppendLine(string.Format("Name: {0};", assembly.Name));
                        sb.AppendLine(string.Format("Path: {0};", assembly.Path));
                        sb.AppendLine(string.Format("Version: {0};", assembly.Version));

                        sb.AppendLine(string.Format("Assembly Types List ({0}) Start.", assembly.Types.Count));

                        foreach (var kvp in assembly.Types)
                        {
                            var values_sb = new StringBuilder();

                            foreach (var v in kvp.Value)
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
            var sb = new StringBuilder();

            if (assemblies == null)
                return;

            foreach (var assembly in assemblies)
            {
                if (assembly != null)
                {
                    var message = string.Format("Full Name: {0};", assembly.FullName);

                    sb.AppendLine(message);
                }
            }

            Debug.Log(sb.ToString());
        }

        public static void DumpTypesNames(List<Type> types)
        {
            var sb = new StringBuilder();

            if (types == null)
                return;

            foreach (var type in types)
            {
                if (type != null)
                {
                    var message = string.Format("Type: {0}; Name: {1}", type, type.Name);

                    sb.AppendLine(message);
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}