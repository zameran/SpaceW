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

using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SpaceEngine.Core
{
    /// <summary>
    /// A TileProducer to load elevation residuals from disk to CPU memory.
    /// </summary>
    public class ResidualProducer : TileProducer
    {
        /// <summary>
        /// The name of the file containing the residual tiles to load.
        /// </summary>
        [SerializeField]
        string FileName = "/Resources/Preprocess/Textures/Terrain/Height.dat";

        [SerializeField]
        float ZScale = 1.0f;

        /// <summary>
        /// The size of the tiles whose level (on disk) is at least <see cref="MinLevel"/>.
        /// This size does not include the borders. A tile contains (TileSize + 5) * (TileSize + 5) samples.
        /// </summary>
        int ResidualTileSize;

        /// <summary>
        /// The level of the root of the tile pyramid managed by this producer in the global set of tile pyramids describing a terrain.
        /// </summary>
        int RootLevel;

        /// <summary>
        /// The stored tiles level that must be considered as the root level in this producer. Must be less than or equal to <see cref="MinLevel"/>.
        /// </summary>
        int DeltaLevel;

        /// <summary>
        /// The logical x coordinate of the root of the tile pyramid managed by this producer in the global set of tile pyramids describing a terrain.
        /// </summary>
        int RootTx;

        /// <summary>
        /// The logical y coordinate of the root of the tile pyramid managed by this producer in the global set of tile pyramids describing a terrain.
        /// </summary>
        int RootTy;

        /// <summary>
        /// The stored tile level of the first tile of size TileSize.
        /// </summary>
        int MinLevel;

        /// <summary>
        /// The maximum level of the stored tiles on disk (inclusive, and relatively to rootLevel).
        /// </summary>
        int MaxLevel;

        /// <summary>
        /// A scaling factor to be applied to all residuals read from disk.
        /// </summary>
        float Scale;

        /// <summary>
        /// The offsets of each tile on disk, relatively to offset, for each tile id <see cref="GetTileId"/>
        /// </summary>
        long[] Offsets;

        float[] MaxR;

        public override void InitNode()
        {
            base.InitNode();

            var storage = Cache.GetStorage(0) as CPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Storage must be a CPUTileStorage!");
            }

            if (storage.Channels != 1)
            {
                throw new InvalidStorageException("Storage channels must be 1!");
            }

            if (storage.DataType != TileStorage.DATA_TYPE.FLOAT)
            {
                throw new InvalidStorageException("Storage data type must be float!");
            }

            var data = new byte[7 * 4];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            MinLevel = BitConverter.ToInt32(data, 0);
            MaxLevel = BitConverter.ToInt32(data, 4);
            ResidualTileSize = BitConverter.ToInt32(data, 8);
            RootLevel = BitConverter.ToInt32(data, 12);
            RootTx = BitConverter.ToInt32(data, 16);
            RootTy = BitConverter.ToInt32(data, 20);
            Scale = BitConverter.ToSingle(data, 24);

            MaxR = new float[MaxLevel + 1];

            data = new byte[MaxR.Length * 4];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(7 * 4, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (var i = 0; i < MaxR.Length; i++)
            {
                MaxR[i] = BitConverter.ToSingle(data, 4 * i);
            }

            DeltaLevel = RootLevel == 0 ? DeltaLevel : 0;
            Scale = Scale * ZScale;

            if (DeltaLevel > MinLevel) { throw new InvalidParameterException("Delta level can not be greater than min level!"); }

            var tilesCount = MinLevel + ((1 << (Mathf.Max(MaxLevel - MinLevel, 0) * 2 + 2)) - 1) / 3;

            Offsets = new long[tilesCount * 2];

            data = new byte[tilesCount * 2 * 8];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek((7 + MaxR.Length) * 4, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (var i = 0; i < tilesCount * 2; i++)
            {
                Offsets[i] = BitConverter.ToInt64(data, 8 * i);
            }
        }

        public override int GetBorder()
        {
            return 2;
        }

        private int GetResidualTileSize(int level)
        {
            return level < MinLevel ? ResidualTileSize >> (MinLevel - level) : ResidualTileSize;
        }

        private int GetTileId(int level, int tx, int ty)
        {
            if (level < MinLevel)
            {
                return level;
            }
            else
            {
                var levelLength = Mathf.Max(level - MinLevel, 0);

                return MinLevel + tx + ty * (1 << levelLength) + ((1 << (2 * levelLength)) - 1) / 3;
            }
        }

        public override bool HasTile(int level, int tx, int ty)
        {
            var levelLength = level + DeltaLevel - RootLevel;

            if (levelLength >= 0 && (tx >> levelLength) == RootTx && (ty >> levelLength) == RootTy)
            {
                if (levelLength <= MaxLevel)
                    return true;
            }

            return false;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var levelLength = level + DeltaLevel - RootLevel;

            if (!(levelLength >= 0 && (tx >> levelLength) == RootTx && (ty >> levelLength) == RootTy)) { return; }

            level = levelLength;
            tx = tx - (RootTx << level);
            ty = ty - (RootTy << level);

            var cpuSlot = slot[0] as CPUTileStorage.CPUSlot<float>;

            if (cpuSlot == null) { throw new NullReferenceException("cpuSlot"); }

            cpuSlot.ClearData();

            ReadTile(level, tx, ty, cpuSlot.Data);

            base.DoCreateTile(level, tx, ty, slot);
        }

        private void ReadTile(int level, int tx, int ty, float[] result)
        {
            var tileSize = GetResidualTileSize(level) + 5;
            var tileId = GetTileId(level, tx, ty);

            var fileTileSize = Offsets[2 * tileId + 1] - Offsets[2 * tileId];
            long actualTileSize = (ResidualTileSize + 5) * (ResidualTileSize + 5) * 2;

            if (fileTileSize > actualTileSize)
            {
                throw new InvalidParameterException($"File size of tile is larger than actual tile size! {fileTileSize}:{actualTileSize}");
            }

            var data = new byte[fileTileSize];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(Offsets[2 * tileId], SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (var j = 0; j < tileSize; ++j)
            {
                for (var i = 0; i < tileSize; ++i)
                {
                    var offset = 2 * (i + j * tileSize);
                    var value = (short)((short)data[offset + 1] << 8 | data[offset]);

                    result[i + j * (ResidualTileSize + 5)] = (float)value / (float)short.MaxValue * MaxR[level] * Scale;
                }
            }
        }
    }
}