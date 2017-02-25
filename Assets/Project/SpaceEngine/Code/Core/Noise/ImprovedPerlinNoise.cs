#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;

using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceEngine.Core.Noise
{
    [Serializable]
    public sealed class ImprovedPerlinNoise
    {
        private const int SIZE = 256;

        private readonly int[] permutation = new int[SIZE + SIZE];

        Texture2D permutationTable1D;
        Texture2D permutationTable2D;
        Texture2D gradient2D;
        Texture2D gradient3D;

        public Texture2D GetPermutationTable1D()
        {
            return permutationTable1D;
        }

        public Texture2D GetPermutationTable2D()
        {
            return permutationTable2D;
        }

        public Texture2D GetGradient2D()
        {
            return gradient2D;
        }

        public Texture2D GetGradient3D()
        {
            return gradient3D;
        }

        public ImprovedPerlinNoise(int seed)
        {
            Random.InitState(seed);

            int i;

            for (i = 0; i < SIZE; i++)
            {
                permutation[i] = i;
            }

            while (--i != 0)
            {
                var k = permutation[i];
                var j = Random.Range(0, SIZE);

                permutation[i] = permutation[j];
                permutation[j] = k;
            }

            for (i = 0; i < SIZE; i++)
            {
                permutation[SIZE + i] = permutation[i];
            }

        }

        public void LoadResourcesFor2DNoise()
        {
            LoadPermTable1D();
            LoadGradient2D();
        }

        public void LoadResourcesFor3DNoise()
        {
            LoadPermTable2D();
            LoadGradient3D();
        }

        void LoadPermTable1D()
        {
            if (permutationTable1D) return;

            permutationTable1D = new Texture2D(SIZE, 1, TextureFormat.Alpha8, false, true);
            permutationTable1D.filterMode = FilterMode.Point;
            permutationTable1D.wrapMode = TextureWrapMode.Repeat;

            for (short x = 0; x < SIZE; x++)
            {
                permutationTable1D.SetPixel(x, 1, new Color(0, 0, 0, permutation[x] / (float)(SIZE - 1)));
            }

            permutationTable1D.Apply();
        }

        void LoadPermTable2D()
        {
            if (permutationTable2D) return;

            permutationTable2D = new Texture2D(SIZE, SIZE, TextureFormat.ARGB32, false, true);
            permutationTable2D.filterMode = FilterMode.Point;
            permutationTable2D.wrapMode = TextureWrapMode.Repeat;

            for (short x = 0; x < SIZE; x++)
            {
                for (short y = 0; y < SIZE; y++)
                {
                    int A = permutation[x] + y;
                    int AA = permutation[A];
                    int AB = permutation[A + 1];

                    int B = permutation[x + 1] + y;
                    int BA = permutation[B];
                    int BB = permutation[B + 1];

                    permutationTable2D.SetPixel(x, y, new Color((float)AA / 255.0f, (float)AB / 255.0f, (float)BA / 255.0f, (float)BB / 255.0f));
                }
            }

            permutationTable2D.Apply();
        }

        void LoadGradient2D()
        {
            if (gradient2D) return;

            gradient2D = new Texture2D(8, 1, TextureFormat.RGB24, false, true);
            gradient2D.filterMode = FilterMode.Point;
            gradient2D.wrapMode = TextureWrapMode.Repeat;

            for (short i = 0; i < 8; i++)
            {
                float R = (GRADIENT2[i * 2 + 0] + 1.0f) * 0.5f;
                float G = (GRADIENT2[i * 2 + 1] + 1.0f) * 0.5f;

                gradient2D.SetPixel(i, 0, new Color(R, G, 0, 1));
            }

            gradient2D.Apply();

        }

        void LoadGradient3D()
        {
            if (gradient3D) return;

            gradient3D = new Texture2D(SIZE, 1, TextureFormat.RGB24, false, true);
            gradient3D.filterMode = FilterMode.Point;
            gradient3D.wrapMode = TextureWrapMode.Repeat;

            for (short i = 0; i < SIZE; i++)
            {
                var idx = permutation[i] % 16;

                float R = (GRADIENT3[idx * 3 + 0] + 1.0f) * 0.5f;
                float G = (GRADIENT3[idx * 3 + 1] + 1.0f) * 0.5f;
                float B = (GRADIENT3[idx * 3 + 2] + 1.0f) * 0.5f;

                gradient3D.SetPixel(i, 0, new Color(R, G, B, 1));
            }

            gradient3D.Apply();

        }

        static float[] GRADIENT2 = new float[]
        {
            0, 1, 1, 1, 1, 0, 1, -1, 0, -1, -1, -1, -1, 0, -1, 1,
        };

        static float[] GRADIENT3 = new float[]
        {
            1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1, 0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
        };
    }
}