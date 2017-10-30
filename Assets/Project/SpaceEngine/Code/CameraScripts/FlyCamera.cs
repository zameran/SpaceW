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
using SpaceEngine.Core.Numerics.Vectors;

using UnityEngine;

namespace SpaceEngine.Cameras
{
    [ExecutionOrder(-9990)]
    public class FlyCamera : GameCamera
    {
        public Body Body { get { return GodManager.Instance.ActiveBody; } }

        public float Speed = 1.0f;
        public float RotationSpeed = 1.0f;

        private Vector3 Velocity = Vector3.zero;
        private Vector3 Rotation = Vector3.zero;
        private Quaternion TargetRotation = Quaternion.identity;

        private float CurrentSpeed;
        private float DistanceToAlign;
        private float DistanceToCore;
        private float NearClipPlaneCache;
        private float FarClipPlaneCache;

        public bool DynamicClipPlanes = true;
        public bool Controllable = true;

        private bool Aligned = false;
        private bool Supercruise = false;

        private Ray RayScreen;

        protected override void Init()
        {
            NearClipPlaneCache = CameraComponent.nearClipPlane;
            FarClipPlaneCache = CameraComponent.farClipPlane;

            Rotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0); // NOTE : Prevent crazy rotation on start...

            UpdateDistances();
            UpdateClipPlanes();

            MainRenderer.Instance.ComposeOutputRender();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            UpdateDistances();
            UpdateClipPlanes();

            if (Controllable)
            {
                Supercruise = !Aligned && Input.GetKey(KeyCode.T);

                if (Input.GetMouseButton(0) && !MouseOverUI)
                {
                    Rotation.z = 0;

                    RayScreen = CameraComponent.ScreenPointToRay(Input.mousePosition);

                    TargetRotation = Quaternion.LookRotation((RayScreen.origin + RayScreen.direction * 10.0f) - transform.position, transform.up);

                    transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, Time.fixedDeltaTime * RotationSpeed);

                    if (!Aligned)
                        transform.Rotate(new Vector3(0.0f, 0.0f, Rotation.z));
                }
                else if (Input.GetMouseButton(1) && !MouseOverUI)
                {
                    RotateAround(true); // NOTE : Force this rotation mode...
                }
                else
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        Rotation.z -= 1.0f * Time.deltaTime;
                    }
                    else if (Input.GetKey(KeyCode.Q))
                    {
                        Rotation.z += 1.0f * Time.deltaTime;
                    }
                    else
                    {
                        Rotation.z = Mathf.Lerp(Rotation.z, 0, Time.deltaTime * 2.0f);
                    }

                    Rotation.z = Mathf.Clamp(Rotation.z, -100.0f, 100.0f);

                    if (!Aligned)
                    {
                        transform.Rotate(new Vector3(0, 0, Rotation.z));

                        if (Input.GetKey(KeyCode.R))
                        {
                            transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.up, Time.fixedDeltaTime * 30.0f);
                        }

