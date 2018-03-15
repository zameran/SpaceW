#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
// All rights reserved.
//  
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
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
// Creation Date: 2018.03.15
// Creation Time: 4:01 PM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SpaceEngine.Managers
{
    public class AsyncGPUManager : MonoSingleton<AsyncGPUManager>
    {
        public struct AsyncGPUReadbackRequestEntry<TType> where TType : struct
        {
            public AsyncGPUReadbackRequest Request;

            public int Layer;

            public Action<NativeArray<TType>> Callback;

            public AsyncGPUReadbackRequestEntry(AsyncGPUReadbackRequest request, int layer, Action<NativeArray<TType>> callback)
            {
                Request = request;

                Callback = callback;

                Layer = layer;
            }
        }

        private Queue<AsyncGPUReadbackRequestEntry<Color32>> Entries;

        public bool CanEnqueue { get { return Entries.Count < 8; } }

        private void Awake()
        {
            Instance = this;

            Entries = new Queue<AsyncGPUReadbackRequestEntry<Color32>>();
        }

        private void Update()
        {
            while (Entries.Count > 0)
            {
                var currentEntry = Entries.Peek();
                var currentRequest = currentEntry.Request;

                if (currentRequest.hasError)
                {
                    Debug.LogError("AsyncGPUManager.Update: GPU Readback error!");

                    Entries.Dequeue();
                }
                else if (currentRequest.done)
                {
                    var data = currentRequest.GetData<Color32>(currentEntry.Layer);

                    if (currentEntry.Callback != null) currentEntry.Callback(data);

                    Entries.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        protected override void OnDestroy()
        {
            Entries.Clear();

            base.OnDestroy();
        }

        #region API

        private bool EnqueueCheck()
        {
            var canEnqueue = CanEnqueue;

            if (canEnqueue == false) Debug.LogWarning("AsyncGPUManager.EnqueueCheck: Too many requests! Can't proceed for now...");

            return canEnqueue;
        }

        public void Enqueue(Texture source, int mipIndex = 0, int layer = 0, Action<NativeArray<Color32>> callback = null)
        {
            if (EnqueueCheck()) Entries.Enqueue(new AsyncGPUReadbackRequestEntry<Color32>(AsyncGPUReadback.Request(source, mipIndex), layer, callback));
        }

        public void Enqueue(Texture source, int mipIndex, TextureFormat dstFormat, int layer = 0, Action<NativeArray<Color32>> callback = null)
        {
            if (EnqueueCheck()) Entries.Enqueue(new AsyncGPUReadbackRequestEntry<Color32>(AsyncGPUReadback.Request(source, mipIndex, dstFormat), layer, callback));
        }

        public void Enqueue(Texture source, int mipIndex, int x, int width, int y, int height, int z, int depth, int layer = 0, Action<NativeArray<Color32>> callback = null)
        {
            if (EnqueueCheck()) Entries.Enqueue(new AsyncGPUReadbackRequestEntry<Color32>(AsyncGPUReadback.Request(source, mipIndex, x, width, y, height, z, depth), layer, callback));
        }

        #endregion
    }
}