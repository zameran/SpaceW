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
// Creation Date: 2017.05.04
// Creation Time: 1:46 PM
// Creator: zameran
#endregion

using System.Collections.Generic;
using UnityEngine;

namespace SpaceEngine.Test
{
    public class InstancingBufferTest : MonoBehaviour
    {
        public Material TestMaterial;

        public Mesh TestMesh;

        public MaterialPropertyBlock MPB;

        [Range(1, 1024)]
        public int Count = 128;

        public bool PerFrameInstancesUpdate = true;

        public readonly List<Matrix4x4> TRSs = new List<Matrix4x4>();

        protected void CalculateInstancesData()
        {
            TRSs.Clear();
            MPB.Clear();

            for (var i = 0; i < Count; i++)
            {
                var position = Random.insideUnitSphere * 100;
                var rotation = Random.rotationUniform;

                var r = Random.Range(0.0f, 1.0f);
                var g = Random.Range(0.0f, 1.0f);
                var b = Random.Range(0.0f, 1.0f);

                MPB.SetColor("_Color", new Color(r, g, b));

                TRSs.Add(Matrix4x4.TRS(position, rotation, Vector3.one));
            }
        }

        private void Start()
        {
            MPB = new MaterialPropertyBlock();

            CalculateInstancesData();
        }

        private void Update()
        {
            if (TestMesh != null && TestMaterial != null)
            {
                if (PerFrameInstancesUpdate == true) CalculateInstancesData();

                for (var i = 0; i < Count; i++)
                {
                    Graphics.DrawMesh(TestMesh, TRSs[i], TestMaterial, 0, Camera.main, 0, MPB);
                }
            }
        }
    }
}