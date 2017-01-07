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

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Startfield
{
    public class Starfield : MonoBehaviour
    {
        public float StarIntensity = 1.0f;
        public float StarsScale = 1.0f;
        public float StarsDistance = 1000.0f;

        public float HDRExposure = 0.2f;

        public Shader StarfieldShader;
        public Material StarfieldMaterial;

        public Mesh BillboardMesh;
        public Mesh StarfieldMesh;

        [HideInInspector]
        public AtmosphereHDR HDRMode = AtmosphereHDR.Proland;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Background;
        public int RenderQueueOffset = 0;

        private void Start()
        {
            InitMaterials();
            InitMesh();
        }

        public void Render(int drawLayer = 8)
        {
            Render(CameraHelper.Main(), drawLayer);
        }

        public void Render(Camera camera, int drawLayer = 8)
        {
            if (StarfieldMesh != null && StarfieldMaterial != null)
            {
                SetUniforms(StarfieldMaterial);

                StarfieldMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

                Graphics.DrawMesh(StarfieldMesh, transform.localToWorldMatrix, StarfieldMaterial, drawLayer, CameraHelper.Main(), 0, null, false, false);
            }
        }

        [ContextMenu("InitMesh")]
        public void InitMesh()
        {
            float starSize = StarsDistance / 100 * StarsScale;

            BillboardMesh = MeshFactory.MakeBillboardQuad(starSize);
            StarfieldMesh = CreateStarfieldMesh(StarsDistance);
        }

        public void InitMaterials()
        {
            if (StarfieldMaterial == null)
            {
                StarfieldMaterial = MaterialHelper.CreateTemp(StarfieldShader, "Starfield");
            }
        }

        public void SetUniforms(Material mat)
        {
            if (mat == null) return;

            mat.SetFloat("_StarIntensity", StarIntensity);
            mat.SetMatrix("_RotationMatrix", Matrix4x4.identity);

            mat.SetFloat("_Exposure", HDRExposure);
            mat.SetFloat("_HDRMode", (int)HDRMode);
        }

        private Mesh CreateStarfieldMesh(float starDistance)
        {
            const int numberOfStars = 9110;

            var dataFile = Resources.Load("Json/Stars", typeof(TextAsset)) as TextAsset;

            if (dataFile == null)
            {
                Debug.Log("Starfield: Data file reading error!");

                return null;
            }

            var starsData = JsonConvert.DeserializeObject<StarfieldStarJson[]>(dataFile.text).ToList();
            var starsCIs = new List<CombineInstance>();

            for (int i = 0; i < numberOfStars - 1; i++)
            {
                var star = new StarfieldStar(starsData[i]);

                float magnitude = Vector3.Dot(new Vector3(star.Color.r, star.Color.g, star.Color.b), new Vector3(0.22f, 0.707f, 0.071f));

                star.Color.a = magnitude;
                star.Position = Vector3.Scale(star.Position, new Vector3(-1.0f, 1.0f, -1.0f));

                CombineInstance ci = new CombineInstance();

                ci.mesh = BillboardMesh;
                ci.mesh.colors = new[] { star.Color, star.Color, star.Color, star.Color };
                ci.transform = MatrixHelper.BillboardMatrix(star.Position * starDistance);

                starsCIs.Add(ci);
            }

            var mesh = new Mesh();
            mesh.name = string.Format("StarfieldMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
            mesh.CombineMeshes(starsCIs.ToArray());
            ;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
            mesh.hideFlags = HideFlags.DontSave;

            return mesh;
        }
    }
}