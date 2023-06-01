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

using System.Collections.Generic;
using SpaceEngine.Core.Containers;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using UnityEngine;

namespace SpaceEngine.Core.Tile.Cache
{
    /// <summary>
    /// A cache of tiles to avoid recomputing recently produced tiles. 
    /// A tile cache keeps track of which tiles (identified by their <see cref="Tile.Id"/>) are
    /// currently stored in an associated <see cref="Storage.TileStorage"/>. 
    /// It also keeps track of which tiles are in use, and which are not. 
    /// Unused tiles are kept in the <see cref="TileStorage"/> as long as possible, in order to avoid re-creating them, if they become needed again. 
    /// But the storage associated with unused tiles can be reused to store
    /// other tiles at any moment (in this case we say that a tile is evicted from the cache of unused tiles).
    /// Conversely, the storage associated with tiles currently in use cannot be reaffected until these tiles become unused. 
    /// A tile is in use when it is returned by <see cref="GetTile"/>, and becomes unused when <see cref="PutTile"/> is called (more
    /// precisely when the number of users of this tile becomes 0, this number being
    /// incremented and decremented by <see cref="GetTile"/> and <see cref="PutTile"/>, respectively). 
    /// The tiles that are needed to render the current frame should be declared in use, so that they are not evicted between their creation and 
    /// their actual rendering.
    /// A cache can have multiple <see cref="Storage.TileStorage"/>'s attached to it and the slot created is made up
    /// of a slot from each of the <see cref="Storage.TileStorage"/>'s. 
    /// This is so <see cref="TileProducer"/> can generate tiles that contain multiple types of data associated with the same tile. 
    /// </summary>
    public class TileCache : NodeSlave<TileCache>
    {
        private static int nextProducerId;

        /// <summary>
        /// Next local identifier to be used for a <see cref="TileProducer"/> using this cache.
        /// </summary>
        public int NextProducerId => nextProducerId++;

        /// <summary>
        /// The total number of slots managed by the <see cref="Storage.TileStorage"/> attached to the cache.
        /// </summary>
        [SerializeField]
        public ushort Capacity = 1296;

        /// <summary>
        /// The storage to store the tiles data.
        /// </summary>
        public TileStorage[] TileStorage;

        /// <summary>
        /// The length of tiles data storage.
        /// </summary>
        public int TileStorageLength => TileStorage.Length;

        /// <summary>
        /// The tiles currently in use. 
        /// These tiles cannot be evicted from the cache and from the <see cref="Storage.TileStorage"/>, until they become unused. 
        /// Maps tile identifiers to actual tiles.
        /// </summary>
        private Dictionary<Tile.TId, Tile> UsedTiles;

        public int UsedTilesCount => UsedTiles.Count;

        /// <summary>
        /// The unused tiles. 
        /// These tiles can be evicted from the cache at any moment.
        /// Uses a custom container (<see cref="DictionaryQueue{TKey,TValue}"/>), 
        /// that can store tiles by there <see cref="Tile.TId"/> for fast look up and also keeps track of the order the tiles were inserted,
        /// so it can also act as a queue.
        /// </summary>
        public DictionaryQueue<Tile.TId, Tile> UnusedTiles;

        public int UnusedTilesCount => UnusedTiles.Count();

        /// <summary>
        /// The producers that use this <see cref="TileCache"/>. Maps local producer identifiers to actual producers.
        /// </summary>
        private Dictionary<int, TileProducer> Producers;

        /// <summary>
        /// Temporary <see cref="Tile.TId"/> class object obly used in <see cref="FindTile"/>.
        /// </summary>
        protected Tile.TId TileTIDBuffer { get; private set; }

        /// <summary>
        /// Maximum tiles ever used.
        /// </summary>
        public int MaximumUsedTiles { get; private set; }

        #region NodeSlave<TileCache>

        public override void InitNode()
        {
            TileStorage = GetComponents<TileStorage>();
            Producers = new Dictionary<int, TileProducer>();
            UsedTiles = new Dictionary<Tile.TId, Tile>(new Tile.EqualityComparerTID());
            UnusedTiles = new DictionaryQueue<Tile.TId, Tile>(new Tile.EqualityComparerTID());
            TileTIDBuffer = new Tile.TId(-1, -1, 0, 0);
        }

        public override void UpdateNode()
        {

        }

        #endregion

        public void InsertProducer(int id, TileProducer producer)
        {
            //Debug.Log(string.Format("TileCache: Producer {0} inserted in to cache with ID: {1}", producer.name, id));
            
            if (Producers == null)
            {
                Debug.LogWarning($"TileCache: Still not Initialized, but used! {producer.name}");
                
                return;
            }
            
            if (Producers.ContainsKey(id))
            {
                Debug.Log($"TileCache: Producer with {id} already inserted!");
            }
            else
            {
                Producers.Add(id, producer);
            }
        }

        /// <summary>
        /// The storage used to store the actual tiles data.
        /// </summary>
        /// <param name="i">Index of the storage.</param>
        /// <returns>Returns the storage used to store the actual tiles data.</returns>
        public TileStorage GetStorage(int i)
        {
            if (i >= TileStorageLength)
            {
                Debug.Log($"TileCache: Tile storage at location {i} does not exist!");
            }

            return TileStorage[i];
        }

