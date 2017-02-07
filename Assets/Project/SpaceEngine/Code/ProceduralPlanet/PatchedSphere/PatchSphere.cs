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
    public Shader Shader;

    public float Radius = 64;

    public int MaxSplitLevel = 8;
    public float SizeSplit = 3;
    public float SizeRejoin = 6;

    public List<PatchTree> PatchTrees = new List<PatchTree>();

    public ushort HighestSplitLevel { get; set; }

    public bool Splitted { get; set; }

    public bool Rejoined { get; set; }

    public float MinCamDist { get; set; }

    public PatchTree CloserNode { get; set; }

    public PatchManager PatchManager { get; private set; }

    private void Start()
    {
        Rebuild();
    }

    private void Update()
    {
        var InversedCameraPosition = CameraHelper.Main().transform.position;

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
            PatchTrees[i].RefreshLOD();
        }

        for (int i = 0; i < PatchTrees.Count; i++)
        {
            PatchTrees[i].RefreshGaps();
        }
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

    public void Rebuild()
    {
        PatchManager = new PatchManager();

        DestroyPlanet();

        PatchTrees.Add(new PatchTree(new Vector3(0, 1, 0), new Vector3(0, 0, -1), this));      //0 : top
        PatchTrees.Add(new PatchTree(new Vector3(0, -1, 0), new Vector3(0, 0, 1), this));      //1 : bottom

        PatchTrees.Add(new PatchTree(new Vector3(-1, 0, 0), new Vector3(0, 1, 0), this));      //2 : left
        PatchTrees.Add(new PatchTree(new Vector3(1, 0, 0), new Vector3(0, 1, 0), this));       //3 : right

        PatchTrees.Add(new PatchTree(new Vector3(0, 0, 1), new Vector3(0, 1, 0), this));       //4 : front
        PatchTrees.Add(new PatchTree(new Vector3(0, 0, -1), new Vector3(0, 1, 0), this));      //5 : back

        //link neighbors of TOP quadtree
        PatchTrees[0].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[5], PatchNeighborDirection.Top);         //back
        PatchTrees[0].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[4], PatchNeighborDirection.Top);      //front
        PatchTrees[0].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[2], PatchNeighborDirection.Top);        //left
        PatchTrees[0].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[3], PatchNeighborDirection.Top);       //right

        //link neighbors of BOTTOM quadtree
        PatchTrees[1].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[4], PatchNeighborDirection.Bottom);      //front
        PatchTrees[1].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[5], PatchNeighborDirection.Bottom);   //back
        PatchTrees[1].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[2], PatchNeighborDirection.Bottom);     //left
        PatchTrees[1].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[3], PatchNeighborDirection.Bottom);    //right

        //link neighbors of LEFT quadtree
        PatchTrees[2].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[0], PatchNeighborDirection.Left);        //top
        PatchTrees[2].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[1], PatchNeighborDirection.Left);     //bottom
        PatchTrees[2].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[5], PatchNeighborDirection.Right);      //back
        PatchTrees[2].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[4], PatchNeighborDirection.Left);      //front

        //link neighbors of RIGHT quadtree
        PatchTrees[3].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[0], PatchNeighborDirection.Right);       //top
        PatchTrees[3].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[1], PatchNeighborDirection.Right);    //bottom
        PatchTrees[3].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[4], PatchNeighborDirection.Right);      //front
        PatchTrees[3].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[5], PatchNeighborDirection.Left);      //back

        //link neighbors of FRONT quadtree
        PatchTrees[4].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[0], PatchNeighborDirection.Bottom);      //top
        PatchTrees[4].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[1], PatchNeighborDirection.Top);      //bottom
        PatchTrees[4].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[2], PatchNeighborDirection.Right);      //left
        PatchTrees[4].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[3], PatchNeighborDirection.Left);      //right

        //link neighbors of BACK quadtree
        PatchTrees[5].SetNeighbor(PatchNeighborDirection.Top, PatchTrees[0], PatchNeighborDirection.Top);         //top
        PatchTrees[5].SetNeighbor(PatchNeighborDirection.Bottom, PatchTrees[1], PatchNeighborDirection.Bottom);   //bottom
        PatchTrees[5].SetNeighbor(PatchNeighborDirection.Left, PatchTrees[3], PatchNeighborDirection.Right);      //right
        PatchTrees[5].SetNeighbor(PatchNeighborDirection.Right, PatchTrees[2], PatchNeighborDirection.Left);      //left
    }

    public void CallUpdate()
    {
        Update();
    }
}