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
using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    ///     A TileStorage that store tiles on CPU as a 2D array of values where is the type of each tile pixel component(e.g. char, float, etc).
    /// </summary>
    public class CPUTileStorage : TileStorage
    {
        public DATA_TYPE DataType = DATA_TYPE.FLOAT;

        public int Channels = 1;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void InitSlots()
        {
            base.InitSlots();

            // NOTE : Size is squared as the array is 2D (but stored as a 1D array)
            var size = TileSize * TileSize * Channels;

            for (ushort i = 0; i < Capacity; i++)
            {
                switch ((int)DataType)
                {
                    case (int)DATA_TYPE.FLOAT:
                        AddSlot(i, new CPUSlot<float>(this, size));

                        break;
                    case (int)DATA_TYPE.INT:
                        AddSlot(i, new CPUSlot<int>(this, size));

                        break;
                    case (int)DATA_TYPE.SHORT:
                        AddSlot(i, new CPUSlot<short>(this, size));

                        break;
                    case (int)DATA_TYPE.BYTE:
                        AddSlot(i, new CPUSlot<byte>(this, size));

                        break;
                    default:
                    {
                        AddSlot(i, new CPUSlot<float>(this, size));

                        Debug.LogWarning($"TileStorage: {DataType.ToString()} data type isn't supported by {GetType().Name}! Float type will be used!");

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     A slot managed by a GPUTileStorage and contains the array of values.
        /// </summary>
        /// <typeparam name="T">Type of container.</typeparam>
        public class CPUSlot<T> : Slot
        {
            public CPUSlot(TileStorage owner, int size) : base(owner)
            {
                Data = new T[size];
                Size = size;
            }

            public T[] Data { get; private set; }
            public int Size { get; }

            public void ClearData()
            {
                Data = new T[Size];
            }
        }
    }
}