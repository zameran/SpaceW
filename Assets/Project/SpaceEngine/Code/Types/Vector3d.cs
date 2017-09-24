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

using System;

namespace UnityEngine
{
    [Serializable]
    public struct Vector3d : IEquatable<Vector3d>
    {
        #region Fields

        public double x;
        public double y;
        public double z;

        #endregion

        #region Properties

        public static Vector3d zero { get { return new Vector3d(0.0); } }

        public static Vector3d one { get { return new Vector3d(1.0); } }

        public static Vector3d forward { get { return new Vector3d(0.0, 0.0, 1.0); } }

        public static Vector3d back { get { return new Vector3d(0.0, 0.0, -1.0); } }

        public static Vector3d up { get { return new Vector3d(0.0, 1.0, 0.0); } }

        public static Vector3d down { get { return new Vector3d(0.0, -1.0, 0.0); } }

        public static Vector3d left { get { return new Vector3d(-1.0, 0.0, 0.0); } }

        public static Vector3d right { get { return new Vector3d(1.0, 0.0, 0.0); } }

        public Vector3d xzy { get { return new Vector3d(x, z, y); } }

        public Vector2d xy { get { return new Vector2d(x, y); } }

        public Vector3d xy0 { get { return new Vector3d(x, y, 0.0); } }

        #endregion

        #region Constructors

        public Vector3d(double value)
        {
            this.x = value;
            this.y = value;
            this.z = value;
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

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() << 2 + z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3d))
                return false;

            var vector = (Vector3d)obj;

            return this == vector;
        }

        #endregion

        #region IEquatable<Vector3d>

        public bool Equals(Vector3d other)
        {
            return this == other;
        }

        #endregion

        #region Operations

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
            return !BrainFuckMath.NearlyEqual(v1.x, v2.x) || !BrainFuckMath.NearlyEqual(v1.y, v2.y) || !BrainFuckMath.NearlyEqual(v1.z, v2.z);
        }

        public static bool operator ==(Vector3d v1, Vector3d v2)
        {
            return BrainFuckMath.NearlyEqual(v1.x, v2.x) && BrainFuckMath.NearlyEqual(v1.y, v2.y) && BrainFuckMath.NearlyEqual(v1.z, v2.z);
        }

        public static implicit operator Vector3(Vector3d v)
        {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }

        public static implicit operator Vector3d(Vector3 v)
        {
            return new Vector3d((double)v.x, (double)v.y, (double)v.z);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        #endregion

        #region ConvertTo

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
        }

        #endregion

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

        public Vector3d Normalized()
        {
            var invLength = 1.0 / Magnitude();

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Normalized(double l)
        {
            var invLength = l / Magnitude();

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Normalized(out double previousLength)
        {
            previousLength = Magnitude();

            var invLength = 1.0 / previousLength;

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public static Vector3d Exclude(Vector3d excludeThis, Vector3d fromThat)
        {
            return fromThat - Project(fromThat, excludeThis);
        }

        public static Vector3d Project(Vector3d vector, Vector3d onNormal)
        {
            var dot = Dot(onNormal, onNormal);

            return (dot >= 1.40129846432482E-45 ? (onNormal * Dot(vector, onNormal)) / dot : zero);
        }

        public static Vector3d Projection(Vector3d a, Vector3d b)
        {
            return ((Dot(a, b) / Dot(b, b)) * b);
        }

        public static double Distance(Vector3d a, Vector3d b)
        {
            var v = new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);

            return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public static Vector3d Rejection(Vector3d a, Vector3d b)
        {
            return (a - Projection(a, b));
        }

        public Vector3d Cross(Vector3d v)
        {
            return new Vector3d(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
        }

        public static Vector3d Cross(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z, lhs.x * rhs.y - lhs.y * rhs.x);
        }

        public static Vector3d Lerp(Vector3d from, Vector3d to, double t)
        {
            return new Vector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
        }
    }
}