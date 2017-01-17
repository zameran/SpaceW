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

namespace SpaceEngine.Cameras
{
    public sealed class MouseOrbitImproved : GameCamera
    {
        public Transform target;

        public float xSpeed = 2.0f;
        public float ySpeed = 60.0f;

        public float yMinLimit = -90f;
        public float yMaxLimit = 90f;

        public float distanceMin = 2.0f;
        public float distanceMax = 256.0f;

        private Rigidbody rb;

        float x = 0.0f;
        float y = 0.0f;
        float distance = 5.0f;

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (target)
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                var rotation = Quaternion.Euler(y, x, 0);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

                RaycastHit hit;

                if (Physics.Linecast(target.position, transform.position, out hit)) distance -= hit.distance;

                var negDistance = new Vector3(0.0f, 0.0f, -distance);
                var position = rotation * negDistance + target.position;

                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1.0f * Time.deltaTime);
                transform.position = position; //Vector3.Slerp(transform.position, position, 1.0f * Time.deltaTime);
            }
        }

        protected override void Init()
        {
            var angles = transform.eulerAngles;

            x = angles.y;
            y = angles.x;

            rb = GetComponent<Rigidbody>();

            if (target != null) distance = Vector3.Distance(target.position, transform.position);
            if (rb != null) rb.freezeRotation = true;
        }
    }
}