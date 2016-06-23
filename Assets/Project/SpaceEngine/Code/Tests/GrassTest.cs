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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

using UnityEngine;

public class GrassTest : MonoBehaviour
{
    public Texture2D grassTexture;
    public Texture2D noiseTexture;

    public Shader grassShader;
    public Material grassMaterial;

    public Grass[][] grass;

    public int size = 10;

    public int layer = 0;

    private bool canRender = false;

    private Mesh grassMesh = null;

    void Start()
    {
        grassMaterial = new Material(grassShader);
        grassMaterial.name = "GrassMaterial(Instance)_" + UnityEngine.Random.Range(float.MinValue, float.MaxValue);
        grassMaterial.SetTexture("_MainTex", grassTexture);
        grassMaterial.SetTexture("_NoiseTexture", noiseTexture);

        grassMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XZ, true, false, false);

        grass = new Grass[size][];

        for (int k = 0; k < grass.GetLength(0); k++)
        {
            grass[k] = new Grass[size];
        }

        for (int i = 0; i < grass.GetLength(0); i++)
        {
            for (int j = 0; j < grass[i].Length; j++)
            {
                grass[i][j] = new Grass(new Vector3(i, 0, j));
            }
        }

        canRender = true;
    }

    void OnDestroy()
    {

    }

    void OnPostRender()
    {
        if (!canRender) return;

        for (int i = 0; i < grass.GetLength(0); i++)
        {
            for (int j = 0; j < grass[i].Length; j++)
            {
                if (grass[i][j] == null) return;

                grassMaterial.SetPass(0);
                Graphics.DrawMeshNow(grassMesh, grass[i][j].position, Quaternion.identity);
                //grassMaterial.SetPass(1);
                //Graphics.DrawMeshNow(grassMesh, grass[i][j].position, Quaternion.identity);
            }
        }
    }

    void Update()
    {

    }
}

[Serializable]
public class Grass
{
    public Vector3 position;

    public Grass(Vector3 position)
    {
        this.position = position;
    }
}