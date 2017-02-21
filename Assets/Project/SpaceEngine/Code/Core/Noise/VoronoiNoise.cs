using UnityEngine;

namespace SpaceEngine.Core.Noise
{
    public static class VoronoiNoise
    {
        [System.Serializable]
        public enum DistanceFunction
        {
            EUCLIDIAN,
            MANHATTAN,
            CHEBYSHEV
        }

        [System.Serializable]
        public enum CombineFunction
        {
            d0,
            d1,
            d2
        }

        public static float CellNoise(Vector3 position, float seed, DistanceFunction distanceFunction = DistanceFunction.EUCLIDIAN, CombineFunction combineFunction = CombineFunction.d0)
        {
            var gain = 1.0f;
            var value = 0.0f;
            var octaves = 4;
            var frequency = 2.0f;
            var amplitude = 0.5f;

            for (byte i = 0; i < octaves; i++)
            {
                value += Noise3D(new Vector3(position.x * gain * frequency, position.y * gain * frequency, position.z * gain * frequency), seed, distanceFunction, combineFunction) * amplitude / gain;
                gain *= 2f;
            }

            return value;
        }

        public static float Noise3D(Vector3 input, float seed, DistanceFunction distanceFunction = DistanceFunction.EUCLIDIAN, CombineFunction combineFunction = CombineFunction.d0)
        {
            var randomDiff = Vector3.zero;
            var featurePoint = Vector3.zero;

            int cubeX, cubeY, cubeZ;

            float[] distanceArray = new float[3];

            for (int i = 0; i < distanceArray.Length; i++)
                distanceArray[i] = Mathf.Infinity;

            var evalCubeX = Mathf.FloorToInt(input.x);
            var evalCubeY = Mathf.FloorToInt(input.y);
            var evalCubeZ = Mathf.FloorToInt(input.z);

            for (sbyte i = -1; i < 2; ++i)
            {
                for (sbyte j = -1; j < 2; ++j)
                {
                    for (sbyte k = -1; k < 2; ++k)
                    {
                        cubeX = evalCubeX + i;
                        cubeY = evalCubeY + j;
                        cubeZ = evalCubeZ + k;

                        var lastRandom = LCGRandom(Hash((uint)(cubeX + seed), (uint)(cubeY), (uint)(cubeZ)));
                        uint numberFeaturePoints = 1;

                        for (uint l = 0; l < numberFeaturePoints; ++l)
                        {
                            lastRandom = LCGRandom(lastRandom);
                            randomDiff.x = (float)lastRandom / 0x100000000;

                            lastRandom = LCGRandom(lastRandom);
                            randomDiff.y = (float)lastRandom / 0x100000000;

                            lastRandom = LCGRandom(lastRandom);
                            randomDiff.z = (float)lastRandom / 0x100000000;

                            featurePoint.x = randomDiff.x + (float)cubeX;
                            featurePoint.y = randomDiff.y + (float)cubeY;
                            featurePoint.z = randomDiff.z + (float)cubeZ;

                            switch (distanceFunction)
                            {
                                case DistanceFunction.EUCLIDIAN:
                                    Insert(distanceArray, EuclidianDistanceFunc3(input, featurePoint));
                                    break;
                                case DistanceFunction.MANHATTAN:
                                    Insert(distanceArray, ManhattanDistanceFunc3(input, featurePoint));
                                    break;
                                case DistanceFunction.CHEBYSHEV:
                                    Insert(distanceArray, ChebyshevDistanceFunc3(input, featurePoint));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            var combine = 0f;

            switch (combineFunction)
            {
                case CombineFunction.d0:
                    combine = CombineFunc_D0(distanceArray);
                    break;
                case CombineFunction.d1:
                    combine = CombineFunc_D1_D0(distanceArray);
                    break;
                case CombineFunction.d2:
                    combine = CombineFunc_D2_D0(distanceArray);
                    break;
                default:
                    combine = 0.0f;
                    break;
            }

            return Mathf.Clamp(combine * 2f - 1f, -1f, 1f);
        }

        private static float EuclidianDistanceFunc3(Vector3 p1, Vector3 p2)
        {
            return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
        }

        private static float ManhattanDistanceFunc3(Vector3 p1, Vector3 p2)
        {
            return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
        }

        private static float ChebyshevDistanceFunc3(Vector3 p1, Vector3 p2)
        {
            var diff = p1 - p2;

            return Mathf.Max(Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y)), Mathf.Abs(diff.z));
        }

        private static float CombineFunc_D0(float[] arr)
        {
            var value = 0.0f;

            for (int i = 0; i < arr.Length; i++)
            {
                value += arr[i];
            }

            return arr[0];
        }

        private static float CombineFunc_D1_D0(float[] arr)
        {
            return arr[1] - arr[0];
        }

        private static float CombineFunc_D2_D0(float[] arr)
        {
            return arr[2] - arr[0];
        }

        private static void Insert(float[] arr, float value)
        {
            var temp = 0.0f;

            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (value > arr[i]) break;

                temp = arr[i];
                arr[i] = value;

                if (i + 1 < arr.Length) arr[i + 1] = temp;
            }
        }

        private static uint LCGRandom(uint lastValue)
        {
            return (uint)((1103515245u * lastValue + 12345u) % 0x100000000u);
        }

        private const uint OFFSET_BASIS = 2166136261;
        private const uint FNV_PRIME = 16777619;

        private static uint Hash(uint i, uint j, uint k)
        {
            return (uint)((((((OFFSET_BASIS ^ (uint)i) * FNV_PRIME) ^ (uint)j) * FNV_PRIME) ^ (uint)k) * FNV_PRIME);
        }

        private static uint Hash(uint i, uint j)
        {
            return (uint)((((OFFSET_BASIS ^ (uint)i) * FNV_PRIME) ^ (uint)j) * FNV_PRIME);
        }
    }
}