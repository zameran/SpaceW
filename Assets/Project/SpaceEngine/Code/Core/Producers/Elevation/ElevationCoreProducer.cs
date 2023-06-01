﻿#region License
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
// Creation Date: 2017.02.23
// Creation Time: 4:46 PM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using UnityEngine;

namespace SpaceEngine.Core.Producers.Elevation
{
    public class ElevationCoreProducer : TileProducer
    {
        public Material ElevationMaterial;

        public override void InitNode()
        {
            base.InitNode();

            var tileSize = GetTileSize(0);

            if ((tileSize - GetBorder() * 2 - 1) % (TerrainNode.ParentBody.GridResolution - 1) != 0)
            {
                throw new InvalidParameterException($"Tile size - border * 2 - 1 must be divisible by grid mesh resolution - 1: {tileSize}-{GetBorder()}");
            }

            var storage = Cache.GetStorage(0) as GPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            if (gpuSlot == null) { throw new NullReferenceException("gpuSlot"); }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - (1 + GetBorder() * 2);

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            GPUTileStorage.GPUSlot parentGpuSlot = null;

            var upsample = level > 0;
            var parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);

            if (upsample)
            {
                if (parentTile != null) parentGpuSlot = parentTile.GetSlot(0) as GPUTileStorage.GPUSlot;
                else { throw new MissingTileException($"Find parent tile failed! {level - 1}:{tx / 2}-{ty / 2}"); }
            }

            if (parentGpuSlot == null && upsample) { throw new NullReferenceException("parentGpuSlot"); }

            if (upsample)
            {
                var parentTexture = parentGpuSlot.Texture;

                var dx = (float)(tx % 2) * (float)(tileSize / 2.0f);
                var dy = (float)(ty % 2) * (float)(tileSize / 2.0f);

                var coarseLevelOSL = new Vector4(dx / (float)parentTexture.width, dy / (float)parentTexture.height, 1.0f / (float)parentTexture.width, 0.0f);

                ElevationMaterial.SetTexture("_CoarseLevelSampler", parentTexture);
                ElevationMaterial.SetVector("_CoarseLevelOSL", coarseLevelOSL);
            }
            else
            {
                ElevationMaterial.SetTexture("_CoarseLevelSampler", null);
                ElevationMaterial.SetVector("_CoarseLevelOSL", new Vector4(-1.0f, -1.0f, -1.0f, -1.0f));
            }

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = (float)rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)tileSize / (float)(TerrainNode.ParentBody.GridResolution - 1);
            tileWSD.w = 0.0f;

            var tileScreenSize = (0.5 + (float)GetBorder()) / (tileWSD.x - 1 - (float)GetBorder() * 2);
            var tileSD = new Vector2d(tileScreenSize, 1.0 + tileScreenSize * 2.0);

            var offset = new Vector4d(((double)tx / (1 << level) - 0.5) * rootQuadSize,
                                      ((double)ty / (1 << level) - 0.5) * rootQuadSize,
                                      rootQuadSize / (1 << level),
                                      TerrainNode.ParentBody.Size);

            ElevationMaterial.SetVector("_TileWSD", tileWSD);
            ElevationMaterial.SetVector("_TileSD", tileSD.ToVector2());
            ElevationMaterial.SetFloat("_Amplitude", TerrainNode.ParentBody.Amplitude);
            ElevationMaterial.SetFloat("_Frequency", TerrainNode.ParentBody.Frequency);
            ElevationMaterial.SetVector("_Offset", offset.ToVector4());
            ElevationMaterial.SetMatrix("_LocalToWorld", TerrainNode.FaceToLocal.ToMatrix4x4());

            if (TerrainNode.ParentBody.TCCPS != null) TerrainNode.ParentBody.TCCPS.SetUniforms(ElevationMaterial);
            if (TerrainNode.ParentBody.TCCPS != null) TerrainNode.ParentBody.TCCPS.ToggleKeywords(ElevationMaterial);

            Graphics.Blit(null, gpuSlot.Texture, ElevationMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}