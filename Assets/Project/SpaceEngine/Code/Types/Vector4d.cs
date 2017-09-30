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
    public struct Vector4d : IEquatable<Vector4d>
    {
        #region Fields

        public double x;
        public double y;
        public double z;
        public double w;

        #endregion

        #region Properties

        public static Vector4d zero { get { return new Vector4d(0.0); } }

        public static Vector4d one { get { return new Vector4d(1.0); } }

        public Vector3d xyz { get { return new Vector3d(x, y, z); } }

        public Vector4d xyz0 { get { return new Vector4d(x, y, z, 0.0); } }

        public Vector2d xy { get { return new Vector2d(x, y); } }

        #endregion

        #region Constructors

        public Vector4d(double value)
        {
            x = value;
            y = value;
            z = value;
            w = value;
        }

        public Vector4d(double x, double y)
        {
            this.x = x;
            this.y = y;
            z = 0.0;
            w = 0.0;
        }

        public Vector4d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            w = 0.0;
        }

        public Vector4d(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4d(Vector2d v, double z, double w)
        {
            x = v.x;
            y = v.y;
            this.z = z;
            this.w = w;
        }

        public Vector4d(Vector2 v, double z, double w)
        {
            x = v.x;
            y = v.y;
            this.z = z;
            this.w = w;
        }

        public Vector4d(Vector3d v, double w)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.w = w;
        }

        public Vector4d(Vector3 v, double w)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            this.w = w;
        }

        public Vector4d(Vector4d v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public Vector4d(Vector4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            double hashcode = 23;

            hashcode = (hashcode * 37) + x;
            hashcode = (hashcode * 37) + y;
            hashcode = (hashcode * 37) + z;
            hashcode = (hashcode * 37) + w;

            return (int)hashcode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4d) { return Equals((Vector4d)obj); }

            return false;
        }

        #endregion

        #region IEquatable<Vector4d>

        public bool Equals(Vector4d other)
        {
            return this == other;
        }

        #endregion

        #region Operations

        public static Vector4d operator +(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        public static Vector4d operator -(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
        }

        public static Vector4d operator -(Vector4d v1)
        {
            return new Vector4d(-v1.x, -v1.y, -v1.z, -v1.w);
        }

        public static Vector4d operator *(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
        }

        public static Vector4d operator *(Vector4d v, double s)
        {
            return new Vector4d(v.x * s, v.y * s, v.z * s, v.w * s);
        }

        public static Vector4d operator *(double s, Vector4d v)
        {
            return new Vector4d(v.x * s, v.y * s, v.z * s, v.w * s);
        }

        public static Vector4d operator /(Vector4d v, double s)
        {
            return new Vector4d(v.x / s, v.y / s, v.z / s, v.w / s);
        }

        public static Vector4d operator /(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z, v1.w / v2.w);
        }

        public static bool operator !=(Vector4d v1, Vector4d v2)
        {
            return !BrainFuckMath.NearlyEqual(v1.x, v2.x) || !BrainFuckMath.NearlyEqual(v1.y, v2.y) || !BrainFuckMath.NearlyEqual(v1.z, v2.z) || !BrainFuckMath.NearlyEqual(v1.w, v2.w);
        }

        public static bool operator ==(Vector4d v1, Vector4d v2)
        {
            return BrainFuckMath.NearlyEqual(v1.x, v2.x) && BrainFuckMath.NearlyEqual(v1.y, v2.y) && BrainFuckMath.NearlyEqual(v1.z, v2.z) && BrainFuckMath.NearlyEqual(v1.w, v2.w);
        }

        public static implicit operator Vector4(Vector4d v)
        {
            return new Vector4((float)v.x, (float)v.y, (float)v.z);
        }

        public static implicit operator Vector4d(Vector4 v)
        {
            return new Vector4d((double)v.x, (double)v.y, (double)v.z, (double)v.w);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        #endregion

        #region ConvertTo

        public Vector4 ToVector4()
        {
            return new Vector4((float)x, (float)y, (float)z, (float)w);
        }

        #endregion

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public double SqrMagnitude()
        {
            return (x * x + y * y + z * z + w * w);
        }

        public double Dot(Vector3d v)
        {
            return (x * v.x + y * v.y + z * v.z + w);
        }

        public double Dot(Vector4d v)
        {
            return (x * v.x + y * v.y + z * v.z + w * v.w);
        }

        public void Normalize()
        {
            var invLength = 1.0 / Math.Sqrt(x * x + y * y + z * z + w * w);

            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
        }

        public Vector4d Normalized()
        {
            var invLength = 1.0 / Math.Sqrt(x * x + y * y + z * z + w * w);

            return new Vector4d(x * invLength, y * invLength, z * invLength, w * invLength);
        }
    }
}