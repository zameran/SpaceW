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

using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Tile.Tasks;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile
{
    /// <summary>
    /// A tile described by its level and tx, ty coordinates. 
    /// A <see cref="Tile"/> describes where the tile is stored in the <see cref="TileStorage"/>, 
    /// how its data can be produced, and how many users currently use it.
    /// Contains the keys (<see cref="Id"/>, <see cref="TId"/>) commonly used to store the tiles in data structures like dictionaries.
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// A tile identifier for a given <see cref="Producer.TileProducer"/>. 
        /// Contains the tile's <see cref="Tile.Level"/>, <see cref="Tile.Tx"/>, <see cref="Tile.Ty"/>.
        /// </summary>
        public class Id
        {
            public int Level { get; private set; }
            public int Tx { get; private set; }
            public int Ty { get; private set; }

            public Id(Id from)
            {
                Level = from.Level;
                Tx = from.Tx;
                Ty = from.Ty;
            }

            public Id(int level, int tx, int ty)
            {
                Set(level, tx, ty);
            }

            public Id(int tx, int ty)
            {
                Set(0, tx, ty);
            }

            public void Set(int level, int tx, int ty)
            {
                Level = level;
                Tx = tx;
                Ty = ty;
            }

            public int Compare(Id id)
            {
                return Level.CompareTo(id.Level);
            }

            public bool Equals(Id id)
            {
                return (Level == id.Level && Tx == id.Tx && Ty == id.Ty);
            }

            public override int GetHashCode()
            {
                var hashcode = 23;

                hashcode = (hashcode * 37) + Level;
                hashcode = (hashcode * 37) + Tx;
                hashcode = (hashcode * 37) + Ty;

                return hashcode;
            }

            public override string ToString()
            {
                return $"({Level}, {Tx}, {Ty})";
            }
        }

        /// <summary>
        /// A tile identifier. 
        /// Contains a <see cref="Producer.TileProducer"/>'s id and <see cref="Id"/>.
        /// </summary>
        public class TId
        {
            public int ProducerId { get; set; }

            public Id TileId { get; set; }

            public TId(TId from)
            {
                ProducerId = from.ProducerId;

                TileId = new Id(from.TileId);
            }

            public TId(int producerId, int level, int tx, int ty)
            {
                ProducerId = producerId;

                TileId = new Id(level, tx, ty);
            }

            public void Set(int producerId, int level, int tx, int ty)
            {
                ProducerId = producerId;

                TileId.Set(level, tx, ty);
            }

            public bool Equals(TId id)
            {
                return (ProducerId == id.ProducerId && TileId.Equals(id.TileId));
            }

            public override int GetHashCode()
            {
                var hashcode = 23;

                hashcode = (hashcode * 37) + ProducerId;
                hashcode = (hashcode * 37) + TileId.GetHashCode();

                return hashcode;
            }

            public override string ToString()
            {
                return $"({ProducerId}, {TileId})";
            }
        }

        /// <summary>
        /// A <see cref="Id"/> is sorted based as it's level. 
        /// Sorts from lowest level to highest.
        /// </summary>
        public class ComparerID : IComparer<Id>
        {
            public int Compare(Id a, Id b)
            {
                return a.Compare(b);
            }
        }

        /// <summary>
        /// A A <see cref="Id"/> is compared based on it's <see cref="Tile.Level"/>, <see cref="Tile.Tx"/>, <see cref="Tile.Ty"/>.
        /// </summary>
        public class EqualityComparerID : IEqualityComparer<Id>
        {
            public bool Equals(Id t1, Id t2)
            {
                return t1.Equals(t2);
            }

            public int GetHashCode(Id t)
            {
                return t.GetHashCode();
            }
        }

        /// <summary>
        /// A Tid is compared based on it's producer, <see cref="Tile.Level"/>, <see cref="Tile.Tx"/>, <see cref="Tile.Ty"/>.
        /// </summary>
        public class EqualityComparerTID : IEqualityComparer<TId>
        {
            public bool Equals(TId t1, TId t2)
            {
                return t1.Equals(t2);
            }

            public int GetHashCode(TId t)
            {
                return t.GetHashCode();
            }
        }

        /// <summary>
        /// The id of the producer that manages this tile.
        /// </summary>
        public int ProducerId;

        /// <summary>
        /// Number of users currently using this tile.
        /// </summary>
        public int Users { get; private set; }

        /// <summary>
        /// The quadtree level of this tile.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// The quadtree x coordinate of this tile at <see cref="Level"/>. [0, 2 ^ <see cref="Level"/> - 1]
        /// </summary>
        public int Tx { get; private set; }

        /// <summary>
        /// The quadtree y coordinate of this tile at <see cref="Level"/>. [0, 2 ^ <see cref="Level"/> - 1]
        /// </summary>
        public int Ty { get; private set; }

        /// <summary>
        /// The task that produces or produced the actual tile data.
        /// </summary>
        public CreateTileTask Task { get; private set; }

        public List<TileStorage.Slot> Slot => Task.Slot;

        public Id ID { get; private set; }

        public TId TID { get; private set; }

        public Tile(int producerId, int level, int tx, int ty, CreateTileTask task)
        {
            ProducerId = producerId;
            Level = level;
            Tx = tx;
            Ty = ty;
            Task = task;
            Users = 0;

            ID = GetId(level, tx, ty);
            TID = GetTId(producerId, ID);

            if (Task == null)
            {
                Debug.Log("Tile.ctor: Task can't be null!");
            }
        }

        public TileStorage.Slot GetSlot(int i)
        {
            if (i >= Task.Slot.Count)
            {
                Debug.Log($"Tile: Slot at location {i} does not exist!");
            }

            return Task.Slot[i];
        }

        public void IncrementUsers()
        {
            Users++;
        }

        public void DecrementUsers()
        {
            Users--;
        }

        /// <summary>
        /// The identifier of this tile.
        /// </summary>
        /// <param name="level">The <see cref="Tile"/>'s quadtree level.</param>
        /// <param name="tx">The <see cref="Tile"/>'s quadtree x coordinate.</param>
        /// <param name="ty">The <see cref="Tile"/>'s quadtree y coordinate.</param>
        /// <returns>Returns the identifier of this tile.</returns>
        public static Id GetId(int level, int tx, int ty)
        {
            return new Id(level, tx, ty);
        }

        /// <summary>
        /// The identifier of this tile.
        /// </summary>
        /// <param name="producerId">The <see cref="Id"/> of the <see cref="Tile"/>'s producer.</param>
        /// <param name="level">The <see cref="Tile"/>'s quadtree level.</param>
        /// <param name="tx">The <see cref="Tile"/>'s quadtree x coordinate.</param>
        /// <param name="ty">The <see cref="Tile"/>'s quadtree y coordinate.</param>
        /// <returns>Returns the identifier of this tile.</returns>
        public static TId GetTId(int producerId, int level, int tx, int ty)
        {
            return new TId(producerId, level, tx, ty);
        }

        /// <summary>
        /// The identifier of this tile.
        /// </summary>
        /// <param name="producerId">The <see cref="Id"/> of the <see cref="Tile"/>'s producer.</param>
        /// <param name="id">The identifier of this tile.</param>
        /// <returns></returns>
        public static TId GetTId(int producerId, Id id)
        {
            return new TId(producerId, id.Level, id.Tx, id.Ty);
        }
    }
}