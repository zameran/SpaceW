using SpaceEngine.Core.Tile.Cache;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Storage
{
    /// <summary>
    /// A shared storage to store tiles of the same kind.This abstract class defines
    /// the behavior of tile storages but does not provide any storage itself.The
    /// slots managed by a tile storage can be used to store any tile identified by
    /// its(level, tx, ty) coordinates.This means that a TileStorage::Slot can store
    /// the data of some tile at some moment, and then be reused to store the data of
    /// tile some time later.The mapping between tiles and TileStorage::Slot is not
    /// managed by the TileStorage itself, but by a TileCache. A TileStorage just
    /// keeps track of which slots in the pool are currently associated with a
    /// tile (i.e., store the data of a tile), and which are not.The first ones are
    /// called allocated slots, the others free slots.
    /// </summary>
    [RequireComponent(typeof(TileCache))]
    public abstract class TileStorage : MonoBehaviour
    {
        /// <summary>
        /// A slot managed by a TileStorage. Concrete sub classes of this class must provide a reference to the actual tile data.
        /// </summary>
        public abstract class Slot
        {
            /// <summary>
            /// The TileStorage that manages this slot.
            /// </summary>
            public TileStorage Owner { get; protected set; }

            protected Slot(TileStorage owner)
            {
                Owner = owner;
            }

            /// <summary>
            /// Override this, if the slot needs to release data on destroy
            /// </summary>
            public virtual void Release()
            {

            }
        }

        [SerializeField]
        int tileSize;

        /// <summary>
        /// The size of each tile. For tiles made of raster data, this size is the tile width in pixels (the tile height is supposed equal to the tile width).
        /// </summary>
        public int TileSize { get { return tileSize; } protected set { tileSize = value; } }

        /// <summary>
        /// The total number of slots managed by this TileStorage. This includes both unused and used tiles.
        /// </summary>
        public int Capacity { get; protected set; }

        public Slot[] Slots;

        /// <summary>
        /// The used slots counts;
        /// </summary>
        public int SlotsCount { get { return Slots.Length; } }

        /// <summary>
        /// The currently free slots.
        /// </summary>
        LinkedList<Slot> SlotsFree;

        public int FreeSlotsCount { get { return SlotsFree.Count; } }

        protected virtual void Awake()
        {
            Capacity = GetComponent<TileCache>().Capacity;

            Slots = new Slot[Capacity];
            SlotsFree = new LinkedList<Slot>();
        }

        public void OnDestroy()
        {
            for (int i = 0; i < Capacity; i++)
            {
                Slots[i].Release();
            }
        }

        protected void AddSlot(int i, Slot slot)
        {
            Slots[i] = slot;
            SlotsFree.AddLast(slot);
        }

        /// <summary>
        /// Return a free slot, or NULL if all tiles are currently allocated. The returned slot is then considered to be allocated, until it is released with deleteSlot.
        /// </summary>
        /// <returns>A free slot in the pool of slots managed by this TileStorage.</returns>
        public Slot NewSlot()
        {
            if (SlotsFree.Count != 0)
            {
                var slot = SlotsFree.First.Value;

                SlotsFree.RemoveFirst();

                return slot;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Notifies this storage that the given slot is free. The given slot can then be allocated to store a new tile, i.e., it can be returned by a subsequent call to new slot.
        /// </summary>
        /// <param name="slot">A slot that is no longer in use.</param>
        public void DeleteSlot(Slot slot)
        {
            SlotsFree.AddLast(slot);
        }
    }
}