using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace SpaceEngine.Core
{
    [ExecutionOrder(1000)]
    public class OriginalElevationProducer : TileProducer
    {
        public class Uniforms
        {
            public int tileWSD, coarseLevelSampler, coarseLevelOSL;
            public int noiseUVLH, noiseSampler;
            public int residualOSH, residualSampler;

            public Uniforms()
            {
                tileWSD = Shader.PropertyToID("_TileWSD");
                coarseLevelSampler = Shader.PropertyToID("_CoarseLevelSampler");
                coarseLevelOSL = Shader.PropertyToID("_CoarseLevelOSL");
                noiseUVLH = Shader.PropertyToID("_NoiseUVLH");
                noiseSampler = Shader.PropertyToID("_NoiseSampler");
                residualOSH = Shader.PropertyToID("_ResidualOSH");
                residualSampler = Shader.PropertyToID("_ResidualSampler");
            }
        }

        /// <summary>
        /// The Program to perform the upsampling and add procedure on GPU.
        /// </summary>
        [SerializeField]
        Material UpSampleMaterial;

        [SerializeField]
        int Seed = 0;

        /// <summary>
        /// The amplitude of the noise to be added for each level (one amplitude per level).
        /// </summary>
        [SerializeField]
        float[] NoiseAmplitudes = new float[] { -3250.0f, -1590.0f, -1125.0f, -795.0f, -561.0f, -397.0f, -140.0f, -100.0f, 15.0f, 8.0f, 5.0f, 2.5f, 1.5f, 1.0f, 0.5f, 0.25f, 0.1f, 0.05f };

        public float AmplitudeDiviner = 1.0f;

        Uniforms uniforms;

        PerlinNoise Noise;

        [SerializeField]
        RenderTexture[] NoiseTextures;

        protected override void Start()
        {
            base.Start();

            var tileSize = Cache.GetStorage(0).TileSize;

            if ((tileSize - GetBorder() * 2 - 1) % (TerrainNode.Body.GridResolution - 1) != 0)
            {
                throw new InvalidParameterException("Tile size - border * 2 - 1 must be divisible by grid mesh resolution - 1" + string.Format(": {0}-{1}", tileSize, GetBorder()));
            }

            var storage = Cache.GetStorage(0) as GPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }

            if (storage.FilterMode != FilterMode.Point)
            {
                throw new InvalidParameterException("GPUTileStorage filter must be point. There will be seams in the terrain otherwise");
            }

            uniforms = new Uniforms();
            Noise = new PerlinNoise(Seed);

            CreateDemNoise();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (byte i = 0; i < 6; i++)
            {
                NoiseTextures[i].Release();
            }
        }

        public override int GetBorder()
        {
            return 2;
        }

        /// <summary>
        /// This function creates the elevations data and is called by the <see cref="Tile.Tasks.CreateTileTask"/> when the task is run by the <see cref="Utilities.Schedular"/>.
        /// The functions needs the tiles parent data to have already been created. If it has not the program will abort.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <param name="slot"></param>
        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - 5;

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

            var rootQuadSize = (float)TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)(tileWidth - 5) / (float)(TerrainNode.Body.GridResolution - 1);
            tileWSD.w = 0.0f;

            UpSampleMaterial.SetVector(uniforms.tileWSD, tileWSD);

            if (level > 0)
            {
                var parentTexture = parentGpuSlot.Texture;

                var dx = (float)(tx % 2) * (float)(tileSize / 2.0f);
                var dy = (float)(ty % 2) * (float)(tileSize / 2.0f);

                var coarseLevelOSL = new Vector4(dx / (float)parentTexture.width, dy / (float)parentTexture.height, 1.0f / (float)parentTexture.width, 0.0f);

                UpSampleMaterial.SetTexture(uniforms.coarseLevelSampler, parentTexture);
                UpSampleMaterial.SetVector(uniforms.coarseLevelOSL, coarseLevelOSL);
            }
            else
            {
                UpSampleMaterial.SetVector(uniforms.coarseLevelOSL, new Vector4(-1.0f, -1.0f, -1.0f, -1.0f));
            }

            UpSampleMaterial.SetTexture(uniforms.residualSampler, null);
            UpSampleMaterial.SetVector(uniforms.residualOSH, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));

            var rs = level < NoiseAmplitudes.Length ? NoiseAmplitudes[level] : 0.0f;

            rs = rs / AmplitudeDiviner;

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
            UpSampleMaterial.SetVector(uniforms.noiseUVLH, new Vector4(noiseR, (noiseR + 1) % 4, 0, rs));

            Graphics.Blit(null, gpuSlot.Texture, UpSampleMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }

        private float RandomValue()
        {
            return Random.value * 2.0f - 1.0f;
        }

        /// <summary>
        /// Creates a series of textures that contain random noise.
        /// These texture tile together using the Wang Tiling method.
        /// Used by the UpSample shader to create fractal noise for the terrain elevations.
        /// </summary>
        private void CreateDemNoise()
        {
            var tileWidth = Cache.GetStorage(0).TileSize;
            NoiseTextures = new RenderTexture[6];

            var layers = new int[] { 0, 1, 3, 5, 7, 15 };
            var rand = 1234567;

            for (byte nl = 0; nl < 6; ++nl)
            {
                var noiseArray = new float[tileWidth * tileWidth];
                var l = layers[nl];
                var buffer = new ComputeBuffer(tileWidth * tileWidth, sizeof(float));

                for (int j = 0; j < tileWidth; ++j)
                {
                    for (int i = 0; i < tileWidth; ++i)
                    {
                        noiseArray[i + j * tileWidth] = Noise.Noise2D(i, j);
                    }
                }

                // Corners
                for (int j = 0; j < tileWidth; ++j)
                {
                    for (int i = 0; i < tileWidth; ++i)
                    {
                        noiseArray[i + j * tileWidth] = 0.0f;
                    }
                }

                // Bottom border
                Random.InitState((l & 1) == 0 ? 7654321 : 5647381);

                for (int h = 5; h <= tileWidth / 2; ++h)
                {
                    var N = RandomValue();

                    noiseArray[h + 2 * tileWidth] = N;
                    noiseArray[(tileWidth - 1 - h) + 2 * tileWidth] = N;
                }

                for (int v = 3; v < 5; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        var N = RandomValue();

                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(tileWidth - 1 - h) + (4 - v) * tileWidth] = N;
                    }
                }

                // Right border
                Random.InitState((l & 2) == 0 ? 7654321 : 5647381);

                for (int v = 5; v <= tileWidth / 2; ++v)
                {
                    var N = RandomValue();

                    noiseArray[(tileWidth - 3) + v * tileWidth] = N;
                    noiseArray[(tileWidth - 3) + (tileWidth - 1 - v) * tileWidth] = N;
                }

                for (int h = tileWidth - 4; h >= tileWidth - 5; --h)
                {
                    for (int v = 5; v < tileWidth - 5; ++v)
                    {
                        var N = RandomValue();

                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(2 * tileWidth - 6 - h) + (tileWidth - 1 - v) * tileWidth] = N;
                    }
                }

                // Top border
                Random.InitState((l & 4) == 0 ? 7654321 : 5647381);

                for (int h = 5; h <= tileWidth / 2; ++h)
                {
                    var N = RandomValue();

                    noiseArray[h + (tileWidth - 3) * tileWidth] = N;
                    noiseArray[(tileWidth - 1 - h) + (tileWidth - 3) * tileWidth] = N;
                }

                for (int v = tileWidth - 2; v < tileWidth; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        var N = RandomValue();

                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(tileWidth - 1 - h) + (2 * tileWidth - 6 - v) * tileWidth] = N;
                    }
                }

                // Left border
                Random.InitState((l & 8) == 0 ? 7654321 : 5647381);

                for (int v = 5; v <= tileWidth / 2; ++v)
                {
                    var N = RandomValue();

                    noiseArray[2 + v * tileWidth] = N;
                    noiseArray[2 + (tileWidth - 1 - v) * tileWidth] = N;
                }

                for (int h = 1; h >= 0; --h)
                {
                    for (int v = 5; v < tileWidth - 5; ++v)
                    {
                        var N = RandomValue();

                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(4 - h) + (tileWidth - 1 - v) * tileWidth] = N;
                    }
                }

                // Center
                Random.InitState(rand);

                for (int v = 5; v < tileWidth - 5; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        var N = RandomValue();
                        noiseArray[h + v * tileWidth] = N;
                    }
                }

                // Randomize for next texture
                rand = (rand * 1103515245 + 12345) & 0x7FFFFFFF;

                NoiseTextures[nl] = RTExtensions.CreateRTexture(new Vector2(tileWidth, tileWidth), 0, RenderTextureFormat.RHalf, FilterMode.Point, TextureWrapMode.Repeat);

                // Write data into render texture
                buffer.SetData(noiseArray);

                CBUtility.WriteIntoRenderTexture(NoiseTextures[nl], 1, buffer, GodManager.Instance.WriteData);

                buffer.ReleaseAndDisposeBuffer();
            }
        }
    }
}