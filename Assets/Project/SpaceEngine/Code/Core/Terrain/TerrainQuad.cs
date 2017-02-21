using System;

using UnityEngine;

namespace SpaceEngine.Core.Terrain
{
    /// <summary> 
    /// A quad in a terrain quadtree. The quadtree is subdivided based only on the current viewer position.  
    /// All quads are subdivided if they meet the subdivision criterion, even if they are outside the view frustum.  
    /// The quad visibility is stored in <see cref="Visibility"/>.  
    /// It can be used in <see cref="Tile.Samplers.TileSampler"/> to decide whether or not data must be produced for invisible tiles  
    /// (we recall that the terrain quadtree itself does not store any terrain data). 
    /// </summary>
    public class TerrainQuad
    {
        /// <summary> 
        /// The <see cref="TerrainNode"/> to which this terrain quadtree belongs. 
        /// </summary> 
        public TerrainNode Owner { get; private set; }

        /// <summary> 
        /// The parent quad of this quad. 
        /// </summary> 
        public TerrainQuad Parent { get; private set; }

        /// <summary> 
        /// The level of this quad in the quadtree (0 for the root). 
        /// </summary> 
        public int Level { get; private set; }

        /// <summary> 
        /// The quadtree x coordinate of this quad at <see cref="Level"/>. [0, 2 ^ <see cref="Level"/> - 1] 
        /// </summary> 
        public int Tx { get; private set; }

        /// <summary> 
        /// The quadtree y coordinate of this quad at <see cref="Level"/>. [0, 2 ^ <see cref="Level"/> - 1] 
        /// </summary> 
        public int Ty { get; private set; }

        /// <summary> 
        /// The physical x coordinate of the lower left corner of this quad (in local space). 
        /// </summary> 
        public double Ox { get; private set; }

        /// <summary> 
        /// The physical y coordinate of the lower left corner of this quad (in local space). 
        /// </summary> 
        public double Oy { get; private set; }

        /// <summary> 
        /// The physical size of this quad (in local space). 
        /// </summary> 
        public double Length { get; private set; }

        /// <summary> 
        /// Local bounding box. 
        /// </summary> 
        public Box3d LocalBox { get; private set; }

        /// <summary>
        /// Should the quad be drawn?
        /// </summary>
        public bool Drawable { get; set; }

        /// <summary>
        /// The minimum terrain elevation inside this quad. This field must be updated manually by users 
        /// (the <see cref="Tile.Samplers.TileSamplerZ"/> class can do this for you).
        /// </summary>
        public float ZMin { get; set; }

        /// <summary>
        /// The maximum terrain elevation inside this quad. This field must be updated manually by users 
        /// (the <see cref="Tile.Samplers.TileSamplerZ"/> class can do this for you).
        /// </summary>
        public float ZMax { get; set; }

        /// <summary>
        /// The four subquads of this quad. If this quad is not subdivided, the four values are NULL. 
        /// The subquads are stored in the following order: [BottomLeft, BottomRight, TopLeft, TopRight].
        /// </summary>
        TerrainQuad[] Children = new TerrainQuad[4];

        /// <summary>
        /// The visibility of the bounding box of this quad from the current viewer position. 
        /// The bounding box is computed using <see cref="ZMin"/> and <see cref="ZMax"/>, 
        /// which must therefore be up to date to get a correct culling of quads out of the view frustum. 
        /// This visibility only takes frustum culling into account.
        /// </summary>
        public Frustum.VISIBILITY Visibility { get; private set; }

        /// <summary> 
        /// The bounding box of this quad is occluded by the bounding boxes of the quads in front of it? 
        /// </summary> 
        public bool Occluded { get; private set; }

        /// <summary>
        /// This quad is visible?
        /// </summary>
        public bool IsVisible { get { return Visibility != Frustum.VISIBILITY.INVISIBLE; } }

        /// <summary>
        /// This quad is not subdivided?
        /// </summary>
        public bool IsLeaf { get { return Children[0] == null; } }

