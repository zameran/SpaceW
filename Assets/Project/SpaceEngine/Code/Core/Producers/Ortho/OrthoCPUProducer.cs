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
    public class OrthoCPUProducer : TileProducer
    {
        /// <summary>
        /// The name of the file containing the tiles to load.
        /// </summary>
        [SerializeField]
        string FileName = "/Resources/Preprocess/Textures/Terrain/Color.dat";

        /// <summary>
        /// The number of components per pixel in the tiles to load.
        /// </summary>
        [HideInInspector]
        public int Channels;

        /// <summary>
        /// The size of the tiles to load, without borders. A tile contains [(tileSize + 4) * (tileSize + 4) * channels] samples.
        /// </summary>
        int TileSize;

        /// <summary>
        /// The size in pixels of the border around each tile. A tile contains [(tileSize + 4) * (tileSize + 4) * channels] samples.
        /// </summary>
        int Border;

        /// <summary>
        /// The maximum level of the stored tiles on disk (inclusive).
        /// </summary>
        int MaxLevel;

        /// <summary>
        /// The offsets of each tile on disk, relatively to offset, for each tile id
        /// </summary>
        long[] Offsets;

        public override void InitNode()
        {
            base.InitNode();

            var storage = Cache.GetStorage(0) as CPUTileStorage;

            if (storage == null)
            {
                throw new InvalidStorageException("Storage must be a CPUTileStorage");
            }

            if (storage.DataType != TileStorage.DATA_TYPE.BYTE)
            {
                throw new InvalidStorageException("Storage data type must be byte");
            }

            var data = new byte[7 * 4];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            MaxLevel = BitConverter.ToInt32(data, 0);
            TileSize = BitConverter.ToInt32(data, 4);
            Channels = BitConverter.ToInt32(data, 8);
            Border = BitConverter.ToInt32(data, 12);

            if (storage.Channels != Channels)
            {
                throw new InvalidStorageException($"Storage channels must be {Channels}");
            }

            var tilesCount = ((1 << (MaxLevel * 2 + 2)) - 1) / 3;

            Offsets = new long[tilesCount * 2];
            data = new byte[tilesCount * 2 * 8];

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(7 * 4, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (var i = 0; i < tilesCount * 2; i++)
            {
                Offsets[i] = BitConverter.ToInt64(data, 8 * i);
            }
        }

        public int GetChannels()
        {
            return Channels;
        }

        public override int GetBorder()
        {
            return Border;
        }

        public override bool HasTile(int level, int tx, int ty)
        {
            return level <= MaxLevel;
        }

        private int GetTileId(int level, int tx, int ty)
        {
            return tx + ty * (1 << level) + ((1 << (2 * level)) - 1) / 3;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var cpuSlot = slot[0] as CPUTileStorage.CPUSlot<byte>;

            if (cpuSlot == null)
            {
                throw new NullReferenceException("cpuSlot");
            }

            cpuSlot.ClearData();

            var tileid = GetTileId(level, tx, ty);
            var fsize = Offsets[2 * tileid + 1] - Offsets[2 * tileid];

            if (fsize > (TileSize + 2 * Border) * (TileSize + 2 * Border) * Channels)
            {
                throw new InvalidParameterException("File size of tile is larger than actual tile size!");
            }

            using (Stream stream = new FileStream(Application.dataPath + FileName, FileMode.Open))
            {
                stream.Seek(Offsets[2 * tileid], SeekOrigin.Begin);
                stream.Read(cpuSlot.Data, 0, cpuSlot.Size);
            }

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}