using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Shapes;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Terrain.Deformation;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Samplers;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

using Functions = SpaceEngine.Core.Numerics.Functions;

namespace SpaceEngine.Core.Terrain
{
    /// <summary>
    /// Class, that holds all <see cref="TerrainNode"/>'s <see cref="TileSampler"/>s, sorted by priority.
    /// The size of order is limited to 255.
    /// </summary>
    public sealed class TileSamplerOrder
    {
        public List<TileSampler> OrderList;

        public TileSamplerOrder(IEnumerable<TileSampler> queue)
        {
            OrderList = new List<TileSampler>(255);
            OrderList.AddRange(queue);
            OrderList.Sort(new TileSampler.Sort());
        }
    }

    /// <summary>
    /// Provides a base class to draw and update view-dependent, quadtree based terrains.
    /// This base class provides classes to represent the terrain quadtree, classes to
    /// associate data produced by a <see cref="Tile.Producer.TileProducer"/> to the quads of this
    /// quadtree, as well as classes to update and draw such terrains (which can be deformed to get spherical).
    /// A view dependent, quadtree based terrain. This class provides access to the
    /// terrain quadtree, defines the terrain deformation (can be used to get planet
    /// sized terrains), and defines how the terrain quadtree must be subdivided based
    /// on the viewer position. This class does not give any direct or indirect access to the terrain data (elevations, normals, texture). 
    /// The terrain data must be managed by <see cref="Tile.Producer.TileProducer"/>, and stored in TileStorage. 
    /// The link between with the terrain quadtree is provided by the TileSampler class.
    /// </summary>
    public class TerrainNode : NodeSlave<TerrainNode>, IUniformed<Material>
    {
        public class Sort : IComparer<TerrainNode>
        {
            int IComparer<TerrainNode>.Compare(TerrainNode a, TerrainNode b)
            {
                if (a == null || b == null) return 0;

                if (a.Face > b.Face)
                    return 1;
                if (a.Face < b.Face)
                    return -1;
                else
                    return 0;
            }
        }

        public Body ParentBody { get; set; }

        static readonly byte HORIZON_SIZE = 255;

        /// <summary>
        /// The material used by this terrain node.
        /// </summary>
        public Material TerrainMaterial;

        /// <summary>
        /// Describes how the terrain quadtree must be subdivided based on the viewer distance. 
        /// For a field of view of 80 degrees, and a viewport width of 1024 pixels, a quad of size L will be subdivided into subquads, 
        /// if the viewer distance is less than <see cref="SplitFactor"/> * L. 
        /// For a smaller field of view and/or a larger viewport, the quad will be subdivided at a larger distance, so that its size in pixels stays more or less the same. 
        /// This number must be strictly larger than 1.
        /// </summary>
        public float SplitFactor = 2.0f;

        /// <summary>
        /// The maximum level at which the terrain quadtree must be subdivided (inclusive).
        /// The terrain quadtree will never be subdivided beyond this level, even if the viewer comes very close to the terrain.
        /// </summary>
        public int MaxLevel = 16;

        /// <summary>
        /// The terrain quad zmin (only use on start up).
        /// </summary>
        public float ZMin = -5000.0f;

        /// <summary>
        /// The terrain quad zmax (only use on start up).
        /// </summary>
        public float ZMax = 5000.0f;

        /// <summary>
        /// Which face of the cube this terrain is for planets, of 0 for terrains.
        /// </summary>
        public int Face = 0;

        /// <summary>
        /// Perform horizon occlusion culling tests?
        /// </summary>
        public bool UseHorizonCulling = false;

        /// <summary>
        /// Subdivide invisible quads based on distance, like visible ones?
        /// </summary>
        public bool SplitInvisibleQuads = false;

        /// <summary>
        /// The deformation of this terrain. In the terrain local space the terrain sea level surface is flat. 
        /// In the terrain deformed space the sea level surface can be spherical (or flat if the identity deformation is used).
        /// </summary>
        public DeformationBase Deformation { get; protected set; }

        /// <summary>
        /// The root of the terrain quadtree. This quadtree is subdivided based on the current viewer position by the update method.
        /// </summary>
        public TerrainQuad TerrainQuadRoot { get; set; }

