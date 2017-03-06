using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Terrain.Deformation;

using System;

using UnityEngine;

namespace SpaceEngine.Core.Terrain
{
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
    public class TerrainNode : Node<TerrainNode>
    {
        public CelestialBody Body { get; set; }

        readonly static int HORIZON_SIZE = 256;

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
        public bool UseHorizonCulling = true;

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
        /// The current viewer position in the deformed terrain space.
        /// </summary>
        public Vector3d DeformedCameraPosition { get; private set; }

        /// <summary>
        /// The current viewer frustum planes in the deformed terrain space.
        /// </summary>
        public Vector4d[] DeformedFrustumPlanes { get; private set; }

        /// <summary>
        /// The current viewer position in the local terrain space.
        /// </summary>
        public Vector3d LocalCameraPosition { get; private set; }

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

        /// <summary>
        /// The rotation of the face to object space.
        /// </summary>
        public Matrix4x4d FaceToLocal { get; private set; }

        /// <summary>
        /// Rasterized horizon elevation angle for each azimuth angle.
        /// </summary>
        float[] Horizon = new float[HORIZON_SIZE];

        #region Node

        protected override void InitNode()
        {
            Body = GetComponentInParent<CelestialBody>();
            Body.TerrainNodes.Add(this);

            TerrainMaterial = MaterialHelper.CreateTemp(Body.ColorShader, "TerrainNode");
            TerrainMaterial.SetTexture("_DetailedNormal", Body.DetailedNormal);

            if (Body.Atmosphere != null)
            {
                Body.Atmosphere.InitUniforms(TerrainMaterial);
            }

            var faces = new Vector3d[] { new Vector3d(0, 0, 0), new Vector3d(90, 0, 0), new Vector3d(90, 90, 0), new Vector3d(90, 180, 0), new Vector3d(90, 270, 0), new Vector3d(0, 180, 180) };

            FaceToLocal = Matrix4x4d.identity;

            // If this terrain is deformed into a sphere the face matrix is the rotation of the 
            // terrain needed to make up the spherical planet. In this case there should be 6 terrains, each with a unique face number
            if (Face - 1 >= 0 && Face - 1 < 6)
            {
                FaceToLocal = Matrix4x4d.Rotate(faces[Face - 1]);
            }

            // Update local matrices...
            LocalToWorld = Matrix4x4d.ToMatrix4x4d(Body.transform.localToWorldMatrix) * FaceToLocal;
            //LocalToWorld = FaceToLocal;

            Deformation = new DeformationSpherical(Body.Radius);

            TerrainQuadRoot = new TerrainQuad(this, null, 0, 0, -Body.Radius, -Body.Radius, 2.0 * Body.Radius, ZMin, ZMax);
        }

