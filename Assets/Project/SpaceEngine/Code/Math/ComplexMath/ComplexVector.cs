#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
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