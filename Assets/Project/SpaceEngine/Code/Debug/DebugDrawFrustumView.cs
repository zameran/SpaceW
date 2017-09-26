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
    public sealed class DebugDrawFrustumView : DebugDraw
    {
        protected override void Start()
        {
            base.Start();
        }

        protected override void OnPostRender()
        {
            base.OnPostRender();
        }

        protected override void CreateLineMaterial()
        {
            base.CreateLineMaterial();
        }

        protected override void Draw()
        {
            var nearCorners = new Vector3[4]; // Approx'd nearplane corners
            var farCorners = new Vector3[4]; // Approx'd farplane corners
            var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(GodManager.Instance.View.ScreenToCameraMatrix.ToMatrix4x4()); // NOTE : CameraToScreen

            var tempFrustumPlane = frustumPlanes[1];
            frustumPlanes[1] = frustumPlanes[2];
            frustumPlanes[2] = tempFrustumPlane; // Swap [1] and [2] so the order is better for the loop

            GL.PushMatrix();
            GL.LoadIdentity();
            GL.MultMatrix(CameraHelper.Main().worldToCameraMatrix);
            GL.LoadProjectionMatrix(CameraHelper.Main().projectionMatrix);

            lineMaterial.renderQueue = 5000;
            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            for (byte i = 0; i < 4; i++)
            {
                nearCorners[i] = VectorHelper.Plane3Intersect(frustumPlanes[4], frustumPlanes[i], frustumPlanes[(i + 1) % 4]); // Near corners on the created projection matrix
                farCorners[i] = VectorHelper.Plane3Intersect(frustumPlanes[5], frustumPlanes[i], frustumPlanes[(i + 1) % 4]); // Far corners on the created projection matrix
            }

            for (byte i = 0; i < 4; i++)
            {
                GL.Color(Color.red);
                GL.Vertex3(nearCorners[i].x, nearCorners[i].y, nearCorners[i].z);
                GL.Vertex3(nearCorners[(i + 1) % 4].x, nearCorners[(i + 1) % 4].y, nearCorners[(i + 1) % 4].z);

                GL.Color(Color.blue);
                GL.Vertex3(farCorners[i].x, farCorners[i].y, farCorners[i].z);
                GL.Vertex3(farCorners[(i + 1) % 4].x, farCorners[(i + 1) % 4].y, farCorners[(i + 1) % 4].z);

                GL.Color(Color.green);
                GL.Vertex3(nearCorners[i].x, nearCorners[i].y, nearCorners[i].z);
                GL.Vertex3(farCorners[i].x, farCorners[i].y, farCorners[i].z);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}