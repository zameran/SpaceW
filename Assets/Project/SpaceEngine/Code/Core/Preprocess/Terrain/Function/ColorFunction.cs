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

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public class PlaneColorFunction : IColorFunction2D
    {
        InputMap Source;

        int DestinationSize;

        public PlaneColorFunction(InputMap source, int destinationSize)
        {
            this.Source = source;
            this.DestinationSize = destinationSize;
        }

        public Vector4 GetValue(int x, int y)
        {
            return SampleColor((float)x / (float)DestinationSize * (float)Source.Width, (float)y / (float)DestinationSize * (float)Source.Height);
        }

        private Vector4 SampleColor(float x, float y)
        {
            var ix = (int)Mathf.Floor(x);
            var iy = (int)Mathf.Floor(y);

            x -= ix;
            y -= iy;

            var cx = 1.0f - x;
            var cy = 1.0f - y;

            var c1 = Source.Get(ix, iy);
            var c2 = Source.Get(ix + 1, iy);
            var c3 = Source.Get(ix, iy + 1);
            var c4 = Source.Get(ix + 1, iy + 1);

            var color = new Vector4
            (
                (c1.x * cx + c2.x * x) * cy + (c3.x * cx + c4.x * x) * y,
                (c1.y * cx + c2.y * x) * cy + (c3.y * cx + c4.y * x) * y,
                (c1.z * cx + c2.z * x) * cy + (c3.z * cx + c4.z * x) * y,
                (c1.w * cx + c2.w * x) * cy + (c3.w * cx + c4.w * x) * y
            );

            return color;
        }
    }

    public class SphericalColorFunction : IColorFunction2D
    {
        InputMap Source;

        Projection Projection;

        int DestinationSize;

        public SphericalColorFunction(InputMap source, Projection projection, int destinationSize)
        {
            this.Source = source;
            this.Projection = projection;
            this.DestinationSize = destinationSize;
        }

        public Vector4 GetValue(int x, int y)
        {
            double sx, sy, sz;

            Projection(x, y, DestinationSize, out sx, out sy, out sz);

            var lon = Math.Atan2(sy, sx) + Math.PI;
            var lat = Math.Acos(sz);

            return SampleColor(lon, lat);
        }

        private Vector4 SampleColor(double lon, double lat)
        {
            lon = lon / Math.PI * (Source.Width / 2.0);
            lat = lat / Math.PI * Source.Height;

            var ilon = (int)Math.Floor(lon);
            var ilat = (int)Math.Floor(lat);

            lon -= ilon;
            lat -= ilat;

            var clon = 1.0 - lon;
            var clat = 1.0 - lat;

            var c1 = Source.Get((ilon + Source.Width) % Source.Width, ilat);
            var c2 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat);
            var c3 = Source.Get((ilon + Source.Width) % Source.Width, ilat + 1);
            var c4 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat + 1);

            var color = new Vector4
            (
                (float)((c1.x * clon + c2.x * lon) * clat + (c3.x * clon + c4.x * lon) * lat),
                (float)((c1.y * clon + c2.y * lon) * clat + (c3.y * clon + c4.y * lon) * lat),
                (float)((c1.z * clon + c2.z * lon) * clat + (c3.z * clon + c4.z * lon) * lat),
                (float)((c1.w * clon + c2.w * lon) * clat + (c3.w * clon + c4.w * lon) * lat)
            );

            return color;
        }
    }
}