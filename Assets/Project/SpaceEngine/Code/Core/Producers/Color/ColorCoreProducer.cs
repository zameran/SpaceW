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
// Creation Date: 2017.03.07
// Creation Time: 10:30 PM
// Creator: zameran
#endregion

using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core
{
    public class ColorCoreProducer : TileProducer
    {
        public GameObject NormalsProducerGameObject;
        public GameObject ElevationProducerGameObject;

        private TileProducer NormalsProducer;
        private TileProducer ElevationProducer;

        public Material ColorMaterial;

        public override void InitNode()
        {
            base.InitNode();

            if (NormalsProducerGameObject != null)
            {
                if (NormalsProducer == null) { NormalsProducer = NormalsProducerGameObject.GetComponent<TileProducer>(); }
            }

            if (ElevationProducerGameObject != null)
            {
                if (ElevationProducer == null) { ElevationProducer = ElevationProducerGameObject.GetComponent<TileProducer>(); }
            }

            var tileSize = Cache.GetStorage(0).TileSize;
            var normalsTileSize = NormalsProducer.Cache.GetStorage(0).TileSize;
            var elevationTileSize = ElevationProducer.Cache.GetStorage(0).TileSize;

            if (tileSize != normalsTileSize)
            {
                throw new InvalidParameterException($"Tile size must equal normals tile size: {tileSize}-{normalsTileSize}");
            }

            if (tileSize != elevationTileSize)
            {
                throw new InvalidParameterException($"Tile size must equal elevation tile size: {tileSize}-{elevationTileSize}");
            }

            if (GetBorder() != NormalsProducer.GetBorder())
            {
                throw new InvalidParameterException("Border size must be equal to normals border size");
            }

            if (GetBorder() != ElevationProducer.GetBorder())
            {
                throw new InvalidParameterException("Border size must be equal to elevation border size");
            }

            if (!(Cache.GetStorage(0) is GPUTileStorage))
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }

        }

        public override bool HasTile(int level, int tx, int ty)
        {
            return ElevationProducer.HasTile(level, tx, ty) && NormalsProducer.HasTile(level, tx, ty);
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;
            var normalsTile = NormalsProducer.FindTile(level, tx, ty, false, true);
            var elevationTile = ElevationProducer.FindTile(level, tx, ty, false, true);

            GPUTileStorage.GPUSlot normalsGpuSlot = null;

            if (normalsTile != null) normalsGpuSlot = normalsTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            else { throw new MissingTileException("Find normals tile failed"); }

            GPUTileStorage.GPUSlot elevationGpuSlot = null;

            if (elevationTile != null) elevationGpuSlot = elevationTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            else { throw new MissingTileException("Find elevation tile failed"); }

            if (gpuSlot == null) { throw new NullReferenceException("gpuSlot"); }
            if (elevationGpuSlot == null) { throw new NullReferenceException("elevationGpuSlot"); }
            if (normalsGpuSlot == null) { throw new NullReferenceException("normalsGpuSlot"); }

            var tileWidth = gpuSlot.Owner.TileSize;
            var normalsTex = normalsGpuSlot.Texture;
            var elevationTex = elevationGpuSlot.Texture;
            var normalsOSL = new Vector4(0.25f / (float)normalsTex.width, 0.25f / (float)normalsTex.height, 1.0f / (float)normalsTex.width, 0.0f);
            var elevationOSL = new Vector4(0.25f / (float)elevationTex.width, 0.25f / (float)elevationTex.height, 1.0f / (float)elevationTex.width, 0.0f);
            var tileSize = tileWidth - (float)(1 + GetBorder() * 2);

            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

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

            ColorMaterial.SetTexture("_NormalsSampler", normalsTex);
            ColorMaterial.SetVector("_NormalsOSL", normalsOSL);
            ColorMaterial.SetTexture("_ElevationSampler", elevationTex);
            ColorMaterial.SetVector("_ElevationOSL", elevationOSL);

            ColorMaterial.SetFloat("_Level", level);
            ColorMaterial.SetVector("_TileWSD", tileWSD);
            ColorMaterial.SetVector("_TileSD", tileSD.ToVector2());
            ColorMaterial.SetVector("_Offset", offset.ToVector4());
            ColorMaterial.SetMatrix("_LocalToWorld", TerrainNode.FaceToLocal.ToMatrix4x4());

            if (TerrainNode.ParentBody.TCCPS != null) TerrainNode.ParentBody.TCCPS.SetUniforms(ColorMaterial);
            if (TerrainNode.ParentBody.TCCPS != null) TerrainNode.ParentBody.TCCPS.ToggleKeywords(ColorMaterial);

            Graphics.Blit(null, gpuSlot.Texture, ColorMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}