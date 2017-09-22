using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Utilities;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Tasks
{
    /// <summary>
    /// The task that creates the tiles. The task calles the producers <see cref="TileProducer.DoCreateTile"/> function and the data created is stored in the slot.
    /// </summary>
    public class CreateTileTask : Schedular.Task
    {
        /// <summary>
        /// The TileProducer that created this task.
        /// </summary>
        public TileProducer Owner { get; protected set; }

        /// <summary>
        /// The level of the tile to create.
        /// </summary>
        public int Level { get; protected set; }

        /// <summary>
        /// The quadtree x coordinate of the tile to create.
        /// </summary>
        public int Tx { get; protected set; }

        /// <summary>
        /// The quadtree y coordinate of the tile to create.
        /// </summary>
        public int Ty { get; protected set; }

        /// <summary>
        /// Where the created tile data must be stored.
        /// </summary>
        public List<TileStorage.Slot> Slot { get; protected set; }

        public CreateTileTask(TileProducer owner, int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            Owner = owner;
            Level = level;
            Tx = tx;
            Ty = ty;
            Slot = slot;
        }

        public override void Run()
        {
            if (IsDone)
            {
                Debug.Log(string.Format("CreateTileTask.Run: Task for {0} at {1}:{2}:{3} has already been run. This task will not proceed!", Owner.GetType().Name, Level, Tx, Ty));

                return;
            }

            try
            {
                if (GodManager.Instance.DelayedCalculations && !Owner.IsLastInSequence)
                {
                    Owner.StartCoroutine(Owner.DoCreateTileCoroutine(Level, Tx, Ty, Slot, () =>
                    {
                        // Manualy finish the particular tile creation task.
                        this.Finish();
                    }));

                    // So, task will be finished in the end of coroutine...
                }
                else
                {
                    Owner.DoCreateTile(Level, Tx, Ty, Slot);

                    // So, finish task NOW! It's done already, yah?
                    Finish();
                }
            }
            catch
            {
                // NOTE : Sometimes tile producers can't find parent tile due to parent tile Task uncomplection...
                Debug.Log("CreateTileTask.Run: There are exception inside the Task while processing!");

                return;
            }
        }

        public override void Finish()
        {
            base.Finish();
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2},{3})", Owner.name, Level, Tx, Ty);
        }
    }
}