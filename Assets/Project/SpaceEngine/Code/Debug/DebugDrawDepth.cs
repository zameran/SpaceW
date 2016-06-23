#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DebugDrawDepth : MonoBehaviour
{
    public Shader depthShader;

    public Transform sunTransform;

    public RenderTexture DepthTexture;

    private GameObject depthCamera;
    private Camera depthCameraComponent;

    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        DepthTexture = new RenderTexture((int)(Screen.width * 1), (int)(Screen.height * 1), 16, RenderTextureFormat.RFloat);

        DepthTexture.filterMode = FilterMode.Bilinear;
        DepthTexture.useMipMap = false;
        DepthTexture.Create();
    }

    private void Update()
    {
        Shader.SetGlobalVector("_Godray_WorldSunDir", sunTransform.position - transform.position);
    }

    private void OnGUI()
    {
        if (DepthTexture != null)
            GUI.DrawTexture(new Rect(5, 5, Screen.width * 0.35f, Screen.height * 0.35f), DepthTexture, ScaleMode.ScaleAndCrop, false);
    }

    private void OnPreRender()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!depthCamera)
        {
            depthCamera = new GameObject("CustomDepthCamera");
            depthCameraComponent = depthCamera.AddComponent<Camera>();

            depthCameraComponent.CopyFrom(CameraHelper.Main());

            depthCameraComponent.farClipPlane = CameraHelper.Main().farClipPlane;
            depthCameraComponent.nearClipPlane = CameraHelper.Main().nearClipPlane;
            depthCameraComponent.depthTextureMode = DepthTextureMode.None;

            depthCameraComponent.transform.parent = CameraHelper.Main().transform;

            depthCameraComponent.enabled = false;
        }

        depthCameraComponent.CopyFrom(CameraHelper.Main());
        depthCameraComponent.enabled = false;

        bool renderDepthBuffer = true;

        if (renderDepthBuffer)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = DepthTexture;

            GL.Clear(false, true, Color.black);

            depthCameraComponent.targetTexture = DepthTexture;
            depthCameraComponent.RenderWithShader(depthShader, "RenderType");

            RenderTexture.active = rt;
        }

        Graphics.Blit(source, destination);
    }
}