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

using System;
using System.Linq;

using UnityEngine;

using Object = UnityEngine.Object;

[Serializable]
public struct PatchData
{
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector3[] Volume;

    public Vector2[] UV1;
    public Vector2[] UV2;

    public PatchData(int gridSize)
    {
        Vertices = new Vector3[gridSize];
        Normals = new Vector3[gridSize];
        Volume = new Vector3[gridSize];

        UV2 = new Vector2[gridSize];
        UV1 = new Vector2[gridSize];
    }
}

[Serializable]
public class PatchTree
{
    public PatchSphere Sphere;

    public PatchData Data;

    public ushort SplitLevel;

    public bool HasChildren;
    public bool NeedsTerrain;
    public bool NeedsReedge;

    public float Size;

    Vector3 Up, Front, Right;
    Vector3 FrontProjected, UpProjected;

    public PatchAABB Volume;
    public PatchAABB VolumeProjected;

    Plane Plane;

    public Vector3 Middle;
    public Vector3 MiddleProjected;

    public Vector3 Normal;

    public GameObject GameObject;
    public Mesh Mesh;

    public PatchTree Parent;

    [NonSerialized]
    public PatchTree[] Children = new PatchTree[4];

    public PatchNeighbor[] Neighbors = new PatchNeighbor[4];

    byte NeedsRejoinCount;
    byte Edges;
    byte GapFixMask;

    public byte NEXT_EDGE(byte e)
    {
        return (byte)(e == 3 ? 0 : e + 1);
    }

    public byte PREV_EDGE(byte e)
    {
        return (byte)(e == 0 ? 3 : e - 1);
    }

    public PatchTree(Vector3 Up, Vector3 Front, PatchSphere Sphere)
    {
        this.Sphere = Sphere;

        this.Up = UpProjected = Up;
        this.Front = FrontProjected = Front;
        this.Right = -Vector3.Cross(this.Up, this.Front);

        Parent = null;
        SplitLevel = 0;
        Size = this.Sphere.Radius * 2;

        Neighbors[0].Node = Neighbors[1].Node = Neighbors[2].Node = Neighbors[3].Node = null;
        Neighbors[0].isFixed = Neighbors[1].isFixed = Neighbors[2].isFixed = Neighbors[3].isFixed = false;
        Children[0] = Children[1] = Children[2] = Children[3] = null;

        NeedsRejoinCount = 0;
        HasChildren = false;
        NeedsReedge = true;
        NeedsTerrain = true;
        GapFixMask = 15;

        GenerateVolume();

        Plane = new Plane(Volume.Vertices[0], Volume.Vertices[2], Volume.Vertices[1]);
    }