        protected override void UpdateNode()
        {
            // Update local matrices...
            LocalToWorld = Matrix4x4d.ToMatrix4x4d(Body.transform.localToWorldMatrix) * FaceToLocal;
            //LocalToWorld = FaceToLocal;

            var localToCamera = GodManager.Instance.WorldToCamera * LocalToWorld;
            var localToScreen = GodManager.Instance.CameraToScreen * localToCamera;
            var invLocalToCamera = localToCamera.Inverse();

            DeformedCameraPosition = invLocalToCamera * Vector3d.zero; // TODO : Really? zero?
            DeformedFrustumPlanes = Frustum.GetFrustumPlanes(localToScreen);
            LocalCameraPosition = Deformation.DeformedToLocal(DeformedCameraPosition);

            var m = Deformation.LocalToDeformedDifferential(LocalCameraPosition, true);

            var left = DeformedFrustumPlanes[0].XYZ().Normalized();
            var right = DeformedFrustumPlanes[1].XYZ().Normalized();

            var fov = (float)MathUtility.Safe_Acos(-left.Dot(right));

            SplitDistance = SplitFactor * Screen.width / 1024.0f * Mathf.Tan(40.0f * Mathf.Deg2Rad) / Mathf.Tan(fov / 2.0f);
            DistanceFactor = (float)Math.Max((new Vector3d(m.m[0, 0], m.m[1, 0], m.m[2, 0])).Magnitude(), (new Vector3d(m.m[0, 1], m.m[1, 1], m.m[2, 1])).Magnitude());

            if (SplitDistance < 1.1f || !MathUtility.IsFinite(SplitDistance))
            {
                SplitDistance = 1.1f;
            }

            // initializes data structures for horizon occlusion culling
            if (UseHorizonCulling && LocalCameraPosition.z <= TerrainQuadRoot.ZMax)
            {
                var deformedDirection = invLocalToCamera * Vector3d.forward;
                var localDirection = (Deformation.DeformedToLocal(deformedDirection) - LocalCameraPosition).xy.Normalized();

                LocalCameraDirection = new Matrix2x2d(localDirection.y, -localDirection.x, -localDirection.x, -localDirection.y);

                for (int i = 0; i < HORIZON_SIZE; ++i)
                {
                    Horizon[i] = float.NegativeInfinity;
                }
            }

            TerrainQuadRoot.UpdateLOD();

            if (Body.Atmosphere != null)
            {
                Body.Atmosphere.SetUniforms(TerrainMaterial);
            }

            if (Body.Ocean != null)
            {
                Body.Ocean.SetUniforms(TerrainMaterial);
            }
            else
            {
                TerrainMaterial.SetFloat("_Ocean_DrawBRDF", 0.0f);
            }

            //Manager.GetSunNode().SetUniforms(TerrainMaterial);
            Deformation.SetUniforms(this, TerrainMaterial);
            TerrainMaterial.SetTexture("_DetailedNormal", Body.DetailedNormal);

            //if (Manager.GetPlantsNode() != null)
            //    Manager.GetPlantsNode().SetUniforms(TerrainMaterial);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            Helper.Destroy(TerrainMaterial);

            base.OnDestroy();
        }

        #endregion

