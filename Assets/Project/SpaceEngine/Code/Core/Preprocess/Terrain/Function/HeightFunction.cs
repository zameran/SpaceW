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

namespace SpaceEngine.Core.Preprocess.Terrain.Function
{
    public class PlaneHeightFunction : IHeightFunction2D
    {
        private readonly int DestinationSize;
        private readonly InputMap.InputMap Source;

        public PlaneHeightFunction(InputMap.InputMap source, int destinationSize)
        {
            Source = source;
            DestinationSize = destinationSize;
        }

        public float GetValue(int x, int y)
        {
            return SampleHeight(x / (float)DestinationSize * Source.Width, y / (float)DestinationSize * Source.Height);
        }

        private float SampleHeight(float x, float y)
        {
            var ix = (int)Mathf.Floor(x);
            var iy = (int)Mathf.Floor(y);

            x -= ix;
            y -= iy;

            var cx = 1.0f - x;
            var cy = 1.0f - y;
            var h1 = Source.Get(ix, iy).x;
            var h2 = Source.Get(ix + 1, iy).x;
            var h3 = Source.Get(ix, iy + 1).x;
            var h4 = Source.Get(ix + 1, iy + 1).x;

            return (h1 * cx + h2 * x) * cy + (h3 * cx + h4 * x) * y;
        }
    }

    public class SphericalHeightFunction : IHeightFunction2D
    {
        private readonly int DestinationSize;

        private readonly Projection Projection;
        private readonly InputMap.InputMap Source;

        public SphericalHeightFunction(InputMap.InputMap source, Projection projection, int destinationSize)
        {
            Source = source;
            Projection = projection;
            DestinationSize = destinationSize;
        }

        public float GetValue(int x, int y)
        {
            double sx, sy, sz;

            Projection(x, y, DestinationSize, out sx, out sy, out sz);

            var lon = Math.Atan2(sy, sx) + Math.PI;
            var lat = Math.Acos(sz);

            return SampleHeight(lon, lat);
        }

        private float SampleHeight(double lon, double lat)
        {
            lon = lon / Math.PI * (Source.Width / 2.0);
            lat = lat / Math.PI * Source.Height;

            var ilon = (int)Math.Floor(lon);
            var ilat = (int)Math.Floor(lat);

            lon -= ilon;
            lat -= ilat;

            var clon = 1.0 - lon;
            var clat = 1.0 - lat;

            var h1 = Source.Get((ilon + Source.Width) % Source.Width, ilat).x;
            var h2 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat).x;
            var h3 = Source.Get((ilon + Source.Width) % Source.Width, ilat + 1).x;
            var h4 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat + 1).x;

            return (float)((h1 * clon + h2 * lon) * clat + (h3 * clon + h4 * lon) * lat);
        }
    }
}