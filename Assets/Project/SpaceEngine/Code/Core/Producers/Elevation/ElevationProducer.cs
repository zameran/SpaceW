using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Utilities;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core
{
    /// <summary>
    /// Creates the elevations data for the terrain from perlin noise.
    /// The noise amplitudes set in the <see cref="NoiseAmplitudes"/> array are the amplitude of the noise for that level of the terrain quad.
    /// If the amplitude is a negative number the upsample shader will apply the noise every where, 
    /// if it is a positive number the noise will only be applied to steep areas,
    /// if the amplitude is 0 then the elevations will be upsampled but have no new noise applied.
    /// </summary>
    public class ElevationProducer : TileProducer
    {
        public GameObject ResidualProducerGameObject;

        private TileProducer ResidualProducer;

        public Material UpSampleMaterial;

        public float[] NoiseAmplitudes = new float[] { -3250.0f, -1590.0f, -1125.0f, -795.0f, -561.0f, -397.0f, -140.0f, -100.0f, 15.0f, 8.0f, 5.0f, 2.5f, 1.5f, 1.0f };

        public override void InitNode()
        {
            base.InitNode();

            if (ResidualProducerGameObject != null)
            {
                if (ResidualProducer == null) { ResidualProducer = ResidualProducerGameObject.GetComponent<TileProducer>(); }
            }

            var tileSize = GetTileSize(0);

            if ((tileSize - GetBorder() * 2 - 1) % (TerrainNode.ParentBody.GridResolution - 1) != 0)
            {
                throw new InvalidParameterException("Tile size - border * 2 - 1 must be divisible by grid mesh resolution - 1" + string.Format(": {0}-{1}", tileSize, GetBorder()));
            }

            if (ResidualProducer != null)
            {
                if (ResidualProducer.GetTileSize(0) != tileSize) throw new InvalidParameterException("Residual tile size must match elevation tile size!");

                if (ResidualProducer.IsGPUProducer)
                {
                    if (!(ResidualProducer.Cache.GetStorage(0) is GPUTileStorage)) throw new InvalidStorageException("Residual storage must be a GPUTileStorage");
                }
                else
                {
                    if (!(ResidualProducer.Cache.GetStorage(0) is CPUTileStorage)) throw new InvalidStorageException("Residual storage must be a CPUTileStorage");
                }
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

            if (gpuSlot == null) { throw new NullReferenceException("gpuSlot"); }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - (1 + GetBorder() * 2);

            GPUTileStorage.GPUSlot parentGpuSlot = null;

            var upsample = level > 0;
            var parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);

            // TODO : Make it classwide...
            var residualTileSize = GetTileSize(0);
            var residualTexture = RTExtensions.CreateRTexture(residualTileSize, 0, RenderTextureFormat.RFloat);
            var residualBuffer = new ComputeBuffer(residualTileSize * residualTileSize, sizeof(float));
            
            if (ResidualProducer != null)
            {
                if (ResidualProducer.HasTile(level, tx, ty))
                {
                    if (ResidualProducer.IsGPUProducer)
                    {
                        GPUTileStorage.GPUSlot residualGpuSlot = null;

                        var residualTile = ResidualProducer.FindTile(level, tx, ty, false, true);

                        if (residualTile != null)
                            residualGpuSlot = residualTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                        else { throw new MissingTileException("Find residual tile failed"); }

                        if (residualGpuSlot == null) { throw new MissingTileException("Find parent tile failed"); }

                        UpSampleMaterial.SetTexture("_ResidualSampler", residualGpuSlot.Texture);
                        UpSampleMaterial.SetVector("_ResidualOSH", new Vector4(0.25f / (float)tileWidth, 0.25f / (float)tileWidth, 2.0f / (float)tileWidth, 1.0f));
                    }
                    else
                    {
                        CPUTileStorage.CPUSlot<float> residualCPUSlot = null;

                        var residualTile = ResidualProducer.FindTile(level, tx, ty, false, true);
                        
                        if (residualTile != null)
                            residualCPUSlot = residualTile.GetSlot(0) as CPUTileStorage.CPUSlot<float>;
                        else { throw new MissingTileException("Find residual tile failed"); }

                        if (residualCPUSlot == null) { throw new MissingTileException("Find parent tile failed"); }

                        residualBuffer.SetData(residualCPUSlot.Data);

                        RTUtility.ClearColor(residualTexture);
                        CBUtility.WriteIntoRenderTexture(residualTexture, CBUtility.Channels.R, residualBuffer, GodManager.Instance.WriteData);
                        //RTUtility.SaveAs8bit(residualTileSize, residualTileSize, CBUtility.Channels.R, string.Format("Residual_{0}_{1}-{2}-{3}", TerrainNode.name, level, tx, ty), "/Resources/Preprocess/Textures/Debug/", residualCPUSlot.Data);

                        UpSampleMaterial.SetTexture("_ResidualSampler", residualTexture);
                        UpSampleMaterial.SetVector("_ResidualOSH", new Vector4(0.25f / (float)tileWidth, 0.25f / (float)tileWidth, 2.0f / (float)tileWidth, 1.0f));
                    }
                }
                else
                {
                    UpSampleMaterial.SetTexture("_ResidualSampler", null);
                    UpSampleMaterial.SetVector("_ResidualOSH", new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
                }
            }
            else
            {
                UpSampleMaterial.SetTexture("_ResidualSampler", null);
                UpSampleMaterial.SetVector("_ResidualOSH", new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
            }

            if (upsample)
            {
                if (parentTile != null)
                    parentGpuSlot = parentTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                else { throw new MissingTileException(string.Format("Find parent tile failed! {0}:{1}-{2}", level - 1, tx / 2, ty / 2)); }
            }

            if (parentGpuSlot == null && upsample) { throw new NullReferenceException("parentGpuSlot"); }

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = (float)rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)tileSize / (float)(TerrainNode.ParentBody.GridResolution - 1);
            tileWSD.w = 0.0f;

            var tileScreenSize = (0.5 + (float)GetBorder()) / (tileWSD.x - 1 - (float)GetBorder() * 2);
            var tileSD = new Vector2d(tileScreenSize, 1.0 + tileScreenSize * 2.0);

            UpSampleMaterial.SetVector("_TileWSD", tileWSD);
            UpSampleMaterial.SetVector("_TileSD", tileSD.ToVector2());

            if (upsample)
            {
                var parentTexture = parentGpuSlot.Texture;

                var dx = (float)(tx % 2) * (float)(tileSize / 2.0f);
                var dy = (float)(ty % 2) * (float)(tileSize / 2.0f);

                var coarseLevelOSL = new Vector4(dx / (float)parentTexture.width, dy / (float)parentTexture.height, 1.0f / (float)parentTexture.width, 0.0f);

                UpSampleMaterial.SetTexture("_CoarseLevelSampler", parentTexture);
                UpSampleMaterial.SetVector("_CoarseLevelOSL", coarseLevelOSL);
            }
            else
            {
                UpSampleMaterial.SetTexture("_CoarseLevelSampler", null);
                UpSampleMaterial.SetVector("_CoarseLevelOSL", new Vector4(-1.0f, -1.0f, -1.0f, -1.0f));
            }

            var rs = level < NoiseAmplitudes.Length ? NoiseAmplitudes[level] : 0.0f;

            var offset = new Vector4d(((double)tx / (1 << level) - 0.5) * rootQuadSize,
                                      ((double)ty / (1 << level) - 0.5) * rootQuadSize,
                                      rootQuadSize / (1 << level),
                                      TerrainNode.ParentBody.Size);

            UpSampleMaterial.SetFloat("_Amplitude", rs / (TerrainNode.ParentBody.Amplitude / 10.0f));
            UpSampleMaterial.SetFloat("_Frequency", TerrainNode.ParentBody.Frequency * (1 << level));
            UpSampleMaterial.SetVector("_Offset", offset.ToVector4());
            UpSampleMaterial.SetMatrix("_LocalToWorld", TerrainNode.FaceToLocal.ToMatrix4x4());

            if (TerrainNode.ParentBody.TCCPS != null) TerrainNode.ParentBody.TCCPS.SetUniforms(UpSampleMaterial);

            Graphics.Blit(null, gpuSlot.Texture, UpSampleMaterial);

            residualTexture.ReleaseAndDestroy();
            residualBuffer.ReleaseAndDisposeBuffer();

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}