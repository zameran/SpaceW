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

using System.Reflection;
using SpaceEngine.Tools;
using UnityEditor;
using UnityEngine;

namespace SpaceEngine.Editor
{
    public class ShaderCompiler : EditorWindow
    {
        private int m_ExternPlatformsMask = 262143;
        private bool m_IncludeAllVariants = true;
        private int m_Mode = 1;
        private bool m_PreprocessOnly;
        private Shader m_Shader;
        private bool m_StripLineDirectives;

        public void OnGUI()
        {
            GUILayout.Label("Select shader to compile: ");
            m_Shader = (Shader)EditorGUILayout.ObjectField(m_Shader, typeof(Shader), false);

            GUILayout.Label("Select shader compilation mode: ");
            m_Mode = EditorGUILayout.IntField(m_Mode);

            GUILayout.Label("Select shader compilation custom platforms masks: ");
            m_ExternPlatformsMask = EditorGUILayout.IntField(m_ExternPlatformsMask);

            GUILayout.Label("Should compiled shader contain all shader variants?");
            m_IncludeAllVariants = EditorGUILayout.Toggle(m_IncludeAllVariants);

            GUILayout.Label("Preprocess Only?");
            m_PreprocessOnly = EditorGUILayout.Toggle(m_PreprocessOnly);

            GUILayout.Label("Strip Line Directives?");
            m_StripLineDirectives = EditorGUILayout.Toggle(m_StripLineDirectives);

            if (m_Shader != null)
            {
                if (GUILayout.Button("Compile..."))
                {
                    UnityEngineAPI.InvokeAPI<ShaderUtil>("OpenCompiledShader",
                                                         BindingFlags.Static | BindingFlags.NonPublic,
                                                         null, m_Shader, m_Mode, m_ExternPlatformsMask, m_IncludeAllVariants, m_PreprocessOnly, m_StripLineDirectives);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Nothing to compile! - Select shader to work with...", MessageType.Warning);
            }

            EditorGUILayout.HelpBox("This utility simply calls Unity's Internal Core OpenCompiledShader method...\n See Shader Inspector - It have the same button!", MessageType.Info);
        }

        [MenuItem("SpaceEngine/ShaderCompiler")]
        public static void Init()
        {
            var window = (ShaderCompiler)GetWindow(typeof(ShaderCompiler));
            window.autoRepaintOnSceneChange = true;
        }
    }
}