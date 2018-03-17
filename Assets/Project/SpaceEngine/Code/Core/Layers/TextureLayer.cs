using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Layer;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Layers
{
    /// <summary>
    /// This layer modifies the tile of it's <see cref="TileProducer"/> 
    /// by applying special 'filter' into the tiles, produced by another <see cref="TileProducer"/>, after they have been transformed via a GPU Program.
    /// </summary>
    public class TextureLayer : TileLayer
    {
        public GameObject SourceProducerGameObject;

        private TileProducer SourceProducer;

        public GameObject TargetProducerGameObject;

        private TileProducer TargetProducer;

        public Material LayerMaterial;

        private RenderTexture TargetTextureBuffer;

        #region NodeSlave<TileLayer>

        public override void InitNode()
        {
            base.InitNode();

            if (SourceProducerGameObject != null)
            {
                if (SourceProducer == null) { SourceProducer = SourceProducerGameObject.GetComponent<TileProducer>(); }
            }

            if (TargetProducerGameObject != null)
            {
                if (TargetProducer == null) { TargetProducer = TargetProducerGameObject.GetComponent<TileProducer>(); }
            }

            if (SourceProducer == null) { throw new NullReferenceException("Source producer is null!"); }
            if (TargetProducer == null) { throw new NullReferenceException("Target producer is null!"); }

            var targetSize = TargetProducer.Cache.GetStorage(0).TileSize;
            var sourceSize = SourceProducer.Cache.GetStorage(0).TileSize;

            if (targetSize != sourceSize && targetSize != sourceSize - 1)
            {
                throw new InvalidParameterException("Target tile size must equal source tile size or source tile size-1");
            }

            if (TargetProducer.GetBorder() != SourceProducer.GetBorder())
            {
                throw new InvalidParameterException("Target border size must be equal to source border size");
            }

            var storage = TargetProducer.Cache.GetStorage(0) as GPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Target storage must be a GPUTileStorage");
            }

            storage = SourceProducer.Cache.GetStorage(0) as GPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Source storage must be a GPUTileStorage");
            }
        }

        public override void UpdateNode()
        {
            base.UpdateNode();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (TargetTextureBuffer != null) TargetTextureBuffer.ReleaseAndDestroy();
        }

        #endregion

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null) { throw new NullReferenceException("gpuSlot"); }

            if (TargetTextureBuffer == null) { TargetTextureBuffer = new RenderTexture(gpuSlot.Texture.descriptor); }

            GPUTileStorage.GPUSlot sourceGpuSlot = null;

            var sourceTile = SourceProducer.FindTile(level, tx, ty, false, true);

            if (sourceTile != null)
                sourceGpuSlot = sourceTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            else { throw new MissingTileException("Find source producer tile failed"); }

            if (sourceGpuSlot == null) { throw new MissingTileException("Find source tile failed"); }

            var coords = Vector3.forward;
            var targetSize = TargetProducer.Cache.GetStorage(0).TileSize;
            var sourceSize = SourceProducer.Cache.GetStorage(0).TileSize;

            if (targetSize == sourceSize - 1)
            {
                coords.x = 1.0f / (float)sourceSize;
                coords.y = 1.0f / (float)sourceSize;
                coords.z = 1.0f - coords.x;
            }

            Graphics.Blit(gpuSlot.Texture, TargetTextureBuffer);

            LayerMaterial.SetTexture("_Target", TargetTextureBuffer);
            LayerMaterial.SetTexture("_Source", sourceGpuSlot.Texture);
            LayerMaterial.SetVector("_Coords", coords);

            Graphics.Blit(null, gpuSlot.Texture, LayerMaterial);
        }
    }
}