                        if (Input.GetKey(KeyCode.F))
                        {
                            transform.position = Vector3.MoveTowards(transform.position, transform.position - transform.up, Time.fixedDeltaTime * 30.0f);
                        }
                    }

                    if (Input.GetKey(KeyCode.G))
                    {
                        if (Body != null)
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Body.Origin - transform.position), Time.fixedDeltaTime * RotationSpeed * 30.0f);
                        else
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.zero - transform.position), Time.fixedDeltaTime * RotationSpeed * 30.0f);
                    }
                }
                
                // NOTE : Body shape dependent...
                if (DistanceToCore < DistanceToAlign)
                {
                    Aligned = true;

                    var gravityVector = Body != null ? Body.transform.position : Vector3.zero - transform.position;

                    TargetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);

                    transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, Time.fixedDeltaTime * RotationSpeed * 3.0f);
                }
                else
                {
                    Aligned = false;
                }

                Velocity.z = Input.GetAxis("Vertical");
                Velocity.x = Input.GetAxis("Horizontal");

                CurrentSpeed = Speed;

                if (Input.GetKey(KeyCode.LeftShift))
                    CurrentSpeed = Speed * 10f;
                if (Input.GetKey(KeyCode.LeftControl))
                    CurrentSpeed = Speed * 100f;
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
                    CurrentSpeed = Speed * 1000f;
                if (Input.GetKey(KeyCode.LeftAlt))
                    CurrentSpeed = Speed / 10f;

                if (!MouseOverUI)
                {
                    Speed += Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel") * 100.0f);
                    Speed = Mathf.Clamp(Speed, 1.0f, 100000000.0f);
                }

                if (Supercruise) CurrentSpeed *= 1000.0f;

                transform.Translate(Velocity * CurrentSpeed);
            }

            if (Body != null)
            {
                var worldPosition = (Vector3d)(transform.position - Body.Origin);

                // NOTE : Body shape dependent...
                if (worldPosition.Magnitude() < Body.Size + Body.SizeOffset + Body.HeightZ)
                {
                    worldPosition = worldPosition.Normalized(Body.Size + Body.SizeOffset + Body.HeightZ);
                }

                transform.position = worldPosition + (Vector3d)Body.Origin;
            }
        }

        private void UpdateDistances()
        {
            if (Body != null)
            {
                DistanceToAlign = Body.Size * 1.025f;
                DistanceToCore = Vector3.Distance(transform.position, Body.transform.position);
            }
        }

        private void UpdateClipPlanes()
        {
            if (DynamicClipPlanes)
            {
                if (Body != null)
                {
                    // NOTE : Body shape dependent...
                    var h = (DistanceToCore - Body.Size - (float)Body.HeightZ);

                    if (h < 1.0f) { h = 1.0f; }

                    CameraComponent.nearClipPlane = Mathf.Clamp(0.1f * h, 0.03f, 1000.0f);
                    CameraComponent.farClipPlane = Mathf.Clamp(1e6f * h, 1000.0f, 1e12f);
                }
                else
                {
                    CameraComponent.nearClipPlane = NearClipPlaneCache;
                    CameraComponent.farClipPlane = FarClipPlaneCache;
                }
            }
            else
            {
                CameraComponent.nearClipPlane = NearClipPlaneCache;
                CameraComponent.farClipPlane = FarClipPlaneCache;
            }
        }

        public override void UpdateMatrices()
        {
            WorldToCameraMatrix = CameraComponent.GetWorldToCamera();
            CameraToWorldMatrix = CameraComponent.GetCameraToWorld();
            CameraToScreenMatrix = CameraComponent.GetCameraToScreen();
            ScreenToCameraMatrix = CameraComponent.GetScreenToCamera();

            WorldCameraPosition = transform.position;
        }

        private void RotateAround(bool staticRotation = false)
        {
            var mouseX = (Input.GetAxis("Mouse Y") * 480.0f) / CameraComponent.pixelWidth;
            var mouseY = (Input.GetAxis("Mouse X") * 440.0f) / CameraComponent.pixelHeight;

            if (staticRotation) Rotation = Vector3.zero;

            Rotation.x += mouseX;
            Rotation.y -= mouseY;
            Rotation.z = 0;

            if (Body != null && !Aligned)
            {
                if (staticRotation)
                {
                    Rotation.x *= 10.0f;
                    Rotation.y *= 10.0f;

                    RotateAround(Rotation);

                    Rotation = Vector3.zero;
                }
                else
                {
                    RotateAround(Rotation, new Vector3(0, 0, -DistanceToCore));
                }
            }
        }

        private void RotateAround(Vector3 rotationVector)
        {
            if (Body != null)
            {
                RotateAroundOrigin(rotationVector, Body.Origin);
            }
            else
            {
                RotateAroundOrigin(rotationVector, Vector3.zero);
            }
        }

        private void RotateAround(Vector3 rotationVector, Vector3 distanceVector)
        {
            if (Body != null)
            {
                RotateAroundOrigin(rotationVector, distanceVector, Body.Origin);
            }
            else
            {
                RotateAroundOrigin(rotationVector, distanceVector, Vector3.zero);
            }
        }

        private void RotateAroundOrigin(Vector3 rotationVector, Vector3 origin)
        {
            transform.RotateAround(origin, Vector3.up, rotationVector.x * (Time.fixedDeltaTime * RotationSpeed) * 10.0f);
            transform.RotateAround(origin, Vector3.up, rotationVector.y * (Time.fixedDeltaTime * RotationSpeed) * 10.0f);
        }

        private void RotateAroundOrigin(Vector3 rotationVector, Vector3 distanceVector, Vector3 origin)
        {
            var currentRotation = Quaternion.Euler(rotationVector + TargetRotation.eulerAngles);
            var currentPosition = currentRotation * distanceVector + origin;

            transform.rotation = Quaternion.Slerp(transform.rotation, currentRotation, (Time.fixedDeltaTime * RotationSpeed) * 10.0f);
            transform.position = Vector3.Slerp(transform.position, currentPosition, (Time.deltaTime * RotationSpeed) * 5.0f);
        }
    }
}