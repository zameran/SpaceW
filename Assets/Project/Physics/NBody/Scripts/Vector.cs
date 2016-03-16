using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NBody
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector : IEquatable<Vector>
    {
        public static readonly Vector Zero;
        public static readonly Vector XAxis;
        public static readonly Vector YAxis;
        public static readonly Vector ZAxis;

        public double X;
        public double Y;
        public double Z;

        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Vector Multiply(Vector a, double b)
        {
            return new Vector(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector operator *(Vector a, double b)
        {
            return Multiply(a, b);
        }

        public static Vector operator *(double a, Vector b)
        {
            return Multiply(b, a);
        }

        public static Vector Divide(Vector a, double b)
        {
            double num = 1.0 / b;
            return new Vector(a.X * num, a.Y * num, a.Z * num);
        }

        public static Vector operator /(Vector a, double b)
        {
            return Divide(a, b);
        }

        public static Vector Add(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return Add(a, b);
        }

        public static Vector Subtract(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return Subtract(a, b);
        }

        public static Vector Negate(Vector a)
        {
            return new Vector(-a.X, -a.Y, -a.Z);
        }

        public static Vector operator -(Vector a)
        {
            return Negate(a);
        }

        public bool Equals(Vector a)
        {
            return (((this.X == a.X) && (this.Y == a.Y)) && (this.Z == a.Z));
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return !object.Equals(a, b);
        }

        public static double Dot(Vector a, Vector b)
        {
            return (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));
        }

        public static Vector Cross(Vector a, Vector b)
        {
            return new Vector((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
        }

        public static double Angle(Vector a, Vector b)
        {
            return Math.Acos(Dot(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public static double Distance(Vector a, Vector b)
        {
            return Math.Sqrt((((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y))) + ((a.Z - b.Z) * (a.Z - b.Z)));
        }

        public static Vector Projection(Vector a, Vector b)
        {
            return (Vector)((Dot(a, b) / Dot(b, b)) * b);
        }

        public static Vector Rejection(Vector a, Vector b)
        {
            return (a - Projection(a, b));
        }

        public Vector Rotate(double pointX, double pointY, double pointZ, double directionX, double directionY, double directionZ, double angle)
        {
            double num = 1.0 / Math.Sqrt(((directionX * directionX) + (directionY * directionY)) + (directionZ * directionZ));
            directionX *= num;
            directionY *= num;
            directionZ *= num;
            double num2 = Math.Cos(angle);
            double num3 = Math.Sin(angle);
            double x = ((((pointX * ((directionY * directionY) + (directionZ * directionZ))) - (directionX * (((((pointY * directionY) + (pointZ * directionZ)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - num2)) + (this.X * num2)) + (((((-pointZ * directionY) + (pointY * directionZ)) - (directionZ * this.Y)) + (directionY * this.Z)) * num3);
            double y = ((((pointY * ((directionX * directionX) + (directionZ * directionZ))) - (directionY * (((((pointX * directionX) + (pointZ * directionZ)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - num2)) + (this.Y * num2)) + (((((pointZ * directionX) - (pointX * directionZ)) + (directionZ * this.X)) - (directionX * this.Z)) * num3);
            return new Vector(x, y, ((((pointZ * ((directionX * directionX) + (directionY * directionY))) - (directionZ * (((((pointX * directionX) + (pointY * directionY)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - num2)) + (this.Z * num2)) + (((((-pointY * directionX) + (pointX * directionY)) - (directionY * this.X)) + (directionX * this.Y)) * num3));
        }

        public Vector Rotate(Vector point, Vector direction, double angle)
        {
            return this.Rotate(point.X, point.Y, point.Z, direction.X, direction.Y, direction.Z, angle);
        }

        public Vector To(Vector a)
        {
            return (a - this);
        }

        public Vector Unit()
        {
            return (Vector)(this / this.Magnitude());
        }

        public double Magnitude()
        {
            return Math.Sqrt(((this.X * this.X) + (this.Y * this.Y)) + (this.Z * this.Z));
        }

        public static Vector Sum(ICollection<Vector> vectors)
        {
            Vector vector = new Vector();
            foreach (Vector vector2 in vectors)
            {
                vector += vector2;
            }
            return vector;
        }

        public static Vector Average(ICollection<Vector> vectors)
        {
            return (Vector)(Sum(vectors) / ((double)vectors.Count));
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "[", this.X, " ", this.Y, " ", this.Z, "]" });
        }

        public override bool Equals(object a)
        {
            return ((a is Vector) && this.Equals((Vector)a));
        }

        public override int GetHashCode()
        {
            int num = BitConverter.DoubleToInt64Bits(this.X).GetHashCode() * 0x1f;
            num += BitConverter.DoubleToInt64Bits(this.Y).GetHashCode();
            num *= 0x1f;
            return (num + BitConverter.DoubleToInt64Bits(this.Z).GetHashCode());
        }

        static Vector()
        {
            Zero = new Vector();
            XAxis = new Vector(1.0, 0.0, 0.0);
            YAxis = new Vector(0.0, 1.0, 0.0);
            ZAxis = new Vector(0.0, 0.0, 1.0);
        }
    }
}