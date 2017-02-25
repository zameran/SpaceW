using System;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// A view for flat terrains. The camera position is specified from a "look at" position (<see cref="Position.x0"/>, <see cref="Position.y0"/>) on ground, 
    /// with a distance <see cref="Position.distance"/> between camera and this position, 
    /// and two angles (<see cref="Position.theta"/>, <see cref="Position.phi"/>) for the direction of this vector.
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
            /// For a planet these are the longtitudes;
            /// </summary>
            public double x0;

            /// <summary>
            /// The y coordinate of the point the camera is looking at on the ground.
            /// For a planet these are the latitudes;
            /// </summary>
            public double y0;

            /// <summary>
            /// The zenith angle of the vector between the "look at" point and the camera.
            /// </summary>
            public double theta;

            /// <summary>
            /// The azimuth angle of the vector between the "look at" point and the camera.
            /// </summary>
            public double phi;

            /// <summary>
            /// The distance between the "look at" point and the camera.
            /// </summary>
            public double distance;
        };

        [SerializeField]
        protected Position m_position;

        protected Matrix4x4d m_worldToCameraMatrix;
        protected Matrix4x4d m_cameraToWorldMatrix;
        protected Matrix4x4d m_cameraToScreenMatrix;
        protected Matrix4x4d m_screenToCameraMatrix;

        protected Vector3d m_worldCameraPos;
        protected Vector3d m_cameraDir;

        protected double m_groundHeight = 0.0;

        protected Vector3d m_worldPos;

        public double GetGroundHeight()
        {
            return m_groundHeight;
        }

        public void SetGroundHeight(double ht)
        {
            m_groundHeight = ht;
        }

        public Position GetPos()
        {
            return m_position;
        }

        public Matrix4x4d GetWorldToCamera()
        {
            return m_worldToCameraMatrix;
        }

        public Matrix4x4d GetCameraToWorld()
        {
            return m_cameraToWorldMatrix;
        }

        public Matrix4x4d GetCameraToScreen()
        {
            return m_cameraToScreenMatrix;
        }

        public Matrix4x4d GetScreenToCamera()
        {
            return m_screenToCameraMatrix;
        }

        public Vector3d GetWorldCameraPos()
        {
            return m_worldCameraPos;
        }

        public Vector3d GetCameraDir()
        {
            return m_cameraDir;
        }

        public virtual Vector3d GetLookAtPos()
        {
            return new Vector3d(m_position.x0, m_position.y0, 0.0);
        }

        public virtual double GetHeight()
        {
            return m_worldPos.z;
        }

        /// <summary>
        /// Any contraints you need on the position are applied here.
        /// </summary>
        public virtual void Constrain()
        {
            m_position.theta = Math.Max(0.0001, Math.Min(Math.PI, m_position.theta));
            m_position.distance = Math.Max(0.1, m_position.distance);
        }

        protected virtual void Start()
        {
            m_worldToCameraMatrix = Matrix4x4d.Identity();
            m_cameraToWorldMatrix = Matrix4x4d.Identity();
            m_cameraToScreenMatrix = Matrix4x4d.Identity();
            m_screenToCameraMatrix = Matrix4x4d.Identity();
            m_worldCameraPos = new Vector3d();
            m_cameraDir = new Vector3d();
            m_worldPos = new Vector3d();

            Constrain();
        }

        public virtual void UpdateView()
        {
            Constrain();

            SetWorldToCameraMatrix();
            SetProjectionMatrix();

            m_worldCameraPos = m_worldPos;
            m_cameraDir = (m_worldPos - GetLookAtPos()).Normalized();
        }

        protected virtual void SetWorldToCameraMatrix()
        {
            Vector3d po = new Vector3d(m_position.x0, m_position.y0, 0.0);
            Vector3d px = new Vector3d(1.0, 0.0, 0.0);
            Vector3d py = new Vector3d(0.0, 1.0, 0.0);
            Vector3d pz = new Vector3d(0.0, 0.0, 1.0);

            double ct = Math.Cos(m_position.theta);
            double st = Math.Sin(m_position.theta);
            double cp = Math.Cos(m_position.phi);
            double sp = Math.Sin(m_position.phi);

            Vector3d cx = px * cp + py * sp;
            Vector3d cy = (px * -1.0) * sp * ct + py * cp * ct + pz * st;
            Vector3d cz = px * sp * st - py * cp * st + pz * ct;

            m_worldPos = po + cz * m_position.distance;

            if (m_worldPos.z < m_groundHeight + 10.0)
            {
                m_worldPos.z = m_groundHeight + 10.0;
            }

            Matrix4x4d view = new Matrix4x4d(cx.x, cx.y, cx.z, 0.0, cy.x, cy.y, cy.z, 0.0, cz.x, cz.y, cz.z, 0.0, 0.0, 0.0, 0.0, 1.0);

            m_worldToCameraMatrix = view * Matrix4x4d.Translate(m_worldPos * -1.0);

            m_worldToCameraMatrix.m[0, 0] *= -1.0;
            m_worldToCameraMatrix.m[0, 1] *= -1.0;
            m_worldToCameraMatrix.m[0, 2] *= -1.0;
            m_worldToCameraMatrix.m[0, 3] *= -1.0;

            m_cameraToWorldMatrix = m_worldToCameraMatrix.Inverse();

            GetComponent<Camera>().worldToCameraMatrix = m_worldToCameraMatrix.ToMatrix4x4();
            GetComponent<Camera>().transform.position = m_worldPos.ToVector3();

        }

        protected virtual void SetProjectionMatrix()
        {
            var h = (float)(GetHeight() - m_groundHeight);

            GetComponent<Camera>().nearClipPlane = 0.1f * h;
            GetComponent<Camera>().farClipPlane = 1e6f * h;

            GetComponent<Camera>().ResetProjectionMatrix();

            var p = GetComponent<Camera>().projectionMatrix;
            var d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;

            if (d3d)
            {
                if (GetComponent<Camera>().actualRenderingPath == RenderingPath.DeferredLighting)
                {
                    for (byte i = 0; i < 4; i++)
                    {
                        p[1, i] = -p[1, i];
                    }
                }

                // Scale and bias depth range
                for (byte i = 0; i < 4; i++)
                {
                    p[2, i] = p[2, i] * 0.5f + p[3, i] * 0.5f;
                }
            }

            m_cameraToScreenMatrix = new Matrix4x4d(p);
            m_screenToCameraMatrix = m_cameraToScreenMatrix.Inverse();
        }

        /// <summary>
        /// Moves the "look at" point so that <see cref="oldp"/> appears at the position of <see cref="p"/> on screen.
        /// </summary>
        /// <param name="oldp"></param>
        /// <param name="p"></param>
        /// <param name="speed"></param>
        public virtual void Move(Vector3d oldp, Vector3d p, double speed)
        {
            m_position.x0 -= (p.x - oldp.x) * speed * Math.Max(1.0, GetHeight());
            m_position.y0 -= (p.y - oldp.y) * speed * Math.Max(1.0, GetHeight());
        }

        public virtual void MoveForward(double distance)
        {
            m_position.x0 -= Math.Sin(m_position.phi) * distance;
            m_position.y0 += Math.Cos(m_position.phi) * distance;
        }

        public virtual void Turn(double angle)
        {
            m_position.phi += angle;
        }

        public virtual double Interpolate(double sx0, double sy0, double stheta, double sphi, double sd, double dx0, double dy0, double dtheta, double dphi, double dd, double t)
        {
            // TODO : Interpolation

            m_position.x0 = dx0;
            m_position.y0 = dy0;
            m_position.theta = dtheta;
            m_position.phi = dphi;
            m_position.distance = dd;

            return 1.0;
        }

        public virtual void InterpolatePos(double sx0, double sy0, double dx0, double dy0, double t, ref double x0, ref double y0)
        {
            x0 = sx0 * (1.0 - t) + dx0 * t;
            y0 = sy0 * (1.0 - t) + dy0 * t;
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

            lat = MathUtility.Safe_Asin(v.z);
            lon = Math.Atan2(v.y, v.x);
        }
    }
}