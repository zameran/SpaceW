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

using UnityEngine;

using System;
using System.Runtime.InteropServices;

namespace NBody
{
    public static class PseudoRandom
    {
        private static ulong seed = ((ulong)DateTime.Now.ToFileTime());

        static PseudoRandom()
        {
            for (int i = 0; i < 10; i++)
            {
                UInt64();
            }
        }

        public static bool Boolean()
        {
            return ((UInt64() & ((ulong)1L)) == 0L);
        }

        public static Vector DirectionVector(double magnitude = 1)
        {
            Vector vector;

            do
            {
                vector = new Vector(Double(-1.0, 1.0), Double(-1.0, 1.0), Double(-1.0, 1.0));
            }

            while (vector.Magnitude() == 0.0);

            return (Vector)((magnitude / vector.Magnitude()) * vector);
        }

        public static double Double()
        {
            return (UInt64() * 5.4210108624275222E-20);
        }

        public static double Double(double a, double b = 0)
        {
            return (a + (Double() * (b - a)));
        }

        public static int Int32(int a, int b = 0)
        {
            double num = 0.5 * Math.Sign((double)(a - b));

            return (int)Math.Round(Double(a + num, b - num));
        }

        public static ulong UInt64()
        {
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 0x11;

            return seed;
        }

        public static Vector Vector(double maximumMagnitude = 1)
        {
            return (Vector)(Double(maximumMagnitude, 0.0) * DirectionVector(1.0));
        }
    }
}