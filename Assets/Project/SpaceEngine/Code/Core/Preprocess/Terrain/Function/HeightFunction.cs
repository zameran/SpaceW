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
            int ix = (int)Mathf.Floor(x);
            int iy = (int)Mathf.Floor(y);

            x -= ix;
            y -= iy;

            float cx = 1.0f - x;
            float cy = 1.0f - y;
            float h1 = Source.Get(ix, iy).x;
            float h2 = Source.Get(ix + 1, iy).x;
            float h3 = Source.Get(ix, iy + 1).x;
            float h4 = Source.Get(ix + 1, iy + 1).x;

            return (h1 * cx + h2 * x) * cy + (h3 * cx + h4 * x) * y;
        }
    }

    public class SphericalHeightFunction : IHeightFunction2D
    {
        TextureInputMapSpherical Source;

        Projection Projection;

        int DestinationSize;

        public SphericalHeightFunction(TextureInputMapSpherical source, Projection projection, int destinationSize)
        {
            this.Source = source;
            this.Projection = projection;
            this.DestinationSize = destinationSize;
        }

        public float GetValue(int x, int y)
        {
            double sx, sy, sz;

            Projection(x, y, DestinationSize, out sx, out sy, out sz);

            double lon = Math.Atan2(sy, sx) + Math.PI;
            double lat = Math.Acos(sz);

            return SampleHeight(lon, lat);
        }

        private float SampleHeight(double lon, double lat)
        {
            lon = lon / Math.PI * (Source.Width / 2.0);
            lat = lat / Math.PI * Source.Height;

            int ilon = (int)Math.Floor(lon);
            int ilat = (int)Math.Floor(lat);

            lon -= ilon;
            lat -= ilat;

            double clon = 1.0 - lon;
            double clat = 1.0 - lat;

            float h1 = Source.Get((ilon + Source.Width) % Source.Width, ilat).x;
            float h2 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat).x;
            float h3 = Source.Get((ilon + Source.Width) % Source.Width, ilat + 1).x;
            float h4 = Source.Get((ilon + Source.Width + 1) % Source.Width, ilat + 1).x;

            return (float)((h1 * clon + h2 * lon) * clat + (h3 * clon + h4 * lon) * lat);
        }
    }
}