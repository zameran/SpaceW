#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace SpaceEngine
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]

    internal sealed class ExecutionOrderAttribute : Attribute
    {
        public readonly int ExecutionOrder = 0;

        public ExecutionOrderAttribute(int executionOrder)
        {
            ExecutionOrder = executionOrder;
        }

        #if UNITY_EDITOR
        private const string PB_TITLE = "Updating Execution Order";
        private const string PB_MESSAGE = "Hold on to your butt, Cap'n!";
        private const string ERR_MESSAGE = "Unable to locate and set execution order for {0}";

        [InitializeOnLoadMethod]
        private static void Execute()
        {
            var type = typeof(ExecutionOrderAttribute);
            var assembly = type.Assembly;
            var types = assembly.GetTypes();
            var scripts = new Dictionary<MonoScript, ExecutionOrderAttribute>();

            var progress = 0.0f;
            var step = 1.0f / types.Length;

            foreach (var item in types)
            {
                var attributes = item.GetCustomAttributes(type, false);

                if (attributes.Length != 1) continue;

                var attribute = attributes[0] as ExecutionOrderAttribute;

                var asset = "";
                var guids = AssetDatabase.FindAssets($"{item.Name} t:script");

                if (guids.Length > 1)
                {
                    foreach (var guid in guids)
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        var filename = Path.GetFileNameWithoutExtension(assetPath);

                        if (filename == item.Name)
                        {
                            asset = guid;

                            break;
                        }
                    }
                }
                else if (guids.Length == 1)
                {
                    asset = guids[0];
                }
                else
                {
                    Debug.LogErrorFormat(ERR_MESSAGE, item.Name);

                    return;
                }

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(asset));

                scripts.Add(script, attribute);
            }

            var changed = false;

            foreach (var item in scripts)
            {
                if (MonoImporter.GetExecutionOrder(item.Key) != item.Value.ExecutionOrder)
                {
                    changed = true;

                    break;
                }
            }

            if (changed)
            {
                foreach (var item in scripts)
                {
                    var cancelled = EditorUtility.DisplayCancelableProgressBar(PB_TITLE, PB_MESSAGE, progress);

                    progress += step;

                    if (MonoImporter.GetExecutionOrder(item.Key) != item.Value.ExecutionOrder)
                    {
                        MonoImporter.SetExecutionOrder(item.Key, item.Value.ExecutionOrder);
                    }

                    if (cancelled) break;
                }
            }

            EditorUtility.ClearProgressBar();
        }
        #endif
    }
}