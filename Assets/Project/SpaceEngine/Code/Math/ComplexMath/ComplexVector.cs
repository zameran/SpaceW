using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityEngine
{
    [Serializable]
    public struct ComplexVector : IEquatable<ComplexVector>
    {
        public Complex x;
        public Complex y;
        public Complex z;

        public ComplexVector(Complex x)
        {
            this.x = x;
            this.y = Complex.Zero;
            this.z = Complex.Zero;
        }

        public ComplexVector(Complex x, Complex y)
        {
            this.x = x;
            this.y = y;
            this.z = Complex.Zero;
        }

        public ComplexVector(Complex x, Complex y, Complex z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static readonly ComplexVector Zero = new ComplexVector(Complex.Zero, Complex.Zero, Complex.Zero);
        public static readonly ComplexVector One = new ComplexVector(Complex.One, Complex.One, Complex.One);

        public static ComplexVector operator +(ComplexVector left, ComplexVector right)
        {
            return new ComplexVector(left.x + right.x, left.y + right.y, left.z + left.z);
        }

        public static ComplexVector operator -(ComplexVector left, ComplexVector right)
        {
            return new ComplexVector(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static ComplexVector operator -(ComplexVector left)
        {
            return new ComplexVector(-left.x, -left.y, -left.z);
        }

        public static ComplexVector operator *(ComplexVector left, ComplexVector right)
        {
            return new ComplexVector(left.x * right.x, left.y * right.y, left.z * right.z);
        }

        public static ComplexVector operator *(ComplexVector left, Complex right)
        {
            return new ComplexVector(left.x * right, left.y * right, left.x * right);
        }

        public static ComplexVector operator *(ComplexVector left, double right)
        {
            return new ComplexVector(left.x * right, left.y * right, left.z * right);
        }

        public static ComplexVector operator /(ComplexVector left, ComplexVector right)
        {
            return new ComplexVector(left.x / right.x, left.y / right.y, left.z / right.z);
        }

        public static ComplexVector operator /(ComplexVector left, Complex right)
        {
            return new ComplexVector(left.x / right, left.y / right, left.z / right);
        }

        public static ComplexVector operator /(ComplexVector left, double right)
        {
            return new ComplexVector(left.x / right, left.y / right, left.z / right);
        }

        public static bool operator ==(ComplexVector left, ComplexVector right)
        {
            return (left.x == right.x && left.y == right.y && left.z == right.z);
        }

        public static bool operator !=(ComplexVector left, ComplexVector right)
        {
            return (left.x != right.x || left.y != right.y || left.z != right.z);
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() ^ y.GetHashCode()) << 2 + z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ComplexVector)) return false;

            return this == (ComplexVector)obj;
        }

        public bool Equals(ComplexVector value)
        {
            return (this.x.Equals(value.x) && this.y.Equals(value.y) && this.z.Equals(value.z));
        }

        public override string ToString()
        {
            return (string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2})", this.x.ToString(), this.y.ToString(), this.z.ToString()));
        }
    }
}