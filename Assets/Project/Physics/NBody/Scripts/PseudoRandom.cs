using UnityEngine;

using System;
using System.Runtime.InteropServices;

namespace NBody
{
    public static class PseudoRandom
    {
        private static ulong seed = ((ulong)DateTime.Now.ToFileTime());

        static PseudoRandom()
        {
            for (int i = 0; i < 10; i++)
            {
                UInt64();
            }
        }

        public static bool Boolean()
        {
            return ((UInt64() & ((ulong)1L)) == 0L);
        }

        public static Vector DirectionVector(double magnitude = 1)
        {
            Vector vector;

            do
            {
                vector = new Vector(Double(-1.0, 1.0), Double(-1.0, 1.0), Double(-1.0, 1.0));
            }

            while (vector.Magnitude() == 0.0);

            return (Vector)((magnitude / vector.Magnitude()) * vector);
        }

        public static double Double()
        {
            return (UInt64() * 5.4210108624275222E-20);
        }

        public static double Double(double a, double b = 0)
        {
            return (a + (Double() * (b - a)));
        }

        public static int Int32(int a, int b = 0)
        {
            double num = 0.5 * Math.Sign((double)(a - b));

            return (int)Math.Round(Double(a + num, b - num));
        }

        public static ulong UInt64()
        {
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 0x11;

            return seed;
        }

        public static Vector Vector(double maximumMagnitude = 1)
        {
            return (Vector)(Double(maximumMagnitude, 0.0) * DirectionVector(1.0));
        }
    }
}