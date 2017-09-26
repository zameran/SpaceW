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

using UnityEngine;

namespace SpaceEngine.Core.Noise
{
    public class PerlinNoiseSimple
    {
        private const int B = 256;
        private readonly int[] Permutation = new int[B + B];

        public PerlinNoiseSimple(int seed)
        {
            Random.InitState(seed);

            int i;

            for (i = 0; i < B; i++)
            {
                Permutation[i] = i;
            }

            while (--i != 0)
            {
                var k = Permutation[i];
                var j = Random.Range(0, B);

                Permutation[i] = Permutation[j];
                Permutation[j] = k;
            }

            for (i = 0; i < B; i++)
            {
                Permutation[B + i] = Permutation[i];
            }
        }

        private static float FADE(float t)
        {
            return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
        }

        private static float LERP(float t, float a, float b)
        {
            return (a) + (t) * ((b) - (a));
        }

        private static float GRAD1(int hash, float x)
        {
            var h = hash & 15;
            var grad = 1.0f + (h & 7);

            if ((h & 8) != 0) grad = -grad;

            return (grad * x);
        }

        private static float GRAD2(int hash, float x, float y)
        {
            var h = hash & 7;
            var u = h < 4 ? x : y;
            var v = h < 4 ? y : x;

            return (((h & 1) != 0) ? -u : u) + (((h & 2) != 0) ? -2.0f * v : 2.0f * v);
        }


        private static float GRAD3(int hash, float x, float y, float z)
        {
            var h = hash & 15;
            var u = h < 8 ? x : y;
            var v = (h < 4) ? y : (h == 12 || h == 14) ? x : z;

            return (((h & 1) != 0) ? -u : u) + (((h & 2) != 0) ? -v : v);
        }

        /// <summary>
        /// Calculate noise value.
        /// </summary>
        /// <param name="x">Value.</param>
        /// <returns>Returns a noise value in [-0.5:0.5] range.</returns>
        public float Noise1D(float x)
        {
            var ix0 = (int)Mathf.Floor(x);          // Integer part of x
            var fx0 = x - ix0;                      // Fractional part of x
            var fx1 = fx0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff;

            ix0 = ix0 & 0xff;                       // Wrap to 0..255

            var s = FADE(fx0);

            var n0 = GRAD1(Permutation[ix0], fx0);
            var n1 = GRAD1(Permutation[ix1], fx1);

            return 0.188f * LERP(s, n0, n1);
        }

        /// <summary>
        /// Calculate noise value.
        /// </summary>
        /// <param name="x">Value on X axis.</param>
        /// <param name="y">Value on Y axis.</param>
        /// <returns>>Returns a noise value in [-0.75:0.75] range.</returns>
        public float Noise2D(float x, float y)
        {
            var ix0 = (int)Mathf.Floor(x);          // Integer part of x
            var iy0 = (int)Mathf.Floor(y);          // Integer part of y
            var fx0 = x - ix0;                      // Fractional part of x
            var fy0 = y - iy0;                      // Fractional part of y
            var fx1 = fx0 - 1.0f;
            var fy1 = fy0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff;             // Wrap to 0..255
            var iy1 = (iy0 + 1) & 0xff;

            ix0 = ix0 & 0xff;
            iy0 = iy0 & 0xff;

            var t = FADE(fy0);
            var s = FADE(fx0);

            var nx0 = GRAD2(Permutation[ix0 + Permutation[iy0]], fx0, fy0);
            var nx1 = GRAD2(Permutation[ix0 + Permutation[iy1]], fx0, fy1);

            var n0 = LERP(t, nx0, nx1);

            nx0 = GRAD2(Permutation[ix1 + Permutation[iy0]], fx1, fy0);
            nx1 = GRAD2(Permutation[ix1 + Permutation[iy1]], fx1, fy1);

            var n1 = LERP(t, nx0, nx1);

            return 0.507f * LERP(s, n0, n1);
        }

        /// <summary>
        /// Calculate noise value.
        /// </summary>
        /// <param name="x">Value on X axis.</param>
        /// <param name="y">Value on Y axis.</param>
        /// <param name="z">Value on Z axis.</param>
        /// <returns>>Returns a noise value in [-1.5:1.5] range.</returns>
        public float Noise3D(float x, float y, float z)
        {
            var ix0 = (int)Mathf.Floor(x);          // Integer part of x
            var iy0 = (int)Mathf.Floor(y);          // Integer part of y
            var iz0 = (int)Mathf.Floor(z);          // Integer part of z
            var fx0 = x - ix0;                      // Fractional part of x
            var fy0 = y - iy0;                      // Fractional part of y
            var fz0 = z - iz0;                      // Fractional part of z
            var fx1 = fx0 - 1.0f;
            var fy1 = fy0 - 1.0f;
            var fz1 = fz0 - 1.0f;
            var ix1 = (ix0 + 1) & 0xff;             // Wrap to 0..255
            var iy1 = (iy0 + 1) & 0xff;
            var iz1 = (iz0 + 1) & 0xff;

            ix0 = ix0 & 0xff;
            iy0 = iy0 & 0xff;
            iz0 = iz0 & 0xff;

            var r = FADE(fz0);
            var t = FADE(fy0);
            var s = FADE(fx0);

            var nxy0 = GRAD3(Permutation[ix0 + Permutation[iy0 + Permutation[iz0]]], fx0, fy0, fz0);
            var nxy1 = GRAD3(Permutation[ix0 + Permutation[iy0 + Permutation[iz1]]], fx0, fy0, fz1);
            var nx0 = LERP(r, nxy0, nxy1);

            nxy0 = GRAD3(Permutation[ix0 + Permutation[iy1 + Permutation[iz0]]], fx0, fy1, fz0);
            nxy1 = GRAD3(Permutation[ix0 + Permutation[iy1 + Permutation[iz1]]], fx0, fy1, fz1);

            var nx1 = LERP(r, nxy0, nxy1);

            var n0 = LERP(t, nx0, nx1);

            nxy0 = GRAD3(Permutation[ix1 + Permutation[iy0 + Permutation[iz0]]], fx1, fy0, fz0);
            nxy1 = GRAD3(Permutation[ix1 + Permutation[iy0 + Permutation[iz1]]], fx1, fy0, fz1);
            nx0 = LERP(r, nxy0, nxy1);

            nxy0 = GRAD3(Permutation[ix1 + Permutation[iy1 + Permutation[iz0]]], fx1, fy1, fz0);
            nxy1 = GRAD3(Permutation[ix1 + Permutation[iy1 + Permutation[iz1]]], fx1, fy1, fz1);
            nx1 = LERP(r, nxy0, nxy1);

            var n1 = LERP(t, nx0, nx1);

            return 0.936f * LERP(s, n0, n1);
        }
    }
}