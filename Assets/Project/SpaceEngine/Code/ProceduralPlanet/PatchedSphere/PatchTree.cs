using UnityEngine;

using System.Collections.Generic;

public class PatchTree
{
    PatchSphere Sphere;

    Vector3[] vertices;
    Vector2[] uv;
    Vector3[] normals;

    Vector3[] vols;
    Vector2[] uvvols;

    ushort SplitLevel;

    bool HasChildren;
    bool NeedsTerrain;
    bool NeedsReedge;

    float Size;

    Vector3 Up, Front, Right;
    Vector3 FrontProjected, UpProjected;

    public PatchAABB Volume;
    public PatchAABB VolumeProjected;

    Plane Plane;

    Vector3 Middle;
    Vector3 MiddleProjected;

    Vector3 PatchNormal;

    GameObject GameObject;
    Mesh Mesh;
    MeshCollider Collider = null;

    public static List<PatchColliderQueue> ColliderQueueList = new List<PatchColliderQueue>(32);

    PatchTree Parent;
    PatchTree[] Children = new PatchTree[4];
    PatchNeighbor[] Neighbors = new PatchNeighbor[4];

    byte NeedsRejoinCount;
    byte Edges;
    byte GapFixMask;

    static float lastCollider = Time.time;

    public byte NEXT_EDGE(byte e) { return (byte)(e == 3 ? 0 : e + 1); }
    public byte PREV_EDGE(byte e) { return (byte)(e == 0 ? 3 : e - 1); }

    public static void NewColliderStart()
    {
        const int MaxProcess = 1;
        int c = (MaxProcess > ColliderQueueList.Count ? ColliderQueueList.Count : MaxProcess);

        if (c > 0)
        {
            float t = Time.time;
            if (t - lastCollider < 0.01f) return;
            lastCollider = t;
        }

        for (int i = 0; i < c; i++)
        {
            if (ColliderQueueList[i].Update)
            {
                if (ColliderQueueList[i].Tree.Collider != null)
                {
                    // update mesh collider
                    ColliderQueueList[i].Tree.Collider.sharedMesh = null;
                    ColliderQueueList[i].Tree.Collider.sharedMesh = ColliderQueueList[i].Tree.Mesh;
                }
            }
            else
            {
                if (ColliderQueueList[i].Tree.GameObject != null)
                {
                    // add mesh collider to the patch
                    ColliderQueueList[i].Tree.Collider = (MeshCollider)ColliderQueueList[i].Tree.GameObject.AddComponent<MeshCollider>();
                }
            }
        }

        ColliderQueueList.RemoveRange(0, c);
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

        Plane = new Plane(Volume.vertices[0], Volume.vertices[2], Volume.vertices[1]);
    }

