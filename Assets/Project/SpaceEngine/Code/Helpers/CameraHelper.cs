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

#define UNITY_GL_PROJECTION_MATRIX
//#define SE_PROJECTION_MATRIX

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEngine.Helpers
{
    /// <summary>
    ///     Class - extensions holder for a <see cref="Camera" />.
    /// </summary>
    public static class CameraHelper
    {
        public static Camera Main()
        {
            return Camera.main;
        }

        public static Camera DepthCamera()
        {
            return FindCamera("CustomDepthCamera");
        }

        public static Camera NearCamera()
        {
            return FindCamera("NearCamera");
        }

        /// <summary>
        ///     Find a <see cref="Camera" /> in <see cref="GameObject" />'s components with specified name.
        /// </summary>
        /// <param name="gameObjectName">Target <see cref="GameObject" /> name to search in.</param>
        /// <returns>Returns a <see cref="Camera" /> component from existing <see cref="GameObject" />'s components.</returns>
        public static Camera FindCamera(string gameObjectName = "Camera")
        {
            var mainCamera = Main();
            var resultCameraGameObject = mainCamera.transform.Find(gameObjectName);

            if (resultCameraGameObject != null)
            {
                var resultCameraComponent = resultCameraGameObject.GetComponent<Camera>();

                if (resultCameraComponent != null)
                {
                    return resultCameraComponent;
                }
            }

            return null;
        }

        public static Matrix4x4 GetWorldToCamera(this Camera camera)
        {
            return camera.worldToCameraMatrix;
        }

        public static Matrix4x4 GetCameraToWorld(this Camera camera)
        {
            return camera.cameraToWorldMatrix;
        }

        /// <summary>
        ///     Get <see cref="Camera" />'s projection <see cref="Matrix4x4" />.
        /// </summary>
        /// <param name="camera">Target <see cref="Camera" />.</param>
        /// <param name="useFix">Render in to texture?</param>
        /// <returns>Returns the <see cref="Camera" />'s projection <see cref="Matrix4x4" />.</returns>
        public static Matrix4x4 GetCameraToScreen(this Camera camera, bool useFix = true)
        {
            #if UNITY_GL_PROJECTION_MATRIX
            var projectionMatrix = camera.projectionMatrix;

            projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, useFix);

            return projectionMatrix;
            #elif SE_PROJECTION_MATRIX
            var projectionMatrix = camera.projectionMatrix;

            if (!useFix) return projectionMatrix;

            if (SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D", System.StringComparison.Ordinal) > -1)
            {
                // NOTE : Default unity antialiasing breaks matrices?
                if ((IsDeferred(camera) || QualitySettings.antiAliasing == 0))
                {
                    // Invert Y for rendering to a render texture
                    for (var i = 0; i < 4; i++)
                    {
                        projectionMatrix[1, i] = -projectionMatrix[1, i];
                    }
                }

                // Scale and bias depth range
                for (var i = 0; i < 4; i++)
                {
                    // NOTE : Hm. I saw something about reverse depth buffer in release notes...
                    projectionMatrix[2, i] = -(projectionMatrix[2, i] * 0.5f + projectionMatrix[3, i] * -0.5f);
                    //projectionMatrix[2, i] = projectionMatrix[2, i] * 0.5f + projectionMatrix[3, i] * 0.5f;
                }
            }

            return projectionMatrix;
            #else
            return camera.projectionMatrix;
            #endif
        }

        public static Matrix4x4 GetScreenToCamera(this Camera camera)
        {
            return camera.GetCameraToScreen().inverse;
        }

        public static Vector3 GetProjectedDirection(this Vector3 v)
        {
            var cameraToWorld = Main().GetCameraToWorld();
            var screenToCamera = Main().GetScreenToCamera();

            return cameraToWorld.MultiplyPoint(screenToCamera.MultiplyPoint(v));
        }

        public static Vector3 GetRelativeProjectedDirection(this Vector3 v, Matrix4x4 worldToLocal)
        {
            return worldToLocal.MultiplyPoint(v.GetProjectedDirection());
        }

        public static bool IsDeferred(this Camera camera)
        {
            return camera.actualRenderingPath == RenderingPath.DeferredShading;
        }

        public static int GetAntiAliasing(this Camera camera)
        {
            var antiAliasing = QualitySettings.antiAliasing;

            if (antiAliasing == 0)
            {
                antiAliasing = 1;
            }

            // Reset aa value to 1 in case camera is in DeferredLighting or DeferredShading Rendering Path
            if (camera.IsDeferred())
            {
                antiAliasing = 1;
            }

            return antiAliasing;
        }

        public static bool CommandBufferExistByName(this Camera camera, CameraEvent evt, string name)
        {
            return camera.GetCommandBuffers(evt).ToList().Any(buffer => buffer.name == name);
        }

        public static IEnumerable<CommandBuffer> GetCommandBuffersByName(this Camera camera, CameraEvent evt, string name)
        {
            return camera.GetCommandBuffers(evt).ToList().Where(buffer => buffer.name == name);
        }

        public static void RemoveAllCommandBuffersByName(this Camera camera, CameraEvent evt, string name)
        {
            var commandBuffersToRemove = camera.GetCommandBuffersByName(evt, name);

            foreach (var commandBufferToRemove in commandBuffersToRemove)
            {
                camera.RemoveCommandBuffer(evt, commandBufferToRemove);
            }
        }
    }
}