using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Noise;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceEngine.Core
{
    public class OrthoProducer : TileProducer
    {
        public class Uniforms
        {
            public int tileWSD, coarseLevelSampler, coarseLevelOSL;
            public int noiseSampler, noiseUVLH, noiseColor;
            public int noiseRootColor;
            public int residualOSH, residualSampler;

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

        [SerializeField]
        GameObject OrthoCpuProducerGameObject;

        OrthoCPUProducer OrthoCPUProducer;

        [SerializeField]
        Material UpSampleMaterial;

        [SerializeField]
        Color RootNoiseColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [SerializeField]
        Color NoiseColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// Maximum quadtree level, or -1 to allow any level.
        /// </summary>
        [SerializeField]
        int MaxLevel = -1;

        [SerializeField]
        bool HSV = true;

        [SerializeField]
        int Seed = 0;

        [SerializeField]
        float[] NoiseAmplitudes = new float[] { 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };

        Uniforms uniforms;

        PerlinNoiseSimple Noise;

        Texture2D[] NoiseTextures;

        Texture2D ResidualTexture;

        protected override void Start()
        {
            base.Start();

            if (OrthoCpuProducerGameObject != null)
            {
                OrthoCPUProducer = OrthoCpuProducerGameObject.GetComponent<OrthoCPUProducer>();
            }

            int tileSize = Cache.GetStorage(0).TileSize;

            if (OrthoCPUProducer != null && OrthoCPUProducer.GetTileSize(0) != tileSize)
            {
                throw new InvalidParameterException("ortho CPU tile size must match ortho tile size");
            }

            if (!(Cache.GetStorage(0) is GPUTileStorage))
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }

            uniforms = new Uniforms();

            Noise = new PerlinNoiseSimple(Seed);

            ResidualTexture = new Texture2D(tileSize, tileSize, TextureFormat.ARGB32, false);
            ResidualTexture.wrapMode = TextureWrapMode.Clamp;
            ResidualTexture.filterMode = FilterMode.Point;

            CreateOrthoNoise();
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override bool HasTile(int level, int tx, int ty)
        {
            return (MaxLevel == -1 || level <= MaxLevel);
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - (GetBorder() * 2);

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = (float)rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)tileSize / (float)(TerrainNode.ParentBody.GridResolution - 1);
            tileWSD.w = 0.0f;

            GPUTileStorage.GPUSlot parentGpuSlot = null;

            if (level > 0)
            {
                var parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);

                if (parentTile != null)
                    parentGpuSlot = parentTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                else
                {
                    throw new MissingTileException("Find parent tile failed");
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

                var dx = (float)(tx % 2) * (float)(tileSize / 2.0f);
                var dy = (float)(ty % 2) * (float)(tileSize / 2.0f);

                var coarseLevelOSL = new Vector4((dx + 0.5f) / (float)tex.width, (dy + 0.5f) / (float)tex.height, 1.0f / (float)tex.width, 0.0f);

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

                for (int x = 0; x < tileWidth; x++)
                {
                    for (int y = 0; y < tileWidth; y++)
                    {
                        color.r = data[(x + y * tileWidth) * channels];

                        if (channels > 1) color.g = data[(x + y * tileWidth) * channels + 1];
                        if (channels > 2) color.b = data[(x + y * tileWidth) * channels + 2];
                        if (channels > 3) color.a = data[(x + y * tileWidth) * channels + 3];

                        ResidualTexture.SetPixel(x, y, color);
                    }
                }

                ResidualTexture.Apply();

                UpSampleMaterial.SetTexture(uniforms.residualSampler, ResidualTexture);
                UpSampleMaterial.SetVector(uniforms.residualOSH, new Vector4(0.5f / (float)tileWidth, 0.5f / (float)tileWidth, 1.0f / (float)tileWidth, 0.0f));
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
                    int offset = 1 << level;
                    int bottomB = Noise.Noise2D(tx + 0.5f, ty + offset) > 0.0f ? 1 : 0;
                    int rightB = (tx == offset - 1 ? Noise.Noise2D(ty + offset + 0.5f, offset) : Noise.Noise2D(tx + 1.0f, ty + offset + 0.5f)) > 0.0f ? 2 : 0;
                    int topB = (ty == offset - 1 ? Noise.Noise2D((3.0f * offset - 1.0f - tx) + 0.5f, offset) : Noise.Noise2D(tx + 0.5f, ty + offset + 1.0f)) > 0.0f ? 4 : 0;
                    int leftB = (tx == 0 ? Noise.Noise2D((4.0f * offset - 1.0f - ty) + 0.5f, offset) : Noise.Noise2D(tx, ty + offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else if (TerrainNode.Face == 6)
                {
                    int offset = 1 << level;
                    int bottomB = (ty == 0 ? Noise.Noise2D((3.0f * offset - 1.0f - tx) + 0.5f, 0) : Noise.Noise2D(tx + 0.5f, ty - offset)) > 0.0f ? 1 : 0;
                    int rightB = (tx == offset - 1.0f ? Noise.Noise2D((2.0f * offset - 1.0f - ty) + 0.5f, 0) : Noise.Noise2D(tx + 1.0f, ty - offset + 0.5f)) > 0.0f ? 2 : 0;
                    int topB = Noise.Noise2D(tx + 0.5f, ty - offset + 1.0f) > 0.0f ? 4 : 0;
                    int leftB = (tx == 0 ? Noise.Noise2D(3.0f * offset + ty + 0.5f, 0) : Noise.Noise2D(tx, ty - offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else
                {
                    int offset = (1 << level) * (TerrainNode.Face - 2);
                    int bottomB = Noise.Noise2D(tx + offset + 0.5f, ty) > 0.0f ? 1 : 0;
                    int rightB = Noise.Noise2D((tx + offset + 1) % (4 << level), ty + 0.5f) > 0.0f ? 2 : 0;
                    int topB = Noise.Noise2D(tx + offset + 0.5f, ty + 1.0f) > 0.0f ? 4 : 0;
                    int leftB = Noise.Noise2D(tx + offset, ty + 0.5f) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
            }

            var noiseRs = new int[] { 0, 0, 1, 0, 2, 0, 1, 0, 3, 3, 1, 3, 2, 2, 1, 0 };
            var noiseR = noiseRs[noiseL];

            var noiseLs = new int[] { 0, 1, 1, 2, 1, 3, 2, 4, 1, 2, 3, 4, 2, 4, 4, 5 };
            noiseL = noiseLs[noiseL];

            UpSampleMaterial.SetTexture(uniforms.noiseSampler, NoiseTextures[noiseL]);
            UpSampleMaterial.SetVector(uniforms.noiseUVLH, new Vector4(noiseR, (noiseR + 1) % 4, 0.0f, HSV ? 1.0f : 0.0f));

            if (HSV)
            {
                Vector4 col = NoiseColor * rs / 255.0f;
                col.w *= 2.0f;

                UpSampleMaterial.SetVector(uniforms.noiseColor, col);
            }
            else
            {
                Vector4 col = NoiseColor * rs * 2.0f / 255.0f;
                col.w *= 2.0f;

                UpSampleMaterial.SetVector(uniforms.noiseColor, col);
            }

            UpSampleMaterial.SetVector(uniforms.noiseRootColor, RootNoiseColor);

            Graphics.Blit(null, gpuSlot.Texture, UpSampleMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }

        public override IEnumerator DoCreateTileCoroutine(int level, int tx, int ty, List<TileStorage.Slot> slot, Action Callback)
        {
            if (level > 0)
            {
                do
                {
                    yield return Yielders.EndOfFrame;
                }
                while (FindTile(level - 1, tx / 2, ty / 2, false, true) == null);
            }

            yield return base.DoCreateTileCoroutine(level, tx, ty, slot, Callback);
        }

        private void CreateOrthoNoise()
        {
            var tileWidth = Cache.GetStorage(0).TileSize;
            var color = new Color();

            NoiseTextures = new Texture2D[6];

            var layers = new int[] { 0, 1, 3, 5, 7, 15 };
            var rand = 1234567;

            for (byte nl = 0; nl < 6; ++nl)
            {
                var layer = layers[nl];

                NoiseTextures[nl] = new Texture2D(tileWidth, tileWidth, TextureFormat.ARGB32, false, true);

                // Corners
                for (int j = 0; j < tileWidth; ++j)
                {
                    for (int i = 0; i < tileWidth; ++i)
                    {
                        NoiseTextures[nl].SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    }
                }

                // Bottom border
                Random.InitState((layer & 1) == 0 ? 7654321 : 5647381);

                for (int v = 2; v < 4; ++v)
                {
                    for (int h = 4; h < tileWidth - 4; ++h)
                    {
                        for (byte c = 0; c < 4; ++c)
                        {
                            color[c] = Random.value;
                        }

                        NoiseTextures[nl].SetPixel(h, v, color);
                        NoiseTextures[nl].SetPixel(tileWidth - 1 - h, 3 - v, color);
                    }
                }

                // Right border
                Random.InitState((layer & 2) == 0 ? 7654321 : 5647381);

                for (int h = tileWidth - 3; h >= tileWidth - 4; --h)
                {
                    for (int v = 4; v < tileWidth - 4; ++v)
                    {
                        for (byte c = 0; c < 4; ++c)
                        {
                            color[c] = Random.value;
                        }

                        NoiseTextures[nl].SetPixel(h, v, color);
                        NoiseTextures[nl].SetPixel(2 * tileWidth - 5 - h, tileWidth - 1 - v, color);
                    }
                }

                // Top border
                Random.InitState((layer & 4) == 0 ? 7654321 : 5647381);

                for (int v = tileWidth - 2; v < tileWidth; ++v)
                {
                    for (int h = 4; h < tileWidth - 4; ++h)
                    {
                        for (byte c = 0; c < 4; ++c)
                        {
                            color[c] = Random.value;
                        }

                        NoiseTextures[nl].SetPixel(h, v, color);
                        NoiseTextures[nl].SetPixel(tileWidth - 1 - h, 2 * tileWidth - 5 - v, color);
                    }
                }

                // Left border
                Random.InitState((layer & 8) == 0 ? 7654321 : 5647381);

                for (int h = 1; h >= 0; --h)
                {
                    for (int v = 4; v < tileWidth - 4; ++v)
                    {
                        for (byte c = 0; c < 4; ++c)
                        {
                            color[c] = Random.value;
                        }

                        NoiseTextures[nl].SetPixel(h, v, color);
                        NoiseTextures[nl].SetPixel(3 - h, tileWidth - 1 - v, color);
                    }
                }

                // Center
                Random.InitState(rand);

                for (int v = 4; v < tileWidth - 4; ++v)
                {
                    for (int h = 4; h < tileWidth - 4; ++h)
                    {
                        for (byte c = 0; c < 4; ++c)
                        {
                            color[c] = Random.value;
                        }

                        NoiseTextures[nl].SetPixel(h, v, color);
                    }
                }

                //randomize for next texture
                rand = (rand * 1103515245 + 12345) & 0x7FFFFFFF;

                NoiseTextures[nl].Apply();
            }
        }
    }
}