        public void SetPerQuadUniforms(TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
        {
            Deformation.SetUniforms(this, quad, matPropertyBlock);
        }

        public Frustum.VISIBILITY GetVisibility(Box3d localBox)
        {
            return Deformation.GetVisibility(this, localBox);
        }

        /// <summary>
        /// Check if the given bounding box is occluded.
        /// </summary>
        /// <param name="box">A bounding box in local (non deformed) coordinates.</param>
        /// <returns>Returns 'True' if the given bounding box is occluded by the bounding boxes previously added as occluders by <see cref="AddOccluder"/></returns>
        public bool IsOccluded(Box3d box)
        {
            if (!UseHorizonCulling || LocalCameraPosition.z > TerrainQuadRoot.ZMax)
            {
                return false;
            }

            var corners = new Vector2d[4];
            var plane = LocalCameraPosition.xy;

            corners[0] = LocalCameraDirection * (new Vector2d(box.xmin, box.ymin) - plane);
            corners[1] = LocalCameraDirection * (new Vector2d(box.xmin, box.ymax) - plane);
            corners[2] = LocalCameraDirection * (new Vector2d(box.xmax, box.ymin) - plane);
            corners[3] = LocalCameraDirection * (new Vector2d(box.xmax, box.ymax) - plane);

            if (corners[0].y <= 0.0 || corners[1].y <= 0.0 || corners[2].y <= 0.0 || corners[3].y <= 0.0)
            {
                return false;
            }

            var dz = box.zmax - LocalCameraPosition.z;

            corners[0] = new Vector2d(corners[0].x, dz) / corners[0].y;
            corners[1] = new Vector2d(corners[1].x, dz) / corners[1].y;
            corners[2] = new Vector2d(corners[2].x, dz) / corners[2].y;
            corners[3] = new Vector2d(corners[3].x, dz) / corners[3].y;

            var xmin = Math.Min(Math.Min(corners[0].x, corners[1].x), Math.Min(corners[2].x, corners[3].x)) * 0.33 + 0.5;
            var xmax = Math.Max(Math.Max(corners[0].x, corners[1].x), Math.Max(corners[2].x, corners[3].x)) * 0.33 + 0.5;
            var zmax = Math.Max(Math.Max(corners[0].y, corners[1].y), Math.Max(corners[2].y, corners[3].y));

            var imin = Math.Max((int)Math.Floor(xmin * HORIZON_SIZE), 0);
            var imax = Math.Min((int)Math.Ceiling(xmax * HORIZON_SIZE), HORIZON_SIZE - 1);

            for (int i = imin; i <= imax; ++i)
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

            var corners = new Vector2d[4];
            var plane = LocalCameraPosition.xy;

            corners[0] = LocalCameraDirection * (new Vector2d(occluder.xmin, occluder.ymin) - plane);
            corners[1] = LocalCameraDirection * (new Vector2d(occluder.xmin, occluder.ymax) - plane);
            corners[2] = LocalCameraDirection * (new Vector2d(occluder.xmax, occluder.ymin) - plane);
            corners[3] = LocalCameraDirection * (new Vector2d(occluder.xmax, occluder.ymax) - plane);

            if (corners[0].y <= 0.0 || corners[1].y <= 0.0 || corners[2].y <= 0.0 || corners[3].y <= 0.0)
            {
                // Skips bounding boxes that are not fully behind the "near plane" of the reference frame used for horizon occlusion culling
                return false;
            }

            var dzmin = occluder.zmin - LocalCameraPosition.z;
            var dzmax = occluder.zmax - LocalCameraPosition.z;

            var bounds = new Vector3d[4];

            bounds[0] = new Vector3d(corners[0].x, dzmin, dzmax) / corners[0].y;
            bounds[1] = new Vector3d(corners[1].x, dzmin, dzmax) / corners[1].y;
            bounds[2] = new Vector3d(corners[2].x, dzmin, dzmax) / corners[2].y;
            bounds[3] = new Vector3d(corners[3].x, dzmin, dzmax) / corners[3].y;

            var xmin = Math.Min(Math.Min(bounds[0].x, bounds[1].x), Math.Min(bounds[2].x, bounds[3].x)) * 0.33 + 0.5;
            var xmax = Math.Max(Math.Max(bounds[0].x, bounds[1].x), Math.Max(bounds[2].x, bounds[3].x)) * 0.33 + 0.5;
            var zmin = Math.Min(Math.Min(bounds[0].y, bounds[1].y), Math.Min(bounds[2].y, bounds[3].y));
            var zmax = Math.Max(Math.Max(bounds[0].z, bounds[1].z), Math.Max(bounds[2].z, bounds[3].z));

            var imin = Math.Max((int)Math.Floor(xmin * HORIZON_SIZE), 0);
            var imax = Math.Min((int)Math.Ceiling(xmax * HORIZON_SIZE), HORIZON_SIZE - 1);

            // First checks if the bounding box projection is below the current horizon line
            var occluded = (imax >= imin);

            for (int i = imin; i <= imax; ++i)
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

                for (int i = imin; i <= imax; ++i)
                {
                    Horizon[i] = (float)Math.Max(Horizon[i], zmin);
                }
            }

            return occluded;
        }

        /// <summary>
        /// Distance between the current viewer position and the given bounding box.
        /// This distance is measured in the local terrain space. Deformations taken in to account.
        /// </summary>
        /// <param name="localBox"></param>
        /// <returns>Returns the distance between the current viewer position and the given bounding box.</returns>
        public double GetCameraDist(Box3d localBox)
        {
            return Math.Max(Math.Abs(LocalCameraPosition.z - localBox.zmax) / DistanceFactor,
                   Math.Max(Math.Min(Math.Abs(LocalCameraPosition.x - localBox.xmin), Math.Abs(LocalCameraPosition.x - localBox.xmax)),
                   Math.Min(Math.Abs(LocalCameraPosition.y - localBox.ymin), Math.Abs(LocalCameraPosition.y - localBox.ymax))));
        }
    }
}