        /// <summary>
        /// The current viewer frustum planes in the deformed terrain space.
        /// </summary>
        public Vector4d[] DeformedFrustumPlanes { get; private set; }

        /// <summary>
        /// The current viewer position in the deformed terrain space.
        /// </summary>
        public Vector3d DeformedCameraPosition { get; private set; }

        /// <summary>
        /// The current viewer position in the local terrain space.
        /// </summary>
        public Vector3d LocalCameraPosition { get; private set; }

        public Vector2 DistanceBlending { get; private set; }

        /// <summary>
        /// The viewer distance at which a quad is subdivided, relatively to the quad size.
        /// </summary>
        public float SplitDistance { get; private set; }

        /// <summary>
        /// The ratio between local and deformed lengths at <see cref="LocalCameraPosition"/>.
        /// </summary>
        public float DistanceFactor { get; private set; }

        /// <summary>
        /// Local reference frame used to compute horizon occlusion culling.
        /// </summary>
        public Matrix2x2d LocalCameraDirection { get; private set; }

        /// <summary>
        /// Local to world matrix.
        /// </summary>
        public Matrix4x4d LocalToWorld { get; private set; }

        public Matrix4x4d LocalToCamera { get; private set; }

        public Matrix4x4d LocalToScreen { get; private set; }

        /// <summary>
        /// The rotation of the face to object space.
        /// </summary>
        public Matrix4x4d FaceToLocal { get; private set; }

        /// <summary>
        /// Tangent frame to world matrix.
        /// </summary>
        public Matrix3x3d TangentFrameToWorld { get; private set; }

        public Matrix4x4d DeformedLocalToTangent { get; private set; }

        /// <summary>
        /// Rasterized horizon elevation angle for each azimuth angle.
        /// </summary>
        float[] Horizon = new float[HORIZON_SIZE];

        public List<TileSampler> Samplers = new List<TileSampler>(255);
        public List<TileSampler> SamplersSuitable = new List<TileSampler>(255);

        public TileSamplerOrder SamplersOrder;

        #region NodeSlave<TerrainNode>

        public override void InitNode()
        {
            ParentBody = GetComponentInParent<Body>();

            TerrainMaterial = MaterialHelper.CreateTemp(ParentBody.ColorShader, "TerrainNode");

            FaceToLocal = Matrix4x4d.identity;

            // NOTE : Body shape dependent...
            var celestialBody = ParentBody as CelestialBody;
            var faces = new Vector3d[] { new Vector3d(0, 0, 0), new Vector3d(90, 0, 0), new Vector3d(90, 90, 0), new Vector3d(90, 180, 0), new Vector3d(90, 270, 0), new Vector3d(0, 180, 180) };

            // If this terrain is deformed into a sphere the face matrix is the rotation of the 
            // terrain needed to make up the spherical planet. In this case there should be 6 terrains, each with a unique face number
            if (Face - 1 >= 0 && Face - 1 < 6) { FaceToLocal = Matrix4x4d.Rotate(faces[Face - 1]); }
            if (celestialBody == null) { throw new Exception("Wow! Celestial body isn't Celestial?!"); }

            LocalToWorld = Matrix4x4d.ToMatrix4x4d(celestialBody.transform.localToWorldMatrix) * FaceToLocal;
            Deformation = new DeformationSpherical(celestialBody.Size);

            TangentFrameToWorld = new Matrix3x3d(LocalToWorld.m[0, 0], LocalToWorld.m[0, 1], LocalToWorld.m[0, 2],
                                                 LocalToWorld.m[1, 0], LocalToWorld.m[1, 1], LocalToWorld.m[1, 2],
                                                 LocalToWorld.m[2, 0], LocalToWorld.m[2, 1], LocalToWorld.m[2, 2]);

            InitUniforms(TerrainMaterial);

            CreateTerrainQuadRoot(ParentBody.Size);

            CollectSamplers();
            CollectSamplersSuitable();

            SamplersOrder = new TileSamplerOrder(Samplers);

            var producers = GetComponentsInChildren<TileProducer>();
            var lastProducer = producers[producers.Length - 1];

            if (lastProducer.IsLastInSequence == false)
            {
                lastProducer.IsLastInSequence = true;

                Debug.Log(string.Format("TerrainNode: {0} probably last in generation sequence, but maybe accidentally not marked as. Fixed!", lastProducer.name));
            }
        }

