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
using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Vectors;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using UnityEngine;

namespace SpaceEngine.Core.Producers.Normal
{
    public class NormalsCoreProducer : TileProducer
    {
        public GameObject ElevationProducerGameObject;

        private TileProducer ElevationProducer;

        public Material NormalsMaterial;

        public override void InitNode()
        {
            base.InitNode();

            if (ElevationProducerGameObject != null)
            {
                if (ElevationProducer == null) { ElevationProducer = ElevationProducerGameObject.GetComponent<TileProducer>(); }
            }

            var tileSize = Cache.GetStorage(0).TileSize;
            var elevationTileSize = ElevationProducer.Cache.GetStorage(0).TileSize;

            if (tileSize != elevationTileSize)
            {
                throw new InvalidParameterException($"Tile size must equal elevation tile size: {tileSize}-{elevationTileSize}");
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
            return ElevationProducer.HasTile(level, tx, ty);
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;
            var elevationTile = ElevationProducer.FindTile(level, tx, ty, false, true);

            GPUTileStorage.GPUSlot elevationGpuSlot = null;

            if (elevationTile != null) elevationGpuSlot = elevationTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            else { throw new MissingTileException("Find elevation tile failed"); }

            if (gpuSlot == null) { throw new NullReferenceException("gpuSlot"); }
            if (elevationGpuSlot == null) { throw new NullReferenceException("elevationGpuSlot"); }

            var tileWidth = gpuSlot.Owner.TileSize;
            var elevationTex = elevationGpuSlot.Texture;
            var elevationOSL = new Vector4(0.25f / (float)elevationTex.width, 0.25f / (float)elevationTex.height, 1.0f / (float)elevationTex.width, 0.0f);

            var D = TerrainNode.TerrainQuadRoot.Length;
            var R = D / 2.0;

            // NOTE : Body shape dependent...
            var x0 = (double)(tx) / (double)(1 << level) * D - R;
            var x1 = (double)(tx + 1) / (double)(1 << level) * D - R;
            var y0 = (double)(ty) / (double)(1 << level) * D - R;
            var y1 = (double)(ty + 1) / (double)(1 << level) * D - R;

            var p0 = new Vector3d(x0, y0, R);
            var p1 = new Vector3d(x1, y0, R);
            var p2 = new Vector3d(x0, y1, R);
            var p3 = new Vector3d(x1, y1, R);
            var pc = new Vector3d((x0 + x1) * 0.5, (y0 + y1) * 0.5, R);

            double l0 = 0, l1 = 0, l2 = 0, l3 = 0;

            var v0 = p0.Normalized(out l0);
            var v1 = p1.Normalized(out l1);
            var v2 = p2.Normalized(out l2);
            var v3 = p3.Normalized(out l3);
            var vc = (v0 + v1 + v2 + v3) * 0.25;

            var deformedCorners = new Matrix4x4d(v0.x * R - vc.x * R, v1.x * R - vc.x * R, v2.x * R - vc.x * R, v3.x * R - vc.x * R,
                                                 v0.y * R - vc.y * R, v1.y * R - vc.y * R, v2.y * R - vc.y * R, v3.y * R - vc.y * R,
                                                 v0.z * R - vc.z * R, v1.z * R - vc.z * R, v2.z * R - vc.z * R, v3.z * R - vc.z * R, 
                                                 1.0, 1.0, 1.0, 1.0);

            var deformedVerticals = new Matrix4x4d(v0.x, v1.x, v2.x, v3.x, v0.y, v1.y, v2.y, v3.y, v0.z, v1.z, v2.z, v3.z, 0.0, 0.0, 0.0, 0.0);

            var uz = pc.Normalized();
            var ux = new Vector3d(0.0, 1.0, 0.0).Cross(uz).Normalized();
            var uy = uz.Cross(ux);

            var worldToTangentFrame = new Matrix4x4d(ux.x, ux.y, ux.z, 0.0, uy.x, uy.y, uy.z, 0.0, uz.x, uz.y, uz.z, 0.0, 0.0, 0.0, 0.0, 0.0);

            NormalsMaterial.SetMatrix("_PatchCorners", deformedCorners.ToMatrix4x4());
            NormalsMaterial.SetMatrix("_PatchVerticals", deformedVerticals.ToMatrix4x4());
            NormalsMaterial.SetVector("_PatchCornerNorms", new Vector4((float)l0, (float)l1, (float)l2, (float)l3));
            NormalsMaterial.SetVector("_Deform", new Vector4((float)x0, (float)y0, (float)D / (float)(1 << level), (float)R));
            NormalsMaterial.SetMatrix("_WorldToTangentFrame", worldToTangentFrame.ToMatrix4x4());

            NormalsMaterial.SetVector("_TileSD", new Vector2((float)tileWidth, (float)(tileWidth - 1) / (float)(TerrainNode.ParentBody.GridResolution - 1)));
            NormalsMaterial.SetTexture("_ElevationSampler", elevationTex);
            NormalsMaterial.SetVector("_ElevationOSL", elevationOSL);
            NormalsMaterial.SetFloat("_Level", (float)(1 << level) / 2.0f);

            Graphics.Blit(null, gpuSlot.Texture, NormalsMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}