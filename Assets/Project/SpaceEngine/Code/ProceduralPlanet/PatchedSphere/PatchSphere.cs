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

using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class PatchSphere : MonoBehaviour
{
    public Shader CoreShader;
    public Shader Shader;

    [HideInInspector]
    public Material CoreMaterial;

    public Planet ParentPlanet;

    public PatchQuality PatchQuality = PatchQuality.Standard;
    public PatchResolution PatchResoulution = PatchResolution.Standard;

    public TextureWrapMode WrapMode = TextureWrapMode.Clamp;
    public FilterMode TextureFilterMode = FilterMode.Bilinear;
    public bool Mipmaps = false;
    public bool POT = true;
    public int AnisoLevel = 0;

    public float Radius = 64;

    public int MaxSplitLevel = 8;
    public float SizeSplit = 3;
    public float SizeRejoin = 6;
    public float TerrainMaxHeight = 8;

    public List<PatchTree> PatchTrees = new List<PatchTree>();

    public ushort HighestSplitLevel { get; set; }

    public bool Splitted { get; set; }

    public bool Rejoined { get; set; }

    public float MinCamDist { get; set; }

    public PatchTree CloserNode { get; set; }

    public PatchManager PatchManager { get; private set; }

    public PatchConfig PatchConfig { get; private set; }

    private void Start()
    {
        Rebuild();
    }

    public void DestroyPlanet()
    {
        for (int i = 0; i < PatchTrees.Count; i++)
        {
            PatchTrees[i].DestroyTree();
        }

        PatchTrees.Clear();

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
        }
    }

    public void RenderQuadVolume(float width, float height, Material material, PatchAABB volume, int pass)
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Viewport(new Rect(0, 0, width, height));

        material.SetPass(pass);

        Vector3 v1 = volume.vertices[0];
        Vector3 uv1 = volume.uvs[0];

        Vector3 v2 = volume.vertices[3];
        Vector3 uv2 = volume.uvs[3];

        Vector3 v3 = volume.vertices[2];
        Vector3 uv3 = volume.uvs[2];

        Vector3 v4 = volume.vertices[1];
        Vector3 uv4 = volume.uvs[1];

        GL.Begin(GL.QUADS);

        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            GL.MultiTexCoord(0, new Vector3(0, 0, 0));
            GL.MultiTexCoord(1, v1);
            GL.MultiTexCoord(2, uv1);
            GL.Vertex3(-1, -1, 0);

            GL.MultiTexCoord(0, new Vector3(0, 1, 0));
            GL.MultiTexCoord(1, v2);
            GL.MultiTexCoord(2, uv2);
            GL.Vertex3(-1, 1, 0);

            GL.MultiTexCoord(0, new Vector3(1, 1, 0));
            GL.MultiTexCoord(1, v3);
            GL.MultiTexCoord(2, uv3);
            GL.Vertex3(1, 1, 0);

            GL.MultiTexCoord(0, new Vector3(1, 0, 0));
            GL.MultiTexCoord(1, v4);
            GL.MultiTexCoord(2, uv4);
            GL.Vertex3(1, -1, 0);
        }
        else
        {
            GL.MultiTexCoord(0, new Vector3(0, 0, 0));
            GL.MultiTexCoord(1, v1);
            GL.MultiTexCoord(2, uv1);
            GL.Vertex3(-1, 1, 0);

            GL.MultiTexCoord(0, new Vector3(0, 1, 0));
            GL.MultiTexCoord(1, v2);
            GL.MultiTexCoord(2, uv2);
            GL.Vertex3(-1, -1, 0);

            GL.MultiTexCoord(0, new Vector3(1, 1, 0));
            GL.MultiTexCoord(1, v3);
            GL.MultiTexCoord(2, uv3);
            GL.Vertex3(1, -1, 0);

            GL.MultiTexCoord(0, new Vector3(1, 0, 0));
            GL.MultiTexCoord(1, v4);
            GL.MultiTexCoord(2, uv4);
            GL.Vertex3(1, 1, 0);
        }

        GL.End();
        GL.PopMatrix();
    }

    public void Rebuild()
    {
        PatchConfig = new PatchConfig(PatchQuality, PatchResoulution);
        PatchManager = new PatchManager(PatchConfig, this);

        CoreMaterial = MaterialHelper.CreateTemp(CoreShader, "PatchCore");
        CoreMaterial.hideFlags = HideFlags.DontSave;

        DestroyPlanet();

        PatchTrees.Add(new PatchTree(new Vector3(0, 1, 0), new Vector3(0, 0, -1), this));      //0 : top
        PatchTrees.Add(new PatchTree(new Vector3(0, -1, 0), new Vector3(0, 0, 1), this));      //1 : bottom

        PatchTrees.Add(new PatchTree(new Vector3(-1, 0, 0), new Vector3(0, 1, 0), this));      //2 : left
        PatchTrees.Add(new PatchTree(new Vector3(1, 0, 0), new Vector3(0, 1, 0), this));       //3 : right

        PatchTrees.Add(new PatchTree(new Vector3(0, 0, 1), new Vector3(0, 1, 0), this));       //4 : front
        PatchTrees.Add(new PatchTree(new Vector3(0, 0, -1), new Vector3(0, 1, 0), this));      //5 : back

        //link neighbors of TOP quadtree
        PatchTrees[0].SetNeighbor(NeighborDirection.Top, PatchTrees[5], NeighborDirection.Top);         //back
        PatchTrees[0].SetNeighbor(NeighborDirection.Bottom, PatchTrees[4], NeighborDirection.Top);      //front
        PatchTrees[0].SetNeighbor(NeighborDirection.Left, PatchTrees[2], NeighborDirection.Top);        //left
        PatchTrees[0].SetNeighbor(NeighborDirection.Right, PatchTrees[3], NeighborDirection.Top);       //right

        //link neighbors of BOTTOM quadtree
        PatchTrees[1].SetNeighbor(NeighborDirection.Top, PatchTrees[4], NeighborDirection.Bottom);      //front
        PatchTrees[1].SetNeighbor(NeighborDirection.Bottom, PatchTrees[5], NeighborDirection.Bottom);   //back
        PatchTrees[1].SetNeighbor(NeighborDirection.Left, PatchTrees[2], NeighborDirection.Bottom);     //left
        PatchTrees[1].SetNeighbor(NeighborDirection.Right, PatchTrees[3], NeighborDirection.Bottom);    //right

        //link neighbors of LEFT quadtree
        PatchTrees[2].SetNeighbor(NeighborDirection.Top, PatchTrees[0], NeighborDirection.Left);        //top
        PatchTrees[2].SetNeighbor(NeighborDirection.Bottom, PatchTrees[1], NeighborDirection.Left);     //bottom
        PatchTrees[2].SetNeighbor(NeighborDirection.Left, PatchTrees[5], NeighborDirection.Right);      //back
        PatchTrees[2].SetNeighbor(NeighborDirection.Right, PatchTrees[4], NeighborDirection.Left);      //front

        //link neighbors of RIGHT quadtree
        PatchTrees[3].SetNeighbor(NeighborDirection.Top, PatchTrees[0], NeighborDirection.Right);       //top
        PatchTrees[3].SetNeighbor(NeighborDirection.Bottom, PatchTrees[1], NeighborDirection.Right);    //bottom
        PatchTrees[3].SetNeighbor(NeighborDirection.Left, PatchTrees[4], NeighborDirection.Right);      //front
        PatchTrees[3].SetNeighbor(NeighborDirection.Right, PatchTrees[5], NeighborDirection.Left);      //back

        //link neighbors of FRONT quadtree
        PatchTrees[4].SetNeighbor(NeighborDirection.Top, PatchTrees[0], NeighborDirection.Bottom);      //top
        PatchTrees[4].SetNeighbor(NeighborDirection.Bottom, PatchTrees[1], NeighborDirection.Top);      //bottom
        PatchTrees[4].SetNeighbor(NeighborDirection.Left, PatchTrees[2], NeighborDirection.Right);      //left
        PatchTrees[4].SetNeighbor(NeighborDirection.Right, PatchTrees[3], NeighborDirection.Left);      //right

        //link neighbors of BACK quadtree
        PatchTrees[5].SetNeighbor(NeighborDirection.Top, PatchTrees[0], NeighborDirection.Top);         //top
        PatchTrees[5].SetNeighbor(NeighborDirection.Bottom, PatchTrees[1], NeighborDirection.Bottom);   //bottom
        PatchTrees[5].SetNeighbor(NeighborDirection.Left, PatchTrees[3], NeighborDirection.Right);      //right
        PatchTrees[5].SetNeighbor(NeighborDirection.Right, PatchTrees[2], NeighborDirection.Left);      //left
    }

    private void LateUpdate()
    {
        if (ParentPlanet == null) return;
        if (ParentPlanet.LODTarget == null) return;

        Vector3 InversedCameraPosition = transform.InverseTransformPoint(ParentPlanet.LODTarget.position);

        Splitted = Rejoined = false;
        HighestSplitLevel = 0;
        MinCamDist = 9999999.0f;
        CloserNode = null;

        for (int i = 0; i < PatchTrees.Count; i++)
        {
            PatchTrees[i].Update(InversedCameraPosition);
        }

        for (int i = 0; i < PatchTrees.Count; i++)
        {
            PatchTrees[i].RefreshTerrain(InversedCameraPosition, InversedCameraPosition);
        }

        for (int i = 0; i < PatchTrees.Count; i++)
        {
            PatchTrees[i].RefreshGaps();
        }
    }

    public void CallLateUpdate()
    {
        LateUpdate();
    }
}