        public override void UpdateNode()
        {
            TerrainMaterial.renderQueue = (int)ParentBody.RenderQueue + ParentBody.RenderQueueOffset;

            // NOTE : Body shape dependent...
            LocalToWorld = Matrix4x4d.ToMatrix4x4d(ParentBody.transform.localToWorldMatrix) * FaceToLocal;

            TangentFrameToWorld = new Matrix3x3d(LocalToWorld.m[0, 0], LocalToWorld.m[0, 1], LocalToWorld.m[0, 2],
                                                 LocalToWorld.m[1, 0], LocalToWorld.m[1, 1], LocalToWorld.m[1, 2],
                                                 LocalToWorld.m[2, 0], LocalToWorld.m[2, 1], LocalToWorld.m[2, 2]);

            LocalToCamera = GodManager.Instance.View.WorldToCameraMatrix * LocalToWorld;
            LocalToScreen = GodManager.Instance.View.CameraToScreenMatrix * LocalToCamera;

            var invLocalToCamera = LocalToCamera.Inverse();

            DeformedCameraPosition = invLocalToCamera * Vector3d.zero;
            DeformedFrustumPlanes = Frustum3d.GetFrustumPlanes(LocalToScreen); // NOTE : Extract frustum planes from LocalToScreen matrix...

            LocalCameraPosition = Deformation.DeformedToLocal(DeformedCameraPosition);

            DeformedLocalToTangent = Deformation.DeformedToTangentFrame(GodManager.Instance.View.WorldCameraPosition) * LocalToWorld * Deformation.LocalToDeformedDifferential(LocalCameraPosition);

            var m = Deformation.LocalToDeformedDifferential(LocalCameraPosition, true);

            var left = DeformedFrustumPlanes[0].xyz.Normalized();
            var right = DeformedFrustumPlanes[1].xyz.Normalized();

            var fov = (float)Functions.Safe_Acos(-left.Dot(right));

            SplitDistance = SplitFactor * Screen.width / 1024.0f * Mathf.Tan(40.0f * Mathf.Deg2Rad) / Mathf.Tan(fov / 2.0f);
            DistanceFactor = (float)Math.Max(new Vector3d(m.m[0, 0], m.m[1, 0], m.m[2, 0]).Magnitude(), new Vector3d(m.m[0, 1], m.m[1, 1], m.m[2, 1]).Magnitude());

            if (SplitDistance < 1.1f || SplitDistance > 128.0f || !Functions.IsFinite(SplitDistance)) { SplitDistance = 1.1f; }

            var splitDistanceBlending = SplitDistance + 1.0f;

            DistanceBlending = new Vector2(splitDistanceBlending, 2.0f * SplitDistance - splitDistanceBlending);

            // Initializes data structures for horizon occlusion culling
            if (UseHorizonCulling && LocalCameraPosition.z <= TerrainQuadRoot.ZMax)
            {
                var deformedDirection = invLocalToCamera * Vector3d.forward;
                var localDirection = (Deformation.DeformedToLocal(deformedDirection) - LocalCameraPosition).Normalized();

                LocalCameraDirection = new Matrix2x2d(localDirection.y, -localDirection.x, -localDirection.x, -localDirection.y);

                for (byte i = 0; i < HORIZON_SIZE; ++i)
                {
                    Horizon[i] = float.NegativeInfinity;
                }
            }

            if (ParentBody.UpdateLOD)
            {
                TerrainQuadRoot.UpdateLOD();
            }

            SetUniforms(TerrainMaterial);
        }

        protected override void OnDestroy()
        {
            Helper.Destroy(TerrainMaterial);

            base.OnDestroy();
        }

        #endregion

        #region IUniformed<Material>

        public virtual void InitUniforms(Material target)
        {
            if (target == null) return;

            ParentBody.InitUniforms(target);
        }

        public virtual void SetUniforms(Material target)
        {
            if (target == null) return;

            ParentBody.SetUniforms(target);

            Deformation.SetUniforms(this, target);
        }

        #endregion

        #region IUniformed

        public virtual void InitSetUniforms()
        {
            InitUniforms(TerrainMaterial);
            SetUniforms(TerrainMaterial);
        }

