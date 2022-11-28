using System;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public class PlaneHeightFunction : IHeightFunction2D
    {
        InputMap Source;

        int DestinationSize;

        public PlaneHeightFunction(InputMap source, int destinationSize)
        {
            this.Source = source;
            this.DestinationSize = destinationSize;
        }

        public float GetValue(int x, int y)
        {
            return SampleHeight((float)x / (float)DestinationSize * (float)Source.Width, (float)y / (float)DestinationSize * (float)Source.Height);
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
        InputMap Source;

        Projection Projection;

        int DestinationSize;

        public SphericalHeightFunction(InputMap source, Projection projection, int destinationSize)
        {
            this.Source = source;
            this.Projection = projection;
            this.DestinationSize = destinationSize;
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