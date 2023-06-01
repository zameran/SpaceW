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
using System.Collections.Generic;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Singleton;
using SpaceEngine.Managers;
using UnityEngine;

namespace SpaceEngine
{
    [ExecutionOrder(-9996)]
    [RequireComponent(typeof(Camera))]
    public sealed class MainRenderer : MonoSingleton<MainRenderer>
    {
        public bool ZSort = true;

        private BodySort Comparer;

        private void Awake()
        {
            Instance = this;
            Comparer = new BodySort();
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.J) || ZSort)
            {
                SortBodies();
            }
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnPostRender()
        {
        }

        public void SortBodies()
        {
            Array.Sort(GodManager.Instance.Bodies, Comparer);

            // TODO : Find a way to properly sort the space bodies...
            // NOTE : Maybe several near hi-priority bodies can be exist or something another. This is space - all is possible...
            //-----------------------------------------------------------------------------
            var highPriorityBody = GodManager.Instance.ActiveBody;
            var bodies = GodManager.Instance.Bodies;

            if (highPriorityBody == null || bodies == null)
            {
                return;
            }

            highPriorityBody.RenderQueueOffset = 5;
            if (highPriorityBody.Atmosphere != null)
            {
                highPriorityBody.Atmosphere.RenderQueueOffset = 6;
            }

            if (highPriorityBody.Ocean != null)
            {
                highPriorityBody.Ocean.RenderQueueOffset = 7;
            }

            if (highPriorityBody.Ring != null)
            {
                highPriorityBody.Ring.RenderQueueOffset = 4;
            }

            for (var i = 1; i < bodies.Length; i++)
            {
                var lowPriorityBody = bodies[i];

                lowPriorityBody.RenderQueueOffset = 1;
                if (lowPriorityBody.Atmosphere != null)
                {
                    lowPriorityBody.Atmosphere.RenderQueueOffset = 2;
                }

                if (lowPriorityBody.Ocean != null)
                {
                    lowPriorityBody.Ocean.RenderQueueOffset = 3;
                }

                if (lowPriorityBody.Ring != null)
                {
                    lowPriorityBody.Ring.RenderQueueOffset = 0;
                }
            }
            //-----------------------------------------------------------------------------
        }

        public class BodySort : IComparer<Body>
        {
            int IComparer<Body>.Compare(Body a, Body b)
            {
                if (a == null || b == null)
                {
                    return 0;
                }

                var D2A = Vector3.Distance(GodManager.Instance.View.WorldCameraPosition, a.Origin) - a.Size;
                var D2B = Vector3.Distance(GodManager.Instance.View.WorldCameraPosition, b.Origin) - b.Size;

                if (D2A > D2B)
                {
                    return 1;
                }

                if (D2A < D2B)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
}