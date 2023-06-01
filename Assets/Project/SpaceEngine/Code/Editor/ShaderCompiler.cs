﻿#region License
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

using System.Reflection;
using SpaceEngine.Tools;
using UnityEditor;
using UnityEngine;

namespace SpaceEngine.Editor
{
    public class ShaderCompiler : EditorWindow
    {
        public Shader shader;

        private int mode = 1;
        private int externPlatformsMask = 262143;
        private bool includeAllVariants = true;
        private bool preprocessOnly = false;
        private bool stripLineDirectives = false;

        [MenuItem("SpaceEngine/ShaderCompiler")]
        public static void Init()
        {
            var window = (ShaderCompiler)GetWindow(typeof(ShaderCompiler));
            window.autoRepaintOnSceneChange = true;
        }

        public void OnGUI()
        {
            GUILayout.Label("Select shader to compile: ");
            shader = (Shader)EditorGUILayout.ObjectField(shader, typeof(Shader), false);

            GUILayout.Label("Select shader compilation mode: ");
            mode = EditorGUILayout.IntField(mode);

            GUILayout.Label("Select shader compilation custom platforms masks: ");
            externPlatformsMask = EditorGUILayout.IntField(externPlatformsMask);

            GUILayout.Label("Should compiled shader contain all shader variants?");
            includeAllVariants = EditorGUILayout.Toggle(includeAllVariants);

            GUILayout.Label("Preprocess Only?");
            preprocessOnly = EditorGUILayout.Toggle(preprocessOnly);

            GUILayout.Label("Strip Line Directives?");
            stripLineDirectives = EditorGUILayout.Toggle(stripLineDirectives);

            if (shader != null)
            {
                if (GUILayout.Button("Compile..."))
                {
                    UnityEngineAPI.InvokeAPI<ShaderUtil>("OpenCompiledShader",
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        new object[]
                        {
                            shader,
                            mode,
                            externPlatformsMask,
                            includeAllVariants,
                            preprocessOnly,
                            stripLineDirectives
                        });
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Nothing to compile! - Select shader to work with...", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("This utility simply calls Unity's Internal Core OpenCompiledShader method...\n" +
                                    "See Shader Inspector - It have the same button!", MessageType.Info);
        }
    }
}