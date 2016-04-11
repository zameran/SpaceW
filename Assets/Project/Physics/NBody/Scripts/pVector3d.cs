#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace NBody
{
    [StructLayout(LayoutKind.Sequential)]
    public struct pVector3d : IEquatable<pVector3d>
    {
        public static readonly pVector3d Zero;
        public static readonly pVector3d XAxis;
        public static readonly pVector3d YAxis;
        public static readonly pVector3d ZAxis;

        public double X;
        public double Y;
        public double Z;

        public pVector3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static pVector3d Multiply(pVector3d a, double b)
        {
            return new pVector3d(a.X * b, a.Y * b, a.Z * b);
        }

        public static pVector3d operator *(pVector3d a, double b)
        {
            return Multiply(a, b);
        }

        public static pVector3d operator *(double a, pVector3d b)
        {
            return Multiply(b, a);
        }

        public static pVector3d Divide(pVector3d a, double b)
        {
            double num = 1.0 / b;
            return new pVector3d(a.X * num, a.Y * num, a.Z * num);
        }

        public static pVector3d operator /(pVector3d a, double b)
        {
            return Divide(a, b);
        }

        public static pVector3d Add(pVector3d a, pVector3d b)
        {
            return new pVector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static pVector3d operator +(pVector3d a, pVector3d b)
        {
            return Add(a, b);
        }

        public static pVector3d Subtract(pVector3d a, pVector3d b)
        {
            return new pVector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static pVector3d operator -(pVector3d a, pVector3d b)
        {
            return Subtract(a, b);
        }

        public static pVector3d Negate(pVector3d a)
        {
            return new pVector3d(-a.X, -a.Y, -a.Z);
        }

        public static pVector3d operator -(pVector3d a)
        {
            return Negate(a);
        }

        public bool Equals(pVector3d a)
        {
            return (((this.X == a.X) && (this.Y == a.Y)) && (this.Z == a.Z));
        }

        public static bool operator ==(pVector3d a, pVector3d b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(pVector3d a, pVector3d b)
        {
            return !object.Equals(a, b);
        }

        public static explicit operator pVector3d(Vector3 v)
        {
            return new pVector3d((double)v.x, (double)v.y, (double)v.z);
        }

        public static implicit operator Vector3(pVector3d v)
        {
            return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static double Dot(pVector3d a, pVector3d b)
        {
            return (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));
        }

        public static pVector3d Cross(pVector3d a, pVector3d b)
        {
            return new pVector3d((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
        }

        public static double Angle(pVector3d a, pVector3d b)
        {
            return Math.Acos(Dot(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public static double Distance(pVector3d a, pVector3d b)
        {
            return Math.Sqrt((((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y))) + ((a.Z - b.Z) * (a.Z - b.Z)));
        }

        public static pVector3d Projection(pVector3d a, pVector3d b)
        {
            return (pVector3d)((Dot(a, b) / Dot(b, b)) * b);
        }

        public static pVector3d Rejection(pVector3d a, pVector3d b)
        {
            return (a - Projection(a, b));
        }

        public pVector3d Rotate(double pointX, double pointY, double pointZ, double directionX, double directionY, double directionZ, double angle)
        {
            double length = 1.0 / Math.Sqrt(((directionX * directionX) + (directionY * directionY)) + (directionZ * directionZ));
            directionX *= length;
            directionY *= length;
            directionZ *= length;
            double cosa = Math.Cos(angle);
            double sina = Math.Sin(angle);
            double x = ((((pointX * ((directionY * directionY) + (directionZ * directionZ))) - (directionX * (((((pointY * directionY) + (pointZ * directionZ)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - cosa)) + (this.X * cosa)) + (((((-pointZ * directionY) + (pointY * directionZ)) - (directionZ * this.Y)) + (directionY * this.Z)) * sina);
            double y = ((((pointY * ((directionX * directionX) + (directionZ * directionZ))) - (directionY * (((((pointX * directionX) + (pointZ * directionZ)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - cosa)) + (this.Y * cosa)) + (((((pointZ * directionX) - (pointX * directionZ)) + (directionZ * this.X)) - (directionX * this.Z)) * sina);
            return new pVector3d(x, y, ((((pointZ * ((directionX * directionX) + (directionY * directionY))) - (directionZ * (((((pointX * directionX) + (pointY * directionY)) - (directionX * this.X)) - (directionY * this.Y)) - (directionZ * this.Z)))) * (1.0 - cosa)) + (this.Z * cosa)) + (((((-pointY * directionX) + (pointX * directionY)) - (directionY * this.X)) + (directionX * this.Y)) * sina));
        }

        public pVector3d Rotate(pVector3d point, pVector3d direction, double angle)
        {
            return this.Rotate(point.X, point.Y, point.Z, direction.X, direction.Y, direction.Z, angle);
        }

        public pVector3d Unit()
        {
            return (this / this.Magnitude());
        }

        public double Magnitude()
        {
            return Math.Sqrt(((this.X * this.X) + (this.Y * this.Y)) + (this.Z * this.Z));
        }

        public static pVector3d Sum(ICollection<pVector3d> vectors)
        {
            pVector3d vector = new pVector3d();

            foreach (pVector3d vector2 in vectors)
            {
                vector += vector2;
            }

            return vector;
        }

        public static pVector3d Average(ICollection<pVector3d> vectors)
        {
            return (Sum(vectors) / ((double)vectors.Count));
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "[", this.X, " ", this.Y, " ", this.Z, "]" });
        }

        public override bool Equals(object a)
        {
            return ((a is pVector3d) && this.Equals((pVector3d)a));
        }

        public override int GetHashCode()
        {
            int code = BitConverter.DoubleToInt64Bits(this.X).GetHashCode() * 0x1f;
            code += BitConverter.DoubleToInt64Bits(this.Y).GetHashCode();
            code *= 0x1f;
            return (code + BitConverter.DoubleToInt64Bits(this.Z).GetHashCode());
        }

        static pVector3d()
        {
            Zero = new pVector3d();
            XAxis = new pVector3d(1.0, 0.0, 0.0);
            YAxis = new pVector3d(0.0, 1.0, 0.0);
            ZAxis = new pVector3d(0.0, 0.0, 1.0);
        }
    }
}