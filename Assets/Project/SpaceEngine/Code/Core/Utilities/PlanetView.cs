using System;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// A <see cref="TerrainView"/> for spherical terrains. 
    /// This subclass interprets the <see cref="TerrainView.Position.x0"/> and <see cref="TerrainView.Position.y0"/> fields as longitudes and latitudes on the planet,
    /// and considers <see cref="TerrainView.Position.theta"/> and <see cref="TerrainView.Position.phi"/> as relative to the tangent plane at the point.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlanetView : TerrainView
    {
        public double Radius { get; set; }

        public override double GetHeight()
        {
            return m_worldPos.Magnitude() - Radius;
        }

        public override Vector3d GetLookAtPos()
        {
            var co = Math.Cos(m_position.x0); // x0 = longitude
            var so = Math.Sin(m_position.x0);
            var ca = Math.Cos(m_position.y0); // y0 = latitude
            var sa = Math.Sin(m_position.y0);

            return new Vector3d(co * ca, so * ca, sa) * Radius;
        }

        public override void Constrain()
        {
            m_position.y0 = Math.Max(-Math.PI / 2.0, Math.Min(Math.PI / 2.0, m_position.y0));
            m_position.theta = Math.Max(0.1, Math.Min(Math.PI, m_position.theta));
            m_position.distance = Math.Max(0.1, m_position.distance);
        }

        protected override void Start()
        {
            base.Start();

            Constrain();
        }

        protected override void SetWorldToCameraMatrix()
        {
            double co = Math.Cos(m_position.x0); // x0 = longitude
            double so = Math.Sin(m_position.x0);
            double ca = Math.Cos(m_position.y0); // y0 = latitude
            double sa = Math.Sin(m_position.y0);

            Vector3d po = new Vector3d(co * ca, so * ca, sa) * Radius;
            Vector3d px = new Vector3d(-so, co, 0.0);
            Vector3d py = new Vector3d(-co * sa, -so * sa, ca);
            Vector3d pz = new Vector3d(co * ca, so * ca, sa);

            double ct = Math.Cos(m_position.theta);
            double st = Math.Sin(m_position.theta);
            double cp = Math.Cos(m_position.phi);
            double sp = Math.Sin(m_position.phi);

            Vector3d cx = px * cp + py * sp;
            Vector3d cy = (px * -1.0) * sp * ct + py * cp * ct + pz * st;
            Vector3d cz = px * sp * st - py * cp * st + pz * ct;

            m_worldPos = po + cz * m_position.distance;

            if (m_worldPos.Magnitude() < Radius + 10.0 + m_groundHeight)
            {
                m_worldPos = m_worldPos.Normalized(Radius + 10.0 + m_groundHeight);
            }

            Matrix4x4d view = new Matrix4x4d(cx.x, cx.y, cx.z, 0.0, cy.x, cy.y, cy.z, 0.0, cz.x, cz.y, cz.z, 0.0, 0.0, 0.0, 0.0, 1.0);

            m_worldToCameraMatrix = view * Matrix4x4d.Translate(m_worldPos * -1.0);

            //Flip first row to match Unity's winding order.
            m_worldToCameraMatrix.m[0, 0] *= -1.0;
            m_worldToCameraMatrix.m[0, 1] *= -1.0;
            m_worldToCameraMatrix.m[0, 2] *= -1.0;
            m_worldToCameraMatrix.m[0, 3] *= -1.0;

            m_cameraToWorldMatrix = m_worldToCameraMatrix.Inverse();

            GetComponent<Camera>().worldToCameraMatrix = m_worldToCameraMatrix.ToMatrix4x4();
            GetComponent<Camera>().transform.position = m_worldPos.ToVector3();

        }

        public override void Move(Vector3d oldp, Vector3d p, double speed)
        {
            Vector3d oldpos = oldp.Normalized();
            Vector3d pos = p.Normalized();

            double oldlat = MathUtility.Safe_Asin(oldpos.z);
            double oldlon = Math.Atan2(oldpos.y, oldpos.x);
            double lat = MathUtility.Safe_Asin(pos.z);
            double lon = Math.Atan2(pos.y, pos.x);

            m_position.x0 -= (lon - oldlon) * speed * Math.Max(1.0, GetHeight());
            m_position.y0 -= (lat - oldlat) * speed * Math.Max(1.0, GetHeight());
        }

        public override void MoveForward(double distance)
        {
            double co = Math.Cos(m_position.x0); // x0 = longitude
            double so = Math.Sin(m_position.x0);
            double ca = Math.Cos(m_position.y0); // y0 = latitude
            double sa = Math.Sin(m_position.y0);

            Vector3d po = new Vector3d(co * ca, so * ca, sa) * Radius;
            Vector3d px = new Vector3d(-so, co, 0.0);
            Vector3d py = new Vector3d(-co * sa, -so * sa, ca);
            Vector3d pd = (po - px * Math.Sin(m_position.phi) * distance + py * Math.Cos(m_position.phi) * distance).Normalized();

            m_position.x0 = Math.Atan2(pd.y, pd.x);
            m_position.y0 = MathUtility.Safe_Asin(pd.z);
        }

        public override void Turn(double angle)
        {
            m_position.phi += angle;
        }

        public override double Interpolate(double sx0, double sy0, double stheta, double sphi, double sd, double dx0, double dy0, double dtheta, double dphi, double dd, double t)
        {
            Vector3d s = new Vector3d(Math.Cos(sx0) * Math.Cos(sy0), Math.Sin(sx0) * Math.Cos(sy0), Math.Sin(sy0));
            Vector3d e = new Vector3d(Math.Cos(dx0) * Math.Cos(dy0), Math.Sin(dx0) * Math.Cos(dy0), Math.Sin(dy0));
            double dist = Math.Max(MathUtility.Safe_Acos(s.Dot(e)) * Radius, 1e-3);

            t = Math.Min(t + Math.Min(0.1, 5000.0 / dist), 1.0);
            double T = 0.5 * Math.Atan(4.0 * (t - 0.5)) / Math.Atan(4.0 * 0.5) + 0.5;

            InterpolateDirection(sx0, sy0, dx0, dy0, T, ref m_position.x0, ref m_position.y0);
            InterpolateDirection(sphi, stheta, dphi, dtheta, T, ref m_position.phi, ref m_position.theta);

            double W = 10.0;
            m_position.distance = sd * (1.0 - t) + dd * t + dist * (Math.Exp(-W * (t - 0.5) * (t - 0.5)) - Math.Exp(-W * 0.25));

            return t;
        }

        public override void InterpolatePos(double sx0, double sy0, double dx0, double dy0, double t, ref double x0, ref double y0)
        {
            InterpolateDirection(sx0, sy0, dx0, dy0, t, ref x0, ref y0);
        }
    }
}