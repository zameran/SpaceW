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

        Vector4 SampleColor(float x, float y)
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
}