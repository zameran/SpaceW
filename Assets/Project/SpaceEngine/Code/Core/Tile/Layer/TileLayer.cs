using SpaceEngine.Core.Tile.Storage;

using System.Collections.Generic;

namespace SpaceEngine.Core.Tile.Layer
{
    /// <summary>
    /// An abstract layer for a TileProducer. Some tile producers can be customized with layers modifying the default tile production algorithm.
    /// For these kind of producers, each method of this class is called during the corresponding method in the TileProducer. 
    /// The default implementation of these methods in this class is empty.
    /// </summary>
    public abstract class TileLayer : NodeSlave<TileLayer>
    {
        #region NodeSlave<TileLayer>

        public override void InitNode()
        {

        }

        public override void UpdateNode()
        {

        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        public abstract void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot);
    }
}