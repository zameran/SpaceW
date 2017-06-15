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
        /// The physical size of this quad divided by two (in local space).
        /// </summary>
        public double LengthHalf { get; private set; }

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
        [NonSerialized]
        public TerrainQuad[] Children = new TerrainQuad[4];

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
        public bool IsLeaf { get { return Children[0] == null || Children == null; } }

        public Matrix4x4d DeformedCorners { get; private set; }

        public Matrix4x4d DeformedVerticals { get; private set; }

        /// <summary>
        /// Tangent frame to world matrix.
        /// </summary>
        public Matrix3x3d TangentFrameToWorld { get; private set; }

        public Vector3d Center { get; private set; }

        public Vector4d Lengths { get; private set; }

        private void CalculateMatrices(double ox, double oy, double length, double r)
        {
            var p0 = new Vector3d(ox, oy, r);
            var p1 = new Vector3d(ox + length, oy, r);
            var p2 = new Vector3d(ox, oy + length, r);
            var p3 = new Vector3d(ox + length, oy + length, r);

            Center = (p0 + p3) * 0.5;

            double l0 = 0.0, l1 = 0.0, l2 = 0.0, l3 = 0.0;

            var v0 = p0.Normalized(ref l0);
            var v1 = p1.Normalized(ref l1);
            var v2 = p2.Normalized(ref l2);
            var v3 = p3.Normalized(ref l3);

            Lengths = new Vector4d(l0, l1, l2, l3);

            DeformedCorners = new Matrix4x4d(v0.x * r, v1.x * r, v2.x * r, v3.x * r,
                                             v0.y * r, v1.y * r, v2.y * r, v3.y * r,
                                             v0.z * r, v1.z * r, v2.z * r, v3.z * r,
                                             1.0, 1.0, 1.0, 1.0);

            DeformedVerticals = new Matrix4x4d(v0.x, v1.x, v2.x, v3.x,
                                               v0.y, v1.y, v2.y, v3.y,
                                               v0.z, v1.z, v2.z, v3.z,
                                               0.0, 0.0, 0.0, 0.0);

            var uz = Center.Normalized();
            var ux = new Vector3d(0.0, 1.0, 0.0).Cross(uz).Normalized();
            var uy = uz.Cross(ux);

            TangentFrameToWorld = Owner.TangentFrameToWorld * new Matrix3x3d(ux.x, uy.x, uz.x, ux.y, uy.y, uz.y, ux.z, uy.z, uz.z);
        }

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
            LengthHalf = length / 2.0;
            LocalBox = new Box3d(Ox, Ox + Length, Oy, Oy + Length, ZMin, ZMax);

            // TODO : Hm. Maybe too heavy for a ctor? Threading? Hueading?
            CalculateMatrices(ox, oy, length, owner.ParentBody.Size);
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
                Visibility = Owner.Deformation.GetVisibility(Owner, LocalBox);
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

            var ground = Owner.ParentBody.HeightZ;
            var distance = Owner.GetCameraDistance(new Box3d(Ox, Ox + Length, Oy, Oy + Length, Math.Min(0.0, ground), Math.Max(0.0, ground)));

            if ((Owner.SplitInvisibleQuads || Visibility != Frustum.VISIBILITY.INVISIBLE) && distance < Length * Owner.SplitDistance && Level < Owner.MaxLevel)
            {
                if (IsLeaf)
                {
                    Subdivide();
                }

                var order = CalculateOrder(Owner.LocalCameraPosition.x, Owner.LocalCameraPosition.y, Ox + LengthHalf, Oy + LengthHalf);

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

        public byte[] CalculateOrder(double cameraX, double cameraY, double quadX, double quadY)
        {
            var order = new byte[4];

            if (cameraY < quadY)
            {
                if (cameraX < quadX)
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
                if (cameraX < quadX)
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

            return order;
        }

        /// <summary>
        /// Creates the four subquads of this quad.
        /// </summary>
        private void Subdivide()
        {
            Children[0] = new TerrainQuad(Owner, this, 2 * Tx, 2 * Ty, Ox, Oy, LengthHalf, ZMin, ZMax);
            Children[1] = new TerrainQuad(Owner, this, 2 * Tx + 1, 2 * Ty, Ox + LengthHalf, Oy, LengthHalf, ZMin, ZMax);
            Children[2] = new TerrainQuad(Owner, this, 2 * Tx, 2 * Ty + 1, Ox, Oy + LengthHalf, LengthHalf, ZMin, ZMax);
            Children[3] = new TerrainQuad(Owner, this, 2 * Tx + 1, 2 * Ty + 1, Ox + LengthHalf, Oy + LengthHalf, LengthHalf, ZMin, ZMax);
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