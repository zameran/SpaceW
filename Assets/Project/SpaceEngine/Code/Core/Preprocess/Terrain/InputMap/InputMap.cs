using SpaceEngine.Core.Containers;
using SpaceEngine.Core.Exceptions;

using UnityEngine;

using EqualityComparerID = SpaceEngine.Core.Tile.Tile.EqualityComparerID;
using Id = SpaceEngine.Core.Tile.Tile.Id;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    /// <summary>
    /// An abstract raster data map. A map is a 2D array of pixels, whose values can come from anywhere (this depends on how you implement the <see cref="GetValue"/> method). 
    /// A map can be read pixel by pixel, or tile by tile. The tiles are cached for better efficiency.
    /// </summary>
    public abstract class InputMap : MonoBehaviour, IInputMap
    {
        DictionaryQueue<Id, Tile> Cache;

        /// <summary>
        /// Capacity of the cache.
        /// </summary>
        [SerializeField]
        private int Capacity = 200;

        /// <summary>
        /// The tile size to use when reading this map by tile.
        /// The width and height must be multiples of this size.
        /// </summary>
        [SerializeField]
        private Vector2 TileSize => new Vector2(Width, Height);

        [SerializeField]
        private bool IgnoreSizeRatio = true;

        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int Channels { get; }

        public Vector2 GetTileSize()
        {
            return TileSize;
        }

        protected virtual void Awake()
        {
            if (TileSize.x <= 0 || TileSize.y <= 0) { throw new InvalidParameterException("Tile size must be greater than 0!"); }

            if (!IgnoreSizeRatio)
            {
                if (Width % (int)TileSize.x != 0) { throw new InvalidParameterException($"Tile size must be dividable by width! W:{Width}; S:{TileSize}; W%S:{Width % TileSize.x}"); }
                if (Height % (int)TileSize.y != 0) { throw new InvalidParameterException($"Tile size must be dividable by height! H:{Height}; S:{TileSize}; H%S:{Height % TileSize.y}"); }
            }

            Cache = new DictionaryQueue<Id, Tile>(new EqualityComparerID());
        }

        protected virtual void Update()
        {
            
        }

        /// <summary>
        /// Returns the value of the given pixel. Use it on your own.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel to be read.</param>
        /// <param name="y">The y coordinate of the pixel to be read.</param>
        /// <returns>Returns the value of the (x, y) pixel.</returns>
        public abstract Vector4 GetValue(int x, int y);

        /// <summary>
        /// Returns the values of the pixels of the given tile. The default implementation of this method calls <see cref="GetValue"/> to read each pixel. 
        /// If <see cref="GetValue"/> reads a value from disk, it is strongly advised to override this method for better efficiency.
        /// In the [Tx * tileSize, (Tx + 1) * tileSize] - [Ty * tileSize, (Ty + 1) * tileSize] region.
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
            var key = new Id(tx, ty);

            // TODO : Cache initialization...
            if (Cache == null) Cache = new DictionaryQueue<Id, Tile>(new EqualityComparerID());

            if (!Cache.ContainsKey(key))
            {
                var data = GetValues(tx * (int)TileSize.x, ty * (int)TileSize.y);

                if (Cache.Count() == Capacity)
                {
                    // Evict least recently used tile if cache is full
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

        /// <summary>
        /// This method uses a cache for better efficiency: it reads the <see cref="Tile"/> containing the given pixel,
        /// if it is not already in cache, puts it in cache, and returns the requested pixel from this <see cref="Tile"/>.
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
    }
}