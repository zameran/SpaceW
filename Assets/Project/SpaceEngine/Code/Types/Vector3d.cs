namespace UnityEngine
{
    public class Vector3d
    {
        public double x, y, z;

        public Vector3d()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

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

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")";
        }

        public double Magnitude()
        {
            return System.Math.Sqrt(x * x + y * y + z * z);
        }

        public double SqrMagnitude()
        {
            return (x * x + y * y + z * z);
        }

        public double Dot(Vector3d v)
        {
            return (x * v.x + y * v.y + z * v.z);
        }

        public void Normalize()
        {
            double invLength = 1.0 / System.Math.Sqrt(x * x + y * y + z * z);
            x *= invLength;
            y *= invLength;
            z *= invLength;
        }

        public Vector3d Normalized()
        {
            double invLength = 1.0 / System.Math.Sqrt(x * x + y * y + z * z);

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Normalized(double l)
        {
            double length = System.Math.Sqrt(x * x + y * y + z * z);
            double invLength = l / length;

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Normalized(ref double previousLength)
        {
            previousLength = System.Math.Sqrt(x * x + y * y + z * z);

            double invLength = 1.0 / previousLength;

            return new Vector3d(x * invLength, y * invLength, z * invLength);
        }

        public Vector3d Cross(Vector3d v)
        {
            return new Vector3d(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
        }

        public Vector2d XY()
        {
            return new Vector2d(x, y);
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)x, (float)y, (float)z);
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

        public static Vector3d Zero()
        {
            return new Vector3d(0, 0, 0);
        }
    } 
}