        /// <summary> 
        /// Creates a new <see cref="TerrainQuad"/> 
        /// </summary> 
        /// <param name="owner">The <see cref="TerrainNode"/> to which the terrain quadtree belongs.</param> 
        /// <param name="parent">The <see cref="TerrainQuad"/> parent of this quad.</param> 
        /// <param name="tx">The logical x coordinate of this quad.</param> 
        /// <param name="ty">The logical y coordinate of this quad.</param> 
        /// <param name="ox">The physical x coordinate of the lower left corner of this quad.</param> 
        /// <param name="oy">The physical y coordinate of the lower left corner of this quad.</param> 
        /// <param name="length">The physical size of this quad.</param> 
        /// <param name="zmin">The minimum terrain elevation inside this quad.</param> 
        /// <param name="zmax">The maximum terrain elevation inside this quad.</param> 
        public TerrainQuad(TerrainNode owner, TerrainQuad parent, int tx, int ty, double ox, double oy, double length, float zmin, float zmax)
        {
            Owner = owner;
            Parent = parent;
            Level = (Parent == null) ? 0 : Parent.Level + 1;
            Tx = tx;
            Ty = ty;
            Ox = ox;
            Oy = oy;
            ZMax = zmax;
            ZMin = zmin;
            Length = length;
            LocalBox = new Box3d(Ox, Ox + Length, Oy, Oy + Length, ZMin, ZMax);
        }

        /// <summary>
        /// The child with specified index.
        /// </summary>
        /// <param name="i">Index of the child.</param>
        /// <returns>Returns the child with specified index.</returns>
        public TerrainQuad GetChild(int i)
        {
            return Children[i];
        }

        /// <summary>
        /// The number of quads in the tree below this quad.
        /// </summary>
        /// <returns>Returns the number of quads in the tree below this quad.</returns>
        public int GetSize()
        {
            var s = 1;

            if (IsLeaf)
            {
                return s;
            }
            else
            {
                return s + Children[0].GetSize() + Children[1].GetSize() + Children[2].GetSize() + Children[3].GetSize();
            }
        }

        /// <summary>
        /// The depth of the tree below this quad.
        /// </summary>
        /// <returns>Returns the depth of the tree below this quad.</returns>
        public int GetDepth()
        {
            if (IsLeaf)
            {
                return Level;
            }
            else
            {
                return Mathf.Max(Mathf.Max(Children[0].GetDepth(), Children[1].GetDepth()), Mathf.Max(Children[2].GetDepth(), Children[3].GetDepth()));
            }
        }

        private void Destroy()
        {
            for (byte i = 0; i < 4; i++)
            {
                if (Children[i] != null)
                {
                    Children[i].Destroy();
                    Children[i] = null;
                }
            }
        }

        /// <summary>
        /// Subdivides or unsubdivides this quad based on the current viewer distance to this quad, relatively to its size. 
        /// This method uses the current viewer position provided by the <see cref="TerrainNode"/> to which this <see cref="QuadTree"/> belongs.
        /// </summary>
        public void UpdateLOD()
        {
            var visibility = (Parent == null) ? Frustum.VISIBILITY.PARTIALLY : Parent.Visibility;

            if (visibility == Frustum.VISIBILITY.PARTIALLY)
            {
                Visibility = Owner.GetVisibility(LocalBox);
            }
            else
            {
                Visibility = visibility;
            }

            // Here we reuse the occlusion test from the previous frame: if the quad was found unoccluded in the previous frame, we suppose it is
            // still unoccluded at this frame. If it was found occluded, we perform an occlusion test to check if it is still occluded.
            if (Visibility != Frustum.VISIBILITY.INVISIBLE && Occluded)
            {
                Occluded = Owner.IsOccluded(LocalBox);

                if (Occluded)
                {
                    Visibility = Frustum.VISIBILITY.INVISIBLE;
                }
            }

            var ground = Owner.Body.HeightZ;
            var distance = Owner.GetCameraDist(new Box3d(Ox, Ox + Length, Oy, Oy + Length, Math.Min(0.0, ground), Math.Max(0.0, ground)));

            if ((Owner.SplitInvisibleQuads || Visibility != Frustum.VISIBILITY.INVISIBLE) && distance < Length * Owner.SplitDistance && Level < Owner.MaxLevel)
            {
                if (IsLeaf)
                {
                    Subdivide();
                }

                var order = new int[4];
                var ox = Owner.LocalCameraPosition.x;
                var oy = Owner.LocalCameraPosition.y;
                var cx = Ox + Length / 2.0;
                var cy = Oy + Length / 2.0;

                if (oy < cy)
                {
                    if (ox < cx)
                    {
                        order[0] = 0;
                        order[1] = 1;
                        order[2] = 2;
                        order[3] = 3;
                    }
                    else
                    {
                        order[0] = 1;
                        order[1] = 0;
                        order[2] = 3;
                        order[3] = 2;
                    }
                }
                else
                {
                    if (ox < cx)
                    {
                        order[0] = 2;
                        order[1] = 0;
                        order[2] = 3;
                        order[3] = 1;
                    }
                    else
                    {
                        order[0] = 3;
                        order[1] = 1;
                        order[2] = 2;
                        order[3] = 0;
                    }
                }

                Children[order[0]].UpdateLOD();
                Children[order[1]].UpdateLOD();
                Children[order[2]].UpdateLOD();
                Children[order[3]].UpdateLOD();

                // We compute a more precise occlusion for the next frame (see above), by combining the occlusion status of the child nodes.
                Occluded = (Children[0].Occluded && Children[1].Occluded && Children[2].Occluded && Children[3].Occluded);
            }
            else
            {
                if (Visibility != Frustum.VISIBILITY.INVISIBLE)
                {
                    // We add the bounding box of this quad to the occluders list.
                    Occluded = Owner.AddOccluder(LocalBox);

                    if (Occluded)
                    {
                        Visibility = Frustum.VISIBILITY.INVISIBLE;
                    }
                }

                if (!IsLeaf)
                {
                    Destroy();
                }
            }
        }

