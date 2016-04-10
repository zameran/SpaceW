#region License
/*
 * Procedural planet renderer.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Justin Hawkins 2014.
 * Modified by Denis Ovchinnikov 2015-2016
 */
#endregion

namespace UnityEngine
{
    using System;

    public class MathUtility
    {
        public static readonly double Rad2Deg = 180.0 / Math.PI;
        public static readonly double Deg2Rad = Math.PI / 180.0;

        public static double Safe_Acos(double r)
        {
            return Math.Acos(Math.Min(1.0, Math.Max(-1.0, r)));
        }

        public static double Safe_Asin(double r)
        {
            return Math.Asin(Math.Min(1.0, Math.Max(-1.0, r)));
        }

        public static float Safe_Acos(float r)
        {
            return Mathf.Acos(Mathf.Min(1.0f, Mathf.Max(-1.0f, r)));
        }

        public static float Safe_Asin(float r)
        {
            return Mathf.Asin(Mathf.Min(1.0f, Mathf.Max(-1.0f, r)));
        }

        public static bool IsFinite(float f)
        {
            return !(float.IsInfinity(f) || float.IsNaN(f));
        }

        public static bool IsFinite(double f)
        {
            return !(double.IsInfinity(f) || double.IsNaN(f));
        }

        public static double HorizontalFovToVerticalFov(double hfov, double screenWidth, double screenHeight)
        {
            return 2.0 * Math.Atan(Math.Tan(hfov * 0.5 * Deg2Rad) * screenHeight / screenWidth) * Rad2Deg;
        }

        public static double VerticalFovToHorizontalFov(double vfov, double screenWidth, double screenHeight)
        {
            return 2.0 * Math.Atan(Math.Tan(vfov * 0.5 * Deg2Rad) * screenWidth / screenHeight) * Rad2Deg;
        }

        private static bool USE_LAST = false;
        private static float Y2;

        public static void ResetGRandom()
        {
            USE_LAST = false;
        }

        public static float GRandom(float mean, float stdDeviation)
        {
            float x1, x2, w, y1;

            if (USE_LAST)
            {
                y1 = Y2;
                USE_LAST = false;
            }
            else
            {
                do
                {
                    x1 = 2.0f * UnityEngine.Random.value - 1.0f;
                    x2 = 2.0f * UnityEngine.Random.value - 1.0f;
                    w = x1 * x1 + x2 * x2;
                }
                while (w >= 1.0f);

                w = Mathf.Sqrt((-2.0f * Mathf.Log(w)) / w);
                y1 = x1 * w;
                Y2 = x2 * w;
                USE_LAST = true;
            }

            return mean + y1 * stdDeviation;
        }
    } 
}