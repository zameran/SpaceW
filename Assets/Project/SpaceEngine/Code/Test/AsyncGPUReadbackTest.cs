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
// Creation Time: 5:23 PM
// Creator: zameran
#endregion

using System;

using SpaceEngine.Managers;
using SpaceEngine.Tools;
using SpaceEngine.Utilities;

using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEngine.Tests
{
    [RequireComponent(typeof(Camera))]
    public class AsyncGPUReadbackTest : MonoBehaviour
    {
        private readonly CachedComponent<Camera> CameraCachedComponent = new CachedComponent<Camera>();

        public Camera CameraComponent { get { return CameraCachedComponent.Component; } }

        #region Test Stuff

        public int TestTextureSize = 1024;
        public bool ComputationDone = true;
        public RenderTexture TestTexture;
        public ComputeShader TestComputeShader;

        #endregion

        private void Start()
        {
            CameraCachedComponent.TryInit(this);

            TestTexture = RTExtensions.CreateRTexture(new Vector2(TestTextureSize, TestTextureSize), 0, RenderTextureFormat.ARGBFloat, FilterMode.Point, TextureWrapMode.Clamp, false, true, 0);
        }

        private void OnDestroy()
        {
            if (TestTexture != null) TestTexture.ReleaseAndDestroy();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var kernel = TestComputeShader.FindKernel("ExampleTextureUVWrite");
            
            if (ComputationDone)
            {
                ComputationDone = !ComputationDone;
                
                uint x, y, z = 0;
                
                RTUtility.ClearColor(TestTexture, true, true);

                TestComputeShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
                TestComputeShader.SetFloat("textureSize", TestTextureSize);
                TestComputeShader.SetTexture(kernel, "Result", TestTexture);
                TestComputeShader.Dispatch(kernel,
                    (int)(TestTextureSize / x),
                    (int)(TestTextureSize / y),
                    (int)z);


                AsyncGPUManager.Instance.Enqueue(TestTexture, 0, request => { ComputationDone = true; });
            }

            Graphics.Blit(source, destination);
        }

        private void OnGUI()
        {
            if (TestTexture == null || !TestTexture.IsCreated()) return;
            
            GUI.DrawTexture(new Rect(0, 0, 512, 512), TestTexture, ScaleMode.ScaleToFit, false);
        }
    }
}














