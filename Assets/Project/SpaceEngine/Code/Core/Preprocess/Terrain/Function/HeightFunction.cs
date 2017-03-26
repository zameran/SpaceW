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

        public float SampleHeight(float x, float y)
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
}