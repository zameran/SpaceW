#region License
/*
 * Procedural planet renderer.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Justin Hawkins 2014.
 * Modified by Denis Ovchinnikov 2015-2016
 */
#endregion

namespace UnityEngine
{
    public class Vector4d
    {
        public double x, y, z, w;

        public Vector4d()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vector4d(double v)
        {
            this.x = v;
            this.y = v;
            this.z = v;
            this.w = v;
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

        public Vector4d(Vector3d v, double w)
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
            x = (double)v.x;
            y = (double)v.y;
            z = (double)v.z;
            w = (double)v.w;
        }

        public Vector4d(Vector3 v, double w)
        {
            x = (double)v.x;
            y = (double)v.y;
            z = (double)v.z;
            this.w = w;
        }

        public Vector4d(double[] v)
        {
            x = v[0];
            y = v[1];
            z = v[2];
            w = v[3];
        }

        public static Vector4d operator +(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        public static Vector4d operator -(Vector4d v1, Vector4d v2)
        {
            return new Vector4d(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
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

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + "," + w + ")";
        }

        public Vector4 ToVector4()
        {
            return new Vector4((float)x, (float)y, (float)z, (float)w);
        }

        public Vector3d XYZ()
        {
            return new Vector3d(x, y, z);
        }

        public Vector4d XYZ0()
        {
            return new Vector4d(x, y, z, 0);
        }

        public double Magnitude()
        {
            return System.Math.Sqrt(x * x + y * y + z * z + w * w);
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
            double invLength = 1.0 / System.Math.Sqrt(x * x + y * y + z * z + w * w);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
        }

        public Vector4d Normalized()
        {
            double invLength = 1.0 / System.Math.Sqrt(x * x + y * y + z * z + w * w);
            return new Vector4d(x * invLength, y * invLength, z * invLength, w * invLength);
        }

        public static Vector4d UnitX()
        {
            return new Vector4d(1, 0, 0, 0);
        }

        public static Vector4d UnitY()
        {
            return new Vector4d(0, 1, 0, 0);
        }

        public static Vector4d UnitZ()
        {
            return new Vector4d(0, 0, 1, 0);
        }

        public static Vector4d UnitW()
        {
            return new Vector4d(0, 0, 0, 1);
        }

        public static Vector4d Zero()
        {
            return new Vector4d(0, 0, 0, 0);
        }
    } 
}