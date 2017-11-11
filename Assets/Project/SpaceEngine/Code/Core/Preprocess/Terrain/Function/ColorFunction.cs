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
            int ix = (int)Mathf.Floor(x);
            int iy = (int)Mathf.Floor(y);

            x -= ix;
            y -= iy;

            float cx = 1.0f - x;
            float cy = 1.0f - y;

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
        TextureInputMapSpherical Source;

        Projection Projection;

        int DestinationSize;

        public SphericalColorFunction(TextureInputMapSpherical source, Projection projection, int destinationSize)
        {
            this.Source = source;
            this.Projection = projection;
            this.DestinationSize = destinationSize;
        }

        public Vector4 GetValue(int x, int y)
        {
            double sx, sy, sz;

            Projection(x, y, DestinationSize, out sx, out sy, out sz);

            double lon = Math.Atan2(sy, sx) + Math.PI;
            double lat = Math.Acos(sz);

            return SampleColor(lon, lat);
        }

        private Vector4 SampleColor(double lon, double lat)
        {
            lon = lon / Math.PI * (Source.Width / 2.0);
            lat = lat / Math.PI * Source.Height;

            int ilon = (int)Math.Floor(lon);
            int ilat = (int)Math.Floor(lat);

            lon -= ilon;
            lat -= ilat;

            double clon = 1.0 - lon;
            double clat = 1.0 - lat;

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