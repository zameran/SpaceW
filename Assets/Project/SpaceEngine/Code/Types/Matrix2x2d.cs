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

#define MATICES_UNROLL

using System;

namespace UnityEngine
{
    public struct Matrix2x2d : IEquatable<Matrix2x2d>
    {
        #region Fields

        public readonly double[,] m;

        #endregion

        #region Properties

        public static Matrix2x2d identity { get { return new Matrix2x2d(1.0, 0.0, 0.0, 1.0); } }

        public static Matrix2x2d zero { get { return new Matrix2x2d(0.0); } }

        public static Matrix2x2d one { get { return new Matrix2x2d(1.0); } }

        #endregion

        #region Constructors

        public Matrix2x2d(double value)
        {
            m = new double[2, 2];

            m[0, 0] = value;
            m[0, 1] = value;
            m[1, 0] = value;
            m[1, 1] = value;
        }

        public Matrix2x2d(double m00, double m01, double m10, double m11)
        {
            m = new double[2, 2];

            m[0, 0] = m00;
            m[0, 1] = m01;
            m[1, 0] = m10;
            m[1, 1] = m11;
        }

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            return m[0, 0].GetHashCode() + m[1, 0].GetHashCode() +
                   m[0, 1].GetHashCode() + m[1, 1].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix2x2d) { return Equals((Matrix2x2d)obj); }
            return false;
        }

        #endregion

        #region IEquatable<Matrix2x2d>

        public bool Equals(Matrix2x2d other)
        {
            return this == other;
        }

        #endregion

        #region Operations

        public static Matrix2x2d operator +(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kSum = Matrix2x2d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] + m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] + m2.m[0, 1];

            kSum.m[1, 0] = m1.m[1, 0] + m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] + m2.m[1, 1];
#else
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix2x2d operator -(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kSum = Matrix2x2d.identity;

#if (MATICES_UNROLL)
            kSum.m[0, 0] = m1.m[0, 0] - m2.m[0, 0];
            kSum.m[0, 1] = m1.m[0, 1] - m2.m[0, 1];

            kSum.m[1, 0] = m1.m[1, 0] - m2.m[1, 0];
            kSum.m[1, 1] = m1.m[1, 1] - m2.m[1, 1];
#else
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
                }
            }
#endif

            return kSum;
        }

        public static Matrix2x2d operator *(Matrix2x2d m1, Matrix2x2d m2)
        {
            var kProd = Matrix2x2d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m1.m[0, 0] * m2.m[0, 0] + m1.m[0, 1] * m2.m[1, 0];
            kProd.m[0, 1] = m1.m[0, 0] * m2.m[0, 1] + m1.m[0, 1] * m2.m[1, 1];

            kProd.m[1, 0] = m1.m[1, 0] * m2.m[0, 0] + m1.m[1, 1] * m2.m[1, 0];
            kProd.m[1, 1] = m1.m[1, 0] * m2.m[0, 1] + m1.m[1, 1] * m2.m[1, 1];
#else
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] + m1.m[iRow, 1] * m2.m[1, iCol];
                }
            }
#endif

            return kProd;
        }

        public static Vector2d operator *(Matrix2x2d m, Vector2d v)
        {
            return new Vector2d
            {
                x = m.m[0, 0] * v.x + m.m[0, 1] * v.y,
                y = m.m[1, 0] * v.x + m.m[1, 1] * v.y
            };
        }

        public static Matrix2x2d operator *(Matrix2x2d m, double s)
        {
            var kProd = Matrix2x2d.identity;

#if (MATICES_UNROLL)
            kProd.m[0, 0] = m.m[0, 0] * s;
            kProd.m[0, 1] = m.m[0, 1] * s;

            kProd.m[1, 0] = m.m[1, 0] * s;
            kProd.m[1, 1] = m.m[1, 1] * s;
#else
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kProd.m[iRow, iCol] = m.m[iRow, iCol] * s;
                }
            }
#endif

            return kProd;
        }

        public static bool operator ==(Matrix2x2d m1, Matrix2x2d m2)
        {
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return false;
                }
            }

            return true;
        }

        public static bool operator !=(Matrix2x2d m1, Matrix2x2d m2)
        {
            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    if (!BrainFuckMath.NearlyEqual(m1.m[iRow, iCol], m2.m[iRow, iCol])) return true;
                }
            }

            return false;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("[({0}, {1})({2}, {3})]", m[0, 0], m[0, 1],
                                                           m[1, 0], m[1, 1]);
        }

        #endregion

        #region Column/Row

        public Vector2d GetColumn(int iCol)
        {
            return new Vector2d(m[0, iCol], m[1, iCol]);
        }

        public Vector2d GetRow(int iRow)
        {
            return new Vector2d(m[iRow, 0], m[iRow, 1]);
        }

        public void SetColumn(int iCol, Vector2 v)
        {
            m[0, iCol] = v.x;
            m[1, iCol] = v.y;
        }

        public void SetRow(int iRow, Vector3 v)
        {
            m[iRow, 0] = v.x;
            m[iRow, 1] = v.y;
        }

        #endregion

        public Matrix2x2d Transpose()
        {
            var kTranspose = Matrix2x2d.identity;

            for (byte iRow = 0; iRow < 2; iRow++)
            {
                for (byte iCol = 0; iCol < 2; iCol++)
                {
                    kTranspose.m[iRow, iCol] = m[iCol, iRow];
                }
            }

            return kTranspose;
        }

        public double Determinant()
        {
            return m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1];
        }

        public bool Inverse(ref Matrix2x2d mInv, double tolerance = 1e-06)
        {
            var det = Determinant();

            if (System.Math.Abs(det) <= tolerance) { return false; }

            var invDet = 1.0 / det;

            mInv.m[0, 0] = m[1, 1] * invDet;
            mInv.m[0, 1] = -m[0, 1] * invDet;
            mInv.m[1, 0] = -m[1, 0] * invDet;
            mInv.m[1, 1] = m[0, 0] * invDet;

            return true;
        }

        public Matrix2x2d Inverse(double tolerance = 1e-06)
        {
            var kInverse = Matrix2x2d.identity;

            Inverse(ref kInverse, tolerance);

            return kInverse;
        }
    }
}