        /// <summary>
        /// Call this when a tile is no longer needed.
        /// If the number of users of the tile is 0, then the tile will be moved from the used to the unused cache.
        /// </summary>
        /// <param name="tile">Tile.</param>
        public void PutTile(Tile tile)
        {
            if (tile == null) return;

            tile.DecrementUsers();

            // If there are no more users of this tile move the tile from the used cahce to the unused cache
            if (tile.Users <= 0)
            {
                var id = tile.TID;

                if (UsedTiles.ContainsKey(id))
                {
                    UsedTiles.Remove(id);
                }

                if (!UnusedTiles.ContainsKey(id))
                {
                    UnusedTiles.AddLast(id, tile);
                }
            }

        }

        /// <summary>
        /// Creates a new slot for a tile. 
        /// A slot is made up of a slot from each of the <see cref="Storage.TileStorage"/>s attached to the <see cref="TileCache"/>.
        /// If anyone of the storages runs out of slots then null will be returned and the program should abort if this happens.
        /// </summary>
        /// <returns>New <see cref="Storage.TileStorage.Slot"/> instance.</returns>
        private List<TileStorage.Slot> AddSlot()
        {
            var slots = new List<TileStorage.Slot>();

            foreach (var storage in TileStorage)
            {
                var slot = storage.AddSlot();

                if (slot == null) return null;

                slots.Add(slot);
            }

            return slots;
        }

        /// <summary>
        /// Call this if a tile is needed. 
        /// Will move the tile from the unused to the used cache if its is found there.
        /// If the tile is not found then a new tile will be created with a new slot. 
        /// If there are no more free slots then the cache capacity has not been set to a high enough value and the program must abort.
        /// </summary>
        /// <param name="producerId">Producer id.</param>
        /// <param name="level">Tile level.</param>
        /// <param name="tx">Tile Tx.</param>
        /// <param name="ty">Tile Ty.</param>
        /// <returns>Tile instance.</returns>
        public Tile GetTile(int producerId, int level, int tx, int ty)
        {
            // If this producer id does not exist can not create tile.
            if (!Producers.ContainsKey(producerId))
            {
                Debug.Log($"TileCache.GetTile: Producer with ID: {producerId} not been inserted into cache!");
                return null;
            }

            var id = Tile.GetTId(producerId, level, tx, ty);

            Tile tile = null;

            // If tile is not in the used cache
            if (!UsedTiles.ContainsKey(id))
            {
                // If tile is also not in the unused cache
                if (!UnusedTiles.ContainsKey(id))
                {
                    var slot = AddSlot();

                    // If there are no more free slots then start recyling slots from the unused tiles
                    if (slot == null && !UnusedTiles.Empty())
                    {
                        // Remove the tile and recylce its slot
                        slot = UnusedTiles.RemoveFirst().Slot;
                    }

                    // If a slot is found create a new tile with a new task
                    if (slot != null)
                    {
                        var task = Producers[producerId].CreateTile(level, tx, ty, slot);

                        tile = new Tile(producerId, level, tx, ty, task);
                    }

                    // If a free slot is not found then program has must abort. Try setting the cache capacity to higher value.
                    if (slot == null)
                    {
                        throw new CacheCapacityException($"No more free slots found. Insufficient storage capacity for cache {name}");
                    }
                }
                else
                {
                    // Else if the tile is in the unused cache remove it and keep a reference to it
                    tile = UnusedTiles.Remove(id);
                }

                if (tile != null)
                {
                    UsedTiles.Add(id, tile);
                }

            }
            else
            {
                tile = UsedTiles[id];
            }

            // Should never be null be this stage
            if (tile == null)
            {
                throw new System.NullReferenceException("Tile should't be null!");
            }

            // Keep track of the max number of tiles ever used for debug purposes
            if (UsedTilesCount > MaximumUsedTiles)
            {
                MaximumUsedTiles = UsedTilesCount;
            }

            // Increment the num of users
            tile.IncrementUsers();

            return tile;
        }

        /// <summary>
        /// Finds a tile based on its Tid. 
        /// If <paramref name="includeUnusedCache"/> is true,
        /// then will also look in the unused cache but be warned that tiles in the unused cache maybe evicted and have there slot recylced as any time.
        /// </summary>
        /// <param name="producerId">Producer id.</param>
        /// <param name="level">Tile level.</param>
        /// <param name="tx">Tile Tx.</param>
        /// <param name="ty">Tile Ty.</param>
        /// <param name="includeUnusedCache">Shoud look in to the unused cache?</param>
        /// <returns>Tile instance.</returns>
        public Tile FindTile(int producerId, int level, int tx, int ty, bool includeUnusedCache)
        {
            TileTIDBuffer.Set(producerId, level, tx, ty);

            Tile tile = null;

            // Looks for the requested tile in the used tiles list
            if (UsedTiles.TryGetValue(TileTIDBuffer, out tile))
            {
                return tile;
            }
            else
            {
                // Looks for the requested tile in the unused tiles list (if includeUnusedCache is true)
                if (includeUnusedCache)
                {
                    if (UnusedTiles.ContainsKey(TileTIDBuffer))
                    {
                        return UnusedTiles.Get(TileTIDBuffer);
                    }
                }

            }

            return null;
        }
    }
}