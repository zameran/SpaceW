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
using SpaceEngine.Core.Exceptions;
using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain.InputMap
{
    /// <summary>
    ///     An abstract raster data map. A map is a 2D array of pixels, whose values can come from anywhere (this depends on how you implement the <see cref="GetValue" /> method).
    ///     A map can be read pixel by pixel, or tile by tile. The tiles are cached for better efficiency.
    /// </summary>
    public abstract class InputMap : MonoBehaviour, IInputMap
    {
        /// <summary>
        ///     Capacity of the cache.
        /// </summary>
        [SerializeField]
        private int Capacity = 200;

        [SerializeField]
        private bool IgnoreSizeRatio = true;

        private DictionaryQueue<Core.Tile.Tile.Id, Tile> Cache;

        /// <summary>
        ///     The tile size to use when reading this map by tile.
        ///     The width and height must be multiples of this size.
        /// </summary>
        [SerializeField]
        private Vector2 TileSize => new(Width, Height);

        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int Channels { get; }

        protected virtual void Awake()
        {
            if (TileSize.x <= 0 || TileSize.y <= 0)
            {
                throw new InvalidParameterException("Tile size must be greater than 0!");
            }

            if (!IgnoreSizeRatio)
            {
                if (Width % (int)TileSize.x != 0)
                {
                    throw new InvalidParameterException($"Tile size must be dividable by width! W:{Width}; S:{TileSize}; W%S:{Width % TileSize.x}");
                }

                if (Height % (int)TileSize.y != 0)
                {
                    throw new InvalidParameterException($"Tile size must be dividable by height! H:{Height}; S:{TileSize}; H%S:{Height % TileSize.y}");
                }
            }

            Cache = new DictionaryQueue<Core.Tile.Tile.Id, Tile>(new Core.Tile.Tile.EqualityComparerID());
        }

        protected virtual void Update()
        {
        }

        /// <summary>
        ///     This method uses a cache for better efficiency: it reads the <see cref="Tile" /> containing the given pixel,
        ///     if it is not already in cache, puts it in cache, and returns the requested pixel from this <see cref="Tile" />.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to be read.</param>
        /// <param name="y">The y coordinate of the pixel to be read.</param>
        /// <returns>Returns the value of the given pixel.</returns>
        public Vector4 Get(int x, int y)
        {
            x = Mathf.Max(Mathf.Min(x, Width - 1), 0);
            y = Mathf.Max(Mathf.Min(y, Height - 1), 0);

            var tx = x / (int)TileSize.x;
            var ty = y / (int)TileSize.y;

            x = x % (int)TileSize.x;
            y = y % (int)TileSize.y;

            var offset = (x + y * (int)TileSize.x) * Channels;
            var data = GetTile(tx, ty);
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

        public Vector2 GetTileSize()
        {
            return TileSize;
        }

        /// <summary>
        ///     Returns the value of the given pixel. Use it on your own.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to be read.</param>
        /// <param name="y">The y coordinate of the pixel to be read.</param>
        /// <returns>Returns the value of the (x, y) pixel.</returns>
        public abstract Vector4 GetValue(int x, int y);

        /// <summary>
        ///     Returns the values of the pixels of the given tile. The default implementation of this method calls <see cref="GetValue" /> to read each pixel.
        ///     If <see cref="GetValue" /> reads a value from disk, it is strongly advised to override this method for better efficiency.
        ///     In the [Tx * tileSize, (Tx + 1) * tileSize] - [Ty * tileSize, (Ty + 1) * tileSize] region.
        /// </summary>
        /// <param name="tx">The Tx coordinate of the pixel to be read.</param>
        /// <param name="ty">The Ty coordinate of the pixel to be read.</param>
        /// <returns>Returns the values of the pixels of the given tile.</returns>
        public virtual float[] GetValues(int tx, int ty)
        {
            var values = new float[(int)TileSize.x * (int)TileSize.y * Channels];

            for (var j = 0; j < (int)TileSize.y; ++j)
            {
                for (var i = 0; i < (int)TileSize.x; ++i)
                {
                    var value = GetValue(tx + i, ty + j);
                    var offset = (i + j * (int)TileSize.x) * Channels;

                    values[offset] = value.x;

                    if (Channels > 1)
                    {
                        values[offset + 1] = value.y;
                    }

                    if (Channels > 2)
                    {
                        values[offset + 2] = value.z;
                    }

                    if (Channels > 3)
                    {
                        values[offset + 3] = value.w;
                    }
                }
            }

            return values;
        }

        private float[] GetTile(int tx, int ty)
        {
            var key = new Core.Tile.Tile.Id(tx, ty);

            // TODO : Cache initialization...
            if (Cache == null)
            {
                Cache = new DictionaryQueue<Core.Tile.Tile.Id, Tile>(new Core.Tile.Tile.EqualityComparerID());
            }

            if (!Cache.ContainsKey(key))
            {
                var data = GetValues(tx * (int)TileSize.x, ty * (int)TileSize.y);

                if (Cache.Count() == Capacity)
                    // Evict least recently used tile if cache is full
                {
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
    }
}