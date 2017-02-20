using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core
{
    /*
     * Creates the elevations data for the terrain from perlin noise
     * The noise amps set in the m_noiseAmps array are the amp of the noise for that level of the terrain quad.
     * If the amp is a negative number the upsample shader will apply the noise every where,
     * if it is a positive number the noise will only be applied to steep areas and if the amp
     * is 0 then the elevations will be upsampled but have no new noise applied
     * 
     * NOTE - this is not the original method used in Proland. For the original see the OriginalElevationProducer.cs script
     */

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

        //The Program to perform the upsampling and add procedure on GPU.
        [SerializeField]
        Material m_upsampleMat;

        [SerializeField]
        int m_seed = 0;

        //The amplitude of the noise to be added for each level (one amplitude per level).
        //example of planet amps
        [SerializeField]
        float[] m_noiseAmp = new float[] { -3250.0f, -1590.0f, -1125.0f, -795.0f, -561.0f, -397.0f, -140.0f, -100.0f, 15.0f, 8.0f, 5.0f, 2.5f, 1.5f, 1.0f, 0.5f, 0.25f, 0.1f, 0.05f };

        public float AmplitudeDiviner = 1.0f;

        Uniforms m_uniforms;

        PerlinNoise m_noise;

        [SerializeField]
        RenderTexture[] m_noiseTextures;

        protected override void Start()
        {
            base.Start();

            int tileSize = Cache.GetStorage(0).TileSize;

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

            m_uniforms = new Uniforms();
            m_noise = new PerlinNoise(m_seed);

            CreateDemNoise();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < 6; i++)
                m_noiseTextures[i].Release();
        }

        public override int GetBorder()
        {
            return 2;
        }

        /*
         * This function creates the elevations data and is called by the CreateTileTask when the task is run by the schedular
         * The functions needs the tiles parent data to have already been created. If it has not the program will abort.
         */
        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            GPUTileStorage.GPUSlot gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            int tileWidth = gpuSlot.Owner.TileSize;
            int tileSize = tileWidth - 5;

            GPUTileStorage.GPUSlot parentGpuSlot = null;
            Tile.Tile parentTile = null;

            if (level > 0)
            {
                parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);

                if (parentTile != null)
                    parentGpuSlot = parentTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                else
                {
                    throw new MissingTileException("Find parent tile failed");
                }
            }

            float rootQuadSize = (float)TerrainNode.TerrainQuadRoot.GetLength();

            Vector4 tileWSD = new Vector4();
            tileWSD.x = (float)tileWidth;
            tileWSD.y = rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)(tileWidth - 5) / (float)(TerrainNode.Body.GridResolution - 1);
            tileWSD.w = 0.0f;

            m_upsampleMat.SetVector(m_uniforms.tileWSD, tileWSD);

            if (level > 0)
            {
                RenderTexture tex = parentGpuSlot.Texture;

                m_upsampleMat.SetTexture(m_uniforms.coarseLevelSampler, tex);

                float dx = (float)(tx % 2) * (float)(tileSize / 2);
                float dy = (float)(ty % 2) * (float)(tileSize / 2);

                Vector4 coarseLevelOSL = new Vector4(dx / (float)tex.width, dy / (float)tex.height, 1.0f / (float)tex.width, 0.0f);

                m_upsampleMat.SetVector(m_uniforms.coarseLevelOSL, coarseLevelOSL);
            }
            else
            {
                m_upsampleMat.SetVector(m_uniforms.coarseLevelOSL, new Vector4(-1.0f, -1.0f, -1.0f, -1.0f));
            }

            m_upsampleMat.SetTexture(m_uniforms.residualSampler, null);
            m_upsampleMat.SetVector(m_uniforms.residualOSH, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));

            float rs = level < m_noiseAmp.Length ? m_noiseAmp[level] : 0.0f;

            rs = rs / AmplitudeDiviner;

            int noiseL = 0;
            int face = TerrainNode.Face;

            if (rs != 0.0f)
            {
                if (face == 1)
                {
                    int offset = 1 << level;
                    int bottomB = m_noise.Noise2D(tx + 0.5f, ty + offset) > 0.0f ? 1 : 0;
                    int rightB = (tx == offset - 1 ? m_noise.Noise2D(ty + offset + 0.5f, offset) : m_noise.Noise2D(tx + 1.0f, ty + offset + 0.5f)) > 0.0f ? 2 : 0;
                    int topB = (ty == offset - 1 ? m_noise.Noise2D((3.0f * offset - 1.0f - tx) + 0.5f, offset) : m_noise.Noise2D(tx + 0.5f, ty + offset + 1.0f)) > 0.0f ? 4 : 0;
                    int leftB = (tx == 0 ? m_noise.Noise2D((4.0f * offset - 1.0f - ty) + 0.5f, offset) : m_noise.Noise2D(tx, ty + offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else if (face == 6)
                {
                    int offset = 1 << level;
                    int bottomB = (ty == 0 ? m_noise.Noise2D((3.0f * offset - 1.0f - tx) + 0.5f, 0) : m_noise.Noise2D(tx + 0.5f, ty - offset)) > 0.0f ? 1 : 0;
                    int rightB = (tx == offset - 1.0f ? m_noise.Noise2D((2.0f * offset - 1.0f - ty) + 0.5f, 0) : m_noise.Noise2D(tx + 1.0f, ty - offset + 0.5f)) > 0.0f ? 2 : 0;
                    int topB = m_noise.Noise2D(tx + 0.5f, ty - offset + 1.0f) > 0.0f ? 4 : 0;
                    int leftB = (tx == 0 ? m_noise.Noise2D(3.0f * offset + ty + 0.5f, 0) : m_noise.Noise2D(tx, ty - offset + 0.5f)) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
                else
                {
                    int offset = (1 << level) * (face - 2);
                    int bottomB = m_noise.Noise2D(tx + offset + 0.5f, ty) > 0.0f ? 1 : 0;
                    int rightB = m_noise.Noise2D((tx + offset + 1) % (4 << level), ty + 0.5f) > 0.0f ? 2 : 0;
                    int topB = m_noise.Noise2D(tx + offset + 0.5f, ty + 1.0f) > 0.0f ? 4 : 0;
                    int leftB = m_noise.Noise2D(tx + offset, ty + 0.5f) > 0.0f ? 8 : 0;
                    noiseL = bottomB + rightB + topB + leftB;
                }
            }

            int[] noiseRs = new int[] { 0, 0, 1, 0, 2, 0, 1, 0, 3, 3, 1, 3, 2, 2, 1, 0 };
            int noiseR = noiseRs[noiseL];

            int[] noiseLs = new int[] { 0, 1, 1, 2, 1, 3, 2, 4, 1, 2, 3, 4, 2, 4, 4, 5 };
            noiseL = noiseLs[noiseL];

            m_upsampleMat.SetTexture(m_uniforms.noiseSampler, m_noiseTextures[noiseL]);
            m_upsampleMat.SetVector(m_uniforms.noiseUVLH, new Vector4(noiseR, (noiseR + 1) % 4, 0, rs));

            Graphics.Blit(null, gpuSlot.Texture, m_upsampleMat);

            base.DoCreateTile(level, tx, ty, slot);
        }

        float Rand()
        {
            return Random.value * 2.0f - 1.0f;
        }

        /*
         * Creates a series of textures that contain random noise.
         * These texture tile together using the Wang Tiling method.
         * Used by the UpSample shader to create fractal noise for the terrain elevations.
         */
        void CreateDemNoise()
        {
            int tileWidth = Cache.GetStorage(0).TileSize;
            m_noiseTextures = new RenderTexture[6];

            int[] layers = new int[] { 0, 1, 3, 5, 7, 15 };
            int rand = 1234567;

            for (int nl = 0; nl < 6; ++nl)
            {
                float[] noiseArray = new float[tileWidth * tileWidth];
                int l = layers[nl];

                ComputeBuffer buffer = new ComputeBuffer(tileWidth * tileWidth, sizeof(float));

                for (int j = 0; j < tileWidth; ++j)
                {
                    for (int i = 0; i < tileWidth; ++i)
                    {
                        noiseArray[i + j * tileWidth] = m_noise.Noise2D(i, j);
                    }
                }

                // corners
                for (int j = 0; j < tileWidth; ++j)
                {
                    for (int i = 0; i < tileWidth; ++i)
                    {
                        noiseArray[i + j * tileWidth] = 0.0f;
                    }
                }

                // bottom border
                Random.InitState((l & 1) == 0 ? 7654321 : 5647381);
                for (int h = 5; h <= tileWidth / 2; ++h)
                {
                    float N = Rand();
                    noiseArray[h + 2 * tileWidth] = N;
                    noiseArray[(tileWidth - 1 - h) + 2 * tileWidth] = N;
                }

                for (int v = 3; v < 5; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        float N = Rand();
                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(tileWidth - 1 - h) + (4 - v) * tileWidth] = N;
                    }
                }

                // right border
                Random.InitState((l & 2) == 0 ? 7654321 : 5647381);
                for (int v = 5; v <= tileWidth / 2; ++v)
                {
                    float N = Rand();
                    noiseArray[(tileWidth - 3) + v * tileWidth] = N;
                    noiseArray[(tileWidth - 3) + (tileWidth - 1 - v) * tileWidth] = N;
                }

                for (int h = tileWidth - 4; h >= tileWidth - 5; --h)
                {
                    for (int v = 5; v < tileWidth - 5; ++v)
                    {
                        float N = Rand();
                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(2 * tileWidth - 6 - h) + (tileWidth - 1 - v) * tileWidth] = N;
                    }
                }

                // top border
                Random.InitState((l & 4) == 0 ? 7654321 : 5647381);
                for (int h = 5; h <= tileWidth / 2; ++h)
                {
                    float N = Rand();
                    noiseArray[h + (tileWidth - 3) * tileWidth] = N;
                    noiseArray[(tileWidth - 1 - h) + (tileWidth - 3) * tileWidth] = N;
                }

                for (int v = tileWidth - 2; v < tileWidth; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        float N = Rand();
                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(tileWidth - 1 - h) + (2 * tileWidth - 6 - v) * tileWidth] = N;
                    }
                }

                // left border
                Random.InitState((l & 8) == 0 ? 7654321 : 5647381);
                for (int v = 5; v <= tileWidth / 2; ++v)
                {
                    float N = Rand();
                    noiseArray[2 + v * tileWidth] = N;
                    noiseArray[2 + (tileWidth - 1 - v) * tileWidth] = N;
                }

                for (int h = 1; h >= 0; --h)
                {
                    for (int v = 5; v < tileWidth - 5; ++v)
                    {
                        float N = Rand();
                        noiseArray[h + v * tileWidth] = N;
                        noiseArray[(4 - h) + (tileWidth - 1 - v) * tileWidth] = N;
                    }
                }

                // center
                Random.InitState(rand);
                for (int v = 5; v < tileWidth - 5; ++v)
                {
                    for (int h = 5; h < tileWidth - 5; ++h)
                    {
                        float N = Rand();
                        noiseArray[h + v * tileWidth] = N;
                        //noiseArray[h + v * tileWidth] = (VoronoiNoise.CellNoise(new Vector3(0, h, v), N) * 4);
                    }
                }

                //randomize for next texture
                rand = (rand * 1103515245 + 12345) & 0x7FFFFFFF;

                m_noiseTextures[nl] = new RenderTexture(tileWidth, tileWidth, 0, RenderTextureFormat.RHalf);
                m_noiseTextures[nl].wrapMode = TextureWrapMode.Repeat;
                m_noiseTextures[nl].filterMode = FilterMode.Point;
                m_noiseTextures[nl].enableRandomWrite = true;
                m_noiseTextures[nl].Create();
                //write data into render texture
                buffer.SetData(noiseArray);
                CBUtility.WriteIntoRenderTexture(m_noiseTextures[nl], 1, buffer, GodManager.Instance.WriteData);
                buffer.Release();
            }
        }
    }
}