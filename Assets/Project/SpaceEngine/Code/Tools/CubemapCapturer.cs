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
// Creation Date: 2016.05.16
// Creation Time: 18:58
// Creator: zameran
#endregion

//TODO : Make it work with space sheet. (Quads, Atmospheres etc)

using UnityEngine;

public class CubemapCapturer : MonoBehaviour
{
    public Planetoid planet = null;

    public int cubemapSize = 128;

    public bool oneFacePerFrame = false;

    public LayerMask layerMask;

    private Camera renderCamera;

    public RenderTexture cubeRenderTexture;

    void Start()
    {
        UpdateCubemap(63);
    }

    void LateUpdate()
    {
        if (oneFacePerFrame)
        {
            int faceToRender = Time.frameCount % 6;
            int faceMask = 1 << faceToRender;

            UpdateCubemap(faceMask);
        }
        else
        {
            UpdateCubemap(63);
        }
    }

    void UpdateCubemap(int faceMask)
    {
        Camera mainCamera = CameraHelper.Main();

        if (mainCamera == null || planet == null) return;

        if (!renderCamera)
        {
            GameObject go = new GameObject("CubemapCamera", typeof(Camera));
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.identity;
            go.transform.parent = mainCamera.transform;

            renderCamera = go.GetComponent<Camera>();
            renderCamera.cullingMask = layerMask;
            renderCamera.nearClipPlane = mainCamera.nearClipPlane;
            renderCamera.farClipPlane = mainCamera.farClipPlane;
            renderCamera.backgroundColor = mainCamera.backgroundColor;
            renderCamera.enabled = false;
        }

        if (cubeRenderTexture == null)
        {
            cubeRenderTexture = new RenderTexture(cubemapSize, cubemapSize, 16);
            cubeRenderTexture.isPowerOfTwo = true;
            cubeRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            cubeRenderTexture.hideFlags = HideFlags.HideAndDontSave;

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m.HasProperty("_Cube"))
                    {
                        m.SetTexture("_Cube", cubeRenderTexture);
                    }
                }
            }
        }

        planet.Render(renderCamera);

        renderCamera.transform.position = transform.position;

        renderCamera.RenderToCubemap(cubeRenderTexture, faceMask);
    }

    void OnDisable()
    {
        DestroyImmediate(renderCamera);

        if (cubeRenderTexture != null) cubeRenderTexture.ReleaseAndDestroy();
    }
}