        #endregion

        public void SetPerQuadUniforms(TerrainQuad quad, MaterialPropertyBlock target)
        {
            // TODO : BOTTLENECK!
            Deformation.SetUniforms(this, quad, target);

            // TODO : Planet texturing...
            /*
            var rootQuadSize = TerrainQuadRoot.Length;
            var offset = new Vector4d(((double)quad.Tx / (1 << quad.Level) - 0.5) * rootQuadSize,
                                      ((double)quad.Ty / (1 << quad.Level) - 0.5) * rootQuadSize,
                                      rootQuadSize / (1 << quad.Level),
                                      ParentBody.Size);
            
            target.SetVector("_Offset", offset.ToVector4());

            if (ParentBody.TCCPS != null) ParentBody.TCCPS.SetUniforms(target);
            */

            /*
            if (ParentBody.TCCPS != null) ParentBody.TCCPS.SetUniforms(target);
            */
        }

        #region Rendering

        private bool FindDrawableSamplers(TerrainQuad quad)
        {
            for (short i = 0; i < SamplersSuitable.Count; ++i)
            {
                var producer = SamplersSuitable[i].Producer;

                if (producer.HasTile(quad.Level, quad.Tx, quad.Ty) && producer.FindTile(quad.Level, quad.Tx, quad.Ty, false, true) == null)
                {
                    return true;
                }
            }

            return false;
        }

        public void FindDrawableQuads(TerrainQuad quad)
        {
            quad.Drawable = false;

            if (!quad.IsVisible)
            {
                quad.Drawable = true;

                return;
            }

            if (quad.IsLeaf)
            {
                if (FindDrawableSamplers(quad)) return;
            }
            else
            {
                byte drawableCount = 0;

                for (byte i = 0; i < 4; ++i)
                {
                    var targetQuad = quad.GetChild(i);

                    FindDrawableQuads(targetQuad);

                    if (targetQuad.Drawable)
                    {
                        ++drawableCount;
                    }
                }

                if (drawableCount < 4)
                {
                    if (FindDrawableSamplers(quad)) return;
                }
            }

            quad.Drawable = true;
        }

        private void DrawMesh(TerrainQuad quad, Mesh mesh, MaterialPropertyBlock mpb, int layer)
        {
            // Set the uniforms unique to each quad
            SetPerQuadUniforms(quad, mpb);

            Graphics.DrawMesh(mesh, Matrix4x4.identity, TerrainMaterial, layer, CameraHelper.Main(), 0, mpb, ShadowCastingMode.On, true);
        }

        public void DrawQuads(TerrainQuad quad, Mesh mesh, MaterialPropertyBlock mpb, int layer)
        {
            if (!quad.IsVisible) return;
            if (!quad.Drawable) return;

            if (quad.IsLeaf)
            {
                for (byte i = 0; i < SamplersSuitable.Count; ++i)
                {
                    // Set the unifroms needed to draw the texture for this sampler
                    SamplersSuitable[i].SetUniforms(mpb, quad);
                }

                DrawMesh(quad, mesh, mpb, layer);
            }
            else
            {
                quad.CalculateOrder(quad.Owner.LocalCameraPosition.x, quad.Owner.LocalCameraPosition.y, quad.Ox + quad.LengthHalf, quad.Oy + quad.LengthHalf);

                // Draw quads in a order based on distance to camera
                var done = 0;

                for (byte i = 0; i < 4; ++i)
                {
                    var targetQuad = quad.GetChild(quad.Order[i]);

                    if (targetQuad.Visibility == Frustum3d.VISIBILITY.INVISIBLE)
                    {
                        done |= 1 << quad.Order[i];
                    }
                    else if (targetQuad.Drawable)
                    {
                        DrawQuads(targetQuad, mesh, mpb, layer);

                        done |= 1 << quad.Order[i];
                    }
                }

                if (done < 15)
                {
                    // If the a leaf quad needs to be drawn but its tiles are not ready, then this will draw the next parent tile instead that is ready.
                    // Because of the current set up all tiles always have there tasks run on the frame they are generated, so this section of code is never reached.
                    Debug.LogWarning(string.Format("Looks like rendering false start! {0}:{1}:{2}", quad.Level, quad.Tx, quad.Ty));

                    for (byte i = 0; i < SamplersSuitable.Count; ++i)
                    {
                        // Set the unifroms needed to draw the texture for this sampler
                        SamplersSuitable[i].SetUniforms(mpb, quad);
                    }

                    DrawMesh(quad, mesh, mpb, layer);
                }
            }
        }

