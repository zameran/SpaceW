using System;

using UnityEngine;

using Functions = SpaceEngine.Core.Numerics.Functions;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// A view for flat terrains. The camera position is specified from a "look at" position (<see cref="Position.X"/>, <see cref="Position.Y"/>) on ground, 
    /// with a distance <see cref="Position.Distance"/> between camera and this position, 
    /// and two angles (<see cref="Position.Theta"/>, <see cref="Position.Phi"/>) for the direction of this vector.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(Controller))]
    public class TerrainView : MonoBehaviour
    {
        [Serializable]
        public class Position
        {
            /// <summary>
            /// The x coordinate of the point the camera is looking at on the ground.
            /// For a planet these are the longtitudes.
            /// </summary>
            public double X;

            /// <summary>
            /// The y coordinate of the point the camera is looking at on the ground.
            /// For a planet these are the latitudes.
            /// </summary>
            public double Y;

            /// <summary>
            /// The zenith angle of the vector between the "look at" point and the camera.
            /// </summary>
            public double Theta;

            /// <summary>
            /// The azimuth angle of the vector between the "look at" point and the camera.
            /// </summary>
            public double Phi;

            /// <summary>
            /// The distance between the "look at" point and the camera.
            /// </summary>
            public double Distance;
        };

        private readonly CachedComponent<Camera> CameraCachedComponent = new CachedComponent<Camera>();
        private readonly CachedComponent<Controller> ControllerCachedComponent = new CachedComponent<Controller>();

        public Camera CameraComponent { get { return CameraCachedComponent.Component; } }
        public Controller ControllerComponent { get { return ControllerCachedComponent.Component; } }

        [SerializeField]
        public Position position;

        public Matrix4x4d WorldToCameraMatrix { get; protected set; }
        public Matrix4x4d CameraToWorldMatrix { get; protected set; }
        public Matrix4x4d CameraToScreenMatrix { get; protected set; }
        public Matrix4x4d ScreenToCameraMatrix { get; protected set; }

        public Vector3d WorldCameraPosition { get; protected set; }
        public Vector3d CameraDirection { get; protected set; }

        public double GroundHeight { get; set; }

        [HideInInspector]
        public Vector3d worldPosition;

        public virtual Vector3d GetLookAtPosition()
        {
            return new Vector3d(position.X, position.Y, 0.0);
        }

        public virtual double GetHeight()
        {
            return worldPosition.z;
        }

        /// <summary>
        /// Any contraints you need on the position are applied here.
        /// </summary>
        public virtual void Constrain()
        {
            position.Theta = Math.Max(0.0001, Math.Min(Math.PI, position.Theta));
            position.Distance = Math.Max(0.1, position.Distance);
        }

        protected virtual void Start()
        {
            CameraCachedComponent.TryInit(this);
            ControllerCachedComponent.TryInit(this);

            WorldToCameraMatrix = Matrix4x4d.identity;
            CameraToWorldMatrix = Matrix4x4d.identity;
            CameraToScreenMatrix = Matrix4x4d.identity;
            ScreenToCameraMatrix = Matrix4x4d.identity;
            WorldCameraPosition = Vector3d.zero;
            CameraDirection = Vector3d.zero;
            worldPosition = Vector3d.zero;

            Constrain();
        }

        public virtual void UpdateView()
        {
            Constrain();

            SetWorldToCameraMatrix();
            SetProjectionMatrix();

            WorldCameraPosition = worldPosition;
            CameraDirection = (worldPosition - GetLookAtPosition()).Normalized();
        }

        protected virtual void SetWorldToCameraMatrix()
        {
            var po = new Vector3d(position.X, position.Y, 0.0);
            var px = Vector3d.right;
            var py = Vector3d.up;
            var pz = Vector3d.forward;

            // NOTE : ct - x; st - y; cp - z; sp - w;
            var tp = CalculatelongitudeLatitudeVector(position.Theta, position.Phi);

            Vector3d cx = px * tp.z + py * tp.w;
            Vector3d cy = (px * -1.0) * tp.w * tp.x + py * tp.z * tp.x + pz * tp.y;
            Vector3d cz = px * tp.w * tp.y - py * tp.z * tp.y + pz * tp.x;

            worldPosition = po + cz * position.Distance;

            if (worldPosition.z < GroundHeight + 10.0)
            {
                worldPosition.z = GroundHeight + 10.0;
            }

            Matrix4x4d view = new Matrix4x4d(cx.x, cx.y, cx.z, 0.0, cy.x, cy.y, cy.z, 0.0, cz.x, cz.y, cz.z, 0.0, 0.0, 0.0, 0.0, 1.0);

            WorldToCameraMatrix = view * Matrix4x4d.Translate(worldPosition * -1.0);

            //Flip first row to match Unity's winding order.
            WorldToCameraMatrix.m[0, 0] *= -1.0;
            WorldToCameraMatrix.m[0, 1] *= -1.0;
            WorldToCameraMatrix.m[0, 2] *= -1.0;
            WorldToCameraMatrix.m[0, 3] *= -1.0;

            CameraToWorldMatrix = WorldToCameraMatrix.Inverse();

            CameraComponent.worldToCameraMatrix = WorldToCameraMatrix.ToMatrix4x4();
            CameraComponent.transform.position = worldPosition.ToVector3();
        }

        protected virtual void SetProjectionMatrix()
        {
            var h = (float)(GetHeight() - GroundHeight);

            CameraComponent.nearClipPlane = Mathf.Clamp(0.1f * h, 0.03f, 1000.0f);
            CameraComponent.farClipPlane = Mathf.Clamp(1e6f * h, 1000.0f, 1e12f);

            CameraComponent.ResetProjectionMatrix();

            var projectionMatrix = CameraComponent.projectionMatrix;

            if (SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1)
            {
                // NOTE : Default unity antialiasing breaks matrices?
                if (CameraHelper.IsDeferred(CameraComponent) || QualitySettings.antiAliasing == 0)
                {
                    // Invert Y for rendering to a render texture
                    for (byte i = 0; i < 4; i++)
                    {
                        projectionMatrix[1, i] = -projectionMatrix[1, i];
                    }
                }

                // Scale and bias depth range
                for (byte i = 0; i < 4; i++)
                {
                    projectionMatrix[2, i] = projectionMatrix[2, i] * 0.5f + projectionMatrix[3, i] * 0.5f;
                }
            }

            CameraToScreenMatrix = new Matrix4x4d(projectionMatrix);
            ScreenToCameraMatrix = CameraToScreenMatrix.Inverse();
        }

        public Vector4d CalculatelongitudeLatitudeVector(double x, double y)
        {
            return new Vector4d(Math.Cos(x), Math.Sin(x), Math.Cos(y), Math.Sin(y));
        }

        /// <summary>
        /// Moves the "look at" point so that <see cref="oldp"/> appears at the position of <see cref="p"/> on screen.
        /// </summary>
        /// <param name="oldp"></param>
        /// <param name="p"></param>
        /// <param name="speed"></param>
        public virtual void Move(Vector3d oldp, Vector3d p, double speed)
        {
            position.X -= (p.x - oldp.x) * speed * Math.Max(1.0, GetHeight());
            position.Y -= (p.y - oldp.y) * speed * Math.Max(1.0, GetHeight());
        }

        public virtual void MoveForward(double distance)
        {
            position.X -= Math.Sin(position.Phi) * distance;
            position.Y += Math.Cos(position.Phi) * distance;
        }

        public virtual void Turn(double angle)
        {
            position.Phi += angle;
        }

        public virtual double Interpolate(Position from, Position to, double t)
        {
            // TODO : Interpolation

            position.X = to.X;
            position.Y = to.Y;
            position.Theta = to.Theta;
            position.Phi = to.Phi;
            position.Distance = to.Distance;

            return 1.0;
        }

        public virtual void InterpolatePos(Position from, Position to, double t, ref Position current)
        {
            current.X = from.X * (1.0 - t) + to.X * t;
            current.Y = from.Y * (1.0 - t) + to.Y * t;
        }

        /// <summary>
        /// A direction interpolated between the two given directions.
        /// </summary>
        /// <param name="slon">Start longitude.</param>
        /// <param name="slat">Start latitude.</param>
        /// <param name="elon">End longitude.</param>
        /// <param name="elat">End latitude.</param>
        /// <param name="t">Interpolation.</param>
        /// <param name="lon">Output longitude.</param>
        /// <param name="lat">Output latitude.</param>
        public virtual void InterpolateDirection(double slon, double slat, double elon, double elat, double t, ref double lon, ref double lat)
        {
            var s = new Vector3d(Math.Cos(slon) * Math.Cos(slat), Math.Sin(slon) * Math.Cos(slat), Math.Sin(slat));
            var e = new Vector3d(Math.Cos(elon) * Math.Cos(elat), Math.Sin(elon) * Math.Cos(elat), Math.Sin(elat));
            var v = (s * (1.0 - t) + e * t).Normalized();

            lat = Functions.Safe_Asin(v.z);
            lon = Math.Atan2(v.y, v.x);
        }
    }
}