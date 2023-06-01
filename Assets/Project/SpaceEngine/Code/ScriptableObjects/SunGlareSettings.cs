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
// Creation Date: 2017.01.25
// Creation Time: 10:46 AM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEngine.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "SunGlareSettings", menuName = "Create SunGlare Settings")]
    public class SunGlareSettings : ScriptableObject
    {
        [Header("Textures")]
        public Texture2D SunSpikes;
        public Texture2D SunFlare;
        public Texture2D SunGhost1;
        public Texture2D SunGhost2;
        public Texture2D SunGhost3;

        [Header("Settings")]
        public Vector3 FlareSettings = new Vector3(0.45f, 1.0f, 0.85f);
        public Vector3 SpikesSettings = new Vector3(0.6f, 1.0f, 1.0f);

        public List<Vector4> Ghost1SettingsList = new List<Vector4>
        {
            new Vector4(0.54f, 0.65f, 2.3f, 0.5f),
            new Vector4(0.54f, 1.0f, 6.0f, 0.7f)
        };

        public List<Vector4> Ghost2SettingsList = new List<Vector4>
        {
            new Vector4(0.135f, 1.0f, 3.0f, 0.9f),
            new Vector4(0.054f, 1.0f, 8.0f, 1.1f),
            new Vector4(0.054f, 1.0f, 4.0f, 1.3f),
            new Vector4(0.054f, 1.0f, 5.0f, 1.5f)
        };

        public List<Vector4> Ghost3SettingsList = new List<Vector4>
        {
            new Vector4(0.135f, 1.0f, 3.0f, 0.9f),
            new Vector4(0.054f, 1.0f, 8.0f, 1.1f),
            new Vector4(0.054f, 1.0f, 4.0f, 1.3f),
            new Vector4(0.054f, 1.0f, 5.0f, 1.5f)
        };
    }
}