    public PatchTree(PatchTree Parent, PatchAABB Volume)
    {
        this.Parent = Parent;
        this.Volume = Volume;
        this.Sphere = Parent.Sphere;

        SplitLevel = (ushort)(this.Parent.SplitLevel + 1);

        if (SplitLevel > Sphere.HighestSplitLevel) Sphere.HighestSplitLevel = SplitLevel;

        Size = this.Parent.Size / 2;

        Vector3 v1 = this.Volume.vertices[0];
        Vector3 v2 = this.Volume.vertices[1];
        Vector3 v3 = this.Volume.vertices[2];
        Vector3 v4 = this.Volume.vertices[3];

        Vector2 uv1 = this.Volume.uvs[0];
        Vector2 uv2 = this.Volume.uvs[1];
        Vector2 uv3 = this.Volume.uvs[2];
        Vector2 uv4 = this.Volume.uvs[3];

        VolumeProjected = new PatchAABB();

        VolumeProjected.vertices.Add(v1.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.vertices.Add(v2.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.vertices.Add(v3.NormalizeToRadius(Sphere.Radius));
        VolumeProjected.vertices.Add(v4.NormalizeToRadius(Sphere.Radius));

        VolumeProjected.uvs.Add(uv1);
        VolumeProjected.uvs.Add(uv2);
        VolumeProjected.uvs.Add(uv3);
        VolumeProjected.uvs.Add(uv4);

        Neighbors[0].Node = Neighbors[1].Node = Neighbors[2].Node = Neighbors[3].Node = null;
        Neighbors[0].isFixed = Neighbors[1].isFixed = Neighbors[2].isFixed = Neighbors[3].isFixed = false;
        Children[0] = Children[1] = Children[2] = Children[3] = null;

        NeedsRejoinCount = 0;
        HasChildren = false;
        NeedsReedge = true;
        NeedsTerrain = true;
        GapFixMask = 15;

        PatchNormal = this.Parent.PatchNormal;

        Middle = (Volume.vertices[0] + Volume.vertices[1] + Volume.vertices[2] + Volume.vertices[3]) / 4;
        MiddleProjected = Middle;
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius);

        UpProjected = MiddleProjected;
        UpProjected.Normalize();

        FrontProjected = Vector3.Lerp(VolumeProjected.vertices[0], VolumeProjected.vertices[1], 0.5f) - MiddleProjected;
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
        Vector3 left = -Right;

        Vector3 v1 = (left * Sphere.Radius) + (Front * Sphere.Radius) + (Up * Sphere.Radius);       //left far
        Vector3 v2 = (left * -Sphere.Radius) + (Front * Sphere.Radius) + (Up * Sphere.Radius);      //right far
        Vector3 v3 = (left * -Sphere.Radius) + (Front * -Sphere.Radius) + (Up * Sphere.Radius);     //right near
        Vector3 v4 = (left * Sphere.Radius) + (Front * -Sphere.Radius) + (Up * Sphere.Radius);      //left near

        Vector3 uv1 = new Vector3(0, 0, 0);
        Vector3 uv2 = new Vector3(1, 0, 0);
        Vector3 uv3 = new Vector3(1, 1, 0);
        Vector3 uv4 = new Vector3(0, 1, 0);

        Volume = new PatchAABB();
        Volume.vertices.Add(v1); Volume.uvs.Add(uv1);
        Volume.vertices.Add(v2); Volume.uvs.Add(uv2);
        Volume.vertices.Add(v3); Volume.uvs.Add(uv3);
        Volume.vertices.Add(v4); Volume.uvs.Add(uv4);

        Vector3 v5 = v1;
        Vector3 v6 = v2;
        Vector3 v7 = v3;
        Vector3 v8 = v4;

        v5 = v5.NormalizeToRadius(Sphere.Radius);
        v6 = v6.NormalizeToRadius(Sphere.Radius);
        v7 = v7.NormalizeToRadius(Sphere.Radius);
        v8 = v8.NormalizeToRadius(Sphere.Radius);

        VolumeProjected = new PatchAABB();
        VolumeProjected.vertices.Add(v5);
        VolumeProjected.vertices.Add(v6);
        VolumeProjected.vertices.Add(v7);
        VolumeProjected.vertices.Add(v8);

        PatchNormal = Up;

        Middle = (v1 + v2 + v3 + v4) / 4;
        MiddleProjected = Middle;
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius);
    }

