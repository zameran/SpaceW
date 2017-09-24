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

using SpaceEngine.Core;
using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Patterns.Strategy.Renderable;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;

using UnityEngine;

namespace SpaceEngine.AtmosphericScattering.Cloudsphere
{
    public class Cloudsphere : Node<Cloudsphere>, IUniformed<Material>, IRenderable<Cloudsphere>
    {
        public Body ParentBody;

        public Mesh CloudsphereMesh;

        public Shader CloudShader;
        public Material CloudMaterial;

        public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
        public int RenderQueueOffset = 0;

        public float Radius;
        public float Height;

        [Range(0.0f, 1.0f)]
        public float TransmittanceOffset = 0.625f;

        #region Node

        protected override void InitNode()
        {
            InitMaterials();

            InitUniforms(CloudMaterial);
        }

        protected override void UpdateNode()
        {
            CloudMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

            SetUniforms(CloudMaterial);
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
            //Helper.Destroy(CloudsphereMesh);
            Helper.Destroy(CloudMaterial);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed

        public void InitUniforms(Material target)
        {
            if (target == null) return;

            ParentBody.InitUniforms(target);

            target.SetFloat("_TransmittanceOffset", TransmittanceOffset);
        }

        public void SetUniforms(Material target)
        {
            if (target == null) return;

            ParentBody.SetUniforms(target);

            target.SetFloat("_TransmittanceOffset", TransmittanceOffset);
        }

        public void InitSetUniforms()
        {
            InitUniforms(CloudMaterial);
            SetUniforms(CloudMaterial);
        }

        #endregion

        #region IRenderable

        public void Render(int layer = 0)
        {
            if (CloudsphereMesh == null) return;

            var CloudsTRS = Matrix4x4.TRS(ParentBody.transform.position, transform.rotation, Vector3.one * (Radius + Height));

            Graphics.DrawMesh(CloudsphereMesh, CloudsTRS, CloudMaterial, layer, CameraHelper.Main(), 0, ParentBody.MPB);
        }

        #endregion

        public void InitMaterials()
        {
            CloudMaterial = MaterialHelper.CreateTemp(CloudShader, "Cloudsphere", (int)RenderQueue);
        }
    }
}