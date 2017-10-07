using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Vectors;

using System;

using UnityEngine;

using Functions = SpaceEngine.Core.Numerics.Functions;

namespace SpaceEngine.Core.Utilities
{
    /// <summary>
    /// A <see cref="TerrainView"/> for spherical terrains. 
    /// This subclass interprets the <see cref="TerrainView.Position.X"/> and <see cref="TerrainView.Position.Y"/> fields as longitudes and latitudes on the planet,
    /// and considers <see cref="TerrainView.Position.Theta"/> and <see cref="TerrainView.Position.Phi"/> as relative to the tangent plane at the point.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlanetView : TerrainView
    {
        public double Radius { get; set; }

        public Vector3d Origin { get { return GodManager.Instance.ActiveBody.Origin; } }

        public override double GetHeight()
        {
            return (worldPosition - Origin).Magnitude() - Radius;
        }

        public override Vector3d GetLookAtPosition()
        {
            // NOTE : co - x; so - y; ca - z; sa - w;
            var oa = CalculatelongitudeLatitudeVector(position.X, position.Y);

            return new Vector3d(oa.x * oa.z, oa.y * oa.z, oa.w) * Radius;
        }

        public override void Constrain()
        {
            position.Y = Math.Max(-Math.PI / 2.0, Math.Min(Math.PI / 2.0, position.Y));
            position.Theta = Math.Max(0.1, Math.Min(Math.PI, position.Theta));
            position.Distance = Math.Max(0.1, position.Distance);
        }

        protected override void Start()
        {
            base.Start();

            Constrain();
        }

        protected override void SetWorldToCameraMatrix()
        {
            // NOTE : co - x; so - y; ca - z; sa - w;
            var oa = CalculatelongitudeLatitudeVector(position.X, position.Y);

            var po = new Vector3d(oa.x * oa.z, oa.y * oa.z, oa.w) * Radius;
            var px = new Vector3d(-oa.y, oa.x, 0.0);
            var py = new Vector3d(-oa.x * oa.w, -oa.y * oa.w, oa.z);
            var pz = new Vector3d(oa.x * oa.z, oa.y * oa.z, oa.w);

            // NOTE : ct - x; st - y; cp - z; sp - w;
            var tp = CalculatelongitudeLatitudeVector(position.Theta, position.Phi);

            Vector3d cx = px * tp.z + py * tp.w;
            Vector3d cy = (px * -1.0) * tp.w * tp.x + py * tp.z * tp.x + pz * tp.y;
            Vector3d cz = px * tp.w * tp.y - py * tp.z * tp.y + pz * tp.x;

            worldPosition = po + cz * position.Distance;

            if (worldPosition.Magnitude() < Radius + 10.0 + GroundHeight)
            {
                worldPosition = worldPosition.Normalized(Radius + 10.0 + GroundHeight);
            }

            worldPosition = worldPosition + Origin;

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

        public override void Move(Vector3d oldp, Vector3d p, double speed)
        {
            var oldPosition = oldp.Normalized();
            var position = p.Normalized();

            var oldlat = Functions.Safe_Asin(oldPosition.z);
            var oldlon = Math.Atan2(oldPosition.y, oldPosition.x);
            var lat = Functions.Safe_Asin(position.z);
            var lon = Math.Atan2(position.y, position.x);

            base.position.X -= (lon - oldlon) * speed * Math.Max(1.0, worldPosition.Magnitude() - Radius);
            base.position.Y -= (lat - oldlat) * speed * Math.Max(1.0, worldPosition.Magnitude() - Radius);
        }

        public override void MoveForward(double distance)
        {
            // NOTE : co - x; so - y; ca - z; sa - w;
            var oa = CalculatelongitudeLatitudeVector(position.X, position.Y);

            var po = new Vector3d(oa.x * oa.z, oa.y * oa.z, oa.w) * Radius;
            var px = new Vector3d(-oa.y, oa.x, 0.0);
            var py = new Vector3d(-oa.x * oa.w, -oa.y * oa.w, oa.z);
            var pd = (po - px * Math.Sin(position.Phi) * distance + py * Math.Cos(position.Phi) * distance).Normalized();

            position.X = Math.Atan2(pd.y, pd.x);
            position.Y = Functions.Safe_Asin(pd.z);
        }

        public override void Turn(double angle)
        {
            position.Phi += angle;
        }

        public override double Interpolate(Position from, Position to, double t)
        {
            var s = new Vector3d(Math.Cos(from.X) * Math.Cos(from.Y), Math.Sin(from.X) * Math.Cos(from.Y), Math.Sin(from.Y));
            var e = new Vector3d(Math.Cos(to.X) * Math.Cos(to.Y), Math.Sin(to.X) * Math.Cos(to.Y), Math.Sin(to.Y));
            var distance = Math.Max(Functions.Safe_Acos(s.Dot(e)) * Radius, 1e-3);

            t = Math.Min(t + Math.Min(0.1, 5000.0 / distance), 1.0);

            var T = 0.5 * Math.Atan(4.0 * (t - 0.5)) / Math.Atan(4.0 * 0.5) + 0.5;
            var W = 10.0;

            InterpolateDirection(from.X, from.Y, to.X, to.Y, T, ref position.X, ref position.Y);
            InterpolateDirection(from.Phi, from.Theta, to.Phi, to.Theta, T, ref position.Phi, ref position.Theta);

            position.Distance = from.Distance * (1.0 - t) + to.Distance * t + distance * (Math.Exp(-W * (t - 0.5) * (t - 0.5)) - Math.Exp(-W * 0.25));

            return t;
        }

        public override void InterpolatePos(Position from, Position to, double t, ref Position current)
        {
            InterpolateDirection(from.X, from.Y, to.X, to.Y, t, ref current.X, ref current.Y);
        }
    }
}