    private void GenerateTerrain()
    {
        GameObject = new GameObject();
        GameObject.name = "Patch_LOD_ " + SplitLevel + " : [" + Up + "]";
        GameObject.layer = Sphere.gameObject.layer;
        GameObject.AddComponent<MeshFilter>();
        GameObject.AddComponent<MeshRenderer>();
        GameObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));

        Mesh = new Mesh();

        vertices = new Vector3[Sphere.PatchConfig.GridSize];
        uv = new Vector2[Sphere.PatchConfig.GridSize];
        vols = new Vector3[Sphere.PatchConfig.GridSize];
        uvvols = new Vector2[Sphere.PatchConfig.GridSize];
        normals = new Vector3[Sphere.PatchConfig.GridSize];

        Vector3 origin = Volume.vertices[0];

        float vertStep = Size / (Sphere.PatchConfig.PatchSize - 1);                                         //vertex spacing

        float startHMap = 1.0f / Sphere.PatchConfig.LevelHeightMapRes(SplitLevel);
        float endHMap = 1.0f - startHMap;
        float uCoord = startHMap, vCoord = startHMap;                                                       //uv coordinates for the heightmap
        float uvStep = (endHMap - startHMap) / (Sphere.PatchConfig.PatchSize - 1);                          //hmap uv step size inside the loop

        float uVolCoord, vVolCoord = Volume.uvs[0].y;                                                       //flat uv coordinates for the cube face
        float volCoordStep = (Volume.uvs[1].x - Volume.uvs[0].x) / (Sphere.PatchConfig.PatchSize - 1);      //step size of flat uv inside the loop

        float maxHeight = -999999999.0f;

        int idx = 0;

        for (ushort y = 0; y < Sphere.PatchConfig.PatchSize; y++)
        {
            Vector3 offset = origin;

            uCoord = startHMap;
            uVolCoord = Volume.uvs[0].x;

            for (ushort x = 0; x < Sphere.PatchConfig.PatchSize; x++)
            {
                //get sampled height from the low res packed heightmap
                float height = 1;

                height = height * 1;
                if (height > maxHeight) maxHeight = height;

                //heightmap texture coordinates
                uv[idx] = new Vector2(uCoord, vCoord);
                uCoord += uvStep;

                //volume texture coordinates
                //x,y = flat volume uv coordinates
                //z = vertex slope
                uvvols[idx] = new Vector2(uVolCoord, vVolCoord);
                uVolCoord += volCoordStep;

                //calculate vertex position
                Vector3 vtx = offset;

                //use normalized vertex position as vertex normal
                vtx.Normalize();
                normals[idx] = vtx;

                //scale to sphere
                vtx = vtx * (Sphere.Radius + height);

                //store
                vertices[idx] = vtx;
                vols[idx] = offset;

                idx++;
                offset += Right * vertStep;
            }

            origin -= Front * vertStep;
            vCoord += uvStep;
            vVolCoord += volCoordStep;
        }

        //update projected center
        MiddleProjected = MiddleProjected.NormalizeToRadius(Sphere.Radius + maxHeight);

        //save original parent transformations
        Vector3 parentPos = Sphere.gameObject.transform.position;
        Quaternion parentQua = Sphere.gameObject.transform.rotation;

        //reset parent transformations before assigning mesh data (so our vertices will be centered on the parent transform)
        Sphere.gameObject.transform.position = Vector3.zero;
        Sphere.gameObject.transform.rotation = Quaternion.identity;

        //put this node as a child of parent
        GameObject.transform.parent = Sphere.gameObject.transform;

        //assign data to this node's mesh
        Mesh.vertices = vertices;
        Mesh.uv = uv;                   //vertex uv coordinates
        Mesh.uv2 = uvvols;              //passing flat patch volume uv coordinates as second texcoords
        Mesh.normals = normals;
        Mesh.triangles = Sphere.PatchManager.Patches[Edges];
        Mesh.hideFlags = HideFlags.DontSave;

        MeshFactory.SolveTangents(Mesh);

        Mesh.RecalculateBounds();
        GameObject.GetComponent<MeshFilter>().mesh = Mesh;

        //restore parent transformations
        Sphere.gameObject.transform.position = parentPos;
        Sphere.gameObject.transform.rotation = parentQua;

        //GameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
        GameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        if (SplitLevel >= Sphere.MaxSplitLevel)
        {
            //add mesh collider to the patch through the lazy creator
            ColliderQueueList.Add(new PatchColliderQueue() { Tree = this, Update = false });
        }

        NeedsTerrain = false;

        //discard parent's resources
        if (Parent != null)
        {
            Parent.DestroyNode();
        }

        Patch patch = GameObject.AddComponent<Patch>();

        patch.Mesh = Mesh;
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

        short idxTopLeft = 0;
        short idxTopRight = (short)(Sphere.PatchConfig.PatchSize - 1);
        short idxBottomLeft = (short)((Sphere.PatchConfig.PatchSize - 1) * Sphere.PatchConfig.PatchSize);
        short idxBottomRight = (short)(idxBottomLeft + idxTopRight);

        for (byte direction = 0; direction < 4; direction++)
        {
            byte bit = (byte)(1 << direction);
            if ((bit & directionsMask) > 0)
            {
                short add = 0;

                if (Neighbors[direction].Node.HasChildren) { }
                else
                {
                    switch ((NeighborDirection)direction)
                    {
                        case NeighborDirection.Top:
                            {
                                posHere = 0;
                                incHere = 1;

                                PatchTree np = Neighbors[(int)NeighborDirection.Right].Node.Parent;

                                add = (short)(np != null && Parent != null && np.Equals(Parent) && Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)NeighborDirection.Top].Node.Equals(Neighbors[direction].Node) ? 0 : (Sphere.PatchConfig.PatchSize >> 1));

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
                                            incThere = (short)(Sphere.PatchConfig.PatchSize);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-Sphere.PatchConfig.PatchSize);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Right:
                            {
                                posHere = idxTopRight;
                                incHere = (short)(Sphere.PatchConfig.PatchSize);

                                PatchTree np = Neighbors[(int)NeighborDirection.Bottom].Node.Parent;

                                add = (short)(np != null && Parent != null && np.Equals(Parent) && Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)NeighborDirection.Right].Node.Equals(Neighbors[direction].Node) ? 0 : (Sphere.PatchConfig.PatchSize >> 1));

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
                                            incThere = (short)(Sphere.PatchConfig.PatchSize);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-Sphere.PatchConfig.PatchSize);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Bottom:
                            {
                                posHere = idxBottomRight;
                                incHere = -1;

                                PatchTree np = Neighbors[(int)NeighborDirection.Left].Node.Parent;

                                add = (short)(np != null && Parent != null && np.Equals(Parent) && Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)NeighborDirection.Bottom].Node.Equals(Neighbors[direction].Node) ? 0 : (Sphere.PatchConfig.PatchSize >> 1));

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
                                            incThere = (short)(Sphere.PatchConfig.PatchSize);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-Sphere.PatchConfig.PatchSize);
                                            break;
                                        }
                                }
                                break;
                            }

                        case NeighborDirection.Left:
                            {
                                posHere = idxBottomLeft;
                                incHere = (short)(-Sphere.PatchConfig.PatchSize);

                                PatchTree np = Neighbors[(int)NeighborDirection.Top].Node.Parent;

                                add = (short)(np != null && Parent != null && np.Equals(Parent) && Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)NeighborDirection.Left].Node.Equals(Neighbors[direction].Node) ? 0 : (Sphere.PatchConfig.PatchSize >> 1));

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
                                            incThere = (short)(Sphere.PatchConfig.PatchSize);
                                            break;
                                        }

                                    case NeighborDirection.Right:
                                        {
                                            posThere = idxBottomRight;
                                            incThere = (short)(-Sphere.PatchConfig.PatchSize);
                                            break;
                                        }
                                }
                                break;
                            }
                    }

                    ushort loopLen = Sphere.PatchConfig.PatchSize;

                    //check for half-resolution neighbor
                    if ((Edges & bit) > 0)
                    {
                        //half resolution
                        incHere <<= 1;
                        loopLen >>= 1;

                        loopLen++;

                        posThere += (short)(add * incThere);
                    }

                    bool fixedHere = false;

                    //fix the first edge vertex only
                    //if it's not already fixed by the edge at left (counter-clockwise) of current edge
                    if (!Neighbors[PREV_EDGE(direction)].isFixed)
                    {
                        vertices[posHere] = Neighbors[direction].Node.vertices[posThere];
                        fixedHere = true;
                    }
                    else
                    {
                        //instead, fix the vertex of the other node
                        Neighbors[direction].Node.vertices[posThere] = vertices[posHere];
                    }

                    posHere += incHere;
                    posThere += incThere;

                    ushort x = 0;

                    while (x < loopLen - 2)
                    {
                        Neighbors[direction].Node.vertices[posThere] = vertices[posHere];

                        x++;

                        posHere += incHere;
                        posThere += incThere;
                    }

                    //fix the last edge vertex only
                    //if it's not already fixed by the edge at right (clockwise) of current edge
                    if (!Neighbors[NEXT_EDGE(direction)].isFixed)
                    {
                        vertices[posHere] = Neighbors[direction].Node.vertices[posThere];
                        fixedHere = true;
                    }
                    else
                    {
                        //instead, fix the vertex of the other node
                        Neighbors[direction].Node.vertices[posThere] = vertices[posHere];
                    }

                    //reupload vertices to the mesh in this node and update its physics mesh
                    if (fixedHere)
                    {
                        Mesh.vertices = vertices;
                        Mesh.RecalculateBounds();

                        if (Collider != null)
                        {
                            ColliderQueueList.Add(new PatchColliderQueue() { Tree = this, Update = true });
                        }
                    }

                    //reupload vertices to the neighbor mesh in the other node and update its physics mesh
                    Neighbors[direction].Node.Mesh.vertices = Neighbors[direction].Node.vertices;
                    Neighbors[direction].Node.Mesh.RecalculateBounds();

                    if (Neighbors[direction].Node.Collider != null)
                    {
                        ColliderQueueList.Add(new PatchColliderQueue() { Tree = Neighbors[direction].Node, Update = true });
                    }

                    //fixed
                    Neighbors[direction].isFixed = true;

                    //the other node's edge is fixed as well.
                    Neighbors[direction].Node.Neighbors[(byte)Neighbors[direction].Direction].isFixed = true;
                }
            }
        }
    }

    public void RefreshTerrain(Vector3 entityPos, Vector3 invCamPos)
    {
        if (!HasChildren)
        {
            if (NeedsTerrain)
            {
                GenerateTerrain();
            }

            NewColliderStart();
        }
        else
        {
            for (byte i = 0; i < 4; i++)
            {
                Children[i].RefreshTerrain(entityPos, invCamPos);
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
            Vector3 pos = entityPos;

            float t;
            float d1 = Plane.GetDistanceToPoint(pos);
            float d2 = Plane.distance;
            float dd = d2 - d1;

            if (dd != 0)
                t = -d1 / dd;
            else
                t = 0;

            pos = pos * (1.0f - t);

            //check distance to the center of the patch
            float patchDist = VectorHelper.QuickDistance(Middle, pos);

            if (SplitLevel > 1 && patchDist < Sphere.MinCamDist)
            {
                Sphere.MinCamDist = patchDist;
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
                    float LastCamDist = VectorHelper.QuickDistance(MiddleProjected, entityPos);

                    if (LastCamDist < Size * Size * Sphere.SizeSplit)
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
                            float lastParentDist = VectorHelper.QuickDistance(Parent.MiddleProjected, entityPos);

                            if (lastParentDist > Parent.Size * Parent.Size * Sphere.SizeRejoin)
                            {
                                if ((SplitLevel > Neighbors[(int)NeighborDirection.Top].Node.SplitLevel || !Neighbors[(int)NeighborDirection.Top].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Top].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Right].Node.SplitLevel || !Neighbors[(int)NeighborDirection.Right].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Right].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Bottom].Node.SplitLevel || !Neighbors[(int)NeighborDirection.Bottom].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Bottom].Node.SplitLevel) &&
                                    (SplitLevel > Neighbors[(int)NeighborDirection.Left].Node.SplitLevel || !Neighbors[(int)NeighborDirection.Left].Node.HasChildren && SplitLevel == Neighbors[(int)NeighborDirection.Left].Node.SplitLevel))
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
                Neighbors[i].Node.Split(); return;
            }
        }

        //first child - top left
        PatchAABB vol1 = new PatchAABB();
        Vector3 v1a = Volume.vertices[0];
        Vector3 v1b = Vector3.Lerp(Volume.vertices[0], Volume.vertices[1], 0.5f);
        Vector3 v1c = Middle;
        Vector3 v1d = Vector3.Lerp(Volume.vertices[3], Volume.vertices[0], 0.5f);
        vol1.vertices.Add(v1a);
        vol1.vertices.Add(v1b);
        vol1.vertices.Add(v1c);
        vol1.vertices.Add(v1d);
        Vector3 uv1a = Volume.uvs[0];
        Vector3 uv1b = Vector3.Lerp(Volume.uvs[0], Volume.uvs[1], 0.5f);
        Vector3 uv1d = Vector3.Lerp(Volume.uvs[3], Volume.uvs[0], 0.5f);
        Vector3 uv1c = new Vector3(uv1b.x, uv1d.y, 0);
        vol1.uvs.Add(uv1a);
        vol1.uvs.Add(uv1b);
        vol1.uvs.Add(uv1c);
        vol1.uvs.Add(uv1d);
        PatchTree q1 = new PatchTree(this, vol1);

        //second child - top right
        PatchAABB vol2 = new PatchAABB();
        Vector3 v2a = Vector3.Lerp(Volume.vertices[0], Volume.vertices[1], 0.5f);
        Vector3 v2b = Volume.vertices[1];
        Vector3 v2c = Vector3.Lerp(Volume.vertices[1], Volume.vertices[2], 0.5f);
        Vector3 v2d = Middle;
        vol2.vertices.Add(v2a);
        vol2.vertices.Add(v2b);
        vol2.vertices.Add(v2c);
        vol2.vertices.Add(v2d);
        Vector3 uv2a = Vector3.Lerp(Volume.uvs[0], Volume.uvs[1], 0.5f);
        Vector3 uv2b = Volume.uvs[1];
        Vector3 uv2c = Vector3.Lerp(Volume.uvs[1], Volume.uvs[2], 0.5f);
        Vector3 uv2d = new Vector3(uv2a.x, uv2c.y, 0);
        vol2.uvs.Add(uv2a);
        vol2.uvs.Add(uv2b);
        vol2.uvs.Add(uv2c);
        vol2.uvs.Add(uv2d);
        PatchTree q2 = new PatchTree(this, vol2);

        //third child - bottom right
        PatchAABB vol3 = new PatchAABB();
        Vector3 v3a = Middle;
        Vector3 v3b = Vector3.Lerp(Volume.vertices[1], Volume.vertices[2], 0.5f);
        Vector3 v3c = Volume.vertices[2];
        Vector3 v3d = Vector3.Lerp(Volume.vertices[3], Volume.vertices[2], 0.5f);
        vol3.vertices.Add(v3a);
        vol3.vertices.Add(v3b);
        vol3.vertices.Add(v3c);
        vol3.vertices.Add(v3d);
        Vector3 uv3b = Vector3.Lerp(Volume.uvs[1], Volume.uvs[2], 0.5f);
        Vector3 uv3c = Volume.uvs[2];
        Vector3 uv3d = Vector3.Lerp(Volume.uvs[3], Volume.uvs[2], 0.5f);
        Vector3 uv3a = new Vector3(uv3d.x, uv3b.y, 0);
        vol3.uvs.Add(uv3a);
        vol3.uvs.Add(uv3b);
        vol3.uvs.Add(uv3c);
        vol3.uvs.Add(uv3d);
        PatchTree q3 = new PatchTree(this, vol3);

        //fourth child - bottom left
        PatchAABB vol4 = new PatchAABB();
        Vector3 v4a = Vector3.Lerp(Volume.vertices[3], Volume.vertices[0], 0.5f);
        Vector3 v4b = Middle;
        Vector3 v4c = Vector3.Lerp(Volume.vertices[3], Volume.vertices[2], 0.5f);
        Vector3 v4d = Volume.vertices[3];
        vol4.vertices.Add(v4a);
        vol4.vertices.Add(v4b);
        vol4.vertices.Add(v4c);
        vol4.vertices.Add(v4d);
        Vector3 uv4a = Vector3.Lerp(Volume.uvs[3], Volume.uvs[0], 0.5f);
        Vector3 uv4c = Vector3.Lerp(Volume.uvs[3], Volume.uvs[2], 0.5f);
        Vector3 uv4d = Volume.uvs[3];
        Vector3 uv4b = new Vector3(uv4c.x, uv4a.y, 0);
        vol4.uvs.Add(uv4a);
        vol4.uvs.Add(uv4b);
        vol4.uvs.Add(uv4c);
        vol4.uvs.Add(uv4d);
        PatchTree q4 = new PatchTree(this, vol4);

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

    private void ReLink()
    {
        Children[0].SetNeighbor(NeighborDirection.Top, Neighbors[(int)NeighborDirection.Top].Node, Neighbors[(int)NeighborDirection.Top].Direction);
        Children[0].SetNeighbor(NeighborDirection.Left, Neighbors[(int)NeighborDirection.Left].Node, Neighbors[(int)NeighborDirection.Left].Direction);

        Children[1].SetNeighbor(NeighborDirection.Top, Neighbors[(int)NeighborDirection.Top].Node, Neighbors[(int)NeighborDirection.Top].Direction);
        Children[1].SetNeighbor(NeighborDirection.Right, Neighbors[(int)NeighborDirection.Right].Node, Neighbors[(int)NeighborDirection.Right].Direction);

        Children[2].SetNeighbor(NeighborDirection.Bottom, Neighbors[(int)NeighborDirection.Bottom].Node, Neighbors[(int)NeighborDirection.Bottom].Direction);
        Children[2].SetNeighbor(NeighborDirection.Right, Neighbors[(int)NeighborDirection.Right].Node, Neighbors[(int)NeighborDirection.Right].Direction);

        Children[3].SetNeighbor(NeighborDirection.Bottom, Neighbors[(int)NeighborDirection.Bottom].Node, Neighbors[(int)NeighborDirection.Bottom].Direction);
        Children[3].SetNeighbor(NeighborDirection.Left, Neighbors[(int)NeighborDirection.Left].Node, Neighbors[(int)NeighborDirection.Left].Direction);
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

        byte EdgesTemp = Edges;

        Edges = 0;

        for (byte i = 0; i < 4; i++)
        {
            if (Neighbors[i].Node.SplitLevel < SplitLevel) Edges |= (byte)(1 << i);
        }

        if (Edges != EdgesTemp)
        {
            //reassign the index buffer
            Mesh.triangles = Sphere.PatchManager.Patches[Edges];
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

            PatchTree pCorrectNode = null;

            float dist = 0;

            byte neighDirection = 0;

            //for each child of that node...
            for (byte i = 0; i < 4; i++)
            {
                PatchTree pChild = tree.Children[i];

                //for each direction of that child of that node...
                for (byte j = 0; j < 4; j++)
                {
                    //check if that child links from that direction to our parent
                    if (pChild.Neighbors[j].Node.Equals(Parent))
                    {
                        if (pCorrectNode == null)
                        {
                            //as there is no best correct child yet,
                            //temporarily selects that child as the correct
                            pCorrectNode = pChild;
                            neighDirection = j;
                            dist = VectorHelper.QuickDistance(pChild.Middle, Middle);
                            break;
                        }
                        else
                        {
                            //check if this child is closer than
                            //the currently selected as the closer child
                            if (VectorHelper.QuickDistance(pChild.Middle, Middle) < dist)
                            {
                                pCorrectNode = pChild;
                                neighDirection = j;

                                //as we can have only two childs
                                //pointing to our own parent, and the other child has been scanned already,
                                //we can safely bail out of the outer loop and stop searching
                                i = 4;
                                break;
                            }
                        }
                    }
                    else if (pChild.Neighbors[j].Node == this)
                    {
                        //that child relinked to this node first
                        //which means both nodes are at same level
                        //so just get it and bail out
                        pCorrectNode = pChild;
                        neighDirection = j;

                        //link back to that node
                        Neighbors[(int)direction].Node = pCorrectNode;
                        Neighbors[(int)direction].Direction = (NeighborDirection)neighDirection;

                        //update edges of this node
                        NeedsReedge = true;

                        //bail out
                        return;
                    }
                }
            }

            if (pCorrectNode != null)
            {
                //link to that node
                Neighbors[(int)direction].Node = pCorrectNode;
                Neighbors[(int)direction].Direction = (NeighborDirection)neighDirection;

                //link that node back to this node
                pCorrectNode.Neighbors[neighDirection].Node = this;
                pCorrectNode.Neighbors[neighDirection].Direction = direction;

                //update edges and gaps
                NeedsReedge = true;
                pCorrectNode.NeedsReedge = true;

                //the other node was discarding resolution
                //because this node was at coarse level
                //now that both are at same level,
                //lets force the other node to use full mesh at the edge that links to this node
                pCorrectNode.GapFixMask |= (byte)(1 << neighDirection);
                pCorrectNode.Neighbors[neighDirection].isFixed = false;
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

    public PatchNeighbor GetNeighbor(NeighborDirection direction)
    {
        return Neighbors[(int)direction];
    }

    private void ReJoin()
    {
        HasChildren = false;
        NeedsReedge = true;

        //relinks all children neighbors to point to this level
        //then delete children

        Children[0].Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)Children[0].Neighbors[(int)NeighborDirection.Top].Direction].Node = this;
        Children[0].Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)Children[0].Neighbors[(int)NeighborDirection.Top].Direction].isFixed = false;
        Children[0].Neighbors[(int)NeighborDirection.Top].Node.GapFixMask |= (byte)(1 << (int)Children[0].Neighbors[(int)NeighborDirection.Top].Direction);
        Children[0].Neighbors[(int)NeighborDirection.Top].Node.NeedsReedge = true;

        Children[0].Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)Children[0].Neighbors[(int)NeighborDirection.Left].Direction].Node = this;
        Children[0].Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)Children[0].Neighbors[(int)NeighborDirection.Left].Direction].isFixed = false;
        Children[0].Neighbors[(int)NeighborDirection.Left].Node.GapFixMask |= (byte)(1 << (int)Children[0].Neighbors[(int)NeighborDirection.Left].Direction);
        Children[0].Neighbors[(int)NeighborDirection.Left].Node.NeedsReedge = true;

        //

        Children[1].Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)Children[1].Neighbors[(int)NeighborDirection.Top].Direction].Node = this;
        Children[1].Neighbors[(int)NeighborDirection.Top].Node.Neighbors[(int)Children[1].Neighbors[(int)NeighborDirection.Top].Direction].isFixed = false;
        Children[1].Neighbors[(int)NeighborDirection.Top].Node.GapFixMask |= (byte)(1 << (int)Children[1].Neighbors[(int)NeighborDirection.Top].Direction);
        Children[1].Neighbors[(int)NeighborDirection.Top].Node.NeedsReedge = true;

        Children[1].Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)Children[1].Neighbors[(int)NeighborDirection.Right].Direction].Node = this;
        Children[1].Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)Children[1].Neighbors[(int)NeighborDirection.Right].Direction].isFixed = false;
        Children[1].Neighbors[(int)NeighborDirection.Right].Node.GapFixMask |= (byte)(1 << (int)Children[1].Neighbors[(int)NeighborDirection.Right].Direction);
        Children[1].Neighbors[(int)NeighborDirection.Right].Node.NeedsReedge = true;

        //

        Children[2].Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)Children[2].Neighbors[(int)NeighborDirection.Bottom].Direction].Node = this;
        Children[2].Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)Children[2].Neighbors[(int)NeighborDirection.Bottom].Direction].isFixed = false;
        Children[2].Neighbors[(int)NeighborDirection.Bottom].Node.GapFixMask |= (byte)(1 << (int)Children[2].Neighbors[(int)NeighborDirection.Bottom].Direction);
        Children[2].Neighbors[(int)NeighborDirection.Bottom].Node.NeedsReedge = true;

        Children[2].Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)Children[2].Neighbors[(int)NeighborDirection.Right].Direction].Node = this;
        Children[2].Neighbors[(int)NeighborDirection.Right].Node.Neighbors[(int)Children[2].Neighbors[(int)NeighborDirection.Right].Direction].isFixed = false;
        Children[2].Neighbors[(int)NeighborDirection.Right].Node.GapFixMask |= (byte)(1 << (int)Children[2].Neighbors[(int)NeighborDirection.Right].Direction);
        Children[2].Neighbors[(int)NeighborDirection.Right].Node.NeedsReedge = true;

        //

        Children[3].Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)Children[3].Neighbors[(int)NeighborDirection.Bottom].Direction].Node = this;
        Children[3].Neighbors[(int)NeighborDirection.Bottom].Node.Neighbors[(int)Children[3].Neighbors[(int)NeighborDirection.Bottom].Direction].isFixed = false;
        Children[3].Neighbors[(int)NeighborDirection.Bottom].Node.GapFixMask |= (byte)(1 << (int)Children[3].Neighbors[(int)NeighborDirection.Bottom].Direction);
        Children[3].Neighbors[(int)NeighborDirection.Bottom].Node.NeedsReedge = true;

        Children[3].Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)Children[3].Neighbors[(int)NeighborDirection.Left].Direction].Node = this;
        Children[3].Neighbors[(int)NeighborDirection.Left].Node.Neighbors[(int)Children[3].Neighbors[(int)NeighborDirection.Left].Direction].isFixed = false;
        Children[3].Neighbors[(int)NeighborDirection.Left].Node.GapFixMask |= (byte)(1 << (int)Children[3].Neighbors[(int)NeighborDirection.Left].Direction);
        Children[3].Neighbors[(int)NeighborDirection.Left].Node.NeedsReedge = true;

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

    public float Collided(Vector3 entityWorldPos, float collisionRadius, ref Vector3 collisionPoint)
    {
        int indexCount = Sphere.PatchManager.Patches[Edges].Length;

        int[] indexList = Sphere.PatchManager.Patches[Edges];
        int[] idx = new int[3];

        Plane triplane = new Plane();
        Plane p1 = new Plane();
        Plane p2 = new Plane();
        Plane p3 = new Plane();

        //inverse transform entity
        Vector3 pos = Sphere.transform.InverseTransformPoint(entityWorldPos);

        float minDist = float.MaxValue;

        for (int f = 0; f < indexCount; f += 3)
        {
            //face indexes
            idx[0] = indexList[f + 0];
            idx[1] = indexList[f + 1];
            idx[2] = indexList[f + 2];

            //determine triangle plane
            triplane.Set3Points(vertices[idx[0]], vertices[idx[1]], vertices[idx[2]]);

            //make three planes, one for each edge of this triangle

            //edge 1
            Vector3 e1 = Vector3.Cross(triplane.normal, vertices[idx[1]] - vertices[idx[0]]);
            e1.Normalize();

            //calculate edge center
            Vector3 me1 = Vector3.Lerp(vertices[idx[0]], vertices[idx[1]], 0.5f);
            p1.SetNormalAndPosition(e1, me1);

            //edge 2
            Vector3 e2 = Vector3.Cross(triplane.normal, vertices[idx[2]] - vertices[idx[1]]);
            e2.Normalize();

            //calculate edge center
            Vector3 me2 = Vector3.Lerp(vertices[idx[1]], vertices[idx[2]], 0.5f);
            p2.SetNormalAndPosition(e2, me2);

            //edge 3
            Vector3 e3 = Vector3.Cross(triplane.normal, vertices[idx[0]] - vertices[idx[2]]);
            e3.Normalize();

            //calculate edge center
            Vector3 me3 = Vector3.Lerp(vertices[idx[2]], vertices[idx[0]], 0.5f);
            p3.SetNormalAndPosition(e3, me3);

            //check if entity is inside the three planes
            float d1 = p1.GetDistanceToPoint(pos);
            float d2 = p2.GetDistanceToPoint(pos);
            float d3 = p3.GetDistanceToPoint(pos);

            if (d1 < 0 || d2 < 0 || d3 < 0)
            {
                //not inside, skip further testing
                continue;
            }

            //finally, check collision against the selected triangle
            //and do collision response
            float dst = triplane.GetDistanceToPoint(pos);

            if (dst < minDist) minDist = dst;

            if (dst < collisionRadius)
            {
                float adj = (collisionRadius - dst);

                //rotate normal contrary to the planet's orientation,
                //because entity position was inverse transformed here.
                Vector3 vadj = Sphere.transform.InverseTransformDirection(triplane.normal);

                collisionPoint = vadj * adj;

                return dst;
            }
        }

        collisionPoint = Vector3.zero;

        return minDist;
    }

    public static void RenderQuadVolume(int width, int height, Material material, PatchAABB volume)
    {
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Viewport(new Rect(0, 0, width, height));

        material.SetPass(0);

        Vector3 v1 = volume.vertices[0];
        Vector3 uv1 = volume.uvs[0];

        Vector3 v2 = volume.vertices[3];
        Vector3 uv2 = volume.uvs[3];

        Vector3 v3 = volume.vertices[2];
        Vector3 uv3 = volume.uvs[2];

        Vector3 v4 = volume.vertices[1];
        Vector3 uv4 = volume.uvs[1];

        GL.Begin(GL.QUADS);

        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.OSXEditor)
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
}