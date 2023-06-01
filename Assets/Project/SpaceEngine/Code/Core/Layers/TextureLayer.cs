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
using System.Collections.Generic;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Layer;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Tools;
using UnityEngine;

namespace SpaceEngine.Core.Layers
{
    /// <summary>
    ///     This layer modifies the tile of it's <see cref="TileProducer" />
    ///     by applying special 'filter' into the tiles, produced by another <see cref="TileProducer" />, after they have been transformed via a GPU Program.
    /// </summary>
    public class TextureLayer : TileLayer
    {
        public GameObject SourceProducerGameObject;

        public GameObject TargetProducerGameObject;

        public Material LayerMaterial;

        private TileProducer SourceProducer;

        private TileProducer TargetProducer;

        private RenderTexture TargetTextureBuffer;

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            if (TargetTextureBuffer == null)
            {
                TargetTextureBuffer = new RenderTexture(gpuSlot.Texture.descriptor);
            }

            GPUTileStorage.GPUSlot sourceGpuSlot = null;

            var sourceTile = SourceProducer.FindTile(level, tx, ty, false, true);

            if (sourceTile != null)
            {
                sourceGpuSlot = sourceTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            }
            else
            {
                throw new MissingTileException("Find source producer tile failed");
            }

            if (sourceGpuSlot == null)
            {
                throw new MissingTileException("Find source tile failed");
            }

            var coords = Vector3.forward;
            var targetSize = TargetProducer.Cache.GetStorage(0).TileSize;
            var sourceSize = SourceProducer.Cache.GetStorage(0).TileSize;

            if (targetSize == sourceSize - 1)
            {
                coords.x = 1.0f / sourceSize;
                coords.y = 1.0f / sourceSize;
                coords.z = 1.0f - coords.x;
            }

            Graphics.Blit(gpuSlot.Texture, TargetTextureBuffer);

            LayerMaterial.SetTexture("_Target", TargetTextureBuffer);
            LayerMaterial.SetTexture("_Source", sourceGpuSlot.Texture);
            LayerMaterial.SetVector("_Coords", coords);

            Graphics.Blit(null, gpuSlot.Texture, LayerMaterial);
        }

        #region NodeSlave<TileLayer>

        public override void InitNode()
        {
            base.InitNode();

            if (SourceProducerGameObject != null)
            {
                if (SourceProducer == null)
                {
                    SourceProducer = SourceProducerGameObject.GetComponent<TileProducer>();
                }
            }

            if (TargetProducerGameObject != null)
            {
                if (TargetProducer == null)
                {
                    TargetProducer = TargetProducerGameObject.GetComponent<TileProducer>();
                }
            }

            if (SourceProducer == null)
            {
                throw new NullReferenceException("Source producer is null!");
            }

            if (TargetProducer == null)
            {
                throw new NullReferenceException("Target producer is null!");
            }

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

            if (TargetTextureBuffer != null)
            {
                TargetTextureBuffer.ReleaseAndDestroy();
            }
        }

        #endregion
    }
}