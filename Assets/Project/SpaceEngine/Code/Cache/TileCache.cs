using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A cache of tiles to avoid recomputing recently produced tiles. A tile cache
/// keeps track of which tiles (identified by their level,tx,ty coordinates) are
/// currently stored in an associated TileStorage. It also keeps track of which
/// tiles are in use, and which are not. Unused tiles are kept in the TileStorage
/// as long as possible, in order to avoid re creating them if they become needed
/// again. But the storage associated with unused tiles can be reused to store
/// other tiles at any moment (in this case we say that a tile is evicted from
/// the cache of unused tiles).
/// Conversely, the storage associated with tiles currently in use cannot be
/// reaffected until these tiles become unused. A tile is in use when it is
/// returned by GetTile, and becomes unused when PutTile is called (more
/// precisely when the number of users of this tile becomes 0, this number being
/// incremented and decremented by GetTile and PutTile, respectively). The
/// tiles that are needed to render the current frame should be declared in use,
/// so that they are not evicted between their creation and their actual
/// rendering.
/// A cache can have multiple TileStorages attached to it and the slot created is made up
/// of a slot from each of the TileStorages. This is so producer can generate tiles that contain 
/// multiple types of data associated with the same tile. For example the PlantsProducer uses a cache with 2 CBTileStorages,
/// one slot for the plants position and one for the plants other parameters.
/// </summary>
public class TileCache : MonoBehaviour
{
    [SerializeField]
    int m_capacity;

    TileStorage[] m_tileStorage;

    Dictionary<Quad.Id, Quad> m_usedTiles;

    DictionaryQueue<Quad.Id, Quad> m_unusedTiles;

    int m_maxUsedTiles = 0;

    void Awake()
    {
        m_tileStorage = GetComponents<TileStorage>();
        m_usedTiles = new Dictionary<Quad.Id, Quad>(new Quad.EqualityComparerID());
        m_unusedTiles = new DictionaryQueue<Quad.Id, Quad>(new Quad.EqualityComparerID());
    }

    public TileStorage GetStorage(int i)
    {
        if (i >= m_tileStorage.Length)
        {
            Debug.Log("TileCache.GetStorage - tile storage at location " + i + " does not exist");
        }

        return m_tileStorage[i];
    }

    public int GetCapacity()
    {
        return m_capacity;
    }

    public int GetTileStorageCount()
    {
        return m_tileStorage.Length;
    }

    public int GetUsedTilesCount()
    {
        return m_usedTiles.Count;
    }

    public int GetUnusedTilesCount()
    {
        return m_unusedTiles.Count();
    }

    public int GetMaxUsedTiles()
    {
        return m_maxUsedTiles;
    }

    public void PutTile(Quad tile)
    {
        if (tile == null) return;

        tile.DecrementUsers();

        //if there are no more users of this tile move the tile from the used cahce to the unused cache
        if (tile.GetUsers() <= 0)
        {
            Quad.Id id = tile.GetId();

            if (m_usedTiles.ContainsKey(id))
                m_usedTiles.Remove(id);

            if (!m_unusedTiles.ContainsKey(id))
                m_unusedTiles.AddLast(id, tile);
        }

    }

    List<TileStorage.Slot> NewSlot()
    {
        List<TileStorage.Slot> slot = new List<TileStorage.Slot>();

        foreach (TileStorage storage in m_tileStorage)
        {
            TileStorage.Slot s = storage.NewSlot();
            if (s == null) return null;
            slot.Add(s);
        }

        return slot;
    }

    public Quad GetTile(int LODLevel, int ID, int Position)
    {
        Quad.Id id = Quad.GetId(LODLevel, ID, Position);
        Quad tile = null;

        //If tile is not in the used cache
        if (!m_usedTiles.ContainsKey(id))
        {
            //If tile is also not in the unused cache
            if (!m_unusedTiles.ContainsKey(id))
            {
                List<TileStorage.Slot> slot = NewSlot();

                ////if there are no more free slots then start recyling slots from the unused tiles
                //if (slot == null && !m_unusedTiles.Empty())
                //{
                //    //Remove the tile and recylce its slot
                //    slot = m_unusedTiles.RemoveFirst().GetSlot();
                //}

                ////If a slot is found create a new tile with a new task
                //if (slot != null)
                //{
                //    tile = new Quad();
                //}

                //If a free slot is not found then program has must abort. Try setting the cache capacity to higher value.
                if (slot == null)
                {
                    throw new System.NullReferenceException("No more free slots found. Insufficient storage capacity for cache " + name);
                }
            }
            else
            {
                //else if the tile is in the unused cache remove it and keep a reference to it
                tile = m_unusedTiles.Remove(id);
            }

            if (tile != null)
            {
                m_usedTiles.Add(id, tile);
            }

        }
        else
        {
            tile = m_usedTiles[id];
        }

        //Should never be null be this stage
        if (tile == null)
        {
            throw new System.ArgumentNullException("Tile should not be null");
        }

        //Keep track of the max number of tiles ever used for debug purposes
        if (m_usedTiles.Count > m_maxUsedTiles)
            m_maxUsedTiles = m_usedTiles.Count;

        //inc the num of users
        tile.IncrementUsers();

        return tile;
    }

    public Quad FindTile(int LODLevel, int ID, int Position, bool includeUnusedCache)
    {
        Quad.Id id = Quad.GetId(LODLevel, ID, Position);
        Quad tile = null;

        // looks for the requested tile in the used tiles list
        if (m_usedTiles.ContainsKey(id))
            tile = m_usedTiles[id];

        // looks for the requested tile in the unused tiles list (if includeUnusedCache is true)
        if (tile == null && includeUnusedCache)
        {
            if (m_unusedTiles.ContainsKey(id))
                tile = m_unusedTiles.Get(id);
        }

        return tile;
    }
}