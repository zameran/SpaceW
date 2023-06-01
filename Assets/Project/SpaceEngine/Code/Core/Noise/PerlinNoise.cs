#region License

// Procedural planet generator.
//  
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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
// Creation Date: 2017.03.28
// Creation Time: 2:18 PM
// Creator: zameran

#endregion

using System;
using UnityEngine;

namespace SpaceEngine.Core.Noise
{
    public class PerlinNoise
    {
        private static readonly int[] Permutation;

        private static readonly int[] Table =
        {
            15, 131, 91, 90, 13, 95, 201, 194, 96, 7, 53, 233, 140, 30, 225, 69, 36, 103, 142, 6, 8, 99, 21, 37, 240, 10, 23, 120, 190, 148, 0, 247, 94, 234, 75, 197, 26, 62, 203, 32, 252, 219, 57, 117,
            35, 11, 87, 33, 177, 149, 88, 237, 56, 20, 174, 136, 175, 125, 171, 168, 74, 68, 134, 48, 165, 71, 139, 27, 77, 166, 231, 111, 146, 158, 83, 133, 229, 60, 122, 230, 211, 220, 41, 105, 55,
            92, 102, 46, 245, 244, 40, 143, 63, 54, 65, 25, 216, 161, 1, 73, 80, 169, 187, 209, 76, 132, 208, 89, 18, 188, 130, 200, 196, 135, 116, 159, 86, 198, 100, 164, 3, 109, 173, 64, 186, 52,
            226, 5, 217, 250, 124, 202, 123, 38, 255, 147, 126, 118, 59, 85, 82, 212, 207, 206, 227, 16, 47, 58, 189, 28, 17, 182, 183, 42, 170, 223, 213, 248, 152, 119, 44, 221, 2, 155, 154, 163, 70,
            101, 153, 167, 43, 9, 172, 129, 39, 22, 110, 253, 108, 19, 98, 113, 79, 232, 224, 185, 178, 112, 246, 218, 104, 251, 97, 228, 34, 242, 193, 238, 241, 210, 12, 144, 179, 191, 162, 81, 107,
            51, 145, 235, 249, 239, 14, 106, 49, 199, 192, 214, 31, 181, 84, 204, 157, 184, 127, 115, 176, 121, 50, 4, 45, 150, 254, 138, 236, 222, 93, 205, 29, 114, 67, 141, 24, 72, 128, 243, 215,
            195, 66, 78, 137, 156, 61, 160, 180, 151
        };

        static PerlinNoise()
        {
            Permutation = new int[512];

            for (ushort i = 0; i < 512; i++)
            {
                Permutation[i] = Table[i & 255];
            }
        }

        public double Noise(double x)
        {
            var ix0 = (int)Math.Floor(x); // Integer part of x
            var fx0 = x - ix0; // Fractional part of x
            var fx1 = fx0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff;

            ix0 = ix0 & 0xff; // Wrap to 0..255

            var s = Fade(fx0);

            var n0 = Gradient(Permutation[ix0], fx0);
            var n1 = Gradient(Permutation[ix1], fx1);

            return 0.188 * Lerp(s, n0, n1);
        }

        public double Noise(double x, double y)
        {
            var ix0 = (int)Math.Floor(x); // Integer part of x
            var iy0 = (int)Math.Floor(y); // Integer part of y
            var fx0 = x - ix0; // Fractional part of x
            var fy0 = y - iy0; // Fractional part of y
            var fx1 = fx0 - 1.0f;
            var fy1 = fy0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff; // Wrap to 0..255
            var iy1 = (iy0 + 1) & 0xff;

            ix0 = ix0 & 0xff;
            iy0 = iy0 & 0xff;

            var t = Fade(fy0);
            var s = Fade(fx0);

            var nx0 = Gradient(Permutation[ix0 + Permutation[iy0]], fx0, fy0);
            var nx1 = Gradient(Permutation[ix0 + Permutation[iy1]], fx0, fy1);

            var n0 = Lerp(t, nx0, nx1);

            nx0 = Gradient(Permutation[ix1 + Permutation[iy0]], fx1, fy0);
            nx1 = Gradient(Permutation[ix1 + Permutation[iy1]], fx1, fy1);

            var n1 = Lerp(t, nx0, nx1);

            return 0.507 * Lerp(s, n0, n1);
        }

