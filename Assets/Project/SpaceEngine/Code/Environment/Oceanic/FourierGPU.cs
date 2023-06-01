#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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

using SpaceEngine.Utilities;

using System;

namespace UnityEngine
{
    public class FourierGPU
    {
        const int PASS_X_1 = 0, PASS_Y_1 = 1;
        const int PASS_X_2 = 2, PASS_Y_2 = 3;
        const int PASS_X_3 = 4, PASS_Y_3 = 5;
        const int PASS_X_4 = 6, PASS_Y_4 = 7;

        public int Size { get; private set; }
        public int Passes { get; private set; }

        readonly Texture2D[] ButterFlyLookupTable = null;
        readonly Material FourierMaterial;

        public FourierGPU(int size, Shader shader)
        {
            if (size > 256)
            {
                Debug.Log("FourierGPU: Fourier grid size must not be greater than 256, changing to 256...");
                size = 256;
            }

            if (!Mathf.IsPowerOfTwo(size))
            {
                Debug.Log("FourierGPU: fourier grid size must be pow2 number, changing to nearest pow2 number...");
                size = Mathf.NextPowerOfTwo(size);
            }

            FourierMaterial = new Material(shader);

            Size = size; // Must be pow2 num
            Passes = (int)(Mathf.Log((float)size) / Mathf.Log(2.0f));

            ButterFlyLookupTable = new Texture2D[Passes];

            ComputeButterflyLookupTable();

            FourierMaterial.SetFloat("_Size", (float)size);
        }

        private int BitReverse(int i)
        {
            var Sum = 0;
            var W = 1;
            var M = Size / 2;

            while (M != 0)
            {
                var j = ((i & M) > M - 1) ? 1 : 0;
                Sum += j * W;
                W *= 2;
                M /= 2;
            }

            return Sum;
        }

        private Texture2D Make1DTex(int i)
        {
            var tex = new Texture2D(Size, 1, TextureFormat.ARGB32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            return tex;
        }

        private void ComputeButterflyLookupTable()
        {
            for (var i = 0; i < Passes; i++)
            {
                var nBlocks = (int)Mathf.Pow(2, Passes - 1 - i);
                var nHInputs = (int)Mathf.Pow(2, i);

                ButterFlyLookupTable[i] = Make1DTex(i);

                for (var j = 0; j < nBlocks; j++)
                {
                    for (var k = 0; k < nHInputs; k++)
                    {
                        int i1, i2, j1, j2;

                        if (i == 0)
                        {
                            i1 = j * nHInputs * 2 + k;
                            i2 = j * nHInputs * 2 + nHInputs + k;
                            j1 = BitReverse(i1);
                            j2 = BitReverse(i2);
                        }
                        else
                        {
                            i1 = j * nHInputs * 2 + k;
                            i2 = j * nHInputs * 2 + nHInputs + k;
                            j1 = i1;
                            j2 = i2;
                        }

                        ButterFlyLookupTable[i].SetPixel(i1, 0, new Color((float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k * nBlocks) / 255.0f, 0));
                        ButterFlyLookupTable[i].SetPixel(i2, 0, new Color((float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k * nBlocks) / 255.0f, 1));

                    }
                }

                ButterFlyLookupTable[i].name = $"ButterFlyLookupTable_{i}_{Random.Range(float.MinValue, float.MaxValue)}";
                ButterFlyLookupTable[i].Apply();
            }
        }

        public int PeformFFT(RenderTexture[] data0)
        {
            if (ButterFlyLookupTable == null) return -1;

            var pass0 = new RenderTexture[] { data0[0] };
            var pass1 = new RenderTexture[] { data0[1] };

            int i;
            var idx = 0;
            int idx1;
            var j = 0;

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_X_1);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_X_1);
            }

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_Y_1);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_Y_1);
            }

            return idx;
        }

        public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1)
        {
            if (ButterFlyLookupTable == null) return -1;

            if (SystemInfo.supportedRenderTargetCount < 2)
                throw new InvalidOperationException("System does not support at least 2 render targets");

            var pass0 = new RenderTexture[] { data0[0], data1[0] };
            var pass1 = new RenderTexture[] { data0[1], data1[1] };

            int i;
            var idx = 0;
            int idx1;
            var j = 0;

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_X_2);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_X_2);
            }

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_Y_2);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_Y_2);
            }

            return idx;
        }

        public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
        {
            if (ButterFlyLookupTable == null) return -1;

            if (SystemInfo.supportedRenderTargetCount < 3)
                throw new InvalidOperationException("System does not support at least 3 render targets");

            var pass0 = new RenderTexture[] { data0[0], data1[0], data2[0] };
            var pass1 = new RenderTexture[] { data0[1], data1[1], data2[1] };

            int i;
            var idx = 0;
            int idx1;
            var j = 0;

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer2", data2[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_X_3);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_X_3);
            }

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer2", data2[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_Y_3);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_Y_3);
            }

            return idx;
        }

        public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2, RenderTexture[] data3)
        {
            if (ButterFlyLookupTable == null) return -1;

            if (SystemInfo.supportedRenderTargetCount < 4)
                throw new InvalidOperationException("System does not support at least 4 render targets");

            var pass0 = new RenderTexture[] { data0[0], data1[0], data2[0], data3[0] };
            var pass1 = new RenderTexture[] { data0[1], data1[1], data2[1], data3[1] };

            int i;
            var idx = 0; int idx1;
            var j = 0;

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer2", data2[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer3", data3[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_X_4);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_X_4);
            }

            for (i = 0; i < Passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                FourierMaterial.SetTexture("_ButterFlyLookUp", ButterFlyLookupTable[i]);

                FourierMaterial.SetTexture("_ReadBuffer0", data0[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer1", data1[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer2", data2[idx1]);
                FourierMaterial.SetTexture("_ReadBuffer3", data3[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, FourierMaterial, PASS_Y_4);
                else
                    RTUtility.MultiTargetBlit(pass1, FourierMaterial, PASS_Y_4);
            }

            return idx;
        }
    }
}