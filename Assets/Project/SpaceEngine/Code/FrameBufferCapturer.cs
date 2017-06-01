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
//     notice, this list of conditions and the following disclaimer.
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
// Creation Date: 2017.05.04
// Creation Time: 2:29 AM
// Creator: zameran
#endregion

using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEngine
{
    [ExecutionOrder(-9999)]
    [RequireComponent(typeof(Camera))]
    public class FrameBufferCapturer : MonoSingleton<FrameBufferCapturer>
    {
        [Range(0.1f, 1.0f)]
        public float FBOScale = 1.0f;

        public Vector2 ScreenSize { get { return new Vector2(Screen.width, Screen.height); } }
        public Vector2 FBOSize { get { return ScreenSize * FBOScale; } }
        public Vector2 DebugDrawSize { get { return ScreenSize * 0.25f; } }

        private Vector2 LastScreenSize { get; set; }

        private CameraEvent CommandBufferCameraEvent { get { return CameraEvent.BeforeImageEffects; } }

        private CommandBuffer CMDBuffer;

        public RenderTextureFormat FBOFormat = RenderTextureFormat.ARGB32;

        public RenderTexture FBOTexture;

        public RenderTargetIdentifier FBOTextureRTI;
        public RenderTargetIdentifier SourceRTI;
        public RenderTargetIdentifier DestinationRTI;

        public Material FBProcessMaterial;
        public Shader FBProcessShader;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (FBProcessShader == null) FBProcessShader = Shader.Find("Hidden/FrameBufferProcess");

            FBProcessMaterial = MaterialHelper.CreateTemp(FBProcessShader, "FrameBufferProcess");

            CMDBufferCreate();
            FBORecreate();

            LastScreenSize = ScreenSize;
        }

        private void Update()
        {
            var screenSize = ScreenSize;

            if (LastScreenSize != screenSize)
            {
                LastScreenSize = screenSize;

                // NOTE : A special event can be fired here for MOAR globalization...
                FBORecreate();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Helper.Destroy(FBProcessMaterial);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            CMDBufferUpdate(src, dest);

            // Blitting second time to evade the overlay drawing of fbo texture over gui and other stuff.
            Graphics.Blit(src, dest);
        }

        #region API

        public bool FBOExist()
        {
            return FBOTexture != null && FBOTexture.IsCreated();
        }

        public void CMDBufferCreate()
        {
            CMDBuffer = new CommandBuffer();
            CMDBuffer.name = "FrameBufferCapturer";
        }

        public void CMDBufferUpdate(RenderTexture src, RenderTexture dest)
        {
            if (CMDBuffer == null) { CMDBufferCreate(); }
            else { CMDBuffer.Clear(); }

            FBOTextureRTI = new RenderTargetIdentifier(FBOTexture);
            SourceRTI = new RenderTargetIdentifier(src);
            DestinationRTI = new RenderTargetIdentifier(dest);

            if (FBOExist())
            {
                FBProcessMaterial.SetTexture("_FrameBuffer", src);

                CMDBuffer.Blit(SourceRTI, FBOTextureRTI, FBProcessMaterial);
            }

            CMDBuffer.Blit(SourceRTI, DestinationRTI);

            // TODO : A lot of garbage, if there are a lot of command buffers exist...
            // NOTE : FBO Texture inverted verticaly, when rendering path != deffered, so special shader will be executed...

            if (CameraHelper.Main().CommandBufferExistByName(CommandBufferCameraEvent, "FrameBufferCapturer"))
            {
                CameraHelper.Main().RemoveCommandBuffer(CommandBufferCameraEvent, CMDBuffer);
            }

            CameraHelper.Main().AddCommandBuffer(CommandBufferCameraEvent, CMDBuffer);
        }

        public void FBORecreate()
        {
            if (FBOExist())
            {
                FBOTexture.ReleaseAndDestroy();
            }

            FBOTexture = RTExtensions.CreateRTexture(FBOSize, 0, FBOFormat, FilterMode.Point, TextureWrapMode.Clamp, false, true, CameraHelper.Main().GetAntiAliasing());
        }

        #endregion
    }
}