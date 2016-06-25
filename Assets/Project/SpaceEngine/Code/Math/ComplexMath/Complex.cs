// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 

using System;
using System.Globalization;

namespace UnityEngine
{
    [Serializable]
    public struct Complex : IEquatable<Complex>, IFormattable
    {
        [SerializeField]
        private double real;

        [SerializeField]
        private double imaginary;

        private const double LOG_10_INV = 0.43429448190325;

        public double Real
        {
            get
            {
                return real;
            }
            set
            {
                real = value;
            }
        }

        public double Imaginary
        {
            get
            {
                return imaginary;
            }
            set
            {
                imaginary = value;
            }
        }

        public double Magnitude
        {
            get
            {
                return Abs(this);
            }
        }

        public double Phase
        {
            get
            {
                return Math.Atan2(imaginary, real);
            }
        }

        public static readonly Complex Zero = new Complex(0.0, 0.0);
        public static readonly Complex One = new Complex(1.0, 0.0);
        public static readonly Complex ImaginaryOne = new Complex(0.0, 1.0);

        public Complex(Complex from)
        {
            this.real = from.real;
            this.imaginary = from.imaginary;
        }

        public Complex(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public static Complex FromPolarCoordinates(double magnitude, double phase)
        {
            return new Complex((magnitude * Math.Cos(phase)), (magnitude * Math.Sin(phase)));
        }

        public static Complex Negate(Complex value)
        {
            return -value;
        }

        public static Complex Add(Complex left, Complex right)
        {
            return left + right;
        }

        public static Complex Subtract(Complex left, Complex right)
        {
            return left - right;
        }

        public static Complex Multiply(Complex left, Complex right)
        {
            return left * right;
        }

        public static Complex Divide(Complex dividend, Complex divisor)
        {
            return dividend / divisor;
        }

        public static Complex operator -(Complex value)
        {
            return new Complex((-value.real), (-value.imaginary));
        }

        public static Complex operator +(Complex left, Complex right)
        {
            return new Complex((left.real + right.real), (left.imaginary + right.imaginary));
        }

        public static Complex operator -(Complex left, Complex right)
        {
            return new Complex((left.real - right.real), (left.imaginary - right.imaginary));
        }

        public static Complex operator *(Complex left, Complex right)
        {
            // Multiplication : (a + bi)(c + di) = (ac -bd) + (bc + ad)i
            double result_Realpart = (left.real * right.real) - (left.imaginary * right.imaginary);
            double result_Imaginarypart = (left.imaginary * right.real) + (left.real * right.imaginary);

            return new Complex(result_Realpart, result_Imaginarypart);
        }

        public static Complex operator /(Complex left, Complex right)
        {
            // Division : Smith's formula.
            double a = left.real;
            double b = left.imaginary;
            double c = right.real;
            double d = right.imaginary;

            if (Math.Abs(d) < Math.Abs(c))
            {
                double doc = d / c;

                return new Complex((a + b * doc) / (c + d * doc), (b - a * doc) / (c + d * doc));
            }
            else
            {
                double cod = c / d;

                return new Complex((b + a * cod) / (d + c * cod), (-a + b * cod) / (d + c * cod));
            }
        }

        public static double Abs(Complex value)
        {
            if (double.IsInfinity(value.real) || double.IsInfinity(value.imaginary))
            {
                return double.PositiveInfinity;
            }

            // |value| == sqrt(a^2 + b^2)
            // sqrt(a^2 + b^2) == a/a * sqrt(a^2 + b^2) = a * sqrt(a^2/a^2 + b^2/a^2)
            // Using the above we can factor out the square of the larger component to dodge overflow.

            double c = Math.Abs(value.real);
            double d = Math.Abs(value.imaginary);

            if (c > d)
            {
                double r = d / c;

                return c * Math.Sqrt(1.0 + r * r);
            }
            else if (d == 0.0)
            {
                return c;  // c is either 0.0 or NaN
            }
            else
            {
                double r = c / d;

                return d * Math.Sqrt(1.0 + r * r);
            }
        }
        public static Complex Conjugate(Complex value)
        {
            // Conjugate of a Complex number : the conjugate of x+i*y is x-i*y 
            return new Complex(value.real, (-value.imaginary));

        }
        public static Complex Reciprocal(Complex value)
        {
            // Reciprocal of a Complex number : the reciprocal of x+i*y is 1/(x+i*y)
            if ((value.real == 0) && (value.imaginary == 0))
            {
                return Complex.Zero;
            }

            return Complex.One / value;
        }

        public static bool operator ==(Complex left, Complex right)
        {
            return ((left.real == right.real) && (left.imaginary == right.imaginary));
        }

        public static bool operator !=(Complex left, Complex right)
        {
            return ((left.real != right.real) || (left.imaginary != right.imaginary));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Complex)) return false;

            return this == (Complex)obj;
        }