    public PatchTree(PatchTree Parent, PatchAABB Volume)
    {
        this.Parent = Parent;
        this.Volume = Volume;
        this.Sphere = Parent.Sphere;

        SplitLevel = (ushort)(this.Parent.SplitLevel + 1);

        if (SplitLevel > Sphere.HighestSplitLevel) Sphere.HighestSplitLevel = SplitLevel;

        Size = this.Parent.Size / 2;

        var v1 = this.Volume.Vertices[0];
        var v2 = this.Volume.Vertices[1];
        var v3 = this.Volume.Vertices[2];
        var v4 = this.Volume.Vertices[3];

        var uv1 = this.Volume.UVs[0];
        var uv2 = this.Volume.UVs[1];
        var uv3 = this.Volume.UVs[2];
        var uv4 = this.Volume.UVs[3];

        VolumeProjected = new PatchAABB();

        VolumeProjected.Vertices.Add(v1.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.Vertices.Add(v2.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.Vertices.Add(v3.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.Vertices.Add(v4.NormalizeToRadius(Sphere.Radius));

        VolumeProjected.UVs.Add(uv1);
        VolumeProjected.UVs.Add(uv2);
        VolumeProjected.UVs.Add(uv3);
        VolumeProjected.UVs.Add(uv4);

        Neighbors[0].Node = Neighbors[1].Node = Neighbors[2].Node = Neighbors[3].Node = null;
        Neighbors[0].isFixed = Neighbors[1].isFixed = Neighbors[2].isFixed = Neighbors[3].isFixed = false;
        Children[0] = Children[1] = Children[2] = Children[3] = null;

        NeedsRejoinCount = 0;
        HasChildren = false;
        NeedsReedge = true;
        NeedsTerrain = true;
        GapFixMask = 15;

        Normal = this.Parent.Normal;

        Middle = (Volume.Vertices[0] + Volume.Vertices[1] + Volume.Vertices[2] + Volume.Vertices[3]) / 4;
        MiddleProjected = Middle;
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius);

        UpProjected = MiddleProjected;
        UpProjected.Normalize();

        FrontProjected = Vector3.Lerp(VolumeProjected.Vertices[0], VolumeProjected.Vertices[1], 0.5f) - MiddleProjected;
        FrontProjected.Normalize();

        Plane = this.Parent.Plane;

        Front = this.Parent.Front;
        Up = this.Parent.Up;
        Right = this.Parent.Right;
    }

    public void DestroyTree()
    {
        if (HasChildren)
        {
            for (int i = 0; i < 4; i++)
            {
                Children[i].DestroyTree();
                Children[i] = null;
            }

            HasChildren = false;
        }

        DestroyNode();
    }

    private void DestroyNode()
    {
        if (GameObject != null)
        {
            GameObject.transform.parent = null;

            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(Mesh);
                Object.DestroyImmediate(GameObject);
            }
            else
            {
                Object.Destroy(Mesh);
                Object.Destroy(GameObject);
            }

            Mesh = null;
            GameObject = null;
        }
    }

    private void GenerateVolume()
    {
        var left = -Right;

        var v1 = (left * Sphere.Radius) + (Front * Sphere.Radius) + (Up * Sphere.Radius); //left far
        var v2 = (left * -Sphere.Radius) + (Front * Sphere.Radius) + (Up * Sphere.Radius); //right far
        var v3 = (left * -Sphere.Radius) + (Front * -Sphere.Radius) + (Up * Sphere.Radius); //right near
        var v4 = (left * Sphere.Radius) + (Front * -Sphere.Radius) + (Up * Sphere.Radius); //left near

        var uv1 = new Vector3(0, 0, 0);
        var uv2 = new Vector3(1, 0, 0);
        var uv3 = new Vector3(1, 1, 0);
        var uv4 = new Vector3(0, 1, 0);

        Volume = new PatchAABB();

        Volume.Vertices.Add(v1);
        Volume.Vertices.Add(v2);
        Volume.Vertices.Add(v3);
        Volume.Vertices.Add(v4);


        Volume.UVs.Add(uv1);
        Volume.UVs.Add(uv2);
        Volume.UVs.Add(uv3);
        Volume.UVs.Add(uv4);

        var v5 = v1;
        var v6 = v2;
        var v7 = v3;
        var v8 = v4;

        v5 = v5.NormalizeToRadius(Sphere.Radius);
        v6 = v6.NormalizeToRadius(Sphere.Radius);
        v7 = v7.NormalizeToRadius(Sphere.Radius);
        v8 = v8.NormalizeToRadius(Sphere.Radius);

        VolumeProjected = new PatchAABB();

        VolumeProjected.Vertices.Add(v5);
        VolumeProjected.Vertices.Add(v6);
        VolumeProjected.Vertices.Add(v7);
        VolumeProjected.Vertices.Add(v8);

        Normal = Up;

        Middle = (v1 + v2 + v3 + v4) / 4;
        MiddleProjected = Middle;
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius);
    }

