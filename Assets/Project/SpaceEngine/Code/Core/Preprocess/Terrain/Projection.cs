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
// Creation Date: 2017.09.28
// Creation Time: 11:18 AM
// Creator: zameran
#endregion

using System;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public delegate void Projection(int x, int y, int w, out double sx, out double sy, out double sz);

    public static class ProjectionHelper
    {
        public static void Projection1(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = yl / l;
            sz = 1.0 / l;
        }

        public static void Projection2(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -1.0 / l;
            sz = yl / l;
        }

        public static void Projection3(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = 1.0 / l;
            sy = xl / l;
            sz = yl / l;
        }

        public static void Projection4(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -xl / l;
            sy = 1.0 / l;
            sz = yl / l;
        }

        public static void Projection5(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -1.0 / l;
            sy = -xl / l;
            sz = yl / l;
        }

        public static void Projection6(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max((double)Math.Min(x, w), 0.0)) / w * 2.0 - 1.0;
            var yl = (Math.Max((double)Math.Min(y, w), 0.0)) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -yl / l;
            sz = -1.0 / l;
        }

        public static void Projection1f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = yl / l;
            sz = 1.0 / l;
        }

        public static void Projection2f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -1.0 / l;
            sz = yl / l;
        }

        public static void Projection3f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = 1.0 / l;
            sy = xl / l;
            sz = yl / l;
        }

        public static void Projection4f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -xl / l;
            sy = 1.0 / l;
            sz = yl / l;
        }

        public static void Projection5f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -1.0 / l;
            sy = -xl / l;
            sz = yl / l;
        }

        public static void Projection6f(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = ((double)x / (double)w) * 2.0 - 1.0;
            var yl = ((double)y / (double)w) * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -yl / l;
            sz = -1.0 / l;
        }

        public static void Projection1h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = yl / l;
            sz = 1.0 / l;
        }

        public static void Projection2h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -1.0 / l;
            sz = yl / l;
        }

        public static void Projection3h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = 1.0 / l;
            sy = xl / l;
            sz = yl / l;
        }

        public static void Projection4h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -xl / l;
            sy = 1.0 / l;
            sz = yl / l;
        }

        public static void Projection5h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = -1.0 / l;
            sy = -xl / l;
            sz = yl / l;
        }

        public static void Projection6h(int x, int y, int w, out double sx, out double sy, out double sz)
        {
            var xl = (Math.Max(Math.Min(x, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var yl = (Math.Max(Math.Min(y, w - 1), 0) + 0.5) / w * 2.0 - 1.0;
            var l = Math.Sqrt(xl * xl + yl * yl + 1.0);

            sx = xl / l;
            sy = -yl / l;
            sz = -1.0 / l;
        }
    }
}