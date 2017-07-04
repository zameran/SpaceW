using SpaceEngine.Core.Tile.Cache;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Storage
{
    /// <summary>
    /// A shared storage to store tiles of the same kind.This abstract class defines the behavior of tile storages but does not provide any storage itself.
    /// The slots managed by a tile storage can be used to store any tile identified by it's coordinates.
    /// This means that a <see cref="Slot"/> can store the data of some tile at some moment, and then be reused to store the data of
    /// tile some time later.The mapping between tiles and <see cref="Slot"/> is not managed by the <see cref="TileStorage"/> itself, but by a <see cref="TileCache"/>. 
    /// A <see cref="TileStorage"/> just keeps track of which slots in the pool are currently associated with a tile (i.e., store the data of a tile), 
    /// and which are not. 
    /// The first ones are called allocated <see cref="Slot"/>'s, the others free <see cref="Slot"/>'s.
    /// </summary>
    [RequireComponent(typeof(TileCache))]
    public abstract class TileStorage : MonoBehaviour
    {
        [Serializable]
        public enum DATA_TYPE
        {
            FLOAT,
            INT,
            SHORT,
            BYTE
        }

        /// <summary>
        /// A slot managed by a <see cref="TileStorage"/>. Concrete sub classes of this class must provide a reference to the actual tile data.
        /// </summary>
        public abstract class Slot
        {
            /// <summary>
            /// The <see cref="TileStorage"/> that manages this slot.
            /// </summary>
            public TileStorage Owner { get; private set; }

            protected Slot(TileStorage owner)
            {
                Owner = owner;
            }

            /// <summary>
            /// Override this, if the slot needs to release data on destroy.
            /// </summary>
            public virtual void Release()
            {

            }

            public virtual void Clear()
            {

            }
        }

        [SerializeField] private bool OnePixelBorder = false;

        /// <summary>
        /// The size of each tile. For tiles made of raster data, this size is the tile width in pixels (the tile height is supposed equal to the tile width).
        /// </summary>
        public int TileSize { get { var size = GodManager.Instance.TileSize; return OnePixelBorder ? size + 1 : size; } }

        /// <summary>
        /// The total number of slots managed by this <see cref="TileStorage"/>. This includes both unused and used tiles.
        /// </summary>
        public ushort Capacity { get; protected set; }

        public Slot[] Slots;

        /// <summary>
        /// The currently free slots.
        /// </summary>
        protected LinkedList<Slot> SlotsFree;

        /// <summary>
        /// The used slots counts;
        /// </summary>
        public int SlotsCount { get { return Slots.Length; } }

        public int FreeSlotsCount { get { return SlotsFree.Count; } }

        protected virtual void Awake()
        {
            Capacity = GetComponent<TileCache>().Capacity;

            InitSlots();
        }

        public void OnDestroy()
        {
            Release();
        }

        protected void AddSlot(int i, Slot slot)
        {
            Slots[i] = slot;
            SlotsFree.AddLast(slot);
        }

        /// <summary>
        /// Return a free <see cref="Slot"/>, or null, if all tiles are currently allocated. 
        /// The returned <see cref="Slot"/> is then considered to be allocated, until it is released.
        /// </summary>
        /// <returns>A free <see cref="Slot"/> in the pool of slots managed by this <see cref="TileStorage"/>.</returns>
        public Slot AddSlot()
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
        /// Notifies this storage that the given <see cref="Slot"/> is free. 
        /// The given <see cref="Slot"/> can then be allocated to store a new tile, i.e., it can be returned by a subsequent call to new <see cref="Slot"/>.
        /// </summary>
        /// <param name="slot">A <see cref="Slot"/> that is no longer in use.</param>
        public void DeleteSlot(Slot slot)
        {
            SlotsFree.AddLast(slot);
        }

        /// <summary>
        /// Calls <see cref="Slot.Release"/> method on every <see cref="Slot"/>.
        /// </summary>
        public void Release()
        {
            for (ushort i = 0; i < Capacity; i++)
            {
                Slots[i].Release();
            }
        }

        public void Clear()
        {
            for (ushort i = 0; i < Capacity; i++)
            {
                Slots[i].Clear();
            }
        }

        public virtual void InitSlots()
        {
            Slots = new Slot[Capacity];
            SlotsFree = new LinkedList<Slot>();
        }
    }
}