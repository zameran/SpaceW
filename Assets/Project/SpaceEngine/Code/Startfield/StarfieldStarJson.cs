using System.Runtime.InteropServices;

using UnityEngine;

namespace SpaceEngine.Startfield
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct StarfieldStarJson
    {
        public float X;
        public float Y;
        public float Z;

        public float R;
        public float G;
        public float B;
        public float A;

        public StarfieldStarJson(float X, float Y, float Z, float R, float G, float B, float A)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;

            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }
    }
}