        public Queue<TerrainQuad> Traverse(TerrainQuad root)
        {
            if (!root.IsVisible) return null;
            if (!root.Drawable) return null;

            var traverse = new Queue<TerrainQuad>();
            var quadsQueue = new Queue<TerrainQuad>();
            var quadsSet = new HashSet<TerrainQuad>();

            quadsQueue.Enqueue(root);
            quadsSet.Add(root);

            while (quadsQueue.Count > 0)
            {
                var currentQuad = quadsQueue.Dequeue();

                traverse.Enqueue(currentQuad);

                if (currentQuad.IsLeaf)
                {
                    quadsSet.Add(currentQuad);
                }
                else
                {
                    for (byte i = 0; i < 4; ++i)
                    {
                        var currentQuadChild = currentQuad.GetChild(currentQuad.Order[i]);

                        if (!quadsSet.Contains(currentQuadChild) && (currentQuadChild.IsVisible && currentQuadChild.Drawable))
                        {
                            quadsQueue.Enqueue(currentQuadChild);
                            quadsSet.Add(currentQuadChild);
                        }
                    }
                }
            }

            return traverse;
        }

        #endregion

        /// <summary>
        /// This mehod will collect all child <see cref="TileSampler"/>s in to <see cref="Samplers"/> collection.
        /// Don't forget to call this method after Add/Remove operations on <see cref="TileSampler"/>.
        /// </summary>
        public virtual void CollectSamplers()
        {
            Samplers.Clear();
            
            var samplers = GetComponentsInChildren<TileSampler>().ToList();

            if (samplers.Count > 255)
            {
                Debug.LogWarning(string.Format("TerrainNode: Toomuch samplers! {0}; Only first 255 will be taken!", samplers.Count));

                Samplers = samplers.GetRange(0, 255);

                return;
            }

            Samplers = samplers;      
        }

        /// <summary>
        /// This mehod will collect all child <see cref="TileSampler"/>s, wich will be used by rendering pipeline in to <see cref="SamplersSuitable"/> collection.
        /// Don't forget to call this method after Add/Remove operations on <see cref="TileSampler"/>.
        /// </summary>
        public virtual void CollectSamplersSuitable()
        {
            // NOTE : Should i check for Samplers list before? I SAY - NOPE!

            SamplersSuitable.Clear();

            var samplersSuitable = Samplers.Where(sampler => sampler.enabled && sampler.StoreLeaf).ToList();

            if (samplersSuitable.Count > 255)
            {
                Debug.LogWarning(string.Format("TerrainNode: Toomuch suitable samplers! {0}; Only first 255 will be taken!", samplersSuitable.Count));

                Samplers = samplersSuitable.GetRange(0, 255);

                return;
            }

            SamplersSuitable = samplersSuitable;
        }

        private void CreateTerrainQuadRoot(float size)
        {
            if (TerrainQuadRoot != null) { Debug.Log("TerrainNode: Hey! You're gonna create quad root, but it's already exist!"); return; }

            TerrainQuadRoot = new TerrainQuad(this, null, 0, 0, -size, -size, 2.0 * size, ZMin, ZMax);
        }

        #region Occluding

        private readonly Vector2d[] OccluderCorners = new Vector2d[4];
        private readonly Vector3d[] OccluderBounds = new Vector3d[4];