        /// <summary>
        /// Creates the four subquads of this quad.
        /// </summary>
        private void Subdivide()
        {
            var hl = (float)Length / 2.0f;

            Children[0] = new TerrainQuad(Owner, this, 2 * Tx, 2 * Ty, Ox, Oy, hl, ZMin, ZMax);
            Children[1] = new TerrainQuad(Owner, this, 2 * Tx + 1, 2 * Ty, Ox + hl, Oy, hl, ZMin, ZMax);
            Children[2] = new TerrainQuad(Owner, this, 2 * Tx, 2 * Ty + 1, Ox, Oy + hl, hl, ZMin, ZMax);
            Children[3] = new TerrainQuad(Owner, this, 2 * Tx + 1, 2 * Ty + 1, Ox + hl, Oy + hl, hl, ZMin, ZMax);
        }

        public void DrawQuadOutline(Camera camera, Material lineMaterial, Color lineColor)
        {
            if (IsLeaf)
            {
                int[,] ORDER = new int[,] { { 1, 0 }, { 2, 3 }, { 0, 2 }, { 3, 1 } };

                if (Visibility == Frustum.VISIBILITY.INVISIBLE) return;

                var verts = new Vector3[8];

                verts[0] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox, Oy, ZMin)).ToVector3();
                verts[1] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox + Length, Oy, ZMin)).ToVector3();
                verts[2] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox, Oy + Length, ZMin)).ToVector3();
                verts[3] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox + Length, Oy + Length, ZMin)).ToVector3();

                verts[4] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox, Oy, ZMax)).ToVector3();
                verts[5] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox + Length, Oy, ZMax)).ToVector3();
                verts[6] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox, Oy + Length, ZMax)).ToVector3();
                verts[7] = Owner.Deformation.LocalToDeformed(new Vector3d(Ox + Length, Oy + Length, ZMax)).ToVector3();

                GL.PushMatrix();

                GL.LoadIdentity();
                GL.MultMatrix(camera.worldToCameraMatrix * Owner.LocalToWorld.ToMatrix4x4());
                GL.LoadProjectionMatrix(camera.projectionMatrix);

                lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(lineColor);

                for (byte i = 0; i < 4; i++)
                {
                    // Draw bottom quad
                    GL.Vertex3(verts[ORDER[i, 0]].x, verts[ORDER[i, 0]].y, verts[ORDER[i, 0]].z);
                    GL.Vertex3(verts[ORDER[i, 1]].x, verts[ORDER[i, 1]].y, verts[ORDER[i, 1]].z);

                    // Draw top quad
                    GL.Vertex3(verts[ORDER[i, 0] + 4].x, verts[ORDER[i, 0] + 4].y, verts[ORDER[i, 0] + 4].z);
                    GL.Vertex3(verts[ORDER[i, 1] + 4].x, verts[ORDER[i, 1] + 4].y, verts[ORDER[i, 1] + 4].z);

                    // Draw verticals
                    GL.Vertex3(verts[ORDER[i, 0]].x, verts[ORDER[i, 0]].y, verts[ORDER[i, 0]].z);
                    GL.Vertex3(verts[ORDER[i, 0] + 4].x, verts[ORDER[i, 0] + 4].y, verts[ORDER[i, 0] + 4].z);
                }

                GL.End();
                GL.PopMatrix();
            }

            if (!IsLeaf)
            {
                Children[0].DrawQuadOutline(camera, lineMaterial, lineColor);
                Children[1].DrawQuadOutline(camera, lineMaterial, lineColor);
                Children[2].DrawQuadOutline(camera, lineMaterial, lineColor);
                Children[3].DrawQuadOutline(camera, lineMaterial, lineColor);
            }
        }
    }
}