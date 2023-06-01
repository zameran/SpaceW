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

using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Utilities;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Tasks
{
    /// <summary>
    /// The task that creates the tiles. 
    /// The task calles the producers <see cref="TileProducer.DoCreateTile"/> function and the data created is stored in the slot.
    /// </summary>
    public class CreateTileTask : Schedular.Task
    {
        /// <summary>
        /// The <see cref="TileProducer"/> that created this task.
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
                Debug.Log($"CreateTileTask.Run: Task for {Owner.GetType().Name} at {Level}:{Tx}:{Ty} has already been run. This task will not proceed!");

                return;
            }

            // NOTE : Process with default even if tile level is 0 or 1 and delayed calculations is enabled...
            if (GodManager.Instance.DelayedCalculations && !Owner.IsLastInSequence && Level > 1)
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

        public override void Finish()
        {
            base.Finish();
        }

        public override string ToString()
        {
            return $"({Owner.name},{Level},{Tx},{Ty})";
        }
    }
}