﻿#region License

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
// Creation Date: 2017.01.21
// Creation Time: 5:36 PM
// Creator: zameran

#endregion

using System;
using UnityEngine;

namespace SpaceEngine.Core
{
    [Serializable]
    public class CachedComponent<T> where T : Component
    {
        public CachedComponent()
        {
            Component = null;
        }

        public T Component { get; protected set; }

        public void TryInit(MonoBehaviour mb)
        {
            Component = mb.GetComponent<T>();
        }

        public void TryInitFromParent(MonoBehaviour mb)
        {
            if (mb.transform.parent == null)
            {
                return;
            }

            Component = mb.transform.parent.GetComponent<T>();
        }

        public void TryInitAdd(MonoBehaviour mb)
        {
            var component = mb.GetComponent<T>();

            if (component == null)
            {
                component = mb.gameObject.AddComponent<T>();
            }

            Component = component;
        }
    }
}