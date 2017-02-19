using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Utilities;

using System.Collections.Generic;

namespace SpaceEngine.Core.Tile.Layer
{
    /// <summary>
    /// An abstract layer for a TileProducer. Some tile producers can be customized with layers modifying the default tile production algorithm.
    /// For these kind of producers, each method of this class is called during the corresponding method in the TileProducer. 
    /// The default implementation of these methods in this class is empty.
    /// </summary>
    public abstract class TileLayer : Node
    {
        protected override void Start()
        {
            base.Start();
        }

        public abstract void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot);
    }
}