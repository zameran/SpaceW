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

using SpaceEngine.Core.Containers;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    /// <summary>
    /// Abstract tile cache for working with special <see cref="Tile"/> class in preprocessing.
    /// </summary>
    public abstract class TileCache
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int TileSize { get; protected set; }
        public int Channels { get; protected set; }
        public int Capacity { get; protected set; }

        private DictionaryQueue<int, Tile> Cache;

        public TileCache(int width, int height, int tileSize, int channels, int capacity = 20)
        {
            Width = width;
            Height = height;
            TileSize = tileSize;
            Channels = Mathf.Clamp(channels, 1, 4);
            Capacity = capacity;

            Cache = new DictionaryQueue<int, Tile>();
        }

        protected abstract float[] ReadTile(int tx, int ty);

        protected float[] GetTile(int tx, int ty)
        {
            var key = Tile.Key(tx, ty, Width / TileSize + 1);

            if (!Cache.ContainsKey(key))
            {
                var data = ReadTile(tx, ty);

                if (Cache.Count() == Capacity)
                {
                    // Vvict least recently used tile if cache is full
                    Cache.RemoveFirst();
                }

                // Create tile, put it at the end of tileCache
                var tile = new Tile(tx, ty, data);

                Cache.AddLast(key, tile);

                return data;
            }
            else
            {
                var tile = Cache.Get(key);

                Cache.Remove(key);
                Cache.AddLast(key, tile);

                return tile.Data;
            }
        }

        public virtual float GetTileHeight(int x, int y)
        {
            x = Mathf.Max(Mathf.Min(x, Width), 0);
            y = Mathf.Max(Mathf.Min(y, Height), 0);

            var tx = Mathf.Min(x, Width - 1) / TileSize;
            var ty = Mathf.Min(y, Height - 1) / TileSize;

            x = (x == Width ? TileSize : x % TileSize) + 2;
            y = (y == Height ? TileSize : y % TileSize) + 2;

            var data = GetTile(tx, ty);
            var offset = (x + y * (TileSize + 5));

            return (float)data[offset];
        }

        public virtual Vector4 GetTileColor(int x, int y)
        {
            x = Mathf.Max(Mathf.Min(x, Width - 1), 0);
            y = Mathf.Max(Mathf.Min(y, Height - 1), 0);

            var tx = x / TileSize;
            var ty = y / TileSize;

            x = x % TileSize + 2;
            y = y % TileSize + 2;

            var data = GetTile(tx, ty);
            var offset = (x + y * (TileSize + 4)) * Channels;

            var color = Vector4.zero;

            color.x = data[offset];

            if (Channels > 1)
            {
                color.y = data[offset + 1];
            }

            if (Channels > 2)
            {
                color.z = data[offset + 2];
            }

            if (Channels > 3)
            {
                color.w = data[offset + 3];
            }

            return color;
        }

        public virtual void Reset(int width, int height, int tileSize)
        {
            Cache.Clear();
            Width = width;
            Height = height;
            TileSize = tileSize;
        }
    }
}