        /// <summary>
        /// Check if the given bounding box is occluded.
        /// </summary>
        /// <param name="occluder">A bounding box in local (non deformed) coordinates.</param>
        /// <returns>Returns 'True' if the given bounding box is occluded by the bounding boxes previously added as occluders by <see cref="AddOccluder"/></returns>
        public bool IsOccluded(Box3d occluder)
        {
            if (!UseHorizonCulling || LocalCameraPosition.z > TerrainQuadRoot.ZMax)
            {
                return false;
            }

            var plane = LocalCameraPosition.xy;

            OccluderCorners[0] = LocalCameraDirection * (new Vector2d(occluder.Min.x, occluder.Min.y) - plane);
            OccluderCorners[1] = LocalCameraDirection * (new Vector2d(occluder.Min.x, occluder.Max.y) - plane);
            OccluderCorners[2] = LocalCameraDirection * (new Vector2d(occluder.Max.x, occluder.Min.y) - plane);
            OccluderCorners[3] = LocalCameraDirection * (new Vector2d(occluder.Max.x, occluder.Max.y) - plane);

            if (OccluderCorners[0].y <= 0.0 || OccluderCorners[1].y <= 0.0 || OccluderCorners[2].y <= 0.0 || OccluderCorners[3].y <= 0.0)
            {
                return false;
            }

            var dzmax = occluder.Max.z - LocalCameraPosition.z;

            OccluderCorners[0] = new Vector2d(OccluderCorners[0].x, dzmax) / OccluderCorners[0].y;
            OccluderCorners[1] = new Vector2d(OccluderCorners[1].x, dzmax) / OccluderCorners[1].y;
            OccluderCorners[2] = new Vector2d(OccluderCorners[2].x, dzmax) / OccluderCorners[2].y;
            OccluderCorners[3] = new Vector2d(OccluderCorners[3].x, dzmax) / OccluderCorners[3].y;

            var xmin = Math.Min(Math.Min(OccluderCorners[0].x, OccluderCorners[1].x), Math.Min(OccluderCorners[2].x, OccluderCorners[3].x)) * 0.33 + 0.5;
            var xmax = Math.Max(Math.Max(OccluderCorners[0].x, OccluderCorners[1].x), Math.Max(OccluderCorners[2].x, OccluderCorners[3].x)) * 0.33 + 0.5;
            var zmax = Math.Max(Math.Max(OccluderCorners[0].y, OccluderCorners[1].y), Math.Max(OccluderCorners[2].y, OccluderCorners[3].y));

            var imin = Math.Max((int)Math.Floor(xmin * HORIZON_SIZE), 0);
            var imax = Math.Min((int)Math.Ceiling(xmax * HORIZON_SIZE), HORIZON_SIZE - 1);

            // NOTE : Looks like horizon culling isn't working properly. Maybe should be debugged or something...
            for (byte i = (byte)imin; i <= imax; ++i)
            {
                if (zmax > Horizon[i])
                {
                    return false;
                }
            }

            return (imax >= imin);
        }

