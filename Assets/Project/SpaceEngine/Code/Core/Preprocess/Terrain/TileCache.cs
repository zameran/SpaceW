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