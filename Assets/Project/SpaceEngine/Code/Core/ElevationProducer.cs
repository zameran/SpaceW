using SpaceEngine.Code.Core.Bodies;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core
{
    /// <summary>
    /// Creates the elevations data for the terrain from perlin noise the noise amps set in the m_noiseAmps array are the amplitude of the noise for that level of the terrain quad.
    /// If the amplitude is a negative number the upsample shader will apply the noise every where, 
    /// if it is a positive number the noise will only be applied to steep areas and if the amplitude is 0 then the elevations will be upsampled but have no new noise applied.
    /// </summary>
    public class ElevationProducer : TileProducer
    {
        [Serializable]
        public class NoiseSettings
        {
            public float Freqeuncy = 40.0f;
            public float Amplitude = 1.0f;
            public int Seed = 0;
        }

        public class Uniforms
        {
            public int tileWSD, coarseLevelSampler, coarseLevelOSL;
            public int offset, localToWorld, frequency, amp;
            public int residualOSH, residualSampler;

            public Uniforms()
            {
                tileWSD = Shader.PropertyToID("_TileWSD");
                coarseLevelSampler = Shader.PropertyToID("_CoarseLevelSampler");
                coarseLevelOSL = Shader.PropertyToID("_CoarseLevelOSL");
                offset = Shader.PropertyToID("_Offset");
                localToWorld = Shader.PropertyToID("_LocalToWorld");
                frequency = Shader.PropertyToID("_Frequency");
                amp = Shader.PropertyToID("_Amp");
                residualOSH = Shader.PropertyToID("_ResidualOSH");
                residualSampler = Shader.PropertyToID("_ResidualSampler");
            }
        }

        [SerializeField]
        Material UpSampleMaterial;

        [SerializeField]
        NoiseSettings UpsampleSettings;

        [SerializeField]
        float[] NoiseAmplitudes = new float[] { -3250.0f, -1590.0f, -1125.0f, -795.0f, -561.0f, -397.0f, -140.0f, -100.0f, 15.0f, 8.0f, 5.0f, 2.5f, 1.5f, 1.0f };

        public float AmplitudeDiviner = 1.0f;

        ImprovedPerlinNoise Noise;

        Uniforms uniforms;

        protected override void Start()
        {
            base.Start();

            if (TerrainNode == null) { TerrainNode = transform.parent.GetComponent<TerrainNode>(); }
            if (TerrainNode.Body == null) { TerrainNode.Body = transform.parent.GetComponentInParent<CelestialBody>(); }

            var tileSize = GetTileSize(0);

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

            Noise = new ImprovedPerlinNoise(UpsampleSettings.Seed);
            Noise.LoadResourcesFor3DNoise();
            UpSampleMaterial.SetTexture("_PermTable2D", Noise.GetPermutationTable2D());
            UpSampleMaterial.SetTexture("_Gradient3D", Noise.GetGradient3D());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
            var tileSize = tileWidth - (1 + GetBorder() * 2);

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

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = (float)rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)tileSize / (float)(TerrainNode.Body.GridResolution - 1);
            tileWSD.w = 0.0f;

            UpSampleMaterial.SetVector(uniforms.tileWSD, tileWSD);

            if (level > 0)
            {
                var parentTexture = parentGpuSlot.Texture;

                UpSampleMaterial.SetTexture(uniforms.coarseLevelSampler, parentTexture);

                var dx = (float)(tx % 2) * (float)(tileSize / 2.0f);
                var dy = (float)(ty % 2) * (float)(tileSize / 2.0f);

                var coarseLevelOSL = new Vector4(dx / (float)parentTexture.width, dy / (float)parentTexture.height, 1.0f / (float)parentTexture.width, 0.0f);

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

            var offset = Vector4d.Zero();
            offset.x = ((double)tx / (1 << level) - 0.5) * rootQuadSize;
            offset.y = ((double)ty / (1 << level) - 0.5) * rootQuadSize;
            offset.z = rootQuadSize / (1 << level);
            offset.w = TerrainNode.Body.Radius;

            var ltow = TerrainNode.FaceToLocal.ToMatrix4x4();

            UpSampleMaterial.SetFloat(uniforms.frequency, UpsampleSettings.Freqeuncy * (1 << level));
            UpSampleMaterial.SetFloat(uniforms.amp, rs * UpsampleSettings.Amplitude);
            UpSampleMaterial.SetVector(uniforms.offset, offset.ToVector4());
            UpSampleMaterial.SetMatrix(uniforms.localToWorld, ltow);

            Graphics.Blit(null, gpuSlot.Texture, UpSampleMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}