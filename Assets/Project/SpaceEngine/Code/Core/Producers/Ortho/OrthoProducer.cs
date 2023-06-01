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
// Creation Date: 2017.03.28
// Creation Time: 2:18 PM
// Creator: zameran

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Noise;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Helpers;
using SpaceEngine.Managers;
using SpaceEngine.Tools;
using UnityEngine;

namespace SpaceEngine.Core.Producers.Ortho
{
    public class OrthoProducer : TileProducer
    {
        [SerializeField]
        private GameObject OrthoCpuProducerGameObject;

        [SerializeField]
        private Material UpSampleMaterial;

        [SerializeField]
        private UnityEngine.Color RootNoiseColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        [SerializeField]
        private UnityEngine.Color NoiseColor = new(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        ///     Maximum quadtree level, or -1 to allow any level.
        /// </summary>
        [SerializeField]
        private int MaxLevel = -1;

        [SerializeField]
        private bool HSV = true;

        [SerializeField]
        private float[] NoiseAmplitudes = { 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };

        private PerlinNoise Noise;

        private OrthoCPUProducer OrthoCPUProducer;

        private Texture2D ResidualTexture;

        private Uniforms uniforms;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ResidualTexture != null)
            {
                Helper.Destroy(ResidualTexture);
            }
        }

        public override void InitNode()
        {
            base.InitNode();

            if (OrthoCpuProducerGameObject != null)
            {
                if (OrthoCPUProducer == null)
                {
                    OrthoCPUProducer = OrthoCpuProducerGameObject.GetComponent<OrthoCPUProducer>();
                }
            }

            var tileSize = Cache.GetStorage(0).TileSize;

            if (OrthoCPUProducer != null && OrthoCPUProducer.GetTileSize(0) != tileSize)
            {
                throw new InvalidParameterException("ortho CPU tile size must match ortho tile size");
            }

            if (!(Cache.GetStorage(0) is GPUTileStorage))
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }

            uniforms = new Uniforms();

            Noise = new PerlinNoise();

            ResidualTexture = new Texture2D(tileSize, tileSize, TextureFormat.ARGB32, false);
            ResidualTexture.wrapMode = TextureWrapMode.Clamp;
            ResidualTexture.filterMode = FilterMode.Point;
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override bool HasTile(int level, int tx, int ty)
        {
            return MaxLevel == -1 || level <= MaxLevel;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - GetBorder() * 2;

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = tileWidth;
            tileWSD.y = (float)rootQuadSize / (1 << level) / tileSize;
            tileWSD.z = tileSize / (float)(TerrainNode.ParentBody.GridResolution - 1);
            tileWSD.w = 0.0f;

            GPUTileStorage.GPUSlot parentGpuSlot = null;

            if (level > 0)
            {
                var parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);

                if (parentTile != null)
                {
                    parentGpuSlot = parentTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                }
                else
                {
                    throw new MissingTileException($"Find parent tile failed! {level - 1}:{tx / 2}-{ty / 2}");
                }
            }

            if (parentGpuSlot == null && level > 0)
            {
                throw new NullReferenceException("parentGpuSlot");
            }

            UpSampleMaterial.SetVector(uniforms.tileWSD, tileWSD);

            if (level > 0)
            {
                var tex = parentGpuSlot.Texture;

                UpSampleMaterial.SetTexture(uniforms.coarseLevelSampler, tex);

                var dx = tx % 2 * (tileSize / 2.0f);
                var dy = ty % 2 * (tileSize / 2.0f);

                var coarseLevelOSL = new Vector4((dx + 0.5f) / tex.width, (dy + 0.5f) / tex.height, 1.0f / tex.width, 0.0f);

                UpSampleMaterial.SetVector(uniforms.coarseLevelOSL, coarseLevelOSL);
            }
            else
            {
                UpSampleMaterial.SetVector(uniforms.coarseLevelOSL, new Vector4(-1.0f, -1.0f, -1.0f, -1.0f));
            }

            if (OrthoCPUProducer != null && OrthoCPUProducer.HasTile(level, tx, ty))
            {
                var orthoCPUTile = OrthoCPUProducer.FindTile(level, tx, ty, false, true);

                CPUTileStorage.CPUSlot<byte> orthoCPUSlot = null;

                if (orthoCPUTile != null)
                {
                    orthoCPUSlot = orthoCPUTile.GetSlot(0) as CPUTileStorage.CPUSlot<byte>;
                }
                else
                {
                    throw new MissingTileException("Find orthoCPU tile failed");
                }

                if (orthoCPUSlot == null)
                {
                    throw new NullReferenceException("orthoCPUSlot");
                }

                var channels = OrthoCPUProducer.Channels;
                var color = new Color32();
                var data = orthoCPUSlot.Data;

                for (var x = 0; x < tileWidth; x++)
                {
                    for (var y = 0; y < tileWidth; y++)
                    {
                        color.r = data[(x + y * tileWidth) * channels];

                        if (channels > 1)
                        {
                            color.g = data[(x + y * tileWidth) * channels + 1];
                        }

                        if (channels > 2)
                        {
                            color.b = data[(x + y * tileWidth) * channels + 2];
                        }

                        if (channels > 3)
                        {
                            color.a = data[(x + y * tileWidth) * channels + 3];
                        }

                        ResidualTexture.SetPixel(x, y, color);
                    }
                }

                ResidualTexture.Apply();

                UpSampleMaterial.SetTexture(uniforms.residualSampler, ResidualTexture);
                UpSampleMaterial.SetVector(uniforms.residualOSH, new Vector4(0.5f / tileWidth, 0.5f / tileWidth, 1.0f / tileWidth, 0.0f));
            }
            else
            {
                UpSampleMaterial.SetTexture(uniforms.residualSampler, null);
                UpSampleMaterial.SetVector(uniforms.residualOSH, new Vector4(-1, -1, -1, -1));
            }

