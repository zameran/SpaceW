﻿#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
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

using System.IO;
using SpaceEngine.Tools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SpaceEngine.Editor.Postprocessors
{
    public sealed class BuildPostprocessor
    {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var fileName = Path.GetFileName(pathToBuiltProject);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathToBuiltProject);

            if (string.IsNullOrEmpty(fileName)) { Debug.Log("BuildPostprocessor.OnPostprocessBuild: Can't find exe!"); return; }

            var dataPath = $"{pathToBuiltProject.Remove(pathToBuiltProject.Length - fileName.Length, fileName.Length)}{fileNameWithoutExtension}_Data";

            // TODO : Other platforms support...
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                var pureBuildPath = Path.GetDirectoryName(pathToBuiltProject);

                if (string.IsNullOrEmpty(pureBuildPath)) { Debug.Log("BuildPostprocessor.OnPostprocessBuild: Can't find pure build path!"); return; }

                foreach (var path in Directory.GetFiles(pureBuildPath, "*Log*.txt"))
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }

            Directory.CreateDirectory(PathGlobals.GlobalModFolderPathEditor(dataPath));
            Directory.CreateDirectory(PathGlobals.GlobalConfigFolderPathEditor(dataPath));
        }
    }
}