    private void GenerateTerrain()
    {
        GameObject = new GameObject
        {
            name = "Patch_LOD_ " + SplitLevel + " : [" + Up + "]",
            layer = Sphere.gameObject.layer
        };

        Mesh = new Mesh();
        Data = new PatchData(PatchSettings.Vertices);

        var origin = Volume.Vertices[0];

        var vertStep = Size / (PatchSettings.VerticesPerSide - 1); //vertex spacing
        var startHMap = 1.0f;
        var endHMap = 1.0f - startHMap;

        var uCoord = startHMap;
        var vCoord = startHMap; //uv coordinates for the heightmap

        var uvStep = (endHMap - startHMap) / (PatchSettings.VerticesPerSide - 1); //hmap uv step size inside the loop

        var uVolCoord = Volume.UVs[0].x;
        var vVolCoord = Volume.UVs[0].y; //flat uv coordinates for the cube face

        var volCoordStep = (Volume.UVs[1].x - Volume.UVs[0].x) / (PatchSettings.VerticesPerSide - 1); //step size of flat uv inside the loop
        var idx = 0;

        for (ushort y = 0; y < PatchSettings.VerticesPerSide; y++)
        {
            var offset = origin;

            uCoord = startHMap;
            uVolCoord = Volume.UVs[0].x;

            for (ushort x = 0; x < PatchSettings.VerticesPerSide; x++)
            {
                //heightmap texture coordinates
                Data.UV1[idx] = new Vector2(uCoord, vCoord);
                uCoord += uvStep;

                //volume texture coordinates
                //x,y = flat volume uv coordinates
                //z = vertex slope
                Data.UV2[idx] = new Vector2(uVolCoord, vVolCoord);
                uVolCoord += volCoordStep;

                //calculate vertex position
                var vtx = offset;

                //use normalized vertex position as vertex normal
                vtx.Normalize();
                Data.Normals[idx] = vtx;

                //scale to sphere
                vtx = vtx * Sphere.Radius;

                //store
                Data.Vertices[idx] = vtx;
                Data.Volume[idx] = offset;

                idx++;
                offset += Right * vertStep;
            }

            origin -= Front * vertStep;
            vCoord += uvStep;
            vVolCoord += volCoordStep;
        }

        //update projected center
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius);

        //save original parent transformations
        var parentPosition = Sphere.gameObject.transform.position;
        var parentRotation = Sphere.gameObject.transform.rotation;

        //reset parent transformations before assigning data (so our vertices will be centered on the parent transform)
        Sphere.gameObject.transform.position = Vector3.zero;
        Sphere.gameObject.transform.rotation = Quaternion.identity;

        //put this node as a child of parent
        GameObject.transform.parent = Sphere.gameObject.transform;

        //assign data to this node's mesh
        Mesh.SetVertices(Data.Vertices.ToList());
        Mesh.SetUVs(0, Data.UV1.ToList());
        Mesh.SetUVs(1, Data.UV2.ToList());
        Mesh.SetNormals(Data.Normals.ToList());
        Mesh.SetTriangles(Sphere.PatchManager.Patches[Edges], 0);
        Mesh.hideFlags = HideFlags.DontSave;

        MeshFactory.SolveTangents(Mesh);

        Mesh.RecalculateBounds();

        //restore parent transformations
        Sphere.gameObject.transform.position = parentPosition;
        Sphere.gameObject.transform.rotation = parentRotation;

        NeedsTerrain = false;

        //discard parent's resources
        if (Parent != null)
        {
            Parent.DestroyNode();
        }

        var patch = GameObject.AddComponent<Patch>();
        var meshFilter = GameObject.AddComponent<MeshFilter>();

        GameObject.AddComponent<MeshRenderer>();
        GameObject.GetComponent<MeshRenderer>().sharedMaterial = MaterialHelper.CreateTemp(Sphere.Shader, "Patch");

