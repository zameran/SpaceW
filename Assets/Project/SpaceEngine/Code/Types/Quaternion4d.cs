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

using Constants = SpaceEngine.Core.Numerics.Constants;
using Functions = SpaceEngine.Core.Numerics.Functions;

namespace UnityEngine
{
#pragma warning disable 660, 661

    [Serializable]
    public struct Quaternion4d : IEquatable<Quaternion4d>
    {
        #region Fields

        public double x;
        public double y;
        public double z;
        public double w;

        #endregion

        #region Properties

        public static Quaternion4d identity { get { return new Quaternion4d(0.0, 0.0, 0.0, 1.0); } }

        public static Quaternion4d zero { get { return new Quaternion4d(0.0, 0.0, 0.0, 0.0); } }

        public static Quaternion4d one { get { return new Quaternion4d(1.0, 1.0, 1.0, 1.0); } }

        #endregion

        #region Constructors

        public Quaternion4d(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion4d(double[] v)
        {
            x = v[0];
            y = v[1];
            z = v[2];
            w = v[3];
        }

        public Quaternion4d(Quaternion4d q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion4d(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion4d(Vector3d axis, double angle)
        {
            var axisN = axis.Normalized();

            var a = angle * 0.5;
            var sina = Math.Sin(a);
            var cosa = Math.Cos(a);

            x = axisN.x * sina;
            y = axisN.y * sina;
            z = axisN.z * sina;
            w = cosa;
        }

        public Quaternion4d(Vector3 axis, float angle)
        {
            var axisN = axis.normalized;

            var a = angle * 0.5f;
            var sina = Mathf.Sin(a);
            var cosa = Mathf.Cos(a);

            x = axisN.x * sina;
            y = axisN.y * sina;
            z = axisN.z * sina;
            w = cosa;
        }

        public Quaternion4d(Vector3d to, Vector3d from)
        {
            var f = from.Normalized();
            var t = to.Normalized();

            var dotProdPlus1 = 1.0 + f.Dot(t);

            if (dotProdPlus1 < 1e-7)
            {
                w = 0;

                if (Math.Abs(f.x) < 0.6)
                {
                    var norm = Math.Sqrt(1 - f.x * f.x);

                    x = 0;
                    y = f.z / norm;
                    z = -f.y / norm;
                }
                else if (Math.Abs(f.y) < 0.6)
                {
                    var norm = Math.Sqrt(1 - f.y * f.y);

                    x = -f.z / norm;
                    y = 0;
                    z = f.x / norm;
                }
                else
                {
                    var norm = Math.Sqrt(1 - f.z * f.z);

                    x = f.y / norm;
                    y = -f.x / norm;
                    z = 0;
                }
            }
            else
            {
                var s = Math.Sqrt(0.5 * dotProdPlus1);
                var tmp = (f.Cross(t)) / (2.0 * s);

                x = tmp.x;
                y = tmp.y;
                z = tmp.z;
                w = s;
            }
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Quaternion4d) { return Equals((Quaternion4d)obj); }

            return false;
        }

        public override int GetHashCode()
        {
            double hashcode = 23;

            hashcode = (hashcode * 37) + x;
            hashcode = (hashcode * 37) + y;
            hashcode = (hashcode * 37) + z;
            hashcode = (hashcode * 37) + w;

            return (int)hashcode;
        }

        #endregion

        #region IEquatable<Quaternion4d>

        public bool Equals(Quaternion4d other)
        {
            return this == other;
        }

        #endregion

        #region Operations

        public static Quaternion4d operator *(Quaternion4d q1, Quaternion4d q2)
        {
            return new Quaternion4d(q2.w * q1.x + q2.x * q1.w + q2.y * q1.z - q2.z * q1.y, q2.w * q1.y - q2.x * q1.z + q2.y * q1.w + q2.z * q1.x,
                                    q2.w * q1.z + q2.x * q1.y - q2.y * q1.x + q2.z * q1.w, q2.w * q1.w - q2.x * q1.x - q2.y * q1.y - q2.z * q1.z);
        }

        public static Vector3d operator *(Quaternion4d q, Vector3d v)
        {
            return q.ToMatrix3x3d() * v;
        }

        public static bool operator ==(Quaternion4d lhs, Quaternion4d rhs)
        {
            return Dot(lhs, rhs) > 0.999999f;
        }

        public static bool operator !=(Quaternion4d lhs, Quaternion4d rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator Quaternion(Quaternion4d q)
        {
            return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }

        public static implicit operator Quaternion4d(Quaternion q)
        {
            return new Quaternion4d(q.x, q.y, q.z, q.w);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        #endregion

        #region CovertTo

        public Matrix3x3d ToMatrix3x3d()
        {
            double xx = x * x,
                   xy = x * y,
                   xz = x * z,
                   xw = x * w,
                   yy = y * y,
                   yz = y * z,
                   yw = y * w,
                   zz = z * z,
                   zw = z * w;

            return new Matrix3x3d(1.0 - 2.0 * (yy + zz),
                                  2.0 * (xy - zw), 2.0 * (xz + yw),
                                  2.0 * (xy + zw), 1.0 - 2.0 * (xx + zz),
                                  2.0 * (yz - xw), 2.0 * (xz - yw),
                                  2.0 * (yz + xw), 1.0 - 2.0 * (xx + yy));
        }

        public Matrix3x3 ToMatrix3x3()
        {
            float xx = (float)(x * x),
                  xy = (float)(x * y),
                  xz = (float)(x * z),
                  xw = (float)(x * w),
                  yy = (float)(y * y),
                  yz = (float)(y * z),
                  yw = (float)(y * w),
                  zz = (float)(z * z),
                  zw = (float)(z * w);

            return new Matrix3x3(1.0f - 2.0f * (yy + zz),
                                 2.0f * (xy - zw), 2.0f * (xz + yw),
                                 2.0f * (xy + zw), 1.0f - 2.0f * (xx + zz),
                                 2.0f * (yz - xw), 2.0f * (xz - yw),
                                 2.0f * (yz + xw), 1.0f - 2.0f * (xx + yy));
        }

        public Matrix4x4d ToMatrix4x4d()
        {
            double xx = x * x,
                   xy = x * y,
                   xz = x * z,
                   xw = x * w,
                   yy = y * y,
                   yz = y * z,
                   yw = y * w,
                   zz = z * z,
                   zw = z * w;

            return new Matrix4x4d(1.0 - 2.0 * (yy + zz),
                                  2.0 * (xy - zw), 2.0 * (xz + yw),
                                  0.0, 2.0 * (xy + zw),
                                  1.0 - 2.0 * (xx + zz), 2.0 * (yz - xw),
                                  0.0, 2.0 * (xz - yw),
                                  2.0 * (yz + xw), 1.0 - 2.0 * (xx + yy),
                                  0.0, 0.0, 0.0, 0.0, 1.0);
        }

        public Matrix4x4d ToMatrix4x4()
        {
            float xx = (float)(x * x),
                  xy = (float)(x * y),
                  xz = (float)(x * z),
                  xw = (float)(x * w),
                  yy = (float)(y * y),
                  yz = (float)(y * z),
                  yw = (float)(y * w),
                  zz = (float)(z * z),
                  zw = (float)(z * w);

            return new Matrix4x4d(1.0 - 2.0 * (yy + zz),
                                  2.0 * (xy - zw), 2.0 * (xz + yw),
                                  0.0, 2.0 * (xy + zw),
                                  1.0 - 2.0 * (xx + zz), 2.0 * (yz - xw),
                                  0.0, 2.0 * (xz - yw),
                                  2.0 * (yz + xw), 1.0 - 2.0 * (xx + yy),
                                  0.0, 0.0, 0.0, 0.0, 1.0);
        }

        #endregion

        public static Quaternion4d AngleAxis(double angle, Vector3d axis)
        {
            double x;
            double y;
            double z;
            double w;

            var magnitude = axis.Magnitude();

            if (magnitude <= 0.0001)
            {
                w = 1;
                x = 0;
                y = 0;
                z = 0;
            }
            else
            {
                var cosa = Math.Cos(angle * Constants.Deg2Rad / 2);
                var sina = Math.Sin(angle * Constants.Deg2Rad / 2);

                x = axis.x / magnitude * sina;
                y = axis.y / magnitude * sina;
                z = axis.z / magnitude * sina;
                w = cosa;
            }

            return new Quaternion4d(x, y, z, w);
        }

        public static double Dot(Quaternion4d a, Quaternion4d b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public Quaternion4d Inverse()
        {
            return new Quaternion4d(-x, -y, -z, w);
        }

        private double Length()
        {
            return Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public void Normalize()
        {
            var invLength = 1.0 / Length();

            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
        }

        public Quaternion4d Normalized()
        {
            var invLength = 1.0 / Length();

            return new Quaternion4d(x * invLength, y * invLength, z * invLength, w * invLength);
        }

        public Quaternion4d Slerp(Quaternion4d @from, Quaternion4d to, double t)
        {
            if (t <= 0)
            {
                return new Quaternion4d(@from);
            }
            else if (t >= 1)
            {
                return new Quaternion4d(to);
            }
            else
            {
                var cosom = @from.x * to.x + @from.y * to.y + @from.z * to.z + @from.w * to.w;
                var absCosom = Math.Abs(cosom);

                double scale0;
                double scale1;

                if ((1 - absCosom) > 1e-6)
                {
                    var omega = Functions.Safe_Acos(absCosom);
                    var sinom = 1.0 / Math.Sin(omega);

                    scale0 = Math.Sin((1.0 - t) * omega) * sinom;
                    scale1 = Math.Sin(t * omega) * sinom;
                }
                else
                {
                    scale0 = 1 - t;
                    scale1 = t;
                }

                return new Quaternion4d(scale0 * @from.x + scale1 * to.x, 
                                        scale0 * @from.y + scale1 * to.y, 
                                        scale0 * @from.z + scale1 * to.z, 
                                        scale0 * @from.w + scale1 * to.w).Normalized();
            }
        }
    }
}