            var rs = level < NoiseAmplitudes.Length ? NoiseAmplitudes[level] : 0.0f;

            var noiseL = 0;

            if (rs != 0.0f)
            {
                if (TerrainNode.Face == 1)
                {
                    var offset = 1 << level;
                    var bottomB = Noise.Noise(tx + 0.5f, ty + offset) > 0.0f ? 1 : 0;
                    var rightB = (tx == offset - 1 ? Noise.Noise(ty + offset + 0.5f, offset) : Noise.Noise(tx + 1.0f, ty + offset + 0.5f)) > 0.0f ? 2 : 0;
                    var topB = (ty == offset - 1 ? Noise.Noise(3.0f * offset - 1.0f - tx + 0.5f, offset) : Noise.Noise(tx + 0.5f, ty + offset + 1.0f)) > 0.0f ? 4 : 0;
                    var leftB = (tx == 0 ? Noise.Noise(4.0f * offset - 1.0f - ty + 0.5f, offset) : Noise.Noise(tx, ty + offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else if (TerrainNode.Face == 6)
                {
                    var offset = 1 << level;
                    var bottomB = (ty == 0 ? Noise.Noise(3.0f * offset - 1.0f - tx + 0.5f, 0) : Noise.Noise(tx + 0.5f, ty - offset)) > 0.0f ? 1 : 0;
                    var rightB = (tx == offset - 1.0f ? Noise.Noise(2.0f * offset - 1.0f - ty + 0.5f, 0) : Noise.Noise(tx + 1.0f, ty - offset + 0.5f)) > 0.0f ? 2 : 0;
                    var topB = Noise.Noise(tx + 0.5f, ty - offset + 1.0f) > 0.0f ? 4 : 0;
                    var leftB = (tx == 0 ? Noise.Noise(3.0f * offset + ty + 0.5f, 0) : Noise.Noise(tx, ty - offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else
                {
                    var offset = (1 << level) * (TerrainNode.Face - 2);
                    var bottomB = Noise.Noise(tx + offset + 0.5f, ty) > 0.0f ? 1 : 0;
                    var rightB = Noise.Noise((tx + offset + 1) % (4 << level), ty + 0.5f) > 0.0f ? 2 : 0;
                    var topB = Noise.Noise(tx + offset + 0.5f, ty + 1.0f) > 0.0f ? 4 : 0;
                    var leftB = Noise.Noise(tx + offset, ty + 0.5f) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
            }

            var noiseRs = new[] { 0, 0, 1, 0, 2, 0, 1, 0, 3, 3, 1, 3, 2, 2, 1, 0 };
            var noiseR = noiseRs[noiseL];

            var noiseLs = new[] { 0, 1, 1, 2, 1, 3, 2, 4, 1, 2, 3, 4, 2, 4, 4, 5 };
            noiseL = noiseLs[noiseL];

            UpSampleMaterial.SetTexture(uniforms.noiseSampler, GodManager.Instance.NoiseTextures[noiseL]);
            UpSampleMaterial.SetVector(uniforms.noiseUVLH, new Vector4(noiseR, (noiseR + 1) % 4, 0.0f, HSV ? 1.0f : 0.0f));

            Vector4 noiseColor = NoiseColor * rs * (HSV ? 1.0f : 2.0f) / 255.0f;
            noiseColor.w *= 2.0f;

            UpSampleMaterial.SetVector(uniforms.noiseColor, noiseColor);
            UpSampleMaterial.SetVector(uniforms.noiseRootColor, RootNoiseColor);

            Graphics.Blit(null, gpuSlot.Texture, UpSampleMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }

        public override IEnumerator DoCreateTileCoroutine(int level, int tx, int ty, List<TileStorage.Slot> slot, Action callback)
        {
            if (level > 0)
            {
                do
                {
                    yield return Yielders.EndOfFrame;
                } while (FindTile(level - 1, tx / 2, ty / 2, false, true) == null);
            }

            yield return base.DoCreateTileCoroutine(level, tx, ty, slot, callback);
        }

        public class Uniforms
        {
            public int noiseRootColor;
            public int noiseSampler, noiseUVLH, noiseColor;
            public int residualOSH, residualSampler;
            public int tileWSD, coarseLevelSampler, coarseLevelOSL;

            public Uniforms()
            {
                tileWSD = Shader.PropertyToID("_TileWSD");
                coarseLevelSampler = Shader.PropertyToID("_CoarseLevelSampler");
                coarseLevelOSL = Shader.PropertyToID("_CoarseLevelOSL");
                noiseSampler = Shader.PropertyToID("_NoiseSampler");
                noiseUVLH = Shader.PropertyToID("_NoiseUVLH");
                noiseColor = Shader.PropertyToID("_NoiseColor");
                noiseRootColor = Shader.PropertyToID("_NoiseRootColor");
                residualOSH = Shader.PropertyToID("_ResidualOSH");
                residualSampler = Shader.PropertyToID("_ResidualSampler");
            }
        }
    }
}