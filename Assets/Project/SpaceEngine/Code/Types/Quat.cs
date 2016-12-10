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
// Modified by Denis Ovchinnikov 2015-2016
#endregion

namespace UnityEngine
{
    [System.Serializable]
    public struct Quat
    {
        public double x, y, z, w;

        public Quat(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quat(double[] v)
        {
            this.x = v[0];
            this.y = v[1];
            this.z = v[2];
            this.w = v[3];
        }

        public Quat(Quat q)
        {
            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public Quat(Quaternion q)
        {
            this.x = (double)q.x;
            this.y = (double)q.y;
            this.z = (double)q.z;
            this.w = (double)q.w;
        }

        public Quat(Vector3d axis, double angle)
        {
            var axisN = axis.Normalized();

            var a = angle * 0.5;
            var sina = System.Math.Sin(a);
            var cosa = System.Math.Cos(a);

            x = axisN.x * sina;
            y = axisN.y * sina;
            z = axisN.z * sina;
            w = cosa;
        }

        public Quat(Vector3 axis, float angle)
        {
            var axisN = axis.normalized;

            var a = angle * 0.5f;
            var sina = Mathf.Sin(a);
            var cosa = Mathf.Cos(a);

            x = (double)(axisN.x * sina);
            y = (double)(axisN.y * sina);
            z = (double)(axisN.z * sina);
            w = (double)cosa;
        }

        public Quat(Vector3d to, Vector3d from)
        {
            var f = from.Normalized();
            var t = to.Normalized();

            var dotProdPlus1 = 1.0 + f.Dot(t);

            if (dotProdPlus1 < 1e-7)
            {
                w = 0;

                if (System.Math.Abs(f.x) < 0.6)
                {
                    var norm = System.Math.Sqrt(1 - f.x * f.x);
                    x = 0;
                    y = f.z / norm;
                    z = -f.y / norm;
                }
                else if (System.Math.Abs(f.y) < 0.6)
                {
                    var norm = System.Math.Sqrt(1 - f.y * f.y);
                    x = -f.z / norm;
                    y = 0;
                    z = f.x / norm;
                }
                else
                {
                    var norm = System.Math.Sqrt(1 - f.z * f.z);
                    x = f.y / norm;
                    y = -f.x / norm;
                    z = 0;
                }
            }
            else
            {
                var s = System.Math.Sqrt(0.5 * dotProdPlus1);
                var tmp = (f.Cross(t)) / (2.0 * s);

                x = tmp.x;
                y = tmp.y;
                z = tmp.z;
                w = s;
            }
        }

        public static Quat AngleAxis(double angle, Vector3d axis)
        {
            double x;
            double y;
            double z;
            double w;

            var magnitude = axis.magnitude;

            if (magnitude <= 0.0001)
            {
                w = 1;
                x = 0;
                y = 0;
                z = 0;
            }
            else
            {
                var cosa = System.Math.Cos(angle * MathUtility.Deg2Rad / 2);
                var sina = System.Math.Sin(angle * MathUtility.Deg2Rad / 2);

                x = axis.x / magnitude * sina;
                y = axis.y / magnitude * sina;
                z = axis.z / magnitude * sina;
                w = cosa;
            }
            return new Quat(x, y, z, w);
        }

        public static Quat operator *(Quat q1, Quat q2)
        {
            return new Quat(q2.w * q1.x + q2.x * q1.w + q2.y * q1.z - q2.z * q1.y, q2.w * q1.y - q2.x * q1.z + q2.y * q1.w + q2.z * q1.x,
                q2.w * q1.z + q2.x * q1.y - q2.y * q1.x + q2.z * q1.w, q2.w * q1.w - q2.x * q1.x - q2.y * q1.y - q2.z * q1.z);
        }

        public static Vector3d operator *(Quat q, Vector3d v)
        {
            return q.ToMatrix3x3d() * v;
        }

        public static implicit operator Quaternion(Quat q)
        {
            return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }

        public static implicit operator Quat(Quaternion q)
        {
            return new Quat(q.x, q.y, q.z, q.w);
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

            return new Matrix3x3(1.0f - 2.0f * (yy + zz), 2.0f * (xy - zw), 2.0f * (xz + yw), 2.0f * (xy + zw), 1.0f - 2.0f * (xx + zz), 2.0f * (yz - xw), 2.0f * (xz - yw),
                2.0f * (yz + xw), 1.0f - 2.0f * (xx + yy));
        }

        public Matrix3x3d ToMatrix3x3d()
        {
            double xx = x * x, xy = x * y, xz = x * z, xw = x * w, yy = y * y, yz = y * z, yw = y * w, zz = z * z, zw = z * w;

            return new Matrix3x3d(1.0 - 2.0 * (yy + zz), 2.0 * (xy - zw), 2.0 * (xz + yw), 2.0 * (xy + zw), 1.0 - 2.0 * (xx + zz), 2.0 * (yz - xw), 2.0 * (xz - yw),
                2.0 * (yz + xw), 1.0 - 2.0 * (xx + yy));
        }

        public Matrix4x4d ToMatrix4x4d()
        {
            double xx = x * x, xy = x * y, xz = x * z, xw = x * w, yy = y * y, yz = y * z, yw = y * w, zz = z * z, zw = z * w;

            return new Matrix4x4d(1.0 - 2.0 * (yy + zz), 2.0 * (xy - zw), 2.0 * (xz + yw), 0.0, 2.0 * (xy + zw), 1.0 - 2.0 * (xx + zz), 2.0 * (yz - xw), 0.0, 2.0 * (xz - yw),
                2.0 * (yz + xw), 1.0 - 2.0 * (xx + yy), 0.0, 0.0, 0.0, 0.0, 1.0);
        }

        public Quat Inverse()
        {
            return new Quat(-x, -y, -z, w);
        }

        private double Length()
        {
            return System.Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        public void Normalize()
        {
            var invLength = 1.0 / Length();

            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
        }

        public Quat Normalized()
        {
            var invLength = 1.0 / Length();

            return new Quat(x * invLength, y * invLength, z * invLength, w * invLength);
        }

        public Quat Slerp(Quat _from, Quat to, double t)
        {
            if (t <= 0)
            {
                return new Quat(_from);
            }
            else if (t >= 1)
            {
                return new Quat(to);
            }
            else
            {
                var cosom = _from.x * to.x + _from.y * to.y + _from.z * to.z + _from.w * to.w;
                var absCosom = System.Math.Abs(cosom);

                double scale0;
                double scale1;

                if ((1 - absCosom) > 1e-6)
                {
                    var omega = MathUtility.Safe_Acos(absCosom);
                    var sinom = 1.0 / System.Math.Sin(omega);

                    scale0 = System.Math.Sin((1.0 - t) * omega) * sinom;
                    scale1 = System.Math.Sin(t * omega) * sinom;
                }
                else
                {
                    scale0 = 1 - t;
                    scale1 = t;
                }

                return new Quat(scale0 * _from.x + scale1 * to.x, scale0 * _from.y + scale1 * to.y, scale0 * _from.z + scale1 * to.z, scale0 * _from.w + scale1 * to.w).Normalized();
            }
        }
    }
}