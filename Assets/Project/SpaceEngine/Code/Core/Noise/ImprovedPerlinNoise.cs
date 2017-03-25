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
        private const short SIZE = 256;

        private readonly int[] Permutation = new int[SIZE + SIZE];

        public Texture2D PermutationTable1D { get; private set; }
        public Texture2D PermutationTable2D { get; private set; }
        public Texture2D Gradient2D { get; private set; }
        public Texture2D Gradient3D { get; private set; }

        public ImprovedPerlinNoise(int seed)
        {
            Random.InitState(seed);

            int i;

            for (i = 0; i < SIZE; i++)
            {
                Permutation[i] = i;
            }

            while (--i != 0)
            {
                var k = Permutation[i];
                var j = Random.Range(0, SIZE);

                Permutation[i] = Permutation[j];
                Permutation[j] = k;
            }

            for (i = 0; i < SIZE; i++)
            {
                Permutation[SIZE + i] = Permutation[i];
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
            if (PermutationTable1D) return;

            PermutationTable1D = new Texture2D(SIZE, 1, TextureFormat.Alpha8, false, true);
            PermutationTable1D.filterMode = FilterMode.Point;
            PermutationTable1D.wrapMode = TextureWrapMode.Repeat;

            for (short x = 0; x < SIZE; x++)
            {
                PermutationTable1D.SetPixel(x, 1, new Color(0, 0, 0, Permutation[x] / (float)(SIZE - 1)));
            }

            PermutationTable1D.Apply();
        }

        void LoadPermTable2D()
        {
            if (PermutationTable2D) return;

            PermutationTable2D = new Texture2D(SIZE, SIZE, TextureFormat.ARGB32, false, true);
            PermutationTable2D.filterMode = FilterMode.Point;
            PermutationTable2D.wrapMode = TextureWrapMode.Repeat;

            for (short x = 0; x < SIZE; x++)
            {
                for (short y = 0; y < SIZE; y++)
                {
                    int A = Permutation[x] + y;
                    int AA = Permutation[A];
                    int AB = Permutation[A + 1];

                    int B = Permutation[x + 1] + y;
                    int BA = Permutation[B];
                    int BB = Permutation[B + 1];

                    PermutationTable2D.SetPixel(x, y, new Color((float)AA / 255.0f, (float)AB / 255.0f, (float)BA / 255.0f, (float)BB / 255.0f));
                }
            }

            PermutationTable2D.Apply();
        }

        void LoadGradient2D()
        {
            if (Gradient2D) return;

            Gradient2D = new Texture2D(8, 1, TextureFormat.RGB24, false, true);
            Gradient2D.filterMode = FilterMode.Point;
            Gradient2D.wrapMode = TextureWrapMode.Repeat;

            for (short i = 0; i < 8; i++)
            {
                float R = (GRADIENT2[i * 2 + 0] + 1.0f) * 0.5f;
                float G = (GRADIENT2[i * 2 + 1] + 1.0f) * 0.5f;

                Gradient2D.SetPixel(i, 0, new Color(R, G, 0, 1));
            }

            Gradient2D.Apply();

        }

        void LoadGradient3D()
        {
            if (Gradient3D) return;

            Gradient3D = new Texture2D(SIZE, 1, TextureFormat.RGB24, false, true);
            Gradient3D.filterMode = FilterMode.Point;
            Gradient3D.wrapMode = TextureWrapMode.Repeat;

            for (short i = 0; i < SIZE; i++)
            {
                var idx = Permutation[i] % 16;

                float R = (GRADIENT3[idx * 3 + 0] + 1.0f) * 0.5f;
                float G = (GRADIENT3[idx * 3 + 1] + 1.0f) * 0.5f;
                float B = (GRADIENT3[idx * 3 + 2] + 1.0f) * 0.5f;

                Gradient3D.SetPixel(i, 0, new Color(R, G, B, 1));
            }

            Gradient3D.Apply();

        }

        private static float[] GRADIENT2 = new float[]
        {
            0, 1, 1, 1, 1, 0, 1, -1, 0, -1, -1, -1, -1, 0, -1, 1,
        };

        private static float[] GRADIENT3 = new float[]
        {
            1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1, 0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
        };
    }
}