        public double Noise(double x, double y, double z)
        {
            var ix0 = (int)Math.Floor(x); // Integer part of x
            var iy0 = (int)Math.Floor(y); // Integer part of y
            var iz0 = (int)Math.Floor(z); // Integer part of z
            var fx0 = x - ix0; // Fractional part of x
            var fy0 = y - iy0; // Fractional part of y
            var fz0 = z - iz0; // Fractional part of z
            var fx1 = fx0 - 1.0f;
            var fy1 = fy0 - 1.0f;
            var fz1 = fz0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff; // Wrap to 0..255
            var iy1 = (iy0 + 1) & 0xff;
            var iz1 = (iz0 + 1) & 0xff;

            ix0 = ix0 & 0xff;
            iy0 = iy0 & 0xff;
            iz0 = iz0 & 0xff;

            var r = Fade(fz0);
            var t = Fade(fy0);
            var s = Fade(fx0);

            var nxy0 = Gradient(Permutation[ix0 + Permutation[iy0 + Permutation[iz0]]], fx0, fy0, fz0);
            var nxy1 = Gradient(Permutation[ix0 + Permutation[iy0 + Permutation[iz1]]], fx0, fy0, fz1);
            var nx0 = Lerp(r, nxy0, nxy1);

            nxy0 = Gradient(Permutation[ix0 + Permutation[iy1 + Permutation[iz0]]], fx0, fy1, fz0);
            nxy1 = Gradient(Permutation[ix0 + Permutation[iy1 + Permutation[iz1]]], fx0, fy1, fz1);

            var nx1 = Lerp(r, nxy0, nxy1);

            var n0 = Lerp(t, nx0, nx1);

            nxy0 = Gradient(Permutation[ix1 + Permutation[iy0 + Permutation[iz0]]], fx1, fy0, fz0);
            nxy1 = Gradient(Permutation[ix1 + Permutation[iy0 + Permutation[iz1]]], fx1, fy0, fz1);
            nx0 = Lerp(r, nxy0, nxy1);

            nxy0 = Gradient(Permutation[ix1 + Permutation[iy1 + Permutation[iz0]]], fx1, fy1, fz0);
            nxy1 = Gradient(Permutation[ix1 + Permutation[iy1 + Permutation[iz1]]], fx1, fy1, fz1);
            nx1 = Lerp(r, nxy0, nxy1);

            var n1 = Lerp(t, nx0, nx1);

            return 0.936 * Lerp(s, n0, n1);
        }

        public static Vector4 dNoise(double x, double y, double z)
        {
            var X = FastFloor(x) & 255;
            var Y = FastFloor(y) & 255;
            var Z = FastFloor(z) & 255;

            x -= FastFloor(x);
            y -= FastFloor(y);
            z -= FastFloor(z);

            var u = x;
            var v = y;
            var w = z;

            var du = 30 * u * u * u * u - 60 * u * u * u + 30 * u * u;
            var dv = 30 * v * v * v * v - 60 * v * v * v + 30 * v * v;
            var dw = 30 * w * w * w * w - 60 * w * w * w + 30 * w * w;

            u = Fade(x);
            v = Fade(y);
            w = Fade(z);

            var A = Permutation[X] + Y;
            var AA = Permutation[A] + Z;
            var AB = Permutation[A + 1] + Z;
            var B = Permutation[X + 1] + Y;
            var BA = Permutation[B] + Z;
            var BB = Permutation[B + 1] + Z;

            var a = Gradient(Permutation[AA], x, y, z);
            var b = Gradient(Permutation[BA], x - 1, y, z);
            var c = Gradient(Permutation[AB], x, y - 1, z);
            var d = Gradient(Permutation[BB], x - 1, y - 1, z);
            var e = Gradient(Permutation[AA + 1], x, y, z - 1);
            var f = Gradient(Permutation[BA + 1], x - 1, y, z - 1);
            var g = Gradient(Permutation[AB + 1], x, y - 1, z - 1);
            var h = Gradient(Permutation[BB + 1], x - 1, y - 1, z - 1);

            var k0 = a;
            var k1 = b - a;
            var k2 = c - a;
            var k3 = e - a;
            var k4 = a - b - c + d;
            var k5 = a - c - e + g;
            var k6 = a - b - e + f;
            var k7 = -a + b + c - d + e - f - g + h;

            var noise = k0 + k1 * u + k2 * v + k3 * w + k4 * u * v + k5 * v * w + k6 * w * u + k7 * u * v * w;

            return new Vector4((float)noise, (float)(du * (k1 + k4 * v + k6 * w + k7 * v * w)),
                               (float)(dv * (k2 + k5 * w + k4 * u + k7 * w * u)),
                               (float)(dw * (k3 + k6 * u + k5 * v + k7 * u * v)));
        }

        private static int FastFloor(double x)
        {
            return x > 0 ? (int)x : (int)x - 1;
        }

        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
        }

        private static double Gradient(int hash, double x)
        {
            var h = hash & 15;
            var grad = 1.0 + (h & 7);

            if ((h & 8) != 0)
            {
                grad = -grad;
            }

            return grad * x;
        }

        private static double Gradient(int hash, double x, double y)
        {
            var h = hash & 7;
            var u = h < 4 ? x : y;
            var v = h < 4 ? y : x;

            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0 * v : 2.0 * v);
        }

        private static double Gradient(int hash, double x, double y, double z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = h < 4 ? y : h == 12 || h == 14 ? x : z;

            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
        }
    }
}