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

using System;
using System.IO;

using UnityEngine;

public static class PathGlobals
{
    public const string GlobalConfigFolderName = "Config";
    public const string GlobalModFolderName = "Mods";
    public const string GlobalMainLogFileName = "Log.txt";

    public static string GlobalConfigFolderPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalConfigFolderName); ;
        }
    }

    public static string GlobalModFolderPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalModFolderName); ;
        }
    }

    public static string GlobalMainLogPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../" + GlobalMainLogFileName); ;
        }
    }

    public static string GlobalRootPath
    {
        get
        {
            return Path.GetFullPath(Application.dataPath + "/../");
        }
    }

    public static string GlobalConfigFolderPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalConfigFolderName);
    }

    public static string GlobalModFolderPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalModFolderName);
    }

    public static string GlobalMainLogPathEditor(string dataPath)
    {
        return Path.GetFullPath(dataPath + "/../" + GlobalMainLogFileName);
    }
}