        /// <summary>
        /// Adds the given bounding box as an occluder. The bounding boxes must be added in front to back order.
        /// </summary>
        /// <param name="occluder">A bounding box in local (non deformed) coordinates.</param>
        /// <returns>Returns 'True' if the given bounding box is occluded by the bounding boxes previously added as occluders by this method.</returns>
        public bool AddOccluder(Box3d occluder)
        {
            if (!UseHorizonCulling || LocalCameraPosition.z > TerrainQuadRoot.ZMax)
            {
                return false;
            }

            var plane = LocalCameraPosition.xy;

            OccluderCorners[0] = LocalCameraDirection * (new Vector2d(occluder.Min.x, occluder.Min.y) - plane);
            OccluderCorners[1] = LocalCameraDirection * (new Vector2d(occluder.Min.x, occluder.Max.y) - plane);
            OccluderCorners[2] = LocalCameraDirection * (new Vector2d(occluder.Max.x, occluder.Min.y) - plane);
            OccluderCorners[3] = LocalCameraDirection * (new Vector2d(occluder.Max.x, occluder.Max.y) - plane);

            if (OccluderCorners[0].y <= 0.0 || OccluderCorners[1].y <= 0.0 || OccluderCorners[2].y <= 0.0 || OccluderCorners[3].y <= 0.0)
            {
                // Skips bounding boxes that are not fully behind the "near plane" of the reference frame used for horizon occlusion culling
                return false;
            }

            var dzmin = occluder.Min.z - LocalCameraPosition.z;
            var dzmax = occluder.Max.z - LocalCameraPosition.z;

            OccluderBounds[0] = new Vector3d(OccluderCorners[0].x, dzmin, dzmax) / OccluderCorners[0].y;
            OccluderBounds[1] = new Vector3d(OccluderCorners[1].x, dzmin, dzmax) / OccluderCorners[1].y;
            OccluderBounds[2] = new Vector3d(OccluderCorners[2].x, dzmin, dzmax) / OccluderCorners[2].y;
            OccluderBounds[3] = new Vector3d(OccluderCorners[3].x, dzmin, dzmax) / OccluderCorners[3].y;

            var xmin = Math.Min(Math.Min(OccluderBounds[0].x, OccluderBounds[1].x), Math.Min(OccluderBounds[2].x, OccluderBounds[3].x)) * 0.33 + 0.5;
            var xmax = Math.Max(Math.Max(OccluderBounds[0].x, OccluderBounds[1].x), Math.Max(OccluderBounds[2].x, OccluderBounds[3].x)) * 0.33 + 0.5;
            var zmin = Math.Min(Math.Min(OccluderBounds[0].y, OccluderBounds[1].y), Math.Min(OccluderBounds[2].y, OccluderBounds[3].y));
            var zmax = Math.Max(Math.Max(OccluderBounds[0].z, OccluderBounds[1].z), Math.Max(OccluderBounds[2].z, OccluderBounds[3].z));

            var imin = Math.Max((int)Math.Floor(xmin * HORIZON_SIZE), 0);
            var imax = Math.Min((int)Math.Ceiling(xmax * HORIZON_SIZE), HORIZON_SIZE - 1);

            // First checks if the bounding box projection is below the current horizon line
            var occluded = (imax >= imin);

            for (byte i = (byte)imin; i <= imax; ++i)
            {
                if (zmax > Horizon[i])
                {
                    occluded = false;

                    break;
                }
            }

            if (!occluded)
            {
                // If it is not, updates the horizon line with the projection of this bounding box
                imin = Math.Max((int)Math.Ceiling(xmin * HORIZON_SIZE), 0);
                imax = Math.Min((int)Math.Floor(xmax * HORIZON_SIZE), HORIZON_SIZE - 1);

                for (byte i = (byte)imin; i <= imax; ++i)
                {
                    Horizon[i] = (float)Math.Max(Horizon[i], zmin);
                }
            }

            return occluded;
        }

        #endregion

        /// <summary>
        /// Distance between the current viewer position and the given bounding box.
        /// This distance is measured in the local terrain space. Deformations taken in to account.
        /// </summary>
        /// <param name="localBox">Target bounding box.</param>
        /// <returns>Returns the distance between the current viewer position and the given bounding box.</returns>
        public double GetCameraDistance(Box3d localBox)
        {
            return GetCameraDistance(localBox.Min.x, localBox.Max.x, localBox.Min.y, localBox.Max.y, localBox.Max.z);
        }

        /// <summary>
        /// Distance between the current viewer position and the given bounding box.
        /// This distance is measured in the local terrain space. Deformations taken in to account.
        /// </summary>
        /// <param name="localBox">Target bounding box.</param>
        /// <param name="z">Target bounding box Z.</param>
        /// <returns>Returns the distance between the current viewer position and the given bounding box.</returns>
        public double GetCameraDistance(Box3d localBox, double z)
        {
            return GetCameraDistance(localBox.Min.x, localBox.Max.x, localBox.Min.y, localBox.Max.y, z);
        }

        /// <summary>
        /// Distance between the current viewer position and the given bounding box, described as Min/Max values.
        /// This distance is measured in the local terrain space. Deformations taken in to account.
        /// </summary>
        /// <param name="minX">Target Bounding box X minimum.</param>
        /// <param name="maxX">Target Bounding box X maximum.</param>
        /// <param name="minY">Target Bounding box Y minimum.</param>
        /// <param name="maxY">Target Bounding box Y maximum.</param>
        /// <param name="z">Target Bounding box Z.</param>
        /// <returns></returns>
        public double GetCameraDistance(double minX, double maxX, double minY, double maxY, double z)
        {
            return Math.Max(Math.Abs(LocalCameraPosition.z - z) / DistanceFactor,
                   Math.Max(Math.Min(Math.Abs(LocalCameraPosition.x - minX), Math.Abs(LocalCameraPosition.x - maxX)),
                   Math.Min(Math.Abs(LocalCameraPosition.y - minY), Math.Abs(LocalCameraPosition.y - maxY))));
        }
    }
}