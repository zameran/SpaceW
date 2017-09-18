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

using SpaceEngine.Core.Bodies;

using System;
using System.Collections.Generic;

using UnityEngine;

[ExecutionOrder(-9996)]
[RequireComponent(typeof(Camera))]
public sealed class MainRenderer : MonoSingleton<MainRenderer>
{
    public bool ZSort = true;

    private BodySort comparer = null;

    public class BodySort : IComparer<Body>
    {
        int IComparer<Body>.Compare(Body a, Body b)
        {
            if (a == null || b == null) return 0;

            var D2A = Vector3.Distance(GodManager.Instance.WorldCameraPos, a.Origin) - a.Size;
            var D2B = Vector3.Distance(GodManager.Instance.WorldCameraPos, b.Origin) - b.Size;

            if (D2A > D2B)
                return 1;
            if (D2A < D2B)
                return -1;
            else
                return 0;
        }
    }

    private void Awake()
    {
        Instance = this;
        comparer = new BodySort();
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.J) || ZSort) ComposeOutputRender();
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

    public void ComposeOutputRender()
    {
        Array.Sort(GodManager.Instance.Bodies, comparer);

        /*
        for (int i = 0; i < GodManager.Instance.Starfields.Length; i++)
        {
            if (GodManager.Instance.Starfields[i] != null)
                GodManager.Instance.Starfields[i].Render();
        }

        for (int i = 0; i < GodManager.Instance.Bodies.Length; i++)
        {
            if (GodManager.Instance.Bodies[i] != null)
                GodManager.Instance.Bodies[i].Render();
        }
        */

        //-----------------------------------------------------------------------------
        GodManager.Instance.Bodies[0].RenderQueueOffset = 5;
        if (GodManager.Instance.Bodies[0].Atmosphere != null)
            GodManager.Instance.Bodies[0].Atmosphere.RenderQueueOffset = 6;
        if (GodManager.Instance.Bodies[0].Ocean != null)
            GodManager.Instance.Bodies[0].Ocean.RenderQueueOffset = 7;
        if (GodManager.Instance.Bodies[0].Ring != null)
            GodManager.Instance.Bodies[0].Ring.RenderQueueOffset = 4;

        for (int i = 1; i < GodManager.Instance.Bodies.Length; i++)
        {
            GodManager.Instance.Bodies[i].RenderQueueOffset = 1;
            if (GodManager.Instance.Bodies[i].Atmosphere != null)
                GodManager.Instance.Bodies[i].Atmosphere.RenderQueueOffset = 2;
            if (GodManager.Instance.Bodies[i].Ocean != null)
                GodManager.Instance.Bodies[i].Ocean.RenderQueueOffset = 3;
            if (GodManager.Instance.Bodies[i].Ring != null)
                GodManager.Instance.Bodies[i].Ring.RenderQueueOffset = 0;
        }
        //-----------------------------------------------------------------------------
    }
}