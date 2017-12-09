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
// Creation Date: 2017.05.31
// Creation Time: 12:52 AM
// Creator: zameran
#endregion

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Octree
{
    public class PointOctreeNode<T> where T : class
    {
        /// <summary>
        /// Center of this node.
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        /// Length of the sides of this node.
        /// </summary>
        public float SideLength { get; private set; }

        /// <summary>
        /// Minimum size for a node in this octree.
        /// </summary>
        private float MinSize;

        /// <summary>
        /// Bounding box that represents this node.
        /// </summary>
        private Bounds Bounds = default(Bounds);

        /// <summary>
        /// Objects in this node.
        /// </summary>
        private readonly List<OctreeObject> Objects = new List<OctreeObject>();

        /// <summary>
        /// Child nodes.
        /// </summary>
        private PointOctreeNode<T>[] Children = null;

        /// <summary>
        /// Bounds of potential children to this node.
        /// </summary>
        private Bounds[] ChildBounds;

        /// <summary>
        /// Split limit, count to handle until split.
        /// </summary>
        private const int NUM_OBJECTS_ALLOWED = 8;

        /// <summary>
        /// For reverting the bounds size after temporary changes.
        /// </summary>
        private Vector3 ActualBoundsSize;

        /// <summary>
        /// This node is not splitted?
        /// </summary>
        public bool IsLeaf { get { return Children == null; } }

        /// <summary>
        /// An object in the octree.
        /// </summary>
        protected class OctreeObject
        {
            public T Object;
            public Vector3 Position;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
        /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
        /// <param name="centerVal">Centre position of this node.</param>
        public PointOctreeNode(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SetValues(baseLengthVal, minSizeVal, centerVal);
        }

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="position">Position of the object.</param>
        /// <returns></returns>
        public bool Add(T obj, Vector3 position)
        {
            if (!Encapsulates(Bounds, position)) { return false; }

            SubAdd(obj, position);

            return true;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            var removed = false;

            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i].Object.Equals(obj))
                {
                    removed = Objects.Remove(Objects[i]);

                    break;
                }
            }

            if (!removed && !IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    removed = Children[i].Remove(obj);

                    if (removed) break;
                }
            }

            if (removed && !IsLeaf)
            {
                // Check if we should merge nodes now that we've removed an item...
                if (ShouldMerge())
                {
                    Merge();
                }
            }

            return removed;
        }

        /// <summary>
        /// Return objects that are within maxDistance of the specified ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <param name="result">List result.</param>
        /// <returns>Objects within range.</returns>
        public void GetNearby(ref Ray ray, ref float maxDistance, ref List<T> result)
        {
            // Does the ray hit this node at all?
            // NOTE : Expanding the bounds is not exactly the same as a real distance check, but it's fast...
            Bounds.Expand(new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));

            var intersected = Bounds.IntersectRay(ray);

            Bounds.size = ActualBoundsSize;

            if (!intersected) return;

            // Check against any objects in this node...
            for (int i = 0; i < Objects.Count; i++)
            {
                if (DistanceToRay(ray, Objects[i].Position) <= maxDistance)
                {
                    result.Add(Objects[i].Object);
                }
            }

            // Check children...
            if (!IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    Children[i].GetNearby(ref ray, ref maxDistance, ref result);
                }
            }
        }

        /// <summary>
        /// Return objects that are within maxDistance of the specified position.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <param name="result">List result.</param>
        public void GetNearby(ref Vector3 position, ref float maxDistance, ref List<T> result)
        {
            // Check against any objects in this node...
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Vector3.Distance(position, Objects[i].Position) <= maxDistance)
                {
                    result.Add(Objects[i].Object);
                }
            }

            // Check children...
            if (!IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    Children[i].GetNearby(ref position, ref maxDistance, ref result);
                }
            }
        }

        /// <summary>
        /// Return nodes that are within maxDistance of the specified position.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <param name="result">List result.</param>
        public void GetNearbyNodes(ref Vector3 position, ref float maxDistance, ref List<PointOctreeNode<T>> result)
        {
            if (Vector3.Distance(position, Center) <= maxDistance)
            {
                result.Add(this);
            }

            // Check children...
            if (!IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    Children[i].GetNearbyNodes(ref position, ref maxDistance, ref result);
                }
            }
        }

        /// <summary>
        /// Return node containing objects.
        /// </summary>
        /// <returns>Containing objects.</returns>
        public IEnumerable<T> GetNodeObjects()
        {
            return Objects.Select(octreeObject => octreeObject.Object);
        }

        /// <summary>
        /// Set the 8 children of this octree.
        /// </summary>
        /// <param name="childOctrees">The 8 new child nodes.</param>
        public void SetChildren(ref PointOctreeNode<T>[] childOctrees)
        {
            if (childOctrees.Length != 8)
            {
                Debug.LogError(string.Format("PointOctreeNode: Child octree array must be length 8. Was length: {0}", childOctrees.Length));

                return;
            }

            Children = childOctrees;
        }

        /// <summary>
        /// Draws node boundaries visually for debugging.
        /// </summary>
        /// <param name="depth">Used for recurcive calls to this method.</param>
        public void DrawAllBounds(float depth = 0)
        {
            var tintVal = depth / 7;

            Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal);

            var thisBounds = new Bounds(Center, Vector3.one * SideLength);

            Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

            if (!IsLeaf)
            {
                depth++;

                for (byte i = 0; i < 8; i++)
                {
                    Children[i].DrawAllBounds(depth);
                }
            }

            Gizmos.color = Color.white;
        }

        /// <summary>
        /// Draws the bounds of all objects in the tree visually for debugging.
        /// </summary>
        public void DrawAllObjects()
        {
            var tintVal = SideLength / 20;

            Gizmos.color = new Color(0, 1.0f - tintVal, tintVal, 0.25f);

            foreach (var obj in Objects)
            {
                Gizmos.DrawCube(obj.Position, Vector3.one * tintVal);
            }

            if (!IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    Children[i].DrawAllObjects();
                }
            }

            Gizmos.color = Color.white;
        }

        public void DrawNodeOutline(Camera camera, Material lineMaterial, int[][] order = null)
        {
            if (order == null) return;

            if (IsLeaf)
            {
                var verts = new Vector3[8];
                var min = Bounds.min;
                var max = Bounds.max;

                verts[0] = min;
                verts[1] = max;
                verts[2] = new Vector3(min.x, min.y, max.z);
                verts[3] = new Vector3(min.x, max.y, min.z);
                verts[4] = new Vector3(max.x, min.y, min.z);
                verts[5] = new Vector3(min.x, max.y, max.z);
                verts[6] = new Vector3(max.x, min.y, max.z);
                verts[7] = new Vector3(max.x, max.y, min.z);

                GL.PushMatrix();

                GL.LoadIdentity();
                GL.MultMatrix(camera.worldToCameraMatrix);
                GL.LoadProjectionMatrix(camera.projectionMatrix);

                lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(Color.blue);

                for (byte i = 0; i < 3; i++)
                {
                    GL.Vertex(verts[order[i][0]]);
                    GL.Vertex(verts[order[i][1]]);

                    GL.Vertex(verts[order[i][2]]);
                    GL.Vertex(verts[order[i][3]]);

                    GL.Vertex(verts[order[i][4]]);
                    GL.Vertex(verts[order[i][5]]);

                    GL.Vertex(verts[order[i][6]]);
                    GL.Vertex(verts[order[i][7]]);
                }

                GL.End();
                GL.PopMatrix();
            }
            else
            {
                for (byte i = 0; i < 8; i++)
                {
                    Children[i].DrawNodeOutline(camera, lineMaterial, order);
                }
            }
        }

        /// <summary>
        /// We can shrink the octree if:
        /// - This node is >= double <see cref="minLength"/> in length...
        /// - All objects in the root node are within one octant...
        /// - This node doesn't have children, or does but 7/8 children are empty...
        /// We can also shrink it? if there are no objects left at all.
        /// </summary>
        /// <param name="minLength">Minimum dimensions of a node in this octree.</param>
        /// <returns>The new root, or the existing one if we didn't shrink.</returns>
        public PointOctreeNode<T> ShrinkIfPossible(float minLength)
        {
            if (SideLength < (2 * minLength)) { return this; }
            if (Objects.Count == 0 && Children.Length == 0) { return this; }

            // Check objects in root...
            int bestFit = -1;

            for (int i = 0; i < Objects.Count; i++)
            {
                var obj = Objects[i];
                var newBestFit = BestFitChild(obj.Position);

                if (i == 0 || newBestFit == bestFit)
                {
                    if (bestFit < 0)
                    {
                        bestFit = newBestFit;
                    }
                }
                else
                {
                    return this; // Can't reduce - objects fit in different octants...
                }
            }

            // Check objects in children if there are any...
            if (!IsLeaf)
            {
                var childHadContent = false;

                for (int i = 0; i < Children.Length; i++)
                {
                    if (Children[i].HasAnyObjects())
                    {
                        if (childHadContent)
                        {
                            return this; // Can't shrink - another child had content already...
                        }

                        if (bestFit >= 0 && bestFit != i)
                        {
                            return this; // Can't reduce - objects in root are in a different octant to objects in child...
                        }

                        childHadContent = true;
                        bestFit = i;
                    }
                }
            }
            else // Can reduce...
            {
                // We don't have any children, so just shrink this node to the new size. We already know that everything will still fit in it...
                SetValues(SideLength / 2, MinSize, ChildBounds[bestFit].center);

                return this;
            }

            // We have children. Use the appropriate child as the new root node...
            return Children[bestFit];
        }

        public int NodesCount()
        {
            var totalCount = 1;

            if (IsLeaf)
            {
                return totalCount;
            }
            else
            {
                return totalCount + Children.Sum(child => child.NodesCount());
            }
        }

        /// <summary>
        /// Set values for this node. 
        /// </summary>
        /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
        /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
        /// <param name="centerVal">Centre position of this node.</param>
        private void SetValues(float baseLengthVal, float minSizeVal, Vector3 centerVal)
        {
            SideLength = baseLengthVal;
            MinSize = minSizeVal;
            Center = centerVal;

            ActualBoundsSize = new Vector3(SideLength, SideLength, SideLength);
            Bounds = new Bounds(Center, ActualBoundsSize);

            var quarter = SideLength / 4.0f;
            var childActualLength = SideLength / 2.0f;
            var childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);

            ChildBounds = new Bounds[8];
            ChildBounds[0] = new Bounds(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
            ChildBounds[1] = new Bounds(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
            ChildBounds[2] = new Bounds(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
            ChildBounds[3] = new Bounds(Center + new Vector3(quarter, quarter, quarter), childActualSize);
            ChildBounds[4] = new Bounds(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
            ChildBounds[5] = new Bounds(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
            ChildBounds[6] = new Bounds(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
            ChildBounds[7] = new Bounds(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
        }

        /// <summary>
        /// Recursive counterpart for <see cref="Add"/> method.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        private void SubAdd(T obj, Vector3 objPos)
        {
            // We know it fits at this level if we've got this far. Just add if few objects are here, or children would be below min size.
            if (Objects.Count < NUM_OBJECTS_ALLOWED || (SideLength / 2) < MinSize)
            {
                Objects.Add(new OctreeObject { Object = obj, Position = objPos });
            }
            else
            {
                // Enough objects in this node already... Create the 8 children...
                int bestFitChild;

                if (Children == null)
                {
                    Split();

                    if (Children == null)
                    {
                        Debug.Log("PointOctreeNode: Child creation failed for an unknown reason! Early exit...");

                        return;
                    }

                    // Now that we have the new children, see if this node's existing objects would fit there
                    for (int i = Objects.Count - 1; i >= 0; i--)
                    {
                        var existingObj = Objects[i];

                        // Find which child the object is closest to based on where the object's center is located in relation to the octree's center.
                        bestFitChild = BestFitChild(existingObj.Position);

                        Children[bestFitChild].SubAdd(existingObj.Object, existingObj.Position); // Go a level deeper...			
                        Objects.Remove(existingObj); // Remove from here...
                    }
                }

                // Now handle the new object we're adding now...
                bestFitChild = BestFitChild(objPos);

                Children[bestFitChild].SubAdd(obj, objPos);
            }
        }

        /// <summary>
        /// Splits the octree into eight children.
        /// </summary>
        private void Split()
        {
            var quarter = SideLength / 4.0f;
            var newLength = SideLength / 2.0f;

            Children = new PointOctreeNode<T>[8];
            Children[0] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(-quarter, quarter, -quarter));
            Children[1] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(quarter, quarter, -quarter));
            Children[2] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(-quarter, quarter, quarter));
            Children[3] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(quarter, quarter, quarter));
            Children[4] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(-quarter, -quarter, -quarter));
            Children[5] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(quarter, -quarter, -quarter));
            Children[6] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(-quarter, -quarter, quarter));
            Children[7] = new PointOctreeNode<T>(newLength, MinSize, Center + new Vector3(quarter, -quarter, quarter));
        }

        /// <summary>
        /// Merge all children into this node - the opposite of Split.
        /// We only have to check one level down since a merge will never happen if the children already have children,
        /// since that won't happen unless there are already too many objects to merge.
        /// </summary>
        private void Merge()
        {
            // NOTE : We know children != null or we wouldn't be merging...
            for (byte i = 0; i < 8; i++)
            {
                var currentChild = Children[i];
                var objectsCount = currentChild.Objects.Count;

                for (int j = objectsCount - 1; j >= 0; j--)
                {
                    var currentObj = currentChild.Objects[j];

                    Objects.Add(currentObj);
                }
            }

            // Remove the child nodes (and the objects in them - they've been added elsewhere now)...
            Children = null;
        }

        /// <summary>
        /// Checks if outerBounds encapsulates the given point.
        /// </summary>
        /// <param name="outerBounds">Outer bounds.</param>
        /// <param name="point">Point.</param>
        /// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
        private static bool Encapsulates(Bounds outerBounds, Vector3 point)
        {
            return outerBounds.Contains(point);
        }

        /// <summary>
        /// Find which child node this object would be most likely to fit in.
        /// </summary>
        /// <param name="objPos">The object's position.</param>
        /// <returns>One of the eight child octants.</returns>
        private int BestFitChild(Vector3 objPos)
        {
            return (objPos.x <= Center.x ? 0 : 1) + (objPos.y >= Center.y ? 0 : 4) + (objPos.z <= Center.z ? 0 : 2);
        }

        /// <summary>
        /// Checks if there are few enough objects in this node and its children that the children should all be merged into this.
        /// </summary>
        /// <returns>True there are less or the same abount of objects in this and its children than numObjectsAllowed.</returns>
        private bool ShouldMerge()
        {
            var totalObjects = Objects.Count;

            if (!IsLeaf)
            {
                foreach (var child in Children)
                {
                    if (!child.IsLeaf)
                    {
                        // If any of the *children* have children, there are definitely too many to merge, or the child woudl have been merged already...
                        return false;
                    }

                    totalObjects += child.Objects.Count;
                }
            }

            return totalObjects <= NUM_OBJECTS_ALLOWED;
        }

        private bool HasAnyObjects()
        {
            if (Objects.Count > 0) return true;

            if (!IsLeaf)
            {
                for (byte i = 0; i < 8; i++)
                {
                    if (Children[i].HasAnyObjects()) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Distance from the point to the closest point of the ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="point">The point to check distance from the ray.</param>
        /// <returns>Returns the closest distance to the given ray from a point.</returns>
        public static float DistanceToRay(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }
    }
}