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
    public struct Vector2d : IEquatable<Vector2d>
    {
        #region Fields

        public double x;
        public double y;

        #endregion

        #region Properties

        public static Vector2d zero { get { return new Vector2d(0.0); } }

        public static Vector2d one { get { return new Vector2d(1.0); } }

        #endregion

        #region Constructors

        public Vector2d(double value)
        {
            this.x = value;
            this.y = value;
        }

        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2d(Vector2d v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2d(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2d(Vector3d v)
        {
            x = v.x;
            y = v.y;
        }

        public Vector2d(Vector3 v)
        {
            x = v.x;
            y = v.y;
        }


        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            double hashcode = 23;

            hashcode = (hashcode * 37) + x;
            hashcode = (hashcode * 37) + y;

            return (int)hashcode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2d) { return Equals((Vector2d)obj); }

            return false;
        }

        #endregion

        #region IEquatable<Vector2d>

        public bool Equals(Vector2d other)
        {
            return this == other;
        }

        #endregion

        #region Operations

        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2d operator -(Vector2d v1)
        {
            return new Vector2d(-v1.x, -v1.y);
        }

        public static Vector2d operator *(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x * v2.x, v1.y * v2.y);
        }

        public static Vector2d operator *(Vector2d v, double s)
        {
            return new Vector2d(v.x * s, v.y * s);
        }

        public static Vector2d operator *(double s, Vector2d v)
        {
            return new Vector2d(v.x * s, v.y * s);
        }

        public static Vector2d operator /(Vector2d v, double s)
        {
            return new Vector2d(v.x / s, v.y / s);
        }

        public static Vector2d operator /(Vector2d v1, Vector2d v2)
        {
            return new Vector2d(v1.x / v2.x, v1.y / v2.y);
        }

        public static bool operator !=(Vector2d v1, Vector2d v2)
        {
            return !BrainFuckMath.NearlyEqual(v1.x, v2.x) || !BrainFuckMath.NearlyEqual(v1.y, v2.y);
        }

        public static bool operator ==(Vector2d v1, Vector2d v2)
        {
            return BrainFuckMath.NearlyEqual(v1.x, v2.x) && BrainFuckMath.NearlyEqual(v1.y, v2.y);
        }

        public static implicit operator Vector2(Vector2d v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        public static implicit operator Vector2d(Vector2 v)
        {
            return new Vector2d((double)v.x, (double)v.y);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        #endregion

        #region ConvertTo

        public Vector2 ToVector2()
        {
            return new Vector2((float)x, (float)y);
        }

        #endregion

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double SqrMagnitude()
        {
            return (x * x + y * y);
        }

        public double Dot(Vector2d v)
        {
            return (x * v.x + y * v.y);
        }

        public void Normalize()
        {
            var invLength = 1.0 / Math.Sqrt(x * x + y * y);

            x *= invLength;
            y *= invLength;
        }

        public Vector2d Normalized()
        {
            var invLength = 1.0 / Math.Sqrt(x * x + y * y);

            return new Vector2d(x * invLength, y * invLength);
        }

        public Vector2d Normalized(double l)
        {
            var length = Math.Sqrt(x * x + y * y);
            var invLength = l / length;

            return new Vector2d(x * invLength, y * invLength);
        }

        public double Cross(Vector2d v)
        {
            return x * v.y - y * v.x;
        }

        public static double Cross(Vector2d u, Vector2d v)
        {
            return u.x * v.y - u.y * v.x;
        }
    }
}