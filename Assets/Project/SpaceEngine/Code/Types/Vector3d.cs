#region License
//
// Procedural planet renderer.
// Copyright (c) 2008-2011 INRIA
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Proland is distributed under a dual-license scheme.
// You can obtain a specific license from Inria: proland-licensing@inria.fr.
//
// Authors: Justin Hawkins 2014.
// Modified by Denis Ovchinnikov 2015-2017
#endregion

namespace UnityEngine
{
    using System;

    [Serializable]
    public struct Vector3d
    {
        public double x, y, z;

        public Vector3d(double v)
        {
            this.x = v;
            this.y = v;
            this.z = v;
        }

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3d(Vector2d v, double z)
        {
            x = v.x;
            y = v.y;
            this.z = z;
        }

        public Vector3d(Vector3d v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3d(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3d(Vector2 v, double z)
        {
            x = v.x;
            y = v.y;
            this.z = z;
        }

        public Vector3d(double[] v)
        {
            x = v[0];
            y = v[1];
            z = v[2];
        }

        public static Vector3d operator +(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3d operator -(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3d operator -(Vector3d v1)
        {
            return new Vector3d(-v1.x, -v1.y, -v1.z);
        }

        public static Vector3d operator *(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        public static Vector3d operator *(Vector3d v, double s)
        {
            return new Vector3d(v.x * s, v.y * s, v.z * s);
        }

        public static Vector3d operator *(double s, Vector3d v)
        {
            return new Vector3d(v.x * s, v.y * s, v.z * s);
        }

        public static Vector3d operator /(Vector3d v, double s)
        {
            return new Vector3d(v.x / s, v.y / s, v.z / s);
        }

        public static Vector3d operator /(Vector3d v1, Vector3d v2)
        {
            return new Vector3d(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }

        public static bool operator !=(Vector3d v1, Vector3d v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        public static bool operator ==(Vector3d v1, Vector3d v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public static implicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d((double)v.x, (double)v.y, (double)v.z);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() << 2 + z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3d))
                return false;

            var Vector = (Vector3d)obj;

            return x == Vector.x && y == Vector.y && z == Vector.z;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }

        public double magnitude { get { return Magnitude(); } }

        public double sqrMagnitude { get { return SqrMagnitude(); } }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double SqrMagnitude()
        {
            return (x * x + y * y + z * z);
        }

        public double Dot(Vector3d v)
        {
            return (x * v.x + y * v.y + z * v.z);
        }

        public static double Dot(Vector3d lhs, Vector3d rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }

        public Vector3d normalized { get { return Normalize(this); } }

        public void Normalize()
        {
            double invLength = 1.0 / System.Math.Sqrt(x * x + y * y + z * z);
            x *= invLength;
            y *= invLength;
            z *= invLength;
        }

        public static Vector3d Exclude(Vector3d excludeThis, Vector3d fromThat)
        {
            return fromThat - Project(fromThat, excludeThis);
        }

        public static Vector3d Project(Vector3d vector, Vector3d onNormal)
        {
            double dot = Dot(onNormal, onNormal);

            return (dot >= 1.40129846432482E-45 ? (onNormal * Dot(vector, onNormal)) / dot : zero);
            ;
        }

        public static Vector3d Normalize(Vector3d value)
        {
            double magnitude = value.Magnitude();

            if (magnitude > 9.9999997473787516E-06) return value / magnitude;
            else return zero;
        }

        public static Vector3d Projection(Vector3d a, Vector3d b)
        {
            return ((Dot(a, b) / Dot(b, b)) * b);
        }

        public static double Distance(Vector3d a, Vector3d b)
        {
            Vector3d v = new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
            return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public Vector3d Rotate(double pointX, double pointY, double pointZ, double directionX, double directionY, double directionZ, double angle)
        {
            double length = 1.0 / Math.Sqrt(((directionX * directionX) + (directionY * directionY)) + (directionZ * directionZ));

            directionX *= length;
            directionY *= length;
            directionZ *= length;

            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);

            double x = ((((pointX * ((directionY * directionY) + (directionZ * directionZ))) -
                          (directionX * (((((pointY * directionY) + (pointZ * directionZ)) - (directionX * this.x)) - (directionY * this.y)) - (directionZ * this.z)))) *
                         (1.0 - cosa)) + (this.x * cosa)) + (((((-pointZ * directionY) + (pointY * directionZ)) - (directionZ * this.y)) + (directionY * this.z)) * sina);
            double y = ((((pointY * ((directionX * directionX) + (directionZ * directionZ))) -
                          (directionY * (((((pointX * directionX) + (pointZ * directionZ)) - (directionX * this.x)) - (directionY * this.y)) - (directionZ * this.z)))) *
                         (1.0 - cosa)) + (this.y * cosa)) + (((((pointZ * directionX) - (pointX * directionZ)) + (directionZ * this.x)) - (directionX * this.z)) * sina);

            return new Vector3d(x, y,
                ((((pointZ * ((directionX * directionX) + (directionY * directionY))) -
                   (directionZ * (((((pointX * directionX) + (pointY * directionY)) - (directionX * this.x)) - (directionY * this.y)) - (directionZ * this.z)))) * (1.0 - cosa)) +
                 (this.z * cosa)) + (((((-pointY * directionX) + (pointX * directionY)) - (directionY * this.x)) + (directionX * this.y)) * sina));
        }

        public Vector3d Rotate(Vector3d point, Vector3d direction, double angle)
        {
            return Rotate(point.x, point.y, point.z, direction.x, direction.y, direction.z, angle);
        }

        public static Vector3d Rejection(Vector3d a, Vector3d b)
        {
            return (a - Projection(a, b));
        }

        public Vector3d Normalized()
        {
            double invLength = 1.0 / Math.Sqrt(x * x + y * y + z * z);

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Normalized(double l)
        {
            double invLength = l / Math.Sqrt(x * x + y * y + z * z);
            ;
            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Cross(Vector3d v)
        {
            return new Vector3d(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
        }

        public static Vector3d Cross(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        /// <summary>
        /// Linear interpolation between two vectors.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="t">T. WARNING : Only 0 - 1 range!</param>
        /// <returns>Interpolated vector.</returns>
        public static Vector3d Lerp(Vector3d from, Vector3d to, double t)
        {
            return new Vector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
        }

        public static Vector3d Lerp01t(Vector3d from, Vector3d to, float t)
        {
            return Lerp(from, to, Mathf.Clamp01(t));
        }

        public Vector2d XY()
        {
            return new Vector2d(x, y);
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        public Vector3d Unit()
        {
            return (this / this.Magnitude());
        }

        public Vector3d Zero()
        {
            return new Vector3d(0.0, 0.0, 0.0);
        }

        public bool IsZero()
        {
            return this.Equals(new Vector3d(0.0, 0.0, 0.0));
        }

        public static Vector3d UnitX()
        {
            return new Vector3d(1, 0, 0);
        }

        public static Vector3d UnitY()
        {
            return new Vector3d(0, 1, 0);
        }

        public static Vector3d UnitZ()
        {
            return new Vector3d(0, 0, 1);
        }

        public static Vector3d zero { get { return new Vector3d(0.0, 0.0, 0.0); } }

        public static Vector3d one { get { return new Vector3d(1.0, 1.0, 1.0); } }

        public static Vector3d forward { get { return new Vector3d(0.0, 0.0, 1.0); } }

        public static Vector3d back { get { return new Vector3d(0.0, 0.0, -1.0); } }

        public static Vector3d up { get { return new Vector3d(0.0, 1.0, 0.0); } }

        public static Vector3d down { get { return new Vector3d(0.0, -1.0, 0.0); } }

        public static Vector3d left { get { return new Vector3d(-1.0, 0.0, 0.0); } }

        public static Vector3d right { get { return new Vector3d(1.0, 0.0, 0.0); } }

        public Vector3d xzy { get { return new Vector3d(this.x, this.z, this.y); } }
    }
}