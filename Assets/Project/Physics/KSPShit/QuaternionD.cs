namespace UnityEngine
{
    using System;
    using System.Runtime.CompilerServices;

    [Serializable]
    public struct QuaternionD
    {
        public const double kEpsilon = 1E-06;

        public double x;

        public double y;

        public double z;

        public double w;

        public double this[int index]
        {
            get
            {
                double result;
                switch (index)
                {
                    case 0:
                        result = this.x;
                        break;
                    case 1:
                        result = this.y;
                        break;
                    case 2:
                        result = this.z;
                        break;
                    case 3:
                        result = this.w;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid QuaternionD index!");
                }
                return result;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid QuaternionD index!");
                }
            }
        }

        public static QuaternionD identity
        {
            get
            {
                return new QuaternionD(0.0, 0.0, 0.0, 1.0);
            }
        }

        public Vector3d eulerAngles
        {
            get
            {
                return QuaternionD.Internal_ToEulerRad(this) * 57.29578;
            }
            set
            {
                this = QuaternionD.Internal_FromEulerRad(value * 0.0174532924);
            }
        }

        public static implicit operator Quaternion(QuaternionD q)
        {
            return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }

        public static implicit operator QuaternionD(Quaternion q)
        {
            return new QuaternionD((double)q.x, (double)q.y, (double)q.z, (double)q.w);
        }

        public QuaternionD(double x, double y, double z, double w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static double Dot(QuaternionD a, QuaternionD b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public static QuaternionD AngleAxis(double angle, Vector3d axis)
        {
            double magnitude = axis.magnitude;

            double x;
            double y;
            double z;
            double w;

            if (magnitude > 0.0001)
            {
                double cosa = Math.Cos(angle * 0.017453292519943295 / 2.0);
                double sina = Math.Sin(angle * 0.017453292519943295 / 2.0);

                x = axis.x / magnitude * sina;
                y = axis.y / magnitude * sina;
                z = axis.z / magnitude * sina;
                w = cosa;
            }
            else
            {
                w = 1.0;
                x = 0.0;
                y = 0.0;
                z = 0.0;
            }

            return new QuaternionD(x, y, z, w);
        }

        public static QuaternionD FromToRotation(Vector3d fromDirection, Vector3d toDirection)
        {
            return QuaternionD.INTERNAL_CALL_FromToRotation(ref fromDirection, ref toDirection);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_FromToRotation(ref Vector3d fromDirection, ref Vector3d toDirection);

        public void SetFromToRotation(Vector3d fromDirection, Vector3d toDirection)
        {
            this = QuaternionD.FromToRotation(fromDirection, toDirection);
        }

        public static QuaternionD LookRotation(Vector3d forward, Vector3d upwards)
        {
            return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref upwards);
        }

        public static QuaternionD LookRotation(Vector3d forward)
        {
            Vector3d up = Vector3d.up;
            return QuaternionD.INTERNAL_CALL_LookRotation(ref forward, ref up);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_LookRotation(ref Vector3d forward, ref Vector3d upwards);

        public void SetLookRotation(Vector3d view)
        {
            Vector3d up = Vector3d.up;
            this.SetLookRotation(view, up);
        }

        public void SetLookRotation(Vector3d view, Vector3d up)
        {
            this = QuaternionD.LookRotation(view, up);
        }

        public static QuaternionD Slerp(QuaternionD from, QuaternionD to, double t)
        {
            return QuaternionD.INTERNAL_CALL_Slerp(ref from, ref to, t);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_Slerp(ref QuaternionD from, ref QuaternionD to, double t);

        public static QuaternionD Lerp(QuaternionD from, QuaternionD to, double t)
        {
            return QuaternionD.INTERNAL_CALL_Lerp(ref from, ref to, t);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_Lerp(ref QuaternionD from, ref QuaternionD to, double t);

        public static QuaternionD RotateTowards(QuaternionD from, QuaternionD to, double maxDegreesDelta)
        {
            double t = Math.Min(1.0, maxDegreesDelta / QuaternionD.Angle(from, to));
            return QuaternionD.UnclampedSlerp(from, to, t);
        }

        private static QuaternionD UnclampedSlerp(QuaternionD from, QuaternionD to, double t)
        {
            return QuaternionD.INTERNAL_CALL_UnclampedSlerp(ref from, ref to, t);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_UnclampedSlerp(ref QuaternionD from, ref QuaternionD to, double t);

        public static QuaternionD Inverse(QuaternionD q)
        {
            return new QuaternionD(-q.x, -q.y, -q.z, q.w);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_Inverse(ref QuaternionD rotation);

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
            {
                this.x,
                this.y,
                this.z,
                this.w
            });
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", new object[]
            {
                this.x.ToString(format),
                this.y.ToString(format),
                this.z.ToString(format),
                this.w.ToString(format)
            });
        }

        public static double Angle(QuaternionD a, QuaternionD b)
        {
            double value = QuaternionD.Dot(a, b);
            return Math.Acos(Math.Min(Math.Abs(value), 1.0)) * 2.0 * 57.29578;
        }

        public static QuaternionD Euler(double x, double y, double z)
        {
            return QuaternionD.Internal_FromEulerRad(new Vector3d(x, y, z) * 0.0174532924);
        }

        public static QuaternionD Euler(Vector3d euler)
        {
            return QuaternionD.Internal_FromEulerRad(euler * 0.0174532924);
        }

        private static Vector3d Internal_ToEulerRad(QuaternionD rotation)
        {
            return QuaternionD.INTERNAL_CALL_Internal_ToEulerRad(ref rotation);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Vector3d INTERNAL_CALL_Internal_ToEulerRad(ref QuaternionD rotation);

        private static QuaternionD Internal_FromEulerRad(Vector3d euler)
        {
            return QuaternionD.INTERNAL_CALL_Internal_FromEulerRad(ref euler);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_Internal_FromEulerRad(ref Vector3d euler);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void INTERNAL_CALL_Internal_ToAxisAngleRad(ref QuaternionD q, out Vector3d axis, out double angle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern QuaternionD INTERNAL_CALL_AxisAngle(ref Vector3d axis, double angle);

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }

        public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs)
        {
            return new QuaternionD(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static Vector3d operator *(QuaternionD rotation, Vector3d point)
        {
            double x = rotation.x * 2.0;
            double y = rotation.y * 2.0;
            double z = rotation.z * 2.0;
            double xx = rotation.x * x;
            double yy = rotation.y * y;
            double zz = rotation.z * z;
            double xy = rotation.x * y;
            double xz = rotation.x * z;
            double yz = rotation.y * z;
            double wx = rotation.w * x;
            double wy = rotation.w * y;
            double wz = rotation.w * z;

            Vector3d result = Vector3d.zero;

            result.x = (1.0 - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z;
            result.y = (xy + wz) * point.x + (1.0 - (xx + zz)) * point.y + (yz - wx) * point.z;
            result.z = (xz - wy) * point.x + (yz + wx) * point.y + (1.0 - (xx + yy)) * point.z;

            return result;
        }
    }
}