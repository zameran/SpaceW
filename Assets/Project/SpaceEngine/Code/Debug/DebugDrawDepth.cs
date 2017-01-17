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

namespace SpaceEngine.Debugging
{
    [RequireComponent(typeof(Camera))]
    public class DebugDrawDepth : MonoBehaviour
    {
        public Shader DepthShader;

        public RenderTexture DepthTexture;

        private void Start()
        {
            var customDepthCameraGameObject = new GameObject("CustomDepthCamera");
            var customDepthCamera = customDepthCameraGameObject.AddComponent<Camera>();

            customDepthCamera.CopyFrom(Camera.main);
            customDepthCamera.transform.parent = Camera.main.transform;
            customDepthCamera.depthTextureMode = DepthTextureMode.Depth;
            customDepthCamera.enabled = false;

            DepthTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);

            DepthTexture.filterMode = FilterMode.Point;
            DepthTexture.useMipMap = false;
            DepthTexture.Create();
        }

        private void Update()
        {

        }

        private void OnGUI()
        {
            if (DepthTexture != null)
                GUI.DrawTexture(new Rect(5, 5, Screen.width * 0.35f, Screen.height * 0.35f), DepthTexture, ScaleMode.ScaleAndCrop, false);
        }

        private void OnPreRender()
        {
            var customDepthCamera = CameraHelper.DepthCamera();

            customDepthCamera.CopyFrom(Camera.main);
            customDepthCamera.targetTexture = DepthTexture;
            customDepthCamera.depthTextureMode = DepthTextureMode.Depth;
            customDepthCamera.RenderWithShader(DepthShader, "RenderType");
            customDepthCamera.enabled = false;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

        }
    }
}