        public bool Equals(Complex value)
        {
            return (this.real.Equals(value.real) && this.imaginary.Equals(value.imaginary));
        }

        public static implicit operator Complex(short value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(int value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(long value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(ushort value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(uint value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(ulong value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(sbyte value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(byte value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(float value)
        {
            return new Complex(value, 0.0);
        }

        public static implicit operator Complex(double value)
        {
            return new Complex(value, 0.0);
        }

        public static explicit operator Complex(decimal value)
        {
            return new Complex((double)value, 0.0);
        }

        public override string ToString()
        {
            return (string.Format(CultureInfo.CurrentCulture, "({0}, {1})", this.real, this.imaginary));
        }

        public string ToString(string format)
        {
            return (string.Format(CultureInfo.CurrentCulture, "({0}, {1})", this.real.ToString(format, CultureInfo.CurrentCulture), this.imaginary.ToString(format, CultureInfo.CurrentCulture)));
        }

        public string ToString(IFormatProvider provider)
        {
            return (string.Format(provider, "({0}, {1})", this.real, this.imaginary));
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return (string.Format(provider, "({0}, {1})", this.real.ToString(format, provider), this.imaginary.ToString(format, provider)));
        }

        public override int GetHashCode()
        {
            int n1 = 99999997;
            int hash_real = this.real.GetHashCode() % n1;
            int hash_imaginary = this.imaginary.GetHashCode();
            int final_hashcode = hash_real ^ hash_imaginary;

            return (final_hashcode);
        }

        public static Complex Sin(Complex value)
        {
            double a = value.real;
            double b = value.imaginary;

            return new Complex(Math.Sin(a) * Math.Cosh(b), Math.Cos(a) * Math.Sinh(b));
        }

        public static Complex Sinh(Complex value)
        {
            double a = value.real;
            double b = value.imaginary;

            return new Complex(Math.Sinh(a) * Math.Cos(b), Math.Cosh(a) * Math.Sin(b));

        }

        public static Complex Asin(Complex value)
        {
            return (-ImaginaryOne) * Log(ImaginaryOne * value + Sqrt(One - value * value));
        }

        public static Complex Cos(Complex value)
        {
            double a = value.real;
            double b = value.imaginary;

            return new Complex(Math.Cos(a) * Math.Cosh(b), -(Math.Sin(a) * Math.Sinh(b)));
        }

        public static Complex Cosh(Complex value)
        {
            double a = value.real;
            double b = value.imaginary;

            return new Complex(Math.Cosh(a) * Math.Cos(b), Math.Sinh(a) * Math.Sin(b));
        }

        public static Complex Acos(Complex value)
        {
            return (-ImaginaryOne) * Log(value + ImaginaryOne * Sqrt(One - (value * value)));

        }

        public static Complex Tan(Complex value)
        {
            return Sin(value) / Cos(value);
        }

        public static Complex Tanh(Complex value)
        {
            return Sinh(value) / Cosh(value);
        }

        public static Complex Atan(Complex value)
        {
            Complex Two = new Complex(2.0, 0.0);

            return (ImaginaryOne / Two) * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
        }

        public static Complex Log(Complex value)
        {
            return new Complex((Math.Log(Abs(value))), (Math.Atan2(value.imaginary, value.real)));

        }

        public static Complex Log(Complex value, double baseValue)
        {
            return Log(value) / Log(baseValue);
        }

        public static Complex Log10(Complex value)
        {
            Complex temp_log = Log(value);

            return Scale(temp_log, LOG_10_INV);
        }

        public static Complex Exp(Complex value)
        {
            double temp_factor = Math.Exp(value.real);
            double result_re = temp_factor * Math.Cos(value.imaginary);
            double result_im = temp_factor * Math.Sin(value.imaginary);

            return new Complex(result_re, result_im);
        }

        public static Complex Sqrt(Complex value)
        {
            return Complex.FromPolarCoordinates(Math.Sqrt(value.Magnitude), value.Phase / 2.0);
        }

        public static Complex Pow(Complex value, Complex power)
        {
            if (power == Complex.Zero)
            {
                return Complex.One;
            }

            if (value == Complex.Zero)
            {
                return Complex.Zero;
            }

            double a = value.real;
            double b = value.imaginary;
            double c = power.real;
            double d = power.imaginary;

            double rho = Complex.Abs(value);
            double theta = Math.Atan2(b, a);
            double newRho = c * theta + d * Math.Log(rho);

            double t = Math.Pow(rho, c) * Math.Pow(Math.E, -d * theta);

            return new Complex(t * Math.Cos(newRho), t * Math.Sin(newRho));
        }

        public static Complex Pow(Complex value, double power)
        {
            return Pow(value, new Complex(power, 0));
        }

        private static Complex Scale(Complex value, double factor)
        {
            double result_re = factor * value.real;
            double result_im = factor * value.imaginary;

            return (new Complex(result_re, result_im));
        }
    }
}