using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Cache;
using SpaceEngine.Core.Tile.Layer;
using SpaceEngine.Core.Tile.Samplers;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Tile.Tasks;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Producer
{
    /// <summary>
    /// An abstract producer of tiles. A TileProducer must be inherited from and overide the <see cref="DoCreateTile"/> function to create the tiles data.
    /// Note that several TileProducer can share the same <see cref="TileCache"/>, and hence the same <see cref="TileStorage"/>.
    /// </summary>
    [RequireComponent(typeof(TileSampler))]
    public abstract class TileProducer : NodeSlave<TileProducer>
    {
        /// <summary>
        /// The tile cache game object that stores the tiles produced by this producer.
        /// </summary>
        [SerializeField]
        public GameObject CacheGameObject;

        public TileCache Cache { get; private set; }

        /// <summary>
        /// The name of the uniforms this producers data will be bound if used in a shader.
        /// </summary>
        [SerializeField]
        public string Name;

        /// <summary>
        /// Does this producer use the GPU?
        /// </summary>
        public bool IsGPUProducer = true;

        /// <summary>
        /// Does this producer calculaed as last one?
        /// </summary>
        public bool IsLastInSequence = false;

        /// <summary>
        /// Layers, that may modify the tile created by this producer and are optional.
        /// </summary>
        public TileLayer[] Layers { get; protected set; }

        /// <summary>
        /// The <see cref="TileSampler"/> associated with this producer.
        /// </summary>
        public TileSampler Sampler { get; protected set; }

        /// <summary>
        /// The id of this producer. This id is local to the <see cref="TileCache"/> used by this producer, and is used to distinguish all the producers that use this cache.
        /// </summary>
        public int ID { get; protected set; }

        public TerrainNode TerrainNode { get { return Sampler.TerrainNode; } set { Sampler.TerrainNode = value; } }

        #region NodeSlave<TileProducer>

        public override void InitNode()
        {
            if (Cache != null) return;

            Cache = CacheGameObject.GetComponent<TileCache>();
            ID = Cache.NextProducerId;
            Cache.InsertProducer(ID, this);

            // Get any layers attached to same GameObject. May have 0 to many attached.
            Layers = GetComponents<TileLayer>();

            if (Layers != null) { foreach (var layer in Layers) { layer.InitNode(); } }

            // Get the samplers attached to GameObject. Must have one sampler attahed.
            Sampler = GetComponent<TileSampler>();
        }

        public override void UpdateNode()
        {
            if (Layers != null) { foreach (var layer in Layers) { layer.UpdateNode(); } }
        }

        #endregion

        public int GetTileSize(int i)
        {
            return Cache.GetStorage(i).TileSize;
        }

        public int GetTileSizeMinBorder(int i)
        {
            return GetTileSize(i) - GetBorder() * 2;
        }

        /// <summary>
        /// Tiles made of raster data may have a border that contains the value of the neighboring pixels of the tile. 
        /// For instance if the tile size (returned by <see cref="TileStorage.TileSize"/>) is 196, and if the tile border is 2, 
        /// this means that the actual tile data is 192x192 pixels, with a 2 pixel border that contains the value of the neighboring pixels. 
        /// Using a border introduces data redundancy but is usefull to get the value of the neighboring pixels of a tile without needing to load the neighboring tiles.
        /// </summary>
        /// <returns>Returns the size in pixels of the border of each tile.</returns>
        public virtual int GetBorder()
        {
            return 0;
        }

        /// <summary>
        /// Check if this producer can produce the given tile.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <returns>Returns 'True' if this producer can produce the given tile.</returns>
        public virtual bool HasTile(int level, int tx, int ty)
        {
            return true;
        }

        /// <summary>
        /// Check if this producer can produce the children of the given tile.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <returns>Returns 'True' if this producer can produce the children of the given tile.</returns>
        public virtual bool HasChildren(int level, int tx, int ty)
        {
            return HasTile(level + 1, 2 * tx, 2 * ty);
        }

        /// <summary>
        /// Decrements the number of users of this tile by one. If this number becomes 0 the tile is marked as unused, and so can be evicted from the cache at any moment.
        /// </summary>
        /// <param name="tile">Tile to put.</param>
        public virtual void PutTile(Tile tile)
        {
            Cache.PutTile(tile);
        }

        /// <summary>
        /// Returns the requested tile, creating it if necessary. 
        /// If the tile is currently in use - it is returned directly.
        /// If it is in cache but unused - it marked as used and returned.
        /// Otherwise a new tile is created, marked as used and returned.
        /// In all cases the number of users of this tile is incremented by one.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <returns>Returns the requested tile.</returns>
        public virtual Tile GetTile(int level, int tx, int ty)
        {
            return Cache.GetTile(ID, level, tx, ty);
        }

        /// <summary>
        /// Looks for a tile in the <see cref="TileCache"/> of this <see cref="TileProducer"/>.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <param name="includeUnusedCache">Include unused tiles in the search, or not?</param>
        /// <param name="done">Check that tile's creation task is done?</param>
        /// <returns>
        /// Returns the requsted tile, or null if it's not in the <see cref="TileCache"/> or if it's not ready. 
        /// This method doesn't change the number of users of the returned tile.
        /// </returns>
        public virtual Tile FindTile(int level, int tx, int ty, bool includeUnusedCache, bool done)
        {
            var tile = Cache.FindTile(ID, level, tx, ty, includeUnusedCache);

            if (done && tile != null && !tile.Task.IsDone)
            {
                tile = null;
            }

            return tile;
        }

        /// <summary>
        /// Creates a <see cref="Utilities.Schedular.Task"/> to produce the data of the given tile.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <param name="slot">Slot, where the crated tile data must be stored.</param>
        public virtual CreateTileTask CreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            return new CreateTileTask(this, level, tx, ty, slot);
        }

        /// <summary>
        /// Creates the given tile. 
        /// If this task requires tiles produced by other. 
        /// The default implementation of this method calls <see cref="TileLayer.DoCreateTile"/> on each Layer of this producer.
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <param name="slot">Slot, where the crated tile data must be stored.</param>
        public virtual void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            if (Layers == null) return;

            foreach (var layer in Layers)
            {
                layer.DoCreateTile(level, tx, ty, slot);
            }
        }

        /// <summary>
        /// Basically, should call <see cref="DoCreateTile"/> and wait some time or frames.
        /// In the base implementation will wait one frame after each <see cref="DoCreateTile"/> call, and one frame after all.
        /// <remarks>WARNING! <see cref="CreateTileTask.IsDone"/> field will be changed here, after all work is done! Use this with attention!</remarks> 
        /// </summary>
        /// <param name="level">The tile's quadtree level.</param>
        /// <param name="tx">The tile's quadtree X coordinate.</param>
        /// <param name="ty">The tile's quadtree Y coordinate.</param>
        /// <param name="slot">Slot, where the crated tile data must be stored.</param>
        /// <param name="Callback">Callback after all. Finish the task here and do some extra post-calculation work.</param>
        public virtual IEnumerator DoCreateTileCoroutine(int level, int tx, int ty, List<TileStorage.Slot> slot, Action Callback)
        {
            var samplersOrder = TerrainNode.SamplersOrder;
            var currentIndexInSamplerQueue = samplersOrder.OrderList.IndexOf(Sampler);
            var samplersToWait = samplersOrder.OrderList.GetRange(0, currentIndexInSamplerQueue);

            if (currentIndexInSamplerQueue != 0)
            {
                foreach (var samplerToWait in samplersToWait)
                {
                    do
                    {
                        yield return Yielders.EndOfFrame;
                    }
                    while (samplerToWait.Producer.FindTile(level, tx, ty, false, true) == null);
                }
            }

            this.DoCreateTile(level, tx, ty, slot); // Do our work...

            var afterWorkAwaitFramesCount = GetAwaitingFramesCount(level); // Calculate idle frames count per particular tile LOD level...

            for (var i = 0; i < afterWorkAwaitFramesCount; i++) // Wait it...
            {
                yield return Yielders.EndOfFrame;
            }

            yield return Yielders.EndOfFrame;

            if (Callback != null) Callback();
        }

        private int GetAwaitingFramesCount(int level)
        {
            return 4 * (level + 1);
        }
    }
}