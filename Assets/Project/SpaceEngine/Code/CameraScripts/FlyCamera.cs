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

public sealed class FlyCamera : GameCamera
{
    public float speed = 1.0f;
    public float rotationSpeed = 3.0f;
    public float alignDistance = 1024.0f;

    public Planetoid planetoid;
    public GameObject planetoidGameObject;

    private float currentSpeed;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    public float distanceToPlanetCore;

    private float nearClipPlaneCache;
    private float farClipPlaneCache;

    public bool dynamicClipPlanes = false;
    public bool aligned = false;
    public bool controllable = true;

    private Ray ray;

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

        UpdateClipPlanes();

        if (controllable)
        {
            if (Input.GetMouseButton(0))
            {
                rotation.z = 0;

                ray = CameraComponent.ScreenPointToRay(Input.mousePosition);

                targetRotation = Quaternion.LookRotation((ray.origin + ray.direction * 10.0f) - transform.position, transform.up);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

                if (!aligned)
                    transform.Rotate(new Vector3(0.0f, 0.0f, rotation.z));
            }
            else if (Input.GetMouseButton(1))
            {
                rotation.x += (Input.GetAxis("Mouse Y") * 480.0f) / CameraComponent.pixelWidth;
                rotation.y -= (Input.GetAxis("Mouse X") * 440.0f) / CameraComponent.pixelHeight;
                rotation.z = 0;

                if (planetoidGameObject != null && !aligned)
                    RotateAround(rotation, new Vector3(0, 0, -distanceToPlanetCore));
            }
            else
            {
                if (Input.GetKey(KeyCode.E))
                {
                    rotation.z -= 1.0f * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.Q))
                {
                    rotation.z += 1.0f * Time.deltaTime;
                }
                else
                {
                    rotation.z = Mathf.Lerp(rotation.z, 0, Time.deltaTime * 2.0f);
                }

                rotation.z = Mathf.Clamp(rotation.z, -100.0f, 100.0f);

                if (!aligned)
                    transform.Rotate(new Vector3(0, 0, rotation.z));
            }

            if (planetoidGameObject != null)
            {
                distanceToPlanetCore = Vector3.Distance(transform.position, planetoidGameObject.transform.position);

                if (distanceToPlanetCore < alignDistance)
                {
                    aligned = true;

                    var gravityVector = planetoidGameObject.transform.position - transform.position;

                    targetRotation = Quaternion.LookRotation(transform.forward, -gravityVector);

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed * 3f);
                }
                else
                {
                    aligned = false;
                }
            }

            velocity.z = Input.GetAxis("Vertical");
            velocity.x = Input.GetAxis("Horizontal");

            currentSpeed = speed;

            if (Input.GetKey(KeyCode.LeftShift))
                currentSpeed = speed * 10f;
            if (Input.GetKey(KeyCode.Space))
                currentSpeed = speed * 100f;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Space))
                currentSpeed = speed * 1000f;
            if (Input.GetKey(KeyCode.LeftControl))
                currentSpeed = speed / 10f;

            speed += Mathf.RoundToInt(Input.GetAxis("Mouse ScrollWheel") * 100.0f);
            speed = Mathf.Clamp(speed, 1.0f, 100.0f);

            transform.Translate(velocity * currentSpeed);
        }
    }

    protected override void Init()
    {
        if (planetoid == null)
            if (planetoidGameObject != null)
                planetoid = planetoidGameObject.GetComponent<Planetoid>();

        if (planetoidGameObject != null)
        {
            distanceToPlanetCore = Vector3.Distance(transform.position, planetoidGameObject.transform.position);
        }

        nearClipPlaneCache = CameraComponent.nearClipPlane;
        farClipPlaneCache = CameraComponent.farClipPlane;

        rotation = transform.eulerAngles;
        rotation.z = 0; // NOTE : Prevent crazy rotation on start...

        UpdateClipPlanes();
    }

    private void UpdateClipPlanes()
    {
        if (dynamicClipPlanes)
        {
            if (planetoid != null)
            {
                var h = (distanceToPlanetCore - planetoid.PlanetRadius - planetoid.TerrainMaxHeight);

                if (h < 1.0f) { h = 1.0f; }

                //NOTE : 0.01 is too small value for near clip plane...

                CameraComponent.nearClipPlane = Mathf.Clamp(0.3f * Mathf.Abs(h), 0.3f, 1000.0f);
                CameraComponent.farClipPlane = Mathf.Clamp(1e8f * Mathf.Abs(h), 1000.0f, 1e8f);
            }
            else
            {
                CameraComponent.nearClipPlane = nearClipPlaneCache;
                CameraComponent.farClipPlane = farClipPlaneCache;
            }
        }
        else
        {
            CameraComponent.nearClipPlane = nearClipPlaneCache;
            CameraComponent.farClipPlane = farClipPlaneCache;
        }
    }

    private void RotateAround(Vector3 rotationVector, Vector3 distanceVector)
    {
        var rotation = Quaternion.Euler(rotationVector + targetRotation.eulerAngles);
        var position = rotation * distanceVector + planetoidGameObject.transform.position;

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, (Time.fixedDeltaTime * rotationSpeed) * 10.0f);
        transform.position = Vector3.Slerp(transform.position, position, (Time.deltaTime * rotationSpeed) * 5.0f);
    }
}