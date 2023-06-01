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
// Creation Date: 2017.09.24
// Creation Time: 12:10 PM
// Creator: zameran
#endregion

using SpaceEngine.Managers;

using UnityEngine;

namespace SpaceEngine.Core
{
    /// <summary>
    /// Class-wrapper around default <see cref="MonoBehaviour"/> for special purposes of space engine.
    /// This class is somewhat extension to default <see cref="Node{T}"/>. The only difference is that:
    /// <see cref="InitNode"/> and <see cref="UpdateNode"/> will not be called from <see cref="Start"/> and <see cref="Update"/>.
    /// So, this node should be updated from parent one.
    /// <remarks>
    /// Various stuff should be destroyed in <see cref="OnDestroy"/>!
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">Generic.</typeparam>
    public abstract class NodeSlave<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {
            if (DebugSequenceManager.Instance != null) DebugSequenceManager.Instance.Debug(this);
        }

        protected virtual void Update()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// This method will be automatically called in <see cref="Start"/>.
        /// Use this for initialization.
        /// </summary>
        public abstract void InitNode();

        /// <summary>
        /// This method will be automatically called in <see cref="Update"/>.
        /// </summary>
        public abstract void UpdateNode();
    }
}