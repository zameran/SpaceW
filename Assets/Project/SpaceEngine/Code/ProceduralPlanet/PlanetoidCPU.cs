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
// Creation Date: 2016.05.18
// Creation Time: 0:12
// Creator: zameran
#endregion

using System.Collections.Generic;

using UnityEngine;

public class PlanetoidCPU : MonoBehaviour, IPlanet
{
    public bool Working = false;

    public Transform LODTarget = null;

    public float PlanetRadius = 1024;

    public List<QuadCPU> MainQuads = new List<QuadCPU>();
    public List<QuadCPU> Quads = new List<QuadCPU>();

    public PlanetoidGenerationConstants GenerationConstants;

   
    public int RenderQueue = 2000;

    public float LODDistanceMultiplier = 1;

    public int LODMaxLevel = 12;
    public int[] LODDistances = new int[13] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2, 1, 0 };
    public float[] LODOctaves = new float[6] { 0.5f, 0.5f, 0.5f, 0.75f, 0.75f, 1.0f };

    public GameObject QuadsRoot = null;

    public float TerrainMaxHeight = 64.0f;

    public Vector3 Origin = Vector3.zero;
    public Quaternion OriginRotation = Quaternion.identity;
}