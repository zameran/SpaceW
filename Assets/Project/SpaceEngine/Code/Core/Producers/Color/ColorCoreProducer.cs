#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
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

using SpaceEngine.Core.Bodies;
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
    public class ColorCoreProducer : TileProducer
    {
        public GameObject NormalsProducerGameObject;
        public GameObject ElevationProducerGameObject;

        private TileProducer NormalsProducer;
        private TileProducer ElevationProducer;

        public Material ColorMaterial;

        protected override void Start()
        {
            base.Start();

            if (TerrainNode == null) { TerrainNode = transform.parent.GetComponent<TerrainNode>(); }
            if (TerrainNode.Body == null) { TerrainNode.Body = transform.parent.GetComponentInParent<CelestialBody>(); }
            if (NormalsProducer == null) { NormalsProducer = NormalsProducerGameObject.GetComponent<TileProducer>(); }
            if (NormalsProducer.Cache == null) { NormalsProducer.InitCache(); }
            if (ElevationProducer == null) { ElevationProducer = ElevationProducerGameObject.GetComponent<TileProducer>(); }
            if (ElevationProducer.Cache == null) { ElevationProducer.InitCache(); }

            var tileSize = Cache.GetStorage(0).TileSize;
            var normalsTileSize = NormalsProducer.Cache.GetStorage(0).TileSize;
            var elevationTileSize = ElevationProducer.Cache.GetStorage(0).TileSize;

            if (tileSize != normalsTileSize)
            {
                throw new InvalidParameterException("Tile size must equal normals tile size" + string.Format(": {0}-{1}", tileSize, normalsTileSize));
            }

            if (tileSize != elevationTileSize)
            {
                throw new InvalidParameterException("Tile size must equal elevation tile size" + string.Format(": {0}-{1}", tileSize, elevationTileSize));
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

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            var tileWidth = gpuSlot.Owner.TileSize;
            var tileSize = tileWidth - (1 + GetBorder() * 2);

            //var parentTile = FindTile(level - 1, tx / 2, ty / 2, false, true);
            var rootQuadSize = TerrainNode.TerrainQuadRoot.Length;

            var tileWSD = Vector4.zero;
            tileWSD.x = (float)tileWidth;
            tileWSD.y = (float)rootQuadSize / (float)(1 << level) / (float)tileSize;
            tileWSD.z = (float)tileSize / (float)(TerrainNode.Body.GridResolution - 1);
            tileWSD.w = 0.0f;

            var tileSD = Vector2d.zero;

            tileSD.x = (0.5 + GetBorder()) / (tileWidth - 1 - GetBorder() * 2);
            tileSD.y = (1.0 + tileSD.x * 2.0);

            var offset = Vector4d.zero;

            offset.x = ((double)tx / (1 << level) - 0.5) * rootQuadSize;
            offset.y = ((double)ty / (1 << level) - 0.5) * rootQuadSize;
            offset.z = rootQuadSize / (1 << level);
            offset.w = TerrainNode.Body.Radius;

            NormalsProducer.Sampler.SetTile(ColorMaterial, level, tx, ty);
            ElevationProducer.Sampler.SetTile(ColorMaterial, level, tx, ty);

            ColorMaterial.SetFloat("_Level", level);
            ColorMaterial.SetVector("_TileWSD", tileWSD);
            ColorMaterial.SetVector("_TileSD", tileSD.ToVector2());
            ColorMaterial.SetVector("_Offset", offset.ToVector4());
            ColorMaterial.SetMatrix("_LocalToWorld", TerrainNode.FaceToLocal.ToMatrix4x4());

            if (TerrainNode.Body.NPS != null) TerrainNode.Body.NPS.SetUniforms(ColorMaterial);
            if (TerrainNode.Body.TCCPS != null) TerrainNode.Body.TCCPS.UpdateUniforms(ColorMaterial);

            Graphics.Blit(null, gpuSlot.Texture, ColorMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}