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

using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Debugging;

using UnityEngine;

namespace SpaceEngine.Cameras
{
    [RequireComponent(typeof(Camera))]
    public abstract class GameCamera : MonoBehaviour, ICamera
    {
        private readonly CachedComponent<Camera> CameraCachedComponent = new CachedComponent<Camera>();

        public Camera CameraComponent { get { return CameraCachedComponent.Component; } }

        public bool Controllable = true;

        // NOTE : A big problem can be here with a float precision, because matrices provided as is.
        // NOTE : If matrices calculation will be provided, i.e from spherical coordinates around the body (Somesort of 'Body Space').

        public Matrix4x4d WorldToCameraMatrix { get; protected set; }
        public Matrix4x4d CameraToWorldMatrix { get; protected set; }
        public Matrix4x4d CameraToScreenMatrix { get; protected set; }
        public Matrix4x4d ScreenToCameraMatrix { get; protected set; }

        public Vector3 WorldCameraPosition { get; protected set; }

        private DebugGUISwitcher DebugGUISwitcherInstance { get { return DebugGUISwitcher.Instance as DebugGUISwitcher; } }

        public bool MouseOverUI { get { if (DebugGUISwitcherInstance != null) return DebugGUISwitcherInstance.MouseOverGUI; else return false; } }

        protected virtual void Start()
        {
            CameraCachedComponent.TryInit(this);

            Init();

            UpdateMatrices();
        }

        protected virtual void Update()
        {

        }

        protected virtual void FixedUpdate()
        {

        }

        protected abstract void Init();

        public abstract void UpdateMatrices();

        public abstract void UpdateVectors();

        protected float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }
    }
}