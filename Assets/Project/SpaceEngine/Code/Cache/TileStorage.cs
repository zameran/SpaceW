using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A shared storage to store tiles of the same kind. This abstract class defines
/// the behavior of tile storages but does not provide any storage itself. The
/// slots managed by a tile storage can be used to store any tile identified by
/// its (level,tx,ty) coordinates. This means that a TileStorage::Slot can store
/// the data of some tile at some moment, and then be reused to store the data of
/// tile some time later. The mapping between tiles and TileStorage::Slot is not
/// managed by the TileStorage itself, but by a TileCache. A TileStorage just
/// keeps track of which slots in the pool are currently associated with a
/// tile (i.e., store the data of a tile), and which are not. The first ones are
/// called allocated slots, the others free slots.
/// </summary>
public abstract class TileStorage : MonoBehaviour
{
    /// <summary>
    /// A slot managed by a TileStorage. Concrete sub classes of this class must
    /// provide a reference to the actual tile data.
    /// </summary>
    [System.Serializable]
    public abstract class Slot
    {
        /// <summary>
        /// The TileStorage that manages this slot.
        /// </summary>
        TileStorage m_owner;

        public TileStorage GetOwner()
        {
            return m_owner;
        }

        public Slot(TileStorage owner)
        {
            m_owner = owner;
        }

        /// <summary>
        /// VOerride this if the slot needs to release data on destroy
        /// </summary>
        public virtual void Release()
        {

        }
    };

    /// <summary>
    /// The size of each tile. For tiles made of raster data, this size is the
    /// tile width in pixels (the tile height is supposed equal to the tile width).
    /// </summary>
    [SerializeField]
    int m_tileSize;

    /// <summary>
    /// The total number of slots managed by this TileStorage. This includes both
    /// unused and used tiles.
    /// </summary>
    [SerializeField]
    int m_capacity;

    [SerializeField]
    Slot[] m_allSlots;

    /// <summary>
    /// The currently free slots.
    /// </summary>
    [SerializeField]
    LinkedList<Slot> m_freeSlots;

    protected virtual void Awake()
    {
        m_allSlots = new Slot[m_capacity];
        m_freeSlots = new LinkedList<Slot>();
    }

    public void OnDestroy()
    {
        for (int i = 0; i < m_capacity; i++)
            m_allSlots[i].Release();
    }

    protected void AddSlot(int i, Slot slot)
    {
        m_allSlots[i] = slot;
        m_freeSlots.AddLast(slot);
    }

    /// <summary>
    /// Returns a free slot in the pool of slots managed by this TileStorage.
    /// <returns>
    /// A free slot, or NULL if all tiles are currently allocated. The
    /// returned slot is then considered to be allocated, until it is
    /// released with deleteSlot.
    /// </returns>
    /// </summary>
    public Slot NewSlot()
    {
        if (m_freeSlots.Count != 0)
        {
            Slot s = m_freeSlots.First.Value;
            m_freeSlots.RemoveFirst();
            return s;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Notifies this storage that the given slot is free. The given slot can
    /// then be allocated to store a new tile, i.e., it can be returned by a
    /// subsequent call to newSlot.
    /// <param name="t">A slot that is no longer in use.</param>
    /// </summary>
    public void DeleteSlot(Slot t)
    {
        m_freeSlots.AddLast(t);
    }

    /// <summary>
    /// Returns the size of each tile. For tiles made of raster data, this size
    /// is the tile width in pixels (the tile height is supposed equal to the
    /// tile width).
    /// </summary>
    public int GetTileSize()
    {
        return m_tileSize;
    }

    /// <summary>
    /// Returns the total number of slots managed by this TileStorage. This
    /// includes both unused and used tiles.
    /// </summary>
    public int GetCapacity()
    {
        return m_capacity;
    }

    /// <summary>
    /// Returns the number of slots in this TileStorage that are currently unused.
    /// </summary>
    public int GetFreeSlots()
    {
        return m_freeSlots.Count;
    }
}