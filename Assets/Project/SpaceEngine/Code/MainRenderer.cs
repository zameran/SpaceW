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

using UnityEngine;

[ExecutionOrder(-9997)]
[RequireComponent(typeof(Camera))]
public sealed class MainRenderer : MonoBehaviour
{
    private void Start()
    {

    }

    private void Update()
    {

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

    public void Render()
    {
        /*
        if (GodManager.Instance == null) return;

        Array.Sort(GodManager.Instance.Bodies, pdtltc);

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

        //-----------------------------------------------------------------------------
        GodManager.Instance.Bodies[0].RenderQueueOffset = 10001;
        if (GodManager.Instance.Bodies[0].Atmosphere != null)
            GodManager.Instance.Bodies[0].Atmosphere.RenderQueueOffset = 10002;
        if (GodManager.Instance.Bodies[0].Ocean != null)
            GodManager.Instance.Bodies[0].Ocean.RenderQueueOffset = 10003;
        if (GodManager.Instance.Bodies[0].Ring != null)
            GodManager.Instance.Bodies[0].Ring.RenderQueueOffset = 10000;

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
        */
    }
}