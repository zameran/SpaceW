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
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
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
// Creation Date: 2018.03.20
// Creation Time: 12:42 PM
// Creator: zameran
#endregion

using SpaceEngine.Core.Bodies;

using UnityEngine;

namespace SpaceEngine
{
    // TODO : Make it generic. See TerrainQuad per class TODO's...
    public class BodyRotator : MonoBehaviour
    {
        private readonly CachedComponent<Body> BodyCachedComponent = new CachedComponent<Body>();

        public Body BodyComponent { get { return BodyCachedComponent.Component; } }

        public Vector3 RotationAxis = Vector3.up;

        public float RotationSpeed = 0.25f;

        private void Start()
        {
            BodyCachedComponent.TryInit(this);
        }

        private void Update()
        {
            var terrainNodes = BodyComponent.TerrainNodes;

            for (var terrainNodeIndex = 0; terrainNodeIndex < terrainNodes.Count; terrainNodeIndex++)
            {
                var terrainNode = terrainNodes[terrainNodeIndex];

                // NOTE : Maybe full matrices recalculations needed via TerrainQuadRoot.CalculateMatrices(...)
                terrainNode.transform.Rotate(RotationAxis * RotationSpeed * Time.deltaTime);   // NOTE : Perform our transformation action...
                terrainNode.TerrainQuadRoot.UpdateMatrices();                                  // NOTE : Recalculate and update critical variables, required for proper rendering.
            }
        }
    }
}