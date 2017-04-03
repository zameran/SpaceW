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

using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceEngine.Startfield
{
    public class Starfield : Node<Starfield>, IUniformed<Material>, IRenderable<Starfield>
    {
        public float StarIntensity = 1.0f;
        public float StarsScale = 1.0f;
        public float StarsDistance = 1000.0f;

        public Shader StarfieldShader;
        public Material StarfieldMaterial;

        public Mesh StarfieldMesh;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Background;
        public int RenderQueueOffset = 0;

        private readonly Vector4[] Tab =
        {
            new Vector2(0.897907815f, -0.347608525f), new Vector2(0.550299290f, 0.273586675f), new Vector2(0.823885965f, 0.098853070f),
            new Vector2(0.922739035f, -0.122108860f), new Vector2(0.800630175f, -0.088956800f), new Vector2(0.711673375f, 0.158864420f),
            new Vector2(0.870537795f, 0.085484560f), new Vector2(0.956022355f, -0.058114540f)
        };

        #region Node

        protected override void InitNode()
        {
            InitMaterials();
            InitMesh();

            InitUniforms(StarfieldMaterial);
        }

        protected override void UpdateNode()
        {
            StarfieldMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            SetUniforms(StarfieldMaterial);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            Helper.Destroy(StarfieldMaterial);
            Helper.Destroy(StarfieldMesh);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed

        public void InitUniforms(Material target)
        {
            if (target == null) return;
        }

        public void SetUniforms(Material target)
        {
            if (target == null) return;

            target.SetFloat("_StarIntensity", StarIntensity);
            target.SetMatrix("_RotationMatrix", Matrix4x4.identity);

            target.SetVectorArray("_Tab", Tab);
        }

        public void InitSetUniforms()
        {
            InitUniforms(StarfieldMaterial);
            SetUniforms(StarfieldMaterial);
        }

        #endregion

        #region IRenderable

        public void Render(int layer = 0)
        {
            if (StarfieldMesh == null) return;

            Graphics.DrawMesh(StarfieldMesh, transform.localToWorldMatrix, StarfieldMaterial, layer, CameraHelper.Main(), 0, null, false, false);
        }

        #endregion

        public void InitMesh()
        {
            StarfieldMesh = CreateStarfieldMesh(StarsDistance);
        }

        public void InitMaterials()
        {
            if (StarfieldMaterial == null)
            {
                StarfieldMaterial = MaterialHelper.CreateTemp(StarfieldShader, "Starfield");
            }
        }

        private Mesh CreateStarfieldMesh(float starDistance)
        {
            const int numberOfStars = 9110;

            var binaryDataFile = Resources.Load("Binary/Stars", typeof(TextAsset)) as TextAsset;

            if (binaryDataFile == null)
            {
                Debug.Log("Starfield: Binary data file reading error!");
                return null;
            }

            var starsCIs = new List<CombineInstance>();

            using (var reader = new BinaryReader(new MemoryStream(binaryDataFile.bytes)))
            {
                for (int i = 0; i < numberOfStars - 1; i++)
                {
                    var star = new StarfieldStar();
                    var starSize = StarsDistance / 100 * StarsScale;

                    // NOTE : Swap Z and Y...
                    star.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    star.Color = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);

                    star.Position = Vector3.Scale(star.Position, new Vector3(-1.0f, 1.0f, -1.0f));
                    star.Color.w = new Vector3(star.Color.x, star.Color.y, star.Color.z).magnitude;

                    if (star.Color.w > 5.7f)
                        star.Color = Vector4.Normalize(star.Color) * 0.5f;

                    var ci = new CombineInstance
                    {
                        mesh = MeshFactory.MakeBillboardQuad(starSize),
                        transform = MatrixHelper.BillboardMatrix(star.Position * starDistance)
                    };

                    ci.mesh.colors = new Color[] { star.Color, star.Color, star.Color, star.Color };

                    starsCIs.Add(ci);
                }
            }

            var mesh = new Mesh();
            mesh.name = string.Format("StarfieldMesh_({0})", Random.Range(float.MinValue, float.MaxValue));
            mesh.CombineMeshes(starsCIs.ToArray());
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
            mesh.hideFlags = HideFlags.DontSave;

            #region Cleanup

            foreach (var ci in starsCIs)
            {
                Helper.Destroy(ci.mesh);
            }

            starsCIs.Clear();

            GC.Collect();

            #endregion

            return mesh;
        }
    }
}