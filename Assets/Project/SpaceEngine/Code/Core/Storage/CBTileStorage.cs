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
using SpaceEngine.Helpers;

using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    /// A tile storage that can contain compute buffers.
    /// </summary>
    public class CBTileStorage : TileStorage
    {
        /// <summary>
        /// A slot managed by a CBTileStorage containing the buffer.
        /// </summary>
        public class CBSlot : Slot
        {
            public ComputeBuffer Buffer { get; private set; }

            public ComputeBuffer GetBuffer()
            {
                return Buffer;
            }

            public override void Release()
            {
                BufferHelper.ReleaseAndDisposeBuffers(Buffer);
            }

            public CBSlot(TileStorage owner, ComputeBuffer buffer) : base(owner)
            {
                Buffer = buffer;
            }
        }

        public DATA_TYPE DataType = DATA_TYPE.FLOAT;

        public int Channels = 1;

        public ComputeBufferType ComputeBufferType = ComputeBufferType.Default;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void InitSlots()
        {
            base.InitSlots();

            for (ushort i = 0; i < Capacity; i++)
            {
                ComputeBuffer buffer;

                switch (DataType)
                {
                    case DATA_TYPE.FLOAT:
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.INT:
                        buffer = new ComputeBuffer(TileSize, sizeof(int) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.BYTE:
                        buffer = new ComputeBuffer(TileSize, sizeof(byte) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.SHORT:
                    default:
                    {
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, ComputeBufferType);

                        Debug.LogWarning($"TileStorage: {DataType.ToString()} data type isn't supported by {GetType().Name}! Float type will be used!");

                        break;
                    }
                }

                var slot = new CBSlot(this, buffer);

                AddSlot(i, slot);
            }
        }
    }
}