        meshFilter.mesh = Mesh;
        patch.PatchTree = this;
    }

    private void GapFix(byte directionsMask)
    {
        short posHere = 0;
        short posThere = 0;

        short incHere = 1;
        short incThere = 1;

        //TBLR (top, right, bottom, left)
        //0000 == all edges at full-res
        //0001 == top edge at half-res
        //0010 == right edge at half-res
        //0100 == bottom edge at half-res
        //1000 == left edge at half-res

        var idxTopLeft = (short)0;
        var idxTopRight = (short)(PatchSettings.VerticesPerSide - 1);
        var idxBottomLeft = (short)((PatchSettings.VerticesPerSide - 1) * PatchSettings.VerticesPerSide);
        var idxBottomRight = (short)(idxBottomLeft + idxTopRight);

        for (byte direction = 0; direction < 4; direction++)
        {
            var bit = (byte)(1 << direction);

            if ((bit & directionsMask) > 0)
            {
                short add = 0;

                if (Neighbors[direction].Node.HasChildren)
                {
                }
                else
                {
                    switch ((NeighborDirection)direction)
                    {
                        case NeighborDirection.Top:
                            {
                                posHere = 0;
                                incHere = 1;

                                var parent = Neighbors[(int)NeighborDirection.Right].Node.Parent;

                                add =
                                    (short)
                                        (parent != null && Parent != null && parent.Equals(Parent) &&
                                         Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)NeighborDirection.Top].Node.Equals(Neighbors[direction].Node)
                                            ? 0
                                            : (PatchSettings.VerticesPerSide >> 1));

                                switch (Neighbors[direction].Direction)
                                {
                                    case NeighborDirection.Top:
                                        {
                                            posThere = idxTopRight;
                                            incThere = -1;
                                            break;
                                        }

                                    case NeighborDirection.Bottom:
                                        {
                                            posThere = idxBottomLeft;
                                            incThere = 1;
                                            break;
                                        }

                                    case NeighborDirection.Left:
                                        {
                                            posThere = idxTopLeft;
                                            incThere = (short)(PatchSettings.VerticesPerSide);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-PatchSettings.VerticesPerSide);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Right:
                            {
                                posHere = idxTopRight;
                                incHere = (short)(PatchSettings.VerticesPerSide);

                                var parent = Neighbors[(int)NeighborDirection.Bottom].Node.Parent;

                                add =
                                    (short)
                                        (parent != null && Parent != null && parent.Equals(Parent) &&
                                         Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)NeighborDirection.Right].Node.Equals(Neighbors[direction].Node)
                                            ? 0
                                            : (PatchSettings.VerticesPerSide >> 1));

                                switch (Neighbors[direction].Direction)
                                {
                                    case NeighborDirection.Top:
                                        {
                                            posThere = idxTopRight;
                                            incThere = -1;
                                            break;
                                        }

                                    case NeighborDirection.Bottom:
                                        {
                                            posThere = idxBottomLeft;
                                            incThere = 1;
                                            break;
                                        }

                                    case NeighborDirection.Left:
                                        {
                                            posThere = idxTopLeft;
                                            incThere = (short)(PatchSettings.VerticesPerSide);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-PatchSettings.VerticesPerSide);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Bottom:
                            {
                                posHere = idxBottomRight;
                                incHere = -1;

                                var parent = Neighbors[(int)NeighborDirection.Left].Node.Parent;

                                add =
                                    (short)
                                        (parent != null && Parent != null && parent.Equals(Parent) &&
                                         Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)NeighborDirection.Bottom].Node.Equals(Neighbors[direction].Node)
                                            ? 0
                                            : (PatchSettings.VerticesPerSide >> 1));

                                switch (Neighbors[direction].Direction)
                                {
                                    case NeighborDirection.Top:
                                        {
                                            posThere = idxTopRight;
                                            incThere = -1;
                                            break;
                                        }

                                    case NeighborDirection.Bottom:
                                        {
                                            posThere = idxBottomLeft;
                                            incThere = 1;
                                            break;
                                        }

                                    case NeighborDirection.Left:
                                        {
                                            posThere = idxTopLeft;
                                            incThere = (short)(PatchSettings.VerticesPerSide);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-PatchSettings.VerticesPerSide);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Left:
                            {
                                posHere = idxBottomLeft;
                                incHere = (short)(-PatchSettings.VerticesPerSide);

                                var parent = Neighbors[(int)NeighborDirection.Top].Node.Parent;

                                add =
                                    (short)
                                        (parent != null && Parent != null && parent.Equals(Parent) &&
                                         Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)NeighborDirection.Left].Node.Equals(Neighbors[direction].Node)
                                            ? 0
                                            : (PatchSettings.VerticesPerSide >> 1));

                                switch (Neighbors[direction].Direction)
                                {
                                    case NeighborDirection.Top:
                                        {
                                            posThere = idxTopRight;
                                            incThere = -1;
                                            break;
                                        }

                                    case NeighborDirection.Bottom:
                                        {
                                            posThere = idxBottomLeft;
                                            incThere = 1;
                                            break;
                                        }

                                    case NeighborDirection.Left:
                                        {
                                            posThere = idxTopLeft;
                                            incThere = (short)(PatchSettings.VerticesPerSide);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-PatchSettings.VerticesPerSide);
                                            break;
                                        }
                                }
                                break;
                            }
                    }

                    var loopLen = PatchSettings.VerticesPerSide;

                    //check for half-resolution neighbor
                    if ((Edges & bit) > 0)
                    {
                        //half resolution
                        incHere <<= 1;
                        loopLen >>= 1;

                        loopLen++;

                        posThere += (short)(add * incThere);
                    }

                    var fixedHere = false;

                    //fix the first edge vertex only
                    //if it's not already fixed by the edge at left (counter-clockwise) of current edge
                    if (!Neighbors[PREV_EDGE(direction)].isFixed)
                    {
                        Data.Vertices[posHere] = Neighbors[direction].Node.Data.Vertices[posThere];
                        fixedHere = true;
                    }
                    else
                    {
                        //instead, fix the vertex of the other node
                        Neighbors[direction].Node.Data.Vertices[posThere] = Data.Vertices[posHere];
                    }

                    posHere += incHere;
                    posThere += incThere;

                    ushort x = 0;

                    while (x < loopLen - 2)
                    {
                        Neighbors[direction].Node.Data.Vertices[posThere] = Data.Vertices[posHere];

                        x++;

                        posHere += incHere;
                        posThere += incThere;
                    }

                    //fix the last edge vertex only
                    //if it's not already fixed by the edge at right (clockwise) of current edge
                    if (!Neighbors[NEXT_EDGE(direction)].isFixed)
                    {
                        Data.Vertices[posHere] = Neighbors[direction].Node.Data.Vertices[posThere];
                        fixedHere = true;
                    }
                    else
                    {
                        //instead, fix the vertex of the other node
                        Neighbors[direction].Node.Data.Vertices[posThere] = Data.Vertices[posHere];
                    }

                    //reupload vertices to the mesh in this node and update its physics mesh
                    if (fixedHere)
                    {
                        Mesh.vertices = Data.Vertices;
                        Mesh.RecalculateBounds();
                    }

                    //reupload vertices to the neighbor mesh in the other node and update its physics mesh
                    Neighbors[direction].Node.Mesh.vertices = Neighbors[direction].Node.Data.Vertices;
                    Neighbors[direction].Node.Mesh.RecalculateBounds();

                    //fixed
                    Neighbors[direction].isFixed = true;

                    //the other node's edge is fixed as well.
                    Neighbors[direction].Node.Neighbors[(byte)Neighbors[direction].Direction].isFixed = true;
                }
            }
        }
    }

    public void RefreshLOD()
    {
        if (!HasChildren)
        {
            if (NeedsTerrain)
            {
                GenerateTerrain();
            }
        }
        else
        {
            for (byte i = 0; i < 4; i++)
            {
                Children[i].RefreshLOD();
            }
        }
    }

    public void RefreshGaps()
    {
        if (!HasChildren)
        {
            if (NeedsReedge)
            {
                ReEdge();
            }

            if (GapFixMask > 0)
            {
                GapFix(GapFixMask);
                GapFixMask = 0;
            }
        }
        else
        {
            for (byte i = 0; i < 4; i++)
            {
                Children[i].RefreshGaps();
            }
        }
    }

    public void Update(Vector3 entityPos)
    {
        if (!HasChildren)
        {
            //project entityPos to the flat plane
            var position = entityPos;

            var distanceToPlane = Plane.GetDistanceToPoint(position);
            var distanceToOrigin = Plane.distance;
            var distanceDelta = distanceToOrigin - distanceToPlane;

            position = position * (1.0f - (Math.Abs(distanceDelta) > 0.01f ? (-distanceToPlane / distanceDelta) : 0));

            //check distance to the center of the patch
            var patchDistance = VectorHelper.QuickDistance(Middle, position);

            if (SplitLevel > 1 && patchDistance < Sphere.MinCamDist)
            {
                Sphere.MinCamDist = patchDistance;
                Sphere.CloserNode = this;
            }

            if (SplitLevel > Sphere.HighestSplitLevel)
            {
                Sphere.HighestSplitLevel = SplitLevel;
            }

            if (Application.isPlaying)
            {
                if (!Sphere.Splitted && !Sphere.Rejoined)
                {
                    //check distance of this node's center to the camera
                    //coordinates are local, as if the planet were in origin
                    var cameraDistance = VectorHelper.QuickDistance(MiddleProjected, entityPos);

                    if (cameraDistance < Size * Size * Sphere.SizeSplit)
                    {
                        if (SplitLevel < Sphere.MaxSplitLevel)
                        {
                            Split();
                        }
                    }
                    else
                    {
                        if (Parent != null)
                        {
                            var parentDistance = VectorHelper.QuickDistance(Parent.MiddleProjected, entityPos);

                            if (parentDistance > Parent.Size * Parent.Size * Sphere.SizeRejoin)
                            {
                                if ((SplitLevel > Neighbors[(int)NeighborDirection.Top].Node.SplitLevel ||
                                     !Neighbors[(int)NeighborDirection.Top].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Top].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Right].Node.SplitLevel ||
                                     !Neighbors[(int)NeighborDirection.Right].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Right].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Bottom].Node.SplitLevel ||
                                     !Neighbors[(int)NeighborDirection.Bottom].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Bottom].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Left].Node.SplitLevel ||
                                     !Neighbors[(int)NeighborDirection.Left].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Left].Node.SplitLevel))
                                {
                                    Parent.NeedsRejoinCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            NeedsRejoinCount = 0;

            for (byte i = 0; i < 4; i++)
            {
                Children[i].Update(entityPos);
            }

            if (NeedsRejoinCount == 4)
            {
                //only rejoins if splitlevel is not minus than any neighbor
                ReJoin();
            }
        }
    }

    private void Split()
    {
        //force too coarse neighbors to split as well
        for (byte i = 0; i < 4; i++)
        {
            if (SplitLevel > Neighbors[i].Node.SplitLevel && !Neighbors[i].Node.HasChildren)
            {
                Neighbors[i].Node.Split();

                return;
            }
        }

        #region TOP LEFT

        var volume1 = new PatchAABB();

        volume1.Vertices.Add(Volume.Vertices[0]);
        volume1.Vertices.Add(Vector3.Lerp(Volume.Vertices[0], Volume.Vertices[1], 0.5f));
        volume1.Vertices.Add(Middle);
        volume1.Vertices.Add(Vector3.Lerp(Volume.Vertices[3], Volume.Vertices[0], 0.5f));

        var uv1a = Volume.UVs[0];
        var uv1b = Vector3.Lerp(Volume.UVs[0], Volume.UVs[1], 0.5f);
        var uv1d = Vector3.Lerp(Volume.UVs[3], Volume.UVs[0], 0.5f);
        var uv1c = new Vector3(uv1b.x, uv1d.y, 0);

        volume1.UVs.Add(uv1a);
        volume1.UVs.Add(uv1b);
        volume1.UVs.Add(uv1c);
        volume1.UVs.Add(uv1d);

        #endregion

        #region TOP RIGHT

        //second child - top right
        var volume2 = new PatchAABB();
        volume2.Vertices.Add(Vector3.Lerp(Volume.Vertices[0], Volume.Vertices[1], 0.5f));
        volume2.Vertices.Add(Volume.Vertices[1]);
        volume2.Vertices.Add(Vector3.Lerp(Volume.Vertices[1], Volume.Vertices[2], 0.5f));
        volume2.Vertices.Add(Middle);

        var uv2a = Vector3.Lerp(Volume.UVs[0], Volume.UVs[1], 0.5f);
        var uv2b = Volume.UVs[1];
        var uv2c = Vector3.Lerp(Volume.UVs[1], Volume.UVs[2], 0.5f);
        var uv2d = new Vector3(uv2a.x, uv2c.y, 0);

        volume2.UVs.Add(uv2a);
        volume2.UVs.Add(uv2b);
        volume2.UVs.Add(uv2c);
        volume2.UVs.Add(uv2d);

        #endregion

        #region BOTTOM RIGHT

        //third child - bottom right
        var volume3 = new PatchAABB();

        volume3.Vertices.Add(Middle);
        volume3.Vertices.Add(Vector3.Lerp(Volume.Vertices[1], Volume.Vertices[2], 0.5f));
        volume3.Vertices.Add(Volume.Vertices[2]);
        volume3.Vertices.Add(Vector3.Lerp(Volume.Vertices[3], Volume.Vertices[2], 0.5f));

        var uv3b = Vector3.Lerp(Volume.UVs[1], Volume.UVs[2], 0.5f);
        var uv3c = Volume.UVs[2];
        var uv3d = Vector3.Lerp(Volume.UVs[3], Volume.UVs[2], 0.5f);
        var uv3a = new Vector3(uv3d.x, uv3b.y, 0);

        volume3.UVs.Add(uv3a);
        volume3.UVs.Add(uv3b);
        volume3.UVs.Add(uv3c);
        volume3.UVs.Add(uv3d);

        #endregion

        #region BOTTOM LEFT

        //fourth child - bottom left
        var volume4 = new PatchAABB();

        volume4.Vertices.Add(Vector3.Lerp(Volume.Vertices[3], Volume.Vertices[0], 0.5f));
        volume4.Vertices.Add(Middle);
        volume4.Vertices.Add(Vector3.Lerp(Volume.Vertices[3], Volume.Vertices[2], 0.5f));
        volume4.Vertices.Add(Volume.Vertices[3]);

        var uv4a = Vector3.Lerp(Volume.UVs[3], Volume.UVs[0], 0.5f);
        var uv4c = Vector3.Lerp(Volume.UVs[3], Volume.UVs[2], 0.5f);
        var uv4d = Volume.UVs[3];
        var uv4b = new Vector3(uv4c.x, uv4a.y, 0);

        volume4.UVs.Add(uv4a);
        volume4.UVs.Add(uv4b);
        volume4.UVs.Add(uv4c);
        volume4.UVs.Add(uv4d);

        #endregion

        var q1 = new PatchTree(this, volume1);
        var q2 = new PatchTree(this, volume2);
        var q3 = new PatchTree(this, volume3);
        var q4 = new PatchTree(this, volume4);

        //set internal neighbors
        q1.SetNeighbor(NeighborDirection.Bottom, q4, NeighborDirection.Top);
        q1.SetNeighbor(NeighborDirection.Right, q2, NeighborDirection.Left);

        //set internal neighbors
        q2.SetNeighbor(NeighborDirection.Bottom, q3, NeighborDirection.Top);
        q2.SetNeighbor(NeighborDirection.Left, q1, NeighborDirection.Right);

        //set internal neighbors
        q3.SetNeighbor(NeighborDirection.Top, q2, NeighborDirection.Bottom);
        q3.SetNeighbor(NeighborDirection.Left, q4, NeighborDirection.Right);

        //set internal neighbors
        q4.SetNeighbor(NeighborDirection.Top, q1, NeighborDirection.Bottom);
        q4.SetNeighbor(NeighborDirection.Right, q3, NeighborDirection.Left);

        //store as children of the current node
        Children[0] = q1;
        Children[1] = q2;
        Children[2] = q3;
        Children[3] = q4;

        Sphere.Splitted = true;
        HasChildren = true;

        ReLink();
    }

    private void ReLinkChild(byte childIndex, NeighborDirection direction)
    {
        Children[childIndex].SetNeighbor(direction, Neighbors[(int)direction].Node, Neighbors[(int)direction].Direction);
    }

    private void ReLink()
    {
        ReLinkChild(0, NeighborDirection.Top);
        ReLinkChild(0, NeighborDirection.Left);

        ReLinkChild(1, NeighborDirection.Top);
        ReLinkChild(1, NeighborDirection.Right);

        ReLinkChild(2, NeighborDirection.Bottom);
        ReLinkChild(2, NeighborDirection.Right);

        ReLinkChild(3, NeighborDirection.Bottom);
        ReLinkChild(3, NeighborDirection.Left);
    }

    private void ReEdge()
    {
        //the flagged directions are of less resolution at that side
        //using binary to represent edge combination
        //TBLR (top, bottom, left, right)
        //0000 == all edges at full-res
        //0001 == top edge at half-res
        //0010 == right edge at half-res
        //0100 == bottom edge at half-res
        //1000 == left edge at half-res

        var EdgesTemp = Edges;

        Edges = 0;

        for (byte i = 0; i < 4; i++)
        {
            if (Neighbors[i].Node.SplitLevel < SplitLevel) Edges |= (byte)(1 << i);
        }

        if (Edges != EdgesTemp)
        {
            //reassign the index buffer
            Mesh.SetTriangles(Sphere.PatchManager.Patches[Edges], 0);
        }

        NeedsReedge = false;
    }

    public void SetNeighbor(NeighborDirection direction, PatchTree tree, NeighborDirection directionFromThere)
    {
        if (tree.HasChildren)
        {
            //the other node has children, which means this node was in coarse resolution,
            //so find correct child to link to...
            //need to find which two of the 4 children
            //of the other node that links to the parent of this node or to this node itself
            //then, decide which of the two children is closer to this node
            //and update the correct (nearest) child to link to this node

            PatchTree correctNode = null;

            float dist = 0;

            byte neighDirection = 0;

            //for each child of that node...
            for (byte i = 0; i < 4; i++)
            {
                var child = tree.Children[i];

                //for each direction of that child of that node...
                for (byte j = 0; j < 4; j++)
                {
                    //check if that child links from that direction to our parent
                    if (child.Neighbors[j].Node.Equals(Parent))
                    {
                        if (correctNode == null)
                        {
                            //as there is no best correct child yet,
                            //temporarily selects that child as the correct
                            correctNode = child;
                            neighDirection = j;
                            dist = VectorHelper.QuickDistance(child.Middle, Middle);
                            break;
                        }
                        else
                        {
                            //check if this child is closer than
                            //the currently selected as the closer child
                            if (VectorHelper.QuickDistance(child.Middle, Middle) < dist)
                            {
                                correctNode = child;
                                neighDirection = j;

                                //as we can have only two childs
                                //pointing to our own parent, and the other child has been scanned already,
                                //we can safely bail out of the outer loop and stop searching
                                i = 4;
                                break;
                            }
                        }
                    }
                    else if (child.Neighbors[j].Node == this)
                    {
                        //that child relinked to this node first
                        //which means both nodes are at same level
                        //so just get it and bail out
                        correctNode = child;
                        neighDirection = j;

                        //link back to that node
                        Neighbors[(int)direction].Node = correctNode;
                        Neighbors[(int)direction].Direction = (NeighborDirection)neighDirection;

                        //update edges of this node
                        NeedsReedge = true;

                        //bail out
                        return;
                    }
                }
            }

            if (correctNode != null)
            {
                //link to that node
                Neighbors[(int)direction].Node = correctNode;
                Neighbors[(int)direction].Direction = (NeighborDirection)neighDirection;

                //link that node back to this node
                correctNode.Neighbors[neighDirection].Node = this;
                correctNode.Neighbors[neighDirection].Direction = direction;

                //update edges and gaps
                NeedsReedge = true;
                correctNode.NeedsReedge = true;

                //the other node was discarding resolution
                //because this node was at coarse level
                //now that both are at same level,
                //lets force the other node to use full mesh at the edge that links to this node
                correctNode.GapFixMask |= (byte)(1 << neighDirection);
                correctNode.Neighbors[neighDirection].isFixed = false;
            }
        }
        else
        {
            //the other node has no children...
            //so, the other node is at a coarse level
            //or at same level (a brother node);
            //link directly to that node
            Neighbors[(int)direction].Node = tree;
            Neighbors[(int)direction].Direction = directionFromThere;
            Neighbors[(int)direction].Node.NeedsReedge = true;

            //only this node needs to update edges and fix gaps
            NeedsReedge = true;
            GapFixMask |= (byte)(1 << (int)direction);
            Neighbors[(int)direction].isFixed = false;

            //the other node stays linked to the node it is already linked to.
        }
    }

    private void ReJoinChild(byte childIndex, NeighborDirection direction)
    {
        Children[childIndex].Neighbors[(int)direction].Node.Neighbors[(int)Children[childIndex].Neighbors[(int)direction].Direction].Node = this;
        Children[childIndex].Neighbors[(int)direction].Node.Neighbors[(int)Children[childIndex].Neighbors[(int)direction].Direction].isFixed = false;
        Children[childIndex].Neighbors[(int)direction].Node.GapFixMask |= (byte)(1 << (int)Children[childIndex].Neighbors[(int)direction].Direction);
        Children[childIndex].Neighbors[(int)direction].Node.NeedsReedge = true;
    }

    private void ReJoin()
    {
        HasChildren = false;
        NeedsReedge = true;

        //relinks all children neighbors to point to this level
        //then delete children

        ReJoinChild(0, NeighborDirection.Top);
        ReJoinChild(0, NeighborDirection.Left);

        ReJoinChild(1, NeighborDirection.Top);
        ReJoinChild(1, NeighborDirection.Right);

        ReJoinChild(2, NeighborDirection.Bottom);
        ReJoinChild(2, NeighborDirection.Right);

        ReJoinChild(3, NeighborDirection.Bottom);
        ReJoinChild(3, NeighborDirection.Left);

        for (byte i = 0; i < 4; i++)
        {
            if (Sphere.CloserNode == Children[i]) Sphere.CloserNode = this;

            Children[i].DestroyTree();
            Children[i] = null;
        }

        if (Parent != null) Parent.ReLink();

        Sphere.Rejoined = true;
        NeedsTerrain = true;
    }
}