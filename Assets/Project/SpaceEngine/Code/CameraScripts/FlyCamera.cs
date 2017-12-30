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

using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Numerics.Vectors;

using System.ComponentModel;

using UnityEngine;

namespace SpaceEngine.Cameras
{
    [ExecutionOrder(-9990)]
    public class FlyCamera : GameCamera
    {
        public enum ClipPlanesControl : byte
        {
            None,
            Constant,
            NearFar,
            Near,
            Far
        }

        public Body Body { get { return GodManager.Instance.ActiveBody; } }

        public float NearClipPlane { get { return CameraComponent.nearClipPlane; } set { CameraComponent.nearClipPlane = value; } }
        public float FarClipPlane { get { return CameraComponent.farClipPlane; } set { CameraComponent.farClipPlane = value; } }

        public ClipPlanesControl ClipPlanesControlType = ClipPlanesControl.NearFar;

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

        private bool Aligned = false;
        private bool Supercruise = false;

        private Ray RayScreen;

        protected override void Init()
        {
            NearClipPlaneCache = NearClipPlane;
            FarClipPlaneCache = FarClipPlane;

            Rotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0); // NOTE : Prevent crazy rotation on start...

            UpdateDistances();
            UpdateClipPlanes();

            MainRenderer.Instance.ComposeOutputRender();
        }

        protected override void Update()
        {
            base.Update();

            if (Controllable)
            {
                Supercruise = !Aligned && Input.GetKey(KeyCode.T);

                if (Input.GetMouseButton(0) && !MouseOverUI)
                {
                    RayScreen = CameraComponent.ScreenPointToRay(Input.mousePosition);

                    var gravityVector = (RayScreen.origin + RayScreen.direction * 10.0f) - transform.position;

                    TargetRotation = Quaternion.LookRotation(gravityVector, transform.up);
                    Rotation.z = 0;

                    RotateSlerp(Time.deltaTime * RotationSpeed);
                }
                else if (Input.GetMouseButton(1) && !MouseOverUI)
                {
                    RotateAround(true); // NOTE : Force this rotation mode...
                }
                else
                {
                    if (!Aligned)
                    {
                        if (Input.GetKey(KeyCode.E))
                        {
                            Rotation.z -= 0.20f * Time.deltaTime;
                        }
                        else if (Input.GetKey(KeyCode.Q))
                        {
                            Rotation.z += 0.20f * Time.deltaTime;
                        }
                        else
                        {
                            Rotation.z = Mathf.Lerp(Rotation.z, 0, Time.deltaTime * 1.25f);
                        }

                        Rotation.z = Mathf.Clamp(Rotation.z, -100.0f, 100.0f);

                        transform.Rotate(new Vector3(0, 0, Rotation.z));

                        if (Input.GetKey(KeyCode.G))
                        {
                            var gravityVector = (Body != null ? Body.Origin : Vector3.zero) - transform.position;

                            TargetRotation = Quaternion.LookRotation(gravityVector, transform.up);

                            RotateSlerp(Time.deltaTime * RotationSpeed * 1.57f);
                        }
                    }
                }

                // NOTE : Body shape dependent...
                if (Aligned)
                {
                    var gravityVector = (Body != null ? Body.Origin : Vector3.zero) - transform.position;

                    TargetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);

                    RotateSlerp(Time.deltaTime * RotationSpeed * 3.0f);
                }

                Velocity.z = Input.GetAxis("Vertical");
                Velocity.y = Input.GetAxis("Diagonal");
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
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            UpdateDistances();
            UpdateClipPlanes();
            UpdatePseudoCollision();
        }

        private void UpdatePseudoCollision()
        {
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
                DistanceToCore = Vector3.Distance(transform.position, Body.Origin);
            }
            else
            {
                DistanceToAlign = 0.0f;
                DistanceToCore = Vector3.Distance(transform.position, Vector3.zero);
            }

            Aligned = DistanceToCore < DistanceToAlign;
        }

        private float CalculateNearClipPlane(float h)
        {
            return Mathf.Clamp(0.1f * h, 0.03f, 1000.0f);
        }

        private float CalculateFarClipPlane(float h)
        {
            return Mathf.Clamp(1e6f * h, 1000.0f, 1e12f);
        }

        private void UpdateClipPlanes()
        {
            // NOTE : Body's shape dependent...
            var h = Body != null ? (DistanceToCore - Body.Size - (float)Body.HeightZ) : 1.0f;

            if (h < 1.0f) { h = 1.0f; }

            var calculatedNearClipPlane = Body != null ? CalculateNearClipPlane(h) : NearClipPlaneCache;
            var calculatedFarClipPlane = Body != null ? CalculateFarClipPlane(h) : FarClipPlaneCache;

            switch (ClipPlanesControlType)
            {
                case ClipPlanesControl.None:
                {
                    // NOTE : What?!
                } break;
                case ClipPlanesControl.Constant:
                {
                    NearClipPlane = NearClipPlaneCache;
                    FarClipPlane = FarClipPlaneCache;
                } break;
                case ClipPlanesControl.NearFar:
                {
                    NearClipPlane = calculatedNearClipPlane;
                    FarClipPlane = calculatedFarClipPlane;
                } break;
                case ClipPlanesControl.Near:
                {
                    NearClipPlane = calculatedNearClipPlane;
                } break;
                case ClipPlanesControl.Far:
                {
                    FarClipPlane = calculatedFarClipPlane;
                } break;
                default: { throw new InvalidEnumArgumentException(); }
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

        private void RotateSlerp(float speed)
        {
            RotateSlerp(transform.rotation, TargetRotation, speed);
        }

        private void RotateSlerp(Quaternion targetRotation, float speed)
        {
            RotateSlerp(transform.rotation, targetRotation, speed);
        }

        private void RotateSlerp(Quaternion currentRotation, Quaternion targetRotation, float speed)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, speed);
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
                RotateAroundOrigin(rotationVector, Body.Origin, RotationSpeed);
            }
            else
            {
                RotateAroundOrigin(rotationVector, Vector3.zero, RotationSpeed);
            }
        }

        private void RotateAround(Vector3 rotationVector, Vector3 distanceVector)
        {
            if (Body != null)
            {
                RotateAroundOrigin(rotationVector, distanceVector, Body.Origin, RotationSpeed);
            }
            else
            {
                RotateAroundOrigin(rotationVector, distanceVector, Vector3.zero, RotationSpeed);
            }
        }

        private void RotateAroundOrigin(Vector3 rotationVector, Vector3 origin, float rotationSpeed)
        {
            transform.RotateAround(origin, Vector3.up, rotationVector.x * (Time.deltaTime * rotationSpeed) * 10.0f);
            transform.RotateAround(origin, Vector3.up, rotationVector.y * (Time.deltaTime * rotationSpeed) * 10.0f);
        }

        private void RotateAroundOrigin(Vector3 rotationVector, Vector3 distanceVector, Vector3 origin, float rotationSpeed)
        {
            var currentRotation = Quaternion.Euler(rotationVector + TargetRotation.eulerAngles);
            var currentPosition = currentRotation * distanceVector + origin;

            transform.rotation = Quaternion.Slerp(transform.rotation, currentRotation, (Time.deltaTime * rotationSpeed) * 10.0f);
            transform.position = Vector3.Slerp(transform.position, currentPosition, (Time.deltaTime * rotationSpeed) * 5.0f);
        }
    }
}